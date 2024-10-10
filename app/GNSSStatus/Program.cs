// #define MQTT_ENABLE

using System.Globalization;
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
    private const int MQTT_SEND_INTERVAL_MILLIS = 15000;    // 15 seconds.
    
    private static readonly GNSSData LatestData = new();
    
    
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
        
        double lastSendTime = 0;
        
        // Read the latest received NMEA sentence from the server.
        foreach (Nmea0183Sentence sentence in nmeaClient.ReadSentence())
        {
            HandleSentence(sentence);
            
            Console.Clear();
            Logger.LogInfo(LatestData.ToString());
            
            double timeSinceLastSend = TimeUtils.GetTimeMillis() - lastSendTime;
            if (timeSinceLastSend < MQTT_SEND_INTERVAL_MILLIS)
                continue;
            
            GNSSPayload payload = LatestData.GetPayload();
            await SendMqttMessage(mqttClient, payload.ToJson());
            
            lastSendTime = TimeUtils.GetTimeMillis();
        }
    }


#region Sentence Processing

    private static void HandleSentence(Nmea0183Sentence sentence)
    {
        switch (sentence.Type)
        {
            case Nmea0183SentenceType.GGA:
            {
                if (sentence.Parts.Length < GGAData.LENGTH)
                {
                    Logger.LogWarning("Invalid GGA sentence received.");
                    return;
                }
                
                LatestData.GGA = new GGAData(sentence);
                break;
            }
            case Nmea0183SentenceType.GSA:
            {
                if (sentence.Parts.Length < GSAData.LENGTH)
                {
                    Logger.LogWarning("Invalid GSA sentence received.");
                    return;
                }
            
                LatestData.GSA = new GSAData(sentence);
                break;
            }
            case Nmea0183SentenceType.GST:
            {
                if (sentence.Parts.Length < GSTData.LENGTH)
                {
                    Logger.LogWarning("Invalid GST sentence received.");
                    return;
                }
            
                LatestData.GST = new GSTData(sentence);
                break;
            }
            case Nmea0183SentenceType.GSV:
            {
                if (sentence.Parts.Length < GSVData.LENGTH)
                {
                    Logger.LogWarning("Invalid GSV sentence received.");
                    return;
                }
            
                LatestData.GSV = new GSVData(sentence);
                break;
            }
            case Nmea0183SentenceType.NTR:
            {
                if (sentence.Parts.Length < NTRData.LENGTH)
                {
                    Logger.LogWarning("Invalid NTR sentence received.");
                    return;
                }
            
                LatestData.NTR = new NTRData(sentence);
                break;
            }
        }
    }

#endregion


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
        MqttApplicationMessage message = new MqttApplicationMessageBuilder()
            .WithTopic(ConfigManager.CurrentConfiguration.MqttBrokerChannelAltitude)
            .WithPayload(payload)
            .Build();

        await mqttClient.PublishAsync(message, CancellationToken.None);
        
        Logger.LogInfo($"Sent MQTT message: {payload}");
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