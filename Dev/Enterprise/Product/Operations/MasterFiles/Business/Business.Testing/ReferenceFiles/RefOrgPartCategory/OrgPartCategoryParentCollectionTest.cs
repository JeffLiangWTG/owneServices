using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartCategoryParentCollection))]
	sealed class OrgPartCategoryParentCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgPartCategoryParentCollection>
	{
		#region TestAddNotificationWhenAdditionalFilterNotMet

		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			var rootCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			var level1Category = Factory.NewWithValidTestData<OrgPartCategory>();
			level1Category.OPC_OPC_Parent = rootCategory.PK;
			var level2Category = Factory.NewWithValidTestData<OrgPartCategory>();
			level2Category.OPC_OPC_Parent = level1Category.PK;

			var collection = new DummyOrgPartCategoryParentCollection(Factory, rootCategory);
			StringCollectionX errors = new StringCollectionX();
			collection.AddNotificationWhenAdditionalFilterNotMet(errors, rootCategory);
			AssertContainsExactElementsInAnyOrder(new string[] { "This category is a sub category of current category, cannot be selected." }, errors.ToArray());

			errors = new StringCollectionX();
			collection.AddNotificationWhenAdditionalFilterNotMet(errors, level1Category);
			AssertContainsExactElementsInAnyOrder(new string[] { "This category is a sub category of current category, cannot be selected." }, errors.ToArray());

			errors = new StringCollectionX();
			collection.AddNotificationWhenAdditionalFilterNotMet(errors, level2Category);
			AssertContainsExactElementsInAnyOrder(new string[] { "This category is a sub category of current category, cannot be selected." }, errors.ToArray());

			var anotherCategory = Factory.NewWithValidTestData<OrgPartCategory>();
			errors = new StringCollectionX();
			collection.AddNotificationWhenAdditionalFilterNotMet(errors, anotherCategory);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), errors.ToArray());
		}

		#endregion

		#region Implementation

		class DummyOrgPartCategoryParentCollection : OrgPartCategoryParentCollection
		{
			public DummyOrgPartCategoryParentCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public DummyOrgPartCategoryParentCollection(BusinessObjectFactory factory, OrgPartCategory category)
				: base(factory, category)
			{
			}

			public new void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
			{
				base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			}
		}

		#endregion
	}
}
