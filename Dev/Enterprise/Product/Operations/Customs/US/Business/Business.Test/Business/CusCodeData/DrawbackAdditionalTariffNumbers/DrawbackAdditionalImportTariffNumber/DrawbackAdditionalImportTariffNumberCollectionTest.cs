using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DrawbackAdditionalImportTariffNumberCollection))]
	sealed class DrawbackAdditionalImportTariffNumberCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestContainsTariffNumber()
		{
			var collection = InvoiceLine.DrawbackAdditionalImportTariffNumbers;
			var newElement = collection.AddNew();
			newElement.US_FormattedTariff = "1111.11.1111";
			Assert("Contains", collection.ContainsTariffNumber("1111111111"));
			Assert("Contains", collection.ContainsTariffNumber("1111.11.1111"));
			Assert("Does not contain", !collection.ContainsTariffNumber("1111111112"));
			Assert("Does not contain", !collection.ContainsTariffNumber("1111.11.1112"));
		}
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DrawbackAdditionalImportTariffNumberCollection(InvoiceLine);
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					var invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}
	}
}
