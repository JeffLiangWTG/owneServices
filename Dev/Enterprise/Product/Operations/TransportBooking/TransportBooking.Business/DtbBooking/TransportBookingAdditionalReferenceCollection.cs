using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.TransportBookings.Business
{
	public class TransportBookingAdditionalReferenceCollection : NonPersistentBusinessObjectCollection<TransportBookingAdditionalReference>
	{
		public TransportBookingAdditionalReferenceCollection(DtbBooking booking)
		{
			this.booking = Argument.NotNull(booking, "Booking");
		}
		readonly DtbBooking booking;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TransportBookingAdditionalReference(booking, booking.AdditionalReferenceNumbers.AddNew());
		}

		public void BuildCollection()
		{
			foreach (Customs.ICusEntryNumber reference in booking.AdditionalReferenceNumbers)
			{
				Add(new TransportBookingAdditionalReference(booking, reference));
			}

			if (booking.ConsolidationSingleJob != null)
			{
				foreach (Customs.ICusEntryNumber reference in booking.ConsolidationSingleJob.AdditionalReferenceNumbers)
				{
					Add(new TransportBookingAdditionalReference(booking, reference));
				}
			}

			if (booking.KM_IsMaster)
			{
				foreach (var sub in booking.SubBookings)
				{
					foreach (Customs.ICusEntryNumber reference in sub.AdditionalReferenceNumbers)
					{
						Add(new TransportBookingAdditionalReference(booking, reference));
					}

					foreach (Customs.ICusEntryNumber reference in sub.ConsolidationSingleJob.AdditionalReferenceNumbers)
					{
						Add(new TransportBookingAdditionalReference(booking, reference));
					}
				}
			}

			if (booking.KM_KM_MasterBooking != ZGuid.Empty)
			{
				foreach (Customs.ICusEntryNumber reference in booking.MasterBooking.AdditionalReferenceNumbers)
				{
					Add(new TransportBookingAdditionalReference(booking, reference));
				}

				foreach (Customs.ICusEntryNumber reference in booking.MasterBooking.ConsolidationSingleJob.AdditionalReferenceNumbers)
				{
					Add(new TransportBookingAdditionalReference(booking, reference));
				}
			}
		}
	}
}
