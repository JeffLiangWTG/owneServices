using System.ComponentModel.Design.Serialization;
using CargoWise.Windows.UI.Design;
using Enterprise.Registry.GUI;

namespace Enterprise.MasterFiles.GUI
{
	[DesignerSerializer(typeof(ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class ComplianceReportConfigurationControl : RegistryZUserControl
	{
		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ParentGrid.ReadOnly = ChildGrid.ReadOnly = readOnly;
		}

		public ComplianceReportConfigurationControl()
		{
			InitializeComponent();
		}
	}
}
