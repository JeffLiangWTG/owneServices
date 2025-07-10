using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PickFaceInfo : IPickFaceInfo
	{
		public PickFaceInfo(ZGuid warehousePK, ZGuid clientPK, ZGuid productPK, ZGuid transferToLocationPK,
			ZDecimal replenishQuantity, ZDecimal replenishMultiple, ZBool isDeadLocked)
			: this(warehousePK, ZGuid.Empty, clientPK, productPK, transferToLocationPK, replenishQuantity, replenishMultiple, isDeadLocked,
				  false, ZDate.Empty, ZDate.Empty, "", "", "", "")
		{
		}

		public PickFaceInfo(ZGuid warehousePK, ZGuid pickToReplenishPK, ZGuid clientPK, ZGuid productPK, ZGuid transferToLocationPK, ZDecimal replenishQuantity, ZDecimal replenishMultiple, ZBool isDeadLocked,
			ZBool isDynamicTransfer, ZDate orderedExpiryDate, ZDate orderedPackingDate, ZString orderedAttribute1, ZString orderedAttribute2, ZString orderedAttribute3, ZString orderedSerialNumber)
		{
			WarehousePK = warehousePK;
			PickToReplenishPK = pickToReplenishPK;
			ClientPK = clientPK;
			ProductPK = productPK;
			TransferToLocationPK = transferToLocationPK;
			ReplenishQuantity = replenishQuantity;
			ReplenishMultiple = replenishMultiple;
			IsDeadLocked = isDeadLocked;
			IsDynamicTransfer = isDynamicTransfer;
			OrderedExpiryDate = orderedExpiryDate;
			OrderedPackingDate = orderedPackingDate;
			OrderedAttribute1 = orderedAttribute1;
			OrderedAttribute2 = orderedAttribute2;
			OrderedAttribute3 = orderedAttribute3;
			OrderedSerialNumber = orderedSerialNumber;
		}

		public ZGuid WarehousePK { get; }
		public ZGuid PickToReplenishPK { get; }
		public ZGuid ClientPK { get; }
		public ZGuid ProductPK { get; }
		public ZGuid TransferToLocationPK { get; }
		public ZDecimal ReplenishQuantity { get; }
		public ZDecimal ReplenishMultiple { get; }
		public ZBool IsDeadLocked { get; }
		public ZBool IsDynamicTransfer { get; }
		public ZDate OrderedExpiryDate { get; }
		public ZDate OrderedPackingDate { get; }
		public ZString OrderedAttribute1 { get; }
		public ZString OrderedAttribute2 { get; }
		public ZString OrderedAttribute3 { get; }
		public ZString OrderedSerialNumber { get; }
	}
}
