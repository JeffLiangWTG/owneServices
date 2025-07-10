using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class PickByLabelLabelInfo : DataObjectInfo
	{
		public PickByLabelLabelInfo()
		{
		}

		public PickByLabelLabelInfo(string packageID)
		{
			PackageID = packageID;
		}
		public string PackageID { get; set; }
	}
}
