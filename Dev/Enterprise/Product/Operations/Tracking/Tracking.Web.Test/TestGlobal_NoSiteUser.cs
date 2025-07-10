using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TestGlobal_NoSiteUser : TestGlobal
	{
		public override WebUser SiteUser => null;
	}
}
