using Enterprise.Environment;
using Enterprise.Security;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class BaseShipmentConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		public override void TestIsTransportModeSupported()
		{
			AssertEquals(true, ConsumerType.IsTransportModeSupported);
		}

		public override void TestIsDirectionSupported()
		{
			AssertEquals(true, ConsumerType.IsDirectionSupported);
		}

		public override void TestSupportsWiseRates()
		{
			AssertEquals(true, ConsumerType.SupportsWiseRates);
		}

		public void TestShouldCreateWIPs()
		{
			var jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(false));
			AssertEquals(true, ConsumerType.ShouldCreateWIPs(jobInvoicingPlugIn, string.Empty));

			jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(true));
			AssertEquals(false, ConsumerType.ShouldCreateWIPs(jobInvoicingPlugIn, string.Empty));
		}

		public override void TestProfitLossApplicable()
		{
			var jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(false));
			AssertEquals(true, ConsumerType.ProfitLossApplicable(jobInvoicingPlugIn));

			jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(true));
			AssertEquals(false, ConsumerType.ProfitLossApplicable(jobInvoicingPlugIn));
		}

		public override void TestCreditStatusApplicable()
		{
			var jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(false));
			AssertEquals(true, ConsumerType.CreditStatusApplicable(jobInvoicingPlugIn));

			jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(true));
			AssertEquals(false, ConsumerType.CreditStatusApplicable(jobInvoicingPlugIn));
		}

		public override void TestInvoicingPrintingApplicable()
		{
			var jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(false));
			AssertEquals(true, ConsumerType.InvoicingPrintingApplicable(jobInvoicingPlugIn));

			jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(true));
			AssertEquals(false, ConsumerType.InvoicingPrintingApplicable(jobInvoicingPlugIn));
		}

		public override void TestAllowRevenuePosting()
		{
			var jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(false));
			AssertEquals(true, ConsumerType.AllowRevenuePosting(jobInvoicingPlugIn));

			jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(true));
			AssertEquals(false, ConsumerType.AllowRevenuePosting(jobInvoicingPlugIn));
		}

		public override void TestAllowCostPosting()
		{
			var jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(false));
			AssertEquals(true, ConsumerType.AllowCostPosting(jobInvoicingPlugIn));

			jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(true));
			AssertEquals(false, ConsumerType.AllowCostPosting(jobInvoicingPlugIn));
		}

		public override void TestMenuName()
		{
			var jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(false));
			AssertNull(ConsumerType.MenuName(jobInvoicingPlugIn));

			jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(true));
			AssertEquals("Invoicing menu name is incorrect for Booking With Quote", "&Quote Charges", ConsumerType.MenuName(jobInvoicingPlugIn).ToString());
		}

		public override void TestDisplayName()
		{
			var jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(false));
			AssertEquals("Name of tab must be Billing.", "Billing", ConsumerType.DisplayName(jobInvoicingPlugIn));

			jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(true));
			AssertEquals("Name of tab must be Quote Charges.", "Quote Charges", ConsumerType.DisplayName(jobInvoicingPlugIn));
		}

		public virtual void TestShouldDisplayClientContractNumber()
		{
			var jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(false));
			AssertEquals(true, ConsumerType.ShouldDisplayClientContractNumber(jobInvoicingPlugIn));

			jobInvoicingPlugIn = CreateJobInvoicingPlugin(CreateJobInvoicingSupporter(true));
			AssertEquals(false, ConsumerType.ShouldDisplayClientContractNumber(jobInvoicingPlugIn));
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint => Env.Security.RoadDistanceCalculationServiceForwarding;

		protected IJobInvoicingPlugIn CreateJobInvoicingPlugin(IJobInvoicingSupporter jobInvoicingSupporter)
		{
			var jobInvoicingPlugIn = new Mock<IJobInvoicingPlugIn>();
			jobInvoicingPlugIn.Setup(x => x.InvoicingSupporter).Returns(jobInvoicingSupporter);
			return jobInvoicingPlugIn.Object;
		}

		IJobInvoicingSupporter CreateJobInvoicingSupporter(bool isQuoteWithBooking)
		{
			var jobInvoicingSupporter = new Mock<IJobInvoicingSupporter>();
			jobInvoicingSupporter.Setup(x => x.IsBookingWithQuote).Returns(isQuoteWithBooking);
			return jobInvoicingSupporter.Object;
		}
	}
}
