using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class HFCUserControl : ZUserControl
	{
		public HFCUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(HFCGrid).AddPGALineEditMenu();
		}

		void ViewEditButton_Click(object sender, System.EventArgs e)
		{
			var line = this.CurrentDataItem as USHFCHeader;
			if (line != null)
			{
				var hfcForm = new HFCForm(line);
				if (fromProduct)
				{
					hfcForm.ChangeVisibilityOfControlsForProduct();
				}
				ZFormModaliser.ShowDialogAndDispose(hfcForm);
				var grid = HFCGrid;
				if (grid != null)
				{
					grid.ListManager.EndCurrentEdit();
				}
			}
			else
			{
				Globals.Message.ShowInformation(NotificationMessage, NotificationCaption);
			}
		}
		internal const string NotificationMessage = "Please select (highlight) a Hydrofluorocarbons Header.";
		internal const string NotificationCaption = "Edit Hydrofluorocarbons Header";

		internal void RemoveColumns()
		{
			fromProduct = true;
			HFCGrid.ColumnStyles.Remove(HFCGrid.GetColumnStyle(PGA.Schema.US_TrackingStatusDesc));
			HFCGrid.ColumnStyles.Remove(HFCGrid.GetColumnStyle("Status"));
			HFCGrid.ColumnStyles.Remove(HFCGrid.GetColumnStyle("StatusDesc"));
			HFCGrid.ColumnStyles.Remove(HFCGrid.GetColumnStyle("StatusDate"));
			HFCGrid.ColumnStyles.Remove(HFCGrid.GetColumnStyle(USHFCHeader.Schema.US_HFCImageSent));
		}

		bool fromProduct;
	}
}
