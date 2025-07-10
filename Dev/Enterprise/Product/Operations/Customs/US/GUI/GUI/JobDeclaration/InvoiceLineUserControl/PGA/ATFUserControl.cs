using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ATFUserControl : ZUserControl
	{
		public ATFUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(ATFGrid).AddPGALineEditMenu();
		}

		internal void RemoveColumns()
		{
			ATFGrid.ColumnStyles.Remove(ATFGrid.GetColumnStyle("US_Quantity")); // Coulmn name US_Quantity should be remove
			ATFGrid.ColumnStyles.Remove(ATFGrid.GetColumnStyle("US_TrackingStatusDesc")); // Coulmn name US_TrackingStatusDesc should be remove
			ATFGrid.ColumnStyles.Remove(ATFGrid.GetColumnStyle("Status"));
			ATFGrid.ColumnStyles.Remove(ATFGrid.GetColumnStyle("StatusDesc"));
			ATFGrid.ColumnStyles.Remove(ATFGrid.GetColumnStyle("StatusDate"));
		}
	}
}
