using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class BiggestPackTypeGrouper
	{
		#region GetGroups

		public static IEnumerable<GroupedQuantityItem> GetGroups(OrgSupplierPart product, ZDecimal quantity, ConversionsToSKUTable conversionsTable = null)
		{
			IEnumerable<GroupedQuantityItem> result;

			if (quantity % 1 > 0)
			{
				result = new[] { GetSingleUngroupedUnit(product, quantity) };
			}
			else
			{
				if (conversionsTable == null)
				{
					conversionsTable = new ConversionsToSKUTable(product);
				}

				var groupedItems = new List<GroupedQuantityItem>();

				while (quantity > 0)
				{
					var conversion = GetBiggestPackTypeForRemainingQuantity(conversionsTable, quantity);
					var packQty = Math.Floor(quantity / conversion.QtySKU);
					var qtySKU = packQty * conversion.QtySKU;
					groupedItems.Add(new GroupedQuantityItem(packQty, conversion.PackType, conversion.UOMType, qtySKU, product.OP_StockKeepingUnit));
					quantity -= qtySKU;
				}

				result = groupedItems;
			}

			return result;
		}

		#endregion

		#region Helpers

		static ConversionToSKU GetBiggestPackTypeForRemainingQuantity(IEnumerable<ConversionToSKU> conversionsTable, ZDecimal quantity)
		{
			return conversionsTable.OrderByDescending(x => x.QtySKU).FirstOrDefault(x => x.QtySKU <= quantity);
		}

		static GroupedQuantityItem GetSingleUngroupedUnit(OrgSupplierPart product, ZDecimal quantity)
		{
			var uomType = product.Lookups.PackTypes.FirstOrDefault(p => p.F3_Code == product.OP_StockKeepingUnit)?.F3_UOMType ?? "";
			return new GroupedQuantityItem(quantity, product.OP_StockKeepingUnit, uomType, quantity, product.OP_StockKeepingUnit);
		}

		#endregion
	}

	public class GroupedQuantityItem : IPackTypeConversion
	{
		public GroupedQuantityItem(ZDecimal packQty, ZString packType, ZString uomType, ZDecimal qty, ZString stockKeepingUnit)
		{
			Qty = qty;
			StockKeepingUnit = stockKeepingUnit;
			PackQty = packQty;
			PackType = packType;
			UOMType = uomType;
		}

		#region Properties

		public ZDecimal PackQty { get; }
		public ZString PackType { get; }
		public ZString UOMType { get; }
		public ZDecimal Qty { get; }
		public ZString StockKeepingUnit { get; }

		public bool IsConverted => PackType != StockKeepingUnit;

		ZString IPackTypeConversion.UOMType => UOMType;

		decimal IPackTypeConversion.PackQty => PackQty;

		decimal IPackTypeConversion.QtySKU => Qty / PackQty;

		#endregion
	}
}
