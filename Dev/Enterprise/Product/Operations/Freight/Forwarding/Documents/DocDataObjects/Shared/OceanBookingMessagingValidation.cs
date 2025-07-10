using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class OceanBookingMessagingValidation : BookingMessagingValidation
	{
		public OceanBookingMessagingValidation(string documentName, ForwardingConsol consol)
		{
			this.documentName = documentName;
			this.consol = consol;
		}

		readonly string documentName;
		readonly ForwardingConsol consol;

		#region Implmentation

		#region ContinueWithResetToOriginal

		public bool? ContinueWithResetToOriginal(IUserNotifications notifications)
		{
			if (Context.DocumentName == ConsolDocumentNames.ShippingOrder &&
				Context.ResponseCode == Events.MessageAcceptedCode)
			{
				notifications.ShowMessage(
					Res.GetString("870adb43-19b8-4209-9d83-c2a71a0a9df8",
						"An original message has previously been sent and received successful confirmation from the Carrier. Resetting to Original and consequent sending of the new original message will cause duplication and errors on the Carrier site. Please cancel the Booking first using “Withdraw/Cancel Message”."),
					ConfirmationMessage);
				return false;
			}

			return null;
		}

		#endregion

		#region ContinueWithSendingMessage

		public bool? ContinueWithSendingMessage(IUserNotifications notifications, IDocument document,
			IMessageInstructions messageInstructions)
		{
			var baseValidationResult = base.ContinueWithSendingMessage(notifications);
			if (baseValidationResult.HasValue)
			{
				return baseValidationResult.Value;
			}

			if (Carrier != null && EnableOceanCarrierMessagingConnectionValidation)
			{
				var response = GetRoutingRuleCheckMessage(document, messageInstructions);
				if (!response.RoutingRuleMessage.IsNullOrEmpty())
				{
					notifications.ShowMessage(response.RoutingRuleMessage, ConfirmationMessage);
					if (!response.IsValid)
					{
						return false;
					}
				}
			}

			return null;
		}

		#endregion

		#region ContinueWithSendingMessageAmendment

		public bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications, IDocument document,
			IMessageInstructions messageInstructions)
		{
			if (Carrier != null && EnableOceanCarrierMessagingConnectionValidation)
			{
				var response = GetRoutingRuleCheckMessage(document, messageInstructions);
				if (!response.RoutingRuleMessage.IsNullOrEmpty())
				{
					notifications.ShowMessage(response.RoutingRuleMessage, ConfirmationMessage);
					if (!response.IsValid)
					{
						return false;
					}
				}
			}

			return base.ContinueWithSendingMessageAmendment(notifications);
		}

		#endregion

		#region ContinueWithSendingMessageWithdrawal

		public bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications, IDocument document,
			IMessageInstructions messageInstructions)
		{
			if (Carrier != null && EnableOceanCarrierMessagingConnectionValidation)
			{
				var response = GetRoutingRuleCheckMessage(document, messageInstructions);
				if (!response.RoutingRuleMessage.IsNullOrEmpty())
				{
					notifications.ShowMessage(response.RoutingRuleMessage, ConfirmationMessage);
					if (!response.IsValid)
					{
						return false;
					}
				}
			}

			return base.ContinueWithSendingMessageWithdrawal(notifications);
		}

		#endregion

		#region GetRoutingRuleCheckMessage

		(bool IsValid, string RoutingRuleMessage) GetRoutingRuleCheckMessage(IDocument document, IMessageInstructions messageInstructions)
		{
			var applicableDocumentNames = new[]
			{
				ConsolDocumentNames.ShippingInstruction, ConsolDocumentNames.BookingRequest,
				ConsolDocumentNames.VerifiedGrossContainerWeight, ConsolDocumentNames.ShippingOrder, ShipmentDocumentNames.eManifest
			};

			if (!applicableDocumentNames.Contains(Context.DocumentName))
			{
				return (IsValid: true, RoutingRuleMessage: string.Empty);
			}

			var interchange = document
				.ToUniversalXmlDataObject()
				.PopulateDataContext(Context.DocumentName)
				.ToUniversalXml(messageInstructions.XmlNamespace)
				.WrapInInterchange(messageInstructions.EHubClientID);

			var routingRuleValidator = ObjectFactory.Get<IRoutingRuleValidator>();
			var response = routingRuleValidator.IsValidWithRecipientIdsRetrieved(interchange);

			var routingRuleMessage = string.Empty;

			if (response.ValidationResult)
			{
				if (response.RecipientIds.Any(s => s.ToUpperInvariant().Contains("INTTRA")))
				{
					routingRuleMessage = DiscontinueINTTRARoutingRuleMessage;
				}
			}
			else
			{
				routingRuleMessage = InvalidRoutingRuleMessage;
			}

			return (IsValid: response.ValidationResult, RoutingRuleMessage: routingRuleMessage);
		}

		string InvalidRoutingRuleMessage => Res.GetString("3ffcfefb-d52e-4b2a-897e-e1d001c53a9e",
			@"You are trying to send the {0} message to {1}, however your Organization/Branch is not registered with this carrier for this message type.
Please raise an eRequest and we will guide you through the registration process.",
			Context.DocumentName, Carrier.OH_FullName);

		string DiscontinueINTTRARoutingRuleMessage => Res.GetString("760e7286-0179-4626-a492-e9f81434a7e0",
			@"Your {0} message to this carrier is being routed via INTTRA, however a direct connection exists between CargoWise and this carrier. Be advised that the CargoWise direct connection will replace the current means of communication to this carrier by the end of 2023. Please raise a CR9 / Ocean Carrier Messaging and follow the process to request all your global branches to be migrated to the direct connection for this carrier.",
			Context.DocumentName);

		#endregion
		#region GetLastSentCarrierCode

		public ZString? GetLastSentCarrierCode(IDialog lastDialog)
		{
			var applicableEventCodes = new HashSet<string>(MessageEventCodes.SentMessagesEventCodes);

			var sentMessageOrStatusUpdateLog = lastDialog?.Logs.FirstOrDefault(log => applicableEventCodes.Contains(log.SL_SE_NKEvent));
			if (sentMessageOrStatusUpdateLog != null &&
				sentMessageOrStatusUpdateLog.Parameters.TryGetValue(EventReferenceParameters.Codes.Company, out var lastCarrierCode))
			{
				return new ZString(lastCarrierCode);
			}

			return null;
		}

		#endregion

		#region GetRequireMessageAmendmentReason

		public bool? GetRequireMessageAmendmentReason()
		{
			return IsSendingAmendment();
		}

		#endregion

		#region IsSendingAmendment

		public bool? IsSendingAmendment()
		{
			if (ShouldResetToOriginal)
			{
				return false;
			}

			return Context.DocumentDataVersion > 1;
		}

		#endregion

		#region Override

		protected override BusinessObject BizO => consol;

		protected override string DocumentName => documentName;

		protected override string DocumentDataStoreName
		{
			get
			{
				switch (DocumentName)
				{
					case ConsolDocumentNames.BookingRequest:
					case ConsolDocumentNames.ShippingInstruction:
						return ConsolDocumentDataStoreNames.SeaBookingRequest2;
					case ConsolDocumentNames.ShippingOrder:
						return ConsolDocumentDataStoreNames.ShippingOrder;
					default:
						return string.Empty;
				}
			}
		}

		protected override bool ShouldResetToOriginal
		{
			get
			{
				var applicableDocumentNames = new[]
				{
					ConsolDocumentNames.ShippingInstruction, ConsolDocumentNames.BookingRequest, ConsolDocumentNames.ShippingOrder
				};

				if (applicableDocumentNames.Contains(Context.DocumentName))
				{
					return base.ShouldResetToOriginal;
				}

				return false;
			}
		}

		protected override bool? CheckMessageStatusForAmendment(IUserNotifications notifications)
		{
			if (Context.DocumentName == ConsolDocumentNames.ShippingOrder
				|| Context.DocumentName == ConsolDocumentNames.ShippingInstruction
				|| Context.DocumentName == ConsolDocumentNames.BookingRequest)
			{
				return base.CheckMessageStatusForAmendment(notifications);
			}

			return null;
		}

		protected override OrgHeader Carrier
		{
			get
			{
				if (Context.DocumentName == ConsolDocumentNames.ShippingOrder && consol?.CarrierBookingAgent != null)
				{
					return consol?.CarrierBookingAgent;
				}
				return consol != null && consol.IsCoLoad ? consol.Creditor : consol?.ShippingLine;
			}
		}

		protected override bool CarrierCanNotReceiveThisMessage
		{
			get
			{
				switch (Context.DocumentName)
				{
					case ConsolDocumentNames.BookingRequest:
						return !Carrier.ShippingLine?.RSL_BookingRequestAvailable ?? true;
					case ConsolDocumentNames.ShippingInstruction:
						return !Carrier.ShippingLine?.RSL_ShippingInstructionAvailable ?? true;
					case ConsolDocumentNames.ShippingOrder:
						return !Carrier.ShippingLine?.RSL_ShippingOrderAvailable ?? true;
					case ConsolDocumentNames.VerifiedGrossContainerWeight:
						return !Carrier.ShippingLine?.RSL_VerifiedGrossContainerWeightAvailable ?? true;
					case ShipmentDocumentNames.eManifest:
						return !Carrier.ShippingLine?.RSL_EManifestAvailable ?? true;
				}

				return false;
			}
		}

		protected override bool CarrierBookingReferenceIsEmpty
		{
			get
			{
				var applicableDocumentNames =
					new[] { ConsolDocumentNames.BookingRequest, ConsolDocumentNames.ShippingOrder };

				return applicableDocumentNames.Contains(Context.DocumentName)
						 && (consol.IsCoLoad && consol.JK_CoLoadBookingReference.IsEmpty
							 || !consol.IsCoLoad && consol.JK_BookingReference.IsEmpty);
			}
		}

		protected override string CarrierCode => consol.GetCarrierCodeWithFallback();

		protected override bool CarrierChangedSinceLastSent
		{
			get
			{
				var applicableDocumentNames = new[]
				{
					ConsolDocumentNames.BookingRequest, ConsolDocumentNames.ShippingOrder, ConsolDocumentNames.ShippingInstruction,
				};
				if (!applicableDocumentNames.Contains(Context.DocumentName))
				{
					return false;
				}

				return base.CarrierChangedSinceLastSent;
			}
		}

		#endregion

		#endregion
	}
}
