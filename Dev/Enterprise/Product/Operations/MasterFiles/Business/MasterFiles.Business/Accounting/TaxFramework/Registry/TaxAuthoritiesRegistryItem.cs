using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class TaxAuthoritiesRegistryItem : StronglyTypedRegistryItem<TaxAuthoritiesConfigurationCollection>
	{
		public TaxAuthoritiesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new RegistryItemImpl(name, category, caption, hint, new TaxAuthoritiesRegistryDataType(), storage, option))
		{
		}

		public TaxAuthoritiesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, TaxAuthoritiesConfigurationCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new TaxAuthoritiesRegistryDataType(), storage, option, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.TaxFramework.GUI.TaxAuthoritiesRegistryItemEditor, Enterprise.Accounting.TaxFramework.GUI")]
	public class TaxAuthoritiesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TaxAuthoritiesConfigurationCollection>
	{
		public TaxAuthoritiesRegistryDataType()
			: base()
		{
		}
	}
}
