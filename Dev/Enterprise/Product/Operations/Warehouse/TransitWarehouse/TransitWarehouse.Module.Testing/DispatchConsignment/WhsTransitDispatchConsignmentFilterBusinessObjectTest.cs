using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(WhsTransitDispatchConsignmentFilterBusinessObject))]
	public class WhsTransitDispatchConsignmentFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestJobID

		public void TestJobID()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, jobID: "DC00000001");
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK, jobID: "DC00000002");
			var dcn3 = Helper.CreateDispatchConsignment("DCN3", warehouse.PK, jobID: "DC00000010");

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcn1, dcn2, dcn3);
			var filter = (ModuleFountainFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.JobID];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "DC0000000";
			Asserter.AssertMatches("Must only return DCNs with Job ID starting with DC0000000", filters.Filter, dcn1, dcn2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "DC00000001";
			Asserter.AssertMatches("Must return the DCN with Job ID DC00000001", filters.Filter, dcn1);

			filter.Property = "";
			Asserter.AssertMatches("Must return all consignments", filters.Filter, dcn1, dcn2, dcn3);
		}

		#endregion

		#region TestReferenceNumber

		public void TestReferenceNumber()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, jobID: "1");
			var dcn2 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, jobID: "2");
			var dcn3 = Helper.CreateDispatchConsignment("DC", warehouse.PK, jobID: "3");

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcn1, dcn2, dcn3);
			var filter = (ModuleNumberFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.ReferenceNumber];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "DCN";
			Asserter.AssertMatches("Must only return DCNs with External Reference starting with DCN", filters.Filter, dcn1, dcn2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "DC";
			Asserter.AssertMatches("Must only return DCNs with External Reference DC", filters.Filter, dcn3);

			filter.Property = "";
			Asserter.AssertMatches("Must return all consignments", filters.Filter, dcn1, dcn2, dcn3);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse1 = Helper.CreateTRWWarehouse();
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");
			var warehouse3 = Helper.CreateTRWWarehouse(warehouseCode: "WH3");

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse1.PK, jobID: "1");
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse1.PK, jobID: "2");
			var dcn3 = Helper.CreateDispatchConsignment("DCN3", warehouse2.PK, jobID: "3");

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcn1, dcn2, dcn3);
			var filter = (ModuleGuidFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.Warehouse];
			filter.IsActive = true;
			AssertEquals(FilterCategories.Other, filter.Category);

			filter.Property = warehouse1.PK;
			Asserter.AssertMatches("Must only return consignments for the given warehouse", filters.Filter, dcn1, dcn2);
			filter.Property = warehouse2.PK;
			Asserter.AssertMatches("Must only return consignments for the given warehouse", filters.Filter, dcn3);
			filter.Property = warehouse3.PK;
			Asserter.AssertMatches("Must only return consignments for the given warehouse", filters.Filter);
		}

		public void TestWarehouse_FiltersTransitWarehouse_Ignores3PLSecurity()
			=> TestWarehouse_FiltersTransitWarehouse(hasSecurity: false, "3PL security should not affect transit warehouses.");

		public void TestWarehouse_FiltersTransitWarehouse_ExcludesProductWarehouse()
			=> TestWarehouse_FiltersTransitWarehouse(hasSecurity: true, "Product warehouses should always be filtered out.");

		public void TestWarehouse_FiltersTransitWarehouse(bool hasSecurity, string message)
		{
			var transitWarehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var productWarehouse = Helper.CreateWarehouse("Product Warehouse", "RowName");
			productWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;

			Env.Security.WhsAllowedWarehouses.IsAllowed = hasSecurity;

			Factory.Save();

			var filters = GetNewFilterStripBusinessObject();

			var filter = (ModuleGuidFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.Warehouse];
			var collection = (WhsWarehouseCollection)filter.List;
			collection.Load();
			AssertContainsExactElementsInAnyOrder(message, new[] { transitWarehouse.PK }, collection.Select(w => w.PK));
		}

		#endregion

		#region TestPackageStatus

		public void TestPackageStatus()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var dcnWithNoPackages = Helper.CreateDispatchConsignment("DCN0", warehouse.PK);

			var dcnWithBookedPackage = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var bookedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcnWithBookedPackage);

			var dcnWithArrivedPackage = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			var arrivedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, dispatchConsignment: dcnWithArrivedPackage);

			var dcnWithBookedAndArrivedPackage = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var bookedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcnWithBookedAndArrivedPackage);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, dispatchConsignment: dcnWithBookedAndArrivedPackage);

			var dcnWithPutawayPackage = Helper.CreateDispatchConsignment("DCN4", warehouse.PK);
			var putawayPackageState = Helper.CreatePackageState(rcn, 1, "PLT", "PKG5", TransitWarehouseStatuses.Codes.Putaway, receiveTransportationUnit, dispatchConsignment: dcnWithPutawayPackage);
			putawayPackageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			var dcnWithCommittedToTransferPackage = Helper.CreateDispatchConsignment("DCN5", warehouse.PK);
			var committedToTransfer = Helper.CreatePackageState(rcn, 1, "PKG", "PKG6", TransitWarehouseStatuses.Codes.Committed, receiveTransportationUnit, dispatchConsignment: dcnWithCommittedToTransferPackage);
			committedToTransfer.WPS_WL_LastLocation = location.PK;
			committedToTransfer.WPS_WL_ReceiveLocation = location.PK;

			var dcnWithStagedPackage = Helper.CreateDispatchConsignment("DCN6", warehouse.PK);
			var stagedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG7", TransitWarehouseStatuses.Codes.Staged, receiveTransportationUnit, dispatchConsignment: dcnWithStagedPackage, dispatchLoadList: dispatchLoadList);
			stagedPackageState.WPS_WL_LastLocation = location.PK;
			stagedPackageState.WPS_WL_ReceiveLocation = location.PK;

			var dcnWithDepartedPackage = Helper.CreateDispatchConsignment("DCN7", warehouse.PK);
			var departedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG8", TransitWarehouseStatuses.Codes.Departed, receiveTransportationUnit, dispatchConsignment: dcnWithDepartedPackage, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			departedPackageState.WPS_WL_LastLocation = location.PK;
			departedPackageState.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState.WPS_IsSecure = true;
			departedPackageState.WPS_SecurityStatus = "SEC";

			var dcnWithAdjustedOutPackage = Helper.CreateDispatchConsignment("DCN8", warehouse.PK);
			var adjustedOutPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG9", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: receiveTransportationUnit, dispatchConsignment: dcnWithAdjustedOutPackage);
			adjustedOutPackageState.WPS_WL_LastLocation = location.PK;

			var dcnWithPickedPackage = Helper.CreateDispatchConsignment("DCN9", warehouse.PK);
			var pickedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG10", TransitWarehouseStatuses.Codes.Picked, receiveTransportationUnit, dispatchConsignment: dcnWithPickedPackage, dispatchLoadList: dispatchLoadList);
			pickedPackageState.WPS_WL_LastLocation = location.PK;
			pickedPackageState.WPS_WL_ReceiveLocation = location.PK;

			var dcnWithLoadedPackage = Helper.CreateDispatchConsignment("DCN10", warehouse.PK);
			var loadedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG11", TransitWarehouseStatuses.Codes.FreightLoaded, receiveTransportationUnit, dispatchConsignment: dcnWithLoadedPackage, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			loadedPackageState.WPS_WL_LastLocation = location.PK;
			loadedPackageState.WPS_WL_ReceiveLocation = location.PK;
			loadedPackageState.WPS_IsSecure = true;
			loadedPackageState.WPS_SecurityStatus = "SEC";

			var dcnWithFinalizedPackage = Helper.CreateDispatchConsignment("DCN11", warehouse.PK);
			var finalizedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG12", TransitWarehouseStatuses.Codes.Finalized, receiveTransportationUnit, dispatchConsignment: dcnWithLoadedPackage, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			finalizedPackageState.WPS_WL_LastLocation = location.PK;
			finalizedPackageState.WPS_WL_ReceiveLocation = location.PK;
			finalizedPackageState.WPS_IsSecure = true;
			finalizedPackageState.WPS_SecurityStatus = "SEC";

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcnWithNoPackages, dcnWithBookedPackage, dcnWithArrivedPackage, dcnWithBookedAndArrivedPackage, dcnWithPutawayPackage, dcnWithStagedPackage, dcnWithLoadedPackage, dcnWithDepartedPackage, dcnWithAdjustedOutPackage, dcnWithCommittedToTransferPackage, dcnWithPickedPackage, dcnWithFinalizedPackage);

			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.PackageStatus];
			filter.IsActive = true;
			filter.Property = "ARV";
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			AssertContainsExactElementsInAnyOrder(new[] { "ARU", "BKD", "GIN", "ARP", "ARV", "UPN", "UPD", "PUT", "CTT", "PIC", "RTP", "PKN", "PKD", "STA", "FLO", "DEP", "FIN", "ADJ" }, filter.List.Cast<CodeDescriptionPair>().Select(s => s.Code));
			Asserter.AssertMatches("Only return DCNs with Arrived Package states.", filters.Filter, dcnWithArrivedPackage, dcnWithBookedAndArrivedPackage);

			filter.Property = "BKD";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Asserter.AssertMatches("Only return DCNs with Booked Package states.", filters.Filter, dcnWithBookedPackage, dcnWithBookedAndArrivedPackage);

			filter.Property = "";
			Asserter.AssertMatches("All DCNs should be returned.", filters.Filter, dcnWithNoPackages, dcnWithBookedPackage, dcnWithArrivedPackage, dcnWithBookedAndArrivedPackage, dcnWithPutawayPackage, dcnWithStagedPackage, dcnWithLoadedPackage, dcnWithDepartedPackage, dcnWithAdjustedOutPackage, dcnWithCommittedToTransferPackage, dcnWithPickedPackage, dcnWithFinalizedPackage);
		}

		public void TestAllPackagesReceived()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var dcnWithNoPackages = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var dcnWithOnlyAdjustedOutPackages = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			var adjustedOutPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: receiveTransportationUnit, dispatchConsignment: dcnWithOnlyAdjustedOutPackages);
			adjustedOutPackageState.WPS_WL_LastLocation = location.PK;

			var dcnWithBookedPackages = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var bookedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcnWithBookedPackages);
			var arrivedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, dispatchConsignment: dcnWithBookedPackages);

			var dcnWithOnlyReceivedPackages = Helper.CreateDispatchConsignment("DCN4", warehouse.PK);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, dispatchConsignment: dcnWithOnlyReceivedPackages);
			var putawayPackageState = Helper.CreatePackageState(rcn, 1, "PLT", "PKG5", TransitWarehouseStatuses.Codes.Putaway, receiveTransportationUnit, dispatchConsignment: dcnWithOnlyReceivedPackages);
			putawayPackageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			var adjustedOutPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG6", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: receiveTransportationUnit, dispatchConsignment: dcnWithOnlyReceivedPackages);
			adjustedOutPackageState2.WPS_WL_LastLocation = location.PK;

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcnWithNoPackages, dcnWithOnlyAdjustedOutPackages, dcnWithBookedPackages, dcnWithOnlyReceivedPackages);

			var receivedFlag = WhsTransitDispatchConsignmentFilterBusinessObject.Codes.AllPackagesReceived;
			var notReceivedFlag = WhsTransitDispatchConsignmentFilterBusinessObject.Codes.NotAllPackagesReceived;
			var allFlag = WhsTransitDispatchConsignmentFilterBusinessObject.Codes.All;

			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.AllPackagesReceived];
			filter.IsActive = true;
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			AssertContainsExactElementsInAnyOrder(new[] { receivedFlag, notReceivedFlag, allFlag }, filter.List.Cast<CodeDescriptionPair>().Select(s => s.Code));
			filter.Property = receivedFlag;
			Asserter.AssertMatches("Return consignments with only received packages, ignoring Adjusted Out packages", filters.Filter, dcnWithOnlyReceivedPackages);

			filter.Property = notReceivedFlag;
			Asserter.AssertMatches("Return consignments with no packages or booked packages", filters.Filter, dcnWithNoPackages, dcnWithOnlyAdjustedOutPackages, dcnWithBookedPackages);

			filter.Property = allFlag;
			Asserter.AssertMatches("Return all consignments", filters.Filter, dcnWithNoPackages, dcnWithOnlyAdjustedOutPackages, dcnWithBookedPackages, dcnWithOnlyReceivedPackages);
		}

		public void TestAllPackagesDeparted()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var dcnWithNoPackages = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var dcnWithOnlyAdjustedOutPackages = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			var adjustedOutPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: receiveTransportationUnit, dispatchConsignment: dcnWithOnlyAdjustedOutPackages);
			adjustedOutPackageState.WPS_WL_LastLocation = location.PK;

			var dcnWithBookedPackages = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var bookedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcnWithBookedPackages);
			var departedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Departed, receiveTransportationUnit, dispatchConsignment: dcnWithBookedPackages, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			departedPackageState.WPS_WL_LastLocation = location.PK;
			departedPackageState.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState.WPS_IsSecure = true;
			departedPackageState.WPS_SecurityStatus = "SEC";

			var dcnWithOnlyDepartedPackages = Helper.CreateDispatchConsignment("DCN4", warehouse.PK);
			var departedPackageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Departed, receiveTransportationUnit, dispatchConsignment: dcnWithOnlyDepartedPackages, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			departedPackageState3.WPS_WL_LastLocation = location.PK;
			departedPackageState3.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState3.WPS_IsSecure = true;
			departedPackageState3.WPS_SecurityStatus = "SEC";
			var finalizedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG5", TransitWarehouseStatuses.Codes.Finalized, receiveTransportationUnit, dispatchConsignment: dcnWithOnlyDepartedPackages, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			finalizedPackageState.WPS_WL_LastLocation = location.PK;
			finalizedPackageState.WPS_WL_ReceiveLocation = location.PK;
			finalizedPackageState.WPS_IsSecure = true;
			finalizedPackageState.WPS_SecurityStatus = "SEC";
			var adjustedOutPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG6", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: receiveTransportationUnit, dispatchConsignment: dcnWithOnlyDepartedPackages);
			adjustedOutPackageState2.WPS_WL_LastLocation = location.PK;

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcnWithNoPackages, dcnWithOnlyAdjustedOutPackages, dcnWithBookedPackages, dcnWithOnlyDepartedPackages);

			var departedFlag = WhsTransitDispatchConsignmentFilterBusinessObject.Codes.AllPackagesDeparted;
			var notDepartedFlag = WhsTransitDispatchConsignmentFilterBusinessObject.Codes.NotAllPackagesDeparted;
			var allFlag = WhsTransitDispatchConsignmentFilterBusinessObject.Codes.All;

			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.AllPackagesDeparted];
			filter.IsActive = true;
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			AssertContainsExactElementsInAnyOrder(new[] { departedFlag, notDepartedFlag, allFlag }, filter.List.Cast<CodeDescriptionPair>().Select(s => s.Code));
			filter.Property = departedFlag;
			Asserter.AssertMatches("Return consignments with only departed packages, ignoring Adjusted Out packages", filters.Filter, dcnWithOnlyDepartedPackages);

			filter.Property = notDepartedFlag;
			Asserter.AssertMatches("Return consignments with non departed packages", filters.Filter, dcnWithNoPackages, dcnWithOnlyAdjustedOutPackages, dcnWithBookedPackages);

			filter.Property = allFlag;
			Asserter.AssertMatches("Return all consignments", filters.Filter, dcnWithNoPackages, dcnWithOnlyAdjustedOutPackages, dcnWithBookedPackages, dcnWithOnlyDepartedPackages);
		}

		#endregion

		#region TestAdditionalReference

		public void TestAdditionalReference()
		{
			var warehouse1 = Helper.CreateTransitWarehouseInCurrentBranch();

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse1.PK, jobID: "1");
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse1.PK, jobID: "2");
			var dcn3 = Helper.CreateDispatchConsignment("DCN3", warehouse1.PK, jobID: "3");
			var dcn4 = Helper.CreateDispatchConsignment("DCN4", warehouse1.PK, jobID: "4");
			Helper.CreateAdditionalReference(dcn1, "Ref1");
			Helper.CreateAdditionalReference(dcn1, "Ref2");
			Helper.CreateAdditionalReference(dcn2, "Ref2");
			Helper.CreateAdditionalReference(dcn3, "Re");

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcn1, dcn2, dcn3, dcn4);
			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.AdditionalReference];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "Ref";
			Asserter.AssertMatches("Must only return DCNs with an Additional Reference starting with Ref", filters.Filter, dcn1, dcn2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "Ref1";
			Asserter.AssertMatches("Must only return DCNs with Additional Reference Ref1", filters.Filter, dcn1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "Re";
			Asserter.AssertMatches("Must only return DCNs with Additional Reference Re", filters.Filter, dcn3);

			filter.Property = "";
			Asserter.AssertMatches("Must return all consignments", filters.Filter, dcn1, dcn2, dcn3, dcn4);
		}

		#endregion

		#region Organisations

		public void TestCompanyNameFilter()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var bookingParty = Helper.CreateClient("BKD1");
			var consignee = Helper.CreateClient("LCI1");
			var consignor = Helper.CreateClient("LCE1");

			var dcnWithAddress = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, bookingParty, consignor, consignee);
			var dcnWithCompanyOverride = Helper.CreateDispatchConsignment("DCN2", warehouse.PK, null, consignor, consignee);
			var dcnWithNoOrg = Helper.CreateDispatchConsignment("DCN3", warehouse.PK, null, consignor, consignee);
			var dcnWithBlankJDA = Helper.CreateDispatchConsignment("DCN4", warehouse.PK, null, consignor, consignee);
			var dcnWithBlankJDAOverride = Helper.CreateDispatchConsignment("DCN5", warehouse.PK, null, consignor, consignee);
			var dcnWithOtherBlankAddresses = Helper.CreateDispatchConsignment("DCN6", warehouse.PK, bookedByParty: null, consignor: null, consignee: null);
			var filters = GetNewFilterStripBusinessObject();

			var blankJDA = Helper.CreateJobDocAddress(dcnWithBlankJDA, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			blankJDA.E2_AddressOverride = false;
			Helper.CreateJobDocAddressWithOverride(dcnWithBlankJDAOverride, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			Helper.CreateJobDocAddressWithOverride(dcnWithCompanyOverride, DocAddressTypes.Codes.BookingPartyDocumentaryAddress, "BKD1");

			Helper.CreateJobDocAddress(dcnWithOtherBlankAddresses, DocAddressTypes.Codes.ClientRequestedBillingParty);
			Helper.CreateJobDocAddressWithOverride(dcnWithOtherBlankAddresses, DocAddressTypes.Codes.ConsigneeDocumentaryAddress);
			Helper.CreateJobDocAddressWithOverride(dcnWithOtherBlankAddresses, DocAddressTypes.Codes.BookingPartyDocumentaryAddress, "BKD1");

			Factory.Save();

			Asserter.AddToScope(dcnWithAddress, dcnWithCompanyOverride, dcnWithNoOrg, dcnWithBlankJDA, dcnWithBlankJDAOverride, dcnWithOtherBlankAddresses);
			var bkdFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.BookingPartyCompanyName];
			bkdFilter.IsActive = true;
			bkdFilter.Property = bookingParty.OH_FullName;
			AssertEquals(FilterCategories.Organisations, bkdFilter.Category);
			Asserter.AssertMatches("Must only return consignments for BKD1", filters.Filter, dcnWithAddress, dcnWithCompanyOverride, dcnWithOtherBlankAddresses);

			bkdFilter.Property = consignee.OH_FullName;
			Asserter.AssertMatches("Must filter on address type", filters.Filter);

			bkdFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, dcnWithAddress, dcnWithNoOrg, dcnWithBlankJDA, dcnWithBlankJDAOverride, dcnWithCompanyOverride, dcnWithOtherBlankAddresses);

			bkdFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("Must return consignments with no company", filters.Filter, dcnWithNoOrg, dcnWithBlankJDA, dcnWithBlankJDAOverride);
		}

		public void TestConsigneeCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var consignee1 = Helper.CreateClient("LCI1");
			var consignee2 = Helper.CreateClient("LCI2");
			var consignor = Helper.CreateClient("LCE1");
			var bookingParty = Helper.CreateClient("BKD1");

			var dcn1a = Helper.CreateDispatchConsignment("DCN1a", warehouse.PK, bookingParty, consignor, consignee1);
			var dcn1b = Helper.CreateDispatchConsignment("DCN1b", warehouse.PK, bookingParty, consignor, consignee1);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK, bookingParty, consignor, consignee2);
			var dcnWithNoOrg = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcn1a, dcn1b, dcn2, dcnWithNoOrg);
			var bkdFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.ConsigneeCompanyName];
			bkdFilter.IsActive = true;
			bkdFilter.Property = consignee1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, bkdFilter.Category);
			Asserter.AssertMatches("Must only return consignments for LCI1", filters.Filter, dcn1a, dcn1b);

			bkdFilter.Property = consignee2.OH_FullName;
			Asserter.AssertMatches("Must only return consignments for LCI2", filters.Filter, dcn2);

			bkdFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, dcn1a, dcn1b, dcn2, dcnWithNoOrg);
		}

		public void TestConsignorCompanyName_DoesNotHaveFilter()
		{
			var filters = GetNewFilterStripBusinessObject();
			AssertNull(filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.ConsignorCompanyName]);
		}

		public void TestBookingPartyCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var bookingParty1 = Helper.CreateClient("BKD1");
			var bookingParty2 = Helper.CreateClient("BKD2");
			var consignee = Helper.CreateClient("LCI1");
			var consignor = Helper.CreateClient("LCE2");

			var dcn1a = Helper.CreateDispatchConsignment("DCN1a", warehouse.PK, bookingParty1, consignor, consignee);
			var dcn1b = Helper.CreateDispatchConsignment("DCN1b", warehouse.PK, bookingParty1, consignor, consignee);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK, bookingParty2, consignor, consignee);
			var dcnWithNoOrg = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcn1a, dcn1b, dcn2, dcnWithNoOrg);
			var bkdFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.BookingPartyCompanyName];
			bkdFilter.IsActive = true;
			bkdFilter.Property = bookingParty1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, bkdFilter.Category);
			Asserter.AssertMatches("Must only return consignments for BKD1", filters.Filter, dcn1a, dcn1b);

			bkdFilter.Property = bookingParty2.OH_FullName;
			Asserter.AssertMatches("Must only return consignments for BKD2", filters.Filter, dcn2);

			bkdFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, dcn1a, dcn1b, dcn2, dcnWithNoOrg);
		}

		public void TestBillingCompanyName_FromJobDocAddress()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var billing1 = Helper.CreateClient("CRB1");
			var billing2 = Helper.CreateClient("CRB2");
			var consignor = Helper.CreateClient("LCE");
			var consignee = Helper.CreateClient("LCI");
			var bookingParty = Helper.CreateClient("BKD");

			var dcn1a = Helper.CreateDispatchConsignment("DCN1a", warehouse.PK, bookingParty, consignor, consignee);
			var dcn1b = Helper.CreateDispatchConsignment("DCN1b", warehouse.PK, bookingParty, consignor, consignee);
			var dcnWithCompanyOverride = Helper.CreateDispatchConsignment("DCN1c", warehouse.PK, bookingParty, consignor, consignee);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK, bookingParty, consignor, consignee);
			var dcnWithNoOrg = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var dcnWithEmptyJDA = Helper.CreateDispatchConsignment("DCN4", warehouse.PK, bookingParty, consignor, consignee);
			Helper.CreateJobDocAddressFromAddress(dcn1a, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(dcn1b, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(dcn2, DocAddressTypes.Codes.ClientRequestedBillingParty, billing2.MainAddress);
			Helper.CreateJobDocAddressWithOverride(dcnWithCompanyOverride, DocAddressTypes.Codes.ClientRequestedBillingParty, "CRB1");
			Helper.CreateJobDocAddressWithOverride(dcnWithEmptyJDA, DocAddressTypes.Codes.ClientRequestedBillingParty);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcn1a, dcn1b, dcnWithCompanyOverride, dcn2, dcnWithNoOrg, dcnWithEmptyJDA);
			var crbFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.BillingPartyCompanyName];
			crbFilter.IsActive = true;
			crbFilter.Property = billing1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, crbFilter.Category);
			Asserter.AssertMatches("Must only return consignments for CRB1", filters.Filter, dcn1a, dcn1b, dcnWithCompanyOverride);

			crbFilter.Property = billing2.OH_FullName;
			Asserter.AssertMatches("Must only return consignments for CRB2", filters.Filter, dcn2);

			crbFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, dcn1a, dcn1b, dcnWithCompanyOverride, dcn2, dcnWithNoOrg, dcnWithEmptyJDA);

			crbFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("Must return consignments with no company", filters.Filter, dcnWithNoOrg, dcnWithEmptyJDA);
		}

		public void TestBillingCompanyName_LocalClientFallback()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var billing1 = Helper.CreateClient("CRB1");
			var billing2 = Helper.CreateClient("CRB2");

			var dcnWithCRB = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dcnWithCRBAndJobHeader = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			var dcnWithCRBAndLocalClient = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var dcnWithNoAddress = Helper.CreateDispatchConsignment("DCN4", warehouse.PK);
			var dcnWithNoCRBAndWithJobHeader = Helper.CreateDispatchConsignment("DCN5", warehouse.PK);

			var job1 = new JobHeader.Loader(dcnWithCRBAndJobHeader).TryLoadOrCreate();
			var job2 = new JobHeader.Loader(dcnWithCRBAndLocalClient).TryLoadOrCreate();
			var job3 = new JobHeader.Loader(dcnWithNoCRBAndWithJobHeader).TryLoadOrCreate();
			job1.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job2.JH_OA_LocalChargesAddr = billing2.MainAddress.PK;
			job3.JH_OA_LocalChargesAddr = ZGuid.Empty;

			Helper.CreateJobDocAddressFromAddress(dcnWithCRB, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(dcnWithCRBAndJobHeader, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(dcnWithCRBAndLocalClient, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcnWithCRB, dcnWithCRBAndJobHeader, dcnWithCRBAndLocalClient, dcnWithNoAddress, dcnWithNoCRBAndWithJobHeader);
			var bkdFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.BillingPartyCompanyName];
			bkdFilter.IsActive = true;
			bkdFilter.Property = billing1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, bkdFilter.Category);
			Asserter.AssertMatches("Must fallback to consignments with billing doc address", filters.Filter, dcnWithCRB);

			bkdFilter.Property = billing2.OH_FullName;
			Asserter.AssertMatches("Must only return consignments with billing local client", filters.Filter, dcnWithCRBAndLocalClient);

			bkdFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("Must only return consignments with no billing party", filters.Filter, dcnWithCRBAndJobHeader, dcnWithNoAddress, dcnWithNoCRBAndWithJobHeader);
		}

		public void TestCTOCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var ctoCompany1 = Helper.CreateClient("DCT1");
			var ctoCompany2 = Helper.CreateClient("DCT2");

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, departureCTO: ctoCompany1);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK, departureCTO: ctoCompany2);
			var dcn3 = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcn1, dcn2, dcn3);
			var ctoFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.CTOCompanyName];
			ctoFilter.IsActive = true;
			AssertEquals(FilterCategories.Organisations, ctoFilter.Category);

			ctoFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, dcn1, dcn2, dcn3);

			ctoFilter.Property = ctoCompany1.OH_FullName;
			Asserter.AssertMatches("Must only return consignment for CTO1", filters.Filter, dcn1);

			ctoFilter.Property = ctoCompany2.OH_FullName;
			Asserter.AssertMatches("Must only return consignment for CTO2", filters.Filter, dcn2);
		}

		public void TestDeliveryCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var deliveryCompany1 = Helper.CreateClient("DCT1");
			var deliveryCompany2 = Helper.CreateClient("DCT2");

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, deliveryAddress: deliveryCompany1);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK, deliveryAddress: deliveryCompany2);
			var dcn3 = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcn1, dcn2, dcn3);
			var cegFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.DeliveryCompanyName];
			cegFilter.IsActive = true;
			AssertEquals(FilterCategories.Organisations, cegFilter.Category);

			cegFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, dcn1, dcn2, dcn3);

			cegFilter.Property = deliveryCompany1.OH_FullName;
			Asserter.AssertMatches("Must only return consignments for Delivery Company 1", filters.Filter, dcn1);

			cegFilter.Property = deliveryCompany2.OH_FullName;
			Asserter.AssertMatches("Must only return consignments for Delivery Company 2", filters.Filter, dcn2);
		}

		public void TestTransportCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var transportCompany1 = Helper.CreateClient("TRA1");
			var transportCompany2 = Helper.CreateClient("TRA2");

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, transportCompany: transportCompany1);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK, transportCompany: transportCompany2);
			var dcn3 = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcn1, dcn2, dcn3);
			var traFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.TransportCompanyName];
			traFilter.IsActive = true;
			AssertEquals(FilterCategories.Organisations, traFilter.Category);

			traFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, dcn1, dcn2, dcn3);

			traFilter.Property = transportCompany1.OH_FullName;
			Asserter.AssertMatches("Must only return consignments for Transport Company 1", filters.Filter, dcn1);

			traFilter.Property = transportCompany2.OH_FullName;
			Asserter.AssertMatches("Must only return consignments for Transport Company 2", filters.Filter, dcn2);
		}

		#endregion

		#region Billing

		public void TestJobInvoicingStatusFilter()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			Asserter.AddToScope(dcn1);

			var filters = GetNewFilterStripBusinessObject();
			AssertNotNull(filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.InvoiceStatus]);
			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.InvoiceStatus];

			var job = new JobHeader.Loader(dcn1).TryLoadOrCreate();
			AssertEquals("Precondition", JobHeaderStatus.Working.Code, job.JH_Status);

			Factory.Save();

			filter.Property = JobHeaderStatus.Working.Code;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.IsActive = true;

			Asserter.AssertMatches("Should find the DCN with the given status", filter, dcn1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;

			Asserter.AssertMatches("Should exclude the DCN with the given status", filter);
		}

		public void TestInvoicedChargesFilter()
		{
			var currentBranch = GlbBranch.CurrentBranch;
			var code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR"));

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var warehouse = Helper.CreateWarehouse("WH1", address, currentBranch);
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var job = new JobHeader.Loader(dcn).TryLoadOrCreate();
			job.JH_GB = currentBranch.PK;

			var filters = GetNewFilterStripBusinessObject();
			AssertNotNull(filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.InvoicedChargesBilling]);
			var invoicedChargesFilter = (ModuleFlagsFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.InvoicedChargesBilling];
			Asserter.AddToScope(dcn);

			Factory.Save();

			invoicedChargesFilter.IsActive = true;
			invoicedChargesFilter["No Charges"] = true;
			Asserter.AssertMatches("Must return dcns with no charges", invoicedChargesFilter, dcn);

			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = code.PK;
			charge.JR_GB = currentBranch.PK;
			charge.JR_LocalSellAmt = 10m;
			Factory.Save();

			Asserter.AssertMatches("All DCNs have charges and should not be returned", invoicedChargesFilter);
		}

		class WhsItemDispatchConsignmentFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<WhsItemDispatchConsignment>
		{
			#region Implementation

			protected override WhsItemDispatchConsignment GetNewBusinessObjectForFilterCollection()
			{
				var poke = Warehouse;
				var dcn = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
				dcn.WDC_WW_Warehouse = Warehouse.PK;
				return dcn;
			}

			protected override ModuleIdentifier FilterStripModuleID
			{
				get { return ModuleIDs.WhsTransitDispatchConsignment; }
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

		#region TestDirection

		public void TestDirection()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, direction: "IMP");
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK, direction: "EXP");
			var dcn3 = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(dcn1, dcn2, dcn3);
			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitDispatchConsignmentFilterBusinessObject.Schema.Direction];
			filter.IsActive = true;
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "IMP";
			Asserter.AssertMatches("Must only return consignments with direction with IMP", filters.Filter, dcn1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "EXP";
			Asserter.AssertMatches("Must return the consignment with direction with EXP", filters.Filter, dcn2);

			filter.Property = "";
			Asserter.AssertMatches("Must return all consignments", filters.Filter, dcn1, dcn2, dcn3);
		}

		#endregion

		#region TestOverrideFilters

		public void TestGetModuleFilterThatOverridesAllOtherFilters()
		{
			var filters = new WhsTransitDispatchConsignmentFilterBusinessObjectForTest();
			AssertNull(filters.GetModuleFilterThatOverridesAllOtherFiltersForTest());
		}

		class WhsTransitDispatchConsignmentFilterBusinessObjectForTest : WhsTransitDispatchConsignmentFilterBusinessObject
		{
			public ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersForTest()
			{
				return base.GetModuleFilterThatOverridesAllOtherFilters();
			}
		}

		#endregion

		public void TestProfitLossReasonFilterWithOperators()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, direction: "IMP");
			var job1 = new JobHeader.Loader(dcn1).TryLoadOrCreate();
			job1.JH_ProfitLossReasonCode = "ND1";

			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK, direction: "EXP");
			var job2 = new JobHeader.Loader(dcn2).TryLoadOrCreate();
			job2.JH_ProfitLossReasonCode = "CD1";

			var dcn3 = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var job3 = new JobHeader.Loader(dcn3).TryLoadOrCreate();
			job3.JH_ProfitLossReasonCode = string.Empty;
			Factory.Save();

			Asserter.AddToScope(dcn1, dcn2, dcn3);

			var filters = GetNewFilterStripBusinessObject();
			var profitLossReasonFilter = (ModuleTextFilter)filters["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for dcn1.", profitLossReasonFilter, dcn1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for dcn1.", profitLossReasonFilter, dcn1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			Asserter.AssertMatches("Has a Match for dcn1 and dcn2", profitLossReasonFilter, dcn1, dcn2);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for dcn2 and dcn3", profitLossReasonFilter, dcn2, dcn3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for dcn2 and dcn3", profitLossReasonFilter, dcn2, dcn3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for dcn2 and dcn3", profitLossReasonFilter, dcn2, dcn3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "";

			Asserter.AssertMatches("Has a Match for dcn3", profitLossReasonFilter, dcn3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for dcn1 and dcn2", profitLossReasonFilter, dcn1, dcn2);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WhsTransitDispatchConsignmentFilterBusinessObject();
		}

		protected FilterStripAsserter<WhsItemDispatchConsignment> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<WhsItemDispatchConsignment>(Factory, dcn => dcn.WDC_ConsignmentID)); }
		}
		FilterStripAsserter<WhsItemDispatchConsignment> asserter;

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
