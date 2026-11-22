namespace Assets
{
    using M2MqttUnity;
    using System;
    using System.Text;
    using UnityEngine;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using static ButtonComponent;
    using static TemperatureRenderer;

    public class MQTTClientSubscriber : M2MqttUnityClient
    {

       // [SerializeField] private string topic = "building/room1/environment/kw";
        //[SerializeField] private TemperatureRenderer roomTwin;
        [SerializeField] private EventHub eventHub;

        [Header("MQTT Topics")]
        public static string environmentTopic = "building/room1/environment/kw";
        public static string buttonInputTopic = "bridge/node01/input/button";
        public static string servoCommandTopic = "bridge/node01/command/servo";
        public static string servoStateTopic = "bridge/node01/state/servo";

        //public event Action<string> OnButtonPressed;
        //public event Action<float> OnServoStateReported;
        //public event Action<EnvironmentPayload> OnEnvironmentTelemetryReceived;

        protected override void SubscribeTopics()
        {
            //base.SubscribeTopics();
            //client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            //ushort msgId = client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            //Debug.Log($"Subscribed to MQTT topic: {topic}");
            //Debug.Log($"Subscribed to '{topic}' (msgId={msgId})");

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
    }
}
