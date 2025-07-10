using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	[DefaultDataSourceBindingMember(null)]
	public partial class JobCommonTradeDetailsInnerControl : ZUserControl, IJobCommonTradeDetailsInnerControl
	{
		[Obsolete("This just for designer. Use the constructor that takes sales product.")]
		public JobCommonTradeDetailsInnerControl()
		{
			InitializeComponent();
		}

		public JobCommonTradeDetailsInnerControl(ZString salesProductCode)
		{
			InitializeComponent();

			GroupingGrid.LayoutKey += "." + salesProductCode;
			GroupingGrid.GridId += "|" + salesProductCode;
			TradeDetailsGrid.LayoutKey += "." + salesProductCode;
			TradeDetailsGrid.GridId += "|" + salesProductCode;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var collection = (OrgTradeDetailJobCommonGroupingCollection)dataSource;
			if (collection != null)
			{
				groupingGrid.SetAvailability(true, OrgTradeDetailJobCommonGrouping.Schema.TradeType);
				tradeDetailsGrid.SetAvailability(false, OrgTradeDetail.Schema.PA_TradeType);
				mainSplitContainer.SplitterDistance = ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			}

			base.SetDataBinding(dataSource, dataMember);
		}

		public ZGrid GroupingGrid
		{
			get { return groupingGrid; }
		}

		public ZGrid TradeDetailsGrid
		{
			get { return tradeDetailsGrid; }
		}

		public bool ReadOnly { get; set; }
	}
}
