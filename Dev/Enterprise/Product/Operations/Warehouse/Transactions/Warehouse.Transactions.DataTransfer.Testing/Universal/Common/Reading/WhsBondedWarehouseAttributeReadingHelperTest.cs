using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WhsBondedWarehouseAttributeReadingHelperTest : TestCaseWithFactory
	{
		#region TestReadEntryKeyAndOutwardTypeIntoBusinessObject

		public void TestReadEntryKeyAndOutwardTypeIntoBusinessObject()
		{
			var bondedWarehouseAttribute = Factory.New<WhsBondedWarehouseAttribute>();
			var dataObjectReader = GetNewDataObjectReader();
			dataObjectReader.ReadEntryKeyAndOutwardTypeIntoBusinessObject(GetCustomsEntryInfo(), bondedWarehouseAttribute);
			AssertEquals("WB_EntryKey", "KEYZOR", bondedWarehouseAttribute.WB_EntryKey);
			AssertEquals("WB_EntryLineNo", new ZShort(5), bondedWarehouseAttribute.WB_EntryLineNo);
			AssertEquals("WB_OutwardType", "CNN", bondedWarehouseAttribute.WB_OutwardType);
		}

		[TestDate(2024, 5, 23, 12, 34, 56)]
		public void TestPopulateTotalLiabilityAmount()
		{
			var arrivalDate = ZDateTime.Now.AddDays(-1);
			var mockDutyAndTaxCalculator = SetupMockDutyAndTaxCalculator(arrivalDate);
			var bondedWarehouseAttribute = Factory.New<WhsBondedWarehouseAttribute>();
			var dataObjectReader = GetNewDataObjectReader();
			dataObjectReader.ReadCustomsInfoIntoBusinessObject(GetCustomsEntryInfo(), bondedWarehouseAttribute, new UniversalObjectFactory(Factory));
			dataObjectReader.CalculateTotalLiabilityAmount(bondedWarehouseAttribute, CountryCodes.UnitedKingdom, arrivalDate, Factory);
			AssertEquals("WB_AllDutiesAmount should be zero for non-ZA", 0m, bondedWarehouseAttribute.WB_AllDutiesAmount);
			AssertEquals("WB_VATAmount should be zero for non-ZA", 0m, bondedWarehouseAttribute.WB_VATAmount);
			dataObjectReader.CalculateTotalLiabilityAmount(bondedWarehouseAttribute, CountryCodes.SouthAfrica, arrivalDate, Factory);
			AssertEquals("WB_AllDutiesAmount should be non-zero for ZA", 10m, bondedWarehouseAttribute.WB_AllDutiesAmount);
			AssertEquals("WB_VATAmount should be non-zero for ZA", 2.3m, bondedWarehouseAttribute.WB_VATAmount);
			mockDutyAndTaxCalculator.VerifyAll();
		}

		WhsBondedWarehouseAttributeDataObjectReader GetNewDataObjectReader() => new WhsBondedWarehouseAttributeDataObjectReader(new TestErrorLogger());

		#endregion

		#region TestHelp

		internal static Mock<IWhsInventoryDutyAndTaxCalculator> SetupMockDutyAndTaxCalculator(ZDateTime arrivalDate)
		{
			var mockDutyAndTaxCalculator = new Mock<IWhsInventoryDutyAndTaxCalculator>();
			mockDutyAndTaxCalculator
				.Setup(provider => provider.Calculate(It.IsAny<Dictionary<ZString, IZType>>(),
					It.Is<ZDecimal>(value => value == 1m), // ratio
					It.Is<ZDateTime>(value => value == arrivalDate), // arrivalDate
					It.Is<ZDateTime>(value => value == ZDateTime.Now), // valuationDate
					It.Is<ZString>(value => value == "TRF"), // tariff
					It.Is<ZString>(value => value == "AU"), // countryOfOrigin
					It.Is<ZDecimal>(value => value == 24.5m), // customsValue
					It.Is<ZDecimal>(value => value == 13.3m), // customsQty1
					It.Is<ZString>(value => value == "KG"), // customsUQ1
					It.Is<ZDecimal>(value => value == 64.8m), // customsQty2
					It.Is<ZString>(value => value == "GRM"), // customsUQ2
					It.Is<ZDecimal>(value => value == 12.3m), // customsQty3
					It.Is<ZString>(value => value == "GRM"))) // customsUQ3
				.Callback((Dictionary<ZString, IZType> data, ZDecimal ratio, ZDateTime arrivalDate, ZDateTime valuationDate,
					ZString tariff, ZString countryOfOrigin, ZDecimal customsValue,
					ZDecimal customsQty1, ZString customsUQ1, ZDecimal customsQty2, ZString customsUQ2, ZDecimal customsQty3, ZString customsUQ3) =>
				{
					data.Add("ALLDTY", (ZDecimal)10m);
					data.Add("VAT", (ZDecimal)2.3m);
				});
			var mockDutyAndTaxCalculatorProvider = new Mock<IWhsInventoryDutyAndTaxCalculatorProvider>();
			mockDutyAndTaxCalculatorProvider
				.Setup(provider => provider.GetProviderFor(It.IsAny<BusinessObjectFactory>(), It.Is<ZString>(value => value == CountryCodes.SouthAfrica)))
				.Returns(mockDutyAndTaxCalculator.Object);
			ObjectFactory.Substitute(mockDutyAndTaxCalculatorProvider.Object);
			return mockDutyAndTaxCalculator;
		}

		internal static CustomsEntryInfo GetCustomsEntryInfo(bool includeManufacturer = false)
		{
			var customsData = new CustomsEntryInfo();
			customsData.AdditionalInformation = "This was information that wasn't useful";
			customsData.CountryOfOrigin = new Country { Code = "AU", Name = "Australia" };
			customsData.CustomsQuantity = 13.3m;
			customsData.CustomsQuantityUnit = new CodeDescriptionPair6Char { Code = "KG", Description = "Kilograms" };
			customsData.DeclarationReference = "Polo";
			customsData.EntryDate = new ZDateTime(2011, 1, 1);
			customsData.EntryKey = "KEYZOR";
			customsData.EntryLineNumber = new ZShort(5);
			customsData.InwardsEntryKey = "WHATNIP";
			customsData.InwardsEntryLineNumber = new ZShort(9);
			customsData.TILV = 32.9m;
			customsData.ValueForDuty = 24.5m;
			customsData.CustomsSecondQuantity = 64.8m;
			customsData.CustomsSecondUnitQty = new CodeDescriptionPair6Char { Code = "GRM", Description = "Gram" };
			customsData.Tariff = "TRF";
			customsData.PrimaryPreference = "STANDARD";
			customsData.ZoneStatus = new CodeDescriptionPair { Code = "D", Description = "Domestic Merchandise" };
			customsData.IsFromOtherFTZWarehouse = true;
			customsData.OutwardType = new CodeDescriptionPair { Code = "CNN", Description = "Consumption" };
			customsData.CustomsDeadline = ZDate.Today;
			customsData.InwardStyle = "TT";
			customsData.InwardProcedure = "TT";
			customsData.IsMainInwardsProcessedItem = true;
			customsData.IsSecondaryInwardsProcessedItem = false;

			customsData.CustomsThirdQuantity = 12.3m;
			customsData.CustomsThirdUnitQty = new CodeDescriptionPair6Char { Code = "GRM", Description = "Gram" };
			if (includeManufacturer)
			{
				var manufacturerAddressDataObject = OrganizationAddressTestHelper.GetAddressData(nameof(DocAddressType.Manufacturer), "test", "AUSYD");
				manufacturerAddressDataObject.CompanyName = "MANUFACTURER1";
				manufacturerAddressDataObject.Email = "a1@manufacturer.com";
				customsData.ManufacturerAddress = manufacturerAddressDataObject;
			}

			customsData.DataImportMatchingKey = "ef01234567";

			return customsData;
		}

		internal static void AssertContents(WhsBondedWarehouseAttribute customsDataBO, bool includeManufacturerAddress = false)
		{
			AssertEquals("customsDataBO.WB_AddInfo", "This was information that wasn't useful", customsDataBO.WB_AddInfo);
			AssertEquals("customsDataBO.WB_CustomsQty", 13.3m, customsDataBO.WB_CustomsQty);
			AssertEquals("customsDataBO.WB_CustomsUnitOfQty", "KG", customsDataBO.WB_CustomsUnitOfQty);
			AssertEquals("customsDataBO.WB_DeclarationReference", "Polo", customsDataBO.WB_DeclarationReference);
			AssertEquals("customsDataBO.WB_EntryDate", new ZDateTime(2011, 1, 1), customsDataBO.WB_EntryDate);
			AssertEquals("customsDataBO.WB_EntryKey", "KEYZOR", customsDataBO.WB_EntryKey);
			AssertEquals("customsDataBO.WB_EntryLineNo", new ZShort(5), customsDataBO.WB_EntryLineNo);
			AssertEquals("customsDataBO.WB_RN_NKCountryOfOrigin", "AU", customsDataBO.WB_RN_NKCountryOfOrigin);
			AssertEquals("customsDataBO.WB_TILV", 32.9m, customsDataBO.WB_TILV);
			AssertEquals("customsDataBO.WB_ValueForDuty", 24.5m, customsDataBO.WB_ValueForDuty);
			AssertEquals("customsDataBO.WB_CustomsSecondQuantity", 64.8m, customsDataBO.WB_CustomsSecondQuantity);
			AssertEquals("customsDataBO.WB_CustomsSecondUnitQty", "GRM", customsDataBO.WB_CustomsSecondUnitQty);
			AssertEquals("customsDataBO.WB_Tariff", "TRF", customsDataBO.WB_Tariff);
			AssertEquals("customsDataBO.WB_PrimaryPreference", "STANDARD", customsDataBO.WB_PrimaryPreference);
			AssertEquals("customsDataBO.WB_CustomsThirdQuantity", 12.3m, customsDataBO.WB_CustomsThirdQuantity);
			AssertEquals("customsDataBO.WB_CustomsThirdUnitQty", "GRM", customsDataBO.WB_CustomsThirdUnitQty);
			AssertEquals("customsDataBO.WB_ZoneStatus", "D", customsDataBO.WB_ZoneStatus);
			AssertEquals("customsDataBO.WB_IsFromAnotherFTZWhs", true, customsDataBO.WB_IsFromAnotherFTZWhs);
			AssertEquals("customsDataBO.OutwardType", "CNN", customsDataBO.WB_OutwardType);
			AssertEquals("customsDataBO.WB_CustomsDeadline", ZDate.Today, customsDataBO.WB_CustomsDeadline);
			AssertEquals("customsDataBO.WB_InwardStyle", "TT", customsDataBO.WB_InwardStyle);
			AssertEquals("customsDataBO.WB_InwardProcedure", "TT", customsDataBO.WB_InwardProcedure);
			if (includeManufacturerAddress)
			{
				var addressDetail = (IAddressDetails)customsDataBO.ManufacturerAddress;
				AssertEquals("customsDataBO.WB_ManufacturerAddress", "MANUFACTURER1", addressDetail.CompanyName);
				AssertEquals("customsDataBO.WB_ManufacturerAddress", "a1@manufacturer.com", addressDetail.Email);
			}
			AssertEquals("customsDataBO.WB_MatchingKey", "ef01234567", customsDataBO.WB_MatchingKey);
		}

		#endregion
	}
}
