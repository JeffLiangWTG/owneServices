using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public class CargoGuideApiSettingsRegistryItem : StronglyTypedRegistryItem<CargoGuideApiSettings>
	{
		public CargoGuideApiSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new CargoGuideApiSettingsRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Rating.GUI.CargoGuideApiSettingsRegistryItemEditor, Enterprise.Rating.GUI")]
	public class CargoGuideApiSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CargoGuideApiSettings>
	{
		public CargoGuideApiSettingsRegistryDataType()
		{
		}
	}
}

