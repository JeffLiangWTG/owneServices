using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class JobComInvoiceLineFetchStrategyTestCase : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			BaseJobComInvoiceLine invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			JobComInvoiceLineFetchStrategy strategy = new JobComInvoiceLineFetchStrategy(invoiceLine);
			AssertEquals(invoiceLine, strategy.BusinessObject);
		}

		public void TestFetchForLoad()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();

			invoiceLine.JI_JZ = ZGuid.NewZGuid();
			invoiceLine.JI_OP = ZGuid.NewZGuid();
			invoiceLine.JI_CC = ZGuid.NewZGuid();
			invoiceLine.JI_CL = ZGuid.NewZGuid();
			var strategy = new JobComInvoiceLineFetchStrategy(invoiceLine);
			var count = Factory.ActiveTableFetchHints;
			strategy.FetchForLoad();
			AssertEquals("Some fetch hints should be added for Load Count", count + 3, Factory.ActiveTableFetchHints);
		}

		public void TestFetchForLoad_CusClassPartPivotHasImmediateFetchHint()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "123";
			var pivot = part.PivotsForBinding.AddNew();
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_OP = part.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var dec = newFactory.Load<BaseJobDeclaration>(declaration.PK);
			dec.SuspendInvoiceLineDataInitialization();
			_ = dec.InvoiceLines[0];
			newFactory.Load<BaseCusClassPartPivot>(ZGuid.NewZGuid().ToGuid());

			AssertEquals(1, newFactory.GetBizOsForPK(pivot.PK.ToGuid()).Length);
		}

		public void TestFetchForDeleteCore()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			Factory.Save();

			invoiceLine.Charges.AddNew();
			invoiceLine.InvoiceLineRefs.AddNew();
			invoiceLine.ContainersPivot.AddNew();
			invoiceLine.ComponentInventoryCollection.AddNew();

			var uNDG = Factory.NewWithValidTestData<MasterFiles.Business.UNDGDataItem>();
			uNDG.DI_ParentID = invoiceLine.PK;

			var address = Factory.NewWithValidTestData<MasterFiles.Business.JobDocAddress>();
			address.E2_ParentID = invoiceLine.PK;

			var strategy = new JobComInvoiceLineFetchStrategy(invoiceLine);
			var count = Factory.ActiveTableFetchHints;
			strategy.FetchForDelete();
			if (invoiceLine.SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				AssertEquals("Some Fetch hints should be added for preparing to delete", count + 6, Factory.ActiveTableFetchHints);
			}
			else
			{
				AssertEquals("Some Fetch hints should be added for preparing to delete", count + 6, Factory.ActiveTableFetchHints);
			}
		}
	}
}
