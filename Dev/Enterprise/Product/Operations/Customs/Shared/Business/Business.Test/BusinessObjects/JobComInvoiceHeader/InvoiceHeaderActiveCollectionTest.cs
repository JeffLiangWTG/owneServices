using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderActiveCollection))]
	public class InvoiceHeaderActiveCollectionTest : TestCaseWithFactory
	{
		public void TestAddNew_IInvoiceHeaderActiveCollection()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceHeaders = new InvoiceHeaderActiveCollection(declaration);
			var newInvoice = ((IInvoiceHeaderActiveCollection)invoiceHeaders).AddNew();
			AssertEquals("The implementation of IInvoiceHeaderActiveCollection.AddNew() should be the same as this.AddNew()", 1, invoiceHeaders.Count);
			AssertEquals("The implementation of IInvoiceHeaderActiveCollection.AddNew() should be the same as this.AddNew()", true, invoiceHeaders.Contains(newInvoice));
		}
	}
}
