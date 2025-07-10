using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class USCustomsExportCodeCorrectionTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				var sql = @"SELECT COUNT(*) FROM RefCusCodeList WHERE ZZD_StartDate = '1900-01-01' AND ZZD_Code = '{0}'";
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql, "4701");
				Assert.AreEqual(1, cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql, "9999");
				Assert.AreEqual(1, cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql, "3581");
				Assert.AreEqual(0, cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql, "4110");
				Assert.AreEqual(0, cmd.ExecuteScalar());

				sql = @"SELECT COUNT(*) FROM RefCusCodeList 
JOIN RefDbVersionControl ON RVC_ParentPK = ZZD_PK
WHERE ZZD_Code = '{0}' AND RVC_Deleted = 1";
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql, "4701");
				Assert.AreEqual(1, cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql, "9999");
				Assert.AreEqual(1, cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql, "3581");
				Assert.AreEqual(0, cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql, "4110");
				Assert.AreEqual(0, cmd.ExecuteScalar());

				sql = @"SELECT COUNT(*) FROM RefCusCodeList
JOIN RefCusCodeOrAttributeTransportMode ON ZZU_ZZD_CodeList = ZZD_PK
WHERE ZZD_Code = '{0}'";
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql, "4701");
				Assert.AreEqual(1, cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql, "9999");
				Assert.AreEqual(1, cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql, "3581");
				Assert.AreEqual(1, cmd.ExecuteScalar());
				cmd.CommandText = string.Format(CultureInfo.InvariantCulture, sql, "4110");
				Assert.AreEqual(1, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new USCustomsExportCodeCorrectionTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = "DROP INDEX IF EXISTS IX_RefCusCodeList_ZZD_ZZZ_NKDataGrouping_ZZD_ZZN_NKCodeType_ZZD_Code ON RefCusCodeList";
				cmd.ExecuteNonQuery();
			}
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description)
VALUES (newid(), 'US', 'X'),
	(newid(), 'ZZ', 'X')

INSERT INTO RefCusCodeType (ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_ZZZ_NKDataGrouping)
VALUES (newid(), 'CUSOF', 'XX', 'US'),
	(newid(), 'CUSOF', 'XX', 'ZZ'),
	(newid(), 'CUSO1', 'XX', 'US')

INSERT INTO RefCusCodeList (ZZD_PK, ZZD_ZZK_NKCodeType, ZZD_Code, ZZD_Description, ZZD_StartDate, ZZD_EndDate, ZZD_ZZZ_NKDataGrouping)
VALUES 
	('3EE3B8C0-2F82-450A-AC0D-DA727B3F3C05', 'CUSOF', '4701', 'JOHN F KENNEDY AIRPORT, N', '2018-01-19', '2079-06-06 23:59:00', 'US'),
	('72BA9BF6-BD6D-49E0-AB1F-33D7159076BE', 'CUSOF', '9999', 'GARY, I', '2018-01-19', '2079-06-06 23:59:00', 'US'),
	('DA210AC5-D442-4685-9BBE-BBEC51984D71', 'CUSOF', '3581', 'USER FEE AIRPORT, M', '2018-01-19', '2079-06-06 23:59:00', 'ZZ'),
	('0A0D2B9C-97CA-423E-9F4E-027B46CA660D', 'CUSO1', '4110', 'INDIANAPOLIS, I', '2018-01-19', '2079-06-06 23:59:00', 'US')

INSERT INTO RefCusCodeOrAttributeTransportMode (ZZU_PK, ZZU_TransportMode, ZZU_ZZD_CodeList)
VALUES 
	(newid(), 'SEA', '3EE3B8C0-2F82-450A-AC0D-DA727B3F3C05'),
	(newid(), 'RAI', '3EE3B8C0-2F82-450A-AC0D-DA727B3F3C05'),
	(newid(), 'AIR', '72BA9BF6-BD6D-49E0-AB1F-33D7159076BE'),
	(newid(), 'RAI', 'DA210AC5-D442-4685-9BBE-BBEC51984D71'),
	(newid(), 'RAI', '0A0D2B9C-97CA-423E-9F4E-027B46CA660D')
";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
