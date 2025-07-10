using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class NHTSAUserControl : ZUserControl
	{
		public NHTSAUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(NHTSAHeaderGrid).AddPGALineEditMenu();
		}

		void ViewEditButton_Click(object sender, System.EventArgs e)
		{
			var header = this.CurrentDataItem as NHTSAHeader;
			if (header != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new NHTSAEditForm(header));
				var grid = NHTSAHeaderGrid;
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
		internal const string NotificationMessage = "Please select (highlight) a NHTSA line.";
		internal const string NotificationCaption = "Edit NHTSA Line";

		public void RemoveUnavailableComlumnsForProduct()
		{
			var columnsShouldBeRemoved = new[]
			{
				NHTSAHeader.Schema.US_NHTElectronicImage,
				NHTSAHeader.Schema.US_NHTEmbassyNationality,
				NHTSAHeader.Schema.US_NHTTravelDocNationality,
				NHTSAHeader.Schema.US_NHTTravelDocType,
				NHTSAHeader.Schema.US_NHTTravelDocNumber,
				NHTSAHeader.Schema.US_PGAContactName,
				NHTSAHeader.Schema.US_PGAContactPhoneNo,
				NHTSAHeader.Schema.US_PGAContactEmail,
				NHTSAHeader.Schema.US_TrackingStatusDesc,
				"Status",
				"StatusDesc",
				"StatusDate"
			};

			NHTSAHeaderGrid.RemoveFromAvailableColumns(columnsShouldBeRemoved);
		}
	}
}
