namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler
{
	public class ResponseResult
	{
		public bool IsValidResponse { get; }

		public string ResponseMessage { get; }

		public string StackTrace { get; }

		public ResponseResult(bool isValidResponse, string responseMessage, string stackTrace)
		{
			IsValidResponse = isValidResponse;
			ResponseMessage = responseMessage;
			StackTrace = IsValidResponse ? null : stackTrace;
		}
	}
}
