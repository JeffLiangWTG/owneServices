using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class DBStatusQueryAndFilterConstraintsTest : TestCaseWithFactory
	{
		public void TestStatusQueryAgainstEntry()
		{
			var declaration0 = Factory.New<JobDeclaration>();
			var cusEntryHeader0 = declaration0.CustomsEntryHeaders.AddNew();
			cusEntryHeader0.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			var declaration1 = Factory.New<JobDeclaration>();
			var cusEntryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			cusEntryHeader1.CH_Status = ImportMessageStatusList.Codes.EntrySummaryCanceled;
			Factory.Save();
			var query0 = new ZDBOnlyQuery(typeof(JobDeclaration));
			var subQuery0 = DBStatusQueryAndFilterConstraints.GetStatusQueryAgainstEntry(SQLComparisonOperator.NotEqual, "CED", new[] { "ENS" });
			query0.AddSubQuery(subQuery0, JoinCondition.And);
			var declarationCollection = Factory.Load<JobDeclaration>(query0);
			AssertEquals(1, declarationCollection.Length);
			var declarationBO = declarationCollection[0];
			AssertEquals(declaration1.PK, declarationBO.PK);
			var query1 = new ZDBOnlyQuery(typeof(JobDeclaration));
			var subQuery1 = DBStatusQueryAndFilterConstraints.GetStatusQueryAgainstEntry(SQLComparisonOperator.NotEqual, "ESC", new[] { "ENS" });
			query1.AddSubQuery(subQuery1, JoinCondition.And);
			declarationCollection = Factory.Load<JobDeclaration>(query1);
			AssertEquals(1, declarationCollection.Length);
			declarationBO = declarationCollection[0];
			AssertEquals(declaration0.PK, declarationBO.PK);
		}

		public void TestFilterConstraints()
		{
			var textFilter = new ModuleTextFilter("Test", DummyBizoSchema.GenericStringSchemaColumn);
			DBStatusQueryAndFilterConstraints.SetFilterConstraints(textFilter, FilterCategories.StatusAndFlags);
			AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, textFilter.ComparisonOperator);
			AssertEquals(3, textFilter.ComparisonOperator_List.Count);
			AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, textFilter.ComparisonOperator_List[0].Code);
			AssertEquals(ModuleTextFilter.ComparisonConstants.StartsWith, textFilter.ComparisonOperator_List[1].Code);
			AssertEquals(ModuleTextFilter.ComparisonConstants.NotEqual, textFilter.ComparisonOperator_List[2].Code);
		}
	}
}
