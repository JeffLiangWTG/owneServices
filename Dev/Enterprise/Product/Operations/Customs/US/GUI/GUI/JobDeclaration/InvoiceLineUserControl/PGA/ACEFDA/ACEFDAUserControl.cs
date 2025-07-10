using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ACEFDAUserControl : ZUserControl
	{
		public ACEFDAUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(FDAGrid).AddPGALineEditMenu();
		}

		void ViewEditButton_Click(object sender, System.EventArgs e)
		{
			var line = this.CurrentDataItem as ACEFDA;
			if (line != null)
			{
				if (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.Value)
				{
					ZFormModaliser.ShowDialogAndDispose(new ACEFDAEditForm(line));
				}
				else
				{
					ZFormModaliser.ShowDialogAndDispose(new ACEFDAPopupForm(line));
				}
				var grid = FDAGrid;
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
		internal const string NotificationMessage = "Please select (highlight) a FDA line.";
		internal const string NotificationCaption = "Edit FDA Line";

		internal void SetContainerControlsHideAndRemoveTransactionalColumns()
		{
			this.LotsAndOtherSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
			this.DetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);

			var fdaColumnsShouldBeRemoved = new[]
			{
				ACEFDA.Schema.US_InvCurrValue,
				ACEFDA.Schema.US_TotalUSDValue,
				ACEFDA.Schema.US_UnitValue,
				ACEFDA.Schema.US_TrackingStatusDesc,
				"Status",
				"StatusDesc",
				"StatusDate"
			};

			this.FDAGrid.RemoveFromAvailableColumns(fdaColumnsShouldBeRemoved);
		}
	}
}
