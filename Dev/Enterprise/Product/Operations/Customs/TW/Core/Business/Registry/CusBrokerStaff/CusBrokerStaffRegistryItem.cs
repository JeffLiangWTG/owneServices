using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business
{
	public class CusBrokerStaffRegistryItem : StronglyTypedRegistryItem<CusBrokerStaff>
	{
		public CusBrokerStaffRegistryItem(
					string name,
					MultilingualString category,
					MultilingualString caption,
					MultilingualString hint,
					RegistryStorageFlags storage)
						: base(new RegistryItemImpl(
						name, category, caption, hint, new CusBrokerStaffRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.TW.GUI.CusBrokerStaffRegistryItemEditor, Enterprise.Customs.TW.GUI")]
	public class CusBrokerStaffRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CusBrokerStaff>
	{
		public CusBrokerStaffRegistryDataType()
		{
		}
	}
}
