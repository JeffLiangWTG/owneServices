using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	internal class DeleteInvalidRefLocoMapAndDropRefUNLOCOUtcOffsetConstraintsFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			var sql = @"SELECT COUNT(1) FROM RefLocoMap LEFT JOIN RefUNLOCO ON RY_RL_NKLocoPort=RL_Code WHERE RL_PK IS NULL;";
			Assert.AreEqual(0, DbHelper.ExecuteScalar(Transaction, sql));
			sql = @"SELECT COUNT(1) FROM RefUNLOCOUtcOffset LEFT JOIN RefUNLOCO ON RLO_RL_NKCode = RL_Code WHERE RL_PK IS NULL";
			Assert.AreEqual(0, DbHelper.ExecuteScalar(Transaction, sql));
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DeleteInvalidRefLocoMapAndRefUNLOCOUtcOffset(0);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RefLocoMap_RefUNLOCO')
ALTER TABLE RefLocoMap DROP FK_RefLocoMap_RefUNLOCO;
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RefUNLOCOUtcOffset_RefUNLOCO')
ALTER TABLE RefUNLOCOUtcOffset DROP FK_RefUNLOCOUtcOffset_RefUNLOCO;

INSERT INTO Refcountry (RN_PK,RN_Code,RN_Desc,RN_CountryDialingCode,RN_RX_NKLocalCurrency,RN_RX_NKAirWaybillCurrency,RN_IsoAlpha3Code,RN_IsoNumericUNM49Code)
VALUES ('43DE5231-FB99-473B-890B-2989740DB446','ZA','South Africa','27','ZAR','ZAR','ZAF','710');
INSERT INTO RefLocoMap (RY_PK, RY_LocalPortCode, RY_RL_NKLocoPort, RY_SystemUsage, RY_RN_NKCountryCode) VALUES
('4268E19E-8FD5-45F1-8F72-266AF91318C5', '1000', 'ABCDE', 'TST', 'ZA');
INSERT INTO RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc)
VALUES ('F84AAAA6-549D-4DB6-A47B-6EC9C9C5C4B4', 'ABCDE', '2022-01-01 22:00:00', '2022-12-31 22:00:00', 120);
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
