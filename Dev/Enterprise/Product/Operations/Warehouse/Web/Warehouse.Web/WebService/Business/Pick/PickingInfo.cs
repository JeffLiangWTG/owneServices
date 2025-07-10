using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class PickingInfo : DataObjectInfo
	{
		public PickingInfo()
		{
		}

		public PickingInfo(decimal pickedQty, bool isVerifiedNonEmpty, bool shouldSplit = false, bool isPickingSuspended = false)
		{
			PickedQty = pickedQty;
			IsVerifiedNonEmpty = isVerifiedNonEmpty;
			ShouldSplit = shouldSplit;
			IsPickingSuspended = isPickingSuspended;
		}

		public decimal PickedQty { get; set; }
		public bool IsVerifiedNonEmpty { get; set; }
		public bool ShouldSplit { get; set; }
		public bool IsPickingSuspended { get; set; }
	}
}
