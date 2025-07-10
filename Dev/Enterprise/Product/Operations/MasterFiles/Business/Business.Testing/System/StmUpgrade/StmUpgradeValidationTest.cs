using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StmUpgradeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSZ_Status()
		{
			StmUpgrade upgrade = Factory.New<StmUpgrade>();

			upgrade.SZ_Status = "XYZ";
			upgrade.Validation.ValidateSZ_Status();
			Assert("Upgrade should have errors if SZ_Status is invalid", upgrade.HasErrors);

			upgrade.SZ_Status = "";
			upgrade.Validation.ValidateSZ_Status();
			Assert("Upgrade should not have errors if SZ_Status is empty", !upgrade.HasErrors);

			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade.Validation.ValidateSZ_Status();
			Assert("Upgrade should not have errors if SZ_Status is valid", !upgrade.HasErrors);

			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.Applied;
			upgrade.Validation.ValidateSZ_Status();
			Assert("Upgrade should not have errors if SZ_Status is valid", !upgrade.HasErrors);

			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.NotApplied;
			upgrade.Validation.ValidateSZ_Status();
			Assert("Upgrade should not have errors if SZ_Status is valid", !upgrade.HasErrors);

			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.Deleted;
			upgrade.Validation.ValidateSZ_Status();
			Assert("Upgrade should not have errors if SZ_Status is valid", !upgrade.HasErrors);

			upgrade.SZ_Status = StmUpgrade.StmUpgradeStatus.Obsolete;
			upgrade.Validation.ValidateSZ_Status();
			Assert("Upgrade should not have errors if SZ_Status is valid", !upgrade.HasErrors);
		}
	}
}
