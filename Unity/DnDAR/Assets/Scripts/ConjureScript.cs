using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using Auki.Util;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ConjureScript : MonoBehaviour
{
    [SerializeField] private Camera arCamera;

    [SerializeField] private Text sessionState;
    [SerializeField] private Text sessionID;

    [SerializeField] private GameObject cube;
    [SerializeField] private Button spawnButton;

    Char _thisChar;
    Transform _parent;

    [SerializeField] bool qrCodeBool;
    //[SerializeField] Button qrCodeButton;

    private IConjureKit _conjureKit;
    private Manna _manna;
    private MainController manager;

    public UIManager uiMan;

    private ARCameraManager arCameraManager;
    private Texture2D _videoTexture;

    /// <summary>
    /// When pressed will try to join session with id from customSessionInputField.
    /// </summary>
    public Button joinCustomSessionButton;

    /// <summary>
    /// Input field that allows you to specify session to join by id.
    /// If the session does not exist the participant will be redirected to a new one.
    /// </summary>
    public InputField customSessionInputField;

    private string _lastJoinedSessionId;

    void Start()
    {
        manager = FindObjectOfType<MainController>();
        arCameraManager = arCamera.GetComponent<ARCameraManager>();

        _conjureKit = new ConjureKit(
            arCamera.transform,
            "8e6f1bcc-1d4c-4bcc-bce2-67bd65f775d6",
            "9a5e53a8-4431-48b5-b201-f4a5efce8605769f1bdb-16c6-4e71-95bc-e0148dc63f32");

        _manna = new Manna(_conjureKit);

        _conjureKit.OnStateChanged += state =>
        {
            sessionState.text = state.ToString();
            ToggleControlsState(state == State.JoinedSession);
        };

        _conjureKit.OnJoined += session =>
        {
            sessionID.text = session.Id.ToString();
        };

        _conjureKit.OnLeft += () =>
        {
            sessionID.text = "";
        };

        _conjureKit.OnEntityAdded += SpawnChar;
        //_conjureKit.OnEntityAdded += CreateCube;

        _conjureKit.Init(ConjureKitConfiguration.Get());

        _conjureKit.Connect();
    }

    private void Update()
    {
        FeedMannaWithVideoFrames();
    }

    private void FeedMannaWithVideoFrames()
    {
        var imageAcquired = arCameraManager.TryAcquireLatestCpuImage(out var cpuImage);
        if (!imageAcquired)
        {
            AukiDebug.LogInfo("Couldn't acquire CPU image");
            return;
        }

        if (_videoTexture == null) _videoTexture = new Texture2D(cpuImage.width, cpuImage.height, TextureFormat.R8, false);

        var conversionParams = new XRCpuImage.ConversionParams(cpuImage, TextureFormat.R8);
        cpuImage.ConvertAsync(
            conversionParams,
            (status, @params, buffer) =>
            {
                _videoTexture.SetPixelData(buffer, 0, 0);
                _videoTexture.Apply();
                cpuImage.Dispose();

                _manna.ProcessVideoFrameTexture(
                    _videoTexture,
                    arCamera.projectionMatrix,
                    arCamera.worldToCameraMatrix
                );
            }
        );
    }

    private void ToggleControlsState(bool interactable)
    {
        if (spawnButton) spawnButton.interactable = interactable;
        //if (qrCodeButton) qrCodeButton.interactable = interactable;
    }

    public void ToggleLighthouse()
    {
        qrCodeBool = !qrCodeBool;
        _manna.SetLighthouseVisible(qrCodeBool);
    }

    //public void CreateCubeEntity()
    //{
    //    if (_conjureKit.GetState() != State.Calibrated)
    //        return;

    //    Vector3 position = arCamera.transform.position + arCamera.transform.forward * 0.5f;
    //    Quaternion rotation = Quaternion.Euler(0, arCamera.transform.eulerAngles.y, 0);

    //    Pose entityPos = new Pose(position, rotation);

    //    _conjureKit.GetSession().AddEntity(
    //        entityPos,
    //        onComplete: entity => CreateCube(entity),
    //        onError: error => Debug.Log(error));
    //}

    private void CreateChar(Entity entity)
    {
        if (entity.Flag == EntityFlag.EntityFlagParticipantEntity) return;

        var pose = _conjureKit.GetSession().GetEntityPose(entity);
        Instantiate(cube, pose.position, pose.rotation);
    }


    /// <summary>
    /// Tries to join a session by the id in the custom session InputField in the top right.
    /// </summary>
    public void JoinCustomSessionButtonPressed()
    {
        var targetSessionId = customSessionInputField.text;
        if (string.IsNullOrEmpty(targetSessionId) || targetSessionId == _lastJoinedSessionId) return;
        Debug.Log($"Joining custom session: {targetSessionId}");
        _conjureKit.Disconnect();
        _conjureKit.Connect(
            targetSessionId,
            session =>
            {
                Debug.Log($"Successfuly joined session with {targetSessionId}.");
                uiMan.ToScan();

            },
            errorMsg =>
            {
                Debug.LogWarning($"Failed to join session {targetSessionId}, connecting to a new session instead. ({errorMsg})");
                _conjureKit.Connect();
            }
        );
    }

    public void AddCharEntity(Transform parent, Char c)
    {
        _thisChar = c;
        _parent = parent;

        Pose entityPos = new Pose(parent.position, parent.rotation);
        _conjureKit.GetSession().AddEntity(
    entityPos,
    onComplete: entity => SpawnChar(entity),
    onError: error => Debug.Log(error));
    }


    public void SpawnChar(Entity entity)
    {
        if (entity.Flag == EntityFlag.EntityFlagParticipantEntity) return;

        if (manager.ScanCanvas.activeInHierarchy)
            manager.ScanCanvas.SetActive(false);


        var prefab = manager.CharacterPrefabs.Where(x => x.GetComponent<Character>().ThisChar == _thisChar).FirstOrDefault();
        var pose = _conjureKit.GetSession().GetEntityPose(entity);
        Instantiate(prefab, pose.position, pose.rotation);
        var t = Instantiate(prefab, _parent);

        if (t != null)
            manager.SpawnedChars.Add(prefab.GetComponent<Character>());

        if (!manager.LiveCanvas.activeInHierarchy)
            manager.LiveCanvas.SetActive(true);
    }
}
