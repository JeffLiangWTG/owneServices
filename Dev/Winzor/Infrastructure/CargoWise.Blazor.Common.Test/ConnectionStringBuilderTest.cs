using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Blazor.Common.Data;
using CargoWise.Data.Providers.Common;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace CargoWise.Blazor.Common.Test
{
	internal class ConnectionStringBuilderTest
	{
		[Test]
		public void ConnectionStringIsValidTest()
		{
				string connectionString = ConnectionStringBuilder.GetConnectionString(Environment.MachineName, "Odyssey", "Blazor.SessionBroker.Test");
				Assert.That(string.IsNullOrWhiteSpace(connectionString), Is.False);
				Assert.That(connectionString, Does.Contain(Environment.MachineName));
				Assert.That(connectionString, Does.Contain("Odyssey"));
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		[Test]
		public void CheckConnectionTest()
		{
			var config = new TestConfiguration();
			var cwOptions = ConfigurationBinder.Get<CargoWiseOptions>(config.GetSection("CargoWiseOptions")) ?? new CargoWiseOptions();
			Assert.That(string.IsNullOrWhiteSpace(cwOptions.DbServerName), Is.False);
			Assert.That(string.IsNullOrWhiteSpace(cwOptions.DatabaseName), Is.False);
			string connectionString = ConnectionStringBuilder.GetConnectionString(cwOptions.DbServerName, cwOptions.DatabaseName, "Blazor.SessionBroker.Test");
			Assert.That(string.IsNullOrWhiteSpace(connectionString), Is.False);
			using (var connection = new SqlConnection(connectionString))
			{
				connection.Open();
				Assert.That(connection.State, Is.EqualTo(System.Data.ConnectionState.Open));
			}
		}

		[Test]
		public void EnabledMultiSubnetFailover()
		{
			// Arrange
			var serverName = "server1";
			using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: new[] { serverName }))
			{
				// Act
				var connectionString = ConnectionStringBuilder.GetConnectionString(serverName, "database1", "app1");

				// Assert
				Assert.That(new SqlConnectionStringBuilder(connectionString).MultiSubnetFailover, Is.EqualTo(true));
			}
		}
	}
}
