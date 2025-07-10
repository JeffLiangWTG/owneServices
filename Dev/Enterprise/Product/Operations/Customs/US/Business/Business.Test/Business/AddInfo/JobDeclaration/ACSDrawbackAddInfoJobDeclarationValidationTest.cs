using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACSDrawbackAddInfoJobDeclarationValidationTest : CommonDrawbackAddInfoJobDeclarationValidationTest
	{
		public void TestCheckUS_EntryType()
		{
			var message = "Please enter a valid Entry Type Code";
			declaration.US_EntryType = "";
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			CodeDescriptionPairList list = Enterprise.Customs.US.Business.EntryTypeList.GetDrawbackSummaryEntryTypeList();
			foreach (CodeDescriptionPair pair in list)
			{
				declaration.US_EntryType = pair.Code;
				AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			}

			declaration.US_EntryType = "11";
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			declaration.US_EntryType = EntryTypeList.Codes.DirectIdentificationManufacturingDrawback;
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, ACSDrawbackAddInfoJobDeclarationValidation.ContractNumberRequiredMessage);
			declaration.US_EntryType = EntryTypeList.Codes.DirectIdentificationUnusedMerchandiseDrawback;
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, ACSDrawbackAddInfoJobDeclarationValidation.ContractNumberRequiredMessage);
			declaration.US_EntryType = EntryTypeList.Codes.SubstitutionManufacturerDrawback;
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, ACSDrawbackAddInfoJobDeclarationValidation.ContractNumberRequiredMessage);
			ContractNumber contractNumber = declaration.ContractNumbers.AddNew();
			contractNumber.CY_Data = "12-12345-123";
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, ACSDrawbackAddInfoJobDeclarationValidation.ContractNumberRequiredMessage);
			contractNumber.CY_Data = "";
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, ACSDrawbackAddInfoJobDeclarationValidation.ContractNumberRequiredMessage);
		}

		[TestDate(2008, 4, 1)]
		public new void TestCheckUS_EstimatedEntryDate()
		{
			base.TestCheckUS_EstimatedEntryDate();
			declaration.US_EstimatedEntryDate = ZDateTime.Empty;
			declaration.AddInfoValidation.ValidateUS_EstimatedEntryDate();
			AssertHasMessageErrorContaining(declaration.US_EstimatedEntryDateInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_EstimatedEntryDate = ZDateTime.Now;
			AssertNoMessageErrorContaining(declaration.US_EstimatedEntryDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_NAFTADrawbackCountry()
		{
			declaration.US_NAFTAClaimInd = true;
			declaration.US_NAFTADrawbackCountry = "";
			declaration.AddInfoValidation.ValidateUS_NAFTADrawbackCountry();
			AssertHasMessageErrorContaining(declaration.US_NAFTADrawbackCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_NAFTADrawbackCountryInfo, ListValidation.InvalidCodeMessageError);
			CodeDescriptionPairList list = declaration.AddInfoLookups.US_NAFTACountryCodeList;
			foreach (CodeDescriptionPair pair in list)
			{
				declaration.US_NAFTADrawbackCountry = pair.Code;
				AssertNoMessageErrorContaining(declaration.US_NAFTADrawbackCountryInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.US_NAFTADrawbackCountryInfo, ListValidation.InvalidCodeMessageError);
			}

			declaration.US_NAFTADrawbackCountry = "AU";
			AssertNoMessageErrorContaining(declaration.US_NAFTADrawbackCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_NAFTADrawbackCountryInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_NAFTAClaimInd = false;
			AssertHasMessageErrorContaining(declaration.US_NAFTADrawbackCountryInfo, MandatoryValidation.DoNotEntered);
			declaration.US_NAFTADrawbackCountry = "";
			AssertNoMessageErrorContaining(declaration.US_NAFTADrawbackCountryInfo, MandatoryValidation.DoNotEntered);
		}

		public void TestCheckUS_PreparerDistrictPort()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2407", "Test Name", startDate, endDate);
			newFactory.Save();

			declaration.US_PreparerDistrictPort = "";
			declaration.AddInfoValidation.ValidateUS_PreparerDistrictPort();
			AssertHasMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_PreparerDistrictPort = "2407";
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_PreparerDistrictPort = "XXXX";
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DRWTotalPRDC()
		{
			declaration.US_DRWTotalPRDC = 1m;
			AssertNoMessageErrors(declaration.US_DRWTotalPRDCInfo);
			declaration.US_DRWTotalPRDC = 0m;
			AssertNoMessageErrors(declaration.US_DRWTotalPRDCInfo);
			declaration.US_DRWTotalPRDC = -11m;
			AssertHasMessageErrorContaining(declaration.US_DRWTotalPRDCInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestCheckUS_DRWFilingMethod()
		{
			declaration.US_DRWFilingMethod = "A";
			AssertNoMessageErrors(declaration.US_DRWFilingMethodInfo);
			declaration.US_DRWFilingMethod = "";
			AssertHasMessageErrorContaining(declaration.US_DRWFilingMethodInfo, "You have not entered a value");
			declaration.US_DRWFilingMethod = "M";
			AssertNoMessageErrors(declaration.US_DRWFilingMethodInfo);
			declaration.US_DRWFilingMethod = "X";
			AssertHasMessageErrorContaining(declaration.US_DRWFilingMethodInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_MessageStatus = DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryOriginal;
			declaration.US_DRWFilingMethod = DrawbackMethodOfFilingList.Codes.Manual;
			AssertHasError(declaration.US_DRWFilingMethodInfo, ACSDrawbackAddInfoJobDeclarationValidation.CannotSetMethodOfFilingAsManual);
			declaration.JE_MessageStatus = DrawbackSummaryStatusList.Codes.ClearDrawbackSummaryDelete;
			declaration.US_DRWFilingMethod = DrawbackMethodOfFilingList.Codes.ABI;
			declaration.US_DRWFilingMethod = DrawbackMethodOfFilingList.Codes.Manual;
			AssertNoError(declaration.US_DRWFilingMethodInfo, ACSDrawbackAddInfoJobDeclarationValidation.CannotSetMethodOfFilingAsManual);
		}

		public void TestUS_DRWPurpose()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWPurpose = ZString.Empty;
			declaration.AddInfoValidation.ValidateUS_DRWPurpose();
			AssertHasErrorContaining(declaration.US_DRWPurposeInfo, MandatoryValidation.MustBeEntered);
			declaration.US_DRWPurpose = "~~";
			AssertHasErrorContaining(declaration.US_DRWPurposeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckUS_DRWTransferee()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			declaration.US_DRWTransferee = ZGuid.Empty;
			AssertNoMessageErrorContaining(declaration.US_DRWTransfereeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			declaration.US_DRWTransferee = ZGuid.Empty;
			AssertHasMessageErrorContaining(declaration.US_DRWTransfereeInfo, MandatoryValidation.YouHaveNotEntered);
			var tranferee = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>();
			declaration.US_DRWTransferee = tranferee.PK;
			AssertNoMessageErrorContaining(declaration.US_DRWTransfereeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWCertOfManufacture()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			declaration.US_DRWCertOfManufacture = "$#@$#@$";
			AssertNoMessageError(declaration.US_DRWCertOfManufactureInfo, ACSDrawbackAddInfoJobDeclarationValidation.CertOfManufactureFormatIsInvalid);
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CM;
			declaration.US_DRWCertOfManufacture = "$#@$#@$";
			AssertHasMessageError(declaration.US_DRWCertOfManufactureInfo, ACSDrawbackAddInfoJobDeclarationValidation.CertOfManufactureFormatIsInvalid);
			declaration.US_DRWCertOfManufacture = "123456";
			AssertNoMessageError(declaration.US_DRWCertOfManufactureInfo, ACSDrawbackAddInfoJobDeclarationValidation.CertOfManufactureFormatIsInvalid);
			declaration.US_DRWCertOfManufacture = ZString.Empty;
			AssertNoMessageError(declaration.US_DRWCertOfManufactureInfo, ACSDrawbackAddInfoJobDeclarationValidation.CertOfManufactureFormatIsInvalid);
		}

		public void TestCheckUS_DRWRulingNo()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.DRW;
			declaration.US_DRWRulingNo = "$#@$#@$";
			AssertNoMessageError(declaration.US_DRWRulingNoInfo, ACSDrawbackAddInfoJobDeclarationValidation.RulingNoFormatIsInvalid);
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			declaration.US_DRWRulingNo = "$#@$#@$";
			AssertHasMessageError(declaration.US_DRWRulingNoInfo, ACSDrawbackAddInfoJobDeclarationValidation.RulingNoFormatIsInvalid);
			declaration.US_DRWRulingNo = "11-11111-111";
			AssertNoMessageError(declaration.US_DRWRulingNoInfo, ACSDrawbackAddInfoJobDeclarationValidation.RulingNoFormatIsInvalid);
			declaration.US_DRWRulingNo = ZString.Empty;
			AssertNoMessageError(declaration.US_DRWRulingNoInfo, ACSDrawbackAddInfoJobDeclarationValidation.RulingNoFormatIsInvalid);
		}
	}
}
