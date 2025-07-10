using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TrackingPasswordResetHelperForTest : TrackingPasswordResetHelper
	{
		public TrackingPasswordResetHelperForTest(BusinessObjectFactory factory, string email) : base(factory,
			email)
		{
		}

		public TrackingPasswordResetHelperForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		public OrgContact DefaultOrgContactExposed => DefaultOrgContact;
	}
}
