using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BusinessObjectExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestGetDecimalPlacesMetaData()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals(3, invoice.GetDecimalPlacesMetaData(JobComInvoiceHeaderSchema.JZ_Weight.Name));
			AssertEquals(3, invoice.GetDecimalPlacesMetaData(JobComInvoiceHeaderSchema.JZ_Volume.Name));

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(6, invoiceLine.GetDecimalPlacesMetaData(JobComInvoiceLineSchema.JI_CustomsQuantity.Name));
		}
	}
}
