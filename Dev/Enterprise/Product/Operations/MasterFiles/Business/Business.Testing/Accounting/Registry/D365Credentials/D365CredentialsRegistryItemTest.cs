using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(D365CredentialsRegistryItem))]
	sealed class D365CredentialsRegistryItemTest : StronglyTypedRegistryItemTestCase<D365Credentials>
	{
		public void TestOnAllValuesSavedAction()
		{
			AssertNull("OnAllValuesSavedAction", Item.OnAllValuesSavedAction);
		}
		protected override StronglyTypedRegistryItem<D365Credentials, D365Credentials> GetNewRegistryItem()
			=> new D365CredentialsRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);

		protected override D365Credentials ValidValue => new D365Credentials { ClientID = "abc", ClientSecret = "efg", TenantID = "ijk", };
	}
}
