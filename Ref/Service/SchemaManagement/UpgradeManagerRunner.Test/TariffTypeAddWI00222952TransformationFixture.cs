using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class TariffTypeAddWI00222952TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusTariffType Where ZZI_TariffType = '6P5'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new TariffTypeAddWI00222952Transformation(36);
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
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','6P4','6P4','ZA','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description)
VALUES ('DAA86362-673C-469B-AB26-E6B55B921B34','1P1','E9E87774-45AC-49CA-925B-6756A31C5E3A','1P1')

";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
