namespace Enterprise.Warehouse.Environment.Business
{
	public interface IModuleGridCollectionRefreshable
	{
		void RefreshDisplay();
		string[] GetTableNamesToMonitor();
	}
}
