# Smart Building Room Environmental Twin (ESP32 + MQTT + Unity)

A real-time room environmental digital twin that reads live temperature (`°F`) and relative humidity (`% RH`) from an **ESP32** and **DHT11** sensor node, publishes JSON telemetry over **MQTT** through a local **Eclipse Mosquitto** broker, and drives scene materials, `TextMeshPro` readouts, and stale/disconnected state indicators inside **Unity**.

> VIDEO

---

## System Architecture

```text
[DHT11 Sensor] --(GPIO 14 + pull-up)--> [DOIT ESP32 DevKit V1 (Wi-Fi)]
                                                  |
                                          MQTT Publish (5s)
                                        Port 1883 | JSON Payload
                                                  v
                                    [Local Eclipse Mosquitto Broker]
                                      (listener 1883, anonymous)
                                                  |
                                        MQTT Subscribe (QoS 1)
                                                  v
                             [M2MqttUnity / MQTTClientSubscriber.cs]
                                      (Main-Thread Queue + _lock)
                                                  |
                                                  v
                                    [TemperatureRenderer.cs]
                        (Material Colors, TMP Readouts, Stale Check)


```

# Example JSON Payload Schema
```
{
  "device": "esp32-env-node-01",
  "temperature_f": 73.4,
  "humidity": 31.0
}
```

# Setup

Microcontroller: DOIT ESP32 DEVKIT V1 (flash/serial over USB on COM3 in Windows)

Sensor: DHT11 temperature & humidity sensor on a breadboard

VCC / VIN: Row 3 (3.3V / VIN rail per sensor spec)

Data / Signal: Row 4 connected to GPIO 14 (#define DHTPIN 14) with a 10kΩ pull-up resistor between data and power

GND: Row 5 (GND)

Sketch Libraries: WiFi.h, DHT.h (Adafruit DHT sensor library), PubSubClient.h

Firmware Config: Set your 2.4 GHz Wi-Fi credentials, mqtt_server to your PC's local IPv4 address (e.g., 192.168.x.x), mqtt_port = 1883, and mqtt_topic = "building/room1/environment/kw".

local configuration file for Mosquitto broker:
```
listener 1883
allow_anonymous true
```
