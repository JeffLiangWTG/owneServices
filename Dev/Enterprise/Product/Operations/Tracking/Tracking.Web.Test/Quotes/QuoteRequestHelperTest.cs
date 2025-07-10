using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(QuoteRequestHelper))]
	sealed class QuoteRequestHelperTest : DataRequestHelperTestCase
	{
		protected override DataRequestHelper GetRequestHelper()
		{
			return new QuoteRequestHelper();
		}

		protected override bool ExpectedEnableCache
		{
			get { return true; }
		}

		protected override bool ExpectedUseSecureQueryString
		{
			get { return true; }
		}

		protected override string ExpectedBaseUrl
		{
			get { return "QuoteRequestHandler.axd"; }
		}
	}
}
