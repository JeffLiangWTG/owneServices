using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class ConsolidationPackageInfo : DataObjectInfo
	{
		public ConsolidationPackageInfo()
		{
		}

		public ConsolidationPackageInfo(PackageInfo package, Guid orderPK, bool cannotOverrideConsolidationLocation)
		{
			Package = package;
			OrderPK = orderPK;
			CannotOverrideConsolidationLocation = cannotOverrideConsolidationLocation;
		}

		public PackageInfo Package { get; set; }
		public Guid OrderPK { get; set; }
		public bool CannotOverrideConsolidationLocation { get; set; }
	}
}
