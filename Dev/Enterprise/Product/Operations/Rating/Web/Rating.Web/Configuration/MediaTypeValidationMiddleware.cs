using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Enterprise.ZArchitecture.Core;
using Microsoft.Owin;

namespace Enterprise.Rating.Web.Configuration
{
	class MediaTypeValidationMiddleware : OwinMiddleware
	{
		public MediaTypeValidationMiddleware(OwinMiddleware next, IRatesAPIsAppSettings configurations) : base(next)
		{
			this.configurations = configurations;
		}

		readonly IRatesAPIsAppSettings configurations;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Key value")]
		public async override Task Invoke(IOwinContext context)
		{
			var urlsToCheck = new[]
			{
				"api/rating/costing",
				"api/rating/companytariffs",
				"api/rating/intercompanytariffs",
				"api/rating/clientrates",
				"api/rating/jobcharges",
			};

			if (urlsToCheck.Any(url => context.Request.Uri.OriginalString.ToLowerInvariant().Contains(url)))
			{
				var supportedMediaTypes = new[] { "*/*", "application/xml", "application/json" };

				if (!configurations.SupportJsonMediaType)
				{
					supportedMediaTypes = new[] { "*/*", "application/xml" };
				}

				if (context.Request.Headers.ContainsKey("Accept") && !supportedMediaTypes.Contains(context.Request.Headers["Accept"], StringComparer.InvariantCultureIgnoreCase))
				{
					context.Response.StatusCode = (int)HttpStatusCode.NotAcceptable;
					context.Response.ReasonPhrase = (NoResString)"Acceptable media types are: " + string.Join(", ", supportedMediaTypes.Skip(1));
					return;
				}
			}

			await this.Next.Invoke(context);
		}
	}
}
