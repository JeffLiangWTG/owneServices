using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace CargoWise.eServices.Authentication.WebService
{
    public class SkipCacheResponseAttribute : Attribute { }

    public class CacheResponseAttribute : ActionFilterAttribute
	{
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
			loggerID = Guid.NewGuid();
			Logger.Debug($"{loggerID}: New Request");
            ResultCache cache;
            if (actionContext.ActionDescriptor != null)
            {
                if (actionContext.ActionDescriptor.GetCustomAttributes<SkipCacheResponseAttribute>().Any()) return;
            }
        
            var isValidating = actionContext.RequestContext.RouteData.Values["Action"].ToString().StartsWith("Validate");
			var actionArgument = actionContext.ActionArguments.First();
			var param = actionArgument.Value.GetType().IsArray ? new List<string>(actionArgument.Value as string[]) : new List<string>() { actionArgument.Value as string };
			var actionRequest = string.Empty;
			var paramLength = isValidating ? param.Count - 1 : param.Count;

			for (int i = 0; i < paramLength; i++)
			{
				actionRequest += param[i];
			}

			var isCached = isValidating
				? validationCaches.TryGetValue(actionRequest, out cache)
				: existenceCaches.TryGetValue(actionRequest, out cache);

			if (!isCached)
			{
				Logger.Debug($"{loggerID}: {actionContext.RequestContext.RouteData.Values["Action"].ToString()} - {actionRequest}: Using Database");
				InvokeAuthenticationAction(actionContext);
			}
			else
			{
				if (cache.ExpirationTime > DateTimeWrapper.Now() || cache.LastCheck.AddSeconds(CheckInterval) > DateTimeWrapper.Now())
				{
					if ((isValidating && cache.Password == param[paramLength]) || !isValidating)
					{
						Logger.Debug($"{loggerID}: {actionRequest}: Using Cache");
						actionContext.Response = cache.Result;
					}
					else
					{
						Logger.Debug($"{loggerID}: {actionContext.RequestContext.RouteData.Values["Action"].ToString()} - {actionRequest}: Using Database");
						InvokeAuthenticationAction(actionContext);
					}
				}
				else
				{
					cache.LastCheck = DateTimeWrapper.Now();
					if(DatabaseHelper.IsDatabaseAlive() || cache.ExpirationTime.AddSeconds(CacheDurationOnDatabaseDown - CacheDuration) <= DateTimeWrapper.Now())
					{
						Logger.Debug($"{loggerID}: {actionContext.RequestContext.RouteData.Values["Action"].ToString()} - {actionRequest}: Using Database");
						InvokeAuthenticationAction(actionContext);
					}
					else
					{
						if ((isValidating && cache.Password == param[paramLength]) || !isValidating)
						{
							Logger.Debug($"{loggerID}: {actionRequest}: Using Cache");
							actionContext.Response = cache.Result;
						}
						else
						{
							Logger.Debug($"{loggerID}: {actionContext.RequestContext.RouteData.Values["Action"].ToString()} - {actionRequest}: Using Database");
							InvokeAuthenticationAction(actionContext);
						}
					}
				}
			}
		}

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			Logger.Debug($"{loggerID}: Saving Cache");
			if (actionExecutedContext.ActionContext.ActionDescriptor != null)
            {
                if (actionExecutedContext.ActionContext.ActionDescriptor.GetCustomAttributes<SkipCacheResponseAttribute>().Any()) return;
            }

            var isValidating = actionExecutedContext.ActionContext.RequestContext.RouteData.Values["Action"].ToString().StartsWith("Validate");
			var actionArgument = actionExecutedContext.ActionContext.ActionArguments.First();
			var param = actionArgument.Value.GetType().IsArray ? new List<string>(actionArgument.Value as string[]) : new List<string>() { actionArgument.Value as string };
			var actionRequest = string.Empty;
			var paramLength = isValidating ? param.Count - 1 : param.Count;

			for (int i = 0; i < paramLength; i++)
			{
				actionRequest += param[i];
			}

			if (isValidating)
			{
				validationCaches[actionRequest] = CacheResult(actionExecutedContext.Response, param[param.Count - 1]);
			}
			else
			{
				existenceCaches[actionRequest] = CacheResult(actionExecutedContext.Response);
			}
			Logger.Debug($"{loggerID}: {actionRequest}: Cache Saved");
		}

		ResultCache CacheResult(HttpResponseMessage result, string password = null)
		{
			return new ResultCache
			{
				Result = result,
				ExpirationTime = DateTimeWrapper.Now().AddSeconds(CacheDuration),
				Password = password,
				LastCheck = DateTimeWrapper.Now()
			};
		}

		public virtual void InvokeAuthenticationAction(HttpActionContext actionContext)
		{
			base.OnActionExecuting(actionContext);
		}

		public void ClearCaches()
		{
			existenceCaches.Clear();
			validationCaches.Clear();
		}

		public virtual IDateTime DateTimeWrapper => dateTime ?? (dateTime = new DateTimeWrapper());

		public virtual IDatabaseHelper DatabaseHelper => databaseHelper ?? (databaseHelper = new DatabaseHelper());

		DatabaseHelper databaseHelper;
		DateTimeWrapper dateTime = null;
		static readonly ConcurrentDictionary<string, ResultCache> existenceCaches = new ConcurrentDictionary<string, ResultCache>();
		static readonly ConcurrentDictionary<string, ResultCache> validationCaches = new ConcurrentDictionary<string, ResultCache>();
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CacheResponseAttribute).Name);
		private Guid loggerID;

		const int secondsOf24Hours = 60 * 60 * 24;
		const int secondsOf5Minutes = 300;

		private int CacheDurationOnDatabaseDown { get; } = int.TryParse(ConfigurationManager.AppSettings["CargoWise.eServices.Authentication.WebService.CacheDurationOnDatabaseDown"], out int duration) ? duration : secondsOf24Hours;
		private int CacheDuration { get; } = int.TryParse(ConfigurationManager.AppSettings["CargoWise.eServices.Authentication.WebService.CacheDuration"], out int duration) ? duration : secondsOf5Minutes;
		private int CheckInterval { get; } = 60;
	}
}