using GNSSStatus.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GNSSStatus.Configuration;

public static class ConfigManager
{
    private const string CONFIG_PATH = "assets/config.yaml";
    private static bool createdDefaultConfiguration = false;

    public const int MAX_JSON_PAYLOAD_LENGTH = 255;
    public const int MAX_COMBINED_PAYLOAD_COUNT = 8;
    public const int MQTT_SEND_INTERVAL_MILLIS = 15000;    // 15 seconds.
    public const int MQTT_MAX_PAYLOAD_SIZE_BYTES = 2999;

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
        ConfigurationData defaultConfig = ConfigurationData.GetDefault();
        
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