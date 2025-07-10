using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Warehouse.Transit.Module.WhsItemReceiveASNFilterBusinessObject;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemReceiveASNFilterBusinessObject))]
	public class WhsItemReceiveASNFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse1 = Helper.CreateTRWWarehouse();
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");
			var warehouse3 = Helper.CreateTRWWarehouse(warehouseCode: "WH3");

			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse1.PK);
			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse1.PK);
			var asn3 = Helper.CreateReceiveASN("ASN3", warehouse2.PK);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(asn1, asn2, asn3);
			var filter = (ModuleGuidFilter)filters.ModuleFilters[WhsItemReceiveASNFilterBusinessObject.Schema.Warehouse];
			filter.IsActive = true;
			AssertEquals(FilterCategories.Other, filter.Category);

			filter.Property = warehouse1.PK;
			Asserter.AssertMatches("Must only return asns for the given warehouse", filters.Filter, asn1, asn2);
			filter.Property = warehouse2.PK;
			Asserter.AssertMatches("Must only return asns for the given warehouse", filters.Filter, asn3);
			filter.Property = warehouse3.PK;
			Asserter.AssertMatches("Must only return asns for the given warehouse", filters.Filter);
		}

		#endregion

		#region TestASNID

		public void TestASNID()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var asn1 = Helper.CreateReceiveASN("TRT00000001", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("TRT00000002", warehouse.PK);
			Factory.Save();
			Asserter.AddToScope(asn1, asn2);

			var filters = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filters.ModuleFilters[WhsItemReceiveASNFilterBusinessObject.Schema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = warehouse.PK;
			var filter = (ModuleTextFilter)filters.ModuleFilters[FilterSchema.ASNID];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.Property = "TRT0000000";
			Asserter.AssertMatches("Return asns start with 'TRT0000000'", filters.Filter, asn1, asn2);

			filter.Property = asn1.WRP_ReferenceNumber;
			Asserter.AssertMatches("Return specific asn with given WRP_ReferenceNumber", filters.Filter, asn1);

			filter.Property = asn2.WRP_ReferenceNumber;
			Asserter.AssertMatches("Return specific asn with given WRP_ReferenceNumber", filters.Filter, asn2);

			filter.Property = "SOMETHING ELSE";
			Asserter.AssertMatches("No asn should return", filters.Filter);
		}

		#endregion

		#region TestASNExternalReference

		public void TestASNExternalReference()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			asn1.WRP_VehicleReference = "V0001";
			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			asn2.WRP_VehicleReference = "V0002";
			Factory.Save();
			Asserter.AddToScope(asn1, asn2);

			var filters = GetNewFilterStripBusinessObject();
			var warehouseFilter = (ModuleGuidFilter)filters.ModuleFilters[WhsItemReceiveASNFilterBusinessObject.Schema.Warehouse];
			warehouseFilter.IsActive = true;
			warehouseFilter.Property = warehouse.PK;
			var filter = (ModuleTextFilter)filters.ModuleFilters[FilterSchema.ASNExternalReference];
			filter.IsActive = true;
			AssertEquals(FilterCategories.TextSearch, filter.Category);

			filter.Property = "V";
			Asserter.AssertMatches("Return asns start with 'V'", filters.Filter, asn1, asn2);

			filter.Property = asn1.WRP_VehicleReference;
			Asserter.AssertMatches("Return specific asn with given WRP_ReferenceNumber", filters.Filter, asn1);

			filter.Property = asn2.WRP_VehicleReference;
			Asserter.AssertMatches("Return specific asn with given WRP_ReferenceNumber", filters.Filter, asn2);

			filter.Property = "SOMETHING ELSE";
			Asserter.AssertMatches("No asn should return", filters.Filter);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WhsItemReceiveASNFilterBusinessObject();
		}

		protected FilterStripAsserter<WhsItemReceiveASN> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<WhsItemReceiveASN>(Factory, asn => asn.WRP_ReferenceNumber)); }
		}
		FilterStripAsserter<WhsItemReceiveASN> asserter;

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
