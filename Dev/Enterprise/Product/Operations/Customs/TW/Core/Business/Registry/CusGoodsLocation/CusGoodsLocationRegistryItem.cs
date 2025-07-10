using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business
{
	public class CusGoodsLocationRegistryItem : StronglyTypedRegistryItem<CusGoodsLocationCollection>
	{
		public CusGoodsLocationRegistryItem(
					string name,
					MultilingualString category,
					MultilingualString caption,
					MultilingualString hint,
					RegistryStorageFlags storage)
						: base(new RegistryItemImpl(
						name, category, caption, hint, new CusGoodsLocationRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.TW.GUI.CusGoodsLocationRegistryItemEditor, Enterprise.Customs.TW.GUI")]
	public class CusGoodsLocationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CusGoodsLocationCollection>
	{
		public CusGoodsLocationRegistryDataType()
		{
		}
	}
}
