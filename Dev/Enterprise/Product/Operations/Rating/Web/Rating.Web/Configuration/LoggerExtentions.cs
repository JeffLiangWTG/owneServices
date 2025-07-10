using System.Collections.Generic;
using Enterprise.Rating.Business;
using Enterprise.Rating.Web.Model;

namespace Enterprise.Rating.Web.Configuration
{
	static class LoggerExtentions
	{
		public static void PushLogs(this ApiResponse response, SourceEndpoint sourceEndpoint, ILoggerExtended logger)
		{
			var logsToPush = new List<string>();
			logsToPush.AddRange(ExtraValidationLogger.GetExtraValidationMessages());
			ExtraValidationLogger.RemoveAllLogs();

			if (sourceEndpoint == SourceEndpoint.JobCharges)
			{
				logsToPush.AddRange(logger.GetAllLogs());
			}
			else
			{
				logsToPush.AddRange(logger.GetErrorsAndWarnings());
			}

			response.Warnings = logsToPush.ToArray();
		}
	}
}
