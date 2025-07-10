using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Module.Testing
{
	public class CommercialInvoiceFilterLookupsTest : TestCaseWithFactory
	{
		public void TestImportersAndSuppliersList()
		{
			AssertEquals("Importers should not be loaded by the property", false, lookups.Importers.IsLoaded);
			AssertEquals("Importers of correct type", typeof(ConsigneeCollection), lookups.Importers.GetType());
			AssertEquals("Suppliers should not be loaded by the property", false, lookups.Suppliers.IsLoaded);
			AssertEquals("Suppliers of correct type", typeof(ConsignorCollection), lookups.Suppliers.GetType());
		}

		public void TestLists()
		{
			AssertEquals("BranchList should not be loaded by the property", false, lookups.BranchList.IsLoaded);
			AssertEquals("BranchList of correct type", typeof(GlbBranchCollection), lookups.BranchList.GetType());
			AssertEquals("Shipping Lines", typeof(OrgHeaderCollection), lookups.ShippingLines.GetType());
			AssertEquals("Vessels", typeof(RefVesselCollection), lookups.VesselList.GetType());
		}

		public virtual void TestMessageTypesList()
		{
			AssertEquals("Message Types", typeof(JobMessageTypeList), lookups.MessageTypes.GetType());
			Assert(lookups.MessageTypes.ContainsCode(JobMessageTypeList.MoreCodes.AdvanceShippingNotice));
			var filterBizObj2 = new CommercialInvoiceFilterBusinessObject();
			var lookups2 = new CommercialInvoiceFilterLookups(filterBizObj2);
			AssertEquals("CommercialInvoiceFilterLookups.MessageTypes should be cached", lookups.MessageTypes, lookups2.MessageTypes);
		}

		CommercialInvoiceFilterLookups lookups;
		CommercialInvoiceFilterBusinessObject filterBizObj;
		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new CommercialInvoiceFilterBusinessObject();
			lookups = new CommercialInvoiceFilterLookups(filterBizObj);
		}
	}
}
