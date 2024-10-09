﻿using GNSSStatus.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GNSSStatus.Configuration;

public static class ConfigManager
{
    private const string CONFIG_PATH = "assets/config.yaml";
    private static bool createdDefaultConfiguration = false;

    public static ConfigurationData CurrentConfiguration { get; private set; } = null!;


    public static void LoadConfiguration()
    {
        // Check that the config file exists.
        if (!File.Exists(CONFIG_PATH))
        {
            Logger.LogWarning("Configuration file not found, creating default configuration.");
            CreateDefaultConfiguration();
        }
        
        IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithEnforceRequiredMembers()
            .Build();

        try
        {
            CurrentConfiguration = deserializer.Deserialize<ConfigurationData>(File.ReadAllText(CONFIG_PATH));
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to load configuration: {e.Message}");
            
            if (!createdDefaultConfiguration)
            {
                Logger.LogWarning("Overwriting configuration with default values.");
                CreateDefaultConfiguration();
            }
        }
        
        Logger.LogInfo("Configuration loaded successfully.");
    }


    private static void CreateDefaultConfiguration()
    {
        ConfigurationData defaultConfig = new()
        {
            ServerAddress = "192.168.1.42",
            ServerPort = 2999,
            GkValue = 21,
            //Rover antenna base point, measured with total station
            StaticX = "6996389.622",
            StaticY = "21533297.613",
            StaticZ = "12.220",
            MqttBrokerAddress = "mqtt3.thingspeak.com",
            MqttBrokerPort = 8883,
            MqttBrokerChannelAltitude = "channels/2688542/publish/fields/field1",
            MqttUsername = "username",
            MqttPassword = "password",
            MqttClientId = "clientID",
            UseTls = true
        };
        
        ISerializer serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        
        File.WriteAllText(CONFIG_PATH, serializer.Serialize(defaultConfig));
        createdDefaultConfiguration = true;
        
        string configPath = Path.GetFullPath(CONFIG_PATH);
        Logger.LogInfo($"Default configuration created. Please edit the configuration file ({configPath}) and restart the application.");
            
        // Exit
        Environment.Exit(0);
    }
}