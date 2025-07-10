
using System.Windows.Forms;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public partial class GteGateMovementBookingForm : ZTemplateForm
	{
		public GteGateMovementBookingForm(GteGateMovementBooking gteGateMovementBooking) : base(gteGateMovementBooking)
		{
			ControllerID = ControllerIDs.GteGateMovementBooking;
			InitializeComponent();
			zWorkflowTabPage.Initialize(GteGateMovementBooking);
		}

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		void GlowLinkLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("Goto/GteBooking", GteGateMovementBooking.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", GteGateMovementBooking.Booking.PK.ToString()) });

			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		GteGateMovementBooking GteGateMovementBooking
		{
			get { return (GteGateMovementBooking)DataSource; }
		}
	}
}
