using System.Data;
using CargoWise.Blazor.Common;
using CargoWise.Blazor.SessionBroker.Helpers;
using CargoWise.Data.Providers.Common;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace CargoWise.Blazor.SessionBroker.Test
{
	public abstract class TestWithDatabase
	{
		protected DatabaseAccessor databaseAccessor;
		protected CustomWebApplicationFactory<Startup> factory;
		string serverName;
		string databaseName;
		IDataProviderFactory adminConnProvider;

		[SetUp]
		public virtual void Setup()
		{
			factory = new CustomWebApplicationFactory<Startup>();

			var cargoWiseOptions = factory.Services.GetRequiredService<IOptions<CargoWiseOptions>>();
			serverName = cargoWiseOptions.Value.DbServerName;
			databaseName = cargoWiseOptions.Value.DatabaseName;
			Assert.That(string.IsNullOrWhiteSpace(serverName), Is.False);
			Assert.That(string.IsNullOrWhiteSpace(databaseName), Is.False);

			var pdsFactory = factory.Services.GetRequiredService<IProtectedDataServiceFactory>();
			var sqlConnectionProvider = factory.Services.GetRequiredService<ISqlConnectionProvider>();
			databaseAccessor = new DatabaseAccessor(NullLogger.Instance, cargoWiseOptions, pdsFactory, sqlConnectionProvider);
			adminConnProvider = new SqlDataProviderFactory(pdsFactory.CreateEnterpriseService(cargoWiseOptions.Value.DbServerName), sqlConnectionProvider);
		}

		[TearDown]
		public virtual void Teardown()
		{
			factory.Dispose();
		}

		protected IDbConnection NewAdminConnection()
		{
			return adminConnProvider.OpenNewDbConnection<OdysseyAdminCredentials>(
					serverName: serverName,
					databaseName: databaseName,
					applicationName: "Blazor.SessionBroker.Test",
					connectTimeout: 15,
					connectionPooling: true,
					loadBalanceTimeout: 0,
					maxPoolSize: 5,
					minPoolSize: 0);
		}
	}
}
