using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DtbBookingConfirmationValidationTest : DtbTransportConfirmationValidationTest
	{
		public void TestValidateParentID_InstructionOrPackage()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var divot1 = instruction.PackageDivots.AddNew();
			var divot2 = instruction.PackageDivots.AddNew();
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			divot1.KD_KP_Package = package1.PK;
			divot2.KD_KP_Package = package2.PK;
			var confirmation = instruction.Confirmations.AddNew();

			confirmation.Validation.ValidateParentID_InstructionOrPackageDivot();
			AssertNoErrors(confirmation.ParentID_InstructionOrPackageDivotInfo);

			confirmation.ParentID_InstructionOrPackageDivot = divot1.PK;
			AssertNoErrors(confirmation.ParentID_InstructionOrPackageDivotInfo);

			confirmation.ParentID_InstructionOrPackageDivot = Factory.New<DtbBookingInstructionPkgDivot>().PK;
			AssertHasErrors(confirmation.ParentID_InstructionOrPackageDivotInfo);

			confirmation.ParentID_InstructionOrPackageDivot = divot2.PK;
			AssertNoErrors(confirmation.ParentID_InstructionOrPackageDivotInfo);

			confirmation.ParentID_InstructionOrPackageDivot = ZGuid.Empty;
			AssertHasErrors(confirmation.ParentID_InstructionOrPackageDivotInfo);

			confirmation.ParentID_InstructionOrPackageDivot = instruction.PK;
			AssertNoErrors(confirmation.ParentID_InstructionOrPackageDivotInfo);

			confirmation.ParentID_InstructionOrPackageDivot = ZGuid.Invalid;
			AssertHasErrors(confirmation.ParentID_InstructionOrPackageDivotInfo);

			confirmation.ParentID_InstructionOrPackageDivot = instruction.PK;
			AssertNoErrors(confirmation.ParentID_InstructionOrPackageDivotInfo);
		}

		public void TestValidateConfirmationDescription()
		{
			var dateAndReferences = DateAndReferenceCollection.GetDefault();
			var dateAndReferenceCFS = dateAndReferences.AddNew();
			dateAndReferenceCFS.Code = "CUS";
			dateAndReferenceCFS.Description = (NoResString)"Custom";
			dateAndReferenceCFS.OrganisationType = "CFS";
			dateAndReferenceCFS.AllowActualDate = true;

			TransportRegistry.Instance.DateAndReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateAndReferences);

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();

			instruction.OrganisationType = "CFS";
			confirmation.ConfirmationDescription = "Custom";
			AssertEquals("CUS", confirmation.KK_ConfirmationType);
			AssertNoErrors(confirmation.ConfirmationDescriptionInfo);

			var confirmation2 = instruction.Confirmations.AddNew();
			instruction.OrganisationType = "CTO";
			confirmation2.ConfirmationDescription = "Custom";
			AssertEquals("Custom is not valid for CTO", "", confirmation2.KK_ConfirmationType);
			AssertHasErrors(confirmation2.ConfirmationDescriptionInfo);
		}

		public void TestCheckKK_RequiredFrom()
		{
			var now = ZDateTime.Now;

			var confirmation = Factory.New<DtbBookingConfirmation>();
			confirmation.Validation.ValidateKK_RequiredFrom();
			AssertEquals(false, confirmation.KK_RequiredFromInfo.HasError("'Required From' needs to be earlier than 'Required To'."));

			confirmation.KK_RequiredFrom = now;
			AssertEquals(false, confirmation.KK_RequiredFromInfo.HasError("'Required From' needs to be earlier than 'Required To'."));

			confirmation.KK_RequiredTo = now;
			confirmation.Validation.ValidateKK_RequiredFrom();
			AssertEquals("Allowed to be the same.", false, confirmation.KK_RequiredFromInfo.HasError("'Required From' needs to be earlier than 'Required To'."));

			confirmation.KK_RequiredTo = now.AddDays(1);
			confirmation.Validation.ValidateKK_RequiredFrom();
			AssertEquals(false, confirmation.KK_RequiredFromInfo.HasError("'Required From' needs to be earlier than 'Required To'."));

			confirmation.KK_RequiredTo = now.AddDays(-1);
			confirmation.Validation.ValidateKK_RequiredFrom();
			AssertEquals(true, confirmation.KK_RequiredFromInfo.HasError("'Required From' needs to be earlier than 'Required To'."));

			confirmation.KK_RequiredFrom = ZDateTime.Empty;
			AssertEquals(false, confirmation.KK_RequiredFromInfo.HasError("'Required From' needs to be earlier than 'Required To'."));
		}

		public void TestCheckKK_RequiredFrom_WhenSendingXUSToCTO()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking);
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);

			AssertEquals("Precondition: KK_RequiredFrom is blank", ZDateTime.Empty, confirmation.KK_RequiredFrom);
			AssertEquals("Precondition: IsSendingXUSToCTO is false", false, booking.IsSendingXUSToCTO);
			AssertNoNotifications("Precondition: KK_RequiredFrom doesn't have any kind of notification as ValidateForSendingXUSToCTO has not been run", confirmation.KK_RequiredFromInfo);

			booking.KM_Status = TransportStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);
			confirmation.Validation.ValidateKK_RequiredFrom();

			AssertHasMessageError("Should have message error for KK_RequiredFrom", confirmation.KK_RequiredFromInfo, "This Booking must have Required From and Required To entered for all Confirmations.");

			confirmation.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;
			confirmation.Validation.ValidateKK_RequiredFrom();

			AssertHasMessageError("Should have message error for KK_RequiredFrom for delivery confirmation type", confirmation.KK_RequiredFromInfo, "This Booking must have Required From and Required To entered for all Confirmations.");

			confirmation.KK_RequiredFrom = new ZDateTime(2025, 2, 11);

			AssertNoNotifications("KK_RequiredFrom should not have any kind of notification as it is non-empty", confirmation.KK_RequiredFromInfo);

			confirmation.KK_ConfirmationType = ConfirmationTypes.Codes.ConNoteNo;
			confirmation.KK_RequiredFrom = ZDateTime.Empty;

			AssertNoNotifications("KK_RequiredFrom should not have any kind of notification as it is not a pickup or delivery type", confirmation.KK_RequiredFromInfo);
		}

		public void TestCheckKK_RequiredTo()
		{
			var now = ZDateTime.Now;

			var confirmation = Factory.New<DtbBookingConfirmation>();
			confirmation.Validation.ValidateKK_RequiredTo();
			AssertEquals(false, confirmation.KK_RequiredToInfo.HasError("'Required To' needs to be later than 'Required From'."));

			confirmation.KK_RequiredTo = now;
			AssertEquals(false, confirmation.KK_RequiredToInfo.HasError("'Required To' needs to be later than 'Required From'."));

			confirmation.KK_RequiredFrom = now;
			confirmation.Validation.ValidateKK_RequiredTo();
			AssertEquals("Allowed to be the same.", false, confirmation.KK_RequiredToInfo.HasError("'Required To' needs to be later than 'Required From'."));

			confirmation.KK_RequiredFrom = now.AddDays(-1);
			confirmation.Validation.ValidateKK_RequiredTo();
			AssertEquals(false, confirmation.KK_RequiredToInfo.HasError("'Required To' needs to be later than 'Required From'."));

			confirmation.KK_RequiredFrom = now.AddDays(1);
			confirmation.Validation.ValidateKK_RequiredTo();
			AssertEquals(true, confirmation.KK_RequiredToInfo.HasError("'Required To' needs to be later than 'Required From'."));

			confirmation.KK_RequiredTo = ZDateTime.Empty;
			AssertEquals(false, confirmation.KK_RequiredToInfo.HasError("'Required To' needs to be later than 'Required From'."));
		}

		public void TestCheckKK_RequiredTo_WhenSendingXUSToCTO()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking);
			var confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);

			AssertEquals("Precondition: KK_RequiredTo is blank", ZDateTime.Empty, confirmation.KK_RequiredTo);
			AssertEquals("Precondition: IsSendingXUSToCTO is false", false, booking.IsSendingXUSToCTO);
			AssertNoNotifications("Precondition: KK_RequiredTo doesn't have any kind of notification as ValidateForSendingXUSToCTO has not been run", confirmation.KK_RequiredToInfo);

			booking.KM_Status = TransportStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);
			confirmation.Validation.ValidateKK_RequiredTo();

			AssertHasMessageError("Should have message error for KK_RequiredTo", confirmation.KK_RequiredToInfo, "This Booking must have Required From and Required To entered for all Confirmations.");

			confirmation.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;
			confirmation.Validation.ValidateKK_RequiredTo();

			AssertHasMessageError("Should have message error for KK_RequiredTo for delivery confirmation type", confirmation.KK_RequiredToInfo, "This Booking must have Required From and Required To entered for all Confirmations.");

			confirmation.KK_RequiredTo = new ZDateTime(2025, 2, 11);

			AssertNoNotifications("KK_RequiredTo should not have any kind of notification as it is non-empty", confirmation.KK_RequiredToInfo);

			confirmation.KK_ConfirmationType = ConfirmationTypes.Codes.ConNoteNo;
			confirmation.KK_RequiredTo = ZDateTime.Empty;

			AssertNoNotifications("KK_RequiredTo should not have any kind of notification as it is not a pickup or delivery type", confirmation.KK_RequiredToInfo);
		}

		public void TestCheckKK_IsEmptyContainer()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();

			confirmation.Validation.ValidateKK_IsEmptyContainer();
			AssertEquals("Not marked as empty", false, confirmation.KK_IsEmptyContainerInfo.HasError("Only Containers can be Empty."));

			confirmation.KK_IsEmptyContainer = true;
			confirmation.Validation.ValidateKK_IsEmptyContainer();
			AssertEquals("Instruction does not have a packages, therefore should not validate for MT containers",
				false, confirmation.KK_IsEmptyContainerInfo.HasError("Only Containers can be Empty."));

			var packageJob = Helper.CreatePackageJob(consolidation);
			var packageDivot = Helper.CreatePackageDivot(instruction, 1);
			var package = Helper.CreatePackage("Package1", packageDivot, 1);
			confirmation.Validation.ValidateKK_IsEmptyContainer();
			AssertEquals("Instruction no container, only loose", true, confirmation.KK_IsEmptyContainerInfo.HasError("Only Containers can be Empty."));

			var containerDivot = Helper.CreatePackageDivot(instruction, 1);
			var container = Helper.CreatePackageContainer("CONT1", containerDivot);
			confirmation.Validation.ValidateKK_IsEmptyContainer();
			AssertEquals("Instruction w/ container, but also loose", true, confirmation.KK_IsEmptyContainerInfo.HasError("Only Containers can be Empty."));

			confirmation.ParentID_InstructionOrPackageDivot = containerDivot.PK;
			confirmation.Validation.ValidateKK_IsEmptyContainer();
			AssertEquals("Confirmation w/ only container", false, confirmation.KK_IsEmptyContainerInfo.HasError("Only Containers can be Empty."));
		}

		public void TestKK_Actual_MultipleConfirmationsOnPackage()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pallet = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);

			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = instruction1.PackageDivots.AddNew();
			divot1.KD_KP_Package = pallet.PK;

			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot2 = instruction2.PackageDivots.AddNew();
			divot2.KD_KP_Package = pallet.PK;

			const string dateIsEarlierWarning = "Date is earlier than a previous Confirmation for the same package.";
			const string dateIsLaterWarning = "Date is later than a following Confirmation for the same package.";

			var confirmation1a = Helper.CreateConfirmation(divot1, ConfirmationTypes.Codes.PickUp);
			var confirmation1b = Helper.CreateConfirmation(divot1, ConfirmationTypes.Codes.PickUp);
			var confirmation1c = Helper.CreateConfirmation(divot1, ConfirmationTypes.Codes.PickUp);
			AssertEquals("Precondition", 3, divot1.Confirmations.Count);

			var confirmation2a = Helper.CreateConfirmation(divot2, ConfirmationTypes.Codes.PickUp);

			var justNow = DateTime.Now;
			confirmation1a.KK_Actual = justNow.AddHours(-4);
			confirmation1b.KK_Actual = justNow.AddHours(-2);
			confirmation1c.KK_Actual = justNow;

			AssertEquals("Instruction.Actual should represent the most recent confirmation", confirmation1c.KK_Actual, instruction1.Actual);

			confirmation2a.KK_Actual = justNow;
			AssertNoWarnings("confirmation1a.KK_Actual", confirmation1a.KK_ActualInfo);
			AssertNoWarnings("confirmation1b.KK_Actual", confirmation1b.KK_ActualInfo);
			AssertNoWarnings("confirmation1c.KK_Actual", confirmation1c.KK_ActualInfo);
			AssertNoWarnings("confirmation2a.KK_Actual", confirmation2a.KK_ActualInfo);

			confirmation2a.KK_Actual = justNow.AddHours(-3);
			AssertHasWarning(confirmation2a.KK_ActualInfo, dateIsEarlierWarning);
			AssertHasWarning(confirmation1c.KK_ActualInfo, dateIsLaterWarning);
			AssertHasWarning(confirmation1b.KK_ActualInfo, dateIsLaterWarning);
			AssertNoWarning(confirmation1a.KK_ActualInfo, dateIsLaterWarning);

			confirmation2a.KK_Actual = justNow;
			AssertNoWarning(confirmation2a.KK_ActualInfo, dateIsEarlierWarning);
			AssertNoWarning(confirmation1a.KK_ActualInfo, dateIsLaterWarning);
			AssertNoWarning(confirmation1b.KK_ActualInfo, dateIsLaterWarning);
			AssertNoWarning(confirmation1c.KK_ActualInfo, dateIsLaterWarning);

			confirmation1a.KK_Actual = justNow.AddHours(6);
			AssertHasWarning(confirmation2a.KK_ActualInfo, dateIsEarlierWarning);
			AssertHasWarning(confirmation1a.KK_ActualInfo, dateIsLaterWarning);
			AssertNoWarnings("confirmation1b.KK_Actual", confirmation1b.KK_ActualInfo);
			AssertNoWarnings("confirmation1c.KK_Actual", confirmation1c.KK_ActualInfo);
		}

		public void TestKK_ActualPickupBeforeDelivery_SamePackage()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pallet = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickupDivot = pickupInstruction.PackageDivots.AddNew();
			pickupDivot.KD_KP_Package = pallet.PK;
			var pickupConfirmation = Helper.CreateConfirmation(pickupDivot, ConfirmationTypes.Codes.PickUp);

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryDivot = deliveryInstruction.PackageDivots.AddNew();
			deliveryDivot.KD_KP_Package = pallet.PK;
			var deliveryConfirmation = Helper.CreateConfirmation(deliveryDivot, ConfirmationTypes.Codes.Delivery);

			Assert("Instruction should be Pickup", pickupInstruction.IsPickUp);
			Assert("Instruction should be Delivery", deliveryInstruction.IsDelivery);
			Assert("Both instructions should have the same package", pickupInstruction.DivotsWithPackages.Packages.Single().PK == deliveryInstruction.DivotsWithPackages.Packages.Single().PK);
			AssertNoWarnings("Precondition", pickupConfirmation.KK_ActualInfo);
			AssertNoWarnings("Precondition", deliveryConfirmation.KK_ActualInfo);

			const string dateIsEarlierWarning = "Date is earlier than a previous Confirmation for the same package.";
			const string dateIsLaterWarning = "Date is later than a following Confirmation for the same package.";

			var justNow = DateTime.Now;
			pickupConfirmation.KK_Actual = justNow;
			deliveryConfirmation.KK_Actual = justNow;
			AssertNoWarnings("pickupConfirmation.KK_Actual", pickupConfirmation.KK_ActualInfo);
			AssertNoWarnings("deliveryConfirmation.KK_Actual", deliveryConfirmation.KK_ActualInfo);

			deliveryConfirmation.KK_Actual = justNow.AddHours(1);
			AssertNoWarnings("pickupConfirmation.KK_Actual", pickupConfirmation.KK_ActualInfo);
			AssertNoWarnings("deliveryConfirmation.KK_Actual", deliveryConfirmation.KK_ActualInfo);

			deliveryConfirmation.KK_Actual = justNow.AddHours(-1);
			AssertHasWarning(pickupConfirmation.KK_ActualInfo, dateIsLaterWarning);
			AssertHasWarning(deliveryConfirmation.KK_ActualInfo, dateIsEarlierWarning);
			AssertEquals("Delivery Instruction should have 1 warning", 1, deliveryConfirmation.KK_ActualInfo.GetWarnings().Count());

			deliveryConfirmation.KK_Actual = justNow.AddHours(-2);
			AssertEquals("Should not add the same warning twice", 1, deliveryConfirmation.KK_ActualInfo.GetWarnings().Count());

			deliveryConfirmation.KK_Actual = justNow;
			AssertNoWarning(deliveryConfirmation.KK_ActualInfo, dateIsEarlierWarning);
			AssertNoWarning(pickupConfirmation.KK_ActualInfo, dateIsLaterWarning);
		}

		public void TestKK_ActualPickupBeforeDelivery_SamePackageSplitDelivery()
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
			var pickupConfirmation = Helper.CreateConfirmation(pickupDivot, ConfirmationTypes.Codes.PickUp);

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryDivot = deliveryInstruction.PackageDivots.AddNew();
			deliveryDivot.KD_KP_Package = threeBoxes.PK;
			var deliveryConfirmation = Helper.CreateConfirmation(deliveryDivot, ConfirmationTypes.Codes.Delivery);

			Assert("Instruction should be Pickup", pickupInstruction.IsPickUp);
			Assert("Instruction should be Delivery", deliveryInstruction.IsDelivery);
			Assert("Instructions should have different packages", pickupInstruction.DivotsWithPackages.Packages.Single().PK != deliveryInstruction.DivotsWithPackages.Packages.Single().PK);

			var justNow = DateTime.Now;
			pickupConfirmation.KK_Actual = justNow;
			deliveryConfirmation.KK_Actual = justNow;
			AssertNoWarnings("pickupConfirmation.KK_Actual", pickupConfirmation.KK_ActualInfo);
			AssertNoWarnings("deliveryConfirmation.KK_Actual", deliveryConfirmation.KK_ActualInfo);

			deliveryConfirmation.KK_Actual = justNow.AddHours(1);
			AssertNoWarnings(pickupConfirmation.KK_ActualInfo);
			AssertNoWarnings(deliveryConfirmation.KK_ActualInfo);

			deliveryConfirmation.KK_Actual = justNow.AddHours(-1);
			AssertNoWarnings("Is not expected to detect the pickup being after the delivery", pickupConfirmation.KK_ActualInfo);
			AssertNoWarnings("Is not expected to detect the delivery being before the pickup", deliveryConfirmation.KK_ActualInfo);
		}

		public void TestKK_ActualPickupBeforeDelivery_DifferentPackage()
		{
			var pallet1 = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);
			var pallet2 = Helper.CreatePackage("PLT-2", 1, Constants.PkgUnit.Pallet);

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();

			AssertPickupIsAfterDelivery(booking, pallet1, pallet2, (c) => (ZPropertyInfo<ZDateTime>)c.KK_ActualInfo);
		}

		public void TestKK_ActualPickupBeforeDelivery_SplitDeliveryOffPallet()
		{
			var pallet1 = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);
			var threeBoxes = Helper.CreatePackage("", 3, Constants.PkgUnit.Box);
			var sevenBoxes = Helper.CreatePackage("", 7, Constants.PkgUnit.Box);

			threeBoxes.KP_KP_ParentPackage = pallet1.PK;
			sevenBoxes.KP_KP_ParentPackage = pallet1.PK;

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();

			AssertPickupIsAfterDelivery(booking, threeBoxes, sevenBoxes, (c) => (ZPropertyInfo<ZDateTime>)c.KK_ActualInfo);
		}

		public void TestKK_Estimated_MultipleConfirmationsOnPackage()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pallet = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);

			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = instruction1.PackageDivots.AddNew();
			divot1.KD_KP_Package = pallet.PK;

			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot2 = instruction2.PackageDivots.AddNew();
			divot2.KD_KP_Package = pallet.PK;

			const string dateIsEarlierWarning = "Date is earlier than a previous Confirmation for the same package.";
			const string dateIsLaterWarning = "Date is later than a following Confirmation for the same package.";

			var confirmation1a = Helper.CreateConfirmation(divot1, ConfirmationTypes.Codes.PickUp);
			var confirmation1b = Helper.CreateConfirmation(divot1, ConfirmationTypes.Codes.PickUp);
			var confirmation1c = Helper.CreateConfirmation(divot1, ConfirmationTypes.Codes.PickUp);
			AssertEquals("Precondition", 3, divot1.Confirmations.Count);

			var confirmation2a = Helper.CreateConfirmation(divot2, ConfirmationTypes.Codes.PickUp);

			var justNow = DateTime.Now;
			confirmation1a.KK_Estimated = justNow.AddHours(-4);
			confirmation1b.KK_Estimated = justNow.AddHours(-2);
			confirmation1c.KK_Estimated = justNow;

			AssertEquals("Instruction.Estimated should represent the most recent confirmation", confirmation1c.KK_Estimated, instruction1.Estimated);

			confirmation2a.KK_Estimated = justNow;
			AssertNoWarnings("confirmation1a.KK_Estimated", confirmation1a.KK_EstimatedInfo);
			AssertNoWarnings("confirmation1b.KK_Estimated", confirmation1b.KK_EstimatedInfo);
			AssertNoWarnings("confirmation1c.KK_Estimated", confirmation1c.KK_EstimatedInfo);
			AssertNoWarnings("confirmation2a.KK_Estimated", confirmation2a.KK_EstimatedInfo);

			confirmation2a.KK_Estimated = justNow.AddHours(-3);
			AssertHasWarning(confirmation2a.KK_EstimatedInfo, dateIsEarlierWarning);
			AssertHasWarning(confirmation1c.KK_EstimatedInfo, dateIsLaterWarning);
			AssertHasWarning(confirmation1b.KK_EstimatedInfo, dateIsLaterWarning);
			AssertNoWarning(confirmation1a.KK_EstimatedInfo, dateIsLaterWarning);

			confirmation2a.KK_Estimated = justNow;
			AssertNoWarning(confirmation2a.KK_EstimatedInfo, dateIsEarlierWarning);
			AssertNoWarning(confirmation1a.KK_EstimatedInfo, dateIsLaterWarning);
			AssertNoWarning(confirmation1b.KK_EstimatedInfo, dateIsLaterWarning);
			AssertNoWarning(confirmation1c.KK_EstimatedInfo, dateIsLaterWarning);

			confirmation1a.KK_Estimated = justNow.AddHours(6);
			AssertHasWarning(confirmation2a.KK_EstimatedInfo, dateIsEarlierWarning);
			AssertHasWarning(confirmation1a.KK_EstimatedInfo, dateIsLaterWarning);
			AssertNoWarnings("confirmation1b.KK_Estimated", confirmation1b.KK_EstimatedInfo);
			AssertNoWarnings("confirmation1c.KK_Estimated", confirmation1c.KK_EstimatedInfo);
		}

		public void TestKK_EstimatedPickupBeforeDelivery_SamePackage()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var pallet = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);

			var pickupInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickupDivot = pickupInstruction.PackageDivots.AddNew();
			pickupDivot.KD_KP_Package = pallet.PK;
			var pickupConfirmation = Helper.CreateConfirmation(pickupDivot, ConfirmationTypes.Codes.PickUp);

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryDivot = deliveryInstruction.PackageDivots.AddNew();
			deliveryDivot.KD_KP_Package = pallet.PK;
			var deliveryConfirmation = Helper.CreateConfirmation(deliveryDivot, ConfirmationTypes.Codes.Delivery);

			Assert("Instruction should be Pickup", pickupInstruction.IsPickUp);
			Assert("Instruction should be Delivery", deliveryInstruction.IsDelivery);
			Assert("Both instructions should have the same package", pickupInstruction.DivotsWithPackages.Packages.Single().PK == deliveryInstruction.DivotsWithPackages.Packages.Single().PK);

			const string dateIsEarlierWarning = "Date is earlier than a previous Confirmation for the same package.";
			const string dateIsLaterWarning = "Date is later than a following Confirmation for the same package.";

			var justNow = DateTime.Now;
			pickupConfirmation.KK_Estimated = justNow;
			deliveryConfirmation.KK_Estimated = justNow;
			AssertNoWarnings("pickupConfirmation.KK_Estimated", pickupConfirmation.KK_EstimatedInfo);
			AssertNoWarnings("deliveryConfirmation.KK_Estimated", deliveryConfirmation.KK_EstimatedInfo);

			deliveryConfirmation.KK_Estimated = justNow.AddHours(1);
			AssertNoWarnings("pickupConfirmation.KK_Estimated", pickupConfirmation.KK_EstimatedInfo);
			AssertNoWarnings("deliveryConfirmation.KK_Estimated", deliveryConfirmation.KK_EstimatedInfo);

			deliveryConfirmation.KK_Estimated = justNow.AddHours(-1);
			AssertHasWarning(pickupConfirmation.KK_EstimatedInfo, dateIsLaterWarning);
			AssertHasWarning(deliveryConfirmation.KK_EstimatedInfo, dateIsEarlierWarning);
			AssertEquals("Delivery Instruction should have 1 warning", 1, deliveryConfirmation.KK_EstimatedInfo.GetWarnings().Count());

			deliveryConfirmation.KK_Estimated = justNow.AddHours(-2);
			AssertEquals("Should not add the same warning twice", 1, deliveryConfirmation.KK_EstimatedInfo.GetWarnings().Count());

			deliveryConfirmation.KK_Estimated = justNow;
			AssertNoWarning(deliveryConfirmation.KK_EstimatedInfo, dateIsEarlierWarning);
			AssertNoWarning(pickupConfirmation.KK_EstimatedInfo, dateIsLaterWarning);
		}

		public void TestKK_EstimatedPickupBeforeDelivery_SamePackageSplitDelivery()
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
			var pickupConfirmation = Helper.CreateConfirmation(pickupDivot, ConfirmationTypes.Codes.PickUp);

			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryDivot = deliveryInstruction.PackageDivots.AddNew();
			deliveryDivot.KD_KP_Package = threeBoxes.PK;
			var deliveryConfirmation = Helper.CreateConfirmation(deliveryDivot, ConfirmationTypes.Codes.Delivery);

			Assert("Instruction should be Pickup", pickupInstruction.IsPickUp);
			Assert("Instruction should be Delivery", deliveryInstruction.IsDelivery);
			Assert("Instructions should have different packages", pickupInstruction.DivotsWithPackages.Packages.Single().PK != deliveryInstruction.DivotsWithPackages.Packages.Single().PK);

			var justNow = DateTime.Now;
			pickupConfirmation.KK_Estimated = justNow;
			deliveryConfirmation.KK_Estimated = justNow;
			AssertNoWarnings("pickupConfirmation.KK_Estimated", pickupConfirmation.KK_EstimatedInfo);
			AssertNoWarnings("deliveryConfirmation.KK_Estimated", deliveryConfirmation.KK_EstimatedInfo);

			deliveryConfirmation.KK_Estimated = justNow.AddHours(1);
			AssertNoWarnings(pickupConfirmation.KK_EstimatedInfo);
			AssertNoWarnings(deliveryConfirmation.KK_EstimatedInfo);

			deliveryConfirmation.KK_Estimated = justNow.AddHours(-1);
			AssertNoWarnings("Is not expected to detect the pickup being after the delivery", pickupConfirmation.KK_EstimatedInfo);
			AssertNoWarnings("Is not expected to detect the delivery being before the pickup", deliveryConfirmation.KK_EstimatedInfo);
		}

		public void TestKK_EstimatedPickupBeforeDelivery_DifferentPackage()
		{
			var pallet1 = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);
			var pallet2 = Helper.CreatePackage("PLT-2", 1, Constants.PkgUnit.Pallet);

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();

			AssertPickupIsAfterDelivery(booking, pallet1, pallet2, (c) => (ZPropertyInfo<ZDateTime>)c.KK_EstimatedInfo);
		}

		public void TestKK_EstimatedPickupBeforeDelivery_SplitDeliveryOffPallet()
		{
			var pallet1 = Helper.CreatePackage("PLT-1", 1, Constants.PkgUnit.Pallet);
			var threeBoxes = Helper.CreatePackage("", 3, Constants.PkgUnit.Box);
			var sevenBoxes = Helper.CreatePackage("", 7, Constants.PkgUnit.Box);

			threeBoxes.KP_KP_ParentPackage = pallet1.PK;
			sevenBoxes.KP_KP_ParentPackage = pallet1.PK;

			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();

			AssertPickupIsAfterDelivery(booking, threeBoxes, sevenBoxes, (c) => (ZPropertyInfo<ZDateTime>)c.KK_EstimatedInfo);
		}

		void AssertPickupIsAfterDelivery(DtbBooking booking, PkgPackage pkgA, PkgPackage pkgB, Func<DtbBookingConfirmation, ZPropertyInfo<ZDateTime>> getDateInfoFunc)
		{
			var pickupInstructionA = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickupDivotA = pickupInstructionA.PackageDivots.AddNew();
			pickupDivotA.KD_KP_Package = pkgA.PK;
			var pickupConfirmationA = Helper.CreateConfirmation(pickupDivotA, ConfirmationTypes.Codes.PickUp);

			var pickupInstructionB = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickupDivotB = pickupInstructionB.PackageDivots.AddNew();
			pickupDivotB.KD_KP_Package = pkgB.PK;
			var pickupConfirmationB = Helper.CreateConfirmation(pickupDivotB, ConfirmationTypes.Codes.PickUp);

			var deliveryInstructionA = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryDivotA = deliveryInstructionA.PackageDivots.AddNew();
			deliveryDivotA.KD_KP_Package = pkgA.PK;
			var deliveryConfirmationA = Helper.CreateConfirmation(deliveryDivotA, ConfirmationTypes.Codes.Delivery);

			var deliveryInstructionB = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryDivotB = deliveryInstructionB.PackageDivots.AddNew();
			deliveryDivotB.KD_KP_Package = pkgB.PK;
			var deliveryConfirmationB = Helper.CreateConfirmation(deliveryDivotB, ConfirmationTypes.Codes.Delivery);

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

			const string dateIsEarlierWarning = "Date is earlier than a previous Confirmation for the same package.";
			const string dateIsLaterWarning = "Date is later than a following Confirmation for the same package.";

			var pickupConfirmationA_DateInfo = getDateInfoFunc(pickupConfirmationA);
			var deliveryConfirmationA_DateInfo = getDateInfoFunc(deliveryConfirmationA);
			var pickupConfirmationB_DateInfo = getDateInfoFunc(pickupConfirmationB);
			var deliveryConfirmationB_DateInfo = getDateInfoFunc(deliveryConfirmationB);

			var justNow = DateTime.Now;

			pickupConfirmationA_DateInfo.Value = justNow;
			deliveryConfirmationA_DateInfo.Value = justNow;
			pickupConfirmationB_DateInfo.Value = justNow;
			deliveryConfirmationB_DateInfo.Value = justNow;

			AssertNoWarnings(pickupConfirmationA_DateInfo);
			AssertNoWarnings(deliveryConfirmationA_DateInfo);
			AssertNoWarnings(pickupConfirmationB_DateInfo);
			AssertNoWarnings(deliveryConfirmationB_DateInfo);

			deliveryConfirmationA_DateInfo.Value = justNow.AddHours(2);
			deliveryConfirmationB_DateInfo.Value = justNow.AddHours(1);
			AssertNoWarnings(pickupConfirmationA_DateInfo);
			AssertNoWarnings(deliveryConfirmationA_DateInfo);
			AssertNoWarnings(pickupConfirmationB_DateInfo);
			AssertNoWarnings(deliveryConfirmationB_DateInfo);

			deliveryConfirmationA_DateInfo.Value = justNow.AddHours(-1);
			AssertHasWarning(pickupConfirmationA_DateInfo, dateIsLaterWarning);
			AssertHasWarning(deliveryConfirmationA_DateInfo, dateIsEarlierWarning);
			AssertNoWarning(pickupConfirmationB_DateInfo, dateIsLaterWarning);
			AssertNoWarning(deliveryConfirmationB_DateInfo, dateIsEarlierWarning);

			deliveryConfirmationA_DateInfo.Value = justNow;
			deliveryConfirmationB_DateInfo.Value = justNow.AddHours(-2);
			AssertNoWarning(pickupConfirmationA_DateInfo, dateIsLaterWarning);
			AssertNoWarning(deliveryConfirmationA_DateInfo, dateIsEarlierWarning);
			AssertHasWarning(pickupConfirmationB_DateInfo, dateIsLaterWarning);
			AssertHasWarning(deliveryConfirmationB_DateInfo, dateIsEarlierWarning);
		}

		public void TestKK_ReferenceNum_CheckIfEmpty()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var confirmation = Helper.CreateConfirmation(instruction1, ConfirmationTypes.Codes.PickUp);
			booking.KM_Status = BookingStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: KK_ReferenceNum is empty", string.Empty, confirmation.KK_ReferenceNum);
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);

			confirmation.Validation.ValidateKK_ReferenceNum();
			AssertHasMessageError("Should have message error about empty KK_ReferenceNum", confirmation.KK_ReferenceNumInfo, "Confirmation must have Reference Number");
		}

		public void TestKK_ReferenceNum_JobDirectionDLV_OrganisationTypeCTO_CheckIfEmpty()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO, null);
			var confirmation = Helper.CreateConfirmation(instruction1, ConfirmationTypes.Codes.PickUp);
			booking.KM_Status = BookingStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: KK_ReferenceNum is empty", string.Empty, confirmation.KK_ReferenceNum);
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);

			confirmation.Validation.ValidateKK_ReferenceNum();
			AssertHasMessageError("Should have message error about empty KK_ReferenceNum, when Job Direction is Delivery and organisation type is CTO", confirmation.KK_ReferenceNumInfo, "Confirmation must have Reference Number");
		}

		public void TestKK_ReferenceNum_CheckNonFirstInstructionConfirmation()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var confirmation = Helper.CreateConfirmation(instruction2, ConfirmationTypes.Codes.PickUp);
			booking.KM_Status = BookingStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: instruction2 is not the first instruction for the booking", 2, instruction2.KN_Sequence);
			AssertEquals("Precondition: KK_ReferenceNum is empty", string.Empty, confirmation.KK_ReferenceNum);
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);

			confirmation.Validation.ValidateKK_ReferenceNum();
			AssertHasMessageError("Should have message error about empty KK_ReferenceNum on a non first instruction if it's a pickup from a container yard", confirmation.KK_ReferenceNumInfo, "Confirmation must have Reference Number");
		}

		public void TestKK_ReferenceNum_AllowNonEmpty()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var confirmation = Helper.CreateConfirmation(instruction1, ConfirmationTypes.Codes.PickUp);
			booking.KM_Status = BookingStatuses.Codes.ActionRequired;
			confirmation.KK_ReferenceNum = "Not empty";
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);

			confirmation.Validation.ValidateKK_ReferenceNum();
			AssertNoNotifications("Should not have notifications if KK_ReferenceNum is not empty", confirmation.KK_ReferenceNumInfo);
		}

		public void TestKK_ReferenceNum_KB_JobDirectionNotPic_IgnoreValidation()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var confirmation = Helper.CreateConfirmation(instruction1, ConfirmationTypes.Codes.PickUp);
			booking.KM_Status = BookingStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: KK_ReferenceNum is empty", string.Empty, confirmation.KK_ReferenceNum);
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);

			confirmation.Validation.ValidateKK_ReferenceNum();
			AssertNoNotifications("Should not have notifications if KB_JobDirection is not PIC", confirmation.KK_ReferenceNumInfo);
		}

		public void TestKK_ReferenceNum_KN_InstructionTypeNotPic_IgnoreValidation()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CYD, null);
			var confirmation = Helper.CreateConfirmation(instruction1, ConfirmationTypes.Codes.PickUp);
			booking.KM_Status = BookingStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: KK_ReferenceNum is empty", string.Empty, confirmation.KK_ReferenceNum);
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);

			confirmation.Validation.ValidateKK_ReferenceNum();
			AssertNoNotifications("Should not have notifications if instruction type is not pick up", confirmation.KK_ReferenceNumInfo);
		}

		public void TestKK_ReferenceNum_JobDirectionPic_OrganisationTypeNotCYD_IgnoreValidation()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO, null);
			var confirmation = Helper.CreateConfirmation(instruction1, ConfirmationTypes.Codes.PickUp);
			booking.KM_Status = BookingStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: KK_ReferenceNum is empty", string.Empty, confirmation.KK_ReferenceNum);
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);

			confirmation.Validation.ValidateKK_ReferenceNum();
			AssertNoNotifications("Should not have notifications if KB_JobDirection is PIC, and organisation type is not CYD", confirmation.KK_ReferenceNumInfo);
		}

		public void TestKK_ReferenceNum_JobDirectionDlv_OrganisationTypeNotCTO_IgnoreValidation()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var confirmation = Helper.CreateConfirmation(instruction1, ConfirmationTypes.Codes.PickUp);
			booking.KM_Status = BookingStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: KK_ReferenceNum is empty", string.Empty, confirmation.KK_ReferenceNum);
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);

			confirmation.Validation.ValidateKK_ReferenceNum();
			AssertNoNotifications("Should not have notifications if KB_JobDirection is DLV, and organisation type is not CTO", confirmation.KK_ReferenceNumInfo);
		}

		public void TestKK_ReferenceNum_ConfirmationTypeNotPic_IgnoreValidation()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var confirmation = Helper.CreateConfirmation(instruction1, ConfirmationTypes.Codes.Delivery);
			booking.KM_Status = BookingStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: KK_ReferenceNum is empty", string.Empty, confirmation.KK_ReferenceNum);
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);

			confirmation.Validation.ValidateKK_ReferenceNum();
			AssertNoNotifications("Should not have notifications if confirmation type not PIC", confirmation.KK_ReferenceNumInfo);
		}

		public void TestKK_ReferenceNum_CYD_PickUp_InstructionNotFirst_IgnoreValidation()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.WHS, null);
			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var confirmation = Helper.CreateConfirmation(instruction2, ConfirmationTypes.Codes.PickUp);
			booking.KM_Status = BookingStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: First Instruction is not pick up from container yard", 1, instruction1.KN_Sequence);
			AssertEquals("Precondition: Pick up from container yard is second instruction", 2, instruction2.KN_Sequence);
			AssertEquals("Precondition: KK_ReferenceNum is empty", string.Empty, confirmation.KK_ReferenceNum);
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);

			confirmation.Validation.ValidateKK_ReferenceNum();
			AssertNoNotifications("Should not have notifications if the instruction with PickUp and CYD is not the first instruction", confirmation.KK_ReferenceNumInfo);
		}

		public void TestKK_ReferenceNum_Not_CYD_PickUp_Instruction_IgnoreValidation()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.WHS, null);
			var confirmation = Helper.CreateConfirmation(instruction2, ConfirmationTypes.Codes.PickUp);
			booking.KM_Status = BookingStatuses.Codes.ActionRequired;
			AssertEquals("Precondition: Pick up from container yard is first instruction", 1, instruction1.KN_Sequence);
			AssertEquals("Precondition: Instruction associated with confirmation is second instruction", 2, instruction2.KN_Sequence);
			AssertEquals("Precondition: KK_ReferenceNum is empty", string.Empty, confirmation.KK_ReferenceNum);
			AssertEquals("Precondition: IsSendingXUSToCTO is true", true, booking.IsSendingXUSToCTO);

			confirmation.Validation.ValidateKK_ReferenceNum();
			AssertNoNotifications("Should not have notifications even if the first instruction is pick up from container yard if the confirmation's instruction is not a pick up from container yard", confirmation.KK_ReferenceNumInfo);
		}

		public void TestKK_ReferenceNum_IsSendingXUSToCTOFalse_IgnoreValidation()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.PIC);
			var booking = consolidation.Bookings.AddNew();
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CYD, null);
			var confirmation = Helper.CreateConfirmation(instruction1, ConfirmationTypes.Codes.PickUp);
			AssertEquals("Precondition: KK_ReferenceNum is empty", string.Empty, confirmation.KK_ReferenceNum);
			AssertEquals("Precondition: IsSendingXUSToCTO is false", false, booking.IsSendingXUSToCTO);

			confirmation.Validation.ValidateKK_ReferenceNum();
			AssertNoNotifications("Should not have notifications when not sending XUS to CTO", confirmation.KK_ReferenceNumInfo);
		}

		public void TestKK_ReferenceNum_DoesNotThrowNullReference()
		{
			var confirmation = Factory.New<DtbBookingConfirmation>();
			AssertEquals("Precondition: KK_ReferenceNum is empty", string.Empty, confirmation.KK_ReferenceNum);
			AssertNoExceptionThrown("Should not throw for confirmation without booking", () =>
			{
				confirmation.Validation.ValidateKK_ReferenceNum();
			});
			AssertNoNotifications("Should not have notifications if there is no booking", confirmation.KK_ReferenceNumInfo);

			var booking = Factory.New<DtbBooking>();
			var instruction = Helper.CreateInstruction(booking);
			confirmation = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			AssertNoExceptionThrown("Should not throw for confirmation without booking consolidation", () =>
			{
				confirmation.Validation.ValidateKK_ReferenceNum();
			});
			AssertNoNotifications("Should not have notifications if there is no consolidation", confirmation.KK_ReferenceNumInfo);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
