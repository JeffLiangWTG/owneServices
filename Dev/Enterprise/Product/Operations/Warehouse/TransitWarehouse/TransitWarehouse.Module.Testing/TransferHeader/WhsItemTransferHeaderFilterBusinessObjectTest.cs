using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemTransferHeaderFilterBusinessObject))]
	public class WhsItemTransferHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse1 = Helper.CreateTRWWarehouse();
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");
			var warehouse3 = Helper.CreateTRWWarehouse(warehouseCode: "WH3");

			var trf1 = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TRF1", warehouse1.PK, false);
			var trf2 = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TRF2", warehouse1.PK, false);
			var trf3 = Helper.CreateTransitTransfer(TransferTypes.Codes.PIC, "TRF3", warehouse2.PK, false);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(trf1, trf2, trf3);
			var filter = (ModuleGuidFilter)filters.ModuleFilters[WhsItemTransferHeaderFilterBusinessObject.Schema.Warehouse];
			filter.IsActive = true;
			AssertEquals(FilterCategories.Other, filter.Category);

			filter.Property = warehouse1.PK;
			Asserter.AssertMatches("Must only return asns for the given warehouse", filters.Filter, trf1, trf2);
			filter.Property = warehouse2.PK;
			Asserter.AssertMatches("Must only return asns for the given warehouse", filters.Filter, trf3);
			filter.Property = warehouse3.PK;
			Asserter.AssertMatches("Must only return asns for the given warehouse", filters.Filter);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new WhsItemTransferHeaderFilterBusinessObject();

		protected FilterStripAsserter<WhsItemTransferHeader> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<WhsItemTransferHeader>(Factory, trf => trf.WTH_ReferenceNumber)); }
		}
		FilterStripAsserter<WhsItemTransferHeader> asserter;

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
