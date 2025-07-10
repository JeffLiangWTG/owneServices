using System.Reflection;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed partial class VoyageDetailsControlTestForm : ZForm
	{
		public VoyageDetailsControlTestForm(JobVoyage voyage)
			: base(voyage)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public ZLabel VendorDataStatusLabel
		{
			get { return control.VendorDataStatusLabel; }
		}

		public VoyageDetailsControl control;
		public ZGrid ExRatesGrid;
		public ZCalcEditColumnStyleInfo ExRateColumnStyle;
		public SailingsGrid SailingsGrid;

		Control GetControl(string name)
		{
			return typeof(VoyageDetailsControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(control) as Control;
		}
	}
}
