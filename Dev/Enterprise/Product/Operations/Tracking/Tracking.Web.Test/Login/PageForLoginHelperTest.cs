using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class PageForLoginHelperTest : BasePage
	{
		protected override BusinessObject GetNewDataSource() => new TrackingLoginManager();

		protected override ZGlobal GetNewTestGlobal() => new GlobalForLoginHelperTest();
	}
}
