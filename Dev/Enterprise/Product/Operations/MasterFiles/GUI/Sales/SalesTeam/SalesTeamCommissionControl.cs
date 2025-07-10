using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesTeamCommissionControl : ZUserControl
	{
		public SalesTeamCommissionControl()
		{
			InitializeComponent();

			if (!CommissionLookups.ShouldShowServicesAndSubModules)
			{
				commissionRulesGrid.RemoveFromAvailableColumns(AccCommissionRule.Schema.ACM_Service, AccCommissionRule.Schema.ACM_SubModule);
			}
		}
	}
}
