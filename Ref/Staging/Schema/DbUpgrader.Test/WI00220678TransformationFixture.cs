using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader.Test
{
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	class WI00220678TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZA_NKTradeGroup = ''";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 0);
				cmd.CommandText = @"SELECT COUNT(*) FROM RefCusApplicability WHERE ZZT_ZZA_NKTradeGroup = 'STANDARD'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
				cmd.CommandText = @"SELECT COUNT(*) FROM DataProcessingInformation WHERE DPI_Status = 'ERR'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 0);
				cmd.CommandText = @"SELECT COUNT(*) FROM DataProcessingInformation WHERE DPI_Status = 'QUE'";
				Assert.AreEqual(Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture), 1);
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new WI00220678Transformation(4);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
DECLARE @SourcePK uniqueidentifier
Select @SourcePK = newid();

INSERT INTO [dbo].[SourceData] ([SDA_PK],[SDA_Source],[SDA_Filename],[SDA_Filetype],[SDA_ContentText],[SDA_Status],[SDA_CreatedTime],[SDA_ContentType],[SDA_SourceTime],[SDA_SubSource])
VALUES (@SourcePK,'ZAA','Test.txt','TXT','TEST','PRS',GetDate(),'PRO',GetDate(),'ZA Tariffs');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_NKTariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','1P1','101010',0,'Description','2015-01-01 00:00:00','2016-01-01 00:00:00','VAT','ZA','');

INSERT INTO [dbo].[RefCusRate] ([ZZ2_PK],[ZZ2_ZZ1_Tariff],[ZZ2_StartDate],[ZZ2_EndDate],[ZZ2_ZY1_NKRateCode],[ZZ2_ZY1_ZZR_NKRateType],[ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping],[ZZ2_RateFormula],[ZZ2_ZZS_NKPreference],[ZZ2_ZZS_ZZZ_NKDataGrouping],[ZZ2_SelectorFormula],[ZZ2_ZZZ_NKDataGrouping],[ZZ2_RateFormulaDerivedFrom])
VALUES ('30CCA83F-B359-42BC-AC57-B533ADCFBC72','30A0F7E1-EB26-4004-A4C2-5F18E500C1B3','1900-01-01 00:00:00','2079-06-06 23:59:00','DUT','DUT','ZA','0','PRE','ZA','Selector','ZA','0');

INSERT INTO [dbo].[RefCusApplicability] ([ZZT_ZZ2_Rate],[ZZT_StartDate],[ZZT_EndDate],[ZZT_ZZA_NKTradeGroup],[ZZT_ZZA_ZZZ_NKDataGrouping],[ZZT_AdditionalCode],[ZZT_OrderNumber])
VALUES ('30CCA83F-B359-42BC-AC57-B533ADCFBC72','1900-01-01','2079-06-06 23:59:00','','ZA','','')

INSERT INTO [dbo].[DataProcessingInformation]([DPI_Status],[DPI_Message],[DPI_SourceId],[DPI_ParentTableCode],[DPI_ParentPk])
VALUES ('ERR','Addition',@SourcePK,'ZZ1','30A0F7E1-EB26-4004-A4C2-5F18E500C1B3');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
