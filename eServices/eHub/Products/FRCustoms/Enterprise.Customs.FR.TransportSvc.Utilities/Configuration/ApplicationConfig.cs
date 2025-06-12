using System;
using System.Configuration;

namespace Enterprise.Customs.FR.TransportSvc.Utilities
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
	public sealed class ApplicationConfig
	{
		public static ApplicationConfig Instance => instance.Value;
		static readonly Lazy<ApplicationConfig> instance = new Lazy<ApplicationConfig>(() => new ApplicationConfig());

		ApplicationConfig()
		{
			TimerInterval = ConfigurationManager.AppSettings["TimerInterval"];
			FTP_Host = ConfigurationManager.AppSettings["FTP_Host"];
			FTP_Port = Convert.ToUInt16(ConfigurationManager.AppSettings["FTP_Port"]);
			FTP_User = ConfigurationManager.AppSettings["FTP_User"];
			FTP_PrivateKeyFileDirectory = ConfigurationManager.AppSettings["FTP_PrivateKeyFileDirectory"];
			FTPReceptionDirectory = ConfigurationManager.AppSettings["FTPReceptionDirectory"];
			FTPSendDirectory = ConfigurationManager.AppSettings["FTPSendDirectory"];
			LogPath = ConfigurationManager.AppSettings["LogPath"];
			DisplayMessagesInConsole = ConfigurationManager.AppSettings["DisplayMessagesInConsole"];
			VerboseMode = ConfigurationManager.AppSettings["VerboseMode"];
			Taille_max_logs = ConfigurationManager.AppSettings["taille_max_logs"];
			RunningMode = ConfigurationManager.AppSettings["RunningMode"];
			EHubAddress = ConfigurationManager.AppSettings["eHubAddress"];
			EHubForTestAddress = ConfigurationManager.AppSettings["eHubForTestAddress"];
			EHubProductionLogin = ConfigurationManager.AppSettings["eHubProductionLogin"];
			EHubProductionPassword = ConfigurationManager.AppSettings["eHubProductionPassword"];
			EHubTestLogin = ConfigurationManager.AppSettings["eHubTestLogin"];
			EHubTestPassword = ConfigurationManager.AppSettings["eHubTestPassword"];
			WorkingDirectory = ConfigurationManager.AppSettings["WorkingDirectory"];
			ExecuteForDebugging = ConfigurationManager.AppSettings["ExecuteForDebugging"];
			APPLUSMode = ConfigurationManager.AppSettings["APPLUSMode"];
			EHubProductionEnabled = ConfigurationManager.AppSettings["eHubProductionEnabled"];
			EHubTestEnabled = ConfigurationManager.AppSettings["eHubTestEnabled"];
			ClientsUsingeHubForTest = ConfigurationManager.AppSettings["ClientsUsingeHubForTest"];
		}

		public string TimerInterval { get; set; }
		public string FTP_Host { get; set; }
		public int FTP_Port { get; set; }
		public string FTP_User { get; set; }
		public string FTP_PrivateKeyFileDirectory { get; set; }
		public string FTPReceptionDirectory { get; set; }
		public string FTPSendDirectory { get; set; }
		public string LogPath { get; set; }
		public string DisplayMessagesInConsole { get; set; }
		public string VerboseMode { get; set; }
		public string Taille_max_logs { get; set; }
		public string RunningMode { get; set; }
		public string EHubAddress { get; set; }
		public string EHubForTestAddress { get; set; }
		public string EHubProductionLogin { get; set; }
		public string EHubProductionPassword { get; set; }
		public string EHubTestLogin { get; set; }
		public string EHubTestPassword { get; set; }
		public string EHubProductionEnabled { get; set; }
		public string EHubTestEnabled { get; set; }
		public string WorkingDirectory { get; set; }
		public string ExecuteForDebugging { get; set; }
		public string APPLUSMode { get; set; }
		public string ClientsUsingeHubForTest { get; set; }
	}
}
