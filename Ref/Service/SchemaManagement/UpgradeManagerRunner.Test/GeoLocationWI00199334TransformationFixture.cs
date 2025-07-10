using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test;

[TestFixture]
public class GeoLocationWI00199334TransformationFixture
{
	[Test]
	[TransactionedTestCase]
	public void Run()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		var dbCreator = new DbCreator(conn);
		PrepareData(dbCreator, dbName);

		using (var trans = conn.BeginTransaction())
		{
			var task = new GeoLocationWI00199334Transformation(32);
			task.Run(trans);
			trans.Commit();
		}
		using (var cmd = conn.CreateCommand())
		{
			cmd.CommandText = @"Select Count(1) from RefUNLOCO Where RL_Code = 'TEST1' and RL_GeoLocation.Lat = 42.5";
			Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			cmd.CommandText = @"Select Count(1) from RefUNLOCO Where RL_Code = 'TEST1' and RL_GeoLocation.Long = 1.51667";
			Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			cmd.CommandText = @"Select Count(1) from RefUNLOCO Where RL_Code = 'TEST2' and RL_GeoLocation.Lat = -42.5";
			Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			cmd.CommandText = @"Select Count(1) from RefUNLOCO Where RL_Code = 'TEST2' and RL_GeoLocation.Long = -1.51667";
			Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			cmd.CommandText = @"Select Count(1) from RefUNLOCO Where RL_Code = 'TEST3' and RL_GeoLocation.Lat = 42.5";
			Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			cmd.CommandText = @"Select Count(1) from RefUNLOCO Where RL_Code = 'TEST3' and RL_GeoLocation.Long = -1.51667";
			Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			cmd.CommandText = @"Select Count(1) from RefUNLOCO Where RL_Code = 'TEST4' and RL_GeoLocation.Lat = -42.5";
			Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			cmd.CommandText = @"Select Count(1) from RefUNLOCO Where RL_Code = 'TEST4' and RL_GeoLocation.Long = 1.51667";
			Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
		}
	}

	void PrepareData(DbCreator dbCreator, string dbName)
	{
		dbCreator.ExcuteDbScript(dbName, @"INSERT INTO [dbo].[RefUNLOCO] ([RL_PK],[RL_Code],[RL_IsActive],[RL_PortName],[RL_NameWithDiacriticals],[RL_IATA],[RL_CoOrdinates],[RL_RN_NKCountryCode],[RL_IATARegionCode])
VALUES (newid(),'TEST1',1,'Test Port','Test','TST','4230N 00131E','AU','AUS');");
		dbCreator.ExcuteDbScript(dbName, @"INSERT INTO [dbo].[RefUNLOCO] ([RL_PK],[RL_Code],[RL_IsActive],[RL_PortName],[RL_NameWithDiacriticals],[RL_IATA],[RL_CoOrdinates],[RL_RN_NKCountryCode],[RL_IATARegionCode])
VALUES (newid(),'TEST2',1,'Test Port','Test','TST','4230S 00131W','AU','AUS');");
		dbCreator.ExcuteDbScript(dbName, @"INSERT INTO [dbo].[RefUNLOCO] ([RL_PK],[RL_Code],[RL_IsActive],[RL_PortName],[RL_NameWithDiacriticals],[RL_IATA],[RL_CoOrdinates],[RL_RN_NKCountryCode],[RL_IATARegionCode])
VALUES (newid(),'TEST3',1,'Test Port','Test','TST','4230N 00131W','AU','AUS');");
		dbCreator.ExcuteDbScript(dbName, @"INSERT INTO [dbo].[RefUNLOCO] ([RL_PK],[RL_Code],[RL_IsActive],[RL_PortName],[RL_NameWithDiacriticals],[RL_IATA],[RL_CoOrdinates],[RL_RN_NKCountryCode],[RL_IATARegionCode])
VALUES (newid(),'TEST4',1,'Test Port','Test','TST','4230S 00131E','AU','AUS');");
	}
}
