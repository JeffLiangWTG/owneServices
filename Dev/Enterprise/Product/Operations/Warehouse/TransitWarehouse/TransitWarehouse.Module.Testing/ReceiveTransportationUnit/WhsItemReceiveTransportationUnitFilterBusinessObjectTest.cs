using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsItemReceiveTransportationUnitFilterBusinessObject))]
	public class WhsItemReceiveTransportationUnitFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse1 = Helper.CreateTRWWarehouse();
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");
			var warehouse3 = Helper.CreateTRWWarehouse(warehouseCode: "WH3");

			var rtu1 = Helper.CreateReceiveTransportationUnit("RCN1", warehouse1.PK, warehouse1.DefaultOutboundDockDoorLocation.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RCN2", warehouse1.PK, warehouse1.DefaultOutboundDockDoorLocation.PK);
			var rtu3 = Helper.CreateReceiveTransportationUnit("RCN3", warehouse2.PK, warehouse2.DefaultOutboundDockDoorLocation.PK);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rtu1, rtu2, rtu3);
			var filter = (ModuleGuidFilter)filters.ModuleFilters[WhsItemReceiveTransportationUnitFilterBusinessObject.Schema.Warehouse];
			filter.IsActive = true;
			AssertEquals(FilterCategories.Other, filter.Category);

			filter.Property = warehouse1.PK;
			Asserter.AssertMatches("Must only return transportation units for the given warehouse", filters.Filter, rtu1, rtu2);
			filter.Property = warehouse2.PK;
			Asserter.AssertMatches("Must only return transportation units for the given warehouse", filters.Filter, rtu3);
			filter.Property = warehouse3.PK;
			Asserter.AssertMatches("Must only return transportation units for the given warehouse", filters.Filter);
		}

		#endregion

		#region TestJobID

		public void TestJobID()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var dock = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rtu1 = Helper.CreateReceiveTransportationUnit("TR00000001", warehouse.PK, dock.PK, "VehRef001");
			var rtu2 = Helper.CreateReceiveTransportationUnit("TR00000002", warehouse.PK, dock.PK, "VehRef002");
			var rtu3 = Helper.CreateReceiveTransportationUnit("TR00000011", warehouse.PK, dock.PK, "VehRef011");

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();
			Asserter.AddToScope(rtu1, rtu2, rtu3);

			var filter = (ModuleFountainFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.JobID];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "TR0000000";
			Asserter.AssertMatches("Must only return rtus with Job ID starting with TR0000000", filters.Filter, rtu1, rtu2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "TR00000011";
			Asserter.AssertMatches("Must return the rtu with Job ID TR00000011", filters.Filter, rtu3);

			filter.Property = "";
			Asserter.AssertMatches("Must return all rtus", filters.Filter, rtu1, rtu2, rtu3);
		}

		#endregion

		#region TestReferenceNumber

		public void TestReferenceNumber()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var dock = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, dock.PK, "VehRef001");
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU002", warehouse.PK, dock.PK, "VehRef002");
			var rtu3 = Helper.CreateReceiveTransportationUnit("RTU011", warehouse.PK, dock.PK, "VehRef011");

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();
			Asserter.AddToScope(rtu1, rtu2, rtu3);

			var filter = (ModuleNumberFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.ReferenceNumber];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "VehRef00";
			Asserter.AssertMatches("Must only return rtus with Job ID starting with VehRef00", filters.Filter, rtu1, rtu2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "VehRef011";
			Asserter.AssertMatches("Must return the rtu with Job ID VehRef011", filters.Filter, rtu3);

			filter.Property = "";
			Asserter.AssertMatches("Must return all rtus", filters.Filter, rtu1, rtu2, rtu3);
		}

		#endregion

		#region TestUnloadComplete

		public void TestUnloadComplete()
		{
			var now = ZDateTimeOffset.Now;
			var nowZDateTime = now.ToZDateTime();

			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var dock = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, dock.PK, "VehRef001");
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU002", warehouse.PK, dock.PK, "VehRef002");
			var rtu3 = Helper.CreateReceiveTransportationUnit("RTU011", warehouse.PK, dock.PK, "VehRef011");
			Helper.FinishUnloading(rtu1, now.AddDays(-6), now.AddDays(-4), now.AddDays(-2));
			Helper.FinishUnloading(rtu2, now.AddDays(-4), now.AddDays(-2), now);

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();
			Asserter.AddToScope(rtu1, rtu2, rtu3);

			var filter = (ModuleDateTimeOffsetFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.UnloadComplete];
			filter.IsActive = true;
			AssertEquals(FilterCategories.Dates, filter.Category);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, rtu1, rtu2, rtu3);

			filter.Property1 = nowZDateTime.AddDays(-5);
			filter.Property2 = nowZDateTime.AddDays(-3);
			Asserter.AssertMatches("Should only show first rtu", filter, rtu1);

			filter.Property1 = nowZDateTime.AddDays(-3);
			filter.Property2 = nowZDateTime.AddDays(-1);
			Asserter.AssertMatches("Should only show 2nd rtu", filter, rtu2);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, rtu1, rtu2);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, rtu3);
		}

		#endregion

		#region TestTransportCompanyName

		public void TestTransportCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var location = Helper.CreateLocation(warehouse);
			var transportCompany1 = Helper.CreateClient("TC1");
			var transportCompany2 = Helper.CreateClient("TC2");

			var rtuWithTransportCompany1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rtuWithTransportCompany2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var dcnWithTransportCompany1CompanyOverride = Helper.CreateReceiveTransportationUnit("RTU3", warehouse.PK, location.PK);
			var dcnWithNoTransportCompany = Helper.CreateReceiveTransportationUnit("RTU4", warehouse.PK, location.PK);
			var dcnWithEmptyTransportCompany = Helper.CreateReceiveTransportationUnit("RTU5", warehouse.PK, location.PK);
			Helper.CreateJobDocAddressFromAddress(rtuWithTransportCompany1, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(rtuWithTransportCompany2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany2.MainAddress);
			Helper.CreateJobDocAddressWithOverride(dcnWithTransportCompany1CompanyOverride, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, "TC1");
			Helper.CreateJobDocAddressWithOverride(dcnWithEmptyTransportCompany, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rtuWithTransportCompany1, rtuWithTransportCompany2, dcnWithTransportCompany1CompanyOverride, dcnWithNoTransportCompany, dcnWithEmptyTransportCompany);
			var crbFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.TransportCompanyName];
			crbFilter.IsActive = true;
			crbFilter.Property = transportCompany1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, crbFilter.Category);
			Asserter.AssertMatches("Must only return RTUs for TC1", filters.Filter, rtuWithTransportCompany1, dcnWithTransportCompany1CompanyOverride);

			crbFilter.Property = transportCompany2.OH_FullName;
			Asserter.AssertMatches("Must only return RTUs for TC2", filters.Filter, rtuWithTransportCompany2);

			crbFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all RTUs", filters.Filter, rtuWithTransportCompany1, rtuWithTransportCompany2, dcnWithTransportCompany1CompanyOverride, dcnWithNoTransportCompany, dcnWithEmptyTransportCompany);

			crbFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("Must return RTUs with no company", filters.Filter, dcnWithNoTransportCompany, dcnWithEmptyTransportCompany);
		}

		#endregion

		#region TestBillingPartyCompanyName

		public void TestBillingPartyCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var location = Helper.CreateLocation(warehouse);
			var billing1 = Helper.CreateClient("CRB1");
			var billing2 = Helper.CreateClient("CRB2");

			var rtuWithBilling1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rtuWithBilling2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var dcnWithBilling1CompanyOverride = Helper.CreateReceiveTransportationUnit("RTU3", warehouse.PK, location.PK);
			var dcnWithNoBilling = Helper.CreateReceiveTransportationUnit("RTU4", warehouse.PK, location.PK);
			var dcnWithEmptyBilling = Helper.CreateReceiveTransportationUnit("RTU5", warehouse.PK, location.PK);
			Helper.CreateJobDocAddressFromAddress(rtuWithBilling1, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(rtuWithBilling2, DocAddressTypes.Codes.ClientRequestedBillingParty, billing2.MainAddress);
			Helper.CreateJobDocAddressWithOverride(dcnWithBilling1CompanyOverride, DocAddressTypes.Codes.ClientRequestedBillingParty, "CRB1");
			Helper.CreateJobDocAddressWithOverride(dcnWithEmptyBilling, DocAddressTypes.Codes.ClientRequestedBillingParty);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rtuWithBilling1, rtuWithBilling2, dcnWithBilling1CompanyOverride, dcnWithNoBilling, dcnWithEmptyBilling);
			var crbFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.BillingPartyCompanyName];
			crbFilter.IsActive = true;
			crbFilter.Property = billing1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, crbFilter.Category);
			Asserter.AssertMatches("Must only return RTUs for CRB1", filters.Filter, rtuWithBilling1, dcnWithBilling1CompanyOverride);

			crbFilter.Property = billing2.OH_FullName;
			Asserter.AssertMatches("Must only return RTUs for CRB2", filters.Filter, rtuWithBilling2);

			crbFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all RTUs", filters.Filter, rtuWithBilling1, rtuWithBilling2, dcnWithBilling1CompanyOverride, dcnWithNoBilling, dcnWithEmptyBilling);

			crbFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("Must return RTUs with no company", filters.Filter, dcnWithNoBilling, dcnWithEmptyBilling);
		}

		#endregion

		#region Billing

		public void TestJobInvoicingStatusFilter()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var location = Helper.CreateLocation(warehouse);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			Asserter.AddToScope(rtu1);

			var filters = GetNewFilterStripBusinessObject();
			AssertNotNull(filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.InvoiceStatus]);
			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.InvoiceStatus];

			var job = new JobHeader.Loader(rtu1).TryLoadOrCreate();
			AssertEquals("Precondition", JobHeaderStatus.Working.Code, job.JH_Status);

			Factory.Save();

			filter.Property = JobHeaderStatus.Working.Code;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.IsActive = true;

			Asserter.AssertMatches("Should find the RTU with the given status", filter, rtu1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;

			Asserter.AssertMatches("Should exclude the RTU with the given status", filter);
		}

		public void TestInvoicedChargesFilter()
		{
			var currentBranch = GlbBranch.CurrentBranch;
			var code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR"));

			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			var location = Helper.CreateLocation(warehouse);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var job = new JobHeader.Loader(rtu).TryLoadOrCreate();
			job.JH_GB = currentBranch.PK;

			var filters = GetNewFilterStripBusinessObject();
			AssertNotNull(filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.InvoicedChargesBilling]);
			var invoicedChargesFilter = (ModuleFlagsFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.InvoicedChargesBilling];
			Asserter.AddToScope(rtu);

			Factory.Save();

			invoicedChargesFilter.IsActive = true;
			invoicedChargesFilter["No Charges"] = true;
			Asserter.AssertMatches("Must return RTUs with no charges", invoicedChargesFilter, rtu);

			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = code.PK;
			charge.JR_GB = currentBranch.PK;
			charge.JR_LocalSellAmt = 10m;
			Factory.Save();

			Asserter.AssertMatches("All RTUs have charges and should not be returned", invoicedChargesFilter);
		}

		class WhsItemReceiveTransportationUnitFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<WhsItemReceiveTransportationUnit>
		{
			#region Implementation

			protected override WhsItemReceiveTransportationUnit GetNewBusinessObjectForFilterCollection()
			{
				var poke = Warehouse;
				var rtu = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
				rtu.WRH_WW_Warehouse = Warehouse.PK;
				return rtu;
			}

			protected override ModuleIdentifier FilterStripModuleID
			{
				get { return ModuleIDs.WhsItemReceiveTransportationUnit; }
			}

			WhsWarehouse Warehouse
			{
				get
				{
					if (warehouse == null)
					{
						var address = Factory.NewWithValidTestData<OrgAddress>();
						warehouse = new WhsTransitTestHelper(Factory).CreateWarehouse("WH1", address, GlbBranch.CurrentBranch);
						warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
						Factory.Save();
					}

					return warehouse;
				}
			}

			WhsWarehouse warehouse;

			#endregion
		}

		#endregion

		#region Container Type

		public void TestContainerType()
		{
			var packingHelper = new PackingTestHelper(Factory);

			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var location = Helper.CreateLocation(warehouse);

			var rtuWith20GP = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK, containerType: "20GP");
			var rtuWith40GP = Helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, location.PK, containerType: "40GP");
			var rtuWithoutContainer = Helper.CreateReceiveTransportationUnit("RTU3", warehouse.PK, location.PK);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rtuWith20GP, rtuWith40GP, rtuWithoutContainer);
			var containerTypeFilter = (ModuleGuidFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.ContainerType];
			containerTypeFilter.IsActive = true;
			containerTypeFilter.Property = packingHelper.LoadRefContainer("20GP").PK;

			AssertEquals(FilterCategories.ModesAndTypes, containerTypeFilter.Category);
			Asserter.AssertMatches("Must only return RTU with 20GP", filters.Filter, rtuWith20GP);

			containerTypeFilter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Must return all RTUs", filters.Filter, rtuWith20GP, rtuWith40GP, rtuWithoutContainer);

			containerTypeFilter.Property = packingHelper.LoadRefContainer("20FR").PK;
			Asserter.AssertMatches("Must not return any RTUs", filters.Filter);
		}

		#endregion

		public void TestProfitLossReasonFilterWithOperators()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var location = Helper.CreateLocation(warehouse);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var job1 = new JobHeader.Loader(rtu1).TryLoadOrCreate();
			job1.JH_ProfitLossReasonCode = "ND1";

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var job2 = new JobHeader.Loader(rtu2).TryLoadOrCreate();
			job2.JH_ProfitLossReasonCode = "CD1";

			var rtu3 = Helper.CreateReceiveTransportationUnit("RTU3", warehouse.PK, location.PK);
			var job3 = new JobHeader.Loader(rtu3).TryLoadOrCreate();
			job3.JH_ProfitLossReasonCode = string.Empty;
			Factory.Save();

			Asserter.AddToScope(rtu1, rtu2, rtu3);

			var filters = GetNewFilterStripBusinessObject();
			var profitLossReasonFilter = (ModuleTextFilter)filters["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for rtu1.", profitLossReasonFilter, rtu1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for rtu1.", profitLossReasonFilter, rtu1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			Asserter.AssertMatches("Has a Match for rtu1 and rtu2", profitLossReasonFilter, rtu1, rtu2);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for rtu2 and rtu3", profitLossReasonFilter, rtu2, rtu3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for rtu2 and rtu3", profitLossReasonFilter, rtu2, rtu3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for rtu2 and rtu3", profitLossReasonFilter, rtu2, rtu3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "";

			Asserter.AssertMatches("Has a Match for rtu3", profitLossReasonFilter, rtu3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for rtu1 and rtu2", profitLossReasonFilter, rtu1, rtu2);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WhsItemReceiveTransportationUnitFilterBusinessObject();
		}

		protected FilterStripAsserter<WhsItemReceiveTransportationUnit> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<WhsItemReceiveTransportationUnit>(Factory, rtu => rtu.WRH_ReferenceNumber)); }
		}
		FilterStripAsserter<WhsItemReceiveTransportationUnit> asserter;

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
