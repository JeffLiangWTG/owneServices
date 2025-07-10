using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CargoWise.RefDbRepo.Deployment.TestRigConfiguration
{
	public static class RestoreManager
	{
		public static string GetRestoreScript(IEnumerable<IBackupFileInfo> backupFileInfos, DatabaseInfo dbInfo)
		{
			var scriptBuilder = new StringBuilder($@"RESTORE DATABASE {dbInfo.DatabaseName} FROM DISK = N'{dbInfo.BackupFullFileName}'
					WITH  FILE = 1,");

			foreach(var backupFileInfo in backupFileInfos)
			{
				scriptBuilder.Append(CultureInfo.InvariantCulture, $@"MOVE N'{backupFileInfo.LogicalName}' TO N'{backupFileInfo.PhysicalName}',");
			}
			scriptBuilder.Append($@"NOUNLOAD, REPLACE,  STATS = 5");
			return scriptBuilder.ToString();
		}
	}
}
