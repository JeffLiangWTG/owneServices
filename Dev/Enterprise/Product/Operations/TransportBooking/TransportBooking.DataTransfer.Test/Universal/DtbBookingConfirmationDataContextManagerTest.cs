using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	[TestedType(typeof(DtbBookingConfirmationDataContextManager))]
	sealed class DtbBookingConfirmationDataContextManagerTest : DataContextManagerTestCase<DtbBookingConfirmationDataContextManager, DtbBookingConfirmation>
	{
		public void TestDataContextKey()
		{
			var confirmation = GetNewBusinessObjectForTesting();
			var manager = new DtbBookingConfirmationDataContextManager();
			((IDataContextManager)manager).Init(confirmation);

			AssertEquals(ZString.Empty, manager.DataContextKey);
		}

		public void TestEventContextValues_ConfirmationWithMultiplePackages()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertNull("Precondition", confirmation.PackageDivot);

			var container1 = Helper.CreatePackageContainer("CN1");
			var container2 = Helper.CreatePackageContainer("CN2");
			var container3 = Helper.CreatePackageContainer("CN3");
			var package = Helper.CreatePackage("P1");
			var divot1 = Helper.CreatePackageDivot(instruction, container1, 1);
			var divot2 = Helper.CreatePackageDivot(instruction, container2, 2);
			var divot3 = Helper.CreatePackageDivot(instruction, container3, 3);
			var divot4 = Helper.CreatePackageDivot(instruction, package, 4);
			AssertContainsExactElementsInAnyOrder(new[] { container1, container2, container3, package }, instruction.DivotsWithPackages.Packages);

			var manager = confirmation.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertMultilineASCIIEquals("manager.EventContextValues", @"
ContainerNumber - CN1
ContainerNumber - CN2
ContainerNumber - CN3".Trim(), manager.EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		public void TestEventContextValues_ConfirmationWithSinglePackage()
		{
			var consolidation = Helper.CreateConsolidation();
			var booking = consolidation.Bookings.AddNew();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertNull("Precondition", confirmation.PackageDivot);

			var containerOnConfirmation = Helper.CreatePackageContainer("CN1");
			var containerOnInstruction = Helper.CreatePackageContainer("CN2");
			var package = Helper.CreatePackageContainer("P1");
			var divot = Helper.CreatePackageDivot(instruction, containerOnConfirmation, 1);
			Helper.CreatePackageDivot(instruction, containerOnInstruction, 2);
			Helper.CreatePackageDivot(instruction, package, 3);
			confirmation.KK_KD_BookingInstructionPkgDivot = divot.PK;
			AssertContainsExactElementsInAnyOrder(new[] { containerOnConfirmation, package, containerOnInstruction }, instruction.DivotsWithPackages.Packages);

			var manager = confirmation.GetUniversalDataContextManager() as IEventDataContextManager;
			AssertMultilineASCIIEquals("manager.EventContextValues", @"
ContainerNumber - CN1".Trim(), manager.EventContextValues.ToStringContents(e => e.Key + " - " + e.Value));
		}

		public void TestManagesEvents()
		{
			var manager = new DtbBookingConfirmationDataContextManager();
			AssertEquals(true, manager.ManagesEvents);
		}

		public void TestOnUniversalEventAdded_PickedUp()
		{
			TestOnUniversalEventAdded(AutoEvents.PickedUpCode);
			TestOnUniversalEventAdded_IsEstimate_KK_Estimated_Set(AutoEvents.PickedUpCode);
		}

		public void TestOnUniversalEventAdded_Delivery()
		{
			TestOnUniversalEventAdded(AutoEvents.DeliveredCode);
			TestOnUniversalEventAdded_IsEstimate_KK_Estimated_Set(AutoEvents.DeliveredCode);
		}

		public void TestOnUniversalEventAdded_PickupCartageCompleteFinalisedCode()
		{
			TestOnUniversalEventAdded(AutoEvents.PickupCartageCompleteFinalisedCode);
			TestOnUniversalEventAdded_IsEstimate_KK_Actual_Set(AutoEvents.PickupCartageCompleteFinalisedCode);
		}

		public void TestOnUniversalEventAdded_DeliveryCartageCompleteFinalisedCode()
		{
			TestOnUniversalEventAdded(AutoEvents.DeliveryCartageCompleteFinalisedCode);
			TestOnUniversalEventAdded_IsEstimate_KK_Actual_Set(AutoEvents.DeliveryCartageCompleteFinalisedCode);
		}

		public void TestOnUniversalEventAdded_CartageCompleteFinalisedCode()
		{
			TestOnUniversalEventAdded(AutoEvents.CartageCompleteFinalisedCode);
			TestOnUniversalEventAdded_IsEstimate_KK_Actual_Set(AutoEvents.CartageCompleteFinalisedCode);
		}

		public void TestOnUniversalEventAdded_GateInCode()
		{
			var uEvent = new Event
			{
				EventType = AutoEvents.GateInCode,
				EventTime = ZDateTimeOffset.Now
			};

			var confirmation = Factory.New<DtbBookingConfirmation>();
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);

			var manager = (IEventDataContextManager)confirmation.GetUniversalDataContextManager();

			uEvent.EventParameters = new EventParameters();
			uEvent.EventParameters.Facility = null;

			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals(ZDateTime.Empty, confirmation.KK_Actual);

			uEvent.EventParameters.Facility = "XXX";

			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals(ZDateTime.Empty, confirmation.KK_Actual);

			uEvent.EventParameters.Facility = CargoWise.EventReference.Constants.Facilities.Code.ContainerYard;

			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals(uEvent.EventTime.GetValueOrDefault().ToZDateTime(), confirmation.KK_Actual);
		}

		public void TestOnUniversalEventAdded_GateInCode_IsEstimate()
		{
			var uEvent = new Event
			{
				EventType = AutoEvents.GateInCode,
				EventTime = ZDateTimeOffset.Now,
				IsEstimate = true
			};

			var confirmation = Factory.New<DtbBookingConfirmation>();
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);

			var manager = (IEventDataContextManager)confirmation.GetUniversalDataContextManager();

			uEvent.EventParameters = new EventParameters();
			uEvent.EventParameters.Facility = null;

			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals(ZDateTime.Empty, confirmation.KK_Estimated);
			AssertEquals(ZDateTime.Empty, confirmation.KK_Actual);

			uEvent.EventParameters.Facility = "XXX";

			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals(ZDateTime.Empty, confirmation.KK_Estimated);
			AssertEquals(ZDateTime.Empty, confirmation.KK_Actual);

			uEvent.EventParameters.Facility = CargoWise.EventReference.Constants.Facilities.Code.ContainerYard;

			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("Regression testing of existing functionality: KK_Estimated is not set", ZDateTime.Empty, confirmation.KK_Estimated);
			AssertEquals("Regression testing of existing functionality: KK_Actual is set", uEvent.EventTime.GetValueOrDefault().ToZDateTime(), confirmation.KK_Actual);
		}

		public void TestOnUniversalEventAdded_SlotConfirmed()
		{
			var uEvent = new Event
			{
				EventType = AutoEvents.SlotConfirmedCode,
				EventTime = ZDateTimeOffset.Now,
				EventReference = "123|FAC=CTO|LOC=Alexandria"
			};

			var confirmation = Factory.New<DtbBookingConfirmation>();
			var manager = confirmation.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals(ZDateTime.Empty, confirmation.KK_Actual);
			AssertEquals(uEvent.EventTime.GetValueOrDefault().ToZDateTime(), confirmation.KK_SlotDateTime);
			AssertEquals("123", confirmation.KK_ReferenceNum);

			uEvent = new Event
			{
				EventType = AutoEvents.SlotConfirmedCode,
				EventTime = ZDateTimeOffset.Now.AddDays(1),
				EventReference = "|FAC=CTO|LOC=Alexandria|RFN=131313"
			};
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("131313", confirmation.KK_ReferenceNum);
			AssertEquals(ZDateTime.Empty, confirmation.KK_Actual);
			AssertEquals(uEvent.EventTime.GetValueOrDefault().ToZDateTime(), confirmation.KK_SlotDateTime);
		}

		public void TestOnUniversalEventAdded_SlotRequested()
		{
			var uEvent = new Event
			{
				EventType = AutoEvents.SlotRequestedCode,
				EventTime = ZDateTimeOffset.Now,
				EventReference = "123|FAC=CTO|LOC=Alexandria"
			};

			var confirmation = Factory.New<DtbBookingConfirmation>();
			var manager = confirmation.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals(ZDateTime.Empty, confirmation.KK_Actual);
			AssertEquals(uEvent.EventTime.GetValueOrDefault().ToZDateTime(), confirmation.KK_SlotDateTime);
			AssertEquals("123", confirmation.KK_ReferenceNum);

			uEvent = new Event
			{
				EventType = AutoEvents.SlotRequestedCode,
				EventTime = ZDateTimeOffset.Now.AddDays(1),
				EventReference = "|FAC=CTO|LOC=Alexandria|RFN=131313"
			};
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("131313", confirmation.KK_ReferenceNum);
			AssertEquals(ZDateTime.Empty, confirmation.KK_Actual);
			AssertEquals(uEvent.EventTime.GetValueOrDefault().ToZDateTime(), confirmation.KK_SlotDateTime);
		}

		public void TestOnUniversalEventAdded_SlotCancelled()
		{
			var uEvent = new Event
			{
				EventType = AutoEvents.SlotCancelledCode,
				EventTime = ZDateTimeOffset.Now,
				EventReference = "123|FAC=CTO|LOC=Alexandria"
			};

			var confirmation = Factory.New<DtbBookingConfirmation>();
			var manager = confirmation.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals(ZDateTime.Empty, confirmation.KK_Actual);
			AssertEquals(ZDateTime.Empty, confirmation.KK_SlotDateTime);
			AssertEquals(ZString.Empty, confirmation.KK_ReferenceNum);

			uEvent = new Event
			{
				EventType = AutoEvents.SlotCancelledCode,
				EventTime = ZDateTimeOffset.Now.AddDays(1),
				EventReference = "|FAC=CTO|LOC=Alexandria|RFN=131313"
			};
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals(ZString.Empty, confirmation.KK_ReferenceNum);
			AssertEquals(ZDateTime.Empty, confirmation.KK_Actual);
			AssertEquals(ZDateTime.Empty, confirmation.KK_SlotDateTime);
		}

		public void TestOnUniversalEventAdded_ServiceCommenced()
		{
			var uEvent = new Event() { EventType = AutoEvents.ServiceCommencedCode };
			uEvent.EventTime = ZDateTimeOffset.Now;
			uEvent.EventReference = "12345|FAC=Terminal|LOC=Alexandria";
			var booking = Factory.New<DtbBooking>();
			Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var confirmation = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.ConNoteNo);
			var manager = confirmation.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("12345", dlvInstruction.ConNoteNo);
			AssertEquals("12345", confirmation.KK_ReferenceNum);
			AssertEquals("12345", booking.KM_TransportReference);

			var anotherUEvent = new Event() { EventType = AutoEvents.ServiceCommencedCode };
			anotherUEvent.EventTime = ZDateTimeOffset.Now;
			anotherUEvent.EventReference = "NewID|FAC=Terminal|LOC=Alexandria";
			manager.OnUniversalEventAdded(messageLogger, anotherUEvent);
			AssertEquals("NewID", dlvInstruction.ConNoteNo);
			AssertEquals("NewID", confirmation.KK_ReferenceNum);
			AssertEquals("NewID", booking.KM_TransportReference);

			var largeUEvent = new Event() { EventType = AutoEvents.ServiceCommencedCode };
			largeUEvent.EventTime = ZDateTimeOffset.Now;
			largeUEvent.EventReference = "ThisIsAReallyLongReferenceThatWontFitToAnyOfTheFields|FAC=Terminal|LOC=Alexandria";
			manager.OnUniversalEventAdded(messageLogger, largeUEvent);
			AssertEquals("Max Length for reference number is 50 characters", "ThisIsAReallyLongReferenceThatWontFitToAnyOfTheFie", dlvInstruction.ConNoteNo);
			AssertEquals("Max Length for reference number is 50 characters", "ThisIsAReallyLongReferenceThatWontFitToAnyOfTheFie", confirmation.KK_ReferenceNum);
			AssertEquals("Max Length for transport reference number is 35 characters", "ThisIsAReallyLongReferenceThatWontF", booking.KM_TransportReference);

			booking.KM_TransportReference = ""; // clear
			var uEvent2 = new Event() { EventType = AutoEvents.ServiceCommencedCode };
			uEvent2.EventTime = ZDateTimeOffset.Now;
			uEvent2.EventReference = "u2|FAC=Terminal|LOC=Alexandria";
			var dlvInstruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var confirmation2 = Helper.CreateConfirmation(dlvInstruction2, ConfirmationTypes.Codes.ConNoteNo);
			var manager2 = confirmation2.GetUniversalDataContextManager() as IEventDataContextManager;
			manager2.OnUniversalEventAdded(messageLogger, uEvent2);
			AssertEquals("Max Length for reference number is 50 characters", "ThisIsAReallyLongReferenceThatWontFitToAnyOfTheFie", dlvInstruction.ConNoteNo);
			AssertEquals("Max Length for reference number is 50 characters", "ThisIsAReallyLongReferenceThatWontFitToAnyOfTheFie", confirmation.KK_ReferenceNum);
			AssertEquals("u2", dlvInstruction2.ConNoteNo);
			AssertEquals("u2", confirmation2.KK_ReferenceNum);
			AssertEquals("Don't update Booking Transport Reference if there are multiple bookings", "", booking.KM_TransportReference);
		}

		public void TestOnUniversalEventAdded_ServiceCommenced_EndToEnd()
		{
			var booking = Helper.CreateBooking();
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var confirmation = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.ConNoteNo);
			AssertEquals("Precondition.", TransportStatuses.Codes.Available, booking.KM_Status);
			Factory.SaveForTesting();

			var uEvent = new Event();
			uEvent.EventType = Events.ServiceCommencedCode;
			uEvent.EventTime = ZDateTimeOffset.Now;
			uEvent.EventReference = "12345|FAC=Terminal|LOC=Alexandria";
			uEvent.DataContext = DataContextFactory.New();
			uEvent.DataContext.AddDataTarget(DataContextType.TransportBookingConfirmation, null);
			uEvent.ContextCollection = new List<Context>
			{
				new Context() { Type = nameof(Event.ContextTypes.TransportBookingJobID), Value = booking.KM_JobID },
				new Context() { Type = nameof(Event.ContextTypes.TransportBookingInstructionID), Value = "2" }
			};

			var manager = new UniversalMessageProcessingManager(new ServiceTaskLogForTesting());
			var message = GetQueuedUniversalEventMessage(string.Empty);
			manager.Process(message, uEvent);
			AssertEquals("Should have created Service Commenced log.", true, confirmation.Logs.HasLogWith(l => l.SL_SE_NKEvent == Events.ServiceCommencedCode));
			AssertEquals("12345", dlvInstruction.ConNoteNo);
			AssertEquals("12345", confirmation.KK_ReferenceNum);
			AssertEquals("12345", booking.KM_TransportReference);
			AssertEquals(TransportStatuses.Codes.ServiceCommenced, booking.KM_Status);

			picInstruction.KN_Status = TransportStatuses.Codes.PickedUp;
			booking.UpdateStatus();
			AssertEquals("Precondition.", TransportStatuses.Codes.PickedUp, booking.KM_Status);
			manager.Process(message, uEvent);
			AssertEquals("Status should *not* have been demoted to Service Commenced.", TransportStatuses.Codes.PickedUp, booking.KM_Status);
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		public void TestOnUniversalEventAdded_SetReference_Issue00822236()
		{
			var uEvent = new Event() { EventType = AutoEvents.SlotConfirmedCode };
			uEvent.EventTime = ZDateTimeOffset.Now;
			uEvent.EventReference = "1234|FAC=Terminal|LOC=JONESTOWN|RES=Pickup|RES=Pickup||";

			var confirmation = Factory.New<DtbBookingConfirmation>();
			var manager = confirmation.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			AssertNoExceptionThrown(() => manager.OnUniversalEventAdded(messageLogger, uEvent));
		}

		public void TestAdditionalFieldsInEventUpdatesConfirmation()
		{
			var helper = new TransportBookingTestHelper(Factory.BOFactory);
			var bookingConsolidation = helper.CreateConsolidation();
			var booking = helper.CreateBooking(bookingConsolidation);
			booking.KM_JobID = "TB00009231";
			var bookingInstructionPic = helper.CreateInstruction(booking, "PIC");
			bookingInstructionPic.KN_Sequence = 1;
			var bookingInstructionDlv = helper.CreateInstruction(booking, "DLV");
			bookingInstructionDlv.KN_Sequence = 2;

			var bookingConfirmationPic = helper.CreateConfirmation(instruction: bookingInstructionPic, confirmationTypeCode: "PIC");
			var bookingConfirmationDlv = helper.CreateConfirmation(instruction: bookingInstructionDlv, confirmationTypeCode: "DLV");

			var packageJob = helper.CreatePackageJob(bookingConsolidation);

			var package = helper.CreatePackage("PKG001", 1, "PKG");
			package.KP_Sequence = 1;
			package.KP_KJ_ParentPackageJob = packageJob.PK;

			var packageDivotPic = helper.CreatePackageDivot(bookingInstructionPic, package, 1);
			var packageDivotDlv = helper.CreatePackageDivot(bookingInstructionDlv, package, 1);

			Factory.SaveForTesting();
			var additionalFieldsEvent = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.AdditionalFields - Event Updates Confirmation Fields.xml");
			var message = GetQueuedUniversalEventMessage(additionalFieldsEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			var matchedConfirmations = Factory.Load<DtbBookingConfirmation>(new ZQuery(DtbBookingConfirmationSchema.PK, bookingConfirmationDlv.PK));
			CombineAssertions("Additional Fields should have updated fields on DtbBookingConfirmation",
				() =>
				{
					AssertEquals("1 matching confirmation record", 1, matchedConfirmations.Length);
					AssertEquals(new ZDateTime(2021, 7, 31, 12, 0, 0), matchedConfirmations[0].KK_Actual);
					AssertEquals(new ZDateTime(2021, 7, 31, 11, 0, 0), matchedConfirmations[0].KK_Estimated);
					AssertEquals("Sam", matchedConfirmations[0].KK_ReceivedBy);
				});
		}

		public void TestAdditionalFieldsInEventDoesNotUpdateConfirmationWithDataTargetInMessage()
		{
			var helper = new TransportBookingTestHelper(Factory.BOFactory);
			var bookingConsolidation = helper.CreateConsolidation();
			var booking = helper.CreateBooking(bookingConsolidation);
			booking.KM_JobID = "TB00009231";
			var bookingInstructionPic = helper.CreateInstruction(booking, "PIC");
			bookingInstructionPic.KN_Sequence = 1;
			var bookingInstructionDlv = helper.CreateInstruction(booking, "DLV");
			bookingInstructionDlv.KN_Sequence = 2;

			var bookingConfirmationPic = helper.CreateConfirmation(instruction: bookingInstructionPic, confirmationTypeCode: "PIC");
			var bookingConfirmationDlv = helper.CreateConfirmation(instruction: bookingInstructionDlv, confirmationTypeCode: "DLV");

			var packageJob = helper.CreatePackageJob(bookingConsolidation);

			var package = helper.CreatePackage("PKG001", 1, "PKG");
			package.KP_Sequence = 1;
			package.KP_KJ_ParentPackageJob = packageJob.PK;

			var packageDivotPic = helper.CreatePackageDivot(bookingInstructionPic, package, 1);
			var packageDivotDlv = helper.CreatePackageDivot(bookingInstructionDlv, package, 1);

			Factory.SaveForTesting();

			var additionalFieldsEvent = resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.AdditionalFields - Event Does Not Update Confirmation Fields.xml");
			var message = GetQueuedUniversalEventMessage(additionalFieldsEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			var matchedConfirmations = Factory.Load<DtbBookingConfirmation>(new ZQuery(DtbBookingConfirmationSchema.PK, bookingConfirmationDlv.PK));
			CombineAssertions("Additional Fields should not have updated fields on DtbBookingConfirmation with DataTargetCollection in message",
				() =>
				{
					AssertEquals("1 matching confirmation record", 1, matchedConfirmations.Length);
					AssertEquals(ZDateTime.Empty, matchedConfirmations[0].KK_Actual);
					AssertEquals(ZDateTime.Empty, matchedConfirmations[0].KK_Estimated);
					AssertEquals(ZString.Empty, matchedConfirmations[0].KK_ReceivedBy);
				});
		}

		public void TestPickedUpTransform_IsEstimate_ActualTimeExists_EventIsNotTransformed()
		{
			var helper = new TransportBookingTestHelper(Factory.BOFactory);
			var bookingConsolidation = helper.CreateConsolidation();
			var booking = helper.CreateBooking(bookingConsolidation);
			booking.KM_JobID = "TB00009231";
			var bookingInstructionPic = helper.CreateInstruction(booking, "PIC");
			bookingInstructionPic.KN_Sequence = 1;
			var bookingInstructionDlv = helper.CreateInstruction(booking, "DLV");
			bookingInstructionDlv.KN_Sequence = 2;

			var bookingConfirmationPic = helper.CreateConfirmation(instruction: bookingInstructionPic, confirmationTypeCode: "PIC");
			helper.CreateConfirmation(instruction: bookingInstructionDlv, confirmationTypeCode: "DLV");

			var packageJob = helper.CreatePackageJob(bookingConsolidation);

			var package = helper.CreatePackage("PKG001", 1, "PKG");
			package.KP_Sequence = 1;
			package.KP_KJ_ParentPackageJob = packageJob.PK;

			helper.CreatePackageDivot(bookingInstructionPic, package, 1);
			helper.CreatePackageDivot(bookingInstructionDlv, package, 1);

			// Set KK_Actual
			bookingConfirmationPic.KK_Actual = new ZDateTime(2021, 1, 31, 12, 0, 0);

			Factory.SaveForTesting();

			var pickedUpQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PickedUpCode);
			var statusUpdatedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdatedCode);

			var pickedUpQueryLogs = bookingConfirmationPic.Logs.Find(pickedUpQuery);
			AssertEquals($"One PickedUp event should be added to {bookingConfirmationPic.HumanReadableName}", 1, pickedUpQueryLogs.Length);
			var statusUpdatedLogs = bookingConfirmationPic.Logs.Find(statusUpdatedQuery);
			AssertEquals($"No StatusUpdated event was added to {bookingConfirmationPic.HumanReadableName}", 0, statusUpdatedLogs.Length);
			AssertEquals("Prerequisite", new ZDateTime(2021, 1, 31, 12, 0, 0), bookingConfirmationPic.KK_Actual);
			AssertEquals("Prerequisite", ZDateTime.Empty, bookingConfirmationPic.KK_Estimated);

			var message = GetQueuedUniversalEventMessage(PUPEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			pickedUpQueryLogs = bookingConfirmationPic.Logs.Find(pickedUpQuery);
			// One event from KK_Actual being set prior to message processing, one event from imported message, and one event from KK_Estimated being set
			AssertEquals($"Two PickedUp events were added to {bookingConfirmationPic.HumanReadableName} since KK_Actual was set (one caused by update to KK_Estimated field)", 3, pickedUpQueryLogs.Length);
			statusUpdatedLogs = bookingConfirmationPic.Logs.Find(statusUpdatedQuery);
			AssertEquals($"No StatusUpdated event should be added to {bookingConfirmationPic.HumanReadableName}", 0, statusUpdatedLogs.Length);

			var bookingConfirmationPic_Reloaded = Factory.Load<DtbBookingConfirmation>(bookingConfirmationPic.PK);
			AssertEquals("KK_Actual unchanged", new ZDateTime(2021, 1, 31, 12, 0, 0), bookingConfirmationPic_Reloaded.KK_Actual);
			AssertEquals("KK_Estimated changed", new ZDateTime(2021, 7, 31, 12, 0, 0), bookingConfirmationPic_Reloaded.KK_Estimated);
		}

		public void TestPickedUpTransform_IsEstimate_ActualTimeDoesNotExist_EventIsNotTransformed()
		{
			var helper = new TransportBookingTestHelper(Factory.BOFactory);
			var bookingConsolidation = helper.CreateConsolidation();
			var booking = helper.CreateBooking(bookingConsolidation);
			booking.KM_JobID = "TB00009231";
			var bookingInstructionPic = helper.CreateInstruction(booking, "PIC");
			bookingInstructionPic.KN_Sequence = 1;
			var bookingInstructionDlv = helper.CreateInstruction(booking, "DLV");
			bookingInstructionDlv.KN_Sequence = 2;

			var bookingConfirmationPic = helper.CreateConfirmation(instruction: bookingInstructionPic, confirmationTypeCode: "PIC");
			helper.CreateConfirmation(instruction: bookingInstructionDlv, confirmationTypeCode: "DLV");

			var packageJob = helper.CreatePackageJob(bookingConsolidation);

			var package = helper.CreatePackage("PKG001", 1, "PKG");
			package.KP_Sequence = 1;
			package.KP_KJ_ParentPackageJob = packageJob.PK;

			helper.CreatePackageDivot(bookingInstructionPic, package, 1);
			helper.CreatePackageDivot(bookingInstructionDlv, package, 1);

			// Do not set KK_Actual

			Factory.SaveForTesting();

			var pickedUpQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.PickedUpCode);
			var statusUpdatedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdatedCode);

			var pickedUpQueryLogs = bookingConfirmationPic.Logs.Find(pickedUpQuery);
			AssertEquals($"No PickedUp event should be added to {bookingConfirmationPic.HumanReadableName}", 0, pickedUpQueryLogs.Length);
			var statusUpdatedLogs = bookingConfirmationPic.Logs.Find(statusUpdatedQuery);
			AssertEquals($"No StatusUpdated event was added to {bookingConfirmationPic.HumanReadableName}", 0, statusUpdatedLogs.Length);
			AssertEquals("Prerequisite", ZDateTime.Empty, bookingConfirmationPic.KK_Actual);
			AssertEquals("Prerequisite", ZDateTime.Empty, bookingConfirmationPic.KK_Estimated);

			var message = GetQueuedUniversalEventMessage(PUPEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			pickedUpQueryLogs = bookingConfirmationPic.Logs.Find(pickedUpQuery);
			AssertEquals($"Two PickedUp events were added to {bookingConfirmationPic.HumanReadableName} (one caused by update to KK_Estimated field)", 2, pickedUpQueryLogs.Length);
			statusUpdatedLogs = bookingConfirmationPic.Logs.Find(statusUpdatedQuery);
			AssertEquals($"No StatusUpdated event should be added to {bookingConfirmationPic.HumanReadableName}", 0, statusUpdatedLogs.Length);

			var bookingConfirmationPic_Reloaded = Factory.Load<DtbBookingConfirmation>(bookingConfirmationPic.PK);
			AssertEquals("KK_Actual unchanged", ZDateTime.Empty, bookingConfirmationPic_Reloaded.KK_Actual);
			AssertEquals("KK_Estimated changed", new ZDateTime(2021, 7, 31, 12, 0, 0), bookingConfirmationPic_Reloaded.KK_Estimated);
		}

		public void TestDeliveryTransform_IsEstimate_ActualTimeExists_EventIsNotTransformed()
		{
			var helper = new TransportBookingTestHelper(Factory.BOFactory);
			var bookingConsolidation = helper.CreateConsolidation();
			var booking = helper.CreateBooking(bookingConsolidation);
			booking.KM_JobID = "TB00009231";
			var bookingInstructionPic = helper.CreateInstruction(booking, "PIC");
			bookingInstructionPic.KN_Sequence = 1;
			var bookingInstructionDlv = helper.CreateInstruction(booking, "DLV");
			bookingInstructionDlv.KN_Sequence = 2;

			helper.CreateConfirmation(instruction: bookingInstructionPic, confirmationTypeCode: "PIC");
			var bookingConfirmationDlv = helper.CreateConfirmation(instruction: bookingInstructionDlv, confirmationTypeCode: "DLV");

			var packageJob = helper.CreatePackageJob(bookingConsolidation);

			var package = helper.CreatePackage("PKG001", 1, "PKG");
			package.KP_Sequence = 1;
			package.KP_KJ_ParentPackageJob = packageJob.PK;

			helper.CreatePackageDivot(bookingInstructionPic, package, 1);
			helper.CreatePackageDivot(bookingInstructionDlv, package, 1);

			// Set KK_Actual
			bookingConfirmationDlv.KK_Actual = new ZDateTime(2021, 1, 31, 12, 0, 0);

			Factory.SaveForTesting();

			var deliveredQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeliveredCode);
			var statusUpdatedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdatedCode);

			var deliveredQueryLogs = bookingConfirmationDlv.Logs.Find(deliveredQuery);
			AssertEquals($"One Delivered event should be added to {bookingConfirmationDlv.HumanReadableName}", 1, deliveredQueryLogs.Length);
			var statusUpdatedLogs = bookingConfirmationDlv.Logs.Find(statusUpdatedQuery);
			AssertEquals($"No StatusUpdated event was added to {bookingConfirmationDlv.HumanReadableName}", 0, statusUpdatedLogs.Length);
			AssertEquals("Prerequisite", new ZDateTime(2021, 1, 31, 12, 0, 0), bookingConfirmationDlv.KK_Actual);
			AssertEquals("Prerequisite", ZDateTime.Empty, bookingConfirmationDlv.KK_Estimated);

			var message = GetQueuedUniversalEventMessage(DLVEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			deliveredQueryLogs = bookingConfirmationDlv.Logs.Find(deliveredQuery);
			// One event from KK_Actual being set prior to message processing, one event from imported message, and one event from KK_Estimated being set
			AssertEquals($"Two Delivered events were added to {bookingConfirmationDlv.HumanReadableName} since KK_Actual was set (one caused by update to KK_Estimated field)", 3, deliveredQueryLogs.Length);
			statusUpdatedLogs = bookingConfirmationDlv.Logs.Find(statusUpdatedQuery);
			AssertEquals($"No StatusUpdated event should be added to {bookingConfirmationDlv.HumanReadableName}", 0, statusUpdatedLogs.Length);

			var bookingConfirmationDlv_Reloaded = Factory.Load<DtbBookingConfirmation>(bookingConfirmationDlv.PK);
			AssertEquals("KK_Actual unchanged", new ZDateTime(2021, 1, 31, 12, 0, 0), bookingConfirmationDlv_Reloaded.KK_Actual);
			AssertEquals("KK_Estimated changed", new ZDateTime(2021, 7, 31, 12, 0, 0), bookingConfirmationDlv_Reloaded.KK_Estimated);
		}

		public void TestDeliveryTransform_IsEstimate_ActualTimeDoesNotExist_EventIsNotTransformed()
		{
			var helper = new TransportBookingTestHelper(Factory.BOFactory);
			var bookingConsolidation = helper.CreateConsolidation();
			var booking = helper.CreateBooking(bookingConsolidation);
			booking.KM_JobID = "TB00009231";
			var bookingInstructionPic = helper.CreateInstruction(booking, "PIC");
			bookingInstructionPic.KN_Sequence = 1;
			var bookingInstructionDlv = helper.CreateInstruction(booking, "DLV");
			bookingInstructionDlv.KN_Sequence = 2;

			helper.CreateConfirmation(instruction: bookingInstructionPic, confirmationTypeCode: "PIC");
			var bookingConfirmationDlv = helper.CreateConfirmation(instruction: bookingInstructionDlv, confirmationTypeCode: "DLV");

			var packageJob = helper.CreatePackageJob(bookingConsolidation);

			var package = helper.CreatePackage("PKG001", 1, "PKG");
			package.KP_Sequence = 1;
			package.KP_KJ_ParentPackageJob = packageJob.PK;

			helper.CreatePackageDivot(bookingInstructionPic, package, 1);
			helper.CreatePackageDivot(bookingInstructionDlv, package, 1);

			// Do not set KK_Actual

			Factory.SaveForTesting();

			var deliveredQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeliveredCode);
			var statusUpdatedQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdatedCode);

			var deliveredQueryLogs = bookingConfirmationDlv.Logs.Find(deliveredQuery);
			AssertEquals($"No Delivered event should be added to {bookingConfirmationDlv.HumanReadableName}", 0, deliveredQueryLogs.Length);
			var statusUpdatedLogs = bookingConfirmationDlv.Logs.Find(statusUpdatedQuery);
			AssertEquals($"No StatusUpdated event was added to {bookingConfirmationDlv.HumanReadableName}", 0, statusUpdatedLogs.Length);
			AssertEquals("Prerequisite", ZDateTime.Empty, bookingConfirmationDlv.KK_Actual);
			AssertEquals("Prerequisite", ZDateTime.Empty, bookingConfirmationDlv.KK_Estimated);

			var message = GetQueuedUniversalEventMessage(DLVEvent);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			Factory.SaveForTesting();

			AssertEquals("Prerequisite: message processed", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			deliveredQueryLogs = bookingConfirmationDlv.Logs.Find(deliveredQuery);
			AssertEquals($"Two Delivered events were added to {bookingConfirmationDlv.HumanReadableName} (one caused by update to KK_Estimated field)", 2, deliveredQueryLogs.Length);
			statusUpdatedLogs = bookingConfirmationDlv.Logs.Find(statusUpdatedQuery);
			AssertEquals($"No StatusUpdated event should be added to {bookingConfirmationDlv.HumanReadableName}", 0, statusUpdatedLogs.Length);

			var bookingConfirmationDlv_Reloaded = Factory.Load<DtbBookingConfirmation>(bookingConfirmationDlv.PK);
			AssertEquals("KK_Actual unchanged", ZDateTime.Empty, bookingConfirmationDlv_Reloaded.KK_Actual);
			AssertEquals("KK_Estimated changed", new ZDateTime(2021, 7, 31, 12, 0, 0), bookingConfirmationDlv_Reloaded.KK_Estimated);
		}

		protected override DtbBookingConfirmation GetNewBusinessObjectForTesting() => Factory.New<DtbBookingConfirmation>();

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		string PUPEvent => resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.Event Type PUP.xml");

		string DLVEvent => resourceRetriever.Value.GetString("Enterprise.TransportBookings.DataTransfer.Test.Universal.Testing.Event Type DLV.xml");

		TransportBookingTestHelper Helper => helper ?? (helper = new TransportBookingTestHelper(Factory.BOFactory));

		TransportBookingTestHelper helper;

		void TestOnUniversalEventAdded(ZString eventType)
		{
			var uEvent = new Event { EventType = eventType };
			uEvent.EventTime = ZDateTimeOffset.Now;

			var confirmation = Factory.New<DtbBookingConfirmation>();
			var manager = confirmation.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals(uEvent.EventTime.GetValueOrDefault().ToZDateTime(), confirmation.KK_Actual);
		}

		void TestOnUniversalEventAdded_IsEstimate_KK_Estimated_Set(ZString eventType)
		{
			var uEvent = new Event
			{
				EventType = eventType,
				EventTime = ZDateTimeOffset.Now,
				IsEstimate = true
			};

			var confirmation = Factory.New<DtbBookingConfirmation>();
			var manager = confirmation.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			AssertEquals("Prerequisite", ZDateTime.Empty, confirmation.KK_Estimated);
			AssertEquals("Prerequisite", ZDateTime.Empty, confirmation.KK_Actual);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("Should set KK_Estimated", uEvent.EventTime.GetValueOrDefault().ToZDateTime(), confirmation.KK_Estimated);
			AssertEquals("Should not set KK_Actual", ZDateTime.Empty, confirmation.KK_Actual);
		}

		void TestOnUniversalEventAdded_IsEstimate_KK_Actual_Set(ZString eventType)
		{
			var uEvent = new Event
			{
				EventType = eventType,
				EventTime = ZDateTimeOffset.Now,
				IsEstimate = true
			};

			var confirmation = Factory.New<DtbBookingConfirmation>();
			var manager = confirmation.GetUniversalDataContextManager() as IEventDataContextManager;
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			AssertEquals("Prerequisite", ZDateTime.Empty, confirmation.KK_Estimated);
			AssertEquals("Prerequisite", ZDateTime.Empty, confirmation.KK_Actual);
			manager.OnUniversalEventAdded(messageLogger, uEvent);
			AssertEquals("Regression testing of existing functionality: Should not set KK_Estimated", ZDateTime.Empty, confirmation.KK_Estimated);
			AssertEquals("Regression testing of existing functionality: Should set KK_Actual", uEvent.EventTime.GetValueOrDefault().ToZDateTime(), confirmation.KK_Actual);
		}
	}
}
