using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AddInfoCusClassPartPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_ProductExclusion()
		{
			var addInfo = GetNewAddInfo();
			var pivot = addInfo.Parent;
			addInfo.US_ProductExclusion = "~";
			AssertHasMessageErrorContaining(addInfo.US_ProductExclusionInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertNoMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);
			addInfo.US_ProductExclusion = "02";
			AssertNoMessageErrorContaining(addInfo.US_ProductExclusionInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertNoMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);
			pivot.CI_TariffNum = "7201101001";
			addInfo.Validation.ValidateUS_ProductExclusion();
			AssertNoMessageErrorContaining(addInfo.US_ProductExclusionInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertNoMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);
			pivot.CI_TariffNum = "7301101001";
			addInfo.Validation.ValidateUS_ProductExclusion();
			AssertNoMessageErrorContaining(addInfo.US_ProductExclusionInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertNoMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);
			addInfo.US_ProductExclusion = "03";
			AssertNoMessageErrorContaining(addInfo.US_ProductExclusionInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertNoMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);
			pivot.CI_TariffNum = "7601101001";
			addInfo.Validation.ValidateUS_ProductExclusion();
			AssertNoMessageErrorContaining(addInfo.US_ProductExclusionInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertNoMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);
			addInfo.US_ProductExclusion = "02";
			AssertNoMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.SteelProductExclusionNotValid);
			AssertHasMessageError(addInfo.US_ProductExclusionInfo, ACEImportAddInfoJobComInvoiceLineValidation.AluminumProductExclusionNotValid);
		}

		public void TestCheckUS_ExclusionNumber()
		{
			#region Create Reference Data
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USProductExclusionTypes, "Product Exclusion Types", dataGrouping.ZZZ_DataGrouping);
			var attributeEXCMSK = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USProductExclusionCodeMask, "Exclusion Code Mask", codeType.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeEXCERT = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USProductExclusionCodeErrorText, "Exclusion Code Error Text", codeType.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var codeList02 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, AdditionalDeclarationTypeCodeList.Codes._02, AdditionalDeclarationTypeCodeList.Descriptions._02, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList02.PK, attributeEXCMSK.ZXE_Name, @"((STL|SPR|STX)\d{6}|INJS\d{5})");
			var errorMessage02 = @"Format should be STLNNNNNN or SPRNNNNNN or STXNNNNNN, where NNNNNN represent 6 digits, or format should be INJSNNNNN, where NNNNN represent 5 digits.";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList02.PK, attributeEXCERT.ZXE_Name, errorMessage02);
			var codeList03 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, AdditionalDeclarationTypeCodeList.Codes._03, AdditionalDeclarationTypeCodeList.Descriptions._03, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList03.PK, attributeEXCMSK.ZXE_Name, @"((ALU|APR)\d{6}|INJA\d{5})");
			var errorMessage03 = @"Format should be ALUNNNNNN or APRNNNNNN, where NNNNNN represent 6 digits, or format should be INJANNNNN, where NNNNN represent 5 digits.";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeList03.PK, attributeEXCERT.ZXE_Name, errorMessage03);
			Factory.Save();
			#endregion
			var addInfo = GetNewAddInfo();
			addInfo.US_ExclusionNumber = "~";
			AssertHasMessageErrorContaining(addInfo.US_ExclusionNumberInfo, ACEImportAddInfoJobComInvoiceLineValidation.ProductExclustionNumberNotAllowed);
			addInfo.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			addInfo.Validation.ValidateUS_ExclusionNumber();
			var fullErrorMessage02 = ACEImportAddInfoJobComInvoiceLineValidation.InvalidProductExclusionNumberMessagePrefix + errorMessage02;
			var fullErrorMessage03 = ACEImportAddInfoJobComInvoiceLineValidation.InvalidProductExclusionNumberMessagePrefix + errorMessage03;
			AssertNoMessageErrorContaining(addInfo.US_ExclusionNumberInfo, ACEImportAddInfoJobComInvoiceLineValidation.ProductExclustionNumberNotAllowed);
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			addInfo.US_ExclusionNumber = "STL00000 ";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "STL00000A";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "STL000001";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "ALU000001";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "SPR00000 ";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "SPR00000A";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "SPR000001";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "STX00000 ";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "STX00000A";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "STX000001";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "INJS00001";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "APR000001";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._03;
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "ALU00000 ";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "ALU00000A";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "ALU100000";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "APR00000 ";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "APR00000A";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertHasMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "APR100000";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = "INJA00001";
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage02);
			AssertNoMessageError(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
			addInfo.US_ExclusionNumber = ZString.Empty;
			addInfo.Validation.ValidateUS_ExclusionNumber();
			AssertHasMessageErrorContaining(addInfo.US_ExclusionNumberInfo, fullErrorMessage03);
		}

		AddInfoCusClassPartPivot GetNewAddInfo()
		{
			return new AddInfoCusClassPartPivot(Factory.New<CusClassPartPivot>().CI_AddInfoInfo);
		}
	}
}
