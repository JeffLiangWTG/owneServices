using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class DEAUserControl : ZUserControl
	{
		public DEAUserControl()
		{
			InitializeComponent();
			new ZGridPGADataCorrectionSupporter(DEAHeaderGrid).AddPGALineEditMenu();
		}

		public void RemoveUnavailableComlumnsForProduct()
		{
			var deaColumnsShouldBeRemoved = new[]
			{
				DEAHeader.Schema.US_LineNo,
				DEAHeader.Schema.US_PermitNumber,
				DEAHeader.Schema.US_TrackingStatusDesc,
				"Status",
				"StatusDesc",
				"StatusDate",
			};

			DEAHeaderGrid.RemoveFromAvailableColumns(deaColumnsShouldBeRemoved);
		}
	}
}
