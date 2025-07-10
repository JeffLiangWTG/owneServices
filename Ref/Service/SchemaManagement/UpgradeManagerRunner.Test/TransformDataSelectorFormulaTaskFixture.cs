using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	[TestFixture]
	class TransformDataSelectorFormulaTaskFixture
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

Create table RefCusExcludedTradeGroup(
ZZC_PK uniqueidentifier,
ZZC_ZZT_Applicability uniqueidentifier,
ZZC_ZZA_TradeGroup uniqueidentifier,
ZZC_DataSetPK uniqueidentifier,
ZZC_DataSetCode varchar(3)
);

CREATE TABLE RefCusTradeGroupCountry(
ZZB_PK uniqueidentifier NOT NULL,
ZZB_ZZA_TradeGroup uniqueidentifier,
ZZB_RN_NKTradeGroupCountryCode char(2)
);

Create Table RefCusPreference(
ZZS_PK uniqueidentifier,
ZZS_Preference varchar(10),
ZZS_ZZZ_NKDataGrouping varchar(3)
);
");
				var tariff = Guid.NewGuid();
				var tarifftype = Guid.NewGuid();
				var rateStandard = Guid.NewGuid();
				var rateSADC = Guid.NewGuid();
				var rateEU = Guid.NewGuid();
				var rateEFTA = Guid.NewGuid();
				var rateMercosur = Guid.NewGuid();
				var rateMercosur2 = Guid.NewGuid();
				var tradegroupStandard = Guid.NewGuid();
				var tradegroupEFTA = Guid.NewGuid();
				var tradegroupEU = Guid.NewGuid();
				var tradegroupSADC = Guid.NewGuid();
				var tradegroupMercosur = Guid.NewGuid();
				var preference100 = Guid.NewGuid();
				var preference200 = Guid.NewGuid();
				var preference400 = Guid.NewGuid();
				var tradeGroupCountryXX = Guid.NewGuid();
				var tradeGroupCountryYY = Guid.NewGuid();
				var tradeGroupXX = Guid.NewGuid();
				var tradeGroupYY = Guid.NewGuid();
				var rateCode = Guid.NewGuid();
				var tariff2 = Guid.NewGuid();
				var tariff2Rate = Guid.NewGuid();
				var rateNonZA = Guid.NewGuid();
				var tradegroupMontenegro = Guid.NewGuid();

				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping) VALUES ('{tariff}', '{tarifftype}','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTariffType (ZZI_PK, ZZI_TariffType,ZZI_Description,ZZI_ZZZ_NKDataGrouping) VALUES ('{tarifftype}', '1P1', '1P1','ZA');
");

				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping) 
