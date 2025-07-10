using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class FWSUserControl : ZUserControl
	{
		public FWSUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(FWSHeaderGrid).AddPGALineEditMenu();
		}

		void ViewEditButton_Click(object sender, EventArgs e)
		{
			var header = this.CurrentDataItem as FWSHeader;
			if (header != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new FWSEditForm(header));
				var grid = FWSHeaderGrid;
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
		internal const string NotificationMessage = "Please select (highlight) a FWS line.";
		internal const string NotificationCaption = "Edit FWS Detail";

		internal void RemoveColumns()
		{
			FWSHeaderGrid.ColumnStyles.Remove(FWSHeaderGrid.GetColumnStyle(PGA.Schema.US_TrackingStatusDesc));
			FWSHeaderGrid.ColumnStyles.Remove(FWSHeaderGrid.GetColumnStyle("Status"));
			FWSHeaderGrid.ColumnStyles.Remove(FWSHeaderGrid.GetColumnStyle("StatusDesc"));
			FWSHeaderGrid.ColumnStyles.Remove(FWSHeaderGrid.GetColumnStyle("StatusDate"));
		}
	}
}
