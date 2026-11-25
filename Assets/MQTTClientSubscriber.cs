namespace Assets
{
    using M2MqttUnity;
    using System;
    using System.Text;
    using TMPro;
    using UnityEngine;
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using static ButtonComponent;
    using static TemperatureRenderer;

    public class MQTTClientSubscriber : M2MqttUnityClient
    {

        [SerializeField] private EventHub eventHub;
        [SerializeField] private TextMeshProUGUI connectionText;
        private bool IsBrokerConnected => client != null && client.IsConnected;

        [Header("MQTT Topics")]
        public static string environmentTopic = "building/room1/environment/kw";
        public static string buttonInputTopic = "bridge/node01/input/button";
        public static string servoCommandTopic = "bridge/node01/command/servo";
        public static string servoStateTopic = "bridge/node01/state/servo";

        private float reconnectCooldownSeconds = 3.0f;
        private float _nextReconnectTime;
        private bool _pendingReconnect;

        private void OnEnable()
        {
            if (eventHub != null)
            {
                eventHub.Setup(PublishServoCommand);
            }
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            connectionText.text = "Disconnected";
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            connectionText.text = "Connected";
        }

        protected override void OnConnectionLost()
        {
            base.OnConnectionLost();
            connectionText.text = "Connection Lost";

            ScheduleReconnect();
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
            connectionText.text = "Connecting...";
    
        }
        protected override void OnConnectionFailed(string errorMessage)
        {
            base.OnConnectionFailed(errorMessage);
            connectionText.text = "Connection Failed. Retry Connection.";
            ScheduleReconnect();
        }

        // for reconnect button
        public void ReconnectClient()
        {
            base.Disconnect();
            base.Connect();
        }
        
        private void ScheduleReconnect()
        {
            _pendingReconnect = true;
            _nextReconnectTime = Time.unscaledTime + reconnectCooldownSeconds;
        }

        protected override void Update()
        {
            base.Update();

            if (!_pendingReconnect) return;

            if (client != null && client.IsConnected)
            {
                _pendingReconnect = false;
                return;
            }

            if (Time.unscaledTime >= _nextReconnectTime)
            {
                _pendingReconnect = false;
                connectionText.text = "Attempting to Reconnect...";
                ReconnectClient();
            }
        }

        protected override void SubscribeTopics()
        {
            base.SubscribeTopics();

            string[] topicsToSubscribe = new string[]
            {
                environmentTopic,
                buttonInputTopic,
                servoStateTopic
            };

            byte[] qosLevels = new byte[]
            {
                MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
                MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
                MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE
            };

            client.Subscribe(topicsToSubscribe, qosLevels);
            Debug.Log($"Subscribed to: {string.Join(", ", topicsToSubscribe)}");
        }

        protected override void DecodeMessage(string receivedTopic, byte[] message)
        {
            base.DecodeMessage(receivedTopic, message);

            string payload = Encoding.UTF8.GetString(message);
            Debug.Log($"[MQTT RX] {receivedTopic}: {payload}");

            if (eventHub != null)
            {
                eventHub.HandleIncomingMqttMessage(receivedTopic, payload);
            }
        }

        public void PublishServoCommand(float targetAngle)
        {
            int clampedAngle = Mathf.Clamp(Mathf.RoundToInt(targetAngle), 0, 180);

            if (client == null || !client.IsConnected)
            {
                Debug.LogWarning($"Cannot publish servo command. clientNull={client == null}, isConnected={client?.IsConnected}");
                return;
            }

            string topic = !string.IsNullOrWhiteSpace(servoCommandTopic)
                ? servoCommandTopic
                : "bridge/node01/command/servo";

            string payload = $"{{\"target_angle\":{clampedAngle}}}";
            byte[] body = Encoding.UTF8.GetBytes(payload);

            client.Publish(topic, body, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
            Debug.Log($"[MQTT TX] {topic} -> {payload}");
        }
    }
}
