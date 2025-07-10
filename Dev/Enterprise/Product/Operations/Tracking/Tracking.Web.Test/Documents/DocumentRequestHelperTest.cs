using System;
using System.Web;
using CargoWise.Types;
using Enterprise.DocumentEngine.GUI;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(DocumentRequestHelper))]
	sealed class DocumentRequestHelperTest : DataRequestHelperTestCase
	{
		public void TestGetHandlerUrl()
		{
			ZGuid docomentSupportableIndex = ZGuid.NewZGuid();
			ZGuid documentCommandPK = ZGuid.NewZGuid();

			string actualHandlerUrl = RequestHelper.GetHandlerUrl(new[] { docomentSupportableIndex, documentCommandPK });
			string expectedHandlerUrl = String.Format("{0}?{1}={2},{3}",
				RequestHelper.BaseUrl,
				DataRequestHelper.DataKey,
				HttpUtility.UrlEncode(docomentSupportableIndex.ToString()),
				HttpUtility.UrlEncode(documentCommandPK.ToString()));

			AssertEquals("The handler URL must contain the Document index.", expectedHandlerUrl, actualHandlerUrl);

			Type helperType = typeof(DocumentsMenuHelper);
			string url = RequestHelper.GetHandlerUrl(new[] { docomentSupportableIndex, documentCommandPK });

			actualHandlerUrl = ((DocumentRequestHelper)RequestHelper).GetHandlerUrl(helperType, DataContentTypes.Pdf, new[] { docomentSupportableIndex, documentCommandPK });
			expectedHandlerUrl = string.Format("{0}{1}{2}={3}&{4}={5}",
				url,
				url.Contains("?") ? "&" : ":",
				TrackingConstants.QueryStringKeys.Helper,
				new QueryParamsEncoder().Encrypt(helperType.AssemblyQualifiedName),
				TrackingConstants.QueryStringKeys.ContentType,
				DataContentTypes.Pdf);

			AssertEquals("The handler url does not match specified format", expectedHandlerUrl, actualHandlerUrl);
		}

		protected override DataRequestHelper GetRequestHelper()
		{
			return new DocumentRequestHelper();
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
			get { return "DocumentRequestHandler.axd"; }
		}
	}
}
