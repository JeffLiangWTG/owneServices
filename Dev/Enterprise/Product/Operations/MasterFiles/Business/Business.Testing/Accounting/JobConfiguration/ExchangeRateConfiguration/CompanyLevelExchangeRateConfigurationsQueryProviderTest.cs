using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class CompanyLevelExchangeRateConfigurationsQueryProviderTest : TestCase
	{
		public void TestGetQuery()
		{
			var companyPK = ZGuid.NewZGuid();
			var companyLevelProvider = new CompanyLevelExchangeRateConfigurationsQueryProvider(companyPK, new ZQuery());
			var query = companyLevelProvider.GetQuery();
			var expectSql = $@"JCE_ParentTableCode = ''
							AND
								JCE_ParentID is NULL
							AND
							(
								JCE_GC = '{companyPK}'
							)";
			AssertEqualsIgnoreSpace(expectSql, query.LiteralTextSqlFormatted);

			var additionalQuery = new ZQuery(AccExchangeRateConfigurationViewSchema.JCE_GC, null);
			companyLevelProvider = new CompanyLevelExchangeRateConfigurationsQueryProvider(companyPK, additionalQuery);
			query = companyLevelProvider.GetQuery();
			expectSql = $@"JCE_ParentTableCode = ''
							AND
								JCE_ParentID is NULL
							AND
							(
								JCE_GC = '{companyPK}'
								OR
								JCE_GC is NULL
							)";
			AssertEqualsIgnoreSpace(expectSql, query.LiteralTextSqlFormatted);
		}

		void AssertEqualsIgnoreSpace(string expect, string actual)
		{
			var expectWithoutSpace = Regex.Replace(expect, @"\s", "", RegexOptions.Singleline);
			var actualWithoutSpace = Regex.Replace(actual, @"\s", "", RegexOptions.Singleline);
			AssertEquals(expectWithoutSpace, actualWithoutSpace);
		}
	}
}
