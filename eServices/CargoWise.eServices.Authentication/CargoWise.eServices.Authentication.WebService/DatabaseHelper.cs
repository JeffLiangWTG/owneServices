using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Common.Logging;

namespace CargoWise.eServices.Authentication.WebService
{
	public class DatabaseHelper : IDatabaseHelper
	{
		static string connectionString;

		public DateTime? GetSystemLastEditUTC(string systemID)
		{
			DateTime? result = null;

			try
			{
				result = ReadSystemLastEditUTC(systemID);
			}
			catch (InvalidOperationException ex)
			{
				Logger.Error("DB opration failed", ex);
			}
			catch (SqlException ex)
			{
				Logger.Error("DB operation failed", ex);
			}

			return result;
		}

		public DateTime? ReadSystemLastEditUTC(string systemID)
		{
			using (var connection = DbConnection)
			{
				Logger.Debug($"{systemID}: GetSystemLastEditUTC: DB Connection Start");
				connection.Open();

				using (var command = GetCommand("SELECT AT_SystemLastModifiedTimeUTC FROM Authentication WHERE AT_SystemID = @SystemID", connection))
				{
					command.Parameters.Add(new SqlParameter("@SystemID", SqlDbType.VarChar, 5) { Value = systemID });
					Logger.Debug($"{systemID}: GetSystemLastEditUTC: DB Execution Start");
					var result = (DateTime?)command.ExecuteScalar();
					Logger.Debug($"{systemID}: GetSystemLastEditUTC: DB Execution End");
					return result;
				}
			}
		}

		public bool CheckSystemIDExistence(string systemID)
		{
			return GetSystemLastEditUTC(systemID) != null;
		}

		public bool CheckCodeExistence(string enterpriseCode, string serverCode)
		{
			var result = false;
			try
			{
				using (var connection = DbConnection)
				{
					Logger.Debug($"{enterpriseCode}-{serverCode}: CheckCodeExistence: DB Connection Start");
					connection.Open();
					using (var command = GetCommand(
						"SELECT COUNT(*) FROM Authentication WHERE AT_EnterpriseCode = @EnterpriseCode AND AT_ServerCode = @ServerCode", connection))
					{
						command.Parameters.Add(new SqlParameter("@EnterpriseCode", SqlDbType.VarChar, 3) { Value = enterpriseCode });
						command.Parameters.Add(new SqlParameter("@ServerCode", SqlDbType.VarChar, 3) { Value = serverCode });
						Logger.Debug($"{enterpriseCode}-{serverCode}: CheckCodeExistence: DB Execution Start");
						if ((int)command.ExecuteScalar() > 0)
						{
							result = true;
						}
						Logger.Debug($"{enterpriseCode}-{serverCode}: CheckCodeExistence: DB Execution End");
					}
					if (!result)
					{
						Logger.InfoFormat("Unable to find/validate system with EnterpriseCode: {0} and ServerCode: {1}", enterpriseCode, serverCode);
					}
				}
			}
			catch (InvalidOperationException ex)
			{
				Logger.Error("DB opration failed", ex);
			}
			catch (SqlException ex)
			{
				Logger.Error("DB operation failed", ex);
			}
			return result;
		}

		public bool ValidateSystemIDAndPassword(string systemID, string password)
		{
			var result = false;
			try
			{
				using (var connection = DbConnection)
				{
					Logger.Debug($"{systemID}: ValidateSystemIDAndPassword: DB Connection Start");
					connection.Open();
					using (var command = GetCommand(ValidateIDAndPasswordQuery, connection))
					{
						command.Parameters.Add(new SqlParameter("@SystemID", SqlDbType.VarChar, 5) { Value = systemID });
						command.Parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 200) { Value = password });
						Logger.Debug($"{systemID}: ValidateSystemIDAndPassword: DB Execution Start");
						if ((int)command.ExecuteScalar() > 0)
						{
							result = true;
						}
						Logger.Debug($"{systemID}: ValidateSystemIDAndPassword: DB Execution End");
					}
					if (!result)
					{
						Logger.InfoFormat("Unable to find/validate system with SystemID: {0}", systemID);
					}
				}
			}
			catch (InvalidOperationException ex)
			{
				Logger.Error("DB opration failed", ex);
			}
			catch (SqlException ex)
			{
				Logger.Error("DB operation failed", ex);
			}
			return result;
		}


		public bool ValidateCodeAndPassword(string enterpriseCode, string serverCode, string password)
		{
			var result = false;
			try
			{
				using (var connection = DbConnection)
				{
					Logger.Debug($"{enterpriseCode}-{serverCode}: ValidateCodeAndPassword: DB Connection Start");
					connection.Open();
					using (var command = GetCommand(ValidateCodeAndPasswordQuery, connection))
					{
						command.Parameters.Add(new SqlParameter("@EnterpriseCode", SqlDbType.VarChar, 3) { Value = enterpriseCode });
						command.Parameters.Add(new SqlParameter("@ServerCode", SqlDbType.VarChar, 3) { Value = serverCode });
						command.Parameters.Add(new SqlParameter("@Password", SqlDbType.VarChar, 200) { Value = password });
						Logger.Debug($"{enterpriseCode}-{serverCode}: ValidateCodeAndPassword: DB Execution Start");
						if ((int)command.ExecuteScalar() > 0)
						{
							result = true;
						}
						Logger.Debug($"{enterpriseCode}-{serverCode}: ValidateCodeAndPassword: DB Execution End");
					}
					if (!result)
					{
						Logger.InfoFormat("Unable to find/validate system with EnterpriseCode: {0} and ServerCode: {1}", enterpriseCode, serverCode);
					}
				}
			}
			catch (InvalidOperationException ex)
			{
				Logger.Error("DB opration failed", ex);
			}
			catch (SqlException ex)
			{
				Logger.Error("DB operation failed", ex);
			}
			return result;
		}

		public virtual IDbCommand GetCommand(string command, IDbConnection connection)
		{
            if (!(connection is SqlConnection sqlConnection))
			{
				throw new ArgumentException($"Expecting {nameof(connection)} type: {nameof(SqlConnection)}");
			}

			return new SqlCommand(command, sqlConnection);
		}

		public virtual IDbConnection DbConnection
		{
			get
			{
				if (string.IsNullOrEmpty(connectionString))
				{
					connectionString = ConfigurationManager.ConnectionStrings["Authentication"].ConnectionString;
				}
				return new SqlConnection(connectionString);
			}
		}

		public virtual bool IsDatabaseAlive()
		{
			var result = false;
			try
			{
				using (var connection = DbConnection)
				{
					connection.Open();
					result = true;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("DB DbConnection open failed", ex);
			}
			return result;
		}

		static readonly ILog Logger = LogManager.GetLogger(typeof(DatabaseHelper));

		const string ValidateIDAndPasswordQuery = @"
SELECT COUNT(*)
FROM Authentication
WHERE AT_SystemID = @SystemID
AND (SELECT REPLACE(AT_Password, '-','')) = CONVERT(VARCHAR(128), HASHBYTES('SHA2_512', CONVERT(VARCHAR(128), [dbo].[Base27Decode](@SystemID)) + @Password), 2)";

		const string ValidateCodeAndPasswordQuery = @"
SELECT COUNT(*)
FROM Authentication
WHERE AT_EnterpriseCode = @EnterpriseCode
AND AT_ServerCode = @ServerCode
AND (SELECT REPLACE(AT_Password, '-','')) = CONVERT(VARCHAR(128), HASHBYTES('SHA2_512', CONVERT(VARCHAR(128), [dbo].[Base27Decode](AT_SystemID)) + @Password), 2)";
	}
}