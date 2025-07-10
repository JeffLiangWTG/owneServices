using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	[TestedType(typeof(MessageStatus))]
	sealed class MessageStatusTest : NonPersistentBusinessObjectTestCase
	{
		#region TestMessageStatus_NoLogs

		public void TestMessageStatus_NoLogs()
		{
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			var messageStatus = new MessageStatus("123", logProvider);

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Not Sent", messageStatus.Description);
				AssertEquals("CurrentState", MessageState.NotSent, messageStatus.CurrentState);
				AssertEquals("AllowSendOriginal", true, messageStatus.AllowSendOriginal);
				AssertEquals("AllowSendAmendment", false, messageStatus.AllowSendAmendment);
				AssertEquals("AllowSendWithdrawal", false, messageStatus.AllowSendWithdrawal);
				AssertEquals("AllowResetToOriginal", false, messageStatus.AllowResetToOriginal);
			});
		}

		#endregion

		#region TestMessageStatus_SentNoResponse

		public void TestMessageStatus_SentNoResponse()
		{
			const string bookingNumber = "123";
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(logProvider, Events.MessageSent, bookingNumber);

			var messageStatus = new MessageStatus(bookingNumber, logProvider);

			CombineAssertions(() =>
			{
				AssertEquals("Description", "eManifest Message Sent Ref No: 123", messageStatus.Description);
				AssertEquals("CurrentState", MessageState.OriginalSent, messageStatus.CurrentState);
				AssertEquals("AllowSendOriginal", false, messageStatus.AllowSendOriginal);
				AssertEquals("AllowSendAmendment", false, messageStatus.AllowSendAmendment);
				AssertEquals("AllowSendWithdrawal", false, messageStatus.AllowSendWithdrawal);
				AssertEquals("AllowResetToOriginal", true, messageStatus.AllowResetToOriginal);
			});
		}

		#endregion

		#region TestMessageStatus_SentThenResetToOriginal

		public void TestMessageStatus_SentThenResetToOriginal()
		{
			const string bookingNumber = "123";
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(logProvider, Events.MessageSent, bookingNumber);
			CreateLog(logProvider, Events.StatusUpdated, bookingNumber);

			var messageStatus = new MessageStatus(bookingNumber, logProvider);

			CombineAssertions(() =>
			{
				AssertEquals("Description", "Status Updated from eManifest, Reference No. 123,", messageStatus.Description);
				AssertEquals("CurrentState", MessageState.NotSent, messageStatus.CurrentState);
				AssertEquals("AllowSendOriginal", true, messageStatus.AllowSendOriginal);
				AssertEquals("AllowSendAmendment", false, messageStatus.AllowSendAmendment);
				AssertEquals("AllowSendWithdrawal", false, messageStatus.AllowSendWithdrawal);
				AssertEquals("AllowResetToOriginal", false, messageStatus.AllowResetToOriginal);
			});
		}

		#endregion

		#region TestMessageStatus_SentAndAcknowledged

		public void TestMessageStatus_SentAndAcknowledged()
		{
			const string bookingNumber = "123";
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(logProvider, Events.MessageSent, bookingNumber);
			CreateLog(logProvider, Events.InterchangeSent, bookingNumber, true);

			var messageStatus = new MessageStatus(bookingNumber, logProvider);

			CombineAssertions(() =>
			{
				AssertEquals("Description", "eManifest Interchange Sent Ref No: 123", messageStatus.Description);
				AssertEquals("CurrentState", MessageState.OriginalAccepted, messageStatus.CurrentState);
				AssertEquals("AllowSendOriginal", false, messageStatus.AllowSendOriginal);
				AssertEquals("AllowSendAmendment", true, messageStatus.AllowSendAmendment);
				AssertEquals("AllowSendWithdrawal", true, messageStatus.AllowSendWithdrawal);
				AssertEquals("AllowResetToOriginal", true, messageStatus.AllowResetToOriginal);
			});
		}

		#endregion

		#region TestMessageStatus_SentAndRejected

		public void TestMessageStatus_SentAndRejected()
		{
			const string bookingNumber = "123";
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(logProvider, Events.MessageSent, bookingNumber);
			CreateLog(logProvider, Events.InterchangeRejected, bookingNumber, true);

			var messageStatus = new MessageStatus(bookingNumber, logProvider);

			CombineAssertions(() =>
			{
				AssertEquals("Description", "eManifest Interchange Rejected Ref No: 123", messageStatus.Description);
				AssertEquals("CurrentState", MessageState.OriginalRejected, messageStatus.CurrentState);
				AssertEquals("AllowSendOriginal", true, messageStatus.AllowSendOriginal);
				AssertEquals("AllowSendAmendment", false, messageStatus.AllowSendAmendment);
				AssertEquals("AllowSendWithdrawal", false, messageStatus.AllowSendWithdrawal);
				AssertEquals("AllowResetToOriginal", false, messageStatus.AllowResetToOriginal);
			});
		}

		#endregion

		#region TestMessageStatus_AmendmentSentAndAcknowledged

		public void TestMessageStatus_AmendmentSentAndAcknowledged()
		{
			const string bookingNumber = "123";
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(logProvider, Events.MessageSent, bookingNumber);
			CreateLog(logProvider, Events.InterchangeSent, bookingNumber, true);
			CreateLog(logProvider, Events.MessageSent, bookingNumber);
			CreateLog(logProvider, Events.InterchangeSent, bookingNumber, true);

			var messageStatus = new MessageStatus(bookingNumber, logProvider);

			CombineAssertions(() =>
			{
				AssertEquals("Description", "eManifest Interchange Sent Ref No: 123", messageStatus.Description);
				AssertEquals("CurrentState", MessageState.AmendmentAccepted, messageStatus.CurrentState);
				AssertEquals("AllowSendOriginal", false, messageStatus.AllowSendOriginal);
				AssertEquals("AllowSendAmendment", true, messageStatus.AllowSendAmendment);
				AssertEquals("AllowSendWithdrawal", true, messageStatus.AllowSendWithdrawal);
				AssertEquals("AllowResetToOriginal", true, messageStatus.AllowResetToOriginal);
			});
		}

		#endregion

		#region TestMessageStatus_AmendmentSentAndRejected

		public void TestMessageStatus_AmendmentSentAndRejected()
		{
			const string bookingNumber = "123";
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(logProvider, Events.MessageSent, bookingNumber);
			CreateLog(logProvider, Events.InterchangeSent, bookingNumber, true);
			CreateLog(logProvider, Events.MessageSent, bookingNumber);
			CreateLog(logProvider, Events.InterchangeRejected, bookingNumber, true);

			var messageStatus = new MessageStatus(bookingNumber, logProvider);

			CombineAssertions(() =>
			{
				AssertEquals("Description", "eManifest Interchange Rejected Ref No: 123", messageStatus.Description);
				AssertEquals("CurrentState", MessageState.AmendmentRejected, messageStatus.CurrentState);
				AssertEquals("AllowSendOriginal", false, messageStatus.AllowSendOriginal);
				AssertEquals("AllowSendAmendment", true, messageStatus.AllowSendAmendment);
				AssertEquals("AllowSendWithdrawal", true, messageStatus.AllowSendWithdrawal);
				AssertEquals("AllowResetToOriginal", true, messageStatus.AllowResetToOriginal);
			});
		}

		#endregion

		#region TestMessageStatus_WithdrawalSent

		public void TestMessageStatus_WithdrawalSent()
		{
			const string bookingNumber = "123";
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(logProvider, Events.MessageSent, bookingNumber);
			CreateLog(logProvider, Events.InterchangeSent, bookingNumber, true);
			CreateLog(logProvider, Events.MessageWithdrawCancelRequest, bookingNumber);

			var messageStatus = new MessageStatus(bookingNumber, logProvider);

			CombineAssertions(() =>
			{
				AssertEquals("Description", "eManifest Message Withdraw/Cancel Request Ref No: 123", messageStatus.Description);
				AssertEquals("CurrentState", MessageState.WithdrawalSent, messageStatus.CurrentState);
				AssertEquals("AllowSendOriginal", false, messageStatus.AllowSendOriginal);
				AssertEquals("AllowSendAmendment", false, messageStatus.AllowSendAmendment);
				AssertEquals("AllowSendWithdrawal", false, messageStatus.AllowSendWithdrawal);
				AssertEquals("AllowResetToOriginal", true, messageStatus.AllowResetToOriginal);
			});
		}

		#endregion

		#region TestMessageStatus_WithdrawalSentAndFailed

		public void TestMessageStatus_WithdrawalSentAndFailed()
		{
			const string bookingNumber = "123";
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(logProvider, Events.MessageSent, bookingNumber);
			CreateLog(logProvider, Events.InterchangeSent, bookingNumber, true);
			CreateLog(logProvider, Events.MessageWithdrawCancelRequest, bookingNumber);
			CreateLog(logProvider, Events.InterchangeRejected, bookingNumber, true);

			var messageStatus = new MessageStatus(bookingNumber, logProvider);

			CombineAssertions(() =>
			{
				AssertEquals("Description", "eManifest Interchange Rejected Ref No: 123", messageStatus.Description);
				AssertEquals("CurrentState", MessageState.WithdrawalRejected, messageStatus.CurrentState);
				AssertEquals("AllowSendOriginal", false, messageStatus.AllowSendOriginal);
				AssertEquals("AllowSendAmendment", true, messageStatus.AllowSendAmendment);
				AssertEquals("AllowSendWithdrawal", true, messageStatus.AllowSendWithdrawal);
				AssertEquals("AllowResetToOriginal", true, messageStatus.AllowResetToOriginal);
			});
		}

		#endregion

		#region TestMessageStatus_WithdrawalAccepted

		public void TestMessageStatus_WithdrawalAccepted()
		{
			const string bookingNumber = "123";
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(logProvider, Events.MessageSent, bookingNumber);
			CreateLog(logProvider, Events.InterchangeSent, bookingNumber, true);
			CreateLog(logProvider, Events.MessageWithdrawCancelRequest, bookingNumber);
			CreateLog(logProvider, Events.InterchangeSent, bookingNumber, true);

			var messageStatus = new MessageStatus(bookingNumber, logProvider);

			CombineAssertions(() =>
			{
				AssertEquals("Description", "eManifest Interchange Sent Ref No: 123", messageStatus.Description);
				AssertEquals("CurrentState", MessageState.WithdrawalAccepted, messageStatus.CurrentState);
				AssertEquals("AllowSendOriginal", true, messageStatus.AllowSendOriginal);
				AssertEquals("AllowSendAmendment", false, messageStatus.AllowSendAmendment);
				AssertEquals("AllowSendWithdrawal", false, messageStatus.AllowSendWithdrawal);
				AssertEquals("AllowResetToOriginal", false, messageStatus.AllowResetToOriginal);
			});
		}

		#endregion

		#region TestMessageStatus_WithdrawalSentThenResetToOriginal

		public void TestMessageStatus_WithdrawalSentThenResetToOriginal()
		{
			const string bookingNumber = "123";
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(logProvider, Events.MessageSent, bookingNumber);
			CreateLog(logProvider, Events.InterchangeSent, bookingNumber, true);
			CreateLog(logProvider, Events.MessageWithdrawCancelRequest, bookingNumber);
			CreateLog(logProvider, Events.StatusUpdated, bookingNumber);

			var messageStatus = new MessageStatus(bookingNumber, logProvider);

			CombineAssertions(() =>
			{
				AssertEquals("Status Updated from eManifest, Reference No. 123,", messageStatus.Description);
				AssertEquals("CurrentState", MessageState.NotSent, messageStatus.CurrentState);
				AssertEquals("AllowSendOriginal", true, messageStatus.AllowSendOriginal);
				AssertEquals("AllowSendAmendment", false, messageStatus.AllowSendAmendment);
				AssertEquals("AllowSendWithdrawal", false, messageStatus.AllowSendWithdrawal);
				AssertEquals("AllowResetToOriginal", false, messageStatus.AllowResetToOriginal);
			});
		}

		#endregion

		#region TestMessageStatus_WithdrawalSentAndAcceptedThenResetToOriginal

		public void TestMessageStatus_WithdrawalSentAndAcceptedThenResetToOriginal()
		{
			const string bookingNumber = "123";
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			CreateLog(logProvider, Events.MessageSent, bookingNumber);
			CreateLog(logProvider, Events.InterchangeSent, bookingNumber, true);
			CreateLog(logProvider, Events.MessageWithdrawCancelRequest, bookingNumber);
			CreateLog(logProvider, Events.InterchangeSent, bookingNumber, true);
			CreateLog(logProvider, Events.StatusUpdated, bookingNumber);

			var messageStatus = new MessageStatus(bookingNumber, logProvider);

			CombineAssertions(() =>
			{
				AssertEquals("Status Updated from eManifest, Reference No. 123,", messageStatus.Description);
				AssertEquals("CurrentState", MessageState.NotSent, messageStatus.CurrentState);
				AssertEquals("AllowSendOriginal", true, messageStatus.AllowSendOriginal);
				AssertEquals("AllowSendAmendment", false, messageStatus.AllowSendAmendment);
				AssertEquals("AllowSendWithdrawal", false, messageStatus.AllowSendWithdrawal);
				AssertEquals("AllowResetToOriginal", false, messageStatus.AllowResetToOriginal);
			});
		}

		#endregion

		#region Implementation

		void CreateLog(IStmALogProvider logProvider, Event @event, ZString bookingNumber, bool simulateTimeZoneDifference = false)
		{
			if (logDateTime.IsEmpty)
			{
				logDateTime = ZDateTime.Now.AddDays(1);
			}
			else
			{
				logDateTime = logDateTime.AddMinutes(1);

				if (simulateTimeZoneDifference)
				{
					logDateTime = logDateTime.AddHours(-2);
				}
			}

			logProvider?.Logs.CreateOrRecreateEventLog(
				@event,
				EstimateActual.Actual,
				logDateTime.ToOffset(),
				$"|MST=eManifest|RFN={bookingNumber}");

			Thread.Sleep(1);
			Factory.Save();
		}

		ZDateTime logDateTime;

		protected override BusinessObject GetNewBusinessObject()
		{
			var logProvider = Factory.New<DummyEnterpriseBusinessObject>();
			return new MessageStatus("123", logProvider);
		}

		#endregion
	}
}
