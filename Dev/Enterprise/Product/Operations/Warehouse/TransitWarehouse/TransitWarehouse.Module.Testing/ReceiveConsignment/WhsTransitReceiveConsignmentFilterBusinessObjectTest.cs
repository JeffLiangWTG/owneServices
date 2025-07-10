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
	[TestedType(typeof(WhsTransitReceiveConsignmentFilterBusinessObject))]
	public class WhsTransitReceiveConsignmentFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestJobID

		public void TestJobID()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, jobID: "RC00000001");
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse.PK, jobID: "RC00000002");
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", "STD", warehouse.PK, jobID: "RC00000010");

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcn1, rcn2, rcn3);
			var filter = (ModuleFountainFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.JobID];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "RC0000000";
			Asserter.AssertMatches("Must only return consignments with Job ID starting with RC0000000", filters.Filter, rcn1, rcn2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "RC00000001";
			Asserter.AssertMatches("Must return the consignment with Job ID RC00000001", filters.Filter, rcn1);

			filter.Property = "";
			Asserter.AssertMatches("Must return all consignments", filters.Filter, rcn1, rcn2, rcn3);
		}

		#endregion

		#region TestReferenceNumber

		public void TestReferenceNumber()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, jobID: "1");
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse.PK, jobID: "2");
			var rcn3 = Helper.CreateReceiveConsignment("RC", "STD", warehouse.PK, jobID: "3");

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcn1, rcn2, rcn3);
			var filter = (ModuleNumberFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.ReferenceNumber];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "RCN";
			Asserter.AssertMatches("Must only return RCNs with External Reference starting with RCN", filters.Filter, rcn1, rcn2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "RC";
			Asserter.AssertMatches("Must only return RCNs with External Reference RC", filters.Filter, rcn3);

			filter.Property = "";
			Asserter.AssertMatches("Must return all consignments", filters.Filter, rcn1, rcn2, rcn3);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse1 = Helper.CreateTRWWarehouse();
			var warehouse2 = Helper.CreateTRWWarehouse(warehouseCode: "WH2");
			var warehouse3 = Helper.CreateTRWWarehouse(warehouseCode: "WH3");

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse1.PK, jobID: "1");
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse1.PK, jobID: "2");
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", "STD", warehouse2.PK, jobID: "3");

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcn1, rcn2, rcn3);
			var filter = (ModuleGuidFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.Warehouse];
			filter.IsActive = true;
			AssertEquals(FilterCategories.Other, filter.Category);

			filter.Property = warehouse1.PK;
			Asserter.AssertMatches("Must only return consignments for the given warehouse", filters.Filter, rcn1, rcn2);
			filter.Property = warehouse2.PK;
			Asserter.AssertMatches("Must only return consignments for the given warehouse", filters.Filter, rcn3);
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

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var rcnWithNoPackages = Helper.CreateReceiveConsignment("RCN0", "STD", warehouse.PK);

			var rcnWithBookedPackage = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var bookedPackageState = Helper.CreatePackageState(rcnWithBookedPackage, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked);

			var rcnWithArrivedPackage = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse.PK);
			var arrivedPackageState = Helper.CreatePackageState(rcnWithArrivedPackage, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var rcnWithBookedAndArrivedPackage = Helper.CreateReceiveConsignment("RCN3", "STD", warehouse.PK);
			var bookedPackageState2 = Helper.CreatePackageState(rcnWithBookedAndArrivedPackage, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked);
			var arrivedPackageState2 = Helper.CreatePackageState(rcnWithBookedAndArrivedPackage, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var rcnWithPutawayPackage = Helper.CreateReceiveConsignment("RCN4", "STD", warehouse.PK);
			var putawayPackageState = Helper.CreatePackageState(rcnWithPutawayPackage, 1, "PLT", "PKG5", TransitWarehouseStatuses.Codes.Putaway, receiveTransportationUnit);
			putawayPackageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			var rcnWithCommittedToTransferPackage = Helper.CreateReceiveConsignment("RCN5", "STD", warehouse.PK);
			var committedToTransfer = Helper.CreatePackageState(rcnWithCommittedToTransferPackage, 1, "PKG", "PKG6", TransitWarehouseStatuses.Codes.Committed, receiveTransportationUnit);
			committedToTransfer.WPS_WL_LastLocation = location.PK;
			committedToTransfer.WPS_WL_ReceiveLocation = location.PK;

			var rcnWithStagedPackage = Helper.CreateReceiveConsignment("RCN6", "STD", warehouse.PK);
			var stagedPackageState = Helper.CreatePackageState(rcnWithStagedPackage, 1, "PKG", "PKG7", TransitWarehouseStatuses.Codes.Staged, receiveTransportationUnit, dispatchConsignment: dcn, dispatchLoadList: dispatchLoadList);
			stagedPackageState.WPS_WL_LastLocation = location.PK;
			stagedPackageState.WPS_WL_ReceiveLocation = location.PK;

			var rcnWithDepartedPackage = Helper.CreateReceiveConsignment("RCN7", "STD", warehouse.PK);
			var departedPackageState = Helper.CreatePackageState(rcnWithDepartedPackage, 1, "PKG", "PKG8", TransitWarehouseStatuses.Codes.Departed, receiveTransportationUnit, dispatchConsignment: dcn, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			departedPackageState.WPS_WL_LastLocation = location.PK;
			departedPackageState.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState.WPS_IsSecure = true;
			departedPackageState.WPS_SecurityStatus = "SEC";

			var rcnWithAdjustedOutPackage = Helper.CreateReceiveConsignment("RCN8", "STD", warehouse.PK);
			var adjustedOutPackageState = Helper.CreatePackageState(rcnWithAdjustedOutPackage, 1, "PKG", "PKG9", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: receiveTransportationUnit);
			adjustedOutPackageState.WPS_WL_LastLocation = location.PK;

			var rcnWithPickedPackage = Helper.CreateReceiveConsignment("RCN9", "STD", warehouse.PK);
			var pickedPackageState = Helper.CreatePackageState(rcnWithPickedPackage, 1, "PKG", "PKG10", TransitWarehouseStatuses.Codes.Picked, receiveTransportationUnit, dispatchConsignment: dcn, dispatchLoadList: dispatchLoadList);
			pickedPackageState.WPS_WL_LastLocation = location.PK;
			pickedPackageState.WPS_WL_ReceiveLocation = location.PK;

			var rcnWithLoadedPackage = Helper.CreateReceiveConsignment("RCN10", "STD", warehouse.PK);
			var loadedPackageState = Helper.CreatePackageState(rcnWithLoadedPackage, 1, "PKG", "PKG11", TransitWarehouseStatuses.Codes.FreightLoaded, receiveTransportationUnit, dispatchConsignment: dcn, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			loadedPackageState.WPS_WL_LastLocation = location.PK;
			loadedPackageState.WPS_WL_ReceiveLocation = location.PK;
			loadedPackageState.WPS_IsSecure = true;
			loadedPackageState.WPS_SecurityStatus = "SEC";

			var rcnWithFinalizedPackage = Helper.CreateReceiveConsignment("RCN11", warehouse.PK);
			var finalizedPackageState = Helper.CreatePackageState(rcnWithFinalizedPackage, 1, "PKG", "PKG12", TransitWarehouseStatuses.Codes.Finalized, receiveTransportationUnit, dispatchConsignment: dcn, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			finalizedPackageState.WPS_WL_LastLocation = location.PK;
			finalizedPackageState.WPS_WL_ReceiveLocation = location.PK;
			finalizedPackageState.WPS_IsSecure = true;
			finalizedPackageState.WPS_SecurityStatus = "SEC";

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcnWithNoPackages, rcnWithBookedPackage, rcnWithArrivedPackage, rcnWithBookedAndArrivedPackage, rcnWithPutawayPackage, rcnWithStagedPackage, rcnWithLoadedPackage, rcnWithDepartedPackage, rcnWithAdjustedOutPackage, rcnWithCommittedToTransferPackage, rcnWithPickedPackage, rcnWithFinalizedPackage);

			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.PackageStatus];
			filter.IsActive = true;
			filter.Property = "ARV";
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			AssertContainsExactElementsInAnyOrder(new[] { "ARU", "BKD", "GIN", "ARP", "ARV", "UPN", "UPD", "PUT", "CTT", "PIC", "RTP", "PKN", "PKD", "STA", "FLO", "DEP", "FIN", "ADJ" }, filter.List.Cast<CodeDescriptionPair>().Select(s => s.Code));
			Asserter.AssertMatches("Only return consignments with Arrived Package states.", filters.Filter, rcnWithArrivedPackage, rcnWithBookedAndArrivedPackage);

			filter.Property = "BKD";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Asserter.AssertMatches("Only return consignments with Booked Package states.", filters.Filter, rcnWithBookedPackage, rcnWithBookedAndArrivedPackage);

			filter.Property = "";
			Asserter.AssertMatches("All consignments should be returned.", filters.Filter, rcnWithNoPackages, rcnWithBookedPackage, rcnWithArrivedPackage, rcnWithBookedAndArrivedPackage, rcnWithPutawayPackage, rcnWithStagedPackage, rcnWithLoadedPackage, rcnWithDepartedPackage, rcnWithAdjustedOutPackage, rcnWithCommittedToTransferPackage, rcnWithPickedPackage, rcnWithFinalizedPackage);
		}

		public void TestAllPackagesReceived()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var rcnWithNoPackages = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var rcnWithOnlyAdjustedOutPackages = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var adjustedOutPackageState = Helper.CreatePackageState(rcnWithOnlyAdjustedOutPackages, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: receiveTransportationUnit);
			adjustedOutPackageState.WPS_WL_LastLocation = location.PK;

			var rcnWithBookedPackages = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var bookedPackageState = Helper.CreatePackageState(rcnWithBookedPackages, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked);
			var arrivedPackageState = Helper.CreatePackageState(rcnWithBookedPackages, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var rcnWithOnlyReceivedPackages = Helper.CreateReceiveConsignment("RCN4", warehouse.PK);
			var arrivedPackageState2 = Helper.CreatePackageState(rcnWithOnlyReceivedPackages, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var putawayPackageState = Helper.CreatePackageState(rcnWithOnlyReceivedPackages, 1, "PLT", "PKG5", TransitWarehouseStatuses.Codes.Putaway, receiveUnit: receiveTransportationUnit);
			putawayPackageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			var adjustedOutPackageState2 = Helper.CreatePackageState(rcnWithOnlyReceivedPackages, 1, "PKG", "PKG6", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: receiveTransportationUnit);
			adjustedOutPackageState2.WPS_WL_LastLocation = location.PK;

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcnWithNoPackages, rcnWithOnlyAdjustedOutPackages, rcnWithBookedPackages, rcnWithOnlyReceivedPackages);

			var receivedFlag = WhsTransitReceiveConsignmentFilterBusinessObject.Codes.AllPackagesReceived;
			var notReceivedFlag = WhsTransitReceiveConsignmentFilterBusinessObject.Codes.NotAllPackagesReceived;
			var allFlag = WhsTransitReceiveConsignmentFilterBusinessObject.Codes.All;

			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.AllPackagesReceived];
			filter.IsActive = true;
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			AssertContainsExactElementsInAnyOrder(new[] { receivedFlag, notReceivedFlag, allFlag }, filter.List.Cast<CodeDescriptionPair>().Select(s => s.Code));
			filter.Property = receivedFlag;
			Asserter.AssertMatches("Return consignments with only received packages, ignoring Adjusted Out packages", filters.Filter, rcnWithOnlyReceivedPackages);

			filter.Property = notReceivedFlag;
			Asserter.AssertMatches("Return consignments with no packages or booked packages", filters.Filter, rcnWithNoPackages, rcnWithOnlyAdjustedOutPackages, rcnWithBookedPackages);

			filter.Property = allFlag;
			Asserter.AssertMatches("Return all consignments", filters.Filter, rcnWithNoPackages, rcnWithOnlyAdjustedOutPackages, rcnWithBookedPackages, rcnWithOnlyReceivedPackages);
		}

		public void TestAllPackagesDeparted()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchLoadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var rcnWithNoPackages = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);

			var rcnWithOnlyAdjustedOutPackages = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse.PK);
			var adjustedOutPackageState = Helper.CreatePackageState(rcnWithOnlyAdjustedOutPackages, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: receiveTransportationUnit);
			adjustedOutPackageState.WPS_WL_LastLocation = location.PK;

			var rcnWithBookedPackages = Helper.CreateReceiveConsignment("RCN3", "STD", warehouse.PK);
			var bookedPackageState = Helper.CreatePackageState(rcnWithBookedPackages, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked);
			var departedPackageState = Helper.CreatePackageState(rcnWithBookedPackages, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Departed, receiveTransportationUnit, dispatchConsignment: dcn, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			departedPackageState.WPS_WL_LastLocation = location.PK;
			departedPackageState.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState.WPS_IsSecure = true;
			departedPackageState.WPS_SecurityStatus = "SEC";

			var rcnWithOnlyDepartedPackages = Helper.CreateReceiveConsignment("RCN4", "STD", warehouse.PK);
			var departedPackageState3 = Helper.CreatePackageState(rcnWithOnlyDepartedPackages, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Departed, receiveTransportationUnit, dispatchConsignment: dcn, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			departedPackageState3.WPS_WL_LastLocation = location.PK;
			departedPackageState3.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState3.WPS_IsSecure = true;
			departedPackageState3.WPS_SecurityStatus = "SEC";
			var finalizedPackageState = Helper.CreatePackageState(rcnWithOnlyDepartedPackages, 1, "PKG", "PKG5", TransitWarehouseStatuses.Codes.Finalized, receiveTransportationUnit, dispatchConsignment: dcn, dispatchLoadList: dispatchLoadList, dispatchUnit: dtu);
			finalizedPackageState.WPS_WL_LastLocation = location.PK;
			finalizedPackageState.WPS_WL_ReceiveLocation = location.PK;
			finalizedPackageState.WPS_IsSecure = true;
			finalizedPackageState.WPS_SecurityStatus = "SEC";
			var adjustedOutPackageState2 = Helper.CreatePackageState(rcnWithOnlyDepartedPackages, 1, "PKG", "PKG5", TransitWarehouseStatuses.Codes.AdjustedOut, receiveUnit: receiveTransportationUnit);
			adjustedOutPackageState2.WPS_WL_LastLocation = location.PK;

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcnWithNoPackages, rcnWithOnlyAdjustedOutPackages, rcnWithBookedPackages, rcnWithOnlyDepartedPackages);

			var departedFlag = WhsTransitDispatchConsignmentFilterBusinessObject.Codes.AllPackagesDeparted;
			var notDepartedFlag = WhsTransitDispatchConsignmentFilterBusinessObject.Codes.NotAllPackagesDeparted;
			var allFlag = WhsTransitDispatchConsignmentFilterBusinessObject.Codes.All;

			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.AllPackagesDeparted];
			filter.IsActive = true;
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			AssertContainsExactElementsInAnyOrder(new[] { departedFlag, notDepartedFlag, allFlag }, filter.List.Cast<CodeDescriptionPair>().Select(s => s.Code));
			filter.Property = departedFlag;
			Asserter.AssertMatches("Return consignments with only departed packages, ignoring Adjusted Out packages", filters.Filter, rcnWithOnlyDepartedPackages);

			filter.Property = notDepartedFlag;
			Asserter.AssertMatches("Return consignments with non departed packages", filters.Filter, rcnWithNoPackages, rcnWithOnlyAdjustedOutPackages, rcnWithBookedPackages);

			filter.Property = allFlag;
			Asserter.AssertMatches("Return all consignments", filters.Filter, rcnWithNoPackages, rcnWithOnlyAdjustedOutPackages, rcnWithBookedPackages, rcnWithOnlyDepartedPackages);
		}

		#endregion

		#region TestAdditionalReference

		public void TestAdditionalReference()
		{
			var warehouse1 = Helper.CreateTransitWarehouseInCurrentBranch();

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse1.PK, jobID: "1");
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse1.PK, jobID: "2");
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", "STD", warehouse1.PK, jobID: "3");
			var rcn4 = Helper.CreateReceiveConsignment("RCN4", "STD", warehouse1.PK, jobID: "4");
			Helper.CreateAdditionalReference(rcn1, "Ref1");
			Helper.CreateAdditionalReference(rcn1, "Ref2");
			Helper.CreateAdditionalReference(rcn2, "Ref2");
			Helper.CreateAdditionalReference(rcn3, "Re");

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcn1, rcn2, rcn3, rcn4);
			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.AdditionalReference];
			filter.IsActive = true;
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "Ref";
			Asserter.AssertMatches("Must only return consignments with an Additional Reference starting with Ref", filters.Filter, rcn1, rcn2);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "Ref1";
			Asserter.AssertMatches("Must only return consignments with Additional Reference Ref1", filters.Filter, rcn1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "Re";
			Asserter.AssertMatches("Must only return consignments with Additional Reference Re", filters.Filter, rcn3);

			filter.Property = "";
			Asserter.AssertMatches("Must return all consignments", filters.Filter, rcn1, rcn2, rcn3, rcn4);
		}

		#endregion

		#region Organisations

		public void TestConsigneeCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var consignee1 = Helper.CreateClient("LCI1");
			var consignee2 = Helper.CreateClient("LCI2");
			var consignor = Helper.CreateClient("LCE1");
			var bookingParty = Helper.CreateClient("BKD1");

			var rcn1a = Helper.CreateReceiveConsignment("RCN1a", warehouse.PK, bookingParty, consignor, consignee1);
			var rcn1b = Helper.CreateReceiveConsignment("RCN1b", warehouse.PK, bookingParty, consignor, consignee1);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK, bookingParty, consignor, consignee2);
			var rcnWithNoOrg = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcn1a, rcn1b, rcn2, rcnWithNoOrg);
			var bkdFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.ConsigneeCompanyName];
			bkdFilter.IsActive = true;
			bkdFilter.Property = consignee1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, bkdFilter.Category);
			Asserter.AssertMatches("Must only return consignments for LCI1", filters.Filter, rcn1a, rcn1b);

			bkdFilter.Property = consignee2.OH_FullName;
			Asserter.AssertMatches("Must only return consignments for LCI2", filters.Filter, rcn2);

			bkdFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, rcn1a, rcn1b, rcn2, rcnWithNoOrg);
		}

		public void TestConsignorCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var consignor1 = Helper.CreateClient("LCE1");
			var consignor2 = Helper.CreateClient("LCE2");
			var consignee = Helper.CreateClient("LCI1");
			var bookingParty = Helper.CreateClient("BKD1");

			var rcn1a = Helper.CreateReceiveConsignment("RCN1a", warehouse.PK, bookingParty, consignor1, consignee);
			var rcn1b = Helper.CreateReceiveConsignment("RCN1b", warehouse.PK, bookingParty, consignor1, consignee);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK, bookingParty, consignor2, consignee);
			var rcnWithNoOrg = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcn1a, rcn1b, rcn2, rcnWithNoOrg);
			var bkdFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.ConsignorCompanyName];
			bkdFilter.IsActive = true;
			bkdFilter.Property = consignor1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, bkdFilter.Category);
			Asserter.AssertMatches("Must only return consignments for LCE1", filters.Filter, rcn1a, rcn1b);

			bkdFilter.Property = consignor2.OH_FullName;
			Asserter.AssertMatches("Must only return consignments for LCE2", filters.Filter, rcn2);

			bkdFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, rcn1a, rcn1b, rcn2, rcnWithNoOrg);
		}

		public void TestCompanyNameFilter()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var bookingParty = Helper.CreateClient("BKD1");
			var consignee = Helper.CreateClient("LCI1");
			var consignor = Helper.CreateClient("LCE1");
			var arrivalCTO = Helper.CreateClient("CTO");

			var rcnWithAddress = Helper.CreateReceiveConsignment("RCN1", warehouse.PK, bookingParty, consignor, consignee, arrivalCTO: arrivalCTO);
			var rcnWithCompanyOverride = Helper.CreateReceiveConsignment("RCN2", warehouse.PK, null, consignor, consignee);
			var rcnWithNoOrg = Helper.CreateReceiveConsignment("RCN3", warehouse.PK, null, consignor, consignee);
			var rcnWithBlankJDA = Helper.CreateReceiveConsignment("RCN4", warehouse.PK, null, consignor, consignee);
			var rcnWithBlankJDAOverride = Helper.CreateReceiveConsignment("RCN5", warehouse.PK, null, consignor, consignee);
			var rcnWithOtherBlankAddresses = Helper.CreateReceiveConsignment("RCN6", warehouse.PK, null, null, null);
			var filters = GetNewFilterStripBusinessObject();

			Helper.CreateJobDocAddress(rcnWithBlankJDA, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			Helper.CreateJobDocAddressWithOverride(rcnWithBlankJDAOverride, DocAddressTypes.Codes.BookingPartyDocumentaryAddress);
			Helper.CreateJobDocAddressWithOverride(rcnWithCompanyOverride, DocAddressTypes.Codes.BookingPartyDocumentaryAddress, "BKD1");

			Helper.CreateJobDocAddress(rcnWithOtherBlankAddresses, DocAddressTypes.Codes.ClientRequestedBillingParty);
			Helper.CreateJobDocAddressWithOverride(rcnWithOtherBlankAddresses, DocAddressTypes.Codes.ConsigneeDocumentaryAddress);
			Helper.CreateJobDocAddressWithOverride(rcnWithOtherBlankAddresses, DocAddressTypes.Codes.BookingPartyDocumentaryAddress, "BKD1");

			Factory.Save();

			Asserter.AddToScope(rcnWithAddress, rcnWithCompanyOverride, rcnWithNoOrg, rcnWithBlankJDA, rcnWithBlankJDAOverride, rcnWithOtherBlankAddresses);
			var bkdFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.BookingPartyCompanyName];
			bkdFilter.IsActive = true;
			bkdFilter.Property = bookingParty.OH_FullName;
			AssertEquals(FilterCategories.Organisations, bkdFilter.Category);
			Asserter.AssertMatches("Must only return consignments for BKD1", filters.Filter, rcnWithAddress, rcnWithCompanyOverride, rcnWithOtherBlankAddresses);

			bkdFilter.Property = consignee.OH_FullName;
			Asserter.AssertMatches("Must filter on address type", filters.Filter);

			bkdFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, rcnWithAddress, rcnWithNoOrg, rcnWithBlankJDA, rcnWithBlankJDAOverride, rcnWithCompanyOverride, rcnWithOtherBlankAddresses);

			bkdFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("Must return consignments with no company", filters.Filter, rcnWithNoOrg, rcnWithBlankJDA, rcnWithBlankJDAOverride);
		}

		public void TestCTOCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var ctoCompany1 = Helper.CreateClient("ACT1");
			var ctoCompany2 = Helper.CreateClient("ACT2");

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK, arrivalCTO: ctoCompany1);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK, arrivalCTO: ctoCompany2);
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcn1, rcn2, rcn3);
			var ctoFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.CTOCompanyName];
			ctoFilter.IsActive = true;
			ctoFilter.Property = ctoCompany1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, ctoFilter.Category);

			ctoFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, rcn1, rcn2, rcn3);

			ctoFilter.Property = ctoCompany1.OH_FullName;
			Asserter.AssertMatches("Must only return consignment for CTO1", filters.Filter, rcn1);

			ctoFilter.Property = ctoCompany2.OH_FullName;
			Asserter.AssertMatches("Must only return consignment for CTO2", filters.Filter, rcn2);
		}

		public void TestBookingPartyCompanyName()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var bookingParty1 = Helper.CreateClient("BKD1");
			var bookingParty2 = Helper.CreateClient("BKD2");
			var consignee = Helper.CreateClient("LCI1");
			var consignor = Helper.CreateClient("LCE2");

			var rcn1a = Helper.CreateReceiveConsignment("RCN1a", warehouse.PK, bookingParty1, consignor, consignee);
			var rcn1b = Helper.CreateReceiveConsignment("RCN1b", warehouse.PK, bookingParty1, consignor, consignee);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK, bookingParty2, consignor, consignee);
			var rcnWithNoOrg = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcn1a, rcn1b, rcn2, rcnWithNoOrg);
			var bkdFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.BookingPartyCompanyName];
			bkdFilter.IsActive = true;
			bkdFilter.Property = bookingParty1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, bkdFilter.Category);
			Asserter.AssertMatches("Must only return consignments for BKD1", filters.Filter, rcn1a, rcn1b);

			bkdFilter.Property = bookingParty2.OH_FullName;
			Asserter.AssertMatches("Must only return consignments for BKD2", filters.Filter, rcn2);

			bkdFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, rcn1a, rcn1b, rcn2, rcnWithNoOrg);
		}

		public void TestBillingCompanyName_FromJobDocAddress()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var billing1 = Helper.CreateClient("CRB1");
			var billing2 = Helper.CreateClient("CRB2");
			var consignor = Helper.CreateClient("LCE");
			var consignee = Helper.CreateClient("LCI");
			var bookingParty = Helper.CreateClient("BKD");

			var rcn1a = Helper.CreateReceiveConsignment("RCN1a", warehouse.PK, bookingParty, consignor, consignee);
			var rcn1b = Helper.CreateReceiveConsignment("RCN1b", warehouse.PK, bookingParty, consignor, consignee);
			var rcnWithCompanyOverride = Helper.CreateReceiveConsignment("RCN1c", warehouse.PK, bookingParty, consignor, consignee);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK, bookingParty, consignor, consignee);
			var rcnWithNoOrg = Helper.CreateReceiveConsignment("RCN3", warehouse.PK, bookingParty, consignor, consignee);
			var rcnWithEmptyJDA = Helper.CreateReceiveConsignment("RCN4", warehouse.PK, bookingParty, consignor, consignee);
			Helper.CreateJobDocAddressFromAddress(rcn1a, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(rcn1b, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(rcn2, DocAddressTypes.Codes.ClientRequestedBillingParty, billing2.MainAddress);
			Helper.CreateJobDocAddressWithOverride(rcnWithCompanyOverride, DocAddressTypes.Codes.ClientRequestedBillingParty, "CRB1");
			Helper.CreateJobDocAddressWithOverride(rcnWithEmptyJDA, DocAddressTypes.Codes.ClientRequestedBillingParty);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcn1a, rcn1b, rcnWithCompanyOverride, rcn2, rcnWithNoOrg, rcnWithEmptyJDA);
			var crbFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.BillingPartyCompanyName];
			crbFilter.IsActive = true;
			crbFilter.Property = billing1.OH_FullName;
			AssertEquals(FilterCategories.Organisations, crbFilter.Category);
			Asserter.AssertMatches("Must only return consignments for CRB1", filters.Filter, rcn1a, rcn1b, rcnWithCompanyOverride);

			crbFilter.Property = billing2.OH_FullName;
			Asserter.AssertMatches("Must only return consignments for CRB2", filters.Filter, rcn2);

			crbFilter.Property = string.Empty;
			Asserter.AssertMatches("Must return all consignments", filters.Filter, rcn1a, rcn1b, rcnWithCompanyOverride, rcn2, rcnWithNoOrg, rcnWithEmptyJDA);

			crbFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("Must return consignments with no company", filters.Filter, rcnWithNoOrg, rcnWithEmptyJDA);
		}

		public void TestBillingCompanyName_LocalClientFallback()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();

			var billing1 = Helper.CreateClient("CRB1");
			var billing2 = Helper.CreateClient("CRB2");

			var rcnWithCRB = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcnWithCRBAndJobHeader = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var rcnWithCRBAndLocalClient = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var rcnWithNoAddress = Helper.CreateReceiveConsignment("RCN4", warehouse.PK);
			var rcnWithNoCRBAndWithJobHeader = Helper.CreateReceiveConsignment("RCN5", warehouse.PK);
			var rcnWithOnlyLocalClient = Helper.CreateReceiveConsignment("RCN6", warehouse.PK);

			var job1 = new JobHeader.Loader(rcnWithCRBAndJobHeader).TryLoadOrCreate();
			var job2 = new JobHeader.Loader(rcnWithCRBAndLocalClient).TryLoadOrCreate();
			var job3 = new JobHeader.Loader(rcnWithNoCRBAndWithJobHeader).TryLoadOrCreate();
			var job4 = new JobHeader.Loader(rcnWithOnlyLocalClient).TryLoadOrCreate();
			job1.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job2.JH_OA_LocalChargesAddr = billing2.MainAddress.PK;
			job3.JH_OA_LocalChargesAddr = ZGuid.Empty;
			job4.JH_OA_LocalChargesAddr = billing2.MainAddress.PK;

			Helper.CreateJobDocAddressFromAddress(rcnWithCRB, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(rcnWithCRBAndJobHeader, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			Helper.CreateJobDocAddressFromAddress(rcnWithCRBAndLocalClient, DocAddressTypes.Codes.ClientRequestedBillingParty, billing1.MainAddress);
			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			AssertEquals(job1.JH_OA_LocalChargesAddr, ZGuid.Empty);
			AssertEquals(job3.JH_OA_LocalChargesAddr, ZGuid.Empty);

			Asserter.AddToScope(rcnWithCRB, rcnWithCRBAndJobHeader, rcnWithCRBAndLocalClient, rcnWithNoAddress, rcnWithNoCRBAndWithJobHeader, rcnWithOnlyLocalClient);
			var bkdFilter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.BillingPartyCompanyName];
			AssertEquals(FilterCategories.Organisations, bkdFilter.Category);
			bkdFilter.IsActive = true;
			bkdFilter.Property = billing1.OH_FullName;
			bkdFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Asserter.AssertMatches("Must fallback to consignments with billing doc address and no job header", filters.Filter, rcnWithCRB);

			bkdFilter.Property = billing2.OH_FullName;
			Asserter.AssertMatches("Must only return consignments with billing local client", filters.Filter, rcnWithCRBAndLocalClient, rcnWithOnlyLocalClient);

			bkdFilter.Property = null;
			bkdFilter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			Asserter.AssertMatches("Must only return consignments with no billing party", filters.Filter, rcnWithNoAddress, rcnWithCRBAndJobHeader, rcnWithNoCRBAndWithJobHeader);

			bkdFilter.Property = null;
			bkdFilter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			Asserter.AssertMatches("Must only return consignments with a billing party", filters.Filter, rcnWithCRB, rcnWithCRBAndLocalClient, rcnWithOnlyLocalClient);
		}

		#endregion

		#region Billing

		public void TestJobInvoicingStatusFilter()
		{
			var warehouse = Helper.CreateTransitWarehouseInCurrentBranch();
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			Asserter.AddToScope(rcn1);

			var filters = GetNewFilterStripBusinessObject();
			AssertNotNull(filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.InvoiceStatus]);
			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.InvoiceStatus];

			var job = new JobHeader.Loader(rcn1).TryLoadOrCreate();
			AssertEquals("Precondition", JobHeaderStatus.Working.Code, job.JH_Status);

			Factory.Save();

			filter.Property = JobHeaderStatus.Working.Code;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.IsActive = true;

			Asserter.AssertMatches("Should find the RCN with the given status", filter, rcn1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;

			Asserter.AssertMatches("Should exclude the RCN with the given status", filter);
		}

		public void TestInvoicedChargesFilter()
		{
			var currentBranch = GlbBranch.CurrentBranch;
			var code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR"));

			var address = Factory.NewWithValidTestData<OrgAddress>();
			var warehouse = Helper.CreateWarehouse("WH1", address, currentBranch);
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var job = new JobHeader.Loader(rcn).TryLoadOrCreate();
			job.JH_GB = currentBranch.PK;

			var filters = GetNewFilterStripBusinessObject();
			AssertNotNull(filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.InvoicedChargesBilling]);
			var invoicedChargesFilter = (ModuleFlagsFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.InvoicedChargesBilling];
			Asserter.AddToScope(rcn);

			Factory.Save();

			invoicedChargesFilter.IsActive = true;
			invoicedChargesFilter["No Charges"] = true;
			Asserter.AssertMatches("Must return consignments with no charges", invoicedChargesFilter, rcn);

			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = code.PK;
			charge.JR_GB = currentBranch.PK;
			charge.JR_LocalSellAmt = 10m;
			Factory.Save();

			Asserter.AssertMatches("All consignments have charges and should not be returned", invoicedChargesFilter);
		}

		class WhsItemReceiveConsignmentFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<WhsItemReceiveConsignment>
		{
			#region Implementation

			protected override WhsItemReceiveConsignment GetNewBusinessObjectForFilterCollection()
			{
				var poke = Warehouse;
				var rcn = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
				rcn.WRC_WW_IntendedWarehouse = Warehouse.PK;
				return rcn;
			}

			protected override ModuleIdentifier FilterStripModuleID
			{
				get { return ModuleIDs.WhsTransitReceiveConsignment; }
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

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, direction: "IMP");
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse.PK, direction: "EXP");
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", "STD", warehouse.PK);

			var filters = GetNewFilterStripBusinessObject();

			Factory.Save();

			Asserter.AddToScope(rcn1, rcn2, rcn3);
			var filter = (ModuleTextFilter)filters.ModuleFilters[WhsTransitReceiveConsignmentFilterBusinessObject.Schema.Direction];
			filter.IsActive = true;
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "IMP";
			Asserter.AssertMatches("Must only return consignments with direction with IMP", filters.Filter, rcn1);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "EXP";
			Asserter.AssertMatches("Must return the consignment with direction with EXP", filters.Filter, rcn2);

			filter.Property = "";
			Asserter.AssertMatches("Must return all consignments", filters.Filter, rcn1, rcn2, rcn3);
		}

		#endregion

		#region TestOverrideFilters

		public void TestGetModuleFilterThatOverridesAllOtherFilters()
		{
			var filters = new WhsTransitReceiveConsignmentFilterBusinessObjectForTest();
			AssertNull(filters.GetModuleFilterThatOverridesAllOtherFiltersForTest());
		}

		class WhsTransitReceiveConsignmentFilterBusinessObjectForTest : WhsTransitReceiveConsignmentFilterBusinessObject
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

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, direction: "IMP");
			var job1 = new JobHeader.Loader(rcn1).TryLoadOrCreate();
			job1.JH_ProfitLossReasonCode = "ND1";

			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse.PK, direction: "EXP");
			var job2 = new JobHeader.Loader(rcn2).TryLoadOrCreate();
			job2.JH_ProfitLossReasonCode = "CD1";

			var rcn3 = Helper.CreateReceiveConsignment("RCN3", "STD", warehouse.PK);
			var job3 = new JobHeader.Loader(rcn3).TryLoadOrCreate();
			job3.JH_ProfitLossReasonCode = string.Empty;
			Factory.Save();

			Asserter.AddToScope(rcn1, rcn2, rcn3);

			var filters = GetNewFilterStripBusinessObject();
			var profitLossReasonFilter = (ModuleTextFilter)filters["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for rcn1.", profitLossReasonFilter, rcn1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for rcn1.", profitLossReasonFilter, rcn1);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";

			Asserter.AssertMatches("Has a Match for rcn1 and rcn2", profitLossReasonFilter, rcn1, rcn2);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for rcn2 and rcn3", profitLossReasonFilter, rcn2, rcn3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";

			Asserter.AssertMatches("Has a Match for rcn2 and rcn3", profitLossReasonFilter, rcn2, rcn3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for rcn2 and rcn3", profitLossReasonFilter, rcn2, rcn3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "";

			Asserter.AssertMatches("Has a Match for rcn3", profitLossReasonFilter, rcn3);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";

			Asserter.AssertMatches("Has a Match for rcn1 and rcn2", profitLossReasonFilter, rcn1, rcn2);
		}

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WhsTransitReceiveConsignmentFilterBusinessObject();
		}

		protected FilterStripAsserter<WhsItemReceiveConsignment> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<WhsItemReceiveConsignment>(Factory, rcn => rcn.WRC_ConsignmentID)); }
		}
		FilterStripAsserter<WhsItemReceiveConsignment> asserter;

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
