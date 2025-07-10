using System;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OrganisationEmdeddedModulePopup : EmbeddedModulePopup
	{
		public OrganisationEmdeddedModulePopup()
		{
			InitializeComponent();
		}

		public OrganisationEmdeddedModulePopup(ZFilterModule module)
			: base(module)
		{
			InitializeComponent();
		}

		void TemporaryButton_Click(object sender, EventArgs e)
		{
			((ITemporaryOrganisationFindBox)FindBox).ShowTemporaryOrgPopup(this);
		}
	}
}
