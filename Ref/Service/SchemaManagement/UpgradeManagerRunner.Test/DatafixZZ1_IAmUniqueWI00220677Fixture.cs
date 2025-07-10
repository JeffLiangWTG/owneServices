using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DatafixZZ1_IAmUniqueWI00220677Fixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusTariff t JOIN RefDbVersionControl vc on vc.RVC_ParentPK = t.ZZ1_PK AND vc.RVC_Deleted = 0 Where ZZ1_TariffCode = '213030208' and ZZ1_IAmUnique = 1";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"Select Count(1) from RefCusTariff t JOIN RefCusTariffAttribute on ZZ3_ZZ1_Tariff = t.ZZ1_PK Join RefCusTariffRelationship on ZZH_ZZ1_Tariff = ZZ1_PK
Where ZZ1_TariffCode = '320120105' And ZZH_TariffCode = '56031' And ZZ3_Value = '52' And ZZ1_IAmUnique = 0";
				Assert.AreEqual(2, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"Select Count(1) from RefCusTariff t JOIN RefCusTariffAttribute on ZZ3_ZZ1_Tariff = t.ZZ1_PK Join RefCusTariffRelationship on ZZH_ZZ1_Tariff = ZZ1_PK
Where ZZ1_TariffCode = '320120105' And ZZH_TariffCode = '56039' And ZZ3_Value = '50' And ZZ1_IAmUnique = 1";
				Assert.AreEqual(2, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"Select Count(1) from RefCusTariff t JOIN RefCusTariffAttribute on ZZ3_ZZ1_Tariff = t.ZZ1_PK Join RefCusTariffRelationship on ZZH_ZZ1_Tariff = ZZ1_PK
Where ZZ1_TariffCode = '2101010' And ZZH_TariffCode = '1010' And ZZ3_Value = '10' And ZZ1_IAmUnique = 0";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
				cmd.CommandText = @"Select Count(1) from RefCusTariff t JOIN RefCusTariffAttribute on ZZ3_ZZ1_Tariff = t.ZZ1_PK Join RefCusTariffRelationship on ZZH_ZZ1_Tariff = ZZ1_PK
Where ZZ1_TariffCode = '2101010' And ZZH_TariffCode = '1020' And ZZ3_Value = '20' And ZZ1_IAmUnique = 1";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DatafixZZ1_IAmUniqueWI00220677(41);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
DROP INDEX IX_RefCusTariff_ZZ1_ZZZ_NKDataGrouping_ZZ1_TariffCode_ZZ1_ZZI_TariffType_ZZ1_EndDate_ZZ1_IAMUnique ON RefCusTariff;

INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','ZA','South Africa','ZA');

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'ZA','','0');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','1P1','1P1','ZA','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('B334FB42-863D-4596-BB39-EA2BD4617072','3P1','3P1','ZA','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('B444FB42-863D-4596-BB39-EA2BD4617072','2P1','2P1','ZA','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description)
VALUES ('DAA86362-673C-469B-AB26-E6B55B921B34','3P1','E9E87774-45AC-49CA-925B-6756A31C5E3A','4P1')

declare @TariffPK Uniqueidentifier
Set @TariffPK = newid();

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@TariffPK,'B334FB42-863D-4596-BB39-EA2BD4617072','320120105',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusTariffRelationship (ZZH_ZZ1_Tariff,ZZH_ZZI_TariffType,ZZH_TariffCode)
VALUES (@TariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','56031')

INSERT INTO [dbo].[RefCusTariffAttribute] ([ZZ3_ZZ1_Tariff],[ZZ3_Name],[ZZ3_Value])
VALUES (@TariffPK,'CheckDigit','52');

Set @TariffPK = newid();

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@TariffPK,'B334FB42-863D-4596-BB39-EA2BD4617072','320120105',1,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusTariffRelationship (ZZH_ZZ1_Tariff,ZZH_ZZI_TariffType,ZZH_TariffCode)
VALUES (@TariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','56039')

INSERT INTO [dbo].[RefCusTariffAttribute] ([ZZ3_ZZ1_Tariff],[ZZ3_Name],[ZZ3_Value])
VALUES (@TariffPK,'CheckDigit','50');

Set @TariffPK = newid();

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@TariffPK,'B334FB42-863D-4596-BB39-EA2BD4617072','320120105',1,'Description','2016-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusTariffRelationship (ZZH_ZZ1_Tariff,ZZH_ZZI_TariffType,ZZH_TariffCode)
VALUES (@TariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','56031')

INSERT INTO [dbo].[RefCusTariffAttribute] ([ZZ3_ZZ1_Tariff],[ZZ3_Name],[ZZ3_Value])
VALUES (@TariffPK,'CheckDigit','52');

Set @TariffPK = newid();

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@TariffPK,'B334FB42-863D-4596-BB39-EA2BD4617072','320120105',0,'Description','2016-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusTariffRelationship (ZZH_ZZ1_Tariff,ZZH_ZZI_TariffType,ZZH_TariffCode)
VALUES (@TariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','56039')

INSERT INTO [dbo].[RefCusTariffAttribute] ([ZZ3_ZZ1_Tariff],[ZZ3_Name],[ZZ3_Value])
VALUES (@TariffPK,'CheckDigit','50');

Set @TariffPK = newid();

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@TariffPK,'B444FB42-863D-4596-BB39-EA2BD4617072','213030208',0,'Description','2017-07-26 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusTariffRelationship (ZZH_ZZ1_Tariff,ZZH_ZZI_TariffType,ZZH_TariffCode)
VALUES (@TariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','56039')

INSERT INTO [dbo].[RefCusTariffAttribute] ([ZZ3_ZZ1_Tariff],[ZZ3_Name],[ZZ3_Value])
VALUES (@TariffPK,'CheckDigit','50');

Set @TariffPK = newid();

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@TariffPK,'B334FB42-863D-4596-BB39-EA2BD4617072','2101010',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusTariffRelationship (ZZH_ZZ1_Tariff,ZZH_ZZI_TariffType,ZZH_TariffCode)
VALUES (@TariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','1010')

INSERT INTO [dbo].[RefCusTariffAttribute] ([ZZ3_ZZ1_Tariff],[ZZ3_Name],[ZZ3_Value])
VALUES (@TariffPK,'CheckDigit','10');

Set @TariffPK = newid();

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES (@TariffPK,'B334FB42-863D-4596-BB39-EA2BD4617072','2101010',0,'Description','2016-01-01 00:00:00','2079-06-06 23:59:00','VAT','ZA','');

INSERT INTO RefCusTariffRelationship (ZZH_ZZ1_Tariff,ZZH_ZZI_TariffType,ZZH_TariffCode)
VALUES (@TariffPK,'4E70545B-4FE7-4EBC-91E4-A78108140A73','1020')

INSERT INTO [dbo].[RefCusTariffAttribute] ([ZZ3_ZZ1_Tariff],[ZZ3_Name],[ZZ3_Value])
VALUES (@TariffPK,'CheckDigit','20');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
