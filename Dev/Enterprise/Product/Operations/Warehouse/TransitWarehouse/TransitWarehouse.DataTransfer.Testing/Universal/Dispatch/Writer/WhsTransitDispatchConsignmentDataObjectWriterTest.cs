using System;
using System.IO;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.DataTransfer.Universal.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitDispatchConsignmentDataObjectWriterTest : TransitUniversalTestCase
	{
		#region TestTopLevelDataContextType

		public void TestTopLevelDataContextType()
		{
			AssertEquals(DataContextType.TransitDispatch,
				((ITopLevelDataObjectWriter)new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).TopLevelDataContextType);
		}

		#endregion

		#region TestEDIMessageSubType

		public void TestEDIMessageSubType()
		{
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				((ITopLevelDataObjectWriter)new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).EDIMessageSubType);
		}

		#endregion

		#region Integration Tests

		#region Test_EventExport_Writes_XML_WithoutError

		public void Test_EventExport_Writes_XML_WithoutError()
		{
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);

			var trigger = helper.GetEventTrigger(dispatchConsignment,
				Core.Constants.Workflow.WorkflowTriggerType,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				"Export UXML From DCN");

			var notification = helper.GetEventNotification(trigger,
				WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
				MessageRecipientPartyTypeList.Codes.OrgProxy);
			Factory.SaveForTesting();

			using (var tempDirectory = new TempDirectory())
			{
				var dcnReference = dispatchConsignment[WhsItemDispatchConsignmentSchema.WDC_ConsignmentID].ToString();

				var dataWriter = trigger.WorkflowDescriptor.GetTestFileWriter(notification, dispatchConsignment);
				var exportResult = dataWriter.Export(tempDirectory.DirectoryName);

				AssertNotNull("exportResult", exportResult);
				CombineAssertions("exportResult", delegate
				{
					AssertContains("Text", $"UniversalShipment from [{dcnReference}] saved", exportResult.Message);
					var messageLines = exportResult.Message.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
					AssertEquals("messageLines.Length", 2, messageLines.Length);
					var uxmlFileName = messageLines[1];
					AssertEquals($"File.Exists(\"{uxmlFileName}\")", true, File.Exists(uxmlFileName));
					AssertContains("fileName", tempDirectory.DirectoryName, uxmlFileName);
					AssertEquals("fileName.EndsWith(\".xml\") failed on: " + uxmlFileName, true, uxmlFileName.EndsWith(".xml"));
					var uxmlText = File.ReadAllText(uxmlFileName);
					AssertContains("Trigger Count is never zero even for samples.", "<TriggerCount>1</TriggerCount>", uxmlText);
				});
			}
		}

		#endregion

		#endregion

		#region PopulateDataObject Tests

		#region TestPopulateDataObject_DispatchConsignment_PackagesAndServiceLevel

		public void TestPopulateDataObject_DispatchConsignment_PackagesAndServiceLevel()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var receiveConsignment2 = helper.CreateReceiveConsignment("RCN2", "STD", Data.Warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);

			var packageState1 = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);
			var packageState2 = helper.CreatePackageState(receiveConsignment2, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);
			var packageState3 = helper.CreatePackageState(receiveConsignment2, 1, "PLT", "PKG3", TransitWarehouseStatuses.Codes.Booked);

			var location = helper.CreateLocation(Data.Warehouse);
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);
			var handlingUnit = helper.CreatePackageState(receiveTransportationUnit, 1, "PLT", "HU1", TransitWarehouseStatuses.Codes.Arrived, dispatchConsignment: dispatchConsignment);
			handlingUnit.WPS_IsHandlingUnit = true;

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			const string packageErrorMessage = "DCNs should include all their packages except handling units.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new[] { "PKG1", "PKG2", "HU1" }, consignmentDataObject.PackingLineCollection.Select(x => x.ReferenceNumber.Value));
			var packingLine1 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1");
			var packingLine2 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2");
			var packingLine3 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "HU1");
		}

		#endregion

		#region TestPopulateDataObject_PopulateOutturnDetail

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestPopulateDataObject_PopulateOutturnDetail()
		{
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);
			var location = helper.CreateLocation(Data.Warehouse);
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);
			var dispatchLoadlist = helper.CreateDispatchLoadList("DLL0001", Data.Warehouse.PK);
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnit("DTU1", Data.Warehouse.PK);
			dispatchTransportationUnit.WDH_VehicleReference = "VEH1";
			var dispatchTransportationUnitWithContainer = helper.CreateDispatchTransportationUnitWithContainerType("DTU2", Data.Warehouse.PK, containerID: "CNT1");
			dispatchTransportationUnitWithContainer.WDH_VehicleReference = "VEH2";

			var packageState1 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadlist, dispatchUnit: dispatchTransportationUnit);
			packageState1.WPS_UnloadedNotYetProcessedTime = packageState1.WPS_UnloadedTime = new ZDateTimeOffset("2025-04-26 12:00:00 +08:00");
			packageState1.WPS_LoadedTime = new ZDateTimeOffset("2025-04-27 12:00:00 +08:00");
			var packageState2 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadlist, dispatchUnit: dispatchTransportationUnitWithContainer);
			packageState2.WPS_UnloadedNotYetProcessedTime = packageState2.WPS_UnloadedTime = new ZDateTimeOffset("2025-04-26 04:00:00 +00:00");
			packageState2.WPS_LoadedTime = new ZDateTimeOffset("2025-04-27 04:00:00 +00:00");
			var packageState3 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment);
			packageState3.WPS_UnloadedNotYetProcessedTime = packageState3.WPS_UnloadedTime = new ZDateTimeOffset("2025-04-26 04:00:00 +00:00");
			var packageState4 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);

			var ovpPackageIsUnload = helper.CreateOverpackPackage("OVPPKG1", receiveConsignment, receiveTransportationUnit, TransitWarehouseStatuses.Codes.FreightLoaded, dcn: dispatchConsignment, dll: dispatchLoadlist, dtu: dispatchTransportationUnit);
			ovpPackageIsUnload.WPS_UnloadedNotYetProcessedTime = ovpPackageIsUnload.WPS_UnloadedTime = new ZDateTimeOffset("2025-04-26 12:00:00 +08:00");
			ovpPackageIsUnload.WPS_LoadedTime = new ZDateTimeOffset("2025-04-27 12:00:00 +08:00");
			var ovpPackageNotUnloadIsLoaded = helper.CreateOverpackPackage("OVPPKG4", receiveConsignment, null, TransitWarehouseStatuses.Codes.FreightLoaded, dcn: dispatchConsignment, dll: dispatchLoadlist, dtu: dispatchTransportationUnit);
			ovpPackageNotUnloadIsLoaded.WPS_LoadedTime = new ZDateTimeOffset("2025-04-27 04:00:00 +00:00");
			var ovpPackageNotUnloadNotLoadNoInner = helper.CreateOverpackPackage("OVPPKG2", receiveConsignment, null, TransitWarehouseStatuses.Codes.Arrived, dcn: dispatchConsignment);
			var ovpPackageNotUnloadNotLoadHasInner = helper.CreateOverpackPackage("OVPPKG3", receiveConsignment, null, TransitWarehouseStatuses.Codes.Arrived, dcn: dispatchConsignment);
			var innerPackageState = helper.CreatePackageState(receiveConsignment, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: receiveTransportationUnit, dispatchConsignment: dispatchConsignment, dispatchLoadList: dispatchLoadlist, dispatchUnit: dispatchTransportationUnit);
			helper.PackPackageIntoHandlingUnit(ovpPackageNotUnloadNotLoadHasInner, innerPackageState, ZDateTimeOffset.Now, "KL6", ovpPackageNotUnloadNotLoadHasInner);
			ovpPackageNotUnloadNotLoadHasInner.WPS_SystemCreateTimeUtc = new ZDateTime(2025, 4, 26, 4, 0, 0);

			Factory.SaveForTesting();

			helper.SetupPackageForOutturn(packageState1.Package, bookedQty: 1, damagedQty: 0, height: 1, length: 2, volume: 10, weight: 4, width: 5);
			helper.SetupPackageForOutturn(packageState2.Package, bookedQty: 1, damagedQty: 1, height: 1, length: 2, volume: 10, weight: 4, width: 5);
			helper.SetupPackageForOutturn(packageState3.Package, bookedQty: 1, damagedQty: 0, height: 1, length: 2, volume: 10, weight: 4, width: 5);
			helper.SetupPackageForOutturn(packageState4.Package, bookedQty: 10, damagedQty: 10, height: 1, length: 2, volume: 100, weight: 4, width: 5);
			helper.SetupPackageForOutturn(ovpPackageIsUnload.Package, bookedQty: 1, damagedQty: 0, height: 1, length: 2, volume: 10, weight: 4, width: 5);
			helper.SetupPackageForOutturn(ovpPackageNotUnloadIsLoaded.Package, bookedQty: 1, damagedQty: 0, height: 1, length: 2, volume: 10, weight: 4, width: 5);
			helper.SetupPackageForOutturn(ovpPackageNotUnloadNotLoadNoInner.Package, bookedQty: 1, damagedQty: 0, height: 1, length: 2, volume: 10, weight: 4, width: 5);
			helper.SetupPackageForOutturn(ovpPackageNotUnloadNotLoadHasInner.Package, bookedQty: 1, damagedQty: 0, height: 1, length: 2, volume: 10, weight: 4, width: 5);

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);
			const string packageErrorMessage = "DCNs should include all their packages.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new[] { "PKG1", "PKG2", "PKG3", "PKG4", "OVPPKG1", "OVPPKG2", "OVPPKG3", "OVPPKG4" }, consignmentDataObject.PackingLineCollection.Select(x => x.ReferenceNumber.Value));
			var packingLine1 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1");
			var packingLine2 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2");
			var packingLine3 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG3");
			var packingLine4 = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG4");
			var ovpPackageIsUnloadedPackingLine = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "OVPPKG1");
			var ovpPackageNotUnloadIsLoadedPackingLine = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "OVPPKG4");
			var ovpPackageNotUnloadNotLoadNoInnerPackingLine = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "OVPPKG2");
			var ovpPackageNotUnloadNotLoadHasInnerPackingLine = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "OVPPKG3");

			AssertOutturnDetail(packingLine1, 1, new ZDateTime(2025, 4, 26, 14, 0, 0), new ZDateTime(2025, 4, 27, 14, 0, 0), 0, 1, 2, 10, 4, 5);
			AssertOutturnDetail(packingLine2, 1, new ZDateTime(2025, 4, 26, 14, 0, 0), new ZDateTime(2025, 4, 27, 14, 0, 0), 1, 1, 2, 10, 4, 5);
			AssertOutturnDetail(packingLine3, 1, new ZDateTime(2025, 4, 26, 14, 0, 0), null, 0, 0, 0, 0, 0, 0);
			AssertOutturnDetail(packingLine4, 0, null, null, 1, 0, 0, 0, 0, 0);
			AssertOutturnDetail(ovpPackageIsUnloadedPackingLine, 1, new ZDateTime(2025, 4, 26, 14, 0, 0), new ZDateTime(2025, 4, 27, 14, 0, 0), 0, 1, 2, 10, 4, 5);
			AssertOutturnDetail(ovpPackageNotUnloadIsLoadedPackingLine, 1, null, new ZDateTime(2025, 4, 27, 14, 0, 0), 0, 1, 2, 10, 4, 5);
			AssertOutturnDetail(ovpPackageNotUnloadNotLoadNoInnerPackingLine, 0, null, null, 0, 0, 0, 0, 0, 0);
			AssertOutturnDetail(ovpPackageNotUnloadNotLoadHasInnerPackingLine, 1, new ZDateTime(2025, 4, 26, 14, 0, 0), null, 0, 0, 0, 0, 0, 0);

			AssertEquals("", packingLine1.TransportReference);
			AssertEquals(ZString.Empty, packingLine2.TransportReference);

			var transportReferenceForPackLine1 = packingLine1.ReferenceNumberCollection.Single(a => a.Type.Code.Value == TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchTransportationUnit);
			var transportReferenceForPackLine2 = packingLine2.ReferenceNumberCollection.Single(a => a.Type.Code.Value == TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchTransportationUnit);
			AssertEquals("DTU1", transportReferenceForPackLine1.ReferenceNumber);
			AssertEquals("DTU2", transportReferenceForPackLine2.ReferenceNumber);
			AssertNull(packingLine3.ReferenceNumberCollection);
			AssertNull(packingLine3.ReferenceNumberCollection);

			const string containerErrorMessage = "DCNs should include containers.";
			AssertContainsExactElementsInAnyOrder(containerErrorMessage, new[] { "VEH2" }, consignmentDataObject.ContainerCollection.Select(x => x.ContainerNumber.Value));

			AssertNull(packingLine1.ContainerLink);
			AssertEquals(0, packingLine2.ContainerLink);
			AssertNull(packingLine3.ContainerLink);
			AssertNull(packingLine4.ContainerLink);
		}

		public void TestPopulateDataObject_PalletizedPackline()
		{
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);
			var location = helper.CreateLocation(Data.Warehouse);
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);
			var dispatchTransportationUnitWithContainer = helper.CreateDispatchTransportationUnitWithContainerType("DTU1", Data.Warehouse.PK);
			dispatchTransportationUnitWithContainer.WDH_VehicleReference = "VEH1";

			var packageState = helper.CreatePackageState(receiveConsignment, 1, "PLT", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnitWithContainer);

			helper.CreateAndAttachArrivedInners(packageState, 10, "CTN");
			helper.SetupPackageForOutturn(packageState.Package, bookedQty: 10, damagedQty: 10, height: 0, length: 0, volume: 0, weight: 0, width: 0);
			helper.SetupPackageForOutturn(packageState.Package.Packages.Single(), bookedQty: 0, damagedQty: 0, height: 0, length: 0, volume: 0, weight: 0, width: 0);

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			const string packageErrorMessage = "DCNs should include all their packages.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new[] { "PKG1" }, consignmentDataObject.PackingLineCollection.Select(x => x.ReferenceNumber.Value));
			var packingLine = consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1");
			AssertEquals("PKG1", packingLine.ReferenceNumber.Value);
			AssertEquals("PLT", packingLine.PackType.Code.Value);
			AssertEquals(0, packingLine.ContainerLink);
			AssertOutturnDetail(packingLine, 1, null, packageState.WPS_LoadedTime.ToLocalZDateTime(), 1, 0, 0, 0, 0, 0);

			AssertEquals(1, packingLine.PackingLineCollection.Count);
			var innerPackingLine = packingLine.PackingLineCollection.Single();
			AssertEquals("CTN", innerPackingLine.PackType.Code.Value);
			AssertNull(innerPackingLine.ContainerLink);
			AssertOutturnDetail(innerPackingLine, 1, null, packageState.WPS_LoadedTime.ToLocalZDateTime(), 0, 0, 0, 0, 0, 0);
		}

		public void TestPopulateDataObject_DispatchConsignment_NestedOverpackHandlingUnitsAndInners()
		{
			var warehouse = Data.Warehouse;

			var row = helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rtu = helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");
			var topLevelOverpackPackageState = helper.CreateOverpackPackage("OuterOverpack", dispatchConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived,
				unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment, dcn: dispatchConsignment);
			var onceNestedOverpackPackageState = helper.CreateOverpackPackage("firstInnerOverpack", dispatchConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived,
				unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment, dcn: dispatchConsignment);
			var twiceNestedsOverpackPackageState = helper.CreateOverpackPackage("secondInnerOverpack", dispatchConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived,
				unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment, dcn: dispatchConsignment);
			var singleLevelHandlingUnitInner1 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "OuterPack_InnerPackage1", TransitWarehouseStatuses.Codes.Arrived, rtu, dispatchConsignment: dispatchConsignment);
			var singleLevelHandlingUnitInner2 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "OuterPack_InnerPackage2", TransitWarehouseStatuses.Codes.Arrived, rtu, dispatchConsignment: dispatchConsignment);
			var firstNestedHandlingUnitInner = helper.CreatePackageState(receiveConsignment, 1, "PKG", "firstNested_InnerPackage", TransitWarehouseStatuses.Codes.Arrived, rtu, dispatchConsignment: dispatchConsignment);
			var secondNestedHandlingUnitInner = helper.CreatePackageState(receiveConsignment, 1, "PKG", "secondNested_InnerPackage", TransitWarehouseStatuses.Codes.Arrived, rtu, dispatchConsignment: dispatchConsignment);
			helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState, onceNestedOverpackPackageState, ZDateTimeOffset.Now, "JEF");
			helper.PackPackageIntoHandlingUnit(onceNestedOverpackPackageState, twiceNestedsOverpackPackageState, ZDateTimeOffset.Now, "JEF");
			helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState, singleLevelHandlingUnitInner1, ZDateTimeOffset.Now, "JEF");
			helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState, singleLevelHandlingUnitInner2, ZDateTimeOffset.Now, "JEF");
			helper.PackPackageIntoHandlingUnit(onceNestedOverpackPackageState, firstNestedHandlingUnitInner, ZDateTimeOffset.Now, "JEF");
			helper.PackPackageIntoHandlingUnit(twiceNestedsOverpackPackageState, secondNestedHandlingUnitInner, ZDateTimeOffset.Now, "JEF");

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			var topLevelPackingLineCollection = consignmentDataObject.PackingLineCollection;
			AssertContainsExactElementsInAnyOrder(new[] { "OuterOverpack" }, topLevelPackingLineCollection.Select(p => p.ReferenceNumber.Value));
			var outerOverPack = topLevelPackingLineCollection.Single(p => p.ReferenceNumber.Value == "OuterOverpack");
			AssertContainsExactElementsInAnyOrder(new[] { "OuterPack_InnerPackage1", "OuterPack_InnerPackage2", "firstInnerOverpack" }, outerOverPack.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			var firstInnerOverPack = outerOverPack.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "firstInnerOverpack");
			AssertContainsExactElementsInAnyOrder(new[] { "firstNested_InnerPackage", "secondInnerOverpack" }, firstInnerOverPack.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			var secondInnerOverPack = firstInnerOverPack.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "secondInnerOverpack");
			AssertContainsExactElementsInAnyOrder(new[] { "secondNested_InnerPackage" }, secondInnerOverPack.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
		}

		public void TestPopulateDataObject_DispatchConsignment_LastScreeningMethodForOverPack()
		{
			var warehouse = Data.Warehouse;

			var row = helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", "STD", warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rtu = helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, vehicleRef: "V1");

			var topLevelOverpackPackageState1 = helper.CreateOverpackPackage("OuterOverpack1", dispatchConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived,
				unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment, dcn: dispatchConsignment);
			var topLevelOverpackPackageState2 = helper.CreateOverpackPackage("OuterOverpack2", dispatchConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived,
				unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment, dcn: dispatchConsignment);
			var topLevelOverpackPackageState3 = helper.CreateOverpackPackage("OuterOverpack3", dispatchConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived,
				unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment, dcn: dispatchConsignment);
			var topLevelOverpackPackageState4 = helper.CreateOverpackPackage("OuterOverpack4", dispatchConsignment, rtu, TransitWarehouseStatuses.Codes.Arrived,
				unitType: PackageStateUnitType.Codes.Overpack, rcn: receiveConsignment, dcn: dispatchConsignment);
			var singleLevelOverpack1Inner1 = helper.CreatePackageState(receiveConsignment, 1, "CTN", "OuterPack1_InnerPackage1", TransitWarehouseStatuses.Codes.Arrived, rtu, dispatchConsignment: dispatchConsignment);
			var singleLevelOverpack1Inner2 = helper.CreatePackageState(receiveConsignment, 1, "CTN", "OuterPack1_InnerPackage2", TransitWarehouseStatuses.Codes.Arrived, rtu, dispatchConsignment: dispatchConsignment);
			var singleLevelOverpack2Inner1 = helper.CreatePackageState(receiveConsignment, 1, "CTN", "OuterPack2_InnerPackage1", TransitWarehouseStatuses.Codes.Arrived, rtu, dispatchConsignment: dispatchConsignment);
			var singleLevelOverpack3Inner1 = helper.CreatePackageState(receiveConsignment, 1, "CTN", "OuterPack3_InnerPackage1", TransitWarehouseStatuses.Codes.Arrived, rtu, dispatchConsignment: dispatchConsignment);
			var singleLevelOverpack4Inner1 = helper.CreatePackageState(receiveConsignment, 1, "CTN", "OuterPack4_InnerPackage1", TransitWarehouseStatuses.Codes.Arrived, rtu, dispatchConsignment: dispatchConsignment);
			var singleLevelOverpack4Inner2 = helper.CreatePackageState(receiveConsignment, 1, "CTN", "OuterPack4_InnerPackage2", TransitWarehouseStatuses.Codes.Arrived, rtu, dispatchConsignment: dispatchConsignment);

			var ovp1Child1Screening = helper.CreatePackageScreening(singleLevelOverpack1Inner1.Package, "SC1", true);
			var ovp1Child2Screening = helper.CreatePackageScreening(singleLevelOverpack1Inner2.Package, "SC1", true);
			ovp1Child1Screening.KPS_Time = DateTime.Now;
			ovp1Child2Screening.KPS_Time = DateTime.Now.AddSeconds(2);

			var ovp2Screening = helper.CreatePackageScreening(topLevelOverpackPackageState2.Package, "OV2", true);
			var ovp2Child1Screening = helper.CreatePackageScreening(singleLevelOverpack2Inner1.Package, "SC8", true);
			ovp2Screening.KPS_Time = DateTime.Now;
			ovp2Child1Screening.KPS_Time = DateTime.Now.AddSeconds(2);

			var ovp3Screening = helper.CreatePackageScreening(topLevelOverpackPackageState3.Package, "OV3", true);
			ovp3Screening.KPS_Time = DateTime.Now;

			var ovp4Child1Screening = helper.CreatePackageScreening(singleLevelOverpack4Inner1.Package, "SC2", true);
			var ovp4Child2Screening = helper.CreatePackageScreening(singleLevelOverpack4Inner2.Package, "SC3", true);
			ovp1Child1Screening.KPS_Time = DateTime.Now;
			ovp1Child2Screening.KPS_Time = DateTime.Now.AddSeconds(2);

			helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState1, singleLevelOverpack1Inner1, ZDateTimeOffset.Now, "JEF");
			helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState1, singleLevelOverpack1Inner2, ZDateTimeOffset.Now, "JEF");
			helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState2, singleLevelOverpack2Inner1, ZDateTimeOffset.Now, "GDS");
			helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState3, singleLevelOverpack3Inner1, ZDateTimeOffset.Now, "ABC");
			helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState4, singleLevelOverpack4Inner1, ZDateTimeOffset.Now, "ABC");
			helper.PackPackageIntoHandlingUnit(topLevelOverpackPackageState4, singleLevelOverpack4Inner2, ZDateTimeOffset.Now, "ABC");

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			var outerLevelPackingLineCollection = consignmentDataObject.PackingLineCollection;
			var outerOverPack1 = outerLevelPackingLineCollection.Single(p => p.ReferenceNumber.Value == "OuterOverpack1");
			var outerOverPack2 = outerLevelPackingLineCollection.Single(p => p.ReferenceNumber.Value == "OuterOverpack2");
			var outerOverPack3 = outerLevelPackingLineCollection.Single(p => p.ReferenceNumber.Value == "OuterOverpack3");
			var outerOverPack4 = outerLevelPackingLineCollection.Single(p => p.ReferenceNumber.Value == "OuterOverpack4");

			AssertContainsExactElementsInAnyOrder(new[] { "OuterOverpack1", "OuterOverpack2", "OuterOverpack3", "OuterOverpack4" }, outerLevelPackingLineCollection.Select(p => p.ReferenceNumber.Value));

			AssertContainsExactElementsInAnyOrder(new[] { "OuterPack1_InnerPackage1", "OuterPack1_InnerPackage2" }, outerOverPack1.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertNull(outerOverPack1.ScreeningMethod);
			AssertNull(outerOverPack1.AviationSecurityInspectionType);
			AssertContainsExactElementsInAnyOrder(new[] { "SC1", "SC1" }, outerOverPack1.PackingLineCollection.Select(p => (string)p.ScreeningMethod));

			AssertContainsExactElementsInAnyOrder(new[] { "OuterPack2_InnerPackage1" }, outerOverPack2.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertEquals("OV2", outerOverPack2.ScreeningMethod);
			AssertEquals("OV2", outerOverPack2.AviationSecurityInspectionType.Code);
			AssertContainsExactElementsInAnyOrder(new[] { "SC8" }, outerOverPack2.PackingLineCollection.Select(p => (string)p.ScreeningMethod));

			AssertContainsExactElementsInAnyOrder(new[] { "OuterPack3_InnerPackage1" }, outerOverPack3.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertEquals("OV3", outerOverPack3.ScreeningMethod);
			AssertEquals("OV3", outerOverPack3.AviationSecurityInspectionType.Code);
			var ovp3PacklineScreeningMethods = outerOverPack3.PackingLineCollection.Select(p => (string)p.ScreeningMethod).ToList();
			AssertEquals(1, ovp3PacklineScreeningMethods.Count);
			AssertNull(ovp3PacklineScreeningMethods[0]);

			AssertContainsExactElementsInAnyOrder(new[] { "OuterPack4_InnerPackage1", "OuterPack4_InnerPackage2" }, outerOverPack4.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertNull(outerOverPack4.ScreeningMethod);
			AssertNull(outerOverPack4.AviationSecurityInspectionType);
			AssertContainsExactElementsInAnyOrder(new[] { "SC2", "SC3" }, outerOverPack4.PackingLineCollection.Select(p => (string)p.ScreeningMethod));
		}

		#endregion

		#region TestPopulateDataObject_Addresses

		public void TestPopulateDataObject_With_BookingParty_Consignee_Consignor_DeliveryAddress_TransportCompany()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var bookedByParty = data.Org1;
			var consignor = data.Org2;
			var consignee = data.Org3;
			var deliveryAddress = data.Org4;
			var transportCompany = data.Org5;
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK, bookedByParty, consignor, consignee, deliveryAddress, transportCompany);

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			var organizationAddresses = consignmentDataObject.OrganizationAddressCollection;
			CombineAssertions(() =>
			{
				AssertEquals(6, organizationAddresses.Count);
				AssertEquals("Booking Party", bookedByParty.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "BookingPartyDocumentaryAddress").OrganizationCode);
				AssertEquals("Consignor", consignor.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "LocalCartageExporter").OrganizationCode);
				AssertEquals("Consignee", consignee.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "ConsigneeDocumentaryAddress").OrganizationCode);
				AssertEquals("Delivery Address", deliveryAddress.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "ConsigneePickupDeliveryAddress").OrganizationCode);
				AssertEquals("Transport Company", transportCompany.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "TransportCompanyDocumentaryAddress").OrganizationCode);
				AssertEquals(Data.Orgs.WUFSHIJNB.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "LocalCartageCFS").OrganizationCode);
			});
		}

		public void TestPopulateDataObject_WithOnlyWarehouseAddress()
		{
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			var organizationAddresses = consignmentDataObject.OrganizationAddressCollection;
			CombineAssertions(() =>
			{
				AssertEquals(1, organizationAddresses.Count);
				AssertEquals(Data.Orgs.WUFSHIJNB.OH_Code, organizationAddresses.Single(o => o.AddressType.Value == "LocalCartageCFS").OrganizationCode);
			});
		}

		#endregion

		#region TestPopulateDataObject_ContainerDates

		[TestDate(2020, 08, 28, 04, 39, 50)]
		public void TestPopulateDataObject_Container_Dates()
		{
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);
			var dispatchTransportationUnit = helper.CreateDispatchTransportationUnitWithContainerType("Good Vehicle", Data.Warehouse.PK);
			dispatchTransportationUnit.WDH_VehicleReference = "VEH1";
			var currentTime = ZDateTimeOffset.Now;
			dispatchTransportationUnit.WDH_LoadCompleteTime = currentTime;

			helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, dispatchUnit: dispatchTransportationUnit, dispatchConsignment: dispatchConsignment);

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			AssertNotNull(consignmentDataObject?.ContainerCollection);
			AssertContainsExactElementsInAnyOrder("Should be the dispatch transportation unit's load time", new[] { currentTime.ToZDateTime() }, consignmentDataObject.ContainerCollection.Select(c => c.PackDate.Value));
		}

		#endregion

		#region TestPopulateDataObject_AdditionalReferences

		public void TestPopulateDataObject_AdditionalReferences()
		{
			var dispatchConsignment = helper.CreateDispatchConsignment("DCNExternalRef", Data.Warehouse.PK, jobID: "DCNJOBID");
			var reference1 = helper.CreateAdditionalReference(dispatchConsignment, "REF1");
			var reference2 = helper.CreateAdditionalReference(dispatchConsignment, "REF2");

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			var references = consignmentDataObject.AdditionalReferenceCollection;
			CombineAssertions(() =>
			{
				AssertEquals(CollectionContent.Partial, consignmentDataObject.AdditionalReferenceCollection.Content);
				AssertEquals(4, references.Count);
				AssertEquals("AAS", references.Single(r => r.ReferenceNumber.Equals(reference1.CE_EntryNum)).Type.Code);
				AssertEquals("AAS", references.Single(r => r.ReferenceNumber.Equals(reference2.CE_EntryNum)).Type.Code);
				AssertEquals(TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatch, references.Single(r => r.ReferenceNumber.Equals("DCNJOBID")).Type.Code);
				AssertEquals(TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchReference, references.Single(r => r.ReferenceNumber.Equals("DCNExternalRef")).Type.Code);
			});
		}

		#endregion

		#region TestPopulateDataObject_Notes

		public void TestPopulateDataObject_Notes()
		{
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);
			var note1 = dispatchConsignment.Notes.AddNew(true, "Custom Description", "Test Note 1");
			var note2 = dispatchConsignment.Notes.AddNew(false, "Non-custom Description", "Test Note 2");

			var noteTypesNotPopulate = TransitWarehouseNoteHelper.GetNoteTypesNotPopulate();
			foreach (var noteTypeDescription in noteTypesNotPopulate)
			{
				dispatchConsignment.Notes.AddNew(false, noteTypeDescription, $"Note Text for {noteTypeDescription}");
			}

			Factory.SaveForTesting();
			var notesInDCN = Factory.BOFactory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_Table, WhsItemDispatchConsignmentSchema.Constants.TableName));
			CombineAssertions(() =>
			{
				AssertEquals(2 + noteTypesNotPopulate.Count, notesInDCN.Length);
				AssertEquals("Test Note 1", note1.ST_NoteText, notesInDCN.Single(r => r.ST_Description.Equals(note1.ST_Description)).ST_NoteText);
				AssertEquals("Test Note 2", note2.ST_NoteText, notesInDCN.Single(r => r.ST_Description.Equals(note2.ST_Description)).ST_NoteText);
				foreach (var noteTypeDescription in noteTypesNotPopulate)
				{
					AssertEquals($"Note Text for {noteTypeDescription}", notesInDCN.Single(r => r.ST_Description.Equals(noteTypeDescription)).ST_NoteText);
				}
			});

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			var notesInDataObject = consignmentDataObject.NoteCollection;
			CombineAssertions(() =>
			{
				AssertEquals(2, notesInDataObject.Count);
				AssertEquals("Test Note 1", note1.ST_NoteText, notesInDataObject.Single(r => r.Description.HasValue && r.NoteText.HasValue && r.Description.Value == note1.ST_Description).NoteText.Value);
				AssertEquals("Test Note 2", note2.ST_NoteText, notesInDataObject.Single(r => r.Description.HasValue && r.NoteText.HasValue && r.Description.Value == note2.ST_Description).NoteText.Value);
			});
		}

		#endregion

		#region TestPopulateDataObject_AdditionalServices

		public void TestPopulateDataObject_AdditionalServices()
		{
			var dispatchConsignment = helper.CreateDispatchConsignment("RCN1", Data.Warehouse.PK);

			var service1 = dispatchConsignment.Services.AddNew();
			service1.ES_ServiceCode = "S1";
			var service2 = dispatchConsignment.Services.AddNew();
			service2.ES_ServiceCode = "S2";

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			var services = consignmentDataObject.LocalProcessing.AdditionalServiceCollection;

			CombineAssertions(() =>
			{
				AssertEquals(2, services.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "S1", "S2" }, consignmentDataObject.LocalProcessing.AdditionalServiceCollection.Select(x => x.ServiceCode.Code.Value));
			});
		}

		#endregion

		#region TestPopulateDataObject_DispatchTransportationUnits

		public void TestPopulateDataObject_DispatchTransportationUnits()
		{
			var location = helper.CreateLocation(Data.Warehouse);
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);

			var dispatchTransportationUnit1 = helper.CreateDispatchTransportationUnit("dtu1", Data.Warehouse.PK);
			var dispatchTransportationUnit2 = helper.CreateDispatchTransportationUnit("dtu2", Data.Warehouse.PK);
			var dispatchTransportationUnitWithContainer = helper.CreateDispatchTransportationUnitWithContainerType("dtu3", Data.Warehouse.PK, "Container1");

			dispatchTransportationUnit1.WDH_VehicleReference = "Vehicle1";
			dispatchTransportationUnit2.WDH_VehicleReference = "Vehicle2";
			dispatchTransportationUnitWithContainer.WDH_VehicleReference = "Vehicle3";

			var packageState1 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnit1);
			var packageState2 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnit2);
			var packageState3 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.FreightLoaded, dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnitWithContainer);

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			var dispatchTransportationUnits = consignmentDataObject.RelatedShipmentCollection;
			AssertNotNull("DCN's dispatch transportation units", dispatchTransportationUnits);
			const string dtuErrorMessage = "DCN should write its DTUs once each.";
			AssertContainsExactElementsInAnyOrder(dtuErrorMessage, new ZString?[] { "Vehicle1", "Vehicle2", "Vehicle3" }, dispatchTransportationUnits.Select(dtu => dtu.VesselName));
			var dtu1 = dispatchTransportationUnits.Single(dtu => dtu.VesselName.Equals("Vehicle1"));
			var dtu2 = dispatchTransportationUnits.Single(dtu => dtu.VesselName.Equals("Vehicle2"));
			var dtu3 = dispatchTransportationUnits.Single(dtu => dtu.VesselName.Equals("Vehicle3"));
			CombineAssertions(() =>
			{
				AssertNull("Dispatch Transportation Unit 1 should not have a container collection", dtu1.ContainerCollection);
				AssertNull("Dispatch Transportation Unit 2 should not have a container collection", dtu2.ContainerCollection);
				AssertEquals("Dispatch Transportation Unit 3 with Container should have one container collection", "Container1", dtu3.ContainerCollection.SingleOrDefault()?.ContainerNumber);

				var packLines = consignmentDataObject.PackingLineCollection;
				AssertEquals("Each package must have a pack line.", 3, packLines.Count);
				var packLineInDTU1 = packLines.Single(p => p.ReferenceNumber.Value == "PKG1");
				var packLineInDTU2 = packLines.Single(p => p.ReferenceNumber.Value == "PKG2");
				var packLineInDTU3 = packLines.Single(p => p.ReferenceNumber.Value == "PKG3");
				AssertPackLineReference(packLineInDTU1, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchTransportationUnit, "dtu1");
				AssertPackLineReference(packLineInDTU2, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchTransportationUnit, "dtu2");
				AssertPackLineReference(packLineInDTU3, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchTransportationUnit, "dtu3");
			});
		}

		static void AssertPackLineReference(PackingLine packLine, string refType, string reference)
		{
			var referenceNumber = packLine.ReferenceNumberCollection.Single(r => r.Type.Code.Value == refType);
			AssertEquals(reference, referenceNumber.ReferenceNumber.Value);
		}

		#endregion

		#region TestPopulateDataObject_DispatchLoadLists

		public void TestPopulateDataObject_DispatchLoadLists()
		{
			var location = helper.CreateLocation(Data.Warehouse);
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);

			var dispatchTransportationUnit1 = helper.CreateDispatchTransportationUnit("dtu1", Data.Warehouse.PK);
			var loadList1 = helper.CreateDispatchLoadList("DLL1", Data.Warehouse.PK);
			var loadList2 = helper.CreateDispatchLoadList("DLL2", Data.Warehouse.PK);

			var packageState1 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnit1, dispatchLoadList: loadList1);
			var packageState2 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnit1, dispatchLoadList: loadList2);
			var packageState3 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, dispatchConsignment: dispatchConsignment, dispatchUnit: dispatchTransportationUnit1, dispatchLoadList: loadList1);
			var packageWithNoLoadList = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, dispatchConsignment: dispatchConsignment);

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			var relatedshipments = consignmentDataObject.RelatedShipmentCollection;
			AssertContainsExactElementsInAnyOrder(new ZString?[] { "dtu1" }, relatedshipments.Select(d => d.DataContext.DataSourceCollection.Single().Key).Distinct());

			var subshipments = consignmentDataObject.SubShipmentCollection;
			AssertContainsExactElementsInAnyOrder(new ZString?[] { "DLL1", "DLL2" }, subshipments.Select(d => d.DataContext.DataSourceCollection.Single().Key).Distinct());

			var packLines = consignmentDataObject.PackingLineCollection;
			AssertEquals("Each package must have a pack line.", 4, packLines.Count);
			var packLineForPKG1 = packLines.Single(p => p.ReferenceNumber.Value == "PKG1");
			var packLineForPKG2 = packLines.Single(p => p.ReferenceNumber.Value == "PKG2");
			var packLineForPKG3 = packLines.Single(p => p.ReferenceNumber.Value == "PKG3");
			var packLineForPKG4 = packLines.Single(p => p.ReferenceNumber.Value == "PKG4");
			AssertPackLineReference(packLineForPKG1, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchTransportationUnit, "dtu1");
			AssertPackLineReference(packLineForPKG1, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchLoadList, "DLL1");

			AssertPackLineReference(packLineForPKG2, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchTransportationUnit, "dtu1");
			AssertPackLineReference(packLineForPKG2, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchLoadList, "DLL2");

			AssertPackLineReference(packLineForPKG3, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchTransportationUnit, "dtu1");
			AssertPackLineReference(packLineForPKG3, TWAdditionalReferenceTypesForUniversalXML.Codes.TransitWarehouseDispatchLoadList, "DLL1");
			AssertNull("No reference number collections since there are no DLLs or DTUs.", packLineForPKG4.ReferenceNumberCollection);
		}

		#endregion

		#region TestPopulateDataObject_TransportMode

		public void TestPopulateDataObject_TransportMode_Sea() => TestPopulateDataObject_TransportMode(Constants.TransportModes.Sea, "Sea Freight");

		public void TestPopulateDataObject_TransportMode_Air() => TestPopulateDataObject_TransportMode(Constants.TransportModes.Air, "Air Freight");

		public void TestPopulateDataObject_TransportMode_AirSea() => TestPopulateDataObject_TransportMode(Constants.TransportModes.AirSea, null);

		public void TestPopulateDataObject_TransportMode_Rail() => TestPopulateDataObject_TransportMode(Constants.TransportModes.Rail, "Rail Freight");

		public void TestPopulateDataObject_TransportMode_Pedestrian() => TestPopulateDataObject_TransportMode(Constants.TransportModes.Pedestrian, null);

		void TestPopulateDataObject_TransportMode(string transportMode, string outputDescription)
		{
			var location = helper.CreateLocation(Data.Warehouse);
			helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			dispatchConsignment.WDC_TransportMode = transportMode;

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			AssertEquals(transportMode, consignmentDataObject.TransportMode.Code);
			AssertEquals(outputDescription, consignmentDataObject.TransportMode.Description);

			dispatchConsignment.WDC_TransportMode = "";
			var consignmentDataObjectWriterForEmptyTransportMode = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObjectForEmptyTransportMode = consignmentDataObjectWriterForEmptyTransportMode.GetDataObject(dispatchConsignment);

			AssertNull(consignmentDataObjectForEmptyTransportMode.TransportMode);
		}

		#endregion

		#region TestPopulateDataObject_PackagesWithAttachedShipmentID

		public void TestPopulateDataObject_PackagesWithAttachedShipmentID()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);

			var packageState = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);
			helper.CreateAdditionalReference(packageState, "S0000001", AdditionalReferenceTypes.Codes.BookingPartyReference);

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			AssertEquals("Packages on the DCN's package job should have AddInfo of ShipmentID.",
				"S0000001", consignmentDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1").AddInfoCollection.Single(a => a.Key.Equals(AddInfoKeyTypes.Types.ForwardingShipment)).Value);
		}

		#endregion

		#region TestPopulateDataObject_OrderReferences

		public void TestPopulateDataObject_OrderReferences()
		{
			var receiveConsignment = helper.CreateReceiveConsignment("RCN", "STD", Data.Warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);
			var packageState1 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);
			var packageState2 = helper.CreatePackageState(receiveConsignment, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);

			var orderReference1 = helper.PackingHelper.CreatePackageOrderReference(packageState1.Package, "ORN1", "BAT1", "CIN1", new ZDate(2024, 01, 01), "LNE1", "SKU1", "SRN1");
			var orderReference2 = helper.PackingHelper.CreatePackageOrderReference(packageState2.Package, "ORN2", "BAT2", "CIN2", new ZDate(2024, 01, 02), "LNE2", "SKU2", "SRN2");

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			var references = consignmentDataObject.RelatedShipmentCollection;
			var packlines = consignmentDataObject.PackingLineCollection;
			CombineAssertions(() =>
			{
				AssertEquals(2, references.Count);
				var orderLine1 = references.Single(r => r.Order.OrderNumber.Value == "ORN1").Order.OrderLineCollection.First();
				var orderLine2 = references.Single(r => r.Order.OrderNumber.Value == "ORN2").Order.OrderLineCollection.First();
				AssertOrderReference(orderLine1, "BAT1", "CIN1", new ZDateTime(2024, 01, 01), "LNE1", "SKU1", "SRN1", 0);
				AssertOrderReference(orderLine2, "BAT2", "CIN2", new ZDateTime(2024, 01, 02), "LNE2", "SKU2", "SRN2", 1);
				var packedItem1 = packlines.Single(p => p.ReferenceNumber.Value == "PKG1").PackedItemCollection.First();
				var packedItem2 = packlines.Single(p => p.ReferenceNumber.Value == "PKG2").PackedItemCollection.First();
				AssertEquals(0, packedItem1.OrderLineLink);
				AssertEquals(1, packedItem2.OrderLineLink);
			});
		}

		void AssertOrderReference(OrderLine orderLine, string batchNumber, string commercialInvoiceNumber, ZDateTime expiryDate, string lineReference, string skuPartNumber, string serialNumber, int link)
		{
			AssertEquals(batchNumber, orderLine.BatchNumber);
			AssertEquals(commercialInvoiceNumber, orderLine.CommercialInvoiceNumber);
			AssertEquals(expiryDate, orderLine.ExpiryDate);
			AssertEquals(lineReference, orderLine.LineReference);
			AssertEquals(link, orderLine.Link);
			AssertEquals(skuPartNumber, orderLine.Product.Code);
			AssertEquals(serialNumber, orderLine.SerialNumber);
		}

		#endregion

		#region TestExportDispatchConsignmentWithPortReferences

		public void TestExportDispatchConsignmentWithPortReferences_OnlyHasPEN() => TestExportDispatchConsignmentWithPortReferencesCore(true, false);

		public void TestExportDispatchConsignmentWithPortReferences_OnlyHasPAN() => TestExportDispatchConsignmentWithPortReferencesCore(false, true);

		public void TestExportDispatchConsignmentWithPortReferences_HasPANAndPEN() => TestExportDispatchConsignmentWithPortReferencesCore(true, true);

		void TestExportDispatchConsignmentWithPortReferencesCore(bool hasPEN, bool hasPAN)
		{
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			if (hasPEN)
			{
				helper.CreateCustomsAdditionalReference(dispatchConsignment, TransitWarehousePortReferenceTypes.Codes.PortExport, "BBE001", TransitWarehouseReferenceCategories.Codes.PortReference, "CLS", "AU");
			}
			if (hasPAN)
			{
				helper.CreateCustomsAdditionalReference(dispatchConsignment, TransitWarehousePortReferenceTypes.Codes.PortAuthority, "BBE002", TransitWarehouseReferenceCategories.Codes.PortReference, "CLS", "AU");
			}

			var expectedPortReferenceNumber = hasPEN ? "BBE001" : "BBE002";
			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			var references = consignmentDataObject.PortReferenceCollection;
			if (hasPAN || hasPEN)
			{
				CombineAssertions(() =>
				{
					if (hasPAN && hasPEN)
					{
						AssertNull("Should not poplulate old PAN if PEN exists", references.FirstOrDefault(r => r.Reference.Value == "BBE002"));
					}

					var panReference = references.First(r => r.Type.Code.Value == TransitWarehousePortReferenceTypes.Codes.PortAuthority);
					helper.AssertPortReference(panReference, TransitWarehousePortReferenceTypes.Codes.PortAuthority, expectedPortReferenceNumber, "CLS", "AU");

					var dcnPortReferences = dispatchConsignment.PortReferences.Cast<CusEntryNumber>().ToList();
					if (hasPEN)
					{
						AssertEquals(1, dcnPortReferences.Count(r => r.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortExport));
						AssertEquals("Should not modify the existing PEN", "BBE001", dcnPortReferences.Single(r => r.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortExport).CE_EntryNum);
					}
					if (hasPAN)
					{
						AssertEquals(1, dcnPortReferences.Count(r => r.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortAuthority));
						AssertEquals("Should not modify the existing PAN", "BBE002", dcnPortReferences.Single(r => r.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortAuthority).CE_EntryNum);
					}
				});
			}
		}

		#endregion

		#region TestPopulateDataObject_ValidationRuleCollection

		public void TestPopulateDataObject_ValidationRuleCollection()
		{
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			AssertEquals(0, consignmentDataObject.ValidationRuleCollection.Count);
		}

		#endregion

		#region TestPopulateDataObject_Destination

		public void TestPopulateDataObject_Destination()
		{
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			dispatchConsignment.WDC_RL_NKDestination = "ARFLO";

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			AssertEquals("ARFLO", consignmentDataObject.PortOfDestination.Code);
			AssertEquals("Florida", consignmentDataObject.PortOfDestination.Name);
		}

		#endregion

		#region TestPopulateDataObject_ContentType

		public void TestPopulateDataObject_ContentType()
		{
			var shipmentJobPK1 = ZGuid.NewZGuid();
			var receiveConsignment = helper.CreateReceiveConsignment("RCN", "STD", Data.Warehouse.PK);
			helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK, jobID: "EXTREF1", parentPK: shipmentJobPK1, parentCode: "JS");
			var dispatchConsignment2 = helper.CreateDispatchConsignment("DCN2", Data.Warehouse.PK, jobID: "EXTREF1", parentPK: shipmentJobPK1, parentCode: "JS");
			helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment2);

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment2)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment2);

			var packlines = consignmentDataObject.PackingLineCollection;
			AssertEquals("content type shoule be Partial.", CollectionContent.Partial, packlines.Content);
		}

		#endregion

		#region TestPopulateDataObject_OrderNumber

		public void TestPopulateDataObject_OrderNumber()
		{
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var order1 = helper.CreateWhsItemConsignmentOrderReference("ORDER1", dispatchConsignment);
			var order2 = helper.CreateWhsItemConsignmentOrderReference("ORDER2", dispatchConsignment);
			var order3 = helper.CreateWhsItemConsignmentOrderReference("ORDER3", dispatchConsignment);

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);
			var consignmentOrderReferenceCollection = consignmentDataObject.LocalProcessing.OrderNumberCollection;

			AssertEquals(3, consignmentDataObject.LocalProcessing.OrderNumberCollection.Count);
			AssertCollectionContains("ORDER1", consignmentOrderReferenceCollection.Select(order => order.OrderReference).ToList());
			AssertCollectionContains("ORDER2", consignmentOrderReferenceCollection.Select(order => order.OrderReference).ToList());
			AssertCollectionContains("ORDER3", consignmentOrderReferenceCollection.Select(order => order.OrderReference).ToList());
		}

		#endregion

		#endregion

		#region TestPopulateDataObject_DispatchConsignment_PackagesAndHandlingUnits

		public void TestPopulateDataObject_DispatchConsignment_PackagesAndHandlingUnits()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var receiveConsignment2 = helper.CreateReceiveConsignment("RCN2", "STD", Data.Warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);

			var packageState1 = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);
			var packageState2 = helper.CreatePackageState(receiveConsignment2, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);
			var packageState3 = helper.CreatePackageState(receiveConsignment2, 1, "PLT", "PKG3", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);

			var location = helper.CreateLocation(Data.Warehouse);
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);
			var handlingUnit = helper.CreatePackageState(receiveTransportationUnit, 1, "PLT", "HU1", TransitWarehouseStatuses.Codes.Arrived);
			handlingUnit.WPS_IsHandlingUnit = true;
			handlingUnit.WPS_UnitType = PackageStateUnitType.Codes.HandlingUnit;

			helper.PackPackageIntoHandlingUnit(handlingUnit, packageState1, DateTime.Now, "BOB", handlingUnit);

			Factory.SaveForTesting();

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			const string packageErrorMessage = "DCN PackingLineCollection should include all their packages except handling units.";
			const string huErrorMessage = "DCNs ParentPackingLineCollection should include all their HUs.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new[] { "PKG1", "PKG2", "PKG3" }, consignmentDataObject.PackingLineCollection.Select(x => x.ReferenceNumber.Value));
			AssertContainsExactElementsInAnyOrder(huErrorMessage, new[] { "HU1" }, consignmentDataObject.ParentPackingLineCollection.Select(x => x.ReferenceNumber.Value));
		}

		public void TestPopulateDataObject_DispatchConsignment_PackagesAndMultipleHandlingUnits()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var receiveConsignment2 = helper.CreateReceiveConsignment("RCN2", "STD", Data.Warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);

			var packageState1 = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);
			var packageState2 = helper.CreatePackageState(receiveConsignment2, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);
			var packageState3 = helper.CreatePackageState(receiveConsignment2, 1, "PLT", "PKG3", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);

			var location = helper.CreateLocation(Data.Warehouse);
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);

			var handlingUnit1 = helper.CreateHandlingUnitPackage("HU1", helper.CreatePackageHandlingUnit(), receiveTransportationUnit);
			helper.PackPackageIntoHandlingUnit(handlingUnit1, packageState1, DateTime.Now, "BOB", handlingUnit1);

			var handlingUnit2 = helper.CreateHandlingUnitPackage("HU2", helper.CreatePackageHandlingUnit(), receiveTransportationUnit);
			helper.PackPackageIntoHandlingUnit(handlingUnit2, packageState2, DateTime.Now, "BOB", handlingUnit2);

			Factory.SaveForTesting();

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			const string packageErrorMessage = "DCN PackingLineCollection should include all their packages except handling units.";
			const string huErrorMessage = "DCNs ParentPackingLineCollection should include all their HUs.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new[] { "PKG1", "PKG2", "PKG3" }, consignmentDataObject.PackingLineCollection.Select(x => x.ReferenceNumber.Value));
			AssertContainsExactElementsInAnyOrder(huErrorMessage, new[] { "HU1", "HU2" }, consignmentDataObject.ParentPackingLineCollection.Select(x => x.ReferenceNumber.Value));
		}

		public void TestPopulateDataObject_DispatchConsignment_PackagesAndHandlingUnits_WithMultiLevelHandlingUnits()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var receiveConsignment2 = helper.CreateReceiveConsignment("RCN2", "STD", Data.Warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);

			var packageState1 = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);
			var packageState2 = helper.CreatePackageState(receiveConsignment2, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);
			var packageState3 = helper.CreatePackageState(receiveConsignment2, 1, "PLT", "PKG3", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);

			var location = helper.CreateLocation(Data.Warehouse);
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);

			var handlingUnit1 = helper.CreatePackageState(receiveTransportationUnit, 1, "PLT", "HU1", TransitWarehouseStatuses.Codes.Arrived);
			handlingUnit1.WPS_IsHandlingUnit = true;
			handlingUnit1.WPS_UnitType = PackageStateUnitType.Codes.HandlingUnit;

			helper.PackPackageIntoHandlingUnit(handlingUnit1, packageState1, DateTime.Now, "BOB", handlingUnit1);

			var handlingUnit2 = helper.CreatePackageState(receiveTransportationUnit, 1, "PLT", "HU2", TransitWarehouseStatuses.Codes.Arrived);
			handlingUnit2.WPS_IsHandlingUnit = true;
			handlingUnit2.WPS_UnitType = PackageStateUnitType.Codes.HandlingUnit;

			helper.PackPackageIntoHandlingUnit(handlingUnit2, packageState2, DateTime.Now, "BOB", handlingUnit1);

			helper.PackPackageIntoHandlingUnit(handlingUnit1, handlingUnit2, DateTime.Now, "BOB", handlingUnit1);

			Factory.SaveForTesting();

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			const string packageErrorMessage = "DCN PackingLineCollection should include all their packages except handling units.";
			const string huErrorMessage = "DCNs ParentPackingLineCollection should include all their top level HUs and also should not include inner HUs";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new[] { "PKG1", "PKG2", "PKG3" }, consignmentDataObject.PackingLineCollection.Select(x => x.ReferenceNumber.Value));
			AssertContainsExactElementsInAnyOrder(huErrorMessage, new[] { "HU1" }, consignmentDataObject.ParentPackingLineCollection.Select(x => x.ReferenceNumber.Value));
		}

		public void TestPopulateDataObject_DispatchConsignment_PackagesAndHandlingUnits_HandlingUnitsAreAttachedMultipleDispatchConsignment()
		{
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var dispatchConsignment1 = helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dispatchConsignment2 = helper.CreateDispatchConsignment("DCN2", Data.Warehouse.PK);

			var packageState1 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment1);
			var packageState2 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment2);

			var location = helper.CreateLocation(Data.Warehouse);
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit("RTU1", Data.Warehouse.PK, location.PK);
			var handlingUnit = helper.CreatePackageState(receiveTransportationUnit, 1, "PLT", "HU1", TransitWarehouseStatuses.Codes.Arrived);
			handlingUnit.WPS_IsHandlingUnit = true;
			handlingUnit.WPS_UnitType = PackageStateUnitType.Codes.HandlingUnit;

			helper.PackPackageIntoHandlingUnit(handlingUnit, packageState1, DateTime.Now, "ABC", handlingUnit);
			helper.PackPackageIntoHandlingUnit(handlingUnit, packageState2, DateTime.Now, "ABC", handlingUnit);

			Factory.SaveForTesting();

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment1)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment1);

			const string packageErrorMessage = "DCN PackingLineCollection should include packages PKG1.";
			const string huErrorMessage = "DCNs ParentPackingLineCollection should be null.";
			AssertNotNull(packageErrorMessage, consignmentDataObject.PackingLineCollection.FirstOrDefault(p => p.ReferenceNumber.Value == "PKG1"));
			AssertNull(huErrorMessage, consignmentDataObject.ParentPackingLineCollection);
		}

		#endregion

		#region TestPopulateDataObject_AdjustedOutPackage

		public void TestPopulateDataObject_AdjustedOutPackage()
		{
			var receiveConsignment = helper.CreateReceiveConsignment("RCN", "STD", Data.Warehouse.PK);
			var dispatchConsignment = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);
			helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dispatchConsignment);
			helper.CreatePackageState(receiveConsignment, 1, "PLT", "PKG2", TransitWarehouseStatuses.Codes.AdjustedOut, dispatchConsignment: dispatchConsignment);

			var consignmentDataObjectWriter = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, dispatchConsignment)));
			var consignmentDataObject = consignmentDataObjectWriter.GetDataObject(dispatchConsignment);

			AssertEquals("DCN should only 1 Package", 1, consignmentDataObject.PackingLineCollection.Count);
			AssertEquals("DCN should include a package PKG1.", "PKG1", consignmentDataObject.PackingLineCollection.Single().ReferenceNumber);
		}

		#endregion

		#region TestPopulateDataObject_EmptyPackingLineCollection

		public void TestPopulateDataObject_EmptyPackingLineCollection()
		{
			var dcn = helper.CreateDispatchConsignment("DCN", Data.Warehouse.PK);
			var writer = new WhsTransitDispatchConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dcn);

			AssertNotNull(dataObject.PackingLineCollection);
			AssertEquals("Content should be completed", CollectionContent.Complete, dataObject.PackingLineCollection.Content);
		}

		#endregion

		#region Implementation

		void AssertOutturnDetail(PackingLine packingLine, int expectedOutturnQty, ZDateTime? expectedUnloadTime, ZDateTime? expectedLoadTime, int expectedOutturnDamagedQty, decimal expectedOutturnedHeight, decimal expectedOtturnedLength, decimal expectedOutturnedVolume, decimal expectedOutturnedWeight, decimal expectedOutturnedWidth)
		{
			AssertEquals(expectedOutturnQty, packingLine.OutturnQty);
			AssertEquals(expectedUnloadTime, packingLine.UnloadDate);
			AssertEquals(expectedLoadTime, packingLine.LoadDate);
			AssertEquals(expectedOutturnDamagedQty, packingLine.OutturnDamagedQty);
			AssertEquals(expectedOutturnedHeight, packingLine.OutturnedHeight);
			AssertEquals(expectedOtturnedLength, packingLine.OutturnedLength);
			AssertEquals(expectedOutturnedVolume, packingLine.OutturnedVolume);
			AssertEquals(expectedOutturnedWeight, packingLine.OutturnedWeight);
			AssertEquals(expectedOutturnedWidth, packingLine.OutturnedWidth);
		}

		WhsTransitTestHelper helper => new WhsTransitTestHelper(Factory.BOFactory);

		#endregion
	}
}
