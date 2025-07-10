using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class SalesHeaderControl : ZUserControl
	{
		public SalesHeaderControl()
		{
			InitializeComponent();
		}

		#region Properties

		#region ShowTradedColumns

		[DefaultValue(true)]
		public bool ShowTradedColumns
		{
			get { return showTradedColumns; }
			set
			{
				if (showTradedColumns != value)
				{
					showTradedColumns = value;
					grid.SetAvailability(value, TradedColumns);
				}
			}
		}
		bool showTradedColumns = true;

		readonly string[] TradedColumns = new string[]
		{
			SalesHeader.Schema.TradedMonthlyAverage,
			SalesHeader.Schema.TradedAnnualTotal,
			SalesHeader.Schema.TradedAnnualTEUTotalQuantity,
		};

		#endregion

		#endregion

		#region Grid

		public ZGrid Grid
		{
			get { return grid; }
		}

		void Grid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (grid.HitTest(e.X, e.Y).Row > -1)
			{
				GridRowDoubleClicked?.Invoke(this, e);
			}
		}

		public event MouseEventHandler GridRowDoubleClicked;

		#endregion
	}
}
