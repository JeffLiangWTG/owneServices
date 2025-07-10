using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsBondedWarehouseAttributeDataObjectWriter : DataObjectWriter<WhsBondedWarehouseAttribute, CustomsEntryInfo>
	{
		internal WhsBondedWarehouseAttributeDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override CustomsEntryInfo PopulateDataObject(WhsBondedWarehouseAttribute customsDataBO)
		{
			var customsData = new CustomsEntryInfo();

			customsData.AdditionalInformation = customsDataBO.WB_AddInfo;
			customsData.CountryOfOrigin = ListHelper.GetWithName<Country>(customsDataBO.WB_RN_NKCountryOfOrigin, customsDataBO.Lookups.CountryOfOrigins);
			customsData.CustomsQuantity = customsDataBO.WB_CustomsQty;
			customsData.CustomsQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(customsDataBO.WB_CustomsUnitOfQty, customsDataBO.Lookups.UQList);
			customsData.DeclarationReference = customsDataBO.WB_DeclarationReference;
			customsData.EntryDate = customsDataBO.WB_EntryDate;
			customsData.EntryKey = customsDataBO.WB_EntryKey;
			customsData.EntryLineNumber = customsDataBO.WB_EntryLineNo;
			customsData.TILV = customsDataBO.WB_TILV;
			customsData.ValueForDuty = customsDataBO.WB_ValueForDuty;
			customsData.CustomsSecondQuantity = customsDataBO.WB_CustomsSecondQuantity;
			customsData.CustomsSecondUnitQty = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(customsDataBO.WB_CustomsSecondUnitQty, customsDataBO.Lookups.UQList);
			customsData.Tariff = customsDataBO.WB_Tariff;
			customsData.PrimaryPreference = customsDataBO.WB_PrimaryPreference;
			customsData.CustomsThirdQuantity = customsDataBO.WB_CustomsThirdQuantity;
			customsData.CustomsThirdUnitQty = ListHelper.GetWithDescription<CodeDescriptionPair6Char>(customsDataBO.WB_CustomsThirdUnitQty, customsDataBO.Lookups.UQList);
			customsData.ManufacturerAddress = new OrganizationDataObjectWriter(writeManager, nameof(OrganisationTypes.None)).GetDataObject(customsDataBO.ManufacturerAddress);
			customsData.ZoneStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(customsDataBO.WB_ZoneStatus, customsDataBO.Lookups.ZoneStatusList);
			customsData.IsFromOtherFTZWarehouse = customsDataBO.WB_IsFromAnotherFTZWhs;
			customsData.OutwardType = ListHelper.GetWithDescription<CodeDescriptionPair>(customsDataBO.WB_OutwardType, customsDataBO.Lookups.OutwardTypes);
			customsData.CustomsDeadline = customsDataBO.WB_CustomsDeadline;
			customsData.InwardStyle = customsDataBO.WB_InwardStyle;
			customsData.InwardProcedure = customsDataBO.WB_InwardProcedure;
			customsData.IsMainInwardsProcessedItem = customsDataBO.WB_IsMainInwardsProcessedItem;
			customsData.IsSecondaryInwardsProcessedItem = customsDataBO.WB_IsSecondaryInwardsProcessedItem;
			customsData.AllDutiesAmount = customsDataBO.WB_AllDutiesAmount;
			customsData.VATAmount = customsDataBO.WB_VATAmount;

			return customsData;
		}
	}
}
