using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	public class AutomaticDeferredSelectionRegistryItem : StronglyTypedRegistryItem<AutomaticDeferredSelection>
	{
		public AutomaticDeferredSelectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new AutomaticDeferredSelectionRegistryDataType(), storage, RegistryOptions.Default))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.ZA.DataRegistry.GUI.AutomaticDeferredSelectionRegistryItemEditor, Enterprise.Customs.ZA.GUI")]
	public class AutomaticDeferredSelectionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AutomaticDeferredSelection>
	{
	}
}
