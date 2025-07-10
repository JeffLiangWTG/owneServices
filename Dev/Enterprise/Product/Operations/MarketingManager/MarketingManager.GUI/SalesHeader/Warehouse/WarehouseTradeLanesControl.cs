using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI
{
	public partial class WarehouseTradeLanesControl : TradeLanesControl
	{
		public WarehouseTradeLanesControl(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			InitializeComponent();

			TradeLanesGrid.LayoutKey += "." + salesProduct.MP_Code;
			TradeLanesGrid.GridId += "|" + salesProduct.MP_Code;
		}

		#region Grids

		public override ZGrid TradeLanesGrid
		{
			get { return tradeLanesGrid; }
		}

		#endregion
	}
}
