using System.Diagnostics;
using CargoWise.RefDbRepo.Common.Utils;
using Common.Logging;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CargoWise.RefDbRepo.Common.Web
{
	public sealed class TrackDurationAttribute : ActionFilterAttribute
	{
		public ILogWrapper logWrapper { get; }
		readonly ILog logger;
		Stopwatch stopwatch;

		public TrackDurationAttribute(ILogWrapper logWrapper)
		{
			this.logWrapper = logWrapper;
			logger = logWrapper.GetLog<TrackDurationAttribute>();
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			stopwatch = Stopwatch.StartNew();
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			stopwatch.Stop();

			var controllerName = context.ActionDescriptor.RouteValues["controller"];
			var actionName = context.ActionDescriptor.RouteValues["action"];

			var apiDurationLog = new ApiDurationLog
			{
				Id = $"{controllerName}.{actionName}",
				Duration = stopwatch.ElapsedMilliseconds,
			};

			logger.Info(apiDurationLog);
		}
	}
}
