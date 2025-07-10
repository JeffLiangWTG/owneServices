using System.Globalization;
using Enterprise.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	class WhsDocketILoggerWrapper : IWhsDocketCreationLogger
	{
		public WhsDocketILoggerWrapper(ILogger logger)
		{
			Logger = logger;
		}

		ILogger Logger { get; }

		void IWhsDocketCreationLogger.LogFailure(string message)
		{
			Logger.Error(message);
		}

		void IWhsDocketCreationLogger.LogSuccess(string message)
		{
			Logger.Information(message);
		}

		void IWhsDocketCreationLogger.LogWarning(string message)
		{
			Logger.Warning(message);
		}

		public void LogHyperLinkSuccess(WhsDocket docket, string message)
		{
			Logger.Information(string.Format(CultureInfo.InvariantCulture, message, docket.WD_DocketID));
		}
	}
}
