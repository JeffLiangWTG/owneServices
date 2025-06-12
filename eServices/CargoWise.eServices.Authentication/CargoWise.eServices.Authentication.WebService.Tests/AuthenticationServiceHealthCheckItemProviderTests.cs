using System.Data.Common;
using System.Data.SqlClient;

namespace CargoWise.eServices.Authentication.WebService.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using CargoWise.eServices.Monitoring.HealthCheck.API;
	using CargoWise.eServices.Monitoring.HealthCheck.API.Test;
	using Moq;
	using Moq.Protected;
	using NUnit.Framework;

	[TestFixture]
	public class AuthenticationServiceHealthCheckItemProviderTests : HealthCheckItemProviderBaseClass<AuthenticationServiceHealthCheckItemProvider>
	{
		[Test]
		public async Task TestAuthenticationServiceHealthCheckItemProvider()
		{
			var mockProvider = new Mock<AuthenticationServiceHealthCheckItemProvider>();
			mockProvider.CallBase = true;
			var provider = mockProvider.Object;

			Assert.That(provider.Name, Is.EqualTo("AuthenticationWebService"));

			mockProvider.Setup<string>(_ => _.GetAppSettings(It.IsAny<string>())).Returns((string)null);

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("ediProd SystemID not found in AppSettings in Web.config."));

			mockProvider.Setup<string>(_ => _.GetAppSettings(It.IsAny<string>())).Returns("C");

			var mockDatabaseHelper = new Mock<IDatabaseHelper>();
			mockDatabaseHelper.CallBase = true;
			var databaseHelper = mockDatabaseHelper.Object;
			mockProvider.Protected().Setup<IDatabaseHelper>("DatabaseHelper").Returns(databaseHelper);

			mockDatabaseHelper.Setup(_ => _.ReadSystemLastEditUTC(It.IsAny<string>())).Returns((DateTime?)null);
			checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("ediProd SystemID not found in database."));


			mockDatabaseHelper.Setup(_ => _.ReadSystemLastEditUTC(It.IsAny<string>())).Returns(DateTime.UtcNow.AddDays(-2));
			checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Warning));
			Assert.That(checkItem.Description, Is.EqualTo("Some records are too old."));

			mockDatabaseHelper.Setup(_ => _.ReadSystemLastEditUTC(It.IsAny<string>())).Returns(DateTime.UtcNow.AddHours(-1));
			checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.OK));
			Assert.That(checkItem.Description, Is.EqualTo("Service is alive."));

			mockDatabaseHelper.Setup(_ => _.ReadSystemLastEditUTC(It.IsAny<string>())).Throws(new InvalidOperationException());
			checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("An exception 'System.InvalidOperationException' was thrown during health check."));
		}


		[Test]
		public async Task TestAuthenticationServiceHealthCheckItemProvider_DatabaseOutageResilience()
		{
			var mockProvider = new Mock<AuthenticationServiceHealthCheckItemProvider>();
			mockProvider.CallBase = true;
			var provider = mockProvider.Object;

			mockProvider.Setup<string>(_ => _.GetAppSettings(It.Is<string>(x => x == "ediProdSystemID"))).Returns("C");
			var mockDatabaseHelper = new Mock<IDatabaseHelper>();
			mockDatabaseHelper.CallBase = true;
			var databaseHelper = mockDatabaseHelper.Object;
			mockProvider.Protected().Setup<IDatabaseHelper>("DatabaseHelper").Returns(databaseHelper);

			mockProvider.Setup<string>(_ => _.GetAppSettings(It.Is<string>(x => x == "DatabaseResilienceTimeout"))).Returns((string)null);
			mockDatabaseHelper.Setup(_ => _.ReadSystemLastEditUTC(It.IsAny<string>())).Throws(NewSqlExceptionForTest());
			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Warning));
			Assert.That(checkItem.Description, Is.EqualTo("A sql exception 'System.Data.SqlClient.SqlException' was thrown during health check."));

			mockProvider.Setup<string>(_ => _.GetAppSettings(It.Is<string>(x => x == "DatabaseResilienceTimeout"))).Returns("1");

			mockDatabaseHelper.Setup(_ => _.ReadSystemLastEditUTC(It.IsAny<string>())).Throws(NewSqlExceptionForTest());
			checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Warning));
			Assert.That(checkItem.Description, Is.EqualTo("A sql exception 'System.Data.SqlClient.SqlException' was thrown during health check."));

			System.Threading.Thread.Sleep(1000);

			mockDatabaseHelper.Setup(_ => _.ReadSystemLastEditUTC(It.IsAny<string>())).Throws(NewSqlExceptionForTest());
			checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("A sql exception 'System.Data.SqlClient.SqlException' was thrown during health check."));
		}

		protected override List<string> GetAllKindsOfDescriptions()
		{
			var descriptions = new List<string>();
			var mockProvider = new Mock<AuthenticationServiceHealthCheckItemProvider>();
			mockProvider.CallBase = true;
			var provider = mockProvider.Object;

			mockProvider.Setup<string>(_ => _.GetAppSettings(It.IsAny<string>())).Returns((string)null);
			var checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			mockProvider.Setup<string>(_ => _.GetAppSettings(It.IsAny<string>())).Returns("C");

			var mockDatabaseHelper = new Mock<IDatabaseHelper>();
			mockDatabaseHelper.CallBase = true;
			var databaseHelper = mockDatabaseHelper.Object;
			mockProvider.Protected().Setup<IDatabaseHelper>("DatabaseHelper").Returns(databaseHelper);

			mockDatabaseHelper.Setup(_ => _.ReadSystemLastEditUTC(It.IsAny<string>())).Returns((DateTime?)null);
			checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			mockDatabaseHelper.Setup(_ => _.ReadSystemLastEditUTC(It.IsAny<string>())).Returns(DateTime.UtcNow.AddDays(-2));
			checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			mockDatabaseHelper.Setup(_ => _.ReadSystemLastEditUTC(It.IsAny<string>())).Returns(DateTime.UtcNow.AddHours(-1));
			checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			mockDatabaseHelper.Setup(_ => _.ReadSystemLastEditUTC(It.IsAny<string>())).Throws(new InvalidOperationException());
			checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			return descriptions;
		}

		[Test]
		public void TestDatabaseHelper()
		{
			var provider = new AuthenticationServiceHealthCheckItemProviderForTest();
			Assert.IsTrue(provider.DatabaseHelper_Exposed != null);
		}

		class AuthenticationServiceHealthCheckItemProviderForTest : AuthenticationServiceHealthCheckItemProvider
		{
			public IDatabaseHelper DatabaseHelper_Exposed
			{
				get
				{
					return base.DatabaseHelper;
				}
			}
		}

		private SqlException NewSqlExceptionForTest()
		{
			//private SqlException(string message, SqlErrorCollection errorCollection, Exception innerException, Guid conId) 
			var arguments = new object[] { "Test SQL Exception", null, null, Guid.NewGuid() };
			return (SqlException)Activator.CreateInstance(typeof(SqlException), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, arguments, null);
		}
	}
}
