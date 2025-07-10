using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(StmUpgradeController))]
	sealed class StmUpgradeControllerTest : ZSingletonControllerBasherTest
	{
		public void TestGetNewBusinessEntityInLocalFactoryOnlyDisplaysEDPs()
		{
			TestCaseHelper.ClearTable("StmUpgrade");

			StmUpgradeControllerForTest controller = new StmUpgradeControllerForTest();
			StmUpgrade upgrade1 = controller.Factory.New<StmUpgrade>();
			StmUpgrade upgrade2 = controller.Factory.New<StmUpgrade>();

			upgrade1.SZ_Type = "EDP";
			upgrade2.SZ_Type = "xxx";

			StmUpgradeCollectionContainer container = (StmUpgradeCollectionContainer)controller.GetNewBusinessEntityInLocalFactoryForTest();
			StmUpgradeCollection collection = container.FullUpgrades;
			collection.Load();

			AssertEquals(1, collection.Count);
			Assert(collection.Contains(upgrade1));
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.StmUpgrade;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return Factory.New(typeof(StmUpgrade));
		}

		sealed class StmUpgradeControllerForTest : StmUpgradeController
		{
			public IBusiness GetNewBusinessEntityInLocalFactoryForTest()
			{
				return GetNewBusinessEntityInLocalFactory();
			}
		}
	}
}
