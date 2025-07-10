using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00271682MeursingDataTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select Count(1) from RefCusRate r Where ZZ2_ZY1_RateCode = 'DAA86362-673C-469B-AB26-E6B55B921B34'";
				//Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar()));
				cmd.CommandText = @"Select Count(1) from RefCusRate r Where ZZ2_ZY1_RateCode <> 'DAA86362-673C-469B-AB26-E6B55B921B34'";
				Assert.AreEqual(1, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));

				cmd.CommandText = @"Select count(1) from RefCusApplicability where ZZT_OrderNumber <> ''";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00271682MeursingDataTransformation(49);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','EUN','EUN');

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'EUN','0');

INSERT INTO RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description)
VALUES ('DAA86362-673C-469B-AB26-E6B55B921B34','EA','E9E87774-45AC-49CA-925B-6756A31C5E3A','EA');

INSERT INTO RefCusRateCode (ZY1_PK,ZY1_RateCode,ZY1_ZZR_RateType,ZY1_Description)
VALUES (newid(),'EAR','E9E87774-45AC-49CA-925B-6756A31C5E3A','EAR');

INSERT INTO [RefCusTradeGroup] ([ZZA_PK],[ZZA_TradeGroup],[ZZA_Description],[ZZA_StartDate],[ZZA_EndDate],[ZZA_ZZZ_NKDataGrouping])
Select '5750DED7-BEE8-4447-992C-EC160DC34EF5','1011','1011','1900-01-01','2079-06-06','EUN'
union all
Select '1E634A7F-B22D-412A-967C-54A880F6F2E7','2200','2200','1900-01-01','2079-06-06','EUN'
GO
INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping])
VALUES ('6398B5FE-CAE4-44C3-9088-845808790522','CN','CN','EUN');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('C0CB362C-26C2-40CF-9B99-846503F705B8','6398B5FE-CAE4-44C3-9088-845808790522','7023',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','EUN','');

declare @ratePK uniqueidentifier
set @ratePK = newid()

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_ZZZ_NKDataGrouping,ZZ2_ZY1_RateCode)
VALUES (@ratePK,'C0CB362C-26C2-40CF-9B99-846503F705B8','2015-01-01 00:00:00','2079-06-06 23:59:00','100 * [DTN]','EUN','DAA86362-673C-469B-AB26-E6B55B921B34');

INSERT INTO [dbo].[RefCusApplicability] ([ZZT_PK],[ZZT_ZZ2_Rate],[ZZT_StartDate],[ZZT_EndDate],[ZZT_ZZA_TradeGroup],[ZZT_AdditionalCode],[ZZT_OrderNumber])
VALUES (newid(),@ratePK,'1900-01-01','2079-06-06','5750DED7-BEE8-4447-992C-EC160DC34EF5','1','incorrect');

set @ratePK = newid()

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_ZZZ_NKDataGrouping,ZZ2_ZY1_RateCode)
VALUES (@ratePK,'C0CB362C-26C2-40CF-9B99-846503F705B8','2015-01-01 00:00:00','2079-06-06 23:59:00','10 * [DTN]','EUN','DAA86362-673C-469B-AB26-E6B55B921B34');

INSERT INTO [dbo].[RefCusApplicability] ([ZZT_PK],[ZZT_ZZ2_Rate],[ZZT_StartDate],[ZZT_EndDate],[ZZT_ZZA_TradeGroup],[ZZT_AdditionalCode],[ZZT_OrderNumber])
VALUES (newid(),@ratePK,'1900-01-01','2079-06-06','1E634A7F-B22D-412A-967C-54A880F6F2E7','1','incorrect');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
