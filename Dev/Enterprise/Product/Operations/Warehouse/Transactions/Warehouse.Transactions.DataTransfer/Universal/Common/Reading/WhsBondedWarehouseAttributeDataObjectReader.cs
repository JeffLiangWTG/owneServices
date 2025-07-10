using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	class WhsBondedWarehouseAttributeDataObjectReader : DataObjectReader
	{
		internal WhsBondedWarehouseAttributeDataObjectReader(IXmlImportLogger logger)
			: base(logger)
		{
		}

		internal void ReadEntryKeyAndOutwardTypeIntoBusinessObject(CustomsEntryInfo customsData, WhsBondedWarehouseAttribute customsDataBO)
		{
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_EntryKey, customsData.EntryKey);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_EntryLineNo, customsData.EntryLineNumber);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_OutwardType, customsData.OutwardType);
		}

		internal void ReadCustomsInfoIntoBusinessObject(CustomsEntryInfo customsData, WhsBondedWarehouseAttribute customsDataBO, UniversalObjectFactory factory)
		{
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_AddInfo, customsData.AdditionalInformation);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_CustomsQty, customsData.CustomsQuantity);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_CustomsUnitOfQty, customsData.CustomsQuantityUnit);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_DeclarationReference, customsData.DeclarationReference);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_EntryDate, customsData.EntryDate);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_RN_NKCountryOfOrigin, customsData.CountryOfOrigin);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_TILV, customsData.TILV);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_ValueForDuty, customsData.ValueForDuty);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_CustomsSecondQuantity, customsData.CustomsSecondQuantity);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_CustomsSecondUnitQty, customsData.CustomsSecondUnitQty);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_Tariff, customsData.Tariff);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_PrimaryPreference, customsData.PrimaryPreference);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_CustomsThirdQuantity, customsData.CustomsThirdQuantity);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_CustomsThirdUnitQty, customsData.CustomsThirdUnitQty);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_ZoneStatus, customsData.ZoneStatus);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_IsFromAnotherFTZWhs, customsData.IsFromOtherFTZWarehouse);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_CustomsDeadline, customsData.CustomsDeadline);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_InwardStyle, customsData.InwardStyle);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_InwardProcedure, customsData.InwardProcedure);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_MatchingKey, customsData.DataImportMatchingKey);
			var manufacturerAddressDataObject = customsData.ManufacturerAddress;
			if (manufacturerAddressDataObject != null)
			{
				var orgReader = new OrganisationDataObjectReader(manufacturerAddressDataObject, logger, factory);
				var address = orgReader.GetMatched();
				if (address != null)
				{
					SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_OA_ManufacturerAddress, address.PK);
				}
			}
		}

		internal void ReadMainAndSecondaryInwardsProcessedItemIntoBusinessObject(CustomsEntryInfo customsData, WhsBondedWarehouseAttribute customsDataBO)
		{
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_IsMainInwardsProcessedItem, customsData.IsMainInwardsProcessedItem);
			SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_IsSecondaryInwardsProcessedItem, customsData.IsSecondaryInwardsProcessedItem);
		}

		internal void CalculateTotalLiabilityAmount(WhsBondedWarehouseAttribute customsDataBO, ZString countryCode, ZDateTime arrivalDate, BusinessObjectFactory factory)
		{
			if (customsDataBO.WB_AllDutiesAmount.IsEmpty || customsDataBO.WB_VATAmount.IsEmpty)
			{
				var dutyAndTaxCalculator = GetDutyAndTaxCalculator(factory, countryCode);
				if (dutyAndTaxCalculator != null)
				{
					var (allDuties, vat) = CalculateTotalLiabilityAmount(dutyAndTaxCalculator, customsDataBO, ZDateTime.Now, arrivalDate);
					SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_AllDutiesAmount, allDuties);
					SetValue(customsDataBO, WhsBondedWarehouseAttributeSchema.WB_VATAmount, vat);
				}
			}
		}

		IWhsInventoryDutyAndTaxCalculator GetDutyAndTaxCalculator(BusinessObjectFactory factory, ZString countryCode)
		{
			try
			{
				var dutyAndTaxCalculatorProvider = ObjectFactory.Get<IWhsInventoryDutyAndTaxCalculatorProvider>();
				return dutyAndTaxCalculatorProvider.GetProviderFor(factory, countryCode);
			}
			catch (NotImplementedException)
			{
				return null;
			}
		}

		(decimal allDuties, decimal vat) CalculateTotalLiabilityAmount(IWhsInventoryDutyAndTaxCalculator dutyAndTaxCalculator, WhsBondedWarehouseAttribute customsDataBO,
			ZDateTime valuationDate, ZDateTime arrivalDate)
		{
			var customsTariffCode = customsDataBO.WB_Tariff;
			var countryOfOrigin = customsDataBO.WB_RN_NKCountryOfOrigin;
			var customsQty = customsDataBO.WB_CustomsQty;
			var customsValue = customsDataBO.WB_ValueForDuty;
			var customsUnitOfQty = customsDataBO.WB_CustomsUnitOfQty;
			var customsSecondQuantity = customsDataBO.WB_CustomsSecondQuantity;
			var customsSecondUnitQty = customsDataBO.WB_CustomsSecondUnitQty;
			var customsThirdQuantity = customsDataBO.WB_CustomsThirdQuantity;
			var customsThirdUnitQty = customsDataBO.WB_CustomsThirdUnitQty;

			var results = new Dictionary<ZString, IZType>();
			dutyAndTaxCalculator.Calculate(results, ratio: 1, arrivalDate, valuationDate, customsTariffCode, countryOfOrigin, customsValue,
				customsQty, customsUnitOfQty, customsSecondQuantity, customsSecondUnitQty, customsThirdQuantity, customsThirdUnitQty);
			return (GetDecimalValue(results["ALLDTY"]), GetDecimalValue(results["VAT"]));
		}

		decimal GetDecimalValue(IZType value) => value is ZDecimal @decimal ? @decimal : 0m;
	}
}
