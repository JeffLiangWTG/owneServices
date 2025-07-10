using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class FixTransformDataSelectorFormulaTaskFixture
	{
		[Test]
		[TransactionedTestCase]
		public void Run()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.None);
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(dbName)))
			{
				conn.Open();
				var dbCreator = new DbCreator(conn);
				dbCreator.ExcuteDbScript(dbName, @"
Create table RefCusRateCode(
ZY1_PK uniqueidentifier,
ZY1_RateCode varchar(5),
ZY1_ZZR_RateType uniqueidentifier,
ZY1_Description varchar(500)
);

Create table RefCusTariff(
ZZ1_PK uniqueidentifier,
ZZ1_ZZI_TariffType uniqueidentifier,
ZZ1_ZZZ_NKDataGrouping varchar(3)
);

Create Table RefCusTariffType
(
ZZI_PK uniqueidentifier,
ZZI_TariffType varchar(5),
ZZI_ZZR_RateType uniqueidentifier,
ZZI_Description varchar(50),
ZZI_ZZZ_NKDataGrouping varchar(3)
);

Create Table RefCusRate
(
ZZ2_PK uniqueidentifier,
ZZ2_ZZS_Preference uniqueidentifier,
ZZ2_ZZ1_Tariff uniqueidentifier,
ZZ2_SelectorFormula varchar(500),
ZZ2_StartDate smalldatetime,
ZZ2_EndDate smalldatetime,
ZZ2_DataSetPK uniqueidentifier,
ZZ2_DataSetCode varchar(3),
ZZ2_ZY1_RateCode uniqueidentifier,
ZZ2_ZZZ_NKDataGrouping varchar(3)
);

Create Table RefCusTradeGroup
(
ZZA_PK uniqueidentifier,
ZZA_TradeGroup varchar(35),
ZZA_ZZZ_NKDataGrouping varchar(3)
);

Create Table RefCusApplicability
(
ZZT_PK uniqueidentifier,
ZZT_ZZ2_Rate uniqueidentifier,
ZZT_StartDate smalldatetime,
ZZT_EndDate smalldatetime,
ZZT_ZZA_TradeGroup uniqueidentifier,
ZZT_AdditionalCode varchar(15),
ZZT_OrderNumber varchar(15),
ZZT_DataSetPK uniqueidentifier,
ZZT_DataSetCode varchar(3)
);
");
				var tariff = Guid.NewGuid();
				var tarifftype = Guid.NewGuid();
				var rateMercosur = Guid.NewGuid();
				var tradegroupMercosur = Guid.NewGuid();
				var tradegroupMontenegro = Guid.NewGuid();
				var applicabilityMercosur = Guid.NewGuid();
				var applicabilityMontenegro = Guid.NewGuid();

				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping) VALUES ('{tariff}', '{tarifftype}','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping) VALUES ('{tarifftype}', '1P1', '1P1','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping) 
VALUES ('{rateMercosur}',null,'{tariff}','pp=''MERCOSUR''','1900-01-01','2079-06-06','{tariff}','ZZ1','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping) 
VALUES ('{tradegroupMercosur}','MERCOSUR','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping) 
VALUES ('{tradegroupMontenegro}','ME','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES ('{applicabilityMercosur}','{rateMercosur}','1900-01-01','2079-06-06','{tradegroupMercosur}','','');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_ZZA_TradeGroup,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES ('{applicabilityMontenegro}','{rateMercosur}','1900-01-01','2079-06-06','{tradegroupMontenegro}','','');
");

				using (var trans = conn.BeginTransaction())
				{
					var task = new FixTransformDataSelectorFormulaTask(25);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZ2_Rate = '{rateMercosur}' AND ZZT_ZZA_TradeGroup = '{tradegroupMercosur}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZ2_Rate = '{rateMercosur}' AND ZZT_ZZA_TradeGroup = '{tradegroupMontenegro}'";
					Assert.AreEqual(0, cmd.ExecuteScalar());
				}
			}
		}
	}
}
