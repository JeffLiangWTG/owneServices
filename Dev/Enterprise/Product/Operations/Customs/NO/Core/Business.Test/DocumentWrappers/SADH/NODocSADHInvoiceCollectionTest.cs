using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NODocSADHInvoiceCollection))]
sealed class NODocSADHInvoiceCollectionTest : DocBaseWrapperCollectionTest<NODocSADHInvoiceCollection>
{
	protected override object GetNewObjectToWrap()
	{
		return InvoiceCollection[0];
	}

	protected override NODocSADHInvoiceCollection GetNewDocumentWrapperCollection()
	{
		return new (InvoiceCollection, Factory);
	}

	public InvoiceHeaderActiveCollection InvoiceCollection => invoiceCollection ??= CreateNewInvoiceCollection();
	InvoiceHeaderActiveCollection invoiceCollection;

	InvoiceHeaderActiveCollection CreateNewInvoiceCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		_ = declaration.Invoices.AddNew();
		return declaration.Invoices;
	}
}
