using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CustomsManifestStatusTest : NonPersistentBusinessObjectTestCase
	{
		public virtual void TestMessageStatus()
		{
			IManifestProvider manifestProvider = NewManifestProvider();
			AddValidMessage(manifestProvider);
			AddValidResponse(manifestProvider);

			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(manifestProvider);
			AssertEquals("ManifestStatus", ManifestStatus.Cleared, testCustomsManifestStatus.LastMessageStatus);
		}

		[TestDateIncremental(0, 0, 1, 0)]
		public void TestGetMessageStatusHistory()
		{
			if (SupportsGetMessageStatusHistory())
			{
				IManifestProvider manifestProvider = NewManifestProvider();
				AddValidMessage(manifestProvider);
				AddValidResponse(manifestProvider);
				AddValidMessage(manifestProvider);
				AddValidResponse(manifestProvider);
				AddValidMessage(manifestProvider);
				AddValidMessage(manifestProvider).EM_Status = EDIMessage.Status.Discarded; // decoys
				AddValidResponse(manifestProvider).EM_Status = EDIMessage.Status.Discarded;

				CustomsManifestStatus customsManifestStatus = NewCustomsManifestStatus(manifestProvider);
				List<ManifestStatus> messageStatuses = new List<ManifestStatus>(customsManifestStatus.GetMessageStatusHistory());
				AssertEquals("Currently we're waiting for a response", ManifestStatus.AwaitingResponse, messageStatuses[0]);
				AssertEquals("Got cleared second", ManifestStatus.Cleared, messageStatuses[1]);
				AssertEquals("Got cleared first", ManifestStatus.Cleared, messageStatuses[2]);
				AssertEquals("ManifestStatus should hold the last status in the message history", ManifestStatus.AwaitingResponse, customsManifestStatus.LastMessageStatus);
			}
			Assert(true);
		}

		public void TestGetMessageStatusHistory_WithNoMessages()
		{
			if (SupportsGetMessageStatusHistory())
			{
				IManifestProvider manifestProvider = NewManifestProvider();
				Factory.Save();
				CustomsManifestStatus customsManifestStatus = NewCustomsManifestStatus(manifestProvider);

				List<ManifestStatus> statuses = new List<ManifestStatus>(customsManifestStatus.GetMessageStatusHistory());
				AssertEquals("Should have no statuses as there are no messages", 0, statuses.Count);
				AssertEquals("LastManifestStatus should be 'notsent' as there are no messages", ManifestStatus.NotSent, customsManifestStatus.LastMessageStatus);
			}
			Assert(true);
		}

		protected virtual bool SupportsGetMessageStatusHistory()
		{
			return true;
		}

		[TestDateIncremental(0, 0, 1, 0)]
		public void TestMessageStatusForWithdrawal()
		{
			IManifestProvider manifestProvider = NewManifestProvider();
			AddValidMessage(manifestProvider);
			Factory.Save();
			AddValidResponse(manifestProvider);
			Factory.Save();
			AddValidMessage(manifestProvider);
			Factory.Save();
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(manifestProvider);
			AssertEquals("ManifestStatus", ManifestStatus.AwaitingResponse, testCustomsManifestStatus.LastMessageStatus);
		}

		[TestDateIncremental(0, 0, 1, 0)]
		public void TestTransmittedMessages()
		{
			EDIMessage message1 = AddValidMessage(fTestManifestProvider);
			EDIMessage message2 = AddValidResponse(fTestManifestProvider);
			EDIMessage message3 = AddValidMessage(fTestManifestProvider);
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			AssertEquals("There are two trasmitted messages", 2, testCustomsManifestStatus.TransmittedMessages.Length);
			AssertEquals("The last transmitted message-ordered", message3.PK, testCustomsManifestStatus.TransmittedMessages[0].PK);
		}

		[TestDateIncremental(0, 0, 1, 0)]
		public void TestReceivedMessages()
		{
			EDIMessage message1 = AddValidMessage(fTestManifestProvider);
			EDIMessage message2 = AddValidResponse(fTestManifestProvider);
			EDIMessage message3 = AddValidMessage(fTestManifestProvider);
			EDIMessage message4 = AddValidResponse(fTestManifestProvider);

			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			AssertEquals("There are two Received messages", 2, testCustomsManifestStatus.ReceivedMessages.Length);
			AssertEquals("The last received message-ordered", message4.PK, testCustomsManifestStatus.ReceivedMessages[0].PK);
		}

		[TestDateIncremental(0, 0, 1, 0)]
		public void TestMessagesIncludingInterchangeRejectionsChecksApplication()
		{
			var message1 = AddValidMessage(fTestManifestProvider);
			var message2 = AddValidResponse(fTestManifestProvider);
			var message3 = AddValidMessage(fTestManifestProvider);
			var message4 = AddValidResponse(fTestManifestProvider);
			message4.EM_ApplicationCode = "zzz";
			var testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			AssertEquals("There are two Received messages", 3, testCustomsManifestStatus.MessagesIncludingInterchangeRejections.Count);
			AssertEquals("The last message is message 3", message3.PK, testCustomsManifestStatus.MessagesIncludingInterchangeRejections[2].PK);
		}

		public virtual void TestCustomsEntryNumber()
		{
			AddValidMessage(fTestManifestProvider);
			Factory.Save();
			AddValidResponse(fTestManifestProvider);
			Factory.Save();

			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			AssertEquals("CustomsEntryNumber", ExpectedCustomsEntryNumber, testCustomsManifestStatus.E2_CustomsEntryNumber);
		}

		public void TestLastMessageDate()
		{
			AddValidMessage(fTestManifestProvider);
			Factory.Save();
			EDIMessage responseMessage = AddValidResponse(fTestManifestProvider);
			Factory.Save();
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			AssertEquals("Last Response Date", GetExpectedLastMessageDate(responseMessage), testCustomsManifestStatus.E2_LastResponseDate);
		}

		public void TestHasNoMessageSent()
		{
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			AssertEquals("There is no message sent for test manifest provider", ManifestStatus.NotSent, testCustomsManifestStatus.LastMessageStatus);
		}

		public virtual void TestIsWaitingForResponse()
		{
			IManifestProvider manifestProvider = NewManifestProvider();
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(manifestProvider);

			AddValidMessage(manifestProvider);
			Factory.Save();

			AssertEquals("Test manifest provider is waiting for response", ManifestStatus.AwaitingResponse, testCustomsManifestStatus.LastMessageStatus);

			AddValidResponse(manifestProvider);
			Factory.Save();

			Assert("Test manifest provider is NOT waiting for response", testCustomsManifestStatus.LastMessageStatus != ManifestStatus.AwaitingResponse);
			Assert("Test manifest provider is sent", testCustomsManifestStatus.LastMessageStatus != ManifestStatus.NotSent);
		}

		public void TestMessagesLoadedAfterFactorySave()
		{
			IManifestProvider manifestProvider = NewManifestProvider();
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(manifestProvider);

			AddValidMessage(manifestProvider);
			Factory.Save();

			AssertEquals("There should be a message now without calling Factory.Load", 1, testCustomsManifestStatus.OrderedMessages.Length);
		}

		public void TestMessagesIncludingInterchangeRejections()
		{
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(NewManifestProvider());
			AssertNotNull(testCustomsManifestStatus.MessagesIncludingInterchangeRejections);
		}

		public void TestCanWeSendAnOriginal()
		{
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			Assert(testCustomsManifestStatus.CanWeSendAnOriginal);
			AddValidMessage(fTestManifestProvider);
			Assert(!testCustomsManifestStatus.CanWeSendAnOriginal);
		}

		public virtual void TestDeclareManifestWhenWeHaveChanges()
		{
			TestDeclareManifestHasChanges(true);
		}

		public virtual void TestDeclareManifestWhenWeDontHaveChanges()
		{
			TestDeclareManifestHasChanges(false);
		}

		protected virtual void TestDeclareManifestHasChanges(bool hasChanges)
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			fTestManifestProvider.HasChanges = hasChanges;
			testCustomsManifestStatus.DeclareManifest(sender);
			AssertEquals("Has Changes Error", hasChanges, sender.InvalidOperationText == "You must save the current record before generating a message");
		}

		public virtual void TestDeclareManifestWhenWeHaveErrors()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			fTestManifestProvider.HasChanges = false;
			testCustomsManifestStatus.DeclareManifest(sender);
			Assert("Unable to send", sender.InvalidOperationText.IndexOf("Unable to send") != -1);
		}

		public virtual void TestDeclareManifestWhenWeAreWaitingForAResponse()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			AddValidMessage(fTestManifestProvider);
			fTestManifestProvider.HasChanges = false;
			testCustomsManifestStatus.DeclareManifest(sender);
			Assert("Unable to send", sender.InvalidOperationText.IndexOf("There is a message pending. Please wait for the Customs response.") != -1);
		}

		public virtual void TestWithdrawWithChanges()
		{
			TestWithdrawManifestHasChanges(true);
		}

		public virtual void TestWithdrawWithoutChanges()
		{
			TestWithdrawManifestHasChanges(false);
		}

		protected virtual void TestWithdrawManifestHasChanges(bool hasChanges)
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			AddValidMessage(fTestManifestProvider);
			AddValidResponse(fTestManifestProvider);
			fTestManifestProvider.HasChanges = hasChanges;
			testCustomsManifestStatus.WithdrawManifest(sender);
			AssertEquals("Has Changes Error", hasChanges, sender.InvalidOperationText == "Please save the changes first. Otherwise messages cannot be sent.");
		}

		public virtual void TestWithdrawWhenHasNoMessages()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			fTestManifestProvider.HasChanges = false;
			testCustomsManifestStatus.WithdrawManifest(sender);
			AssertEquals("Has Changes Error", true, sender.InvalidOperationText == "There are no messages to withdraw.");
		}

		public virtual void TestWithdrawWhenWaitingForAResponse()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			CustomsManifestStatus testCustomsManifestStatus = NewCustomsManifestStatus(fTestManifestProvider);
			AddValidMessage(fTestManifestProvider);
			fTestManifestProvider.HasChanges = false;
			testCustomsManifestStatus.WithdrawManifest(sender);
			AssertEquals("Has Changes Error", true, sender.InvalidOperationText == "There is a message pending. Please wait for the Customs response.");
		}

		public virtual void TestDeclareManifestWhenThereAreEnvironmentErrors()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			TestCustomsManifestStatus testCustomsManifestStatus = new TestCustomsManifestStatus(fTestManifestProvider);
			testCustomsManifestStatus.IsTestingEnvironmentErrors = true;
			testCustomsManifestStatus.DeclareManifest(sender);
			AssertEquals("Has Environment Validation Error", "error", sender.InvalidOperationText);
		}

		public virtual void TestWithdrawManifestWhenThereAreEnvironmentErrors()
		{
			SendsMessagesToCustomsShutterUpperer sender = new SendsMessagesToCustomsShutterUpperer(false);
			TestCustomsManifestStatus testCustomsManifestStatus = new TestCustomsManifestStatus(fTestManifestProvider);
			testCustomsManifestStatus.IsTestingEnvironmentErrors = true;
			testCustomsManifestStatus.WithdrawManifest(sender);
			AssertEquals("Has Environment Validation Error", "error", sender.InvalidOperationText);
		}

		[ExpectNoExceptions()]
		public void TestPreSaveValidationDoesntBlowUp()
		{
			TestCustomsManifestStatus testCustomsManifestStatus = new TestCustomsManifestStatus(fTestManifestProvider);
			testCustomsManifestStatus.RunPreSaveValidation();
		}

		protected abstract ZString ExpectedCustomsEntryNumber { get; }
		protected virtual ZDateTime GetExpectedLastMessageDate(EDIMessage message)
		{
			return message.EM_SystemCreateTimeUtc;
		}

		protected abstract EDIMessage AddValidMessage(IManifestProvider manifestProvider);
		protected abstract EDIMessage AddValidResponse(IManifestProvider manifestProvider);
		protected abstract IManifestProvider NewManifestProvider();

		#region Implementation

		IManifestProvider fTestManifestProvider;

		protected override BusinessObjectFactory NewFactory()
		{
			return new DeclarationsCreatedCancelledBusinessObjectFactory();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			IManifestProvider manifestProvider = NewManifestProvider();
			return NewCustomsManifestStatus(manifestProvider);
		}

		protected virtual CustomsManifestStatus NewCustomsManifestStatus(IManifestProvider manifestProvider)
		{
			CustomsManifestStatus result = null;
			foreach (ConstructorInfo constructor in GetExpectedBusinessObjectType().GetConstructors())
			{
				if (constructor.GetParameters().Length == 1 &&
					typeof(IManifestProvider).IsAssignableFrom(constructor.GetParameters()[0].ParameterType))
				{
					result = (CustomsManifestStatus)constructor.Invoke(new object[] { manifestProvider });
				}
			}
			if (result == null)
			{
				Fail("You must provide a constructor that takes a single argument of type " + nameof(IManifestProvider));
			}
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			fTestManifestProvider = NewManifestProvider();
		}

		protected internal class TestMessageBuilder : IManifestMessageBuilder
		{
			#region IManifestMessageBuilder Members

			public bool IsTestingErrorSituation;
			public int ErrorCount
			{
				get
				{
					return IsTestingErrorSituation ? 1 : 0;
				}
			}

			public string Errors
			{
				get { return ""; }
			}

			public ZString ManifestMessageTypeCode
			{
				get { return "XXX"; }
			}

			#endregion

			#region IMessageBuilder Members

			public Messaging.Business.MessageBuilders.IMessageBuilderResult PopulateMessages()
			{
				return null;
			}

			public ZString[] GeneratedMessageStrings
			{
				get { return new ZString[] { "" }; }
			}
			#endregion
		}

		protected internal class TestCustomsManifestStatus : CustomsManifestStatus
		{
			public TestCustomsManifestStatus(IManifestProvider manifestProvider) : base(manifestProvider)
			{
			}

			public bool IsTestingEnvironmentErrors;
			protected override string ValidateEnvironmentForSendingManifests(ISendsMessagesToCustoms sender)
			{
				return IsTestingEnvironmentErrors ? "error" : "";
			}

			TestMessageBuilder fMessageBuilder;
			public TestMessageBuilder MessageBuilder
			{
				get
				{
					if (fMessageBuilder == null)
					{
						fMessageBuilder = new TestMessageBuilder();
					}
					return fMessageBuilder;
				}
			}

			public override IManifestMessageBuilder NewCreateOrReplaceMessageBuilder()
			{
				return MessageBuilder;
			}

			protected override IManifestMessageBuilder[] NewMessageBuilders(MessageSubTypes messageType)
			{
				return new IManifestMessageBuilder[] { MessageBuilder };
			}

			protected override ManifestStatus GetMessageStatus(EDIMessage message)
			{
				if (message.EM_Status == "AWT")
				{
					return ManifestStatus.AwaitingResponse;
				}
				else
				{
					return ManifestStatus.Cleared;
				}
			}

			CusEntryNumber fEntryNumber;
			public override ZString E2_CustomsEntryNumber
			{
				get
				{
					if (fEntryNumber == null)
					{
						ZQuery filter = new ZQuery(CusEntryNumSchema.CE_ParentID, ((TestConsol)ManifestProvider).PK);
						fEntryNumber = Factory.LoadTop1<CusEntryNumber>(filter);
					}
					return fEntryNumber == null ? ZString.Empty : fEntryNumber.CE_EntryNum;
				}
			}

			public override ZString E2_CustomsEntryNumberHumanReadableName
			{
				get { return "fortest"; }
			}

			protected override string MessageApplicationCode
			{
				get { return "xxx"; }
			}

			protected override EDIMessageCollection AllMessages
			{
				get
				{
					return ((TestConsol)ManifestProvider).Messages;
				}
			}
		}
		#endregion
	}
}
