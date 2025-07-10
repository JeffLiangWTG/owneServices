using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using BelgianPortsConstants = Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE.BelgianPortsConstants;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.BE.Testing
{
	sealed class DangerousGoodsNotificationMessagingExtensionsTest : TestCaseWithFactory
	{
		void CreateLog(ForwardingConsol consol, Event @event, ZDateTime time, params KeyValuePair<string, string>[] parameters)
		{
			consol.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, time.ToOffset(), "", parameters);
			Thread.Sleep(1);
			Factory.Save();
		}

		#region TestContinueWithSendingMessage_NoIFTDGNSent

		public void TestContinueWithSendingMessage_NoIFTDGNSent()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "";

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("allow to send originals", true, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("A message for this dangerous goods notification has previously been sent. This might result in duplicate notifications. Resending this message is not allowed.", "Information"), Times.Never);
		}

		#endregion

		#region TestContinueWithSendingMessage_IFTDGNSentNoReply

		public void TestContinueWithSendingMessage_IFTDGNSentNoReply()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "";

			CreateLog(consol,
				Events.MessageSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("disallow sending dangerousGoodsNotification when a response has not been received for a previous IFTDGN", false, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("A message for this dangerous goods notification has previously been sent. This might result in duplicate notifications. Resending this message is not allowed.", "Information"), Times.Once);
		}

		#endregion

		#region TestContinueWithSendingMessage_IFTDGNSentAndReceivedRejection

		public void TestContinueWithSendingMessage_IFTDGNSentAndReceivedRejection()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "";

			CreateLog(consol,
				Events.MessageSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));
			CreateLog(consol,
				Events.MessageRejected,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment));

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("allow to send originals", true, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("A message for this dangerous goods notification has previously been sent. This might result in duplicate notifications. Resending this message is not allowed.", "Information"), Times.Never);
		}

		#endregion

		#region TestContinueWithSendingMessage_IFTDGNSentAgainAfterReceivingRejection

		public void TestContinueWithSendingMessage_IFTDGNSentAgainAfterReceivingRejection()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "";

			CreateLog(consol,
				Events.MessageSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));

			CreateLog(consol,
				Events.MessageRejected,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment));

			CreateLog(consol,
				Events.MessageSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("disallow sending dangerousGoodsNotification when a response has not been received for a previous IFTDGN", false, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("A message for this dangerous goods notification has previously been sent. This might result in duplicate notifications. Resending this message is not allowed.", "Information"), Times.Once);
		}

		#endregion

		#region TestContinueWithSendingMessage_IFTDGNSentAfterWithDrawCancelRequest

		public void TestContinueWithSendingMessage_IFTDGNSentAfterWithDrawCancelRequest()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "";

			CreateLog(consol,
				Events.MessageSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));

			CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment));

			CreateLog(consol,
				Events.MessageWithdrawCancelRequest,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("allow to send originals", true, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("A message for this dangerous goods notification has previously been sent. This might result in duplicate notifications. Resending this message is not allowed.", "Information"), Times.Never);
		}

		#endregion

		#region TestContinueWithSendingMessage_IFTDGNSentAfterWithDrawAccepted

		public void TestContinueWithSendingMessage_IFTDGNSentAfterWithDrawAccepted()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "";

			CreateLog(consol,
				Events.MessageSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));

			CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment));

			CreateLog(consol,
				Events.MessageWithdrawCancelRequest,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));

			CreateLog(consol,
				Events.MessageWithdrawCancelAccepted,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment));

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("allow to send originals", true, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("A message for this dangerous goods notification has previously been sent. This might result in duplicate notifications. Resending this message is not allowed.", "Information"), Times.Never);
		}

		#endregion

		#region TestContinueWithSendingAmendment_IFTDGNSentReceivedAcceptance

		public void TestContinueWithSendingAmendment_IFTDGNSentReceivedAcceptance()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "DGN012345678";

			CreateLog(consol,
				Events.MessageSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));

			CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment));

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessage(notifications.Object);

			AssertEquals("allow to send 1 orignal and 1 amendment", true, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("A message for this dangerous goods notification has previously been sent. This might result in duplicate notifications. Resending this message is not allowed.", "Information"), Times.Never);
		}

		#endregion

		#region TestIsSendingAmendment_NoIFTDGNSent

		public void TestIsSendingAmendment_NoIFTDGNSent()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "";

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.IsSendingAmendment();

			AssertEquals("The IFTDGN has not been sent yet so we're not sending an amendment", false, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("A message for this dangerous goods notification has previously been sent. This might result in duplicate notifications. Resending this message is not allowed.", "Information"), Times.Never);
		}

		#endregion

		#region TestIsSendingAmendment_IFTDGNAlreadySentAndAccepted

		public void TestIsSendingAmendment_IFTDGNAlreadySentAndAccepted()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "DGN012345678";

			CreateLog(consol,
				Events.MessageSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));

			CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment));

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.IsSendingAmendment();

			AssertEquals("The IFTDGN has been sent and accepted so we're sending an amendment", true, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
		}

		#endregion

		#region TestGetRequireMessageAmendmentReason_NoIFTDGNSent

		public void TestGetRequireMessageAmendmentReason_NoIFTDGNSent()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "";

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.GetRequireMessageAmendmentReason();

			AssertEquals("The IFTDGN has not been sent yet so we're not requiring an amendment reason", false, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("A message for this dangerous goods notification has previously been sent. This might result in duplicate notifications. Resending this message is not allowed.", "Information"), Times.Never);
		}

		#endregion

		#region TestGetRequireMessageAmendmentReason_IFTDGNAlreadySentAndAccepted

		public void TestGetRequireMessageAmendmentReason_IFTDGNAlreadySentAndAccepted()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "DGN012345678";

			CreateLog(consol,
				Events.MessageSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));

			CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment));

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.GetRequireMessageAmendmentReason();

			AssertEquals("The IFTDGN has been sent and accepted so we're requiring an amendment reason", true, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
		}

		#endregion

		#region TestContinueWithSendingMessageWithdrawal_NoIFTDGNAccepted_CannotWithdraw
		public void TestContinueWithSendingMessageWithdrawal_NoIFTDGNAccepted_CannotWithDraw()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "";

			CreateLog(consol,
				Events.MessageSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("The IFTDGN has not been sent or approved yet so we're not able to cancel it", null, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
		}
		#endregion

		#region TestContinueWithSendingMessageWithdrawal_IFTDGNAccepted_CanWithdraw
		public void TestContinueWithSendingMessageWithdrawal_IFTDGNAccepted_CanWithdraw()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "DGN012345678";

			CreateLog(consol,
				Events.MessageSent,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.SendingDepartment));

			CreateLog(consol,
				Events.MessageAccepted,
				new ZDateTime(2020, 02, 02),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, BelgianPortsConstants.DocumentNames.IFTDGNImport),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department, DangerousGoodsNotificationLogConstants.ReceivingDepartment));

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
			var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

			AssertEquals("The IFTDGN has been sent and approved before so we're able to cancel it", true, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
		}
		#endregion

		#region TestGetMessageAmendmentReason

		public void TestGetMessageAmendmentReason()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var documentActionReason = new DocumentActionReasonModel(new CodeDescriptionPairList())
			{
				ReasonCode = "ABC",
				ReasonText = "Free Reason"
			};
			var reasonSelector = new Mock<IDocumentActionReasonSelector>();
			reasonSelector
				.Setup(r => r.SelectDocumentActionReason(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ICodeDescriptionPairList>()))
				.Returns(documentActionReason);
			using (ObjectFactory.Substitute(reasonSelector.Object))
			{
				var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
				dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
				dangerousGoodsNotification.DgnSecurityNumber = "DGN012345678";
				dynamicData
					.SetupGet(d => d.Value)
					.Returns(dangerousGoodsNotification);
				document
					.SetupGet(d => d.Data)
					.Returns(dynamicData.Object);
				document.SetupGet(d => d.Name)
					.Returns("Fake Name");
				var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
				var res = extensions.GetMessageAmendmentReason() as DocumentActionReasonModel;
				AssertEquals("ABC", res.ReasonCode);
				AssertEquals("Free Reason", res.ReasonText);
				reasonSelector.Verify(fake => fake.SelectDocumentActionReason("You are re-sending the Fake Name so this message acts as a replacement. Please enter a reason for this replacement:", "Replacement Reason", It.IsAny<ICodeDescriptionPairList>()), Times.Once);
			}
		}

		#endregion

		#region TestPopulateMessageAmendmentReason

		public void TestPopulateMessageAmendmentReason()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "";

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);

			var documentActionReasonModel = new DocumentActionReasonModel(extensions.GetAmendmentOptions());
			documentActionReasonModel.ReasonCode = "CAM";
			documentActionReasonModel.ReasonText = "User provided input";

			var shipment = new UniversalDataBuss.DataObjects.Universal.Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			AssertNull("Empty shipment should not contain any Note", shipment.NoteCollection);

			var res = extensions.PopulateMessageAmendmentReason(shipment, documentActionReasonModel);

			AssertEquals("Shipment should contain 2 Notes", 2, shipment.NoteCollection.Count);
			var noteCode = shipment.NoteCollection.FirstOrDefault(note => note.Description.Equals("ReasonForMessageAmendment"));
			AssertNotNull("Shipment should have a note for ReasonForMessageAmendment", noteCode);
			AssertEquals("Shipment Note ReasonForMessageAmendment", documentActionReasonModel.ReasonCode, noteCode.NoteText);
			var noteText = shipment.NoteCollection.FirstOrDefault(note => note.Description.Equals("ReasonForMessageAmendmentFreeText"));
			AssertNotNull("Shipment should have a note for ReasonForMessageAmendmentFreeText", noteText);
			AssertEquals("Shipment Note ReasonForMessageAmendmentFreeText", documentActionReasonModel.ReasonText, noteText.NoteText);

			var documentActionReasonModel2 = new DocumentActionReasonModel(extensions.GetAmendmentOptions());
			documentActionReasonModel2.ReasonCode = "CHB";
			documentActionReasonModel2.ReasonText = "Other User provided input";

			res = extensions.PopulateMessageAmendmentReason(shipment, documentActionReasonModel2);

			AssertEquals("Shipment should still contain 2 Notes", 2, shipment.NoteCollection.Count);
			noteCode = shipment.NoteCollection.FirstOrDefault(note => note.Description.Equals("ReasonForMessageAmendment"));
			AssertNotNull("Shipment should have a note for ReasonForMessageAmendment", noteCode);
			AssertEquals("Shipment Note ReasonForMessageAmendment", documentActionReasonModel2.ReasonCode, noteCode.NoteText);
			noteText = shipment.NoteCollection.FirstOrDefault(note => note.Description.Equals("ReasonForMessageAmendmentFreeText"));
			AssertNotNull("Shipment should have a note for ReasonForMessageAmendmentFreeText", noteText);
			AssertEquals("Shipment Note ReasonForMessageAmendmentFreeText", documentActionReasonModel2.ReasonText, noteText.NoteText);
		}

		#endregion

		#region TestGetMessageWithDrawalReason

		public void TestGetMessageWithDrawalReason()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var documentActionReason = new DocumentActionReasonModel(new CodeDescriptionPairList())
			{
				ReasonCode = "ABC",
				ReasonText = "Free Reason"
			};
			var reasonSelector = new Mock<IDocumentActionReasonSelector>();
			reasonSelector
				.Setup(r => r.SelectDocumentActionReason(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ICodeDescriptionPairList>()))
				.Returns(documentActionReason);
			using (ObjectFactory.Substitute(reasonSelector.Object))
			{
				var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
				dangerousGoodsNotification.HandlingInstruction = "LDI";
				dangerousGoodsNotification.DgnSecurityNumber = "DGN012345678";
				dynamicData
					.SetupGet(d => d.Value)
					.Returns(dangerousGoodsNotification);
				document
					.SetupGet(d => d.Data)
					.Returns(dynamicData.Object);
				document.SetupGet(d => d.Name)
					.Returns("Fake Name");
				var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);
				var res = extensions.GetMessageWithdrawalReason() as DocumentActionReasonModel;
				AssertEquals("ABC", res.ReasonCode);
				AssertEquals("Free Reason", res.ReasonText);
				reasonSelector.Verify(fake => fake.SelectDocumentActionReason("You are sending a Fake Name Cancellation message. Please enter a reason for cancellation:", "Cancellation Reason", It.IsAny<ICodeDescriptionPairList>()), Times.Once);
			}
		}

		#endregion

		#region TestPopulateMessageWithdrawalReason

		public void TestPopulateMessageWithdrawalReason()
		{
			var consol = CreateConsol();
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();
			var messageInstructions = new Mock<IMessageInstructions>();

			var bizObj = Factory.New<DummyEnterpriseBusinessObject>();

			var dangerousGoodsNotification = new DangerousGoodsNotification("zzz", "zzz");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.DgnSecurityNumber = "";

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(dangerousGoodsNotification);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new DangerousGoodsNotificationMessagingExtensions(document.Object, consol);

			var documentActionReasonModel = new DocumentActionReasonModel(extensions.GetAmendmentOptions());
			documentActionReasonModel.ReasonCode = "CAM";
			documentActionReasonModel.ReasonText = "User provided input";

			var shipment = new UniversalDataBuss.DataObjects.Universal.Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			AssertNull("Empty shipment should not contain any Note", shipment.NoteCollection);

			var res = extensions.PopulateMessageWithdrawalReason(shipment, documentActionReasonModel);

			AssertEquals("Shipment should contain 2 Notes", 2, shipment.NoteCollection.Count);
			var noteCode = shipment.NoteCollection.FirstOrDefault(note => note.Description.Equals("ReasonForMessageCancellation"));
			AssertNotNull("Shipment should have a note for ReasonForMessageCancellation", noteCode);
			AssertEquals("Shipment Note ReasonForMessageCancellation", documentActionReasonModel.ReasonCode, noteCode.NoteText);
			var noteText = shipment.NoteCollection.FirstOrDefault(note => note.Description.Equals("ReasonForMessageCancellationFreeText"));
			AssertNotNull("Shipment should have a note for ReasonForMessageCancellationFreeText", noteText);
			AssertEquals("Shipment Note ReasonForMessageCancellationFreeText", documentActionReasonModel.ReasonText, noteText.NoteText);

			var documentActionReasonModel2 = new DocumentActionReasonModel(extensions.GetAmendmentOptions());
			documentActionReasonModel2.ReasonCode = "CAN";
			documentActionReasonModel2.ReasonText = "Other User provided input";

			res = extensions.PopulateMessageWithdrawalReason(shipment, documentActionReasonModel2);

			AssertEquals("Shipment should still contain 2 Notes", 2, shipment.NoteCollection.Count);
			noteCode = shipment.NoteCollection.FirstOrDefault(note => note.Description.Equals("ReasonForMessageCancellation"));
			AssertNotNull("Shipment should have a note for ReasonForMessageCancellation", noteCode);
			AssertEquals("Shipment Note ReasonForMessageCancellation", documentActionReasonModel2.ReasonCode, noteCode.NoteText);
			noteText = shipment.NoteCollection.FirstOrDefault(note => note.Description.Equals("ReasonForMessageCancellationFreeText"));
			AssertNotNull("Shipment should have a note for ReasonForMessageCancellationFreeText", noteText);
			AssertEquals("Shipment Note ReasonForMessageCancellationFreeText", documentActionReasonModel2.ReasonText, noteText.NoteText);
		}

		#endregion

		#region Implementation

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00002000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "FRPRA";
			consol.JK_RL_NKDischargePort = "BEANR";
			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			var carrierBookingRequest = consol.Notes.AddNew();
			carrierBookingRequest.ST_Description = PredefinedNoteTypes.Instance.CarrierBookingRequest.Description;
			carrierBookingRequest.ST_NoteText = "carrier booking request";

			var transport = consol.Transports.OfType<Freight.Business.Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "CNYTN";
			transport.JW_RL_NKDiscPort = "BEANR";
			transport.JW_Vessel = "COSCO NEBULA";
			transport.JW_VoyageFlight = "85475";
			transport.JW_ETD = new ZDateTime(2020, 10, 6, 8, 45, 00);
			transport.JW_ETA = new ZDateTime(2020, 10, 22, 12, 15, 00);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transport2.JW_RL_NKLoadPort = "BEANR";
			transport2.JW_RL_NKDiscPort = "BEWJG";
			transport2.JW_ETD = new ZDateTime(2020, 10, 23, 7, 35, 00);
			transport2.JW_ETA = new ZDateTime(2020, 10, 25, 15, 55, 00);
			transport2.JW_Vessel = "TRAILER";
			transport2.JW_VoyageFlight = "1-TRU-CK1";

			return consol;
		}

		#endregion
	}
}
