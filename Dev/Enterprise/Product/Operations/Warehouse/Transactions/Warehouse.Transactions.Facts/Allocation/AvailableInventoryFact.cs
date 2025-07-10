using System;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.Business;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Facts
{
	public class AvailableInventoryFact : IAvailableInventoryFact
	{
		public AvailableInventoryFact(
			IWhsPickAvailableInventory availableInventory,
			IAllocationLocationFact location,
			ZGuid orderedInventoryPK,
			bool canAllocateInQuantitiesDifferentToPickStrategy,
			decimal quantity,
			decimal palletSize,
			Func<decimal, decimal> getQuantityThatCanBeAllocated,
			Action<decimal> reduceRelatedRecords)
		{
			Argument.NotNull(availableInventory, nameof(availableInventory));
			Argument.NotNull(location, nameof(location));
			ReduceRelatedRecords = Argument.NotNull(reduceRelatedRecords, nameof(reduceRelatedRecords));
			GetQuantityThatCanBeAllocated = Argument.NotNull(getQuantityThatCanBeAllocated, nameof(getQuantityThatCanBeAllocated));

			PK = availableInventory.PK.ToGuid();
			OrderedInventoryPK = orderedInventoryPK.ToGuid();
			CanAllocateInQuantitiesDifferentToPickStrategy = canAllocateInQuantitiesDifferentToPickStrategy;

			Location = new FactJoin<IAllocationLocationFact>(location);

			Quantity = quantity;
			PalletID = availableInventory.PalletID;
			PartAttribute1 = availableInventory.PartAttrib1;
			PartAttribute2 = availableInventory.PartAttrib2;
			PartAttribute3 = availableInventory.PartAttrib3;
			SerialNumber = availableInventory.SerialNumber;
			ArrivalDate = availableInventory.ArrivalDate.ToDateTime();
			ExpiryDate = availableInventory.ExpiryDate.ConvertToNullableDateTime();
			PackingDate = availableInventory.PackingDate.ConvertToNullableDateTime();

			BondedEntryKey = availableInventory.BondedEntryKey;
			BondedEntryDate = availableInventory.BondedEntryDate.ConvertToNullableDateTime();
			VFDPerStockUnit = availableInventory.VFDPerStockUnit;
			PalletSize = palletSize;
		}

		public Guid PK { get; }
		public Guid OrderedInventoryPK { get; }

		public FactJoin<IAllocationLocationFact> Location { get; }

		public decimal Quantity { get; private set; }

		public void DecreaseQuantity(decimal quantityToReduce)
		{
			Quantity -= quantityToReduce;
			ReduceRelatedRecords(quantityToReduce);
		}

		Action<decimal> ReduceRelatedRecords { get; }

		public string PalletID { get; }

		public string PartAttribute1 { get; }

		public string PartAttribute2 { get; }

		public string PartAttribute3 { get; }

		public string SerialNumber { get; }

		public string BondedEntryKey { get; }

		public DateTime ArrivalDate { get; }

		public DateTime? PackingDate { get; }

		public DateTime? ExpiryDate { get; }

		public DateTime? BondedEntryDate { get; }

		public decimal VFDPerStockUnit { get; }

		public bool IsPalletOverflow => PalletSize > 0m && Quantity > PalletSize && Quantity % PalletSize > 0m;
		decimal PalletSize { get; }

		public decimal GetQuantityThatCanBeAllocatedViaPickStrategy(decimal quantityToAllocate) => GetQuantityThatCanBeAllocated(quantityToAllocate);

		Func<decimal, decimal> GetQuantityThatCanBeAllocated { get; }

		public bool CanAllocateInQuantitiesDifferentToPickStrategy { get; }
	}
}
