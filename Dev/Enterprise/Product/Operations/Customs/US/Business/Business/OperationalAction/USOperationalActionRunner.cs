using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.OperationalAction
{
	public enum SaveResult { Fail, FailWithConcurrencyError, Success }

	public abstract class USOperationalActionRunner<T>
		where T : BusinessObject
	{
		public USOperationalActionRunner(IOperationalActionSectionLog log)
		{
			this.log = log;
		}
		protected readonly IOperationalActionSectionLog log;

		public IEnumerable<ZGuid> PerformFunctionOperationalAction(bool sendWithMessageErrors, BusinessObject[] targets)
		{
			if (targets.Length == 0)
			{
				log.Notify(OperationalActionLogErrorLevel.Informational, string.Format("0 {0} selected and therefore there is nothing to send.", TypeOfJob));
				yield return ZGuid.Empty;
			}
			else
			{
				log.SetSectionProgressMax(targets.Length);
				foreach (var pK in RunOperationalActionSendMessage(sendWithMessageErrors, targets, true))
				{
					yield return pK;
				}

				if (jobsWithConcurrency.Count > 0)
				{
					foreach (var pK in RunOperationalActionSendMessage(sendWithMessageErrors, jobsWithConcurrency.ToArray(), false))
					{
						yield return pK;
					}
				}
			}
		}
		List<T> jobsWithConcurrency = new List<T>();
		protected abstract string TypeOfJob { get; }

		IEnumerable<ZGuid> RunOperationalActionSendMessage(bool sendWithMessageErrors, BusinessObject[] targets, bool keepConcurrentJobsForNextRun = false)
		{
			foreach (var target in targets)
			{
				var newFactory = new BusinessObjectFactory();
				var job = newFactory.Load<T>(target.PK);
				#region Testing
#if DEBUG
				LastFactoryForTesting = newFactory;
#endif
				#endregion

				if (job != null && IsJobEligibleForSending(job))
				{
					job.LoadChildEditableObjects();

					var operationalResult = SaveResult.Fail;
					var sender = GetMessageSender(job);
					try
					{
						operationalResult = sender.OperationalActionSendMessage(sendWithMessageErrors, log);
						log.BumpSectionProgress();
					}
					finally
					{
						UnlockMergeMutexIfNecessary(job);
					}

					switch (operationalResult)
					{
						case SaveResult.FailWithConcurrencyError:
							if (keepConcurrentJobsForNextRun)
							{
								jobsWithConcurrency.Add(job);
							}
							else
							{
								log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Another user modified the job. {1} submission has been aborted.", new object[] { GetLogControllerLink(job), sender.MessageType });
							}
							break;
						case SaveResult.Success:
							log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "Job {0}: {1} message has been sent successfully.", new object[] { GetLogControllerLink(job), sender.MessageType });
							yield return job.PK;
							break;
					}
				}
			}
		}

		protected abstract bool IsJobEligibleForSending(T job);
		protected abstract OperationalActionBulkMessageSender<T> GetMessageSender(T job);
		protected abstract void UnlockMergeMutexIfNecessary(T job);
		protected abstract LogControllerLink GetLogControllerLink(T job);

		#region Testing
#if DEBUG
		internal BusinessObjectFactory LastFactoryForTesting;

		internal void SetUpJobsWithConcurrency(IEnumerable<T> jobs)
		{
			jobsWithConcurrency = new List<T>();
			jobsWithConcurrency.AddRange(jobs);
		}

#endif
		#endregion
	}
}
