using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(QuotedBookingProcessTask))]
	public class QuotedBookingProcessTaskTest : ProcessTaskTest
	{
		#region ParentControllerID

		public void TestParentControllerID()
		{
			CombineAssertions("ParentControllerID should differentiate between OneOffQuotes and QuotedBookings/QuickBooking", () =>
			{
				var oneOffQuote = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
				AssertEquals("Precondition: OneOffQuote/SpotQuote", QuotedBookingState.QuoteOnly, oneOffQuote.ObjectState);
				AssertParentControllerID(oneOffQuote, expectedParentControllerID: ControllerIDs.OneOffQuotes);

				var acceptedBookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				acceptedBookingWithQuote.Quote.TH_Accepted = ZDateTime.Now;
				AssertEquals("Precondition: Accepted BookingWithQuote", QuotedBookingState.AcceptedBookingWithQuote, acceptedBookingWithQuote.ObjectState);
				AssertParentControllerID(acceptedBookingWithQuote, expectedParentControllerID: ControllerIDs.QuotedBookings);

				var unacceptedBookingWithQuote = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
				unacceptedBookingWithQuote.Quote.TH_Accepted = ZDateTime.Empty;
				AssertEquals("Precondition: Unaccepted BookingWithQuote", QuotedBookingState.UnacceptedBookingWithQuote, unacceptedBookingWithQuote.ObjectState);
				AssertParentControllerID(unacceptedBookingWithQuote, expectedParentControllerID: ControllerIDs.QuotedBookings);

				var quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
				AssertEquals("Precondition: QuickBooking", QuotedBookingState.BookingOnly, quickBooking.ObjectState);
				AssertParentControllerID(quickBooking, expectedParentControllerID: ControllerIDs.QuotedBookings);
			});
		}

		void AssertParentControllerID(QuotedBooking quotedBooking, ControllerID expectedParentControllerID)
		{
			var task = quotedBooking.WorkflowItems.Tasks.AddNew();
			AssertEquals($"{quotedBooking.ObjectState}: Task", expectedParentControllerID, task.ParentControllerID);
			AssertEquals($"{quotedBooking.ObjectState}: Parent", quotedBooking.PK, task.Parent.PK);

			var milestone = quotedBooking.WorkflowItems.Milestones.AddNew();
			AssertEquals($"{quotedBooking.ObjectState}: Milestone", expectedParentControllerID, milestone.ParentControllerID);
			AssertEquals($"{quotedBooking.ObjectState}: Parent", quotedBooking.PK, milestone.Parent.PK);

			var exception = quotedBooking.WorkflowItems.Exceptions.AddNew();
			AssertEquals($"{quotedBooking.ObjectState}: Exception", expectedParentControllerID, exception.ParentControllerID);
			AssertEquals($"{quotedBooking.ObjectState}: Parent", quotedBooking.PK, exception.Parent.PK);

			var trigger = quotedBooking.WorkflowItems.Triggers.AddNew();
			AssertEquals($"{quotedBooking.ObjectState}: Trigger", expectedParentControllerID, trigger.ParentControllerID);
			AssertEquals($"{quotedBooking.ObjectState}: Parent", quotedBooking.PK, trigger.Parent.PK);
		}

		#endregion

		public void TestDoNotDefaultEstimateForConvertedQuotedBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);

			var milestone1 = quotedBooking.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			milestone1.P9_Sequence = 69;
			milestone1.P9_ScheduledDate = ZDateTime.Today;

			var milestone2 = quotedBooking.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Sequence = 96;
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			Factory.Save();

			AssertEquals("P9_ScheduledDate wasn't defaulted (no P9_EstimatedDefaultFromPredecessor was set)", ZDateTime.Empty, milestone2.P9_ScheduledDate);

			milestone2.P9_EstimatedDefaultFromPredecessor = milestone1.P9_Sequence;

			Assert("prerequisite: booking is not yet converted to Forwarding Shipment", !quotedBooking.Booking.JS_IsForwardRegistered);

			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(quotedBooking.Booking, null, null);

			Assert("prerequisite: booking was converted to Forwarding Shipment", quotedBooking.Booking.JS_IsForwardRegistered);

			Factory.Save();

			AssertEquals("P9_ScheduledDate wasn't defaulted (Quoted Booking was converted to Forwarding Shipment)", ZDateTime.Empty, milestone2.P9_ScheduledDate);
		}

		public void TestDefaultEstimateForNotConvertedCFSQuotedBooking()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.Booking.JS_IsCFSRegistered = true;

			var milestone1 = quotedBooking.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			milestone1.P9_Sequence = 69;
			milestone1.P9_ScheduledDate = ZDateTime.Today;

			var milestone2 = quotedBooking.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Sequence = 96;
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			Factory.Save();
			AssertEquals("P9_ScheduledDate wasn't defaulted (no P9_EstimatedDefaultFromPredecessor was set)", ZDateTime.Empty, milestone2.P9_ScheduledDate);

			milestone2.P9_EstimatedDefaultFromPredecessor = milestone1.P9_Sequence;
			Assert("prerequisite: booking is not yet converted to Forwarding Shipment", !quotedBooking.Booking.JS_IsForwardRegistered);
			Factory.Save();
			AssertEquals("P9_ScheduledDate was defaulted (Quoted Booking was not converted to Shipment)", ZDateTime.Today, milestone2.P9_ScheduledDate);
		}

		public void TestQuotedBookingTemplateExceptionsCascadeToShipment()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.TriggerConditions.TriggerEventCode = Events.PickupCartageCompleteFinalisedCode;
			templateMilestone.P9_RespondToCascadedEvents = true;
			templateMilestone.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily;

			var action = templateMilestone.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action.PQ_EmailAddr = "Alex@iscool.com";
			action.PQ_EmailText = "no more shipments";

			Factory.Save();

			var pickupTime = ZDateTime.Now.AddDays(-1).ToSmallDateTimeFloor();
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			quotedBooking.PickupReady = pickupTime;

			AssertEquals("Template Applied Event Found", 0, quotedBooking.Booking.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTemplateAppliedCode && !l.SL_IsCancelled).Count());

			Factory.Save();

			var helper = new BuildConsolHelper();
			helper.TurnBookingIntoShipment(quotedBooking.Booking, null, null);

			var shipment = Factory.Load<ForwardingShipment>(quotedBooking.PK);

			Factory.Save();

			var stmALog = shipment.Logs.Find(log => log.SL_SE_NKEvent == Events.PickupCartageCompleteFinalisedCode).FirstOrDefault();
			AssertNotNull("No PCF event", stmALog);
			AssertEquals("No Template Applied Event", 1, shipment.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTemplateAppliedCode && !l.SL_IsCancelled).Count());
			AssertEquals(1, shipment.WorkflowItems.Milestones.Count);
			var milestone = shipment.WorkflowItems.Milestones[0];
			AssertEquals("Schedule date is wrong", stmALog.SL_EventTime, milestone.P9_ScheduledDate);
		}

		public void TestParent()
		{
			var processTask = Factory.New<QuotedBookingProcessTask>();
			AssertNull(processTask.Parent);

			AssertParent(QuoteBookingType.SpotQuote);
			AssertParent(QuoteBookingType.QuickBooking);
			AssertParent(QuoteBookingType.BookingWithQuote);
		}

		void AssertParent(QuoteBookingType quoteBookingType)
		{
			QuotedBooking quotedBooking = QuotedBooking.New(quoteBookingType, Factory);
			ProcessTask milestone = quotedBooking.WorkflowItems.Milestones.AddNew();

			AssertEquals("not persisted QuotedBookingProcessTask parent is found", quotedBooking, milestone.Parent);

			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			ProcessTask otherFactoryMilestone = otherFactory.Load<ProcessTask>(milestone.PK);

			QuotedBooking otherFactoryQuotedBooking = (QuotedBooking)otherFactoryMilestone.Parent;

			AssertNotNull("persisted QuotedBookingProcessTask parent is found", otherFactoryQuotedBooking);
			AssertEquals(quotedBooking.PK, otherFactoryQuotedBooking.PK);

			if (quotedBooking.Booking != null)
			{
				AssertEquals(quotedBooking.Booking.PK, otherFactoryQuotedBooking.Booking.PK);
			}

			if (quotedBooking.Quote != null)
			{
				AssertEquals(quotedBooking.Quote.PK, otherFactoryQuotedBooking.Quote.PK);
			}
		}

		public void TestParentWithOverridenNewDelegate()
		{
			AssertParentWithOverridenNewDelegate(QuoteBookingType.SpotQuote);
			AssertParentWithOverridenNewDelegate(QuoteBookingType.QuickBooking);
			AssertParentWithOverridenNewDelegate(QuoteBookingType.BookingWithQuote);
		}

		void AssertParentWithOverridenNewDelegate(QuoteBookingType quoteBookingType)
		{
			QuotedBooking quotedBooking = null;
			ProcessTask processTask = null;

			TrickyQuotedBooking.TemporarilyOverrideQuotedBookingNewDelegate();

			quotedBooking = QuotedBooking.New(quoteBookingType, Factory);
			processTask = quotedBooking.WorkflowItems.Milestones.AddNew();

			AssertEquals("BusinessObjectCache.Fetch does not support type inherritance",
				quotedBooking, processTask.Parent);

			AssertEquals(quotedBooking, processTask.Parent);
		}

		public void TestQuotedBookingProcessTasksType()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			ProcessTask processTask = quotedBooking.WorkflowItems.AddNew();
			AssertEquals(typeof(QuotedBookingProcessTask), processTask.GetType());
		}

		public void TestParentLoadBookingWithQuote()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			ProcessTask milestone = quotedBooking.WorkflowItems.Milestones.AddNew();

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ProcessTask newFactoryMilestone = newFactory.Load<ProcessTask>(milestone.PK);
			QuotedBooking newFactoryMilestoneParent = (QuotedBooking)newFactoryMilestone.Parent;

			AssertEquals(quotedBooking.Quote.PK, newFactoryMilestoneParent.Quote.PK);
			AssertEquals(quotedBooking.Booking.PK, newFactoryMilestoneParent.Booking.PK);
		}

		public void TestParentLoadQuickBooking()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			ProcessTask milestone = quotedBooking.WorkflowItems.Milestones.AddNew();

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ProcessTask newFactoryMilestone = newFactory.Load<ProcessTask>(milestone.PK);
			QuotedBooking newFactoryMilestoneParent = (QuotedBooking)newFactoryMilestone.Parent;

			AssertEquals("Prerequisite", quotedBooking.Booking.PK, quotedBooking.PK);
			AssertEquals(null, newFactoryMilestoneParent.Quote);
			AssertEquals(quotedBooking.Booking.PK, newFactoryMilestoneParent.Booking.PK);
		}

		public void TestParentLoadQuote()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, Factory);
			ProcessTask milestone = quotedBooking.WorkflowItems.Milestones.AddNew();

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ProcessTask newFactoryMilestone = newFactory.Load<ProcessTask>(milestone.PK);
			QuotedBooking newFactoryMilestoneParent = (QuotedBooking)newFactoryMilestone.Parent;

			AssertEquals("Prerequisite", quotedBooking.Quote.PK, quotedBooking.PK);
			AssertEquals(quotedBooking.Quote.PK, newFactoryMilestoneParent.Quote.PK);
			AssertEquals(null, newFactoryMilestoneParent.Booking);
		}

		public void TestIQuotedBookingProcessTask()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			ProcessTask milestone = quotedBooking.WorkflowItems.Milestones.AddNew();
			Assert(milestone is IQuotedBookingProcessTask);
		}

		public void TestProcessTaskNotificationCollection()
		{
			QuotedBooking quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			ProcessTask processTask = quotedBooking.WorkflowItems.AddNew();
			AssertEquals(typeof(ProcessTaskNotificationCollection), processTask.ProcessTaskNotifications.GetType());
		}

		[ExpectNoExceptions]
		public void TestAddParentFetchHint_QuickBooking()
		{
			AssertAddParentFetchHint(QuoteBookingType.QuickBooking, 1, 0);
		}

		[ExpectNoExceptions]
		public void TestAddParentFetchHint_QuotedBooking()
		{
			AssertAddParentFetchHint(QuoteBookingType.BookingWithQuote, 6, 1);
		}

		[ExpectNoExceptions]
		public void TestAddParentFetchHint_SpotQuote()
		{
			AssertAddParentFetchHint(QuoteBookingType.SpotQuote, 4, 1);
		}

		void AssertAddParentFetchHint(QuoteBookingType type, int shipmentTableHitCount, int ratingHeaderTableHitCount)
		{
			var quotedBooking1 = QuotedBooking.New(type, Factory);
			var quotedBooking2 = QuotedBooking.New(type, Factory);
			var quotedBooking3 = QuotedBooking.New(type, Factory);
			var pk1 = quotedBooking1.WorkflowItems.Tasks.AddNew().PK;
			var pk2 = quotedBooking2.WorkflowItems.Tasks.AddNew().PK;
			var pk3 = quotedBooking3.WorkflowItems.Tasks.AddNew().PK;

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();

			var task1 = anotherFactory.Load<QuotedBookingProcessTask>(pk1);
			var task2 = anotherFactory.Load<QuotedBookingProcessTask>(pk2);
			var task3 = anotherFactory.Load<QuotedBookingProcessTask>(pk3);

			task1.AddParentFetchHint();
			task2.AddParentFetchHint();
			task3.AddParentFetchHint();

			anotherFactory.ResetDatabaseLoadCount();

			AssertEquals(quotedBooking1.PK, task1.Parent.PK);
			AssertEquals(quotedBooking2.PK, task2.Parent.PK);
			AssertEquals(quotedBooking3.PK, task3.Parent.PK);

			AssertEquals(shipmentTableHitCount, anotherFactory.GetTableHitCount(JobShipmentSchema.Constants.TableName));
			AssertEquals(ratingHeaderTableHitCount, anotherFactory.GetTableHitCount(RatingHeaderSchema.Constants.TableName));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var quotedBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			return quotedBooking.WorkflowItems.AddNew() as QuotedBookingProcessTask;
		}

		class TrickyQuotedBooking : QuotedBooking
		{
			TrickyQuotedBooking(ZGuid quotePK, ZGuid bookingPK, bool attemptToLoadFromOther, BusinessObjectFactory factory)
				: base(quotePK, bookingPK, attemptToLoadFromOther, factory)
			{
			}

			public static void TemporarilyOverrideQuotedBookingNewDelegate()
			{
				OverridableNewDelegate.Value = (quotePK, bookingPK, attemptToLoadFromOther, factoryToWrap) => new TrickyQuotedBooking(quotePK, bookingPK, attemptToLoadFromOther, factoryToWrap);
			}
		}

		#endregion
	}
}
