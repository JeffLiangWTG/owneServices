using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00189669TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusRate t Where ZZ2_ZZS_Preference = 'D087D6D0-371C-4B69-9B6C-782470C82209'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"Select Count(1) from RefCusRate t Where ZZ2_ZZS_Preference = 'BC3580F6-DD2A-4587-B0D8-6D91743F654F'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00189669Transformation(10);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','ZA','South Africa','ZA');

INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('A90427A3-4201-4EBE-BF84-00A2B1715769','EUN','EUN','EUN');

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'ZA','','0');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','1P1','1P1','ZA','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description)
VALUES ('DAA86362-673C-469B-AB26-E6B55B921B34','1P1','E9E87774-45AC-49CA-925B-6756A31C5E3A','1P1')

INSERT INTO RefCusPreference (ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping)
VALUES ('D087D6D0-371C-4B69-9B6C-782470C82209','100','ZA','ZA')

INSERT INTO RefCusPreference (ZZS_PK,ZZS_Preference,ZZS_Description,ZZS_ZZZ_NKDataGrouping)
VALUES ('BC3580F6-DD2A-4587-B0D8-6D91743F654F','100','EUN','EUN')

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('C0CB362C-26C2-40CF-9B99-846503F705B8','4E70545B-4FE7-4EBC-91E4-A78108140A73','213030208',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping,ZZ2_ZZS_Preference)
VALUES ('D987A1F5-AB3C-47AE-A6FA-4A6AABB8269A','C0CB362C-26C2-40CF-9B99-846503F705B8','2017-11-17 00:00:00','2079-06-06 23:59:00','0','','ZA','BC3580F6-DD2A-4587-B0D8-6D91743F654F');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('85EBD672-A7E8-4578-A0B4-419A6731A75E','4E70545B-4FE7-4EBC-91E4-A78108140A73','20041099',0,'Description','2016-05-27 00:00:00','2079-06-06 23:59:00','VAT','EUN','');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping,ZZ2_ZZS_Preference)
VALUES ('91F932C3-D105-4CCC-80E9-968C8FB5B3D8','85EBD672-A7E8-4578-A0B4-419A6731A75E','2016-05-27 00:00:00','2079-06-06 23:59:00','0','','EUN','BC3580F6-DD2A-4587-B0D8-6D91743F654F');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
