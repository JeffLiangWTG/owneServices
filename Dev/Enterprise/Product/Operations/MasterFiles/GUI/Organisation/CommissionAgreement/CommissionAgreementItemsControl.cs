using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CommissionAgreementItemsControl : ZUserControl
	{
		public CommissionAgreementItemsControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				if (!CommissionLookups.ShouldShowServicesAndSubModules)
				{
					ServicesGrid.Visible = false;
					SubModulesGrid.Visible = false;
					tableLayoutPanel.ColumnStyles[1].Width = 0;
					tableLayoutPanel.ColumnStyles[2].Width = 0;
				}
				else
				{
					tableLayoutPanel.ColumnStyles.RemoveAt(3);
					ConditionsGrid.Visible = false;
				}
			}
		}
	}
}
