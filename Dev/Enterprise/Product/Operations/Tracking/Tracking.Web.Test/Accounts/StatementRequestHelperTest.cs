using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web
{
	[TestedType(typeof(StatementRequestHelper))]
	sealed class StatementRequestHelperTest : DataRequestHelperTestCase
	{
		protected override DataRequestHelper GetRequestHelper()
		{
			return new StatementRequestHelper();
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
			get { return "StatementRequestHandler.axd"; }
		}
	}
}
