using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class InvoiceHeaderUserControlAbstractTest : TestCaseWithFactory
	{
		public void TestInvoiceGetSet()
		{
			using (CommonInvoiceHeaderUserControl testControl = GetNewInvoiceHeaderUserControl())
			{
				var invoice = Factory.New<BaseJobComInvoiceHeader>();
				testControl.Invoice = invoice;
				AssertEquals("get/set OK", invoice, testControl.Invoice);
			}
		}

		public void TestInvoiceErrorReporterMessage()
		{
			using (CommonInvoiceHeaderUserControl testControl = GetNewInvoiceHeaderUserControl())
			{
				ErrorReporter.Clear();
				AssertNull("Should be null", testControl.Invoice);
				Assert("Should get error for accessing null property", ErrorReporter.LastMessageReported.Contains("Invoice property has not been set "));
			}

			ErrorReporter.Clear();
		}

		protected abstract CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl();
	}
}
