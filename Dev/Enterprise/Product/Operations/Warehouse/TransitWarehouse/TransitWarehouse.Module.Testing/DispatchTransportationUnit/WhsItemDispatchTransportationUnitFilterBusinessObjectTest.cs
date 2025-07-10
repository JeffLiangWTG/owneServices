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
	[TestedType(typeof(WhsItemDispatchTransportationUnitFilterBusinessObject))]
	class WhsItemDispatchTransportationUnitFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse1 = Helper.CreateTRWWarehouse();
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");
			var warehouse3 = Helper.CreateTRWWarehouse(warehouseCode: "WH3");

			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse1.PK);
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse1.PK);
			var dtu3 = Helper.CreateDispatchTransportationUnit("DTU3", warehouse2.PK);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dtu1, dtu2, dtu3);
			var filter = (ModuleGuidFilter)filters.ModuleFilters[WhsItemDispatchTransportationUnitFilterBusinessObject.Schema.Warehouse];
			filter.IsActive = true;
			AssertEquals(FilterCategories.Other, filter.Category);

			filter.Property = warehouse1.PK;
			Asserter.AssertMatches("Must only return transportation units for the given warehouse", filters.Filter, dtu1, dtu2);
			filter.Property = warehouse2.PK;
			Asserter.AssertMatches("Must only return transportation units for the given warehouse", filters.Filter, dtu3);
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

			var dtu1 = Helper.CreateDispatchTransportationUnit("TD00000001", warehouse.PK, "VehRef001");
			var dtu2 = Helper.CreateDispatchTransportationUnit("TD00000002", warehouse.PK, "VehRef002");
			var dtu3 = Helper.CreateDispatchTransportationUnit("TD00000011", warehouse.PK, "VehRef011");

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();
			Asserter.AddToScope(dtu1, dtu2, dtu3);

			var filter = (ModuleFountainFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.JobID];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "TD0000000";
			Asserter.AssertMatches("Must only return DTUs with Job ID starting with TD0000000", filters.Filter, dtu1, dtu2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "TD00000011";
			Asserter.AssertMatches("Must return the DTU with Job ID TD00000011", filters.Filter, dtu3);

			filter.Property = "";
			Asserter.AssertMatches("Must return all DTUs", filters.Filter, dtu1, dtu2, dtu3);
		}

		#endregion

		#region TestReferenceNumber

		public void TestReferenceNumber()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var dock = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK, "VehRef001");
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU002", warehouse.PK, "VehRef002");
			var dtu3 = Helper.CreateDispatchTransportationUnit("DTU011", warehouse.PK, "VehRef011");

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();
			Asserter.AddToScope(dtu1, dtu2, dtu3);

			var filter = (ModuleNumberFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.ReferenceNumber];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "VehRef00";
			Asserter.AssertMatches("Must only return DTUs with Job ID starting with VehRef00", filters.Filter, dtu1, dtu2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "VehRef011";
			Asserter.AssertMatches("Must return the DTU with Job ID VehRef011", filters.Filter, dtu3);

			filter.Property = "";
			Asserter.AssertMatches("Must return all DTUs", filters.Filter, dtu1, dtu2, dtu3);
		}

		#endregion

		#region TestLoadComplete

		public void TestLoadComplete()
		{
			var now = ZDateTimeOffset.Now;
			var nowZDateTime = now.ToZDateTime();

			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var dock = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK, "VehRef001");
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU002", warehouse.PK, "VehRef002");
			var dtu3 = Helper.CreateDispatchTransportationUnit("DTU011", warehouse.PK, "VehRef011");
			Helper.FinishLoading(dtu1, now.AddDays(-6), now.AddDays(-4), now.AddDays(-2));
			Helper.FinishLoading(dtu2, now.AddDays(-4), now.AddDays(-2), now);

			var filters = GetNewFilterStripBusinessObject();
			Factory.Save();
			Asserter.AddToScope(dtu1, dtu2, dtu3);

			var filter = (ModuleDateTimeOffsetFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.LoadComplete];
			filter.IsActive = true;
			AssertEquals(FilterCategories.Dates, filter.Category);

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			Asserter.AssertMatches("Empty", filter, dtu1, dtu2, dtu3);

			filter.Property1 = nowZDateTime.AddDays(-5);
			filter.Property2 = nowZDateTime.AddDays(-3);
			Asserter.AssertMatches("Should only show first dtu", filter, dtu1);

			filter.Property1 = nowZDateTime.AddDays(-3);
			filter.Property2 = nowZDateTime.AddDays(-1);
			Asserter.AssertMatches("Should only show 2nd dtu", filter, dtu2);

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			Asserter.AssertMatches("Has Date", filter, dtu1, dtu2);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			Asserter.AssertMatches("Has No Date", filter, dtu3);
		}

		#endregion

		#region TestTransportCompanyName

		public void TestTransportCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var transportCompany1 = Helper.CreateClient("TC1");
			var transportCompany2 = Helper.CreateClient("TC2");

			var dtuWithTransportCompany1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dtuWithTransportCompany2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var dcnWithTransportCompany1CompanyOverride = Helper.CreateDispatchTransportationUnit("DTU3", warehouse.PK);
			var dcnWithNoTransportCompany = Helper.CreateDispatchTransportationUnit("DTU4", warehouse.PK);
			var dcnWithEmptyTransportCompany = Helper.CreateDispatchTransportationUnit("DTU5", warehouse.PK);
			Helper.CreateJobDocAddressFromAddress(dtuWithTransportCompany1, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(dtuWithTransportCompany2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany2.MainAddress);
			Helper.CreateJobDocAddressWithOverride(dcnWithTransportCompany1CompanyOverride, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, "TC1");
			Helper.CreateJobDocAddressWithOverride(dcnWithEmptyTransportCompany, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dtuWithTransportCompany1, dtuWithTransportCompany2, dcnWithTransportCompany1CompanyOverride, dcnWithNoTransportCompany, dcnWithEmptyTransportCompany);
			var crbFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.TransportCompanyName];
			crbFilter.IsActive = true;
			crbFilter.Property = transportCompany1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, crbFilter.Category);
			Asserter.AssertMatches("Must only return DTUs for TC1", filters.Filter, dtuWithTransportCompany1, dcnWithTransportCompany1CompanyOverride);

			crbFilter.Property = transportCompany2.OH_FullName;
			Asserter.AssertMatches("Must only return DTUs for TC2", filters.Filter, dtuWithTransportCompany2);

			crbFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all DTUs", filters.Filter, dtuWithTransportCompany1, dtuWithTransportCompany2, dcnWithTransportCompany1CompanyOverride, dcnWithNoTransportCompany, dcnWithEmptyTransportCompany);

			crbFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("Must return DTUs with no company", filters.Filter, dcnWithNoTransportCompany, dcnWithEmptyTransportCompany);
		}

		#endregion

		#region TestBillingPartyCompanyName

		public void TestBillingPartyCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var billing1 = Helper.CreateClient("CRB1");
			var billing2 = Helper.CreateClient("CRB2");

			var dtuWithBilling1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dtuWithBilling2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var dcnWithBilling1CompanyOverride = Helper.CreateDispatchTransportationUnit("DTU3", warehouse.PK);
			var dcnWithNoBilling = Helper.CreateDispatchTransportationUnit("DTU4", warehouse.PK);
			var dcnWithEmptyBilling = Helper.CreateDispatchTransportationUnit("DTU5", warehouse.PK);
			Helper.CreateJobDocAddressFromAddress(dtuWithBilling1, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(dtuWithBilling2, DocAddressTypes.Codes.ClientRequestedBillingParty, billing2.MainAddress);
			Helper.CreateJobDocAddressWithOverride(dcnWithBilling1CompanyOverride, DocAddressTypes.Codes.ClientRequestedBillingParty, "CRB1");
			Helper.CreateJobDocAddressWithOverride(dcnWithEmptyBilling, DocAddressTypes.Codes.ClientRequestedBillingParty);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dtuWithBilling1, dtuWithBilling2, dcnWithBilling1CompanyOverride, dcnWithNoBilling, dcnWithEmptyBilling);
			var crbFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.BillingPartyCompanyName];
			crbFilter.IsActive = true;
			crbFilter.Property = billing1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, crbFilter.Category);
			Asserter.AssertMatches("Must only return DTUs for CRB1", filters.Filter, dtuWithBilling1, dcnWithBilling1CompanyOverride);

			crbFilter.Property = billing2.OH_FullName;
			Asserter.AssertMatches("Must only return DTUs for CRB2", filters.Filter, dtuWithBilling2);

			crbFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all DTUs", filters.Filter, dtuWithBilling1, dtuWithBilling2, dcnWithBilling1CompanyOverride, dcnWithNoBilling, dcnWithEmptyBilling);

			crbFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("Must return DTUs with no company", filters.Filter, dcnWithNoBilling, dcnWithEmptyBilling);
		}

		#endregion

		#region Billing

		public void TestJobInvoicingStatusFilter()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			Asserter.AddToScope(dtu1);

			var filters = GetNewFilterStripBusinessObject();
			AssertNotNull(filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.InvoiceStatus]);
			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.InvoiceStatus];

			var job = new JobHeader.Loader(dtu1).TryLoadOrCreate();
			AssertEquals("Precondition", JobHeaderStatus.Working.Code, job.JH_Status);

			Factory.Save();

			filter.Property = JobHeaderStatus.Working.Code;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.IsActive = true;

			Asserter.AssertMatches("Should find the DTU with the given status", filter, dtu1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;

			Asserter.AssertMatches("Should exclude the DTU with the given status", filter);
		}

		public void TestInvoicedChargesFilter()
		{
			var currentBranch = GlbBranch.CurrentBranch;
			var code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR"));

			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch("WH1");
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var job = new JobHeader.Loader(dtu).TryLoadOrCreate();
			job.JH_GB = currentBranch.PK;

			var filters = GetNewFilterStripBusinessObject();
			AssertNotNull(filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.InvoicedChargesBilling]);
			var invoicedChargesFilter = (ModuleFlagsFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.InvoicedChargesBilling];
			Asserter.AddToScope(dtu);

			Factory.Save();

			invoicedChargesFilter.IsActive = true;
			invoicedChargesFilter["No Charges"] = true;
			Asserter.AssertMatches("Must return DTUs with no charges", invoicedChargesFilter, dtu);

			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = code.PK;
			charge.JR_GB = currentBranch.PK;
			charge.JR_LocalSellAmt = 10m;
			Factory.Save();

			Asserter.AssertMatches("All DTUs have charges and should not be returned", invoicedChargesFilter);
		}

		class WhsItemDispatchTransportationUnitFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<WhsItemDispatchTransportationUnit>
		{
			#region Implementation

			protected override WhsItemDispatchTransportationUnit GetNewBusinessObjectForFilterCollection()
			{
				var poke = Warehouse;
				var dtu = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
				dtu.WDH_WW_Warehouse = Warehouse.PK;
				return dtu;
			}

			protected override ModuleIdentifier FilterStripModuleID
			{
				get { return ModuleIDs.WhsItemDispatchTransportationUnit; }
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

			var dtuWith20GP = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK, containerType: "20GP");
			var dtuWith40GP = Helper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, containerType: "40GP");
			var dtuWithoutContainer = Helper.CreateDispatchTransportationUnit("DTU3", warehouse.PK);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dtuWith20GP, dtuWith40GP, dtuWithoutContainer);
			var containerTypeFilter = (ModuleGuidFilter)filters.ModuleFilters[WhsTransitFilterBusinessObject.Schema.ContainerType];
			containerTypeFilter.IsActive = true;
			containerTypeFilter.Property = packingHelper.LoadRefContainer("20GP").PK;

			AssertEquals(FilterCategories.ModesAndTypes, containerTypeFilter.Category);
			Asserter.AssertMatches("Must only return DTU with 20GP", filters.Filter, dtuWith20GP);

			containerTypeFilter.Property = ZGuid.Empty;
			Asserter.AssertMatches("Must return all DTUs", filters.Filter, dtuWith20GP, dtuWith40GP, dtuWithoutContainer);

			containerTypeFilter.Property = packingHelper.LoadRefContainer("20FR").PK;
			Asserter.AssertMatches("Must not return any DTUs", filters.Filter);
		}

		#endregion

		public void TestProfitLossReasonFilterWithOperators()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var job1 = new JobHeader.Loader(dtu1).TryLoadOrCreate();
			job1.JH_ProfitLossReasonCode = "ND1";

			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var job2 = new JobHeader.Loader(dtu2).TryLoadOrCreate();
			job2.JH_ProfitLossReasonCode = "CD1";

			var dtu3 = Helper.CreateDispatchTransportationUnit("DTU3", warehouse.PK);
			var job3 = new JobHeader.Loader(dtu3).TryLoadOrCreate();
			job3.JH_ProfitLossReasonCode = string.Empty;
			Factory.Save();

			Asserter.AddToScope(dtu1, dtu2, dtu3);

			var filters = GetNewFilterStripBusinessObject();
			var profitLossReasonFilter = (ModuleTextFilter)filters["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for dtu1.", profitLossReasonFilter, dtu1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for dtu1.", profitLossReasonFilter, dtu1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			Asserter.AssertMatches("Has a Match for dtu1 and dtu2", profitLossReasonFilter, dtu1, dtu2);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for dtu2 and dtu3", profitLossReasonFilter, dtu2, dtu3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for dtu2 and dtu3", profitLossReasonFilter, dtu2, dtu3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for dtu2 and dtu3", profitLossReasonFilter, dtu2, dtu3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "";

			Asserter.AssertMatches("Has a Match for dtu3", profitLossReasonFilter, dtu3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for dtu1 and dtu2", profitLossReasonFilter, dtu1, dtu2);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WhsItemDispatchTransportationUnitFilterBusinessObject();
		}

		protected FilterStripAsserter<WhsItemDispatchTransportationUnit> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<WhsItemDispatchTransportationUnit>(Factory, dtu => dtu.WDH_ReferenceNumber)); }
		}
		FilterStripAsserter<WhsItemDispatchTransportationUnit> asserter;

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
