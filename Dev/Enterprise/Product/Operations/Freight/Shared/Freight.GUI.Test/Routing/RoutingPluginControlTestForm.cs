using System.Reflection;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed partial class RoutingPluginControlTestForm : ZChildForm
	{
		public RoutingPluginControlTestForm(RoutingCollection transports)
			: base(transports)
		{ }

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
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
