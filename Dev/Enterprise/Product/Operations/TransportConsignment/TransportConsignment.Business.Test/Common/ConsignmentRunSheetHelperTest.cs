using System.Linq;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class ConsignmentRunSheetHelperTest : DtbConsignmentTestCaseWithFactory
	{
		#region TestIsDepotInstruction

		public void TestIsDepotInstruction()
		{
			AssertIsDepotInstruction(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			AssertIsDepotInstruction(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS);
		}

		void AssertIsDepotInstruction(string consignmentAddressType, string actionType, DocAddressType addressType)
		{
			var org = Helper.CreateOrganisation("CFS");
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddressWithAction(consignment, consignmentAddressType, actionType, addressType, org.MainAddress);

			var runSheet = Helper.CreateRunSheet();
			var instruction = Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.Delivery ? address.DeliveryAction : address.PickupAction);
			AssertEquals(true, ConsignmentRunSheetHelper.IsDepotInstruction(instruction, actionType));
		}

		#endregion

		#region TestGetInstructionAddress

		public void TestGetInstructionAddress()
		{
			AssertGetInstructionAddress(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			AssertGetInstructionAddress(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS);
		}

		void AssertGetInstructionAddress(string consignmentAddressType, string actionType, DocAddressType addressType)
		{
			var org = Helper.CreateOrganisation("CFS");
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddressWithAction(consignment, consignmentAddressType, actionType, addressType, org.MainAddress);

			var runSheet = Helper.CreateRunSheet();
			var instruction = Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.Delivery ? address.DeliveryAction : address.PickupAction);
			AssertEquals(address.Address, ConsignmentRunSheetHelper.GetInstructionAddress(instruction, actionType));
		}

		#endregion

		#region TestGetFirstAction

		public void TestGetFirstAction()
		{
			AssertGetFirstAction(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			AssertGetFirstAction(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS);
		}

		void AssertGetFirstAction(string consignmentAddressType, string actionType, DocAddressType addressType)
		{
			var org = Helper.CreateOrganisation("CFS");
			var consignment = Helper.CreateConsignment();
			var address1 = Helper.CreateConsignmentAddressWithAction(consignment, consignmentAddressType, actionType, addressType, org.MainAddress);
			var address2 = Helper.CreateConsignmentAddressWithAction(consignment, consignmentAddressType, actionType, addressType, org.MainAddress);

			var runSheet = Helper.CreateRunSheet();
			var instruction = Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.Delivery ? address1.DeliveryAction : address1.PickupAction, actionType == ActionTypes.Codes.Delivery ? address2.DeliveryAction : address2.PickupAction);
			var expectedAction = ConsignmentRunSheetHelper.GetFirstAction(instruction, actionType);
			Assert(expectedAction == address1.Actions.Single() || expectedAction == address2.Actions.Single());
		}

		#endregion

		#region TestGetAllConsignments

		public void TestGetAllConsignments()
		{
			AssertGetAllConsignments(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			AssertGetAllConsignments(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS);
		}

		void AssertGetAllConsignments(string consignmentAddressType, string actionType, DocAddressType addressType)
		{
			var org = Helper.CreateOrganisation("CFS");
			var consignment1 = Helper.CreateConsignment();
			var consignment2 = Helper.CreateConsignment();
			var addressFromConsignment1 = Helper.CreateConsignmentAddressWithAction(consignment1, consignmentAddressType, actionType, addressType, org.MainAddress);
			var addressFromConsignment2 = Helper.CreateConsignmentAddressWithAction(consignment2, consignmentAddressType, actionType, addressType, org.MainAddress);

			var runSheet = Helper.CreateRunSheet();
			var instruction = Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.Delivery ? addressFromConsignment1.DeliveryAction : addressFromConsignment1.PickupAction,
				actionType == ActionTypes.Codes.Delivery ? addressFromConsignment2.DeliveryAction : addressFromConsignment2.PickupAction);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1, consignment2 }, ConsignmentRunSheetHelper.GetAllConsignments(instruction, actionType));
		}

		#endregion
	}
}
