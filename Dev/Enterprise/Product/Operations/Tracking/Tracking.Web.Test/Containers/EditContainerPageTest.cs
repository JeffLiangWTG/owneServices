using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class EditContainerPageTest : ZPageTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "need to be checked by code owner")]
		EditContainer TestPage
		{
			get { return Page as EditContainer; }
		}

		protected override ZPage GetNewZPage()
		{
			return new EditContainerForTest();
		}
	}
}
