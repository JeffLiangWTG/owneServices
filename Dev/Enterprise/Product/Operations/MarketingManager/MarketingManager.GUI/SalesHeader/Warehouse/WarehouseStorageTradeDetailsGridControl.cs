using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public partial class WarehouseStorageTradeDetailsGridControl : WarehouseTradeDetailsGridControl
	{
		public WarehouseStorageTradeDetailsGridControl(OrgSalesProduct salesProduct)
			: base(salesProduct, OrgSalesWarehouseServiceTypesList.Codes.Storage)
		{
			InitializeComponent();

			TradeDetailsGrid.SetAvailability(false,
				new string[]
					{
						"CurrentProspectPeriod+" + OrgTradePeriod.Schema.PAS_RepeatsMnth,
						"CurrentProspectPeriod+" + OrgTradePeriod.Schema.PAS_LineCount,
						"ProspectDetail+" + OrgTradeProspect.Schema.PAP_RequiresPacking,
						"ProspectDetail+" + OrgTradeProspect.Schema.PAP_RC_NKContainer,
						"ProspectDetail+" + OrgTradeProspect.Schema.PAP_RequiresCrossDock
					});
		}
	}
}
