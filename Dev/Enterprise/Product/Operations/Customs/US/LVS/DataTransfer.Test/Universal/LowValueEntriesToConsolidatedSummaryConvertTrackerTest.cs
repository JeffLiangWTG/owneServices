using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.LVS.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	[TestedType(typeof(LowValueEntriesToConsolidatedSummaryConvertTracker))]
	class LowValueEntriesToConsolidatedSummaryConvertTrackerTest : TestCaseWithDummy
	{
		public void TestProgressUpdatedForCusUSLVConsignmentExported()
		{
			var monitor = new ProgressMonitorForTest();
			var tracker = new LowValueEntriesToConsolidatedSummaryConvertTracker(2, monitor.UpdateProgress);
			var dummyDO = new Mock<IDataObject>().Object;
			var consignment = Factory.New<CusUSLVConsignment>();

			CombineAssertions("progress gets updated only when CusUSLVConsignment is exported", () =>
			{
				tracker.NotifyExported(dummyDO, Dummy);

				AssertEquals(ZString.Empty, monitor.Text);
				AssertEquals(0, monitor.Progress);

				tracker.NotifyExported(dummyDO, consignment);

				AssertEquals("[1 / 2] invoice lines processed", monitor.Text);
				AssertEquals(50, monitor.Progress);
			});
		}

		public void TestProgressUpdatedForCommercialInvoiceHeaderImported()
		{
			var monitor = new ProgressMonitorForTest();
			var tracker = new LowValueEntriesToConsolidatedSummaryConvertTracker(2, monitor.UpdateProgress);
			var invoice = Factory.New<JobComInvoiceHeader>();

			CombineAssertions("progress gets updated only when CommercialInvoiceHeader is imported", () =>
			{
				tracker.FireDataImportedToBusinessObject(Dummy);

				AssertEquals(ZString.Empty, monitor.Text);
				AssertEquals(0, monitor.Progress);

				tracker.FireDataImportedToBusinessObject(invoice);

				AssertEquals("text should be empty because it's managed elsewhere", ZString.Empty, monitor.Text);
				AssertEquals(50, monitor.Progress);
			});
		}

		class ProgressMonitorForTest
		{
			public void UpdateProgress(ZString text, int progress)
			{
				Text = text;
				Progress = progress;
			}

			public ZString Text { get; private set; }
			public int Progress { get; private set; }
		}
	}
}
