using System.Net;
using Newtonsoft.Json;

namespace Enterprise.MasterFiles.Business
{
	public class ResponseDTO
	{
		[JsonProperty("opCode")]
		public HttpStatusCode OpCode { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("additionalInfo")]
		public AdditionalInformation AdditionalInformation { get; set; }
	}

	public class AdditionalInformation
	{
		// TODO: AdditionalInfo Detail Properties
	}
}
