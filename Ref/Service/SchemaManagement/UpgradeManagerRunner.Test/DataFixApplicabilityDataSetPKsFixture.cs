using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixApplicabilityDataSetPKsFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = "SELECT ZZT_DataSetPK FROM RefCusApplicability WHERE ZZT_PK = '2CC6E559-E84B-48CD-9795-631EF9DE2446'";

				Assert.AreEqual(new Guid("30A0F7E1-EB26-4004-A4C2-5F18E500C1B3"), (Guid)cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixApplicabilityDataSetPKs(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','ZA','South Africa','ZA');

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'ZA','','0');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','1P1','1P1','ZA','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','4E70545B-4FE7-4EBC-91E4-A78108140A73','48114190',0,'Description','2015-01-01 00:00:00','2016-01-01 00:00:00','VAT','ZA','');

INSERT INTO RefCusPreference(ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
VALUES('3E82B265-4D78-412E-B428-4E4AE00A4B99', 'ANY', 'Testing', 'ZA')

INSERT INTO RefCusConditionType (ZX2_PK, ZX2_ConditionClass, ZX2_ConditionType, ZX2_Description, ZX2_ZZZ_NKDataGrouping)
VALUES('4ECE8DC2-8E84-493E-83BD-6C3195F2956E', 'CLASS', 'ZADOC', 'Testing', 'ZA')

INSERT INTO RefCusCondition (ZX1_PK, ZX1_ZX2_ConditionType, ZX1_ZZ1_Tariff, ZX1_StartDate, ZX1_EndDate, ZX1_ZZS_Preference, ZX1_ZZZ_NKDataGrouping)
VALUES('855B6E3C-64CE-4D92-9804-009802E9140F', '4ECE8DC2-8E84-493E-83BD-6C3195F2956E', '30A0F7E1-EB26-4004-A4C2-5F18E500C1B3', '2019-12-14 00:00:00', '2079-06-06 23:59:00', '3E82B265-4D78-412E-B428-4E4AE00A4B99', 'ZA')

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZX1_Conditions,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES ('2CC6E559-E84B-48CD-9795-631EF9DE2446','855B6E3C-64CE-4D92-9804-009802E9140F','2015-01-01 00:00:00','2019-01-01 00:00:00','Additional','OrderNO');

";
				cmd.ExecuteNonQuery();
			}

			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"
UPDATE RefCusApplicability SET ZZT_DataSetPK = '3E82B265-4D78-412E-B428-4E4AE00A4B99' where ZZT_PK = '2CC6E559-E84B-48CD-9795-631EF9DE2446'";
				cmd.ExecuteNonQuery();
			}
		}
	}
}
