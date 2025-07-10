using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OverallStaffCommissionRulePreviewPane : ZUserControl
	{
		public OverallStaffCommissionRulePreviewPane()
		{
			InitializeComponent();

			if (!CommissionLookups.ShouldShowServicesAndSubModules)
			{
				ServiceDropEdit.Visible = false;
				SubModuleDropEdit.Visible = false;
			}
		}
	}
}