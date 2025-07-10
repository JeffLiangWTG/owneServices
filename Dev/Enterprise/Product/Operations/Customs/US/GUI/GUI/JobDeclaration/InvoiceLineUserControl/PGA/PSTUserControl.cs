using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	public partial class PSTUserControl : ZUserControl
	{
		public PSTUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(PSTGrid).AddPGALineEditMenu();
		}

		void ViewEditButton_Click(object sender, System.EventArgs e)
		{
			var line = this.CurrentDataItem as Pesticide;
			if (line != null)
			{
				var pstForm = new PSTEditForm(line);
				if (fromProduct)
				{
					pstForm.ChangeVisibilityOfControlsForProduct();
				}
				ZFormModaliser.ShowDialogAndDispose(pstForm);
				var grid = PSTGrid;
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

		public void SetPropertyForProduct()
		{
			fromProduct = true;
			PSTGrid.RemoveFromAvailableColumns(USPSTAddInfoSchema.Constants.US_PSTLabelsSent);
			PSTGrid.RemoveFromAvailableColumns(USPSTAddInfoSchema.Constants.US_LineNo);
			PSTGrid.RemoveFromAvailableColumns(Pesticide.Schema.US_TrackingStatusDesc);
			PSTGrid.RemoveFromAvailableColumns("Status");
			PSTGrid.RemoveFromAvailableColumns("StatusDesc");
			PSTGrid.RemoveFromAvailableColumns("StatusDate");
		}

		bool fromProduct;
		internal const string NotificationMessage = "Please select (highlight) a Pesticide line.";
		internal const string NotificationCaption = "Edit Pesticide Line";
	}
}
