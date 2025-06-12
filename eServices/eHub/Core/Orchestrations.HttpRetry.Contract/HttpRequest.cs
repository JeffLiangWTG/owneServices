using System;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract
{
	[Serializable]
	public class HttpRequest
	{
		public string Method { get; set; }

		public string AddressUrl { get; set; }

		public HttpHeader[] UserHeaders { get; set; }

		public string UserHeaderNamesWithoutValidation { get; set; }

		public string ContentType { get; set; }

		public XLANGMessage ContentMessage { get; set; }

		public OverridingStatusCodeConfig[] OverridingStatusCodeConfigs { get; set; }

		public OverridingExceptionConfig[] OverridingExceptionConfigs { get; set; }
	}
}