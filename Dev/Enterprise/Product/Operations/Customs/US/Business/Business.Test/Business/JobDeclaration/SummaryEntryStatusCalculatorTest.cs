using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	public class SummaryEntryStatusCalculatorTest : TestCaseWithFactory
	{
		public void TestSummaryMessageStatus()
		{
			AssertEquals("Not Sent status", ImportMessageStatusList.Codes.NotSent, statusCalculator.SummaryMessageStatus);

			CusEntryHeader entryHeader1 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader1.CH_Status = ImportMessageStatusList.Codes.NotSent;
			AssertEquals("Not Sent status", ImportMessageStatusList.Codes.NotSent, statusCalculator.SummaryMessageStatus);

			entryHeader1.CH_Status = ImportMessageStatusList.Codes.ClearExportation;
			AssertEquals("Clear status", ImportMessageStatusList.Codes.ClearExportation, statusCalculator.SummaryMessageStatus);

			CusEntryHeader entryHeader2 = declaration.ActiveEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			entryHeader2.CH_Status = ImportMessageStatusList.Codes.AwaitingExportation;
			AssertEquals("Clear status", ImportMessageStatusList.Codes.ClearExportation, statusCalculator.SummaryMessageStatus);
		}

		JobDeclaration declaration;
		SummaryEntryStatusCalculator statusCalculator;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			statusCalculator = new SummaryEntryStatusCalculator(declaration);
		}
	}
}
