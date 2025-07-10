using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.Business
{
	public static class ServiceTaskHelper
	{
		public static void LogTaskStarting(ILogger logger, ZString serviceTaskCode, ZString serviceTaskDescription)
		{
			logger.Debug($@"Starting service task {serviceTaskCode} ({serviceTaskDescription})");
		}

		public static void LogTaskFinished(ILogger logger, ZString serviceTaskCode, ZString serviceTaskDescription)
		{
			logger.Debug($@"Finished service task {serviceTaskCode} ({serviceTaskDescription})");
		}

		public static void NudgeServiceTaskDelay(ILogger logger, ZString serviceTaskCode, TimeSpan nudgeDelay, bool ignoreNudgeIfInPast = false)
		{
			var datetimeNow = ZDateTime.UtcNow;
			var nudgedRunTime = datetimeNow + nudgeDelay;
			if (nudgeDelay > TimeSpan.Zero)
			{
				ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(serviceTaskCode, nudgeDelay);
				logger?.Log(Integration.LogType.Debug, string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} task nudged to start at {1}", serviceTaskCode, nudgedRunTime));
			}
			else
			{
				if (ignoreNudgeIfInPast)
				{
					logger?.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} task ignored nudge request at {1} as the time is in the past", serviceTaskCode, nudgedRunTime));
				}
				else // Nudge now
				{
					ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(serviceTaskCode);
					logger?.Log(Integration.LogType.Debug, string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} task nudged", datetimeNow));
				}
			}
		}

		public static void NudgeServiceTaskTime(ILogger logger, ZString serviceTaskCode, ZDateTime nudgedRunTime, bool ignoreNudgeIfInPast = false)
		{
			var nudgeDelay = nudgedRunTime - ZDateTime.UtcNow;
			NudgeServiceTaskDelay(logger, serviceTaskCode, nudgeDelay, ignoreNudgeIfInPast);
		}
	}
}
