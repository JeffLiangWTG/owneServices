using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.Data.SqlClient;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test;

[TestFixture]
[TransactionedTestCase]
class SafeDbContextFixture
{
	string DbName { get; } = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
	[Test]
	public void Insert_CommonData()
	{
		var languageType = new RefLanguageType
		{
			ZX6_PK = Guid.NewGuid(),
			ZX6_Language = "ENG",
			ZX6_Description = "English"
		};

		using (var context1 = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
		{
			var timeStamp1 = DateTime.Now;
			context1.RefLanguageTypes.Add(languageType);
			context1.SaveChanges();
		}

		using (var context2 = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
		{
			Assert.AreEqual(1, context2.RefLanguageTypes.Count());
			var languageTypeFromDb = context2.RefLanguageTypes.FirstOrDefault();
			Assert.AreEqual(languageType.ZX6_PK, languageTypeFromDb.ZX6_PK);
			Assert.AreEqual(languageType.ZX6_Language, languageTypeFromDb.ZX6_Language);
			Assert.AreEqual(languageType.ZX6_Description, languageTypeFromDb.ZX6_Description);
		}
	}

	[Test]
	public void Insert_GeometryData()
	{
		var portPolygon = new RefPortPolygon
		{
			RPP_PK = Guid.NewGuid(),
			RPP_PortId = 1,
			RPP_SerializedPolygon = new Point(-122.333056, 47.609722) { SRID = 4326 }
		};

		using (var context1 = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
		{
			var timeStamp1 = DateTime.Now;
			context1.RefPortPolygons.Add(portPolygon);
			context1.SaveChanges();
		}

		using (var context2 = new SafeDbContext(TestConnectionString.GetAdmin(DbName)))
		{
			Assert.AreEqual(1, context2.RefPortPolygons.Count());
			var portPolygonFromDb = context2.RefPortPolygons.FirstOrDefault();
			Assert.AreEqual(portPolygon.RPP_PK, portPolygonFromDb.RPP_PK);
			Assert.AreEqual(portPolygon.RPP_PortId, portPolygonFromDb.RPP_PortId);
			Assert.AreEqual(portPolygon.RPP_SerializedPolygon, portPolygonFromDb.RPP_SerializedPolygon);
		}
	}

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
		var tableList = new List<string>();
		var connString = TestConnectionString.GetAdmin(DbName);
		using var conn = new SqlConnection(connString);
		conn.Open();
		using var cmd = conn.CreateCommand();
		cmd.CommandText = "select * from sys.objects where type in ('U','V') and name not like '%History' and name not like '%Test' order by name";
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
		var types = assembly.GetTypes().Where(x => x.IsClass && x.FullName.StartsWith("CargoWise.RefDbRepo.Service.Schema_0_9_New", StringComparison.OrdinalIgnoreCase));
		classNames.AddRange(types.Select(x => x.Name));
		classNames.Sort();
		return classNames;
	}
}
