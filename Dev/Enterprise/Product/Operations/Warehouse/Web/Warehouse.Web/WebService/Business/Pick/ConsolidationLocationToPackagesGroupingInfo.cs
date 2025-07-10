using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class ConsolidationLocationToPackagesGroupingInfo : DataObjectInfo
	{
		public ConsolidationLocationToPackagesGroupingInfo()
		{
		}

		public ConsolidationLocationToPackagesGroupingInfo(WhsLocationInfo consolidationLocation, ConsolidationPackageInfo[] packages)
		{
			ConsolidationLocation = consolidationLocation;
			Packages = packages ?? Array.Empty<ConsolidationPackageInfo>();
		}

		public WhsLocationInfo ConsolidationLocation { get; set; }

		public ConsolidationPackageInfo[] Packages { get; set; }
	}
}
