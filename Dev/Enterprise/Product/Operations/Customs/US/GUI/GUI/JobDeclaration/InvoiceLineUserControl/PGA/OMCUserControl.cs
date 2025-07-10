using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class OMCUserControl : ZUserControl
	{
		public OMCUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(OMCLineGrid).AddPGALineEditMenu();
		}

		internal void SetControlsHideAndRemoveTransactionalColumns()
		{
			var omcColumnsShouldBeRemoved = new[]
			{
				OMCHeader.Schema.US_LineNo,
				OMCHeader.Schema.US_ElectronicImageSubmitted,
				OMCHeader.Schema.US_DepartureDate,
				OMCHeader.Schema.US_TrackingStatusDesc,
				"Status",
				"StatusDesc",
				"StatusDate"
			};

			this.OMCLineGrid.RemoveFromAvailableColumns(omcColumnsShouldBeRemoved);
		}

		void ViewEditButton_Click(object sender, EventArgs e)
		{
			var header = this.CurrentDataItem as OMCHeader;
			if (header != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new OMCForm(header), this.ParentForm);
				var grid = OMCLineGrid;
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
		internal const string NotificationMessage = "Please select (highlight) a OMC line.";
		internal const string NotificationCaption = "Edit OMC Line";
	}
}
