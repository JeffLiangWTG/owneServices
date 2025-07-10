using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	public class ServiceTaskCreatorOptionRegistryItem : StronglyTypedRegistryItem<ServiceTaskCreatorOptionCollection>
	{
		public ServiceTaskCreatorOptionRegistryItem(
				string name,
				MultilingualString category,
				MultilingualString caption,
				MultilingualString hint,
				RegistryStorageFlags storage,
				RegistryOptions options,
				ServiceTaskCreatorOptionCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ServiceTaskCreatorOptionRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.TransportCommon.GUI.Registry.ServiceTaskCreatorOptionRegistryItemEditor, Enterprise.TransportCommon.GUI")]
	public class ServiceTaskCreatorOptionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ServiceTaskCreatorOptionCollection>
	{
	}
}
