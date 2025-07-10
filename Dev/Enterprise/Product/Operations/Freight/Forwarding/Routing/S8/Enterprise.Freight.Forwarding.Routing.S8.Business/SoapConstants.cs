using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public static class SoapConstants
	{
		public const string XmlContentType = "text/xml";

		public static class SoapAction
		{
			public const string HeaderName = "SOAPAction";
			public const string GetFlight = "S8CWebSv/GetFlight";
			public const string SolveRouting = "S8CWebSv/SolveRouting";
			public const string LogInS8C = "S8CWebSv/LogInS8C";
		}

		public static class SoapMessage
		{
			public static readonly string FlightWithDateMessageTemplate = (NoResString)@"
	<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
		<soap:Body>
			<{0} xmlns=""S8CWebSv"">
				<Token>{1}</Token>
				<SSet>{2}</SSet>
				<Date>{3:yyyy/MM/dd}</Date>
				<Airline>{4}</Airline>
				<Flight>{5}</Flight>
			</{0}>
		</soap:Body>
	</soap:Envelope>";

			public static readonly string FlightWithoutDateMessageTemplate = (NoResString)@"
	<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
		<soap:Body>
			<{0} xmlns=""S8CWebSv"">
				<Token>{1}</Token>
				<SSet>{2}</SSet>
				<Date>*</Date>
				<Airline>{3}</Airline>
				<Flight>{4}</Flight>
			</{0}>
		</soap:Body>
	</soap:Envelope>";

			public static readonly string SolveRoutingMessageTemplate = (NoResString)@"
	<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
		<soap:Body>
			<{0} xmlns=""S8CWebSv"">
				<Token>{1}</Token>
				<SSet>SSIM</SSet>
				<Problem>{2}</Problem>
				<Option>1</Option>
			</{0}>
		</soap:Body>
	</soap:Envelope>";

			public static readonly string LoginMessageTemplate = (NoResString)@"
	<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
		<soap:Body>
			<{0} xmlns=""S8CWebSv"">
				<UserID>{1}</UserID>
				<Password>{2}</Password>
				<Computer>{3}</Computer>
				<LoginID>{4}</LoginID>
				<Program>{5}</Program>
				<ClientVersion>{6}</ClientVersion>
			</{0}>
		</soap:Body>
	</soap:Envelope>";
		}
	}
}
