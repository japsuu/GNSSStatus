using System.Globalization;
using GNSSStatus.Configuration;
using GNSSStatus.Coordinates;
using GNSSStatus.Networking;
using GNSSStatus.Utils;
using MQTTnet;
using MQTTnet.Client;

namespace GNSSStatus;

internal static class Program
{
    private const int MQTT_SEND_INTERVAL_MILLIS = 15000;    // 15 seconds.
    
    private static GKCoordinate? lastGkCoordinate = null;
    
    
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
            
            if (lastGkCoordinate == null)
                continue;

            double timeSinceLastSend = TimeUtils.GetTimeMillis() - lastSendTime;
            if (timeSinceLastSend < MQTT_SEND_INTERVAL_MILLIS)
                continue;
            
            await SendMqttMessage(mqttClient, lastGkCoordinate.Value.Z.ToString());
            
            lastSendTime = TimeUtils.GetTimeMillis();
        }
    }


#region Sentence Processing

    private static void HandleSentence(Nmea0183Sentence sentence)
    {
        if (sentence.Type == Nmea0183SentenceType.GGA)
        {
            /*
                Message ID $GPGGA
                1 UTC of position fix
                2	Latitude
                3	Direction of latitude:
                N: North
                S: South
                
                4	Longitude
                5	Direction of longitude:
                E: East
                W: West
                
                6	GPS Quality indicator:
                0: Fix not valid
                1: GPS fix
                2: Differential GPS fix (DGNSS), SBAS, OmniSTAR VBS, Beacon, RTX in GVBS mode
                3: Not applicable
                4: RTK Fixed, xFill
                5: RTK Float, OmniSTAR XP/HP, Location RTK, RTX
                6: INS Dead reckoning
                
                7	Number of SVs in use, range from 00 through to 24+
                8	HDOP
                9	Orthometric height (MSL reference)
                10	M: unit of measure for orthometric height is meters
                11	Geoid separation
                12	M: geoid separation measured in meters
                13	Age of differential GPS data record, Type 1 or Type 9. Null field when DGPS is not used.
                14	Reference station ID, range 0000 to 4095. A null field when any reference station ID is selected and no corrections are received.
                        See table below for a description of the field values.
                15	The checksum data, always begins with *
            */
            string[] parts = sentence.Data.Split(',');

            if (parts.Length < 10)
                return;
            
            string altitude = parts[9];
            string altitudeUnit = parts[10];
            string utcTime = parts[1];
            string latitudi = parts[2];
            string directionLatitudi = parts[3];
            string longitudi = parts[4];
            string directionLongitudi = parts[5];
            string quality = parts[6];

            GKCoordinate gk = CoordinateConverter.ConvertToGk(latitudi, longitudi, directionLatitudi, directionLongitudi, 21, altitude);

            Logger.LogInfo($"GK21 X: {gk.N.ToString("#.000")} Y: {gk.E.ToString("#.000")} N2000 Korkeus: {gk.Z.ToString("#.000")}");
            
            lastGkCoordinate = gk;
        }
    }

#endregion


#region MQTT

    private static IMqttClient CreateMqttClient()
    {
        Logger.LogInfo("Creating MQTT client...");
        
        MqttFactory factory = new();
        IMqttClient? mqttClient = factory.CreateMqttClient();
        
        Logger.LogInfo("MQTT client created.");
        return mqttClient;
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


    private static async Task SendMqttMessage(IMqttClient mqttClient, string payload)
    {
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