using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class TaxDateDefaultingOptionRegistryItem : StronglyTypedRegistryItem<TaxDateDefaultingOptionCollection>
	{
		public TaxDateDefaultingOptionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, TaxDateDefaultingOptionCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new TaxDateDefaultingOptionRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.TaxDateDefaultingOptionRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	public class TaxDateDefaultingOptionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TaxDateDefaultingOptionCollection>
	{
		public TaxDateDefaultingOptionRegistryDataType()
		{
		}
	}
}
