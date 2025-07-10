using System;
using System.Linq;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentRunSheetInstructionDataObjectWriterTest : DtbConsignmentUniversalTestCase
	{
		#region TestTopLevelDataContextType
		public void TestTopLevelDataContextType()
		{
			AssertEquals(DataContextType.TransportConsignmentRunSheetInstruction, ((ITopLevelDataObjectWriter)new DtbConsignmentRunSheetInstructionDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).TopLevelDataContextType);
		}

		#endregion
		#region TestEDIMessageSubType
		public void TestEDIMessageSubType()
		{
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalShipment, ((ITopLevelDataObjectWriter)new DtbConsignmentRunSheetInstructionDataObjectWriter(new DataWritingManager(new DummyActionInfo()))).EDIMessageSubType);
		}

		#endregion
		#region TestPopulateDataObject_DepotInstruction_DocAddress
		#region TestPopulateDataObject_DeliveryDepotInstruction_DocAddress
		public void TestPopulateDataObject_DeliveryDepotInstruction_DocAddress()
		{
			AssertPopulateDataObject_DepotInstruction_DocAddress(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, "LocalCartageCFS");
		}

		#endregion
		#region TestPopulateDataObject_PickupDepotInstruction_DocAddress
		public void TestPopulateDataObject_PickupDepotInstruction_SingleConsignment_DocAddress()
		{
			AssertPopulateDataObject_DepotInstruction_DocAddress(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, "LocalCartageCFS");
		}

		#endregion
		void AssertPopulateDataObject_DepotInstruction_DocAddress(string consignmentAddressType, string actionType, DocAddressType addressType, string addressTypeCode)
		{
			var org = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddressWithAction(consignment, consignmentAddressType, actionType, addressType, org.MainAddress);
			var runSheet = Helper.CreateRunSheet();
			var instruction = Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.Delivery ? address.DeliveryAction : address.PickupAction);
			var actionInfo = new ActionInfo(actionType == ActionTypes.Codes.Delivery ? RecipientRoleType.ATW : RecipientRoleType.DTW, instruction);
			var runsheetInstructionDataObjectWriter = new DtbConsignmentRunSheetInstructionDataObjectWriter(new DataWritingManager(actionInfo));
			var instructionDataObject = runsheetInstructionDataObjectWriter.GetDataObject(instruction);
			var cfsAddress = instructionDataObject.OrganizationAddressCollection;
			AssertOrganizationBO_CRAHOLSYDExists(cfsAddress, "Address must be exported", addressTypeCode, true);
		}

		#endregion
		#region TestPopulateDataObject_Instruction_NonDepotDocAddress
		#region TestPopulateDataObject_DeliveryInstruction_NonDepotDocAddress
		public void TestPopulateDataObject_DeliveryInstruction_NonDepotDocAddress()
		{
			AssertPopulateDataObject_Instruction_NonDepotDocAddress(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);
		}

		#endregion
		#region TestPopulateDataObject_PickupInstruction_NonDepotDocAddress
		public void TestPopulateDataObject_PickupInstruction_NonDepotDocAddress()
		{
			AssertPopulateDataObject_Instruction_NonDepotDocAddress(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
		}

		#endregion
		void AssertPopulateDataObject_Instruction_NonDepotDocAddress(string addressType, string actionType)
		{
			var org = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddressWithAction(consignment, addressType, actionType, DocAddressType.LocalCartageImporter, org.MainAddress);
			var runSheet = Helper.CreateRunSheet();
			var instruction = Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.Delivery ? address.DeliveryAction : address.PickupAction);
			var actionInfo = new ActionInfo(actionType == ActionTypes.Codes.Delivery ? RecipientRoleType.ATW : RecipientRoleType.DTW, instruction);
			var runsheetInstructionDataObjectWriter = new DtbConsignmentRunSheetInstructionDataObjectWriter(new DataWritingManager(actionInfo));
			var instructionDataObject = runsheetInstructionDataObjectWriter.GetDataObject(instruction);
			var cfsAddress = instructionDataObject.OrganizationAddressCollection;
			AssertNull("No organisation address must be populated since instruction is not for depot.", instructionDataObject.OrganizationAddressCollection);
		}

		#endregion
		#region TestPopulateDataObject_DepotInstruction_SingleConsignment
		public void TestPopulateDataObject_DepotInstruction_SingleConsignment()
		{
			AssertPopulateDataObject_DepotInstruction_SingleConsignment(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS);
			AssertPopulateDataObject_DepotInstruction_SingleConsignment(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
		}

		void AssertPopulateDataObject_DepotInstruction_SingleConsignment(string addressType, string actionType, DocAddressType jobDocAddressType)
		{
			var org = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var consignment = Helper.CreateConsignment();
			var container1 = Helper.CreatePackage(consignment, "CN1", Constants.PkgUnit.Container);
			var container2 = Helper.CreatePackage(consignment, "CN2", Constants.PkgUnit.Container);
			var package1InContainer1 = Helper.CreateChildPackage(container1, "P1", "PLT");
			var package2InContainer1 = Helper.CreateChildPackage(container1, "P2", "BOX");
			var package3InContainer2 = Helper.CreateChildPackage(container2, "P3", "ROL");
			var address = Helper.CreateConsignmentAddressWithAction(consignment, addressType, actionType, jobDocAddressType, org.MainAddress);
			address.Address.DocAddressType = jobDocAddressType;
			var runSheet = Helper.CreateRunSheet();
			var instruction = Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.Delivery ? address.DeliveryAction : address.PickupAction);
			var actionInfo = new ActionInfo(actionType == ActionTypes.Codes.Delivery ? RecipientRoleType.ATW : RecipientRoleType.DTW, instruction);
			var runsheetInstructionDataObjectWriter = new DtbConsignmentRunSheetInstructionDataObjectWriter(new DataWritingManager(actionInfo));
			var instructionDataObject = runsheetInstructionDataObjectWriter.GetDataObject(instruction);
			AssertEquals("Only one subshipment must exists.", 1, instructionDataObject.SubShipmentCollection.Count);
			AssertEquals("Only one container must exists.", 2, instructionDataObject.ContainerCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "CN1", "CN2" }, instructionDataObject.ContainerCollection.Select(c => c.ContainerNumber.ToString()));
		}

		#endregion
		#region TestPopulateDataObject_DepotInstruction_NoTransitWarehouseTargetted
		public void TestPopulateDataObject_DepotInstruction_NoTransitWarehouseTargetted()
		{
			AssertPopulateDataObject_DepotInstruction_SingleConsignment(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS);
			AssertPopulateDataObject_DepotInstruction_SingleConsignment(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
		}

		#endregion
		#region TestPopulateDataObject_DepotInstruction_MultipleConsignment
		public void TestPopulateDataObject_DepotInstruction_MultipleConsignment()
		{
			AssertPopulateDataObject_DepotInstruction_MultipleConsignment(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, a => a.DeliveryAction);
			AssertPopulateDataObject_DepotInstruction_MultipleConsignment(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, a => a.PickupAction);
		}

		void AssertPopulateDataObject_DepotInstruction_MultipleConsignment(string addressType, string actionType, DocAddressType jobDocAddressType, Func<DtbConsignmentAddress, DtbConsignmentAction> getAction)
		{
			var org = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var consignment1 = Helper.CreateConsignment();
			var consignment2 = Helper.CreateConsignment();
			var consignment3 = Helper.CreateConsignment();
			var container1 = Helper.CreatePackage(consignment1, "CN1", Constants.PkgUnit.Container);
			var container2 = Helper.CreatePackage(consignment1, "CN2", Constants.PkgUnit.Container);
			Helper.CreateChildPackage(container1, "P1", "PLT");
			Helper.CreateChildPackage(container1, "P2", "BOX");
			Helper.CreateChildPackage(container2, "P3", "ROL");
			var container3 = Helper.CreatePackage(consignment2, "CN3", Constants.PkgUnit.Container);
			Helper.CreateChildPackage(container3, "P4", "PLT");
			var addressInConsignment1 = Helper.CreateConsignmentAddressWithAction(consignment1, addressType, actionType, jobDocAddressType, org.MainAddress);
			var addressInConsignment2 = Helper.CreateConsignmentAddressWithAction(consignment2, addressType, actionType, jobDocAddressType, org.MainAddress);
			var differentAddressTypeInConsignment2 = Helper.CreateConsignmentAddressWithAction(consignment2, actionType == ActionTypes.Codes.PickUp ? ConsignmentAddressTypes.Codes.Delivery : ConsignmentAddressTypes.Codes.PickUp, actionType == ActionTypes.Codes.PickUp ? ActionTypes.Codes.Delivery : ActionTypes.Codes.PickUp, jobDocAddressType, org.MainAddress);
			var differentAddressTypeInConsignment3 = Helper.CreateConsignmentAddressWithAction(consignment3, actionType == ActionTypes.Codes.PickUp ? ConsignmentAddressTypes.Codes.Delivery : ConsignmentAddressTypes.Codes.PickUp, actionType == ActionTypes.Codes.PickUp ? ActionTypes.Codes.Delivery : ActionTypes.Codes.PickUp, jobDocAddressType, org.MainAddress);
			var runSheet = Helper.CreateRunSheet();
			var actionFromConsignment1 = getAction(addressInConsignment1);
			var actionFromConsignment2 = getAction(addressInConsignment2);
			var differentActionFromConsignment2 = actionType == ActionTypes.Codes.PickUp ? differentAddressTypeInConsignment2.DeliveryAction : differentAddressTypeInConsignment2.PickupAction;
			var actionFromConsignment3 = actionType == ActionTypes.Codes.PickUp ? differentAddressTypeInConsignment3.DeliveryAction : differentAddressTypeInConsignment3.PickupAction;
			var instruction = Helper.CreateRunSheetInstruction(runSheet, actionFromConsignment1, actionFromConsignment2, differentActionFromConsignment2, actionFromConsignment3);
			var actionInfo = new ActionInfo(actionType == ActionTypes.Codes.Delivery ? RecipientRoleType.ATW : RecipientRoleType.DTW, instruction);
			var runsheetInstructionDataObjectWriter = new DtbConsignmentRunSheetInstructionDataObjectWriter(new DataWritingManager(actionInfo));
			var instructionDataObject = runsheetInstructionDataObjectWriter.GetDataObject(instruction);
			AssertEquals("Two subshipments for each consignment must exists.", 2, instructionDataObject.SubShipmentCollection.Count);
			AssertEquals("Three containers must exists.", 3, instructionDataObject.ContainerCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "CN1", "CN2", "CN3" }, instructionDataObject.ContainerCollection.Select(c => c.ContainerNumber.ToString()));
		}
		#endregion
	}
}
