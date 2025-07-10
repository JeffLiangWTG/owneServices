using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OneOffQuoteConsumerTypeTest : JobInvoicingConsumerTypeTest
	{
		#region WIP / Accrual Creation

		public void TestShouldCreateWIPsOrAccruals()
		{
			Assert(!JobInvoicingConsumerTypes.OneOffQuotation.ShouldCreateWIPs(null, ""));
			Assert(!JobInvoicingConsumerTypes.OneOffQuotation.ShouldCreateAccruals(null, ""));
		}

		public void TestShouldPostCharges()
		{
			var info = JobInvoicingConsumerTypes.OneOffQuotation.ShouldPostCharges(null, "", false);
			var message = "Spot Quotes/Quoted Bookings do not allow charges to be posted. Charges should be posted on the Shipment or standalone Booking only.";

			AssertEquals(false, info.PostAllowed);
			AssertEquals(message, info.ReasonForDisallowing);

			info = JobInvoicingConsumerTypes.OneOffQuotation.ShouldPostCharges(null, "", true);
			AssertEquals(false, info.PostAllowed);
			AssertEquals(message, info.ReasonForDisallowing);
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

		#endregion

		public override void TestAllowRevenuePosting()
		{
			var oneOffQuote = new OneOffQuoteMockJob(Factory, JobInvoicingConsumerTypes.OneOffQuotation);
			AssertEquals(true, ConsumerType.AllowRevenuePosting(oneOffQuote));

			((OneOffQuoteMockJobInvoicingSupporter)oneOffQuote.InvoicingSupporter).IsQuote = true;
			AssertEquals(false, ConsumerType.AllowRevenuePosting(oneOffQuote));
		}

		public override void TestAllowCostPosting()
		{
			var oneOffQuote = new OneOffQuoteMockJob(Factory, JobInvoicingConsumerTypes.OneOffQuotation);
			AssertEquals(true, ConsumerType.AllowCostPosting(oneOffQuote));

			((OneOffQuoteMockJobInvoicingSupporter)oneOffQuote.InvoicingSupporter).IsQuote = true;
			AssertEquals(false, ConsumerType.AllowCostPosting(oneOffQuote));
		}

		public override void TestMenuName()
		{
			var oneOffQuote = new OneOffQuoteMockJob(Factory, JobInvoicingConsumerTypes.OneOffQuotation);
			AssertEquals(null, ConsumerType.MenuName(oneOffQuote));

			((OneOffQuoteMockJobInvoicingSupporter)oneOffQuote.InvoicingSupporter).IsQuote = true;
			AssertEquals("&Quote Charges", ConsumerType.MenuName(oneOffQuote));
		}

		public override void TestDisplayName()
		{
			var oneOffQuote = new OneOffQuoteMockJob(Factory, JobInvoicingConsumerTypes.OneOffQuotation);
			AssertEquals("Billing", ConsumerType.DisplayName(oneOffQuote));

			((OneOffQuoteMockJobInvoicingSupporter)oneOffQuote.InvoicingSupporter).IsQuote = true;
			AssertEquals("Quote Charges", ConsumerType.DisplayName(oneOffQuote));
		}

		public override void TestRevenueChargeDescription()
		{
			var oneOffQuote = new OneOffQuoteMockJob(Factory, JobInvoicingConsumerTypes.OneOffQuotation);
			AssertEquals("Revenue", ConsumerType.RevenueChargeDescription(oneOffQuote));

			((OneOffQuoteMockJobInvoicingSupporter)oneOffQuote.InvoicingSupporter).IsQuote = true;
			AssertEquals("Quote Charges", ConsumerType.RevenueChargeDescription(oneOffQuote));
		}

		protected override JobInvoicingConsumerType GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.OneOffQuotation;
		}
		public override void TestSupportsWiseRates()
		{
			AssertEquals(true, ConsumerType.SupportsWiseRates);
		}

		protected override SecurityCheckpoint ExpectedDistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}

		public override void TestIsTransportModeSupported()
		{
			AssertEquals(true, ConsumerType.IsTransportModeSupported);
		}

		public override void TestIsDirectionSupported()
		{
			AssertEquals(true, ConsumerType.IsDirectionSupported);
		}
	}
}
