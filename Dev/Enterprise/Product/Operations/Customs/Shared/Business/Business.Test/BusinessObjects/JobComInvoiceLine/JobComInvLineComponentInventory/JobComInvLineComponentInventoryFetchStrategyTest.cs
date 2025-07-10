using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class JobComInvLineComponentInventoryFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView()
		{
			var collection = CreateCollectionToTest(Factory);
			TestFetchForView((JobComInvLineComponentInventory)collection[0], (JobComInvLineComponentInventory)collection[1]);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			var line = factory.Load<BaseJobComInvoiceLine>(jobComInvoiceLine.PK);
			var collection = new JobComInvLineComponentInventoryCollection(line);
			collection.Load();
			return collection;
		}

		protected override void SetUp()
		{
			var componentPart1 = Factory.New<OrgSupplierPart>();
			componentPart1.OP_PartNum = "PARTNUM1";
			var componentPart2 = Factory.New<OrgSupplierPart>();
			componentPart2.OP_PartNum = "PARTNUM2";

			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			jobComInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var collection = new JobComInvLineComponentInventoryCollection(jobComInvoiceLine);
			var component1 = collection.AddNew();
			var component2 = collection.AddNew();
			Factory.Save();
		}

		BaseJobComInvoiceLine jobComInvoiceLine;
	}
}
