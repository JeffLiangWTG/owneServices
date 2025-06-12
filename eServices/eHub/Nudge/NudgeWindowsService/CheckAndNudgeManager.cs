using System;
using System.Data;
using System.Data.SqlClient;
using Common.Logging;

namespace CargoWise.eHub.Nudge
{
	public class CheckAndNudgeManager
	{
		public void CheckAndNudge()
		{
			if (!CacheManager.SystemInfoCache.IsEmpty)
			{
				try
				{
					using (IDbConnection connection = OpenConnection())
					using (IDbTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted))
					using (IDbCommand command = CreateCommand(connection, transaction))
					using (IDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var systemCode = (string)reader["SYSCODE"];
							NudgeRequestManager.RequestNudge(systemCode);
						}
					}
				}
				catch (InvalidOperationException e)
				{
					Logger.Error("DB operation failed", e);
				}
				catch (SqlException e)
				{
					Logger.Error("DB operation failed", e);
				}
			}
			else
			{
				Logger.Error("CheckAndNudge Task cannot proceed because SystemInfoCache is still empty.");
			}
		}

		internal virtual IDbConnection OpenConnection()
		{
			IDbConnection connection = new SqlConnection(NudgeSettings.Instance.CONNECTION_STRING);
			connection.Open();
			return connection;
		}

		IDbCommand CreateCommand(IDbConnection connection, IDbTransaction transaction)
		{
			IDbCommand command = connection.CreateCommand();
			command.Transaction = transaction;
			command.CommandText = queryString;
			command.CommandTimeout = 0;
			return command;
		}

		public static CheckAndNudgeManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CheckAndNudgeManager();
				}
				return instance;
			}
		}

		static readonly ILog Logger = LogManager.GetLogger(typeof(CheckAndNudgeManager));
		static CheckAndNudgeManager instance;
		const string queryString = @"
SELECT LEFT(CC_ID,3) + RIGHT(CC_ID,3) SYSCODE
FROM eHubOutboxMessage 
JOIN eHubClient ON OI_CC_Recipient = CC_PK
WHERE OI_Status IN (0, 1) AND CC_SystemCategory = 'Enterprise'
GROUP BY LEFT(CC_ID,3) + RIGHT(CC_ID,3)
HAVING SUM(CASE WHEN OI_STATUS = 0 THEN 1 ELSE 0 END) > 0
   AND SUM(CASE WHEN OI_STATUS = 1 THEN 1 ELSE 0 END) = 0
";
	}
}
