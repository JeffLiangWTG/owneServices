using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	class InvoiceHeaderCollectionTest : EU.Business.Declaration.Testing.InvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoice = dec.Invoices.AddNew();
			return invoice;
		}
	}
}
