using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CashFlowCategoryBasedOnCreditorGroupRegistryItem : StronglyTypedRegistryItem<CashFlowCategoryBasedOnCreditorGroupCollection>
	{
		public CashFlowCategoryBasedOnCreditorGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new CashFlowCategoryBasedOnCreditorGroupRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		class CashFlowCategoryBasedOnCreditorGroupRegistryItemImpl : RegistryItemImpl
		{
			public CashFlowCategoryBasedOnCreditorGroupRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new CashFlowCategoryBasedOnCreditorGroupRegistryDataType(), storage, options)
			{
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.CashFlowCategoryBasedOnCreditorGroupRegistryItemEditor, Enterprise.Accounting.GUI")]
	class CashFlowCategoryBasedOnCreditorGroupRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CashFlowCategoryBasedOnCreditorGroupCollection>
	{
		public CashFlowCategoryBasedOnCreditorGroupRegistryDataType()
		{
		}
	}
}
