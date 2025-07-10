using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	[GlowTicketAuthentication]
	public class WebTrackerLogonController : ApiController
	{
		[Route("api/WebTrackerLogon/{moduleID}/{recordID?}")]
		public HttpResponseMessage GetUrl(string moduleID, string recordID = null)
		{
			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var contactPK = GetContactPK(User?.Identity as IGlowAuthenticationTicketIdentity);
				var rootUrl = GetRootUrl();
				var businessContext = GetBusinessContext(moduleID);
				var contextPK = GetBusinessContextPK(recordID);

				var webTrackerUrl = contextPK == Guid.Empty ? TrackingUrlBuilder.BuildUrl(rootUrl, contactPK, businessContext, recordID, true) : TrackingUrlBuilder.BuildUrl(rootUrl, contactPK, businessContext, contextPK, true);

				return new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(webTrackerUrl)
				};
			}
		}

		string GetRootUrl()
		{
			return WebDataRegistry.Instance.WebTrackerUrl.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
		}

		static TrackingConstants.BusinessContext GetBusinessContext(string moduleID)
		{
			if (!string.IsNullOrEmpty(moduleID) && Enum.TryParse<TrackingConstants.BusinessContext>(moduleID, true, out var context))
			{
				return context;
			}

			return TrackingConstants.BusinessContext.NoBusinessContext;
		}

		static Guid GetBusinessContextPK(string recordID)
		{
			if (Guid.TryParse(recordID, out var contextPK))
			{
				return contextPK;
			}

			return Guid.Empty;
		}

		static Guid GetContactPK(IGlowAuthenticationTicketIdentity identity)
		{
			if (identity != null && identity.IsAuthenticated && identity.ProviderType == OrgContactSchema.Constants.Prefix)
			{
				return identity.ProviderKey;
			}

			return Guid.Empty;
		}
	}
}
