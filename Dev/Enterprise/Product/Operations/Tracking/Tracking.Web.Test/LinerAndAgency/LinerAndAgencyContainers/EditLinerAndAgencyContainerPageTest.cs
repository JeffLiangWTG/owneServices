using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	sealed class EditLinerAndAgencyContainerPageTest : ZPageTestCase
	{
		protected override ZPage GetNewZPage()
		{
			return new EditLinerAndAgencyContainerForTest();
		}
	}
}
