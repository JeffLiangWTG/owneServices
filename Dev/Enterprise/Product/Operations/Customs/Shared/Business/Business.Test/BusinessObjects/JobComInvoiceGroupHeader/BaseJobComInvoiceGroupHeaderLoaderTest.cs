using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobComInvoiceGroupHeader.Loader))]
	sealed class BaseJobComInvoiceGroupHeaderLoaderTest : LoaderTestCase
	{
		public void TestGetQueryWithDeclaration()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var declaration2 = Factory.New<BaseJobDeclaration>();
			var invoice = Factory.New<BaseJobComInvoiceHeader>();
			invoice.JZ_JE = declaration1.PK;
			var groupHeader1 = Factory.New<BaseJobComInvoiceGroupHeader>();
			groupHeader1.JZ_JE = declaration1.PK;
			var groupHeader2 = Factory.New<BaseJobComInvoiceGroupHeader>();
			groupHeader2.JZ_JE = declaration2.PK;

			ZQuery query = new BaseJobComInvoiceGroupHeader.Loader(Factory).GetQuery(declaration1);
			AssertEquals("invoice does not match the query", false, invoice.MatchesFilter(query));
			AssertEquals("groupHeader1 does match the query", true, groupHeader1.MatchesFilter(query));
			AssertEquals("groupHeader2 does not match the query", false, groupHeader2.MatchesFilter(query));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new BaseJobComInvoiceGroupHeader.Loader(Factory);
		}
	}
}
