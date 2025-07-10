using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Web;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Business
{
	public class BookingDocumentsService : IBookingDocumentsService
	{
		public IWebTrackerPrintResult PrintFreightLabel(Guid contactPK, Guid shipmentPK)
		{
			return PrintBookingDocument(contactPK, shipmentPK, TrackingDocumentTypes.FreightLabels);
		}

		public IWebTrackerPrintResult PrintHouseBill(Guid contactPK, Guid shipmentPK)
		{
			return PrintBookingDocument(contactPK, shipmentPK, TrackingDocumentTypes.HouseBills);
		}

		BookingDocumentPrintResult PrintBookingDocument(Guid contactPK, Guid shipmentPK, TrackingDocumentTypes documentTypes)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = nameof(BookingDocumentsService) };

			var contact = factory.Load<OrgContact>(new ZGuid(contactPK));

			if (contact == null)
			{
				return null;
			}

			var quotedBooking = QuotedBooking.New(ZGuid.Empty, shipmentPK, factory);
			if (quotedBooking == null)
			{
				return null;
			}

			var trackingBooking = new TrackingBooking(quotedBooking, contact);
			var docMenuHelper = new TrackingBookingDocumentsMenuHelper(trackingBooking, documentTypes);
			var document = docMenuHelper.GetAvailableDocuments().FirstOrDefault();
			if (document == null)
			{
				return null;
			}

			var docCommand = document.DocumentCommand;
			var printTask = new DocumentPrintSet(docCommand, new DocumentEngine.RuntimeOptions.UserControlProviderList(), null);
			if (printTask.Count == 0)
			{
				return new BookingDocumentPrintResult
				{
					ErrorMessage = string.Join("\r\n", printTask.ReasonsForEmptyPacks),
				};
			}

			var pack = printTask[0];
			pack.Organisation = contact.ParentOrg;
			var data = new DocumentUtility(factory).GetDocument(pack, contact, DataContentTypes.Pdf);

			return new BookingDocumentPrintResult
			{
				FileName = $"{docCommand.SU_MenuName}.pdf", // File name
				FileContents = new MemoryStream(data),
			};
		}
	}
}
