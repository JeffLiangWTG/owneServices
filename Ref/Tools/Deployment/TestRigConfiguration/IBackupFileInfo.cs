using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Deployment.TestRigConfiguration
{
	public interface IBackupFileInfo
	{
		IEnumerable<BackupFileInfo> GetBackupFileInfoArray(SqlConnection connection, DatabaseInfo dbInfo);
		string LogicalName { get; }
		string PhysicalName { get; }
		string Type { get; }
	}
}
