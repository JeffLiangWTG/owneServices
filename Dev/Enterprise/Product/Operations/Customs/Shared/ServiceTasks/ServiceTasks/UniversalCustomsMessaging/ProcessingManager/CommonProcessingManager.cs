using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public abstract class CommonProcessingManager : IProcessingManager
	{
		protected CommonProcessingManager()
		{
		}

		public LoggingInformation Logger => logger ?? (logger = new LoggingInformation());
		LoggingInformation logger;

		public void ExecuteBatch(CancellationToken token)
		{
			try
			{
				applicationCodesQueue = new Queue<string>(
					UniversalCustomsMessagingSubscribers.GetApplicationCodes().ToList()
						.ShuffleListInRandomOrder());
				while (applicationCodesQueue.Count > 0)
				{
					applicationCode = applicationCodesQueue.Dequeue();
					token.ThrowIfCancellationRequested();
					ApplicationCode = applicationCode;
					ExecuteBatchCore(token);
				}
			}
			finally
			{
				applicationCodesQueue = null;
			}
		}
		Queue<string> applicationCodesQueue;

		protected void RequeueCurrentApplicationCode()
		{
			if (applicationCodesQueue != null)
			{
				applicationCodesQueue.Enqueue(ApplicationCode);
			}
		}

		protected string ApplicationCode
		{
			get => applicationCode;
			private set
			{
				var oldApplicationCode = applicationCode;
				applicationCode = value;
				OnApplicationCodeChanged(oldApplicationCode);
			}
		}
		string applicationCode;

		protected virtual void OnApplicationCodeChanged(string oldApplicationCode)
		{
		}

		protected abstract void ExecuteBatchCore(CancellationToken token);

		protected void Log(string message, LogType logType)
		{
			Logger.Log(message, logType);
		}

		protected string GetMessageLoggingDetail(EDIMessage message) => FailedMessagesManager.GetMessageIdentification(message) + GetAdditionalMessageLoggingDetail(message);

		string GetAdditionalMessageLoggingDetail(EDIMessage message)
		{
			var result = new ZStringBuilder();
			result.AppendIfNotEmpty(message.EM_MessageSubType);
			result.AppendIfNotEmpty(message.EM_ApplicationReference);
			result.AppendIfNotEmpty(message.EM_MessageOwner);
			if (!result.IsEmpty)
			{
				result.Prepend(string.Empty);
			}
			return result.ToStringWithDelimiterBetweenAppends("|");
		}

		public virtual void Dispose()
		{
		}
	}
}
