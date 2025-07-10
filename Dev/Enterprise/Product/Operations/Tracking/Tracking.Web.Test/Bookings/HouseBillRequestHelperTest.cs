using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(HouseBillRequestHelper))]
	sealed class HouseBillRequestHelperTest : DataRequestHelperTestCase
	{
		protected override DataRequestHelper GetRequestHelper() => new HouseBillRequestHelper();

		protected override bool ExpectedEnableCache => false;

		protected override bool ExpectedUseSecureQueryString => true;

		protected override string ExpectedBaseUrl => "HouseBillRequestHandler.axd";
	}
}
