using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PackageComparerForLabelPrinting : IComparer<PkgPackage>
	{
		public PackageComparerForLabelPrinting(WhsPick pick)
		{
			Pick = Argument.NotNull(pick, nameof(pick));
		}

		readonly WhsPick Pick;

		#region PackTypeToUOMType

		Dictionary<ZString, ZString> PackTypeToUOMType
		{
			get { return packTypeToUOMType ?? (packTypeToUOMType = new Dictionary<ZString, ZString>()); }
		}

		Dictionary<ZString, ZString> packTypeToUOMType;

		#endregion

		#region Compare

		public int Compare(PkgPackage x, PkgPackage y)
		{
			Argument.NotNull(x, nameof(x));
			Argument.NotNull(y, nameof(y));

			var compareResult = 0;

			if (x.PackedItemDivots.Count > 0 && y.PackedItemDivots.Count > 0)
			{
				var pickLineX = new PackagePackingHelper(x).GetPickLineFromPackage();
				var pickLineY = new PackagePackingHelper(y).GetPickLineFromPackage();
				if (pickLineX != null && pickLineY != null)
				{
					// first check by area
					var inventoryLineX = pickLineX.InventoryLineForAvailableInventory;
					var inventoryLineY = pickLineY.InventoryLineForAvailableInventory;
					compareResult = inventoryLineX.LocationAreaName.CompareTo(inventoryLineY.LocationAreaName);

					// then by UOM type (if needed)
					if (compareResult == 0)
					{
						var uomX = GetUOMTypeForRefPackType(pickLineX.WZ_F3_NKAllocatedPackType);
						var uomY = GetUOMTypeForRefPackType(pickLineY.WZ_F3_NKAllocatedPackType);

						compareResult = UOMTypesSortingWeight[uomX].CompareTo(UOMTypesSortingWeight[uomY]);
					}

					// then by pick group
					if (compareResult == 0)
					{
						var orderLineX = pickLineX.DocketLine;
						var orderLineY = pickLineY.DocketLine;

						compareResult = orderLineX.WE_PickGroup.CompareTo(orderLineY.WE_PickGroup);
					}

					// then by location
					if (compareResult == 0)
					{
						compareResult = LocationsSorter.Compare(inventoryLineX, inventoryLineY);
					}
				}
			}

			return compareResult;
		}

		SortLocationsForPicking LocationsSorter => locationsSorter ?? (locationsSorter = new SortLocationsForPicking());
		SortLocationsForPicking locationsSorter;

		class SortLocationsForPicking : SortByStandardPickingOrPutawayFields<WhsDocketLine>
		{
			protected override IEnumerable<IComparer<WhsDocketLine>> GetElementaryComparers()
			{
				return GetComparersForSortByLocationAndThenByProduct(l => l.Location, getProduct: l => null, isPickSort: true);
			}
		}

		#region GetUOMTypeForRefPackType

		ZString GetUOMTypeForRefPackType(ZString quantityUQ)
		{
			ZString uomType;

			if (!PackTypeToUOMType.TryGetValue(quantityUQ, out uomType))
			{
				var refPackType = Pick.Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, quantityUQ);
				uomType = refPackType?.F3_UOMType ?? "";
				if (uomType == "")
				{
					uomType = UOMPackTypesList.Codes.SplitCase;
				}
				PackTypeToUOMType.Add(quantityUQ, uomType);
			}

			return uomType;
		}

		readonly Dictionary<ZString, int> UOMTypesSortingWeight = new Dictionary<ZString, int>()
		{
			{ UOMPackTypesList.Codes.Pallet, 0 },
			{ UOMPackTypesList.Codes.Case, 1 },
			{ UOMPackTypesList.Codes.SplitCase, 2 },
		};

		#endregion

		#endregion

		#region GetDelimeterTypeBetweenPackages

		public DocDelimeterType GetDelimeterTypeBetweenPackages(PkgPackage currentPackage, PkgPackage nextPackage)
		{
			var result = DocDelimeterType.None;

			if (!Pick.IsPrintingWithoutSeparatorLabels && currentPackage != null)
			{
				var pickLineCurrent = new PackagePackingHelper(currentPackage).GetPickLineFromPackage();
				if (pickLineCurrent != null)
				{
					var uomCurrent = GetUOMTypeForRefPackType(pickLineCurrent.WZ_F3_NKAllocatedPackType);
					if (nextPackage == null) // this is for the last package
					{
						result |= DocDelimeterType.EndOfArea; // last package is always end of area
						result |= GetDelimeterTypeFromUOMType(uomCurrent);
						if (pickLineCurrent.DocketLine.WE_PickGroup != 0)
						{
							result |= DocDelimeterType.EndOfPickGroup;
						}
					}
					else if (nextPackage.PackedItemDivots.Count > 0)
					{
						var pickLineNext = new PackagePackingHelper(nextPackage).GetPickLineFromPackage();
						if (pickLineNext != null)
						{
							if (pickLineCurrent.InventoryLineForAvailableInventory.LocationAreaName.CompareTo(pickLineNext.InventoryLineForAvailableInventory.LocationAreaName) != 0)
							{
								result |= DocDelimeterType.EndOfArea;
							}

							var uomNext = GetUOMTypeForRefPackType(pickLineNext.WZ_F3_NKAllocatedPackType);
							if (uomCurrent != uomNext)
							{
								result |= GetDelimeterTypeFromUOMType(uomCurrent);
								if (pickLineCurrent.DocketLine.WE_PickGroup != 0)
								{
									result |= DocDelimeterType.EndOfPickGroup;
								}
							}

							if (pickLineCurrent.DocketLine.WE_PickGroup != pickLineNext.DocketLine.WE_PickGroup)
							{
								result |= DocDelimeterType.EndOfPickGroup;
							}
						}
					}
				}
			}

			return result;
		}

		DocDelimeterType GetDelimeterTypeFromUOMType(ZString uomType)
		{
			switch (uomType)
			{
				case UOMPackTypesList.Codes.Pallet:
					return DocDelimeterType.EndOfPallet;

				case UOMPackTypesList.Codes.Case:
					return DocDelimeterType.EndOfCase;

				case UOMPackTypesList.Codes.SplitCase:
					return DocDelimeterType.EndOfSplitCase;

				default:
					return DocDelimeterType.None;
			}
		}

		[Flags]
		public enum DocDelimeterType
		{
			None = 0,
			EndOfArea = 1,
			EndOfPallet = 2,
			EndOfCase = 4,
			EndOfSplitCase = 8,
			EndOfPickGroup = 16
		}

		#endregion
	}
}
