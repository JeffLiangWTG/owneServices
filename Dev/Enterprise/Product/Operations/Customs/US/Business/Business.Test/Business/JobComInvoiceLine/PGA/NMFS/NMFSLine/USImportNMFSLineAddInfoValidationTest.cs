using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USImportNMFSLineAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_IFTPPermitNumber()
		{
			NMFSLine.US_IFTPPermitNumber = ZString.Empty;
			AssertNoMessageErrorContaining(NMFSLine.US_IFTPPermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			NMFSLine.AddInfoValidation.ValidateUS_IFTPPermitNumber();
			AssertHasMessageErrorContaining(NMFSLine.US_IFTPPermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			NMFSLine.AddInfoValidation.ValidateUS_IFTPPermitNumber();
			AssertHasMessageErrorContaining(NMFSLine.US_IFTPPermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			NMFSLine.AddInfoValidation.ValidateUS_IFTPPermitNumber();
			AssertNoMessageErrorContaining(NMFSLine.US_IFTPPermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_SpeciesCode = "BBB";
			NMFSLine.AddInfoValidation.ValidateUS_IFTPPermitNumber();
			AssertNoMessageErrorContaining(NMFSLine.US_IFTPPermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_SpeciesCode = "AAA";
			NMFSLine.AddInfoValidation.ValidateUS_IFTPPermitNumber();
			AssertHasMessageErrorContaining(NMFSLine.US_IFTPPermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			NMFSLine.AddInfoValidation.ValidateUS_IFTPPermitNumber();
			AssertHasMessageErrorContaining(NMFSLine.US_IFTPPermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.AddInfoValidation.ValidateUS_IFTPPermitNumber();
			AssertNoMessageErrorContaining(NMFSLine.US_IFTPPermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_SpeciesCode = "BBB";
			NMFSLine.AddInfoValidation.ValidateUS_IFTPPermitNumber();
			AssertNoMessageErrorContaining(NMFSLine.US_IFTPPermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_SpeciesCode = "AAA";
			NMFSLine.AddInfoValidation.ValidateUS_IFTPPermitNumber();
			AssertHasMessageErrorContaining(NMFSLine.US_IFTPPermitNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProgramType()
		{
			InvoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
			InvoiceLine.US_NMFSCOAInd = OGAIndicatorList.Codes.Declared;
			InvoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			InvoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			InvoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
			NMFSLine.US_ProgramType = ZString.Empty;
			AssertHasMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);
			NMFSLine.US_ProgramType = "!@";
			AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertNoMessageErrors(NMFSLine.US_ProgramTypeInfo);
			foreach (var programType in new[] { NMFSProgramCodeList.Codes.HMS, NMFSProgramCodeList.Codes._370, NMFSProgramCodeList.Codes.SIM, NMFSProgramCodeList.Codes.COA })
			{
				NMFSLine.US_ProgramType = programType;
				nmfsLine.HarvestingDetails.RemoveAndDeleteAll();
				var message = ValidationConstants.NMFS.HarvestingDetailIsRequired(programType);

				NMFSLine.US_SpeciesCode = "BBB";
				NMFSLine.AddInfoValidation.ValidateUS_ProgramType();
				AssertNoMessageError(NMFSLine.US_ProgramTypeInfo, message);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);

				var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
				NMFSLine.AddInfoValidation.ValidateUS_ProgramType();
				AssertNoMessageError(NMFSLine.US_ProgramTypeInfo, message);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);

				harvestingDetail.Delete();
				NMFSLine.AddInfoValidation.ValidateUS_ProgramType();
				AssertNoMessageError(NMFSLine.US_ProgramTypeInfo, message);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.ValidationModes = ValidationModes.None;
				NMFSLine.AddInfoValidation.ValidateUS_ProgramType();
				AssertNoMessageError(NMFSLine.US_ProgramTypeInfo, message);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.RecalculateValidationModesOnDeclaration();
				NMFSLine.AddInfoValidation.ValidateUS_ProgramType();
				AssertNoMessageError(NMFSLine.US_ProgramTypeInfo, message);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);

				NMFSLine.HarvestingDetails.RemoveAndDeleteAll();
				NMFSLine.US_SpeciesCode = "AAA";
				NMFSLine.AddInfoValidation.ValidateUS_ProgramType();
				AssertHasMessageError(NMFSLine.US_ProgramTypeInfo, message);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);

				var harvestingDetail2 = NMFSLine.HarvestingDetails.AddNew();
				NMFSLine.AddInfoValidation.ValidateUS_ProgramType();
				AssertNoMessageError(NMFSLine.US_ProgramTypeInfo, message);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);

				harvestingDetail2.Delete();
				NMFSLine.AddInfoValidation.ValidateUS_ProgramType();
				AssertHasMessageError(NMFSLine.US_ProgramTypeInfo, message);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.ValidationModes = ValidationModes.None;
				NMFSLine.AddInfoValidation.ValidateUS_ProgramType();
				AssertNoMessageError(NMFSLine.US_ProgramTypeInfo, message);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.RecalculateValidationModesOnDeclaration();
				NMFSLine.AddInfoValidation.ValidateUS_ProgramType();
				AssertHasMessageError(NMFSLine.US_ProgramTypeInfo, message);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(NMFSLine.US_ProgramTypeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckUS_DISDocumentID()
		{
			NMFSLine.US_AMLRPermitNumber = "12345";
			NMFSLine.US_DISDocumentID = "";
			AssertNoWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			NMFSLine.US_DISDocumentID = "ASD@43";
			AssertHasWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);
			AssertNoWarning(NMFSLine.US_DISDocumentIDInfo, ValidationConstants.NMFS.DocumentIDIsRequired);
			NMFSLine.US_DISDocumentID = ZString.Empty;
			AssertNoWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);
			AssertHasWarning(NMFSLine.US_DISDocumentIDInfo, ValidationConstants.NMFS.DocumentIDIsRequired);

			Declaration.ValidationModes = ValidationModes.None;
			NMFSLine.AddInfoValidation.ValidateUS_DISDocumentID();
			AssertNoWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);
			AssertNoWarning(NMFSLine.US_DISDocumentIDInfo, ValidationConstants.NMFS.DocumentIDIsRequired);
			Declaration.RecalculateValidationModesOnDeclaration();
			NMFSLine.AddInfoValidation.ValidateUS_DISDocumentID();
			AssertNoWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);
			AssertHasWarning(NMFSLine.US_DISDocumentIDInfo, ValidationConstants.NMFS.DocumentIDIsRequired);

			NMFSLine.DocumentDetails.AddNew();
			NMFSLine.AddInfoValidation.ValidateUS_DISDocumentID();
			AssertNoWarning(NMFSLine.US_DISDocumentIDInfo, ValidationConstants.NMFS.DocumentIDIsRequired);

			NMFSLine.US_DISDocumentID = "ASD@43";
			AssertHasWarning(NMFSLine.US_DISDocumentIDInfo, ValidationConstants.NMFS.DocumentDetailsWontBeSent);

			NMFSLine.DocumentDetails.RemoveAndDeleteAll();
			NMFSLine.US_DISDocumentID = ZString.Empty;
			AssertNoWarning(NMFSLine.US_DISDocumentIDInfo, ValidationConstants.NMFS.DocumentDetailsWontBeSent);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			NMFSLine.AddInfoValidation.ValidateUS_DISDocumentID();
			AssertNoWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);
			AssertHasWarning(NMFSLine.US_DISDocumentIDInfo, ValidationConstants.NMFS.DocumentIDIsRequired);
			NMFSLine.US_DISDocumentID = "ASD@43";
			AssertHasWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);
			AssertNoWarning(NMFSLine.US_DISDocumentIDInfo, ValidationConstants.NMFS.DocumentIDIsRequired);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertNoMessageErrorContaining(NMFSLine.US_DISDocumentIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);
			NMFSLine.US_DISDocumentID = ZString.Empty;
			AssertHasMessageErrorContaining(NMFSLine.US_DISDocumentIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertNoMessageErrorContaining(NMFSLine.US_DISDocumentIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);
			NMFSLine.US_Commodity = FishStateList.Codes.FreshToothfish;
			AssertHasMessageErrorContaining(NMFSLine.US_DISDocumentIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);

			Declaration.ValidationModes = ValidationModes.None;
			NMFSLine.AddInfoValidation.ValidateUS_DISDocumentID();
			AssertNoMessageErrorContaining(NMFSLine.US_DISDocumentIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);
			Declaration.RecalculateValidationModesOnDeclaration();
			NMFSLine.AddInfoValidation.ValidateUS_DISDocumentID();
			AssertHasMessageErrorContaining(NMFSLine.US_DISDocumentIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoWarningContaining(NMFSLine.US_DISDocumentIDInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestCheckUS_DocumentType()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			NMFSLine.US_DocumentType = NMFSHMSDocumentIdentifierList.Codes.BluefinTunaCatchDocument;
			AssertNoMessageErrorContaining(NMFSLine.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoWarning(NMFSLine.US_DocumentTypeInfo, ValidationConstants.NMFS.DocumentIsRequiredForHMSNonShark);
			NMFSLine.US_DocumentType = "!@";
			AssertHasMessageErrorContaining(NMFSLine.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoWarning(NMFSLine.US_DocumentTypeInfo, ValidationConstants.NMFS.DocumentIsRequiredForHMSNonShark);
			NMFSLine.US_DocumentType = ZString.Empty;
			AssertNoMessageErrorContaining(NMFSLine.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasWarning(NMFSLine.US_DocumentTypeInfo, ValidationConstants.NMFS.DocumentIsRequiredForHMSNonShark);

			NMFSLine.DocumentDetails.AddNew();
			NMFSLine.AddInfoValidation.ValidateUS_DocumentType();
			AssertNoWarning(NMFSLine.US_DocumentTypeInfo, ValidationConstants.NMFS.DocumentIsRequiredForHMSNonShark);

			NMFSLine.US_DocumentType = "!@";
			AssertHasWarning(NMFSLine.US_DocumentTypeInfo, ValidationConstants.NMFS.DocumentDetailsWontBeSent);

			NMFSLine.DocumentDetails.RemoveAndDeleteAll();
			NMFSLine.US_DocumentType = ZString.Empty;
			AssertNoWarning(NMFSLine.US_DocumentTypeInfo, ValidationConstants.NMFS.DocumentDetailsWontBeSent);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			NMFSLine.US_DocumentType = "!@";
			AssertHasMessageErrorContaining(NMFSLine.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);
			NMFSLine.US_DocumentType = ZString.Empty;
			AssertNoMessageErrorContaining(NMFSLine.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoNotifications(NMFSLine.US_DocumentTypeInfo);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			NMFSLine.US_Commodity = FishStateList.Codes.FreshToothfish;
			NMFSLine.US_DocumentType = NMFSAMRDocumentIdentifierList.Codes.DissostichusCatchDocument;
			AssertNoMessageError(NMFSLine.US_DocumentTypeInfo, ValidationConstants.NMFS.DocumentIsRequiredForAMRFreshToothfish);
			AssertNoMessageErrorContaining(NMFSLine.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);
			NMFSLine.US_DocumentType = "!@";
			AssertNoMessageError(NMFSLine.US_DocumentTypeInfo, ValidationConstants.NMFS.DocumentIsRequiredForAMRFreshToothfish);
			AssertHasMessageErrorContaining(NMFSLine.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);
			NMFSLine.US_DocumentType = ZString.Empty;
			AssertHasMessageError(NMFSLine.US_DocumentTypeInfo, ValidationConstants.NMFS.DocumentIsRequiredForAMRFreshToothfish);
			AssertNoMessageErrorContaining(NMFSLine.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertNoMessageError(NMFSLine.US_DocumentTypeInfo, ValidationConstants.NMFS.DocumentIsRequiredForAMRFreshToothfish);
			AssertNoMessageErrorContaining(NMFSLine.US_DocumentTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_DolphinSafeStatus()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			NMFSLine.US_DolphinSafeStatus = DolphinSafeStatusList.Codes.A;
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_DolphinSafeStatus = "!";
			AssertHasMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_DolphinSafeStatus = ZString.Empty;
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.ValidationModes = ValidationModes.None;
			NMFSLine.AddInfoValidation.ValidateUS_DolphinSafeStatus();
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			NMFSLine.AddInfoValidation.ValidateUS_DolphinSafeStatus();
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			NMFSLine.US_DolphinSafeStatus = "#";
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_DolphinSafeStatus = ZString.Empty;
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			NMFSLine.AddInfoValidation.ValidateUS_DolphinSafeStatus();
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertEquals(true, harvestingDetail.IsDeleted);
			NMFSLine.US_DolphinSafeStatus = "#";
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_DolphinSafeStatus = ZString.Empty;
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(NMFSLine.US_DolphinSafeStatusInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_PreApprovalIssuedNumber()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			NMFSLine.US_PreApprovalIssuedNumber = "ADS";
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedNumberInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_PreApprovalIssuedNumber = ZString.Empty;
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedNumberInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_Commodity = FishStateList.Codes.FreshToothfish;
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedNumberInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedNumberInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedNumberInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedNumberInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedNumberInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.ValidationModes = ValidationModes.None;
			NMFSLine.AddInfoValidation.ValidateUS_PreApprovalIssuedNumber();
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedNumberInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			NMFSLine.AddInfoValidation.ValidateUS_PreApprovalIssuedNumber();
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_PreApprovalIssuedQuantity()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			NMFSLine.US_PreApprovalIssuedQuantity = 10m;
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			NMFSLine.US_PreApprovalIssuedQuantity = -10m;
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeNegative);
			NMFSLine.US_PreApprovalIssuedQuantity = 0m;
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeNegative);

			NMFSLine.US_Commodity = FishStateList.Codes.FreshToothfish;
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeZero);

			Declaration.ValidationModes = ValidationModes.None;
			NMFSLine.AddInfoValidation.ValidateUS_PreApprovalIssuedQuantity();
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			Declaration.RecalculateValidationModesOnDeclaration();
			NMFSLine.AddInfoValidation.ValidateUS_PreApprovalIssuedQuantity();
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityInfo, MandatoryValidation.ValueCannotBeZero);
		}

		public void TestCheckUS_PreApprovalIssuedQuantityUQ()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			NMFSLine.US_PreApprovalIssuedQuantityUQ = "KG";
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityUQInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_PreApprovalIssuedQuantityUQ = ZString.Empty;
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityUQInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_Commodity = FishStateList.Codes.FreshToothfish;
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityUQInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityUQInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityUQInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityUQInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityUQInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.ValidationModes = ValidationModes.None;
			NMFSLine.AddInfoValidation.ValidateUS_PreApprovalIssuedQuantityUQ();
			AssertNoMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityUQInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			NMFSLine.AddInfoValidation.ValidateUS_PreApprovalIssuedQuantityUQ();
			AssertHasMessageErrorContaining(NMFSLine.US_PreApprovalIssuedQuantityUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_Commodity()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			NMFSLine.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertNoMessageErrorContaining(NMFSLine.US_CommodityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(NMFSLine.US_CommodityInfo, ListValidation.InvalidCodeMessageError);
			NMFSLine.US_Commodity = "!";
			AssertHasMessageErrorContaining(NMFSLine.US_CommodityInfo, ListValidation.InvalidCodeMessageError);
			NMFSLine.US_Commodity = ZString.Empty;
			AssertNoMessageErrorContaining(NMFSLine.US_CommodityInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(NMFSLine.US_CommodityInfo, MandatoryValidation.YouHaveNotEntered);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			AssertNoMessageErrorContaining(NMFSLine.US_CommodityInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertHasMessageErrorContaining(NMFSLine.US_CommodityInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.ValidationModes = ValidationModes.None;
			NMFSLine.AddInfoValidation.ValidateUS_Commodity();
			AssertNoMessageErrorContaining(NMFSLine.US_CommodityInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			NMFSLine.AddInfoValidation.ValidateUS_Commodity();
			AssertHasMessageErrorContaining(NMFSLine.US_CommodityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_Confidential()
		{
			foreach (var code in new[]
			{
				NMFSProgramCodeList.Codes.SIM,
				NMFSProgramCodeList.Codes.COA
			})
			{
				NMFSLine.US_ProgramType = code;
				NMFSLine.US_Confidential = ZBool.True;
				AssertNoMessageErrorContaining(NMFSLine.US_ConfidentialInfo, $"Confidential must be true for {code}.");
				NMFSLine.US_Confidential = ZBool.False;
				AssertHasMessageErrorContaining(NMFSLine.US_ConfidentialInfo, $"Confidential must be true for {code}.");

				Declaration.ValidationModes = ValidationModes.None;
				NMFSLine.AddInfoValidation.ValidateUS_Confidential();
				AssertNoMessageErrorContaining(NMFSLine.US_ConfidentialInfo, $"Confidential must be true for {code}.");
				Declaration.RecalculateValidationModesOnDeclaration();
				NMFSLine.AddInfoValidation.ValidateUS_Confidential();
				AssertHasMessageErrorContaining(NMFSLine.US_ConfidentialInfo, $"Confidential must be true for {code}.");
			}
		}

		public void TestCheckUS_OtherAuthorizationNumber()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.US_SpeciesCode = "AAA";
			NMFSLine.AddInfoValidation.ValidateUS_OtherAuthorizationNumber();
			AssertNoMessageErrorContaining(NMFSLine.US_OtherAuthorizationNumberInfo, ValidationConstants.NMFS.AuthorizationNumberRequired);

			NMFSLine.US_AuthorizationType = "1";
			NMFSLine.AddInfoValidation.ValidateUS_OtherAuthorizationNumber();
			AssertHasMessageErrorContaining(NMFSLine.US_OtherAuthorizationNumberInfo, ValidationConstants.NMFS.AuthorizationNumberRequired);

			NMFSLine.US_OtherAuthorizationNumber = "123";
			AssertNoMessageErrorContaining(NMFSLine.US_OtherAuthorizationNumberInfo, ValidationConstants.NMFS.AuthorizationNumberRequired);
		}

		public void TestCheckUS_NetWeight()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.US_NetWeight = 1.2;
			AssertNoMessageErrorContaining(NMFSLine.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_NetWeight = ZDecimal.Zero;
			AssertHasMessageErrorContaining(NMFSLine.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.ValidationModes = ValidationModes.None;
			NMFSLine.AddInfoValidation.ValidateUS_NetWeight();
			AssertNoMessageErrorContaining(NMFSLine.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_NetWeightUQ()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.US_NetWeightUQ = "XX";
			AssertHasMessageErrorContaining(NMFSLine.US_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);
			NMFSLine.US_NetWeight = 1.0;
			NMFSLine.US_NetWeightUQ = "KG";
			AssertNoMessageErrorContaining(NMFSLine.US_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(NMFSLine.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			NMFSLine.US_NetWeightUQ = "";
			AssertHasMessageErrorContaining(NMFSLine.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			Declaration.ValidationModes = ValidationModes.None;
			NMFSLine.AddInfoValidation.ValidateUS_NetWeightUQ();
			AssertNoMessageErrorContaining(NMFSLine.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			Declaration.RecalculateValidationModesOnDeclaration();
			NMFSLine.AddInfoValidation.ValidateUS_NetWeightUQ();
			AssertHasMessageErrorContaining(NMFSLine.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_SpeciesCode()
		{
			foreach (var code in new[]
			{
				NMFSProgramCodeList.Codes.SIM,
				NMFSProgramCodeList.Codes.COA
			})
			{
				NMFSLine.US_ProgramType = code;
				NMFSLine.US_SpeciesCode = "CCC";
				AssertHasMessageErrorContaining(NMFSLine.US_SpeciesCodeInfo, ListValidation.InvalidCodeMessageError);
				NMFSLine.US_SpeciesCode = "AAA";
				AssertNoMessageErrorContaining(NMFSLine.US_SpeciesCodeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageErrorContaining(NMFSLine.US_SpeciesCodeInfo, MandatoryValidation.YouHaveNotEntered);
				NMFSLine.US_SpeciesCode = "";
				AssertHasMessageErrorContaining(NMFSLine.US_SpeciesCodeInfo, MandatoryValidation.YouHaveNotEntered);

				Declaration.ValidationModes = ValidationModes.None;
				NMFSLine.AddInfoValidation.ValidateUS_SpeciesCode();
				AssertNoMessageErrorContaining(NMFSLine.US_SpeciesCodeInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.RecalculateValidationModesOnDeclaration();
				NMFSLine.AddInfoValidation.ValidateUS_SpeciesCode();
				AssertHasMessageErrorContaining(NMFSLine.US_SpeciesCodeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckUS_AuthorizationType()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.AddInfoValidation.ValidateUS_AuthorizationType();
			AssertNoMessageErrorContaining(NMFSLine.US_AuthorizationTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(NMFSLine.US_AuthorizationTypeInfo, ValidationConstants.NMFS.AuthorizationTypeRequired);

			NMFSLine.US_AuthorizationType = "~";
			AssertHasMessageErrorContaining(NMFSLine.US_AuthorizationTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(NMFSLine.US_AuthorizationTypeInfo, ValidationConstants.NMFS.AuthorizationTypeRequired);

			NMFSLine.US_AuthorizationType = ZString.Empty;
			NMFSLine.US_OtherAuthorizationNumber = "1";
			AssertNoMessageErrorContaining(NMFSLine.US_AuthorizationTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(NMFSLine.US_AuthorizationTypeInfo, ValidationConstants.NMFS.AuthorizationTypeRequired);

			NMFSLine.US_AuthorizationType = "1";
			AssertNoMessageErrorContaining(NMFSLine.US_AuthorizationTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(NMFSLine.US_AuthorizationTypeInfo, ValidationConstants.NMFS.AuthorizationTypeRequired);
		}

		public void TestCheckUS_SourceType()
		{
			foreach (var code in new[]
			{
				NMFSProgramCodeList.Codes.SIM,
				NMFSProgramCodeList.Codes.COA
			})
			{
				NMFSLine.US_ProgramType = code;
				NMFSLine.US_SpeciesCode = "AAA";
				NMFSLine.US_SourceType = "!";
				AssertNoMessageErrorContaining(NMFSLine.US_SourceTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(NMFSLine.US_SourceTypeInfo, ListValidation.InvalidCodeMessageError);
				NMFSLine.US_SourceType = ZString.Empty;
				AssertHasMessageErrorContaining(NMFSLine.US_SourceTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(NMFSLine.US_SourceTypeInfo, ListValidation.InvalidCodeMessageError);

				Declaration.ValidationModes = ValidationModes.None;
				NMFSLine.AddInfoValidation.ValidateUS_SourceType();
				AssertNoMessageErrorContaining(NMFSLine.US_SourceTypeInfo, MandatoryValidation.YouHaveNotEntered);
				Declaration.RecalculateValidationModesOnDeclaration();
				NMFSLine.AddInfoValidation.ValidateUS_SourceType();
				AssertHasMessageErrorContaining(NMFSLine.US_SourceTypeInfo, MandatoryValidation.YouHaveNotEntered);

				NMFSLine.US_SpeciesCode = "BBB";
				NMFSLine.US_SourceType = ZString.Empty;
				NMFSLine.AddInfoValidation.ValidateUS_SourceType();
				AssertNoMessageErrorContaining(NMFSLine.US_SourceTypeInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "USSIM");
			var code1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "AAA", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("RequiresFullData", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, Core.Constants.CountryCodes.UnitedStates);
			code1.Attributes.AddNew("RequiresFullData", "Yes");
			var code2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, "BBB", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code2.Attributes.AddNew("RequiresFullData", "No");
			Factory.Save();
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_EnableCRL = true;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		NMFSLine NMFSLine
		{
			get { return nmfsLine ?? (nmfsLine = InvoiceLine.NMFSLines.AddNew()); }
		}
		NMFSLine nmfsLine;

		#endregion
	}
}
