using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class UnitOfMeasureConverter
	{
		public static decimal GetQuantityFromLine(BusinessObject parentBizO, IUnitOfMeasure unitOfMeasure, ILineWithProductAndQuantity lineWithProductAndQuantity)
		{
			return GetTotalQuantityFromLines(parentBizO, unitOfMeasure, new[] { lineWithProductAndQuantity });
		}

		public static decimal GetTotalQuantityFromLines(BusinessObject parentBizO, IUnitOfMeasure unitOfMeasure, IEnumerable<ILineWithProductAndQuantity> linesWithProductAndQuantity)
		{
			Argument.NotNull(parentBizO, nameof(parentBizO));
			Argument.NotNull(unitOfMeasure, nameof(unitOfMeasure));
			Argument.NotNull(linesWithProductAndQuantity, nameof(linesWithProductAndQuantity));

			var lines = linesWithProductAndQuantity.Select(line => Argument.NotNull(line, nameof(line))).ToArray();

			var qtyFromLines = 0m;
			var productsDictionary = new Dictionary<ZGuid, (OrgSupplierPart Part, ZDecimal Qty)>();

			parentBizO.ReplaceOverflowExceptionWithRowError(unitOfMeasure.Name, () =>
			{
				foreach (var line in lines)
				{
					if (line.ProductPK.IsValid)
					{
						if (!productsDictionary.TryGetValue(line.ProductPK, out var partAndQty))
						{
							var part = parentBizO.Factory.Load<OrgSupplierPart>(line.ProductPK);
							if (part != null)
							{
								productsDictionary.Add(part.PK, (part, line.Quantity));
							}
						}
						else
						{
							productsDictionary[line.ProductPK] = (partAndQty.Part, partAndQty.Qty + line.Quantity);
						}
					}
				}

				foreach (var (part, qty) in productsDictionary.Values)
				{
					qtyFromLines += part.UnitConverter.Convert(qty * unitOfMeasure.GetQuantityFromProduct(part), unitOfMeasure.GetUQFromProduct(part), unitOfMeasure.TotalUQ);
				}
			});

			return Utilities.Round(qtyFromLines, unitOfMeasure.DecimalPlacesForRounding);
		}
	}
}
