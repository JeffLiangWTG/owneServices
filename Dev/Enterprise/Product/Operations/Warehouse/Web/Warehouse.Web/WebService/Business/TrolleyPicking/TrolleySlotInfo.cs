using System;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class TrolleySlotInfo : DataObjectInfo
	{
		#region Constructors

		public TrolleySlotInfo()
		{
		}

		public TrolleySlotInfo(WhsPickTrolleySlot trolleySlot)
			: this()
		{
			if (trolleySlot != null)
			{
				var package = trolleySlot.Package;
				PackageID = package?.KP_PackageID ?? ZString.Empty;
				PackagePK = package?.PK.ToGuid() ?? Guid.Empty;
				SlotNumber = trolleySlot.WTS_SlotNumber;
				CartonGroupAndSize = package?.CartonGroupAndSize ?? ZString.Empty;
			}
		}

		#endregion

		#region Properties

		public string PackageID { get; set; }

		public Guid PackagePK { get; set; }

		public short SlotNumber { get; set; }

		public string CartonGroupAndSize { get; set; }

		#endregion
	}
}
