using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;
using Enterprise.ZArchitecture.Core;
using Microsoft.Owin;

namespace Enterprise.Rating.Web.Configuration
{
	public class OwinPipelineExceptionHandler : OwinMiddleware
	{
		public OwinPipelineExceptionHandler(OwinMiddleware next, IRatesAPIsAppSettings configurations) : base(next)
		{
			this.configurations = configurations;
		}

		readonly IRatesAPIsAppSettings configurations;

		public override async Task Invoke(IOwinContext context)
		{
			try
			{
				await Next.Invoke(context);
			}
			catch (Exception ex)
			{
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
				var message = string.Empty;

				if (ex.FlattenInnerExceptions().Any((Exception e) => e is DatabaseUpgradeException))
				{
					await WebUpgradeManager.NotifyUpgradeRequired();

					// Same as EnterpriseWebApiExceptionHandler.DatabaseUpgradeResponse
					context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
					message = (NoResString)"Upgrade in progress"; // API response message
				}
				else if (ex.FlattenInnerExceptions().Any(e => e is InvalidBranchException))
				{
					context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
					message = ex.FlattenInnerExceptions().First(e => e is InvalidBranchException).Message;
				}
				else if (ex.FlattenInnerExceptions().Any(e => e is InvalidDepartmentException))
				{
					context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
					message = ex.FlattenInnerExceptions().First(e => e is InvalidDepartmentException).Message;
				}
				else
				{
					ErrorReporter.ReportOnce("RatesAPIs OwinPipelineExceptionHandler", ex); // Internal Error Reporting

					if (configurations.ShowExceptionDetailsInResponse)
					{
						message = string.Join(" -> ", ex.FlattenInnerExceptions().Select(e => e.Message)); // API response message
					}
					else
					{
						message = (NoResString)"An internal exception has occured ... "; // API response message
					}
				}

				context.Response.ContentLength = Encoding.UTF8.GetByteCount(message);
				context.Response.Write(message);
			}
		}
	}
}
