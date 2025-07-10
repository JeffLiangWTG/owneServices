using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgPartProductCategoryHelperTest : TestCaseWithFactory
	{
		#region TestProductCategoryAndSubCategories

		public void TestProductCategoryAndSubCategories()
		{
			var categoryBeverage = (OrgPartCategory)Helper.CreateProductCategory("BEV", "Beverages", ZGuid.Empty);
			var categorySoftdrink = (OrgPartCategory)Helper.CreateProductCategory("SOFTDRK", "Soft Drinks", categoryBeverage.PK);
			var categoryBeer = (OrgPartCategory)Helper.CreateProductCategory("BEER", "All Beers", categoryBeverage.PK);
			var categoryDarkBeer = (OrgPartCategory)Helper.CreateProductCategory("DARKBEER", "Dark Beers", categoryBeer.PK);

			Factory.Save();

			var list = ProductCategoryHelper.ProductCategoryAndSubCategories(categoryBeverage.PK);
			AssertEquals("Find all Beverages.", 4, list.Count);

			list = ProductCategoryHelper.ProductCategoryAndSubCategories(categorySoftdrink.PK);
			AssertEquals("Find all Soft-Drinks.", 1, list.Count);

			list = ProductCategoryHelper.ProductCategoryAndSubCategories(categoryBeer.PK);
			AssertEquals("Find all Beers.", 2, list.Count);

			list = ProductCategoryHelper.ProductCategoryAndSubCategories(ZGuid.Empty);
			AssertEquals("Nothing to find.", 0, list.Count);
		}

		#endregion

		#region TestIsCategroyASubCategoryOfCurrentCategory

		public void TestIsCategroyASubCategoryOfCurrentCategory()
		{
			var categoryBeverage = (OrgPartCategory)Helper.CreateProductCategory("BEV", "Beverages", ZGuid.Empty);
			var categorySoftdrink = (OrgPartCategory)Helper.CreateProductCategory("SOFTDRK", "Soft Drinks", categoryBeverage.PK);
			var categoryBeer = (OrgPartCategory)Helper.CreateProductCategory("BEER", "All Beers", categoryBeverage.PK);
			var categoryDarkBeer = (OrgPartCategory)Helper.CreateProductCategory("DARKBEER", "Dark Beers", categoryBeer.PK);

			Factory.Save();

			AssertEquals(true, ProductCategoryHelper.IsCategroyASubCategoryOfCurrentCategory(categoryBeverage, categoryBeverage));
			AssertEquals(true, ProductCategoryHelper.IsCategroyASubCategoryOfCurrentCategory(categorySoftdrink, categoryBeverage));
			AssertEquals(true, ProductCategoryHelper.IsCategroyASubCategoryOfCurrentCategory(categoryDarkBeer, categoryBeverage));

			var newCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			AssertEquals(false, ProductCategoryHelper.IsCategroyASubCategoryOfCurrentCategory(newCategory, categoryBeverage));
		}

		#endregion

		#region Implementation

		OrgPartProductCategoryHelper ProductCategoryHelper
		{
			get { return categoryHelper ?? (categoryHelper = new OrgPartProductCategoryHelper(Factory)); }
		}
		OrgPartProductCategoryHelper categoryHelper;

		IWhsTransactionTestHelper Helper
		{
			get { return helper ?? (helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory)); }
		}
		IWhsTransactionTestHelper helper;

		#endregion
	}
}
