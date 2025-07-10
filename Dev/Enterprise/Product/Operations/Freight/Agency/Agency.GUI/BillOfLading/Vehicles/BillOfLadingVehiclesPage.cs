using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class BillOfLadingVehiclesPage : ZUserControl
	{
		public BillOfLadingVehiclesPage()
		{
			InitializeComponent();

			TariffFindHelper.AddDefaultPropertyToTariffControl(harmonisedCodeColumnStyleInfo, null);
			importReleaseNumberColumnStyleInfo.CaptionResourceString = CommonContainer.ImportReleaseNumberStringData;
		}

		protected override void OnAfterFirstBinding(System.EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (!this.IsDesignMode())
			{
				new UNDGDataItemFormManager(VehiclesGrid).Initialize();
				workflowFormHelper = new AgencyContainerWorkflowFormHelper(VehiclesGrid);
				workflowFormHelper.Hook();
			}
		}

		AgencyContainerWorkflowFormHelper workflowFormHelper;
	}
}


