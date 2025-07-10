using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Freight.DataTransfer;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.QuotedBookings.DataTransfer
{
	public class QuotedBookingStatusUpdatedLogProcessor : ShipmentStatusUpdatedLogProcessor
	{
		protected override BusinessObject GetLogParent(IQueuedLog log)
		{
			return log.Factory.Load<QuotedBooking>(log.SJ_ParentID);
		}

		protected override bool ShouldProcess(BusinessObject logParent)
		{
			var booking = logParent as QuotedBooking;
			return booking.Booking != null && booking.Booking.Numbers.Cast<CusEntryNumber>().Any(n => n.CE_EntryType == CusEntryNumLookups.HIR);
		}

		protected override ITopLevelDataObjectWriter GetShipmentStatusDataObjectWriterCore(DataWritingManager writeManager,
			BusinessObject logParent, Event @event, string dataContextDocumentName, string rejectionReason, bool shouldPopulateTransportLegCollection)
		{
			return new QuotedBookingStatusDataObjectWriter(writeManager, logParent as QuotedBooking)
			{
				Event = @event,
				DataContextDocumentName = dataContextDocumentName,
				ReasonForRejectionNoteText = rejectionReason,
				ShouldPopulateTransportLegCollection = shouldPopulateTransportLegCollection
			};
		}
	}
}
