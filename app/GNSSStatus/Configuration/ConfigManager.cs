using GNSSStatus.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GNSSStatus.Configuration;

public static class ConfigManager
{
    private const string CONFIG_PATH = "assets/config.yaml";

    public const int MAX_JSON_PAYLOAD_LENGTH = 255;
    public const int MAX_COMBINED_PAYLOAD_COUNT = 8;
    public const int MQTT_MAX_PAYLOAD_SIZE_BYTES = 2999;
    public const string FINPOS_IONO_IMAGE_URL = "https://finpos.nls.fi/iono/ir_ipi_ip0/PROD_seven2three_IPI.png";

    public static ConfigurationData CurrentConfiguration { get; private set; } = null!;


    public static bool TryLoadConfiguration()
    {
        // Check that the config file exists.
        if (!File.Exists(CONFIG_PATH))
        {
            Logger.LogWarning("Configuration file not found, creating default configuration.");
            CreateDefaultConfiguration();
            return false;
        }
        
        IDeserializer deserializer = new DeserializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .WithEnforceRequiredMembers()
            .Build();

        try
        {
            CurrentConfiguration = deserializer.Deserialize<ConfigurationData>(File.ReadAllText(CONFIG_PATH));
            
            if (CurrentConfiguration == null)
                throw new Exception("Deserialization failed.");
            
            if (!ConfigurationData.Verify(CurrentConfiguration))
                throw new Exception("Configuration verification failed.");
        }
        catch (Exception e)
        {
            Logger.LogError($"Failed to load configuration: {e.Message}");
            Logger.LogWarning("Rename or delete the current config file, to create a new configuration with default values on the next startup.");
            return false;
        }
        
        Logger.LogInfo("Configuration loaded successfully.");
        return true;
    }


    private static void CreateDefaultConfiguration()
    {
        ConfigurationData defaultConfig = ConfigurationData.GetDefault();
        
        ISerializer serializer = new SerializerBuilder()
            .WithNamingConvention(PascalCaseNamingConvention.Instance)
            .Build();
        
        File.WriteAllText(CONFIG_PATH, serializer.Serialize(defaultConfig));
        
        string configPath = Path.GetFullPath(CONFIG_PATH);
        Logger.LogInfo($"Default configuration created. Please edit the configuration file ({configPath}) and restart the application.");
    }
}