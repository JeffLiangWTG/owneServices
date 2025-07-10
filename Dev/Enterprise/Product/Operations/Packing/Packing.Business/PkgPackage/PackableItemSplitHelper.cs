using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	static class PackableItemSplitHelper
	{
		public static IPackableItem SplitItem(IPackableItem packableItem, ZDecimal qtyToSplit)
		{
			var splitPackableItem = packableItem.Split(qtyToSplit);
			if (splitPackableItem.TablePrefix != packableItem.TablePrefix)
			{
				throw new InvalidOperationException("Split() Function on IPackableItem should return the same Type of BizO.");
			}
			else if (splitPackableItem.Quantity != qtyToSplit)
			{
				var bizo = packableItem as BusinessObject;
				throw new InvalidOperationException(FormattableString.Invariant($@"Split() Function on IPackableItem should Split the Quantity Correctly.
Packable Item PK: {packableItem.PK}
Table Prefix: {splitPackableItem.TablePrefix}
Packable Item Quantity: {splitPackableItem.Quantity}
Quantity to split: {qtyToSplit}
Is Deleted: {bizo?.IsDeleted}
Is In Database: {bizo?.IsInDatabase}
This error is seen because Packable Item Qty and Qty to split are different.
This can be because Split has failed, the scales of the two decimals are different because one was rounded to fit a database property scale (i.e. 1.55556 vs 1.556), or other unknown reason."));
			}

			return splitPackableItem;
		}
	}
}

