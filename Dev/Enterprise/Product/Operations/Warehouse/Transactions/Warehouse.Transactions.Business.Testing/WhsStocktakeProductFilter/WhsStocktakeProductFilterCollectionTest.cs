using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsStocktakeProductFilterCollection))]
	class WhsStocktakeProductFilterCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsStocktakeProductFilterCollection>
	{
		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			return Helper.CreateWhsStocktakeProductFilter(stocktake, data.Part1);
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		protected override WhsStocktakeProductFilterCollection GetCollectionToTest()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			return new WhsStocktakeProductFilterCollection(Factory, stocktake);
		}

		#endregion
	}
}
