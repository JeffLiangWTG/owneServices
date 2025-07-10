using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoicePackingWeightListDocumentWrapper))]
	sealed class InvoicePackingWeightListDocumentWrapperTest : PackingWeightListDocumentWrapperAbstractTest<InvoicePackingWeightListDocumentWrapper>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invoiceHeader = Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew();
			return new InvoicePackingWeightListDocumentWrapper(invoiceHeader, Factory);
		}

		protected override InvoicePackingWeightListDocumentWrapper GetPackingWeightListDocumentWrapper()
		{
			var invoiceHeader = Factory.NewWithValidTestData<JobDeclaration>().Invoices.AddNew();
			return new InvoicePackingWeightListDocumentWrapper(invoiceHeader, Factory);
		}
	}
}
