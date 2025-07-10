using System;
using System.Globalization;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.ServiceTasks
{
	public abstract class NudgeCustomsServiceTask : CustomsServiceTask
	{
		protected NudgeCustomsServiceTask() { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service task logs are not translated.")]
		protected sealed override void RunTaskCore(CancellationToken token)
		{
			try
			{
				RunTasksHandleOnSavingCriticalCheck(token, RunMainTask);
			}
			catch (OperationCanceledException ex)
			{
				var message = string.Format(CultureInfo.InvariantCulture, "Cancellation event occurring. This can occur when the service task is cancelled by a user or the DB is under heavy load. Cancellation Reason: {0}", GetExceptionMessagesForLog(ex));
				ServiceLogger?.Log(LogType.Information, message);
				Nudge();
			}
		}

		protected void Nudge()
		{
			ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(CurrentServiceTaskCode);
		}

		protected abstract void RunMainTask(CancellationToken token);
		protected abstract string CurrentServiceTaskCode { get; }
	}

	public abstract class CustomsServiceTask : ServiceProviderImpl
	{
		protected CustomsServiceTask()
		{
		}

		public sealed override void RunTask(CancellationToken token)
		{
			RunTaskHandleEmailSendFailure(() => RunTaskCore(token));
		}

		public void RunTaskHandleEmailSendFailure(Action action)
		{
			try
			{
				action();
			}
			catch (EmailSendFailedException ex)
			{
				ServiceLogger.Log(LogType.Error, ex.Message);
			}
		}

		protected void RunTasksHandleOnSavingCriticalCheck(CancellationToken token, Action<CancellationToken> action)
		{
			try
			{
				action.Invoke(token);
			}
			catch (OnSavingCriticalCheckException ex)
			{
				ServiceLogger.GetTaskNotificationSubscriber().AddError(ex.Message);
			}
		}

		protected abstract void RunTaskCore(CancellationToken token);

		protected LoggingInformation Logger
		{
			get
			{
				if (logger == null)
				{
					logger = new LoggingInformation();
					logger.OnLogInfoAdded += new LoggingInformation.LogInfoAdded(logger_OnLogInfoAdded);
				}
				return logger;
			}
		}
		LoggingInformation logger;

		void logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger.Log(logType, log);
		}

		protected string GetExceptionMessagesForLog(Exception ex)
		{
			var builder = new StringBuilder();
			var innerEx = ex;
			while (innerEx != null)
			{
				builder.AppendLine(innerEx.Message);
				innerEx = innerEx.InnerException;
			}

			return builder.ToString();
		}
	}
}
