using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class ShipmentReceivalFilterStripTest : BaseFreightTest
	{
		#region TestBaseFilter

		public void TestBaseFilter()
		{
			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_IsCFSRegistered = true;

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_IsCFSRegistered = false;

			Factory.Save();

			CommonShipment[] results = Factory.Load<CommonShipment>(Strip.Filter);
			AssertCollectionContains(shipment1, results);
			AssertCollectionNotContains(shipment2, results);
		}

		#endregion

		#region TestActiveStatus

		public void TestActiveStatus()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_IsCancelled = false;

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_IsCancelled = true;

			Factory.Save();

			CFSShipment[] results;
			ModuleTextFilter filter = (ModuleTextFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.ActiveStatus];

			filter.Property = ShipmentReceivalFilterStrip.ActiveFilter.Active;
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionNotContains(shipment2, results);

			filter.Property = ShipmentReceivalFilterStrip.ActiveFilter.All;
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionContains(shipment2, results);

			filter.Property = ShipmentReceivalFilterStrip.ActiveFilter.Inactive;
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionNotContains(shipment1, results);
			AssertCollectionContains(shipment2, results);
		}

		#endregion

		#region Billing Filters

		public void TestJobInvoicingStatusFilter()
		{
			var filterBO = new ShipmentReceivalFilterStrip();
			AssertNotNull(filterBO["Job Status"]);
			var jobstatusFilter = (ModuleTextFilter)filterBO["Job Status"];

			var shipment1 = Factory.NewWithValidTestData<CFSShipment>();
			var job = new JobHeader.Loader(shipment1).TryLoadOrCreate();
			AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);

			Factory.Save();

			jobstatusFilter.Property = JobHeaderStatus.Working.Code;
			jobstatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			jobstatusFilter.IsActive = true;

			var shipments = Factory.Load<CFSShipment>(filterBO.Filter);
			AssertCollectionContains(shipment1, shipments);

			jobstatusFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			shipments = Factory.Load<CFSShipment>(filterBO.Filter);

			AssertCollectionNotContains(shipment1, shipments);
		}

		public void TestAPInvoiceNumberFilter()
		{
			ShipmentReceivalFilterStrip filterBO = new ShipmentReceivalFilterStrip();
			AssertNotNull(filterBO["AP Invoice #"]);

			CFSShipment shipment1 = Factory.New<CFSShipment>();
			CFSShipment shipment2 = Factory.New<CFSShipment>();

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment1.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.Parent = shipment1;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001001";

			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001001";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)filterBO["AP Invoice #"];
			filter.Property = "00001001";
			filter.IsActive = true;

			CFSShipment[] shipments = Factory.Load<CFSShipment>(filter.Query);

			AssertEquals("Should have 1 CommonCartage", 1, shipments.Length);
			AssertCollectionContains("Shipment1 is in Collection", shipment1, shipments);
			AssertCollectionNotContains("Shipment2 is not in Collection", shipment2, shipments);

			filter.Property = "00001002";
			filter.IsActive = true;

			shipments = Factory.Load<CFSShipment>(filter.Query);

			AssertEquals("Should have 0 CommonCartage", 0, shipments.Length);
			AssertCollectionNotContains("Shipment1 is in Collection", shipment1, shipments);
			AssertCollectionNotContains("Shipment2 is not in Collection", shipment2, shipments);

			filter.Property = "";
			filter.IsActive = true;

			shipments = Factory.Load<CFSShipment>(filter.Query);

			AssertEquals("Should have 2 CommonCartage", 2, shipments.Length);
			AssertCollectionContains("Shipment1 is in Collection", shipment1, shipments);
			AssertCollectionContains("Shipment2 is not in Collection", shipment2, shipments);
		}

		public void TestJobHoldStatusFilter()
		{
			var shipment1 = Factory.NewWithValidTestData<CFSShipment>();
			var shipment2 = Factory.NewWithValidTestData<CFSShipment>();
			var shipment3 = Factory.NewWithValidTestData<CFSShipment>();
			var shipment4 = Factory.NewWithValidTestData<CFSShipment>();

			var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreate();
			var job2 = new JobHeader.Loader(shipment2).TryLoadOrCreate();
			var job3 = new JobHeader.Loader(shipment3).TryLoadOrCreate();
			var job4 = new JobHeader.Loader(shipment4).TryLoadOrCreate();

			job1.JH_HoldReason = "Reason ABC";
			job2.JH_HoldReason = "Reason DEF";
			job4.JH_HoldReason = "";
			Factory.Save();

			var demoCompany = GlbCompany.GetDemoCompany(Factory);
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), demoCompany.ActiveBranches.First().PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				job3.JH_HoldReason = "Reason GHI";
				job3.JH_GC = demoCompany.PK; // This Job3 belongs to another Company and should not appear in any of the results.
				Factory.Save();
			}

			var filter = (ModuleTextFilter)Strip["Job Status Hold Reason"];

			filter.Property = "Reason";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;

			var filteredCollection = Factory.Load<CFSShipment>(Strip.Filter);

			Assert("Expecting collection to contain Shipment1", filteredCollection.Contains(shipment1));
			Assert("Expecting collection to contain Shipment2", filteredCollection.Contains(shipment2));
			Assert("Expecting collection not to contain Shipment3", !filteredCollection.Contains(shipment3));
			Assert("Expecting collection not to contain Shipment4", !filteredCollection.Contains(shipment4));

			filter.Property = "Reason DEF";
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filteredCollection = Factory.Load<CFSShipment>(Strip.Filter);

			Assert("Expecting collection not to contain Shipment1", !filteredCollection.Contains(shipment1));
			Assert("Expecting collection to contain Shipment2", filteredCollection.Contains(shipment2));
			Assert("Expecting collection not to contain Shipment3", !filteredCollection.Contains(shipment3));
			Assert("Expecting collection not to contain Shipment4", !filteredCollection.Contains(shipment4));

			filter.Property = "DEF";
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.IsActive = true;
			filteredCollection = Factory.Load<CFSShipment>(Strip.Filter);

			Assert("Expecting collection to contain Shipment1", filteredCollection.Contains(shipment1));
			Assert("Expecting collection not to contain Shipment2", !filteredCollection.Contains(shipment2));
			Assert("Expecting collection not to contain Shipment3", !filteredCollection.Contains(shipment3));
			Assert("Expecting collection to contain Shipment4", filteredCollection.Contains(shipment4));

			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filter.IsActive = true;
			filteredCollection = Factory.Load<CFSShipment>(Strip.Filter);

			Assert("Expecting collection not to contain Shipment1", !filteredCollection.Contains(shipment1));
			Assert("Expecting collection not to contain Shipment2", !filteredCollection.Contains(shipment2));
			Assert("Expecting collection not to contain Shipment3", !filteredCollection.Contains(shipment3));
			Assert("Expecting collection to contain Shipment4", filteredCollection.Contains(shipment4));

			filter.Property = "";
			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			filteredCollection = Factory.Load<CFSShipment>(Strip.Filter);

			Assert("Expecting collection to contain Shipment1", filteredCollection.Contains(shipment1));
			Assert("Expecting collection to contain Shipment2", filteredCollection.Contains(shipment2));
			Assert("Expecting collection not to contain Shipment3", !filteredCollection.Contains(shipment3));
			Assert("Expecting collection not to contain Shipment4", !filteredCollection.Contains(shipment4));
		}

		public void TestARTransactionFilter()
		{
			ShipmentReceivalFilterStrip filterBO = new ShipmentReceivalFilterStrip();
			AssertNotNull(filterBO["AR Transaction #"]);

			CFSShipment shipment1 = Factory.New<CFSShipment>();
			CFSShipment shipment2 = Factory.New<CFSShipment>();

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment1.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.Parent = shipment1;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001005";

			AccTransactionHeader newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001005";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

			AccTransactionLines newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			ModuleFountainFilter filter = (ModuleFountainFilter)filterBO["AR Transaction #"];
			filter.Property = "00001005";
			filter.IsActive = true;

			CFSShipment[] shipments = Factory.Load<CFSShipment>(filter.Query);

			AssertEquals("Should have 1 CommonCartage", 1, shipments.Length);
			AssertCollectionContains("Shipment1 is in Collection", shipment1, shipments);
			AssertCollectionNotContains("Shipment2 is not in Collection", shipment2, shipments);

			filter.Property = "00001001";
			filter.IsActive = true;

			shipments = Factory.Load<CFSShipment>(filter.Query);

			AssertEquals("Should have 0 CommonCartage", 0, shipments.Length);
			AssertCollectionNotContains("Shipment1 is in Collection", shipment1, shipments);
			AssertCollectionNotContains("Shipment2 is not in Collection", shipment2, shipments);

			filter.Property = "";
			filter.IsActive = true;

			shipments = Factory.Load<CFSShipment>(filter.Query);

			AssertEquals("Should have 2 CommonCartage", 2, shipments.Length);
			AssertCollectionContains("Shipment1 is in Collection", shipment1, shipments);
			AssertCollectionContains("Shipment2 is not in Collection", shipment2, shipments);
		}

		#endregion

		#region TestTransportMode

		public void TestTransportMode()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_TransportMode = Core.Constants.TransportModes.Air;

			Factory.Save();

			CFSShipment[] results;
			ModuleTextFilter filter = (ModuleTextFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.TransportMode];

			filter.Property = "";
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionContains(shipment2, results);

			filter.Property = Core.Constants.TransportModes.Sea;
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionNotContains(shipment2, results);
		}

		#endregion

		#region TestWharehouseLocationFilter_TrueLocation

		public void TestWarehouseLocationFilter_TrueLocation()
		{
			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var header = Factory.NewWithValidTestData<OrgHeader>();

			var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var warehouse1 = (IWhsWarehouse)helper.CreateWarehouse("WS1", header.MainAddress, GlbBranch.CurrentBranch);
			var warehouse2 = (IWhsWarehouse)helper.CreateWarehouse("WS2", header.MainAddress, GlbCompany.CurrentCompany.Branches.First(g => g.PK != GlbBranch.CurrentBranch.PK));
			var area1 = helper.CreateWhsArea(warehouse1.PK, "AREA");
			var area2 = helper.CreateWhsArea(warehouse2.PK, "AREA");
			var row1 = helper.CreateRowAndGenerateLocations(warehouse1, "BOB");
			var row2 = helper.CreateRowAndGenerateLocations(warehouse1, "FRANK");

			Factory.Save();

			var location1 = (IWhsLocation)row1.Locations[0];
			location1.WLV_WA_PickingArea = area1.PK;

			var location2 = (IWhsLocation)row2.Locations[0];
			location2.WLV_WA_PickingArea = area2.PK;

			var shipment1 = Factory.New<CFSShipment>();
			var packline1 = shipment1.OuterPackLines.AddNew();
			packline1.PackLocations.AddNew().JQ_WL = location1.PK;

			var shipment2 = Factory.New<CFSShipment>();
			var packline2 = shipment1.OuterPackLines.AddNew();
			packline1.PackLocations.AddNew().JQ_WL = location2.PK;

			var shipment3 = Factory.New<CFSShipment>();
			shipment3.JS_WL = location1.PK;

			Factory.Save();

			CFSShipment[] results;
			var filter = (ModuleWarehouseLocationFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.WarehouseLocation];

			filter.Warehouse = ZGuid.Empty;
			filter.Location = "";
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionContains(shipment2, results);
			AssertCollectionContains(shipment3, results);

			filter.Warehouse = warehouse1.PK;
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionNotContains(shipment2, results);
			AssertCollectionContains(shipment3, results);
		}

		#endregion

		#region TestWarehouseLocationFilter_NotTrueLocation

		public void TestWarehouseLocationFilter_NotTrueLocation()
		{
			WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			CFSShipment shipment1 = Factory.New<CFSShipment>();
			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLocation location1 = packline1.PackLocations.AddNew();
			location1.JQ_WarehouseLocation = "WHS1";

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			PackLine packline2 = shipment1.OuterPackLines.AddNew();
			PackLocation location2 = packline1.PackLocations.AddNew();
			location2.JQ_WarehouseLocation = "WHS2";

			CFSShipment shipment3 = Factory.New<CFSShipment>();
			shipment3.JS_WarehouseLocation = "WHS1";

			Factory.Save();

			CFSShipment[] results;
			ModuleTextFilter filter = (ModuleTextFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.WarehouseLocation];

			filter.Property = "";
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionContains(shipment2, results);
			AssertCollectionContains(shipment3, results);

			filter.Property = "WHS1";
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionNotContains(shipment2, results);
			AssertCollectionContains(shipment3, results);
		}

		#endregion

		#region TestWarehouseLocationFilter_NotFilters

		public void TestWarehouseLocationFilter_NotFilters()
		{
			Func<ZString, ZString, CFSShipment> populateShipment = (shipmentLocation, packlineLocation) =>
			 {
				 var shipment = Factory.New<CFSShipment>();

				 var packline = shipment.OuterPackLines.AddNew();
				 var location1 = packline.PackLocations.AddNew();

				 shipment.JS_WarehouseLocation = shipmentLocation;
				 location1.JQ_WarehouseLocation = packlineLocation;

				 return shipment;
			 };

			var shipment1 = populateShipment("", "");
			var shipment2 = populateShipment("ABC", "");
			var shipment3 = populateShipment("", "ABC");
			var shipment4 = populateShipment("", "ABCDEF");
			var shipment5 = populateShipment("ABC", "ABC");
			var shipment6 = populateShipment("ABC", "DEF");
			var shipment7 = populateShipment("DEF", "");
			var shipment8 = populateShipment("", "DEF");
			var shipment9 = populateShipment("DEF", "DEF");
			var shipment10 = populateShipment("DEF", "ABCDEF");

			Factory.Save();

			Func<SQLComparisonOperator, ZString, CFSShipment[]> findMatchingShipment_WarehouseFilter = (comparisonOperator, filterValue) =>
			{
				ModuleTextFilter filter = (ModuleTextFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.WarehouseLocation];
				filter.IsActive = true;
				filter.SqlComparisonOperator = comparisonOperator;
				filter.Property = filterValue;

				return Factory.Load<CFSShipment>(filter.Query);
			};

			var shipmentsExpected = new CFSShipment[] { shipment1, shipment4, shipment7, shipment8, shipment9, shipment10 };
			var shipmentsActual = findMatchingShipment_WarehouseFilter(SQLComparisonOperator.NotEqual, "ABC");
			AssertContainsExactElementsInAnyOrder(shipmentsExpected, shipmentsActual);

			shipmentsExpected = new CFSShipment[] { shipment1, shipment7, shipment8, shipment9 };
			shipmentsActual = findMatchingShipment_WarehouseFilter(SQLComparisonOperator.DoesNotStartWith, "ABC");
			AssertContainsExactElementsInAnyOrder(shipmentsExpected, shipmentsActual);

			shipmentsExpected = new CFSShipment[] { shipment1, shipment7, shipment8, shipment9 };
			shipmentsActual = findMatchingShipment_WarehouseFilter(SQLComparisonOperator.NotContains, "ABC");
			AssertContainsExactElementsInAnyOrder(shipmentsExpected, shipmentsActual);
		}

		#endregion

		#region TestShipmentNumber

		public void TestShipmentNumber()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_UniqueConsignRef = "aoeu";

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_UniqueConsignRef = "snth";

			Factory.Save();

			ModuleNumberFilter filter = (ModuleNumberFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.ShipmentNumber];
			filter.Property = "aoeu";

			CFSShipment[] results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionNotContains(shipment2, results);
		}

		#endregion

		#region TestChargesNotPostedFilter

		public void TestChargesNotPostedFilterWhenUseNOTINAndExistNull()
		{
			var header1 = Factory.NewWithValidTestData<AccTransactionHeader>();
			header1.AH_Ledger = LedgerTypes.AccountsPayable;
			header1.AH_TransactionType = TransactionTypes.Invoice;

			var line1 = Factory.NewWithValidTestData<AccTransactionLines>();
			line1.AL_LineType = "CST";
			line1.AL_ReverseDate = ZDateTime.Empty;
			line1.AL_AH = header1.PK;
			line1.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var shipment1 = Factory.New<CFSShipment>();
			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_ParentID = shipment1.PK;
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job1.JH_GC = GlbCompany.CurrentCompany.PK;

			var header2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			header2.AH_Ledger = LedgerTypes.AccountsPayable;
			header2.AH_TransactionType = TransactionTypes.Invoice;

			var line2 = Factory.NewWithValidTestData<AccTransactionLines>();
			line2.AL_JH = job1.PK;
			line2.AL_LineType = "ACR";
			line2.AL_ReverseDate = ZDateTime.Empty;
			line2.AL_AH = header2.PK;
			line2.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job1.PK;
			charge.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			charge.JR_AL_APLine = line2.PK;

			Factory.Save();

			CFSShipment[] results;
			var filter = (ModuleFlagsFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.Flags];

			filter[ShipmentReceivalFilterStrip.Descriptions.ChargesNotPosted] = true;
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
		}

		public void TestChargesNotPostedFilter()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			JobHeader job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_ParentID = shipment1.PK;
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job1.JH_GC = GlbCompany.CurrentCompany.PK;

			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsPayable;
			header.AH_TransactionType = TransactionTypes.Invoice;

			AccTransactionLines line1 = Factory.NewWithValidTestData<AccTransactionLines>();
			line1.AL_JH = job1.PK;
			line1.AL_LineType = "ACR";
			line1.AL_ReverseDate = ZDateTime.Empty;
			line1.AL_AH = header.PK;
			line1.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job1.PK;
			charge.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			charge.JR_AL_APLine = line1.PK;

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			JobHeader job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_ParentID = shipment2.PK;
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			AccTransactionLines line2 = Factory.NewWithValidTestData<AccTransactionLines>();
			line2.AL_JH = job2.PK;
			line2.AL_LineType = "CST";
			line2.AL_AH = header.PK;
			line2.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			JobCharge charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_JH = job2.PK;
			charge2.JR_AC = Factory.NewWithValidTestData<AccChargeCode>().PK;
			charge2.JR_AL_APLine = line2.PK;

			CFSShipment shipment3 = Factory.New<CFSShipment>();
			Factory.Save();

			ZQuery companyFilter = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			GlbCompany notCurrentCompany = Factory.LoadTop1<GlbCompany>(companyFilter);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, notCurrentCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var job3 = new JobHeader.Loader(shipment3).TryCreateWithMutex();
				Factory.Save();
			}

			CFSShipment[] results;
			ModuleFlagsFilter filter = (ModuleFlagsFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.Flags];

			filter[ShipmentReceivalFilterStrip.Descriptions.ChargesNotPosted] = false;
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionContains(shipment2, results);
			AssertCollectionContains(shipment3, results);

			filter[ShipmentReceivalFilterStrip.Descriptions.ChargesNotPosted] = true;
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, results);
			AssertCollectionNotContains(shipment2, results);
			AssertCollectionNotContains(shipment3, results);
		}

		#endregion

		#region TestServiceLevel

		public void TestServiceLevel()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_RS_NKServiceLevel = "XXX";

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_RS_NKServiceLevel = "YYY";

			Factory.Save();

			CFSShipment[] result;
			ModuleNkFilter filter = (ModuleNkFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.ServiceLevel];

			filter.Property = "";
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);

			filter.Property = "XXX";
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionNotContains(shipment2, result);
		}

		#endregion

		#region TestOriginFilter

		public void TestOriginFilter()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_RL_NKOrigin = HomePort;

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_RL_NKOrigin = AlternateHomePort;

			CFSShipment shipment3 = Factory.New<CFSShipment>();
			shipment3.JS_RL_NKOrigin = OverseasPort;

			Factory.Save();

			CFSShipment[] result;
			ModuleLocationFilter filter = (ModuleLocationFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.OriginDestination];

			filter.Property1 = "";
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionContains(shipment3, result);

			filter.Property1 = HomePort.Substring(0, 2);
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);

			filter.Property1 = HomePort;
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionNotContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);
		}

		#endregion

		#region TestDestinationFilter

		public void TestDestinationFilter()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_RL_NKDestination = HomePort;

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			shipment2.JS_RL_NKDestination = AlternateHomePort;

			CFSShipment shipment3 = Factory.New<CFSShipment>();
			shipment3.JS_RL_NKDestination = OverseasPort;

			Factory.Save();

			CFSShipment[] result;
			ModuleLocationFilter filter = (ModuleLocationFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.OriginDestination];

			filter.Property2 = "";
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionContains(shipment3, result);

			filter.Property2 = HomePort.Substring(0, 2);
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);

			filter.Property2 = HomePort;
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionNotContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);
		}

		#endregion

		#region TestLoadFilter

		public void TestLoadFilter()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			CFSLoadListConsol consol1 = shipment1.Consols.AddNew();
			consol1.Transports[0].JW_RL_NKLoadPort = HomePort;

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			CFSLoadListConsol consol2 = shipment2.Consols.AddNew();
			consol2.Transports[0].JW_RL_NKLoadPort = AlternateHomePort;

			CFSShipment shipment3 = Factory.New<CFSShipment>();
			CFSLoadListConsol consol3 = shipment3.Consols.AddNew();
			consol3.Transports[0].JW_RL_NKLoadPort = OverseasPort;

			Factory.Save();

			CFSShipment[] result;
			ModuleLocationFilter filter = (ModuleLocationFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.LoadDischarge];

			filter.Property1 = "";
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionContains(shipment3, result);

			filter.Property1 = HomePort.Substring(0, 2);
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);

			filter.Property1 = HomePort;
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionNotContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);
		}

		#endregion

		#region TestDischargeFilter

		public void TestDischargeFilter()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			CFSLoadListConsol consol1 = shipment1.Consols.AddNew();
			consol1.Transports[0].JW_RL_NKDiscPort = HomePort;

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			CFSLoadListConsol consol2 = shipment2.Consols.AddNew();
			consol2.Transports[0].JW_RL_NKDiscPort = AlternateHomePort;

			CFSShipment shipment3 = Factory.New<CFSShipment>();
			CFSLoadListConsol consol3 = shipment3.Consols.AddNew();
			consol3.Transports[0].JW_RL_NKDiscPort = OverseasPort;

			Factory.Save();

			CFSShipment[] result;
			ModuleLocationFilter filter = (ModuleLocationFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.LoadDischarge];

			filter.Property2 = "";
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionContains(shipment3, result);

			filter.Property2 = HomePort.Substring(0, 2);
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);

			filter.Property2 = HomePort;
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionNotContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);
		}

		#endregion

		#region Test LoadList # Filter

		public void TestLoadListFilter()
		{
			var shipment1 = Factory.New<CFSShipment>();
			var consol1 = shipment1.Consols.AddNew();

			var shipment2 = Factory.New<CFSShipment>();
			var consol2 = shipment2.Consols.AddNew();

			var shipment3 = Factory.New<CFSShipment>();
			var consol3 = shipment3.Consols.AddNew();

			Factory.Save();

			CFSShipment[] result;
			var filter = (ModuleNumberFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.LoadListNumber];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;

			filter.Property = consol1.JK_UniqueConsignRef;
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionNotContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);

			filter.Property = consol2.JK_UniqueConsignRef;
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionNotContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);

			filter.Property = consol3.JK_UniqueConsignRef;
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionNotContains(shipment1, result);
			AssertCollectionNotContains(shipment2, result);
			AssertCollectionContains(shipment3, result);
		}

		public void TestLoadListFilter_IsBlank()
		{
			var shipment1 = Factory.New<CFSShipment>();
			var consolBlankUniqueRef = shipment1.Consols.AddNew();
			consolBlankUniqueRef.JK_UniqueConsignRef = "L00000001";
			consolBlankUniqueRef.JK_IsCFS = true;

			var shipment2 = Factory.New<CFSShipment>();
			var consolShipment2 = shipment2.Consols.AddNew();
			consolShipment2.JK_UniqueConsignRef = "C00000001";
			consolShipment2.JK_IsCFS = false;

			var shipmentWithoutConsol = Factory.New<CFSShipment>();

			Factory.Save();

			var filter = (ModuleNumberFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.LoadListNumber];

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			var result = Factory.Load<CFSShipment>(filter.Query);

			AssertCollectionNotContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionContains(shipmentWithoutConsol, result);
		}

		public void TestLoadListFilter_IsNotBlank()
		{
			var shipment1 = Factory.New<CFSShipment>();
			var consolShipment1 = shipment1.Consols.AddNew();
			consolShipment1.JK_UniqueConsignRef = "L00000001";
			consolShipment1.JK_IsCFS = true;

			var shipment2 = Factory.New<CFSShipment>();
			var consolShipment2 = shipment2.Consols.AddNew();
			consolShipment2.JK_UniqueConsignRef = "C00000001";
			consolShipment2.JK_IsCFS = false;

			var shipmentWithoutConsol = Factory.New<CFSShipment>();

			Factory.Save();

			var filter = (ModuleNumberFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.LoadListNumber];

			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			var result = Factory.Load<CFSShipment>(filter.Query);

			AssertCollectionContains(shipment1, result);
			AssertCollectionNotContains(shipment2, result);
			AssertCollectionNotContains(shipmentWithoutConsol, result);
		}

		public void TestLoadListNumberNotFilters()
		{
			var shipment1 = Factory.New<CFSShipment>();
			var consolShipment1 = shipment1.Consols.AddNew();
			consolShipment1.JK_UniqueConsignRef = "L00000001";
			consolShipment1.JK_IsCFS = true;

			var shipment2 = Factory.New<CFSShipment>();
			var consolShipment2 = shipment2.Consols.AddNew();
			consolShipment2.JK_UniqueConsignRef = "C00000001";
			consolShipment2.JK_IsCFS = false;

			var shipment3 = Factory.New<CFSShipment>();
			var consolShipment3 = shipment3.Consols.AddNew();
			consolShipment3.JK_UniqueConsignRef = "D00000003";
			consolShipment3.JK_IsCFS = true;

			var shipment4 = Factory.New<CFSShipment>();
			var consolShipment4 = shipment4.Consols.AddNew();
			consolShipment4.JK_UniqueConsignRef = "D00000004";
			consolShipment4.JK_IsCFS = true;

			var consolShipment4NotCfs = shipment4.Consols.AddNew();
			consolShipment4NotCfs.JK_UniqueConsignRef = "C00000003";
			consolShipment4NotCfs.JK_IsCFS = false;

			Factory.Save();

			var filter = (ModuleNumberFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.LoadListNumber];
			filter.IsActive = true;
			filter.Property = "C000";

			void assertComparisonOperator(string comparisonOperator, string filterString)
			{
				filter.ComparisonOperator = comparisonOperator;
				filter.Property = filterString;
				var collection = Factory.Load<CFSShipment>(filter.Query);

				AssertCollectionNotContains(shipment1, collection);
				AssertCollectionNotContains(shipment2, collection);
				AssertCollectionContains(shipment3, collection);
				AssertCollectionContains(shipment4, collection);
			}

			assertComparisonOperator(ModuleTextFilter.ComparisonConstants.NotEqual, "L00000001");
			assertComparisonOperator(ModuleTextFilter.ComparisonConstants.NotContain, "001");
			assertComparisonOperator(ModuleTextFilter.ComparisonConstants.NotStartsWith, "L0");
		}

		#endregion

		#region TestFirstOriginFilter

		public void TestFirstOriginFilter()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			CFSLoadListConsol consol1 = shipment1.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = HomePort;
			consol1.Transports.RemoveAndDeleteAll();

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			CFSLoadListConsol consol2 = shipment2.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = AlternateHomePort;
			consol2.Transports.RemoveAndDeleteAll();

			CFSShipment shipment3 = Factory.New<CFSShipment>();
			CFSLoadListConsol consol3 = shipment3.Consols.AddNew();
			consol3.JK_RL_NKLoadPort = OverseasPort;
			consol3.Transports.RemoveAndDeleteAll();

			Factory.Save();

			CFSShipment[] result;
			ModuleLocationFilter filter = (ModuleLocationFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.LoadListEndPorts];

			filter.Property1 = "";
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionContains(shipment3, result);

			filter.Property1 = HomePort.Substring(0, 2);
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);

			filter.Property1 = HomePort;
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionNotContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);
		}

		#endregion

		#region TestFinalDischargeFilter

		public void TestFinalDischargeFilter()
		{
			CFSShipment shipment1 = Factory.New<CFSShipment>();
			CFSLoadListConsol consol1 = shipment1.Consols.AddNew();
			consol1.JK_RL_NKDischargePort = HomePort;
			consol1.Transports.RemoveAndDeleteAll();

			CFSShipment shipment2 = Factory.New<CFSShipment>();
			CFSLoadListConsol consol2 = shipment2.Consols.AddNew();
			consol2.JK_RL_NKDischargePort = AlternateHomePort;
			consol2.Transports.RemoveAndDeleteAll();

			CFSShipment shipment3 = Factory.New<CFSShipment>();
			CFSLoadListConsol consol3 = shipment3.Consols.AddNew();
			consol3.JK_RL_NKDischargePort = OverseasPort;
			consol3.Transports.RemoveAndDeleteAll();

			Factory.Save();

			CFSShipment[] result;
			ModuleLocationFilter filter = (ModuleLocationFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.LoadListEndPorts];

			filter.Property2 = "";
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionContains(shipment3, result);

			filter.Property2 = HomePort.Substring(0, 2);
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);

			filter.Property2 = HomePort;
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment1, result);
			AssertCollectionNotContains(shipment2, result);
			AssertCollectionNotContains(shipment3, result);
		}

		#endregion

		#region TestATA

		public void TestATA()
		{
			ZDateTime now = ZDateTime.Now;

			ExportSailing.Destination.JB_A_ARV = now;
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.Consols.AddNew().Transports[0].JW_JX = ExportSailing.PK;
			Factory.Save();

			AssertDateRangeFilter(ShipmentReceivalFilterStrip.Descriptions.ATA, now, shipment);
		}

		#endregion

		#region TestATD

		public void TestATD()
		{
			ZDateTime now = ZDateTime.Now;

			ExportSailing.Origin.JA_A_DEP = now;
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.Consols.AddNew().Transports[0].JW_JX = ExportSailing.PK;
			Factory.Save();

			AssertDateRangeFilter(ShipmentReceivalFilterStrip.Descriptions.ATD, now, shipment);
		}

		#endregion

		#region TestETA

		public void TestETA()
		{
			ZDateTime now = ZDateTime.Now;

			ExportSailing.Destination.JB_E_ARV = now;
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.Consols.AddNew().Transports[0].JW_JX = ExportSailing.PK;
			Factory.Save();

			AssertDateRangeFilter(ShipmentReceivalFilterStrip.Descriptions.ETA, now, shipment);
		}

		#endregion

		#region TestETD

		public void TestETD()
		{
			ZDateTime now = ZDateTime.Now;

			ExportSailing.Origin.JA_E_DEP = now;
			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.Consols.AddNew().Transports[0].JW_JX = ExportSailing.PK;
			Factory.Save();

			AssertDateRangeFilter(ShipmentReceivalFilterStrip.Descriptions.ETD, now, shipment);
		}

		#endregion

		#region TestReceivedDate

		public void TestReceivedDate()
		{
			ZDateTime now = ZDateTime.Now;

			CFSShipment shipment = Factory.New<CFSShipment>();
			shipment.JS_A_RCV = now;
			Factory.Save();

			AssertDateRangeFilter(ShipmentReceivalFilterStrip.Descriptions.ReceivedDate, now, shipment);
		}

		#endregion

		#region TestFlightVoyageNoAndVessel

		public void TestFlightVoyageNoAndVessel()
		{
			JobVoyage voyage1 = CreateVoyage(Core.Constants.TransportModes.Sea, "UniqueVessel", "Voy Alpha");
			JobVoyage voyage2 = CreateVoyage(Core.Constants.TransportModes.Sea, "Flying Dutchman", "Voy Beta");

			JobSailing sailing1 = GetOrCreateSailing(voyage1, HomePort, OverseasPort);
			JobSailing sailing2 = GetOrCreateSailing(voyage2, HomePort, OverseasPort);

			CFSShipment shipment1 = Factory.New<CFSShipment>();
			shipment1.JS_IsBooking = true;
			shipment1.JS_TransportMode = sailing1.Voyage.JV_AirSeaRoad;
			shipment1.JS_RL_NKOrigin = sailing1.Origin.JA_RL_NKPortOfLoading;
			shipment1.JS_RL_NKDestination = sailing1.Destination.JB_RL_NKPortOfDischarge;
			shipment1.JS_JX = sailing1.PK;

			CFSLoadListConsol loadList1 = Factory.New<CFSLoadListConsol>();
			Transport transport1 = Factory.NewWithValidTestData<Transport>();
			loadList1.Transports.Add(transport1);
			loadList1.JK_TransportMode = sailing2.Voyage.JV_AirSeaRoad;
			loadList1.JK_RL_NKLoadPort = sailing2.Origin.JA_RL_NKPortOfLoading;
			loadList1.JK_RL_NKDischargePort = sailing2.Destination.JB_RL_NKPortOfDischarge;
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing2.PK;
			CFSShipment shipment2 = loadList1.Shipments.AddNew();

			CFSLoadListConsol loadList2 = Factory.New<CFSLoadListConsol>();
			loadList2.JK_TransportMode = sailing2.Voyage.JV_AirSeaRoad;
			loadList2.JK_RL_NKLoadPort = sailing2.Origin.JA_RL_NKPortOfLoading;
			loadList2.JK_RL_NKDischargePort = sailing2.Destination.JB_RL_NKPortOfDischarge;
			//But don't set sailing
			CFSShipment shipment3 = loadList2.Shipments.AddNew();

			Factory.Save();

			var filter = (VoyageVesselModuleFilter)Strip[ShipmentReceivalFilterStrip.Descriptions.VesselVoyageFlight];

			filter.Vessel = "UniqueVessel";
			filter.IsActive = true;
			CFSShipment[] result = Factory.Load<CFSShipment>(filter.Query);
			AssertEquals(1, result.Length);
			AssertEquals(shipment1.PK, result[0].PK);

			filter.Vessel = "Flying Dutchman";
			filter.IsActive = true;
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertEquals(1, result.Length);
			AssertEquals(shipment2.PK, result[0].PK);

			filter.Vessel = "Flying ";
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertEquals(1, result.Length);
			AssertEquals(shipment2.PK, result[0].PK);

			filter.Vessel = "";
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			result = Factory.Load<CFSShipment>(filter.Query);
			AssertEquals(2, result.Length);
			AssertEquals(shipment1.PK, result[0].PK);
			AssertEquals(shipment2.PK, result[1].PK);
		}

		JobVoyage CreateVoyage(ZString transportMode, ZString vessel, ZString voyageFlight)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = transportMode;
			voyage.JV_RV_NKVessel = vessel;
			voyage.JV_VoyageFlight = voyageFlight;
			return voyage;
		}

		JobSailing GetOrCreateSailing(JobVoyage voyage, ZString load, ZString discharge)
		{
			if (voyage.Origins.GetOriginFromLoading(load) == null)
			{
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = load;
			}

			if (voyage.Destinations.GetDestinationFromDischarge(discharge) == null)
			{
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = discharge;
			}

			voyage.GenerateSailings();
			return voyage.Sailings.GetSailingFromLoadAndDischarge(load, discharge);
		}

		#endregion

		#region TestReferenceNumberFilter
		public void TestReferenceNumberFilter()
		{
			var shipment1 = Factory.NewWithValidTestData<CFSShipment>();
			var shipment2 = Factory.NewWithValidTestData<CFSShipment>();
			var shipment3 = Factory.NewWithValidTestData<CFSShipment>();

			NewReferenceNumber(shipment1, "CA", "CCN", "1234");
			NewReferenceNumber(shipment2, "CA", "PCN", "3456");
			NewReferenceNumber(shipment3, "CN", "SLD", "1234");

			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			var filterBO = new ShipmentReceivalFilterStrip();
			ReferenceNumberFilter filter = (ReferenceNumberFilter)filterBO[ConstantsAndReusables.NumberFilterTypes.AdditionalReferenceNumbers];

			AssertNull("The filter should be invisible when current company is not a CA company", filter);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Canada);
			filterBO = new ShipmentReceivalFilterStrip();
			filter = (ReferenceNumberFilter)filterBO[ConstantsAndReusables.NumberFilterTypes.AdditionalReferenceNumbers];
			AssertNotNull("The filter should be visible when current company is a CA company", filter);

			filter.IsActive = true;

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "", "", "");
			var results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains("Expecting collection to contain Shipment1", shipment1, results);
			AssertCollectionContains("Expecting collection to contain LoadList2", shipment2, results);
			AssertCollectionContains("Expecting collection to contain LoadList3", shipment3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "", "", "12");
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains("Expecting collection to contain Shipment1", shipment1, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList2", shipment2, results);
			AssertCollectionContains("Expecting collection to contain LoadList3", shipment3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.Contains, "CA", "", "3");
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains("Expecting collection to contain Shipment1", shipment1, results);
			AssertCollectionContains("Expecting collection to contain LoadList2", shipment2, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList3", shipment3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.Contains, "", "CCN", "3");
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains("Expecting collection to contain Shipment1", shipment1, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList2", shipment2, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList3", shipment3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "CA", "PCN", "");
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionNotContains("Expecting collection not to contain Shipment1", shipment1, results);
			AssertCollectionContains("Expecting collection to contain LoadList2", shipment2, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList3", shipment3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "CA", "PCN", "34");
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionNotContains("Expecting collection not to contain Shipment1", shipment1, results);
			AssertCollectionContains("Expecting collection to contain LoadList2", shipment2, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList3", shipment3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.StartsWith, "CN", "SLD", "34");
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionNotContains("Expecting collection not to contain Shipment1", shipment1, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList2", shipment2, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList3", shipment3, results);

			SetReferenceNumberFilter(filter, SQLComparisonOperator.Contains, "CN", "SLD", "34");
			results = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionNotContains("Expecting collection not to contain Shipment1", shipment1, results);
			AssertCollectionNotContains("Expecting collection not to contain LoadList2", shipment2, results);
			AssertCollectionContains("Expecting collection to contain LoadList3", shipment3, results);
		}

		void SetReferenceNumberFilter(ReferenceNumberFilter filter, SQLComparisonOperator op, string country, string type, string property)
		{
			filter.SqlComparisonOperator = op;
			filter.Property = property;
			filter.Country = country;
			filter.Type = type;
		}

		static CusEntryNumber NewReferenceNumber(CFSShipment shipment, string countryCode, string type, string number)
		{
			CusEntryNumber result = shipment.Numbers.AddNew();
			result.CE_RN_NKCountryCode = countryCode;
			result.CE_EntryType = type;
			result.CE_EntryNum = number;
			return result;
		}
		#endregion

		#region TestRNSStatusFilterVisibility

		public void TestRNSStatusFilterVisibility()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var filterBO = new ShipmentReceivalFilterStrip();

				AssertNull("RNS Release Status filter should be invisible when current company is not a CA company", filterBO["RNS Release Status"]);
				AssertNull("RNS Release Date filter should be invisible when current company is not a CA company", filterBO["RNS Release Date"]);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				var filterBO = new ShipmentReceivalFilterStrip();

				AssertNotNull("RNS Release Status filter should be visible when current company is not a CA company", filterBO["RNS Release Status"]);
				AssertNotNull("RNS Release Date filter should be visible when current company is not a CA company", filterBO["RNS Release Date"]);
			}
		}

		public void TestArrivalCertificationVisibility()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				var filterBO = new ShipmentReceivalFilterStrip();

				AssertNull("Arrival Certification Status filter should be invisible when current company is not a CA company", filterBO["Arrival Certification Status"]);
				AssertNull("Arrival Certification filter should be invisible when current company is not a CA company", filterBO["Arrival Certification Date"]);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				var filterBO = new ShipmentReceivalFilterStrip();

				AssertNotNull("Arrival Certification Status filter should be visible when current company is not a CA company", filterBO["Arrival Certification Status"]);
				AssertNotNull("Arrival Certification filter should be visible when current company is not a CA company", filterBO["Arrival Certification Date"]);
			}
		}

		#endregion

		public void TestProfitLossReasonFilterWithOperators()
		{
			var filterBO = new ShipmentReceivalFilterStrip();

			var shipment1 = Factory.NewWithValidTestData<CFSShipment>();
			var job1 = new JobHeader.Loader(shipment1).TryLoadOrCreate();
			job1.JH_ProfitLossReasonCode = "ND1";

			var shipment2 = Factory.NewWithValidTestData<CFSShipment>();
			var job2 = new JobHeader.Loader(shipment2).TryLoadOrCreate();
			job2.JH_ProfitLossReasonCode = "CD1";

			var shipment3 = Factory.NewWithValidTestData<CFSShipment>();
			var job3 = new JobHeader.Loader(shipment3).TryLoadOrCreate();
			job3.JH_ProfitLossReasonCode = string.Empty;

			Factory.Save();

			var profitLossReasonFilter = (ModuleTextFilter)filterBO["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			var shipments = Factory.Load<CFSShipment>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, shipments);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			shipments = Factory.Load<CFSShipment>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, shipments);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			shipments = Factory.Load<CFSShipment>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			shipments = Factory.Load<CFSShipment>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment2, shipment3 }, shipments);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			shipments = Factory.Load<CFSShipment>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment2, shipment3 }, shipments);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			shipments = Factory.Load<CFSShipment>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment2, shipment3 }, shipments);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "ND1";

			shipments = Factory.Load<CFSShipment>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment3 }, shipments);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			shipments = Factory.Load<CFSShipment>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		#region Implementation

		#region Strip

		ShipmentReceivalFilterStrip Strip
		{
			get
			{
				if (strip == null)
				{
					strip = new ShipmentReceivalFilterStrip();
				}
				return strip;
			}
		}
		ShipmentReceivalFilterStrip strip;

		#endregion

		#region AssertDateRangeFilter

		void AssertDateRangeFilter(string filterName, ZDateTime date, CFSShipment shipment)
		{
			CFSShipment[] shipments;
			ModuleDateFilter filter = (ModuleDateFilter)Strip[filterName];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			filter.Property1 = date.AddDays(-1);
			shipments = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment, shipments);

			filter.Property2 = date.AddDays(-1);
			shipments = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionNotContains(shipment, shipments);

			filter.Property2 = date.AddDays(1);
			shipments = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment, shipments);

			filter.Property1 = date.AddDays(1);
			shipments = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionNotContains(shipment, shipments);

			filter.Property1 = ZDateTime.Empty;
			shipments = Factory.Load<CFSShipment>(filter.Query);
			AssertCollectionContains(shipment, shipments);
		}

		#endregion

		#endregion
	}
}
