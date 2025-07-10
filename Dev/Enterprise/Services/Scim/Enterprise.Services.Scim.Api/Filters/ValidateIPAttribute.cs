using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Enterprise.Registry.Business;
using Enterprise.Services.Scim.Api.Config;
using Enterprise.Services.Scim.Api.Helpers;
using static Enterprise.Registry.Business.ScimConstants;

namespace Enterprise.Services.Scim.Api.Filters
{
	public sealed class ValidateIPAttribute : ActionFilterAttribute
	{
		readonly ISafelistValidator Validator;
		readonly IAppSettings AppSettings;

		public ValidateIPAttribute(ISafelistValidator validator, IAppSettings appSettings)
		{
			Validator = validator;
			AppSettings = appSettings;
		}

		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			if (actionContext.Request.RequestUri.PathAndQuery.Contains("/wtg/status")
				|| SystemDataRegistry.Instance.ScimSafeListType.Value.Equals(SafelistTypes.None))
			{
				return;
			}

			var allowedIps = AppSettings.SafelistSkippedIps.Split(',');
			string ip;

			if (actionContext.Request.Headers.Contains("X-Forwarded-For"))
			{
				var headerValues = actionContext.Request.Headers.TryGetValues("X-Forwarded-For", out var values) ? string.Join(",", values)	: "";

				var allIps = headerValues
					.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(ip => ip.Trim())
					.ToList();

				ip = allIps.LastOrDefault();
			}
			else
			{
				ip = actionContext.Request.GetOwinContext().Request.LocalIpAddress;
			}

			if (string.IsNullOrEmpty(ip) || !Validator.ValidateIp(ip, allowedIps))
			{
				actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden);
			}
		}
	}
}
