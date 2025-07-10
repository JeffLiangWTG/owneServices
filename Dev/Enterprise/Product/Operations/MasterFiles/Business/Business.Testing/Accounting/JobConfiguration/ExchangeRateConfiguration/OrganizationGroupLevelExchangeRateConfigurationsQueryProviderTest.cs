using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class OrganizationGroupLevelExchangeRateConfigurationsQueryProviderTest : TestCase
	{
		public void TestGetQuery()
		{
			var companyPK = ZGuid.NewZGuid();
			var orgGroupPK = ZGuid.NewZGuid();
			var companyLevelProvider = new OrganizationGroupLevelExchangeRateConfigurationsQueryProvider(companyPK, orgGroupPK, LedgerTypes.AccountsPayable, new ZQuery());
			var query = companyLevelProvider.GetQuery();
			var expectSql = $@"JCE_ParentTableCode = 'OG'
							AND
								JCE_ParentID = '{orgGroupPK}'
							AND
							(
								JCE_GC = '{companyPK}'
							)";
			AssertEqualsIgnoreSpace(expectSql, query.LiteralTextSqlFormatted);

			companyLevelProvider = new OrganizationGroupLevelExchangeRateConfigurationsQueryProvider(companyPK, orgGroupPK, LedgerTypes.AccountsReceivable, new ZQuery());
			query = companyLevelProvider.GetQuery();
			expectSql = $@"JCE_ParentTableCode = 'OJ'
							AND
								JCE_ParentID = '{orgGroupPK}'
							AND
							(
								JCE_GC = '{companyPK}'
							)";
			AssertEqualsIgnoreSpace(expectSql, query.LiteralTextSqlFormatted);

			var additionalQuery = new ZQuery(AccExchangeRateConfigurationViewSchema.JCE_GC, null);
			companyLevelProvider = new OrganizationGroupLevelExchangeRateConfigurationsQueryProvider(companyPK, orgGroupPK, LedgerTypes.AccountsPayable, additionalQuery);
			query = companyLevelProvider.GetQuery();
			expectSql = $@"JCE_ParentTableCode = 'OG'
							AND
								JCE_ParentID = '{orgGroupPK}'
							AND
							(
								JCE_GC = '{companyPK}'
								OR
								JCE_GC is NULL
							)";
			AssertEqualsIgnoreSpace(expectSql, query.LiteralTextSqlFormatted);

			companyLevelProvider = new OrganizationGroupLevelExchangeRateConfigurationsQueryProvider(companyPK, orgGroupPK, LedgerTypes.AccountsReceivable, additionalQuery);
			query = companyLevelProvider.GetQuery();
			expectSql = $@"JCE_ParentTableCode = 'OJ'
							AND
								JCE_ParentID = '{orgGroupPK}'
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
