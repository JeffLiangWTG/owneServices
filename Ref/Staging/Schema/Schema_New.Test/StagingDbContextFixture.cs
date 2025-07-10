using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.Schema_New.Test;

[TestFixture]
[Property("DAT:CapabilityRequirements", "SQL2019+")]
[TransactionedTestCase]
class StagingDbContextFixture
{
	[Test]
	public void ClassNamesAreSameAsTableNames()
	{
		var tableAndViewNameList = GetTableAndViewNames();
		var classNameList = GetClassNamesFromAssembly();
		foreach (var tableName in tableAndViewNameList)
		{
			Assert.True(classNameList.Contains(tableName), $"Schema_New should contain {tableName} class");
		}
	}

	List<string> GetTableAndViewNames()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoStaging);
		var connString = TestConnectionString.GetAdmin(dbName);
		var tableList = new List<string>();
		using var conn = new SqlConnection(connString);
		conn.Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = "select * from sys.objects where type in ('U','V') and name not like '%History' order by name";
		using var reader = cmd.ExecuteReader();
		while (reader.Read())
		{
			var tableName = reader.GetString(0);
			tableList.Add(tableName);
		}

		return tableList;
	}

	List<string> GetClassNamesFromAssembly()
	{
		var classNames = new List<string>();
		var assembly = typeof(RefAccTaxRate).Assembly;
		var types = assembly.GetTypes().Where(x => x.IsClass && x.FullName.StartsWith("CargoWise.RefDbRepo.Staging.Schema_New", StringComparison.OrdinalIgnoreCase));
		classNames.AddRange(types.Select(x => x.Name));
		classNames.Sort();
		return classNames;
	}
}
