namespace GNSSStatus.Configuration;

public class ConfigurationData
{
    // Connection
    public string ServerAddress { get; set; }
    public int ServerPort { get; set; }
    
    // MQTT
    public string MqttBrokerAddress { get; set; }
    public int MqttBrokerPort { get; set; }
    public string MqttUsername { get; set; }
    public string MqttPassword { get; set; }
}