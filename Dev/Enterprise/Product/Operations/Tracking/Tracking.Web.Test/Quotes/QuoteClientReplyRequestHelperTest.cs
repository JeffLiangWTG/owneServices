using System.Net;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(QuoteClientReplyRequestHelper))]
	sealed class QuoteClientReplyRequestHelperTest : DataRequestHelperTestCase
	{
		protected override DataRequestHelper GetRequestHelper()
		{
			return new QuoteClientReplyRequestHelper();
		}

		protected override bool ExpectedEnableCache
		{
			get { return false; }
		}

		protected override bool ExpectedUseSecureQueryString
		{
			get { return true; }
		}

		protected override string ExpectedBaseUrl
		{
			get { return "QuoteClientReplyRequestHandler.axd"; }
		}

		public void TestGetUrlHandler_ClientAccepts()
		{
			var helper = new QuoteClientReplyRequestHelper();
			var url = helper.GetHandlerUrlForClientAcceptance(ZGuid.BrettsGuid);
			var urlparameter = url.Split('=')[1];
			var decoded = WebUtility.UrlDecode(urlparameter);
			var secureQuery = new SecureQueryString(decoded);

			AssertEquals(nameof(QuoteClientReplyRequestHandler.ClientReply.AcceptQuotation), secureQuery.Get(nameof(QuoteClientReplyRequestHandler.ClientReply)));
		}

		public void TestGetUrlHandler_ClientNotAccepts()
		{
			var helper = new QuoteClientReplyRequestHelper();
			var url = helper.GetHandlerUrlForClientNonAcceptance(ZGuid.BrettsGuid);
			var urlparameter = url.Split('=')[1];
			var decoded = WebUtility.UrlDecode(urlparameter);
			var secureQuery = new SecureQueryString(decoded);

			AssertEquals(nameof(QuoteClientReplyRequestHandler.ClientReply.RequestFurtherDiscussion), secureQuery.Get(nameof(QuoteClientReplyRequestHandler.ClientReply)));
		}
	}
}
