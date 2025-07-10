using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	public class ApplicationStatusRegistryItem : StronglyTypedRegistryItem<ApplicationStatusCollection>
	{
		public ApplicationStatusRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new ApplicationStatusCollection())
		{
		}

		public ApplicationStatusRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ApplicationStatusCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ApplicationStatusRegistryDataType(), storage, defaultValue))
		{
		}

		public ApplicationStatusRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ApplicationStatusCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ApplicationStatusRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Recruiter.GUI.ApplicationStatusRegistryItemEditor, Enterprise.Recruiter.GUI")]
	public class ApplicationStatusRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ApplicationStatusCollection>
	{
		public ApplicationStatusRegistryDataType()
		{
		}
	}
}
