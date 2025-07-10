using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	public class CargoSphereCredentialsRegistryItem : StronglyTypedRegistryItem<CargoSphereCredentials>
	{
		public CargoSphereCredentialsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new CargoSphereCredentialsRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Rating.GUI.CargoSphereCredentialsRegistryItemEditor, Enterprise.Rating.GUI")]
	public class CargoSphereCredentialsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CargoSphereCredentials>
	{
		public CargoSphereCredentialsRegistryDataType()
		{
		}
	}
}

