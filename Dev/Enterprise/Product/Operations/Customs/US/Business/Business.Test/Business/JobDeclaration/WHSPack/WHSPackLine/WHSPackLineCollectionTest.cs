using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(WHSPackLineCollection))]
	class WHSPackLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNew_InvoiceLine()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var line = Declaration.WHSPackLines.AddNew(invoiceLine1);
			AssertEquals(1, Declaration.WHSPackLines.Count);
			AssertEquals(invoiceLine1.PK, line.US_JI_InvoiceLine);
		}

		public void TestAddNew_WHSPack()
		{
			var pack1 = Declaration.WHSPacks.AddNew();
			var pack2 = Declaration.WHSPacks.AddNew();
			var line = Declaration.WHSPackLines.AddNew(pack2);
			AssertEquals(1, Declaration.WHSPackLines.Count);
			AssertEquals(pack2.PK, line.US_B7_WHSPack);
		}

		public void TestLoadRightLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(0, declaration.WHSPackLines.Count);

			var pack = declaration.WHSPacks.AddNew();
			var line1 = declaration.WHSPackLines.AddNew(invoiceLine);
			var line2 = declaration.WHSPackLines.AddNew(invoiceLine);
			var line3 = declaration.WHSPackLines.AddNew(invoiceLine);

			var declaration2 = Factory.New<JobDeclaration>();
			var invoice2 = declaration2.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			var pack2 = declaration2.WHSPacks.AddNew();

			AssertEquals("US_B7_WHSPack", pack.PK, line1.US_B7_WHSPack);
			AssertEquals("US_B7_WHSPack", pack.PK, line2.US_B7_WHSPack);
			AssertEquals("US_B7_WHSPack", pack.PK, line3.US_B7_WHSPack);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var decLoaded = factory2.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Three lines", 3, decLoaded.WHSPackLines.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WHSPackLineCollection(Declaration);
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<WHSPackLine>();
		}
	}
}
