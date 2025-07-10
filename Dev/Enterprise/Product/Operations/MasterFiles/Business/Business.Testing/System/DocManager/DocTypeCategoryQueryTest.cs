using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DocTypeCategoryQueryTest : TestCaseWithFactory
	{
		public void TestQuery()
		{
			var query = new DocTypeCategoryQuery(Factory, "QUO");
			AssertEquals("Should apply the correct filter for ALL doctypes, and those within QUO", "\r\n\tWHERE RT_ReferenceType = 'ALL' or RT_ReferenceType = 'QUO'", query.GetAsWhereAndOrderByClause(true));

			query = new DocTypeCategoryQuery(Factory, "SHP");
			AssertEquals("Should apply the correct filter for a reference type that is already master type", "\r\n\tWHERE RT_ReferenceType = 'ALL' or RT_ReferenceType = 'SHP'", query.GetAsWhereAndOrderByClause(true));
		}
	}
}
