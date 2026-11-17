namespace Assets
{
    using System.Text;
    using M2MqttUnity;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using UnityEngine;

    public class MQTTClientSubscriber : M2MqttUnityClient
    {
        [Header("Room Twin Binding")]
        [SerializeField] private string topic = "building/room1/environment/kw";
        [SerializeField] private TemperatureRenderer roomTwin;

        protected override void SubscribeTopics()
        {
            base.SubscribeTopics();
            client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            ushort msgId = client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
            Debug.Log($"Subscribed to MQTT topic: {topic}");
            Debug.Log($"Subscribed to '{topic}' (msgId={msgId})");
        }

        protected override void DecodeMessage(string receivedTopic, byte[] message)
        {
            Debug.Log("Decode message");
            base.DecodeMessage(receivedTopic, message);

            if (receivedTopic != topic)
            {
                Debug.Log("No topic received");
                return;
            }
              

            string payload = Encoding.UTF8.GetString(message);
            Debug.Log($"[MQTT RX] {receivedTopic}: {payload}");

            if (roomTwin != null)
            {
                roomTwin.OnMqttPayloadReceived(payload);
            }
            else
            {
                Debug.LogWarning("MQTTClientSubscriber: 'Room Twin' reference is not assigned in the Inspector.");
            }
        }
    }
}
