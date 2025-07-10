using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CashFlowCategoryBasedOnDebtorGroupRegistryItem : StronglyTypedRegistryItem<CashFlowCategoryBasedOnDebtorGroupCollection>
	{
		public CashFlowCategoryBasedOnDebtorGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new CashFlowCategoryBasedOnDebtorGroupRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		class CashFlowCategoryBasedOnDebtorGroupRegistryItemImpl : RegistryItemImpl
		{
			public CashFlowCategoryBasedOnDebtorGroupRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new CashFlowCategoryBasedOnDebtorGroupRegistryDataType(), storage, options)
			{
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.CashFlowCategoryBasedOnDebtorGroupRegistryItemEditor, Enterprise.Accounting.GUI")]
	class CashFlowCategoryBasedOnDebtorGroupRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CashFlowCategoryBasedOnDebtorGroupCollection>
	{
		public CashFlowCategoryBasedOnDebtorGroupRegistryDataType()
		{
		}
	}
}
