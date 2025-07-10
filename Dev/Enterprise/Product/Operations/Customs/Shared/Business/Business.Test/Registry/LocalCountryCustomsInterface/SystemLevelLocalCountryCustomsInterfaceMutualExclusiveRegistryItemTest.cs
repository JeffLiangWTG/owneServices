using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestsSubclassesOf(typeof(SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryItem))]
	public abstract class SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		public void TestNewStringRegistryItem()
		{
			var registryItem = GetRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System);
			AssertEquals("Name", registryItem.Name);
			AssertEquals("Category", registryItem.Category);
			AssertEquals("Caption", registryItem.Caption);
			AssertEquals("Hint", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
		}

		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return GetRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}

		protected abstract SystemLevelLocalCountryCustomsInterfaceMutualExclusiveRegistryItem GetRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage);
	}
}
