using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class TransitWarehouseHelperTest : TestCaseWithFactory
	{
		#region TestGetAttachedPackagesQuery_PackagesAttachedViaAdditionalReference

		public void TestGetAttachedPackagesQuery_PackagesAttachedViaAdditionalReference()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, "EXTREF1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var attachedPackageStateToJob1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: "JOB1");
			var attachedPackageStateToJob2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: "JOB2");
			var unAssignedPackageState = Helper.CreatePackageState(rcn, 1, "PLT", "PKGPUT", TransitWarehouseStatuses.Codes.Arrived, rtu);

			var packedChildPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: "JOB1");
			var unPackedChildPackage = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, entryNum: "JOB1");
			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, receiveUnit: rtu, entryNum: "JOB1");

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, packedChildPackage, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, unPackedChildPackage, ZDateTimeOffset.Now, "ABC", unpackedTime: ZDateTimeOffset.Now);
			Factory.Save();

			var queryForJob = TransitWarehouseHelper.GetAttachedPackagesQuery("JOB1", warehouse.PK);
			AssertContainsExactElementsInAnyOrder(new[] { attachedPackageStateToJob1, packedChildPackage, unPackedChildPackage }, Factory.Load<WhsItemPackageState>(queryForJob));

			var queryForInvalidJob = TransitWarehouseHelper.GetAttachedPackagesQuery("INVALID", warehouse.PK);
			AssertEquals(0, Factory.Load<WhsItemPackageState>(queryForInvalidJob).Length);

			var queryForInvalidWarehouse = TransitWarehouseHelper.GetAttachedPackagesQuery("JOB1", ZGuid.NewZGuid());
			AssertEquals(0, Factory.Load<WhsItemPackageState>(queryForInvalidWarehouse).Length);
		}

		#endregion

		#region TestGetAttachedPackagesQuery_PackagesAssignedInDifferentWarehouses

		public void TestGetAttachedPackagesQuery_PackagesAssignedInDifferentWarehouses()
		{
			var warehouse1 = Helper.CreateTRWWarehouse("W1");
			var warehouse2 = Helper.CreateTRWWarehouse("W2");
			var rowInWarehouse1 = Helper.CreateRowAndGenerateLocations(warehouse1, "Dock1", 2, 2);
			var dockInWarehouse1 = rowInWarehouse1.Locations.First(l => l.ToLocationString() == "Dock1-1-1");

			var rowInWarehouse2 = Helper.CreateRowAndGenerateLocations(warehouse2, "Dock2", 2, 2);
			var dockInWarehouse2 = rowInWarehouse2.Locations.First(l => l.ToLocationString() == "Dock2-1-1");

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse1.PK, "EXTREF1");
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", "STD", warehouse2.PK, "EXTREF2");
			var rtuInWarehouse1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse1.PK, dockInWarehouse1.PK);
			var rtuInWarehouse2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse2.PK, dockInWarehouse2.PK, "V2");
			var attachedPackageStateToJob1InWarehouse1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtuInWarehouse1, entryNum: "JOB1");
			var attachedPackageStateToJob1InWarehouse2 = Helper.CreatePackageState(rcn2, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtuInWarehouse2, entryNum: "JOB1");

			Factory.Save();

			var queryForJobInWarehouse1 = TransitWarehouseHelper.GetAttachedPackagesQuery("JOB1", warehouse1.PK);
			AssertEquals(attachedPackageStateToJob1InWarehouse1, Factory.Load<WhsItemPackageState>(queryForJobInWarehouse1).Single());

			var queryForJobInWarehouse2 = TransitWarehouseHelper.GetAttachedPackagesQuery("JOB1", warehouse2.PK);
			AssertEquals(attachedPackageStateToJob1InWarehouse2, Factory.Load<WhsItemPackageState>(queryForJobInWarehouse2).Single());
		}

		#endregion

		#region TestGetFreightModeByTransportMode

		public void TestGetFreightModeByTransportMode()
		{
			// AIR
			AssertEquals(FreightMode.ULD, TransitWarehouseHelper.GetFreightModeByTransportMode(TransportModes.Air, true));
			AssertEquals(FreightMode.AIR, TransitWarehouseHelper.GetFreightModeByTransportMode(TransportModes.Air, false));
			AssertEquals(FreightMode.ULD, TransitWarehouseHelper.GetFreightModeByTransportMode(TransportModes.AirSea, true));
			AssertEquals(FreightMode.AIR, TransitWarehouseHelper.GetFreightModeByTransportMode(TransportModes.AirSea, false));

			// SEA
			AssertEquals(FreightMode.FCL, TransitWarehouseHelper.GetFreightModeByTransportMode(TransportModes.Sea, true));
			AssertEquals(FreightMode.SEA, TransitWarehouseHelper.GetFreightModeByTransportMode(TransportModes.Sea, false));
			AssertEquals(FreightMode.FCL, TransitWarehouseHelper.GetFreightModeByTransportMode(TransportModes.SeaAir, true));
			AssertEquals(FreightMode.SEA, TransitWarehouseHelper.GetFreightModeByTransportMode(TransportModes.SeaAir, false));

			// Road
			AssertEquals(FreightMode.FRO, TransitWarehouseHelper.GetFreightModeByTransportMode(TransportModes.Road, true));
			AssertEquals(FreightMode.ROA, TransitWarehouseHelper.GetFreightModeByTransportMode(TransportModes.Road, false));

			// Rail
			AssertEquals(FreightMode.FRA, TransitWarehouseHelper.GetFreightModeByTransportMode(TransportModes.Rail, true));
			AssertEquals(FreightMode.RAI, TransitWarehouseHelper.GetFreightModeByTransportMode(TransportModes.Rail, false));
		}

		#endregion

		#region TestGetViewPackagesQuery

		public void TestGetViewPackagesQuery_LoadedIntoContainerDispatchUnit_ULD()
		{
			TestGetViewPackagesQuery_LoadedIntoContainerDispatchUnitCore(TransportUnitTypes.ULD);
		}

		public void TestGetViewPackagesQuery_LoadedIntoContainerDispatchUnit_Container()
		{
			TestGetViewPackagesQuery_LoadedIntoContainerDispatchUnitCore(TransportUnitTypes.Container);
		}

		void TestGetViewPackagesQuery_LoadedIntoContainerDispatchUnitCore(string unitType)
		{
			var warehouse = Helper.CreateTRWWarehouse();

			var shipmentJobPK = ZGuid.NewZGuid();
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, jobID: "EXTREF1", parentPK: shipmentJobPK, parentCode: "JS");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, jobID: "EXTREF1", parentPK: shipmentJobPK, parentCode: "JS");
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var (dtu, dtuPackage) = Helper.CreateDispatchTransportationUnitWithPackage("DTU1", warehouse.PK, dtuUnitType: unitType, packageType: "CNT");

			var packageStateULD = Helper.CreatePackageState(dtuPackage, TransitWarehouseStatuses.Codes.FreightLoaded, receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);
			packageStateULD.WPS_UnitType = unitType;

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, volume: 10, volumeUQ: "M3", weight: 1, weightUQ: "KG");
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, volume: 1, volumeUQ: "M3", weight: 2, weightUQ: "KG");

			Helper.PackPackageIntoHandlingUnit(packageStateULD, packageState1, ZDateTimeOffset.Now, "XXX", packageStateULD);
			Helper.PackPackageIntoHandlingUnit(packageStateULD, packageState2, ZDateTimeOffset.Now, "XXX", packageStateULD);

			Factory.Save();

			var queryForJob = TransitWarehouseHelper.GetViewPackagesQuery("EXTREF1", shipmentJobPK, warehouse.PK);
			AssertContainsExactElementsInAnyOrder(new[] { packageState1, packageState2 }, Factory.Load<WhsItemPackageState>(queryForJob));
		}

		public void TestGetViewPackagesQuery_PackedInHandlingUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();

			var shipmentJobPK = ZGuid.NewZGuid();
			var rcn = Helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK, jobID: "EXTREF1", parentPK: shipmentJobPK, parentCode: "JS");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK, jobID: "EXTREF1", parentPK: shipmentJobPK, parentCode: "JS");
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackageState = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, volume: 10, volumeUQ: "M3", weight: 1, weightUQ: "KG");
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Departed, rtu, dcn, dtu, dll, volume: 1, volumeUQ: "M3", weight: 2, weightUQ: "KG");

			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageState, packageState1, ZDateTimeOffset.Now, "XXX", handlingUnitPackageState);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackageState, packageState2, ZDateTimeOffset.Now, "XXX", handlingUnitPackageState);

			Factory.Save();

			var queryForJob = TransitWarehouseHelper.GetViewPackagesQuery("EXTREF1", shipmentJobPK, warehouse.PK);
			AssertContainsExactElementsInAnyOrder(new[] { packageState1, packageState2 }, Factory.Load<WhsItemPackageState>(queryForJob));
		}

		#endregion

		#region TestCanSendCRESAMessage

		public void TestCanSendCRESAMessage()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RCN001", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN001", warehouse.PK);
			Factory.Save();

			var consignments = new BusinessObject[] { rcn, dcn };
			var portReferenceTypes = new string[] { "PEN", "PAN" };
			var systemUsers = new List<string> { User.ServiceUserCode, User.SupportUserCode, User.UnKnownUserCode, User.InterchangeUserCode, User.WebUserCode };
			foreach (var portReferenceType in portReferenceTypes)
			{
				foreach (var consignment in consignments)
				{
					var canSendCRESAMessage = TransitWarehouseHelper.CanSendCRESAMessage(consignment);
					AssertEquals(true, canSendCRESAMessage);

					var portReference = Helper.CreateAdditionalReference(consignment, "BBE001", portReferenceType, CusEntryNumber.Categories.PortAuthorityReferenceNumber);
					portReference.CE_SystemCreateUser = "CU3";
					Factory.Save();

					canSendCRESAMessage = TransitWarehouseHelper.CanSendCRESAMessage(consignment);
					AssertEquals(false, canSendCRESAMessage);

					foreach (var user in systemUsers)
					{
						portReference.CE_SystemCreateUser = user;
						Factory.Save();

						canSendCRESAMessage = TransitWarehouseHelper.CanSendCRESAMessage(consignment);
						AssertEquals(true, canSendCRESAMessage);
					}

					portReference.Delete();
					Factory.Save();
				}
			}
		}

		#endregion

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);
	}
}
