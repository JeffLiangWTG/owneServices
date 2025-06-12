using System;
using System.Data.SqlClient;
using Common.Logging;

namespace CargoWise.eServices.Authentication.WindowsService
{
	public interface IDatabaseHelper
	{
		SqlConnection GetDbConnection(string connectionString);
		void TransferData(string ediConnectionString, string authConnectionString);
	}

	public class Databasehelper : IDatabaseHelper
	{
		public virtual SqlConnection GetDbConnection(string connectionString)
		{
			return new SqlConnection(connectionString);
		}

		public virtual void TransferData(string ediConnectionString, string authConnectionString)
		{
			try
			{
				using (var ediConnection = GetDbConnection(ediConnectionString))
				{
					var cm = new SqlCommand(@"SELECT LD_SystemID, LD_Password, LD_ServerCode, LE_EnterpriseCode
FROM ViewLicenceDatabaseSystemId vw
JOIN LicenceDatabase WITH (NOLOCK) ON LicenceDatabase.LD_PK = vw.LD_PK
JOIN LicenceEnterprise WITH (NOLOCK) ON LE_PK = LD_LE
", ediConnection);
					ediConnection.Open();
					var reader = cm.ExecuteReader();
					using (var authConnection = GetDbConnection(authConnectionString))
					{
						authConnection.Open();
						var cm2 = new SqlCommand("SELECT * INTO #TempTable FROM Authentication WHERE 1=2", authConnection);
						cm2.ExecuteNonQuery();
						using (var bc = new SqlBulkCopy(authConnection))
						{
							bc.ColumnMappings.Add("LD_SystemID", "AT_SystemID");
							bc.ColumnMappings.Add("LD_Password", "AT_Password");
							bc.ColumnMappings.Add("LD_ServerCode", "AT_ServerCode");
							bc.ColumnMappings.Add("LE_EnterpriseCode", "AT_EnterpriseCode");
							bc.DestinationTableName = "#TempTable";
							bc.WriteToServer(reader);
						}
						cm2.CommandText = @"MERGE INTO Authentication AS A
USING #TempTable AS T
ON A.AT_SystemID = T.AT_SystemID
WHEN MATCHED
THEN UPDATE SET A.AT_Password = T.AT_Password, A.AT_EnterpriseCode = T.AT_EnterpriseCode, A.AT_ServerCode = T.AT_ServerCode
WHEN NOT MATCHED
THEN INSERT VALUES(T.AT_SystemID, T.AT_Password, T.AT_EnterpriseCode, T.AT_ServerCode, null, null)
WHEN NOT MATCHED BY SOURCE
THEN DELETE;
DROP TABLE #TempTable;";
						cm2.ExecuteNonQuery();
					}
					reader.Close();
				}
			}
			catch (InvalidOperationException ex)
			{
				Logger.Error("DB opration failed", ex);
			}
			catch (SqlException ex)
			{
				Logger.Error("DB opration failed", ex);
			}
		}
		static readonly ILog Logger = LogManager.GetLogger(typeof(Databasehelper));
	}
}
