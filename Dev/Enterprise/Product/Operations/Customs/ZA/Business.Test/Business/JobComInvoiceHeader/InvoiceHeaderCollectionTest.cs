using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	sealed class InvoiceHeaderCollectionTest : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection, JobComInvoiceHeader>
	{
		public void TestRemoveAndDeleteOriginal()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			AssertEquals("Precondition : Count", 1, declaration.Invoices.Count);
			declaration.Invoices.Delete(invoiceHeader);
			AssertEquals(true, invoiceHeader.IsDeleted);
			AssertEquals("Count", 0, declaration.Invoices.Count);
		}
	}

	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	sealed class GroupInvoiceDirectChildInvoiceHeaderCollectionTest : Customs.Business.Testing.BaseGroupInvoiceDirectChildInvoiceHeaderCollectionTest<InvoiceHeaderActiveCollection>
	{
	}
}
