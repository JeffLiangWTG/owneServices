using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	public abstract class OceanBookingMessagingValidationTest : TestCaseWithFactory
	{
		protected abstract IMessagingExtensions GetMessageExtension(IDocument document, ForwardingConsol consol);

		protected abstract string DocumentName { get; }

		protected abstract ZString DocumentDataStoreName { get; }

		protected abstract Mock<IDocument> MockDocument(ForwardingConsol consol);

		protected abstract void SetShippingLineAvailableForMessaging(RefShippingLine shippingLine);

		protected virtual ZBool ShouldPopulateBookingRequestRequiredErrorMessage => false;

		protected virtual ZBool ShouldPopulateCarrierChangedErrorMessage => true;

		#region ContinueWithSendingMessage

		public void TestContinueWithSendingMessage_WithInterchangeRejected()
		{
			var notifications = new Mock<IUserNotifications>();
			var consol = Factory.New<ForwardingConsol>();
			var document = MockDocument(consol);
			var documentData = PrepareDocumentData(consol);
			CreateEvent(documentData, AutoEvents.MessageSent);
			CreateEvent(documentData, AutoEvents.InterchangeRejected);
			var extensions = GetMessageExtension(document.Object, consol);

			AssertEquals(false, extensions.IsSendingAmendment());
			AssertEquals(null, extensions.ContinueWithSendingMessage(notifications.Object));
			AssertEquals(1, documentData.CalculateDataVersion(DocumentName, true));
		}

		public void TestContinueWithSendingMessage_WithMessageRejected()
		{
			var notifications = new Mock<IUserNotifications>();
			var consol = Factory.New<ForwardingConsol>();
			var document = MockDocument(consol);
			var documentData = PrepareDocumentData(consol);
			CreateEvent(documentData, AutoEvents.MessageSent);
			CreateEvent(documentData, AutoEvents.MessageRejected);
			var extensions = GetMessageExtension(document.Object, consol);

			AssertEquals(false, extensions.IsSendingAmendment());
			AssertEquals(null, extensions.ContinueWithSendingMessage(notifications.Object));
			AssertEquals(1, documentData.CalculateDataVersion(DocumentName, true));
		}

		public void TestContinueWithSendingMessage_WithInterchangeReceiptAcknowledged()
		{
			var message = "An withdraw/cancellation message has been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.";
			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				notifications.Setup(x => x.ShowMessage(message, "Confirmation"));
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageWithdrawCancelRequest);
				CreateEvent(documentData, AutoEvents.InterchangeReceiptAcknowledged);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(false, extensions.IsSendingAmendment());
				AssertEquals(false, extensions.ContinueWithSendingMessage(notifications.Object));
				notifications.Verify(x => x.ShowMessage(message, "Confirmation"), Times.Once);
				AssertEquals(1, documentData.CalculateDataVersion(DocumentName, true));
			}
		}

		public void TestContinueWithSendingMessage_WithMessageWithdrawCancelAccepted()
		{
			var message = "An withdraw/cancellation message has been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.";
			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				notifications.Setup(x => x.ShowMessage(message, "Confirmation"));
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageWithdrawCancelRequest);
				CreateEvent(documentData, AutoEvents.MessageWithdrawCancelAccepted);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(false, extensions.IsSendingAmendment());
				AssertEquals(false, extensions.ContinueWithSendingMessage(notifications.Object));
				notifications.Verify(x => x.ShowMessage(message, "Confirmation"), Times.Once);
				AssertEquals(1, documentData.CalculateDataVersion(DocumentName, true));
			}
		}

		public void TestContinueWithSendingMessage_WithMessageAccepted()
		{
			var message = "An withdraw/cancellation message has been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.";

			var notifications = new Mock<IUserNotifications>();
			var consol = Factory.New<ForwardingConsol>();
			var document = MockDocument(consol);
			var documentData = PrepareDocumentData(consol);
			notifications.Setup(x => x.ShowMessage(message, "Confirmation"));
			CreateEvent(documentData, AutoEvents.MessageSent);
			CreateEvent(documentData, AutoEvents.MessageAccepted);
			CreateEvent(documentData, AutoEvents.MessageWithdrawCancelRequest);
			CreateEvent(documentData, AutoEvents.MessageAccepted);

			var extensions = GetMessageExtension(document.Object, consol);

			AssertEquals(false, extensions.IsSendingAmendment());
			AssertEquals(false, extensions.ContinueWithSendingMessage(notifications.Object));
			notifications.Verify(x => x.ShowMessage(message, "Confirmation"), Times.Once);
			AssertEquals(1, documentData.CalculateDataVersion(DocumentName, true));
		}

		public void TestContinueWithSendingMessage_PromptForValidCarrierOnOceanCarrierMessaging()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var notifications = new Mock<IUserNotifications>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";

				var carrier = Factory.New<OrgHeader>();
				carrier.OH_FullName = "MAERSK";

				var coLoadWith = Factory.New<OrgHeader>();
				coLoadWith.OH_FullName = "EVERGREEN";

				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				var document = MockDocument(consol);

				var extensions = GetMessageExtension(document.Object, consol);
				var res = extensions.ContinueWithSendingMessage(notifications.Object);

				AssertEquals("disallow sending message when the carrier doesn't have messaging capability", false, res);

				notifications.Verify(x => x.ShowMessage($@"You are trying to send the {DocumentName} message to MAERSK that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Once);

				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
				consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;

				res = extensions.ContinueWithSendingMessage(notifications.Object);

				AssertEquals("disallow sending message when the carrier doesn't have messaging capability", false, res);

				notifications.Verify(x => x.ShowMessage($@"You are trying to send the {DocumentName} message to EVERGREEN that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Once);
			}
		}

		public void TestContinueWithSendingMessage_NoPromptForValidCarrierOnOceanCarrierMessaging()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var notifications = new Mock<IUserNotifications>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";

				var carrier = Factory.New<OrgHeader>();
				carrier.OH_FullName = "MAERSK";
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				SetShippingLineAvailableForMessaging(shippingLine);
				carrier.OH_RSL_ShippingLine = shippingLine.PK;

				var coLoadWith = Factory.New<OrgHeader>();
				coLoadWith.OH_FullName = "EVERGREEN";
				coLoadWith.OH_RSL_ShippingLine = shippingLine.PK;

				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				var document = MockDocument(consol);

				var extensions = GetMessageExtension(document.Object, consol);
				var res = extensions.ContinueWithSendingMessage(notifications.Object);

				AssertEquals("disallow sending message when the carrier doesn't have messaging capability", false, res);

				notifications.Verify(x => x.ShowMessage($@"You are trying to send the {DocumentName} message to MAERSK that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Never);

				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
				consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;

				res = extensions.ContinueWithSendingMessage(notifications.Object);

				AssertEquals("disallow sending message when the carrier doesn't have messaging capability", false, res);

				notifications.Verify(x => x.ShowMessage($@"You are trying to send the {DocumentName} message to EVERGREEN that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Never);
			}
		}

		public void TestContinueWithSendingMessage_PromptForValidCarrier_WithBookingAgent_OnOceanCarrierMessaging()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var notifications = new Mock<IUserNotifications>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";

				var carrier = Factory.New<OrgHeader>();
				carrier.OH_FullName = "MAERSK";
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_BookingRequestAvailable = false;
				shippingLine.RSL_ShippingInstructionAvailable = false;
				shippingLine.RSL_ShippingOrderAvailable = false;
				carrier.OH_RSL_ShippingLine = shippingLine.PK;
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				var bookingAgent = Factory.New<OrgHeader>();
				bookingAgent.OH_FullName = "MAERSL";
				var bookingAgentShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				bookingAgent.OH_RSL_ShippingLine = bookingAgentShippingLine.PK;
				bookingAgentShippingLine.RSL_ShippingOrderAvailable = false;

				consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

				var document = MockDocument(consol);

				var extensions = GetMessageExtension(document.Object, consol);
				var res = extensions.ContinueWithSendingMessage(notifications.Object);

				AssertEquals("disallow sending message when the carrier doesn't have messaging capability", false, res);

				if (DocumentName == ConsolDocumentNames.ShippingOrder)
				{
					notifications.Verify(x => x.ShowMessage($@"You are trying to send the {DocumentName} message to MAERSL that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Once);
				}
				else
				{
					notifications.Verify(x => x.ShowMessage($@"You are trying to send the {DocumentName} message to MAERSK that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Once);
				}
			}
		}

		public void TestContinueWithSendingMessage_NoPromptForValidCarrier_WithBookingAgent_OnOceanCarrierMessaging()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var notifications = new Mock<IUserNotifications>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";

				var carrier = Factory.New<OrgHeader>();
				carrier.OH_FullName = "MAERSK";
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				SetShippingLineAvailableForMessaging(shippingLine);
				carrier.OH_RSL_ShippingLine = shippingLine.PK;
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				var bookingAgent = Factory.New<OrgHeader>();
				bookingAgent.OH_FullName = "MAERSL";
				var bookingAgentShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				bookingAgent.OH_RSL_ShippingLine = bookingAgentShippingLine.PK;
				bookingAgentShippingLine.RSL_ShippingOrderAvailable = true;

				consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

				var document = MockDocument(consol);

				var extensions = GetMessageExtension(document.Object, consol);
				var res = extensions.ContinueWithSendingMessage(notifications.Object);

				AssertEquals("disallow sending message when the carrier doesn't have messaging capability", false, res);

				notifications.Verify(x => x.ShowMessage($@"You are trying to send the {DocumentName} message to MAERSL that cannot receive this message electronically.
Please raise an eRequest in your system if you’d like us to contact this carrier for future enablement.", "Confirmation"), Times.Never);
			}
		}
		public void TestContinueWithSendingMessage_PromptForInvalidRoutingRule()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var notifications = new Mock<IUserNotifications>();
				var routingRuleValidator = new Mock<IRoutingRuleValidator>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";

				var carrier = Factory.New<OrgHeader>();
				carrier.OH_FullName = "MAERSK";

				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				SetShippingLineAvailableForMessaging(shippingLine);

				carrier.OH_RSL_ShippingLine = shippingLine.PK;

				var coLoadWith = Factory.New<OrgHeader>();
				coLoadWith.OH_FullName = "EVERGREEN";
				coLoadWith.OH_RSL_ShippingLine = shippingLine.PK;

				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				var document = MockDocument(consol);

				var expectedRoutingRuleMessage = $@"Your {DocumentName} message to this carrier is being routed via INTTRA, however a direct connection exists between CargoWise and this carrier. Be advised that the CargoWise direct connection will replace the current means of communication to this carrier by the end of 2023. Please raise a CR9 / Ocean Carrier Messaging and follow the process to request all your global branches to be migrated to the direct connection for this carrier.";
				routingRuleValidator
				.Setup(r => r.IsValidWithRecipientIdsRetrieved(It.IsAny<string>()))
				.Returns((ValidationResult: true, RecipientIds: new string[1] { "MAERSK" }));

				using (ObjectFactory.Substitute(routingRuleValidator.Object))
				{
					var extensions = GetMessageExtension(document.Object, consol);
					var res = extensions.ContinueWithSendingMessage(notifications.Object);

					AssertEquals("allow sending message", null, res);

					notifications.Verify(x => x.ShowMessage(expectedRoutingRuleMessage, "Confirmation"), Times.Never);
				}

				routingRuleValidator
				.Setup(r => r.IsValidWithRecipientIdsRetrieved(It.IsAny<string>()))
				.Returns((ValidationResult: true, RecipientIds: new string[1] { "INTTRA" }));

				using (ObjectFactory.Substitute(routingRuleValidator.Object))
				{
					var extensions = GetMessageExtension(document.Object, consol);
					var res = extensions.ContinueWithSendingMessage(notifications.Object);

					AssertEquals("allow sending message", null, res);

					notifications.Verify(x => x.ShowMessage(expectedRoutingRuleMessage, "Confirmation"), Times.Once);
				}

				routingRuleValidator
					.Setup(r => r.IsValidWithRecipientIdsRetrieved(It.IsAny<string>()))
					.Returns((ValidationResult: false, RecipientIds: new string[1] { "DOESN'T MATTER" }));

				using (ObjectFactory.Substitute(routingRuleValidator.Object))
				{
					var extensions = GetMessageExtension(document.Object, consol);
					var res = extensions.ContinueWithSendingMessage(notifications.Object);

					AssertEquals("disallow sending message when the routing rule validation is invalid", false, res);

					notifications.Verify(x => x.ShowMessage($@"You are trying to send the {DocumentName} message to MAERSK, however your Organization/Branch is not registered with this carrier for this message type.
Please raise an eRequest and we will guide you through the registration process.", "Confirmation"), Times.Once);

					consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
					consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;

					res = extensions.ContinueWithSendingMessage(notifications.Object);

					AssertEquals("disallow sending message when the carrier doesn't have messaging capability", false, res);

					notifications.Verify(x => x.ShowMessage($@"You are trying to send the {DocumentName} message to EVERGREEN, however your Organization/Branch is not registered with this carrier for this message type.
Please raise an eRequest and we will guide you through the registration process.", "Confirmation"), Times.Once);
				}
			}
		}

		#endregion

		#region ContinueWithSendingMessageAmendmen

		public void TestContinueWithSendingMessageAmendment_WithoutResponse()
		{
			{
				var message = GetMessage("An original message has been sent");

				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				notifications.Setup(x => x.ShowConfirmation(message, "Confirmation")).Returns(true);
				CreateEvent(documentData, AutoEvents.MessageSent);
				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, true, message, Times.Once);
				AssertEquals(2, documentData.CalculateDataVersion(DocumentName, true));
			}

			{
				var message = GetMessage("An amendment message has been sent");

				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				notifications.Setup(x => x.ShowConfirmation(message, "Confirmation")).Returns(true);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
				notifications.Verify(x => x.ShowConfirmation(message, "Confirmation"), Times.Never);
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, true, message, Times.Once);
				AssertEquals(3, documentData.CalculateDataVersion(DocumentName, true));
			}
		}

		public void TestContinueWithSendingMessageAmendment_WithInterchangeRejected()
		{
			{
				var message = GetMessage("An original message has been sent");

				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				notifications.Setup(x => x.ShowConfirmation(message, "Confirmation")).Returns(true);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.InterchangeRejected);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, null, message, Times.Never);
				AssertEquals(2, documentData.CalculateDataVersion(DocumentName, true));
			}

			{
				var message = GetMessage("An amendment message has been sent");

				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				notifications.Setup(x => x.ShowConfirmation(message, "Confirmation")).Returns(true);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.InterchangeReceiptAcknowledged);
				CreateEvent(documentData, AutoEvents.InterchangeRejected);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, null, message, Times.Never);
				AssertEquals(3, documentData.CalculateDataVersion(DocumentName, true));
			}

			{
				var message = @"A withdraw/cancellation message has been sent, and there is no reply received from the recipient. Resending the message may cause errors and possibly revert to a manual process.

You may want to reset the message to original first (using ""Reset to Original"" option) if the recipient did not receive it or has not manually rejected your earlier message. Are you sure you want to send the message?";

				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageWithdrawCancelRequest);
				CreateEvent(documentData, AutoEvents.InterchangeRejected);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, null, message, Times.Never);
				AssertEquals(2, documentData.CalculateDataVersion(DocumentName, true));
			}
		}

		public void TestContinueWithSendingMessageAmendment_WithInterchangeReceiptAcknowledged()
		{
			{
				var message = GetMessage("An original message has been sent");
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.InterchangeReceiptAcknowledged);
				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
				notifications.Setup(x => x.ShowConfirmation(message, "Confirmation")).Returns(true);
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, true, message, Times.Once);
				AssertEquals(2, documentData.CalculateDataVersion(DocumentName, true));
			}

			{
				var message = GetMessage("An amendment message has been sent");

				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				notifications.Setup(x => x.ShowConfirmation(message, "Confirmation")).Returns(true);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.InterchangeReceiptAcknowledged);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, true, message, Times.Once);
				AssertEquals(3, documentData.CalculateDataVersion(DocumentName, true));
			}

			{
				var message = GetMessage("An amendment message has been sent");

				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				notifications.Setup(x => x.ShowConfirmation(message, "Confirmation")).Returns(true);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.InterchangeReceiptAcknowledged);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, true, message, Times.Once);
				AssertEquals(4, documentData.CalculateDataVersion(DocumentName, true));
			}
		}

		public void TestContinueWithSendingMessageAmendment_WithMessageRejected()
		{
			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageRejected);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, null);
				AssertEquals(2, documentData.CalculateDataVersion(DocumentName, true));
			}

			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageRejected);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, null);
				AssertEquals(3, documentData.CalculateDataVersion(DocumentName, true));
			}

			{
				var message = @"A withdraw/cancellation message has been sent, and there is no reply received from the recipient. Resending the message may cause errors and possibly revert to a manual process.

You may want to reset the message to original first (using ""Reset to Original"" option) if the recipient did not receive it or has not manually rejected your earlier message. Are you sure you want to send the message?";

				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageWithdrawCancelRequest);
				CreateEvent(documentData, AutoEvents.MessageRejected);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, null, message, Times.Never);
				AssertEquals(2, documentData.CalculateDataVersion(DocumentName, true));
			}
		}

		public void TestContinueWithSendingMessageAmendment_WithMessageAccepted()
		{
			var resetMessage = "An original message has previously been sent and received successful confirmation from the Carrier. Resetting to Original and consequent sending of the new original message will cause duplication and errors on the Carrier site. Please cancel the Booking first using “Withdraw/Cancel Message”.";
			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, null);
				AssertEquals(2, documentData.CalculateDataVersion(DocumentName, true));

				notifications.Setup(x => x.ShowMessage(resetMessage, "Confirmation"));
				if (DocumentName == ConsolDocumentNames.ShippingOrder)
				{
					AssertEquals(false, extensions.ContinueWithResetToOriginal(notifications.Object));
					notifications.Verify(x => x.ShowMessage(resetMessage, "Confirmation"), Times.Once);
				}
				else
				{
					AssertEquals(null, extensions.ContinueWithResetToOriginal(notifications.Object));
					notifications.Verify(x => x.ShowMessage(resetMessage, "Confirmation"), Times.Never);
				}
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
			}

			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, null);
				AssertEquals(3, documentData.CalculateDataVersion(DocumentName, true));

				notifications.Setup(x => x.ShowMessage(resetMessage, "Confirmation"));
				if (DocumentName == ConsolDocumentNames.ShippingOrder)
				{
					AssertEquals(false, extensions.ContinueWithResetToOriginal(notifications.Object));
					notifications.Verify(x => x.ShowMessage(resetMessage, "Confirmation"), Times.Once);
				}
				else
				{
					AssertEquals(null, extensions.ContinueWithResetToOriginal(notifications.Object));
					notifications.Verify(x => x.ShowMessage(resetMessage, "Confirmation"), Times.Never);
				}
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
			}

			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, null);
				AssertEquals(4, documentData.CalculateDataVersion(DocumentName, true));

				notifications.Setup(x => x.ShowMessage(resetMessage, "Confirmation"));
				if (DocumentName == ConsolDocumentNames.ShippingOrder)
				{
					AssertEquals(false, extensions.ContinueWithResetToOriginal(notifications.Object));
					notifications.Verify(x => x.ShowMessage(resetMessage, "Confirmation"), Times.Once);
				}
				else
				{
					AssertEquals(null, extensions.ContinueWithResetToOriginal(notifications.Object));
					notifications.Verify(x => x.ShowMessage(resetMessage, "Confirmation"), Times.Never);
					CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				}
			}
		}

		public void TestContinueWithSendingMessageAmendment_IsCarrierChangedSinceLastSent_OldMessage()
		{
			var message = GetMessage("An original message has been sent");

			var notifications = new Mock<IUserNotifications>();
			var consol = Factory.New<ForwardingConsol>();
			var document = MockDocument(consol);
			var documentData = PrepareDocumentData(consol);
			notifications.Setup(x => x.ShowConfirmation(message, "Confirmation")).Returns(true);
			CreateEvent(documentData, AutoEvents.MessageSent, string.Empty);
			var extensions = GetMessageExtension(document.Object, consol);

			AssertEquals(true, extensions.IsSendingAmendment());
			AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
			AssertEquals(2, documentData.CalculateDataVersion(DocumentName, true));

			if (ShouldPopulateCarrierChangedErrorMessage)
			{
				using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					extensions.ContinueWithSendingMessageAmendment(notifications.Object);
					notifications.Verify(x => x.ShowMessage($"You are trying to send a {DocumentName} amendment message to a new carrier. Please verify Carrier (or Co-Load With) and Reset to Original if you are sending {DocumentName} to new carrier.", "Confirmation"), Times.Never);

					var orgAddress = CreateOrgAddressWithCargoWiseOneCarrierCode("BCDF");

					consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
					consol.JK_OA_CreditorAddress = orgAddress.PK;
					consol.JK_CoLoadBookingReference = "COLOAD Booking Ref";

					extensions.ContinueWithSendingMessageAmendment(notifications.Object);
					notifications.Verify(x => x.ShowMessage($"You are trying to send a {DocumentName} amendment message to a new carrier. Please verify Carrier (or Co-Load With) and Reset to Original if you are sending {DocumentName} to new carrier.", "Confirmation"), Times.Never);
				}
			}
		}

		public void TestContinueWithSendingMessageAmendment_PreviousMessageIsWithdrawCancelRequest()
		{
			var message = @"A withdraw/cancellation message has been sent, and there is no reply received from the recipient. Resending the message may cause errors and possibly revert to a manual process.

You may want to reset the message to original first (using ""Reset to Original"" option) if the recipient did not receive it or has not manually rejected your earlier message. Are you sure you want to send the message?";
			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageWithdrawCancelRequest);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, false, message, Times.Once);
				AssertEquals(2, documentData.CalculateDataVersion(DocumentName, true));
			}

			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageWithdrawCancelRequest);
				CreateEvent(documentData, AutoEvents.MessageRejected);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, null, message, Times.Never);
				AssertEquals(2, documentData.CalculateDataVersion(DocumentName, true));
			}

			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageWithdrawCancelRequest);
				CreateEvent(documentData, AutoEvents.InterchangeRejected);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(true, extensions.IsSendingAmendment());
				AssertEquals(true, extensions.GetRequireMessageAmendmentReason());
				CheckSendingMessageAmendment_BookingRequestRequired(notifications, consol, extensions);
				CheckSendingMessageAmendment_CarrierChanged(notifications, consol, extensions);
				CheckSendingMessageAmendment_ConfirmationMessage(notifications, consol, extensions, null, message, Times.Never);
				AssertEquals(2, documentData.CalculateDataVersion(DocumentName, true));
			}

			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				CreateEvent(documentData, AutoEvents.MessageAccepted);
				CreateEvent(documentData, AutoEvents.MessageWithdrawCancelRequest);
				CreateEvent(documentData, AutoEvents.MessageAccepted);

				var extensions = GetMessageExtension(document.Object, consol);

				AssertEquals(false, extensions.IsSendingAmendment());
				AssertEquals(1, documentData.CalculateDataVersion(DocumentName, true));
			}
		}

		#endregion

		#region ContinueWithSendingMessageWithdrawal

		public void TestContinueWithSendingMessageWithdrawal_IsCarrierChangedSinceLastSent()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent);
				var extensions = GetMessageExtension(document.Object, consol);

				var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
				AssertEquals(false, res);

				if (ShouldPopulateCarrierChangedErrorMessage)
				{
					notifications.Verify(x => x.ShowMessage($"You are trying to send a {DocumentName} withdrawal message to a new carrier. Sending {DocumentName} withdrawal message to a new carrier is not allowed.", "Confirmation"), Times.Once);

					var orgAddress = CreateOrgAddressWithCargoWiseOneCarrierCode();
					consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
					consol.JK_OA_CreditorAddress = orgAddress.PK;

					res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
					AssertEquals(false, res.HasValue);

					orgAddress.Header.CustomsCodes[0].OK_CustomsRegNo = "BCDF";
					notifications = new Mock<IUserNotifications>();
					res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
					AssertEquals(false, res.Value);
					notifications.Verify(x => x.ShowMessage($"You are trying to send a {DocumentName} withdrawal message to a new carrier. Sending {DocumentName} withdrawal message to a new carrier is not allowed.", "Confirmation"), Times.Once);
				}
			}
		}

		public void TestContinueWithSendingMessageWithdrawal_IsCarrierChangedSinceLastSent_OldMessage()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var notifications = new Mock<IUserNotifications>();
				var consol = Factory.New<ForwardingConsol>();
				var document = MockDocument(consol);
				var documentData = PrepareDocumentData(consol);
				CreateEvent(documentData, AutoEvents.MessageSent, string.Empty);
				var extensions = GetMessageExtension(document.Object, consol);

				var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
				AssertEquals(false, res.HasValue);

				if (ShouldPopulateCarrierChangedErrorMessage)
				{
					notifications.Verify(x => x.ShowMessage($"You are trying to send a {DocumentName} withdrawal message to a new carrier. Sending {DocumentName} withdrawal message to a new carrier is not allowed.", "Confirmation"), Times.Never);

					var orgAddress = CreateOrgAddressWithCargoWiseOneCarrierCode();
					consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
					consol.JK_OA_CreditorAddress = orgAddress.PK;

					res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
					AssertEquals(false, res.HasValue);

					orgAddress.Header.CustomsCodes[0].OK_CustomsRegNo = "BCDF";
					notifications = new Mock<IUserNotifications>();
					res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);
					AssertEquals(false, res.HasValue);
					notifications.Verify(x => x.ShowMessage($"You are trying to send a {DocumentName} withdrawal message to a new carrier. Sending {DocumentName} withdrawal message to a new carrier is not allowed.", "Confirmation"), Times.Never);
				}
			}
		}

		public void TestContinueWithSendingMessageWithdrawal_PromptForInvalidRoutingRule()
		{
			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var notifications = new Mock<IUserNotifications>();
				var routingRuleValidator = new Mock<IRoutingRuleValidator>();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";

				var carrier = Factory.New<OrgHeader>();
				carrier.OH_FullName = "MAERSK";

				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_OceanCarrierMessagingAvailable = true;

				carrier.OH_RSL_ShippingLine = shippingLine.PK;

				var coLoadWith = Factory.New<OrgHeader>();
				coLoadWith.OH_FullName = "EVERGREEN";
				coLoadWith.OH_RSL_ShippingLine = shippingLine.PK;

				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				var document = MockDocument(consol);

				var expectedRoutingRuleMessage = $@"Your {DocumentName} message to this carrier is being routed via INTTRA, however a direct connection exists between CargoWise and this carrier. Be advised that the CargoWise direct connection will replace the current means of communication to this carrier by the end of 2023. Please raise a CR9 / Ocean Carrier Messaging and follow the process to request all your global branches to be migrated to the direct connection for this carrier.";
				routingRuleValidator
				.Setup(r => r.IsValidWithRecipientIdsRetrieved(It.IsAny<string>()))
				.Returns((ValidationResult: true, RecipientIds: new string[1] { "MAERSK" }));

				using (ObjectFactory.Substitute(routingRuleValidator.Object))
				{
					var extensions = GetMessageExtension(document.Object, consol);
					var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

					AssertEquals("allow sending message", null, res);

					notifications.Verify(x => x.ShowMessage(expectedRoutingRuleMessage, "Confirmation"), Times.Never);
				}

				routingRuleValidator
				.Setup(r => r.IsValidWithRecipientIdsRetrieved(It.IsAny<string>()))
				.Returns((ValidationResult: true, RecipientIds: new string[3] { "MAERSK", "INTTRA", "EVERGREEN" }));

				using (ObjectFactory.Substitute(routingRuleValidator.Object))
				{
					var extensions = GetMessageExtension(document.Object, consol);
					var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

					AssertEquals("allow sending message", null, res);

					notifications.Verify(x => x.ShowMessage(expectedRoutingRuleMessage, "Confirmation"), Times.Once);
				}

				routingRuleValidator
				.Setup(r => r.IsValidWithRecipientIdsRetrieved(It.IsAny<string>()))
				.Returns((ValidationResult: true, RecipientIds: new string[3] { "MAERSK", "ABC INTTRA DEF", "EVERGREEN" }));
				notifications.Reset();

				using (ObjectFactory.Substitute(routingRuleValidator.Object))
				{
					var extensions = GetMessageExtension(document.Object, consol);
					var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

					AssertEquals("allow sending message", null, res);

					notifications.Verify(x => x.ShowMessage(expectedRoutingRuleMessage, "Confirmation"), Times.Once);
				}

				routingRuleValidator
					.Setup(r => r.IsValidWithRecipientIdsRetrieved(It.IsAny<string>()))
					.Returns((ValidationResult: false, RecipientIds: new string[1] { "DOESN'T MATTER" }));

				using (ObjectFactory.Substitute(routingRuleValidator.Object))
				{
					var extensions = GetMessageExtension(document.Object, consol);
					var res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

					AssertEquals("disallow sending withdrawal when the routing rule validation is invalid", false, res);

					notifications.Verify(x => x.ShowMessage($@"You are trying to send the {DocumentName} message to MAERSK, however your Organization/Branch is not registered with this carrier for this message type.
Please raise an eRequest and we will guide you through the registration process.", "Confirmation"), Times.Once);

					consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
					consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;

					res = extensions.ContinueWithSendingMessageWithdrawal(notifications.Object);

					AssertEquals("disallow sending message withdrawal when the carrier doesn't have messaging capability", false, res);

					notifications.Verify(x => x.ShowMessage($@"You are trying to send the {DocumentName} message to EVERGREEN, however your Organization/Branch is not registered with this carrier for this message type.
Please raise an eRequest and we will guide you through the registration process.", "Confirmation"), Times.Once);
				}
			}
		}

		#endregion

		#region ContinueWithResetToOriginal

		public void TestContinueWithResetToOriginal_WithMessageAccepted()
		{
			var resetMessage = "An original message has previously been sent and received successful confirmation from the Carrier. Resetting to Original and consequent sending of the new original message will cause duplication and errors on the Carrier site. Please cancel the Booking first using “Withdraw/Cancel Message”.";
			var message = "An withdraw/cancellation message has been sent. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.";

			var notifications = new Mock<IUserNotifications>();
			var consol = Factory.New<ForwardingConsol>();
			var document = MockDocument(consol);
			var documentData = PrepareDocumentData(consol);
			notifications.Setup(x => x.ShowMessage(message, "Confirmation"));
			CreateEvent(documentData, AutoEvents.MessageSent);
			CreateEvent(documentData, AutoEvents.MessageAccepted);
			CreateEvent(documentData, AutoEvents.MessageWithdrawCancelRequest);
			CreateEvent(documentData, AutoEvents.MessageAccepted);

			var extensions = GetMessageExtension(document.Object, consol);

			AssertEquals(false, extensions.IsSendingAmendment());
			AssertEquals(1, documentData.CalculateDataVersion(DocumentName, true));

			notifications.Setup(x => x.ShowMessage(resetMessage, "Confirmation"));
			if (DocumentName == ConsolDocumentNames.ShippingOrder)
			{
				AssertEquals(false, extensions.ContinueWithResetToOriginal(notifications.Object));
				notifications.Verify(x => x.ShowMessage(resetMessage, "Confirmation"), Times.Once);
			}
			else
			{
				AssertEquals(null, extensions.ContinueWithResetToOriginal(notifications.Object));
				notifications.Verify(x => x.ShowMessage(resetMessage, "Confirmation"), Times.Never);
			}
		}

		#endregion

		#region Implementation

		void CheckSendingMessageAmendment_CarrierChanged(Mock<IUserNotifications> notifications, ForwardingConsol consol, IMessagingExtensions extensions)
		{
			if (ShouldPopulateCarrierChangedErrorMessage)
			{
				var orgAddress = CreateOrgAddressWithCargoWiseOneCarrierCode("BCDF");

				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
				consol.JK_OA_CreditorAddress = orgAddress.PK;
				consol.JK_CoLoadBookingReference = "COLOAD Booking Ref";

				using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals(false, extensions.ContinueWithSendingMessageAmendment(notifications.Object));
					notifications.Verify(x => x.ShowMessage($"You are trying to send a {DocumentName} amendment message to a new carrier. Please verify Carrier (or Co-Load With) and Reset to Original if you are sending {DocumentName} to new carrier.", "Confirmation"), Times.Once);
				}
			}
		}

		void CheckSendingMessageAmendment_BookingRequestRequired(Mock<IUserNotifications> notifications, ForwardingConsol consol, IMessagingExtensions extensions)
		{
			if (ShouldPopulateBookingRequestRequiredErrorMessage)
			{
				var orgAddress = CreateOrgAddressWithCargoWiseOneCarrierCode();

				consol.JK_OA_ShippingLineAddress = orgAddress.PK;
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_BookingReference = ZString.Empty;

				using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals(false, extensions.ContinueWithSendingMessageAmendment(notifications.Object));
					notifications.Verify(x => x.ShowMessage("You are trying to send an amendment message, carrier booking reference is mandatory to send an amendment message.", "Confirmation"), Times.Once);
				}
			}
		}

		void CheckSendingMessageAmendment_ConfirmationMessage(Mock<IUserNotifications> notifications, ForwardingConsol consol, IMessagingExtensions extensions, ZBool? normalResult, string confirmationMessage = null, Func<Times> confirmationTimes = null)
		{
			var orgAddress = CreateOrgAddressWithCargoWiseOneCarrierCode();

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_BookingReference = "Booking reference";
			consol.JK_OA_ShippingLineAddress = orgAddress.PK;

			using (FreightDataRegistry.Instance.EnableOceanCarrierMessagingConnectionValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(normalResult, extensions.ContinueWithSendingMessageAmendment(notifications.Object));
				if (confirmationMessage != null)
				{
					notifications.Verify(x => x.ShowConfirmation(confirmationMessage, "Confirmation"), confirmationTimes);
				}
			}
		}

		OrgAddress CreateOrgAddressWithCargoWiseOneCarrierCode(string customsRegNo = "ABCD")
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			var orgCusCode = orgAddress.Header.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CargoWiseOneCarrierCode;
			orgCusCode.OK_CustomsRegNo = customsRegNo;

			return orgAddress;
		}

		static string GetMessage(string prefixMessage)
		{
			return string.Format(@"{0}, and there is no reply received from the recipient. Resending the message may cause errors and possibly revert to a manual process.

You may want to reset the message to original first (using ""Reset to Original"" option) if the recipient did not receive it or has not manually rejected your earlier message. Are you sure you want to send the message?", prefixMessage);
		}

		IVisualizerDocumentData PrepareDocumentData(ForwardingConsol consol)
		{
			var documentData = consol.Factory.New<VisualizerDocumentData>();

			using (documentData.SuspendSettingHasChanges())
			{
				documentData.Parent = consol;
				documentData.JDD_Name = DocumentDataStoreName;
			}

			return documentData;
		}

		KeyValuePair<string, string>[] GetParametersForEvent(string eventCode, string companyParameterValue)
		{
			var result = new List<KeyValuePair<string, string>>();

			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, DocumentName));

			HashSet<string> sentMessagesEventCodesHashSet = new HashSet<string>(MessageEventCodes.SentMessagesEventCodes);
			if (eventCode != null && sentMessagesEventCodesHashSet.Contains(eventCode) && !string.IsNullOrEmpty(companyParameterValue))
			{
				result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Company, companyParameterValue));
			}

			return result.ToArray();
		}

		void CreateEvent(IVisualizerDocumentData documentData, Event @event, string companyParameterValue = "ABCD")
		{
			(documentData as IStmALogParent).Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, GetParametersForEvent(@event.Code, companyParameterValue));

			Factory.Save();
			Thread.Sleep(1);
		}

		#endregion
	}
}
