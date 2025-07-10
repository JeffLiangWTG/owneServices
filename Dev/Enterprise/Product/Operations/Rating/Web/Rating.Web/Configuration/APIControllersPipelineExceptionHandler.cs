using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;

namespace Enterprise.Rating.Web.Configuration
{
	class APIControllersPipelineExceptionHandler : EnterpriseWebApiExceptionHandler
	{
		public APIControllersPipelineExceptionHandler()
		{
			this.configurations = ObjectFactory.New<IRatesAPIsAppSettings>();
		}

		readonly IRatesAPIsAppSettings configurations;

		public override void Handle(ExceptionHandlerContext context)
		{
			if (context.Exception is InvalidDepartmentException || context.Exception is InvalidBranchException)
			{
				context.Result = new InvalidSessionResult(context.Exception.Message);
			}
			else
			{
				base.Handle(context);

				if (!(context.Result is DatabaseUpgradeResponse))
				{
					context.Result = new GeneralExceptionResult(context.Exception, configurations);
				}
			}
		}

		class GeneralExceptionResult : IHttpActionResult
		{
			public GeneralExceptionResult(Exception exception, IRatesAPIsAppSettings configurations)
			{
				this.exception = exception;
				this.configurations = configurations;
			}

			readonly Exception exception;
			readonly IRatesAPIsAppSettings configurations;

			public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
			{
				var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);

				if (configurations.ShowExceptionDetailsInResponse)
				{
					response.Content = new StringContent(string.Join(" -> ", exception.FlattenInnerExceptions().Select(e => e.Message))); // API response message
				}
				else
				{
					response.Content = new StringContent((NoResString)"An internal exception has occured ... "); // API response message
				}

				return Task.FromResult(response);
			}
		}

		class InvalidSessionResult : IHttpActionResult
		{
			readonly string message;

			public InvalidSessionResult(string message)
			{
				this.message = message;
			}

			public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
			{
				var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
				response.Content = new StringContent(message);
				return Task.FromResult(response);
			}
		}
	}
}
