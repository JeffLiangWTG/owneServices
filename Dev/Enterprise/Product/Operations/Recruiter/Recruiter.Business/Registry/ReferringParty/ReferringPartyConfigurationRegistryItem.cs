using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	public class ReferringPartyConfigurationRegistryItem : StronglyTypedRegistryItem<ReferringPartyConfigurationCollection>
	{
		public ReferringPartyConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new ReferringPartyConfigurationCollection())
		{
		}

		public ReferringPartyConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ReferringPartyConfigurationCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ReferringPartyConfigurationRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Recruiter.GUI.ReferringPartyConfigurationRegistryItemEditor, Enterprise.Recruiter.GUI")]
	public class ReferringPartyConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ReferringPartyConfigurationCollection>
	{
	}
}
