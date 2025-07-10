using System;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	static class BookingRequestHelper
	{
		public static bool IsBookingConfirmationMessage(this ITopLevelDataObject dataObject) => dataObject?.DataContext.IsBookingConfirmationMessage() ?? false;
		public static bool IsBookingRequestMessage(this ITopLevelDataObject dataObject) => dataObject?.DataContext.IsBookingRequestMessage() ?? false;

		static bool IsBookingRequestMessage(this IDataContextDataObject dataContext)
		{
			var documentName = dataContext
				?.DocumentaryOverride
				?.DocumentName;

			return documentName.HasValue
				   && string.Equals(documentName.Value, BookingRequestDataDocumentName, StringComparison.OrdinalIgnoreCase);
		}

		static bool IsBookingConfirmationMessage(this IDataContextDataObject dataContext)
		{
			var documentName = dataContext
				?.DocumentaryOverride
				?.DocumentName;

			return documentName.HasValue
				   && string.Equals(documentName.Value, BookingConfirmationDocumentName, StringComparison.OrdinalIgnoreCase);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public const string BookingConfirmationDocumentName = "Booking Confirmation";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public const string BookingRequestDataDocumentName = "Booking Request";
	}
}
