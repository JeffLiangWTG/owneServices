using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusPackableItemCollection : ActiveBusinessObjectCollection<CusPackableItem>
	{
		public CusPackableItemCollection(CusPackingList packingList) : base(packingList)
		{
		}

		public CusPackableItemCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
