using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;

namespace Prizm_Start_Utility
{
    public class AppConfig
    {
        public string PrizmDocDirectory { get; set; }
        public string LicensePath { get; set; }
        public string DefaultLicenseSKU { get; set; }
        public DatabaseConfig Database { get; set; }
        public MsOfficeConfig MsOffice { get; set; }
        public PasConfig PAS { get; set; }
        public DockerConfig Docker { get; set; }
    }

    public class DatabaseConfig
    {
        public string Adapter {  get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host {  get; set; }
        public string Port { get; set; }
        public string Schema { get; set; }
    }

    public class MsOfficeConfig
    {
        public string Host { get; set; }
        public string Port { get; set; }
    }

    public class PasConfig
    {
        public PccServerConfig PCCServer { get; set; }
    }

    public class PccServerConfig
    {
        public string Host { get; set; }
        public string Port { get; set; }
    }

    public class DockerConfig
    {
        public string Namespace { get; set; }
        public string ServerRepository { get; set; }
        public string PASRepository { get; set; }
    }

    public class ConfigLoader
    {
        public static AppConfig LoadConfig(string configFilePath)
        {
            string json = File.ReadAllText(configFilePath);
            AppConfig config = JsonConvert.DeserializeObject<AppConfig>(json);
            return config;
        }

        public static void SaveConfig(AppConfig config, string configFilePath)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configFilePath, json);
        }
    }
}