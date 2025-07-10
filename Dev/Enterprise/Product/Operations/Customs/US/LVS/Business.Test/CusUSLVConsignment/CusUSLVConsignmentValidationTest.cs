using System.Linq;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	internal class CusUSLVConsignmentValidationTest : BusinessObjectValidationTestCase
	{
		#region First Item Validation

		public void TestFirstCusUSLVItemProductCode_WarnIfSupplierOrImporterAreNotEntered()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckProductCode_WarnIfSupplierOrImporterAreNotEntered(consignment.FirstCusUSLVItem, consignment.FirstCusUSLVItemProductCodeInfo);
		}

		public void TestCheckCusUSLVItemProductCode_WarnWhenFoundButNotRelated()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckProductCode_WarnWhenFoundButNotRelated(consignment.FirstCusUSLVItem, consignment.FirstCusUSLVItemProductCodeInfo);
		}

		public void TestCheckCusUSLVItemProductCode_WarnWhenPartFoundButInactive()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckProductCode_WarnWhenPartFoundButInactive(consignment.FirstCusUSLVItem, consignment.FirstCusUSLVItemProductCodeInfo);
		}

		public void TestCheckCusUSLVItemProductCode_WarnWhenFoundButNotRelated_WithComplexRelationships()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckProductCode_WarnWhenFoundButNotRelated_WithComplexRelationships(consignment.FirstCusUSLVItem, consignment.FirstCusUSLVItemProductCodeInfo);
		}

		public void TestCheckCusUSLVItemProductCode_WarnPartCodeNotFoundAtAll()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckProductCode_WarnPartCodeNotFoundAtAll(consignment.FirstCusUSLVItem, consignment.FirstCusUSLVItemProductCodeInfo);
		}

		public void TestCheckCusUSLVItemProductCode_WarnIfMoreThanOneMatchingPart()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckProductCode_WarnIfMoreThanOneMatchingPart(consignment.FirstCusUSLVItem, consignment.FirstCusUSLVItemProductCodeInfo);
		}

		public void TestCheckCusUSLVItemLineValue()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckLineValue(Factory, clearance, consignment.FirstCusUSLVItem, consignment.FirstCusUSLVItemLineValueInfo);
		}

		public void TestFirstCusUSLVItemTariff()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			new CusUSLVItemValidationTestHelper().CheckTariff(consignment.FirstCusUSLVItemTariffInfo);
		}

		public void TestFirstCusUSLVItemCountryOfOrigin()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckCountryOfOrigin(consignment.FirstCusUSLVItemCountryOfOriginInfo);
		}

		public void TestFirstCusUSLVItemCurrency()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			new CusUSLVItemValidationTestHelper().CheckCurrency(consignment.FirstCusUSLVItemCurrencyInfo);
		}

		public void TestFirstCusUSLVItemGoodsDescription()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment.CusUSLVItems.AddNew();

			new CusUSLVItemValidationTestHelper().CheckGoodsDescription(consignment.FirstCusUSLVItem, consignment.FirstCusUSLVItemGoodsDescriptionInfo);
		}

		public void TestFirstCusUSLVItemAntiDumpingApplies()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment.CusUSLVItems.AddNew();
			new CusUSLVItemValidationTestHelper().CheckAntiDumping(consignment.FirstCusUSLVItemAntiDumpingInfo, consignment.FirstCusUSLVItem);
		}

		public void TestFirstCusUSLVItemCountervailingApplies()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment.CusUSLVItems.AddNew();
			new CusUSLVItemValidationTestHelper().CheckCountervailing(consignment.FirstCusUSLVItemCountervailingInfo, consignment.FirstCusUSLVItem);
		}

		public void TestRunFirstCusUSLVItemValidationAfterModifyItem_ProductCode()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			var item = consignment.CusUSLVItems.AddNew();

			AssertNoWarnings(consignment.FirstCusUSLVItemProductCodeInfo);

			item.PartSyncManager.Enabled = true;
			item.ULI_PartNo = "Pnvalid ProductCode";

			AssertHasWarning(consignment.FirstCusUSLVItemProductCodeInfo, InvoiceLineProductValidationHelper.Warnings.PartCannotBeFoundBeforeEnteringASupplierAndImporter);
		}

		public void TestRunFirstCusUSLVItemValidationAfterModifyItem_Tariff()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			var item = consignment.CusUSLVItems.AddNew();

			AssertNoWarnings(consignment.FirstCusUSLVItemTariffInfo);

			item.ULI_Tariff = "1234";

			AssertHasMessageError(consignment.FirstCusUSLVItemTariffInfo, "Tariff unable to be found.");
		}

		public void TestRunFirstCusUSLVItemValidationAfterModifyItem_CountryOfOrigin()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			var item = consignment.CusUSLVItems.AddNew();

			AssertNoWarnings(consignment.FirstCusUSLVItemCountryOfOriginInfo);

			item.ULI_RN_NKCountryOfOrigin = "@@";

			AssertHasMessageError(consignment.FirstCusUSLVItemCountryOfOriginInfo, "The code you have selected is not in the list.");
		}

		public void TestRunFirstCusUSLVItemValidationAfterModifyItem_Currency()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			var item = consignment.CusUSLVItems.AddNew();

			AssertNoWarnings(consignment.FirstCusUSLVItemCurrencyInfo);

			item.ULI_RX_NKCurrency = "@@";

			AssertEquals("The code you have selected is not in the list.", consignment.FirstCusUSLVItemCurrencyInfo.Notifications.First().Message);
		}

		public void TestRunFirstCusUSLVItemValidationAfterModifyItem_AntiDumping()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			var item = consignment.CusUSLVItems.AddNew();

			AssertNoMessageErrors(consignment.FirstCusUSLVItemAntiDumpingInfo);

			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "A9085290";
			case1.U5_ISOCountryCode = "KR";
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var tariff1 = case1.CaseTariffs.AddNew();
			tariff1.U9_TariffNumber = "0000000000";

			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			var otherTariff = Factory.New<USCTariff>();
			otherTariff.UE_Tariff = "1111111111";
			otherTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			otherTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			Factory.Save();

			item.ULI_RN_NKCountryOfOrigin = "KR";
			item.ULI_Tariff = importTariff.UE_Tariff;

			item.ULI_AntiDumping = false;
			AssertHasMessageErrorContaining(consignment.FirstCusUSLVItemAntiDumpingInfo, "may be subject to ADD");
		}

		public void TestRunFirstCusUSLVItemValidationAfterModifyItem_Countervailing()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			var item = consignment.CusUSLVItems.AddNew();

			AssertNoMessageErrors(consignment.FirstCusUSLVItemCountervailingInfo);

			var case1 = Factory.New<USCACCase>();
			case1.U5_CaseNumber = "C9085290";
			case1.U5_ISOCountryCode = "KR";
			case1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			case1.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

			var tariff1 = case1.CaseTariffs.AddNew();
			tariff1.U9_TariffNumber = "0000000000";

			var importTariff = Factory.New<USCTariff>();
			importTariff.UE_Tariff = "0000000000";
			importTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			importTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			var otherTariff = Factory.New<USCTariff>();
			otherTariff.UE_Tariff = "1111111111";
			otherTariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			otherTariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			Factory.Save();

			item.ULI_RN_NKCountryOfOrigin = "KR";
			item.ULI_Tariff = importTariff.UE_Tariff;

			item.ULI_Countervailing = false;
			AssertHasMessageErrorContaining(consignment.FirstCusUSLVItemCountervailingInfo, "may be subject to CVD");
		}

		public void TestRunFirstCusUSLVItemValidationAfterModifyItem_GoodsDescription()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			var item = consignment.CusUSLVItems.AddNew();

			AssertNoWarnings(consignment.FirstCusUSLVItemCountryOfOriginInfo);

			item.ULI_GoodsDescription = string.Empty;

			AssertNoMessageError(consignment.FirstCusUSLVItemGoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			item.CusUSLVItemPGAs.AddNew();
			Assert("precondition", item.CusUSLVItemPGAs.Count > 0);
			item.ULI_GoodsDescription = string.Empty;
			AssertHasMessageErrorContaining(consignment.FirstCusUSLVItemGoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			consignment.FirstCusUSLVItemGoodsDescription = "ABC";
			AssertNoMessageError(consignment.FirstCusUSLVItemGoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			consignment.FirstCusUSLVItemGoodsDescription = string.Empty;
			AssertHasMessageErrorContaining(consignment.FirstCusUSLVItemGoodsDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#endregion

		public void TestValidateULB_EntryType()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment.ULB_EntryType = ZString.Empty;
			AssertHasError(consignment.ULB_EntryTypeInfo, MandatoryValidation.MustBeEnteredMessage("Entry Type"));

			consignment.ULB_EntryType = "13";
			AssertListValidationInvalidCodeError(consignment.ULB_EntryTypeInfo, isExpectingError: true);
			AssertMandatoryValidationError(consignment.ULB_EntryTypeInfo, isExpectingError: false);

			consignment.ULB_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertListValidationInvalidCodeError(consignment.ULB_EntryTypeInfo, isExpectingError: false);
			AssertMandatoryValidationError(consignment.ULB_EntryTypeInfo, isExpectingError: false);
		}

		public void TestCheckULB_PackType()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment.ULB_PackType = "AAA";
			AssertHasMessageError(consignment.ULB_PackTypeInfo, "The code you have selected is not in the list.");

			consignment.ULB_PackType = "PKG";
			AssertNoMessageErrors(consignment.ULB_PackTypeInfo);
		}

		public void TestCheckULB_RN_NKConsigneeCountry()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment.ULB_RN_NKConsigneeCountry = "KK";
			AssertHasMessageError(consignment.ULB_RN_NKConsigneeCountryInfo, "The code you have selected is not in the list.");

			consignment.ULB_RN_NKConsigneeCountry = "US";
			AssertNoMessageErrors(consignment.ULB_RN_NKConsigneeCountryInfo);
		}

		public void TestCheckULB_RN_NKSellerCountry()
		{
			var consignment = Factory.NewWithValidTestData<CusUSLVConsignment>();
			consignment.ULB_RN_NKSellerCountry = "KK";
			AssertHasMessageError(consignment.ULB_RN_NKSellerCountryInfo, "The code you have selected is not in the list.");

			consignment.ULB_RN_NKSellerCountry = "US";
			AssertNoMessageErrors(consignment.ULB_RN_NKSellerCountryInfo);
		}

		public void TestCheckULB_EquipmentNumber()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = TransportModes.Air;
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_EquipmentNumber = "287189";

			const string warningForLetters = "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.";
			const string warningForDigits = "Container number does not have a valid check (last) digit. The check digit should be";

			Assert(!consignment.ULB_EquipmentNumberInfo.HasNotifications());

			clearance.ULH_TransportMode = TransportModes.Sea;
			consignment.Validation.ValidateULB_EquipmentNumber();
			AssertHasWarning(consignment.ULB_EquipmentNumberInfo, warningForLetters);

			consignment.ULB_EquipmentNumber = "TEST2871891";
			AssertHasWarningContaining(consignment.ULB_EquipmentNumberInfo, warningForDigits);

			clearance.ULH_TransportMode = TransportModes.Air;
			consignment.Validation.ValidateULB_EquipmentNumber();
			Assert(!consignment.ULB_EquipmentNumberInfo.HasNotifications());

			clearance.ULH_TransportMode = TransportModes.Sea;
			consignment.ULB_EquipmentNumber = "TEST2871897";
			Assert(!consignment.ULB_EquipmentNumberInfo.HasNotifications());
		}

		[TestDate(2019, 11, 7)]
		public void TestCheckULB_GoodsValue()
		{
			var grouping = Factory.NewWithValidTestData<RefDataGrouping>();
			grouping.ZZZ_DataGrouping = "US";
			grouping.ZZZ_Description = "United States";
			var deminimus = Factory.New<RefCusTaxOrFee>();
			deminimus.ZZF_ZZZ_NKDataGrouping = "US";
			deminimus.ZZF_Code = "DEM";
			deminimus.ZZF_StartDate = new ZDateTime(1960, 1, 1);
			deminimus.ZZF_EndDate = ZDateTime.Today.AddYears(1);
			deminimus.ZZF_Value = 800;
			deminimus.ZZF_Description = "a value";
			Factory.Save();

			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			usdCurrency.SetCustomsRate(ZDateTime.Now, ZDateTime.Now, 1m);

			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_DepartureDate = ZDate.Today;
			var consignment = clearance.CusUSLVConsignments.AddNew();
			var item1 = consignment.CusUSLVItems.AddNew();
			item1.ULI_GoodsValue = FeeCalculationHelper.GetDeminimus(Factory);
			item1.ULI_RX_NKCurrency = "USD";

			consignment.Validation.ValidateULB_GoodsValue();
			AssertNoMessageErrors(consignment.ULB_GoodsValueInfo);

			var item2 = consignment.CusUSLVItems.AddNew();
			item2.ULI_GoodsValue = 1;
			item2.ULI_RX_NKCurrency = "USD";

			consignment.Validation.ValidateULB_GoodsValue();
			AssertHasMessageErrorContaining(consignment.ULB_GoodsValueInfo, "Value cannot exceed");
		}

		public void TestCheckULB_NumberOfPacks()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			consignment.ULB_NumberOfPacks = 1;

			AssertNoMessageErrors(consignment.ULB_NumberOfPacksInfo);

			consignment.ULB_NumberOfPacks = 0;

			AssertHasMessageErrorContaining(consignment.ULB_NumberOfPacksInfo, "Please enter a Bill quantity.");
		}

		public void TestCheckULB_ConsigneeQualifierAndULB_ConsigneeIdentifier()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			consignment.ULB_ConsigneeQualifier = "Bob";
			consignment.ULB_ConsigneeIdentifier = "";
			AssertHasMessageErrorContaining(consignment.ULB_ConsigneeIdentifierInfo, "Consignee Identifier is mandatory when a Consignee Qualifier has been entered.");

			consignment.ULB_ConsigneeIdentifier = "Doe";
			consignment.ULB_ConsigneeQualifier = "";
			AssertHasMessageErrorContaining(consignment.ULB_ConsigneeQualifierInfo, "Consignee Qualifier is mandatory when a Consignee Identifier has been entered.");
		}

		public void TestHouseBillIssuerSCAC_WhenTransportModeIsSeaRailOrTruck_IsRequired()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			consignment.ULB_ULH = clearance.PK;

			var messageError = "Standard Carrier Alpha Code (SCAC) required for House Bill (when Mode Of Transport is Sea, Rail or Truck.)";

			consignment.Shipment.ULH_TransportMode = "SEA";
			consignment.ULB_HouseBillIssuerSCAC = "";
			consignment.RunPreSaveValidation();
			AssertHasMessageErrorContaining(consignment.ULB_HouseBillIssuerSCACInfo, messageError);

			consignment.Shipment.ULH_TransportMode = "RAI";
			consignment.ULB_HouseBillIssuerSCAC = "";
			consignment.RunPreSaveValidation();
			AssertHasMessageErrorContaining(consignment.ULB_HouseBillIssuerSCACInfo, messageError);

			consignment.Shipment.ULH_TransportMode = "TRK";
			consignment.ULB_HouseBillIssuerSCAC = "";
			consignment.RunPreSaveValidation();
			AssertHasMessageErrorContaining(consignment.ULB_HouseBillIssuerSCACInfo, messageError);
		}

		public void TestHouseBillIssuerSCAC_WhenSCACIs4CharLongAndNotInDatabase_HasErrorMessage()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			consignment.ULB_ULH = clearance.PK;

			Factory.Save();

			var messageError = $"Issuer Standard Carrier Alpha Code (SCAC) unable to be found.When the Declaration is saved, {BrandingFactory.Instance.ProductName} will automatically submit a request for the latest information relating to the Issuer SCAC that has been entered. A response should be available in a few minutes. Note that SCAC Requests can also be sent manually, at any time, by selecting Customs Declarations > Actions > Reference Files Request > Carrier Codes.";

			consignment.Shipment.ULH_TransportMode = "SEA";
			var sCACNotInDB = "XYZ1";
			clearance.ULH_CarrierSCAC = string.Empty;
			consignment.ULB_HouseBillIssuerSCAC = sCACNotInDB;
			AssertHasMessageError(consignment.ULB_HouseBillIssuerSCACInfo, messageError);
		}

		public void TestHouseBillIssuerSCAC_WhenSCACIsNot2CharLongForAir_HasErrorMessage()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			consignment.ULB_ULH = clearance.PK;

			Factory.Save();

			var messageError = "The Standard Carrier Alpha Code (SCAC) is not valid for the selected mode of transport. SCAC should be 2 characters in length for Air and 4 characters in length for other modes of transport.";

			consignment.Shipment.ULH_TransportMode = "AIR";
			consignment.ULB_HouseBillIssuerSCAC = "AA12";
			AssertHasMessageError(consignment.ULB_HouseBillIssuerSCACInfo, messageError);

			consignment.ULB_HouseBillIssuerSCAC = "1";
			AssertHasMessageError(consignment.ULB_HouseBillIssuerSCACInfo, messageError);
		}

		public void TestHouseBillIssuerSCAC_WhenSCACIsNot4CharLongForOtherTransportModes_HasErrorMessage()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			consignment.ULB_ULH = clearance.PK;

			Factory.Save();

			var messageError = "The Standard Carrier Alpha Code (SCAC) is not valid for the selected mode of transport. SCAC should be 2 characters in length for Air and 4 characters in length for other modes of transport.";

			consignment.Shipment.ULH_TransportMode = "SEA";
			consignment.ULB_HouseBillIssuerSCAC = "AA1";
			AssertHasMessageError(consignment.ULB_HouseBillIssuerSCACInfo, messageError);

			consignment.ULB_HouseBillIssuerSCAC = "1";
			AssertHasMessageError(consignment.ULB_HouseBillIssuerSCACInfo, messageError);

			consignment.Shipment.ULH_TransportMode = "RAI";
			consignment.ULB_HouseBillIssuerSCAC = "AA1";
			AssertHasMessageError(consignment.ULB_HouseBillIssuerSCACInfo, messageError);

			consignment.ULB_HouseBillIssuerSCAC = "1";
			AssertHasMessageError(consignment.ULB_HouseBillIssuerSCACInfo, messageError);

			consignment.Shipment.ULH_TransportMode = "TRK";
			consignment.ULB_HouseBillIssuerSCAC = "AA1";
			AssertHasMessageError(consignment.ULB_HouseBillIssuerSCACInfo, messageError);

			consignment.ULB_HouseBillIssuerSCAC = "1";
			AssertHasMessageError(consignment.ULB_HouseBillIssuerSCACInfo, messageError);
		}

		public void TestHouseBillIssuerSCAC_WhenSCACIsUNKN_HasErrorMessage()
		{
			var consignment = Factory.New<CusUSLVConsignment>();
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			consignment.ULB_ULH = clearance.PK;

			Factory.Save();

			consignment.ULB_HouseBillIssuerSCAC = "UNKN";
			AssertHasMessageError(consignment.ULB_HouseBillIssuerSCACInfo, "SCAC Code \"UNKN\" cannot be used for ACE Cargo Release. Use a valid carrier or \"ZZZZ\" if unknown.");
		}

		public void TestEquipmentNumber_WhenContainerModeIsCNT_IsMandatory()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_ContainerMode = "CNT";
			clearance.ULH_TransportMode = TransportModes.Sea;
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_EquipmentNumber = "";
			AssertHasMessageErrorContaining(consignment.ULB_EquipmentNumberInfo, "The container mode indicates this shipment is containerized, as yet, no containers have been entered.");

			consignment.ULB_EquipmentNumber = "XX";
			AssertNoMessageErrorContaining(consignment.ULB_EquipmentNumberInfo, "The container mode indicates this shipment is containerized, as yet, no containers have been entered.");
		}

		public void TestEquipmentNumber_WhenHasInvalidChar_HasMessageError()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			clearance.ULH_ContainerMode = "CNT";
			clearance.ULH_TransportMode = TransportModes.Sea;
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_EquipmentNumber = "%%%";
			AssertHasMessageErrorContaining(consignment.ULB_EquipmentNumberInfo, "Invalid Characters In Container Number - Container number must only contain alphanumeric characters.");

			consignment.ULB_EquipmentNumber = "123";
			AssertNoMessageErrorContaining(consignment.ULB_EquipmentNumberInfo, "Invalid Characters In Container Number - Container number must only contain alphanumeric characters.");
		}

		public void TestEquipmentNumber_WhenHasInvalidFormat_HasWarning()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = TransportModes.Sea;
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_EquipmentNumber = "123";
			AssertHasWarningContaining(consignment.ULB_EquipmentNumberInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");

			consignment.ULB_EquipmentNumber = "AAAA1234568";
			AssertNoWarningContaining(consignment.ULB_EquipmentNumberInfo, "Container number does not conform to ISO standard of 4 letters followed by 6 digits and a check digit.");
		}

		public void TestEquipmentNumber_WhenHasInvalidCheckDigit_HasWarning()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_TransportMode = TransportModes.Sea;
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_EquipmentNumber = "AAAA1234567";
			AssertHasWarningContaining(consignment.ULB_EquipmentNumberInfo, "Container number does not have a valid check (last) digit. The check digit should be 6.");

			consignment.ULB_EquipmentNumber = "AAAA1234566";
			AssertNoWarningContaining(consignment.ULB_EquipmentNumberInfo, "Container number does not have a valid check (last) digit. The check digit should be 6.");
		}

		public void TestCheckULBHouseBill_WhenHasSpecialCharacters_ShowsWarning()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ULB_HouseBill = "A1/-!@#$%^&*()_+=";

			var billInvalidCharactersWarningMessage = "House Bill" + US.Business.BillValidator.Constants.BillInvalidCharacters;

			AssertHasWarningContaining(consignment.ULB_HouseBillInfo, billInvalidCharactersWarningMessage);
			consignment.ULB_HouseBill = "ABC123";
			AssertNoWarnings(consignment.ULB_HouseBillInfo);
		}

		public void TestCheckITNumber()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.ITNumber = "123";
			AssertHasMessageError(consignment.ITNumberInfo, ITNumberValidator.Constants.ITNumber.Invalid);
			consignment.ITNumber = DeclarationTestHelper.ValidITNumber1ForTesting;
			AssertNoMessageError(consignment.ITNumberInfo, ITNumberValidator.Constants.ITNumber.Invalid);
			clearance.ULH_TransportMode = TransportModes.Air;
			consignment.ITNumber = "Vblah-blah";
			AssertHasMessageError(consignment.ITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			consignment.ITNumber = "12365";
			AssertNoMessageError(consignment.ITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			consignment.ITNumber = "vblah-blah";
			AssertHasMessageError(consignment.ITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			clearance.ULH_TransportMode = TransportModes.Rail;
			consignment.ITNumber = "V236542J144";
			AssertHasMessageError(consignment.ITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			consignment.ITNumber = "V2365425144";
			AssertNoMessageError(consignment.ITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			consignment.ITNumber = "v2365425144";
			AssertNoMessageError(consignment.ITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			consignment.ITNumber = "VHH65425144";
			AssertNoMessageError(consignment.ITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			consignment.ITNumber = "vHH65425144";
			AssertNoMessageError(consignment.ITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			consignment.ITNumber = "VH765425144";
			AssertNoMessageError(consignment.ITNumberInfo, ValidationConstants.AllocateInBondNumber.InvalidPaperless);
			consignment.ITNumber = "V7603245271";
			AssertHasMessageErrorContaining(consignment.ITNumberInfo, "Invalid check digit. The last digit should be");
			consignment.ITNumber = "V7603245275";
			AssertNoMessageErrorContaining(consignment.ITNumberInfo, "Invalid check digit. The last digit should be");
			consignment.ITNumber = "v7603245275";
			AssertNoMessageErrorContaining(consignment.ITNumberInfo, "Invalid check digit. The last digit should be");
			consignment.ITNumber = "123456789";
			AssertHasWarningContaining(consignment.ITNumberInfo, "Invalid check digit. The last digit should be");
			//if Air then AWB number format is also correct (11n)
			clearance.ULH_TransportMode = TransportModes.Air;
			consignment.ITNumber = "12345678";
			AssertHasMessageError(consignment.ITNumberInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);
			consignment.ITNumber = "1111G111111";
			AssertHasMessageError(consignment.ITNumberInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);
			consignment.ITNumber = "11111111111";
			AssertNoMessageError(consignment.ITNumberInfo, ITNumberValidator.Constants.ITNumber.InvalidForAir);
			clearance.ULH_TransportMode = TransportModes.Rail;
			consignment.ITNumber = "11111111111";
			AssertHasMessageError(consignment.ITNumberInfo, ITNumberValidator.Constants.ITNumber.Invalid);
		}
	}
}
