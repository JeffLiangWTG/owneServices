using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public enum VersionType
	{
		DbSchemaVersion,
		DataTransformationVersion
	}

	public class SchemaVersionManager : ISchemaVersionManager
	{
		public SchemaVersionManager(IDbConnection connection, VersionType versionType, string dbName)
		{
			Argument.Argument.NotNull(connection, nameof(connection));
			Argument.Argument.NotNullOrEmpty(dbName, nameof(dbName));

			this.connection = connection;
			this.dbName = dbName;
			this.versionType = versionType;
		}

		[SuppressMessage("Microsoft.Security", "CA2100")]
		public int GetVersion(IDbTransaction transaction)
		{
			connection.ChangeDatabaseIfNeeded(dbName);
			using (var cmd = connection.CreateCommand())
			{
				try
				{
					cmd.CommandText = $@"
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'SystemData')
BEGIN
	SELECT SD_Value FROM SystemData WHERE SD_Name = '{versionType}'
END
ELSE
BEGIN
	SELECT 0
END";
					cmd.Transaction = transaction;
					var verObj = cmd.ExecuteScalar();
					return verObj == null || verObj == DBNull.Value ? 0 : int.Parse(verObj.ToString(), CultureInfo.InvariantCulture);
				}
				catch (SqlException ex) when (ex.Message.StartsWith("Invalid column name", StringComparison.InvariantCulture))
				{
					RenameSystemDataColumns(transaction);
					return GetVersion(transaction);
				}
			}
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public void UpdateVersion(int version, IDbTransaction transaction)
		{
			connection.ChangeDatabaseIfNeeded(dbName);
			using (var cmd = connection.CreateCommand())
			{
				Console.WriteLine($"Updating {versionType} on SystemData to {version}");
				cmd.CommandText = $@"
IF EXISTS (SELECT * FROM SystemData WHERE SD_Name = '{versionType}')
BEGIN
	UPDATE SystemData SET SD_Value = '{version}' WHERE SD_Name = '{versionType}'
END
ELSE
BEGIN
	INSERT SystemData (SD_Name, SD_Value)
	VALUES ('{versionType}', '{version}')
END";
				cmd.Transaction = transaction;
				cmd.ExecuteNonQuery();
			}
		}

		public void RenameSystemDataColumns(IDbTransaction transaction)
		{
			if (transaction == null)
			{
				using (transaction = connection.BeginTransaction())
				{
					new RenameSystemDataColumns(0).Run(transaction);
					transaction.Commit();
				}
			}
			else
			{
				new RenameSystemDataColumns(0).Run(transaction);
			}
		}

		readonly IDbConnection connection;
		readonly string dbName;
		readonly VersionType versionType;
	}
}
