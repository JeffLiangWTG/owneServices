using Enterprise.Semaphores.Common;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	class UCMPSemaphoreType : ISemaphoreType
	{
		public UCMPSemaphoreType(string serviceTaskCode, string applicationCode, int maxConcurrentHandles)
		{
			lockInfo = $"UCMP:{serviceTaskCode}:{applicationCode}";
			MaxConcurrentHandles = maxConcurrentHandles;
		}

		public string LockInfo => lockInfo;

		public string Category => "UCM";

		public int MaxConcurrentHandles { get; }
		readonly string lockInfo;
	}
}
