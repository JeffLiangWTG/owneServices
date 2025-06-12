using CargoWise.eServices.Billing.Tests.Common;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.DataAccess.Tests
{
	[SetUpFixture]
	public class SetupFixture
	{
		[OneTimeSetUp]
		public void RunBeforeTests()
		{
			try
			{
                Deployment.Deploy(Deployments.BillingForTest);
            }
            catch
            {
                Deployment.Recreate(Deployments.BillingForTest);
            }            
		}
	}
}