VALUES ('{rateStandard}',null,'{tariff}','pp=''STANDARD''','1900-01-01','2079-06-06','{tariff}','ZZ1','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping) 
VALUES ('{rateEFTA}',null,'{tariff}','pp=''EFTA''','1900-01-01','2079-06-06','{tariff}','ZZ1','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping) 
VALUES ('{rateEU}',null,'{tariff}','pp=''EUTRADE''','1900-01-01','2079-06-06','{tariff}','ZZ1','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping) 
VALUES ('{rateSADC}',null,'{tariff}','pp=''SADC''','1900-01-01','2079-06-06','{tariff}','ZZ1','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping) 
VALUES ('{rateMercosur}',null,'{tariff}','pp=''MERCOSUR'' & (CofO=''XX'' | CofO=''YY'')','1900-01-01','2079-06-06','{tariff}','ZZ1','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping) 
VALUES ('{rateMercosur2}',null,'{tariff}','pp=''MERCOSUR''','1900-01-01','2079-06-06','{tariff}','ZZ1','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType,ZZ1_ZZZ_NKDataGrouping) VALUES ('{tariff2}', '{tarifftype}','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusRate(ZZ2_PK,ZZ2_ZZS_Preference,ZZ2_ZZ1_Tariff,ZZ2_SelectorFormula,ZZ2_StartDate,ZZ2_EndDate,ZZ2_DataSetPK,ZZ2_DataSetCode,ZZ2_ZZZ_NKDataGrouping) 
VALUES ('{tariff2Rate}',null,'{tariff2}','CofO=''XX''','1900-01-01','2079-06-06','{tariff2}','ZZ1','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping) 
VALUES ('{tradegroupStandard}','STANDARD','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping) 
VALUES ('{tradegroupEFTA}','EFTA','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping) 
VALUES ('{tradegroupEU}','EUTRADE','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping) 
VALUES ('{tradegroupSADC}','SADC','ZA');
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
INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping) 
VALUES ('{tradeGroupXX}','XX','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTradeGroup(ZZA_PK,ZZA_TradeGroup,ZZA_ZZZ_NKDataGrouping) 
VALUES ('{tradeGroupYY}','YY','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusPreference(ZZS_PK,ZZS_Preference,ZZS_ZZZ_NKDataGrouping) 
VALUES ('{preference100}','100','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusPreference(ZZS_PK,ZZS_Preference,ZZS_ZZZ_NKDataGrouping) 
VALUES ('{preference200}','200','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusPreference(ZZS_PK,ZZS_Preference,ZZS_ZZZ_NKDataGrouping) 
VALUES ('{preference400}','400','ZA');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTradeGroupCountry(ZZB_PK,ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode) 
VALUES ('{tradeGroupCountryXX}','{tradegroupMercosur}','XX');
");
				dbCreator.ExcuteDbScript(dbName, $@"
INSERT INTO RefCusTradeGroupCountry(ZZB_PK,ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode) 
VALUES ('{tradeGroupCountryYY}','{tradegroupMercosur}','YY');
");

				using (var trans = conn.BeginTransaction())
				{
					var task = new TransformDataSelectorFormulaTask(21);
					task.Run(trans);
					trans.Commit();
				}
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZ2_Rate = '{rateStandard}' AND ZZT_ZZA_TradeGroup = '{tradegroupStandard}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZ2_Rate = '{rateSADC}' AND ZZT_ZZA_TradeGroup = '{tradegroupSADC}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZ2_Rate = '{rateEFTA}' AND ZZT_ZZA_TradeGroup = '{tradegroupEFTA}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZ2_Rate = '{rateEU}' AND ZZT_ZZA_TradeGroup = '{tradegroupEU}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZ2_Rate = '{rateMercosur}' AND ZZT_ZZA_TradeGroup = '{tradeGroupXX}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZ2_Rate = '{rateMercosur}' AND ZZT_ZZA_TradeGroup = '{tradeGroupYY}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZ2_Rate = '{rateMercosur2}' AND ZZT_ZZA_TradeGroup = '{tradegroupMercosur}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZ2_Rate = '{rateMercosur2}' AND ZZT_ZZA_TradeGroup = '{tradegroupMontenegro}'";
					Assert.AreEqual(0, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZ2_Rate = '{tariff2Rate}' AND ZZT_ZZA_TradeGroup = '{tradeGroupXX}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());

					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusRateCode WHERE ZY1_RateCode = '1P1'";
					Assert.AreEqual(1, cmd.ExecuteScalar());

					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusRate WHERE ZZ2_PK = '{rateStandard}' AND ZZ2_ZZS_Preference = '{preference100}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusRate WHERE ZZ2_PK = '{rateEFTA}' AND ZZ2_ZZS_Preference = '{preference200}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusRate WHERE ZZ2_PK = '{rateEU}' AND ZZ2_ZZS_Preference = '{preference200}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusRate WHERE ZZ2_PK = '{rateSADC}' AND ZZ2_ZZS_Preference = '{preference200}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
					cmd.CommandText = $@"SELECT COUNT(*) FROM RefCusRate WHERE ZZ2_PK = '{rateMercosur}' AND ZZ2_ZZS_Preference = '{preference200}'";
					Assert.AreEqual(1, cmd.ExecuteScalar());
				}
				using (var trans = conn.BeginTransaction())
				{
					var task = new TransformDataSelectorFormulaTask(21);
					task.Run(trans);
					trans.Commit();
				}
			}
		}
	}
}
