using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(EInvoicingCredentialsRegistryItem))]
	sealed class EInvoicingCredentialsRegistryItemTest : StronglyTypedRegistryItemTestCase<EInvoicingCredentials>
	{
		public void TestOnAllValuesSavedAction()
		{
			AssertNull("OnAllValuesSavedAction", Item.OnAllValuesSavedAction);
		}

		protected override StronglyTypedRegistryItem<EInvoicingCredentials, EInvoicingCredentials> GetNewRegistryItem()
			=> new EInvoicingCredentialsRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
	}
}
