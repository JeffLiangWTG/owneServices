using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class SearchFilterCriteriaInfo : DataObjectInfo
	{
		public SearchFilterCriteriaInfo()
		{
			AreaCode = "";
			ClientCode = "";
			PickMethod = "";
			UOMType = "";
			EquipmentRegistrationNumber = "";
		}

		public string AreaCode { get; set; }
		public string ClientCode { get; set; }
		public string PickMethod { get; set; }
		public short PickGroup { get; set; }
		public string UOMType { get; set; }
		public string EquipmentRegistrationNumber { get; set; }
	}
}
