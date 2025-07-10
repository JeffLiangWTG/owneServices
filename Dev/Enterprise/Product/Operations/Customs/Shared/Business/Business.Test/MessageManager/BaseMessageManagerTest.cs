using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Business;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;
using NUnit.Framework;
using static Enterprise.Customs.Business.MessageManager;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseMessageManagerTest : TestCaseWithFactory
	{
		public void TestSendAnyAmendmentsNeeded()
		{
			manager.fSendAmendmentIfNeededDelegates = Array.Empty<SendAmendmentIfNeededDelegate>();
			AssertEquals("SendAmendments", true, manager.SendAnyAmendmentsNeeded(null));

			manager.fSendAmendmentIfNeededDelegates = new SendAmendmentIfNeededDelegate[] { new SendAmendmentIfNeededDelegate(AnswerNo) };
			AssertEquals("SendAmendments", false, manager.SendAnyAmendmentsNeeded(null));

			manager.fSendAmendmentIfNeededDelegates = new SendAmendmentIfNeededDelegate[] { new SendAmendmentIfNeededDelegate(AnswerYes) };
			AssertEquals("SendAmendments", true, manager.SendAnyAmendmentsNeeded(null));

			manager.fSendAmendmentIfNeededDelegates = new SendAmendmentIfNeededDelegate[] { new SendAmendmentIfNeededDelegate(AnswerYes), new SendAmendmentIfNeededDelegate(AnswerNo) };
			AssertEquals("SendAmendments", false, manager.SendAnyAmendmentsNeeded(null));
		}

		public void TestSendMessageWithErrorsAndWarnings_LoadsChildEditableObjectsBeforeValidate()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			Factory.Save();
			BaseJobDeclaration loadedDeclaration = new BusinessObjectFactory().Load<BaseJobDeclaration>(declaration.PK);

			MessageManagerTestHelper manager = new MessageManagerTestHelper(loadedDeclaration);
			AssertEquals("Child editable objects not registered for the test", false, IsInvoiceHeaderCollectionRegistered(loadedDeclaration));
			manager.SendMessage(sender, NonEmptyCollection, NonEmptyCollection, null, null);
			AssertEquals("Child editable objects registered before validating", true, IsInvoiceHeaderCollectionRegistered(loadedDeclaration));
		}

		[GuiTest]
		public void TestCannotSendDueToDeniedParty()
		{
			var dec = Factory.New<MessageManageableJobDeclaration>();
			dec.DPSFreightMovementRestricted = true;
			dec.MessageInitiator = sender;

			MessageManagerTestHelper manager = new MessageManagerTestHelper(dec);
			manager.SendMessage(sender, EmptyCollection, EmptyCollection, new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetBuilderThatReturnsNull) }, null);

			AssertEquals("No SuccessfulSendOccured", false, sender.SuccessfulSendOccured);
			AssertEquals("Warning text", "Unable to submit message due to Denied Party Screening cancellation.", sender.InvalidOperationText);
			AssertEquals("Message Not Sent", false, manager.MessageSent);

			dec.DPSFreightMovementRestricted = false;
			sender.InvalidOperationText = "";
			manager.SendMessage(sender, EmptyCollection, EmptyCollection, new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetBuilderThatReturnsNull) }, null);
			AssertEquals("SuccessfulSendOccured", true, sender.SuccessfulSendOccured);
			AssertEquals("No warning text", string.Empty, sender.InvalidOperationText);
			AssertEquals("No warning text", "Message Sent.", sender.SuccessfulSendText);
			Assert("Message Sent", manager.MessageSent);
		}

		[GuiTest]
		public void TestCancellationProptWhenCPWEnabled()
		{
			var dec = Factory.New<MessageManageableJobDeclaration>();
			dec.DPSFreightMovementRestricted = true;
			dec.MessageInitiator = sender;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;

			dec.JE_JS = shipment.PK;

			MessageManagerTestHelper manager = new MessageManagerTestHelper(dec);
			manager.SendMessage(sender, EmptyCollection, EmptyCollection, new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetBuilderThatReturnsNull) }, null);

			AssertEquals("No SuccessfulSendOccured", false, sender.SuccessfulSendOccured);
			AssertEquals("Warning text", "Unable to submit message due to Job Compliance Risk cancellation.", sender.InvalidOperationText);
			AssertEquals("Message Not Sent", false, manager.MessageSent);
		}

		public void TestSendMessagesWithNullMessageDoesntBlowUp()
		{
			sender.AnswerToContinueWithAction = false;
			manager.SendMessage(sender, EmptyCollection, EmptyCollection, new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetBuilderThatReturnsNull) }, null);
			Assert("No Errors Alerted", sender.LastErrors == null);
			Assert("No Warnings Alerted", sender.LastWarnings == null);
			Assert("Message Sent", manager.MessageSent);
			Assert("Successfuil Send Notified", sender.SuccessfulSendOccured);
		}

		public void TestSendMessageWithWarnings()
		{
			sender.AnswerToContinueWithAction = false;
			manager.SendMessage(sender, EmptyCollection, NonEmptyCollection, null, null);
			Assert("No Errors Alerted", sender.LastErrors == null);
			Assert("Warnings Alerted", sender.LastWarnings.Count > 0);
			Assert("Message Sent", !manager.MessageSent);
			sender.AnswerToContinueWithAction = true;
			manager.SendMessage(sender, EmptyCollection, NonEmptyCollection, new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetFixedBuilder) }, null);
			Assert("Message Sent", manager.MessageSent);
			Assert("Successfuil Send Notified", sender.SuccessfulSendOccured);
		}

		public void TestShouldSendMessagesInTestModeWarning()
		{
			StringCollection emptyWarningCollection = new StringCollection();
			manager.SendMessage(sender, EmptyCollection, emptyWarningCollection, new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetFixedBuilder) }, null);
			AssertEquals(0, emptyWarningCollection.Count);
			sender.AnswerToContinueWithAction = false;
			manager.shouldSendMessagesInTestMode = true;
			manager.SendMessage(sender, EmptyCollection, emptyWarningCollection, null, null);
			AssertEquals(1, emptyWarningCollection.Count);
			AssertEquals(MessageSendingValidation.WarningWhenInTestModeText, emptyWarningCollection[0]);
		}

		public void TestSendMessageWithNoErrorsOrWarnings()
		{
			sender.AnswerToContinueWithAction = false;
			manager.SendMessage(sender, EmptyCollection, EmptyCollection, new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetFixedBuilder) }, null);
			Assert("No Errors Alerted", sender.LastErrors == null);
			Assert("No Warnings Alerted", sender.LastWarnings == null);
			Assert("Message Sent", manager.MessageSent);
			Assert("Successfuil Send Notified", sender.SuccessfulSendOccured);
		}

		public void TestMultipleMessagesBuilt()
		{
			sender.AnswerToContinueWithAction = false;
			manager.SendMessage(sender, EmptyCollection, EmptyCollection, new MessageBuilderDelegate[] { new MessageBuilderDelegate(GetFixedBuilder), new MessageBuilderDelegate(GetRandomMessageBuilder) }, null);
			Assert("No Errors Alerted", sender.LastErrors == null);
			Assert("No Warnings Alerted", sender.LastWarnings == null);
			Assert("Message Sent", manager.MessageSent);
			Assert("Successfuil Send Notified", sender.SuccessfulSendOccured);

			AssertEquals(1, fixedMessageBuildersCreated.Count);
			AssertEquals(1, randomMessageBuildersCreated.Count);
			AssertEquals("PopulateMessagesCalled", true, (((MessageBuilderTestHelper)fixedMessageBuildersCreated[0]).PopulateMessagesCalled));
			AssertEquals("PopulateMessagesCalled", true, (((MessageBuilderTestHelper)randomMessageBuildersCreated[0]).PopulateMessagesCalled));
		}

		public void TestDoChangeResultInADifferentMessageTrue()
		{
			Assert("DifferentMessage", manager.DoChangeResultInADifferentMessage(new MessageBuilderDelegate(GetRandomMessageBuilder)));
		}

		public void TestDoChangeResultInADifferentMessageFalse()
		{
			Assert("SameMessage", !manager.DoChangeResultInADifferentMessage(new MessageBuilderDelegate(GetFixedBuilder)));
		}

		public void TestDoChangeResultInADifferentMessageThrowException()
		{
			Assert("DifferentMessage", manager.DoChangeResultInADifferentMessage(new MessageBuilderDelegate(GetBuilderThatThrowsException)));
			AssertNotNull("ExceptionReported", ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}

		public void TestSendAnAmendmentIfNeededMessageNotDeclared()
		{
			bool amendmentSent = manager.SendAnAmendmentIfNeeded(sender, false, false, new MessageBuilderDelegate(GetRandomMessageBuilder), null, EmptyCollection, EmptyCollection);
			Assert(amendmentSent);
		}

		public void TestSendAnAmendmentIfNeededWaitingForResponseWithChanges()
		{
			bool amendmentSent = manager.SendAnAmendmentIfNeeded(sender, true, true, new MessageBuilderDelegate(GetRandomMessageBuilder), "Message", EmptyCollection, EmptyCollection);
			Assert(!amendmentSent);
			AssertEquals("Invalid Op Text", "Changes you have made affect the Message already declared to Customs.  You may not save because you are still waiting for a response from Customs.", sender.InvalidOperationText);
		}

		public void TestSendAnAmendmentIfNeededWaitingForResponseWithoutChanges()
		{
			bool amendmentSent = manager.SendAnAmendmentIfNeeded(sender, true, true, new MessageBuilderDelegate(GetFixedBuilder), null, EmptyCollection, EmptyCollection);
			Assert(amendmentSent);
		}

		public void TestSendAnAmendmentIfNeededWithErrors()
		{
			bool amendmentSent = manager.SendAnAmendmentIfNeeded(sender, true, false, new MessageBuilderDelegate(GetRandomMessageBuilder), null, EmptyCollection, NonEmptyCollection);
			Assert(!amendmentSent);
		}

		public void TestSendAnAmendmentIfNeededWithWarningsAnsweringYes()
		{
			sender.AnswerToContinueWithAction = true;
			bool amendmentSent = manager.SendAnAmendmentIfNeeded(sender, true, false, new MessageBuilderDelegate(GetRandomMessageBuilder), null, NonEmptyCollection, EmptyCollection);
			Assert("Warnings", sender.LastWarnings.Count > 0);
			Assert(amendmentSent);
		}

		public void TestSendAnAmendmentIfNeededWithWarningsAnsweringNo()
		{
			sender.AnswerToContinueWithAction = false;
			bool amendmentSent = manager.SendAnAmendmentIfNeeded(sender, true, false, new MessageBuilderDelegate(GetRandomMessageBuilder), null, NonEmptyCollection, EmptyCollection);
			Assert(!amendmentSent);
		}

		#region Implementation

		StringCollection EmptyCollection
		{
			get { return new StringCollection(); }
		}

		StringCollection NonEmptyCollection
		{
			get
			{
				StringCollection result = new StringCollection();
				result.Add("Value");
				return result;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Factory.Save();
			manager = new MessageManagerTestHelper(declaration);
			sender = new SendsMessagesToCustomsShutterUpperer(false);
		}

		MessageManagerTestHelper manager;
		SendsMessagesToCustomsShutterUpperer sender;

		bool AnswerYes(ISendsMessagesToCustoms sender)
		{
			return true;
		}

		bool AnswerNo(ISendsMessagesToCustoms sender)
		{
			return false;
		}

		IMessageBuilder GetRandomMessageBuilder(BusinessObject master)
		{
			IMessageBuilder result = new MessageBuilderTestHelper(Guid.NewGuid().ToString());
			randomMessageBuildersCreated.Add(result);
			return result;
		}

		readonly ArrayList randomMessageBuildersCreated = new ArrayList();

		IMessageBuilder GetFixedBuilder(BusinessObject master)
		{
			IMessageBuilder result = new MessageBuilderTestHelper("MessageText");
			fixedMessageBuildersCreated.Add(result);
			return result;
		}

		readonly ArrayList fixedMessageBuildersCreated = new ArrayList();

		IMessageBuilder GetBuilderThatThrowsException(BusinessObject master)
		{
			MessageBuilderTestHelper result = new MessageBuilderTestHelper("MessageText");
			result.ThrowExceptionOnBuild = true;
			return result;
		}

		IMessageBuilder GetBuilderThatReturnsNull(BusinessObject master)
		{
			MessageBuilderTestHelper builder = new MessageBuilderTestHelper("MessageText");
			builder.ReturnNullOnBuild = true;
			return builder;
		}

		bool IsInvoiceHeaderCollectionRegistered(BaseJobDeclaration declaration)
		{
			IBusiness[] children = ((IBusiness)declaration).Children;
			return Array.Exists(children, delegate(IBusiness child)
			{ return child is InvoiceHeaderActiveCollection; });
		}

		#region MessageManagerTestHelper

		protected class MessageManagerTestHelper : MessageManager
		{
			public MessageManagerTestHelper(BaseJobDeclaration declaration)
				: base(false)
			{
				this.declaration = declaration;
			}

			protected override BusinessObject Master
			{
				get { return declaration; }
			}

			protected BaseJobDeclaration declaration;

			public SendAmendmentIfNeededDelegate[] fSendAmendmentIfNeededDelegates;
			protected override SendAmendmentIfNeededDelegate[] SendAmendmentIfNeededDelegates
			{
				get { return fSendAmendmentIfNeededDelegates; }
			}

			protected override void OnMessageSent(ISendsMessagesToCustoms sender)
			{
				base.OnMessageSent(sender);
				MessageSent = true;
			}

			protected override bool ShouldSendMessagesInTestMode
			{
				get { return shouldSendMessagesInTestMode; }
			}
			public bool shouldSendMessagesInTestMode;

			public bool MessageSent;
		}

		#endregion

		#region MessageBuilderTestHelper

		protected class MessageBuilderTestHelper : IMessageBuilder
		{
			public MessageBuilderTestHelper(string messageText)
			{
				this.messageText = messageText;
			}

			public bool ThrowExceptionOnBuild;
			public bool ReturnNullOnBuild;
			public bool PopulateMessagesCalled;

			#region IMessageBuilder Members
			public IMessageBuilderResult PopulateMessages()
			{
				if (ThrowExceptionOnBuild)
				{
					throw new ApplicationException("Exception Message");
				}
				else if (ReturnNullOnBuild)
				{ return null; }
				else
				{
					PopulateMessagesCalled = true;
					var builderResult = new BuilderResult(null, new List<string>(), null);
					builderResult.Message = new BusinessObjectFactory().New<EDIMessage>();
					builderResult.Message.EM_MessageText = messageText;
					var result = new MessageBuilderResult();
					result.AddBuilderResult(builderResult);
					return result;
				}
			}
			#endregion

			#region Implementation

			protected string messageText;

			#endregion
		}

		#endregion

		#endregion
	}
}
