using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ContainerSummaryPageTest : ZPageTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "need to be checked by code owner")]
		ContainerSummary TestPage
		{
			get { return Page as ContainerSummary; }
		}

		protected override ZPage GetNewZPage()
		{
			return new ContainerSummaryForTest();
		}
	}
}
