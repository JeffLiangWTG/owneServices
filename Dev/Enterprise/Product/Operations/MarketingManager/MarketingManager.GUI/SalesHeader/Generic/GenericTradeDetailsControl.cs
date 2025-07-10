using System;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI
{
	public partial class GenericTradeDetailsControl : TradeDetailsControl
	{
		[Obsolete("This just for designer. Use the constructor that takes sales product.")]
		public GenericTradeDetailsControl()
		{
			InitializeComponent();
		}

		public GenericTradeDetailsControl(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			InitializeComponent();
			TradeDetailsGrid.LayoutKey += "." + salesProduct.MP_Code;
			TradeDetailsGrid.GridId += "|" + salesProduct.MP_Code;
		}

		public override ZGrid TradeDetailsGrid
		{
			get { return tradeDetailsGrid; }
		}
	}
}
