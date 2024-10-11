namespace GNSSStatus.Configuration;

public class ConfigurationData
{
    // Connection
    public required string ServerAddress { get; set; }
    public required int ServerPort { get; set; }
    
    // Coordinate System
    public required int GkSystemNumber { get; set; }
    
    // Static rover antenna base point
    public required double RoverLocationX { get; set; }
    public required double RoverLocationY { get; set; }
    public required double RoverLocationZ { get; set; }
    
    // MQTT
    public required string MqttBrokerAddress { get; set; }
    public required int MqttBrokerPort { get; set; }
    public required string MqttBrokerTopic { get; set; }
    public required string MqttClientId { get; set; }
    public required string MqttUsername { get; set; }
    public required string MqttPassword { get; set; }
    public required bool UseTls { get; set; }


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
            UseTls = false
        };
        
        return defaultConfig;
    }
}