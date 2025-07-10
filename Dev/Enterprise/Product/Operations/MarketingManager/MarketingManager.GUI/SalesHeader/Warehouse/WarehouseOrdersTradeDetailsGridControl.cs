using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public partial class WarehouseOrdersTradeDetailsGridControl : WarehouseTradeDetailsGridControl
	{
		public WarehouseOrdersTradeDetailsGridControl(OrgSalesProduct salesProduct)
			: base(salesProduct, OrgSalesWarehouseServiceTypesList.Codes.Orders)
		{
			InitializeComponent();

			TradeDetailsGrid.SetAvailability(false,
				new string[]
					{
						"CurrentProspectPeriod+" + OrgTradePeriod.Schema.PAS_CurrentRate,
						"CurrentProspectPeriod+" + OrgTradePeriod.Schema.PAS_RateOffered
					});

			TradeDetailsGrid.SetColumnCaption("ProspectDetail+" + OrgTradeProspect.Schema.PAP_RequiresPacking, Res.GetString("5d079a1d-8ee6-4b9e-92a0-b17e32b53fbc", "Pack Required"));
		}
	}
}
