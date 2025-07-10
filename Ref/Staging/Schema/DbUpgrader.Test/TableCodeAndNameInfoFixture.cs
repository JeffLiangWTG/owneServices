using System.Collections.Generic;
using System.Data;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test;

[TestFixture]
public class TableCodeAndNameInfoFixture
{
	[Test]
	[TransactionedTestCase]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	public void TestTableCodeAndNameMappingCorrect()
	{
		var expectedDictionary = new Dictionary<string, HashSet<string>>();
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		try
		{
			connection = new SqlConnection(TestConnectionString.GetAdmin(dbName));
			connection.Open();
			transaction = connection.BeginTransaction();
			using (var cmd = connection.CreateCommand())
			{
				cmd.Transaction = transaction;
				cmd.CommandText = @"
SELECT DISTINCT SUBSTRING(COLUMN_NAME, 0, CHARINDEX('_', COLUMN_NAME)) AS 'TablePrefix', TABLE_NAME AS 'TableName'
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME NOT LIKE '%History'
ORDER BY TableName;";
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var key = reader.GetString(0);
						var value = reader.GetString(1);
						if (expectedDictionary.ContainsKey(key))
						{
							expectedDictionary[key].Add(value);
						}
						else
						{
							expectedDictionary.Add(key, new HashSet<string> { value });
						}
					}
					reader.Close();
				}
			}
			foreach (var (tableCode, tableName) in TableCodeAndNameInfo.CodeToNameDictionary())
			{
				Assert.True(expectedDictionary[tableCode].Contains(tableName));
			}
		}

		finally
		{
			try
			{
				if (transaction != null)
				{
					transaction.Rollback();
					transaction.Dispose();
				}
			}
			finally
			{
				connection?.Dispose();
			}
		}
	}

	IDbConnection connection;
	IDbTransaction transaction;
}
