using CargoWise.eServices.TestHelpers.Database.Deployment;
using NUnit.Framework;
using static eServices.eHubDatabase.Tests.Common.Deployments;

namespace CargoWise.eServices.USCustoms.IntegrationTests
{
	[SetUpFixture]
	public class DatabaseSetup
	{
		[OneTimeSetUp]
		public void DeployDatabases()
		{
			try
			{
				Deployment.Deploy(Master, EdiProdCache, EHubTransactions);
			}
			catch
			{
				Deployment.Recreate(Master, EdiProdCache, EHubTransactions);
			}
		}
	}
}
