using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixExchangeRateZWDFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"select count(1)
from RefExchangeRateZZ ex
join RefDbVersionControl vc on vc.RVC_ParentCode = 'ZZN' and vc.RVC_ParentPK = ex.ZZN_PK
WHERE ZZN_RN_NKCountry in ('ZA', 'NA', 'LS', 'SZ') AND ZZN_RX_NKExCurrency = 'ZWL' and vc.RVC_Deleted = 0";
				Assert.AreEqual(4, (int)cmd.ExecuteScalar());
				cmd.CommandText = @"select count(1)
from RefExchangeRateZZ ex
join RefDbVersionControl vc on vc.RVC_ParentCode = 'ZZN' and vc.RVC_ParentPK = ex.ZZN_PK
WHERE ZZN_RN_NKCountry in ('ZA', 'NA', 'LS', 'SZ') AND ZZN_RX_NKExCurrency = 'ZWD' and vc.RVC_Deleted = 0";
				Assert.AreEqual(0, (int)cmd.ExecuteScalar());
				cmd.CommandText = "SELECT ZZN_RX_NKExCurrency FROM RefExchangeRateZZ WHERE ZZN_RN_NKCountry = 'AU'";
				Assert.AreEqual("ZWD", (string)cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixExchangeRateZWD(61);
		}

		protected override void PrepareTestData()
		{
			var sql = @"
INSERT INTO [RefExchangeRateZZ]
([ZZN_PK],[ZZN_ExRateType],[ZZN_StartDate],[ZZN_EndDate]
,[ZZN_Rate],[ZZN_RX_NKExCurrency],[ZZN_RN_NKCountry])
VALUES (newid(),'CUS','2000-01-01','2000-01-02',1,'ZWD','ZA');

INSERT INTO [RefExchangeRateZZ]
([ZZN_PK],[ZZN_ExRateType],[ZZN_StartDate],[ZZN_EndDate]
,[ZZN_Rate],[ZZN_RX_NKExCurrency],[ZZN_RN_NKCountry])
VALUES (newid(),'CUS','2000-01-01','2000-01-02',1,'ZWD','NA');

INSERT INTO [RefExchangeRateZZ]
([ZZN_PK],[ZZN_ExRateType],[ZZN_StartDate],[ZZN_EndDate]
,[ZZN_Rate],[ZZN_RX_NKExCurrency],[ZZN_RN_NKCountry])
VALUES (newid(),'CUS','2000-01-01','2000-01-02',1,'ZWD','SZ');

INSERT INTO [RefExchangeRateZZ]
([ZZN_PK],[ZZN_ExRateType],[ZZN_StartDate],[ZZN_EndDate]
,[ZZN_Rate],[ZZN_RX_NKExCurrency],[ZZN_RN_NKCountry])
VALUES (newid(),'CUS','2000-01-01','2000-01-02',1,'ZWD','LS');

INSERT INTO [RefExchangeRateZZ]
([ZZN_PK],[ZZN_ExRateType],[ZZN_StartDate],[ZZN_EndDate]
,[ZZN_Rate],[ZZN_RX_NKExCurrency],[ZZN_RN_NKCountry])
VALUES (newid(),'CUS','2000-01-01','2000-01-02',1,'ZWD','AU');
";
			DbHelper.ExecuteNonQuery(Transaction, sql);
		}
	}
}
