using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefOrgPartCategoryFilterBusinessObject))]
	sealed class RefOrgPartCategoryFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region TestChildCategoriesFilter

		public void TestChildCategoriesFilter()
		{
			var parentCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			var childCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			childCategory.OPC_OPC_Parent = parentCategory.PK;

			var otherCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			Factory.Save();

			var filterObject = GetNewFilterStripBusinessObject();
			var parentFilter = (ModuleGuidFilter)filterObject["Child Categories"];
			parentFilter.IsActive = true;
			parentFilter.Property = parentCategory.PK;

			var categories = Factory.Load(typeof(OrgPartCategory), filterObject.Filter);
			AssertCollectionContains(childCategory, categories);
			AssertCollectionNotContains(otherCategory, categories);
		}

		public void TestCategoryParentFilter_With2LevelsChildrenCategory()
		{
			var rootCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			var level1ChildCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			level1ChildCategory.OPC_OPC_Parent = rootCategory.PK;
			var level2ChildCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			level2ChildCategory.OPC_OPC_Parent = level1ChildCategory.PK;

			Factory.Save();

			var filterObject = GetNewFilterStripBusinessObject();
			var parentFilter = (ModuleGuidFilter)filterObject["Child Categories"];
			parentFilter.IsActive = true;
			parentFilter.Property = rootCategory.PK;

			var categories = Factory.Load(typeof(OrgPartCategory), filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new OrgPartCategory[] { level1ChildCategory, level2ChildCategory }, categories);
		}

		#endregion

		#region TestCategoryLevelFilter

		public void TestCategoryLevelFilter()
		{
			var rootCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			var level1ChildCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			level1ChildCategory.OPC_OPC_Parent = rootCategory.PK;
			var level2ChildCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			level2ChildCategory.OPC_OPC_Parent = level2ChildCategory.PK;

			var otherCategory = Factory.NewWithValidTestData<OrgPartCategory>();

			Factory.Save();

			var filterObject = GetNewFilterStripBusinessObject();
			var categoryLevelFilter = (ModuleTextFilter)filterObject["Category Level"];
			categoryLevelFilter.IsActive = true;
			categoryLevelFilter.Property = "All Categories";

			var categories1 = Factory.Load(typeof(OrgPartCategory), filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new OrgPartCategory[] { rootCategory, level1ChildCategory, level2ChildCategory, otherCategory }, categories1);

			categoryLevelFilter.Property = "Top-Level Categories";
			var categories2 = Factory.Load(typeof(OrgPartCategory), filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new OrgPartCategory[] { rootCategory, otherCategory }, categories2);
			AssertCollectionNotContains(new OrgPartCategory[] { level1ChildCategory, level2ChildCategory }, categories2);

			categoryLevelFilter.Property = "Child Categories";
			var categories3 = Factory.Load(typeof(OrgPartCategory), filterObject.Filter);
			AssertContainsExactElementsInAnyOrder(new OrgPartCategory[] { level1ChildCategory, level2ChildCategory }, categories3);
			AssertCollectionNotContains(new OrgPartCategory[] { rootCategory, otherCategory }, categories3);
		}

		#endregion

		#region TestTextFilters

		public void TestCategoryCodeFilter()
		{
			TestTextFilter("Code");
		}

		public void TestCategoryDescriptionFilter()
		{
			TestTextFilter("Description");
		}

		void TestTextFilter(string filterDescription)
		{
			var category1 = Factory.NewWithValidTestData<OrgPartCategory>();
			var category2 = Factory.NewWithValidTestData<OrgPartCategory>();
			switch (filterDescription)
			{
				case "Code":
					category1.OPC_CategoryCode = "ABC";
					category2.OPC_CategoryCode = "DEF";
					break;
				case "Description":
					category1.OPC_CategoryDescription = "ABC";
					category2.OPC_CategoryDescription = "DEF";
					break;
			}
			Factory.Save();

			var filterObject = GetNewFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterObject[filterDescription];
			textFilter.IsActive = true;
			textFilter.Property = "ABC";

			var categories = Factory.Load(typeof(OrgPartCategory), filterObject.Filter);
			AssertCollectionContains(category1, categories);
			AssertCollectionNotContains(category2, categories);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefOrgPartCategoryFilterBusinessObject();
		}

		#endregion
	}
}
