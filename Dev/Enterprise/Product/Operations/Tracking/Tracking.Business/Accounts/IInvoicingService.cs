using System;
using System.IO;

namespace Enterprise.Tracking.Business
{
	public interface IInvoicingService
	{
		IWebTrackerPrintResult Print(Guid contactPK, Guid invoicePK);
	}

	public class InvoicingPrintResult : IWebTrackerPrintResult
	{
		public string ErrorMessage { get; set; }
		public string FileName { get; set; }
		public Stream FileContents { get; set; }
	}
}
