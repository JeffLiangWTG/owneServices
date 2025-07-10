using System.Collections;
using CargoWise.Application;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	class ProcessTasksSyncUserProcessorFactory : IProcessTasksSyncUserProcessorFactory
	{
		public IProcessTaskSyncUserProcessor GetUserSyncProcessor(string formFlowType)
		{
			var providers = ObjectFactory.Get<Hashtable>(ProcessTaskSyncUserProcessors);
			var providerHandle = (ObjectHandle)providers[formFlowType];
			return (IProcessTaskSyncUserProcessor)providerHandle?.GetObject();
		}

		const string ProcessTaskSyncUserProcessors = nameof(ProcessTaskSyncUserProcessors);
	}
}
