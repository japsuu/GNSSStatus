using GNSSStatus.Configuration;
using GNSSStatus.Networking;
using GNSSStatus.Nmea;
using MQTTnet;
using MQTTnet.Client;

namespace GNSSStatus;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        ConfigManager.LoadConfiguration();
        
        // Create a new MQTT client.
        IMqttClient mqttClient = CreateMqttClient();

        // Connect to the MQTT broker.
        await ConnectMqttBroker(mqttClient);
        
        // Create a new NMEA client.
        using NmeaClient client = new(ConfigManager.CurrentConfiguration.ServerAddress, ConfigManager.CurrentConfiguration.ServerPort);
        
        // Read the latest received NMEA sentence from the server.
        foreach (Nmea0183Sentence sentence in client.ReadSentence())
        {
            await HandleSentence(mqttClient, sentence);
        }
    }


    private static async Task ConnectMqttBroker(IMqttClient mqttClient)
    {
        Logger.LogInfo("Connecting to MQTT broker...");

        MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder()
            .WithTcpServer(ConfigManager.CurrentConfiguration.MqttBrokerAddress, ConfigManager.CurrentConfiguration.MqttBrokerPort)
            .WithCredentials(ConfigManager.CurrentConfiguration.MqttUsername, ConfigManager.CurrentConfiguration.MqttPassword)
            .WithClientId(ConfigManager.CurrentConfiguration.MqttClientId)
            .WithTimeout(TimeSpan.FromSeconds(10));
        
        MqttClientTlsOptions tlsOptions = new MqttClientTlsOptionsBuilder().UseTls().Build();
        
        MqttClientOptions mqttClientOptions = ConfigManager.CurrentConfiguration.UseTls
            ? builder.WithTlsOptions(tlsOptions).Build()
            : builder.Build();
        
        try
        {
            await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        }
        catch (Exception e)
        {
            Logger.LogException("Failed to connect to MQTT broker", e);
        }
        
        Logger.LogInfo("Connected to MQTT broker.");
    }


    private static IMqttClient CreateMqttClient()
    {
        Logger.LogInfo("Creating MQTT client...");
        
        MqttFactory factory = new();
        IMqttClient? mqttClient = factory.CreateMqttClient();
        
        Logger.LogInfo("MQTT client created.");
        return mqttClient;
    }


    private static async Task HandleSentence(IMqttClient mqttClient, Nmea0183Sentence sentence)
    {
        if (sentence.Type == Nmea0183SentenceType.GGA)
        {
            string[] parts = sentence.Data.Split(',');

            if (parts.Length < 10)
                return;
            
            string altitude = parts[9];
            string altitudeUnit = parts[10];

            Logger.LogInfo($"Altitude: {altitude} {altitudeUnit}");
            
            await SendMqttMessage(mqttClient, altitude);
        }
    }


    private static async Task SendMqttMessage(IMqttClient mqttClient, string altitude)
    {
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
            .WithTopic(ConfigManager.CurrentConfiguration.MqttBrokerChannelAltitude)
            .WithPayload(altitude)
            .Build();

        await mqttClient.PublishAsync(message, CancellationToken.None);
    }
}