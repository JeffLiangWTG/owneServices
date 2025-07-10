using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class EquipmentUserControl : ZUserControl
	{
		public EquipmentUserControl()
		{
			InitializeComponent();

			InsuranceYearPolicyIssueYearEdit.AllowOutsideOfParent();
		}
	}
}
