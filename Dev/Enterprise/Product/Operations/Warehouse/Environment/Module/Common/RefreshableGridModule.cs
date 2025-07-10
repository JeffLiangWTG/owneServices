using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	public abstract class RefreshableGridModule : ZFilterGridModule, IModuleGridCollectionRefreshable
	{
		protected RefreshableGridModule()
		{
		}

		public abstract string[] GetTableNamesToMonitor();

		public void RefreshDisplay()
		{
			if (HasSearched)
			{
				using (GridCollection.SuspendListChanged())
				{
					var result = PerformSearchCore(GridCollection.TypeOfElements, GetCollectionQuery());
					if (result.Type == PerformSearchResultType.Success)
					{
						PushItemsIntoCollectionCore(GridCollection, result, DefaultSortOrder);
					}
				}
			}
		}

		protected virtual ZQuery GetCollectionQuery() => GridCollection.CompleteFilter;
	}
}
