using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmUpgradeCollection))]
	sealed class StmUpgradeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionLoad()
		{
			TestCaseHelper.ClearTable(StmUpgrade.Schema.TableName);

			StmUpgrade upgrade1 = Factory.New<StmUpgrade>();
			upgrade1.UpdateVersionDetails("Package20040719_121209_999_88_77777_6.edp");

			Factory.Save();
			Collection.Load();
			AssertEquals("Collection.Count", 1, Collection.Count);
			AssertEquals("Collection[0]", upgrade1, Collection[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmUpgradeCollection(Factory);
		}

		new StmUpgradeCollection Collection
		{
			get { return (StmUpgradeCollection)base.Collection; }
		}
	}
}
