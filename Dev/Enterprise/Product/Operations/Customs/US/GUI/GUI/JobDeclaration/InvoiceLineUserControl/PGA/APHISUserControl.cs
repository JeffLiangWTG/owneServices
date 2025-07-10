using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class APHISUserControl : ZUserControl
	{
		public APHISUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(APHISHeaderGrid).AddPGALineEditMenu();
		}

		void ViewEditButton_Click(object sender, EventArgs e)
		{
			var header = this.CurrentDataItem as APHISHeader;
			if (header != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new APHISEditForm(header));
				var grid = APHISHeaderGrid;
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
		internal const string NotificationMessage = "Please select (highlight) a APHIS line.";
		internal const string NotificationCaption = "Edit APHIS Detail";

		public void RemoveUnavailableComlumnsForProduct()
		{
			var deaColumnsShouldBeRemoved = new[]
			{
				APHISHeader.Schema.US_LineNo,
				APHISHeader.Schema.US_TrackingStatusDesc,
				"ApplicantOrgPK",
				APHISHeader.Schema.US_OA_ApplicantAddress,
				"Status",
				"StatusDesc",
				"StatusDate"
			};

			APHISHeaderGrid.RemoveFromAvailableColumns(deaColumnsShouldBeRemoved);
		}
	}
}
