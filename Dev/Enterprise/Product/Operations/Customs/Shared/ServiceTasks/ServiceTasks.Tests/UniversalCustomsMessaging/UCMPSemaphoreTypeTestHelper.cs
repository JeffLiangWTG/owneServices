using System.Collections.Generic;
using Enterprise.Customs.Common;
using Enterprise.Semaphores.Common;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class UCMPSemaphoreTypeTestHelper : Semaphores.Common.Testing.ISemaphoreTypeWithoutParameterLessContructorTestHelper
	{
		public IEnumerable<ISemaphoreType> GetUniqueSemaphores()
		{
			foreach (var applicationCode in UniversalCustomsMessagingSubscribers.GetApplicationCodes())
			{
				yield return new UCMPSemaphoreType(UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen, applicationCode, 1);
				yield return new UCMPSemaphoreType(UniversalCustomsMessagingConstants.ServiceTaskCodes.Worker, applicationCode, 1);
			}
		}
	}
}
