using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class ShortfallManager
	{
		public ShortfallManager(WhsPickableDocket docket)
		{
			Docket = Argument.NotNull(docket, nameof(docket));
		}

		WhsPickableDocket Docket { get; }

		public IEnumerable<WhsPickableDocketLine> GetSameProductLines(WhsPickableDocketLine line)
		{
			var product = line.WE_OP;
			var partAttrib1 = line.WE_PartAttrib1;
			var partAttrib2 = line.WE_PartAttrib2;
			var partAttrib3 = line.WE_PartAttrib3;
			var serial = line.WE_SerialNumber;
			var expiry = line.WE_ExpiryDate;
			var packing = line.WE_PackingDate;
			var bondedEntryKey = line.WE_BondedEntryKey;
			var palletID = line.WE_PalletID;
			var currentHeldCode = line.WE_WHC_NKCurrentInventoryHeldCode;
			var isRegistryEnableHeldGoodsForOrders = WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value;
			return Docket.Lines.Find(dl =>
				dl.WE_OP == product
				&& dl.WE_PalletID.EqualsIgnoringCase(palletID)
				&& dl.WE_PartAttrib1.EqualsIgnoringCase(partAttrib1)
				&& dl.WE_PartAttrib2.EqualsIgnoringCase(partAttrib2)
				&& dl.WE_PartAttrib3.EqualsIgnoringCase(partAttrib3)
				&& dl.WE_SerialNumber.EqualsIgnoringCase(serial)
				&& dl.WE_ExpiryDate == expiry
				&& dl.WE_PackingDate == packing
				&& dl.WE_BondedEntryKey.EqualsIgnoringCase(bondedEntryKey)
				&& (!isRegistryEnableHeldGoodsForOrders || dl.WE_WHC_NKCurrentInventoryHeldCode == currentHeldCode)).Cast<WhsPickableDocketLine>();
		}

		public bool IsMarkingLinesAsShortfallPropertiesChangedSuspended => MarkingLinesAsShortfallPropertiesChangedSemaphore.IsSuspended;

		public void AddLineToCheckShortfallPropertiesChangedLater(WhsPickableDocketLine line)
		{
			if (IsMarkingLinesAsShortfallPropertiesChangedSuspended)
			{
				if (OrderLinesAdded == null)
				{
					OrderLinesAdded = new HashSet<AttributesHolder>(new OrderedAttribEqualityComparer());
				}

				OrderLinesAdded.Add(AttributesHolder.FromLine(line));
			}
		}

		HashSet<AttributesHolder> OrderLinesAdded;

		class AttributesHolder
		{
			public AttributesHolder(ZGuid productPK, string partAttrib1, string partAttrib2, string partAttrib3, string serialNumber, ZDate expiry, ZDate packing, string bondedEntryKey)
			{
				ProductPK = productPK;
				PartAttrib1 = partAttrib1;
				PartAttrib2 = partAttrib2;
				PartAttrib3 = partAttrib3;
				SerialNumber = serialNumber;
				Expiry = expiry;
				Packing = packing;
				BondedEntryKey = bondedEntryKey;
			}

			public static AttributesHolder FromLine(WhsDocketLine line)
			{
				return new AttributesHolder(line.WE_OP, line.WE_PartAttrib1, line.WE_PartAttrib2, line.WE_PartAttrib3, line.WE_SerialNumber, line.WE_ExpiryDate, line.WE_PackingDate, line.WE_BondedEntryKey);
			}

			public ZGuid ProductPK { get; }
			public string PartAttrib1 { get; }
			public string PartAttrib2 { get; }
			public string PartAttrib3 { get; }
			public string SerialNumber { get; }
			public ZDate Expiry { get; }
			public ZDate Packing { get; }
			public string BondedEntryKey { get; }
		}

		class OrderedAttribEqualityComparer : IEqualityComparer<AttributesHolder>
		{
			IEqualityComparer<string> Comparer { get; } = StringComparer.OrdinalIgnoreCase;

			public bool Equals(AttributesHolder x, AttributesHolder y)
			{
				return x.ProductPK == y.ProductPK
					&& Comparer.Equals(x.PartAttrib1, y.PartAttrib1)
					&& Comparer.Equals(x.PartAttrib2, y.PartAttrib2)
					&& Comparer.Equals(x.PartAttrib3, y.PartAttrib3)
					&& Comparer.Equals(x.SerialNumber, y.SerialNumber)
					&& x.Expiry == y.Expiry
					&& x.Packing == y.Packing
					&& Comparer.Equals(x.BondedEntryKey, y.BondedEntryKey);
			}

			public int GetHashCode(AttributesHolder obj)
			{
				return HashCodeHelper.GetCompositeHashCode(new[]
				{
					obj.ProductPK.GetHashCode(),
					Comparer.GetHashCode(obj.PartAttrib1),
					Comparer.GetHashCode(obj.PartAttrib2),
					Comparer.GetHashCode(obj.PartAttrib3),
					Comparer.GetHashCode(obj.SerialNumber),
					obj.Expiry.GetHashCode(),
					obj.Packing.GetHashCode(),
					Comparer.GetHashCode(obj.BondedEntryKey)
				});
			}
		}

		public IDisposable DeferMarkingLinesAsShortfallPropertiesChanged()
		{
			var disposable = new SemaphoreManager(MarkingLinesAsShortfallPropertiesChangedSemaphore);
			return new DisposableAction(() =>
			{
				disposable.Dispose();
				if (!IsMarkingLinesAsShortfallPropertiesChangedSuspended && OrderLinesAdded != null)
				{
					foreach (WhsPickableDocketLine line in Docket.Lines.Where(dl => OrderLinesAdded.Contains(AttributesHolder.FromLine(dl))))
					{
						line.Shortfall.HasProductUnitsOrAttribsChanged = true;
					}

					OrderLinesAdded = null;
				}
			});
		}

		Semaphore MarkingLinesAsShortfallPropertiesChangedSemaphore => markingLinesAsShortfallPropertiesChangedSemaphore ?? (markingLinesAsShortfallPropertiesChangedSemaphore = new Semaphore());
		Semaphore markingLinesAsShortfallPropertiesChangedSemaphore;
	}
}
