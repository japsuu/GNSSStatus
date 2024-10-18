using YamlDotNet.Serialization;

namespace GNSSStatus.Configuration;

public class ConfigurationData
{
    // Connection
    [YamlMember(Description = "The address of the NMEA server.")]
    public required string ServerAddress { get; set; }
    [YamlMember(Description = "The port of the NMEA server.")]
    public required int ServerPort { get; set; }
    
    // Coordinate System
    [YamlMember(Description = "The GK system number.")]
    public required int GkSystemNumber { get; set; }
    
    // Static rover antenna base point
    [YamlMember(Description = "The static X coordinate of the rover antenna base point, in GK coordinates.")]
    public required double RoverLocationX { get; set; }
    [YamlMember(Description = "The static Y coordinate of the rover antenna base point, in GK coordinates.")]
    public required double RoverLocationY { get; set; }
    [YamlMember(Description = "The static Z coordinate of the rover antenna base point, in GK coordinates.")]
    public required double RoverLocationZ { get; set; }
    
    // MQTT
    [YamlMember(Description = "The address of the MQTT broker to send sensor data to.")]
    public required string MqttBrokerAddress { get; set; }
    [YamlMember(Description = "The port of the MQTT broker.")]
    public required int MqttBrokerPort { get; set; }
    [YamlMember(Description = "The topic to publish sensor data to.")]
    public required string MqttBrokerTopic { get; set; }
    [YamlMember(Description = "The client ID to use when connecting to the MQTT broker.")]
    public required string MqttClientId { get; set; }
    [YamlMember(Description = "The username to use when connecting to the MQTT broker.")]
    public required string MqttUsername { get; set; }
    [YamlMember(Description = "The password to use when connecting to the MQTT broker.")]
    public required string MqttPassword { get; set; }
    [YamlMember(Description = "Whether to use TLS when connecting to the MQTT broker.")]
    public required bool UseTls { get; set; }
    
    // Other
    [YamlMember(Description = "The interval in seconds to send data to the MQTT broker.")]
    public required int DataSendIntervalSeconds { get; set; }
    [YamlMember(Description = "The interval in seconds to parse the Iono graph.")]
    public required int IonoParseIntervalSeconds { get; set; }


    public static ConfigurationData GetDefault()
    {
        ConfigurationData defaultConfig = new()
        {
            ServerAddress = "192.168.1.42",
            ServerPort = 2999,
            GkSystemNumber = 21,
            RoverLocationX = 6996389.622,
            RoverLocationY = 21533297.613,
            RoverLocationZ = 12.220,
            MqttBrokerAddress = "mqtt3.thingspeak.com",
            MqttBrokerPort = 1883,
            MqttBrokerTopic = "channels/2688542/publish",
            MqttClientId = "clientID",
            MqttUsername = "username",
            MqttPassword = "password",
            UseTls = false,
            DataSendIntervalSeconds = 30,
            IonoParseIntervalSeconds = 60,
        };
        
        return defaultConfig;
    }
}