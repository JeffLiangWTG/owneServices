using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	public class FinancialAccountNumberPortMapRegistryItem : StronglyTypedRegistryItem<FinancialAccountNumberPortMapCollection>
	{
		public FinancialAccountNumberPortMapRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new ManagedAccountCollectionImpl(name, category, caption, hint, storage))
		{
		}

		class ManagedAccountCollectionImpl : RegistryItemImpl
		{
			public ManagedAccountCollectionImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new FinancialAccountNumberPortMapRegistryDataType(), storage)
			{
			}
		}
	}

	[RegistryEditor("Enterprise.Customs.ZA.DataRegistry.GUI.FinancialAccountNumberPortMapRegistryItemEditor, Enterprise.Customs.ZA.GUI")]
	public class FinancialAccountNumberPortMapRegistryDataType : NonPersistentBusinessObjectRegistryDataType<FinancialAccountNumberPortMapCollection>
	{
	}
}
