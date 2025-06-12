using CargoWise.eServices.TestHelpers.Database.Deployment;
using eServices.eHubDatabase.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.IntegrationTests
{
	[SetUpFixture]
	public class SetupFixture
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
            try
            {
                Deployment.Deploy(
                Deployments.MasterSecondary,
                Deployments.EdiProdCache,
                Deployments.EHubTransactions,
                Deployments.EHubArchiveOnlineSecondary,
				Deployments.EHubArchiveOnlineView);
            }
            catch
            {
                Deployment.Recreate(
                Deployments.MasterSecondary,
                Deployments.EdiProdCache,
                Deployments.EHubTransactions,
                Deployments.EHubArchiveOnlineSecondary,
				Deployments.EHubArchiveOnlineView);
            }
		}
	}
}
