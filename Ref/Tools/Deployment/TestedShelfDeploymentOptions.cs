using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Dat.Integration.Deployment;

namespace CargoWise.RefDbRepo.Deployment
{
	public class TestedShelfDeploymentOptions
	{
		static class Options
		{
			public const string WebSiteName = nameof(WebSiteName);
			public const string RefDbRepoSafeRestoreFromBackup = nameof(RefDbRepoSafeRestoreFromBackup);
			public const string RefDbRepoStagingRestoreFromBackup = nameof(RefDbRepoStagingRestoreFromBackup);
			public const string SqlServer = nameof(SqlServer);
			public const string WebServer = nameof(WebServer);
			public const string UpdateServiceDomain = nameof(UpdateServiceDomain);
			public const string DeliveryServiceDomain = nameof(DeliveryServiceDomain);
			public const string UpdateServiceFilesSharedPath = nameof(UpdateServiceFilesSharedPath);
			public const string DeliveryServiceFilesSharedPath = nameof(DeliveryServiceFilesSharedPath);
			public const string DeployUsername = nameof(DeployUsername);
			public const string DeployPassword = nameof(DeployPassword);
			public const string RefDataRepo = nameof(RefDataRepo);
		}

		public static class Defaults
		{
			public const string WebServer = "sydsp-sweb-13.sand.wtg.zone";
			public const string UpdateServiceRootDomain = "refdbrepoupdate.testrig3.sand.wtg.zone";
			public const string DeliveryServiceRootDomain = "refdbrepo.testrig3.sand.wtg.zone";
			public const string RefDbRepoSafeRestoreFromBackup = "";
			public const string RefDbRepoStagingRestoreFromBackup = "";
			public const string WebServerSharedPath = "RefDataRepoTestRigs";
			public const string WebServerLocalPath = @"C:\ProgramData\WiseTech Global\RefDataRepoTestRigs"; // Please use this default folder since it has correct permissions
			public const string DeployUsername = "";
			public const string DeployPassword = "";

			public static string SQLDataServers
			{
				get
				{
					var result = string.Empty;
#if DEBUG
					result = "localhost";
#endif
					if (string.IsNullOrEmpty(result))
					{
						result = @"SYDSP-SSQL-5.sand.wtg.zone\INSTANCE1;SYDSP-SSQL-6.sand.wtg.zone\INSTANCE1";
					}
					return result;
				}
			}

			public static string SqlServer
			{
				get
				{
					if (!string.IsNullOrEmpty(_sqlServer))
					{
						return _sqlServer;
					}
					var sqlDataServers = SQLDataServers;
					if (string.IsNullOrEmpty(sqlDataServers))
					{
						throw new InvalidOperationException("TestRig has no sql database server configured.");
					}
					var availableDbServers = sqlDataServers.Split(';');
					var randomServerIndex = RandomNumberGenerator.GetInt32(availableDbServers.Length);
					_sqlServer = availableDbServers[randomServerIndex];
					return _sqlServer;
				}
			}
			static string _sqlServer;
		}

		public string WebSiteName { get; set; }
		public string RefDbRepoSafeRestoreFromBackup { get; set; }
		public string RefDbRepoStagingRestoreFromBackup { get; set; }
		public string SqlServer { get; set; }
		public string WebServer { get; set; }
		public string UpdateServiceDomain { get; set; }
		public string DeliveryServiceDomain { get; set; }
		public string UpdateServiceFilesSharedPath { get; set; }
		public string DeliveryServiceFilesSharedPath { get; set; }
		public string UpdateServiceFilesLocalPath { get; set; }
		public string DeliveryServiceFilesLocalPath { get; set; }
		public string SafeDbName { get; set; }
		public string StagingDbName { get; set; }
		public string DeployUsername { get; set; }
		public string DeployPassword { get; set; }
		public string RefDataRepo { get; set; }

		public TestedShelfDeploymentOptions(TaskInfo taskInfo)
		{
			var comments = taskInfo.TaskComments;

			WebSiteName = Parse(comments, Options.WebSiteName);

			SqlServer =
				Parse(comments, Options.SqlServer) ??
				Defaults.SqlServer;

			RefDbRepoSafeRestoreFromBackup =
				Parse(comments, Options.RefDbRepoSafeRestoreFromBackup) ??
				Defaults.RefDbRepoSafeRestoreFromBackup;

			RefDbRepoStagingRestoreFromBackup =
				Parse(comments, Options.RefDbRepoStagingRestoreFromBackup) ??
				Defaults.RefDbRepoStagingRestoreFromBackup;

			WebServer =
				Parse(comments, Options.WebServer) ??
				Defaults.WebServer;

			UpdateServiceDomain =
				Parse(comments, Options.UpdateServiceDomain) ??
				$"{WebSiteName}.{Defaults.UpdateServiceRootDomain}";

			DeliveryServiceDomain =
				Parse(comments, Options.DeliveryServiceDomain) ??
				$"{WebSiteName}.{Defaults.DeliveryServiceRootDomain}";

			UpdateServiceFilesSharedPath =
				Parse(comments, Options.UpdateServiceFilesSharedPath) ??
				$"\\\\{WebServer}\\{Defaults.WebServerSharedPath}\\{UpdateServiceDomain}";

			DeliveryServiceFilesSharedPath =
				Parse(comments, Options.DeliveryServiceFilesSharedPath) ??
				$"\\\\{WebServer}\\{Defaults.WebServerSharedPath}\\{DeliveryServiceDomain}";

			UpdateServiceFilesLocalPath = Path.Combine(Defaults.WebServerLocalPath, UpdateServiceDomain);
			DeliveryServiceFilesLocalPath = Path.Combine(Defaults.WebServerLocalPath, DeliveryServiceDomain);

			SafeDbName = $"{WebSiteName}RefDbRepoSafe";
			StagingDbName = $"{WebSiteName}RefDbRepoStaging";

			DeployUsername = Parse(comments, Options.DeployUsername) ?? Defaults.DeployUsername;
			DeployPassword = Parse(comments, Options.DeployPassword) ?? Defaults.DeployPassword;
			RefDataRepo = Parse(comments, Options.RefDataRepo) ?? string.Empty;
			if (!string.IsNullOrEmpty(RefDataRepo) && RefDataRepo.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
			{
				RefDataRepo = "Test";
			}
		}

		static string Parse(string text, string key)
		{
			var regex = new Regex(@"^\s*TestRig" + key + ":(?<value>.*)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);
			var match = regex.Match(text);

			return match.Success ? match.Groups["value"].Value.Trim() : null;
		}
	}
}
