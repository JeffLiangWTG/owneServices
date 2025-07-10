using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Packing.Business
{
	public class PalletProviderRegistryItem : StronglyTypedRegistryItem<PalletTypeParent>
	{
		public PalletProviderRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, PalletTypeParent defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new PalletProviderRegistryDataType(defaultValue), null, RegistryStorageFlags.System, RegistryOptions.Default, defaultValue, false))
		{
		}
	}

	[RegistryEditor("Enterprise.Packing.GUI.PalletProviderRegistryItemEditor, Enterprise.Packing.GUI")]
	public class PalletProviderRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PalletTypeParent>
	{
		public PalletProviderRegistryDataType(PalletTypeParent defaultValue)
			: base(defaultValue)
		{
		}
	}
}
