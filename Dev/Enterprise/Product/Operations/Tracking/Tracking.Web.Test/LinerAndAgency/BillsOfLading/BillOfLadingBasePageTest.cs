using System.Collections.Generic;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	[HttpContextEnabledTest]
	abstract class BillOfLadingBasePageTest : BasePageWithAuthorisationTest
	{
		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebLinerAndAgencyBillsOfLadingModule; }
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerShippingManagerBillsOfLading }; }
		}
	}
}
