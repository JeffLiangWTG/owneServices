using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CargoWise.Data;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.Services.ServiceHost
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
	public sealed class ReportServiceErrorHandlerAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			var service = GetReportDataService(actionContext);
			service?.ClearRunningError();

			base.OnActionExecuting(actionContext);
		}

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			base.OnActionExecuted(actionExecutedContext);

			var service = GetReportDataService(actionExecutedContext.ActionContext);
			if (service?.RunningError?.Errors.Count > 0)
			{
				HttpStatusCode httpStatusCode;
				switch (service.RunningError.ErrorType)
				{
					case ReportServiceErrorType.ValidationError:
						httpStatusCode = HttpStatusCode.BadRequest;
						break;
					case ReportServiceErrorType.Unauthorized:
						httpStatusCode = HttpStatusCode.Forbidden;
						break;
					default:
						httpStatusCode = HttpStatusCode.InternalServerError;
						break;
				}
				var responseMessage = actionExecutedContext.Request.CreateResponse(httpStatusCode, service.RunningError);
				actionExecutedContext.Response = responseMessage;
			}
		}

		IReportDataService GetReportDataService(HttpActionContext httpActionContext)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (httpActionContext?.ControllerContext?.Controller is ReportDataBaseController controller)
				{
					return controller.Service;
				}
			}
			return null;
		}
	}
}
