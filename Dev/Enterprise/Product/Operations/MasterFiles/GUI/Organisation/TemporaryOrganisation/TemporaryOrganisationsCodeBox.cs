using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI
{
	abstract class TemporaryOrganisationsCodeBox : ZFindBoxUserControl.ZCodeBox
	{
		protected TemporaryOrganisationsCodeBox(ITemporaryOrganisationFindBox findBox) : base(findBox)
		{
			Hotkeys.RegisterHotKey(Keys.F5, ShowTemporaryOrganisationForm, Res.GetString("4285C9AF-54FC-4947-B26D-209CA7DDED31", "New Temporary Organization Form"));
		}

		bool ShowTemporaryOrganisationForm(object sender, Keys keyData)
		{
			OrganisationFindBox.ShowTemporaryOrgPopup(null);
			return true;
		}

		protected ITemporaryOrganisationFindBox OrganisationFindBox
		{
			get { return ParentFindBox as ITemporaryOrganisationFindBox; }
		}
	}
}
