using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmUpgradeCollectionContainer))]
	sealed class StmUpgradeCollectionContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new StmUpgradeCollectionContainer(Factory);
		}

		public void TestFullUpgrades()
		{
			StmUpgrade upgrade1 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.New<StmUpgrade>();

			StmUpgradeCollectionContainer container = (StmUpgradeCollectionContainer)GetNewBusinessObject();

			StmUpgradeCollection fullUpgrades = container.FullUpgrades;

			Assert("FullUpgrades should contain Upgrade1", fullUpgrades.Contains(upgrade1));
			Assert("FullUpgrades should contain Upgrade2", fullUpgrades.Contains(upgrade2));
		}

		public void TestUpdateFullUpgradesView()
		{
			StmUpgrade upgrade1 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade3 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade4 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade5 = Factory.New<StmUpgrade>();

			upgrade1.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade2.SZ_Status = StmUpgrade.StmUpgradeStatus.Applied;
			upgrade3.SZ_Status = StmUpgrade.StmUpgradeStatus.NotApplied;
			upgrade4.SZ_Status = StmUpgrade.StmUpgradeStatus.Deleted;
			upgrade5.SZ_Status = StmUpgrade.StmUpgradeStatus.Obsolete;

			StmUpgradeCollectionContainer container = (StmUpgradeCollectionContainer)GetNewBusinessObject();
			Assert("FullUpgradesView should not contain Upgrade1", !container.FullUpgradesView.Contains(upgrade1));
			Assert("FullUpgradesView should not contain Upgrade2", !container.FullUpgradesView.Contains(upgrade2));
			Assert("FullUpgradesView should not contain Upgrade3", !container.FullUpgradesView.Contains(upgrade3));
			Assert("FullUpgradesView should not contain Upgrade4", !container.FullUpgradesView.Contains(upgrade4));
			Assert("FullUpgradesView should not contain Upgrade5", !container.FullUpgradesView.Contains(upgrade5));

			container.UpdateFullUpgradesView(true, false, false, false, false);
			Assert("FullUpgradesView should contain Upgrade1", container.FullUpgradesView.Contains(upgrade1));
			Assert("FullUpgradesView should not contain Upgrade2", !container.FullUpgradesView.Contains(upgrade2));
			Assert("FullUpgradesView should not contain Upgrade3", !container.FullUpgradesView.Contains(upgrade3));
			Assert("FullUpgradesView should not contain Upgrade4", !container.FullUpgradesView.Contains(upgrade4));
			Assert("FullUpgradesView should not contain Upgrade5", !container.FullUpgradesView.Contains(upgrade5));

			container.UpdateFullUpgradesView(true, true, false, false, false);
			Assert("FullUpgradesView should contain Upgrade1", container.FullUpgradesView.Contains(upgrade1));
			Assert("FullUpgradesView should contain Upgrade2", container.FullUpgradesView.Contains(upgrade2));
			Assert("FullUpgradesView should not contain Upgrade3", !container.FullUpgradesView.Contains(upgrade3));
			Assert("FullUpgradesView should not contain Upgrade4", !container.FullUpgradesView.Contains(upgrade4));
			Assert("FullUpgradesView should not contain Upgrade5", !container.FullUpgradesView.Contains(upgrade5));

			container.UpdateFullUpgradesView(true, true, true, false, false);
			Assert("FullUpgradesView should contain Upgrade1", container.FullUpgradesView.Contains(upgrade1));
			Assert("FullUpgradesView should contain Upgrade2", container.FullUpgradesView.Contains(upgrade2));
			Assert("FullUpgradesView should contain Upgrade3", container.FullUpgradesView.Contains(upgrade3));
			Assert("FullUpgradesView should not contain Upgrade4", !container.FullUpgradesView.Contains(upgrade4));
			Assert("FullUpgradesView should not contain Upgrade5", !container.FullUpgradesView.Contains(upgrade5));

			container.UpdateFullUpgradesView(true, true, true, true, false);
			Assert("FullUpgradesView should contain Upgrade1", container.FullUpgradesView.Contains(upgrade1));
			Assert("FullUpgradesView should contain Upgrade2", container.FullUpgradesView.Contains(upgrade2));
			Assert("FullUpgradesView should contain Upgrade3", container.FullUpgradesView.Contains(upgrade3));
			Assert("FullUpgradesView should contain Upgrade4", container.FullUpgradesView.Contains(upgrade4));
			Assert("FullUpgradesView should not contain Upgrade5", !container.FullUpgradesView.Contains(upgrade5));

			container.UpdateFullUpgradesView(true, true, true, true, true);
			Assert("FullUpgradesView should contain Upgrade1", container.FullUpgradesView.Contains(upgrade1));
			Assert("FullUpgradesView should contain Upgrade2", container.FullUpgradesView.Contains(upgrade2));
			Assert("FullUpgradesView should contain Upgrade3", container.FullUpgradesView.Contains(upgrade3));
			Assert("FullUpgradesView should contain Upgrade4", container.FullUpgradesView.Contains(upgrade4));
			Assert("FullUpgradesView should contain Upgrade5", container.FullUpgradesView.Contains(upgrade5));

			container.UpdateFullUpgradesView(false, false, false, false, false);
			Assert("FullUpgradesView should not contain Upgrade1", !container.FullUpgradesView.Contains(upgrade1));
			Assert("FullUpgradesView should not contain Upgrade2", !container.FullUpgradesView.Contains(upgrade2));
			Assert("FullUpgradesView should not contain Upgrade3", !container.FullUpgradesView.Contains(upgrade3));
			Assert("FullUpgradesView should not contain Upgrade4", !container.FullUpgradesView.Contains(upgrade4));
			Assert("FullUpgradesView should not contain Upgrade5", !container.FullUpgradesView.Contains(upgrade5));
		}

		public void TestDeleteUpgrades()
		{
			StmUpgrade upgrade1 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade3 = Factory.New<StmUpgrade>();

			upgrade1.SZ_MajorVersion = 1;
			upgrade2.SZ_MajorVersion = 2;
			upgrade3.SZ_MajorVersion = 3;

			upgrade1.SZ_UpgradeData_Compressed = new byte[] { 10, 10 };
			upgrade2.SZ_UpgradeData_Compressed = new byte[] { 20, 20 };
			upgrade3.SZ_UpgradeData_Compressed = new byte[] { 30, 30 };

			StmUpgradeCollectionContainer container = (StmUpgradeCollectionContainer)GetNewBusinessObject();

			StmUpgrade[] upgrades = new StmUpgrade[] { upgrade1 };

			container.DeleteUpgrades(upgrades);

			AssertEquals("Upgrade1.SZ_Status", StmUpgrade.StmUpgradeStatus.Deleted, upgrade1.SZ_Status);
			AssertEquals("Upgrade2.SZ_Status", "", upgrade2.SZ_Status);
			AssertEquals("Upgrade3.SZ_Status", "", upgrade3.SZ_Status);

			AssertEquals("Upgrade1.SZ_UpgradeData_Compressed", ZBlob.Empty, upgrade1.SZ_UpgradeData_Compressed);
			AssertEquals("Upgrade2.SZ_UpgradeData_Compressed", new byte[] { 20, 20 }, upgrade2.SZ_UpgradeData_Compressed);
			AssertEquals("Upgrade3.SZ_UpgradeData_Compressed", new byte[] { 30, 30 }, upgrade3.SZ_UpgradeData_Compressed);

			Assert("Upgrade1.SZ_StatusTime should be within 1 minute from the current time", ZDateTime.Now.AddMinutes(-1) <= upgrade1.SZ_StatusTime &&
				upgrade1.SZ_StatusTime <= ZDateTime.Now.AddMinutes(1));
			AssertEquals("Upgrade2.SZ_StatusTime", ZDateTime.Empty, upgrade2.SZ_StatusTime);
			AssertEquals("Upgrade3.SZ_StatusTime", ZDateTime.Empty, upgrade3.SZ_StatusTime);

			AssertEquals("Upgrade1.SZ_StatusComment", "Deleted by " + Env.CurrentUser.LoginName, upgrade1.SZ_StatusComment);
			AssertEquals("Upgrade2.SZ_StatusComment", "", upgrade2.SZ_StatusComment);
			AssertEquals("Upgrade3.SZ_StatusComment", "", upgrade3.SZ_StatusComment);

			upgrades = new StmUpgrade[] { upgrade2, upgrade3 };

			container.DeleteUpgrades(upgrades);

			AssertEquals("Upgrade1.SZ_Status", StmUpgrade.StmUpgradeStatus.Deleted, upgrade1.SZ_Status);
			AssertEquals("Upgrade2.SZ_Status", StmUpgrade.StmUpgradeStatus.Deleted, upgrade2.SZ_Status);
			AssertEquals("Upgrade3.SZ_Status", StmUpgrade.StmUpgradeStatus.Deleted, upgrade3.SZ_Status);

			AssertEquals("Upgrade1.SZ_UpgradeData_Compressed", ZBlob.Empty, upgrade1.SZ_UpgradeData_Compressed);
			AssertEquals("Upgrade2.SZ_UpgradeData_Compressed", ZBlob.Empty, upgrade2.SZ_UpgradeData_Compressed);
			AssertEquals("Upgrade3.SZ_UpgradeData_Compressed", ZBlob.Empty, upgrade3.SZ_UpgradeData_Compressed);

			Assert("Upgrade1.SZ_StatusTime should be within 1 minute from the current time", ZDateTime.Now.AddMinutes(-1) <= upgrade1.SZ_StatusTime &&
				upgrade1.SZ_StatusTime <= ZDateTime.Now.AddMinutes(1));
			Assert("Upgrade1.SZ_StatusTime should be within 1 minute from the current time", ZDateTime.Now.AddMinutes(-1) <= upgrade2.SZ_StatusTime &&
				upgrade2.SZ_StatusTime <= ZDateTime.Now.AddMinutes(1));
			Assert("Upgrade1.SZ_StatusTime should be within 1 minute from the current time", ZDateTime.Now.AddMinutes(-1) <= upgrade3.SZ_StatusTime &&
				upgrade3.SZ_StatusTime <= ZDateTime.Now.AddMinutes(1));

			AssertEquals("Upgrade1.SZ_StatusComment", "Deleted by " + Env.CurrentUser.LoginName, upgrade1.SZ_StatusComment);
			AssertEquals("Upgrade2.SZ_StatusComment", "Deleted by " + Env.CurrentUser.LoginName, upgrade2.SZ_StatusComment);
			AssertEquals("Upgrade3.SZ_StatusComment", "Deleted by " + Env.CurrentUser.LoginName, upgrade3.SZ_StatusComment);
		}

		public void TestBusinessObjectsWithRelatedEventsContainsCMRRefFileUpgradeLog()
		{
			StmUpgradeCollectionContainer container = (StmUpgradeCollectionContainer)GetNewBusinessObject();
			BusinessObject[] bOLogs = ((IStmALogParent)container).BusinessObjectsWithRelatedEvents;

			Assert("Always contains something", bOLogs.Length > 0);
			AssertEquals("The last object is CMRReferenceFileUpgradeLog", new ZGuid(CMRReferenceFileUpdateLog.LogReferencePK), bOLogs[bOLogs.Length - 1].PK);
		}
	}
}
