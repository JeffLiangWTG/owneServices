using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TrackingLoginRouterForTest : TrackingLoginRouter
	{
		public TrackingLoginRouterForTest(Uri originalUrl, string token) : base(originalUrl, token)
		{
		}

		public TrackingLoginRouterForTest(Uri originalUrl, OrgContact contact) : base(originalUrl, contact)
		{
		}
	}
}
