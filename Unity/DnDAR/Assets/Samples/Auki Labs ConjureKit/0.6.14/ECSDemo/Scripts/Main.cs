using System;
using System.Collections.Generic;
using Auki.ConjureKit;
using UnityEngine;
using UnityEngine.UI;

namespace AukiSampleECSDemo
{
    /// <summary>
    /// This sample shows a basic ConjureKit HECS use case:
    /// - Creating a new component
    /// - Updating that component
    /// - Sending & receiving component updates between participants
    /// * The InputField in the upper right allows you to input a session id & join another device in a shared session.
    /// * The Spawn button in the upper right will instantiate a cube that will have a new component attached to it.
    /// This cube will have a new HECS component attached to it that will modify its scale.
    /// </summary>
    public class Main : MonoBehaviour
    {
        /// <summary>
        /// Transform of the camera GameObject. Required by ConjureKit.
        /// </summary>
        public Transform cameraTransform;

        /// <summary>
        /// When pressed will instantiate a cube and attach a scaling component to it.
        /// </summary>
        public Button spawnButton;
        
        /// <summary>
        /// Label that shows currently joined session id.
        /// </summary>
        public Text sessionInfo;
        
        /// <summary>
        /// Label that shows current ConjureKit state.
        /// </summary>
        public Text conjureKitStateInfo;
        
        /// <summary>
        /// When pressed will try to join session with id from customSessionInputField.
        /// </summary>
        public Button joinCustomSessionButton;
        
        /// <summary>
        /// Input field that allows you to specify session to join by id.
        /// If the session does not exist the participant will be redirected to a new one.
        /// </summary>
        public InputField customSessionInputField;
        
        private const string TEST_SCALE_COMPONENT_NAME = "test_scale";
    
        private ConjureKit _conjureKit;
       
        private bool _joined;
        private string _lastJoinedSessionId;
    
        private readonly Dictionary<uint, GameObject> _entityMap = new Dictionary<uint, GameObject>();
        private Entity _spawnedCubeEntity;
        private GameObject _cubeGameObject;
    
        private uint _testScaleComponentId;
        
        /// <summary>
        /// If there's no reference to BoxCollider GameObject.CreatePrimitive in Spawn() might fail.
        /// Read note here -- https://docs.unity3d.com/ScriptReference/GameObject.CreatePrimitive.html.
        /// </summary>
        private BoxCollider _preventBoxColliderStripping = new BoxCollider();
        
        private void Start()
        {
            // Initialize the SDK.
            _conjureKit = new ConjureKit(
                cameraTransform,
                "8e6f1bcc-1d4c-4bcc-bce2-67bd65f775d6",
                "9a5e53a8-4431-48b5-b201-f4a5efce8605769f1bdb-16c6-4e71-95bc-e0148dc63f32"
            );
            
            _conjureKit.OnJoined += session =>
            {
                sessionInfo.text = session.Id;
                _joined = true;
                _lastJoinedSessionId = session.Id;

                // Create/retrieve the id of a component of type TEST_SCALE_COMPONENT_NAME and subscribe to its updates.
                session.AddComponentType(
                    TEST_SCALE_COMPONENT_NAME,
                    componentTypeId =>
                    {
                        _testScaleComponentId = componentTypeId;
                        session.SubscribeToComponentType(componentTypeId, () => {}, Debug.Log);
                    },
                    Debug.Log
                );
            };
    
            // Add a cube for all added non-participant entities.
            _conjureKit.OnEntityAdded += entity =>
            {
                if (entity.Flag == EntityFlag.EntityFlagParticipantEntity) return;
                var pose = _conjureKit.GetSession().GetEntityPose(entity.Id);
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.SetPositionAndRotation(pose.position, pose.rotation);
                go.transform.localScale = Vector3.one * 0.1f;
                _entityMap[entity.Id] = go;
            };
    
            // Remove cubes for deleted non-participant entities.
            _conjureKit.OnEntityDeleted += entityId =>
            {
                if (!_entityMap.ContainsKey(entityId)) return;
                var go = _entityMap[entityId];
                _entityMap.Remove(entityId);
                Destroy(go);
            };
    
            // Subscribe to component updates that we've subscribed to.
            _conjureKit.OnComponentUpdate += broadcast =>
            {
                var floatArray = new float[1];
                Buffer.BlockCopy(broadcast.EntityComponent.Data, 0, floatArray, 0, 1 * sizeof(float));
                
                var go = _entityMap[broadcast.EntityComponent.EntityId];
                go.transform.localScale = Vector3.one * floatArray[0];
            };
    
            _conjureKit.OnLeft += () =>
            {
                sessionInfo.text = "";
                _joined = false;
                _entityMap.Clear();
                _spawnedCubeEntity = null;
            };
    
            _conjureKit.OnStateChanged += state =>
            {
                ToggleControlsState(state == State.JoinedSession || state == State.Calibrated);
                conjureKitStateInfo.text = state.ToString();
            };
            
            _conjureKit.Connect();
        }
        
        private void ToggleControlsState(bool interactable)
        {
            spawnButton.interactable = interactable;
            joinCustomSessionButton.interactable = interactable;
        }

        private void Update()
        {
            if (_spawnedCubeEntity == null) return;
    
            var scale = (Time.time % 2 + 1) * 0.01f;
            var scaleArray = new float[] {scale};
            var byteArray = new byte[sizeof(float)];
            Buffer.BlockCopy(scaleArray, 0, byteArray, 0, 1 * sizeof(float));
            
            _cubeGameObject.transform.localScale = Vector3.one * scale;
            _conjureKit.GetSession()?.UpdateComponent(_testScaleComponentId, _spawnedCubeEntity.Id, byteArray);
        }
    
        /// <summary>
        /// Spawn a new cube object in front of the camera.
        /// This is the object we will be driving the scale component of.
        /// The previously spawned cube (if any) will stop changing scale and wont have its component updated anymore.
        /// </summary>
        public void Spawn()
        {
            if (!_joined) return;
            
            var session = _conjureKit.GetSession();
            if (session == null) return;
            
            var position = cameraTransform.position + cameraTransform.forward * 0.05f;
            var cubePose = new Pose(position, Quaternion.identity);
            _cubeGameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _cubeGameObject.transform.SetPositionAndRotation(cubePose.position, cubePose.rotation);
            _cubeGameObject.transform.localScale = Vector3.one * 0.1f;
           
            // Create an entity for this cube.
            session.AddEntity(
                cubePose,
                entity =>
                {
                    _spawnedCubeEntity = entity;

                    var scaleArray = new float[] {0.1f};
                    var byteArray = new byte[sizeof(float)];
                    Buffer.BlockCopy(scaleArray, 0, byteArray, 0, 1 * sizeof(float));

                    // Add component to the cube entity. 
                    session.AddComponent(
                        _testScaleComponentId,
                        entity.Id,
                        byteArray,
                        () => { },
                        Debug.Log
                    );
                }, 
                Debug.Log
            );
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
                _ =>
                {
                    Debug.LogWarning($"Failed to join session {targetSessionId}, connecting to a new session instead.");
                    _conjureKit.Connect();
                }
            );
        }
    }
}
