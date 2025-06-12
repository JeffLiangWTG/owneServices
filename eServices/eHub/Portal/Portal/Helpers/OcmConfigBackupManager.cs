using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Common.Logging;

namespace CargoWise.eHub.Portal.Helpers
{
	public interface IOcmConfigBackupManager
	{
		void Backup(HttpServerUtilityBase utility, string typeId, byte[] rawContent);
	}

	public class OcmConfigBackupManager : IOcmConfigBackupManager
	{
		internal const string TimestampFormat = "yyyyMMddHHmmssfff";
		internal const string BackupFolderKey = "OCMBackupFolder";
		internal const string BackupFolderDefault = "OCMRoutingRuleBackups";
		internal ILog Logger = LogManager.GetLogger(typeof(OcmConfigBackupManager));

		public void Backup(HttpServerUtilityBase utility, string typeId, byte[] rawContent)
		{
			#region parameters validation

			if (rawContent == null || rawContent.Length == 0)
			{
				Logger.ErrorFormat("Empty content for {0}", typeId);
				return;
			}

			if (string.IsNullOrWhiteSpace(typeId))
			{
				Logger.ErrorFormat("typeId is null");
				return;
			}

			#endregion

			try
			{
				var backupFolder = utility.MapPath(GetConfigurationOrDefaultPath());
				var subFolder = Path.Combine(backupFolder, typeId);
				var backupFullPath = Path.Combine(subFolder, $"{DateTime.UtcNow.ToString(TimestampFormat)}_{typeId}.csv");
				var content = Encoding.Default.GetString(rawContent);

				WriteAllText(backupFullPath, content);

				DeleteExpiredBackups(subFolder);
			}
			catch (Exception ex)
			{
				Logger.Error($"Failed to save OCM routing file for {typeId}", ex);
			}
		}

		private string GetConfigurationOrDefaultPath()
		{
			var configPath = ConfigurationManager.AppSettings[BackupFolderKey];
			if (string.IsNullOrWhiteSpace(configPath))
			{
				configPath = BackupFolderDefault;
			}

			configPath = configPath.Trim();
			if (!configPath.StartsWith("~"))
			{
				configPath = Path.Combine("~", configPath);
			}

			return configPath;
		}

		private void DeleteExpiredBackups(string subFolder)
		{
			var filesToDelete = GetFiles(subFolder)
				.Where(file => GetFileInfoCreationTimeUtc(file) < DateTime.UtcNow.AddMonths(-1))
				.ToList();

			foreach (var file in filesToDelete)
			{
				try
				{
					Delete(file);
				}
				catch (Exception ex)
				{
					Logger.ErrorFormat("Failed to delete file {0}", file, ex);
				}
			}
		}

		#region Virtual method wrapper for static methods

		internal virtual string[] GetFiles(string ocmType)
		{
			return Directory.GetFiles(ocmType);
		}

		internal virtual DateTime GetFileInfoCreationTimeUtc(string fileName)
		{
			return new FileInfo(fileName).CreationTimeUtc;
		}

		internal virtual void Delete(string path)
		{
			File.Delete(path);
		}

		internal virtual void WriteAllText(string path, string content)
		{
			var file = new FileInfo(path);
			file.Directory?.Create();
			File.WriteAllText(file.FullName, content);
		}

		#endregion
	}
}
