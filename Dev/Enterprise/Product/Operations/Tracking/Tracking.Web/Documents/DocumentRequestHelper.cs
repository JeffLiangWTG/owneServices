using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Web.Business.Utilities;

namespace Enterprise.Tracking.Web
{
	public class DocumentRequestHelper : DataRequestHelper
	{
		public override string BaseUrl
		{
			get { return "DocumentRequestHandler.axd"; }
		}

		public override bool EnableCache
		{
			get { return false; }
		}

		public override bool UseSecureQueryString
		{
			get { return false; }
		}

		public string GetHandlerUrl(Type documentSupportableBizOCreatorType, string contentType, ZGuid[] pKs)
		{
			string url = base.GetHandlerUrl(pKs);

			url = string.Format("{0}{1}{2}={3}&{4}={5}",
													url,
													url.Contains("?") ? "&" : ":",
													TrackingConstants.QueryStringKeys.Helper,
													new QueryParamsEncoder().Encrypt(documentSupportableBizOCreatorType.AssemblyQualifiedName),
													TrackingConstants.QueryStringKeys.ContentType,
													contentType);

			return url;
		}
	}
}
