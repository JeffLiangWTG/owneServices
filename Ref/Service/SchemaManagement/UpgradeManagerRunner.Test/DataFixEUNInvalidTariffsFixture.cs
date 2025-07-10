using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixEUNInvalidTariffsFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select ZZ1_EndDate from RefCusTariff WHERE ZZ1_TariffCode IN ('0201100010','3215119010', '7304410099', '9880430000')";
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						Assert.AreEqual(reader.GetDateTime(0), new DateTime(2018, 10, 31, 23, 59, 00));
					}
				}
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixEUNInvalidTariffs(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"

INSERT INTO RefDataGrouping (ZZZ_PK,ZZZ_DataGrouping,ZZZ_Description,ZZZ_ZZZ_NKGrouping)
VALUES ('ED0CE102-2C22-4995-9F99-8D5825517E52','EUN','European','EUN');

INSERT INTO [dbo].[RefCusRateType] ([ZZR_PK],[ZZR_RateType],[ZZR_Description],[ZZR_IsPayable],[ZZR_ZZZ_NKDataGrouping],[ZZR_RX_NKFormulaCurrency],[ZZR_CustomsValueFormula])
VALUES ('E9E87774-45AC-49CA-925B-6756A31C5E3A','DUT','DUTY',1,'EUN','','0');

INSERT INTO [dbo].[RefCusTariffType] ([ZZI_PK],[ZZI_TariffType],[ZZI_Description],[ZZI_ZZZ_NKDataGrouping],[ZZI_ZZR_RateType],[ZZI_HasFormulaSpecificQuestions])
VALUES ('4E70545B-4FE7-4EBC-91E4-A78108140A73','IMP','IMP','EUN','E9E87774-45AC-49CA-925B-6756A31C5E3A',1);

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('C0CB362C-26C2-40CF-9B99-846503F705B8','4E70545B-4FE7-4EBC-91E4-A78108140A73','0201100010',0,'Description','2015-01-01 00:00:00','2079-06-06 23:59:00','VAT','EUN','');

INSERT INTO RefCusTariffRelationship (ZZH_PK,ZZH_ZZ1_Tariff,ZZH_ZZI_TariffType,ZZH_TariffCode)
VALUES ('537D8090-0057-4AA4-835F-8C3EBB773507','C0CB362C-26C2-40CF-9B99-846503F705B8','4E70545B-4FE7-4EBC-91E4-A78108140A73','70052925')

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping)
VALUES ('D987A1F5-AB3C-47AE-A6FA-4A6AABB8269A','C0CB362C-26C2-40CF-9B99-846503F705B8','2017-11-17 00:00:00','2079-06-06 23:59:00','0','','EUN');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping)
VALUES ('F06D342F-01F6-4CB3-B85E-AFAF4A33E706','C0CB362C-26C2-40CF-9B99-846503F705B8','2015-01-01 00:00:00','2017-11-16 23:59:00','0','','EUN');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping)
VALUES ('87D324DC-3499-4474-A4C2-2C26F530BBB6','C0CB362C-26C2-40CF-9B99-846503F705B8','2017-11-17 00:00:00','2079-06-06 23:59:00','0','','EUN');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES ('D5E2EA9D-4121-4C97-934E-E619B0610072','D987A1F5-AB3C-47AE-A6FA-4A6AABB8269A','2017-11-17 00:00:00','2079-06-06 23:59:00','Additional','OrderNO')

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES ('1EAFB347-7100-4381-94EB-4DE9230AB8FA','87D324DC-3499-4474-A4C2-2C26F530BBB6','2017-11-17 00:00:00','2079-06-06 23:59:00','Additional','OrderNO')

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES ('D64E748C-A410-4216-8AC7-73E04CFE81A3','F06D342F-01F6-4CB3-B85E-AFAF4A33E706','2015-01-01 00:00:00','2017-11-16 23:59:00','Additional','OrderNO')

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('85EBD672-A7E8-4578-A0B4-419A6731A75E','4E70545B-4FE7-4EBC-91E4-A78108140A73','3215119010',0,'Description','2016-05-27 00:00:00','2079-06-06 23:59:00','VAT','EUN','');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula)
VALUES ('91F932C3-D105-4CCC-80E9-968C8FB5B3D8','85EBD672-A7E8-4578-A0B4-419A6731A75E','2016-05-27 00:00:00','2079-06-06 23:59:00','0','');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula)
VALUES ('A7912BC1-856A-4A05-B55C-0B2979926EE9','85EBD672-A7E8-4578-A0B4-419A6731A75E','2016-05-27 00:00:00','2079-06-06 23:59:00','0','');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('67E46EEC-149E-4CBB-941F-1197F45CCB81','4E70545B-4FE7-4EBC-91E4-A78108140A73','7304410099',0,'Description','2015-04-01 00:00:00','2079-06-06 23:59:00','VAT','EUN','');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula)
VALUES ('3A148A3E-9110-458C-81F6-34545B608B01','67E46EEC-149E-4CBB-941F-1197F45CCB81','2015-04-01 00:00:00','2079-06-06 23:59:00','0','');

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula)
VALUES ('D1FF18A0-E267-4D7D-BE8D-1D5BFC1ECA4F','67E46EEC-149E-4CBB-941F-1197F45CCB81','2015-04-01 00:00:00','2079-06-06 23:59:00','0','');

