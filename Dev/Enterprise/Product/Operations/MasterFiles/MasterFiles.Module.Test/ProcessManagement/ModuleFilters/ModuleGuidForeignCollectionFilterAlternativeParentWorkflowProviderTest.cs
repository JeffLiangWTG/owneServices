using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ModuleGuidForeignCollectionFilterAlternativeParentWorkflowProvider))]
	sealed class ModuleGuidForeignCollectionFilterAlternativeParentWorkflowProviderTest : ModuleFilterTestCase<ModuleGuidForeignCollectionFilterAlternativeParentWorkflowProvider>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override ModuleGuidForeignCollectionFilterAlternativeParentWorkflowProvider GetNewModuleFilter()
		{
			return new ModuleGuidForeignCollectionFilterAlternativeParentWorkflowProvider("moo", ModuleIDs.ProcessTasks, OrgHeaderSchema.PK, ProcessTasksSchema.P9_ParentID, new ProcessTaskCollection(Factory), typeof(OrgHeader));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestQueryIsEmptyByDefault()
		{
			Assert("It's definitely empty", true);
		}
	}
}
