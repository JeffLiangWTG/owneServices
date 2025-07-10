using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ProcessTasksModuleFilter))]
	sealed class ProcessTasksModuleFilterTest : ModuleFilterTestCase<ProcessTasksModuleFilter>
	{
		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		public override void TestQueryIsEmptyByDefault()
		{
			var shouldBeEmpty = !(Filter is ModuleGuidForeignCollectionFilter);
			AssertEquals(shouldBeEmpty, Filter.Query.IsEmpty);
			AssertContains("P9_ParentTableCode = 'OH'", Filter.Query.LiteralTextSqlFormatted);
		}

		protected override ProcessTasksModuleFilter GetNewModuleFilter()
		{
			return new ProcessTasksModuleFilterForTest("moo", ModuleIDs.ProcessTasks, OrgHeaderSchema.PK, ProcessTasksSchema.P9_ParentID, new ProcessTaskCollection(Factory), typeof(OrgHeader));
		}
	}
}
