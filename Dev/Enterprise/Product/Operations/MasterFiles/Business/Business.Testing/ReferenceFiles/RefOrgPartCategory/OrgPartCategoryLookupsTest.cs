using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgPartCategoryLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestParents

		public void TestParents()
		{
			var rootCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			var level1Category = Factory.NewWithValidTestData<OrgPartCategory>();
			level1Category.OPC_OPC_Parent = rootCategory.PK;
			var level2Category = Factory.NewWithValidTestData<OrgPartCategory>();
			level2Category.OPC_OPC_Parent = level1Category.PK;

			var otherRootCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			var otherLevel1Category = Factory.NewWithValidTestData<OrgPartCategory>();
			otherLevel1Category.OPC_OPC_Parent = otherRootCategory.PK;

			var newCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			var parentList = newCategory.Lookups.Parents;
			AssertEquals(6, parentList.Count);
		}

		#endregion
	}
}
