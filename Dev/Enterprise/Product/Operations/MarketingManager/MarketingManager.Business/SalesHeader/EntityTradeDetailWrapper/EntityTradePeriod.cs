using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class EntityTradePeriod : OrgTradePeriod
	{
		public EntityTradePeriod(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override OrgTradeDetail TradeDetail => Factory.Load<EntityTradeDetailWrapper>(PAS_PA);

		public new EntityTradePeriodValidation Validation
		{
			get { return new EntityTradePeriodValidation(this); }
		}
	}
}
