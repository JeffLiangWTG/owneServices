using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class NMFSUserControl : ZUserControl
	{
		public NMFSUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(NMFSGrid).AddPGALineEditMenu();
			visibilityHelper = new NMFSControlVisibilityHelper(this.HarvestingDetailsGrid, this.DocumentDetailsGroupBox, this.HarvestingVesselsGroupBox);

			HarvestingDetailsSplitter.AllowOverlap(HarvestingDetailsGroupBox);
		}

		readonly NMFSControlVisibilityHelper visibilityHelper;

		void ViewEditButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem is NMFSLine line)
			{
				ZFormModaliser.ShowDialogAndDispose(new NMFSEditForm(line));
				NMFSGrid?.ListManager?.EndCurrentEdit();
			}
			else
			{
				Globals.Message.ShowInformation(NotificationMessage, NotificationCaption);
			}
		}
		internal const string NotificationMessage = "Please select (highlight) a NMFS line.";
		internal const string NotificationCaption = "Edit NMFS Line";

		public void RemoveUnavailableControlsForProduct()
		{
			var columnsShouldBeRemoved = new[]
			{
				NMFSLine.Schema.US_DISDocumentID,
				NMFSLine.Schema.US_TrackingStatusDesc,
				"Status",
				"StatusDesc",
				"StatusDate",
			};

			NMFSGrid.RemoveFromAvailableColumns(columnsShouldBeRemoved);

			DocumentDetailsGroupBox.Visible = false;
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			NMFSGridListManager_CurrentChanged();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (visibilityHelper.CurrentLine != null && !visibilityHelper.CurrentLine.IsDeleted)
			{
				visibilityHelper.UnHookNMFSLineEvents();
			}
		}

		void NMFSGrid_AfterBind(object sender, EventArgs e)
		{
			NMFSGridListManager_CurrentChanged();
		}

		void NMFSGridListManager_CurrentChanged()
		{
			NMFSLine currentLine = null;
			var listManager = NMFSGrid.ListManager;
			if (listManager != null && listManager.Count > 0)
			{
				currentLine = listManager.GetCurrent() as NMFSLine;
				visibilityHelper.CurrentLine = currentLine;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				visibilityHelper.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
