using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	public class AccountingIntegrationOptionsRegistryItem : StronglyTypedRegistryItem<AccountingIntegrationOptions>
	{
		public AccountingIntegrationOptionsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new AccountingIntegrationOptionsRegistryItemDataType(), storage, RegistryOptions.Default))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.DataRegistry.GUI.AccountingIntegrationOptionsRegistryItemRegistryItemEditor, Enterprise.Customs.GUI")]
	public class AccountingIntegrationOptionsRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<AccountingIntegrationOptions>
	{
	}
}
