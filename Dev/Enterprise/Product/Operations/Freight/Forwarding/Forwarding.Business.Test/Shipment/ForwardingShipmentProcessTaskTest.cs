using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentProcessTask))]
	sealed class ForwardingShipmentProcessTaskTest : RoutingSupportProcessTaskTest<ForwardingShipment>
	{
		public void TestProxiedFieldsWithoutParent()
		{
			ArrivalMilestone.P9_ParentID = ZGuid.Empty;
			AssertNotNull("should not throw exception", ArrivalMilestone.ETA);
			AssertNotNull("should not throw exception", ArrivalMilestone.ETD);
			AssertNotNull("should not throw exception", ArrivalMilestone.LoadOrOriginPort);
			AssertNotNull("should not thorw exception", ArrivalMilestone.DischargeOrDestinationPort);
		}

		public void TestPickupCartageAdvised_UpdatesDocsAndCartage()
		{
			Milestone.TriggerConditions.TriggerEventCode = Events.PickupCartageAdvised.Code;
			Milestone.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 1));
			AssertEquals(new ZDateTime(2000, 1, 1), Shipment.DocsAndCartage.JP_PickupCartageAdvised);
		}

		public void TestDeliveryCartageAdvised_UpdatesDocsAndCartage()
		{
			Milestone.TriggerConditions.TriggerEventCode = Events.DeliveryCartageAdvised.Code;
			Milestone.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 1));
			AssertEquals(new ZDateTime(2000, 1, 1), Shipment.DocsAndCartage.JP_DeliveryCartageAdvised);
		}

		public void TestMatchesTriggerAction_ForXMFTrigger()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			JobShipmentPreplanning preplanning = Factory.New<JobShipmentPreplanning>();
			preplanning.EF_JS = shipment.PK;

			ProcessTask consolTrigger = ((IWorkflowProvider)consol).WorkflowItems.Triggers.AddNew();
			ProcessTask shipmentTrigger = shipment.WorkflowItems.Triggers.AddNew();
			ProcessTask preplanningTrigger = preplanning.WorkflowItems.Triggers.AddNew();

			AssertEquals(false, shipmentTrigger.MatchesTriggerAction(preplanningTrigger));
			AssertEquals(false, preplanningTrigger.MatchesTriggerAction(shipmentTrigger));
			AssertEquals(false, shipmentTrigger.MatchesTriggerAction(consolTrigger));
			AssertEquals(false, consolTrigger.MatchesTriggerAction(shipmentTrigger));

			ProcessTaskNotification consolNotification = consolTrigger.ProcessTaskNotifications.AddNew();
			consolNotification.PQ_TriggerType = "XMF";
			ProcessTaskNotification shipmentNotification = shipmentTrigger.ProcessTaskNotifications.AddNew();
			shipmentNotification.PQ_TriggerType = "XMF";
			ProcessTaskNotification preplanningNotification = preplanningTrigger.ProcessTaskNotifications.AddNew();
			preplanningNotification.PQ_TriggerType = "XMF";

			AssertEquals("Shipment matches shipment", true, shipmentTrigger.MatchesTriggerAction(shipmentTrigger));
			AssertEquals("Shipment matches attached preadvice", true, shipmentTrigger.MatchesTriggerAction(preplanningTrigger));
			AssertEquals("Preadvice matches attached shipment", true, preplanningTrigger.MatchesTriggerAction(shipmentTrigger));
			AssertEquals("Shipment matches attached consol", true, shipmentTrigger.MatchesTriggerAction(consolTrigger));
			AssertEquals("Consol matches attached shipment", true, consolTrigger.MatchesTriggerAction(shipmentTrigger));
		}

		#region Transport Legs

		[TestDate(2019, 1, 1)]
		public void TestP9_SE_NKMilestoneEvent_UpdatesDepartureDateWhenSet()
		{
			Shipment.JS_E_DEP = new ZDateTime(2020, 1, 1);
			ProcessTask departure = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			departure.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			AssertEquals("ETD populated", Shipment.JS_E_DEP, departure.P9_ScheduledDate.ToZDateTime());
		}

		[TestDate(2019, 1, 1)]
		public void TestP9_SE_NKMilestoneEvent_UpdatesArrivalDateWhenSet()
		{
			Shipment.JS_E_ARV = new ZDateTime(2020, 2, 2);
			ProcessTask arrival = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			arrival.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			AssertEquals("ETA populated", Shipment.JS_E_ARV, arrival.P9_ScheduledDate.ToZDateTime());
		}

		public void TestTransportLinkPopulated_ForDepartureEvent()
		{
			Shipment.JS_RL_NKOrigin = OriginTransport.JW_RL_NKLoadPort;
			Shipment.JS_RL_NKDestination = DestinationTransport.JW_RL_NKDiscPort;
			DepartureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			AssertEquals("Departure event should attach origin transport", OriginTransport.PK, DepartureMilestone.P9_ReferencedID);
		}

		public void TestTransportLinkPopulated_ForGateInEvent()
		{
			Shipment.JS_RL_NKOrigin = OriginTransport.JW_RL_NKLoadPort;
			Shipment.JS_RL_NKDestination = DestinationTransport.JW_RL_NKDiscPort;
			DepartureMilestone.TriggerConditions.TriggerEventCode = Events.GateIn.Code;
			AssertEquals("Gate In Wharf event should attach origin transport", OriginTransport.PK, DepartureMilestone.P9_ReferencedID);
		}

		public void TestTransportLinkPopulated_ForArrivalEvent()
		{
			Shipment.JS_RL_NKOrigin = OriginTransport.JW_RL_NKLoadPort;
			Shipment.JS_RL_NKDestination = DestinationTransport.JW_RL_NKDiscPort;
			ArrivalMilestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			AssertEquals("Departure event should attach origin transport", DestinationTransport.PK, ArrivalMilestone.P9_ReferencedID);
		}

		#endregion

		#region Closing Preadvice AllImportDocumentsReceived Exceptions

		public void TestClosingShipmentAIDExceptionClosesPreadviceAIDException()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ProcessTask shipmentException = shipment.WorkflowItems.Exceptions.AddNew();
			shipmentException.TriggerConditions.TriggerEventCode = Events.AllImportDocumentsReceived.Code;

			JobShipmentPreplanning preadvice = Factory.New<JobShipmentPreplanning>();
			preadvice.EF_JS = shipment.PK;
			ProcessTask preadviceException = preadvice.WorkflowItems.Exceptions.AddNew();
			preadviceException.TriggerConditions.TriggerEventCode = Events.AllImportDocumentsReceived.Code;

			AssertEquals("Preadvice milestone not actioned initially", false, preadviceException.IsClosed);
			shipmentException.IsExceptionActioned = true;
			AssertEquals("Preadvice milestone actioned", true, preadviceException.IsExceptionActioned);
		}

		#endregion

		#region Defaulting Estimate from Available / Storage

		public void TestEstimateDefaultedFromDepartureConsolLoadingETD()
		{
			SetupDepartureArrivalConsols_ForTest();
			DepartureConsol.Transports.DepartureTransport.JW_ETD = ZDateTime.SmallDateTimeNow;

			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultedFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolLoadingETD;
			Factory.Save();
			AssertEquals("Estimated date defaulted from Origin Load ETD", DepartureConsol.Transports.DepartureTransport.JW_ETD, milestone.P9_ScheduledDate.ToZDateTime());
		}

		public void TestEstimateDefaultedFromArrivalConsolDischargeETA()
		{
			SetupDepartureArrivalConsols_ForTest();
			ArrivalConsol.Transports.ArrivalTransport.JW_ETA = ZDateTime.SmallDateTimeNow;

			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultedFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ConsolDischargeETA;
			Factory.Save();
			AssertEquals("Estimated date defaulted from Destination Load ETA", ArrivalConsol.Transports.ArrivalTransport.JW_ETA, milestone.P9_ScheduledDate.ToZDateTime());
		}

		public void TestEstimateDefaultedFromShipmentETD()
		{
			SetupDepartureArrivalConsols_ForTest();
			Shipment.JS_E_DEP = ZDateTime.SmallDateTimeNow;
			var someRandomTime = ZDateTime.UtcNow.AddDays(2).AddHours(3).ToSmallDateTime();
			DepartureConsol.Transports.DepartureTransport.JW_ETD = someRandomTime;

			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultedFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentLoadingETD;
			Factory.Save();
			AssertEquals("Estimated date defaulted from Shipment ETD", Shipment.JS_E_DEP, milestone.P9_ScheduledDate.ToZDateTime());
		}

		public void TestEstimateDefaultedFromShipmentETA()
		{
			SetupDepartureArrivalConsols_ForTest();
			Shipment.JS_E_ARV = ZDateTime.SmallDateTimeNow;
			var someRandomTime = ZDateTime.UtcNow.AddDays(3).AddHours(4).ToSmallDateTime();
			ArrivalConsol.Transports.ArrivalTransport.JW_ETA = someRandomTime;

			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultedFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.ShipmentDischargeETA;
			Factory.Save();
			AssertEquals("Estimated date defaulted from Destination Load ETA", Shipment.JS_E_ARV, milestone.P9_ScheduledDate.ToZDateTime());
		}

		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromFCLAvailable()
		{
			TestEstimateDefaultedFromAvailableStorage(ForwardingShipmentEstimateDefaultedFromList.Codes.FCLAvailable, JobDocsAndCartageSchema.JP_FCLAvailable);
		}

		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromFCLStorage()
		{
			TestEstimateDefaultedFromAvailableStorage(ForwardingShipmentEstimateDefaultedFromList.Codes.FCLStorage, JobDocsAndCartageSchema.JP_FCLStorageCommences);
		}

		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromLCLAvailable()
		{
			TestEstimateDefaultedFromAvailableStorage(ForwardingShipmentEstimateDefaultedFromList.Codes.LCLAvailable, JobDocsAndCartageSchema.JP_LCLAvailable);
		}

		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromLCLStorage()
		{
			TestEstimateDefaultedFromAvailableStorage(ForwardingShipmentEstimateDefaultedFromList.Codes.LCLStorage, JobDocsAndCartageSchema.JP_LCLStorageCommences);
		}

		void TestEstimateDefaultedFromAvailableStorage(ZString estimateDefaultedFrom, SchemaDateTimeColumn docsAndCartageProperty)
		{
			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultedFrom = estimateDefaultedFrom;
			milestone.P9_EstimatedDefaultTimeDelta = new ZDateTime(2000, 1, 1).AddDays(1);
			SetupDepartureArrivalConsols_ForTest();

			Shipment.DocsAndCartage[docsAndCartageProperty] = new ZDateTime(2005, 10, 1);
			Factory.Save();
			AssertEquals("Estimated date populated", new ZDateTime(2005, 10, 2), milestone.P9_ScheduledDate.ToZDateTime());
		}

		#region TestEstimateDefaultedFromDate_WhenDocsAndCartageIsNull

		public void TestEstimateDefaultedFromDate_WhenDocsAndCartageIsNull()
		{
			SetupDepartureArrivalConsols_ForTest();
			Shipment.Delete();
			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultedFrom = ForwardingShipmentEstimateDefaultedFromList.Codes.FCLAvailable;
			AssertNoExceptionThrown(() => milestone.DefaultEstimateIfRequired());
		}

		#endregion

		#endregion

		#region ProcessTask Property Overrides

		public void TestParentControllerID()
		{
			ProcessTask milestone = Job.WorkflowItems.Milestones.AddNew();

			Job.JS_IsForwardRegistered = true;
			AssertEquals("ParentControllerID for ForwardingShipment", ControllerIDs.JobShipment, milestone.ParentControllerID);

			Job.JS_IsForwardRegistered = false;
			AssertEquals("Can't show the form for a non-forwarding shipment, this proved too difficult", null, milestone.ParentControllerID);
		}

		public void TestParentControllerIDAfterDeleting()
		{
			ProcessTask milestone = Job.WorkflowItems.Milestones.AddNew();
			AssertEquals("ForwardingShipmentProcessTask.Parent should be not null before deleting", true, milestone.Parent != null);
			AssertEquals("ForwardingShipmentProcessTask.IsDeleted should be false before deleting", false, milestone.IsDeleted);

			milestone.Delete();
			AssertEquals("ForwardingShipmentProcessTask.Parent should be null after deleting", null, milestone.Parent);
			AssertEquals("ForwardingShipmentProcessTask.IsDeleted should be true after deleting", true, milestone.IsDeleted);

			Job.JS_IsForwardRegistered = true;
			AssertEquals("ParentControllerID for ForwardingShipment", null, milestone.ParentControllerID);

			Job.JS_IsForwardRegistered = false;
			AssertEquals("Can't show the form for a non-forwarding shipment, this proved too difficult", null, milestone.ParentControllerID);

			ErrorReporter.Clear();
		}

		public void TestETA_EmptyInBase()
		{
			ForwardingShipmentProcessTask task = Factory.NewWithValidTestData<ForwardingShipmentProcessTask>();
			task.P9_ParentID = Shipment.PK;
			task.P9_ParentTableCode = "JS";

			Factory.Save();

			AssertEquals("ETA field should return ForwardingShipment.JS_E_ARV for current Exception if its ParentTableCode is JK", Shipment.JS_E_ARV, task.ETA);
		}

		public void TestETD_EmptyInBase()
		{
			ForwardingShipmentProcessTask task = Factory.NewWithValidTestData<ForwardingShipmentProcessTask>();
			task.P9_ParentID = Shipment.PK;
			task.P9_ParentTableCode = "JS";

			Factory.Save();

			AssertEquals("ETD field should return ForwardingShipment.JS_E_DEP for current Exception if its ParentTableCode is JK", Shipment.JS_E_DEP, task.ETD);
		}

		public void TestLoadOrOriginPort_EmptyInBase()
		{
			ForwardingShipmentProcessTask task = Factory.NewWithValidTestData<ForwardingShipmentProcessTask>();
			task.P9_ParentID = Shipment.PK;
			task.P9_ParentTableCode = "JS";

			Factory.Save();

			AssertEquals("LoadOrOriginPort field should return ForwardingShipment.JS_RL_NKOrigin for current Exception if its ParentTableCode is JK", Shipment.JS_RL_NKOrigin, task.LoadOrOriginPort);
		}

		public void TestDischargeOrDestinationPort_EmptyInBase()
		{
			ForwardingShipmentProcessTask task = Factory.NewWithValidTestData<ForwardingShipmentProcessTask>();
			task.P9_ParentID = Shipment.PK;
			task.P9_ParentTableCode = "JS";

			Factory.Save();

			AssertEquals("DischargeOrDestinationPort field should return ForwardingShipment.JS_RL_NKDestination for current Exception if its ParentTableCode is JK", Shipment.JS_RL_NKDestination, task.DischargeOrDestinationPort);
		}

		#endregion

		public void TestIsExceptionActionedReadOnly()
		{
			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			exceptionProcessTask.IsExceptionActioned = true;
			Assert("is read only", exceptionProcessTask.IsExceptionActionedInfo.ReadOnly);

			exceptionProcessTask.P9_Type = Core.Constants.Workflow.MilestoneType;
			Assert("now is not read only", !exceptionProcessTask.P9_NotesInfo.ReadOnly);

			exceptionProcessTask.P9_Type = Core.Constants.Workflow.ExceptionType;
			exceptionProcessTask.IsExceptionActioned = false;
			Assert("now is not read only", !exceptionProcessTask.IsExceptionActionedInfo.ReadOnly);
			exceptionProcessTask.IsExceptionActioned = true;

			exceptionProcessTask.TriggerConditions.TriggerEventCode = "XXX";
			Assert("now is not read only", !exceptionProcessTask.IsExceptionActionedInfo.ReadOnly);

			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			Assert("is read only", exceptionProcessTask.IsExceptionActionedInfo.ReadOnly);
		}

		[TestDate(2008, 1, 31, 10, 15, 21)]
		public void TestSettingtLateCargoReportTextOnSave()
		{
			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			exceptionProcessTask.LateCargoReportReason = "IARRD";
			AssertEquals("Pre-condition", "IARRD:", exceptionProcessTask.P9_Notes.ToAscii());
			Factory.Save();
			AssertEquals("Text set on save", "IARRD:IAR - Vessel Discharge Changed (Submitted by: " + GlbStaff.CurrentUser.GS_FullName + " at 31-Jan-08 10:15:21)", exceptionProcessTask.P9_Notes.ToAscii());
		}

		public void TestLateCargoReportReasonCodes()
		{
			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			AssertEquals("Correct number of codes", 16, exceptionProcessTask.LateCargoReportingReasonCodes.Count);
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("IARRD"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("IARRI"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("IARRW"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("LDOCA"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("LDOCC"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("LDOCI"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("LDOCS"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("MISCC"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("MISCR"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("RLDGC"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("RLDGI"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("RLDOH"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("RLDOV"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("RLDOY"));
			Assert("LateCargoReportingReasonCodes", exceptionProcessTask.LateCargoReportingReasonCodes.ContainsCode("RLDGS"));
		}

		public void TestLateCargoReportReasonAndTextWithBlankNotes()
		{
			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			exceptionProcessTask.LateCargoReportReason = "ABC";
			AssertEquals("P9_Note", "ABC  :", exceptionProcessTask.P9_Notes.ToAscii());
			exceptionProcessTask.LateCargoReportText = "ADDITIONAL TEXT";
			AssertEquals("P9_Note", "ABC  :ADDITIONAL TEXT", exceptionProcessTask.P9_Notes.ToAscii());
		}

		public void TestLateCargoReportReasonAndTextWithExistingNotes()
		{
			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			exceptionProcessTask.P9_Notes = ZBlob.FromAscii("12345:EXISTING TEXT");
			exceptionProcessTask.LateCargoReportReason = "ABC";
			AssertEquals("P9_Note", "ABC  :EXISTING TEXT", exceptionProcessTask.P9_Notes.ToAscii());
			exceptionProcessTask.LateCargoReportText = "NEW TEXT";
			AssertEquals("P9_Note", "ABC  :NEW TEXT", exceptionProcessTask.P9_Notes.ToAscii());
		}

		public void TestLateCargoReportReasonAndTextWithExistingNotesTextOnly()
		{
			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			exceptionProcessTask.P9_Notes = ZBlob.FromAscii("EXISTING TEXT");
			exceptionProcessTask.LateCargoReportReason = "ABC";
			AssertEquals("P9_Note", "ABC  :EXISTING TEXT", exceptionProcessTask.P9_Notes.ToAscii());
			exceptionProcessTask.LateCargoReportText = "NEW TEXT";
			AssertEquals("P9_Note", "ABC  :NEW TEXT", exceptionProcessTask.P9_Notes.ToAscii());
		}

		public void TestIsCargoReportAcceptedProcessTask()
		{
			ForwardingShipmentProcessTask exceptionWithoutParent = Factory.New<ForwardingShipmentProcessTask>();
			exceptionWithoutParent.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			Assert("Not a Cargo Report Accepted process task", !exceptionWithoutParent.IsCargoReportAcceptedProcessTask);

			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = "XXX";
			Assert("Not a Cargo Report Accepted process task", !exceptionProcessTask.IsCargoReportAcceptedProcessTask);

			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			Assert("Is a Cargo Report Accepted process task", exceptionProcessTask.IsCargoReportAcceptedProcessTask);
		}

		public void TestCargoReportAcceptedMilestones()
		{
			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			ForwardingShipmentProcessTask mileStoneProcessTask1 = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Milestones.AddNew();
			mileStoneProcessTask1.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			ForwardingShipmentProcessTask mileStoneProcessTask2 = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Milestones.AddNew();
			mileStoneProcessTask2.TriggerConditions.TriggerEventCode = "XXX";
			Factory.Save();
			AssertEquals("Only one milestone selected", 1, ForwardingShipmentProcessTask.CargoReportAcceptedMilestones(Shipment).Length);
			AssertEquals("Correct milestone", mileStoneProcessTask1, ForwardingShipmentProcessTask.CargoReportAcceptedMilestones(Shipment)[0]);
		}

		public void TestGetNewValidation()
		{
			ForwardingShipmentProcessTask mileStoneProcessTask = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Milestones.AddNew();
			mileStoneProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			AssertNotEquals("A milestone uses base validation", typeof(CargoReportAcceptedExceptionValidation), mileStoneProcessTask.Validation.GetType());

			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			AssertEquals("An exception uses new validation", typeof(CargoReportAcceptedExceptionValidation), exceptionProcessTask.Validation.GetType());
		}

		public void TestNotesReadOnly()
		{
			ForwardingShipmentProcessTask exceptionProcessTask = (ForwardingShipmentProcessTask)Shipment.WorkflowItems.Exceptions.AddNew();
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;
			exceptionProcessTask.IsExceptionActioned = true;
			ZString lb = "xxxx";
			exceptionProcessTask.P9_Notes = ZBlob.FromAscii("xxxx");
			Assert("is read only", exceptionProcessTask.P9_NotesInfo.ReadOnly);

			exceptionProcessTask.P9_Type = Core.Constants.Workflow.MilestoneType;
			Assert("now is not read only", !exceptionProcessTask.P9_NotesInfo.ReadOnly);
			exceptionProcessTask.P9_Type = Core.Constants.Workflow.ExceptionType;

			exceptionProcessTask.IsExceptionActioned = false;
			Assert("now is not read only", !exceptionProcessTask.P9_NotesInfo.ReadOnly);
			exceptionProcessTask.IsExceptionActioned = true;

			exceptionProcessTask.TriggerConditions.TriggerEventCode = "XXX";
			Assert("now is not read only", !exceptionProcessTask.P9_NotesInfo.ReadOnly);
			exceptionProcessTask.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)Shipment).CargoReportAcceptedEvent.Code;

			exceptionProcessTask.P9_Notes = ZBlob.FromAscii("");
			Assert("now is not read only", !exceptionProcessTask.P9_NotesInfo.ReadOnly);
			exceptionProcessTask.P9_Notes = ZBlob.FromAscii("xxxx");
			Assert("is back to read only", exceptionProcessTask.P9_NotesInfo.ReadOnly);
		}

		public void TestUnactionedExceptionForMiletsone()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_E_ARV = new ZDateTime(2008, 1, 15, 1, 2, 3);
			ProcessTask mileStone = shipment.WorkflowItems.Milestones.AddNew();
			mileStone.TriggerConditions.TriggerEventCode = ((IParentForCargoReporter)shipment).CargoReportAcceptedEvent.Code;
			AssertNull("No exception", ForwardingShipmentProcessTask.UnactionedExceptionForMiletsone(mileStone));
			ProcessTask mileStoneException = mileStone.CreateMilestoneException();
			AssertEquals(((IParentForCargoReporter)shipment).CargoReportAcceptedEvent.Code, mileStoneException.TriggerConditions.TriggerEventCode);
			AssertEquals("Exception returned", mileStoneException, ForwardingShipmentProcessTask.UnactionedExceptionForMiletsone(mileStone));
			mileStoneException.IsExceptionActioned = true;
			AssertNull("Exception actioned so no exception returned", ForwardingShipmentProcessTask.UnactionedExceptionForMiletsone(mileStone));
			mileStoneException.IsExceptionActioned = false;
			mileStone.SetMilestoneActualDateForTest(new ZDateTime(2008, 1, 15, 1, 2, 3));
			AssertNull("Milestone satisfied so no exception returned", ForwardingShipmentProcessTask.UnactionedExceptionForMiletsone(mileStone));
			mileStone.SetMilestoneActualDateForTest(ZDateTime.Empty);
			Factory.Save();
			AssertEquals("Exception returned", mileStoneException, ForwardingShipmentProcessTask.UnactionedExceptionForMiletsone(mileStone));
		}

		public void TestTypeDecider()
		{
			ForwardingShipment bookingShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			bookingShipment.JS_IsBooking = true;
			ProcessTask bookingShipmentTask = bookingShipment.WorkflowItems.AddNew();

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ProcessTask shipmentTask = bookingShipment.WorkflowItems.AddNew();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CombineAssertions(delegate
			{
				AssertEquals(typeof(ForwardingShipmentProcessTask), newFactory.Load<ProcessTask>(bookingShipmentTask.PK).GetType());
				AssertEquals(typeof(ForwardingShipmentProcessTask), newFactory.Load<ProcessTask>(shipmentTask.PK).GetType());
			});
		}

		[TestDate(2015, 4, 1)]
		public void TestEstimatedDateDefaulting()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "AUSYD";

			var estimateEventValue = new EventValue(Events.Departure, eventTime: new ZDateTimeOffset(2014, 1, 1), isEstimate: true);
			shipment.Logs.AddNew(estimateEventValue);

			var milestone = shipment.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			AssertEquals("P9_ScheduledDate updated from milestone event with blank reference", new ZDateTime(2014, 1, 1), milestone.P9_ScheduledDate.ToZDateTime());

			var nonStandardEventValue = new EventValue(Events.Departure, eventTime: new ZDateTimeOffset(2014, 1, 5), isEstimate: true, reference: "Not a system reference");
			shipment.Logs.AddNew(nonStandardEventValue);

			var milestone2 = shipment.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			AssertEquals("P9_ScheduledDate not updated by non-standard event reference", new ZDateTime(2014, 1, 1), milestone2.P9_ScheduledDate.ToZDateTime());

			var fromEventValue = new EventValue(Events.Departure, eventTime: new ZDateTimeOffset(2014, 1, 7), isEstimate: true, reference: "From: 01-Jan-2014");
			shipment.Logs.AddNew(fromEventValue);

			var milestone3 = shipment.WorkflowItems.Milestones.AddNew();
			milestone3.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			AssertEquals("P9_ScheduledDate updated by milestone event with 'From' reference", new ZDateTime(2014, 1, 7), milestone3.P9_ScheduledDate.ToZDateTime());

			var toEventValue = new EventValue(Events.Departure, eventTime: new ZDateTimeOffset(2014, 1, 9), isEstimate: true, reference: "To: 05-Jan-2014");
			shipment.Logs.AddNew(toEventValue);

			var milestone4 = shipment.WorkflowItems.Milestones.AddNew();
			milestone4.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			AssertEquals("P9_ScheduledDate updated by milestone event with 'To' reference", new ZDateTime(2014, 1, 9), milestone3.P9_ScheduledDate.ToZDateTime());
		}

		public void TestWorkflowTriggerDateSetFromEvent()
		{
			var testDate = new ZDateTimeOffset(2014, 2, 5);
			var shipment = Factory.New<ForwardingShipment>();

			var ddaEvent = new EventValue(Events.DocumentDelivered, eventTime: testDate, reference: "DDA Test event");
			shipment.Logs.AddNew(ddaEvent);

			ProcessTask ddaTrigger = shipment.WorkflowItems.Triggers.AddNew();
			ddaTrigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			ddaTrigger.TriggerConditions.TriggerConditionValue = "*Non-matching Test*";
			ddaTrigger.TriggerConditions.TriggerEventCode = Events.DocumentDelivered.Code;

			AssertEquals("Trigger date is not set for non matching event reference", ZDateTimeOffset.Empty, ddaTrigger.P9_ActualDateForBinding);

			ddaEvent = new EventValue(Events.DocumentDelivered, eventTime: testDate, reference: "Non matching reference");
			ddaTrigger = shipment.WorkflowItems.Triggers.AddNew();
			ddaTrigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			ddaTrigger.TriggerConditions.TriggerConditionValue = "*DDA Test*";
			ddaTrigger.TriggerConditions.TriggerEventCode = Events.DocumentDelivered.Code;

			AssertEquals("Trigger date is set from event", testDate.ToZDateTime(), ddaTrigger.P9_ActualDate);
		}

		#region Implementation

		ForwardingShipment Shipment
		{
			get { return Job; }
		}

		ForwardingConsol DepartureConsol
		{
			get
			{
				if (departureConsol == null)
				{
					departureConsol = Factory.New<ForwardingConsol>();
					departureConsol.JK_RL_NKLoadPort = Job.JS_RL_NKOrigin;
					departureConsol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				}
				return departureConsol;
			}
		}
		ForwardingConsol departureConsol;

		ForwardingConsol ArrivalConsol
		{
			get
			{
				if (arrivalConsol == null)
				{
					arrivalConsol = Factory.New<ForwardingConsol>();
					arrivalConsol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					arrivalConsol.JK_RL_NKDischargePort = Job.JS_RL_NKDestination;
				}
				return arrivalConsol;
			}
		}
		ForwardingConsol arrivalConsol;

		ProcessTask Milestone
		{
			get
			{
				if (milestone == null)
				{
					milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
				}
				return milestone;
			}
		}
		ProcessTask milestone;

		void SetupDepartureArrivalConsols_ForTest()
		{
			Shipment.Consols.Add(DepartureConsol);
			Shipment.Consols.Add(ArrivalConsol);
			AssertEquals("DepartureConsol for the test", DepartureConsol, Shipment.DepartureConsol);
			AssertEquals("ArrivalConsol for the test", ArrivalConsol, Shipment.ArrivalConsol);
		}

		protected override ZString ParentOrigin
		{
			get { return Job.JS_RL_NKOrigin; }
			set { Job.JS_RL_NKOrigin = value; }
		}

		protected override ZString ParentDestination
		{
			get { return Job.JS_RL_NKDestination; }
			set { Job.JS_RL_NKDestination = value; }
		}

		#endregion
	}
}
