using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class CusISFEquipLookups : AutoCusISFEquipLookups
	{
		public CusISFEquipLookups(AutoCusISFEquip parent)
			: base(parent)
		{
		}

		public USContainerCodeList EquipmentDescriptionCodes
		{
			get { return Factory.GetCachedValue("US ISF Equipment Description Code List", () => USContainerCodeList.GetEquipmentDescriptionCodeList()); }
		}
	}
}
