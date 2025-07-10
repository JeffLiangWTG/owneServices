using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(AccountingIntegrationOptionsRegistryItem))]
	sealed class AccountingIntegrationOptionsRegistryItemTest : StronglyTypedRegistryItemTestCase<AccountingIntegrationOptions>
	{
		protected override StronglyTypedRegistryItem<AccountingIntegrationOptions, AccountingIntegrationOptions> GetNewRegistryItem()
		{
			return new AccountingIntegrationOptionsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
