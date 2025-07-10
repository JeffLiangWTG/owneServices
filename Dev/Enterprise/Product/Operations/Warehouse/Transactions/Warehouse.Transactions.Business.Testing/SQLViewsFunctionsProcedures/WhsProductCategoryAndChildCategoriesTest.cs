#if DEBUG
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.SqlViews.Testing
{
	class WhsProductCategoryAndChildCategoriesTest : WhsTestCaseWithFactory
	{
		#region TestView

		public void TestView()
		{
			var rootCategory = Helper.CreateProductCategory("BEVERAGE", "All Beverages");
			var childCategory1 = Helper.CreateProductCategory("SOFTDRK", "All Soft-Drinks", rootCategory);
			var childCategory2 = Helper.CreateProductCategory("BEERS", "All Beers", rootCategory);
			var grandChildCategory1 = Helper.CreateProductCategory("DarkBeer", "Dark Beers", childCategory2);
			Factory.Save();

			var result1 = Load_ProductCategories(rootCategory.PK);
			AssertEquals(4, result1.Count); // BEVERAGE, SOFTDRK, BEERS, DarkBeer

			var result2 = Load_ProductCategories(childCategory2.PK);
			AssertEquals(2, result2.Count); // BEERS, DarkBeer

			var result3 = Load_ProductCategories(childCategory1.PK);
			AssertEquals(1, result3.Count); // SOFTDRK
		}

		DynamicBusinessObjectCollection Load_ProductCategories(ZGuid categoryPK)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"SELECT * FROM dbo.WhsProductCategoryAndChildCategories(@ProductCategoryPK)";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@ProductCategoryPK", categoryPK, OrgPartCategorySchema.PK);
			result.Load(sql, sqlParams);

			return result;
		}

		#endregion
	}
}

#endif
