using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixZARefCusTariffRelationshipTariffTypeTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusTariffRelationship r JOIN RefCusTariffType tt on tt.ZZI_PK = r.ZZH_ZZI_TariffType Where tt.ZZI_TariffType = '1P1'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixZARefCusTariffRelationshipTariffTypeTask(8);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','ZA','South Africa','ZA');

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'ZA','','0');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','1P1','1P1','ZA','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('38A6CAE8-6AD8-429D-A8A4-F1C7BECB071F','5P1','5P1','ZA','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','4E70545B-4FE7-4EBC-91E4-A78108140A73','501010101',0,'Shedule 5 tariff','2015-01-01 00:00:00','2016-01-01 00:00:00','VAT','ZA','');

INSERT INTO RefCusTariffRelationship (ZZH_PK,ZZH_ZZ1_Tariff,ZZH_ZZI_TariffType,ZZH_TariffCode)
VALUES ('8C9BD150-8ED8-4571-84A3-FED458AE1C0E','30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','38A6CAE8-6AD8-429D-A8A4-F1C7BECB071F','10101');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
