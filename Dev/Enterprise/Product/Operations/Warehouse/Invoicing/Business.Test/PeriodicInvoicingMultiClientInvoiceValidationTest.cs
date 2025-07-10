using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Invoicing.Business.Test
{
	[TestedType(typeof(PeriodicInvoicingMultiClientInvoiceValidation))]
	public class PeriodicInvoicingMultiClientInvoiceValidationTest : BusinessObjectValidationTestCase
	{
		#region TestInvoiceDateValidation

		public void TestInvoiceDateValidation()
		{
			var whsInvoicePeriodicMultiClientInvoice = new PeriodicInvoicingMultiClientInvoice(PeriodicInvoicingStorageTypes.Codes.ContainerYard);
			whsInvoicePeriodicMultiClientInvoice.InvoiceDate = ZDate.Empty;
			AssertHasError(whsInvoicePeriodicMultiClientInvoice.InvoiceDateInfo, "Please enter a value.");

			whsInvoicePeriodicMultiClientInvoice.InvoiceDate = ZDate.Invalid;
			AssertHasError(whsInvoicePeriodicMultiClientInvoice.InvoiceDateInfo, "Enter a valid selection.");

			whsInvoicePeriodicMultiClientInvoice.InvoiceDate = ZDate.Today;
			AssertNoErrors(whsInvoicePeriodicMultiClientInvoice.InvoiceDateInfo);
		}

		#endregion
	}
}
