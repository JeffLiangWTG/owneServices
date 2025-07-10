using System.IO;

namespace Enterprise.Tracking.Business
{
	public interface IWebTrackerPrintResult
	{
		string ErrorMessage { get; set; }
		string FileName { get; set; }
		Stream FileContents { get; set; }
	}
}
