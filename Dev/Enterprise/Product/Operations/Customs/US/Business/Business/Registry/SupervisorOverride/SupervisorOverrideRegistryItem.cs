using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	public sealed class SupervisorOverrideRegistryItem : StronglyTypedRegistryItem<SupervisorOverrideData>
	{
		public SupervisorOverrideRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new SupervisorOverrideRegistryDataType(), storage, RegistryOptions.Default))
		{
		}

		public SupervisorOverrideRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, SupervisorOverrideData defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new SupervisorOverrideRegistryDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}
	}
}
