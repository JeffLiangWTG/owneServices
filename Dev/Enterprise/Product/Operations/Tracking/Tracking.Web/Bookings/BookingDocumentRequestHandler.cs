using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Web
{
	public abstract class BookingDocumentRequestHandler<T> : DocumentRequestHandler<T>
		where T : DocumentRequestHelper, new()
	{
		protected override DocumentsMenuHelper GetHelperForThisThread(ZGuid pk) => MenuHelper;

		protected override BusinessObject[] GetNewBusinessObjects()
		{
			return MenuHelper
				.GetAvailableDocuments()
				.Select(d => d.DocumentCommand)
				.Where(d => d != null)
				.ToArray();
		}

		TrackingBooking Booking
		{
			get
			{
				return PKs
					.Select(pk => TrackingBooking.GetFromRefPK(Factory, pk, WebEnv.AppInstance.SiteUser as TrackingSiteUser))
					.FirstOrDefault();
			}
		}

		TrackingBookingDocumentsMenuHelper MenuHelper => menuHelper ?? (menuHelper = new TrackingBookingDocumentsMenuHelper(Booking, GetTrackingDocumentType()));
		TrackingBookingDocumentsMenuHelper menuHelper;

		protected abstract TrackingDocumentTypes GetTrackingDocumentType();

		public override string ContentType => DataContentTypes.Pdf;
	}
}
