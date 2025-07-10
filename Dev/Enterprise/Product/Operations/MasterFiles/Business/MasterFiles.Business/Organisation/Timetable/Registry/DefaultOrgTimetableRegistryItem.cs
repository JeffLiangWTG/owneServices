using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class DefaultOrgTimetableRegistryItem : StronglyTypedRegistryItem<DefaultOrgTimetableSettingsCollection>
	{
		public DefaultOrgTimetableRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DefaultOrgTimetableSettingsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultOrgTimetableDataType(), storage, options, defaultValue))
		{
		}
	}
}
