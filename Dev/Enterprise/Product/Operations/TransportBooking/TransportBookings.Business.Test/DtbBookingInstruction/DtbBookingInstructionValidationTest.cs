using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business.Test
{
	sealed class DtbBookingInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestKN_IsContainerRateable()
		{
			var booking = Helper.CreateBooking();
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Both;
			var picInstruction = booking.Instructions.AddNew();
			var dlvInstruction = booking.Instructions.AddNew();
			var thirdInstruction = booking.Instructions.AddNew();

			picInstruction.KN_IsContainerRateable = true;
			AssertEquals(true, picInstruction.KN_IsContainerRateableInfo.HasErrors());

			dlvInstruction.KN_IsContainerRateable = true;
			AssertEquals(false, picInstruction.KN_IsContainerRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.KN_IsContainerRateableInfo.HasErrors());

			thirdInstruction.KN_IsContainerRateable = true;
			AssertEquals(false, picInstruction.KN_IsContainerRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.KN_IsContainerRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.KN_IsContainerRateableInfo.HasErrors());
		}

		public void TestKN_IsLooseRateable()
		{
			var booking = Helper.CreateBooking();
			booking.KM_RatingFreightMode = RatingFreightModes.Codes.Both;
			var picInstruction = booking.Instructions.AddNew();
			var dlvInstruction = booking.Instructions.AddNew();
			var thirdInstruction = booking.Instructions.AddNew();

			picInstruction.KN_IsLooseRateable = true;
			AssertEquals(true, picInstruction.KN_IsLooseRateableInfo.HasErrors());

			dlvInstruction.KN_IsLooseRateable = true;
			AssertEquals(false, picInstruction.KN_IsLooseRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.KN_IsLooseRateableInfo.HasErrors());

			thirdInstruction.KN_IsLooseRateable = true;
			AssertEquals(false, picInstruction.KN_IsLooseRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.KN_IsLooseRateableInfo.HasErrors());
			AssertEquals(false, dlvInstruction.KN_IsLooseRateableInfo.HasErrors());
		}

		public void TestKN_Sequence()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = booking.Instructions.AddNew();
			var instruction2 = booking.Instructions.AddNew();
			var instruction3 = booking.Instructions.AddNew();
			AssertNoErrors(instruction1.KN_SequenceInfo);
			AssertNoErrors(instruction2.KN_SequenceInfo);
			AssertNoErrors(instruction3.KN_SequenceInfo);

			instruction1.KN_Sequence = 0;
			AssertHasError(instruction1.KN_SequenceInfo, "The first Instruction should have a sequence number of 1.");

			instruction1.KN_Sequence = 1;
			instruction3.KN_Sequence = 4;
			AssertNoErrors(instruction1.KN_SequenceInfo);
			AssertHasError(instruction3.KN_SequenceInfo, "Sequence must be sequential integers, continuously increasing by one.");

			instruction3.KN_Sequence = 3;
			AssertNoErrors(instruction1.KN_SequenceInfo);
			AssertNoErrors(instruction2.KN_SequenceInfo);
			AssertNoErrors(instruction3.KN_SequenceInfo);
		}

		public void TestActualPickupBeforeDelivery_SamePackage()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pallet = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickupDivot = pickupInstruction.PackageDivots.AddNew();
			pickupDivot.KD_KP_Package = pallet.PK;
			Helper.CreateConfirmation(pickupDivot, ConfirmationTypes.Codes.PickUp);

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryDivot = deliveryInstruction.PackageDivots.AddNew();
			deliveryDivot.KD_KP_Package = pallet.PK;
			Helper.CreateConfirmation(deliveryDivot, ConfirmationTypes.Codes.Delivery);

			Assert("Instruction should be Pickup", pickupInstruction.IsPickUp);
			Assert("Instruction should be Delivery", deliveryInstruction.IsDelivery);
			Assert("Both instructions should have the same package", pickupInstruction.DivotsWithPackages.Packages.Single().PK == deliveryInstruction.DivotsWithPackages.Packages.Single().PK);

			const string dateIsEarlierWarning = "Date is earlier than a previous Instruction for the same package.";
			const string dateIsLaterWarning = "Date is later than a following Instruction for the same package.";

			var justNow = DateTime.Now;
			pickupInstruction.Actual = justNow;
			deliveryInstruction.Actual = justNow;

			AssertNoWarnings(pickupInstruction.ActualInfo);
			AssertNoWarnings(deliveryInstruction.ActualInfo);

			deliveryInstruction.Actual = justNow.AddHours(1);
			AssertNoWarnings(pickupInstruction.ActualInfo);
			AssertNoWarnings(deliveryInstruction.ActualInfo);

			deliveryInstruction.Actual = justNow.AddHours(-1);
			AssertHasWarning(pickupInstruction.ActualInfo, dateIsLaterWarning);
			AssertHasWarning(deliveryInstruction.ActualInfo, dateIsEarlierWarning);
			AssertEquals("Delivery Instruction should have 1 warning", 1, deliveryInstruction.ActualInfo.GetWarnings().Count());

			deliveryInstruction.Actual = justNow.AddHours(-2);
			AssertEquals("Should not add the same warning twice", 1, deliveryInstruction.ActualInfo.GetWarnings().Count());

			deliveryInstruction.Actual = justNow;
			AssertNoWarning(deliveryInstruction.ActualInfo, dateIsEarlierWarning);
			AssertNoWarning(pickupInstruction.ActualInfo, dateIsLaterWarning);
		}

		public void TestActualPickupBeforeDelivery_SamePackageSplitDelivery()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pallet = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);
			var threeBoxes = Helper.CreatePackage("", 3, Constants.PkgUnit.Box);
			var sevenBoxes = Helper.CreatePackage("", 7, Constants.PkgUnit.Box);

			threeBoxes.KP_KP_ParentPackage = pallet.PK;
			sevenBoxes.KP_KP_ParentPackage = pallet.PK;

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickupDivot = pickupInstruction.PackageDivots.AddNew();
			pickupDivot.KD_KP_Package = pallet.PK;
			Helper.CreateConfirmation(pickupDivot, ConfirmationTypes.Codes.PickUp);

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryDivot = deliveryInstruction.PackageDivots.AddNew();
			deliveryDivot.KD_KP_Package = threeBoxes.PK;
			Helper.CreateConfirmation(deliveryDivot, ConfirmationTypes.Codes.Delivery);

			Assert("Instruction should be Pickup", pickupInstruction.IsPickUp);
			Assert("Instruction should be Delivery", deliveryInstruction.IsDelivery);
			Assert("Instructions should have different packages", pickupInstruction.DivotsWithPackages.Packages.Single().PK != deliveryInstruction.DivotsWithPackages.Packages.Single().PK);

			var justNow = DateTime.Now;
			pickupInstruction.Actual = justNow;
			deliveryInstruction.Actual = justNow;
			AssertNoWarnings(pickupInstruction.ActualInfo);
			AssertNoWarnings(deliveryInstruction.ActualInfo);

			deliveryInstruction.Actual = justNow.AddHours(1);
			AssertNoWarnings(pickupInstruction.ActualInfo);
			AssertNoWarnings(deliveryInstruction.ActualInfo);

			deliveryInstruction.Actual = justNow.AddHours(-1);
			AssertNoWarnings("Is not expected to detect the pickup being after the delivery", pickupInstruction.ActualInfo);
			AssertNoWarnings("Is not expected to detect the delivery being before the pickup", deliveryInstruction.ActualInfo);
		}

		public void TestActualPickupBeforeDelivery_DifferentPackage()
		{
			var pallet1 = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);
			var pallet2 = Helper.CreatePackage("PLT-2", 1, Constants.PkgUnit.Pallet);

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();

			AssertPickupIsAfterDelivery(booking, pallet1, pallet2, (i) => i.ActualInfo);
		}

		public void TestActualPickupBeforeDelivery_SplitDeliveryOffPallet()
		{
			var pallet1 = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);
			var threeBoxes = Helper.CreatePackage("", 3, Constants.PkgUnit.Box);
			var sevenBoxes = Helper.CreatePackage("", 7, Constants.PkgUnit.Box);

			threeBoxes.KP_KP_ParentPackage = pallet1.PK;
			sevenBoxes.KP_KP_ParentPackage = pallet1.PK;

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();

			AssertPickupIsAfterDelivery(booking, threeBoxes, sevenBoxes, (i) => i.ActualInfo);
		}

		public void TestEstimatedPickupBeforeDelivery_SamePackage()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pallet = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickupDivot = pickupInstruction.PackageDivots.AddNew();
			pickupDivot.KD_KP_Package = pallet.PK;
			Helper.CreateConfirmation(pickupDivot, ConfirmationTypes.Codes.PickUp);

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryDivot = deliveryInstruction.PackageDivots.AddNew();
			deliveryDivot.KD_KP_Package = pallet.PK;
			Helper.CreateConfirmation(deliveryDivot, ConfirmationTypes.Codes.Delivery);

			Assert("Instruction should be Pickup", pickupInstruction.IsPickUp);
			Assert("Instruction should be Delivery", deliveryInstruction.IsDelivery);
			Assert("Both instructions should have the same package", pickupInstruction.DivotsWithPackages.Packages.Single().PK == deliveryInstruction.DivotsWithPackages.Packages.Single().PK);

			const string dateIsEarlierWarning = "Date is earlier than a previous Instruction for the same package.";
			const string dateIsLaterWarning = "Date is later than a following Instruction for the same package.";

			var justNow = DateTime.Now;
			pickupInstruction.Estimated = justNow;
			deliveryInstruction.Estimated = justNow;

			AssertNoWarnings(pickupInstruction.EstimatedInfo);
			AssertNoWarnings(deliveryInstruction.EstimatedInfo);

			deliveryInstruction.Estimated = justNow.AddHours(1);
			AssertNoWarnings(pickupInstruction.EstimatedInfo);
			AssertNoWarnings(deliveryInstruction.EstimatedInfo);

			deliveryInstruction.Estimated = justNow.AddHours(-1);
			AssertHasWarning(pickupInstruction.EstimatedInfo, dateIsLaterWarning);
			AssertHasWarning(deliveryInstruction.EstimatedInfo, dateIsEarlierWarning);

			deliveryInstruction.Estimated = justNow.AddHours(-2);
			AssertHasWarning(pickupInstruction.EstimatedInfo, dateIsLaterWarning);
			AssertHasWarning(deliveryInstruction.EstimatedInfo, dateIsEarlierWarning);

			deliveryInstruction.Estimated = justNow;
			AssertNoWarning(deliveryInstruction.EstimatedInfo, dateIsEarlierWarning);
			AssertNoWarning(pickupInstruction.EstimatedInfo, dateIsLaterWarning);
		}

		public void TestEstimatedPickupBeforeDelivery_SamePackageSplitDelivery()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pallet = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);
			var threeBoxes = Helper.CreatePackage("", 3, Constants.PkgUnit.Box);
			var sevenBoxes = Helper.CreatePackage("", 7, Constants.PkgUnit.Box);

			threeBoxes.KP_KP_ParentPackage = pallet.PK;
			sevenBoxes.KP_KP_ParentPackage = pallet.PK;

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickupDivot = pickupInstruction.PackageDivots.AddNew();
			pickupDivot.KD_KP_Package = pallet.PK;
			Helper.CreateConfirmation(pickupDivot, ConfirmationTypes.Codes.PickUp);

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryDivot = deliveryInstruction.PackageDivots.AddNew();
			deliveryDivot.KD_KP_Package = threeBoxes.PK;
			Helper.CreateConfirmation(deliveryDivot, ConfirmationTypes.Codes.Delivery);

			Assert("Instruction should be Pickup", pickupInstruction.IsPickUp);
			Assert("Instruction should be Delivery", deliveryInstruction.IsDelivery);
			Assert("Instructions should have different packages", pickupInstruction.DivotsWithPackages.Packages.Single().PK != deliveryInstruction.DivotsWithPackages.Packages.Single().PK);

			var justNow = DateTime.Now;
			pickupInstruction.Estimated = justNow;
			deliveryInstruction.Estimated = justNow;

			AssertNoWarnings(pickupInstruction.EstimatedInfo);
			AssertNoWarnings(deliveryInstruction.EstimatedInfo);

			deliveryInstruction.Estimated = justNow.AddHours(1);
			AssertNoWarnings(pickupInstruction.EstimatedInfo);
			AssertNoWarnings(deliveryInstruction.EstimatedInfo);

			deliveryInstruction.Estimated = justNow.AddHours(-1);
			AssertNoWarnings("Is not expected to detect the pickup being after the delivery", pickupInstruction.EstimatedInfo);
			AssertNoWarnings("Is not expected to detect the delivery being before the pickup", deliveryInstruction.EstimatedInfo);
		}

		public void TestEstimatedPickupBeforeDelivery_DifferentPackage()
		{
			var pallet1 = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);
			var pallet2 = Helper.CreatePackage("PLT-2", 1, Constants.PkgUnit.Pallet);

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();

			AssertPickupIsAfterDelivery(booking, pallet1, pallet2, (i) => i.EstimatedInfo);
		}

		public void TestEstimatedPickupBeforeDelivery_SplitDeliveryOffPallet()
		{
			var pallet1 = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);
			var threeBoxes = Helper.CreatePackage("", 3, Constants.PkgUnit.Box);
			var sevenBoxes = Helper.CreatePackage("", 7, Constants.PkgUnit.Box);

			threeBoxes.KP_KP_ParentPackage = pallet1.PK;
			sevenBoxes.KP_KP_ParentPackage = pallet1.PK;

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();

			AssertPickupIsAfterDelivery(booking, threeBoxes, sevenBoxes, (i) => i.EstimatedInfo);
		}

		void AssertPickupIsAfterDelivery(DtbBooking booking, PkgPackage pkgA, PkgPackage pkgB, Func<DtbBookingInstruction, ZPropertyInfo<ZDateTime>> getDateInfoFunc)
		{
			var pickupInstructionA = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickupDivotA = pickupInstructionA.PackageDivots.AddNew();
			pickupDivotA.KD_KP_Package = pkgA.PK;
			Helper.CreateConfirmation(pickupDivotA, ConfirmationTypes.Codes.PickUp);

			var pickupInstructionB = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickupDivotB = pickupInstructionB.PackageDivots.AddNew();
			pickupDivotB.KD_KP_Package = pkgB.PK;
			Helper.CreateConfirmation(pickupDivotB, ConfirmationTypes.Codes.PickUp);

			var deliveryInstructionA = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryDivotA = deliveryInstructionA.PackageDivots.AddNew();
			deliveryDivotA.KD_KP_Package = pkgA.PK;
			Helper.CreateConfirmation(deliveryDivotA, ConfirmationTypes.Codes.Delivery);

			var deliveryInstructionB = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryDivotB = deliveryInstructionB.PackageDivots.AddNew();
			deliveryDivotB.KD_KP_Package = pkgB.PK;
			Helper.CreateConfirmation(deliveryDivotB, ConfirmationTypes.Codes.Delivery);

			Assert("Instruction should be Pickup", pickupInstructionA.IsPickUp);
			Assert("Instruction should be Delivery", deliveryInstructionA.IsDelivery);
			Assert("Both instructions should have the same package", pickupInstructionA.DivotsWithPackages.Packages.Single().PK == deliveryInstructionA.DivotsWithPackages.Packages.Single().PK);

			Assert("Instruction should be Pickup", pickupInstructionB.IsPickUp);
			Assert("Instruction should be Delivery", deliveryInstructionB.IsDelivery);
			Assert("Both instructions should have the same package", pickupInstructionB.DivotsWithPackages.Packages.Single().PK == deliveryInstructionB.DivotsWithPackages.Packages.Single().PK);

			Assert("Both instructions should have different packages", pickupInstructionA.DivotsWithPackages.Packages.Single().PK != pickupInstructionB.DivotsWithPackages.Packages.Single().PK);
			Assert("Both instructions should have different packages", deliveryInstructionA.DivotsWithPackages.Packages.Single().PK != deliveryInstructionB.DivotsWithPackages.Packages.Single().PK);

			Assert("Pickup A should be sequenced before Pickup B", pickupInstructionA.KN_Sequence < pickupInstructionB.KN_Sequence);
			Assert("Delivery A should be sequenced before Delivery B", deliveryInstructionA.KN_Sequence < deliveryInstructionB.KN_Sequence);
			Assert("Pickup B should be sequenced before Delivery A", pickupInstructionB.KN_Sequence < deliveryInstructionA.KN_Sequence);

			const string dateIsEarlierWarning = "Date is earlier than a previous Instruction for the same package.";
			const string dateIsLaterWarning = "Date is later than a following Instruction for the same package.";

			var pickupInstructionA_DateInfo = getDateInfoFunc(pickupInstructionA);
			var deliveryInstructionA_DateInfo = getDateInfoFunc(deliveryInstructionA);
			var pickupInstructionB_DateInfo = getDateInfoFunc(pickupInstructionB);
			var deliveryInstructionB_DateInfo = getDateInfoFunc(deliveryInstructionB);

			var justNow = DateTime.Now;

			pickupInstructionA_DateInfo.Value = justNow;
			deliveryInstructionA_DateInfo.Value = justNow;
			pickupInstructionB_DateInfo.Value = justNow;
			deliveryInstructionB_DateInfo.Value = justNow;

			AssertNoWarnings(getDateInfoFunc(pickupInstructionA));
			AssertNoWarnings(getDateInfoFunc(deliveryInstructionA));
			AssertNoWarnings(getDateInfoFunc(pickupInstructionB));
			AssertNoWarnings(getDateInfoFunc(deliveryInstructionB));

			deliveryInstructionA_DateInfo.Value = justNow.AddHours(2);
			deliveryInstructionB_DateInfo.Value = justNow.AddHours(1);
			AssertNoWarnings(getDateInfoFunc(pickupInstructionA));
			AssertNoWarnings(getDateInfoFunc(deliveryInstructionA));
			AssertNoWarnings(getDateInfoFunc(pickupInstructionB));
			AssertNoWarnings(getDateInfoFunc(deliveryInstructionB));

			deliveryInstructionA_DateInfo.Value = justNow.AddHours(-1);
			AssertHasWarning(getDateInfoFunc(pickupInstructionA), dateIsLaterWarning);
			AssertHasWarning(getDateInfoFunc(deliveryInstructionA), dateIsEarlierWarning);
			AssertNoWarning(getDateInfoFunc(pickupInstructionB), dateIsLaterWarning);
			AssertNoWarning(getDateInfoFunc(deliveryInstructionB), dateIsEarlierWarning);

			deliveryInstructionA_DateInfo.Value = justNow;
			deliveryInstructionB_DateInfo.Value = justNow.AddHours(-2);
			AssertNoWarning(getDateInfoFunc(pickupInstructionA), dateIsLaterWarning);
			AssertNoWarning(getDateInfoFunc(deliveryInstructionA), dateIsEarlierWarning);
			AssertHasWarning(getDateInfoFunc(pickupInstructionB), dateIsLaterWarning);
			AssertHasWarning(getDateInfoFunc(deliveryInstructionB), dateIsEarlierWarning);
		}

		public void TestKN_DropMode()
		{
			var instruction = GetNewInstruction();
			instruction.Validation.ValidateKN_DropMode();
			AssertNoErrors("Precondition", instruction.KN_DropModeInfo);

			instruction.KN_DropMode = "XXX";
			AssertHasError(instruction.KN_DropModeInfo, "Enter a valid Drop Mode.");

			instruction.KN_DropMode = "SDL";
			AssertNoErrors(instruction.KN_DropModeInfo);
		}

		public void TestKN_DropMode_WhenSendingXUSToCTO()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking);
			instruction.Validation.ValidateKN_DropMode();

			AssertEquals("Precondition: KN_DropMode is blank", string.Empty, instruction.KN_DropMode);
			AssertEquals("Precondition: IsSendingXUSToCTO is false", false, booking.IsSendingXUSToCTO);
			AssertNoNotifications("Precondition: Drop mode doesn't have any kind of notification as ValidateForSendingXUSToCTO has not been run", instruction.KN_DropModeInfo);

			booking.KM_Status = TransportStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);
			instruction.Validation.ValidateKN_DropMode();

			AssertHasMessageError("Should have message error for drop mode", instruction.KN_DropModeInfo, "This Booking must have Drop Mode entered for all Instructions.");

			instruction.KN_DropMode = Constants.FCLEquipmentNeeded.WaitForUnpack;

			AssertNoNotifications("Drop mode should not have any kind of notification", instruction.KN_DropModeInfo);
		}

		public void TestKN_InstructionType()
		{
			var instruction = GetNewInstruction();
			instruction.Validation.ValidateKN_InstructionType();
			AssertHasErrors(instruction.KN_InstructionTypeInfo);

			instruction.KN_InstructionType = "XXX";
			AssertHasErrors(instruction.KN_InstructionTypeInfo);

			instruction.KN_InstructionType = "PIC";
			AssertNoErrors(instruction.KN_InstructionTypeInfo);
		}

		public void TestOrganisationType()
		{
			var instruction = GetNewInstruction();
			instruction.Validation.ValidateOrganisationType();
			AssertHasError(instruction.OrganisationTypeInfo, "Please enter an Organization Type.");

			instruction.OrganisationType = "XXX";
			AssertHasError(instruction.OrganisationTypeInfo, "Enter a valid Organization Type.");

			instruction.OrganisationType = "CTO";
			AssertNoErrors(instruction.OrganisationTypeInfo);
		}

		readonly string errorMsgNoCNE = "There is no Consignee Address for this job, this means you cannot give the Authority to Leave.";
		readonly string errorMsgNoCNR = "There is no Consignor Address for this job, this means you cannot give the Authority to Leave.";
		readonly string errorMsgNoCM = "There is no Client/Billing Party Address for this job, this means you cannot give the Authority to Leave.";
		readonly string errorMsgATLFalseCNE = "The consignee/delivery address has specified that they do not provide Authority to Leave, which means you cannot give the Authority to Leave for this booking.";
		readonly string errorMsgATLFalseCNR = "The consignor/pickup address has specified that they do not provide Authority to Leave, which means you cannot give the Authority to Leave for this booking.";
		readonly string errorMsgATLFalseCM = "The local client address has specified that they do not provide Authority to Leave, which means you cannot give the Authority to Leave for this booking.";
		readonly string errorMsgIsPickup = "Authority to Leave is only available to be given for consignee deliveries.";
		readonly string errorMsgIsContainer = "Authority to Leave is not available to be given when delivering containers.";

		public void TestAuthorisedToLeave()
		{
			var data = ATLTestData();
			data.SetUpOrganizationTestData();
			data.SetUpBookingTestData(GetNewBooking(), GetNewBooking());

			AssertEquals("Should default to pickup address", data.PickupAddress1.PK, data.PickupInstruction1.Address.E2_OA_Address);
			AssertEquals("Should default to delivery address", data.DeliveryAddress1.PK, data.DeliveryInstruction1.Address.E2_OA_Address);
			AssertEquals("ATL default value is false", false, data.DeliveryInstruction1.KN_IsAuthorisedToLeave);
			data.PickupAddress1.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.NO;
			data.DeliveryAddress1.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.NO;
			data.ClientAddress1.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.NO;

			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			AssertHasError("User should receive error because CNE's ATL is false", data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgATLFalseCNE);
			AssertHasError("User should receive error because CNR's ATL is false", data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgATLFalseCNR);
			AssertHasError("User should receive error because CM's ATL is false", data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgATLFalseCM);

			data.PickupAddress2.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			data.DeliveryAddress2.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;
			data.ClientAddress2.OA_AuthorityToLeave = AuthorityToLeaveOptions.Codes.YES;

			data.DeliveryInstruction2.KN_IsAuthorisedToLeave = true;
			AssertNoErrors("User should NOT receive any errors because all ATL's are set to true", data.DeliveryInstruction2.KN_IsAuthorisedToLeaveInfo);
		}

		public void TestAuthorisedToLeave_ConsignorYESAndConsigneeYESAndClientYES()
		{
			string[] noErrors = Array.Empty<string>();
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, noErrors);
		}

		public void TestAuthorisedToLeave_ConsignorYESAndConsigneeYESAndClientNO()
		{
			string[] errors = { errorMsgATLFalseCM };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, errors);
		}

		public void TestAuthorisedToLeave_ConsignorYESAndConsigneeNOAndClientYES()
		{
			string[] errors = { errorMsgATLFalseCNE };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, errors);
		}

		public void TestAuthorisedToLeave_ConsignorYESAndConsigneeNOAndClientNO()
		{
			string[] errors = { errorMsgATLFalseCNE, errorMsgATLFalseCM };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, errors);
		}

		public void TestAuthorisedToLeave_ConsignorNOAndConsigneeYESAndClientYES()
		{
			string[] errors = { errorMsgATLFalseCNR };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.YES, errors);
		}

		public void TestAuthorisedToLeave_ConsignorNOAndConsigneeYESAndClientNO()
		{
			string[] errors = { errorMsgATLFalseCNR, errorMsgATLFalseCM };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, AuthorityToLeaveOptions.Codes.NO, errors);
		}

		public void TestAuthorisedToLeave_ConsignorNOAndConsigneeNOAndClientYES()
		{
			string[] errors = { errorMsgATLFalseCNR, errorMsgATLFalseCNE };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.YES, errors);
		}

		public void TestAuthorisedToLeave_ConsignorNOAndConsigneeNOAndClientNO()
		{
			string[] errors = { errorMsgATLFalseCNR, errorMsgATLFalseCNE, errorMsgATLFalseCM };
			AssertAuthorityToLeave(AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, AuthorityToLeaveOptions.Codes.NO, errors);
		}

		void AssertAuthorityToLeave(string consignorATL, string consigneeATL, string clientATL, string[] expectedValidationErrors)
		{
			var data = ATLTestData();
			data.SetUpOrganizationTestData();
			data.PickupAddress1.OA_AuthorityToLeave = consignorATL;
			data.DeliveryAddress1.OA_AuthorityToLeave = consigneeATL;
			data.ClientAddress1.OA_AuthorityToLeave = clientATL;
			Factory.Save();

			data.SetUpBookingTestData(GetNewBooking(), GetNewBooking());
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			data.DeliveryInstruction1.Validation.ValidateKN_IsAuthorisedToLeave();

			foreach (var error in expectedValidationErrors)
			{
				AssertHasError(data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, error);
			}

			AssertEquals("The only errors found on the delivery instruction should be the ones that were expected", data.DeliveryInstruction1.Notifications.Count(), expectedValidationErrors.Length);
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = false;
			data.DeliveryInstruction1.Validation.ValidateKN_IsAuthorisedToLeave();
			AssertNoErrors(data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo);
		}

		OrgAddress AddAddressToOrganisation(OrgHeader organisation, ZString address1, OrgAddressType addressType)
		{
			var address = organisation.Addresses.AddNew();

			address.AddressCapability.SetCapabilityEnabled(addressType);
			address.AddressCapability.SetIsMainAddress(addressType);
			address.OA_Address1 = address1;

			return address;
		}

		public void TestValidationErrorsWhenAddressIsNull()
		{
			//Setup Instructions
			var booking = GetNewBooking();
			var pickupInstruction = booking.Instructions.AddNew();
			var deliveryInstruction = booking.Instructions.AddNew();

			//Setup Consignor
			var pickupOrganisation = Factory.New<OrgHeader>();
			pickupOrganisation.OH_Code = "PICSYD";
			pickupInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			pickupInstruction.OrganisationType = OrganisationTypesList.Codes.CNR;

			//Setup Consignee
			var deliveryOrganisation = Factory.New<OrgHeader>();
			deliveryOrganisation.OH_Code = "DELSYD";
			deliveryInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			deliveryInstruction.OrganisationType = OrganisationTypesList.Codes.CNE;

			//Setup Client
			var clientOrganisation = Factory.New<OrgHeader>();
			clientOrganisation.OH_Code = "CLISYD";
			Factory.Save();

			AssertEquals("Precondition: KN_IsAuthorisedToLeave should be false.", false, deliveryInstruction.KN_IsAuthorisedToLeave);
			deliveryInstruction.Validation.ValidateKN_IsAuthorisedToLeave();
			AssertNoErrors("Assert only returns errors if KN_IsAuthorisedToLeave is set to true", deliveryInstruction.KN_IsAuthorisedToLeaveInfo);
			deliveryInstruction.KN_IsAuthorisedToLeave = true;

			//Assert correct valudation occurs
			AssertHasError("User should receive an error after attempting to change ATL to true since CNR's address is null", deliveryInstruction.KN_IsAuthorisedToLeaveInfo, errorMsgNoCNR);
			AssertHasError("User should receive an error after attempting to change ATL to true since CNE's address is null", deliveryInstruction.KN_IsAuthorisedToLeaveInfo, errorMsgNoCNE);
			AssertHasError("User should receive an error after attempting to change ATL to true since CM's address is null", deliveryInstruction.KN_IsAuthorisedToLeaveInfo, errorMsgNoCM);
			AssertEquals("Since there is no CNR address, the user should not receive an error stating that the CNR's ATL is false", false, deliveryInstruction.KN_IsAuthorisedToLeaveInfo.HasError(errorMsgATLFalseCNR));
			AssertEquals("Since there is no CNE address, the user should not receive an error stating that the CNE's ATL is false", false, deliveryInstruction.KN_IsAuthorisedToLeaveInfo.HasError(errorMsgATLFalseCNE));
			AssertEquals("Since there is no CM address, the user should not receive an error stating that the CM's ATL is false", false, deliveryInstruction.KN_IsAuthorisedToLeaveInfo.HasError(errorMsgATLFalseCM));

			//Setup Consignor Address and re-test validation
			var pickupAddress = AddAddressToOrganisation(pickupOrganisation, "Pickup Addy", OrgAddressType.Pickup);
			pickupInstruction.Address.OrganisationPK = pickupOrganisation.PK;
			AssertEquals("User should NOT receive an error after attempting to change ATL to true since CNR's address is NO LONGER null", false, deliveryInstruction.KN_IsAuthorisedToLeaveInfo.HasError(errorMsgNoCNR));

			//Setup Consignee Address and re-test validation
			var deliveryAddress = AddAddressToOrganisation(deliveryOrganisation, "Delivery Addy", OrgAddressType.Delivery);
			deliveryInstruction.Address.OrganisationPK = deliveryOrganisation.PK;
			AssertEquals("User should NOT receive an error after attempting to change ATL to true since CNE's address is NO LONGER null", false, deliveryInstruction.KN_IsAuthorisedToLeaveInfo.HasError(errorMsgNoCNE));

			//Setup Client Address and re-test validation
			var clientAddress = AddAddressToOrganisation(clientOrganisation, "Client Addy", OrgAddressType.Miscellaneous);
			var job = new JobHeader.Loader(booking).TryCreate();
			job.JH_OA_LocalChargesAddr = clientAddress.PK;
			clientAddress = deliveryInstruction.Booking.BillingPartyOrLocalClientAddress;
			AssertEquals("User should NOT receive an error after attempting to change ATL to true since CM's address is NO LONGER null", false, deliveryInstruction.KN_IsAuthorisedToLeaveInfo.HasError(errorMsgNoCM));
		}

		public void TestValidationIfIsPickup()
		{
			var data = ATLTestData();
			data.SetUpOrganizationTestData();
			data.SetUpBookingTestData(GetNewBooking(), GetNewBooking());
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			AssertNoError(data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgIsPickup);

			data.DeliveryInstruction1.KN_InstructionType = InstructionTypes.Codes.PickUp;
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			data.DeliveryInstruction1.Validation.ValidateKN_IsAuthorisedToLeave();
			AssertHasError(data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgIsPickup);
		}

		public void TestValidationIfHasContainer()
		{
			// Setup Data
			var data = ATLTestData();
			data.SetUpOrganizationTestData();
			data.SetUpBookingTestData(GetNewBooking(), GetNewBooking());
			AssertEquals("Precondition: Package count should be 0", 0, data.DeliveryInstruction1.DivotsWithPackages.Typed.Count());
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			AssertNoError("There are no packages for this delivery, therefore the user should not receive an error regarding containers/packages", data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgIsContainer);

			// Add package
			var package = data.Booking1.PackageJob.Packages.AddNew("BOX", 1);
			data.DeliveryInstruction1.DivotsWithPackages.AddPackage(package);
			AssertEquals("Precondition: There should be 1 package on the instruction", 1, data.DeliveryInstruction1.DivotsWithPackages.Typed.Count());
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			AssertNoError("There are no containers for this delivery, therefore the user should not receive an error regarding containers", data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgIsContainer);

			// Add container
			var container = data.Booking1.PackageJob.Packages.AddNew("CNT", 1);
			data.DeliveryInstruction1.DivotsWithPackages.AddPackage(container);
			AssertEquals("Precondition: There should be 2 packages on the instruction", 2, data.DeliveryInstruction1.DivotsWithPackages.Typed.Count());
			data.DeliveryInstruction1.KN_IsAuthorisedToLeave = true;
			data.DeliveryInstruction1.Validation.ValidateKN_IsAuthorisedToLeave();
			AssertHasError("A container has been added for delivery, therefore the user should be unable to give authority to leave and should receive an error", data.DeliveryInstruction1.KN_IsAuthorisedToLeaveInfo, errorMsgIsContainer);
		}

		public void TestValidateAll()
		{
			var instruction = GetNewInstruction();
			instruction.OrganisationType = "CTO";
			AssertNoErrors("Precondition", instruction.OrganisationTypeInfo);

			using (instruction.GetValidationSuspender())
			{
				instruction.OrganisationType = "XXX";
			}

			instruction.Validation.ValidateAll();
			AssertHasError(instruction.OrganisationTypeInfo, "Enter a valid Organization Type.");
		}

		DtbBookingInstructionBizOTest.ATLTestData ATLTestData()
		{
			return new DtbBookingInstructionBizOTest.ATLTestData(Helper);
		}

		DtbBookingInstruction GetNewInstruction()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			return booking.Instructions.AddNew();
		}

		DtbBooking GetNewBooking()
		{
			return Helper.CreateBooking();
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
