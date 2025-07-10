using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Environment.Business
{
	public class ModuleGridCollectionSynchronisationManager : IDataRefreshBusSubscriber
	{
		public ModuleGridCollectionSynchronisationManager(IBusinessObjectCollection moduleCollectionForFactory, IModuleGridCollectionRefreshable refreshable)
		{
			ModuleCollectionForFactory = Argument.NotNull(moduleCollectionForFactory, nameof(moduleCollectionForFactory));
			Refreshable = Argument.NotNull(refreshable, nameof(refreshable));

			RefreshManager = new DataRefreshManager();

			var tablesToMonitor = refreshable.GetTableNamesToMonitor();
			if (tablesToMonitor == null || tablesToMonitor.Length == 0)
			{
				throw new InvalidOperationException("No tables to monitor were provided.");
			}

			foreach (var table in tablesToMonitor)
			{
				RefreshManager.StartManaging(table, this);
			}
		}

		IBusinessObjectCollection ModuleCollectionForFactory { get; }
		DataRefreshManager RefreshManager { get; }
		IModuleGridCollectionRefreshable Refreshable { get; }

		public BusinessObjectFactory Factory => ModuleCollectionForFactory.Factory;

		public bool IncludeDeletedObjectsInRefresh => true;

		public void UpdatedByDataRefresh(IEnumerable<object> publishedObjects)
		{
			Refreshable.RefreshDisplay();
		}
	}
}
