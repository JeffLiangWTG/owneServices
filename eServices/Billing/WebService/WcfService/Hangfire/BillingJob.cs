using System;
using System.Diagnostics;
using System.Threading;
using Common.Logging;
using Hangfire.Server;

namespace CargoWise.eServices.Billing.WcfService.Hangfire
{
	public abstract class BillingJob
	{
		public void StartProcess(CancellationToken token, PerformContext context, params object[] args)
		{
			JobName = $"{Name}-{GetBackgroundJobId(context)}";
			circuitBreaker = new CircuitBreaker.CircuitBreaker(1, MaxRetries, TimeSpan.FromMilliseconds(RetryTimeoutInMilliseconds));
			Logger.InfoFormat("{0} running in process {1}", JobName, Process.GetCurrentProcess().Id);
			Logger.Info($"Start processing {ItemName}");
			while (true)
			{
				if (token.IsCancellationRequested)
				{
					Logger.InfoFormat("{0} stopping in process {1}", JobName, Process.GetCurrentProcess().Id);
					return;
				}

				circuitBreaker.AttemptCall(() => Processing(token, args));

				var exception = circuitBreaker.GetExceptionFromLastAttemptCall();
				if (exception != null)
				{
					Logger.ErrorFormat($"Error processing {ItemName}. Retry", exception);
				}

				if (circuitBreaker.IsMaxRetriesReached())
				{
					Logger.Error("Reached maximum retries. Stop processing.");
					if (exception != null)
					{
						throw exception;
					}
					else
					{
						throw new InvalidOperationException($"{JobName} failed but did not throw an exception.");
					}
				}

				if (circuitBreaker.IsClosed && ExitWhenClosed)
				{
					Logger.InfoFormat("{0} exiting in process {1}", JobName, Process.GetCurrentProcess().Id);
					break;
				}
			}
		}

		public abstract void Processing(CancellationToken token, params object[] args);
		CircuitBreaker.CircuitBreaker circuitBreaker;
		public abstract ILog Logger { get; }
		protected string JobName { get; private set; }
		protected internal virtual string GetBackgroundJobId(PerformContext context) => context.BackgroundJob.Id;
		protected internal virtual IDateTimeProvider DateTimeProvider => null;
		public abstract string Name { get; }
		protected IConfigurationProvider Configuration { get; } = Global.WindsorContainer.Resolve<IConfigurationProvider>();
		protected virtual bool ExitWhenClosed => false;
		public abstract string ItemName { get; }
		protected virtual int MaxRetries => Configuration.MaxRetries;
		protected virtual int RetryTimeoutInMilliseconds => Configuration.RetryTimeoutInMilliseconds;
	}
}
