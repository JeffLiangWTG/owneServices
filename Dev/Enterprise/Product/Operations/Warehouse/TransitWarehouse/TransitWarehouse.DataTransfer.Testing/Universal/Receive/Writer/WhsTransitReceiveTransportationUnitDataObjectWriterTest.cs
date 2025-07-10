using System.Linq;
using CargoWise.Definitions;
using CargoWise.Definitions.Customs;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitReceiveTransportationUnitDataObjectWriterTest : TransitUniversalTestCase
	{
		#region TestTopLevelDataContextType

		public void TestTopLevelDataContextType()
		{
			AssertEquals(DataContextType.TransitReceiveHeader,
				((ITopLevelDataObjectWriter)new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).TopLevelDataContextType);
		}

		#endregion

		#region TestEDIMessageSubType

		public void TestEDIMessageSubType()
		{
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment,
				((ITopLevelDataObjectWriter)new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).EDIMessageSubType);
		}

		#endregion

		#region PopulateDataObject Tests

		#region TestBasicFieldMappings

		public void TestBasicFieldMappings()
		{
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");
			receiveTransportationUnit.WRH_VehicleReference = "Good Vehicle";

			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveTransportationUnit);

			AssertNotNull(dataObject);
			AssertEquals("Should be the receive transportation unit's staging location", "Dock-1-1", dataObject.WarehouseLocation.ToString());
			AssertEquals("Should be the receive transportation unit's vehicle reference", "Good Vehicle", dataObject.VesselName.ToString());
		}

		#endregion

		#region TestPopulateDataObject_Packages

		public void TestPopulateDataObject_PackagesOnRTU()
		{
			var receiveTranportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");

			helper.CreatePackageState(receiveTranportationUnit, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveTranportationUnit)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(receiveTranportationUnit);

			const string errorMessage = "Packages on the RTU's package job should be on its own packline collection.";
			AssertNotNull(errorMessage, receiveTransportationUnitDataObject.PackingLineCollection);
			AssertContainsExactElementsInAnyOrder(errorMessage, new string[] { "PKG1" }, receiveTransportationUnitDataObject.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
		}

		public void TestPopulateDataObject_PackagesWithAttachedShipmentID()
		{
			var receiveTranportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");

			var packageState = helper.CreatePackageState(receiveTranportationUnit, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived);
			helper.CreateAdditionalReference(packageState, "S0000001", AdditionalReferenceTypes.Codes.BookingPartyReference);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveTranportationUnit)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(receiveTranportationUnit);

			AssertEquals("Packages on the RTU's package job should have AddInfo of ShipmentID.",
				"S0000001", receiveTransportationUnitDataObject.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1").AddInfoCollection.Single(a => a.Key.Equals(AddInfoKeyTypes.Types.ForwardingShipment)).Value);
		}

		public void TestPopulateDataObject_PackagesOnConsignments()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId1 = receiveConsignment1.WRC_JobID;
			var receiveConsignment2 = helper.CreateReceiveConsignment("RCN2", "STD", Data.Warehouse.PK);
			var jobId2 = receiveConsignment2.WRC_JobID;
			var receiveConsignment3 = helper.CreateReceiveConsignment("RCN3", "STD", Data.Warehouse.PK);
			var receiveTranportationUnit1 = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");
			var receiveTranportationUnit2 = SetupSimpleReceiveTransportationUnitForTesting("Not As Good");

			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTranportationUnit1);
			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTranportationUnit2);
			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked);
			helper.CreatePackageState(receiveConsignment2, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTranportationUnit1);
			helper.CreatePackageState(receiveConsignment3, 1, "PKG", "PKG5", TransitWarehouseStatuses.Codes.Booked);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveTranportationUnit1)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(receiveTranportationUnit1);

			var subShipmentKeys = receiveTransportationUnitDataObject.SubShipmentCollection?.Select(s => s.DataContext.DataSourceCollection.Single().Key.Value);
			AssertContainsExactElementsInAnyOrder("RCNs found through packages on the RTU should be included as subshipments.", new string[] { jobId1, jobId2 }, subShipmentKeys);

			var rcn1 = receiveTransportationUnitDataObject.SubShipmentCollection.Single(s => jobId1 == s.DataContext.DataSourceCollection.Single().Key.Value);
			var rcn2 = receiveTransportationUnitDataObject.SubShipmentCollection.Single(s => jobId2 == s.DataContext.DataSourceCollection.Single().Key.Value);
			const string packageErrorMessage = "RCNs should include all packages on their package job.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG1", "PKG2", "PKG3" }, rcn1.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG4" }, rcn2.PackingLineCollection.Select(p => p.ReferenceNumber.Value));

			AssertNull(receiveTransportationUnitDataObject.ContainerCollection);
		}

		#endregion

		#region TestPopulateDataObject_PopulateOutturnDetail

		public void TestPopulateDataObject_PopulateOutturnDetail()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId1 = receiveConsignment1.WRC_JobID;

			var rtu = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1");
			var asn = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			var pivot = helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			var packageState1 = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, receiveASN: asn);
			var packageState2 = helper.CreatePackageState(receiveConsignment1, 9, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);

			helper.SetupPackageForOutturn(packageState1.Package, bookedQty: 1, damagedQty: 1, height: 1, length: 3, volume: 15, weight: 6, width: 5);
			helper.SetupPackageForOutturn(packageState2.Package, bookedQty: 9, damagedQty: 0, height: 2, length: 4, volume: 48, weight: 4, width: 6);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(rtu);

			var rcn1SubShipment = receiveTransportationUnitDataObject.SubShipmentCollection.Single();
			AssertEquals("RCN found through packages on the RTU should be included as subshipments.", jobId1, rcn1SubShipment.DataContext.DataSourceCollection.Single().Key.Value);

			const string packageErrorMessage = "RCNs should include all packages on their package job.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG1", "PKG2" }, rcn1SubShipment.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			var packingLine1 = rcn1SubShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1");
			var packingLine2 = rcn1SubShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2");

			AssertOutturnDetail(packingLine1, 1, 1, 1, 3, 15, 6, 5);
			AssertOutturnDetail(packingLine2, 0, 0, 0, 0, 0, 0, 0);
		}

		#endregion

		#region TestPopulateDataObject_PopulateSeaCargoOutturnDataForMatching

		public void TestPopulateDataObject_PopulateSeaCargoOutturnDataForMatching_UsingLatestASNWithVesselLloyds_FromPivot()
		{
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID123");
			Factory.SaveForTesting();

			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId1 = receiveConsignment1.WRC_JobID;

			var rtu = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("");

			var asn = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			helper.CreateAdditionalReference(asn, "ASN1Lloyds", WarehouseAdditionalReferenceTypes.Codes.VesselLloyds);
			helper.CreateAdditionalReference(asn, "ASN1Voyage", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			helper.CreateAdditionalReference(asn, "ASN1Vessel", WarehouseAdditionalReferenceTypes.Codes.Vessel);

			var asnWithEmptyVesselLloyds = helper.CreateReceiveASN("ASN2", Data.Warehouse.PK);
			helper.CreateAdditionalReference(asnWithEmptyVesselLloyds, "ASN2Lloyds", WarehouseAdditionalReferenceTypes.Codes.VesselLloyds);
			helper.CreateAdditionalReference(asnWithEmptyVesselLloyds, "ASN2Voyage", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			helper.CreateAdditionalReference(asnWithEmptyVesselLloyds, "ASN2Vessel", WarehouseAdditionalReferenceTypes.Codes.Vessel);

			var asnWithoutVesselLloyds = helper.CreateReceiveASN("ASN3", Data.Warehouse.PK);
			helper.CreateAdditionalReference(asnWithoutVesselLloyds, "ASN3Voyage", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			helper.CreateAdditionalReference(asnWithoutVesselLloyds, "ASN3Vessel", WarehouseAdditionalReferenceTypes.Codes.Vessel);

			var pivot = helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);
			var pivot2 = helper.CreateReceiveASNRTUPivot(rtu.PK, asnWithEmptyVesselLloyds.PK);
			var pivot3 = helper.CreateReceiveASNRTUPivot(rtu.PK, asnWithoutVesselLloyds.PK);

			var packageState1 = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, receiveASN: asn);
			var packageState2 = helper.CreatePackageState(receiveConsignment1, 9, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);
			helper.SetupPackageForOutturn(packageState1.Package, bookedQty: 1, damagedQty: 1, height: 1, length: 3, volume: 15, weight: 6, width: 5);
			helper.SetupPackageForOutturn(packageState2.Package, bookedQty: 9, damagedQty: 0, height: 2, length: 4, volume: 48, weight: 4, width: 6);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(rtu);

			var rcn1SubShipment = receiveTransportationUnitDataObject.SubShipmentCollection.Single();
			AssertEquals("RCN found through packages on the RTU should be included as subshipments.", jobId1, rcn1SubShipment.DataContext.DataSourceCollection.Single().Key.Value);

			AssertSeaCargoOutturnDataForMatching(receiveTransportationUnitDataObject, "ASN1Lloyds", "ASN1Voyage", "ASN1Vessel", "PREMISEID123");
		}

		public void TestPopulateDataObject_PopulateSeaCargoOutturnDataForMatching_UsingLatestASNWithVesselLloyds_FromPackageStates()
		{
			var orgCusCode = Data.Warehouse.WarehouseAddress.CustomsCodes.AddNew();
			orgCusCode.OK_OA_PremisesAddress = Data.Warehouse.WarehouseAddress.PK;
			orgCusCode.OK_OH = Data.Warehouse.WarehouseAddress.Header.PK;
			orgCusCode.OK_CustomsRegNo = "PremiseID123";
			orgCusCode.OK_CodeType = CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID;
			orgCusCode.OK_RN_NKCodeCountry = GlbBranch.CurrentBranch.BaseCountry.Code;

			Factory.SaveForTesting();

			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId1 = receiveConsignment1.WRC_JobID;

			var rtu = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("");
			var asn = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			helper.CreateAdditionalReference(asn, "ASN1Lloyds", WarehouseAdditionalReferenceTypes.Codes.VesselLloyds);
			helper.CreateAdditionalReference(asn, "ASN1Voyage", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			helper.CreateAdditionalReference(asn, "ASN1Vessel", WarehouseAdditionalReferenceTypes.Codes.Vessel);

			var asnWithEmptyVesselLloyds = helper.CreateReceiveASN("ASN2", Data.Warehouse.PK);
			helper.CreateAdditionalReference(asnWithEmptyVesselLloyds, "", WarehouseAdditionalReferenceTypes.Codes.VesselLloyds);
			helper.CreateAdditionalReference(asnWithEmptyVesselLloyds, "ASN2Voyage", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			helper.CreateAdditionalReference(asnWithEmptyVesselLloyds, "ASN2Vessel", WarehouseAdditionalReferenceTypes.Codes.Vessel);

			var asnWithoutVesselLloyds = helper.CreateReceiveASN("ASN3", Data.Warehouse.PK);
			helper.CreateAdditionalReference(asnWithoutVesselLloyds, "ASN3Voyage", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			helper.CreateAdditionalReference(asnWithoutVesselLloyds, "ASN3Vessel", WarehouseAdditionalReferenceTypes.Codes.Vessel);

			var packageState1 = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, receiveASN: asn);
			var packageState2 = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, receiveASN: asnWithEmptyVesselLloyds);
			var packageState3 = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, receiveASN: asnWithoutVesselLloyds);
			helper.SetupPackageForOutturn(packageState1.Package, bookedQty: 1, damagedQty: 1, height: 1, length: 3, volume: 15, weight: 6, width: 5);
			helper.SetupPackageForOutturn(packageState2.Package, bookedQty: 1, damagedQty: 1, height: 1, length: 3, volume: 15, weight: 6, width: 5);
			helper.SetupPackageForOutturn(packageState3.Package, bookedQty: 1, damagedQty: 1, height: 1, length: 3, volume: 15, weight: 6, width: 5);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(rtu);

			var rcn1SubShipment = receiveTransportationUnitDataObject.SubShipmentCollection.Single();
			AssertEquals("RCN found through packages on the RTU should be included as subshipments.", jobId1, rcn1SubShipment.DataContext.DataSourceCollection.Single().Key.Value);

			AssertSeaCargoOutturnDataForMatching(receiveTransportationUnitDataObject, "ASN1Lloyds", "ASN1Voyage", "ASN1Vessel", "PREMISEID123");
		}

		public void TestPopulateDataObject_PopulateSeaCargoOutturnDataForMatching_UsingLatestASNWithVesselLloyds_FromPivotAndPackageStates()
		{
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID123");
			Factory.SaveForTesting();

			var now = ZDateTime.Now;

			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId1 = receiveConsignment1.WRC_JobID;

			var rtu = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("");

			var asn = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			asn.WRP_SystemCreateTimeUtc = now;
			helper.CreateAdditionalReference(asn, "ASN1Lloyds", WarehouseAdditionalReferenceTypes.Codes.VesselLloyds);
			helper.CreateAdditionalReference(asn, "ASN1Voyage", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			helper.CreateAdditionalReference(asn, "ASN1Vessel", WarehouseAdditionalReferenceTypes.Codes.Vessel);
			var pivot = helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			var asn2 = helper.CreateReceiveASN("ASN2", Data.Warehouse.PK);
			asn2.WRP_SystemCreateTimeUtc = now.AddHours(2);
			helper.CreateAdditionalReference(asn2, "ASN2Lloyds", WarehouseAdditionalReferenceTypes.Codes.VesselLloyds);
			helper.CreateAdditionalReference(asn2, "ASN2Voyage", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			helper.CreateAdditionalReference(asn2, "ASN2Vessel", WarehouseAdditionalReferenceTypes.Codes.Vessel);
			var pivot2 = helper.CreateReceiveASNRTUPivot(rtu.PK, asn2.PK);

			var asn3 = helper.CreateReceiveASN("ASN3", Data.Warehouse.PK);
			asn3.WRP_SystemCreateTimeUtc = now.AddHours(4);
			helper.CreateAdditionalReference(asn3, "ASN3Voyage", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			helper.CreateAdditionalReference(asn3, "ASN3Vessel", WarehouseAdditionalReferenceTypes.Codes.Vessel);
			var pivot3 = helper.CreateReceiveASNRTUPivot(rtu.PK, asn3.PK);

			var packageState1 = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, receiveASN: asn2);
			var packageState2 = helper.CreatePackageState(receiveConsignment1, 9, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn3);
			helper.SetupPackageForOutturn(packageState1.Package, bookedQty: 1, damagedQty: 1, height: 1, length: 3, volume: 15, weight: 6, width: 5);
			helper.SetupPackageForOutturn(packageState2.Package, bookedQty: 9, damagedQty: 0, height: 2, length: 4, volume: 48, weight: 4, width: 6);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(rtu);

			var rcn1SubShipment = receiveTransportationUnitDataObject.SubShipmentCollection.Single();
			AssertEquals("RCN found through packages on the RTU should be included as subshipments.", jobId1, rcn1SubShipment.DataContext.DataSourceCollection.Single().Key.Value);

			AssertSeaCargoOutturnDataForMatching(receiveTransportationUnitDataObject, "ASN2Lloyds", "ASN2Voyage", "ASN2Vessel", "PREMISEID123");
		}

		public void TestPopulateDataObject_PopulateSeaCargoOutturnDataForMatching_ContainerCollectionHasOneContainerWithRTUVehicleReference()
		{
			WhsTransitTestHelper.SetPremiseIDForWarehouse(Data.Warehouse, "PremiseID123");
			Factory.SaveForTesting();

			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId1 = receiveConsignment1.WRC_JobID;

			var containerTypeRTU = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("CTN1");
			var rtu = SetupSimpleReceiveTransportationUnitForTesting("RTU1");

			var asn = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			helper.CreateAdditionalReference(asn, "ASN1Lloyds", WarehouseAdditionalReferenceTypes.Codes.VesselLloyds);
			helper.CreateAdditionalReference(asn, "ASN1Voyage", WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber);
			helper.CreateAdditionalReference(asn, "ASN1Vessel", WarehouseAdditionalReferenceTypes.Codes.Vessel);

			var pivot = helper.CreateReceiveASNRTUPivot(containerTypeRTU.PK, asn.PK);
			var pivot2 = helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			var writer1 = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, containerTypeRTU)));
			var containerTypeRTUDataObject = writer1.GetDataObject(containerTypeRTU);

			var container = containerTypeRTUDataObject.ContainerCollection.Single();
			AssertEquals("Container collection should have one container with the RTU vehicle reference.", containerTypeRTU.WRH_VehicleReference, container.ContainerNumber);

			var writer2 = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu)));
			var rtuDataObject = writer2.GetDataObject(rtu);

			var rtuContainer = rtuDataObject.ContainerCollection.Single();
			AssertEquals("Container collection should have one container with the RTU vehicle reference.", rtu.WRH_VehicleReference, rtuContainer.ContainerNumber);
		}

		#endregion

		#region TestPopulateDataObject_MultipleRTUToSingleASN

		public void TestPopulateDataObject_MultipleRTUToSingleASN()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId1 = receiveConsignment1.WRC_JobID;
			var receiveConsignment2 = helper.CreateReceiveConsignment("RCN2", "STD", Data.Warehouse.PK);
			var jobId2 = receiveConsignment2.WRC_JobID;

			var rtu1 = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1");
			var rtu2 = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU2");

			var asn1 = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);

			var pivot1 = helper.CreateReceiveASNRTUPivot(rtu1.PK, asn1.PK);
			var pivot2 = helper.CreateReceiveASNRTUPivot(rtu2.PK, asn1.PK);

			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn1, receiveUnit: rtu1);
			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn1, receiveUnit: rtu2);

			helper.CreatePackageState(receiveConsignment2, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn1, receiveUnit: rtu1);
			helper.CreatePackageState(receiveConsignment2, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn1, receiveUnit: rtu2);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu1)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(rtu1);

			var subShipmentKeys = receiveTransportationUnitDataObject.SubShipmentCollection.Select(s => s.DataContext.DataSourceCollection.Single().Key.Value);
			AssertContainsExactElementsInAnyOrder("RCNs found through packages on the RTU should be included as subshipments.", new string[] { jobId1, jobId2 }, subShipmentKeys);

			var rcn1 = receiveTransportationUnitDataObject.SubShipmentCollection.Single(s => jobId1 == s.DataContext.DataSourceCollection.Single().Key.Value);
			var rcn2 = receiveTransportationUnitDataObject.SubShipmentCollection.Single(s => jobId2 == s.DataContext.DataSourceCollection.Single().Key.Value);

			const string packageErrorMessage = "RCNs should include all packages on their package job.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG1", "PKG2" }, rcn1.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG3", "PKG4" }, rcn2.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertNotNull(rcn1.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1" && p.OutturnQty == 1 && p.ContainerLink == 0));
			AssertNotNull("Package hasn't arrived with the RTU yet.", rcn1.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2" && p.OutturnQty == 1 && p.ContainerLink == null));
			AssertNotNull(rcn2.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG3" && p.OutturnQty == 1 && p.ContainerLink == 0));
			AssertNotNull("Package hasn't arrived with the RTU yet.", rcn2.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG4" && p.OutturnQty == 1 && p.ContainerLink == null));

			AssertEquals(0, receiveTransportationUnitDataObject.ContainerCollection.Single().Link);
			AssertEquals("A packing line collection should only be created if the receive transportation unit has packages", receiveTransportationUnitDataObject.PackingLineCollection.Count == 0, ZBool.True);
		}

		#endregion

		#region TestPopulateDataObject_SomePackagesArrivedWithoutRCN

		public void TestPopulateDataObject_SomePackagesArrivedWithoutRCN()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId1 = receiveConsignment1.WRC_JobID;

			var rtu1 = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1");
			var asn1 = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			var pivot1 = helper.CreateReceiveASNRTUPivot(rtu1.PK, asn1.PK);

			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn1, receiveUnit: rtu1);
			helper.CreatePackageState(rtu1, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn1);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu1)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(rtu1);

			AssertEquals(0, receiveTransportationUnitDataObject.ContainerCollection.Single().Link);
			var packlineNoRCN = receiveTransportationUnitDataObject.PackingLineCollection.Single();
			AssertEquals("PKG2", packlineNoRCN.ReferenceNumber.Value);
			AssertEquals(0, packlineNoRCN.OutturnQty);
			AssertEquals(null, packlineNoRCN.ContainerLink);

			var rcn1SubShipment = receiveTransportationUnitDataObject.SubShipmentCollection.Single();
			AssertEquals("RCN found through packages on the RTU should be included as subshipments.", jobId1, rcn1SubShipment.DataContext.DataSourceCollection.Single().Key.Value);

			var packline = rcn1SubShipment.PackingLineCollection.Single();
			AssertEquals("PKG1", packline.ReferenceNumber.Value);
			AssertEquals(1, packline.OutturnQty);
			AssertEquals(0, packline.ContainerLink);
		}

		#endregion

		#region TestPopulateDataObject_SingleRTUToMultipleASNs

		public void TestPopulateDataObject_SingleRTUToMultipleASNs()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId1 = receiveConsignment1.WRC_JobID;
			var receiveConsignment2 = helper.CreateReceiveConsignment("RCN2", "STD", Data.Warehouse.PK);
			var jobId2 = receiveConsignment2.WRC_JobID;

			var rtu1 = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1");

			var asn1 = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			var asn2 = helper.CreateReceiveASN("ASN2", Data.Warehouse.PK);

			var pivot1 = helper.CreateReceiveASNRTUPivot(rtu1.PK, asn1.PK);
			var pivot2 = helper.CreateReceiveASNRTUPivot(rtu1.PK, asn2.PK);

			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn1, receiveUnit: rtu1);
			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);

			helper.CreatePackageState(receiveConsignment2, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn2);
			helper.CreatePackageState(receiveConsignment2, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn2, receiveUnit: rtu1);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu1)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(rtu1);

			var subShipmentKeys = receiveTransportationUnitDataObject.SubShipmentCollection.Select(s => s.DataContext.DataSourceCollection.Single().Key.Value);
			AssertContainsExactElementsInAnyOrder("RCNs found through packages on the RTU should be included as subshipments.", new string[] { jobId1, jobId2 }, subShipmentKeys);

			var rcn1 = receiveTransportationUnitDataObject.SubShipmentCollection.Single(s => jobId1 == s.DataContext.DataSourceCollection.Single().Key.Value);
			var rcn2 = receiveTransportationUnitDataObject.SubShipmentCollection.Single(s => jobId2 == s.DataContext.DataSourceCollection.Single().Key.Value);

			const string packageErrorMessage = "RCNs should include all packages on their package job.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG1", "PKG2" }, rcn1.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG3", "PKG4" }, rcn2.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertNotNull(rcn1.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1" && p.OutturnQty == 1 && p.ContainerLink == 0));
			AssertNotNull(rcn1.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2" && p.OutturnQty == 0 && p.ContainerLink == null));
			AssertNotNull(rcn2.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG3" && p.OutturnQty == 0 && p.ContainerLink == null));
			AssertNotNull(rcn2.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG4" && p.OutturnQty == 1 && p.ContainerLink == 0));
		}

		#endregion

		#region TestPopulateDataObject_ExtraPackage

		public void TestPopulateDataObject_ExtraPackage()
		{
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId = receiveConsignment.WRC_JobID;

			var rtu = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1");
			var asn = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			var pivot = helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, receiveASN: asn);
			helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(rtu);

			var rcn1SubShipment = receiveTransportationUnitDataObject.SubShipmentCollection.Single();
			AssertEquals("RCN found through packages on the RTU should be included as subshipments.", jobId, rcn1SubShipment.DataContext.DataSourceCollection.Single().Key.Value);

			const string packageErrorMessage = "RCNs should include all packages on their package job.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG1", "PKG2" }, rcn1SubShipment.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertNotNull(rcn1SubShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1" && p.OutturnQty == 1 && p.ContainerLink == 0));
			AssertNotNull(rcn1SubShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2" && p.OutturnQty == 1 && p.ContainerLink == 0));
		}

		#endregion

		#region TestPopulateDataObject_ShortPackages

		public void TestPopulateDataObject_ShortPackages()
		{
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId = receiveConsignment.WRC_JobID;

			var rtu = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1");
			var asn = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			var pivot = helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			var packageState1 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, receiveASN: asn);
			var packageState2 = helper.CreatePackageState(receiveConsignment, 10, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);
			helper.SetupPackageForOutturn(packageState1.Package, bookedQty: 1, damagedQty: 0, height: 0, length: 0, volume: 0, weight: 0, width: 0);
			helper.SetupPackageForOutturn(packageState2.Package, bookedQty: 10, damagedQty: 10, height: 0, length: 0, volume: 0, weight: 0, width: 0);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(rtu);

			var rcn1SubShipment = receiveTransportationUnitDataObject.SubShipmentCollection.Single();
			AssertEquals("RCN found through packages on the RTU should be included as subshipments.", jobId, rcn1SubShipment.DataContext.DataSourceCollection.Single().Key.Value);

			const string packageErrorMessage = "RCNs should include all packages on their package job.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG1", "PKG2" }, rcn1SubShipment.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			var packingLine1 = rcn1SubShipment.PackingLineCollection.SingleOrDefault(p => p.ReferenceNumber.Value == "PKG1" && p.ContainerLink == 0);
			var packingLine2 = rcn1SubShipment.PackingLineCollection.SingleOrDefault(p => p.ReferenceNumber.Value == "PKG2" && p.ContainerLink == null);
			AssertNotNull(packingLine1);
			AssertNotNull(packingLine2);
			AssertOutturnDetail(packingLine1, 1, 0, 0, 0, 0, 0, 0);
			AssertOutturnDetail(packingLine2, 0, 10, 0, 0, 0, 0, 0);
		}

		#endregion

		#region TestPopulateDataObject_NonTrackedItems

		public void TestPopulateDataObject_NonTrackedItems()
		{
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId = receiveConsignment.WRC_JobID;

			var rtu = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1");
			var asn = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			var pivot = helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			var packageState1 = helper.CreatePackageState(receiveConsignment, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, receiveASN: asn);
			helper.CreateAndAttachArrivedInners(packageState1, 20, "BOX");
			helper.CreateAndAttachArrivedInners(packageState1, 30, "PLT");
			helper.SetupPackageForOutturn(packageState1.Package, bookedQty: 1, damagedQty: 1, height: 0, length: 0, volume: 0, weight: 0, width: 0);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(rtu);

			var rcn1SubShipment = receiveTransportationUnitDataObject.SubShipmentCollection.Single();
			AssertEquals("RCN found through packages on the RTU should be included as subshipments.", jobId, rcn1SubShipment.DataContext.DataSourceCollection.Single().Key.Value);

			var packline = rcn1SubShipment.PackingLineCollection.Single();
			AssertEquals("PKG1", packline.ReferenceNumber.Value);
			AssertEquals(0, packline.ContainerLink);
			AssertOutturnDetail(packline, 1, 1, 0, 0, 0, 0, 0);

			AssertEquals(2, packline.PackingLineCollection.Count);
			AssertNotNull(packline.PackingLineCollection.Single(p => p.OutturnQty == 1 && p.ContainerLink == null && p.PackType.Code.Value == "BOX"));
			AssertNotNull(packline.PackingLineCollection.Single(p => p.OutturnQty == 1 && p.ContainerLink == null && p.PackType.Code.Value == "PLT"));
		}

		#endregion

		#region TestPopulateDataObject_PalletizedPackline

		public void TestPopulateDataObject_PalletizedPackline()
		{
			var receiveConsignment = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId = receiveConsignment.WRC_JobID;

			var rtu = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1");
			var asn = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			var pivot = helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			var packageState = helper.CreatePackageState(receiveConsignment, 1, "PLT", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, receiveASN: asn);
			helper.CreateAndAttachArrivedInners(packageState, 10, "CTN");
			helper.SetupPackageForOutturn(packageState.Package, bookedQty: 10, damagedQty: 10, height: 0, length: 0, volume: 0, weight: 0, width: 0);
			helper.SetupPackageForOutturn(packageState.Package.Packages.Single(), bookedQty: 0, damagedQty: 0, height: 0, length: 0, volume: 0, weight: 0, width: 0);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(rtu);

			var rcn1SubShipment = receiveTransportationUnitDataObject.SubShipmentCollection.Single();
			AssertEquals("RCN found through packages on the RTU should be included as subshipments.", jobId, rcn1SubShipment.DataContext.DataSourceCollection.Single().Key.Value);

			AssertEquals(1, rcn1SubShipment.PackingLineCollection.Count);
			var packingLine = rcn1SubShipment.PackingLineCollection.Single();
			AssertEquals("PKG1", packingLine.ReferenceNumber.Value);
			AssertEquals("PLT", packingLine.PackType.Code.Value);
			AssertEquals(0, packingLine.ContainerLink);
			AssertOutturnDetail(packingLine, 1, 1, 0, 0, 0, 0, 0);

			AssertEquals(1, packingLine.PackingLineCollection.Count);
			var innerPackingLine = packingLine.PackingLineCollection.Single();
			AssertEquals("CTN", innerPackingLine.PackType.Code.Value);
			AssertNull(innerPackingLine.ContainerLink);
			AssertOutturnDetail(innerPackingLine, 1, 0, 0, 0, 0, 0, 0);
		}

		#endregion

		#region TestPopulateDataObject_HandlingUnit

		public void TestPopulateDataObject_HandlingUnit()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId1 = receiveConsignment1.WRC_JobID;

			var rtu1 = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("RTU1");
			var asn1 = helper.CreateReceiveASN("ASN1", Data.Warehouse.PK);
			var pivot1 = helper.CreateReceiveASNRTUPivot(rtu1.PK, asn1.PK);

			var handlingUnit = helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu1);

			var childPackage1 = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1, receiveASN: asn1);
			var childPackage2 = helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1, receiveASN: asn1);
			helper.DisableTopLevelHUFKForTest(TestConnection);
			helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA");
			helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA");

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, rtu1)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(rtu1);

			var rcn1SubShipment = receiveTransportationUnitDataObject.SubShipmentCollection.Single();
			AssertEquals("RCN found through packages on the RTU should be included as subshipments.", jobId1, rcn1SubShipment.DataContext.DataSourceCollection.Single().Key.Value);

			const string packageErrorMessage = "RCNs should include all packages on their package job.";
			AssertContainsExactElementsInAnyOrder(packageErrorMessage, new string[] { "PKG1", "PKG2" }, rcn1SubShipment.PackingLineCollection.Select(p => p.ReferenceNumber.Value));
			AssertNotNull(rcn1SubShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG1" && p.OutturnQty == 1 && p.ContainerLink == 0));
			AssertNotNull(rcn1SubShipment.PackingLineCollection.Single(p => p.ReferenceNumber.Value == "PKG2" && p.OutturnQty == 1 && p.ContainerLink == 0));
		}

		#endregion

		#region TestPopulateDataObject_Addresses

		public void TestPopulateDataObject_Addresses()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory.BOFactory);
			var transportCompany = data.Org1;

			var address = Factory.New<JobDocAddress>();
			address.E2_AddressOverride = true;
			address.E2_AddressType = AutoDocAddressTypes.Codes.TransportCompanyDocumentaryAddress;
			address.E2_ParentID = data.Org1.PK;
			address.E2_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			address.E2_OA_Address = transportCompany.MainAddress.PK;

			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");
			receiveTransportationUnit.DocAddresses.Add(address);

			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveTransportationUnit);

			AssertContainsExactElementsInAnyOrder(
				"Should include transport company and warehouse addresses.",
				new string[] { transportCompany.OH_Code, warehouse.WarehouseAddress.Header.OH_Code },
				dataObject.OrganizationAddressCollection?.Select(a => a.OrganizationCode.ToString()));
		}

		public void TestPopulateDataObject_Addresses_GivenNoJobDocAddress()
		{
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");

			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveTransportationUnit);

			AssertNotNull(dataObject.OrganizationAddressCollection);
			AssertContainsExactElementsInAnyOrder("Should include warehouse address.",
				new string[] { warehouse.WarehouseAddress.Header.OH_Code },
				dataObject.OrganizationAddressCollection.Select(a => a.OrganizationCode.ToString()));
		}

		#endregion

		#region TestPopulateDataObject_Dates

		[TestDate(2020, 08, 28, 04, 39, 50)]
		public void TestPopulateDataObject_Vehicle_Dates()
		{
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");
			var currentTime = ZDateTimeOffset.Now;
			receiveTransportationUnit.WRH_UnloadCompleteTime = currentTime;
			receiveTransportationUnit.WRH_UnloadCompleteNotYetProcessedTime = receiveTransportationUnit.WRH_UnloadCompleteTime;

			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveTransportationUnit);

			AssertNotNull(dataObject?.DateCollection);
			AssertContainsExactElementsInAnyOrder("Should be the receive transportation unit's unload time", new[] { DateType.Unpack }, dataObject.DateCollection.Select(d => d.Type));
			var unpackDate = dataObject.DateCollection.Single();
			AssertEquals(currentTime.ToZDateTime(), unpackDate.Value);
			AssertEquals(ZBool.False, unpackDate.IsEstimate);
		}

		[TestDate(2020, 08, 28, 04, 39, 50)]
		public void TestPopulateDataObject_Container_Dates()
		{
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("Good Vehicle");
			var currentTime = ZDateTimeOffset.Now;
			receiveTransportationUnit.WRH_UnloadCompleteTime = currentTime;
			receiveTransportationUnit.WRH_UnloadCompleteNotYetProcessedTime = receiveTransportationUnit.WRH_UnloadCompleteTime;

			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveTransportationUnit);

			AssertNotNull(dataObject.ContainerCollection);
			AssertContainsExactElementsInAnyOrder("Should be the receive transportation unit's unload time", new[] { currentTime.ToZDateTime() }, dataObject.ContainerCollection.Select(c => c.LCLUnpack.Value));
		}

		public void TestPopulateDataObject_InvalidDate()
		{
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("Good Vehicle");
			receiveTransportationUnit.WRH_UnloadCompleteTime = ZDateTimeOffset.Invalid;
			receiveTransportationUnit.WRH_UnloadCompleteNotYetProcessedTime = receiveTransportationUnit.WRH_UnloadCompleteTime;

			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveTransportationUnit);

			AssertNull(dataObject.DateCollection);
		}

		public void TestPopulateDataObject_NoDates()
		{
			var dispatchTransportationUnit = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("Good Vehicle");

			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(dispatchTransportationUnit);

			AssertNull(dataObject.DateCollection);
		}

		#endregion

		#region TestPopulateDataObject_AdditionalReferences

		public void TestPopulateDataObject_AdditionalReferences()
		{
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");

			var reference1 = helper.CreateAdditionalReference(receiveTransportationUnit, "Good Name", entryType: WarehouseAdditionalReferenceTypes.Codes.DriverName);
			var reference2 = helper.CreateAdditionalReference(receiveTransportationUnit, "Good License", entryType: WarehouseAdditionalReferenceTypes.Codes.DriverLicense);

			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveTransportationUnit);

			AssertEquals(CollectionContent.Partial, dataObject.AdditionalReferenceCollection.Content);
			AssertContainsExactElementsInAnyOrder(
				"Should include all additional references.",
				new string[] { "Good Name", "Good License" },
				dataObject.AdditionalReferenceCollection?.Select(r => r.ReferenceNumber.ToString()));
		}

		#endregion

		#region TestPopulateDataObject_VehicleRun

		public void TestPopulateDataObject_VehicleRun_WhenVehicleAndNotSignedNull()
		{
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");
			receiveTransportationUnit.WRH_SignedBy = null;

			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveTransportationUnit);

			AssertNull(nameof(dataObject.VehicleRun), dataObject.VehicleRun);
		}

		public void TestPopulateDataObject_VehicleRun_WhenVehicleAndNotSignedEmpty()
		{
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");
			receiveTransportationUnit.WRH_SignedBy = "  ";

			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveTransportationUnit);

			AssertNull(nameof(dataObject.VehicleRun), dataObject.VehicleRun);
		}

		public void TestPopulateDataObject_VehicleRun_WhenVehicleAndSigned()
		{
			ZString driverName = "Sebastien Loeb";
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");
			receiveTransportationUnit.WRH_SignedBy = driverName;
			receiveTransportationUnit.WRH_UnitType = TransportUnitTypes.Vehicle;

			TestPopulateDataObject_VehicleRun_Core(receiveTransportationUnit, driverName);
		}

		public void TestPopulateDataObject_VehicleRun_WhenContainerAndSignedWithContainer()
		{
			TestPopulateDataObject_VehicleRun_ContainerCore(TransportUnitTypes.Container, true);
		}

		public void TestPopulateDataObject_VehicleRun_WhenContainerAndSignedWithParent()
		{
			TestPopulateDataObject_VehicleRun_ContainerCore(TransportUnitTypes.Container);
		}

		public void TestPopulateDataObject_VehicleRun_WhenULDAndSigned()
		{
			TestPopulateDataObject_VehicleRun_ContainerCore(TransportUnitTypes.ULD);
		}

		public void TestPopulateDataObject_VehicleRun_WhenULDAndParentSignedEmpty()
		{
			TestPopulateDataObject_VehicleRun_WhenContainerAndParentIsEmptyCore(TransportUnitTypes.ULD);
		}

		public void TestPopulateDataObject_VehicleRun_WhenContainerAndParentSignedEmpty()
		{
			TestPopulateDataObject_VehicleRun_WhenContainerAndParentIsEmptyCore(TransportUnitTypes.Container);
		}

		void TestPopulateDataObject_VehicleRun_WhenContainerAndParentIsEmptyCore(ZString unitType)
		{
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("Good Container");
			receiveTransportationUnit.WRH_UnitType = unitType;

			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveTransportationUnit);

			AssertNull(nameof(dataObject.VehicleRun), dataObject.VehicleRun);
		}

		void TestPopulateDataObject_VehicleRun_ContainerCore(ZString unitType, bool testContainerSigned = false)
		{
			var driverName = "TestDriver";
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting("Good Container");
			receiveTransportationUnit.WRH_UnitType = unitType;

			var row = helper.CreateRowAndGenerateLocations(warehouse, "R", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "R-1-1");
			var vehRTU = helper.CreateReceiveTransportationUnit("Vehicle", warehouse.PK, location.PK);
			vehRTU.WRH_UnitType = TransportUnitTypes.Vehicle;
			if (testContainerSigned)
			{
				receiveTransportationUnit.WRH_SignedBy = driverName;
			}
			else
			{
				vehRTU.WRH_SignedBy = driverName;
			}
			var packageState = receiveTransportationUnit.ContainerizedPackageState;
			packageState.WPS_WL_LastLocation = location.PK;
			packageState.WPS_WRH_TransitReceiveHeader = vehRTU.PK;
			Factory.SaveForTesting();
			TestPopulateDataObject_VehicleRun_Core(receiveTransportationUnit, driverName);
		}

		void TestPopulateDataObject_VehicleRun_Core(WhsItemReceiveTransportationUnit receiveTransportationUnit, ZString driverName)
		{
			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveTransportationUnit);

			AssertNotNull(nameof(dataObject.VehicleRun), dataObject.VehicleRun);
			CombineAssertions(() =>
			{
				AssertNull(nameof(dataObject.VehicleRun.Vehicle), dataObject.VehicleRun.Vehicle);
				var crewCollection = dataObject.VehicleRun.CrewCollection;
				AssertContainsExactElementsInExactOrder("Should include driver type", new[] { CrewType.Driver, }, crewCollection.Select(c => c.CrewType));
				AssertContainsExactElementsInExactOrder("Should include driver name", new[] { driverName, }, crewCollection.Select(c => c.FullName));
			});
		}

		#endregion

		#region TestPopulateDataObject_Notes

		public void TestPopulateDataObject_Notes()
		{
			var receiveTransportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");

			receiveTransportationUnit.Notes.AddNew(true, "Good Description1", "Good Text");
			receiveTransportationUnit.Notes.AddNew(true, "Good Description3", "Good Text");
			receiveTransportationUnit.Notes.AddNew(true, "Good Description2", "Good Text");

			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(receiveTransportationUnit);

			AssertArrayEqualsByElements(
				"Should include all additional references.",
				new string[] { "Good Description1", "Good Description2", "Good Description3" },
				dataObject.NoteCollection.Select(n => n.Description.ToString()).ToArray());
		}

		#endregion

		#region TestPopulateDataObject_ReceiveConsignments

		#region TestPopulateDataObject_ShouldPopulate_ReceiveConsignments

		public void TestPopulateDataObject_ShouldPopulate_ReceiveConsignments()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var jobId1 = receiveConsignment1.WRC_JobID;
			var receiveConsignment2 = helper.CreateReceiveConsignment("RCN2", "STD", Data.Warehouse.PK);
			var jobId2 = receiveConsignment2.WRC_JobID;
			var receiveConsignment3 = helper.CreateReceiveConsignment("RCN3", "STD", Data.Warehouse.PK);
			var receiveTranportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");

			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTranportationUnit);
			helper.CreatePackageState(receiveConsignment2, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTranportationUnit);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveTranportationUnit)));
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(receiveTranportationUnit);

			var subShipments = receiveTransportationUnitDataObject.SubShipmentCollection;
			CombineAssertions(() =>
			{
				AssertEquals(2, subShipments.Count);
				AssertNotNull(subShipments.Single(s => jobId1 == s.DataContext.DataSourceCollection.Single().Key.Value));
				AssertNotNull(subShipments.Single(s => jobId2 == s.DataContext.DataSourceCollection.Single().Key.Value));
			});
		}

		#endregion

		#region TestPopulateDataObject_ShouldNotPopulate_ReceiveConsignments

		public void TestPopulateDataObject_ShouldNotPopulate_ReceiveConsignments()
		{
			var receiveConsignment1 = helper.CreateReceiveConsignment("RCN1", "STD", Data.Warehouse.PK);
			var receiveConsignment2 = helper.CreateReceiveConsignment("RCN2", "STD", Data.Warehouse.PK);
			var receiveTranportationUnit = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");

			helper.CreatePackageState(receiveConsignment1, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTranportationUnit);
			helper.CreatePackageState(receiveConsignment2, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: receiveTranportationUnit);

			var receiveTransportationUnitDataObjectWriter = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, receiveTranportationUnit)), shouldPopulateConsignments: false);
			var receiveTransportationUnitDataObject = receiveTransportationUnitDataObjectWriter.GetDataObject(receiveTranportationUnit);

			AssertNull(receiveTransportationUnitDataObject.SubShipmentCollection);
		}

		#endregion

		#endregion

		#region TestPopulateDataObject_EmptyPackingLineCollection

		public void TestPopulateDataObject_EmptyPackingLineCollection()
		{
			var rtu = SetupSimpleReceiveTransportationUnitForTesting("Good Vehicle");
			var writer = new WhsTransitReceiveTransportationUnitDataObjectWriter(new DataWritingManager(new DummyActionInfo()));
			var dataObject = writer.GetDataObject(rtu);

			AssertNotNull(dataObject.PackingLineCollection);
			AssertEquals("Content should be completed", CollectionContent.Complete, dataObject.PackingLineCollection.Content);
		}

		#endregion

		#endregion

		#region Implementation

		void AssertSeaCargoOutturnDataForMatching(Shipment dataObject, string vesselLloyds, string voyageNumber, string vesselName, string premiseID)
		{
			AssertEquals("Vessel Lloyds from asn should have been populated.", vesselLloyds, dataObject.LloydsIMO);
			AssertEquals("Voyage Number from asn should have been populated.", voyageNumber, dataObject.VoyageFlightNo);
			AssertEquals("Vessel Name from asn should have been populated.", vesselName, dataObject.VesselName);

			var premiseIDFromDataObject = dataObject.AdditionalReferenceCollection?.Where(a => a.Type.Code.Equals(CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID)).FirstOrDefault()?.ReferenceNumber;
			AssertEquals("Vessel Lloyds/IMO from asn should have been populated.", premiseID, premiseIDFromDataObject);
		}

		void AssertOutturnDetail(PackingLine packingLine, int expectedOutturnQty, int expectedOutturnDamagedQty, decimal expectedOutturnedHeight, decimal expectedOtturnedLength, decimal expectedOutturnedVolume, decimal expectedOutturnedWeight, decimal expectedOutturnedWidth)
		{
			AssertEquals(expectedOutturnQty, packingLine.OutturnQty);
			AssertEquals(expectedOutturnDamagedQty, packingLine.OutturnDamagedQty);
			AssertEquals(expectedOutturnedHeight, packingLine.OutturnedHeight);
			AssertEquals(expectedOtturnedLength, packingLine.OutturnedLength);
			AssertEquals(expectedOutturnedVolume, packingLine.OutturnedVolume);
			AssertEquals(expectedOutturnedWeight, packingLine.OutturnedWeight);
			AssertEquals(expectedOutturnedWidth, packingLine.OutturnedWidth);
		}

		WhsTransitTestHelper helper => new WhsTransitTestHelper(Factory.BOFactory);
		WhsWarehouse warehouse => Data.Warehouse;

		WhsItemReceiveTransportationUnit SetupSimpleReceiveTransportationUnitWithContainerTypeForTesting(string reference)
		{
			var row = helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnitWithContainerType(reference, warehouse.PK, location.PK);

			return receiveTransportationUnit;
		}

		WhsItemReceiveTransportationUnit SetupSimpleReceiveTransportationUnitForTesting(string reference)
		{
			var row = helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var receiveTransportationUnit = helper.CreateReceiveTransportationUnit(reference, warehouse.PK, location.PK);

			return receiveTransportationUnit;
		}

		#endregion
	}
}
