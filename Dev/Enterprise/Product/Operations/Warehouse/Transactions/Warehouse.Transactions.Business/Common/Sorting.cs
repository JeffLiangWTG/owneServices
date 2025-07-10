using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region SortForPickingByAttribute

	static class SortForPickingByAttribute
	{
		public static IEnumerable<IComparer<T>> GetBaseComparers<T>(SortByPropertiesComparer<T> comparer)
			where T : ILineAttributes
		{
			yield return ComparerHolder<T>.GetComparer(line => line.ExpiryDate);
			yield return ComparerHolder<T>.GetComparer(line => line.PackingDate);
			yield return ComparerHolder<T>.GetComparer(line => line.BondedEntryKey);
			yield return ComparerHolder<T>.GetComparer(line => line.PartAttrib1);
			yield return ComparerHolder<T>.GetComparer(line => line.PartAttrib2);
			yield return ComparerHolder<T>.GetComparer(line => line.PartAttrib3);
			yield return ComparerHolder<T>.GetComparer(line => line.SerialNumber);
		}

		abstract class ComparerHolder<T> : SortByPropertiesComparer<T>
			where T : ILineAttributes
		{
			ComparerHolder()
			{
			}

			public static IComparer<T> GetComparer(Func<T, IZType> getProperty)
			{
				return new ValueComparer(o => getProperty(o));
			}
		}
	}

	#endregion

	#region SortItemsForPicking

	public class SortItemsForPicking : SortByPropertiesComparer<WhsPickOrderedInventory>
	{
		protected override IEnumerable<IComparer<WhsPickOrderedInventory>> GetElementaryComparers()
		{
			var result = SortForPickingByAttribute.GetBaseComparers(this).ToList();
			result.AddRange(new IComparer<WhsPickOrderedInventory>[]
			{
				new ValueComparer(delegate(WhsPickOrderedInventory orderedInventory) { return orderedInventory.IsComponentOrderedInventoryOnSalesOrder; }),
				new ValueComparer(delegate(WhsPickOrderedInventory orderedInventory) { return orderedInventory.Product?.Parent.OP_PartNum ?? ""; }),
				new ValueComparer(delegate(WhsPickOrderedInventory orderedInventory) { return orderedInventory.QuantityOrdered; })
			});

			return result;
		}
	}

	#endregion

	#region SortInventoryForPicking

	public class SortInventoryForPicking : SortByPropertiesComparer<WhsInventoryView>
	{
		protected override IEnumerable<IComparer<WhsInventoryView>> GetElementaryComparers()
		{
			var result = new List<IComparer<WhsInventoryView>>()
			{
				new ValueComparer(delegate(WhsInventoryView line) { return line.WI_ExpiryDate; }),
				new ValueComparer(delegate(WhsInventoryView line) { return line.WI_PackingDate; }),
				new ValueComparer(delegate(WhsInventoryView line) { return (ZBool)!line.IsAllocatedToPickFace(); }),
				new ValueComparer(delegate(WhsInventoryView line) { return line.WI_ArrivalDate; })
			};

			result.AddRange(SortForPickingByAttribute.GetBaseComparers(this));
			result.AddRange(new IComparer<WhsInventoryView>[]
			{
				new ValueComparer(delegate(WhsInventoryView line) { return line.Location?.RowName ?? ZString.Empty; }),
				new ValueComparer(delegate(WhsInventoryView line) { return line.Location?.WLV_Column ?? ZShort.Zero; }),
				new ValueComparer(delegate(WhsInventoryView line) { return line.Location?.WLV_Level ?? ZShort.Zero; }),
				new ValueComparer(delegate(WhsInventoryView line) { return line.Location?.WLV_Tray ?? ZShort.Zero; })
			});

			return result;
		}
	}

	#endregion

	#region SortPickInventoryForPicking

	public class SortPickInventoryForPicking : SortByPropertiesComparer<WhsPickAvailableInventory>
	{
		protected override IEnumerable<IComparer<WhsPickAvailableInventory>> GetElementaryComparers()
		{
			var result = GetEntryDateComparer().ToList();
			result.AddRange(new IComparer<WhsPickAvailableInventory>[]
			{
				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.ExpiryDate; }),
				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.PackingDate; }),
				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return (ZBool)!availableInventory.IsAllocatedToPickFace(); }),
				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.ArrivalDate; }),

				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.Location?.RowName ?? ZString.Empty; }),
				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.Location?.WLV_Column ?? ZShort.Zero; }),
				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.Location?.WLV_Level ?? ZShort.Zero; }),
				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.Location?.WLV_Tray ?? ZShort.Zero; }),

				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.BondedEntryKey; }),
				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.PartAttrib1; }),
				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.PartAttrib2; }),
				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.PartAttrib3; }),
				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.SerialNumber; }),

				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.PalletID; }),
			});

			return result;
		}

		protected virtual IEnumerable<IComparer<WhsPickAvailableInventory>> GetEntryDateComparer() => Array.Empty<IComparer<WhsPickAvailableInventory>>();
	}

	public class SortPickInventoryForBondReleasePicking : SortPickInventoryForPicking
	{
		protected override IEnumerable<IComparer<WhsPickAvailableInventory>> GetEntryDateComparer()
		{
			return new IComparer<WhsPickAvailableInventory>[]
			{
				new ValueComparer(delegate(WhsPickAvailableInventory availableInventory) { return availableInventory.Inventory[0].CustomsData.WB_EntryDate; })
			};
		}
	}

	#endregion

	#region SortPickLinesForPickingSlip

	public class SortPickLinesForPickingSlip : SortByStandardPickingOrPutawayFields<WhsPickLine>
	{
		protected override IEnumerable<IComparer<WhsPickLine>> GetElementaryComparers()
		{
			return GetComparersForSortByClientAndThenByPickMethod(l => l.InventoryLineForAvailableInventory.Location, l => l.Inventory.Client)
				.Concat(new IComparer<WhsPickLine>[]
				{
					new ValueComparer(line => line.DocketLine.WE_PickGroup)
				})
				.Concat(GetComparersForSortByLocationAndThenByProduct(line => line.InventoryLineForAvailableInventory.Location, line => line.Inventory.SupplierPart, true));
		}

		public static string EquivalentOrderBySql
		{
			get
			{
				return @"
OH_Code ASC,
CASE WHEN PickMethodDesc IS NOT NULL THEN 1 ELSE 0 END DESC,
ISNULL(PickMethodDesc, '') ASC,
CASE WHEN WE_PickGroup = 0 THEN 256 ELSE WE_PickGroup END ASC,
CASE WHEN WR_PickPathSequence = 0 THEN 32767 ELSE WR_PickPathSequence END ASC,
CASE WHEN WR_PickPathSequence = 0 THEN WLV_RowName END ASC,
CASE WHEN WLV_PickPathSequence = 0 THEN 2147483647 ELSE WLV_PickPathSequence END ASC,
WLV_RowName ASC,
WLV_Column ASC,
WLV_Level ASC,
WLV_Tray ASC,
CASE WHEN IsPalletIDNeutral = 0 THEN PalletID ELSE '' END DESC, 
OP_PartNum ASC,
CASE WHEN IsPalletIDNeutral = 1 THEN WZ_Units END ASC,
CASE WHEN IsPalletIDNeutral = 1 THEN OrderedPartAttrib1 ELSE '' END ASC,
CASE WHEN IsPalletIDNeutral = 1 THEN OrderedPartAttrib2 ELSE '' END ASC,
CASE WHEN IsPalletIDNeutral = 1 THEN OrderedPartAttrib3 ELSE '' END ASC,
CASE WHEN IsPalletIDNeutral = 1 THEN OrderedSerialNumber ELSE '' END ASC,
CASE WHEN IsPalletIDNeutral = 1 THEN OrderedExpiryDate ELSE '' END ASC,
CASE WHEN IsPalletIDNeutral = 1 THEN OrderedPackingDate ELSE '' END ASC"; // not feasible to use schema constants in massive query
			}
		}
	}

	#endregion

	#region SortTransferLinesForPicking

	public class SortTransferLinesForPicking : SortByStandardPickingOrPutawayFields<WhsTransferLine>
	{
		protected override IEnumerable<IComparer<WhsTransferLine>> GetElementaryComparers()
		{
			return GetComparersForPickSort(l => l.TransferFromLocation, l => l.SupplierPart, l => l.Docket.Client);
		}
	}

	#endregion

	#region SortTransferLinesForPutaway

	public class SortTransferLinesForPutaway : SortByStandardPickingOrPutawayFields<WhsTransferLine>
	{
		protected override IEnumerable<IComparer<WhsTransferLine>> GetElementaryComparers()
		{
			return GetComparersForPutawaySort(l => l.Location, l => l.SupplierPart);
		}
	}

	#endregion

	#region SortStocktakeLines

	public class SortStocktakeLines : SortByStandardPickingOrPutawayFields<WhsStocktakeLine>
	{
		protected override IEnumerable<IComparer<WhsStocktakeLine>> GetElementaryComparers()
		{
			return GetComparersForPickSort(l => l.Location, l => l.SupplierPart, getClient: l => null);
		}
	}

	#endregion

	#region SortByStandardPickingOrPutawayFields

	public abstract class SortByStandardPickingOrPutawayFields<T> : SortByPropertiesComparer<T>
	{
		protected IComparer<T>[] GetComparersForPickSort(Func<T, WhsLocation> getLocation, Func<T, OrgSupplierPart> getProduct, Func<T, OrgHeader> getClient)
		{
			return GetComparersForSortByClientAndThenByPickMethod(getLocation, getClient).Concat(GetComparersForSortByLocationAndThenByProduct(getLocation, getProduct, true)).ToArray();
		}

		protected IComparer<T>[] GetComparersForPutawaySort(Func<T, WhsLocation> getLocation, Func<T, OrgSupplierPart> getProduct)
		{
			return GetComparersForSortByLocationAndThenByProduct(getLocation, getProduct, false).ToArray();
		}

		protected IComparer<T>[] GetComparersForSortByClientAndThenByPickMethod(Func<T, WhsLocation> getLocation, Func<T, OrgHeader> getClient)
		{
			return new IComparer<T>[]
			{
				new ValueComparer(l => getClient(l)?.OH_Code ?? ZString.Empty),
				new ValueComparer(l =>
				{
					var value = ZString.Empty;
					var location = getLocation(l);
					if (location != null)
					{
						value = location.PickMethods.GetDescriptionFromCode(location.WLV_PickMethod);
					}
					return value;
				})
			};
		}

		protected IComparer<T>[] GetComparersForSortByLocationAndThenByProduct(Func<T, WhsLocation> getLocation, Func<T, OrgSupplierPart> getProduct, bool isPickSort)
		{
			var sequenceComparer = isPickSort ? new ValueComparer(l => getLocation(l)?.WLV_PickPathSequence ?? 0) : new ValueComparer(l => getLocation(l)?.WLV_PutawayPathSequence ?? int.MaxValue);
			return new IComparer<T>[]
			{
				new ValueComparer(l => getLocation(l)?.RowPathSequence ?? 0),
				// If Row Sequence is 0 we need to Sort by Row Name first to retain existing Functionality
				new ValueComparer(l =>
				{
					var value = ZString.Empty;
					var location = getLocation(l);
					if (location != null)
					{
						value = location.RowPathSequence.IsEmpty ? location.RowName : ZString.Empty;
					}
					return value;
				}),
				sequenceComparer,
				new ValueComparer(l => getLocation(l)?.RowName ?? ZString.Empty),
				new ValueComparer(l => getLocation(l)?.WLV_Column ?? 0),
				new ValueComparer(l => getLocation(l)?.WLV_Level ?? 0),
				new ValueComparer(l => getLocation(l)?.WLV_Tray ?? 0),
				new ValueComparer(l => getProduct(l)?.OP_PartNum ?? ZString.Empty)
			};
		}
	}

	#endregion

	#region SortByProduct

	public class SortByProduct : SortByPropertiesComparer<WhsStocktakeLine>
	{
		protected override IEnumerable<IComparer<WhsStocktakeLine>> GetElementaryComparers()
		{
			return new IComparer<WhsStocktakeLine>[]
			{
				new ValueComparer(delegate(WhsStocktakeLine line) { return line.SupplierPart.OP_PartNum; })
			};
		}
	}

	#endregion

	#region SortInventoryByProduct

	public class SortInventoryByProduct : SortByPropertiesComparer<WhsInventoryView>
	{
		protected override IEnumerable<IComparer<WhsInventoryView>> GetElementaryComparers()
		{
			return new IComparer<WhsInventoryView>[]
			{
				new ValueComparer(delegate(WhsInventoryView line) { return line.SupplierPart.OP_PartNum; })
			};
		}
	}

	#endregion

	#region SortByHeldCodeAndThenByProduct

	public class SortByStatusThenByHeldCodeAndThenByProduct : SortByPropertiesComparer<ILineToPutaway>
	{
		protected override IEnumerable<IComparer<ILineToPutaway>> GetElementaryComparers()
		{
			return new IComparer<ILineToPutaway>[]
			{
				new ValueComparer(delegate(ILineToPutaway line) { return line.InventoryStatus; }),
				new ValueComparer(delegate(ILineToPutaway line) { return line.InventoryHeldCode; }),
				new ValueComparer(delegate(ILineToPutaway line) { return line.Product.OP_PartNum; })
			};
		}
	}

	#endregion
}
