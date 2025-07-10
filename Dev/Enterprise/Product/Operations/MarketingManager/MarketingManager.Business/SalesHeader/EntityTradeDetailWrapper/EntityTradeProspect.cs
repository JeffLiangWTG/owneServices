using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class EntityTradeProspect : OrgTradeProspect
	{
		public EntityTradeProspect(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override OrgTradeDetail TradeDetail => Factory.Load<EntityTradeDetailWrapper>(PAP_PA);
	}
}
