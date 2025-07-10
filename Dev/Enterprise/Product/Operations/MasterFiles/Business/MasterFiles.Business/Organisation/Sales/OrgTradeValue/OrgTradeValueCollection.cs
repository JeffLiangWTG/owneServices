using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradeValueCollection : ActiveBusinessObjectCollection<OrgTradeValue>
	{
		public OrgTradeValueCollection(OrgTradePeriod tradePeriod)
			: base(tradePeriod)
		{
		}
	}
}
