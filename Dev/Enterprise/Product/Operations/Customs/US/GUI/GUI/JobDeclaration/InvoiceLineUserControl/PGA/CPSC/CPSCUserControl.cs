using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class CPSCUserControl : ZUserControl
	{
		public CPSCUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(HeaderGrid).AddPGALineEditMenu();
		}

		public bool LotsGridVisible
		{
			get => !LotsAndOtherSplitContainer.Panel1Collapsed;
			set => LotsAndOtherSplitContainer.Panel1Collapsed = !value;
		}

		void ViewEditButton_Click(object sender, EventArgs e)
		{
			var header = this.CurrentDataItem as CPSCHeader;
			if (header != null)
			{
				var form = new CPSCForm(header);
				form.LotsGridVisible = LotsGridVisible;
				ZFormModaliser.ShowDialogAndDispose(form, this.ParentForm);
				var grid = HeaderGrid;
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
		internal const string NotificationMessage = "Please select (highlight) a CPSC line.";
		internal const string NotificationCaption = "Edit CPSC Line";

		internal void SetControlsHideAndRemoveTransactionalColumns()
		{
			var cpscColumnsShouldBeRemoved = new[]
			{
				CPSCHeader.Schema.US_LineNo,
				CPSCHeader.Schema.US_TrackingStatusDesc,
				"Status",
				"StatusDesc",
				"StatusDate"
			};

			this.HeaderGrid.RemoveFromAvailableColumns(cpscColumnsShouldBeRemoved);
		}
	}
}
