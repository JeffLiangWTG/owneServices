using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Packing.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transactions.Business
{
	[Immutable]
	class InventoryGroupingKey : GroupingKey
	{
		InventoryGroupingKey()
		{
		}

		InventoryGroupingKey(ZGuid orderLinePK, ZGuid productPK, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate expiry, ZDate packing)
		{
			OrderLinePK = orderLinePK;
			ProductPK = productPK;
			PartAttrib1 = partAttrib1.ToUpper();
			PartAttrib2 = partAttrib2.ToUpper();
			PartAttrib3 = partAttrib3.ToUpper();
			SerialNumber = serialNumber.ToUpper();
			Expiry = expiry;
			Packing = packing;
		}

		readonly ZGuid OrderLinePK;
		readonly ZGuid ProductPK;
		readonly ZString PartAttrib1;
		readonly ZString PartAttrib2;
		readonly ZString PartAttrib3;
		readonly ZString SerialNumber;
		readonly ZDate Expiry;
		readonly ZDate Packing;

		#region New

		public static InventoryGroupingKey New(WhsReleaseLine releaseLine, WhsPickableDocketLine orderLine)
		{
			Argument.NotNull(releaseLine, nameof(releaseLine));
			Argument.NotNull(orderLine, nameof(orderLine));

			return new InventoryGroupingKey(orderLine.PK,
				orderLine.WE_OP,
				releaseLine.PartAttribute1,
				releaseLine.PartAttribute2,
				releaseLine.PartAttribute3,
				releaseLine.SerialNumber,
				releaseLine.ExpiryDate,
				releaseLine.PackingDate);
		}

		public static InventoryGroupingKey New(WhsPickLine pickLine)
		{
			Argument.NotNull(pickLine, nameof(pickLine));
			InventoryGroupingKey result;

			var inventory = pickLine.InventoryLine;
			if (inventory != null)
			{
				result = new InventoryGroupingKey(
					pickLine.WZ_WE_TransactionLine,
					inventory.WE_OP,
					pickLine.WZ_ReleaseCapturedPartAttrib1.IsEmpty ? inventory.WE_PartAttrib1 : pickLine.WZ_ReleaseCapturedPartAttrib1,
					pickLine.WZ_ReleaseCapturedPartAttrib2.IsEmpty ? inventory.WE_PartAttrib2 : pickLine.WZ_ReleaseCapturedPartAttrib2,
					pickLine.WZ_ReleaseCapturedPartAttrib3.IsEmpty ? inventory.WE_PartAttrib3 : pickLine.WZ_ReleaseCapturedPartAttrib3,
					pickLine.WZ_ReleaseCapturedSerialNumber.IsEmpty ? inventory.WE_SerialNumber : pickLine.WZ_ReleaseCapturedSerialNumber,
					inventory.WE_ExpiryDate,
					inventory.WE_PackingDate);
			}
			else
			{
				result = Empty;
			}

			return result;
		}

		#endregion

		#region Equals / HashCode / IsSimilarItem

		protected override bool Equals(GroupingKey other)
		{
			var inventoryKey = other as InventoryGroupingKey;
			return inventoryKey != null
				&& OrderLinePK == inventoryKey.OrderLinePK
				&& PartAttrib1 == inventoryKey.PartAttrib1
				&& PartAttrib2 == inventoryKey.PartAttrib2
				&& PartAttrib3 == inventoryKey.PartAttrib3
				&& SerialNumber == inventoryKey.SerialNumber
				&& Expiry == inventoryKey.Expiry
				&& Packing == inventoryKey.Packing;
		}

		public override int GetHashCode()
		{
			return
				OrderLinePK.GetHashCode()
				^ PartAttrib1.GetHashCode()
				^ PartAttrib2.GetHashCode()
				^ PartAttrib3.GetHashCode()
				^ SerialNumber.GetHashCode()
				^ Expiry.GetHashCode()
				^ Packing.GetHashCode();
		}

		protected override bool IsSimilarItem_DoNotUseCore(GroupingKey otherKey)
		{
			var inventoryKey = otherKey as InventoryGroupingKey;
			return inventoryKey != null
				&& ProductPK == inventoryKey.ProductPK
				&& PartAttrib1 == inventoryKey.PartAttrib1
				&& PartAttrib2 == inventoryKey.PartAttrib2
				&& PartAttrib3 == inventoryKey.PartAttrib3
				&& Expiry == inventoryKey.Expiry
				&& Packing == inventoryKey.Packing;
		}

		#endregion

		#region ToString

		public override string ToString()
		{
			return string.Join("|", new string[] { OrderLinePK.ToString(), PartAttrib1, PartAttrib2, PartAttrib3, SerialNumber, Expiry.ToShortDateString(), Packing.ToShortDateString() });
		}

		#endregion

		#region Empty

		public static InventoryGroupingKey Empty { get; } = new EmptyInventoryKey();

		class EmptyInventoryKey : InventoryGroupingKey
		{
			public EmptyInventoryKey()
			{
			}

			protected override bool Equals(GroupingKey other) => other is EmptyInventoryKey;
			public override int GetHashCode() => -1;
		}

		#endregion

		#region KeyForAutoPackCore

		protected override GroupingKey KeyForAutoPackCore => new InventoryGroupingKey(
			OrderLinePK,
			ProductPK,
			PartAttrib1,
			PartAttrib2,
			PartAttrib3,
			string.Empty,
			Expiry,
			Packing);

		#endregion

#if DEBUG
		#region NewForTesting		

		public static InventoryGroupingKey NewForTesting(ZGuid orderLinePK, ZGuid productPK, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate expiry, ZDate packing)
		{
			return new InventoryGroupingKey(orderLinePK, productPK, partAttrib1, partAttrib2, partAttrib3, serialNumber, expiry, packing);
		}

		#endregion
#endif
	}
}
