using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Common.Logging;

namespace CargoWise.eHub.Nudge
{
	public class ReloadSystemInfoManager
	{
		public void ReloadSystemInfo()
		{
			do
			{
				try
				{
					DateTime nowUTC = DateTime.UtcNow;
					int count = RetrieveUpdatedSystemInfo(nowUTC);
					lastLoadUTC = nowUTC;
					Logger.InfoFormat("Cache for eHubClientSystem reloaded with {0} new/updated item(s).", count);
				}
				catch (InvalidOperationException e)
				{
					Logger.Error("DB operation failed", e);
					WaitBeforeNextTry();
				}
				catch (SqlException e)
				{
					Logger.Error("DB operation failed", e);
					WaitBeforeNextTry();
				}
			} while (lastLoadUTC == DateTime.MinValue);
		}

		public void WaitBeforeNextTry()
		{
			Thread.Sleep(1000);
		}

		public virtual int RetrieveUpdatedSystemInfo(DateTime nowUTC)
		{
			int count = 0;
			using (IDbConnection connection = OpenConnection())
			using (IDbTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadUncommitted))
			using (IDbCommand command = CreateCommand(connection, transaction, nowUTC))
			using (IDataReader reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					string systemCode = (string)reader["EH_ID"];
					string url = (string)reader["EH_URL"];
					EHubClientSystem eHubClientSystem = CacheManager.SystemInfoCache.GetItemFromCacheOrCreateWhenNotExist(systemCode, delegate { return new EHubClientSystem(systemCode, url); });
					eHubClientSystem.URL = url;
					count++;
				}
			}
			return count;
		}

		internal virtual IDbConnection OpenConnection()
		{
			IDbConnection connection = new SqlConnection(NudgeSettings.Instance.CONNECTION_STRING);
			connection.Open();
			return connection;
		}

		IDbCommand CreateCommand(IDbConnection connection, IDbTransaction transaction, DateTime nowUTC)
		{
			IDbCommand command = connection.CreateCommand();
			command.Transaction = transaction;
			command.CommandText = queryString;
			command.CommandTimeout = 0;
			AddParameter(command, "@fromUTC", lastLoadUTC);
			AddParameter(command, "@toUTC", nowUTC);
			return command;
		}

		void AddParameter(IDbCommand command, string name, DateTime value)
		{
			IDbDataParameter parameter = command.CreateParameter();
			parameter.ParameterName = name;
			parameter.DbType = DbType.DateTime2;
			parameter.Value = value;
			command.Parameters.Add(parameter);
		}

		public static ReloadSystemInfoManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new ReloadSystemInfoManager();
				}
				return instance;
			}
		}

		static ReloadSystemInfoManager instance;
		internal DateTime lastLoadUTC = DateTime.MinValue;
		const string queryString = "SELECT EH_ID, EH_URL FROM eHubClientSystem WHERE EH_LastUpdateUTC > @fromUTC and EH_LastUpdateUTC <= @toUTC";
		static readonly ILog Logger = LogManager.GetLogger(typeof(ReloadSystemInfoManager));
	}
}
