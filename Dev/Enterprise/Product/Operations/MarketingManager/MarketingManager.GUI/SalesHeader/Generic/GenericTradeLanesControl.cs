using System;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI
{
	public partial class GenericTradeLanesControl : TradeLanesControl
	{
		[Obsolete("This just for designer. Use the constructor that takes sales product.")]
		public GenericTradeLanesControl()
			: base(null)
		{
			InitializeComponent();
		}

		public GenericTradeLanesControl(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			InitializeComponent();

			TradeLanesGrid.LayoutKey += "." + salesProduct.MP_Code;
			TradeLanesGrid.GridId += "|" + salesProduct.MP_Code;
		}

		public override ZGrid TradeLanesGrid
		{
			get { return tradeLanesGrid; }
		}
	}
}
