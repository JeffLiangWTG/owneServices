using System.Windows.Forms;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public partial class GteVehicleMovementForm : ZTemplateForm
	{
		public GteVehicleMovementForm(GteVehicleMovement getVehicleMovemnt) : base(getVehicleMovemnt) {
			ControllerID = ControllerIDs.GteVehicleMovement;
			InitializeComponent();
			zWorkflowTabPage.Initialize(GteVehicleMovement);
		}

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		void GlowLinkLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("Goto/GteVehicleMovement", GteVehicleMovement.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", GteVehicleMovement.PK.ToString()) });

			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		GteVehicleMovement GteVehicleMovement
		{
			get { return (GteVehicleMovement)DataSource; }
		}
	}
}
