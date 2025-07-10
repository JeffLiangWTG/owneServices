using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	public class RecruiterTestTypeRegistryItem : StronglyTypedRegistryItem<RecruiterTestTypeCollection>
	{
		public RecruiterTestTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new RecruiterTestTypeCollection())
		{
		}

		public RecruiterTestTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RecruiterTestTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new RecruiterTestTypeRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Recruiter.GUI.RecruiterTestTypeRegistryItemEditor, Enterprise.Recruiter.GUI")]
	public class RecruiterTestTypeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<RecruiterTestTypeCollection>
	{
		public RecruiterTestTypeRegistryDataType()
		{
		}
	}
}
