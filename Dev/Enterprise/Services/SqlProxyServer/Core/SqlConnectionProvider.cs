using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data.Providers.Common;
using CargoWise.Data.SqlProxy.Interface.Models;
using CargoWise.Data.SqlProxyServer.Core.Db;
using Enterprise.DbUpgrader.Resource.Version;
using static System.FormattableString;

namespace CargoWise.Data.SqlProxyServer.Core;

[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "connection over HTTP")]
[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer")]
static class SqlConnectionProvider
{
	public static SqlConnection GetNewOpenConnection(SqlProxyDatabaseDetails connection)
	{
		if (connection == null)
		{
			throw new ArgumentNullException(nameof(connection));
		}

		var sqlConnection = new SqlConnection(GetConnectionString());

		try
		{
			sqlConnection.Open();

			EnsureConnectionIsOpen();
			RunTasksAfterOpenConnection();

			return sqlConnection;
		}
		catch
		{
			sqlConnection.Dispose();
			throw;
		}

		void EnsureConnectionIsOpen()
		{
			if (sqlConnection.State != ConnectionState.Open)
			{
				sqlConnection.Open();
			}

			using var cmd = sqlConnection.CreateCommand();
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = "--Connection.EnsureIsOpen";
			cmd.ExecuteNonQuery();
		}

		string GetConnectionString()
		{
			var encrypt = SqlTlsSetting.ShouldEncryptSqlConnection(connection.ServerName);
			var builder = new SqlConnectionStringBuilder
			{
				PersistSecurityInfo = false,
				ApplicationName = Invariant($"{connection.SuffixedApplicationName}-{ApplicationNameSuffix}"),
				DataSource = connection.ServerName,
				InitialCatalog = connection.Database,
				Encrypt = encrypt,
				TrustServerCertificate = !encrypt,
				UserID = connection.UserName,
				Password = connection.Password,
				ConnectTimeout = connection.ConnectTimeout,
				Pooling = true,
				MinPoolSize = 1,
				MaxPoolSize = 1000,
			};

			if (SqlFailoverSettings.ShouldSpecifyMultiSubnetFailover(connection.ServerName))
			{
				builder.MultiSubnetFailover = true;
			}

			return builder.ToString();
		}

		void RunTasksAfterOpenConnection()
		{
			CheckForUpgrade();
			SuspendAuditTriggers();

			return;

			void CheckForUpgrade()
			{
				if (!ApplicationContext.IsMainDb(connection.ServerName, connection.Database))
				{
					return;
				}

				if (connection.SchemaVersionMajor == null || connection.SchemaVersionMinor == null)
				{
					return;
				}

				var schemaVersions = DbUtils.GetSchemaVersions(sqlConnection);
				var assemblySchemaVersion = new VersionLabel(connection.SchemaVersionMajor ?? 0, connection.SchemaVersionMinor ?? 0);
				var assemblyScriptVersion = new VersionLabel(connection.ScriptVersionMajor ?? 0, connection.ScriptVersionMinor ?? 0);
				var assemblyTransformationVersion = new VersionLabel(connection.TransformationVersionMajor ?? 0, connection.TransformationVersionMinor ?? 0);

				var versionDiff = CompareDbVersions(assemblySchemaVersion, assemblyScriptVersion, assemblyTransformationVersion);

				if (versionDiff != 0)
				{
					throw new DatabaseUpgradedException(versionDiff > 0);
				}

				return;

				int CompareDbVersions(VersionLabel dbSchemaVersion, VersionLabel dbScriptVersion,
					VersionLabel dbTransformationVersion)
				{
					var diff = schemaVersions.SchemaVersion.CompareTo(dbSchemaVersion);

					if (diff != 0)
					{
						return diff;
					}

					diff = schemaVersions.ScriptVersion.CompareTo(dbScriptVersion);

					if (diff != 0)
					{
						return diff;
					}

					diff = schemaVersions.TransformationVersion.CompareTo(dbTransformationVersion);

					return diff != 0 ? diff : 0;
				}
			}

			void SuspendAuditTriggers(string state = "1")
			{
				using var command = sqlConnection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "sys.sp_set_session_context";

				AddParameter("@Key", DbType.String, 128, TriggerKey);
				AddParameter("@Value", DbType.Object, 8016, state ?? (object)DBNull.Value);

				_ = command.ExecuteNonQuery();
				return;

				void AddParameter(string name, DbType type, int size, object value)
				{
					var param = command.CreateParameter();
					param.ParameterName = name;
					param.DbType = type;
					param.Size = size;
					param.Value = value;

					_ = command.Parameters.Add(param);
				}
			}
		}
	}

	const string ApplicationNameSuffix = "SqlProxy";
	const string TriggerKey = "Suspend_System_Audit_Columns_Guard";
}
