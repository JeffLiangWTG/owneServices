using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	class DtbBookingConfirmationEventParentFinderTest : DtbBookingTestCaseWithFactory
	{
		public void TestGetLogParentsFor_BookingDoesNotExist()
		{
			DtbBooking booking = Helper.CreateBooking();
			booking.KM_JobID = "TB00000001";
			var pickUpInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickUpInstructionConfirmation_ConNoteNo = Helper.CreateConfirmation(pickUpInstruction, ConfirmationTypes.Codes.ConNoteNo);
			var pickUpInstructionConfirmation_Delivery = Helper.CreateConfirmation(pickUpInstruction, ConfirmationTypes.Codes.Delivery);
			var pickUpInstructionConfirmation_PickUp = Helper.CreateConfirmation(pickUpInstruction, ConfirmationTypes.Codes.PickUp);

			var pickUpInstructionPkgDivot = Helper.CreatePackageDivot(pickUpInstruction, 1);
			var pickUpInstructionPkgDivotConfirmation_ConNoteNo = Helper.CreateConfirmation(pickUpInstructionPkgDivot, ConfirmationTypes.Codes.ConNoteNo);
			var pickUpInstructionPkgDivotConfirmation_Delivery = Helper.CreateConfirmation(pickUpInstructionPkgDivot, ConfirmationTypes.Codes.Delivery);
			var pickUpInstructionPkgDivotConfirmation_PickUp = Helper.CreateConfirmation(pickUpInstructionPkgDivot, ConfirmationTypes.Codes.PickUp);

			var subscriber = GetNewEventParentFinder();

			string eventXmlText = GetTestEventXmlText("TB00000002", 1, "Jon Won 1", "Ref 101");
			var eventDataObject = new XmlEventDeserializer().Parse(eventXmlText);

			BusinessObject[] logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			AssertNull("No transport booking with Job ID 'TB00000002' exist, so no confirmations should be updated.", logParents);
		}

		public void TestGetLogParentsFor_InstructionDoesNotExist()
		{
			DtbBooking booking = Helper.CreateBooking();
			booking.KM_JobID = "TB00000001";
			var pickUpInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickUpInstructionConfirmation_ConNoteNo = Helper.CreateConfirmation(pickUpInstruction, ConfirmationTypes.Codes.ConNoteNo);
			var pickUpInstructionConfirmation_Delivery = Helper.CreateConfirmation(pickUpInstruction, ConfirmationTypes.Codes.Delivery);
			var pickUpInstructionConfirmation_PickUp = Helper.CreateConfirmation(pickUpInstruction, ConfirmationTypes.Codes.PickUp);

			var pickUpInstructionPkgDivot = Helper.CreatePackageDivot(pickUpInstruction, 1);
			var pickUpInstructionPkgDivotConfirmation_ConNoteNo = Helper.CreateConfirmation(pickUpInstructionPkgDivot, ConfirmationTypes.Codes.ConNoteNo);
			var pickUpInstructionPkgDivotConfirmation_Delivery = Helper.CreateConfirmation(pickUpInstructionPkgDivot, ConfirmationTypes.Codes.Delivery);
			var pickUpInstructionPkgDivotConfirmation_PickUp = Helper.CreateConfirmation(pickUpInstructionPkgDivot, ConfirmationTypes.Codes.PickUp);

			var subscriber = GetNewEventParentFinder();

			string eventXmlText = GetTestEventXmlText("TB00000001", 5, "Jon Won 2", "Ref 201");
			var eventDataObject = new XmlEventDeserializer().Parse(eventXmlText);

			BusinessObject[] logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			AssertContainsExactElementsInAnyOrder("Transport Booking have only 1 instruction, instruction #5 should not be found, no confirmations should be updated. But Log should be added to the booking",
				new[] { booking }, logParents);
		}

		public void TestGetLogParentsFor_MultipleConfirmationsOnDifferentLevels()
		{
			DtbBooking booking = Helper.CreateBooking();
			booking.KM_JobID = "TB00000001";

			// Pickup Instruction
			var pickUpInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickUpInstructionConfirmation_ConNoteNo = Helper.CreateConfirmation(pickUpInstruction, ConfirmationTypes.Codes.ConNoteNo);
			var pickUpInstructionConfirmation_Delivery = Helper.CreateConfirmation(pickUpInstruction, ConfirmationTypes.Codes.Delivery);
			var pickUpInstructionConfirmation_PickUp = Helper.CreateConfirmation(pickUpInstruction, ConfirmationTypes.Codes.PickUp);

			var pickUpInstructionPkgDivot = Helper.CreatePackageDivot(pickUpInstruction, 1);
			var pickUpInstructionPkgDivotConfirmation_ConNoteNo = Helper.CreateConfirmation(pickUpInstructionPkgDivot, ConfirmationTypes.Codes.ConNoteNo);
			var pickUpInstructionPkgDivotConfirmation_Delivery = Helper.CreateConfirmation(pickUpInstructionPkgDivot, ConfirmationTypes.Codes.Delivery);
			var pickUpInstructionPkgDivotConfirmation_PickUp = Helper.CreateConfirmation(pickUpInstructionPkgDivot, ConfirmationTypes.Codes.PickUp);

			// Delivery Instruction
			var deliveryInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryInstructionConfirmation_ConNoteNo = Helper.CreateConfirmation(deliveryInstruction, ConfirmationTypes.Codes.ConNoteNo);
			var deliveryInstructionConfirmation_Delivery = Helper.CreateConfirmation(deliveryInstruction, ConfirmationTypes.Codes.Delivery);
			var deliveryInstructionConfirmation_PickUp = Helper.CreateConfirmation(deliveryInstruction, ConfirmationTypes.Codes.PickUp);

			var deliveryInstructionPkgDivot = Helper.CreatePackageDivot(deliveryInstruction, 1);
			var deliveryInstructionPkgDivotConfirmation_ConNoteNo = Helper.CreateConfirmation(deliveryInstructionPkgDivot, ConfirmationTypes.Codes.ConNoteNo);
			var deliveryInstructionPkgDivotConfirmation_Delivery = Helper.CreateConfirmation(deliveryInstructionPkgDivot, ConfirmationTypes.Codes.Delivery);
			var deliveryInstructionPkgDivotConfirmation_PickUp = Helper.CreateConfirmation(deliveryInstructionPkgDivot, ConfirmationTypes.Codes.PickUp);

			var subscriber = GetNewEventParentFinder();

			string eventXmlText = GetTestEventXmlText("TB00000001", 1, "Jon Won 3", "Ref 301");
			var eventDataObject = new XmlEventDeserializer().Parse(eventXmlText);

			BusinessObject[] logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			AssertContainsExactElementsInAnyOrder("Only pickup confirmations of pickup instructions should be updated. No Package ID specified and found an ALL confirmation, so just use that.",
				new BusinessObject[] { pickUpInstructionConfirmation_PickUp },
				logParents);
		}

		public void TestGetLogParentsFor_MultiInstruction()
		{
			DtbBooking booking = Helper.CreateBooking();
			booking.KM_JobID = "TB00000001";
			var multiInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi);//DeliveryAndPickup);
			var multiInstructionConfirmation_ConNoteNo = Helper.CreateConfirmation(multiInstruction, ConfirmationTypes.Codes.ConNoteNo);
			var multiInstructionConfirmation_Delivery = Helper.CreateConfirmation(multiInstruction, ConfirmationTypes.Codes.Delivery);
			var multiInstructionConfirmation_PickUp = Helper.CreateConfirmation(multiInstruction, ConfirmationTypes.Codes.PickUp);

			var subscriber = GetNewEventParentFinder();

			booking.KM_Direction = Constants.CartageDirection.Origin;

			string eventXmlText = GetTestEventXmlText("TB00000001", 1, "Jon Won 4", "Ref 401");
			var eventDeserializer = new XmlEventDeserializer();
			var eventDataObject = eventDeserializer.Parse(eventXmlText);

			BusinessObject[] logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			AssertContainsExactElementsInAnyOrder("Only pickup confirmations of pickup instructions should be updated.",
				new BusinessObject[] { multiInstructionConfirmation_PickUp },
				logParents);

			booking.KM_Direction = Constants.CartageDirection.Destination;
			eventDataObject = eventDeserializer.Parse(eventXmlText);
			logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			AssertContainsExactElementsInAnyOrder("Only pickup confirmations of pickup instructions should be updated.",
				new BusinessObject[] { multiInstructionConfirmation_Delivery },
				logParents);
		}

		public void TestGetLogParentsFor_NoConfirmationOfInstructionTypeExist()
		{
			DtbBooking booking = Helper.CreateBooking();
			booking.KM_JobID = "TB00000001";
			var pickUpInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);

			var subscriber = GetNewEventParentFinder();

			string eventXmlText = GetTestEventXmlText("TB00000001", 1, "Jon Won 6", "Ref 601");
			var eventDataObject = new XmlEventDeserializer().Parse(eventXmlText);

			AssertEquals("Precondition - no confirmations exist on pickUpInstruction", 0, pickUpInstruction.Confirmations.Count);

			BusinessObject[] logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			AssertEquals("If no confirmation that much instruction type exist, then new one should be created.", 1, pickUpInstruction.Confirmations.Count);
			AssertEquals("Newly created confirmation should also be in list of update.", 1, logParents.Length);
			AssertEquals("Newly created confirmation should also be in list of update.", pickUpInstruction.Confirmations[0], logParents[0]);
		}

		public void TestNonDCFUniversalEventForTransportBookingPackageGoesToDeliveryInstruction()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "TB00000001";

			var packingJob = Helper.CreatePackageJob(consolidation);

			var package1 = PackingHelper.CreatePackage(packingJob, "NABA0909090990", 1, Constants.PkgUnit.Package, "");
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CFS, null);
			Helper.CreatePackageDivot(instruction1, package1).KD_Quantity = 1;

			var package2 = PackingHelper.CreatePackage(packingJob, "PCK127890012943", 1, Constants.PkgUnit.Package, "");
			var instruction2_1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			Helper.CreatePackageDivot(instruction2_1, package2).KD_Quantity = 1;
			var instruction2_2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);
			Helper.CreatePackageDivot(instruction2_2, package2).KD_Quantity = 1;

			Factory.Save();

			var subscriber = GetNewEventParentFinder();
			var eventDataObject = new XmlEventDeserializer().Parse(universalEventXmlWithPackageID.Replace(Events.DeliveryCartageCompleteFinalisedCode, Events.CustomsClearedCode));
			AssertEquals("eventDataObject.EventType", Events.CustomsClearedCode, eventDataObject.EventType);

			BusinessObject[] logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			AssertEquals("Should be linked to one package.", 1, logParents.Length);
			AssertEquals("Should be linked to package.", package2, logParents[0]);
		}

		public void TestDCFUniversalEventForTransportBookingPackageGoesToDeliveryInstruction()
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "TB00000001";

			var packingJob = Helper.CreatePackageJob(consolidation);

			var package1 = PackingHelper.CreatePackage(packingJob, "NABA0909090990", 1, Constants.PkgUnit.Package, "");
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CFS, null);
			Helper.CreatePackageDivot(instruction1, package1).KD_Quantity = 1;

			var package2 = PackingHelper.CreatePackage(packingJob, "PCK127890012943", 1, Constants.PkgUnit.Package, "");
			var instruction2_1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			Helper.CreatePackageDivot(instruction2_1, package2).KD_Quantity = 1;
			var instruction2_2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);
			Helper.CreatePackageDivot(instruction2_2, package2).KD_Quantity = 1;

			Factory.Save();

			var subscriber = GetNewEventParentFinder();
			var eventDataObject = new XmlEventDeserializer().Parse(universalEventXmlWithPackageID);

			BusinessObject[] logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			AssertEquals("If no confirmation that much instruction type exist, then new one should be created.", 1, instruction2_2.Confirmations.Count);
			AssertEquals("Should be linked to one confirmation.", 1, logParents.Length);
			AssertEquals("Newly created confirmation should also be in list of update.", instruction2_2.Confirmations[0], logParents[0]);
		}

		public void TestIfNoMatchOnInstructionAndPackageIDDoesNotMatchShouldFallBackToBooking()
		{
			const string packageID1_NotInEvent = "NABA0909090990";
			const string packageID2_NotInEvent = "PCKOTHER123456";

			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = "TB00000001";

			var packingJob = Helper.CreatePackageJob(consolidation);

			var package1 = PackingHelper.CreatePackage(packingJob, packageID1_NotInEvent, 1, Constants.PkgUnit.Package, "");
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CFS, null);
			Helper.CreatePackageDivot(instruction1, package1).KD_Quantity = 1;

			var package2 = PackingHelper.CreatePackage(packingJob, packageID2_NotInEvent, 1, Constants.PkgUnit.Package, "");
			var instruction2_1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			Helper.CreatePackageDivot(instruction2_1, package2).KD_Quantity = 1;
			var instruction2_2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);
			Helper.CreatePackageDivot(instruction2_2, package2).KD_Quantity = 1;

			Factory.Save();

			var subscriber = GetNewEventParentFinder();
			var eventDataObject = new XmlEventDeserializer().Parse(universalEventXmlWithPackageID);

			BusinessObject[] logParents = null;
			AssertNoExceptionThrown(
				"Should not throw exception from EventParentFinder.GetLogParentsForEventUsingContextAndLogIfFound() about enumerables returning null elements",
				() =>
				{
					logParents = subscriber.GetLogParentsForEvent(eventDataObject);
				});
			AssertEquals("Should not find a package and simply return booking", booking, logParents[0]);
		}

		public void TestIfNoMatchOnInstructionAndPackageIDDoesNotMatchShouldFallBackToBooking_NonIntegrationTest()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			Factory.Save();

			var eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "", "NABA0909090990", "", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertEquals("Should not throw exception from EventParentFinder.GetLogParentsForEventUsingContextAndLogIfFound() about enumerables returning null elements due to not finding matching package", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals("Should not find a package and simply return booking", booking, logParents[0]);
		}

		public void TestTransportBookingID()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			Factory.Save();

			var eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "", "", "", "", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertNull("Precondition: Should return nothing if booking ID is not supplied", logParents);

			eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "", "", "", "", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder("Should return booking if booking ID is supplied", new[] { booking }, logParents);
		}

		public void TestTransportInstructionID()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO, null);
			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CTO, null);
			instruction1.KN_Sequence = 1;
			instruction2.KN_Sequence = 2;
			Factory.Save();

			var eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "1", "", "", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder("Should create a confirmation. Confirmation should be Pickup because the instruction is a Pickup.", new[] { instruction1.FirstPickupConfirmation }, logParents);

			eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "2", "", "", "", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder("Should create a confirmation. Confirmation should be Delivery because the instruction is a Multi, falls back to booking direction.", new[] { instruction2.LastDeliveryConfirmation }, logParents);
		}

		public void TestPackageID()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO, null);
			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CTO, null);

			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var container1 = PackingHelper.CreatePackage(packingJob, "CONT142536", 1, Constants.PkgUnit.Container, "20GP");
			var container2 = PackingHelper.CreatePackage(packingJob, "CONT000000", 1, Constants.PkgUnit.Container, "20GP");

			// container 1
			var containe1Divot = Helper.CreatePackageDivot(instruction1, container1);
			var containe2Divot = Helper.CreatePackageDivot(instruction2, container2);
			var container1Confirmation = Helper.CreateConfirmation(containe1Divot, ConfirmationTypes.Codes.PickUp);
			var container2Confirmation = Helper.CreateConfirmation(containe2Divot, ConfirmationTypes.Codes.Delivery);
			Factory.Save();

			var eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "", "CONT142536", "", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { container1Confirmation }, logParents);

			eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "", "CONT000000", "", "", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { container2Confirmation }, logParents);
		}

		public void TestFacility()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO, null);
			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CFS, null);
			Factory.Save();

			var eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "", "", CargoWise.EventReference.Constants.Facilities.Code.Terminal, "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.FirstPickupConfirmation }, logParents);

			eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "", "", CargoWise.EventReference.Constants.Facilities.Code.Depot, "", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { instruction2.LastDeliveryConfirmation }, logParents);
		}

		public void TestCity()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO, null);
			instruction1.Address.E2_City = "BOTANY";
			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var container = PackingHelper.CreatePackage(packingJob, "CONT142536", 1, Constants.PkgUnit.Container, "20GP");

			var containeDivot1 = Helper.CreatePackageDivot(instruction1, container);
			var containerConfirmation1 = Helper.CreateConfirmation(containeDivot1, ConfirmationTypes.Codes.PickUp);

			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CTO, null);
			instruction2.Address.E2_City = "ALEXANDRIA";

			var containeDivot2 = Helper.CreatePackageDivot(instruction2, container);
			var containerConfirmation2 = Helper.CreateConfirmation(containeDivot2, ConfirmationTypes.Codes.PickUp);
			Factory.Save();

			var eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "", "CONT142536", "", "BOTANY", Constants.EventReferenceParameterReasons.Pickup);
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { containerConfirmation1 }, logParents);

			booking.Instructions.Delete(instruction1);

			//make sure loose match rule apply for city
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { containerConfirmation2 }, logParents);
		}

		public void TestReason()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var instruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CTO, null);
			instruction1.Address.E2_City = "Alexandria";
			var instruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi, OrganisationTypesList.Codes.CFS, null);
			instruction2.Address.E2_City = "Botany";
			Factory.Save();

			var eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "", "", "", "Alexandria", Constants.EventReferenceParameterReasons.Pickup);
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.FirstPickupConfirmation }, logParents);

			eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "", "", "", "Alexandria", Constants.EventReferenceParameterReasons.Pack);
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.FirstPickupConfirmation }, logParents);

			eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "", "", "", "Alexandria", Constants.EventReferenceParameterReasons.Delivery);
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.LastDeliveryConfirmation }, logParents);

			eDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "", "", "", "Alexandria", Constants.EventReferenceParameterReasons.Unpack);
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { instruction1.LastDeliveryConfirmation }, logParents);
		}

		public void TestSlot_Exists()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CTO, null);

			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var container1 = PackingHelper.CreatePackage(packingJob, "CONT142536", 1, Constants.PkgUnit.Container, "20GP");
			var container2 = PackingHelper.CreatePackage(packingJob, "CONT000000", 1, Constants.PkgUnit.Container, "20GP");

			var divotC1p = Helper.CreatePackageDivot(picInstruction, container1);
			var divotC1d = Helper.CreatePackageDivot(dlvInstruction, container1);
			var divotC2p = Helper.CreatePackageDivot(picInstruction, container2);
			var divotC2d = Helper.CreatePackageDivot(dlvInstruction, container2);

			var confirmationC1p = Helper.CreateConfirmation(divotC1p, ConfirmationTypes.Codes.PickUp);
			var confirmationC1d = Helper.CreateConfirmation(divotC1d, ConfirmationTypes.Codes.Delivery);
			var confirmationC2p = Helper.CreateConfirmation(divotC2p, ConfirmationTypes.Codes.PickUp);
			var confirmationC2d = Helper.CreateConfirmation(divotC2d, ConfirmationTypes.Codes.Delivery);

			Factory.Save();

			var eDataObject = GetEvent(Events.SlotConfirmedCode, ZDateTime.Now, "TB00001234", "", "CONT142536", "Terminal", "", "Pickup");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmationC1p }, logParents);

			eDataObject = GetEvent(Events.SlotConfirmedCode, ZDateTime.Now, "TB00001234", "", "CONT000000", "Terminal", "", "Delivery");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmationC2d }, logParents);
		}

		public void TestSlot_Create()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CTO, null);

			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var container1 = PackingHelper.CreatePackage(packingJob, "CONT142536", 1, Constants.PkgUnit.Container, "20GP");
			var container2 = PackingHelper.CreatePackage(packingJob, "CONT000000", 1, Constants.PkgUnit.Container, "20GP");

			var divotC1p = Helper.CreatePackageDivot(picInstruction, container1);
			var divotC1d = Helper.CreatePackageDivot(dlvInstruction, container1);
			var divotC2p = Helper.CreatePackageDivot(picInstruction, container2);
			var divotC2d = Helper.CreatePackageDivot(dlvInstruction, container2);
			Factory.Save();

			var eDataObject = GetEvent(Events.SlotConfirmedCode, ZDateTime.Now, "TB00001234", "", "CONT142536", "Terminal", "", "Pickup");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertEquals(1, logParents.Length);
			var pickupSlotConfirmation = (DtbBookingConfirmation)logParents[0];
			AssertEquals(ConfirmationTypes.Codes.PickUp, pickupSlotConfirmation.KK_ConfirmationType);
			AssertEquals(divotC1p.PK, pickupSlotConfirmation.KK_KD_BookingInstructionPkgDivot);

			eDataObject = GetEvent(Events.SlotConfirmedCode, ZDateTime.Now, "TB00001234", "", "CONT000000", "Terminal", "", "Delivery");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertEquals(1, logParents.Length);
			var deliverySlotConfirmation = (DtbBookingConfirmation)logParents[0];
			AssertEquals(ConfirmationTypes.Codes.Delivery, deliverySlotConfirmation.KK_ConfirmationType);
			AssertEquals(divotC2d.PK, deliverySlotConfirmation.KK_KD_BookingInstructionPkgDivot);
		}

		public void TestPackCount()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);

			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Pallet);
			var package2 = PackingHelper.CreatePackage(packingJob, "", 20, Constants.PkgUnit.Pallet);

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1, 6); // 6 of 10
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1, 6);
			var divot2p = Helper.CreatePackageDivot(picInstruction, package2, 13); // 13 of 20
			var divot2d = Helper.CreatePackageDivot(dlvInstruction, package2, 13);
			var confirmation1p = Helper.CreateConfirmation(divot1p, ConfirmationTypes.Codes.PickUp);
			var confirmation1d = Helper.CreateConfirmation(divot1d, ConfirmationTypes.Codes.Delivery);
			var confirmation2p = Helper.CreateConfirmation(divot2p, ConfirmationTypes.Codes.PickUp);
			var confirmation2d = Helper.CreateConfirmation(divot2d, ConfirmationTypes.Codes.Delivery);
			Factory.Save();

			var eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 13, "", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation2d }, logParents);

			eDataObject = GetEvent(Events.PickedUpCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 6, "", "", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation1p }, logParents);
		}

		public void TestPackCount_ForAll()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);

			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Pallet);
			var package2 = PackingHelper.CreatePackage(packingJob, "", 20, Constants.PkgUnit.Pallet);

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1, 10);
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1, 10);
			var divot2p = Helper.CreatePackageDivot(picInstruction, package2, 20);
			var divot2d = Helper.CreatePackageDivot(dlvInstruction, package2, 20);
			var confirmation1p = Helper.CreateConfirmation(divot1p, ConfirmationTypes.Codes.PickUp, 10);
			var confirmation1d = Helper.CreateConfirmation(divot1d, ConfirmationTypes.Codes.Delivery, 10);
			var confirmation2p = Helper.CreateConfirmation(divot2p, ConfirmationTypes.Codes.PickUp, 20);
			var confirmation2d = Helper.CreateConfirmation(divot2d, ConfirmationTypes.Codes.Delivery, 20);
			Factory.Save();

			var eDataObject = GetEvent(Events.PickedUpCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 20, "", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation2p }, logParents);

			eDataObject = GetEvent(Events.PickedUpCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 10, "", "", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation1p }, logParents);
		}

		public void TestPackCount_WithPackageType()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);

			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var pallets = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Pallet);
			var drums = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Drum);

			var divot1p = Helper.CreatePackageDivot(picInstruction, pallets, 10);
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, pallets, 10);
			var divot2p = Helper.CreatePackageDivot(picInstruction, drums, 10);
			var divot2d = Helper.CreatePackageDivot(dlvInstruction, drums, 10);
			var confirmation1p = Helper.CreateConfirmation(divot1p, ConfirmationTypes.Codes.PickUp);
			var confirmation1d = Helper.CreateConfirmation(divot1d, ConfirmationTypes.Codes.Delivery);
			var confirmation2p = Helper.CreateConfirmation(divot2p, ConfirmationTypes.Codes.PickUp);
			var confirmation2d = Helper.CreateConfirmation(divot2d, ConfirmationTypes.Codes.Delivery);
			Factory.Save();

			var eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 10, "DRM", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation2d }, logParents);

			eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 10, "PLT", "", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation1d }, logParents);
		}

		public void TestPackType()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var instruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);

			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 1, Constants.PkgUnit.Pallet);
			var package2 = PackingHelper.CreatePackage(packingJob, "", 1, Constants.PkgUnit.Box);

			var divot1 = Helper.CreatePackageDivot(instruction, package1, 1);
			var divot2 = Helper.CreatePackageDivot(instruction, package2, 1);

			Helper.CreateConfirmation(divot1, ConfirmationTypes.Codes.PickUp);
			var confirmationToMatch = Helper.CreateConfirmation(divot2, ConfirmationTypes.Codes.PickUp);

			Factory.Save();

			var eDataObject = GetEvent(Events.PickedUpCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 1, "BOX", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmationToMatch }, logParents);
		}

		public void TestWeight()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);

			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Pallet);
			package1.KP_Weight = 100;
			package1.KP_WeightUQ = Constants.Weight.Kilograms;

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1, 6); // 6 of 10
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1, 6);
			var confirmation1p = Helper.CreateConfirmation(divot1p, ConfirmationTypes.Codes.PickUp);
			var confirmation1d = Helper.CreateConfirmation(divot1d, ConfirmationTypes.Codes.Delivery);
			Factory.Save();

			var eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 0, "", "60 KG", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation1d }, logParents);
		}

		public void TestWeightWithUnitConversion()
		{
			// Note : We do not handle Part Delivery of Weight yet. ie, Book for 1 package of 100 KG, and we get a returned event for 50kg, we consider this DONE ie 100% / 100 KG

			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);

			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Pallet);
			package1.KP_Weight = 100;
			package1.KP_WeightUQ = Constants.Weight.Kilograms;

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1, 6); // 6 of 10
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1, 6);
			var confirmation1p = Helper.CreateConfirmation(divot1p, ConfirmationTypes.Codes.PickUp);
			var confirmation1d = Helper.CreateConfirmation(divot1d, ConfirmationTypes.Codes.Delivery);
			Factory.Save();

			var eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 0, "", "132.277 LB", ""); // =60kg
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation1d }, logParents);

			eDataObject = GetEvent(Events.PickedUpCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 0, "", "132.277123 LB", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation1p }, logParents);
		}

		public void TestVolume()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);

			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Pallet);
			package1.KP_Volume = 100;
			package1.KP_VolumeUQ = Constants.Volume.CubicMetres;

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1, 6); // 6 of 10
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1, 6);
			var confirmation1p = Helper.CreateConfirmation(divot1p, ConfirmationTypes.Codes.PickUp);
			var confirmation1d = Helper.CreateConfirmation(divot1d, ConfirmationTypes.Codes.Delivery);
			Factory.Save();

			var eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 0, "", "", "60");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation1d }, logParents);
		}

		public void TestVolumeWithUnitConversion()
		{
			// Note : We do not handle Part Delivery of Volume yet. ie, Book for 1 package of 100 M3, and we get a returned event for 50 M3, we consider this DONE ie 100% / 100 M3

			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);

			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Pallet);
			package1.KP_Volume = 10;
			package1.KP_VolumeUQ = Constants.Volume.CubicMetres;

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1, 6);
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1, 6);
			var confirmation1p = Helper.CreateConfirmation(divot1p, ConfirmationTypes.Codes.PickUp);
			var confirmation1d = Helper.CreateConfirmation(divot1d, ConfirmationTypes.Codes.Delivery);
			Factory.Save();

			var eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 0, "", "", "211.888 CF"); // =6m3
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation1d }, logParents);

			eDataObject = GetEvent(Events.PickedUpCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 0, "", "", "211.88800 CF");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { confirmation1p }, logParents);
		}

		public void TestPackWeightVolume_All()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);
			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Pallet);
			var package2 = PackingHelper.CreatePackage(packingJob, "", 20, Constants.PkgUnit.Pallet);
			package1.KP_Weight = 100;
			package1.KP_Volume = 100;
			package1.KP_WeightUQ = Constants.Weight.Kilograms;
			package1.KP_VolumeUQ = Constants.Volume.CubicMetres;
			package2.KP_Weight = 200;
			package2.KP_Volume = 200;
			package2.KP_WeightUQ = Constants.Weight.Kilograms;
			package2.KP_VolumeUQ = Constants.Volume.CubicMetres;

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1, 6); // 6 of 10
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1, 6);
			var divot2p = Helper.CreatePackageDivot(picInstruction, package2, 13); // 13 of 20
			var divot2d = Helper.CreatePackageDivot(dlvInstruction, package2, 13);
			var picConfirmation = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);
			var dlvConfirmation = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			var eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 13, "", "130.000", "130.000 M3");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			var confirmationSelected = (DtbBookingConfirmation)logParents[0];
			AssertEquals("Can't find exact match, fallback to ALL, but then split it to divot confirmations", divot2d, confirmationSelected.PackageDivot);
			AssertEquals("Should be the delivery divot with qty 13", 13, confirmationSelected.KK_Quantity);

			eDataObject = GetEvent(Events.PickedUpCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 6, "", "60", "60");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			confirmationSelected = (DtbBookingConfirmation)logParents[0];
			AssertEquals("Found exact match after splitting ALL", divot1p, confirmationSelected.PackageDivot);
			AssertEquals("Should be the delivery divot with qty 6", 6, confirmationSelected.KK_Quantity);
		}

		public void TestSplit_ALL_Containers()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, null);
			var mulInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi, LocalCartageJobOrgTypeList.Codes.CFS, null);
			var delInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, null);
			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var container1 = PackingHelper.CreatePackage(packingJob, "CONT000001", 1, Constants.PkgUnit.Container, "20GP");
			var container2 = PackingHelper.CreatePackage(packingJob, "CONT000002", 1, Constants.PkgUnit.Container, "20GP");
			var container3 = PackingHelper.CreatePackage(packingJob, "CONT000003", 1, Constants.PkgUnit.Container, "20GP");
			var container4 = PackingHelper.CreatePackage(packingJob, "CONT000004", 1, Constants.PkgUnit.Container, "20GP");

			// container 1
			var containe1Divot = Helper.CreatePackageDivot(mulInstruction, container1);
			var containe2Divot = Helper.CreatePackageDivot(mulInstruction, container2);
			var containe3Divot = Helper.CreatePackageDivot(mulInstruction, container3);
			var containe4Divot = Helper.CreatePackageDivot(mulInstruction, container4);

			var dlvConfirmation = Helper.CreateConfirmation(mulInstruction, ConfirmationTypes.Codes.Delivery);
			AssertEquals(4, dlvConfirmation.KK_Quantity);
			Factory.Save();

			var container1DlvTime = new ZDateTime(2016, 1, 1);
			var container2DlvTime = new ZDateTime(2016, 1, 2);
			var container3And4DlvTime = new ZDateTime(2016, 1, 3);
			var event1 = GetEvent(Events.DeliveredCode, container1DlvTime, "TB00001234", "", "CONT000001", "", "", "", 0, LocalCartageJobOrgTypeList.Codes.CFS, "", "", "CONT000001");
			var event2 = GetEvent(Events.DeliveredCode, container2DlvTime, "TB00001234", "", "CONT000002", "", "", "", 0, LocalCartageJobOrgTypeList.Codes.CFS, "", "", "CONT000002");
			var eventWithMultipleContainerNumbers = GetEvent(Events.DeliveredCode, container3And4DlvTime, "TB00001234", "", "CONT000003", "", "", "", 0, LocalCartageJobOrgTypeList.Codes.CFS, "", "", "CONT000003");
			eventWithMultipleContainerNumbers.ContextCollection.Add(new Context() { Type = nameof(ContextTypes.ContainerNumber), Value = "CONT000004" });

			var logParents1 = GetNewEventParentFinder().GetLogParentsForEvent(event1);
			var logParents2 = GetNewEventParentFinder().GetLogParentsForEvent(event2);
			var logParents3 = GetNewEventParentFinder().GetLogParentsForEvent(eventWithMultipleContainerNumbers);

			AssertEquals(4, mulInstruction.Confirmations.Count);
			AssertNotNull(mulInstruction.Confirmations.FirstOrDefault(c => c.KK_Quantity == 1 && c.PackageDivot.Package.KP_PackageID == "CONT000001"));
			AssertNotNull(mulInstruction.Confirmations.FirstOrDefault(c => c.KK_Quantity == 1 && c.PackageDivot.Package.KP_PackageID == "CONT000002"));
			AssertNotNull(mulInstruction.Confirmations.FirstOrDefault(c => c.KK_Quantity == 1 && c.PackageDivot.Package.KP_PackageID == "CONT000003"));
			AssertNotNull(mulInstruction.Confirmations.FirstOrDefault(c => c.KK_Quantity == 1 && c.PackageDivot.Package.KP_PackageID == "CONT000004"));
		}

		public void TestSplit_ALL_Confirmation()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, null);
			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Pallet);
			package1.KP_Weight = 100;
			package1.KP_Volume = 100;

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1);
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1);
			var picConfirmation = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);
			var dlvConfirmation = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			var eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 6, "", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder("Can't find exact match, split the ALL confirmation and assign to it.", new[] { dlvConfirmation }, logParents);
			AssertEquals("Should have split all confirmation.", 6, dlvConfirmation.KK_Quantity);
			AssertEquals(2, dlvInstruction.Confirmations.Count);
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 6, 4 }, dlvInstruction.Confirmations.Select(c => c.KK_Quantity));

			eDataObject = GetEvent(Events.PickedUpCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 6, "", "", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder("Can't find exact match, , split the ALL confirmation and assign to it.", new[] { picConfirmation }, logParents);
			AssertEquals(6, picConfirmation.KK_Quantity);
			AssertEquals(2, picInstruction.Confirmations.Count);
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 6, 4 }, picInstruction.Confirmations.Select(c => c.KK_Quantity));
		}

		public void TestSplit_Divot_Confirmation()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, null);
			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Pallet);
			package1.KP_Weight = 100;
			package1.KP_Volume = 100;

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1, 6);
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1, 6);
			var picConfirmation = Helper.CreateConfirmation(divot1p, ConfirmationTypes.Codes.PickUp);
			var dlvConfirmation = Helper.CreateConfirmation(divot1d, ConfirmationTypes.Codes.Delivery);

			var eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 4, "", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder("Can't find exact match, split the Divot confirmation and assign to it.", new[] { dlvConfirmation }, logParents);
			AssertEquals(4, dlvConfirmation.KK_Quantity);
			AssertEquals(2, dlvInstruction.Confirmations.Count);
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 4, 2 }, dlvInstruction.Confirmations.Select(c => c.KK_Quantity));

			eDataObject = GetEvent(Events.PickedUpCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 2, "", "", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder("Can't find exact match, split the Divot confirmation and assign to it.", new[] { picConfirmation }, logParents);
			AssertEquals(2, picConfirmation.KK_Quantity);
			AssertEquals(2, picInstruction.Confirmations.Count);
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 2, 4 }, picInstruction.Confirmations.Select(c => c.KK_Quantity));
		}

		public void TestSplit_ALL_Confirmation_AssignToNext()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, null);
			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Pallet);
			package1.KP_Weight = 100;
			package1.KP_Volume = 100;

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1);
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1);
			var picConfirmation = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);
			var dlvConfirmation = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			var eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 6, "", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder("Can't find exact match, split the ALL confirmation and assign to it.", new[] { dlvConfirmation }, logParents);
			AssertEquals(6, dlvConfirmation.KK_Quantity);
			AssertEquals(2, dlvInstruction.Confirmations.Count);
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 6, 4 }, dlvInstruction.Confirmations.Select(c => c.KK_Quantity));

			dlvConfirmation.Logs.AddNew(Events.Delivered, ZDateTimeOffset.Now); // simulate what Importer would do
			eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 3, "", "", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			var confirmationAssigned_3 = (DtbBookingConfirmation)logParents[0];
			AssertNotEquals("Should not assign to last confirmation", dlvConfirmation, confirmationAssigned_3);
			AssertEquals(3, confirmationAssigned_3.KK_Quantity);
			AssertEquals(3, dlvInstruction.Confirmations.Count);
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 6, 3, 1 }, dlvInstruction.Confirmations.Select(c => c.KK_Quantity));

			confirmationAssigned_3.Logs.AddNew(Events.Delivered, ZDateTimeOffset.Now); // simulate what Importer would do
			eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 1, "", "", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			var confirmationAssigned_1 = (DtbBookingConfirmation)logParents[0];
			AssertNotEquals("Should not assign to last confirmation", dlvConfirmation, confirmationAssigned_1);
			AssertNotEquals("Should not assign to last confirmation", confirmationAssigned_3, confirmationAssigned_1);
			AssertEquals(1, confirmationAssigned_1.KK_Quantity);
			AssertEquals(3, dlvInstruction.Confirmations.Count);
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 6, 3, 1 }, dlvInstruction.Confirmations.Select(c => c.KK_Quantity));

			confirmationAssigned_1.Logs.AddNew(Events.Delivered, ZDateTimeOffset.Now); // simulate what Importer would do
			var additionalEDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 1, "", "", "");
			logParents = GetNewEventParentFinder().GetLogParentsForEvent(additionalEDataObject);
			var bookingLogParent = logParents[0];
			AssertEquals("Should assign to booking as all Confirmations filled.", booking, bookingLogParent);
			AssertEquals("Ensure no new Confirmations created.", 3, dlvInstruction.Confirmations.Count);
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 6, 3, 1 }, dlvInstruction.Confirmations.Select(c => c.KK_Quantity));
		}

		public void TestSplit_ALL_Confirmation_Into1s_AssignToMultiple()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, null);
			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 1, Constants.PkgUnit.Pallet);
			var package2 = PackingHelper.CreatePackage(packingJob, "", 1, Constants.PkgUnit.Pallet);
			var package3 = PackingHelper.CreatePackage(packingJob, "", 1, Constants.PkgUnit.Pallet);

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1);
			var divot2p = Helper.CreatePackageDivot(picInstruction, package2);
			var divot3p = Helper.CreatePackageDivot(picInstruction, package3);
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1);
			var divot2d = Helper.CreatePackageDivot(dlvInstruction, package2);
			var divot3d = Helper.CreatePackageDivot(dlvInstruction, package3);
			var picConfirmation = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);
			var dlvConfirmation = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			var eDataObject = GetEvent(Events.DeliveredCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 2, "", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertEquals("Assigned to 2", 2, logParents.Length);
			AssertEquals("Split into 3", 3, dlvInstruction.Confirmations.Count);
			AssertEquals(1, ((DtbBookingConfirmation)logParents[0]).KK_Quantity);
			AssertEquals(1, ((DtbBookingConfirmation)logParents[1]).KK_Quantity);
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 1, 1, 1 }, dlvInstruction.Confirmations.Select(c => c.KK_Quantity));
		}

		public void TestNoConfirmations()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, null);
			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 1, Constants.PkgUnit.Pallet);
			var package2 = PackingHelper.CreatePackage(packingJob, "", 1, Constants.PkgUnit.Pallet);
			var package3 = PackingHelper.CreatePackage(packingJob, "", 1, Constants.PkgUnit.Pallet);

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1);
			var divot2p = Helper.CreatePackageDivot(picInstruction, package2);
			var divot3p = Helper.CreatePackageDivot(picInstruction, package3);
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1);
			var divot2d = Helper.CreatePackageDivot(dlvInstruction, package2);
			var divot3d = Helper.CreatePackageDivot(dlvInstruction, package3);
			AssertEquals("Precondition, there should be no confirmations.", 0, picInstruction.Confirmations.Count);

			var eDataObject = GetEvent(Events.PickedUpCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 2, "", "", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertEquals("Assigned to 2", 2, logParents.Length);
			AssertEquals("Split into 3", 3, picInstruction.Confirmations.Count);
			AssertEquals(1, ((DtbBookingConfirmation)logParents[0]).KK_Quantity);
			AssertEquals(ConfirmationTypes.Codes.PickUp, ((DtbBookingConfirmation)logParents[0]).KK_ConfirmationType);
			AssertEquals(1, ((DtbBookingConfirmation)logParents[1]).KK_Quantity);
			AssertEquals(ConfirmationTypes.Codes.PickUp, ((DtbBookingConfirmation)logParents[1]).KK_ConfirmationType);
			AssertContainsExactElementsInAnyOrder(new ZInt[] { 1, 1, 1 }, picInstruction.Confirmations.Select(c => c.KK_Quantity));
		}

		public void TestSplit_ALL_Confirmation_PacksWeightVolumeProportion()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CFS, null);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, null);
			var packingJob = Helper.CreatePackageJob(booking.ConsolidationSingleJob);
			var package1 = PackingHelper.CreatePackage(packingJob, "", 10, Constants.PkgUnit.Pallet);
			package1.KP_Weight = 10;
			package1.KP_Volume = 10;

			var divot1p = Helper.CreatePackageDivot(picInstruction, package1);
			var divot1d = Helper.CreatePackageDivot(dlvInstruction, package1);
			var picConfirmation = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);
			var dlvConfirmation = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			var inDataObject = GetEvent(Events.ArrivalCode, ZDateTime.Now, "TB00001234", "", "", "", "", Constants.EventReferenceParameterReasons.Pickup, 5, "PLT", "5.000 KG", "5.000 M3");
			var outDataObject = GetEvent(Events.DepartureCode, ZDateTime.Now, "TB00001234", "", "", "", "", Constants.EventReferenceParameterReasons.Pickup, 5, "PLT", "5.000 KG", "5.000 M3");
			var pupDataObject = GetEvent(Events.PickedUpCode, ZDateTime.Now, "TB00001234", "", "", "", "", "", 5, "PLT", "5.000 KG", "5.000 M3");
			var inLogParents = (DtbBookingConfirmation)GetNewEventParentFinder().GetLogParentsForEvent(inDataObject).FirstOrDefault();
			var outLogParents = (DtbBookingConfirmation)GetNewEventParentFinder().GetLogParentsForEvent(outDataObject).FirstOrDefault();
			var pupLogParents = (DtbBookingConfirmation)GetNewEventParentFinder().GetLogParentsForEvent(pupDataObject).FirstOrDefault();

			AssertNotNull(inLogParents);
			AssertNotNull(outLogParents);
			AssertNotNull(pupLogParents);

			AssertNotNull(inLogParents.PackageDivot);
			AssertNotNull(outLogParents.PackageDivot);
			AssertNotNull(pupLogParents.PackageDivot);

			AssertEquals(5, inLogParents.KK_Quantity);
			AssertEquals(5, outLogParents.KK_Quantity);
			AssertEquals(5, pupLogParents.KK_Quantity);

			AssertEquals(2, picInstruction.Confirmations.Count);
		}

		public void TestPickupCartageCompleteFinalised()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CTO, null);
			picInstruction.Address.E2_City = "Alexandria";
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CFS, null);
			dlvInstruction.Address.E2_City = "Botany";
			Factory.Save();

			var eDataObject = GetEvent(Events.PickupCartageCompleteFinalisedCode, ZDateTime.Now, "TB00001234", "", "", CargoWise.EventReference.Constants.Facilities.Code.Terminal, "Alexandria", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { picInstruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp) }, logParents);
		}

		public void TestPickupCartageCompleteFinalisedMulti()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi, LocalCartageJobOrgTypeList.Codes.CTO, null);
			picInstruction.Address.E2_City = "Alexandria";
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CFS, null);
			dlvInstruction.Address.E2_City = "Botany";
			Factory.Save();

			var eDataObject = GetEvent(Events.PickupCartageCompleteFinalisedCode, ZDateTime.Now, "TB00001234", "", "", CargoWise.EventReference.Constants.Facilities.Code.Terminal, "Alexandria", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { picInstruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.PickUp) }, logParents);
		}

		public void TestDeliveryCartageCompleteFinalised()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CTO, null);
			picInstruction.Address.E2_City = "Alexandria";
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CFS, null);
			dlvInstruction.Address.E2_City = "Botany";
			Factory.Save();

			var eDataObject = GetEvent(Events.DeliveryCartageCompleteFinalisedCode, ZDateTime.Now, "TB00001234", "", "", CargoWise.EventReference.Constants.Facilities.Code.Depot, "Botany", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { dlvInstruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery) }, logParents);
		}

		public void TestDeliveryCartageCompleteFinalisedMulti()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CTO, null);
			picInstruction.Address.E2_City = "Alexandria";
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi, LocalCartageJobOrgTypeList.Codes.CFS, null);
			dlvInstruction.Address.E2_City = "Botany";
			Factory.Save();

			var eDataObject = GetEvent(Events.DeliveryCartageCompleteFinalisedCode, ZDateTime.Now, "TB00001234", "", "", CargoWise.EventReference.Constants.Facilities.Code.Depot, "Botany", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { dlvInstruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery) }, logParents);
		}

		public void TestGateIn()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CTO, null);
			picInstruction.Address.E2_City = "Alexandria";
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CYD, null);
			dlvInstruction.Address.E2_City = "Botany";
			Factory.Save();

			var eDataObject = GetEvent(Events.GateInCode, ZDateTime.Now, "TB00001234", "", "", CargoWise.EventReference.Constants.Facilities.Code.ContainerYard, "Botany", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { dlvInstruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery) }, logParents);
		}

		public void TestCartageCompleteFinalise()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CTO, null);
			picInstruction.Address.E2_City = "Alexandria";
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CFS, null);
			dlvInstruction.Address.E2_City = "Botany";
			Factory.Save();

			var eDataObject = GetEvent(Events.CartageCompleteFinalisedCode, ZDateTime.Now, "TB00001234", "", "", CargoWise.EventReference.Constants.Facilities.Code.Depot, "Botany", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { dlvInstruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.Delivery) }, logParents);
		}

		public void TestServiceCommenced()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CTO, null);
			picInstruction.Address.E2_City = "Alexandria";
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CFS, null);
			dlvInstruction.Address.E2_City = "Botany";
			Factory.Save();

			var eDataObject = GetEvent(Events.ServiceCommencedCode, ZDateTime.Now, "TB00001234", "", "", CargoWise.EventReference.Constants.Facilities.Code.Depot, "Botany", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { dlvInstruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.ConNoteNo) }, logParents);
		}

		public void TestServiceCommenced_MultiDelivery()
		{
			var booking = CreateBookingWithConsolidation("TB00001234");
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, OrganisationTypesList.Codes.CFS, null);
			picInstruction.Address.E2_City = "Alexandria";
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);
			dlvInstruction.Address.E2_City = "Botany";
			var dlvInstruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, OrganisationTypesList.Codes.CNE, null);
			dlvInstruction2.Address.E2_City = "Mascot";
			Factory.Save();

			var eDataObject = GetEvent(Events.ServiceCommencedCode, ZDateTime.Now, "TB00001234", "", "", CargoWise.EventReference.Constants.Facilities.Code.Consignee, "Mascot", "");
			var logParents = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject);
			AssertContainsExactElementsInAnyOrder(new[] { dlvInstruction2.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.ConNoteNo) }, logParents);

			var eDataObject2 = GetEvent(Events.ServiceCommencedCode, ZDateTime.Now, "TB00001234", "", "", CargoWise.EventReference.Constants.Facilities.Code.Consignee, "Botany", "");
			var logParents2 = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject2);
			AssertContainsExactElementsInAnyOrder(new[] { dlvInstruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.ConNoteNo) }, logParents2);

			var eDataObject3 = GetEvent(Events.ServiceCommencedCode, ZDateTime.Now, "TB00001234", "", "", CargoWise.EventReference.Constants.Facilities.Code.Consignee, "Botany", "");
			var logParents3 = GetNewEventParentFinder().GetLogParentsForEvent(eDataObject2);
			AssertContainsExactElementsInAnyOrder("Clobber the previous value", new[] { dlvInstruction.Confirmations.Single(c => c.KK_ConfirmationType == ConfirmationTypes.Codes.ConNoteNo) }, logParents3);
		}

		UniversalDataBuss.DataObjects.Universal.Event GetEvent(ZString eventCode, ZDateTime eventTime, ZString bookingID, ZString instructionID, ZString packageID, ZString facility, ZString city, ZString reason)
		{
			return GetEvent(eventCode, eventTime, bookingID, instructionID, packageID, facility, city, reason, 0, "", "", "", "");
		}

		UniversalDataBuss.DataObjects.Universal.Event GetEvent(ZString eventCode, ZDateTime eventTime, ZString bookingID, ZString instructionID, ZString packageID, ZString facility, ZString city, ZString reason, ZInt packCount, ZString packType, ZString weight, ZString volume)
		{
			return GetEvent(eventCode, eventTime, bookingID, instructionID, packageID, facility, city, reason, packCount, packType, weight, volume, "");
		}

		UniversalDataBuss.DataObjects.Universal.Event GetEvent(ZString eventCode, ZDateTime eventTime, ZString bookingID, ZString instructionID, ZString packageID, ZString facility, ZString city, ZString reason, ZInt packCount, ZString packType, ZString weight, ZString volume, ZString containerNumber)
		{
			var result = new UniversalDataBuss.DataObjects.Universal.Event();
			result.EventType = eventCode;
			result.EventTime = eventTime.ToOffset();

			result.ContextCollection = new List<Context>();
			if (!bookingID.IsEmpty)
			{
				result.ContextCollection.Add(new Context() { Type = nameof(ContextTypes.TransportBookingJobID), Value = bookingID });
			}

			if (!instructionID.IsEmpty)
			{
				result.ContextCollection.Add(new Context() { Type = nameof(ContextTypes.TransportBookingInstructionID), Value = instructionID });
			}

			if (!packageID.IsEmpty)
			{
				result.ContextCollection.Add(new Context() { Type = nameof(ContextTypes.TransportBookingPackageID), Value = packageID });
			}

			if (!packCount.IsEmpty)
			{
				result.ContextCollection.Add(new Context() { Type = nameof(ContextTypes.NumberOfPieces), Value = packCount.ToString() });
			}

			if (!packType.IsEmpty)
			{
				result.ContextCollection.Add(new Context() { Type = nameof(ContextTypes.PackageType), Value = packType });
			}

			if (!weight.IsEmpty)
			{
				result.ContextCollection.Add(new Context() { Type = nameof(ContextTypes.WeightOfGoods), Value = weight });
			}

			if (!volume.IsEmpty)
			{
				result.ContextCollection.Add(new Context() { Type = nameof(ContextTypes.VolumeOfGoods), Value = volume });
			}

			if (!containerNumber.IsEmpty)
			{
				result.ContextCollection.Add(new Context() { Type = nameof(ContextTypes.ContainerNumber), Value = containerNumber });
			}

			result.EventParameters = new EventParameters();
			if (!facility.IsEmpty)
			{
				result.EventParameters.Facility = facility;
			}

			if (!city.IsEmpty)
			{
				result.EventParameters.Location = city;
			}

			if (!reason.IsEmpty)
			{
				result.EventParameters.Reason = reason;
			}

			return result;
		}

		DtbBooking CreateBookingWithConsolidation(ZString jobID)
		{
			var consolidation = Helper.CreateConsolidation();
			consolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;

			var booking = Helper.CreateBooking(consolidation);
			booking.KM_JobID = jobID;

			return booking;
		}

		const string universalEventXmlWithPackageID = @"
				<UniversalEvent>
					<Event>
						<EventType>DCF</EventType>
						<EventTime>10-JUL-2010 18:00</EventTime>
						<EventReference>Dummy Description</EventReference>
						<DataProvider>Dummy</DataProvider>
						<ContextCollection>
							<Context>
								<Type>TransportBookingJobID</Type>
								<Value>TB00000001</Value>
							</Context>
							<Context>
								<Type>TransportBookingPackageID</Type>
								<Value>PCK127890012943</Value>
							</Context>
						</ContextCollection>
					</Event>
				</UniversalEvent>";

		DtbBookingConfirmationEventParentFinder GetNewEventParentFinder()
		{
			return new DtbBookingConfirmationEventParentFinder(Factory, new DtbBookingConfirmationDataContextManager(), new DummyLogger());
		}

		internal static string GetTestEventXmlText(ZString bookingID, ZInt instructionSequence, ZString confirmationReceivedBy, ZString referenceNum)
		{
			string eventXmlText = @"
				<UniversalEvent>
					<Event>
						<EventType>CCD</EventType>
						<EventTime>10-JUL-2010 18:00</EventTime>
						<EventReference>Dummy Description</EventReference>
						<DataProvider>Dummy</DataProvider>
						<ContextCollection>
							<Context>
								<Type>TransportBookingJobID</Type>
								<Value>{0}</Value>
							</Context>
							<Context>
								<Type>TransportBookingInstructionID</Type>
								<Value>{1}</Value>
							</Context>
						</ContextCollection>

						<AdditionalFieldsToUpdateCollection>
							<AdditionalFieldsToUpdate>
								<Type>DtbBookingConfirmation.KK_ReceivedBy</Type>
								<Value>{2}</Value>
							</AdditionalFieldsToUpdate>
							<AdditionalFieldsToUpdate>
								<Type>DtbBookingConfirmation.KK_ReferenceNum</Type>
								<Value>{3}</Value>
							</AdditionalFieldsToUpdate>
						</AdditionalFieldsToUpdateCollection>
					</Event>
				</UniversalEvent>";
			return string.Format(eventXmlText, bookingID, instructionSequence, confirmationReceivedBy, referenceNum);
		}
	}
}
