using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public partial class WarehouseReceiptsTradeDetailsGridControl : WarehouseTradeDetailsGridControl
	{
		public WarehouseReceiptsTradeDetailsGridControl(OrgSalesProduct salesProduct)
			: base(salesProduct, OrgSalesWarehouseServiceTypesList.Codes.Receipts)
		{
			InitializeComponent();

			TradeDetailsGrid.SetAvailability(false,
				new string[]
					{
						"ProspectDetail+" + OrgTradeProspect.Schema.PAP_RequiresCrossDock,
						"CurrentProspectPeriod+" + OrgTradePeriod.Schema.PAS_CurrentRate,
						"CurrentProspectPeriod+" + OrgTradePeriod.Schema.PAS_RateOffered
					});

			TradeDetailsGrid.SetColumnCaption("ProspectDetail+" + OrgTradeProspect.Schema.PAP_RequiresPacking, Res.GetString("0d08285a-d0fc-4661-9ec3-2e9e55414ec2", "Unpack Required"));
		}
	}
}
