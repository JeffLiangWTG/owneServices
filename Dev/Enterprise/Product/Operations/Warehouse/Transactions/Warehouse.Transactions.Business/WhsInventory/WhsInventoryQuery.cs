using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region Stock Filter Builder

	public static class WhsInventoryFilterBuilder
	{
		public static ZQuery BuildFilter(
			OrgHeader client,
			OrgSupplierPart part,
			WhsWarehouse warehouse,
			WhsArea area,
			WhsLocation location,
			ZString palletID,
			ZString bondedEntryKey,
			ZDate expiryDate,
			ZDate packingDate,
			ZString partAttrib1,
			ZString partAttrib2,
			ZString partAttrib3,
			ZString serialNumber,
			ZDateTimeOffset arrivalDate,
			string inventoryStatus = "",
			bool isMandatoryCheck = false)
		{
			var filter = GetFilterObject(warehouse, area, location);
			AddClientToFilter(client, filter);
			AddPartToFilter(part, filter);
			AddPalletIDToFilter(palletID, isMandatoryCheck, filter);
			AddArrivalDateToFilter(arrivalDate, filter);
			AddExpiryDateToFilter(expiryDate, isMandatoryCheck, filter);
			AddPackingDateToFilter(packingDate, isMandatoryCheck, filter);
			AddPartAttributeOneToFilter(partAttrib1, isMandatoryCheck, filter);
			AddPartAttributeTwoToFilter(partAttrib2, isMandatoryCheck, filter);
			AddPartAttributeThreeToFilter(partAttrib3, isMandatoryCheck, filter);
			AddSerialNumberToFilter(serialNumber, isMandatoryCheck, filter);
			AddTotalUnitsToFilter(filter);
			AddInventoryStatusToFilter(inventoryStatus, filter);
			AddBondedKeyToFilter(bondedEntryKey, isMandatoryCheck, filter);

			var dbOnlyQuery = filter as ZDBOnlyQuery;
			if (location != null)
			{
				filter.AddToFilter(WhsInventoryViewSchema.WI_WL, location.PK);
			}
			else if (warehouse != null || area != null)
			{
				// omfg i hate these things grrr =p
				var areaFilter = new ZDBOnlySubQuery(typeof(WhsArea), WhsLocationViewSchema.WLV_WA_PickingArea);
				if (area != null)
				{
					areaFilter.AddToFilter(WhsAreaSchema.PK, area.PK);
				}

				if (warehouse != null)
				{
					areaFilter.AddToFilter(WhsAreaSchema.WA_WW_Whs, warehouse.PK);
				}

				var locationFilter = new ZDBOnlySubQuery(typeof(WhsLocation), WhsInventoryViewSchema.WI_WL);
				locationFilter.AddSubQuery(areaFilter, JoinCondition.And);
				dbOnlyQuery.AddSubQuery(locationFilter, JoinCondition.And);
			}

			return filter;
		}

		static void AddClientToFilter(OrgHeader client, ZQuery filter)
		{
			if (client != null)
			{
				filter.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, client.PK);
			}
		}

		static void AddPartToFilter(OrgSupplierPart part, ZQuery filter)
		{
			if (part != null)
			{
				filter.AddToFilter(WhsInventoryViewSchema.WI_OP, part.PK);
			}
		}

		static void AddPalletIDToFilter(ZString palletID, bool isMandatoryCheck, ZQuery filter)
		{
			if (!palletID.IsEmpty || isMandatoryCheck)
			{
				filter.AddToFilter(WhsInventoryViewSchema.WI_PalletID, palletID);

				if (!palletID.IsEmpty)
				{
					filter.AddToFilter(WhsInventoryViewSchema.WI_PalletID, SQLComparisonOperator.NotEqual, ZString.Empty);
				}
			}
		}

		static void AddArrivalDateToFilter(ZDateTimeOffset arrivalDate, ZQuery filter)
		{
			if (arrivalDate.IsValid)
			{
				filter.AddToFilter(WhsInventoryViewSchema.WI_ArrivalDate, SQLComparisonOperator.EqualToDatePartOnly, arrivalDate);
			}
		}

		static void AddExpiryDateToFilter(ZDate expiryDate, bool isMandatoryCheck, ZQuery filter)
		{
			if (expiryDate.IsValid || (isMandatoryCheck && expiryDate.IsEmpty))
			{
				filter.AddToFilter(WhsInventoryViewSchema.WI_ExpiryDate, expiryDate);
			}
		}

		static void AddPackingDateToFilter(ZDate packingDate, bool isMandatoryCheck, ZQuery filter)
		{
			if (packingDate.IsValid || (isMandatoryCheck && packingDate.IsEmpty))
			{
				filter.AddToFilter(WhsInventoryViewSchema.WI_PackingDate, packingDate);
			}
		}

		static void AddPartAttributeOneToFilter(ZString partAttrib1, bool isMandatoryCheck, ZQuery filter)
		{
			if (!partAttrib1.IsEmpty || isMandatoryCheck)
			{
				filter.AddToFilter(WhsInventoryViewSchema.WI_PartAttrib1, partAttrib1);
			}
		}

		static void AddPartAttributeTwoToFilter(ZString partAttrib2, bool isMandatoryCheck, ZQuery filter)
		{
			if (!partAttrib2.IsEmpty || isMandatoryCheck)
			{
				filter.AddToFilter(WhsInventoryViewSchema.WI_PartAttrib2, partAttrib2);
			}
		}

		static void AddPartAttributeThreeToFilter(ZString partAttrib3, bool isMandatoryCheck, ZQuery filter)
		{
			if (!partAttrib3.IsEmpty || isMandatoryCheck)
			{
				filter.AddToFilter(WhsInventoryViewSchema.WI_PartAttrib3, partAttrib3);
			}
		}

		static void AddSerialNumberToFilter(ZString serialNumber, bool isMandatoryCheck, ZQuery filter)
		{
			if (!serialNumber.IsEmpty || isMandatoryCheck)
			{
				filter.AddToFilter(WhsInventoryViewSchema.WI_SerialNumber, serialNumber);
			}
		}

		static void AddTotalUnitsToFilter(ZQuery filter)
		{
			filter.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);
		}

		static void AddInventoryStatusToFilter(string inventoryStatus, ZQuery filter)
		{
			if (!string.IsNullOrEmpty(inventoryStatus))
			{
				filter.AddToFilter(WhsInventoryViewSchema.WI_InventoryStatus, inventoryStatus);
			}
		}

		static void AddBondedKeyToFilter(ZString bondedEntryKey, bool isMandatoryCheck, ZQuery filter)
		{
			if (!bondedEntryKey.IsEmpty || isMandatoryCheck)
			{
				if (bondedEntryKey.EndsWith("-", StringComparison.Ordinal))
				{
					filter.AddToFilter(WhsInventoryViewSchema.WI_BondedEntryKey, SQLComparisonOperator.StartsWith, bondedEntryKey);
				}
				else
				{
					filter.AddToFilter(WhsInventoryViewSchema.WI_BondedEntryKey, bondedEntryKey);
				}
			}
		}

		#region Implementation

		static ZQuery GetFilterObject(WhsWarehouse warehouse, WhsArea area, WhsLocation location)
		{
			if (location != null || (warehouse == null && area == null))
			{
				return new ZQuery();
			}
			else
			{
				return new ZDBOnlyQuery(typeof(WhsInventoryView));
			}
		}

		#endregion
	}

	#endregion
}
