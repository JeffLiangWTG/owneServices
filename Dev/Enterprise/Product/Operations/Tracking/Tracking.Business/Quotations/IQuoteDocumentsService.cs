using System;
using System.IO;

namespace Enterprise.Tracking.Business
{
	public interface IQuoteDocumentsService
	{
		IWebTrackerPrintResult Print(Guid contactPK, Guid quotePK);
	}

	public class QuoteDocumentPrintResult : IWebTrackerPrintResult
	{
		public string ErrorMessage { get; set; }
		public string FileName { get; set; }
		public Stream FileContents { get; set; }
	}
}
