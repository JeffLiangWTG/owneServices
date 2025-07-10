using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccCommissionRulePreviewPane : ZUserControl
	{
		public AccCommissionRulePreviewPane()
		{
			InitializeComponent();

			if (!CommissionLookups.ShouldShowServicesAndSubModules)
			{
				ACM_ServiceDropEdit.Visible = false;
				ACM_SubModuleDropEdit.Visible = false;
			}
		}
	}
}