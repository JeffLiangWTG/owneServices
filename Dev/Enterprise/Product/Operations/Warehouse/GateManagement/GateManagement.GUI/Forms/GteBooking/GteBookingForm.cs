
using System.Windows.Forms;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public partial class GteBookingForm : ZTemplateForm
	{
		public GteBookingForm(GteBooking gteBooking) : base(gteBooking)
		{
			ControllerID = ControllerIDs.GteBooking;
			InitializeComponent();
			zWorkflowTabPage.Initialize(GteBooking);
			PlugIns.Add(ControllerIDs.DocAddresses);
		}

		public override string FormCaption
		{
			get { return BusinessEntity != null ? BusinessEntity.HumanReadableName.ToString() : base.FormCaption; }
		}

		void GlowLinkLabelClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("Goto/GteBooking", GteBooking.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", GteBooking.PK.ToString()) });

			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		GteBooking GteBooking
		{
			get { return (GteBooking)DataSource; }
		}
	}
}
