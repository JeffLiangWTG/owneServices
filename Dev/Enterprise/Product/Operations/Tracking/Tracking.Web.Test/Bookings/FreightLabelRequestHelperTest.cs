using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web
{
	[TestedType(typeof(FreightLabelRequestHelper))]
	sealed class FreightLabelRequestHelperTest : DataRequestHelperTestCase
	{
		protected override DataRequestHelper GetRequestHelper() => new FreightLabelRequestHelper();

		protected override bool ExpectedEnableCache => false;

		protected override bool ExpectedUseSecureQueryString => true;

		protected override string ExpectedBaseUrl => "FreightLabelRequestHandler.axd";
	}
}
