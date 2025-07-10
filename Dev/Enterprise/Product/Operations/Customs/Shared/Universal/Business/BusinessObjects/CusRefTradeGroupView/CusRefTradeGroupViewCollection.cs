using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public class CusRefTradeGroupViewCollection : ActiveBusinessObjectCollection<CusRefTradeGroupView>
	{
		public CusRefTradeGroupViewCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public CusRefTradeGroupViewCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
