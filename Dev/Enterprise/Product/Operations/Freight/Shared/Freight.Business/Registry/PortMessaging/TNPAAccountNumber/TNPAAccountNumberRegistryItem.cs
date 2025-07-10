using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	public class TNPAAccountNumberRegistryItem : StronglyTypedRegistryItem<TNPAAccountNumberCollection>
	{
		public TNPAAccountNumberRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, TNPAAccountNumberCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new TNPAAccountNumberRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Freight.GUI.TNPAAccountNumberRegistryItemEditor, Enterprise.Freight.GUI")]
	public class TNPAAccountNumberRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TNPAAccountNumberCollection>
	{
		public TNPAAccountNumberRegistryDataType()
		{
		}
	}
}
