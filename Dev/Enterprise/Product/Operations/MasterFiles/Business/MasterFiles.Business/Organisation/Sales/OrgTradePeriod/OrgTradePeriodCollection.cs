using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTradePeriodCollection : ActiveBusinessObjectCollection<OrgTradePeriod>
	{
		public OrgTradePeriodCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public OrgTradePeriodCollection(OrgTradeDetail tradeDetail, bool defaultIsTraded)
			: base(tradeDetail)
		{
			this.defaultIsTraded = defaultIsTraded;
		}
		readonly bool defaultIsTraded;

		protected override void SetDefaultsForNewElementCore(OrgTradePeriod newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.PAS_IsTraded = defaultIsTraded;
		}
	}
}
