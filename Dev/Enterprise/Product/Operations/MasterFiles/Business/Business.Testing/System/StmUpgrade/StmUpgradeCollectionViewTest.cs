using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmUpgradeCollectionView))]
	sealed class StmUpgradeCollectionViewTest : BusinessObjectCollectionViewTestCase<StmUpgradeCollectionView>
	{
		public void TestIsThisPartOfTheCollection()
		{
			TestCaseHelper.ClearTable(StmUpgradeSchema.Constants.TableName);

			StmUpgrade upgrade1 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade2 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade3 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade4 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade5 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade6 = Factory.New<StmUpgrade>();
			StmUpgrade upgrade7 = Factory.New<StmUpgrade>();

			upgrade1.SZ_Status = "";
			upgrade2.SZ_Status = StmUpgrade.StmUpgradeStatus.Ready;
			upgrade3.SZ_Status = StmUpgrade.StmUpgradeStatus.Applied;
			upgrade4.SZ_Status = StmUpgrade.StmUpgradeStatus.NotApplied;
			upgrade5.SZ_Status = StmUpgrade.StmUpgradeStatus.Deleted;
			upgrade6.SZ_Status = StmUpgrade.StmUpgradeStatus.Obsolete;
			upgrade7.SZ_Status = StmUpgrade.StmUpgradeStatus.CurrentVersion;

			upgrade7.VersionNumber = ReleaseInfo.Instance.VersionNumber;

			StmUpgradeCollection collection = new StmUpgradeCollection(Factory);
			StmUpgradeCollectionView collectionView = new StmUpgradeCollectionView(collection);
			collection.Load();
			AssertEquals("CollectionView.Count", 1, collectionView.Count);
			AssertEquals("Contains(Upgrade7)", true, collectionView.Contains(upgrade7));
			CheckDoesNotContain(collectionView, upgrade1, upgrade2, upgrade3, upgrade4, upgrade5, upgrade6);

			collectionView.ShowNoStatus = true;
			collectionView.Rebuild();
			AssertEquals("Contains(Upgrade1)", true, collectionView.Contains(upgrade1));
			AssertEquals("Contains(Upgrade7)", true, collectionView.Contains(upgrade7));
			CheckDoesNotContain(collectionView, upgrade2, upgrade3, upgrade4, upgrade5, upgrade6);

			collectionView.ShowNoStatus = false;
			collectionView.ShowReady = true;
			collectionView.Rebuild();
			AssertEquals("Contains(Upgrade2)", true, collectionView.Contains(upgrade2));
			AssertEquals("Contains(Upgrade7)", true, collectionView.Contains(upgrade7));
			CheckDoesNotContain(collectionView, upgrade1, upgrade3, upgrade4, upgrade5, upgrade6);

			collectionView.ShowReady = false;
			collectionView.ShowApplied = true;
			collectionView.Rebuild();
			AssertEquals("Contains(Upgrade3)", true, collectionView.Contains(upgrade3));
			AssertEquals("Contains(Upgrade7)", true, collectionView.Contains(upgrade7));
			CheckDoesNotContain(collectionView, upgrade1, upgrade2, upgrade4, upgrade5, upgrade6);

			collectionView.ShowApplied = false;
			collectionView.ShowNotApplied = true;
			collectionView.Rebuild();
			AssertEquals("Contains(Upgrade4)", true, collectionView.Contains(upgrade4));
			AssertEquals("Contains(Upgrade7)", true, collectionView.Contains(upgrade7));
			CheckDoesNotContain(collectionView, upgrade1, upgrade2, upgrade3, upgrade5, upgrade6);

			collectionView.ShowNotApplied = false;
			collectionView.ShowDeleted = true;
			collectionView.Rebuild();
			AssertEquals("Contains(Upgrade5)", true, collectionView.Contains(upgrade5));
			AssertEquals("Contains(Upgrade7)", true, collectionView.Contains(upgrade7));
			CheckDoesNotContain(collectionView, upgrade1, upgrade2, upgrade3, upgrade4, upgrade6);

			collectionView.ShowDeleted = false;
			collectionView.ShowObsolete = true;
			collectionView.Rebuild();
			AssertEquals("Contains(Upgrade6)", true, collectionView.Contains(upgrade6));
			AssertEquals("Contains(Upgrade7)", true, collectionView.Contains(upgrade7));
			CheckDoesNotContain(collectionView, upgrade1, upgrade2, upgrade3, upgrade4, upgrade5);

			collectionView.ShowNoStatus = true;
			collectionView.ShowReady = true;
			collectionView.ShowApplied = true;
			collectionView.ShowNotApplied = true;
			collectionView.ShowDeleted = true;
			collectionView.ShowObsolete = true;
			collectionView.Rebuild();
			AssertEquals("Contains(Upgrade1)", true, collectionView.Contains(upgrade1));
			AssertEquals("Contains(Upgrade2)", true, collectionView.Contains(upgrade2));
			AssertEquals("Contains(Upgrade3)", true, collectionView.Contains(upgrade3));
			AssertEquals("Contains(Upgrade4)", true, collectionView.Contains(upgrade4));
			AssertEquals("Contains(Upgrade5)", true, collectionView.Contains(upgrade5));
			AssertEquals("Contains(Upgrade6)", true, collectionView.Contains(upgrade6));
			AssertEquals("Contains(Upgrade7)", true, collectionView.Contains(upgrade7));
		}

		#region Implementation

		protected override StmUpgradeCollectionView GetCollectionToTest()
		{
			var collectionView = new StmUpgradeCollectionView(new StmUpgradeCollection(Factory));
			collectionView.ShowNoStatus = true;
			return collectionView;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(StmUpgrade));
		}

		void CheckDoesNotContain(StmUpgradeCollectionView collection, params StmUpgrade[] upgrades)
		{
			foreach (StmUpgrade upgrade in upgrades)
			{
				Assert("Collection should not contain Upgrade with status of " + upgrade.SZ_Status, !collection.Contains(upgrade));
			}
		}

		#endregion
	}
}
