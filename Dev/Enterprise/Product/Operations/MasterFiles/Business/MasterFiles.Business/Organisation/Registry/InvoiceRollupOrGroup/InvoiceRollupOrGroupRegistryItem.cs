using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class InvoiceRollupOrGroupRegistryItem : StronglyTypedRegistryItem<InvoiceRollupOrGroupCollection>
	{
		public InvoiceRollupOrGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, InvoiceRollupOrGroupCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new InvoiceRollupOrGroupDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.MasterFiles.GUI.InvoiceRollupOrGroupRegistryItemEditor, Enterprise.MasterFiles.GUI")]
	class InvoiceRollupOrGroupDataType : NonPersistentBusinessObjectRegistryDataType<InvoiceRollupOrGroupCollection>
	{
		public InvoiceRollupOrGroupDataType()
		{
		}
	}
}
