using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public class CargoguideCredentialsRegistryItem : StronglyTypedRegistryItem<CargoGuideCredentials>
	{
		public CargoguideCredentialsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new CargoGuideCredentialsRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Rating.GUI.CargoGuideCredentialsRegistryItemEditor, Enterprise.Rating.GUI")]
	public class CargoGuideCredentialsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CargoGuideCredentials>
	{
		public CargoGuideCredentialsRegistryDataType()
		{
		}
	}
}

