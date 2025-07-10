using System.Windows.Forms;

using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI
{
	public interface ITemporaryOrganisationFindBox : IFindBox, IFindBoxUserControl
	{
		void ShowTemporaryOrgPopup(OrganisationEmdeddedModulePopup parentFindBoxPopup);
		Form ParentForm { get; }
		IOrgHeaderCollection List { get; }
	}
}