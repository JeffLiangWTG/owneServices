using System;
using System.IO;
using Enterprise.Tracking.Business;

namespace Enterprise.Tracking.Business
{
	public interface IBookingDocumentsService
	{
		IWebTrackerPrintResult PrintHouseBill(Guid contactPK, Guid shipmentPK);

		IWebTrackerPrintResult PrintFreightLabel(Guid contactPK, Guid shipmentPK);
	}
}

public class BookingDocumentPrintResult : IWebTrackerPrintResult
{
	public string ErrorMessage { get; set; }
	public string FileName { get; set; }
	public Stream FileContents { get; set; }
}
