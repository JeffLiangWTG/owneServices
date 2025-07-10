using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	sealed class EManifestMessagingExtensions : BaseMessagingExtensions
	{
		public EManifestMessagingExtensions(IDocument document, ForwardingShipment shipment, IMessageInstructions messageInstructions)
		{
			this.document = document;
			eManifest = document?.Data.Value as EManifest;
			Argument.NotNull(eManifest, nameof(eManifest));
			bookingValidation = new OceanBookingMessagingValidation(ShipmentDocumentNames.eManifest, shipment.GetExportConsolFromChina());
			this.messageInstructions = messageInstructions;
		}

		readonly IDocument document;
		readonly EManifest eManifest;
		readonly OceanBookingMessagingValidation bookingValidation;
		readonly IMessageInstructions messageInstructions;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:DoNotInitializeStringFieldsWithResGetString", Justification = "There's no form designer for this class so using readonly is fine")]
		readonly string information = Res.GetString("2d7a614b-d880-4fa4-9933-fa4ecf7a398b", "Information"); // there's no form designer for this class so using readonly is fine

		#region ContinueWithSendingMessage

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			var bookingValidationResult = bookingValidation.ContinueWithSendingMessage(notifications, document, messageInstructions);
			if (bookingValidationResult.HasValue && !bookingValidationResult.Value)
			{
				return false;
			}

			var incompatibleBookingNumbers = eManifest
				.Bookings
				.Where(b => b.Send && !b.MessageStatus.AllowSendOriginal && !b.MessageStatus.AllowSendAmendment)
				.Select(b => b.BookingNumber)
				.ToArray();

			if (incompatibleBookingNumbers.Length > 0)
			{
				var message = Res.GetString("b0c7a25c-c325-48b3-b2ca-f549ff720b62", "The following SLD numbers cannot be sent because responses have not yet been received. {0}",
					string.Join(", ", incompatibleBookingNumbers));

				notifications?.ShowMessage(message, information);
				return false;
			}

			return true;
		}

		#endregion

		#region ContinueWithSendingMessageAmendment

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			return ContinueWithSendingMessage(notifications);
		}

		#endregion

		#region ContinueWithSendingMessageWithdrawal

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			var bookingValidationResult = bookingValidation.ContinueWithSendingMessageWithdrawal(notifications, document, messageInstructions);
			if (bookingValidationResult.HasValue && !bookingValidationResult.Value)
			{
				return false;
			}

			if (!eManifest.IsLoadingInNingboPort())
			{
				var message = Res.GetString("A7519B28-1F37-4F96-9FB2-C7A98FCEFF9D", "The withdrawal functionality is only available for Ningbo");
				notifications?.ShowMessage(message, information);
				return false;
			}

			var selectedBookings = eManifest.Bookings.Where(b => b.Send);
			var relatedBookings = GetAllRelatedBookings(eManifest, selectedBookings);
			IEnumerable<Booking> bookingsToWithdraw = selectedBookings.Concat(relatedBookings);

			if (relatedBookings.Any())
			{
				var confirmation = Res.GetString("0004C815-CC99-4109-A70E-5A58D9BE14E0", "SLD/s selected are packed into Container/s with other already submitted SLD's, and as a result all previously submitted SLD/s for this Container/s will also be withdrawn: {0}{1}",
						System.Environment.NewLine,
						string.Join(", ", bookingsToWithdraw.Select(c => c.BookingNumber)));

				if (!(notifications?.ShowConfirmation(confirmation, Res.GetString("8A4EABF7-42B2-494A-B56B-A1018BD538AB", "Confirmation")) ?? false))
				{
					return false;
				}
			}

			bookingsToWithdraw.ForEach(x => x.Send = true);
			document.Data.Validate();

			var incompatibleBookingNumbers = bookingsToWithdraw
				.Where(b => !b.MessageStatus.AllowSendWithdrawal)
				.Select(b => b.BookingNumber)
				.ToArray();

			if (incompatibleBookingNumbers.Length > 0)
			{
				var message = Res.GetString("44D9C2E0-D6AB-4AEE-9D90-4A2E84066ABD", "The following selected booking have not been submitted or received any reply yet. Sending withdrawal message may cause errors and possibly revert to a manual process. Please wait for reply before sending.{0}{1}",
							System.Environment.NewLine,
							string.Join(", ", incompatibleBookingNumbers));

				notifications?.ShowMessage(message, information);
				return false;
			}

			if (eManifest.HasMessageErrors)
			{
				var errorMessage = Res.GetString("D20D5EFC-78FF-4E18-8096-EE912A8DC66F", "The document contains validation errors, please fix them and withdraw again.");
				notifications?.ShowMessage(errorMessage, Res.GetString("0DC79065-88FD-475F-8215-6FEB1E3E78E5", "Errors"));
				return false;
			}

			return true;
		}

		static IEnumerable<Booking> GetAllRelatedBookings(EManifest eManifest, IEnumerable<Booking> bookingsToSend)
		{
			var containerNums = bookingsToSend
				.SelectMany(b => b
					.Containers
					.Select(c => c.Number)
				).Distinct();

			return eManifest
				.Bookings
				.Where(b => b.Containers.Any(c => containerNums.Contains(c.Number))
						&& !bookingsToSend.Any(s => s.BookingNumber == b.BookingNumber)
						&& !b.MessageStatus.AllowSendOriginal);
		}

		#endregion

		#region ContinueWithResetToOriginal

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications)
		{
			var incompatibleBookingNumbers = eManifest
				.Bookings
				.Where(b => b.Send && !b.MessageStatus.AllowResetToOriginal)
				.Select(b => b.BookingNumber)
				.ToArray();

			if (incompatibleBookingNumbers.Length > 0)
			{
				var message = Res.GetString("c751fefd-4e71-4ac7-b9e6-d3751b72a7b2", "The following SLD numbers cannot be reset to original because they have not yet been sent. {0}",
					string.Join(", ", incompatibleBookingNumbers));

				notifications?.ShowMessage(message, information);
				return false;
			}

			return null;
		}

		#endregion

		#region GetRequireMessageAmendmentReason

		public override bool? GetRequireMessageAmendmentReason()
		{
			if (!eManifest.IsLoadingInNingboPort())
			{
				return false;
			}

			return eManifest
				.Bookings
				.Any(b => b.Send && b.MessageStatus.AllowSendAmendment);
		}

		#endregion

		#region GetWithdrawalOptions

		public override ICodeDescriptionPairList GetWithdrawalOptions()
		{
			//withdrawal options should be the same as amendment
			return GetAmendmentOptions();
		}

		#endregion

		#region GetAmendmentOptions

		public override ICodeDescriptionPairList GetAmendmentOptions()
		{
			return eManifest.IsLoadingInNingboPort()
				? new NingboMessageAmendmentOptions()
				: null;
		}

		#endregion

		#region IsSendingAmendment

		public override bool? IsSendingAmendment()
		{
			return eManifest
				.Bookings
				.Any(b => b.Send && b.MessageStatus.AllowSendAmendment);
		}

		#endregion
	}
}
