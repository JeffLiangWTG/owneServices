using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Prepare SSO Url for Android Glow Application")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public GlowNavigationWebServiceResponse PrepareGlowNavigationUrl(GlowModule module)
		{
			return HandleWebServiceRequest<GlowNavigationWebServiceResponse>(r => PrepareGlowNavigationUrl(r, module));
		}

		void PrepareGlowNavigationUrl(GlowNavigationWebServiceResponse response, GlowModule module)
		{
			string endpoint;

			switch (module)
			{
				case GlowModule.CycleCount:
					endpoint = StartCycleCountEndpoint;
					break;
				case GlowModule.Load:
					endpoint = StartLoadEndpoint;
					break;
				case GlowModule.PackingConsolidation:
					endpoint = StartPackingConsolidationEndpoint;
					break;
				case GlowModule.Service:
					endpoint = StartServiceEndpoint;
					break;
				default:
					throw new ArgumentException("Module not valid");
			}

			response.GlowSSONavigationUrl = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl(endpoint).ToString();
		}

		const string StartCycleCountEndpoint = "goto/PerformCycleCount";
		const string StartLoadEndpoint = "goto/StartLoad";
		const string StartPackingConsolidationEndpoint = "goto/StartPackingConsolidation";
		const string StartServiceEndpoint = "goto/RFService";

		public enum GlowModule
		{
			CycleCount,
			Load,
			PackingConsolidation,
			Service
		}
	}
}
