using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class QuotedBookingProcessTaskNotificationValidationTest : ProcessTask_ProcessTaskNotificationValidationTest
	{
		public void TestTriggerTypes_NotAvailableForSpotQuotes_WorkflowTemplate()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = TriggerLineTypes.Codes.QuotedBooking;
			template1.P0_SubType2 = QuotedBooking.SpotQuoteCode;

			var processTask1 = template1.WorkflowItems.Triggers.AddNew();
			var triggerAction1 = processTask1.CompletionTriggerActionsCollection().AddNew();
			triggerAction1.Parent.Description = "BingBong";
			triggerAction1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			AssertError_NotAvailableForSpotQuote(triggerAction1);

			template1.P0_SubType2 = QuotedBooking.BookingWithQuoteCode;
			AssertNoError_NotAvailableForSpotQuote(triggerAction1);

			template1.P0_SubType2 = QuotedBooking.SpotQuoteCode;
			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			AssertNoError_NotAvailableForSpotQuote(triggerAction1);

			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;
			AssertError_NotAvailableForSpotQuote(triggerAction1);

			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance;
			AssertError_NotAvailableForSpotQuote(triggerAction1);

			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertNoError_NotAvailableForSpotQuote(triggerAction1);

			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			AssertError_NotAvailableForSpotQuote(triggerAction1);

			template1.P0_SubType2 = QuotedBooking.QuickBookingCode;
			AssertNoError_NotAvailableForSpotQuote(triggerAction1);
		}

		public void TestTriggerTypes_NotAvailableForSpotQuotes()
		{
			AssertNotAvailableTriggerTypes(QuotedBooking.New(QuoteBookingType.SpotQuote, Factory));
			AssertNotAvailableTriggerTypes(QuotedBooking.New(QuoteBookingType.QuickBooking, Factory));
			AssertNotAvailableTriggerTypes(QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory));
		}

		void AssertNotAvailableTriggerTypes(QuotedBooking quotedBooking)
		{
			var isOneOffQuote = quotedBooking.Quote != null && quotedBooking.Booking == null;
			var task = quotedBooking.WorkflowItems.Triggers.AddNew();

			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			AssertError_NotAvailableForSpotQuote(notification, isOneOffQuote);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			AssertNoError_NotAvailableForSpotQuote(notification);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;
			AssertError_NotAvailableForSpotQuote(notification, isOneOffQuote);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance;
			AssertError_NotAvailableForSpotQuote(notification, isOneOffQuote);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			AssertError_NotAvailableForSpotQuote(notification, isOneOffQuote);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			AssertError_NotAvailableForSpotQuote(notification, isOneOffQuote);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertNoError_NotAvailableForSpotQuote(notification);
		}

		void AssertNoError_NotAvailableForSpotQuote(ProcessTaskNotification notification)
		{
			notification.Validation.ValidateAll();
			AssertNoError("PQ_TriggerTypeInfo", notification.PQ_TriggerTypeInfo, errorMessageNotAvailableForSpotQuote);
		}

		void AssertError_NotAvailableForSpotQuote(ProcessTaskNotification notification)
		{
			notification.Validation.ValidateAll();
			AssertHasError("PQ_TriggerTypeInfo", notification.PQ_TriggerTypeInfo, errorMessageNotAvailableForSpotQuote);
		}

		void AssertError_NotAvailableForSpotQuote(ProcessTaskNotification notification, bool isOneOffQuote)
		{
			if (isOneOffQuote)
			{
				AssertError_NotAvailableForSpotQuote(notification);
			}
			else
			{
				AssertNoError_NotAvailableForSpotQuote(notification);
			}
		}

		const string errorMessageNotAvailableForSpotQuote = "Not available for Spot Quotes.";
	}
}
