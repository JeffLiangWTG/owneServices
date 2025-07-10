using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class SerialNumberCollectionFirstItemComparer : IComparer
	{
		public SerialNumberCollectionFirstItemComparer()
		{
			enableSchemaRedesignChanges = WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value;
		}
		readonly bool enableSchemaRedesignChanges;

		int Compare(WhsInventoryView x, WhsInventoryView y)
		{
			IComparable xValue;
			IComparable yValue;

			if (enableSchemaRedesignChanges)
			{
				if (x?.InDocketLine is not ISerialNumberParent xParent ||
					y?.InDocketLine is not ISerialNumberParent yParent)
				{
					return 0;
				}

				xValue = GetMinSerialNumber(xParent);
				yValue = GetMinSerialNumber(yParent);
			}
			else
			{
				xValue = x.WI_SerialNumber;
				yValue = y.WI_SerialNumber;
			}

			return Comparer<IComparable>.Default.Compare(xValue, yValue);
		}

		IComparable GetMinSerialNumber(ISerialNumberParent parent)
		{
			if (parent.SerialNumbers.Count == 0)
			{
				return null;
			}

			return parent.SerialNumbers
				.Cast<WhsSerialNumberPivot>()
				.Select(pivot => pivot.SerialNumberValue)
				.Min();
		}

		int IComparer.Compare(object x, object y)
		{
			if (x is WhsInventoryView viewX && y is WhsInventoryView viewY)
			{
				return Compare(viewX, viewY);
			}
			throw new ArgumentException("Invalid arguments (x, y)");
		}
	}
}
