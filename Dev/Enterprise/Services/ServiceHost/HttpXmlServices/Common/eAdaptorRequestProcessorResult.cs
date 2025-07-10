using System.Collections.Generic;
using System.Net;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.Services.ServiceHost
{
	public readonly struct eAdaptorRequestProcessorResult : IeAdaptorRequestProcessorResult
	{
		public eAdaptorRequestProcessorResult(HttpStatusCode httpStatus, ZString processingStatus, SubStreamableStream universalResponse, Dictionary<string, string> customHeader = null)
		{
			ProcessingStatus = processingStatus;
			HttpStatus = httpStatus;
			UniversalResponse = universalResponse;
			CustomHeaders = customHeader;
		}

		public HttpStatusCode HttpStatus { get; }
		public ZString ProcessingStatus { get; }
		public SubStreamableStream UniversalResponse { get; }
		public Dictionary<string, string> CustomHeaders { get; }
	}
}
