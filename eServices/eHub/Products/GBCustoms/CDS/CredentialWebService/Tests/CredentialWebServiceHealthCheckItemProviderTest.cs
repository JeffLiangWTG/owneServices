namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Data.SqlClient;
	using System.Threading.Tasks;

	using CargoWise.eServices.Monitoring.HealthCheck.API;
	using CargoWise.eServices.Monitoring.HealthCheck.API.Test;

	using Rhino.Mocks;

	using NUnit.Framework;
	
    [TestFixture]
	public class CredentialWebServiceHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<CredentialWebServiceHealthCheckItemProvider>
	{
		[Test]
		public async Task TestCredentialWebServiceHealthCheckItemProvider_Success()
		{
			var mockProvider = MockRepository.GenerateMock<CredentialWebServiceHealthCheckItemProvider>();

			mockProvider.Retries = () => 0;
			var mockConnection = MockRepository.GenerateStub<SqlConnection>();
			var mockConfigHelper = MockRepository.GenerateStub<IConfigurationHelper>();

			mockConfigHelper.Expect(_ => _.GeteHubTransactionsConnection()).Return(mockConnection);
			mockConnection.Expect(_ => _.Open()).Repeat.Once();
			mockProvider.Expect(_ => _.DatabaseConnector).Return(mockConfigHelper).Repeat.Any();

			var checkItem = await mockProvider.CheckHealthAsync().ConfigureAwait(false);

			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.OK));
			Assert.That(checkItem.Description, Is.EqualTo("Service is alive."));

			mockConfigHelper.VerifyAllExpectations();
			mockConnection.VerifyAllExpectations();
		}

		[Test]
		public async Task TestCredentialWebServiceHealthCheckItemProvider_DatabaseError()
		{
			var mockProvider = MockRepository.GenerateMock<CredentialWebServiceHealthCheckItemProvider>();

			mockProvider.Retries = () => 1;
			var mockConnection = MockRepository.GenerateStub<SqlConnection>();
			var mockConfigHelper = MockRepository.GenerateStub<IConfigurationHelper>();

			mockConfigHelper.Expect(_ => _.GeteHubTransactionsConnection()).Return(mockConnection);
			mockConnection.Expect(_ => _.Open()).Throw(new Exception("Simulated exception")).Repeat.Twice();
			mockProvider.Expect(_ => _.DatabaseConnector).Return(mockConfigHelper).Repeat.Any();

			var checkItem = await mockProvider.CheckHealthAsync().ConfigureAwait(false);

			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("\r\nCould not create a connection to database."));

			mockConfigHelper.VerifyAllExpectations();
			mockConnection.VerifyAllExpectations();
		}

		protected override List<string> GetAllKindsOfDescriptions()
		{
			var descriptions = new List<string>();
			return descriptions;
		}

	}
}
