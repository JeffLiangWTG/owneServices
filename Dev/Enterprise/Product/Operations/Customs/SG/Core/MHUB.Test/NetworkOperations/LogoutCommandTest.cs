using System.Net;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.Registry;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.MHUB.Testing
{
	sealed class LogoutCommandTest : TestCaseWithFactory
	{
		[TestDate(2017, 07, 20)]
		public void TestLogout()
		{
			var settingsProvider = new MHUBSettingsProvider();
			var details = new LoginDetails("http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet", "TNET", "v13t004");
			var login = new LoginCommand(details, settingsProvider, null, false) { LoginState = LoginCommand.LoginStateType.LoggedIn };
			var mock = new Mock<LogoutCommand>(login, settingsProvider, null, false) { CallBase = true };
			var logout = mock.Object;
			var webResult = new WebResult
			{
				StatusCode = HttpStatusCode.OK,
				SessionCookie =
						"{JSESSIONID=G7zi1OX5rWC7GRagjPGA28Xzht1epiNseiADCrKgOcTA1y92Y8tC!-1414207516!NONE; path=/}",
				ResponseString = @"{<html>
<head><title>EDI Servlet</title></head>
<body>

requestStatus=0|
</body></html>
}"
			};
			mock.Protected().Setup<WebResult>("GetResponseFromMHub", ItExpr.IsAny<string>()).Returns(webResult);
			Assert("Login return true", logout.Execute());
			AssertEquals("Login Command String is correct", "http://twebsn01-wl.asianconnect.com/mhbweb/mhb/EDIServlet?Command=Login&Userid=k%2Ffk8qmsB18%3D&Password=c%2FxPot6IlXI%3D&AppId=CargoWise&DigestForLibs=Ev8ADWab7lyd6tCwjRr%2FsneYVRjETv7XyyuTcsIxwd%2BmHsVbWdkl61EvRtP%2Fda7YrPaYY4nP3%2FBCIetxASUzptDJyfGg5%2F7e&Encrypted=true&ClientId=MHXWIN&CurrentVersion=4.0.3&VndId=A7", login.CommandStringForTesting);
			AssertEquals("LoginState is loggedin", LoginCommand.LoginStateType.NotLoggedIn, login.LoginState);
			ServerResponse response = logout.ResponseParameters.GetFirst();
			AssertEquals("The Response Parameter requestStatus", "0", response.GetValue("requestStatus"));
		}
	}
}
