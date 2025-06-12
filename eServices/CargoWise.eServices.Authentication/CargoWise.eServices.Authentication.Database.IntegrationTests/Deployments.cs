using CargoWise.eServices.TestHelpers.Database.Deployment;
using Microsoft.SqlServer.Dac;

namespace CargoWise.eServices.Authentication.IntegrationTests
{
	public class Deployments
    {
		public static DeploymentInfo Authentication = new DeploymentInfo(
			"AuthenticationWebService",
			"Databases\\CargoWise.eServices.Authentication.Database\\CargoWise.eServices.Authentication.Database.dacpac",
			new DacDeployOptions
			{
				CreateNewDatabase = true,
				BlockOnPossibleDataLoss = false,
				ScriptDatabaseOptions = false,
				CompareUsingTargetCollation = false,
				ScriptDatabaseCollation = true,
				NoAlterStatementsToChangeClrTypes = false,
				DropConstraintsNotInSource = true,
				DropDmlTriggersNotInSource = true,
				DropExtendedPropertiesNotInSource = true,
				DropIndexesNotInSource = true,
				DropObjectsNotInSource = true,
				DropPermissionsNotInSource = true,
				DropRoleMembersNotInSource = true,
				DropStatisticsNotInSource = true,
				BackupDatabaseBeforeChanges = false,
				DeployDatabaseInSingleUserMode = false
			});
    }
}
