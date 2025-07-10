using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	[TestedType(typeof(WhsInvoiceDocumentSupporter))]
	internal class WhsInvoiceDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		public override void TestGetContactOrganisation()
		{
			Assert(true);
		}

		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsPeriodicBilling, DocSupporter.BusinessContext);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<WhsInvoice>();
		}
		protected override bool GetExpectedShowReasonForNotPrinting() => false;

		protected override Core.Constants.DataContext DataContext => Core.Constants.DataContext.GenericFreightJob;

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsInvoicingCustomiseDocuments;
	}
}
