using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business
{
	public class CusCustomsOfficeRegistryItem : StronglyTypedRegistryItem<CusCustomsOffice>
	{
		public CusCustomsOfficeRegistryItem(
					string name,
					MultilingualString category,
					MultilingualString caption,
					MultilingualString hint,
					RegistryStorageFlags storage)
						: base(new RegistryItemImpl(
						name, category, caption, hint, new CusCustomsOfficeRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.TW.GUI.CusCustomsOfficeRegistryItemEditor, Enterprise.Customs.TW.GUI")]
	public class CusCustomsOfficeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CusCustomsOffice>
	{
		public CusCustomsOfficeRegistryDataType()
		{
		}
	}
}
