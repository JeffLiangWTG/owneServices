using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	internal class DataFixDeleteRefCarrierVesselPivotWhereVesselIsDeletedFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Transaction?.Connection.CreateCommand())
			{
				var query = @"SELECT COUNT(*) FROM RefCarrierVesselPivot where ZZQ_ZZO = '01614E5C-8407-43E3-A66F-D5B5EDF1AF1F'";
				Assert.AreEqual(0, DbHelper.ExecuteScalar(Transaction, query));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixDeleteRefCarrierVesselPivotWhereVesselIsDeleted(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_NKGrouping)
VALUES(newid(), 'ZA', 'Desc', NULL)

INSERT INTO RefVesselZZ(ZZO_PK, ZZO_Code, ZZO_LloydsNumber, ZZO_RadioCallSign, ZZO_RN_NKCountryOfReg, ZZO_VesselType, ZZO_ZZZ_NKDataGrouping)
VALUES('01614E5C-8407-43E3-A66F-D5B5EDF1AF1F', 'CD', '1', '1', 'AU', 'TP', 'ZA')

INSERT INTO RefCarrierCode(ZZ4_PK, ZZ4_Code, ZZ4_Description, ZZ4_IsAir, ZZ4_IsRail, ZZ4_IsRoad, ZZ4_IsSea, ZZ4_ZZZ_NKDataGrouping)
VALUES('D27E478A-EA05-4FA6-9123-E1657A947C6C', 'CD', 'Desc', 1, 0, 0, 0, 'ZA')

INSERT INTO RefCarrierVesselPivot(ZZQ_PK, ZZQ_ZZ4, ZZQ_ZZO)
VALUES(newid(), 'D27E478A-EA05-4FA6-9123-E1657A947C6C', '01614E5C-8407-43E3-A66F-D5B5EDF1AF1F')
";

			DbHelper.ExecuteNonQuery(Transaction, sql);

			var updateRefDbVersionControlSql = @"
UPDATE RefDbVersionControl SET RVC_Deleted = 1 WHERE RVC_ParentPK = '01614E5C-8407-43E3-A66F-D5B5EDF1AF1F'
";
			DbHelper.ExecuteNonQuery(Transaction, updateRefDbVersionControlSql);
		}
	}
}
