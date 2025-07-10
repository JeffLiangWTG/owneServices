using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	public class WhsBondedWarehouseAttributeDataObjectWriterTest : TestCaseWithFactory
	{
		#region TestBasicCustomsLevelFieldMappings

		public void TestBasicCustomsLevelFieldMappings()
		{
			var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
			whsOrder.WD_DocketSubType = "CUS";

			var line = whsOrder.Lines.AddNew();
			var customsData = GetCustomsData(line);
			var customsDataObject = new WhsBondedWarehouseAttributeDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, customsData))).GetDataObject(customsData);

			AssertNotNull("customsDataObject", customsDataObject);

			CombineAssertions(delegate
			{
				AssertContents(customsDataObject);
			});
		}

		public void TestBasicCustomsLevelFieldMappings_NoManufacturer()
		{
			var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
			whsOrder.WD_DocketSubType = "CUS";

			var line = whsOrder.Lines.AddNew();
			var customsDataObject = new WhsBondedWarehouseAttributeDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, line.CustomsData))).GetDataObject(line.CustomsData);

			AssertNotNull("customsDataObject", customsDataObject);

			CombineAssertions(delegate
			{
				AssertNull("Since no Manufacturer is set ManufacturerAddress must be null.", customsDataObject.ManufacturerAddress);
			});
		}

		#endregion

		#region Implementation

		internal static void AssertContents(CustomsEntryInfo customsDataObject)
		{
			AssertEquals("customsDataObject.AdditionalInformation", "There is no Add to Subtract", customsDataObject.AdditionalInformation);
			AssertEquals("customsDataObject.CountryOfOrigin.Code", "AU", customsDataObject.CountryOfOrigin.Code);
			AssertEquals("customsDataObject.CountryOfOrigin.Name", "Australia", customsDataObject.CountryOfOrigin.Name);
			AssertEquals("customsDataObject.CustomsQuantity", 11.1m, customsDataObject.CustomsQuantity);
			AssertEquals("customsDataObject.CustomsQuantityUnit.Code", "CF", customsDataObject.CustomsQuantityUnit.Code);
			AssertEquals("customsDataObject.CustomsQuantityUnit.Description", "Cubic Feet", customsDataObject.CustomsQuantityUnit.Description);
			AssertEquals("customsDataObject.DeclarationReference", "DECLARE THIS ROCK HARD CANDY!", customsDataObject.DeclarationReference);
			AssertEquals("customsDataObject.EntryDate", new ZDateTime(2011, 1, 1), customsDataObject.EntryDate);
			AssertEquals("customsDataObject.EntryKey", "The Key to my Heart", customsDataObject.EntryKey);
			AssertEquals("customsDataObject.EntryLineNumber", new ZShort(5), customsDataObject.EntryLineNumber);
			AssertEquals("customsDataObject.TILV", 22.2m, customsDataObject.TILV);
			AssertEquals("customsDataObject.ValueForDuty", 33.3m, customsDataObject.ValueForDuty);
			AssertEquals("customsDataObject.CustomsSecondQuantity", 12m, customsDataObject.CustomsSecondQuantity);
			AssertEquals("customsDataObject.CustomsSecondUnitQty.Code", "KN", customsDataObject.CustomsSecondUnitQty.Code);
			AssertEquals("customsDataObject.Tariff", "87120010", customsDataObject.Tariff);
			AssertEquals("customsDataObject.PrimaryPreference", "STANDARD", customsDataObject.PrimaryPreference);
			AssertEquals("customsDataObject.CustomsThirdQuantity", 24m, customsDataObject.CustomsThirdQuantity);
			AssertEquals("customsDataObject.CustomsThirdUnitQty.Code", "LI", customsDataObject.CustomsThirdUnitQty.Code);
			AssertEquals("customsDataObject.ManufacturerAddress.OrganizationCode must be exported.", "Test_Org", customsDataObject.ManufacturerAddress.OrganizationCode);
			AssertEquals("customsDataObject.ManufacturerAddress must be exported.", "Test Address1", customsDataObject.ManufacturerAddress.Address1);
			AssertEquals("customsDataObject.ManufacturerAddress type must be None.", nameof(OrganisationTypes.None), customsDataObject.ManufacturerAddress.AddressType);
			AssertEquals("customsDataObject.CustomsThirdQuantity", "Z", customsDataObject.ZoneStatus.Code);
			AssertEquals("customsDataObject.CustomsThirdQuantity", true, customsDataObject.IsFromOtherFTZWarehouse);
			AssertEquals("customsDataObject.CustomsThirdQuantity", "TOF", customsDataObject.OutwardType.Code);
			AssertEquals("customsDataObject.CustomsDeadline", new ZDate(2020, 1, 1), customsDataObject.CustomsDeadline);
			AssertEquals("customsDataObject.InwardStyle", "S1", customsDataObject.InwardStyle);
			AssertEquals("customsDataObject.InwardProcedure", "P1", customsDataObject.InwardProcedure);
			AssertEquals("customsDataObject.IsMainInwardsProcessedItem", true, customsDataObject.IsMainInwardsProcessedItem);
			AssertEquals("customsDataObject.IsSecondaryInwardsProcessedItem", true, customsDataObject.IsSecondaryInwardsProcessedItem);
			AssertEquals("customsDataObject.AllDutiesAmount", 10m, customsDataObject.AllDutiesAmount);
			AssertEquals("customsDataObject.VATAmount", 2.3m, customsDataObject.VATAmount);
		}

		internal static WhsBondedWarehouseAttribute GetCustomsData(WhsDocketLine line)
		{
			line.WE_BondedEntryKey = "InwardsKey-1";

			var customsData = line.CustomsData;
			customsData.WB_AddInfo = "There is no Add to Subtract";
			customsData.WB_CustomsQty = 11.1m;
			customsData.WB_CustomsUnitOfQty = "CF";
			customsData.WB_DeclarationReference = "DECLARE THIS ROCK HARD CANDY!";
			customsData.WB_EntryDate = new ZDateTime(2011, 1, 1);
			customsData.WB_EntryKey = "The Key to my Heart";
			customsData.WB_EntryLineNo = 5;
			customsData.WB_RN_NKCountryOfOrigin = "AU";
			customsData.WB_TILV = 22.2m;
			customsData.WB_ValueForDuty = 33.3m;
			customsData.WB_CustomsSecondQuantity = 12m;
			customsData.WB_CustomsSecondUnitQty = "KN";
			customsData.WB_Tariff = "87120010";
			customsData.WB_PrimaryPreference = "STANDARD";
			customsData.WB_CustomsThirdQuantity = 24m;
			customsData.WB_CustomsThirdUnitQty = "LI";
			customsData.WB_ZoneStatus = "Z";
			customsData.WB_IsFromAnotherFTZWhs = true;
			customsData.WB_OutwardType = "TOF";
			customsData.WB_CustomsDeadline = new ZDate(2020, 1, 1);
			customsData.WB_InwardStyle = "S1";
			customsData.WB_InwardProcedure = "P1";
			customsData.WB_IsMainInwardsProcessedItem = true;
			customsData.WB_IsSecondaryInwardsProcessedItem = true;
			customsData.WB_AllDutiesAmount = 10m;
			customsData.WB_VATAmount = 2.3m;

			var manufacturerOrg = line.Factory.NewWithValidTestData<OrgHeader>();
			manufacturerOrg.OH_Code = "Test_Org";
			var manufacturerAddress = manufacturerOrg.MainAddress;
			manufacturerAddress.Address1 = "Test Address1";
			customsData.WB_OA_ManufacturerAddress = manufacturerAddress.PK;

			return customsData;
		}

		#endregion
	}
}
