using System.Collections.Generic;
using CargoWise.RefDbRepo.Deployment.TestRigConfiguration;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Deployment.Test
{
	[TestFixture]
	internal class RestoreManagerFixture
	{
		[Test]
		public void GenerateScript()
		{
			var backupFileInfos = new List<BackupFileInfo>
			{
				new BackupFileInfo { LogicalName = "RefDbRepoSafe", PhysicalName = "SomeCrazyPathing" },
				new BackupFileInfo { LogicalName = "RefDbRepoSafe_log", PhysicalName = "SomeCrazyPathingForLog" }
			};
			var databaseInfo = new DatabaseInfo("WI101010RefDbRepoSafe", @"SomeNetworkPathing\SomeOtherFolder\RefDbRepoSafe.bak");

			var restoreScript = RestoreManager.GetRestoreScript(backupFileInfos, databaseInfo);
			Assert.That(restoreScript, Does.Contain(@"RESTORE DATABASE WI101010RefDbRepoSafe FROM DISK = N'SomeNetworkPathing\SomeOtherFolder\RefDbRepoSafe.bak'"));
			Assert.That(restoreScript, Does.Contain(@"MOVE N'RefDbRepoSafe' TO N'SomeCrazyPathing'"));
			Assert.That(restoreScript, Does.Contain(@"MOVE N'RefDbRepoSafe_log' TO N'SomeCrazyPathingForLog'"));
		}
	}
}
