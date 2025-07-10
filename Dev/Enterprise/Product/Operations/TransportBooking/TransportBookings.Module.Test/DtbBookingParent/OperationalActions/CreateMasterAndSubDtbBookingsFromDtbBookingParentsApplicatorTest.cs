using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(CreateMasterAndSubDtbBookingsFromDtbBookingParentsApplicator))]
	sealed class CreateMasterAndSubDtbBookingsFromDtbBookingParentsApplicatorTest : BaseCreateDtbBookingsFromDtbBookingParentsApplicatorTest
	{
		public void TestCreateMasterAndSubDtbBookingsFromDtbBookingParents()
		{
			var parents = CreateForwardingShipmentsToCreateDtbBookingsFrom();

			Applicator.Direction = "PIC";
			Applicator.BookingTemplate = "EFPL";
			var helper = new TransportBookingTestHelper(parents[0].Factory);
			helper.AssertEFPLTemplateHasFourInstructionsWithSpecificPackageCategories(isPreConditionAssert: true);

			var log = SimulateRun(parents, true);

			var jobIDs = new List<string>();

			CombineAssertions("Check that CreateTransportBookings() with default template and settings from operational action will result in correct bookings and log", () =>
			{
				foreach (IDtbBookingParent shipment in parents)
				{
					var jobID = AssertBookingWasCreatedForShipmentAndHasCorrectDetailsAndReturnBookingJobID(shipment);
					jobIDs.Add(jobID);
				}
				AssertEquals("Should have been four Transport Bookings created, one for each Shipment", 4, jobIDs.Count);

				Assert("Should have been no Global messages", !UnitTestUserNotification.Instance.PreviousMessages.Any(m => !m.Text.IsNullOrEmpty()));

				var lastController = (ZController)ControllerFactory.LastController;
				AssertNotNull("Should create a controller which will open a booking form", lastController);
				var transportBookingForm = (TransportBookingForm)lastController.LastShownForm;
				AssertNotNull("Should open a booking form", transportBookingForm);
				var formBooking = (DtbBooking)transportBookingForm.BusinessEntity;
				AssertEquals("Booking in booking form should be master booking", true, formBooking.KM_IsMaster);
				AssertEquals("Master booking should have four sub bookings", 4, formBooking.SubBookings.Count);
				AssertEquals("Should show list of sub bookings", TransportBookingInstructionView.TransportBookings, transportBookingForm.InstructionViewsControl.View);
				AssertMasterBookingHasCorrectInstructionsAndPackageDivots(formBooking);

				jobIDs.Add(formBooking.KM_JobID);
				var jobIDsArray = jobIDs.ToArray();
				var expectedLog = FormattableString.Invariant(
@$"INFO: Checking Shipment S1
INFO: Checking Shipment S2
INFO: Checking Shipment S3
INFO: Checking Shipment S4
INFO: Transport Booking {jobIDsArray[0]} created for Shipment S1
INFO: Transport Booking {jobIDsArray[1]} created for Shipment S2
INFO: Transport Booking {jobIDsArray[2]} created for Shipment S3
INFO: Transport Booking {jobIDsArray[3]} created for Shipment S4
INFO: Master Transport Booking {jobIDsArray[4]} created with 4 sub Transport Bookings
").SplitByLine();
				var actualLog = log.MessagesString().SplitByLine();
				AssertContainsExactElementsInExactOrder(
					"Operational Action Section Log is correct",
					expectedLog,
					actualLog);
			});
		}

		public void TestNoBookingsCreated_NoMasterCreated()
		{
			var parents = Array.Empty<BusinessObject>();

			Applicator.Direction = "PIC";
			Applicator.BookingTemplate = "EFPR";

			var log = SimulateRun(parents, true);

			var query = new ZDBOnlyQuery(typeof(DtbBooking));
			var bookings = Factory.Load<DtbBooking>(query);
			AssertEquals("Should not create a master booking", 0, bookings.Length);

			AssertEquals(
				"Operational Action Section Log is correct",
				"WARNING: Master Transport Booking could not be created since no Transport Bookings could be attached to it",
				log.MessagesString()
			);

			var lastController = (ZController)ControllerFactory.LastController;
			AssertNull("Should not create a controller for opening a booking form", lastController);
		}

		public void TestSomeBookingsInvalidForAttaching_NoMasterCreated()
		{
			var helper = new TransportBookingTestHelper(Factory);

			// Shipment1 will return an invalid booking, as it has a status of held
			var shipment1 = helper.CreateForwardingShipment();
			shipment1.JS_UniqueConsignRef = "S1";
			var consolidation1 = helper.CreateConsolidation((IDtbBookingParent)shipment1);
			consolidation1.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			consolidation1.KB_JobID = "CM1";
			var booking1 = helper.CreateBooking(consolidation1);
			booking1.KM_KT_NKBookingTemplate = "EFPR";
			booking1.KM_Status = TransportStatuses.Codes.Held;
			consolidation1.KB_IsOverridden = true;
			Factory.Save();
			consolidation1.KB_JobID = "CM1";
			Factory.Save();

			// Shipment2 will return a valid booking
			var shipment2 = helper.CreateForwardingShipment();
			shipment2.JS_UniqueConsignRef = "S2";
			var consolidation2 = helper.CreateConsolidation((IDtbBookingParent)shipment2);
			consolidation2.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			consolidation2.KB_JobID = "CM2";
			var booking2 = helper.CreateBooking(consolidation2);
			booking2.KM_KT_NKBookingTemplate = "EFPR";
			consolidation2.KB_IsOverridden = true;
			Factory.Save();
			consolidation2.KB_JobID = "CM2";
			Factory.Save();

			var parents = new BusinessObject[] { (BusinessObject)shipment1, (BusinessObject)shipment2 };

			Applicator.Direction = nameof(DtbBookingDirection.DLV);
			Applicator.BookingTemplate = "EFPR";

			var log = SimulateRun(parents, true);

			var query = new ZDBOnlyQuery(typeof(DtbBooking));
			var bookings = Factory.Load<DtbBooking>(query);
			AssertEquals("Should not create a master booking", 2, bookings.Length);
			AssertEquals("First booking should not have master", ZGuid.Empty, bookings.First().KM_KM_MasterBooking);
			AssertEquals("Second booking should not have master", ZGuid.Empty, bookings.Last().KM_KM_MasterBooking);

			var expectedLog = @$"INFO: Checking Shipment S1
WARNING: Booking Consolidation CM1 for Shipment S1 is overridden and cannot be overwritten
WARNING: Bookings TB00000001 under Booking Consolidation CM1 for Shipment S1 have not been changed
INFO: Checking Shipment S2
WARNING: Booking Consolidation CM2 for Shipment S2 is overridden and cannot be overwritten
WARNING: Bookings TB00000002 under Booking Consolidation CM2 for Shipment S2 have not been changed
WARNING: Booking TB00000001 not added to Master Transport Booking because it is not valid. Error - KM_Status: Cannot attach Booking which does not have a status of Available.
WARNING: Master Transport Booking could not be created since 1 sub bookings were inconsistent with the first sub booking
".SplitByLine();
			var actualLog = log.MessagesString().SplitByLine();
			AssertContainsExactElementsInExactOrder(
				"Operational Action Section Log is correct",
				expectedLog,
				actualLog);

			var lastController = (ZController)ControllerFactory.LastController;
			AssertNull("Should not create a controller for opening a booking form", lastController);
		}

		public void TestBookingsInvalidForSubsAreNotMadeSubs()
		{
			// Don't need separate factory as containers are not involved
			var helper = new TransportBookingTestHelper(Factory);
			var shipment = helper.CreateForwardingShipment();
			shipment.JS_UniqueConsignRef = "S1";
			var consolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
			consolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			consolidation.KB_JobID = "CM1";
			var booking = helper.CreateBooking(consolidation);
			booking.KM_KT_NKBookingTemplate = "EFPR";
			booking.KM_Status = TransportStatuses.Codes.Held;
			consolidation.KB_IsOverridden = true;
			Factory.Save();
			consolidation.KB_JobID = "CM1";
			Factory.Save();

			var parents = new BusinessObject[] { (BusinessObject)shipment };

			Applicator.Direction = nameof(DtbBookingDirection.DLV);
			Applicator.BookingTemplate = "EFPR";

			var log = SimulateRun(parents, true);

			var query = new ZDBOnlyQuery(typeof(DtbBooking));
			var bookings = Factory.Load<DtbBooking>(query);
			AssertEquals("Should not create a master booking", 1, bookings.Length);
			AssertEquals("Booking should not have a master", ZGuid.Empty, bookings.First().KM_KM_MasterBooking);

			var expectedLog = @$"INFO: Checking Shipment S1
WARNING: Booking Consolidation CM1 for Shipment S1 is overridden and cannot be overwritten
WARNING: Bookings TB00000001 under Booking Consolidation CM1 for Shipment S1 have not been changed
WARNING: Booking TB00000001 not added to Master Transport Booking because it is not valid. Error - KM_Status: Cannot attach Booking which does not have a status of Available.
WARNING: Master Transport Booking could not be created since 1 sub bookings were inconsistent with the first sub booking
".SplitByLine();
			var actualLog = log.MessagesString().SplitByLine();
			AssertContainsExactElementsInExactOrder(
				"Operational Action Section Log is correct",
				expectedLog,
				actualLog);
		}

		public void TestAlreadySubBookingsDoNotBecomeNonSubs()
		{
			var helper = new TransportBookingTestHelper(Factory);

			var existingMasterBooking = helper.CreateBooking();
			existingMasterBooking.KM_IsMaster = true;

			// shipment whose booking is sub to existingMasterBooking
			var shipment1 = helper.CreateForwardingShipment();
			shipment1.JS_UniqueConsignRef = "S1";
			var consolidation1 = helper.CreateConsolidation((IDtbBookingParent)shipment1);
			consolidation1.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			consolidation1.KB_JobID = "CM1";
			var booking1 = helper.CreateBooking(consolidation1);
			booking1.KM_KT_NKBookingTemplate = "EFPR";
			consolidation1.KB_IsOverridden = true;
			Factory.Save();
			consolidation1.KB_JobID = "CM1";
			Factory.Save();

			booking1.KM_KM_MasterBooking = existingMasterBooking.PK;
			Factory.Save();

			// shipment who can produce a normal sub booking
			var shipment2 = helper.CreateForwardingShipment();
			shipment2.JS_UniqueConsignRef = "S2";
			var consolidation2 = helper.CreateConsolidation((IDtbBookingParent)shipment2);
			consolidation2.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			consolidation2.KB_JobID = "CM2";
			var booking2 = helper.CreateBooking(consolidation2);
			booking2.KM_KT_NKBookingTemplate = "EFPR";
			consolidation2.KB_IsOverridden = true;
			Factory.Save();
			consolidation2.KB_JobID = "CM2";
			Factory.Save();

			var parents = new BusinessObject[] { (BusinessObject)shipment1, (BusinessObject)shipment2 };

			Applicator.Direction = nameof(DtbBookingDirection.DLV);
			Applicator.BookingTemplate = "EFPR";

			SimulateRun(parents, true);

			AssertEquals("Precondition: booking2 should have no master as no master was created", ZGuid.Empty, booking2.KM_KM_MasterBooking);
			AssertEquals("booking1 should still be sub to existingMasterBooking", existingMasterBooking.PK, booking1.KM_KM_MasterBooking);
		}

		public void TestConsolidationPropertiesPopulated()
		{
			var helper = new TransportBookingTestHelper(Factory);

			var shipment = helper.CreateForwardingShipment();
			var subConsolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
			subConsolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var subBooking = helper.CreateBooking(subConsolidation);
			subBooking.KM_KT_NKBookingTemplate = "EFPR";
			subConsolidation.KB_IsOverridden = true;
			Factory.Save();

			var parents = new BusinessObject[] { (BusinessObject)shipment };

			Applicator.Direction = nameof(DtbBookingDirection.DLV);
			Applicator.BookingTemplate = "EFPR";

			SimulateRun(parents, true);

			var masterBooking = (DtbBooking)((TransportBookingForm)((ZController)ControllerFactory.LastController).LastShownForm).BusinessEntity;
			var masterConsolidation = masterBooking.ConsolidationSingleJob;

			AssertEquals("Precondition: subConsolidation is sub of masterConsolidation", masterConsolidation.PK, subConsolidation.KB_KB_MasterBookingConsolidation);
			AssertEquals("KB_JobDirection should be set directly by applicator", nameof(DtbBookingDirection.DLV), masterConsolidation.KB_JobDirection);
			AssertEquals("KB_Status should be same in master and sub", subConsolidation.KB_Status, masterConsolidation.KB_Status);
		}

		public void TestBookingPropertiesPopulated()
		{
			var helper = new TransportBookingTestHelper(Factory);

			var transportCo = helper.CreateOrganisation("TRANSCO1");

			var bookingRequestedDate = new DateTime(2024, 06, 24, 1, 2, 0);

			var carrierAccount = Factory.New<OrgCarrierAccount>();
			carrierAccount.OAN_AccountNumber = "CARRACC1";
			carrierAccount.OAN_OH_Carrier = transportCo.PK;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR1";
			branch.GB_BranchName = "Branch1";

			var shipment = helper.CreateForwardingShipment();
			var subConsolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
			subConsolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var subBooking = helper.CreateBooking(subConsolidation);
			subConsolidation.KB_IsOverridden = true;

			subBooking.KM_RS_NKServiceLevel = "DEF";
			subBooking.KM_PL_NKCarrierServiceLevel = "STD";
			subBooking.KM_OAN_CarrierAccount = carrierAccount.PK;
			subBooking.KM_GB_Branch = branch.PK;
			subBooking.KM_BookingOfTransportRequestedDate = bookingRequestedDate;
			AssertEquals("Precondition: KM_IsAgentBooking is false by default", false, subBooking.KM_IsAgentBooking); // Can't attach a booking with a false value, so not possible to test that this property is populated
			subBooking.KM_TransportReference = "REF1";
			AssertEquals("Precondition: KM_IsActive is active by default", true, subBooking.KM_IsActive);
			AssertEquals("Precondition: KM_Status is available by default", TransportStatuses.Codes.Available, subBooking.KM_Status);
			subBooking.KM_KT_NKBookingTemplate = "EFPR";
			subBooking.KM_RatingFreightMode = "BTH";
			AssertEquals("Precondition: KM_Description is set by the booking template", "Export FCL/ULD, Pack at CNR", subBooking.KM_Description);
			AssertEquals("Precondition: KM_Direction is set by the booking template", "ORG", subBooking.KM_Direction);
			subBooking.KM_Distance = 1.0m;
			subBooking.KM_DistanceUnit = "MI";
			Factory.Save();

			var parents = new BusinessObject[] { (BusinessObject)shipment };

			Applicator.Direction = nameof(DtbBookingDirection.DLV);
			Applicator.BookingTemplate = "EFPR";

			var log = SimulateRun(parents, true);

			var masterBooking = (DtbBooking)((TransportBookingForm)((ZController)ControllerFactory.LastController).LastShownForm).BusinessEntity;

			AssertEquals("Precondition: subBooking is sub of masterBooking", masterBooking.PK, subBooking.KM_KM_MasterBooking);
			CombineAssertions("Should populate properties on booking", () =>
			{
				AssertEquals("KM_RS_NKServiceLevel should be populated on the master booking", "DEF", masterBooking.KM_RS_NKServiceLevel);
				AssertEquals("KM_PL_NKCarrierServiceLevel should be populated on the master booking", "STD", masterBooking.KM_PL_NKCarrierServiceLevel);
				AssertEquals("KM_OAN_CarrierAccount should be populated on the master booking", carrierAccount.PK, masterBooking.KM_OAN_CarrierAccount);
				AssertEquals("KM_GB_Branch should be populated on the master booking", branch.PK, masterBooking.KM_GB_Branch);
				AssertEquals("KM_BookingOfTransportRequestedDate should be populated on the master booking", bookingRequestedDate, masterBooking.KM_BookingOfTransportRequestedDate);
				AssertEquals("KM_IsAgentBooking should be populated on the master booking", false, masterBooking.KM_IsAgentBooking);
				AssertEquals("KM_TransportReference should be populated on the master booking", "REF1", masterBooking.KM_TransportReference);
				AssertEquals("KM_IsActive should be populated on the master booking", true, masterBooking.KM_IsActive);
				AssertEquals("KM_Status should be populated on the master booking", TransportStatuses.Codes.Available, masterBooking.KM_Status);
				AssertEquals("KM_RatingFreightMode should be populated on the master booking", "BTH", masterBooking.KM_RatingFreightMode);
				AssertEquals("KM_Description should be determined by the booking template, which is chosen by the user", "Export FCL/ULD, Pack at CNR", masterBooking.KM_Description);
				AssertEquals("KM_Direction should be determined by the booking template, which is chosen by the user", "ORG", masterBooking.KM_Direction);
				AssertEquals("KM_Distance should be populated on the master booking even though it's obsolete", 1.0m, masterBooking.KM_Distance);
				AssertEquals("KM_DistanceUnit should be populated on the master booking even though it's obsolete", "MI", masterBooking.KM_DistanceUnit);
			});
		}

		public void TestInstructionPropertiesPopulated()
		{
			var helper = new TransportBookingTestHelper(Factory);

			var equipment = Factory.NewWithValidTestData<RefEquipment>();
			var transportProvider = (BusinessObject)Factory.New<IRateTransportProvider>();
			transportProvider.FillWithValidTestData();
			var zone = (BusinessObject)Factory.New<IRateTransportZone>();
			zone[RateTransportZonesSchema.TZ_TP] = transportProvider.PK;

			var shipment = helper.CreateForwardingShipment();
			var subConsolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
			subConsolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var subBooking = helper.CreateBooking(subConsolidation);
			subBooking.KM_KT_NKBookingTemplate = "EFPR";
			subConsolidation.KB_IsOverridden = true;
			Factory.Save();

			var subInstruction = subBooking.Instructions.Last();
			AssertEquals("Precondition: KN_Sequence is set to 3", 3, subInstruction.KN_Sequence);
			AssertEquals("Precondition: KN_InstructionType is set to delivery", InstructionTypes.Codes.Delivery, subInstruction.KN_InstructionType);
			subInstruction.KN_DropMode = "ALL";
			subInstruction.KN_ServiceInstruction = "Modified Service Instruction";
			AssertEquals("Precondition: KN_Status is set to available", TransportStatuses.Codes.Available, subInstruction.KN_Status);
			subInstruction.KN_RQ_Equipment = equipment.PK;
			subInstruction.KN_IsContainerRateable = false;
			subInstruction.KN_IsLooseRateable = true;
			subInstruction.KN_TZ_DomesticZone = zone.PK;
			subInstruction.KN_IsAuthorisedToLeave = true;
			Factory.Save();

			var parents = new BusinessObject[] { (BusinessObject)shipment };

			Applicator.Direction = nameof(DtbBookingDirection.DLV);
			Applicator.BookingTemplate = "EFPR";

			SimulateRun(parents, true);

			var masterBooking = (DtbBooking)((TransportBookingForm)((ZController)ControllerFactory.LastController).LastShownForm).BusinessEntity;
			var masterInstruction = masterBooking.Instructions.Last();

			CombineAssertions("Should populate properties on instruction", () =>
			{
				AssertEquals("KN_Sequence should be populated on the master instruction", 3, masterInstruction.KN_Sequence);
				AssertEquals("KN_InstructionType should be populated on the master instruction", InstructionTypes.Codes.Delivery, masterInstruction.KN_InstructionType);
				AssertEquals("KN_DropMode should be populated on the master instruction", "ALL", masterInstruction.KN_DropMode);
				AssertEquals("KN_ServiceInstruction should be populated on the master instruction", "Modified Service Instruction", masterInstruction.KN_ServiceInstruction);
				AssertEquals("KN_Status should be populated on the master instruction", TransportStatuses.Codes.Available, masterInstruction.KN_Status);
				AssertEquals("KN_RQ_Equipment should be populated on the master instruction", equipment.PK, masterInstruction.KN_RQ_Equipment);
				AssertEquals("KN_IsContainerRateable should be populated on the master instruction", false, masterInstruction.KN_IsContainerRateable);
				AssertEquals("KN_IsLooseRateable should be populated on the master instruction", true, masterInstruction.KN_IsLooseRateable);
				AssertEquals("KN_TZ_DomesticZone should be populated on the master instruction", zone.PK, masterInstruction.KN_TZ_DomesticZone);
				AssertEquals("KN_IsAuthorisedToLeave should be populated on the master instruction", true, masterInstruction.KN_IsAuthorisedToLeave);
			});
		}

		public void TestConfirmationPropertiesPopulated()
		{
			var helper = new TransportBookingTestHelper(Factory);

			var estimatedDate = new ZDateTime(2024, 6, 25, 1, 2, 0);
			var actualDate = new ZDateTime(2023, 5, 24, 1, 2, 0);
			var requiredFrom = new ZDateTime(2022, 4, 23, 1, 2, 0);
			var requiredTo = requiredFrom.AddDays(7);
			var slotDateTime = new ZDateTime(2021, 3, 22, 1, 2, 0);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var driver = Factory.NewWithValidTestData<OrgContact>();
			driver.OC_OH = org.PK;
			driver.OC_ContactName = "Driver Two";

			var shipment = helper.CreateForwardingShipment();
			var subConsolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
			subConsolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var subBooking = helper.CreateBooking(subConsolidation);
			subBooking.KM_KT_NKBookingTemplate = "EFPR";
			subConsolidation.KB_IsOverridden = true;
			Factory.Save();

			var subInstruction = subBooking.Instructions.Last();
			var subConfirmation = subInstruction.Confirmations.First();
			AssertEquals("Precondition: KK_ConfirmationType is set to delivery", ConfirmationTypes.Codes.Delivery, subConfirmation.KK_ConfirmationType);

			var package = subBooking.PackageJob.Packages.AddNew("CNT", 2);
			package.Container.K0_RC_ContainerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			var divot = subInstruction.PackageDivots.AddNew();
			divot.KD_KP_Package = package.PK;
			divot.KD_Quantity = package.KP_PackageQty;

			subConfirmation.KK_Estimated = estimatedDate;
			subConfirmation.KK_Actual = actualDate;
			subConfirmation.KK_RequiredFrom = requiredFrom;
			subConfirmation.KK_RequiredTo = requiredTo;
			subConfirmation.KK_ReferenceNum = "Changed ref";
			subConfirmation.KK_ReceivedBy = "YOU";
			subConfirmation.KK_ReceivedBySignature = ZBlob.FromAscii("You were there");
			subConfirmation.KK_SlotDateTime = slotDateTime;
			subConfirmation.KK_SlotReference = "Changed SlotRef";
			subConfirmation.KK_OC_Driver = driver.PK;
			subConfirmation.KK_VehicleRegistration = "REGO2";
			subConfirmation.KK_Quantity = 1;
			subConfirmation.KK_IsEmptyContainer = true;
			subConfirmation.KK_KD_BookingInstructionPkgDivot = divot.PK;

			Factory.Save();

			var parents = new BusinessObject[] { (BusinessObject)shipment };

			Applicator.Direction = nameof(DtbBookingDirection.DLV);
			Applicator.BookingTemplate = "EFPR";

			SimulateRun(parents, true);

			var masterBooking = (DtbBooking)((TransportBookingForm)((ZController)ControllerFactory.LastController).LastShownForm).BusinessEntity;
			var masterInstruction = masterBooking.Instructions.Last();
			var totalInstructionPackageDivotQuantity = masterInstruction.DivotsWithPackages.Sum(d => d.KD_Quantity);
			var masterConfirmation = masterInstruction.Confirmations.First();

			CombineAssertions("Should populate properties on confirmation", () =>
			{
				AssertEquals("KK_ConfirmationType should be populated on the master confirmation", ConfirmationTypes.Codes.Delivery, masterConfirmation.KK_ConfirmationType);
				AssertEquals("KK_Estimated should be populated on the master confirmation", estimatedDate, masterConfirmation.KK_Estimated);
				AssertEquals("KK_Actual should be populated on the master confirmation", actualDate, masterConfirmation.KK_Actual);
				AssertEquals("KK_RequiredFrom should be populated on the master confirmation", requiredFrom, masterConfirmation.KK_RequiredFrom);
				AssertEquals("KK_RequiredTo should be populated on the master confirmation", requiredTo, masterConfirmation.KK_RequiredTo);
				AssertEquals("KK_ReferenceNum should be populated on the master confirmation", "Changed ref", masterConfirmation.KK_ReferenceNum);
				AssertEquals("KK_ReceivedBy should be populated on the master confirmation", "YOU", masterConfirmation.KK_ReceivedBy);
				AssertEquals("KK_ReceivedBySignature should be populated on the master confirmation", ZBlob.FromAscii("You were there"), masterConfirmation.KK_ReceivedBySignature);
				AssertEquals("KK_SlotDateTime should be populated on the master confirmation", slotDateTime, masterConfirmation.KK_SlotDateTime);
				AssertEquals("KK_SlotReference should be populated on the master confirmation", "Changed SlotRef", masterConfirmation.KK_SlotReference);
				AssertEquals("KK_OC_Driver should be populated on the master confirmation", driver.PK, masterConfirmation.KK_OC_Driver);
				AssertEquals("KK_VehicleRegistration should be populated on the master confirmation", "REGO2", masterConfirmation.KK_VehicleRegistration);
				AssertEquals("KK_IsEmptyContainer should be populated on the master confirmation", true, masterConfirmation.KK_IsEmptyContainer);
				AssertEquals("KK_KD_BookingInstructionPkgDivot should not be populated on the master confirmation", ZGuid.Empty, masterConfirmation.KK_KD_BookingInstructionPkgDivot);
				AssertEquals("KK_Quantity should be equal to the total divot quantity on the instruction, as KK_KD_BookingInstructionPkgDivot was not populated", totalInstructionPackageDivotQuantity, masterConfirmation.KK_Quantity);
			});
		}

		public void TestAddressesPopulated()
		{
			var helper = new TransportBookingTestHelper(Factory);

			var transportCoABC = helper.CreateOrganisation("ABC");

			var shipment = helper.CreateForwardingShipment();
			var subConsolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
			subConsolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var subBooking = helper.CreateBooking(subConsolidation);
			subBooking.KM_KT_NKBookingTemplate = "EFPR";
			subConsolidation.KB_IsOverridden = true;
			Factory.Save();

			subBooking.Address.OrganisationPK = transportCoABC.PK;
			Factory.Save();

			var parents = new BusinessObject[] { (BusinessObject)shipment };

			Applicator.Direction = nameof(DtbBookingDirection.DLV);
			Applicator.BookingTemplate = "EFPR";

			SimulateRun(parents, true);

			var masterBooking = (DtbBooking)((TransportBookingForm)((ZController)ControllerFactory.LastController).LastShownForm).BusinessEntity;

			AssertEquals("Precondition: subBooking is sub of masterBooking", masterBooking.PK, subBooking.KM_KM_MasterBooking);
			CombineAssertions("Should populate addresses", () =>
			{
				AssertEquals("Should populate address", transportCoABC.PK, masterBooking.Address.OrganisationPK);
			});
		}

		public void TestInstructionAddressesPopulated()
		{
			var helper = new TransportBookingTestHelper(Factory);

			var organisationABC = helper.CreateOrganisation("ABC");

			var shipment = helper.CreateForwardingShipment();
			var subConsolidation = helper.CreateConsolidation((IDtbBookingParent)shipment);
			subConsolidation.KB_JobDirection = nameof(DtbBookingDirection.DLV);
			var subBooking = helper.CreateBooking(subConsolidation);
			subBooking.KM_KT_NKBookingTemplate = "EFPR";
			subConsolidation.KB_IsOverridden = true;
			Factory.Save();

			subBooking.FirstConsignor.Address.OrganisationPK = organisationABC.PK;
			Factory.Save();

			var parents = new BusinessObject[] { (BusinessObject)shipment };

			Applicator.Direction = nameof(DtbBookingDirection.DLV);
			Applicator.BookingTemplate = "EFPR";

			var log = SimulateRun(parents, true);

			var masterBooking = (DtbBooking)((TransportBookingForm)((ZController)ControllerFactory.LastController).LastShownForm).BusinessEntity;

			AssertEquals("Precondition: subBooking is sub of masterBooking", masterBooking.PK, subBooking.KM_KM_MasterBooking);
			CombineAssertions("Should populate addresses on instruction", () =>
			{
				AssertEquals("Should populate address on instruction", organisationABC.PK, masterBooking.FirstConsignor.Address.OrganisationPK);
			});
		}

		string AssertBookingWasCreatedForShipmentAndHasCorrectDetailsAndReturnBookingJobID(IDtbBookingParent shipment)
		{
			var expectedTemplate = "EFPL";
			var query = new ZQuery();

			var bookingQuery = new ZDBOnlyQuery(typeof(DtbBooking));
			var bookingConsolidationQuery = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			bookingConsolidationQuery.AddToFilter(DtbBookingConsolidationSchema.KB_ParentID, SQLComparisonOperator.Equal, shipment.PK);
			bookingConsolidationQuery.AddToFilter(DtbBookingConsolidationSchema.KB_ParentTableCode, SQLComparisonOperator.Equal, shipment.TablePrefix);
			bookingQuery.AddSubQuery(bookingConsolidationQuery, JoinCondition.And);
			query.AddToFilter(bookingQuery);
			var booking = Factory.LoadTop1<DtbBooking>(query);
			AssertNotNull("Should have created a booking for shipment " + shipment.JobNumber, booking);
			if (booking != null)
			{
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should have specified template", expectedTemplate, booking.KM_KT_NKBookingTemplate);
				AssertEquals("Booking consolidation for shipment " + shipment.JobNumber + " should have correct direction", "PIC", booking.ConsolidationSingleJob.KB_JobDirection);
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should have correct direction", "ORG", booking.KM_Direction);
				AssertEquals("Booking for shipment " + shipment.JobNumber + " should not be overridden", false, booking.ConsolidationSingleJob.KB_IsOverridden);
				AssertEquals("Should have 2 packages on booking", 2, booking.PackageJob.Packages.Count);
				Assert("All packages on booking should be containers", booking.PackageJob.Packages.All(p => p.IsContainer));

				var masterBooking = booking.MasterBooking;
				AssertNotNull("Should have created a Master Booking", masterBooking);
				AssertEquals("Master booking should have specified template", expectedTemplate, masterBooking.KM_KT_NKBookingTemplate);
				AssertEquals("Booking consolidation for master booking should have correct direction", "PIC", masterBooking.ConsolidationSingleJob.KB_JobDirection);
				AssertEquals("Master booking should have correct direction", "ORG", masterBooking.KM_Direction);
				AssertEquals("Master booking should not be overridden", false, masterBooking.ConsolidationSingleJob.KB_IsOverridden);
			}
			return booking?.KM_JobID ?? string.Empty;
		}

		void AssertMasterBookingHasCorrectInstructionsAndPackageDivots(DtbBooking masterBooking)
		{
			AssertEquals("Master booking should have correct template", "EFPL", masterBooking.KM_KT_NKBookingTemplate);
			AssertEquals("Master booking should have four instructions from template", 4, masterBooking.Instructions.Count);

			var containersOnMaster = masterBooking.PackageJob.Packages;
			var topLevelLoosePackagesInContainersOnMaster = containersOnMaster.SelectMany(c => c.Packages);
			var expectedLoosePackages = topLevelLoosePackagesInContainersOnMaster.Select(p => (packagePK: p.PK, packageQty: p.KP_PackageQty)).ToArray();
			var expectedContainerPackages = containersOnMaster.Select(p => (packagePK: p.PK, packageQty: p.KP_PackageQty)).ToArray();
			var expectedBothPackages = containersOnMaster.Concat(topLevelLoosePackagesInContainersOnMaster).Select(p => (packagePK: p.PK, packageQty: p.KP_PackageQty)).ToArray();

			foreach (var instruction in masterBooking.Instructions)
			{
				string expectedPackageCategory = string.Empty;
				string expectedPackageCategoryDescription = string.Empty;
				(ZGuid packagePK, ZInt packageQty)[] expectedDivotsWithPackages = null;

				// as per template EFPL
				switch (instruction.KN_Sequence)
				{
					case 1:
						expectedPackageCategory = PackageCategories.Codes.Loose;
						expectedPackageCategoryDescription = PackageCategories.Descriptions.Loose;
						expectedDivotsWithPackages = expectedLoosePackages;
						break;

					case 2:
						expectedPackageCategory = PackageCategories.Codes.Containers;
						expectedPackageCategoryDescription = PackageCategories.Descriptions.Containers;
						expectedDivotsWithPackages = expectedContainerPackages;
						break;

					case 3:
						expectedPackageCategory = PackageCategories.Codes.Both;
						expectedPackageCategoryDescription = PackageCategories.Descriptions.Both;
						expectedDivotsWithPackages = expectedBothPackages;
						break;

					case 4:
						expectedPackageCategory = PackageCategories.Codes.Containers;
						expectedPackageCategoryDescription = PackageCategories.Descriptions.Containers;
						expectedDivotsWithPackages = expectedContainerPackages;
						break;

					default:
						Assert(FormattableString.Invariant($"Incorrect master instruction with sequence {instruction.KN_Sequence} of type {instruction.KN_InstructionType} generated, not from template"), false);
						continue;
				}

				AssertEquals(FormattableString.Invariant($"Master instruction {instruction.KN_Sequence} of type {instruction.KN_InstructionType} should have PackageCategory '{expectedPackageCategory}'"), expectedPackageCategory, instruction.PackageCategory);
				AssertContainsExactElementsInAnyOrder(FormattableString.Invariant($"Master instruction {instruction.KN_Sequence} of type {instruction.KN_InstructionType} should have correct DivotsWithPackages - {expectedPackageCategoryDescription}"), expectedDivotsWithPackages, instruction.DivotsWithPackages.Select(d => (packagePK: d.KD_KP_Package, packageQty: d.KD_Quantity)));
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			ControllerFactory = new MockControllerFactory();
			var result = new CreateMasterAndSubDtbBookingsFromDtbBookingParentsApplicator(new BusinessObjectFactory());
			result.ControllerFactory = ControllerFactory;
			return result;
		}

		new CreateMasterAndSubDtbBookingsFromDtbBookingParentsApplicator Applicator => (CreateMasterAndSubDtbBookingsFromDtbBookingParentsApplicator)base.Applicator;

		MockControllerFactory ControllerFactory { get; set; }

		protected override void TearDown()
		{
			var lastController = (ZController)ControllerFactory?.LastController;
			var transportBookingForm = (TransportBookingForm)lastController?.LastShownForm;
			transportBookingForm?.Dispose();
		}
	}

	class MockControllerFactory : IControllerFactory
	{
		public MockControllerFactory() {
			ControllerFactory = ZControllerFactory.Instance;
		}

		public IController Create(ControllerID controllerId)
		{
			LastController = ControllerFactory.Create(controllerId);
			return LastController;
		}

		public IController GetControllerForType(Type type)
		{
			LastController = ControllerFactory.GetControllerForType(type);
			return LastController;
		}

		public ControllerID GetRegisteredIdentifierByName(string identifier)
		{
			return ControllerFactory.GetRegisteredIdentifierByName(identifier);
		}

		public (IController Controller, BusinessObject BusinessObject) GetCorrectControllerAndBusinessObject(ControllerID controllerId, ZGuid pk, bool shouldReportError)
		{
			(LastController, var businessObject) = ControllerFactory.GetCorrectControllerAndBusinessObject(controllerId, pk, shouldReportError);
			return (LastController, businessObject);
		}

		IControllerFactory ControllerFactory { get; }
		public IController LastController { get; private set; }
	}
}
