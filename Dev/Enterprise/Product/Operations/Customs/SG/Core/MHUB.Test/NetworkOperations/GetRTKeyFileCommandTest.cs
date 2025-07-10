using System.Net;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.Registry;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.SG.V4.MHUB.Testing
{
	sealed class GetRTKeyFileCommandTest : TestCaseWithFactory
	{
		public void TestExecute()
		{
			const string expectedDecodedRsaKey = @"-----BEGIN RSA PRIVATE KEY-----
MIICXAIBAAKBgQCvtRwgHKLaxBhRizOnfTXkHpF7n4zJC0urclJ9m2DBPr5sY2eJ
M7vCNoCQQdPV5iSBoVw4ThCIjuG4/hERHnP1wViRe/tJl7a/FcH+e7OMDreZm4+V
Tv/Q1a4UTvxu8JnKYakBKk7X5bjEMdqUCYVvUsuo82tKAtCO50WFlKtVSwIBIwKB
gHh8MIsMUmogEKzxvQUiqJxsu4f4YInb3B3KvDjfks2nXgEuOF4U2IUsr+3j/vkS
1zRRY87Wcby5v1pH0TBPZXQugQSYHnivaOflJX2DqvyIX9lXyVhC5vaZNUyv2pi+
+96vNmcHrtcTMDTn56NTQnjvD3zgZI1MSZoJA7/9BxOLAkEA1h8vBCynuEeqUH9V
qSya+Qb9uAjWb69105kaEQZM4V37l4MOhsW6QPCZrWrVJn3A5V9sfn6bgBq5+ijM
e/gviwJBANISkRn3hzT2F+xr/HKC63l6KZjWc32/iLf8lRLXWKNh1LVL8bhUzqjM
iAeTRgmcE2YgUQMEtcjKfvVMlnJeKUECQQDQAQkZ/35bPk2tSH8ZXofqmRO6Fza1
oyIAwJz6mGfw3vRnTBy9b579QX9YAWFYl2rtcqPomD9JPojHIFGc/7knAkEAkAy7
RQGBSONDms20a8d85ZWY3dThesx7AdHFTr+NPNVeqDQTdxWVBgiXyq4hZazaGiTJ
1i8dkP/iB0p1ysQ5iwJBAJf/JQNfTQaIisB4+DJFjYipNVIpQkIg90wTYY+noBJu
EUhKOsrp1m8cG0jkqgD5lhN68YlW0ZgtFIYxDtHuI2E=
-----END RSA PRIVATE KEY-----
";
			const string cannedHtmlReponse = @"<html>
<head><title>EDI Servlet</title></head>
<body>

<?xml version='1.0' encoding='UTF-8'?><RSAKEY>LS0tLS1CRUdJTiBSU0EgUFJJVkFURSBLRVktLS0tLQ0KTUlJQ1hBSUJBQUtCZ1FDdnRSd2dIS0xheEJoUml6T25mVFhrSHBGN240ekpDMHVyY2xKOW0yREJQcjVzWTJlSg0KTTd2Q05vQ1FRZFBWNWlTQm9WdzRUaENJanVHNC9oRVJIblAxd1ZpUmUvdEpsN2EvRmNIK2U3T01EcmVabTQrVg0KVHYvUTFhNFVUdnh1OEpuS1lha0JLazdYNWJqRU1kcVVDWVZ2VXN1bzgydEtBdENPNTBXRmxLdFZTd0lCSXdLQg0KZ0hoOE1Jc01VbW9nRUt6eHZRVWlxSnhzdTRmNFlJbmIzQjNLdkRqZmtzMm5YZ0V1T0Y0VTJJVXNyKzNqL3ZrUw0KMXpSUlk4N1djYnk1djFwSDBUQlBaWFF1Z1FTWUhuaXZhT2ZsSlgyRHF2eUlYOWxYeVZoQzV2YVpOVXl2MnBpKw0KKzk2dk5tY0hydGNUTURUbjU2TlRRbmp2RDN6Z1pJMU1TWm9KQTcvOUJ4T0xBa0VBMWg4dkJDeW51RWVxVUg5Vg0KcVN5YStRYjl1QWpXYjY5MTA1a2FFUVpNNFYzN2w0TU9oc1c2UVBDWnJXclZKbjNBNVY5c2ZuNmJnQnE1K2lqTQ0KZS9ndml3SkJBTklTa1JuM2h6VDJGK3hyL0hLQzYzbDZLWmpXYzMyL2lMZjhsUkxYV0tOaDFMVkw4YmhVenFqTQ0KaUFlVFJnbWNFMllnVVFNRXRjaktmdlZNbG5KZUtVRUNRUURRQVFrWi8zNWJQazJ0U0g4WlhvZnFtUk82RnphMQ0Kb3lJQXdKejZtR2Z3M3ZSblRCeTliNTc5UVg5WUFXRllsMnJ0Y3FQb21EOUpQb2pISUZHYy83a25Ba0VBa0F5Nw0KUlFHQlNPTkRtczIwYThkODVaV1kzZFRoZXN4N0FkSEZUcitOUE5WZXFEUVRkeFdWQmdpWHlxNGhaYXphR2lUSg0KMWk4ZGtQL2lCMHAxeXNRNWl3SkJBSmYvSlFOZlRRYUlpc0I0K0RKRmpZaXBOVklwUWtJZzkwd1RZWStub0JKdQ0KRVVoS09zcnAxbThjRzBqa3FnRDVsaE42OFlsVzBaZ3RGSVl4RHRIdUkyRT0NCi0tLS0tRU5EIFJTQSBQUklWQVRFIEtFWS0tLS0tDQo=</RSAKEY>
</body></html>
";
			var settingsProvider = new MHUBSettingsProvider();
			var details = new LoginDetails("http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet", "TNET", "v13t004");
			var mockLoginCommand = new Mock<LoginCommand>(details, settingsProvider, null, false);
			var loginCommand = mockLoginCommand.Object;
			loginCommand.sessionCookie = "CookieMonster";
			var mock = new Mock<GetRTKeyFileCommand>(details, settingsProvider, null, false, loginCommand) { CallBase = true };
			var getKeyCommand = mock.Object;
			var webResult = new WebResult { StatusCode = HttpStatusCode.OK, ResponseString = cannedHtmlReponse };
			mock.Protected().Setup<WebResult>("GetResponseFromMHub", ItExpr.IsAny<string>()).Returns(webResult);
			getKeyCommand.Execute(); // Don't care about return value
			AssertEquals("Login Command String is correct", "http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet?Command=getRTkey", getKeyCommand.CommandStringForTesting);
			AssertEquals(expectedDecodedRsaKey, getKeyCommand.KeyFromResponse);
			AssertEquals("Cookie from login was made available to getKey function", "CookieMonster", getKeyCommand.SessionCookie);
		}
	}
}
