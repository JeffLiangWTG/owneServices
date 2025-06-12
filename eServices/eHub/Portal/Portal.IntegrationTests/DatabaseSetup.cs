using CargoWise.eServices.TestHelpers.Database.Common;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using NUnit.Framework;
using static eServices.eHubDatabase.Tests.Common.Deployments;

namespace CargoWise.eHub.Portal.IntegrationTests
{
    [SetUpFixture]
    public class DatabaseSetup
    {
        [OneTimeSetUp]
        public void DeployDatabases()
        {
            try
            {
                Deployment.Deploy(
                Master,
                EHubTransactions
                );
				SqlServerHelper.AddTestAccountToDatabase("eHubTransactions");
				SqlServerHelper.AddTestAccountToDatabase("ediProdCache");
			}
            catch
            {
                Deployment.Recreate(
                Master,
                EHubTransactions);
				SqlServerHelper.AddTestAccountToDatabase("eHubTransactions");
				SqlServerHelper.AddTestAccountToDatabase("ediProdCache");
			}
		}
    } 
}
