using System.Reflection;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed partial class ConsolLegDetailsTestForm : ZForm
	{
		public ConsolLegDetailsTestForm(CommonConsol consol)
			: base(consol)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		Control GetControl(string name)
		{
			return (Control)typeof(ConsoLegDetailsControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}

		public ConsoLegDetailsControl control;
		public Label warningLabel;
		public Panel AirPanel;
		public Panel RoadPanel;
		public Panel RailPanel;
		public Panel SeaPanel;
		public CheckBox CargoOnlyControl;
	}
}
