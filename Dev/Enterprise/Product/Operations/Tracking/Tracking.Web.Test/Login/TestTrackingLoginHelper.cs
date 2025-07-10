using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TestTrackingLoginHelper : TrackingLoginHelper
	{
		public TestTrackingLoginHelper(ZPage page) : base(page)
		{
		}

		public TestTrackingLoginHelper(ZPage page, bool isAutoMaticallyHookupOnLoad) : base(page, isAutoMaticallyHookupOnLoad)
		{
		}

		public LoginManager LoginManForTesting => LoginMan;

		public string DefaultUrlExposed
		{
			get { return DefaultUrl; }
		}

		public void SetupLoginDataFromParamsForTest()
		{
			SetUpLoginDataFromParams();
		}
	}
}
