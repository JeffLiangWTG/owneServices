using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class WarehouseTradeDetailsGridControl : ZUserControl
	{
		#region Static / Constructors

		public static WarehouseTradeDetailsGridControl New(OrgSalesProduct salesProduct, ZString service)
		{
			switch (service)
			{
				case OrgSalesWarehouseServiceTypesList.Codes.Orders:
					return new WarehouseOrdersTradeDetailsGridControl(salesProduct);

				case OrgSalesWarehouseServiceTypesList.Codes.Receipts:
					return new WarehouseReceiptsTradeDetailsGridControl(salesProduct);
			}

			return new WarehouseStorageTradeDetailsGridControl(salesProduct);
		}

		public WarehouseTradeDetailsGridControl()
		{
			InitializeComponent();
		}

		public WarehouseTradeDetailsGridControl(OrgSalesProduct salesProduct, ZString service)
		{
			Argument.NotNull(salesProduct, "salesProduct");

			InitializeComponent();

			TradeDetailsGrid.LayoutKey = "tradeDetailsGrid." + service;
			TradeDetailsGrid.GridId = "tradeDetailsGrid|" + service;
		}

		#endregion

		public ZGrid TradeDetailsGrid
		{
			get { return tradeDetailsGrid; }
		}
	}
}
