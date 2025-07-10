using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(BranchManagementCodeDescriptionBoolNewRegistryItem))]
	sealed class BranchManagementCodeDescriptionBoolNewRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionBoolCollection, CodeDescriptionBoolCollection>
	{
		public void TestConstructor()
		{
			var item = new BranchManagementCodeDescriptionBoolNewRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint",
				RegistryStorageFlags.System | RegistryStorageFlags.Company, new CodeDescriptionBoolRegistryEditorInfo((NoResString)"Active"), new BranchManagementCodeDescriptionBoolCollection());

			AssertEquals("Name", "Name", item.Name);
			AssertEquals("Category", "Category", item.Category);
			AssertEquals("Caption", "Caption", item.Caption);
			AssertEquals("Hint", "Hint", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);
			AssertEquals("EditorInfo.BoolColumnCaption", "Active", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);

			var defaultValue = (BranchManagementCodeDescriptionBoolCollection)item.DefaultValue;
			AssertEquals("DefaultValue.Count", 0, defaultValue.Count);
			AssertEquals("DefaultValue.AddNew().Bool", true, defaultValue.AddNew().Bool);
		}

		protected override StronglyTypedRegistryItem<CodeDescriptionBoolCollection, CodeDescriptionBoolCollection> GetNewRegistryItem()
		{
			return new BranchManagementCodeDescriptionBoolNewRegistryItem("", null, null, null, RegistryStorageFlags.System, new CodeDescriptionBoolRegistryEditorInfo((NoResString)""), new BranchManagementCodeDescriptionBoolCollection());
		}
	}
}
