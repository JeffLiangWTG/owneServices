using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ReconEntryLineGroupTest : TestCaseWithFactory
	{
		public void TestIReconEntryLineGroup_IsNAFTARecon_OriginalCustomsValue_OriginalDuty()
		{
			var changedLine = new ReconChangedLine(Factory);
			changedLine.US_NAFTAReconIndicator = true;
			changedLine.US_OrigCustomsValue = 1m;
			changedLine.US_OrigDuty = 2m;
			changedLine.US_Year = "2023";
			var reconEntryLineGroup = (IReconEntryLineGroup)(new ReconEntryLineGroup(new List<IReconEntryLine>() { changedLine }));
			Assert(reconEntryLineGroup.IsNAFTARecon);
			AssertEquals(1m, reconEntryLineGroup.OriginalCustomsValue);
			AssertEquals(2m, reconEntryLineGroup.OriginalDuty);
			AssertEquals("2023", reconEntryLineGroup.CalculateYear);
		}
	}
}
