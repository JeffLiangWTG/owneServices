using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class TaxSystemsRegistryItem : StronglyTypedRegistryItem<TaxSystemsConfigurationCollection>
	{
		public TaxSystemsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
		: base(new RegistryItemImpl(name, category, caption, hint, new TaxSystemsRegistryDataType(), storage, option))
		{
		}

		public TaxSystemsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, TaxSystemsConfigurationCollection defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new TaxSystemsRegistryDataType(), storage, option, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.TaxFramework.GUI.TaxSystemsRegistryItemEditor, Enterprise.Accounting.TaxFramework.GUI")]
	public class TaxSystemsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TaxSystemsConfigurationCollection>
	{
		public TaxSystemsRegistryDataType()
			: base()
		{
		}
	}
}
