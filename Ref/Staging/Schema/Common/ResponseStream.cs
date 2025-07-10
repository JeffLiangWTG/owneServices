using System;
using System.IO;
using System.Net.Http;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.Staging.Common
{
	public class ResponseStream : IResponseStream
	{
		readonly HttpResponseMessage responseMessage;

		public ResponseStream(HttpResponseMessage responseMessage)
		{
			Argument.NotNull(responseMessage, nameof(responseMessage));
			this.responseMessage = responseMessage;
		}

		public Stream GetResponseStream()
		{
			return responseMessage.Content.ReadAsStream() ?? new MemoryStream();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			responseMessage.Dispose();
		}
	}
}
