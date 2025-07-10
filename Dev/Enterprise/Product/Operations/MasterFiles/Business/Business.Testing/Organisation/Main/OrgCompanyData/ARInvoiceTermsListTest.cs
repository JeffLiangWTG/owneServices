using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class ARInvoiceTermsListTest : TestCaseWithFactory
	{
		public void TestUseMultipleInstallments()
		{
			var list = new ARInvoiceTermsList();
			Assert("List should not contain the MLI code", !list.ContainsCode(Constants.InvoiceTerms.MultipleInstallments));
		}

		public void Test_ARInvoiceTermsList_LSILocation_ShouldBeBeforeDLP()
		{
			var list = new ARInvoiceTermsList();

			Assert("LSI should be located before DLP", list.IndexOfCode(Constants.InvoiceTerms.LaterOfShipmentOrInvoiceDate) == list.IndexOfCode(Constants.InvoiceTerms.FromDeliveryOrPickupDate) - 1);
		}

		public void Test_ARInvoiceTermsList_DLPLocation_ShouldBeBeforePIA()
		{
			var list = new ARInvoiceTermsList();

			Assert("DLP should be located before PAI", list.IndexOfCode(Constants.InvoiceTerms.FromDeliveryOrPickupDate) == list.IndexOfCode(Constants.InvoiceTerms.PaymentInAdvance) - 1);
		}
	}
}
