using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry.AWB
{
	public class AWBDisplayOptionRegistryItem : StronglyTypedRegistryItem<AWBDisplayOptionCollection>
	{
		public AWBDisplayOptionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, AWBDisplayOptionCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AWBDisplayOptionRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Freight.Forwarding.GUI.AWB.AWBDisplayOptionRegistryItemEditor, Enterprise.Freight.Forwarding.GUI")]
	class AWBDisplayOptionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<AWBDisplayOptionCollection>
	{
		public AWBDisplayOptionRegistryDataType()
		{
		}
	}
}
