using log4net;
using WCFAppender_log4net.Interface;

namespace Log4NetWCFService
{
	public class WCFLogger : IWCFLogger
	{
		readonly ILog logger;

		public WCFLogger()
		{
			if (!LogManager.GetRepository().Configured)
			{
				log4net.Config.XmlConfigurator.Configure();
			}

			logger = LogManager.GetLogger(typeof(WCFLogger));
		}

		public void Append(LoggingEventWrapper[] logEvents)
		{
			foreach (var logEvent in logEvents)
			{
				logger.Logger.Log(logEvent.GetReconstructedLoggingEvent(logger.Logger.Repository));
			}
		}

		public void Append(string[] logEntries)
		{
			foreach (string logEvent in logEntries)
			{
				logger.Debug(logEntries);
			}
		}
	}
}
