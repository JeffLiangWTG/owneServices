using CargoWise.ComponentModel;
using Enterprise.Core;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.GUI
{
	static class CustomsDataGridHelper
	{
		public static void ShowHideCustomsData(ZGrid grid, WhsDocket docket, string[] columnsForCustomsUS)
		{
			var isCustomsTransaction = docket != null
				&& !docket.WD_WW_WhsInfo.HasErrors()
				&& !docket.WD_DocketSubTypeInfo.HasErrors()
				&& docket.IsCustomsTransaction;

			var isCustomsDataVisible = isCustomsTransaction && docket.IsCustomsDataVisible;
			var isBondedEntryKeyVisible = isCustomsTransaction && docket.IsBondedEntryKeyVisibleForCustomsTransactions;

			grid.SetAvailability(isBondedEntryKeyVisible, WhsDocketLineSchema.Constants.WE_BondedEntryKey);
			grid.SetAvailability(isCustomsDataVisible && docket.IsUSWarehouse(), columnsForCustomsUS);
			var isUSFTZWarehouseBondedEnabled = docket?.Warehouse?.IsFTZBondedEnabledAndUSJurisdiction() ?? false;

			grid.SetAvailability(isUSFTZWarehouseBondedEnabled, new[] {
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_ZoneStatus)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_IsFromAnotherFTZWhs)}",
			});

			grid.SetAvailability(isUSFTZWarehouseBondedEnabled && isCustomsDataVisible && docket.WD_DocketType == CodeLists.DocketType.Codes.Order, new[] {
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_OutwardType)}",
			});

			grid.SetAvailability(isCustomsDataVisible, new[]
			{
				nameof(WhsDocketLine.CustomsTariffLookup),
				nameof(WhsDocketLine.CustomsTariffItem),
				nameof(WhsDocketLine.CustomsTariffDesc),
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_EntryKey)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_EntryLineNo)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_EntryDate)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_DeclarationReference)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_CustomsDeadline)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_InwardStyle)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_InwardProcedure)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_RN_NKCountryOfOrigin)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_CustomsQty)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_CustomsUnitOfQty)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_ValueForDuty)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_TILV)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_AddInfo)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_CustomsSecondQuantity)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_CustomsSecondUnitQty)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_Tariff)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_PrimaryPreference)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_CustomsThirdQuantity)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_CustomsThirdUnitQty)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.ManufacturerCode)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_OA_ManufacturerAddress)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_IsMainInwardsProcessedItem)}",
				$"{nameof(WhsDocketLine.CustomsData)}+{nameof(WhsBondedWarehouseAttribute.WB_IsSecondaryInwardsProcessedItem)}",
			});
		}

		static bool IsUSWarehouse(this WhsDocket docket) => docket.CountryCode == Constants.CountryCodes.UnitedStates;
	}
}
