using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(MainFormConsolCollection))]
	sealed class MainFormConsolCollectionTest : MainFormGenericConsolCollectionBOCollectionTest<CommonConsol>
	{
		public override void TestIndexer()
		{
			MainFormConsolCollection consols = new MainFormConsolCollection(Factory);
			CommonConsol consol1 = consols.AddNew();
			AssertEquals(consol1, consols[0]);

			CommonConsol consol2 = consols.AddNew();
			AssertEquals(consol2, consols[1]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new MainFormConsolCollection(Factory);
		}
	}
}
