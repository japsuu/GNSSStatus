namespace GNSSStatus.Configuration;

public class ConfigurationData
{
    // Connection
    public required string ServerAddress { get; set; }
    public required int ServerPort { get; set; }
    
    // Coordinate System
    public required int GkValue { get; set; }
    
    //Rover base point
    public required string StaticX { get; set; }
    public required string StaticY { get; set; }
    public required string StaticZ { get; set; }
    
    // MQTT
    public required string MqttBrokerAddress { get; set; }
    public required int MqttBrokerPort { get; set; }
    public required string MqttBrokerChannelAltitude { get; set; }
    public required string MqttUsername { get; set; }
    public required string MqttPassword { get; set; }
    public required string MqttClientId { get; set; }
    public required bool UseTls { get; set; }
}