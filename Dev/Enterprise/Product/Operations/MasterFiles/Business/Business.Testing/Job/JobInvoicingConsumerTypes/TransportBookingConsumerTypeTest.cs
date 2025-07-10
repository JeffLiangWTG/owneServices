using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TransportBookingConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.TransportBooking;
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceLocalTransport; }
		}

		public override void TestAllowRevenuePosting()
		{
			AssertEquals(false, ConsumerType.AllowRevenuePosting(null));
		}

		public override void TestAllowCostPosting()
		{
			AssertEquals(true, ConsumerType.AllowCostPosting(null));
		}

		public override void TestInvoicingPrintingApplicable()
		{
			AssertEquals(false, ConsumerType.InvoicingPrintingApplicable(null));
		}

		public override void TestCreditStatusApplicable()
		{
			AssertEquals(false, ConsumerType.CreditStatusApplicable(null));
		}

		public override void TestProfitLossApplicable()
		{
			AssertEquals(false, ConsumerType.ProfitLossApplicable(null));
		}

		public override void TestMenuName()
		{
			AssertEquals("&Quote and Costing", ConsumerType.MenuName(null));
		}

		public override void TestDisplayName()
		{
			AssertEquals("Quote and Costing", ConsumerType.DisplayName(null));
		}

		public override void TestRevenueChargeDescription()
		{
			AssertEquals("Quote Charges", ConsumerType.RevenueChargeDescription(null));
		}

		public override void TestSupportsWiseRates()
		{
			AssertEquals(true, ConsumerType.SupportsWiseRates);
		}
	}
}
