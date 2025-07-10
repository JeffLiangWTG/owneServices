using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class VGMMessagingExtensions : BaseMessagingExtensions
	{
		public VGMMessagingExtensions(IDocument document, ForwardingConsol consol, IMessageInstructions messageInstructions)
		{
			this.document = document;
			this.messageInstructions = messageInstructions;
			vgm = document?.Data.Value as VerifiedGrossMass;
			Argument.NotNull(vgm, nameof(vgm));
			bookingValidation = new OceanBookingMessagingValidation(ConsolDocumentNames.VerifiedGrossContainerWeight, consol);
			this.consol = consol;
		}

		readonly VerifiedGrossMass vgm;
		readonly OceanBookingMessagingValidation bookingValidation;
		readonly IDocument document;
		readonly IMessageInstructions messageInstructions;
		readonly ForwardingConsol consol;
		static string information => Res.GetString("2d7a614b-d880-4fa4-9933-fa4ecf7a398b", "Information");
		static string confirmation => Res.GetString("04e042e2-7743-4a05-94e1-cdd12e495768", "Confirmation");

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications)
		{
			var incompatibleContainerNumbers = vgm
				.Containers
				.Where(c => !c.MessageStatus.AllowResetToOriginal)
				.Select(c => c.Number)
				.ToArray();

			if (!incompatibleContainerNumbers.Any())
			{
				return null;
			}

			var message = Res.GetString("55EA09EE-E901-4F5A-8F55-7C119AE5D88D", "The following containers cannot be reset to original because they have not yet been sent. {0}",
				string.Join(", ", incompatibleContainerNumbers));

			notifications?.ShowMessage(message, information);
			return false;
		}

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			var bookingValidationResult = bookingValidation.ContinueWithSendingMessage(notifications, document, messageInstructions);
			if (bookingValidationResult.HasValue && !bookingValidationResult.Value)
			{
				return false;
			}

			var incompatibleContainerNumbers = vgm
				.Containers
				.Where(c => !c.MessageStatus.AllowSendOriginal)
				.Select(c => c.Number)
				.ToArray();

			if (!incompatibleContainerNumbers.Any())
			{
				return true;
			}

			var message = Res.GetString("E72A3012-C941-410D-814B-BD4AAE8038A9", "The following container numbers cannot be sent because responses have not yet been received. {0}",
				string.Join(", ", incompatibleContainerNumbers));

			notifications?.ShowMessage(message, information);
			return false;
		}

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			var bookingValidationResult = bookingValidation.ContinueWithSendingMessageAmendment(notifications, document, messageInstructions);
			if (bookingValidationResult.HasValue && !bookingValidationResult.Value)
			{
				return false;
			}

			if (AnyCarrierChanged())
			{
				var carrierChangedErrorMessage = Res.GetString("aab9e638-3bc2-4c94-a4e0-d9df9a9e7520", "You are trying to send a Verified Gross Container Weight amendment message to a new carrier. Please verify Carrier and Reset to Original if you are sending Verified Gross Container Weight to new carrier.");
				notifications?.ShowMessage(carrierChangedErrorMessage, confirmation);
				return false;
			}

			var incompatibleContainerNumbers = vgm
				.Containers
				.Where(c => !c.MessageStatus.AllowSendAmendment)
				.Select(c => c.Number)
				.ToArray();

			if (!incompatibleContainerNumbers.Any())
			{
				return true;
			}

			var message = Res.GetString("947DA100-8936-4C79-B527-4C79A55ABA95", "The following container numbers cannot be sent because responses have not yet been received or original has not yet been sent. {0}",
				string.Join(", ", incompatibleContainerNumbers));

			notifications?.ShowMessage(message, information);
			return false;
		}

		bool AnyCarrierChanged()
		{
			var carrierSCACCode = consol.GetCarrierCodeWithFallback();
			var applicableEventCodes = new HashSet<string>(MessageEventCodes.SentMessagesEventCodes);
			applicableEventCodes.Add(Events.StatusUpdatedCode);

			foreach (var container in vgm.Containers)
			{
				var logParent = container.LogParent;

				if (logParent != null)
				{
					var lastDialog = logParent.GetDialogs(ConsolDocumentNames.VerifiedGrossContainerWeight, false).LastOrDefault();

					var lastCarrierCode = bookingValidation.GetLastSentCarrierCode(lastDialog);

					if (lastCarrierCode.HasValue && lastCarrierCode.Value != carrierSCACCode)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			var bookingValidationResult = bookingValidation.ContinueWithSendingMessageWithdrawal(notifications, document, messageInstructions);
			if (bookingValidationResult.HasValue && !bookingValidationResult.Value)
			{
				return false;
			}

			if (AnyCarrierChanged())
			{
				var carrierChangedErrorMessage = Res.GetString("841bfbb0-7be7-4d6c-94ca-cbec6c8d6912", "You are trying to send a Verified Gross Container Weight withdrawal message to a new carrier. Sending Verified Gross Container Weight withdrawal message to a new carrier is not allowed.");
				notifications?.ShowMessage(carrierChangedErrorMessage, confirmation);
				return false;
			}

			var incompatibleContainerNumbers = vgm
				.Containers
				.Where(c => !c.MessageStatus.AllowSendWithdrawal)
				.Select(c => c.Number)
				.ToArray();

			if (!incompatibleContainerNumbers.Any())
			{
				return true;
			}

			var message = Res.GetString("78E5110D-1A05-4D49-92EF-30380B8D0448", "The withdrawal message of the following container numbers cannot be sent because responses have not yet been received or there is no message to withdraw. {0}",
				string.Join(", ", incompatibleContainerNumbers));

			notifications?.ShowMessage(message, information);
			return false;
		}

		public override bool? IsSendingAmendment()
		{
			return vgm
				.Containers
				.Any(b => !b.MessageStatus.AllowSendOriginal);
		}

		public override KeyValuePair<string, string>[] GetAdditionalParametersForEvent()
		{
			var result = new List<KeyValuePair<string, string>>();

			var carrierSCACCode = consol.GetCarrierCodeWithFallback();
			if (!carrierSCACCode.IsEmpty)
			{
				result.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Company, carrierSCACCode));
			}

			return result.ToArray();
		}
	}
}
