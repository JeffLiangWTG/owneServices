using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	public abstract class TrackingWebControlTest : WebControlTest
	{
		protected override ZTestPage GetNewZTestPage()
		{
			return new TrackingTestPage();
		}
	}
}
