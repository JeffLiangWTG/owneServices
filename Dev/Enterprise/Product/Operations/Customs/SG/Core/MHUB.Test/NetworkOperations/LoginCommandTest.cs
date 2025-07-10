using System.Net;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.Registry;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.MHUB.Testing
{
	sealed class LoginCommandTest : TestCaseWithFactory
	{
		[TestDate(2017, 07, 20)]
		public void TestExecuteWithValidDetails()
		{
			LoginDetails details = new LoginDetails("http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet", "TNET", "v13t004");
			var mock = new Mock<LoginCommand>(details, settingsProvider, null, false) { CallBase = true };
			LoginCommand login = mock.Object;
			WebResult webResult = new WebResult
			{
				StatusCode = HttpStatusCode.OK,
				SessionCookie =
						"{JSESSIONID=G7zi1OX5rWC7GRagjPGA28Xzht1epiNseiADCrKgOcTA1y92Y8tC!-1414207516!NONE; path=/}",
				ResponseString = @"{<html>
<head><title>EDI Servlet</title></head>
<body>

requestStatus=0|sessionId=1182495651091|</body></html>
}"
			};
			mock.Protected().Setup<WebResult>("GetResponseFromMHub", ItExpr.IsAny<string>()).Returns(webResult);
			Assert("Login return true", login.Execute());
			AssertEquals("Login Command String is correct", "http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet?Command=Login&Userid=k%2Ffk8qmsB18%3D&Password=c%2FxPot6IlXI%3D&AppId=CargoWise&DigestForLibs=Ev8ADWab7lyd6tCwjRr%2FsneYVRjETv7XyyuTcsIxwd%2BmHsVbWdkl61EvRtP%2Fda7YrPaYY4nP3%2FBCIetxASUzptDJyfGg5%2F7e&Encrypted=true&ClientId=MHXWIN&CurrentVersion=4.0.3&VndId=A7", login.CommandStringForTesting);
			AssertEquals("LoginState is loggedin", LoginCommand.LoginStateType.LoggedIn, login.LoginState);
			ServerResponse response = login.ResponseParameters.GetFirst();
			AssertEquals("The Response Parameter requestStatus", "0", response.GetValue("requestStatus"));
			AssertEquals("The Response Parameter sessionId", "1182495651091", response.GetValue("sessionId"));
		}

		[TestDate(2017, 08, 10)]
		public void TestExecuteWithNewDetails()
		{
			LoginDetails details = new LoginDetails("http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet", "TNET", "v13t004");
			var mock = new Mock<LoginCommand>(details, settingsProvider, null, false) { CallBase = true };
			LoginCommand login = mock.Object;
			WebResult webResult = new WebResult
			{
				StatusCode = HttpStatusCode.OK,
				SessionCookie =
						"{JSESSIONID=G7zi1OX5rWC7GRagjPGA28Xzht1epiNseiADCrKgOcTA1y92Y8tC!-1414207516!NONE; path=/}",
				ResponseString = @"{<html>
<head><title>EDI Servlet</title></head>
<body>

requestStatus=0|sessionId=1182495651091|</body></html>
}"
			};
			mock.Protected().Setup<WebResult>("GetResponseFromMHub", ItExpr.IsAny<string>()).Returns(webResult);
			Assert("Login return true", login.Execute());
			AssertEquals("Login Command String is correct for MHAccess upgrade (2017/07/24)", "http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet?Command=Login&Userid=k%2Ffk8qmsB18%3D&Password=c%2FxPot6IlXI%3D&AppId=CargoWise&DigestForLibs=6zqJ6sXqX0hEaoEFZdJ6TG%2FYuHn3mq2c0CZ4KlsR3RUQC82evqhjH64qB4gVj6F%2F2xBD5sYAAaaa5%2BXyLT11F9DJyfGg5%2F7e&Encrypted=true&ClientId=MHXWIN&CurrentVersion=4.0.4&VndId=A7", login.CommandStringForTesting);
			AssertEquals("LoginState is loggedin", LoginCommand.LoginStateType.LoggedIn, login.LoginState);
			ServerResponse response = login.ResponseParameters.GetFirst();
			AssertEquals("The Response Parameter requestStatus", "0", response.GetValue("requestStatus"));
			AssertEquals("The Response Parameter sessionId", "1182495651091", response.GetValue("sessionId"));
		}

		[TestDate(2017, 07, 20)]
		public void TestExecuteWithInvalidDetails()
		{
			LoginDetails details = new LoginDetails("http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet", "CargoWise", "v13t004");
			var mock = new Mock<LoginCommand>(details, settingsProvider, null, false) { CallBase = true };
			LoginCommand login = mock.Object;
			WebResult webResult = new WebResult();
			webResult.StatusCode = HttpStatusCode.OK;
			webResult.SessionCookie = "{JSESSIONID=G76G9EtaWFv1qz5x8VWDnkfZjZEH1NBi2CdEH3EI96vpD215IepR!-1602168798!-1414207516; path=/}";
			webResult.ResponseString = @"{{<html>
<head><title>EDI Servlet</title></head>
<body>

requestStatus=-1|Server Exception caught: 1896::Invalid User ID / Password</body></html>
}";
			mock.Protected().Setup<WebResult>("GetResponseFromMHub", ItExpr.IsAny<string>()).Returns(webResult);
			Assert("Login return false", !login.Execute());
			AssertEquals("Login Command String is correct", "http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet?Command=Login&Userid=ISJTN7e6duJQhfKYjTuJDQ%3D%3D&Password=c%2FxPot6IlXI%3D&AppId=CargoWise&DigestForLibs=Ev8ADWab7lyd6tCwjRr%2FsneYVRjETv7XyyuTcsIxwd%2BmHsVbWdkl61EvRtP%2Fda7YrPaYY4nP3%2FBCIetxASUzptDJyfGg5%2F7e&Encrypted=true&ClientId=MHXWIN&CurrentVersion=4.0.3&VndId=A7", login.CommandStringForTesting);
			AssertEquals("LoginState is loggedin", LoginCommand.LoginStateType.NotLoggedIn, login.LoginState);
			ServerResponse response = login.ResponseParameters.GetFirst();
			AssertEquals("The Response Parameter requestStatus", "-1", response.GetValue("requestStatus"));
			AssertEquals("The Response Parameter ErrorCode", "1896", response.GetValue("ErrorCode"));
			AssertEquals("The Response Parameter ErrorMsg", "Invalid User ID / Password", response.GetValue("ErrorMsg"));
		}

		public void TestAccountFrozenResponse()
		{
			LoginDetails details = new LoginDetails("http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet", "CargoWise", "v13t004", "ediCrap");
			var mock = new Mock<LoginCommand>(details, settingsProvider, null, false) { CallBase = true };
			LoginCommand login = mock.Object;
			WebResult webResult = new WebResult
			{
				StatusCode = HttpStatusCode.OK,
				SessionCookie =
						"{JSESSIONID=G76G9EtaWFv1qz5x8VWDnkfZjZEH1NBi2CdEH3EI96vpD215IepR!-1602168798!-1414207516; path=/}",
				ResponseString = @"{{<html>
<head><title>EDI Servlet</title></head>
<body>

requestStatus=-1|Server Exception caught: 1326::User account currently frozen</body></html>
}"
			};
			mock.Protected().Setup<WebResult>("GetResponseFromMHub", ItExpr.IsAny<string>()).Returns(webResult);
			Assert("Login return false", !login.Execute());
			AssertNotNull("Broker Account Error", login.BrokerAccountError);
			AssertEquals("The deactivation code is correct", SGDeactivationCodes.Codes.FRZ, login.BrokerAccountError.DeactivationCode);
			AssertEquals("The error message is correct", "User account currently frozen", login.BrokerAccountError.ErrorMessage);
		}

		public void TestPasswordExpiredResponse()
		{
			LoginDetails details = new LoginDetails("http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet", "CargoWise", "v13t004", "ediCrap");
			var mock = new Mock<LoginCommand>(details, settingsProvider, null, false) { CallBase = true };
			LoginCommand login = mock.Object;
			WebResult webResult = new WebResult
			{
				StatusCode = HttpStatusCode.OK,
				SessionCookie =
						"{JSESSIONID=G76G9EtaWFv1qz5x8VWDnkfZjZEH1NBi2CdEH3EI96vpD215IepR!-1602168798!-1414207516; path=/}",
				ResponseString = @"{{<html>
<head><title>EDI Servlet</title></head>
<body>

requestStatus=-1|Server Exception caught: 1307::Password has expired</body></html>
}"
			};
			mock.Protected().Setup<WebResult>("GetResponseFromMHub", ItExpr.IsAny<string>()).Returns(webResult);
			Assert("Login return false", !login.Execute());
			AssertNotNull("Broker Account Error", login.BrokerAccountError);
			AssertEquals("The deactivation code is correct", SGDeactivationCodes.Codes.PEX, login.BrokerAccountError.DeactivationCode);
			AssertEquals("The error message is correct", "Password has expired", login.BrokerAccountError.ErrorMessage);
		}

		public void TestInvalidUserIdOrPasswordResponse()
		{
			LoginDetails details = new LoginDetails("http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet", "CargoWise", "v13t004", "ediCrap");
			var mock = new Mock<LoginCommand>(details, settingsProvider, null, false) { CallBase = true };
			LoginCommand login = mock.Object;
			WebResult webResult = new WebResult
			{
				StatusCode = HttpStatusCode.OK,
				SessionCookie =
						"{JSESSIONID=G76G9EtaWFv1qz5x8VWDnkfZjZEH1NBi2CdEH3EI96vpD215IepR!-1602168798!-1414207516; path=/}",
				ResponseString = @"{{<html>
<head><title>EDI Servlet</title></head>
<body>

requestStatus=-1|Server Exception caught: 1311::Invalid User ID / Password</body></html>
}"
			};
			mock.Protected().Setup<WebResult>("GetResponseFromMHub", ItExpr.IsAny<string>()).Returns(webResult);
			Assert("Login return false", !login.Execute());
			AssertNotNull("Broker Account Error", login.BrokerAccountError);
			AssertEquals("The deactivation code is correct", SGDeactivationCodes.Codes.IID, login.BrokerAccountError.DeactivationCode);
			AssertEquals("The error message is correct", "Invalid User ID / Password", login.BrokerAccountError.ErrorMessage);
		}

		public void TestPasswordNeedsChangingResponse()
		{
			LoginDetails details = new LoginDetails("http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet", "CargoWise", "v13t004", "ediCrap");
			var mock = new Mock<LoginCommand>(details, settingsProvider, null, false) { CallBase = true };
			LoginCommand login = mock.Object;
			WebResult webResult = new WebResult
			{
				StatusCode = HttpStatusCode.OK,
				SessionCookie =
						"{JSESSIONID=G76G9EtaWFv1qz5x8VWDnkfZjZEH1NBi2CdEH3EI96vpD215IepR!-1602168798!-1414207516; path=/}",
				ResponseString = @"{{<html>
<head><title>EDI Servlet</title></head>
<body>

requestStatus=-1|Server Exception caught: 1318::User needs to change the password</body></html>
}"
			};
			mock.Protected().Setup<WebResult>("GetResponseFromMHub", ItExpr.IsAny<string>()).Returns(webResult);
			Assert("Login return false", !login.Execute());
			AssertNotNull("Broker Account Error", login.BrokerAccountError);
			AssertEquals("The deactivation code is correct", SGDeactivationCodes.Codes.PCH, login.BrokerAccountError.DeactivationCode);
			AssertEquals("The error message is correct", "User needs to change the password", login.BrokerAccountError.ErrorMessage);
		}

		public void TestAccountDoesNotExistResponse()
		{
			LoginDetails details = new LoginDetails("http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet", "CargoWise", "v13t004", "ediCrap");
			var mock = new Mock<LoginCommand>(details, settingsProvider, null, false) { CallBase = true };
			LoginCommand login = mock.Object;
			WebResult webResult = new WebResult
			{
				StatusCode = HttpStatusCode.OK,
				SessionCookie =
						"{JSESSIONID=G76G9EtaWFv1qz5x8VWDnkfZjZEH1NBi2CdEH3EI96vpD215IepR!-1602168798!-1414207516; path=/}",
				ResponseString = @"{{<html>
<head><title>EDI Servlet</title></head>
<body>

requestStatus=-1|Server Exception caught: 1807::Login ID doesn't exist in M-Hub for user</body></html>
}"
			};
			mock.Protected().Setup<WebResult>("GetResponseFromMHub", ItExpr.IsAny<string>()).Returns(webResult);
			Assert("Login return false", !login.Execute());
			AssertNotNull("Broker Account Error", login.BrokerAccountError);
			AssertEquals("The deactivation code is correct", SGDeactivationCodes.Codes.ANE, login.BrokerAccountError.DeactivationCode);
			AssertEquals("The error message is correct", "Login ID doesn't exist in M-Hub for user", login.BrokerAccountError.ErrorMessage);
		}

		public void TestPasswordIncorrectResponse()
		{
			LoginDetails details = new LoginDetails("http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet", "CargoWise", "v13t004", "ediCrap");
			var mock = new Mock<LoginCommand>(details, settingsProvider, null, false) { CallBase = true };
			LoginCommand login = mock.Object;
			WebResult webResult = new WebResult
			{
				StatusCode = HttpStatusCode.OK,
				SessionCookie =
						"{JSESSIONID=G76G9EtaWFv1qz5x8VWDnkfZjZEH1NBi2CdEH3EI96vpD215IepR!-1602168798!-1414207516; path=/}",
				ResponseString = @"{{<html>
<head><title>EDI Servlet</title></head>
<body>

requestStatus=-1|Server Exception caught: 1308::Password is incorrect</body></html>
}"
			};
			mock.Protected().Setup<WebResult>("GetResponseFromMHub", ItExpr.IsAny<string>()).Returns(webResult);
			Assert("Login return false", !login.Execute());
			AssertNotNull("Broker Account Error", login.BrokerAccountError);
			AssertEquals("The deactivation code is correct", SGDeactivationCodes.Codes.PIC, login.BrokerAccountError.DeactivationCode);
			AssertEquals("The error message is correct", "Password is incorrect", login.BrokerAccountError.ErrorMessage);
		}

		public void TestInvalidNewResponse()
		{
			LoginDetails details = new LoginDetails("http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet", "CargoWise", "v13t004", "ediCrap");
			var mock = new Mock<LoginCommand>(details, settingsProvider, null, false) { CallBase = true };
			LoginCommand login = mock.Object;
			WebResult webResult = new WebResult
			{
				StatusCode = HttpStatusCode.OK,
				SessionCookie =
						"{JSESSIONID=G76G9EtaWFv1qz5x8VWDnkfZjZEH1NBi2CdEH3EI96vpD215IepR!-1602168798!-1414207516; path=/}",
				ResponseString = @"{{<html>
<head><title>EDI Servlet</title></head>
<body>

requestStatus=-1|Server Exception caught: 1803::Invalid password has been used</body></html>
}"
			};
			mock.Protected().Setup<WebResult>("GetResponseFromMHub", ItExpr.IsAny<string>()).Returns(webResult);
			Assert("Login return false", !login.Execute());
			AssertNotNull("Broker Account Error", login.BrokerAccountError);
			AssertEquals("The deactivation code is correct", SGDeactivationCodes.Codes.PCS, login.BrokerAccountError.DeactivationCode);
			AssertEquals("The error message is correct", "Invalid password has been used", login.BrokerAccountError.ErrorMessage);
		}

		readonly MHUBSettingsProvider settingsProvider = new MHUBSettingsProvider();
	}
}
