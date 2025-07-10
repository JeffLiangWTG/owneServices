using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class OrganizationLevelExchangeRateConfigurationsQueryProviderTest : TestCase
	{
		public void TestGetQuery()
		{
			var orgHeaderPK = ZGuid.NewZGuid();
			var companyLevelProvider = new OrganizationLevelExchangeRateConfigurationsQueryProvider(orgHeaderPK, new ZQuery());
			var query = companyLevelProvider.GetQuery();
			var expectSql = $@"JCE_ParentTableCode = 'OH'
							AND
								JCE_ParentID = '{orgHeaderPK}'";
			AssertEqualsIgnoreSpace(expectSql, query.LiteralTextSqlFormatted);

			var subQuery = new ZQuery(AccExchangeRateConfigurationViewSchema.JCE_Ledger, "AR");
			var companyQuery = new ZQuery(AccExchangeRateConfigurationViewSchema.JCE_GC, null);
			var companyPK = ZGuid.NewZGuid();
			companyQuery.AddToFilter(JoinCondition.Or, AccExchangeRateConfigurationViewSchema.JCE_GC, companyPK);
			subQuery.AddToFilter(companyQuery);
			companyLevelProvider = new OrganizationLevelExchangeRateConfigurationsQueryProvider(orgHeaderPK, subQuery);
			query = companyLevelProvider.GetQuery();
			expectSql = $@"JCE_ParentTableCode = 'OH' 
						AND
							JCE_ParentID = '{orgHeaderPK}' 
						AND
						(
							JCE_Ledger = 'AR' 
							AND
							(
								JCE_GC is NULL 
								OR
								JCE_GC = '{companyPK}'
							)
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
