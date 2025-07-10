namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public class ServerParseErrorHandler
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "cummunication between server and client")]
		public const string ParseError = "parse error";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "cummunication between server and client")]
		public const string InvalidAirline = "invalid airline";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "cummunication between server and client")]
		public const string InvalidAirport = "invalid airport";

		public enum ParseErrorMessageType
		{
			None,
			InvalidAirline,
			InvalidAirport
		}

		readonly string originalServerResponse;
		readonly RoutingRequest routingRequest;

		public ServerParseErrorHandler(string originalServerResponse, RoutingRequest request)
		{
			this.originalServerResponse = originalServerResponse;
			routingRequest = request;

			if (!string.IsNullOrEmpty(originalServerResponse))
			{
				var refinedParseError = originalServerResponse.ToLower();
				if (refinedParseError.Contains(ParseError))
				{
					ResponseContainsParseError = true;

					if (refinedParseError.Contains(InvalidAirline))
					{
						ErrorType = ParseErrorMessageType.InvalidAirline;
					}
					else if (refinedParseError.Contains(InvalidAirport))
					{
						ErrorType = ParseErrorMessageType.InvalidAirport;
					}
					else
					{
						ErrorType = ParseErrorMessageType.None;
					}
				}
			}
		}

		public bool ResponseContainsParseError { get; private set; }

		public ParseErrorMessageType ErrorType { get; private set; }

		public bool ShouldReportError
		{
			get
			{
				return (ErrorType != ParseErrorMessageType.InvalidAirline) && (ErrorType != ParseErrorMessageType.InvalidAirport);
			}
		}

		public string Message
		{
			get
			{
				switch (ErrorType)
				{
					case ParseErrorMessageType.InvalidAirline:
						return Res.GetString("ff62bf26-91dd-47e4-b417-4cd7ebe9e6f5", "Selected Airline is not a valid supported Carrier");
					case ParseErrorMessageType.InvalidAirport:
						var spec = GetAirportSpecification();
						return Res.GetString("c79a58ce-6d9b-4af0-b054-7725e6427e9b", "{0} airport <{1}> has an invalid IATA code", spec.OriginOrDest, spec.UnlocoCode);
					default:
						return originalServerResponse;
				}
			}
		}

		(string OriginOrDest, string UnlocoCode) GetAirportSpecification()
		{
			var airport = GetResponseAirport();
			if (routingRequest.OriginUNLOCO != null && routingRequest.OriginUNLOCO.RL_IATA.Equals(airport))
			{
				return (Res.GetString("28537599-d463-4014-b33c-a57d794b8f70", "Origin"), routingRequest.OriginUNLOCO.Code);
			}
			else if (routingRequest.DestinationUNLOCO != null && routingRequest.DestinationUNLOCO.RL_IATA.Equals(airport))
			{
				return (Res.GetString("9058745c-462d-414d-bc60-deb1ec601ef5", "Destination"), routingRequest.DestinationUNLOCO.Code);
			}

			return (string.Empty, string.Empty);
		}

		string GetResponseAirport()
		{
			var splitResponse = originalServerResponse.Split(':');
			if (splitResponse.Length >= 3)
			{
				return splitResponse[2].Trim();
			}

			return string.Empty;
		}
	}
}
