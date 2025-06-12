using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using CargoWise.eServices.Monitoring.HealthCheck.API.Test;
using NUnit.Framework;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CargoWise.eHub.Products.GBCustoms.Pentant.InboundMessageWebService.Tests
{
    [TestFixture]
	public class InboundMessageHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<InboundMessageWebServiceHealthCheckItemProvider>
	{
		[Test]
		public async Task TestInboundMessageWebServiceHealthCheckItemProvider_Success()
		{
			var mockProvider = MockRepository.GenerateMock<InboundMessageWebServiceHealthCheckItemProvider>();

			mockProvider.Retries = () => 0;
			var mockConnection = MockRepository.GenerateStub<SqlConnection>();
			var mockDataConnectionHelper = MockRepository.GenerateStub<IDataConnector>();

			mockDataConnectionHelper.Expect(_ => _.GeteHubTransactionsConnection("eHubTransactionsContext")).Return(mockConnection);
			mockConnection.Expect(_ => _.Open()).Repeat.Once();
			mockProvider.Expect(_ => _.DatabaseConnector).Return(mockDataConnectionHelper).Repeat.Any();
			mockProvider.Expect(_ => _.GetAppSettings("GBCustomsRegistrationType")).Return("MockValue");

			var checkItem = await mockProvider.CheckHealthAsync().ConfigureAwait(false);

			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.OK));
			Assert.That(checkItem.Description, Is.EqualTo("Service is alive."));

			mockDataConnectionHelper.VerifyAllExpectations();
			mockConnection.VerifyAllExpectations();
		}

		[Test]
		public async Task TestInboundMessageWebServiceHealthCheckItemProvider_ConfigError()
		{
			var mockProvider = MockRepository.GenerateMock<InboundMessageWebServiceHealthCheckItemProvider>();

			mockProvider.Retries = () => 0;
			var mockConnection = MockRepository.GenerateStub<SqlConnection>();
			var mockDataConnectionHelper = MockRepository.GenerateStub<IDataConnector>();

			mockDataConnectionHelper.Expect(_ => _.GeteHubTransactionsConnection("eHubTransactionsContext")).Return(mockConnection);
			mockConnection.Expect(_ => _.Open()).Repeat.Once();
			mockProvider.Expect(_ => _.DatabaseConnector).Return(mockDataConnectionHelper).Repeat.Any();
			mockProvider.Expect(_ => _.GetAppSettings("GBCustomsRegistrationType")).Return(null);

			var checkItem = await mockProvider.CheckHealthAsync().ConfigureAwait(false);

			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("GBCustomsRegistrationType not found in AppSettings in Web.config."));

			mockDataConnectionHelper.VerifyAllExpectations();
			mockConnection.VerifyAllExpectations();
		}

		[Test]
		public async Task TestCredentialWebServiceHealthCheckItemProvider_DatabaseError()
		{
			var mockProvider = MockRepository.GenerateMock<InboundMessageWebServiceHealthCheckItemProvider>();

			mockProvider.Retries = () => 1;
			var mockConnection = MockRepository.GenerateStub<SqlConnection>();
			var mockDataConnectionHelper = MockRepository.GenerateStub<IDataConnector>();

			mockDataConnectionHelper.Expect(_ => _.GeteHubTransactionsConnection("eHubTransactionsContext")).Return(mockConnection);
			mockConnection.Expect(_ => _.Open()).Throw(new Exception("Simulated exception")).Repeat.Twice();
			mockProvider.Expect(_ => _.DatabaseConnector).Return(mockDataConnectionHelper).Repeat.Any();
			mockProvider.Expect(_ => _.GetAppSettings("GBCustomsRegistrationType")).Return("MockValue");

			var checkItem = await mockProvider.CheckHealthAsync().ConfigureAwait(false);

			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("Could not create a connection to database."));

			mockDataConnectionHelper.VerifyAllExpectations();
			mockConnection.VerifyAllExpectations();
		}

		[Test]
		public async Task TestCredentialWebServiceHealthCheckItemProvider_DatabaseAndConfigError()
		{
			var mockProvider = MockRepository.GenerateMock<InboundMessageWebServiceHealthCheckItemProvider>();

			mockProvider.Retries = () => 1;
			var mockConnection = MockRepository.GenerateStub<SqlConnection>();
			var mockDataConnectionHelper = MockRepository.GenerateStub<IDataConnector>();

			mockDataConnectionHelper.Expect(_ => _.GeteHubTransactionsConnection("eHubTransactionsContext")).Return(mockConnection);
			mockConnection.Expect(_ => _.Open()).Throw(new Exception("Simulated exception")).Repeat.Twice();
			mockProvider.Expect(_ => _.DatabaseConnector).Return(mockDataConnectionHelper).Repeat.Any();

			var checkItem = await mockProvider.CheckHealthAsync().ConfigureAwait(false);

			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("GBCustomsRegistrationType not found in AppSettings in Web.config.|Could not create a connection to database."));

			mockDataConnectionHelper.VerifyAllExpectations();
			mockConnection.VerifyAllExpectations();
		}

		protected override List<string> GetAllKindsOfDescriptions()
		{
			var descriptions = new List<string>();
			return descriptions;
		}

	}
}
