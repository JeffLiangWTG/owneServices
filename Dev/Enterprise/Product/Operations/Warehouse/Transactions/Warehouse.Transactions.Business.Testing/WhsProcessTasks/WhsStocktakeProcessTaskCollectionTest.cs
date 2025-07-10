using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsStocktakeProcessTaskCollection))]
	class WhsStocktakeProcessTaskCollectionTest : ProcessTaskCollectionTest<WhsStocktakeProcessTaskCollection>
	{
		#region Implementation

		protected override WhsStocktakeProcessTaskCollection GetCollectionToTestCore()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);

			return new WhsStocktakeProcessTaskCollection(stocktake);
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
