using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Enterprise.Registry.Business;

namespace Enterprise.Services.Scim.Api.Filters
{
	public sealed class ScimEnabledAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			if (actionContext == null)
			{
				throw new ArgumentNullException(nameof(actionContext));
			}

			if (!SystemDataRegistry.Instance.EnableScimService.Value)
			{
				throw new InvalidOperationException($"The SCIM function is not enabled. Please check Registry : {SystemDataRegistry.Instance.EnableScimService.GetLocation()}");
			}
		}
	}
}
