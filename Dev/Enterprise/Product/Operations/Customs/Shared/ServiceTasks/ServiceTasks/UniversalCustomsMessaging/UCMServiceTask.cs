using System.Threading;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public abstract class UCMServiceTask : GraphEngine.ServiceTasks.ServiceTask
	{
		public const string ServiceTaskCategory = "CUS";

		[HostedServiceRequirement]
		public static string HasUniversalCustomsMessagingSubscribers() =>
			UniversalCustomsMessagingSubscribers.HasUniversalCustomsMessagingSubscribers()
			? string.Empty
			: (NoResString)"There is no Customs message subscribed to Universal Customs Message Processing";

		protected sealed override void RunCore(CancellationToken token)
		{
			using (var manager = CreatNewManager())
			{
				RunWithLogger(token, manager);
			}
		}

		protected abstract CommonProcessingManager CreatNewManager();
	}
}
