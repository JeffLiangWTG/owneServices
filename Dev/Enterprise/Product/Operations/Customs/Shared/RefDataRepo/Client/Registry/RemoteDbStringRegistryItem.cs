using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class RemoteDbStringRegistryItem : StringRegistryItem
	{
		public RemoteDbStringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new RemoteDbStringRegistryDataType(), null, storage, options, defaultValue, false))
		{
		}
	}
}
