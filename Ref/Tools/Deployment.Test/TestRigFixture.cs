using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Dat.Integration.Deployment;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Deployment.Test
{
	[TestFixture]
	[Property("DAT:CapabilityRequirements", "SQL2019+")]
	internal class TestRigFixture
	{
		TaskInfo CreateTaskInfo(string taskComments)
		{
			return new TaskInfo("SH0Testing", "Test", taskComments);
		}

		TestedShelfDeploymentOptions CreateShelfDeploymentOptions(TaskInfo taskInfo)
		{
			return new TestedShelfDeploymentOptions(taskInfo);
		}

		[Test]
		public void DefaultDeploymentOptions()
		{
			var taskInfo = CreateTaskInfo(@"
TestRigWebSiteName:WITesting");
			var deploymentOptions = CreateShelfDeploymentOptions(taskInfo);

			Assert.That(deploymentOptions.DeliveryServiceDomain, Is.EqualTo($"WITesting.{TestedShelfDeploymentOptions.Defaults.DeliveryServiceRootDomain}"));
			Assert.That(deploymentOptions.DeliveryServiceFilesLocalPath, Is.EqualTo($"{TestedShelfDeploymentOptions.Defaults.WebServerLocalPath}\\WITesting.{TestedShelfDeploymentOptions.Defaults.DeliveryServiceRootDomain}"));
			Assert.That(deploymentOptions.DeliveryServiceFilesSharedPath, Is.EqualTo($"\\\\{TestedShelfDeploymentOptions.Defaults.WebServer}\\{TestedShelfDeploymentOptions.Defaults.WebServerSharedPath}\\WITesting.{TestedShelfDeploymentOptions.Defaults.DeliveryServiceRootDomain}"));
			Assert.That(deploymentOptions.UpdateServiceDomain, Is.EqualTo($"WITesting.{TestedShelfDeploymentOptions.Defaults.UpdateServiceRootDomain}"));
			Assert.That(deploymentOptions.UpdateServiceFilesLocalPath, Is.EqualTo($"{TestedShelfDeploymentOptions.Defaults.WebServerLocalPath}\\WITesting.{TestedShelfDeploymentOptions.Defaults.UpdateServiceRootDomain}"));
			Assert.That(deploymentOptions.UpdateServiceFilesSharedPath, Is.EqualTo($"\\\\{TestedShelfDeploymentOptions.Defaults.WebServer}\\{TestedShelfDeploymentOptions.Defaults.WebServerSharedPath}\\WITesting.{TestedShelfDeploymentOptions.Defaults.UpdateServiceRootDomain}"));
			Assert.That(deploymentOptions.RefDbRepoSafeRestoreFromBackup, Is.EqualTo(""));
			Assert.That(deploymentOptions.RefDbRepoStagingRestoreFromBackup, Is.EqualTo(""));
			Assert.That(deploymentOptions.SafeDbName, Is.EqualTo("WITestingRefDbRepoSafe"));
			Assert.That(deploymentOptions.StagingDbName, Is.EqualTo("WITestingRefDbRepoStaging"));
			Assert.That(deploymentOptions.SqlServer, Is.EqualTo("localhost")); //default when "debug"
			Assert.That(deploymentOptions.WebSiteName, Is.EqualTo("WITesting"));
		}

		[Test]
		public void CustomDeploymentOptions()
		{
			var taskInfo = CreateTaskInfo(@"
TestRigWebSiteName:WICustom
TestRigDeliveryServiceDomain:WICustom.customdeliveryservice.domain
TestRigDeliveryServiceFilesSharedPath:\\somenetworklocal\sharedDeliveryFolder
TestRigUpdateServiceDomain:WICustom.customupdateservice.domain
TestRigUpdateServiceFilesSharedPath:\\somenetworklocal\sharedUpdateFolder
TestRigRefDbRepoSafeRestoreFromBackup:\\network\\db_backups\safedb.bak
TestRigRefDbRepoStagingRestoreFromBackup:\\network\\db_backups\stagingdb.bak
TestRigSqlServer:\\MYSQLServer\INSTANCE1
TestRigDeployUsername:myusername
TestRigDeployPassword:mypass
TestRigQuartzPort:9010
TestRigWebServer:myserver.sand.pit.wtg
");

			var deploymentOptions = CreateShelfDeploymentOptions(taskInfo);

			Assert.That(deploymentOptions.DeliveryServiceDomain, Is.EqualTo("WICustom.customdeliveryservice.domain"));
			Assert.That(deploymentOptions.DeliveryServiceFilesSharedPath, Is.EqualTo(@"\\somenetworklocal\sharedDeliveryFolder"));
			Assert.That(deploymentOptions.UpdateServiceDomain, Is.EqualTo("WICustom.customupdateservice.domain"));
			Assert.That(deploymentOptions.UpdateServiceFilesSharedPath, Is.EqualTo(@"\\somenetworklocal\sharedUpdateFolder"));
			Assert.That(deploymentOptions.RefDbRepoSafeRestoreFromBackup, Is.EqualTo(@"\\network\\db_backups\safedb.bak"));
			Assert.That(deploymentOptions.RefDbRepoStagingRestoreFromBackup, Is.EqualTo(@"\\network\\db_backups\stagingdb.bak"));
			Assert.That(deploymentOptions.SqlServer, Is.EqualTo("\\\\MYSQLServer\\INSTANCE1"));
			Assert.That(deploymentOptions.WebSiteName, Is.EqualTo("WICustom"));
			Assert.That(deploymentOptions.DeployUsername, Is.EqualTo("myusername"));
			Assert.That(deploymentOptions.DeployPassword, Is.EqualTo("mypass"));
			Assert.That(deploymentOptions.WebServer, Is.EqualTo("myserver.sand.pit.wtg"));
		}

		[Test]
		public void CreateAndDropEmptyDb()
		{
			var taskInfo = CreateTaskInfo(@"
TestRigWebSiteName:WI0010001");
			var options = CreateShelfDeploymentOptions(taskInfo);
			var testRig = new TestRig(options);
			testRig.CreateDbAndRestoreFromBackupIfNeeded();

			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(options.SafeDbName)))
			{
				Assert.DoesNotThrow(() => conn.Open());
			}
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(options.StagingDbName)))
			{
				Assert.DoesNotThrow(() => conn.Open());
			}

			testRig.DropDatabases();

			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(options.SafeDbName)))
			{
				Assert.Throws<SqlException>(() => conn.Open());
			}
			using (var conn = new SqlConnection(TestConnectionString.GetAdmin(options.StagingDbName)))
			{
				Assert.Throws<SqlException>(() => conn.Open());
			}
		}
	}
}
