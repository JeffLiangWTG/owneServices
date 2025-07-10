using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business
{
	public class CusBrokerageBoxNumberRegistryItem : StronglyTypedRegistryItem<CusBrokerageBoxNumberCollection>
	{
		public CusBrokerageBoxNumberRegistryItem(
					string name,
					MultilingualString category,
					MultilingualString caption,
					MultilingualString hint,
					RegistryStorageFlags storage)
						: base(new RegistryItemImpl(
						name, category, caption, hint, new CusBrokerageBoxNumberRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.TW.GUI.CusBrokerageBoxNumberRegistryItemEditor, Enterprise.Customs.TW.GUI")]
	public class CusBrokerageBoxNumberRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CusBrokerageBoxNumberCollection>
	{
		public CusBrokerageBoxNumberRegistryDataType()
		{
		}
	}
}
