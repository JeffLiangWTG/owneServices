using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Unity;

namespace Hawking.Unity.Config
{
    public static class ApplicationConfig
    {
        public static IConfiguration Configuration { get; set; }

        public static void Initialise(string jsonFileSettings = "appsettings.json")
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), jsonFileSettings)))
            {
                throw new FileNotFoundException("The required appSettings.json config file is not found.");
            }

            if (Configuration == null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(jsonFileSettings)
                    .AddEnvironmentVariables();

                Configuration = builder.Build();
                DependencyFactory.Container.RegisterInstance<IConfiguration>(Configuration);
            }
        }

        public static bool TryParse(string key, out int value)
        {
            value = int.MinValue;
            if (TryParse(key, out string stringValue))
            {
                return int.TryParse(stringValue, out value);
            }

            return false;
        }

        public static bool TryParse(string key, out double value)
        {
            value = double.NaN;
            if (TryParse(key, out string stringValue))
            {
                return double.TryParse(stringValue, out value);
            }

            return false;
        }

        public static bool TryParse(string key, out DateTime value)
        {
            value = DateTime.MinValue;
            if (TryParse(key, out string stringValue))
            {
                return DateTime.TryParse(stringValue, out value);
            }

            return false;
        }

        public static bool TryParse(string key, out bool value)
        {
            value = false;
            if (TryParse(key, out string stringValue))
            {
                return bool.TryParse(stringValue, out value);
            }

            return false;
        }

        public static bool TryParse(string key, out string value)
        {
            Initialise();

            if (Configuration == null)
            {
                value = null;
                return false;
            }

            value = Configuration[key];
            return true;
        }
    }
}
