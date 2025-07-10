using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentAddress))]
	sealed class DtbConsignmentAddressTest : EnterpriseBusinessObjectTestCase
	{
		#region TestPickupAction

		[ExpectException(typeof(InvalidOperationException))]
		public void TestPickupAction_NoActions()
		{
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddress(consignment, "");
			var action = address.PickupAction;
		}

		public void TestPickupAction()
		{
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
			AssertEquals(address.Actions.Single(c => c.IsPickUp), address.PickupAction);
		}

		#endregion

		#region TestDeliveryAction

		[ExpectException(typeof(InvalidOperationException))]
		public void TestDeliveryAction_NoActions()
		{
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddress(consignment, "");
			var action = address.DeliveryAction;
		}

		public void TestDeliveryAction()
		{
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);

			AssertEquals(address.Actions.Single(c => c.IsDelivery), address.DeliveryAction);
		}

		#endregion

		#region TestIsPickUp

		public void TestIsPickUp()
		{
			var address = Helper.CreateConsignmentAddress();
			AssertEquals(false, address.IsPickUp);

			address.LTS_InstructionType = InstructionTypes.Codes.PickUp;
			AssertEquals(true, address.IsPickUp);

			address.LTS_InstructionType = InstructionTypes.Codes.Delivery;
			AssertEquals(false, address.IsPickUp);
		}

		#endregion

		#region TestIsDepot

		public void TestIsDepot()
		{
			var addressWithNoAddress = Helper.CreateConsignmentAddress();
			AssertEquals(false, addressWithNoAddress.IsDepot);

			var ctoAddress = Helper.CreateConsignmentAddress();
			ctoAddress.Address.DocAddressType = DocAddressType.LocalCartageCTO;
			AssertEquals(false, ctoAddress.IsDepot);

			var cfsAddress = Helper.CreateConsignmentAddress();
			cfsAddress.Address.DocAddressType = DocAddressType.LocalCartageCFS;
			AssertEquals(true, cfsAddress.IsDepot);
		}

		#endregion

		#region TestDefaultActions

		public void TestDefaultActions()
		{
			var consignorOrg = Helper.CreateOrganisation("Cnr", address1: "123 Test st");
			var consigneeOrg = Helper.CreateOrganisation("Cne", address1: "456 Test st");

			var consignment = Helper.CreateConsignment();

			var pickup = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, consignorOrg.MainAddress, "PIC", 1);
			var delivery = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, consigneeOrg.MainAddress, "DLV", 1);

			var pickupAction = Helper.CreateConsignmentAction(pickup, ActionTypes.Codes.PickUp, ZDateTimeOffset.Empty, true);
			var deliveryAction = Helper.CreateConsignmentAction(delivery, ActionTypes.Codes.Delivery, ZDateTimeOffset.Empty, true);

			AssertContainsExactElementsInAnyOrder(pickup.DefaultActions, new[] { pickupAction });
			AssertContainsExactElementsInAnyOrder(delivery.DefaultActions, new[] { deliveryAction });
		}

		#endregion

		#region TestReqFrom

		public void TestReqFrom()
		{
			var now = ZDateTimeOffset.Now;

			var consignorOrg = Helper.CreateOrganisation("Cnr", address1: "123 Test st");
			var consigneeOrg = Helper.CreateOrganisation("Cne", address1: "456 Test st");

			var consignment = Helper.CreateConsignment();

			var pickup = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, consignorOrg.MainAddress, "PIC", 1);
			var pickupAction11 = Helper.CreateConsignmentAction(pickup, ActionTypes.Codes.PickUp, ZDateTimeOffset.Empty, true);
			var pickupAction12 = Helper.CreateConsignmentAction(pickup, ActionTypes.Codes.PickUp, ZDateTimeOffset.Empty, true);
			var deliveryAction11 = Helper.CreateConsignmentAction(pickup, ActionTypes.Codes.Delivery, ZDateTimeOffset.Empty, true);
			var deliveryAction12 = Helper.CreateConsignmentAction(pickup, ActionTypes.Codes.Delivery, ZDateTimeOffset.Empty, true);

			AssertEquals("Actions with no date.", ZDateTimeOffset.Empty, pickup.ReqFrom);

			pickupAction11.LTA_RequiredFrom = now;
			pickupAction12.LTA_RequiredFrom = now.AddDays(1);
			deliveryAction11.LTA_RequiredFrom = now;
			AssertEquals("Should take pickupAction12 requiredFrom as this is the latest.", pickupAction12.LTA_RequiredFrom, pickup.ReqFrom);

			pickup.ReqFrom = now.AddDays(2);
			AssertEquals(now.AddDays(2), pickup.ReqFrom);
			AssertEquals("Should set pickupAction11 requiredFrom.", pickupAction11.LTA_RequiredFrom, pickup.ReqFrom);
			AssertEquals("Should set pickupAction11 requiredFrom.", pickupAction12.LTA_RequiredFrom, pickup.ReqFrom);

			var delivery = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, consignorOrg.MainAddress, "DLV", 1);
			var pickupAction21 = Helper.CreateConsignmentAction(delivery, ActionTypes.Codes.PickUp, ZDateTimeOffset.Empty, true);
			var pickupAction22 = Helper.CreateConsignmentAction(delivery, ActionTypes.Codes.PickUp, ZDateTimeOffset.Empty, true);
			var deliveryAction21 = Helper.CreateConsignmentAction(delivery, ActionTypes.Codes.Delivery, ZDateTimeOffset.Empty, true);
			var deliveryAction22 = Helper.CreateConsignmentAction(delivery, ActionTypes.Codes.Delivery, ZDateTimeOffset.Empty, true);

			AssertEquals("Actions with no date.", ZDateTimeOffset.Empty, delivery.ReqFrom);

			pickupAction21.LTA_RequiredFrom = now;
			deliveryAction21.LTA_RequiredFrom = now;
			deliveryAction22.LTA_RequiredFrom = now.AddDays(1);
			AssertEquals("Should take deliveryAction22 requiredFrom as this is the latest.", deliveryAction22.LTA_RequiredFrom, delivery.ReqFrom);

			delivery.ReqFrom = now.AddDays(2);
			AssertEquals(now.AddDays(2), delivery.ReqFrom);
			AssertEquals("Should set deliveryAction21 requiredFrom.", deliveryAction21.LTA_RequiredFrom, delivery.ReqFrom);
			AssertEquals("Should set deliveryAction22 requiredFrom.", deliveryAction22.LTA_RequiredFrom, delivery.ReqFrom);
		}

		#endregion

		#region TestReqTo

		public void TestReqTo()
		{
			var now = ZDateTimeOffset.Now;
			var consignorOrg = Helper.CreateOrganisation("Cnr", address1: "123 Test st");
			var consigneeOrg = Helper.CreateOrganisation("Cne", address1: "456 Test st");

			var consignment = Helper.CreateConsignment();

			var pickup = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, consignorOrg.MainAddress, "PIC", 1);
			var pickupAction11 = Helper.CreateConsignmentAction(pickup, ActionTypes.Codes.PickUp, ZDateTimeOffset.Empty, true);
			var pickupAction12 = Helper.CreateConsignmentAction(pickup, ActionTypes.Codes.PickUp, ZDateTimeOffset.Empty, true);
			var deliveryAction11 = Helper.CreateConsignmentAction(pickup, ActionTypes.Codes.Delivery, ZDateTimeOffset.Empty, true);
			var deliveryAction12 = Helper.CreateConsignmentAction(pickup, ActionTypes.Codes.Delivery, ZDateTimeOffset.Empty, true);

			AssertEquals("Actions with no date.", ZDateTimeOffset.Empty, pickup.ReqTo);

			pickupAction11.LTA_RequiredTo = now;
			pickupAction12.LTA_RequiredTo = now.AddDays(1);
			deliveryAction11.LTA_RequiredTo = now.AddDays(1);
			AssertEquals("Should take pickupAction's requiredTo as this is the latest.", pickupAction12.LTA_RequiredTo, pickup.ReqTo);

			pickup.ReqTo = now.AddDays(2);
			AssertEquals(now.AddDays(2), pickup.ReqTo);
			AssertEquals("Should set pickupAction11 requiredTo.", pickupAction11.LTA_RequiredTo, pickup.ReqTo);
			AssertEquals("Should set pickupAction12 requiredTo.", pickupAction12.LTA_RequiredTo, pickup.ReqTo);

			var delivery = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, consignorOrg.MainAddress, "DLV", 1);
			var pickupAction21 = Helper.CreateConsignmentAction(delivery, ActionTypes.Codes.PickUp, ZDateTimeOffset.Empty, true);
			var pickupAction22 = Helper.CreateConsignmentAction(delivery, ActionTypes.Codes.PickUp, ZDateTimeOffset.Empty, true);
			var deliveryAction21 = Helper.CreateConsignmentAction(delivery, ActionTypes.Codes.Delivery, ZDateTimeOffset.Empty, true);
			var deliveryAction22 = Helper.CreateConsignmentAction(delivery, ActionTypes.Codes.Delivery, ZDateTimeOffset.Empty, true);

			AssertEquals("Actions with no date.", ZDateTimeOffset.Empty, delivery.ReqTo);

			pickupAction21.LTA_RequiredTo = now;
			deliveryAction21.LTA_RequiredTo = now;
			deliveryAction22.LTA_RequiredTo = now.AddDays(1);
			AssertEquals("Should take deliveryAction22 requiredTo as this is the latest.", deliveryAction22.LTA_RequiredTo, delivery.ReqTo);

			delivery.ReqTo = now.AddDays(2);
			AssertEquals(now.AddDays(2), delivery.ReqTo);
			AssertEquals("Should set deliveryAction21 requiredTo.", deliveryAction21.LTA_RequiredTo, delivery.ReqTo);
			AssertEquals("Should set deliveryAction22 requiredTo.", deliveryAction22.LTA_RequiredTo, delivery.ReqTo);
		}

		#endregion

		#region TestIsDelivery

		public void TestIsDelivery()
		{
			var address = Helper.CreateConsignmentAddress();
			AssertEquals(false, address.IsDelivery);

			address.LTS_InstructionType = InstructionTypes.Codes.Delivery;
			AssertEquals(true, address.IsDelivery);

			address.LTS_InstructionType = InstructionTypes.Codes.PickUp;
			AssertEquals(false, address.IsDelivery);
		}

		#endregion

		#region IConsignmentAddress

		#region TestIsMulti

		public void TestIsMulti()
		{
			AssertFlag("IsMulti", InstructionTypes.Codes.Multi, InstructionTypes.Codes.Delivery);
		}

		void AssertFlag(ZString flagPropertyName, ZString validCode, ZString invalidCode)
		{
			var address = Helper.CreateConsignmentAddress();
			AssertEquals(false, address[flagPropertyName]);

			address.LTS_InstructionType = validCode;
			AssertEquals(true, address[flagPropertyName]);

			address.LTS_InstructionType = invalidCode;
			AssertEquals(false, address[flagPropertyName]);
		}

		#endregion

		#region TestUpdateStatus

		public void TestUpdateStatusWithDeletedConsignmentAddress()
		{
			var consignment = Helper.CreateConsignment();
			var address = consignment.Addresses.AddNew(InstructionTypes.Codes.PickUp);

			address.Delete();

			Assert(address.IsDeleted);
			AssertNoExceptionThrown(() => address.UpdateStatus());
		}

		public void TestUpdateStatus()
		{
			var consignment = Helper.CreateConsignment();
			var address = consignment.Addresses.AddNew(InstructionTypes.Codes.PickUp);

			address.LTS_Status = "";
			AssertEquals("Precondition", "", address.LTS_Status);

			address.UpdateStatus();
			AssertEquals(TransportStatuses.Codes.Available, address.LTS_Status);
		}

		public void TestUpdateStatusWithAddressAndActions()
		{
			var consignorOrg = Helper.CreateOrganisation("Cnr", address1: "123 Test st");
			var consigneeOrg = Helper.CreateOrganisation("Cne", address1: "456 Test st");

			var consignment = Helper.CreateConsignment();

			var pickup = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, consignorOrg.MainAddress, "PIC", 1);
			var pickupAction1 = Helper.CreateConsignmentAction(pickup, ActionTypes.Codes.PickUp, ZDateTimeOffset.Empty, false);
			var delivery = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, consignorOrg.MainAddress, "DLV", 1);
			var deliveryAction1 = Helper.CreateConsignmentAction(delivery, ActionTypes.Codes.Delivery, ZDateTimeOffset.Empty, false);

			var runSheet = Helper.CreateRunSheet();
			var pickupRunsheetInstruction = runSheet.RunSheetInstructions.AddNew();
			var deliveryRunsheetInstruction = runSheet.RunSheetInstructions.AddNew();

			var pickupAddress = consignment.PickupAddress;
			var deliveryAddress = consignment.DeliveryAddress;
			AssertConsignmentAddressStatus(pickupAddress, TransportStatuses.Codes.Available);

			pickupAction1.LTA_K1_RunSheetInstruction = pickupRunsheetInstruction.PK;
			AssertConsignmentAddressStatus(pickupAddress, TransportStatuses.Codes.Allocated);

			pickupAddress.LTS_Status = "";
			deliveryAddress.LTS_Status = "";
			pickupAddress.UpdateStatus();
			deliveryAddress.UpdateStatus();
			AssertEquals(TransportStatuses.Codes.Allocated, pickupAddress.LTS_Status);
			AssertEquals(TransportStatuses.Codes.Available, deliveryAddress.LTS_Status);

			// test for pickup Actions
			var pickupAction2 = Helper.CreateConsignmentAction(pickup, ActionTypes.Codes.PickUp, ZDateTimeOffset.Empty, false);
			AssertEquals(TransportStatuses.Codes.Available, pickupAddress.LTS_Status);
			AssertConsignmentAddressStatus(pickupAddress, TransportStatuses.Codes.Available);
			AssertAction(pickupRunsheetInstruction, pickupAddress, pickupAction2, TransportStatuses.Codes.PickedUp);

			// test for delivery Actions
			deliveryAction1.LTA_K1_RunSheetInstruction = deliveryRunsheetInstruction.PK;
			var deliveryAction2 = Helper.CreateConsignmentAction(delivery, ActionTypes.Codes.Delivery, ZDateTimeOffset.Empty, false);
			AssertConsignmentAddressStatus(deliveryAddress, TransportStatuses.Codes.Available);
			AssertAction(deliveryRunsheetInstruction, deliveryAddress, deliveryAction2, TransportStatuses.Codes.Delivered);

			var multiAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi, consignorOrg.MainAddress, "PIC", 1);
			multiAddress.Address.DocAddressType = DocAddressType.LocalCartageCTO;
			var pickupAction3 = Helper.CreateConsignmentAction(multiAddress, ActionTypes.Codes.PickUp, ZDateTimeOffset.Empty, false);
			var deliveryAction3 = Helper.CreateConsignmentAction(multiAddress, ActionTypes.Codes.Delivery, ZDateTimeOffset.Empty, false);
			AssertConsignmentAddressStatus(multiAddress, TransportStatuses.Codes.Available);

			AssertAction(runSheet.RunSheetInstructions.AddNew(), multiAddress, deliveryAction3, TransportStatuses.Codes.Delivered, allocatedStatus: TransportStatuses.Codes.DeliveryAllocated);
			AssertAction(runSheet.RunSheetInstructions.AddNew(), multiAddress, pickupAction3, TransportStatuses.Codes.PickedUp, allocatedStatus: TransportStatuses.Codes.PickUpAllocated);
		}

		void AssertAction(DtbConsignmentRunSheetInstruction runsheetInstruction, DtbConsignmentAddress address, DtbConsignmentAction action, string completeStatus, string allocatedStatus = TransportStatuses.Codes.Allocated)
		{
			action.LTA_K1_RunSheetInstruction = runsheetInstruction.PK;
			AssertConsignmentAddressStatus(address, allocatedStatus);

			runsheetInstruction.K1_TimeIn = ZDateTimeOffset.Now;
			AssertConsignmentAddressStatus(address, completeStatus);

			runsheetInstruction.K1_TimeIn = ZDateTimeOffset.Empty;
			AssertConsignmentAddressStatus(address, allocatedStatus);

			runsheetInstruction.K1_TimeOut = ZDateTimeOffset.Now;
			AssertConsignmentAddressStatus(address, completeStatus);
		}

		void AssertConsignmentAddressStatus(DtbConsignmentAddress address, string expectedStatus)
		{
			address.LTS_Status = "";
			address.UpdateStatus();
			AssertEquals(expectedStatus, address.LTS_Status);
		}
		#endregion

		#region TestIConsignmentAddress_Properties

		public void TestIConsignmentAddress_Properties()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var equipment = Factory.New<RefEquipment>();

			address.LTS_RQ_RequiredEquipment = equipment.PK;
			address.LTS_DropMode = Constants.LCLAIREquipmentNeeded.Premise;
			address.LTS_Status = ConsignmentAddressStatus.Codes.Allocated;
			address.LTS_Notes = "Test Notes";

			AssertEquals(Constants.LCLAIREquipmentNeeded.Premise, address.DropMode);
			AssertEquals(ConsignmentAddressStatus.Codes.Allocated, address.Status);
			AssertEquals("Test Notes", address.ServiceInstruction);
			AssertEquals(equipment, address.Equipment);
			AssertEquals(ConsignmentAddressTypes.Codes.PickUp, address.ConsignmentAddressType);
		}

		#endregion

		#region TestIConsignmentAddress_Packages

		public void TestIConsignmentAddress_Packages()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			AssertEquals(0, address.GetPackages.Count());

			var package = consignment.PackageJob.Packages.AddNew(Constants.PkgUnit.Package);
			AssertEquals(1, address.GetPackages.Count());
		}

		#endregion

		#region TestIConsignmentAddress_Booking

		public void TestIConsignmentAddress_Booking()
		{
			var booking = HelperBooking.CreateBooking("TB001");
			var consignment = Helper.CreateConsignment("LTC001");
			var address = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			AssertNull(address.Booking);

			var reference = consignment.AdditionalReferenceNumbers.AddNew();
			reference.CE_EntryType = TransportCommonAdditionalReferenceTypes.Codes.BookingJobId;
			reference.CE_EntryNum = booking.KM_JobID.ToString();

			AssertEquals(booking, address.Booking);
		}

		#endregion

		#endregion

		#region TestOnDocAddressChanged_DropMode

		public void TestOnDocAddressChanged_DropMode()
		{
			var consignment = Helper.CreateConsignment();
			var org1 = Helper.CreateOrganisation("O1");
			var org2 = Helper.CreateOrganisation("O2");
			var address = Helper.CreateConsignmentAddress(consignment, TransportStatuses.Codes.PickedUp);

			var address1ForOrg1 = org1.Addresses.AddNew();
			var address2ForOrg1 = org1.Addresses.AddNew();
			address1ForOrg1.OA_LCLEquipmentNeeded = "HUL";
			address2ForOrg1.OA_LCLEquipmentNeeded = "HWL";

			var addressForOrg2 = org2.Addresses.AddNew();
			addressForOrg2.OA_LCLEquipmentNeeded = "TRL";

			AssertEquals("Precondition", "", address.LTS_DropMode);

			address.Address.E2_OA_Address = address1ForOrg1.PK;
			AssertEquals("HUL", address.LTS_DropMode);

			address.Address.E2_OA_Address = address2ForOrg1.PK;
			AssertEquals("HWL", address.LTS_DropMode);

			address.Address.E2_OA_Address = addressForOrg2.PK;
			AssertEquals("TRL", address.LTS_DropMode);

			address.Address.E2_OA_Address = ZGuid.Empty;
			AssertEquals("TRL", address.LTS_DropMode);

			address.Address.E2_OA_Address = address1ForOrg1.PK;
			AssertEquals("HUL", address.LTS_DropMode);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Helper.CreateConsignmentAddress(ConsignmentAddressTypes.Codes.PickUp);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Helper.CreateConsignmentAddress(ConsignmentAddressTypes.Codes.PickUp);
		}

		#endregion

		#region Delete

		#region TestDelete_DeletesActions

		public void TestDelete_DeletesActions()
		{
			var consignment = Helper.CreateConsignment("LT001");
			var address = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var action = Helper.CreateConsignmentAction(address);
			AssertEquals("Precondition", false, action.IsDeleted);

			address.Delete();
			AssertEquals(true, address.IsDeleted);
			AssertEquals(true, action.IsDeleted);
		}

		#endregion

		#region TestDelete_DeletesDocAddress

		public void TestDelete_DeletesDocAddress()
		{
			var consignment = Helper.CreateConsignment("LT001");
			var address = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var jobDocAddress = address.Address;
			AssertEquals("Precondition", false, jobDocAddress.IsDeleted);

			address.Delete();
			AssertEquals(true, address.IsDeleted);
			AssertEquals(true, jobDocAddress.IsDeleted);
		}

		#endregion

		#endregion

		#region IDocAddresses
		public void TestIDocAddresses_GetCanOverrideCheckpoint()
		{
			var consignment = Helper.CreateConsignment("LT001");
			var address = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var docAddress = (IDocAddresses)address;
			AssertEquals(Env.Security.None, docAddress.GetCanOverrideCheckpoint(address.Address));
		}
		#endregion

		#region Helper

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		TransportBookingConsignmentTestHelper HelperBooking
		{
			get { return helperBooking ?? (helperBooking = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helperBooking;

		#endregion
	}
}
