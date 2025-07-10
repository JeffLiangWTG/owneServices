using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public sealed class StaffOnlyAuthorizationFilterAttribute : AuthorizationFilterAttribute
	{
		public override void OnAuthorization(HttpActionContext actionContext)
		{
			var isStaff = actionContext.RequestContext.Principal?.Identity is IGlowAuthenticationTicketIdentity identity && identity.ProviderType == GlbStaffSchema.Constants.Prefix;
			if (!isStaff)
			{
				actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden);
			}
		}
	}
}