INSERT INTO [dbo].[RefCusTariff] ([ZZ1_PK],[ZZ1_ZZI_TariffType],[ZZ1_TariffCode],[ZZ1_IAMUnique],[ZZ1_Description],[ZZ1_StartDate],[ZZ1_EndDate],[ZZ1_ZZF_NKTaxOrFeeCode],[ZZ1_ZZZ_NKDataGrouping],[ZZ1_CompositeKeyOnZZ5])
VALUES ('3759325B-B381-4894-B07D-8EA78294AE02','4E70545B-4FE7-4EBC-91E4-A78108140A73','9880430000',0,'Description','1997-08-01 00:00:00','2017-12-28 23:59:00','VAT','EUN','');

INSERT INTO RefCusTariffRelationship (ZZH_PK,ZZH_ZZ1_Tariff,ZZH_ZZI_TariffType,ZZH_TariffCode)
VALUES (newid(),'3759325B-B381-4894-B07D-8EA78294AE02','4E70545B-4FE7-4EBC-91E4-A78108140A73','5407')

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping)
VALUES ('5D018D61-462D-4B84-851F-B76EE17AD23F','3759325B-B381-4894-B07D-8EA78294AE02','1997-08-01 00:00:00','2017-12-28 23:59:00','0','','EUN');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (newid(),'5D018D61-462D-4B84-851F-B76EE17AD23F','1997-08-01 00:00:00','2017-12-28 23:59:00','Additional','OrderNO')

INSERT INTO RefCusRate (ZZ2_PK,ZZ2_ZZ1_Tariff,ZZ2_StartDate,ZZ2_EndDate,ZZ2_RateFormula,ZZ2_SelectorFormula,ZZ2_ZZZ_NKDataGrouping)
VALUES ('5EEE2603-FA12-415E-8AFF-CF91367D5406','3759325B-B381-4894-B07D-8EA78294AE02','1997-08-01 00:00:00','2017-12-28 23:59:00','0','','EUN');

INSERT INTO RefCusApplicability (ZZT_PK,ZZT_ZZ2_Rate,ZZT_StartDate,ZZT_EndDate,ZZT_AdditionalCode,ZZT_OrderNumber)
VALUES (newid(),'5EEE2603-FA12-415E-8AFF-CF91367D5406','1997-08-01 00:00:00','2017-12-28 23:59:00','Additional','OrderNO')

";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
