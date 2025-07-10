using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEDrawbackAddInfoJobDeclarationValidationTest : CommonDrawbackAddInfoJobDeclarationValidationTest
	{
		public void TestCheckUS_EntryType()
		{
			var message = "Please enter a valid Entry Type Code";
			declaration.US_EntryType = "";
			declaration.AddInfoValidation.ValidateUS_EntryType();
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			declaration.US_EntryType = "~";
			AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			foreach (CodeDescriptionPair pair in ACEDrawbackProvisionsList.GetDrawbackProvisionList(Factory))
			{
				declaration.US_EntryType = pair.Code;
				AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(declaration.US_EntryTypeInfo, message);
			}
		}

		public void TestCheckUS_DRWOneTimeWaiverInd()
		{
			declaration.US_DRWOneTimeWaiverInd = true;
			AssertHasWarning(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.BeSureProvideOTWDataToCBP);
			declaration.US_DRWOneTimeWaiverInd = false;
			AssertNoWarning(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.BeSureProvideOTWDataToCBP);
			declaration.US_DRWExamWitness = false;
			declaration.US_WaiverNoticeInd = false;
			declaration.US_DRWOneTimeWaiverInd = false;
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._07;
			AssertEquals(false, ACEDrawbackProvisionsList.IsApplicableProvisionsForOneTimeWaiverInd(declaration.US_EntryType));
			declaration.AddInfoValidation.ValidateUS_DRWOneTimeWaiverInd();
			AssertNoMessageErrorContaining(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.WaiverIndMessageError);

			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._05;
			AssertEquals(true, ACEDrawbackProvisionsList.IsApplicableProvisionsForOneTimeWaiverInd(declaration.US_EntryType));
			declaration.AddInfoValidation.ValidateUS_DRWOneTimeWaiverInd();
			AssertHasMessageErrorContaining(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.WaiverIndMessageError);

			declaration.US_DRWExamWitness = true;
			AssertNoMessageErrorContaining(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.WaiverIndMessageError);
			declaration.US_DRWExamWitness = false;
			AssertHasMessageErrorContaining(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.WaiverIndMessageError);

			declaration.US_WaiverNoticeInd = true;
			AssertNoMessageErrorContaining(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.WaiverIndMessageError);

			declaration.US_DRWOneTimeWaiverInd = false;
			declaration.US_WaiverNoticeInd = true;
			declaration.AddInfoValidation.ValidateUS_DRWOneTimeWaiverInd();
			AssertNoMessageErrorContaining(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.WaiverIndMessageError);
			declaration.US_DRWOneTimeWaiverInd = true;
			declaration.US_WaiverNoticeInd = false;
			declaration.AddInfoValidation.ValidateUS_DRWOneTimeWaiverInd();
			AssertNoMessageErrorContaining(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.WaiverIndMessageError);
			declaration.US_DRWOneTimeWaiverInd = true;
			declaration.US_WaiverNoticeInd = true;
			declaration.AddInfoValidation.ValidateUS_DRWOneTimeWaiverInd();
			AssertNoMessageErrorContaining(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.WaiverIndMessageError);
			declaration.US_DRWExamWitness = true;
			declaration.US_WaiverNoticeInd = false;
			declaration.US_DRWOneTimeWaiverInd = false;
			declaration.AddInfoValidation.ValidateUS_DRWOneTimeWaiverInd();
			AssertNoMessageErrorContaining(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.WaiverIndMessageError);
			declaration.US_DRWOneTimeWaiverInd = false;
			declaration.US_WaiverNoticeInd = true;
			declaration.AddInfoValidation.ValidateUS_DRWOneTimeWaiverInd();
			AssertNoMessageErrorContaining(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.WaiverIndMessageError);
			declaration.US_DRWOneTimeWaiverInd = true;
			declaration.US_WaiverNoticeInd = false;
			declaration.AddInfoValidation.ValidateUS_DRWOneTimeWaiverInd();
			AssertNoMessageErrorContaining(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.WaiverIndMessageError);
			declaration.US_DRWOneTimeWaiverInd = true;
			declaration.US_WaiverNoticeInd = true;
			declaration.AddInfoValidation.ValidateUS_DRWOneTimeWaiverInd();
			AssertNoMessageErrorContaining(declaration.US_DRWOneTimeWaiverIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.WaiverIndMessageError);
		}

		public override void TestCheckUS_BondType()
		{
			declaration.US_AcceleratedClaimInd = false;
			declaration.US_BondType = "";
			declaration.AddInfoValidation.ValidateUS_BondType();
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondType = "X";
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_AcceleratedClaimInd = true;
			declaration.US_BondType = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.US_AcceleratedClaimInd = true;
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, ACEDrawbackAddInfoJobDeclarationValidation.ACEBondTypeRequiredMessage);
			declaration.US_AcceleratedClaimInd = false;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ACEDrawbackAddInfoJobDeclarationValidation.ACEBondTypeRequiredMessage);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.US_ExporterSummaryInd = true;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ACEDrawbackAddInfoJobDeclarationValidation.ACEBondTypeRequiredMessage);
			declaration.US_ExporterSummaryInd = false;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ACEDrawbackAddInfoJobDeclarationValidation.ACEBondTypeRequiredMessage);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.US_AcceleratedClaimInd = true;
			AssertHasMessageErrorContaining(declaration.US_BondTypeInfo, ACEDrawbackAddInfoJobDeclarationValidation.ACEBondTypeRequiredMessage);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ACEDrawbackAddInfoJobDeclarationValidation.ACEBondTypeRequiredMessage);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertNoMessageErrorContaining(declaration.US_BondTypeInfo, ACEDrawbackAddInfoJobDeclarationValidation.ACEBondTypeRequiredMessage);
		}

		public void TestCheckUS_BondDesignationCode()
		{
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.AddInfoValidation.ValidateUS_BondDesignationCode();
			AssertNoMessageErrorContaining(declaration.US_BondDesignationCodeInfo, ACEDrawbackAddInfoJobDeclarationValidation.DesignationCodeRequired);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_BondDesignationCodeInfo, ACEDrawbackAddInfoJobDeclarationValidation.DesignationCodeRequired);
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.AddInfoValidation.ValidateUS_BondDesignationCode();
			AssertNoMessageErrorContaining(declaration.US_BondDesignationCodeInfo, ACEDrawbackAddInfoJobDeclarationValidation.DesignationCodeRequired);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondDesignationCode = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_BondDesignationCodeInfo, ACEDrawbackAddInfoJobDeclarationValidation.DesignationCodeRequired);
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.SubstitutionBond;
			AssertNoMessageErrorContaining(declaration.US_BondDesignationCodeInfo, ACEDrawbackAddInfoJobDeclarationValidation.DesignationCodeRequired);
			AssertHasMessageErrorContaining(declaration.US_BondDesignationCodeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.TerminateContinuousBond;
			AssertHasMessageErrorContaining(declaration.US_BondDesignationCodeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.BasicBond;
			AssertNoMessageErrorContaining(declaration.US_BondDesignationCodeInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_BondDesignationCode = BondDesignationCodeList.Codes.SupersedingBond;
			AssertHasMessageErrorContaining(declaration.US_BondDesignationCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_BondWaiverCode()
		{
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_BondWaiverCode = BondWaiverReasonCodeList.Codes._995;
			AssertHasMessageError(declaration.US_BondWaiverCodeInfo, ACEImportAddInfoJobDeclarationValidation.BondWaiverCodeNotForNoBondRequired);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			AssertNoMessageError(declaration.US_BondWaiverCodeInfo, ACEImportAddInfoJobDeclarationValidation.BondWaiverCodeNotForNoBondRequired);
		}

		public new void TestCheckUS_SuretyCode()
		{
			declaration.US_BondType = ZString.Empty;
			declaration.US_SuretyCode = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.US_SuretyCode = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_SuretyCode = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_SuretyCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_SuretyCode = "~";
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
			declaration.US_SuretyCode = "123";
			AssertNoMessageErrorContaining(declaration.US_SuretyCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(declaration.US_SuretyCodeInfo, SuretyCodeValidator.SuretyCodeRightFormat);
		}

		public void TestCheckUS_BondAmount()
		{
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_BondAmount = 10m;
			AssertHasMessageError(declaration.US_BondAmountInfo, ACEImportAddInfoJobDeclarationValidation.NotRelevantForContinuousBond);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondAmount = 0m;
			AssertNoMessageError(declaration.US_BondAmountInfo, ACEImportAddInfoJobDeclarationValidation.NotRelevantForContinuousBond);
		}

		public void TestValidateProcessorIfRequired()
		{
			declaration.US_DRWIntendedPortOfExport = "1001";
			AssertHasMessageErrorContaining(declaration.US_DRWProcNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_DRWProcBadgeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_DRWProcPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_DRWProcDateInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWExamWitness = true;
			AssertHasMessageErrorContaining(declaration.US_DRWProcNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_DRWProcBadgeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_DRWProcPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_DRWProcDateInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWLocatOfDest = "ABCDE";
			AssertHasMessageErrorContaining(declaration.US_DRWProcNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_DRWProcBadgeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_DRWProcPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_DRWProcDateInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWProcName = "TESTNAME";
			AssertNoMessageErrorContaining(declaration.US_DRWProcNameInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWProcBadge = "1123333333";
			AssertNoMessageErrorContaining(declaration.US_DRWProcBadgeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWProcPhone = "1111122222";
			AssertNoMessageErrorContaining(declaration.US_DRWProcPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWProcDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(declaration.US_DRWProcDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidateExaminerIfRequired()
		{
			declaration.US_DRWExamWitness = true;
			AssertHasMessageErrorContaining(declaration.US_DRWExamNameInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_DRWExamBadgeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_DRWExamPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_DRWExamDateInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWExamName = "TESTNAME";
			AssertNoMessageErrorContaining(declaration.US_DRWExamNameInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWExamBadge = "1123333333";
			AssertNoMessageErrorContaining(declaration.US_DRWExamBadgeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWExamPhone = "1111122222";
			AssertNoMessageErrorContaining(declaration.US_DRWExamPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWExamDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(declaration.US_DRWExamDateInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWDestructionResult = DrawbackDestructionResultCodes.Codes.Waived;
			declaration.AddInfoValidation.ValidateUS_DRWExamName();
			AssertHasMessageError(declaration.US_DRWExamNameInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationInformationShouldNotBeEntered);
			AssertHasMessageError(declaration.US_DRWExamBadgeInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationInformationShouldNotBeEntered);
			AssertHasMessageError(declaration.US_DRWExamPhoneInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationInformationShouldNotBeEntered);
			AssertHasMessageError(declaration.US_DRWExamDateInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationInformationShouldNotBeEntered);
			declaration.US_DRWExamName = "";
			AssertNoMessageError(declaration.US_DRWExamNameInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationInformationShouldNotBeEntered);
			AssertNoMessageErrorContaining(declaration.US_DRWExamNameInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWExamBadge = "";
			AssertNoMessageError(declaration.US_DRWExamBadgeInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationInformationShouldNotBeEntered);
			AssertNoMessageErrorContaining(declaration.US_DRWExamBadgeInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWExamPhone = "";
			AssertNoMessageError(declaration.US_DRWExamPhoneInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationInformationShouldNotBeEntered);
			AssertNoMessageErrorContaining(declaration.US_DRWExamPhoneInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_DRWExamDate = ZDateTime.Empty;
			AssertNoMessageError(declaration.US_DRWExamDateInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationInformationShouldNotBeEntered);
			AssertNoMessageErrorContaining(declaration.US_DRWExamDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DRWCommRuling()
		{
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			declaration.US_DRWCommRuling = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.US_DRWCommRulingInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(declaration.US_DRWCommRulingInfo, ACEDrawbackAddInfoJobDeclarationValidation.CommercialRulingNotAllowedForTFTEA);
			declaration.US_DRWCommRuling = "~";
			AssertHasMessageErrorContaining(declaration.US_DRWCommRulingInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(declaration.US_DRWCommRulingInfo, ACEDrawbackAddInfoJobDeclarationValidation.CommercialRulingNotAllowedForTFTEA);
			declaration.US_DRWCommRuling = CommercialRulingCodeList.Codes.BindingRuling;
			AssertNoMessageErrorContaining(declaration.US_DRWCommRulingInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(declaration.US_DRWCommRulingInfo, ACEDrawbackAddInfoJobDeclarationValidation.CommercialRulingNotAllowedForTFTEA);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._51;
			declaration.AddInfoValidation.ValidateUS_DRWCommRuling();
			AssertNoMessageErrorContaining(declaration.US_DRWCommRulingInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(declaration.US_DRWCommRulingInfo, ACEDrawbackAddInfoJobDeclarationValidation.CommercialRulingNotAllowedForTFTEA);
			declaration.US_EntryType = "77";
			declaration.AddInfoValidation.ValidateUS_DRWCommRuling();
			AssertNoMessageErrorContaining(declaration.US_DRWCommRulingInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(declaration.US_DRWCommRulingInfo, ACEDrawbackAddInfoJobDeclarationValidation.CommercialRulingNotAllowedForTFTEA);
			declaration.US_DRWCommRuling = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.US_DRWCommRulingInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(declaration.US_DRWCommRulingInfo, ACEDrawbackAddInfoJobDeclarationValidation.CommercialRulingNotAllowedForTFTEA);
		}

		public void TestCheckUS_DRWUnUsedWine()
		{
			declaration.US_DRWUnUsedWine = false;
			AssertNoMessageError(declaration.US_DRWUnUsedWineInfo, ACEDrawbackAddInfoJobDeclarationValidation.SubstitutedUnusedWineCertificationRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._74;
			declaration.AddInfoValidation.ValidateUS_DRWUnUsedWine();
			AssertHasMessageError(declaration.US_DRWUnUsedWineInfo, ACEDrawbackAddInfoJobDeclarationValidation.SubstitutedUnusedWineCertificationRequired);
			declaration.US_DRWUnUsedWine = true;
			AssertNoMessageError(declaration.US_DRWUnUsedWineInfo, ACEDrawbackAddInfoJobDeclarationValidation.SubstitutedUnusedWineCertificationRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._75;
			declaration.US_DRWUnUsedWine = false;
			AssertNoMessageError(declaration.US_DRWUnUsedWineInfo, ACEDrawbackAddInfoJobDeclarationValidation.SubstitutedUnusedWineCertificationRequired);
		}

		public void TestCheckUS_DRWBillOfFormula()
		{
			declaration.US_DRWBillOfFormula = false;
			AssertNoMessageError(declaration.US_DRWBillOfFormulaInfo, ACEDrawbackAddInfoJobDeclarationValidation.BillofMaterialsCertificationRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._51;
			declaration.AddInfoValidation.ValidateUS_DRWBillOfFormula();
			AssertHasMessageError(declaration.US_DRWBillOfFormulaInfo, ACEDrawbackAddInfoJobDeclarationValidation.BillofMaterialsCertificationRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._75;
			declaration.AddInfoValidation.ValidateUS_DRWBillOfFormula();
			AssertHasMessageError(declaration.US_DRWBillOfFormulaInfo, ACEDrawbackAddInfoJobDeclarationValidation.BillofMaterialsCertificationRequired);
			declaration.US_DRWBillOfFormula = true;
			AssertNoMessageError(declaration.US_DRWBillOfFormulaInfo, ACEDrawbackAddInfoJobDeclarationValidation.BillofMaterialsCertificationRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._63;
			declaration.US_DRWBillOfFormula = false;
			AssertNoMessageError(declaration.US_DRWBillOfFormulaInfo, ACEDrawbackAddInfoJobDeclarationValidation.BillofMaterialsCertificationRequired);
		}

		public void TestCheckUS_DRWDestroyedValuation()
		{
			declaration.US_DRWDestroyedValuation = false;
			AssertNoMessageError(declaration.US_DRWDestroyedValuationInfo, ACEDrawbackAddInfoJobDeclarationValidation.DestroyedValuationCertificationRequired);
			declaration.US_DRWDestroyedValuation = true;
			AssertHasMessageError(declaration.US_DRWDestroyedValuationInfo, ACEDrawbackAddInfoJobDeclarationValidation.DestroyedValuationCertificationRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._53;
			declaration.AddInfoValidation.ValidateUS_DRWDestroyedValuation();
			AssertNoMessageError(declaration.US_DRWDestroyedValuationInfo, ACEDrawbackAddInfoJobDeclarationValidation.DestroyedValuationCertificationRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._73;
			declaration.AddInfoValidation.ValidateUS_DRWDestroyedValuation();
			AssertNoMessageError(declaration.US_DRWDestroyedValuationInfo, ACEDrawbackAddInfoJobDeclarationValidation.DestroyedValuationCertificationRequired);
			declaration.US_EntryType = "77";
			declaration.AddInfoValidation.ValidateUS_DRWDestroyedValuation();
			AssertNoMessageError(declaration.US_DRWDestroyedValuationInfo, ACEDrawbackAddInfoJobDeclarationValidation.DestroyedValuationCertificationRequired);
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._57;
			declaration.US_DRWDestroyedValuation = false;
			AssertNoMessageError(declaration.US_DRWDestroyedValuationInfo, ACEDrawbackAddInfoJobDeclarationValidation.DestroyedValuationCertificationRequired);
		}

		public void TestCheckUS_DRWDestructionResult()
		{
			declaration.US_DRWDestructionResult = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.US_DRWDestructionResultInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(declaration.US_DRWDestructionResultInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationResultRequired);
			declaration.US_DRWDestructionResult = "~";
			AssertHasMessageErrorContaining(declaration.US_DRWDestructionResultInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(declaration.US_DRWDestructionResultInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationResultRequired);
			declaration.US_DRWDestructionResult = DrawbackDestructionResultCodes.Codes.Discrepant;
			AssertNoMessageErrorContaining(declaration.US_DRWDestructionResultInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(declaration.US_DRWDestructionResultInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationResultRequired);
			declaration.US_DRWExamWitness = true;
			declaration.US_DRWDestructionResult = ZString.Empty;
			AssertNoMessageErrorContaining(declaration.US_DRWDestructionResultInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(declaration.US_DRWDestructionResultInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationResultRequired);
			declaration.US_DRWDestructionResult = DrawbackDestructionResultCodes.Codes.Discrepant;
			AssertNoMessageErrorContaining(declaration.US_DRWDestructionResultInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(declaration.US_DRWDestructionResultInfo, ACEDrawbackAddInfoJobDeclarationValidation.ExaminationResultRequired);
		}

		public void TestCheckUS_PreparerDistrictPort()
		{
			declaration.US_PreparerDistrictPort = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_PreparerDistrictPort = "~";
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, ListValidation.InvalidCodeMessageError);
			declaration.US_PreparerDistrictPort = "9900";
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(declaration.US_PreparerDistrictPortInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_AcceleratedClaimInd()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_AcceleratedClaimInd = true;
			declaration.AddInfoValidation.ValidateUS_AcceleratedClaimInd();
			AssertHasWarning(declaration.US_AcceleratedClaimIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.OtherFeeWillNotBeCalculatedOrClaimed);
			declaration.US_AcceleratedClaimInd = false;
			AssertNoWarning(declaration.US_AcceleratedClaimIndInfo, ACEDrawbackAddInfoJobDeclarationValidation.OtherFeeWillNotBeCalculatedOrClaimed);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
		}
	}
}
