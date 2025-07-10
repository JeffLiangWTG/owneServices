using System;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Winzor.Architecture.Test;

class UpgradeManagerTest
{
	[Test]
	public void UpgradeManagerCanInitialiseUpgraderMutex()
	{
		using (var connection = Db.NewAdminConnection())
		using (var installationDirectory = new TempDirectory())
		{
			var upgradeManager = UpgradeManagerFactory.NewUpgradeManager(connection);
			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), new Version("1.2.3.4"));

			// If we throw an InvalidPackageException, then we were able to successfully initialise the UpgraderMutex
			Assert.Throws<InvalidPackageException>(() => upgradeManager.InstallUpgradePackage(upgradeInfo, installationDirectory, null));
		}
	}
}
