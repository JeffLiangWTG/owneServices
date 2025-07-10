using System.Collections.Generic;
using Enterprise.Integration.Licensing;

namespace Enterprise.Tracking.Web.Testing
{
	abstract class WarehousingPageBaseTest : BasePageWithAuthorisationTest
	{
		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerWarehouse }; }
		}
	}
}
