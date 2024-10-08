using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GNSSStatus.Configuration;

public static class ConfigManager
{
    private const string CONFIG_PATH = "config.yaml";

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
            .Build();

        try
        {
            CurrentConfiguration = deserializer.Deserialize<ConfigurationData>(File.ReadAllText(CONFIG_PATH));
        }
        catch (Exception e)
        {
            Logger.LogException("Failed to load configuration", e);
        }
        
        Logger.LogInfo("Configuration loaded successfully.");
    }


    private static void CreateDefaultConfiguration()
    {
        ConfigurationData defaultConfig = new()
        {
            ServerAddress = "192.168.1.42",
            ServerPort = 2999,
            MqttBrokerAddress = "mqtt3.thingspeak.com",
            MqttBrokerPort = 1883,
            MqttUsername = "username",
            MqttPassword = "password"
        };
        
        ISerializer serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        
        File.WriteAllText(CONFIG_PATH, serializer.Serialize(defaultConfig));
    }
}