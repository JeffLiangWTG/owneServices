using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Types;
	using Enterprise.Integration;
	using MasterFiles.Business;

	public sealed class LoggerDecorator : IAutoRatingGUIInteractor
	{
		public struct LogInfo
		{
			public string Message { get; set; }
			public LogType Type { get; set; }
		}

		public LoggerDecorator(ILogger logger = null)
		{
			this.logger = logger;
			this.interactor = logger as IAutoRatingGUIInteractor;
			this.internalLog = new List<LogInfo>();
			this.debugEnabled = RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug.Value;
		}

		readonly ILogger logger;
		readonly IAutoRatingGUIInteractor interactor;
		List<LogInfo> internalLog;
		readonly bool debugEnabled;

		Dictionary<string, bool> YesNoWarningAnswersCache
		{
			get { return yesNoWarningAnswersCache ?? (yesNoWarningAnswersCache = new Dictionary<string, bool>()); }
		}
		Dictionary<string, bool> yesNoWarningAnswersCache;

		#region SuppressResourceStringsCheckRegion

		public bool YesNoWarning(string message)
		{
			if (YesNoWarningAnswersCache.TryGetValue(message, out var result))
			{
				return result;
			}

			result = interactor == null || interactor.YesNoWarning(message);
			YesNoWarningAnswersCache.Add(message, result);

			var resultMessage = result ? "Selected Yes" : "Selected No";
			message = string.Concat(message, System.Environment.NewLine, resultMessage);
			Log(LogType.Information, message);

			return result;
		}

		public void ShowException(Exception e)
		{
			Log(LogType.Error, "Autorating could not be completed.", e);
		}

		#endregion

		public void SuspendLayout()
		{
			interactor?.SuspendLayout();
		}

		public void ResumeLayout()
		{
			interactor?.ResumeLayout();
		}

		public void ShowPossibleMatchesDialog(AutoRater.AdditionalRatesNotifications notifications)
		{
			interactor?.ShowPossibleMatchesDialog(notifications);
		}

		public Quote SelectQuote(QuoteCollection quotes)
		{
			return interactor?.SelectQuote(quotes);
		}

		public ZDialogResult ShowNamedAccountMessageBox(string jobNamedAccount, string rateNamedAccount)
		{
			return interactor?.ShowNamedAccountMessageBox(jobNamedAccount, rateNamedAccount) ?? ZDialogResult.None;
		}

		public void SetJobInvoicingSecurityOverrideProvider(JobHeader job)
		{
			interactor?.SetJobInvoicingSecurityOverrideProvider(job);
		}

		public void ReportProgress(string currentProcessName, int totalItems, int done, TimeSpan eta, decimal speedPerTick)
		{
			interactor?.ReportProgress(currentProcessName, totalItems, done, eta, speedPerTick);
		}

		public void ShowRatesAdditionResult(AutoRatesAdditionResult additionResult, string entityName, CostSell costOrSell)
		{
			var actionDescription = costOrSell == CostSell.Revenue
				? Res.GetString("0cb1e2cc-14b3-4972-b6fb-cce07caea1bf", "was auto-rated.")
				: Res.GetString("5c8b7ab0-460f-4777-85f7-171c0e7541e7", "was auto-costed.");

			var logType = !additionResult.CreatedCharges.Any() && !additionResult.ModifiedCharges.Any() ? LogType.Warning : LogType.Information;
			var message = string.Concat(entityName, " ", actionDescription, System.Environment.NewLine, additionResult.GetLog());
			Log(logType, message);

			var loggerWithSession = logger as ILoggerWithSession;
			loggerWithSession?.ShowRatesAdditionResult(additionResult, entityName, costOrSell);
		}

		public IDisposable StartRatingSession()
		{
			if (interactor != null)
			{
				return interactor.StartRatingSession();
			}
			else
			{
				return (logger as ILoggerWithSession)?.StartRatingSession();
			}
		}

		#region SuppressResourceStringsCheckRegion

		public void Log(LogType type, string message) => Log(type, message, null);

		public void Log(LogType type, string message, Exception ex)
		{
			if (!debugEnabled && type == LogType.Debug)
			{
				return;
			}

			message = GroupDuplicateLines(message).ToStringWithDelimiterBetweenStrings(System.Environment.NewLine + '\t');

			logger?.Log(type, message, ex);
			if (ex != null)
			{
				internalLog.Add(new LogInfo
				{
					Message = ZString.Format("{0}: {1}. Exception: {2}", type, message, ex.Message),
					Type = type
				});
			}
			else
			{
				internalLog.Add(new LogInfo
				{
					Message = ZString.Format("{0}: {1}", type, message),
					Type = type
				});
			}
		}

		public static IEnumerable<string> GroupDuplicateLines(string message)
		{
			var lines = new List<string>(message.Split(new string[] { "\r\n" }, StringSplitOptions.None));
			var tuples = new List<Tuple<string, int>>();

			Tuple<string, int> tuple = null;
			foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
			{
				if (tuple != null && line.Equals(tuple.Item1))
				{
					tuples[tuples.Count - 1] = tuple = Tuple.Create(line, tuple.Item2 + 1);
				}
				else
				{
					tuples.Add(tuple = Tuple.Create(line, 1));
				}
			}

			return tuples.Select(x => x.Item1 + (x.Item2 == 1 ? "" : " (x" + x.Item2 + ")"));
		}

		#endregion

		public IEnumerable<string> DumpLog()
		{
			var result = new List<LogInfo>(internalLog);
			internalLog = new List<LogInfo>();

			return result.Select(x => x.Message);
		}

		public IEnumerable<string> GetLogs(Func<LogInfo, string> projector = null)
		{
			return internalLog.Select(projector ?? (x => x.Message));
		}

		public IDisposable DeferErrorPopup()
		{
			return (logger as IAutoRatingGUIInteractor)?.DeferErrorPopup();
		}

		public IEnumerable<AutoRateInfo> SelectRate(IRatingContext ratingContext, RatingCriteria criteria)
		{
			return interactor?.SelectRate(ratingContext, criteria);
		}
	}
}

