using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable IDE0001 // Simplify names. Designer requires fully qualified names to correctly deserialize properties
#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Recruiter.GUI
{
	public partial class RecruiterTestTypeControl : RegistryZUserControl
	{
		public RecruiterTestTypeControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			RecruiterTestTypeGrid.ReadOnly = readOnly;
			zTextBox1.ReadOnly = readOnly;
		}

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZGroupBox zGroupBox1;
		private ZTextBox zTextBox1;
	}
}
