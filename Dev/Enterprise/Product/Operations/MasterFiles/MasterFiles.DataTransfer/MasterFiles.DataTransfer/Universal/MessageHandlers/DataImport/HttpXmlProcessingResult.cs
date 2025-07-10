using CargoWise.IO;
using Enterprise.Messaging.Integration;

namespace Enterprise.MasterFiles.DataTransfer
{
	sealed public class HttpXmlProcessingResult : IHttpXmlProcessingResult
	{
		public SubStreamableStream ResponseMessageText { get; set; }

		public SubStreamableStream FullResponseMessageText { get; set; }

		public string Status { get; set; }

		public void Dispose()
		{
			ResponseMessageText?.Dispose();
			FullResponseMessageText?.Dispose();
		}

		public bool ShouldRetry { get; set; }
	}
}
