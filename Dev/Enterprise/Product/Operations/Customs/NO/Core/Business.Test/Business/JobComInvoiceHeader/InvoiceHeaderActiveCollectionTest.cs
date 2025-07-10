using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	sealed class InvoiceHeaderCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
	}
}

