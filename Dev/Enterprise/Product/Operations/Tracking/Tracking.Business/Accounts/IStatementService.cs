using System;
using System.IO;

namespace Enterprise.Tracking.Business
{
	public interface IStatementService
	{
		IWebTrackerPrintResult Print(Guid contactPK, Guid companyPK);
	}

	public class StatementPrintResult : IWebTrackerPrintResult
	{
		public string ErrorMessage { get; set; }
		public string FileName { get; set; }
		public Stream FileContents { get; set; }
	}
}
