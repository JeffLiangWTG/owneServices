using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessTaskRowFetchStrategyWrapper : IRowFetchStrategy
	{
		readonly Lazy<ProcessTaskRowFetchStrategy> internalRowFetchStrategies = new Lazy<ProcessTaskRowFetchStrategy>(GetInternalProcessTaskFetchStrategy);

		void IRowFetchStrategy.FetchForLoad(BusinessObjectFactory factory, DataRow[] rows)
		{
			var innerStrategy = internalRowFetchStrategies.Value;
			if (innerStrategy != null)
			{
				innerStrategy.FetchForLoad(factory, rows);
			}
		}

		static ProcessTaskRowFetchStrategy GetInternalProcessTaskFetchStrategy()
		{
			var type = TypeDecider.GetTypeForBinding(typeof(ProcessTaskRowFetchStrategy));
			return (ProcessTaskRowFetchStrategy)Activator.CreateInstance(type);
		}
	}
}



