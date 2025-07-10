using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	sealed class LoadedPackageInfo
	{
		public LoadedPackageInfo(ZGuid packagePK, ZDateTimeOffset loadedTime, ZString loadID)
		{
			PackagePK = packagePK;
			IsLoaded = loadedTime.IsValid;
			LoadID = loadID;
		}

		public ZGuid PackagePK { get; }
		public bool IsLoaded { get; }
		public ZString LoadID { get; }
	}
}
