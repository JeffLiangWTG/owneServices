using System.Reflection;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class RoutingPluginControlTestForm_TransportBooking : ZChildForm
	{
		public RoutingPluginControlTestForm_TransportBooking(TransportCollection transports)
			: base(transports)
		{ }

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			base.InitializeComponent();
			control = new TestRoutingPluginControl();
			control.Dock = DockStyle.Fill;
			this.ClientSize = control.Size;
			this.Controls.Add(control);

			Grid = (ZGrid)GetControl("TransportsGrid");

			ReceivalAvailabilityPanel = (ZPanel)GetControl("ReceivalAvailabilityPanel");
			OriginDestinationPanel = (ZPanel)GetControl("OriginDestinationPanel");

			this.CaptionRenderingEnabled = true;
		}

		Control GetControl(string name)
		{
			return (Control)typeof(RoutingPluginControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}

		public ZPanel SelectedEndPointPanel
		{
			get { return (ZPanel)typeof(RoutingPluginControl).GetProperty("SelectedEndPointPanel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control, null); }
		}

		public void Select(Transport transport)
		{
			Grid.ListManager.Position = Grid.ListManager.List.IndexOf(transport);
		}

		public RoutingPluginControl control;
		public ZGrid Grid;
		public ZPanel ReceivalAvailabilityPanel;
		public ZPanel OriginDestinationPanel;
	}
}
