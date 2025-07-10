using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentRunSheetDataObjectWriterTest : DtbConsignmentUniversalTestCase
	{
		#region TestTopLevelDataContextType
		public void TestTopLevelDataContextType()
		{
			AssertEquals(DataContextType.TransportConsignmentRunSheet, ((ITopLevelDataObjectWriter)new DtbConsignmentRunSheetDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).TopLevelDataContextType);
		}

		#endregion
		#region TestEDIMessageSubType
		public void TestEDIMessageSubType()
		{
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment, ((ITopLevelDataObjectWriter)new DtbConsignmentRunSheetDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).EDIMessageSubType);
		}

		#endregion
		#region TestWriterDoesNotExportRunsheetsWithMoreThanOneDepot
		public void TestWriterDoesNotExportRunsheetsWithMoreThanOneDepot()
		{
			AssertPopulateDataObject_MoreThanOneDepotInstruction(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			AssertPopulateDataObject_MoreThanOneDepotInstruction(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);
		}

		void AssertPopulateDataObject_MoreThanOneDepotInstruction(string addressType, string actionType)
		{
			var depot1 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var depot2 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			var consignment = Helper.CreateConsignment();
			var container1 = Helper.CreatePackage(consignment, "CN1", Constants.PkgUnit.Container);
			var container2 = Helper.CreatePackage(consignment, "CN2", Constants.PkgUnit.Container);
			var package1InContainer1 = Helper.CreateChildPackage(container1, "P1", "PLT");
			var package2InContainer1 = Helper.CreateChildPackage(container1, "P2", "BOX");
			var package3InContainer2 = Helper.CreateChildPackage(container2, "P3", "ROL");
			var address1 = Helper.CreateConsignmentAddressWithAction(consignment, addressType, actionType, DocAddressType.LocalCartageCFS, depot1.MainAddress);
			var address2 = Helper.CreateConsignmentAddressWithAction(consignment, addressType, actionType, DocAddressType.LocalCartageCFS, depot2.MainAddress);
			var runSheet = Helper.CreateRunSheet();
			Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.PickUp ? address1.PickupAction : address1.DeliveryAction); // depot instruction 1
			Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.PickUp ? address2.PickupAction : address2.DeliveryAction); // depot instruction 2
			var runsheetDataObjectWriter = new DtbConsignmentRunSheetDataObjectWriter(new DataWritingManager(new ActionInfo(addressType == ConsignmentAddressTypes.Codes.PickUp ? RecipientRoleType.DTW : RecipientRoleType.ATW, runSheet)));
			var runsheetDataObject = runsheetDataObjectWriter.GetDataObject(runSheet);
			AssertNull("No subshipments must be exported since there are more than one depot.", runsheetDataObject.SubShipmentCollection);
			AssertNull("No containers must be exported since there are more than one depot.", runsheetDataObject.ContainerCollection);
		}

		#endregion
		#region TestPopulateDataObject_RunsheetWithNonDepotInstruction
		public void TestPopulateDataObject_RunsheetWithNonDepotInstruction()
		{
			AssertPopulateDataObject_DepotInstruction(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCTO);
			AssertPopulateDataObject_DepotInstruction(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCTO);
		}

		#endregion
		#region TestPopulateDataObject_RunsheetWithDepotInstruction_ExportsDepotInstruction
		public void TestPopulateDataObject_RunsheetWithDepotInstruction_ExportsDepotInstruction()
		{
			AssertPopulateDataObject_DepotInstruction(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			AssertPopulateDataObject_DepotInstruction(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS);
		}

		void AssertPopulateDataObject_DepotInstruction(string addressType, string actionType, DocAddressType jobDocAddressType)
		{
			var depot = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var consignment = Helper.CreateConsignment();
			var container1 = Helper.CreatePackage(consignment, "CN1", Constants.PkgUnit.Container);
			var container2 = Helper.CreatePackage(consignment, "CN2", Constants.PkgUnit.Container);
			var package1InContainer1 = Helper.CreateChildPackage(container1, "P1", "PLT");
			var package2InContainer1 = Helper.CreateChildPackage(container1, "P2", "BOX");
			var package3InContainer2 = Helper.CreateChildPackage(container2, "P3", "ROL");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, addressType, actionType, jobDocAddressType, depot.MainAddress);
			var otherAddressType = addressType == ConsignmentAddressTypes.Codes.PickUp ? ConsignmentAddressTypes.Codes.Delivery : ConsignmentAddressTypes.Codes.PickUp;
			var otherActionType = actionType == ActionTypes.Codes.PickUp ? ActionTypes.Codes.Delivery : ActionTypes.Codes.PickUp;
			var addressForOtherActionType = Helper.CreateConsignmentAddressWithAction(consignment, otherAddressType, otherActionType, DocAddressType.LocalCartageCFS, depot.MainAddress);
			var runSheet = Helper.CreateRunSheet();
			Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.PickUp ? address.PickupAction : address.DeliveryAction);
			Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.PickUp ? addressForOtherActionType.DeliveryAction : addressForOtherActionType.PickupAction);
			var runsheetDataObjectWriter = new DtbConsignmentRunSheetDataObjectWriter(new DataWritingManager(new ActionInfo(addressType == ConsignmentAddressTypes.Codes.PickUp ? RecipientRoleType.DTW : RecipientRoleType.ATW, runSheet)));
			var runsheetDataObject = runsheetDataObjectWriter.GetDataObject(runSheet);
			if (jobDocAddressType == DocAddressType.LocalCartageCFS)
			{
				AssertOrganizationBO_CRAHOLSYDExists(runsheetDataObject.OrganizationAddressCollection, "Depot Address must exists as Local cartage CFS address.", nameof(DocAddressType.LocalCartageCFS), true);
				AssertEquals("Only one subshipment must exists.", 1, runsheetDataObject.SubShipmentCollection.Count);
				AssertEquals("Only one container must exists.", 2, runsheetDataObject.ContainerCollection.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "CN1", "CN2" }, runsheetDataObject.ContainerCollection.Select(c => c.ContainerNumber.ToString()));
			}
			else
			{
				AssertNull("No subshipments must be exported.", runsheetDataObject.SubShipmentCollection);
				AssertNull("No containers must be exported..", runsheetDataObject.ContainerCollection);
			}
		}

		#endregion
		#region TestPopulateDataObject_RunsheetWithDepotInstruction_NonTransitWarehouseRecipientRole
		public void TestPopulateDataObject_RunsheetWithDepotInstruction_NonTransitWarehouseRecipientRole()
		{
			AssertPopulateDataObject_DepotInstruction_NonTransitWarehouseRecipientRole(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			AssertPopulateDataObject_DepotInstruction_NonTransitWarehouseRecipientRole(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);
		}

		void AssertPopulateDataObject_DepotInstruction_NonTransitWarehouseRecipientRole(string addressType, string actionType)
		{
			var depot = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var consignment = Helper.CreateConsignment();
			var container1 = Helper.CreatePackage(consignment, "CN1", Constants.PkgUnit.Container);
			var container2 = Helper.CreatePackage(consignment, "CN2", Constants.PkgUnit.Container);
			var package1InContainer1 = Helper.CreateChildPackage(container1, "P1", "PLT");
			var package2InContainer1 = Helper.CreateChildPackage(container1, "P2", "BOX");
			var package3InContainer2 = Helper.CreateChildPackage(container2, "P3", "ROL");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, addressType, actionType, DocAddressType.LocalCartageCFS, depot.MainAddress);
			var otherAddressType = addressType == ConsignmentAddressTypes.Codes.PickUp ? ConsignmentAddressTypes.Codes.Delivery : ConsignmentAddressTypes.Codes.PickUp;
			var otherActionType = actionType == ActionTypes.Codes.PickUp ? ActionTypes.Codes.Delivery : ActionTypes.Codes.PickUp;
			var addressForOtherActionType = Helper.CreateConsignmentAddressWithAction(consignment, otherAddressType, otherActionType, DocAddressType.LocalCartageCFS, depot.MainAddress);
			var runSheet = Helper.CreateRunSheet();
			Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.PickUp ? address.PickupAction : address.DeliveryAction);
			Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.PickUp ? addressForOtherActionType.DeliveryAction : addressForOtherActionType.PickupAction);
			var runsheetDataObjectWriter = new DtbConsignmentRunSheetDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.TPC, runSheet)));
			var runsheetDataObject = runsheetDataObjectWriter.GetDataObject(runSheet);
			AssertNull("No subshipments must be exported since transit warehouse is not targetted.", runsheetDataObject.SubShipmentCollection);
			AssertNull("No containers must be exported since transit warehouse is not targetted.", runsheetDataObject.ContainerCollection);
		}

		#endregion
		#region TestPopulateDataObject_CustomFields_Runsheet
		public void TestPopulateDataObject_CustomFields_Runsheet()
		{
			var runSheet = Helper.CreateRunSheet();

			runSheet.SetUserDefinedValue("Squanch", new ZString("Schwifty"));

			var runSheetDataObjectWriter = new DtbConsignmentRunSheetDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.DTW, runSheet)));
			var runSheetDataObject = runSheetDataObjectWriter.GetDataObject(runSheet);

			var customFields = runSheetDataObject.CustomizedFieldCollection
				.Select(x => $"{x.DataType}: {x.Key} - {x.Value}");

			AssertContainsExactElementsInAnyOrder(new[] { "String: Squanch - Schwifty" }, customFields);
		}
		#endregion
		#region TestPopulateDataObject_VoyageFlightNo
		public void TestPopulateDataObject_VoyageFlightNo_Pickup()
		{
			AssertPopulateDataObject_DepotInstruction(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
		}

		public void TestPopulateDataObject_VoyageFlightNo_Delivery()
		{
			AssertPopulateDataObject_DepotInstruction(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);
		}

		void AssertPopulateDataObject_DepotInstruction(string addressType, string actionType)
		{
			var depot = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var consignment = Helper.CreateConsignment();
			var otherAddressType = addressType == ConsignmentAddressTypes.Codes.PickUp ? ConsignmentAddressTypes.Codes.Delivery : ConsignmentAddressTypes.Codes.PickUp;
			var otherActionType = actionType == ActionTypes.Codes.PickUp ? ActionTypes.Codes.Delivery : ActionTypes.Codes.PickUp;
			var addressForOtherActionType = Helper.CreateConsignmentAddressWithAction(consignment, otherAddressType, otherActionType, DocAddressType.LocalCartageCFS, depot.MainAddress);
			var address = Helper.CreateConsignmentAddressWithAction(consignment, addressType, actionType, DocAddressType.LocalCartageCFS, depot.MainAddress);
			var runSheetWithTruck = Helper.CreateRunSheet(runSheetNumber: "RS1");
			runSheetWithTruck.KG_AdHocTruckRegistration = "BQ1";
			Helper.CreateRunSheetInstruction(runSheetWithTruck, actionType == ActionTypes.Codes.PickUp ? address.PickupAction : address.DeliveryAction);
			Helper.CreateRunSheetInstruction(runSheetWithTruck, actionType == ActionTypes.Codes.PickUp ? addressForOtherActionType.DeliveryAction : addressForOtherActionType.PickupAction);
			CreateEquipmentItem(runSheetWithTruck, "SQ1", true);
			var container1 = Helper.CreatePackage(consignment, "CN1", Constants.PkgUnit.Container);
			var container2 = Helper.CreatePackage(consignment, "CN2", Constants.PkgUnit.Container);
			var package1InContainer1 = Helper.CreateChildPackage(container1, "P1", "PLT");
			var package2InContainer1 = Helper.CreateChildPackage(container1, "P2", "BOX");
			var package3InContainer2 = Helper.CreateChildPackage(container2, "P3", "ROL");
			var dataObjectWriterForRunSheetWithTruck = new DtbConsignmentRunSheetDataObjectWriter(new DataWritingManager(new ActionInfo(addressType == ConsignmentAddressTypes.Codes.PickUp ? RecipientRoleType.DTW : RecipientRoleType.ATW, runSheetWithTruck)));
			AssertEquals("Truck short code must be populated as VoyageFlightNo.", "SQ1", dataObjectWriterForRunSheetWithTruck.GetDataObject(runSheetWithTruck).VoyageFlightNo);
			var runSheetWithAdhocTruckRegistrationNumber = Helper.CreateRunSheet(runSheetNumber: "RS2");
			runSheetWithAdhocTruckRegistrationNumber.KG_AdHocTruckRegistration = "BQ1";
			Helper.CreateRunSheetInstruction(runSheetWithAdhocTruckRegistrationNumber, actionType == ActionTypes.Codes.PickUp ? address.PickupAction : address.DeliveryAction);
			Helper.CreateRunSheetInstruction(runSheetWithAdhocTruckRegistrationNumber, actionType == ActionTypes.Codes.PickUp ? addressForOtherActionType.DeliveryAction : addressForOtherActionType.PickupAction);
			var dataObjectWriterForRunSheetWithAhocTruckReg = new DtbConsignmentRunSheetDataObjectWriter(new DataWritingManager(new ActionInfo(addressType == ConsignmentAddressTypes.Codes.PickUp ? RecipientRoleType.DTW : RecipientRoleType.ATW, runSheetWithAdhocTruckRegistrationNumber)));
			AssertEquals("Adhoc truck number must be populated as VoyageFlightNo.", "BQ1", dataObjectWriterForRunSheetWithAhocTruckReg.GetDataObject(runSheetWithAdhocTruckRegistrationNumber).VoyageFlightNo);
		}

		void CreateEquipmentItem(DtbConsignmentRunSheet runSheet, string shortCode, bool isVehicle)
		{
			var equipment = Helper.CreateVehicleWithEquipmentType($"XX {shortCode}", shortCode);
			equipment.RQ_ShortCode = shortCode;
			equipment.RQ_IsVehicle = isVehicle;
			Factory.SaveForTesting();
			CreateConsignmentRunSheetEquipmentItem(runSheet.PK, equipment);
		}

		void CreateConsignmentRunSheetEquipmentItem(ZGuid consignmentRunSheetPK, RefEquipment equipment)
		{
			new CargoWise.Database.TestFramework.ObjectModel.DtbEquipmentItem()
			{ LTE_ParentID = consignmentRunSheetPK.ToGuid(), LTE_ParentTableCode = DtbConsignmentRunSheetSchema.Constants.Prefix, LTE_RQ_Equipment = equipment.PK.ToGuid(), LTE_RC_EquipmentType = equipment.RQ_RC_RoadContainerType.ToGuid() }.Insert(TestConnection);
		}
		#endregion
	}
}
