using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class OrgLocatedWithinModuleFilterControl : ZUserControl
	{
		public OrgLocatedWithinModuleFilterControl()
		{
			InitializeComponent();
		}

		void FilterTypeDropEdit_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (FilterTypeDropEdit.CodeBox.Text == OrgLocatedWithinModuleFilter.SearchTypeUNLOCO)
			{
				OrgzGuidFindBoxWithSelectedEvent.Visible = false;
				AddressPKzDropEdit.Visible = false;

				UNLOCOzCodeFindBox.Visible = true;
			}
			else if (FilterTypeDropEdit.CodeBox.Text == OrgLocatedWithinModuleFilter.SearchTypeOrgAddress)
			{
				UNLOCOzCodeFindBox.Visible = false;

				OrgzGuidFindBoxWithSelectedEvent.Visible = true;
				AddressPKzDropEdit.Visible = true;
			}
			else
			{
				OrgzGuidFindBoxWithSelectedEvent.Visible = false;
				AddressPKzDropEdit.Visible = false;
				UNLOCOzCodeFindBox.Visible = false;
			}
		}
	}
}
