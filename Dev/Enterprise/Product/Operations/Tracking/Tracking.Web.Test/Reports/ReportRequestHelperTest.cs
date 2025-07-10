using System;
using System.Web;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ReportRequestHelper))]
	sealed class ReportRequestHelperTest : DataRequestHelperTestCase
	{
		public void TestGetHandlerUrl()
		{
			ReportRequestHelper requestHelper = new ReportRequestHelper();

			ZGuid reportIndex = ZGuid.NewZGuid();

			string actualHandlerUrl = requestHelper.GetHandlerUrl(reportIndex);
			string expectedHandlerUrl = String.Format("{0}?{1}={2}", requestHelper.BaseUrl, DataRequestHelper.DataKey,
				HttpUtility.UrlEncode(reportIndex.ToString()));

			AssertEquals("The handler URL must contain the report index.", expectedHandlerUrl, actualHandlerUrl);
		}

		protected override DataRequestHelper GetRequestHelper()
		{
			return new ReportRequestHelper();
		}

		protected override bool ExpectedEnableCache
		{
			get { return false; }
		}

		protected override bool ExpectedUseSecureQueryString
		{
			get { return false; }
		}

		protected override string ExpectedBaseUrl
		{
			get { return "ReportRequestHandler.axd"; }
		}
	}
}
