using System.Collections.Generic;
using System.Data;
using System.IO;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Deployment.TestRigConfiguration
{
	public class BackupFileInfo : IBackupFileInfo
	{
		public const string DataFileType = "D";
		public const string LogFileType = "L";

		public string LogicalName { get; set; }

		public string PhysicalName { get; set; }

		public string Type { get; set; }

		public IEnumerable<BackupFileInfo> GetBackupFileInfoArray(SqlConnection connection, DatabaseInfo dbInfo)
		{
			var logFilePath = GetPhysicalPath(connection, dbInfo, LogFileType);
			var dataFilePath = GetPhysicalPath(connection, dbInfo, DataFileType);

			var logFileCount = 0;
			var dataFileCount = 0;

			var sqlText = "RESTORE FILELISTONLY FROM DISK = @fileName";
			using (var cmd = new SqlCommand())
			{
				cmd.Connection = connection;
				cmd.Parameters.Add("@fileName", SqlDbType.NChar).Value = dbInfo.BackupFullFileName;
				cmd.CommandText = sqlText;
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var bkpFileInfo = new BackupFileInfo();
						if (reader["LogicalName"] != null)
						{
							bkpFileInfo.LogicalName = reader["LogicalName"].ToString().Trim();
						}
						if (reader["Type"] != null)
						{
							bkpFileInfo.Type = reader["Type"].ToString().Trim();
						}

						switch (bkpFileInfo.Type)
						{
							case LogFileType:
								string fileNum = logFileCount > 0 ? $"{logFileCount}" : string.Empty;
								bkpFileInfo.PhysicalName = Path.Combine(logFilePath, $"{dbInfo.DatabaseName}_Log{fileNum}.ldf");
								logFileCount++;
								break;

							case DataFileType:
								fileNum = dataFileCount > 0 ? $"{dataFileCount}" : string.Empty;
								bkpFileInfo.PhysicalName = Path.Combine(dataFilePath, $"{dbInfo.DatabaseName}_Data{fileNum}.mdf");
								dataFileCount++;
								break;
						}

						yield return bkpFileInfo;
					}
				}
			}
		}

		static string GetPhysicalPath(SqlConnection connection, DatabaseInfo info, string fileType)
		{
			return Path.GetDirectoryName(GetPhysicalNameFromDatabase(connection, info.DatabaseName, fileType));
		}

		static string GetPhysicalNameFromDatabase(SqlConnection connection, string databaseName, string fileType)
		{
			var result = string.Empty;
			var sqlText = @"
					SELECT TOP 1 physical_name 
					FROM sys.master_files
					WHERE database_id = db_id(@databaseName)
					AND type = @fileType
					ORDER BY file_id";

			using (var cmd = new SqlCommand())
			{
				cmd.Connection = connection;
				cmd.Parameters.Add("@databaseName", SqlDbType.NChar).Value = databaseName;
				cmd.Parameters.Add("@fileType", SqlDbType.Bit).Value = fileType == DataFileType ? 0 : 1;
				cmd.CommandText = sqlText;
				var execute = cmd.ExecuteScalar();

				if (execute != null)
				{
					result = (string)execute;
				}
			}

			return result;
		}
	}
}
