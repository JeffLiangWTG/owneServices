using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[TestedType(typeof(RemoteDbStringRegistryItem))]
	class RemoteDbStringRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new RemoteDbStringRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, "");
		}

		public void TestDataType()
		{
			var testRegistryItem = (RemoteDbStringRegistryItem)GetNewRegistryItem();
			Assert("DataType must be correct.", testRegistryItem.DataType is RemoteDbStringRegistryDataType);
		}
	}
}
