using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.SG.Registry
{
	public class CycleNoRegistryItem : StronglyTypedRegistryItem<CycleNoCollection>
	{
		public CycleNoRegistryItem(string name, MultilingualString category, string caption, string hint, CycleNoCollection defaultValue)
			: this(name, category, caption, hint, RegistryStorageFlags.System, defaultValue)
		{
		}

		public CycleNoRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage, CycleNoCollection defaultValue)
			: base(new CycleNoRegistryItemImpl(name, category, caption, hint, storage, defaultValue))
		{
		}

		class CycleNoRegistryItemImpl : RegistryItemImpl
		{
			public CycleNoRegistryItemImpl(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage, CycleNoCollection defaultValue)
				: base(name, category, (NoResString)caption, (NoResString)hint, new CycleNoRegistryDataType(), storage, defaultValue)
			{
			}
		}
	}
}
