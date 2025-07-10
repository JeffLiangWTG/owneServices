using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using Common.Logging;

namespace CargoWise.RefDbRepo.NewService
{
	public class LogHelper : ILogHelper
	{
		public LogHelper(ILogWrapper logWrapper)
		{
			Argument.NotNull(logWrapper, nameof(logWrapper));
			log = logWrapper.GetLog<LogHelper>();
		}

		public void LogInfo(string userId, string message, string requestUri = null)
		{
			var apiLog = new ApiLogRequest
			{
				UserId = userId,
				Body = message,
				Uri = requestUri
			};
			log.Info(apiLog);
		}

		readonly ILog log;
	}
}
