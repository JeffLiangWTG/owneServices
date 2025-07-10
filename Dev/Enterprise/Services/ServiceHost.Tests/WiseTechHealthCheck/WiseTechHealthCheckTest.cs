using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Registry.Business;
using Enterprise.Services.ServiceHost.WiseTechHealthCheck;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	class WiseTechHealthCheckTest : TestCase
	{
		public void TestWiseTechHealthCheckNotAllowedForVerb()
		{
			AssertNotAllowedForVerb("POST");
			AssertNotAllowedForVerb("PUT");
			AssertNotAllowedForVerb("DELETE");
			AssertNotAllowedForVerb("OPTIONS");
			AssertNotAllowedForVerb("ACE");
			AssertNotAllowedForVerb("TRACE");
			AssertNotAllowedForVerb("CONNECT");
		}

		void AssertNotAllowedForVerb(string verb)
		{
			var handler = new WiseTechHealthCheckHttpHandler();

			requestMock.Setup(r => r.HttpMethod).Returns(verb);

			handler.ProcessRequest(contextMock.Object);
			AssertEquals((int)HttpStatusCode.MethodNotAllowed, responseMock.Object.StatusCode);
		}

		[UseSnapshotProtection]
		public void TestReturnsForbiddenForIPAddressPair()
		{
			AssertReturnsForbiddenForIPAddressPair("acv,IO2N82h89ndjhwuiodhjqwhji", "127.0.0.1", null);
			AssertReturnsForbiddenForIPAddressPair("127.0.0.1", "asdnasd", null);

			var ruleset = new InternetAddressRuleset();
			var rule1 = ruleset.AddNew();
			rule1.Text = "192.168.1.1";
			rule1.Enabled = true;

			var rule2 = ruleset.AddNew();
			rule2.Text = "192.168.3.1-192.168.3.10";
			rule2.Enabled = true;

			var rule3 = ruleset.AddNew();
			rule3.Text = "2001:0db8::/64";
			rule3.Enabled = true;

			WebDataRegistry.Instance.HealthCheckAccessIPWhitelistRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ruleset);
			AssertReturnsForbiddenForIPAddressPair("192.168.1.2", "127.0.0.1", null);
			AssertReturnsForbiddenForIPAddressPair("192.168.3.11", "127.0.0.1", null);
			AssertReturnsForbiddenForIPAddressPair("2001:0db9::", "127.0.0.1", null);
			AssertReturnsForbiddenForIPAddressPair("192.168.1.1", null, null); // Local address is empty
			AssertReturnsForbiddenForIPAddressPair("10.61.178.129", "127.0.0.1", "X-Forwarded-For", "52.230.19.157"); // X-Forwarded address is not in the white ip list
			AssertReturnsForbiddenForIPAddressPair("52.230.19.157", "127.0.0.1", "X-Forwarded-For", "192.168.1.1"); // Will ignore X-Forwarded address if host address is not private
		}

		void AssertReturnsForbiddenForIPAddressPair(string userHostAddress, string localAddress, string header, string headValues = null)
		{
			requestMock.Setup(r => r.HttpMethod).Returns(HttpMethod.Get.Method);
			requestMock.Setup(r => r.UserHostAddress).Returns(userHostAddress);

			serverVariables["LOCAL_ADDR"] = localAddress;

			if (header != null)
			{
				headers[header] = headValues ?? "ItDoesn'tReallyMatterWhatTheValueIs";
			}

			var handler = new WiseTechHealthCheckHttpHandler();
			handler.ProcessRequest(contextMock.Object);
			AssertEquals((int)HttpStatusCode.Forbidden, responseMock.Object.StatusCode);
		}

		public void TestWiseTechHealthCheck()
		{
			var handler = new WiseTechHealthCheckHttpHandler();

			using (var ms = new MemoryStream())
			{
				requestMock.Setup(r => r.HttpMethod).Returns(HttpMethod.Get.Method);
				responseMock.Setup(r => r.OutputStream).Returns(ms);

				handler.ProcessRequest(contextMock.Object);
				var responseBody = Encoding.UTF8.GetString(ms.ToArray());

				CombineAssertions(() =>
				{
					AssertEquals((int)HttpStatusCode.OK, responseMock.Object.StatusCode);
					AssertContains("INFO(Database): OK", responseBody);
				});
			}
		}

		[UseSnapshotProtection]
		public void TestWiseTechHealthCheckDoesntCauseDatabaseUpgradeExceptionCaughtExceptionErrorReport()
		{
			int schemaMajorVersion = 0;

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				using (new DisposableAction(() => SimulateDatabaseUpgradedScenario(connection), () => CleanUpDatabaseUpgradedScenario(connection)))
				{
					WiseTechHealthCheckHttpHandler.CheckDb(TimeSpan.FromSeconds(10));
					WiseTechHealthCheckHttpHandler.CheckDb(TimeSpan.FromSeconds(10));

					AssertEquals("No Errors should be reported from WiseTechHealthCheckHttpHandler", 0, ErrorReporter.TotalErrorCount);
				}
			}

			void SimulateDatabaseUpgradedScenario(DbConnection connection)
			{
				schemaMajorVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(connection);
				DbRegistry.DatabaseMajorSchemaVersion.SaveValue(schemaMajorVersion + 1, connection);
			}

			void CleanUpDatabaseUpgradedScenario(DbConnection connection)
			{
				DbRegistry.DatabaseMajorSchemaVersion.SaveValue(schemaMajorVersion, connection);
				ErrorReporter.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestWiseTechHealthCheckFromNewThread()
		{
			var handler = new WiseTechHealthCheckHttpHandler();

			using (var ms = new MemoryStream())
			{
				requestMock.Setup(r => r.HttpMethod).Returns(HttpMethod.Get.Method);
				responseMock.Setup(r => r.OutputStream).Returns(ms);

				var thread = new Thread(() =>
				{
					handler.ProcessRequest(contextMock.Object);
				});

				thread.Start();
				thread.Join();
			}
		}

		[UseSnapshotProtection]
		public void TestDatabaseUpgradeInProgressOfflineStatus()
		{
			const string expectedMessage = "INFO(Database): The Database is currently being upgraded, please refresh the page in a few minutes.";

			// Arrange
			using (Db.DisposableUpgrade_ForTest(acquireLockOut: true, killOtherConnections: false, updateSchemaVersion: false))
			{
				// Act
				var (status, statusMessage) = GetWtgStatus();

				// Assert
				AssertContains(expectedMessage, statusMessage);
				AssertEquals(HttpStatusCode.OK, status);
			}
		}

		[UseSnapshotProtection]
		public void TestDatabaseUpgradedStatus()
		{
			const string expectedMessage = "INFO(Database): The Database has been upgraded and Web Application is being upgraded, please refresh the page in a few minutes.";

			// Arrange
			using (Db.DisposableUpgrade_ForTest(acquireLockOut: false, killOtherConnections: false, updateSchemaVersion: true))
			{
				// Act
				var (status, statusMessage) = GetWtgStatus();

				// Assert
				AssertContains(expectedMessage, statusMessage);
				AssertEquals(HttpStatusCode.OK, status);
			}
		}

		public void TestDatabaseHealthyStatus()
		{
			const string expectedMessage = "INFO(Database): OK";

			// Arrange
			// Act
			var (status, statusMessage) = GetWtgStatus();

			// Assert
			AssertContains(expectedMessage, statusMessage);
			AssertEquals(HttpStatusCode.OK, status);
		}

		(HttpStatusCode status, string statusMessage) GetWtgStatus()
		{
			// Arrange
			var handler = new WiseTechHealthCheckHttpHandler();
			using (var memoryStream = new MemoryStream())
			{
				requestMock.Setup(x => x.HttpMethod).Returns(HttpMethod.Get.Method);
				responseMock.Setup(x => x.OutputStream).Returns(memoryStream);
				var context = contextMock.Object;

				// Act
				handler.ProcessRequest(context);

				// Result
				var responseBody = Encoding.UTF8.GetString(memoryStream.ToArray());
				return ((HttpStatusCode)context.Response.StatusCode, responseBody);
			}
		}

		public void TestGetStatusFromCheckDbResult_Timeout()
		{
			var status = WiseTechHealthCheckHttpHandler.GetStatusFromCheckDbResult(taskCompleted: false, ex: null);
			AssertEquals(false, status.isOk);
			AssertEquals("ERROR(Database): Database is not responding.", status.statusMessage);
		}

		protected override void SetUp()
		{
			contextMock = new Mock<HttpContextBase>();
			requestMock = new Mock<HttpRequestBase>();
			responseMock = new Mock<HttpResponseBase>();

			headers = new NameValueCollection();

			serverVariables = new NameValueCollection();
			serverVariables["LOCAL_ADDR"] = IPAddress.Loopback.ToString();

			requestMock.SetupAllProperties();
			requestMock.Setup(r => r.Headers).Returns(headers);
			requestMock.Setup(r => r.ServerVariables).Returns(serverVariables);
			requestMock.Setup(r => r.UserHostAddress).Returns(IPAddress.Loopback.ToString());
			requestMock.Setup(r => r.ServerVariables).Returns(serverVariables);

			contextMock.Setup(r => r.Request).Returns(requestMock.Object);
			contextMock.Setup(r => r.Response).Returns(responseMock.Object);

			responseMock.SetupAllProperties();
		}

		Mock<HttpContextBase> contextMock;
		Mock<HttpRequestBase> requestMock;
		Mock<HttpResponseBase> responseMock;

		NameValueCollection headers;
		NameValueCollection serverVariables;
	}
}
