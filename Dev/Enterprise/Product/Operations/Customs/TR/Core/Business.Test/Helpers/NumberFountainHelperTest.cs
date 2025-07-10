using System;
using System.IO;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[UseSnapshotProtection]
	public class NumberFountainHelperTest : TestCaseWithFactory
	{
		[TestDate(2022, 11, 10)]
		public static ZString GetTxtFile(ZString fileName, string filePath = "Enterprise.Customs.TR.Business.Testing.TestFiles.")
		{
			var result = ZString.Empty;
			using (var stream = typeof(NumberFountainHelperTest).Assembly.GetManifestResourceStream(filePath + fileName))
			{
				if (stream != null)
				{
					result = new StreamReader(stream).ReadToEnd();
				}
			}
			return result;
		}

		[TestDate(2022, 08, 31)]
		public void TestGetNextStampDutyLedgerNumber()
		{
			var dataType = new StampDutyLedgerNumberCustomizationRegistryItemDataType();
			using (dataType.SuspendValidation())
			{
				var item = TRCustomsDataRegistry.Instance.StampDutyLedgerNumberCustomization;
				item.DataType = dataType;

				GlbCompany.CurrentCompany.GC_Code = "ULU";
				var setting = new StampDutyLedgerNumberCustomizationRegistrySetting { StartNumber = 1, ExpiredYear = 2022 };
				using (item.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, setting))
				{
					var number = StampDutyLedgerNumberFountainHelper.GetNextStampDutyLedgerNumber(Factory, GlbCompany.CurrentCompany.GC_Code);
					AssertEquals("Normal case", "ULU00000001", number);
				}

				GlbCompany.CurrentCompany.GC_Code = "WTG";
				setting.StartNumber = 3456;
				using (item.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, setting))
				{
					var number = StampDutyLedgerNumberFountainHelper.GetNextStampDutyLedgerNumber(Factory, GlbCompany.CurrentCompany.GC_Code);
					AssertEquals("Change company and min value", "WTG00003456", number);
				}

				setting.StartDate = new ZDateTime(2022, 07, 01);
				using (item.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, setting))
				{
					var number = StampDutyLedgerNumberFountainHelper.GetNextStampDutyLedgerNumber(Factory, GlbCompany.CurrentCompany.GC_Code);
					AssertEquals("Reset when start from july and exceed expired end", "WTG00000001", number);
				}
			}
		}

		[TestDate(2023, 06, 02)]
		public void TestGetNextStampDutyLedgerNumber_ResetWhenExceedExpiredEnd()
		{
			var dataType = new StampDutyLedgerNumberCustomizationRegistryItemDataType();
			using (dataType.SuspendValidation())
			{
				var item = TRCustomsDataRegistry.Instance.StampDutyLedgerNumberCustomization;
				item.DataType = dataType;

				GlbCompany.CurrentCompany.GC_Code = "WTG";
				var setting = new StampDutyLedgerNumberCustomizationRegistrySetting { StartDate = new ZDateTime(2022, 07, 01), StartNumber = 3456, ExpiredYear = 2022 };
				using (item.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, setting))
				{
					var number = StampDutyLedgerNumberFountainHelper.GetNextStampDutyLedgerNumber(Factory, GlbCompany.CurrentCompany.GC_Code);
					AssertEquals("WTG00000001", number);

					number = StampDutyLedgerNumberFountainHelper.GetNextStampDutyLedgerNumber(Factory, GlbCompany.CurrentCompany.GC_Code);
					AssertEquals("WTG00000002", number);
				}
			}
		}

		[TestDate(2022, 07, 08)]
		public void TestGetFiscalYear()
		{
			var year = StampDutyLedgerNumberFountainHelper.GetFiscalYear(ZDateTime.Empty, ZDateTime.Empty);
			AssertEquals(2022, year);

			year = StampDutyLedgerNumberFountainHelper.GetFiscalYear(new ZDateTime(2022, 07, 01), ZDateTime.Empty);
			AssertEquals(2023, year);

			year = StampDutyLedgerNumberFountainHelper.GetFiscalYear(ZDateTime.Empty, new ZDateTime(2022, 12, 31));
			AssertEquals(2022, year);

			year = StampDutyLedgerNumberFountainHelper.GetFiscalYear(ZDateTime.Empty, new ZDateTime(2022, 06, 30));
			AssertEquals(2023, year);

			year = StampDutyLedgerNumberFountainHelper.GetFiscalYear(ZDateTime.Empty, new ZDateTime(2021, 06, 30));
			AssertEquals(2023, year);

			year = StampDutyLedgerNumberFountainHelper.GetFiscalYear(new ZDateTime(2023, 04, 01), new ZDateTime(2024, 03, 31));
			AssertEquals(2023, year);

			year = StampDutyLedgerNumberFountainHelper.GetFiscalYear(new ZDateTime(2022, 04, 01), new ZDateTime(2023, 03, 31));
			AssertEquals(2023, year);
		}
	}
}
