using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Manifest.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using UniversalConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class AsycudaBillForRegularBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_UCRNumber()
		{
			CombineAssertions(() =>
			{
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, ZaManifestTypes.Codes.ALM, ApplicationCodeTypeList.Codes.ShippingLine);
				header.FillWithValidTestData();
				var bill = header.Bills.AddNew();
				foreach (var shipmentType in new ShipmentTypeList().GetAllCodes().Concat(new[] { string.Empty }))
				{
					header.AMA_Nature = shipmentType;
					foreach (var manifestDocumentType in Enum.GetValues(typeof(ManifestDocumentType)).Cast<ManifestDocumentType>().Select(x => x.ToString()).Concat(new[] { string.Empty }))
					{
						header.AMA_ManifestType = manifestDocumentType;
						if (shipmentType == ShipmentTypeList.Codes.Export22 && (manifestDocumentType == nameof(ManifestDocumentType.COM) || manifestDocumentType == nameof(ManifestDocumentType.COH) || manifestDocumentType == nameof(ManifestDocumentType.BBB) || manifestDocumentType == nameof(ManifestDocumentType.FWB) || manifestDocumentType == nameof(ManifestDocumentType.HAB) || manifestDocumentType == nameof(ManifestDocumentType.RMA) || manifestDocumentType == nameof(ManifestDocumentType.RFM) || manifestDocumentType == nameof(ManifestDocumentType.ALM) || manifestDocumentType == nameof(ManifestDocumentType.ALH)))
						{
							bill.ABL_UCRNumber = ZString.Empty;
							AssertHasMessageErrorContaining($"Mandatory UCR is Missing : '{shipmentType}' '{manifestDocumentType}'", bill.ABL_UCRNumberInfo, MandatoryValidation.YouHaveNotEntered);
							bill.ABL_UCRNumber = "UCR";
							AssertNoNotifications($"Mandatory UCR is Entered: '{shipmentType}' '{manifestDocumentType}'", bill.ABL_UCRNumberInfo);
						}
						else
						{
							bill.ABL_UCRNumber = ZString.Empty;
							AssertNoNotifications($"Not Mandatory UCR is Missing: '{shipmentType}' '{manifestDocumentType}'", bill.ABL_UCRNumberInfo);
							bill.ABL_UCRNumber = "UCR";
							AssertNoNotifications($"Not Mandatory UCR is Entered: '{shipmentType}' '{manifestDocumentType}'", bill.ABL_UCRNumberInfo);
						}
					}
				}
			});
		}

		public void TestCheckABL_BillNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_MasterBill = "ALMMASTER1";
			header.AMA_ManifestType = nameof(ManifestDocumentType.ALM);
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill1.ABL_BillNumber = "123";
			AssertHasMessageError(bill1.ABL_BillNumberInfo, ValidationConstants.BillNumberRequiresTheSame);
			header.Bills.RemoveAll();
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
			bill2.ABL_BillNumber = "123";
			AssertHasMessageError(bill2.ABL_BillNumberInfo, ValidationConstants.BillNumberRequiresTheSame);
			header.Bills.RemoveAll();
			var bill5 = header.Bills.AddNew();
			bill5.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
			bill5.ABL_BillNumber = "ALMMASTER1";
			AssertNoMessageError(bill5.ABL_BillNumberInfo, ValidationConstants.BillNumberRequiresTheSame);
		}

		public void TestCheckCustomsEntryNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var bill = header.Bills.AddNew();

			bill.Validation.ValidateCustomsEntryNumber();
			const string errorString = "At least one LRN Number must be captured per bill";
			AssertHasMessageErrorContaining(bill.CustomsEntryNumberInfo, errorString);

			var entryNum1 = bill.CustomsEntryNumbers.AddNew();
			entryNum1.CE_EntryType = "CCC";
			bill.Validation.ValidateCustomsEntryNumber();
			AssertHasMessageErrorContaining(bill.CustomsEntryNumberInfo, errorString);

			entryNum1.CE_EntryNum = "EN123";
			bill.Validation.ValidateCustomsEntryNumber();
			AssertNoMessageErrorContaining(bill.CustomsEntryNumberInfo, errorString);

			entryNum1.CE_EntryType = ZaLRNTypes.Codes.ABT;
			bill.Validation.ValidateCustomsEntryNumber();
			AssertNoMessageErrorContaining(bill.CustomsEntryNumberInfo, errorString);

			entryNum1.CE_EntryType = ZaLRNTypes.Codes.AFM;
			bill.Validation.ValidateCustomsEntryNumber();
			AssertNoMessageErrorContaining(bill.CustomsEntryNumberInfo, errorString);

			entryNum1.CE_EntryNum = ZString.Empty;
			bill.Validation.ValidateCustomsEntryNumber();
			AssertHasMessageErrorContaining(bill.CustomsEntryNumberInfo, errorString);
		}

		public void TestCheckABL_OA_Shipper()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var orgHeader = Factory.New<OrgHeader>();
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "Test";
			bill.ABL_BolType = "STD";
			bill.Validation.ValidateABL_OA_Shipper();
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, "enter");
			bill.ABL_OA_Shipper = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, "enter");
			bill.ABL_BolType = "CLD";
			bill.ABL_OA_Shipper = ZGuid.Empty;
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, "enter");
			bill.ABL_OA_Shipper = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, "enter");
		}

		public void TestValicateConsignee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(UniversalConstants.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, UniversalConstants.RefCusCodeListTypes.Codes.ManifestValidationRule, UniversalConstants.RefCusCodeList.ManifestValidationRuleCodes.Consignee, "Consignee", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(UniversalConstants.RefCusCodeList.ManifestValidationRuleCodes.Mandatory, "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.SouthAfrica, UniversalConstants.RefCusCodeListTypes.Codes.ManifestValidationRule);
			validationRule.Attributes.AddNew(UniversalConstants.RefCusCodeList.ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			Factory.Save();
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "Test";
			bill.ABL_BolType = "STD";
			bill.Validation.ValidateABL_OA_Consignee();
			AssertHasMessageErrors(bill.ABL_OA_ConsigneeInfo);
			bill.Validation.ValidateABL_ConsigneeCity();
			bill.Validation.ValidateABL_RN_NKConsigneeCountry();
			bill.Validation.ValidateABL_ConsigneePostcode();
			AssertHasMessageErrors(bill.ABL_ConsigneeCityInfo);
			AssertHasMessageErrors(bill.ABL_RN_NKConsigneeCountryInfo);
			AssertHasMessageErrors(bill.ABL_ConsigneePostcodeInfo);
			bill.ABL_ConsigneeName = "TO ORDER";
			AssertEquals("Required value should default to Street Address field 1", "NO ADDRESS SUPPLIED", bill.ABL_ConsigneeStreet1);
			bill.Validation.ValidateABL_OA_Consignee();
			AssertNoMessageErrors(bill.ABL_OA_ConsigneeInfo);
			bill.Validation.ValidateABL_ConsigneeCity();
			bill.Validation.ValidateABL_RN_NKConsigneeCountry();
			bill.Validation.ValidateABL_ConsigneePostcode();
			AssertNoMessageErrors(bill.ABL_ConsigneeCityInfo);
			AssertNoMessageErrors(bill.ABL_RN_NKConsigneeCountryInfo);
			AssertNoMessageErrors(bill.ABL_ConsigneePostcodeInfo);
		}

		public void TestCheckCustomsEntryNumberType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumber = "01234567890";
			bill.CustomsEntryNumberType = "ABT";
			AssertNoMessageErrors(bill.CustomsEntryNumberTypeInfo);
			bill.CustomsEntryNumber = "01234567890";
			bill.CustomsEntryNumberType = "";
			AssertHasMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.CustomsEntryNumber = "01234567890";
			bill.CustomsEntryNumberType = "ABC";
			AssertHasMessageErrorContaining(bill.CustomsEntryNumberTypeInfo, ListValidation.InvalidCodeMessageError);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			bill.CustomsEntryNumber = "01234567890";
			bill.CustomsEntryNumberType = "";
			AssertNoMessageErrors(bill.CustomsEntryNumberTypeInfo);
		}
	}
}
