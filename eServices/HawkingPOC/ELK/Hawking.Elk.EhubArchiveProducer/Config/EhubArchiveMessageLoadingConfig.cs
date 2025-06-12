using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Hawking.Elk.EhubArchiveProducer.Config
{
    internal static class EhubArchiveMessageLoadingConfig
    {
        #region Member Variables

        const string AppSettingsKeyArchivedUTC = "LastRead_ArchivedUTC";
        const string AppSettingKeyLoadBatchSize = "LoadBatchSize";

        const string ArchivedUTCDateTimeFormat = "u";
        const int DefaultLoadBatchSize = 100;

        const string DefaultConnectionString = "ArchiveDbContext";

        static Configuration AppConfig;
        static DateTime lastReadArchivedUtc;

        #endregion

        #region Constructor

        static EhubArchiveMessageLoadingConfig()
        {
            ProcessId = Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
            StationId = Environment.MachineName;

            AppConfig = OpenAppConfig();

            DataSource = GetDataSource(ConfigurationManager.ConnectionStrings[DefaultConnectionString].ConnectionString);

            if (AppConfig.AppSettings.Settings[AppSettingsKeyArchivedUTC] == null)
            {
                AppConfig.AppSettings.Settings.Add(AppSettingsKeyArchivedUTC, string.Empty);
                UpdateLastReadArchivedUtc(DateTime.UtcNow.AddYears(-3));
            }
            else
            {
                if (DateTime.TryParse(AppConfig.AppSettings.Settings[AppSettingsKeyArchivedUTC].Value, out DateTime result))
                {
                    lastReadArchivedUtc = result;
                }
            }

            if (AppConfig.AppSettings.Settings[AppSettingKeyLoadBatchSize] == null)
            {
                AppConfig.AppSettings.Settings.Add(AppSettingKeyLoadBatchSize, DefaultLoadBatchSize.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                if (int.TryParse(AppConfig.AppSettings.Settings[AppSettingKeyLoadBatchSize].Value, out int result))
                {
                    LoadBatchSize = result;
                }
            }
        }

        static Configuration OpenAppConfig()
        {
            return ConfigurationManager.OpenExeConfiguration(Path.Combine(Environment.CurrentDirectory, Assembly.GetEntryAssembly().ManifestModule.Name));
        }

        static string GetDataSource(string connectionString)
        {
            var match = Regex.Match(connectionString, "data source=(.*?);");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return string.Empty;
        }

        static string GetAppSetting(string key)
        {
            if (AppConfig.AppSettings.Settings[key] != null)
            {
                return AppConfig.AppSettings.Settings[key].Value.ToString();
            }

            return string.Empty;
        }

        #endregion

        #region Properties

        public static DateTime LastReadArchivedUtc => lastReadArchivedUtc;
        public static int LoadBatchSize { get; private set; }
        public static string DataSource { get; set; }
        public static string StationId { get; private set; }
        public static string ProcessId { get; private set; }

        #endregion

        #region Methods

        internal static void UpdateLastReadArchivedUtc(DateTime lastReadMessageAchivedUtc)
        {
            if (lastReadArchivedUtc != lastReadMessageAchivedUtc)
            {
                lastReadArchivedUtc = lastReadMessageAchivedUtc;
                AppConfig.AppSettings.Settings[AppSettingsKeyArchivedUTC].Value = lastReadMessageAchivedUtc.ToString(ArchivedUTCDateTimeFormat);

                AppConfig.Save(ConfigurationSaveMode.Modified);
            }
        }

        #endregion
    }
}
