using System;
#if NETFRAMEWORK
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
#else
using Microsoft.AspNetCore.Mvc;
#endif
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.ServiceHost
{
	public static class GlowUserContextSwitcher
	{
#if NETFRAMEWORK
		public static IDisposable SetUserContextBasedOnHttp(HttpControllerContext controllerContext)
		{
			var controller = controllerContext?.Controller as ApiController;
			if (controller?.User?.Identity is not GlowAuthenticationTicketIdentity identity)
			{
				return null;
			}
#else
		public static IDisposable SetUserContextBasedOnHttp(ControllerContext controllerContext)
		{
			var user = controllerContext?.HttpContext?.User;
			if (user?.Identity is not GlowAuthenticationTicketIdentity identity)
			{
				return null;
			}
#endif

			var userContext = identity.ProviderType switch
			{
				GlbStaffSchema.Constants.Prefix => new Environment.UserContext(identity.ProviderKey, identity.BranchKey, identity.DepartmentKey),
				OrgContactSchema.Constants.Prefix => new Environment.UserContext(User.WebUserName, identity.BranchKey, identity.DepartmentKey),
#if NETFRAMEWORK
				_ => throw new HttpResponseException(HttpStatusCode.Forbidden)
#else
				_ => throw new UnauthorizedAccessException()
#endif
			};

			var suppressionOrder = Env.Instance.SuppressSwitchContextCheck();
			return new DisposableList([Env.SetTemporaryUserContext(userContext), suppressionOrder]);
		}
	}
}
