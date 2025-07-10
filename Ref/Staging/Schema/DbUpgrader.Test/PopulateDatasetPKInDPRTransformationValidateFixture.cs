using System;
using System.Data;
using System.Globalization;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test;

class PopulateDatasetPKInDPRTransformationValidateFixture
{
	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public void ValidateTableColumns()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var connection = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		connection.Open();
		ChechRootTables(connection);
		CheckTablesWithParentFKOrDataSetPK(connection);
		CheckTablesWithMatchValue(connection);
	}

	void ChechRootTables(IDbConnection connection)
	{
		using var cmd = connection.CreateCommand();
		Assert.Multiple(() =>
		{
			foreach (var item in TableCodeAndNameInfo.RootTables)
			{
				cmd.CommandText = $@"select count(*) from INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME='{item.TableName}' and COLUMN_NAME ='{item.TableCode}_PK';";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		});
	}

	void CheckTablesWithParentFKOrDataSetPK(IDbConnection connection)
	{
		using var cmd = connection.CreateCommand();
		Assert.Multiple(() =>
		{
			foreach (var item in TableCodeAndNameInfo.TablesWithParentFKOrDataSetPK)
			{
				var datasetField = $"{item.TableCode}_{(string.IsNullOrEmpty(item.DatasetNameOrParentPK) ? "DataSetPK" : item.DatasetNameOrParentPK)}";

				cmd.CommandText = $@"select count(*) from INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME='{item.TableName}' and COLUMN_NAME ='{datasetField}';";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		});
	}

	void CheckTablesWithMatchValue(IDbConnection connection)
	{
		using var cmd = connection.CreateCommand();
		Assert.Multiple(() =>
		{
			foreach (var item in TableCodeAndNameInfo.TablesWithMatchValue)
			{
				var fields = item.MatchCondition.Replace(" ", "").Split("=");
				Assert.AreEqual(2, fields.Length);

				cmd.CommandText = $@"select count(*) from INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME='{item.TableName}' and COLUMN_NAME ='{fields[0]}';";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);

				cmd.CommandText = $@"select count(*) from INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME='{item.ParentTableName}' and COLUMN_NAME ='{fields[1]}';";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		});
	}
}
