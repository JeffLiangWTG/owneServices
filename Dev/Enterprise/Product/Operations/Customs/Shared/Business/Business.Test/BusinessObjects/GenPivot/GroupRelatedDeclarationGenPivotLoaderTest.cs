using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(GroupRelatedDeclarationGenPivot.Loader))]
	sealed class GroupRelatedDeclarationGenPivotLoaderTest : LoaderTestCase
	{
		public void TestGetQueryWithDeclaration()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var group = Factory.New<BaseJobComInvoiceGroupHeader>();
			var pivot1 = Factory.New<GroupRelatedDeclarationGenPivot>();
			pivot1.XX_Relation1ID = group.PK;
			pivot1.XX_Relation2ID = declaration1.PK;
			var pivot2 = Factory.New<GroupRelatedDeclarationGenPivot>();
			pivot2.XX_Relation1ID = group.PK;
			pivot2.XX_Relation2ID = declaration2.PK;

			ZQuery query = new GroupRelatedDeclarationGenPivot.Loader(Factory).GetQuery(declaration1);
			AssertEquals("pivot1 does match the query", true, pivot1.MatchesFilter(query));
			AssertEquals("pivot2 does not match the query", false, pivot2.MatchesFilter(query));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new GroupRelatedDeclarationGenPivot.Loader(Factory);
		}
	}
}
