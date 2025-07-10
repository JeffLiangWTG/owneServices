using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ContainerDetailsPageTest : ZPageTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "need to be checked by code owner")]
		ContainerDetails TestPage
		{
			get { return Page as ContainerDetails; }
		}

		protected override ZPage GetNewZPage()
		{
			return new ContainerDetailsForTest();
		}
	}
}
