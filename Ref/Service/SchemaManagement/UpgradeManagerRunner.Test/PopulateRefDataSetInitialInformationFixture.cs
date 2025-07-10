using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test;

[TestFixture]
class PopulateRefDataSetInitialInformationFixture
{
	[Test]
	[TransactionedTestCase]
	public void Run()
	{
		var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
		using var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName));
		conn.Open();
		var dbCreator = new DbCreator(conn);
		dbCreator.ExcuteDbScript(dbName, @"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES(newid(), 'AU', 'Australia'), (newid(), 'FR', 'France')

INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES (newid(), 'ANY', 'Any Code Type AU', 0, 0, 'AU')

INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
VALUES('C9924078-3C28-4DE3-BFE2-20AC73042BF3', 'ANY', 'ZZZXXX', 'ANY Code List', 'AU', '1900-01-01 00:00:00', '2079-06-06 00:00:00')

INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_IsReadonly, ZZK_MaxLength, ZZK_ZZZ_NKDataGrouping)
VALUES (newid(), 'FBK', 'French Fallback', 0, 0, 'FR')

INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
VALUES('E9CBB0F3-060A-47C2-8946-EE24D36D8163', 'FBK', 'XXXZZZ', 'TESTING', 'FR', '1900-01-01 00:00:00', '2079-06-06 00:00:00')
");
		using (var cmd = conn.CreateCommand())
		{
			cmd.CommandText = "UPDATE RefDbVersionControl SET RVC_LastUpdatedUTC = SYSUTCDATETIME()";
			cmd.ExecuteNonQuery();
		}
		using (var trans = conn.BeginTransaction())
		{
			var task = new PopulateRefDataSetInitialInformation(0);
			task.Run(trans);
			trans.Commit();
		}
		using (var cmd = conn.CreateCommand())
		{
			cmd.CommandText = @"SELECT COUNT(*) FROM RefDataSetInformation";
			Assert.That(cmd.ExecuteScalar(), Is.GreaterThan(0));
			cmd.CommandText = @"SELECT COUNT(*) FROM RefDataSetInformationDefinition";
			Assert.That(cmd.ExecuteScalar(), Is.GreaterThan(0));
			cmd.CommandText = @"SELECT COUNT(*) FROM RefDbVersionControl where RVC_DataSetId IS NOT NULL";
			Assert.That(cmd.ExecuteScalar(), Is.GreaterThan(0));
			cmd.CommandText = @"SELECT COUNT(*) FROM RefDataSetInformation where RDS_LastUpdatedUTC IS NOT NULL";
			Assert.That(cmd.ExecuteScalar(), Is.GreaterThan(0));

			cmd.CommandText = "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = 'C9924078-3C28-4DE3-BFE2-20AC73042BF3'";
			Assert.AreEqual(10, cmd.ExecuteScalar());
			cmd.CommandText = "SELECT RVC_DataSetId FROM RefDbVersionControl WHERE RVC_ParentPK = 'E9CBB0F3-060A-47C2-8946-EE24D36D8163'";
			Assert.AreEqual(200, cmd.ExecuteScalar());
		}
		using (var trans = conn.BeginTransaction())
		{
			var task = new PopulateRefDataSetInitialInformation(8);
			task.Run(trans);
			trans.Commit();
		}
	}
}
