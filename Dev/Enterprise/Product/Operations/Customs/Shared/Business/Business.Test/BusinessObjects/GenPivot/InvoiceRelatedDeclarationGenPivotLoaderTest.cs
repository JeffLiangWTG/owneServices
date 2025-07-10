using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(InvoiceRelatedDeclarationGenPivot.Loader))]
	sealed class InvoiceRelatedDeclarationGenPivotLoaderTest : LoaderTestCase
	{
		public void TestGetQueryWithDeclaration()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			var pivot1 = Factory.New<InvoiceRelatedDeclarationGenPivot>();
			pivot1.XX_Relation1ID = invoice.PK;
			pivot1.XX_Relation2ID = declaration1.PK;
			var pivot2 = Factory.New<InvoiceRelatedDeclarationGenPivot>();
			pivot2.XX_Relation1ID = invoice.PK;
			pivot2.XX_Relation2ID = declaration2.PK;

			ZQuery query = new InvoiceRelatedDeclarationGenPivot.Loader(Factory).GetQuery(declaration1);
			AssertEquals("pivot1 does match the query", true, pivot1.MatchesFilter(query));
			AssertEquals("pivot2 does not match the query", false, pivot2.MatchesFilter(query));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new InvoiceRelatedDeclarationGenPivot.Loader(Factory);
		}
	}
}
