using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemPackageStateTest : TestCaseWithFactory
	{
		#region TestReadOnlyFields

		static readonly string[] ReadOnlyProperties =
		{
			"WPS_KP_Package",
			"WPS_WL_LastLocation",
			"WPS_Status",
			"WPS_WRH_TransitReceiveHeader",
			"WPS_WRC_TransitReceiveConsignment",
			"WPS_WL_ReceiveLocation",
			"WPS_WDC_TransitDispatchConsignment",
			"WPS_WDH_TransitDispatchHeader",
			"WPS_WRP_ReceiveExpectedPacking",
			"WPS_IsHandlingUnit",
			"WPS_WDL_LoadList",
			"WPS_AdjustedOut",
			"WPS_LoadedTime",
			"WPS_UnloadedTime",
			"WPS_CustomsStatus",
			"WPS_SystemCreateTimeUtc",
			"WPS_SystemCreateUser",
			"WPS_SystemLastEditTimeUtc",
			"WPS_SystemLastEditUser",
			"WPS_RemoveFromDTU",
		};

		public void TestReadOnly()
		{
			var wps = Factory.New<WhsItemPackageState>();
			foreach (var propertyName in ReadOnlyProperties)
			{
				var readOnly = ((ZPropertyInfo)typeof(WhsItemPackageState).GetProperty(propertyName + "Info").GetValue(wps)).ReadOnly;
				var readOnlyAction = ActionFieldAttribute.Get(typeof(WhsItemPackageState).GetProperty(propertyName))?.ReadOnly;
				AssertEquals(propertyName, true, readOnly);
				AssertEquals(propertyName, true, readOnlyAction);
			}
		}

		#endregion

		#region TestPackage

		public void TestPackage()
		{
			var package = Factory.New<PkgPackage>();
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_KP_Package = package.PK;
			AssertEquals(package, packageState.Package);
		}

		#endregion

		#region TestReceiveConsignment

		public void TestReceiveConsignment()
		{
			var consignment = Factory.New<WhsItemReceiveConsignment>();
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_WRC_TransitReceiveConsignment = consignment.PK;
			AssertEquals(consignment, packageState.ReceiveConsignment);
		}

		#endregion

		#region TestReceiveConsignmentID

		public void TestReceiveConsignmentID()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignee = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment1 = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveConsignment2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState1 = Helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState2 = Helper.CreatePackageState(receiveConsignment2, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState3 = Helper.CreatePackageState(receiveConsignment2, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState4 = Helper.CreatePackageState(receiveConsignment2, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState5 = Helper.CreatePackageState(receiveTransportationUnit, 1, "PKG", "PKG5", TransitWarehouseStatuses.Codes.Arrived);
			var standalonePackage = Helper.CreatePackageState(receiveConsignment2, 1, "PKG", "PKG6", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var handlingUnit1 = Helper.CreatePackageHandlingUnit();
			var huWithChildrenInMultipleRCNs = Helper.CreateHandlingUnitPackage("HU1", handlingUnit1, receiveTransportationUnit);
			var handlingUnit2 = Helper.CreatePackageHandlingUnit();
			var huWithNoChildren = Helper.CreateHandlingUnitPackage("HU2", handlingUnit2, receiveTransportationUnit);
			var handlingUnit3 = Helper.CreatePackageHandlingUnit();
			var huWithChildrenInSameRCN = Helper.CreateHandlingUnitPackage("HU3", handlingUnit3, receiveTransportationUnit);

			Helper.PackPackageIntoHandlingUnit(huWithChildrenInMultipleRCNs, childPackageState1, ZDateTimeOffset.Now, "ABC", huWithChildrenInMultipleRCNs);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenInMultipleRCNs, childPackageState2, ZDateTimeOffset.Now, "ABC", huWithChildrenInMultipleRCNs);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenInSameRCN, childPackageState3, ZDateTimeOffset.Now, "ABC", huWithChildrenInSameRCN);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenInSameRCN, childPackageState4, ZDateTimeOffset.Now, "ABC", huWithChildrenInSameRCN);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenInSameRCN, childPackageState5, ZDateTimeOffset.Now, "ABC", huWithChildrenInSameRCN);
			Factory.Save();

			AssertEquals("When child package states of handling unit are in different receive consignments then ReceiveConsignmentID is Many", "Many", huWithChildrenInMultipleRCNs.ReceiveConsignmentID);
			AssertEquals("When child package states of handling unit are in same receive consignment then ReceiveConsignmentID is Receive Consignment ID", "RC2", huWithChildrenInSameRCN.ReceiveConsignmentID);
			AssertEquals("When handling unit has no child package then ReceiveConsignmentID is empty", "", huWithNoChildren.ReceiveConsignmentID);
			AssertEquals("Should return ReceiveConsignmentID if the package state is not handling unit", "RC2", standalonePackage.ReceiveConsignmentID);
		}

		#endregion

		#region TestReceiveTransportationUnit

		public void TestReceiveTransportationUnit()
		{
			var receiveUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_WRH_TransitReceiveHeader = receiveUnit.PK;
			AssertEquals(receiveUnit, packageState.ReceiveTransportationUnit);
		}

		#endregion

		#region TestReceiveASN

		public void TestReceiveASN()
		{
			var recieveASN = Factory.New<WhsItemReceiveASN>();
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_WRP_ReceiveExpectedPacking = recieveASN.PK;
			AssertEquals(recieveASN, packageState.ReceiveASN);
		}

		#endregion

		#region TestDispatchTransportationUnit

		public void TestDispatchTransportationUnit()
		{
			var dipatchUnit = Factory.New<WhsItemDispatchTransportationUnit>();
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_WDH_TransitDispatchHeader = dipatchUnit.PK;
			AssertEquals(dipatchUnit, packageState.DispatchTransportationUnit);
		}

		#endregion

		#region TestDispatchConsignment

		public void TestDispatchConsignment()
		{
			var consignment = Factory.New<WhsItemDispatchConsignment>();
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_WDC_TransitDispatchConsignment = consignment.PK;

			AssertEquals(consignment, packageState.DispatchConsignment);
		}

		#endregion

		#region TestDispatchConsignmentID

		public void TestDispatchConsignmentID()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignee = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dispatchConsignment1 = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var dispatchConsignment2 = Helper.CreateDispatchConsignment("DC2", warehouse.PK);
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DL1", warehouse.PK);

			var childPackageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment1, dispatchUnit: dispatchTransportationUnit, dispatchLoadList: loadList);
			var childPackageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment2, dispatchUnit: dispatchTransportationUnit, dispatchLoadList: loadList);
			var childPackageState3 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment2, dispatchUnit: dispatchTransportationUnit, dispatchLoadList: loadList);
			var childPackageState4 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment2, dispatchUnit: dispatchTransportationUnit, dispatchLoadList: loadList);
			var childPackageState5 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG5", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var standalonePackage = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG6", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment2, dispatchUnit: dispatchTransportationUnit, dispatchLoadList: loadList);
			childPackageState1.WPS_WL_LastLocation = location.PK;
			childPackageState2.WPS_WL_LastLocation = location.PK;
			childPackageState3.WPS_WL_LastLocation = location.PK;
			childPackageState4.WPS_WL_LastLocation = location.PK;
			standalonePackage.WPS_WL_LastLocation = location.PK;

			var handlingUnit1 = Helper.CreatePackageHandlingUnit();
			var huWithChildrenInMultipleDCNs = Helper.CreateHandlingUnitPackage("HU1", handlingUnit1, receiveTransportationUnit);
			var handlingUnit2 = Helper.CreatePackageHandlingUnit();
			var huWithNoChildren = Helper.CreateHandlingUnitPackage("HU2", handlingUnit2, receiveTransportationUnit);
			var handlingUnit3 = Helper.CreatePackageHandlingUnit();
			var huWithChildrenInSameDCN = Helper.CreateHandlingUnitPackage("HU3", handlingUnit3, receiveTransportationUnit);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenInMultipleDCNs, childPackageState1, ZDateTimeOffset.Now, "ABC", huWithChildrenInMultipleDCNs);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenInMultipleDCNs, childPackageState2, ZDateTimeOffset.Now, "ABC", huWithChildrenInMultipleDCNs);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenInSameDCN, childPackageState3, ZDateTimeOffset.Now, "ABC", huWithChildrenInSameDCN);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenInSameDCN, childPackageState4, ZDateTimeOffset.Now, "ABC", huWithChildrenInSameDCN);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenInSameDCN, childPackageState5, ZDateTimeOffset.Now, "ABC", huWithChildrenInSameDCN);
			Factory.Save();

			AssertEquals("When child package states of handling unit are in different dispatch consignments then DispatchConsignmentID is Many", "Many", huWithChildrenInMultipleDCNs.DispatchConsignmentID);
			AssertEquals("When child package states of handling unit are in same dispatch consignment then DispatchConsignmentID is Dispatch Consignment ID", "DC2", huWithChildrenInSameDCN.DispatchConsignmentID);
			AssertEquals("When handling unit has no child package then DispatchConsignmentID is empty", "", huWithNoChildren.DispatchConsignmentID);
			AssertEquals("Should return DispatchConsignmentID if the package state is not handling unit", "DC2", standalonePackage.DispatchConsignmentID);
		}

		#endregion

		#region TestPortReferenceNumber

		public void TestPortReferenceNumber()
		{
			var packageState = Factory.New<WhsItemPackageState>();
			AssertEquals(0, (packageState as ITransitPackage).Numbers.Count());

			var consignment = Factory.New<WhsItemReceiveConsignment>();

			packageState.WPS_WRC_TransitReceiveConsignment = consignment.PK;
			AssertEquals(0, (packageState as ITransitPackage).Numbers.Count());

			var panReference = Factory.New<ICusEntryNumber>();
			panReference.CE_EntryType = TransitWarehousePortReferenceTypes.Codes.PortAuthority;
			panReference.CE_Category = TransitWarehouseReferenceCategories.Codes.PortReference;
			panReference.CE_EntryNum = "ABC";
			panReference.Parent = consignment;

			AssertEquals(1, (packageState as ITransitPackage).Numbers.Count());
			AssertEquals(TransitWarehousePortReferenceTypes.Codes.PortAuthority, (packageState as ITransitPackage).Numbers.First().CE_EntryType);
			AssertEquals("ABC", (packageState as ITransitPackage).Numbers.First().CE_EntryNum);

			var penReference = Factory.New<ICusEntryNumber>();
			penReference.CE_EntryType = TransitWarehousePortReferenceTypes.Codes.PortExport;
			penReference.CE_Category = TransitWarehouseReferenceCategories.Codes.PortReference;
			penReference.CE_EntryNum = "DEF";
			penReference.Parent = consignment;

			AssertEquals(1, (packageState as ITransitPackage).Numbers.Count());
			AssertEquals(TransitWarehousePortReferenceTypes.Codes.PortExport, (packageState as ITransitPackage).Numbers.First().CE_EntryType);
			AssertEquals("DEF", (packageState as ITransitPackage).Numbers.First().CE_EntryNum);
		}

		#endregion

		#region TestIAdditionalReferenceNumberTypeProviderMembers

		public void TestIAdditionalReferenceNumberTypeProviderMembers()
		{
			var packageState1 = Factory.New<WhsItemPackageState>();
			var packageState2 = Factory.New<WhsItemPackageState>();
			var bkpRefForPS1 = packageState1.AdditionalReferenceNumbers.AddNew();
			bkpRefForPS1.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference;
			bkpRefForPS1.CE_EntryNum = "ABC";

			var bkpRefForPS2 = packageState2.AdditionalReferenceNumbers.AddNew();
			bkpRefForPS2.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.ConsignmentNumber;
			bkpRefForPS2.CE_EntryNum = "ABC1";
			AssertEquals(false, bkpRefForPS1.CE_EntryTypeInfo.HasErrors());
			AssertEquals(true, bkpRefForPS2.CE_EntryTypeInfo.HasErrors());
		}

		#endregion

		#region IEDocsPluginHostDecider Memebers

		public void TestHostBusinessEntity()
		{
			var asn = Factory.New<WhsItemReceiveASN>();
			var rcn = Factory.New<WhsItemReceiveConsignment>();
			var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
			var dcn = Factory.New<WhsItemDispatchConsignment>();
			var dtu = Factory.New<WhsItemDispatchTransportationUnit>();
			var dll = Factory.New<WhsItemDispatchLoadList>();

			var packageStateWithASN = Factory.New<WhsItemPackageState>();
			var packageStateWithRCN = Factory.New<WhsItemPackageState>();
			var packageStateWithRTU = Factory.New<WhsItemPackageState>();
			var packageStateWithDCN = Factory.New<WhsItemPackageState>();
			var packageStateWithDTU = Factory.New<WhsItemPackageState>();
			var packageStateWithDLL = Factory.New<WhsItemPackageState>();
			var packageStateWithAll = Factory.New<WhsItemPackageState>();

			var packageWithASN = Factory.New<PkgPackage>();
			var packageWithRCN = Factory.New<PkgPackage>();
			var packageWithRTU = Factory.New<PkgPackage>();
			var packageWithDCN = Factory.New<PkgPackage>();
			var packageWithDTU = Factory.New<PkgPackage>();
			var packageWithDLL = Factory.New<PkgPackage>();
			var packageWithAll = Factory.New<PkgPackage>();

			packageStateWithASN.WPS_WRP_ReceiveExpectedPacking = asn.PK;
			packageStateWithRCN.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			packageStateWithRTU.WPS_WRH_TransitReceiveHeader = rtu.PK;
			packageStateWithDCN.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			packageStateWithDTU.WPS_WDH_TransitDispatchHeader = dtu.PK;
			packageStateWithDLL.WPS_WDL_LoadList = dll.PK;

			packageStateWithAll.WPS_WRP_ReceiveExpectedPacking = asn.PK;
			packageStateWithAll.WPS_WRC_TransitReceiveConsignment = rcn.PK;
			packageStateWithAll.WPS_WRH_TransitReceiveHeader = rtu.PK;
			packageStateWithAll.WPS_WDC_TransitDispatchConsignment = dcn.PK;
			packageStateWithAll.WPS_WDH_TransitDispatchHeader = dtu.PK;
			packageStateWithAll.WPS_WDL_LoadList = dll.PK;

			packageStateWithASN.WPS_KP_Package = packageWithASN.PK;
			packageStateWithRCN.WPS_KP_Package = packageWithRCN.PK;
			packageStateWithRTU.WPS_KP_Package = packageWithRTU.PK;
			packageStateWithDCN.WPS_KP_Package = packageWithDCN.PK;
			packageStateWithDTU.WPS_KP_Package = packageWithDTU.PK;
			packageStateWithDLL.WPS_KP_Package = packageWithDLL.PK;
			packageStateWithAll.WPS_KP_Package = packageWithAll.PK;

			AssertRelatedObjectOnHostedEntity(packageStateWithASN, new[] { asn }, packageWithASN);
			AssertRelatedObjectOnHostedEntity(packageStateWithRCN, new[] { rcn }, packageWithRCN);
			AssertRelatedObjectOnHostedEntity(packageStateWithRTU, new[] { rtu }, packageWithRTU);
			AssertRelatedObjectOnHostedEntity(packageStateWithDCN, new[] { dcn }, packageWithDCN);
			AssertRelatedObjectOnHostedEntity(packageStateWithDTU, new[] { dtu }, packageWithDTU);
			AssertRelatedObjectOnHostedEntity(packageStateWithDLL, new[] { dll }, packageWithDLL);
			AssertRelatedObjectOnHostedEntity(packageStateWithAll, new BusinessObject[] { asn, rcn, rtu, dcn, dtu, dll }, packageWithAll);
		}

		static void AssertRelatedObjectOnHostedEntity(WhsItemPackageState packageState, BusinessObject[] expectedRelatedBizOs, PkgPackage expectedPackage)
		{
			var package = (PkgPackage)((IEDocsPluginHostDecider)packageState).HostBusinessEntity;
			AssertEquals(expectedPackage, package);
			AssertContainsExactElementsInAnyOrder(expectedRelatedBizOs, package.GetRelatedBusinessObjects());
		}

		#endregion

		#region Statuses

		public void TestStatuses()
		{
			var packageState = Factory.New<WhsItemPackageState>();

			packageState.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
			Assert(packageState.IsLoaded);

			packageState.WPS_Status = TransitWarehouseStatuses.Codes.Picked;
			Assert(packageState.IsPicked);
		}

		#endregion

		#region TestLastLocation

		public void TestLastLocation()
		{
			var location = Factory.New<WhsLocation>();
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_WL_LastLocation = location.PK;
			AssertEquals(location, packageState.LastLocation);
		}

		#endregion

		#region TestParentJobNumber

		public void TestParentJobNumber()
		{
			var packageState = Factory.New<WhsItemPackageState>();
			AssertEquals(0, packageState.AdditionalReferenceNumbers.Count);
			packageState.ParentJobNumber = "SHP123";
			var additionalReferenceForBookingParty = packageState.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Single();
			AssertEquals(WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, additionalReferenceForBookingParty.CE_EntryType);
			AssertEquals("SHP123", additionalReferenceForBookingParty.CE_EntryNum);
			AssertEquals("SHP123", packageState.ParentJobNumber);

			packageState.ParentJobNumber = "SHP124";
			AssertEquals(WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, additionalReferenceForBookingParty.CE_EntryType);
			AssertEquals("SHP124", additionalReferenceForBookingParty.CE_EntryNum);
			AssertEquals("SHP124", packageState.ParentJobNumber);

			packageState.ParentJobNumber = "";
			AssertEquals("", packageState.ParentJobNumber);
			var bookingPartyReferencesAfterDeleting = packageState.AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Where(a => a.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			AssertEquals(0, bookingPartyReferencesAfterDeleting.Count());

			var packageState2 = Factory.New<WhsItemPackageState>();
			AssertEquals(0, packageState2.AdditionalReferenceNumbers.Count);

			var emptyParentJobNumber = Factory.New<ICusEntryNumber>();
			emptyParentJobNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference;
			packageState2.AdditionalReferenceNumbers.Add(emptyParentJobNumber);

			AssertEquals(1, packageState2.AdditionalReferenceNumbers.Count);

			var nonEmptyParentJobNumber1 = Factory.New<ICusEntryNumber>();
			emptyParentJobNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference;
			emptyParentJobNumber.CE_EntryNum = "CBA";
			packageState2.AdditionalReferenceNumbers.Add(nonEmptyParentJobNumber1);

			var nonEmptyParentJobNumber2 = Factory.New<ICusEntryNumber>();
			emptyParentJobNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference;
			emptyParentJobNumber.CE_EntryNum = "ABC";
			packageState2.AdditionalReferenceNumbers.Add(nonEmptyParentJobNumber2);

			AssertEquals(3, packageState2.AdditionalReferenceNumbers.Count);
			AssertEquals("ABC", packageState2.ParentJobNumber);
		}

		public void TestParentJobNumber_HandlingUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignee = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: "SHP4321");
			var childPackageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: "SHP1234");
			var childPackageState3 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: "SHP1234");
			var childPackageState4 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, entryNum: "SHP1234");
			var childPackageState5 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG5", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var handlingUnit1 = Helper.CreatePackageHandlingUnit();
			var huWithChildrenInMultipleJobs = Helper.CreateHandlingUnitPackage("HU1", handlingUnit1, receiveTransportationUnit);
			var handlingUnit2 = Helper.CreatePackageHandlingUnit();
			var huWithChildrenNotAssignedToJob = Helper.CreateHandlingUnitPackage("HU2", handlingUnit2, receiveTransportationUnit);
			var handlingUnit3 = Helper.CreatePackageHandlingUnit();
			var huWithChildrenInSameJob = Helper.CreateHandlingUnitPackage("HU3", handlingUnit3, receiveTransportationUnit);
			var handlingUnit4 = Helper.CreatePackageHandlingUnit();
			var huWithoutChild = Helper.CreateHandlingUnitPackage("HU4", handlingUnit3, receiveTransportationUnit);

			Helper.PackPackageIntoHandlingUnit(huWithChildrenInMultipleJobs, childPackageState1, ZDateTimeOffset.Now, "ABC", huWithChildrenInMultipleJobs);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenInMultipleJobs, childPackageState2, ZDateTimeOffset.Now, "ABC", huWithChildrenInMultipleJobs);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenInSameJob, childPackageState3, ZDateTimeOffset.Now, "ABC", huWithChildrenInSameJob);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenInSameJob, childPackageState4, ZDateTimeOffset.Now, "ABC", huWithChildrenInSameJob);
			Helper.PackPackageIntoHandlingUnit(huWithChildrenNotAssignedToJob, childPackageState5, ZDateTimeOffset.Now, "ABC", huWithChildrenNotAssignedToJob);
			Factory.Save();

			AssertEquals("Child packages are in different jobs should return Many", "Many", huWithChildrenInMultipleJobs.ParentJobNumber);
			AssertEquals("Child packages are in same job should return Job Number", "SHP1234", huWithChildrenInSameJob.ParentJobNumber);
			AssertEquals("Child packages do not belong to a job, therefore ParentJobNumber must be empty", "", huWithChildrenNotAssignedToJob.ParentJobNumber);
			AssertEquals("Empty HU's ParentJobNumber must be empty", "", huWithoutChild.ParentJobNumber);
		}

		#endregion

		#region TestHandlingUnit

		public void TestHandlingUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignee = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK, null, null, consignee);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState_Unpacked = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveTransportationUnit);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			var openDivot = Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState_Unpacked, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.UnpackPackageFromHandlingUnit(openDivot, ZDateTimeOffset.Now, "ABC");
			Factory.Save();

			AssertEquals(handlingUnitPackage, childPackageState.HandlingUnit);
			AssertEquals(null, childPackageState_Unpacked.HandlingUnit);
		}

		#endregion

		#region TestBreakDownParentPackageState

		public void TestBreakDownParentPackageState()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var overpackPackageState = Helper.CreateOverpackPackage("OVP-1", receiveConsignment, receiveTransportationUnit, rcn: receiveConsignment);
			var childPackageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "INNER-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			Helper.PackPackageIntoHandlingUnit(overpackPackageState, childPackageState, ZDateTimeOffset.Now, "LWC", ZDateTimeOffset.Now);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveTransportationUnit);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState, ZDateTimeOffset.Now.AddHours(1), "ABC", ZDateTimeOffset.Now.AddHours(1));
			Factory.Save();

			AssertEquals(childPackageState.BreakDownParentPackageState.PK, overpackPackageState.PK);
		}

		#endregion

		#region TestConsignor

		public void TestConsignor_Empty()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignor = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK, null, null, null);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			AssertEquals(ZString.Empty, packageState.Consignor);
		}

		public void TestConsignor()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignor = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK, null, consignor, null);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			AssertEquals(receiveConsignment.ConsignorDocAddress.CompanyName, packageState.Consignor);
		}

		public void TestConsignor_HandlingUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignor = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK, null, consignor, null);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveTransportationUnit);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			Factory.Save();

			AssertEquals(receiveConsignment.ConsignorDocAddress.CompanyName, handlingUnitPackage.Consignor);
		}

		public void TestConsignor_HandlingUnit_NoReceiveConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignor = Helper.CreateClient("ORG1", "Org1");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState = Helper.CreatePackageState(receiveTransportationUnit, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveTransportationUnit);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			Factory.Save();

			AssertNull("Precondition: No receive consignment", childPackageState.ReceiveConsignment);
			AssertEquals(ZString.Empty, handlingUnitPackage.Consignor);
		}

		#endregion

		#region TestConsignee

		public void TestConsignee_Empty()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignor = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK, null, null, null);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			AssertEquals(ZString.Empty, packageState.Consignee);
		}

		public void TestConsignee()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignee = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK, null, null, consignee);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			AssertEquals(receiveConsignment.ConsigneeDocAddress.CompanyName, packageState.Consignee);
		}

		public void TestConsignee_HandlingUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignee = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK, null, null, consignee);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveTransportationUnit);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			Factory.Save();

			AssertEquals(receiveConsignment.ConsigneeDocAddress.CompanyName, handlingUnitPackage.Consignee);
		}

		public void TestConsignee_HandlingUnit_NoReceiveConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var childPackageState = Helper.CreatePackageState(receiveTransportationUnit, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveTransportationUnit);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			Factory.Save();

			AssertNull("Precondition: No receive consignment", childPackageState.ReceiveConsignment);
			AssertEquals(ZString.Empty, handlingUnitPackage.Consignee);
		}

		#endregion

		#region TestInners_NonLabeledInnerQty

		public void TestInners_NonLabeledInnerQty()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignee = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK, null, null, consignee);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var innerPackage1 = Helper.PackingHelper.CreatePackage(packageState.Package, 10, "BOX");
			var innerPackage2 = Helper.PackingHelper.CreatePackage(packageState.Package, 20, "BOX");

			Factory.Save();

			AssertEquals(30, packageState.Inners);
		}

		#endregion

		#region TestLabeledInnerQty

		public void TestInners_LabeledInnerQty()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignee = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK, null, null, consignee);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState_Unpacked = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveTransportationUnit);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);

			var openDivot = Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState_Unpacked, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.UnpackPackageFromHandlingUnit(openDivot, ZDateTimeOffset.Now, "ABC");
			Factory.Save();

			AssertEquals(1, handlingUnitPackage.Inners);
		}

		#endregion

		#region TestLabeledInnerQtyWithInnerPackline

		public void TestInners_LabeledInnerQty_WithInnerPackline()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var consignee = Helper.CreateClient("ORG1", "Org1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK, null, null, consignee);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState = Helper.CreatePackageState(receiveConsignment, 10, "PKG", null, TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveTransportationUnit);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Factory.Save();

			AssertEquals(10, handlingUnitPackage.Inners);
		}

		#endregion

		#region TestHasDangerousGoods

		public void TestHasDangerousGoods()
		{
			var warehouse = Helper.CreateTRWWarehouse();

			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);

			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked);
			AssertEquals(false, packageState.HasDangerousGoods);

			var undgSubstance = Helper.CreateUNDGSubstance("BCDE", "1.5D", "BCDEa");
			undgSubstance.DG_ExceptedQuantityCode = "E1";

			var undgDataItem = Helper.CreateUNDGDataItem(packageState.Package.PK, packageState.Package.TablePrefix, undgSubstance, 2, 3);
			packageState.Package.UNDGs.Add(undgDataItem);
			AssertEquals(true, packageState.HasDangerousGoods);
		}

		#endregion

		#region TestOverpackUNDGs

		public void TestOverpackUNDGs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var overpackPackageState = Helper.CreateOverpackPackage("OVP-1", receiveConsignment, receiveTransportationUnit, rcn: receiveConsignment);
			var childPackageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "INNER-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "INNER-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var undgSubstance = Helper.CreateUNDGSubstance("BCDE", "1.5D", "BCDEa");
			undgSubstance.DG_ExceptedQuantityCode = "E1";
			var undgDataItem = Helper.CreateUNDGDataItem(childPackageState.Package.PK, childPackageState.Package.TablePrefix, undgSubstance, 2, 3);
			childPackageState.Package.UNDGs.Add(undgDataItem);

			var undgSubstance2 = Helper.CreateUNDGSubstance("ABCD", "1.1D", "ABCDa");
			undgSubstance2.DG_ExceptedQuantityCode = "E2";
			var undgDataItem2 = Helper.CreateUNDGDataItem(childPackageState2.Package.PK, childPackageState.Package.TablePrefix, undgSubstance2, 2, 3);
			childPackageState2.Package.UNDGs.Add(undgDataItem2);

			overpackPackageState.Package.AllowLoadingOverpackChildrenUndgs = true;

			Helper.PackPackageIntoHandlingUnit(handlingPackage: overpackPackageState, childPackageState, packedTime: ZDateTimeOffset.Now, packedUser: "LWC", topHandlingUnit: overpackPackageState);
			Helper.PackPackageIntoHandlingUnit(handlingPackage: overpackPackageState, childPackageState2, packedTime: ZDateTimeOffset.Now, packedUser: "LWC", topHandlingUnit: overpackPackageState);

			Factory.Save();

			var childPkgUndgDataItems = childPackageState.Package.UNDGDataItems;
			AssertEquals(1, childPkgUndgDataItems.Count());
			AssertEquals(true, childPkgUndgDataItems.Count(undg => undg.DI_DG == undgSubstance.PK) == 1);

			var childPkg2UndgDataItems = childPackageState2.Package.UNDGDataItems;
			AssertEquals(1, childPkg2UndgDataItems.Count());
			AssertEquals(true, childPkg2UndgDataItems.Count(undg => undg.DI_DG == undgSubstance2.PK) == 1);

			var ovpUndgDataItems = overpackPackageState.Package.UNDGDataItems;
			AssertEquals(2, ovpUndgDataItems.Count());
			AssertEquals(true, ovpUndgDataItems.Count(undg => undg.DI_DG == undgSubstance.PK) == 1);
			AssertEquals(true, ovpUndgDataItems.Count(undg => undg.DI_DG == undgSubstance2.PK) == 1);
		}

		#endregion

		#region TestCalculatedProperty

		public void TestHandlingUnitID()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var childPackageState1 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState2 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var childPackageState3 = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);
			var packageState = Helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveTransportationUnit);
			var cntHandlingUnit = Helper.CreatePackageHandlingUnit();
			var cntHandlingUnitPackage = Helper.CreateHandlingUnitPackage("HU2", cntHandlingUnit, receiveTransportationUnit, unitType: PackageStateUnitType.Codes.SeaContainer);
			var ovpHandlingUnit = Helper.CreatePackageHandlingUnit();
			var ovpHandlingUnitPackage = Helper.CreateHandlingUnitPackage("HU3", ovpHandlingUnit, receiveTransportationUnit, unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment);

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackageState1, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(cntHandlingUnitPackage, childPackageState2, ZDateTimeOffset.Now, "ABC", cntHandlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(ovpHandlingUnitPackage, childPackageState3, ZDateTimeOffset.Now, "ABC", ovpHandlingUnitPackage);

			Factory.Save();

			AssertEquals(childPackageState1.HandlingUnitID, handlingUnitPackage.Package.KP_PackageID);
			AssertEquals(childPackageState2.HandlingUnitID, cntHandlingUnitPackage.Package.KP_PackageID);
			AssertNullOrEmpty(childPackageState3.HandlingUnitID);
			AssertNullOrEmpty(packageState.HandlingUnitID);
		}

		#endregion

		#region TestLastLocation

		public void TestOverriddenAviationSecurityInspectionType()
		{
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;
			AssertEquals(TransitWarehouseSecurityStatuses.Codes.Approved, ((ITransitPackage)packageState).OverriddenAviationSecurityInspectionType);

			packageState.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Overridden;
			AssertEquals(ZString.Empty, ((ITransitPackage)packageState).OverriddenAviationSecurityInspectionType);
		}

		#endregion

		#region EventLogs

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestDepartureEventInPackage()
		{
			var warehouse = Helper.CreateTRWWarehouse("TRW");
			warehouse.WarehouseAddress.OA_City = "Sydney";
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			dtu.WDH_ReferenceNumber = "DTU123";
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			Factory.Save();

			AssertEquals("Precondition: DTU gate out time is empty.", true, dtu.WDH_GateOutTime.IsEmpty);
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.FreightLoaded);

			dtu.WDH_LoadCompleteTime = ZDateTimeOffset.Now.AddDays(-1);
			dtu.IsGatedOut = true;

			AssertEquals("DTU gate out time should be set.", false, dtu.WDH_GateOutTime.IsEmpty);
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.Departed);

			var freightLoadedEventLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.FreightLoaded.Code);
			var packageFreightLoadedLog = packageState.Package.Logs.Find(freightLoadedEventLogQuery).SingleOrDefault();
			AssertNull("Should not create freight loaded log for package", packageFreightLoadedLog);

			var departureEventLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Departure.Code);
			var packageDepartureLog = packageState.Package.Logs.Find(departureEventLogQuery).SingleOrDefault();
			AssertNotNull("Should create departure log for package", packageDepartureLog);
			AssertEquals("DTU123|FAC=CFS|LOC=Sydney|RES=Scanned|RFN=DCN1|TYP=VehicleReference|WHS=TRW", packageDepartureLog.SL_Reference);
		}

		[TestDate(2021, 1, 1)]
		public void TestFinalizeEventInPackage()
		{
			var now = ZDateTimeOffset.Now;
			var dateTimeOffsetNowWithoutSeconds = new ZDateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0, now.Offset);

			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			Factory.Save();

			dtu.WDH_GateInTime = dateTimeOffsetNowWithoutSeconds.AddHours(-2);
			dtu.WDH_LoadCompleteTime = dateTimeOffsetNowWithoutSeconds.AddHours(-1);
			dtu.WDH_GateOutTime = dateTimeOffsetNowWithoutSeconds.AddHours(-1);

			dtu.Finalise(dateTimeOffsetNowWithoutSeconds, "Test");
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.Finalized);

			var departedEventLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Departure.Code);
			var packageDepartureLog = packageState.Package.Logs.Find(departedEventLogQuery).SingleOrDefault();
			AssertNull("Should not create departure log for package", packageDepartureLog);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
			var finalizeLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ItemDocumentJobFinalised.Code);
			var packageLog = packageState.Package.Logs.Find(finalizeLogQuery).SingleOrDefault();
			AssertNotNull("Should create finalise log for package", packageLog);
			AssertEquals("P1|FAC=CFS|TYP=Finalised|WHS=WHS", packageLog.SL_Reference);
		}
		#endregion

		#region TestDelete

		public void TestDelete()
		{
			var packageState = Factory.New<WhsItemPackageState>();

			AssertEquals(1, Factory.Load<WhsItemPackageState>(new ZQuery()).Length);

			ProcessTask task = null;

			if (packageState.WorkflowItems != null)
			{ 
				task = packageState.WorkflowItems.AddNew();
				AssertEquals(1, packageState.WorkflowItems.Count);
			}

			packageState.Delete();

			AssertEquals(0, Factory.Load<WhsItemPackageState>(new ZQuery()).Length);

			if (packageState.WorkflowItems != null)
			{
				AssertEquals(0, packageState.WorkflowItems.Count);
			}
		}

		#endregion

		#region TestIWorkflowProvider Members

		public void TestIWorkflowProviderMembers()
		{
			var packageState = Factory.New<WhsItemPackageState>();
			var newWorkflowItem = packageState.WorkflowItems.AddNew();
			var workflowProvider = (IWorkflowProvider)packageState;

			AssertEquals(WorkflowDescriptors.TransitPackage, workflowProvider.WorkflowType);
			AssertEquals(newWorkflowItem, workflowProvider.WorkflowItems.Single());
			AssertEquals(typeof(ColumnValueRanker), workflowProvider.GetTemplateSelectionCriteria().GetType());
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var intendedWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			var rcn = Helper.CreateReceiveConsignment("RCN", intendedWarehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "P1", TransitWarehouseStatuses.Codes.Booked);
			Factory.Save();

			var criteriaForPackageState = (ColumnValueRanker)((IWorkflowProvider)packageState).GetTemplateSelectionCriteria();
			AssertEquals(intendedWarehouse.PK, criteriaForPackageState.GetValues(ProcessTaskTemplateSchema.P0_WW).First());
		}

		#endregion

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);
	}

	[TestedType(typeof(WhsItemPackageState))]
	public class Triggers_PreventMismatchRCNOrDCNWithOverpackInner : DeferrableTriggerTestCase<WhsItemPackageState>
	{
		#region TestTG_PreventCreateCycleCountLocationsWithOpenVariance

		[ExpectNoExceptions]
		public void TestTG_PreventMismatchRCNOrDCNWithOverpackInner_UpdateParent()
		{
			var now = DateTimeOffset.Now;
			var warehouse = Helper.CreateTRWWarehouse();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse.PK);
			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);

			Factory.Save();

			var overpack = helper.CreateOverpackPackage("OVP", rcn1, rtu, rcn: rcn1);
			overpack.WPS_WDC_TransitDispatchConsignment = dcn1.PK;
			var childPackage = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn1);
			Helper.PackPackageIntoHandlingUnit(overpack, childPackage, ZDateTimeOffset.Now, "AAA", overpack);
			Factory.Save();

			childPackage.WPS_WRC_TransitReceiveConsignment = rcn2.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsItemPackageState.PreventOverpackInnerRCNOrDCNDifferentFromParentMessage, true), "Trigger should prevent updating overpack parent rcn different from its inner.");

			childPackage.WPS_WDC_TransitDispatchConsignment = dcn2.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsItemPackageState.PreventOverpackInnerRCNOrDCNDifferentFromParentMessage, true), "Trigger should prevent updating overpack parent rcn different from its inner.");
		}

		[ExpectNoExceptions]
		public void TestTG_PreventMismatchRCNOrDCNWithOverpackInner_UpdateChild()
		{
			var now = DateTimeOffset.Now;
			var warehouse = Helper.CreateTRWWarehouse();

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse.PK);
			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);

			Factory.Save();

			var overpack = helper.CreateOverpackPackage("OVP", rcn1, rtu, rcn: rcn1);
			overpack.WPS_WDC_TransitDispatchConsignment = dcn1.PK;
			var childPackage = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn1);
			Helper.PackPackageIntoHandlingUnit(overpack, childPackage, ZDateTimeOffset.Now, "AAA", overpack);
			Factory.Save();

			overpack.WPS_WRC_TransitReceiveConsignment = rcn2.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsItemPackageState.PreventOverpackParentRCNOrDCNDifferentFromInnerMessage, true), "Trigger should prevent updating overpack parent dcn different from its inner.");

			overpack.WPS_WDC_TransitDispatchConsignment = dcn2.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsItemPackageState.PreventOverpackParentRCNOrDCNDifferentFromInnerMessage, true), "Trigger should prevent updating overpack parent dcn different from its inner.");
		}

		#endregion

		#region Implementation

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#endregion
	}
}
