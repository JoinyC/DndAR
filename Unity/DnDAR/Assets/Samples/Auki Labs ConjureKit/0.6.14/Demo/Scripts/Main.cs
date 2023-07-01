using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Auki.ConjureKit;

namespace AukiSampleDemo
{
    /// <summary>
    /// This sample shows some common ConjureKit use cases:
    /// - Connecting to a new session.
    /// - Connecting to an existing session by id.
    /// - Sending & receiving a sample string message between participants in a session.
    /// Before use you must create an application in the Auki Console (https://console.aukiverse.com/).
    /// Use the generated app key & app secret instead of placeholders "insert_app_key_here" and "insert_app_secret_here" when initializing _conjureKit.
    /// * On start the demo will join a new session.
    /// * Labels will indicate joined session id & the ConjureKit state.
    /// * The InputField in the top right can be used to join another existing session by its id.
    /// * When in a session with another participant(s) the Send Msg button will broadcast a small string message to them.
    /// The participants will be subscribed to the ConjureKit OnCustomMessageBroadcast callback and will log it.
    /// * The Leave button will disconnect you from the current session and join new one.
    /// </summary>
    public class Main : MonoBehaviour
    {
        /// <summary>
        /// Transform of the camera GameObject. Required by ConjureKit.
        /// </summary>
        public Transform cameraTransform;

        /// <summary>
        /// Label that shows currently joined session id.
        /// </summary>
        public Text sessionInfo;
        
        /// <summary>
        /// Label that shows current ConjureKit state.
        /// </summary>
        public Text conjureKitStateInfo;
        
        /// <summary>
        /// When pressed will leave the current ConjureKit session.
        /// </summary>
        public Button leaveButton;
        
        /// <summary>
        /// When pressed will send a custom string message to other session participants.
        /// </summary>
        public Button sendMessageButton;
        
        /// <summary>
        /// When pressed will try to join session with id from customSessionInputField.
        /// </summary>
        public Button joinCustomSessionButton;
        
        /// <summary>
        /// Input field that allows you to specify session to join by id.
        /// If the session does not exist the participant will be redirected to a new one.
        /// </summary>
        public InputField customSessionInputField;
        
        private IConjureKit _conjureKit;
        private string _lastJoinedSessionId;

        private void Start()
        {
            // Initialize the SDK.
            _conjureKit = new ConjureKit(
                cameraTransform,
                "8e6f1bcc-1d4c-4bcc-bce2-67bd65f775d6",
                "9a5e53a8-4431-48b5-b201-f4a5efce8605769f1bdb-16c6-4e71-95bc-e0148dc63f32"
            );
            
            _conjureKit.OnJoined += session => sessionInfo.text = session.Id;

            _conjureKit.OnLeft += () => sessionInfo.text = "";

            // Subscribe to custom messages from other participants.
            _conjureKit.OnCustomMessageBroadcast += broadcast =>
            {
                var decodedMessage = Encoding.UTF8.GetString(broadcast.Body);
                Debug.Log($"Received custom message: {decodedMessage}");
            };

            _conjureKit.OnStateChanged += state =>
            {
                ToggleControlsState(state == State.JoinedSession || state == State.Calibrated);
                conjureKitStateInfo.text = state.ToString();
            };

            // Everything is set up so let's connect.
            _conjureKit.Connect();
        }

        private void ToggleControlsState(bool interactable)
        {
            leaveButton.interactable = interactable;
            sendMessageButton.interactable = interactable;
            joinCustomSessionButton.interactable = interactable;
        }

        /// <summary>
        /// Leaves the current session.
        /// </summary>
        public void LeaveSession()
        {
            Debug.Log("Leaving session.");
            _conjureKit.Disconnect();
            _conjureKit.Connect();
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
                },
                errorMsg =>
                {
                    Debug.LogWarning($"Failed to join session {targetSessionId}, connecting to a new session instead. ({errorMsg})");
                    _conjureKit.Connect();
                }
            );
        }

        /// <summary>
        /// Broadcasts a message that contains a simple string message to all participants in the session.
        /// The server will not be sending the message to the one that broadcast it.
        /// </summary>
        public void SendSampleMessageButtonPressed()
        {
            var encodedMessage = Encoding.UTF8.GetBytes($"Message from participant {_conjureKit.GetSession().ParticipantId}.");
            var participantIds = _conjureKit.GetSession().GetParticipantsIds().ToArray();
            _conjureKit.SendCustomMessage(participantIds, encodedMessage);
        }
    }
}
