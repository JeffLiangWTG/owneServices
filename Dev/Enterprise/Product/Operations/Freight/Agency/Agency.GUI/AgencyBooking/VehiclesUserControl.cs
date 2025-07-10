using CargoWise.Windows.UI;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class VehiclesUserControl : ZUserControl
	{
		public VehiclesUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(System.EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (!this.IsDesignMode())
			{
				new UNDGDataItemFormManager(VehiclesGrid).Initialize();

				TariffFindHelper.AddDefaultPropertyToTariffControl(harmonisedCodeColumnStyleInfo, null);

				this.workflowFormHelper = new AgencyContainerWorkflowFormHelper(VehiclesGrid);
				this.workflowFormHelper.Hook();
			}
		}

		AgencyContainerWorkflowFormHelper workflowFormHelper;
	}
}


