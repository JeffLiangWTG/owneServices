using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PickLinesDataRefreshBusSubscriber : IDataRefreshBusSubscriber
	{
		public PickLinesDataRefreshBusSubscriber(BusinessObjectFactory factory, IPickLineUpdatesRefreshable pickLineUpdatesRefreshable)
		{
			Factory = Argument.NotNull(factory, "Factory");
			PickLineUpdatesRefreshable = Argument.NotNull(pickLineUpdatesRefreshable, "PickLineUpdatesRefreshable");

			RefreshManager = new DataRefreshManager();
			RefreshManager.StartManaging(WhsPickLineSchema.Constants.TableName, this);
		}

		readonly BusinessObjectFactory Factory;
		readonly DataRefreshManager RefreshManager;
		readonly IPickLineUpdatesRefreshable PickLineUpdatesRefreshable;

		bool IDataRefreshBusSubscriber.IncludeDeletedObjectsInRefresh => false;

		BusinessObjectFactory IDataRefreshBusSubscriber.Factory => Factory;

		void IDataRefreshBusSubscriber.UpdatedByDataRefresh(IEnumerable<object> publishedObjects)
		{
			if (Factory.ThreadSentry.IsOwner)
			{
				PickLineUpdatesRefreshable.Refresh(publishedObjects.Cast<WhsPickLine>());
			}
		}
	}
}
