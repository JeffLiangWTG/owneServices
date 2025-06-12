using CargoWise.eServices.TestHelpers.Database.Common;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using Microsoft.SqlServer.Dac;

namespace CargoWise.eServices.Billing.Tests.Common
{
	public static class Deployments
	{
		public static DeploymentInfo Billing = 
			new DeploymentInfo(
					IntegrationTestingDbName,
				"Databases\\CargoWise.eServices.Billing.Database\\CargoWise.eServices.Billing.Database.dacpac",
				new DacDeployOptions().WithEHubOptions().WithCommonOptions())
			.WithPostDeploymentAction(() => 
				SqlServerHelper.AddTestAccountToDatabase(IntegrationTestingDbName));

		public static DeploymentInfo BillingForTest = 
			new DeploymentInfo(
				UnitTestingDbName, 
				Billing.DacpacFile.FullName, 
				Billing.Options);

		public const string IntegrationTestingDbName = "CargoWise.eServices.Billing.IntegrationTesting";
		public const string UnitTestingDbName = "CargoWise.eServices.Billing.UnitTesting";
	}
}
