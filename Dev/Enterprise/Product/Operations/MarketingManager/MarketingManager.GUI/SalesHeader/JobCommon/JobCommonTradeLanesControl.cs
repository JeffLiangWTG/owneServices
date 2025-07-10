using System.Collections.Generic;
using System.Linq;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MarketingManager.GUI
{
	public partial class JobCommonTradeLanesControl : TradeLanesControl
	{
		public JobCommonTradeLanesControl(OrgSalesProduct salesProduct)
			: base(salesProduct)
		{
			InitializeComponent();

			TradeLanesGrid.LayoutKey += "." + salesProduct.MP_Code;
			TradeLanesGrid.GridId += "|" + salesProduct.MP_Code;
			TradeLanesGrid.SetAvailability(false, LocationTypeColumnsInTradeLanesGrid.ToArray());

			if (salesProduct.LocationArrangement == OrgSalesProductLocationArrangement.SingleLocation)
			{
				TradeLanesGrid.SetAvailability(false, new string[] { OrgSales.Schema.OW_DestinationID });
				TradeLanesGrid.GetColumnStyle(OrgSales.Schema.OW_OriginID).CaptionResourceString = Res.GetData("61f30312-62ee-4aa5-99a3-e3356d3e8029", "Location");
			}
		}

		#region Grids

		public override ZGrid TradeLanesGrid
		{
			get { return tradeLanesGrid; }
		}

		#endregion

		#region Columns

		protected IEnumerable<string> LocationTypeColumnsInTradeLanesGrid
		{
			get
			{
				yield return OrgSales.Schema.OriginLocationType;
				yield return OrgSales.Schema.DestinationLocationType;
			}
		}

		#endregion
	}
}
