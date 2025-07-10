using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconChangedLine))]
	sealed class ReconChangedLineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestChange()
		{
			var changedLine = new ReconChangedLine(Factory);
			changedLine.US_CustomsValue = 1m;
			changedLine.US_OrigCustomsValue = 2m;
			AssertEquals("CustomsValueChange", -1m, changedLine.CustomsValueChange);
			changedLine.US_Duty = 3m;
			changedLine.US_OrigDuty = 1m;
			AssertEquals("DutyChange", 2m, changedLine.DutyChange);
			AssertEquals("Duty Rate", 3m, changedLine.DutyRate);
			AssertEquals("Orig Duty Rate", 0.5m, changedLine.OrigDutyRate);
			changedLine.US_CustomsValue = 0m;
			changedLine.US_OrigCustomsValue = 0m;
			AssertEquals("Duty Rate", 0m, changedLine.DutyRate);
			AssertEquals("Orig Duty Rate", 0m, changedLine.OrigDutyRate);
		}

		public void TestHasRateChanged()
		{
			var changedLine = new ReconChangedLine(Factory);
			changedLine.US_OrigTariff = "3201.90.10";
			changedLine.US_Tariff = "3201.90.10";
			changedLine.US_OrigDuty = 100m;
			changedLine.US_OrigCustomsValue = 200m;
			changedLine.US_Duty = 100m;
			changedLine.US_CustomsValue = 400m;
			AssertEquals("50.00%", changedLine.OrigDutyRateDesc);
			AssertEquals("25.00%", changedLine.DutyRateDesc);
			changedLine.US_OrigDutyRateDesc = "44c/KG";
			AssertEquals("44c/KG", changedLine.OrigDutyRateDesc);
			AssertEquals("44c/KG", changedLine.DutyRateDesc);
			changedLine.US_Tariff = "9102.11.10";
			AssertEquals("50.00%", changedLine.OrigDutyRateDesc);
			AssertEquals("25.00%", changedLine.DutyRateDesc);
		}

		public void TestRoundValues()
		{
			var changedLine = new ReconChangedLine(Factory);
			changedLine.US_CustomsValue = 123.25m;
			changedLine.US_OrigCustomsValue = 50.45m;
			changedLine.RoundValues();
			AssertEquals(123m, changedLine.US_CustomsValue);
			AssertEquals(50m, changedLine.US_OrigCustomsValue);
		}

		public void TestIReconEntryLine_IsNAFTARecon_OriginalCustomsValue_OriginalDuty()
		{
			var changedLine = new ReconChangedLine(Factory);
			changedLine.US_NAFTAReconIndicator = true;
			changedLine.US_OrigCustomsValue = 1m;
			changedLine.US_OrigDuty = 2m;
			changedLine.US_Year = "2023";
			var reconEntryLine = (IReconEntryLine)changedLine;
			Assert(reconEntryLine.IsNAFTARecon);
			AssertEquals(1m, reconEntryLine.OriginalCustomsValue);
			AssertEquals(2m, reconEntryLine.OriginalDuty);
			AssertEquals("2023", reconEntryLine.CalculateYear);
		}

		public void TestIReconSecondaryLine_OriginalCustomsValue_OriginalDuty()
		{
			var changedLine = new ReconChangedLine(Factory);
			changedLine.US_OrigCustomsValue = 1m;
			changedLine.US_OrigDuty = 2m;
			var reconSecondaryLine = (IReconSecondaryLine)changedLine;
			AssertEquals(1m, reconSecondaryLine.OriginalCustomsValue);
			AssertEquals(2m, reconSecondaryLine.OriginalDuty);
		}
	}
}
