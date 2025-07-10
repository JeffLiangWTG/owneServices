using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web
{
	[TestedType(typeof(InvoiceRequestHelper))]
	sealed class InvoiceRequestHelperTest : DataRequestHelperTestCase
	{
		protected override DataRequestHelper GetRequestHelper()
		{
			return new InvoiceRequestHelper();
		}

		protected override bool ExpectedUseSecureQueryString
		{
			get { return true; }
		}

		protected override bool ExpectedEnableCache => false;

		protected override string ExpectedBaseUrl
		{
			get { return "InvoiceRequestHandler.axd"; }
		}
	}
}
