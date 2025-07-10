using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Services.Scim.Api.Config
{
	class ScimServiceAPIGlobalExceptionHandler : EnterpriseWebApiExceptionHandler
	{
		public ScimServiceAPIGlobalExceptionHandler(ServiceProvider serviceProvider)
		{
			configurations = serviceProvider.GetRequiredService<IAppSettings>();
		}

		readonly IAppSettings configurations;

		public override void Handle(ExceptionHandlerContext context)
		{
			base.Handle(context);

			if (!context.Exception.FlattenInnerExceptions().Any(e => e is DatabaseUpgradeException))
			{
				context.Result = new GeneralExceptionResult(context.Exception, configurations);
			}
			else
			{
				WebUpgradeManager.NotifyUpgradeRequired();
			}
		}

		public class GeneralExceptionResult : IHttpActionResult
		{
			public GeneralExceptionResult(Exception exception, IAppSettings configurations)
			{
				this.exception = exception;
				this.configurations = configurations;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Auto generated baseline suppressions - WI00637629")]
			readonly Exception exception;
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Auto generated baseline suppressions - WI00637629")]
			readonly IAppSettings configurations;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "API response message")]
			public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
			{
				var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
				response.Content = new StringContent("An internal exception has occured ... ");

				return Task.FromResult(response);
			}
		}
	}
}
