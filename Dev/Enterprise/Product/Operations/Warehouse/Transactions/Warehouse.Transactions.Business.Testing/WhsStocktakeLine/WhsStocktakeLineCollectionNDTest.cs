using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsStocktakeLineCollectionND))]
	internal class WhsStocktakeLineCollectionNDTest : WhsBusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsStocktakeLineCollectionND(Factory, new ZQuery());
		}
	}
}
