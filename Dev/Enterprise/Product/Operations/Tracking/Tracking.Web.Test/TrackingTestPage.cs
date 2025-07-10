using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TrackingTestPage : ZTestPage
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}
	}
}
