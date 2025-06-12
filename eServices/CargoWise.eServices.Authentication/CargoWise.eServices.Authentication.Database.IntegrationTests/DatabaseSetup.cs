using CargoWise.eServices.TestHelpers.Database.Deployment;
using NUnit.Framework;

namespace CargoWise.eServices.Authentication.IntegrationTests
{
	[SetUpFixture]
	public class DatabaseSetup
	{
		[OneTimeSetUp]
		public void InitializeAssembly()
		{
            try
            {
                Deployment.Deploy(Deployments.Authentication);
            }
            catch
            {
                Deployment.Recreate(Deployments.Authentication);
            }
		}
	}
}
