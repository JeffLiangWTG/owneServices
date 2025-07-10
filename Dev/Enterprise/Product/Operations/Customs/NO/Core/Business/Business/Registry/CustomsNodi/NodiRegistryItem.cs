using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NO.Registry
{
	public class NodiRegistryItem : StronglyTypedRegistryItem<NodiRegistryCollection>
	{
		public NodiRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, NodiRegistryCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new NodiDataType(), storage, RegistryOptions.IsOnlyForDevelopers, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.NO.Registry.GUI.NodiRegistryItemEditor, Enterprise.Customs.NO.GUI")]
	public class NodiDataType : NonPersistentBusinessObjectRegistryDataType<NodiRegistryCollection>
	{
	}
}
