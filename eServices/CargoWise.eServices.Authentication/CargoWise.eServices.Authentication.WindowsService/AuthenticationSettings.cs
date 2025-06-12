using System;
using System.Configuration;
using System.IO;
using System.Threading;
using Common.Logging;

namespace CargoWise.eServices.Authentication.WindowsService
{
	public class AuthenticationSettings
	{
		public void LoadApplicationSettings(IFileSystemWatcherWrapper fileSystemWatcherWrapper)
		{
			SetFileWatcherOnSettingFile(fileSystemWatcherWrapper);
			LoadSettingsFromFile();
		}

		void SetFileWatcherOnSettingFile(IFileSystemWatcherWrapper fileSystemWatcherWrapper)
		{
			Configuration config = GetConfiguration();

			fileSystemWatcherWrapper.Path = Path.GetDirectoryName(config.FilePath);
			fileSystemWatcherWrapper.Filter = Path.GetFileName(config.FilePath);
			fileSystemWatcherWrapper.EnableRaisingEvents = true;

			fileSystemWatcherWrapper.Changed += OnSettingsFileChanged;
			fileSystemWatcherWrapper.Renamed += OnSettingsFileChanged;

			Logger.InfoFormat("Started monitoring settings file '{0}' in '{1}'", fileSystemWatcherWrapper.Filter, fileSystemWatcherWrapper.Path);
		}

		public virtual Configuration GetConfiguration()
		{
			return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
		}

		void LoadSettingsFromFile()
		{
			try
			{
				ConfigurationManager.RefreshSection("connectionStrings");
				ConfigurationManager.RefreshSection("appSettings");
				AUTH_DB_CONNECTION_STRING = ConfigurationManager.ConnectionStrings["AuthenticationDB"].ConnectionString;
				EDIPROD_DB_CONNECTION_STRING = ConfigurationManager.ConnectionStrings["ediProdDB"].ConnectionString;
				if (!int.TryParse(ConfigurationManager.AppSettings["IntervalSeconds"], out INTERVAL))
				{
					Logger.Info("Could not load the interval setting, the interval has been set to the default value(5 min).");
					INTERVAL = 300;
				}
			}
			catch (NullReferenceException e)
			{
				Logger.Error("Failed to read setting file", e);
			}
		}

		void OnSettingsFileChanged(object sender, FileSystemEventArgs e)
		{
			Logger.InfoFormat("Changes to settings file {0}\\{1} have been detected.", ((IFileSystemWatcherWrapper)sender).Path, ((IFileSystemWatcherWrapper)sender).Filter);
			try
			{
				LoadSettingsFromFile();
			}
			catch
			{
				var attempt = 0;
				while (++attempt < 5)
				{
					Thread.Sleep(1000);
					LoadSettingsFromFile();
				}
			}
		}

		public static AuthenticationSettings Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new AuthenticationSettings();
				}
				return instance;
			}
		}

		public string AUTH_DB_CONNECTION_STRING;
		public string EDIPROD_DB_CONNECTION_STRING;
		public int INTERVAL;
		static AuthenticationSettings instance;
		static readonly ILog Logger = LogManager.GetLogger(typeof(AuthenticationSettings));
	}
}
