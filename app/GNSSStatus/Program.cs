#define MQTT_ENABLE

using System.Globalization;
using System.Net;
using System.Text;
using GNSSStatus.Configuration;
using GNSSStatus.Coordinates;
using GNSSStatus.Networking;
using GNSSStatus.Parsing;
using GNSSStatus.Utils;
using MQTTnet;
using MQTTnet.Client;

namespace GNSSStatus;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        InitializeThreadCultureInfo();
        ConfigManager.LoadConfiguration();
        
        using IMqttClient mqttClient = CreateMqttClient();
        using NmeaClient nmeaClient = new(ConfigManager.CurrentConfiguration.ServerAddress, ConfigManager.CurrentConfiguration.ServerPort);
        
        // Run the main loop.
        await Run(nmeaClient, mqttClient);
    }


    /// <summary>
    /// The main (infinite) loop of the program.
    /// </summary>
    /// <param name="nmeaClient">The NMEA client to read data from.</param>
    /// <param name="mqttClient">The MQTT client to send data to.</param>
    private static async Task Run(NmeaClient nmeaClient, IMqttClient mqttClient)
    {
        // Connect to the MQTT broker.
        await ConnectMqttBroker(mqttClient);
        
        CoordinateConverter.create_dem();
        
        // Avoid sending the first message immediately.
        double lastSendTime = TimeUtils.GetTimeMillis() + 5000;
        
        // Read the latest received NMEA sentence from the server.
        foreach (Nmea0183Sentence sentence in nmeaClient.ReadSentence())
        {
            SentenceParser.Parse(sentence);
            
            double timeSinceLastSend = TimeUtils.GetTimeMillis() - lastSendTime;
            if (timeSinceLastSend < ConfigManager.MQTT_SEND_INTERVAL_MILLIS)
                continue;
            
            string payload = SentenceParser.ParsedData.GetPayloadJson();
            
            if (string.IsNullOrEmpty(payload))
            {
                Logger.LogWarning("Empty payload received. Skipping MQTT send.");
                continue;
            }
            
            await SendMqttMessage(mqttClient, payload);
            
            lastSendTime = TimeUtils.GetTimeMillis();
        }
    }


#region MQTT

    private static IMqttClient CreateMqttClient()
    {
#if !MQTT_ENABLE
        return null!;
#endif
        Logger.LogInfo("Creating MQTT client...");
        
        MqttFactory factory = new();
        IMqttClient? mqttClient = factory.CreateMqttClient();
        
        Logger.LogInfo("MQTT client created.");
        return mqttClient;
    }


    private static async Task ConnectMqttBroker(IMqttClient mqttClient)
    {
#if !MQTT_ENABLE
        return;
#endif
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


    private static async Task SendMqttMessage(IMqttClient mqttClient, string payload)
    {
#if !MQTT_ENABLE
        return;
#endif
            
        // Percent-encode the payload.
        payload = WebUtility.UrlEncode(payload);
        
        int bytes = Encoding.UTF8.GetByteCount(payload);
        
        if (bytes > ConfigManager.MQTT_MAX_PAYLOAD_SIZE_BYTES)
        {
            Logger.LogWarning($"Payload exceeds max supported byte size ({bytes}/{ConfigManager.MQTT_MAX_PAYLOAD_SIZE_BYTES}). Skipping.");
            return;
        }
        
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
            .WithTopic(ConfigManager.CurrentConfiguration.MqttBrokerTopic)
            .WithPayload(payload)
            .Build();

        await mqttClient.PublishAsync(message, CancellationToken.None);
        
        Logger.LogInfo($"Sent MQTT message ({bytes} bytes): {payload}");
    }

#endregion


#region Utility

    private static void InitializeThreadCultureInfo()
    {
        CultureInfo culture = new("en-US");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

#endregion
}