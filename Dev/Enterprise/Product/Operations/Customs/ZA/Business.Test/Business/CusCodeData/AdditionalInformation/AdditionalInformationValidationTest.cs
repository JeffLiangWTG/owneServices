using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AdditionalInformationValidationTest : TestCaseWithFactory
	{
		public void TestRCCAndRCVIsPair()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry("RCC");
			testHelper.CreateAdditionalInformationCusCodeEntry("RCV");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var rccMissingValueMessage = ValidationConstants.AdditionalInformation.MissingAdditionalInformationCode("Rebate Credit Certificate", "RCC");
			var rcvMissingValueMessage = ValidationConstants.AdditionalInformation.MissingAdditionalInformationCode("Credit Rebate Value", "RCV");
			var addInfoRCC = entryLine.AdditionalInformationCodes.AddNew("RCC");
			AssertHasMessageError(addInfoRCC.CY_CodeInfo, rcvMissingValueMessage);
			var addInfoRCV = entryLine.AdditionalInformationCodes.AddNew("RCV");
			AssertNoMessageError(addInfoRCV.CY_CodeInfo, rccMissingValueMessage);
			addInfoRCC.Validation.ValidateCY_Code();
			AssertNoMessageError(addInfoRCV.CY_CodeInfo, rcvMissingValueMessage);
			addInfoRCC.Delete();
			addInfoRCV.Validation.ValidateCY_Code();
			AssertHasMessageError(addInfoRCV.CY_CodeInfo, rccMissingValueMessage);
		}

		public void TestDCCAndDCVIsPair()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry("DCC");
			testHelper.CreateAdditionalInformationCusCodeEntry("DCV");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var dccMissingValueMessage = ValidationConstants.AdditionalInformation.MissingAdditionalInformationCode("Duty Credit Certificate", "DCC");
			var dcvMissingValueMessage = ValidationConstants.AdditionalInformation.MissingAdditionalInformationCode("Duty Credit Value", "DCV");
			var addInfoDCC = entryLine.AdditionalInformationCodes.AddNew("DCC");
			AssertHasMessageError(addInfoDCC.CY_CodeInfo, dcvMissingValueMessage);
			var addInfoDCV = entryLine.AdditionalInformationCodes.AddNew("DCV");
			AssertNoMessageError(addInfoDCV.CY_CodeInfo, dccMissingValueMessage);
			addInfoDCC.Validation.ValidateCY_Code();
			AssertNoMessageError(addInfoDCV.CY_CodeInfo, dcvMissingValueMessage);
			addInfoDCC.Delete();
			addInfoDCV.Validation.ValidateCY_Code();
			AssertHasMessageError(addInfoDCV.CY_CodeInfo, dccMissingValueMessage);
		}

		public void TestBNDandPPSIsAllowedToBeAPair()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.ProvisionalPaymentSurety);
			testHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var addInfoBND = entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount);
			var addInfoPPS = entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.ProvisionalPaymentSurety);
			addInfoBND.Validation.ValidateCY_Code();
			AssertNoMessageErrors(addInfoBND.CY_CodeInfo);
			addInfoPPS.Validation.ValidateCY_Code();
			AssertNoMessageErrors(addInfoPPS.CY_CodeInfo);
		}

		public void TestAFTandAFHCanBeUsedTogether()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignImporterOrExporter);
			testHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignHaulier);
			testHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.VehicleIdentificationNumber);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;
			var addInfoAFT = entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignImporterOrExporter);
			var addInfoAFH = entryLine.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignHaulier);
			addInfoAFT.Validation.ValidateCY_Code();
			AssertNoMessageErrors(addInfoAFT.CY_CodeInfo);
			AssertNoMessageErrors(addInfoAFH.CY_CodeInfo);
			addInfoAFT.CY_Code = UniversalReferenceConstants.AdditionalInformation.VehicleIdentificationNumber;
			AssertNoMessageErrors(addInfoAFT.CY_CodeInfo);
			addInfoAFH.Validation.ValidateCY_Code();
			AssertNoMessageErrors(addInfoAFH.CY_CodeInfo);
		}

		public void TestCheckCY_Data()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.BondHolder);
			testHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RulesOfOrigin);
			testHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignImporterOrExporter);
			var emp = testHelper.CreateAdditionalInformationCusCodeEntry("EMP");
			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName("Empty", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.CountryCodes.SouthAfrica);
			emp.Attributes.AddNew("Empty", ZString.Empty);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			var addInfo1 = entryLine1.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.BondHolder);
			var additionalInfoCodeRequireDataMessage = ValidationConstants.AdditionalInformation.AdditionalInfoCodeRequireData(UniversalReferenceConstants.AdditionalInformation.BondHolder);
			AssertHasMessageError(addInfo1.CY_DataInfo, additionalInfoCodeRequireDataMessage);
			addInfo1.CY_Data = "1";
			AssertNoMessageErrors(addInfo1.CY_DataInfo);
			addInfo1.CY_Code = emp.ZZD_Code;
			var additionalInfoCodeShouldNotHaveDataMessage = ValidationConstants.AdditionalInformation.AdditionalInfoCodeShouldNotHaveData(emp.ZZD_Code);
			AssertEquals("1", addInfo1.CY_Data);
			AssertHasMessageError(addInfo1.CY_DataInfo, additionalInfoCodeShouldNotHaveDataMessage);
			addInfo1.CY_Data = ZString.Empty;
			AssertNoMessageErrors(addInfo1.CY_DataInfo);
			addInfo1.CY_Code = UniversalReferenceConstants.AdditionalInformation.RulesOfOrigin;
			AssertNoMessageErrors(addInfo1.CY_DataInfo);
			addInfo1.CY_Code = UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignImporterOrExporter;
			addInfo1.CY_Data = "123 456";
			AssertHasError(addInfo1.CY_DataInfo, ValidationConstants.AdditionalInformation.AdditionalInfoCodeRequireNoSpaceData(UniversalReferenceConstants.AdditionalInformation.AgentRepresentingAForeignImporterOrExporter));
			addInfo1.CY_Data = "123456";
			AssertNoErrors(addInfo1.CY_DataInfo);
		}

		public void TestBondHolderforLine1()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.BondHolder);
			testHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount);
			testHelper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.VehicleIdentificationNumber);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			entryLine1.CL_LineNumber = 1;
			var addInfo1 = entryLine1.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.BondHolder);
			var allLinesNeedBNDWhenBHRIsEntered = ValidationConstants.AdditionalInformation.AllLinesNeedBNDWhenBHRIsEntered;
			AssertHasMessageError(addInfo1.CY_CodeInfo, allLinesNeedBNDWhenBHRIsEntered);
			var addInfo2 = entryLine1.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount);
			AssertHasMessageError(addInfo2.CY_DataInfo, allLinesNeedBNDWhenBHRIsEntered);
			addInfo1.Validation.ValidateCY_Code();
			AssertNoMessageError(addInfo1.CY_CodeInfo, allLinesNeedBNDWhenBHRIsEntered);
			addInfo2.CY_Data = "1";
			AssertNoMessageError(addInfo2.CY_DataInfo, allLinesNeedBNDWhenBHRIsEntered);
			addInfo1.Validation.ValidateCY_Code();
			AssertNoMessageError(addInfo1.CY_CodeInfo, allLinesNeedBNDWhenBHRIsEntered);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			entryLine2.CL_LineNumber = 2;
			addInfo1.Validation.ValidateCY_Code();
			AssertHasMessageError(addInfo1.CY_CodeInfo, allLinesNeedBNDWhenBHRIsEntered);
			var addInfo3 = entryLine2.AdditionalInformationCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount);
			AssertHasMessageError(addInfo3.CY_DataInfo, allLinesNeedBNDWhenBHRIsEntered);
			addInfo1.Validation.ValidateCY_Code();
			AssertNoMessageError(addInfo1.CY_CodeInfo, allLinesNeedBNDWhenBHRIsEntered);
			addInfo3.CY_Data = "1";
			AssertNoMessageError(addInfo3.CY_DataInfo, allLinesNeedBNDWhenBHRIsEntered);
			addInfo1.Validation.ValidateCY_Code();
			AssertNoMessageError(addInfo1.CY_CodeInfo, allLinesNeedBNDWhenBHRIsEntered);
			addInfo1.CY_Code = UniversalReferenceConstants.AdditionalInformation.VehicleIdentificationNumber;
			AssertNoMessageError(addInfo1.CY_CodeInfo, allLinesNeedBNDWhenBHRIsEntered);
			addInfo3.CY_Data = ZString.Empty;
			AssertNoMessageError(addInfo3.CY_DataInfo, allLinesNeedBNDWhenBHRIsEntered);
		}

		public void TestProvisionalPaymentValidation()
		{
			var diamondLevyError = CusEntryLineValidation.DiamondLevyValueAndAmountRequiredError;
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "EXP";
			var entryLine = dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var provisionalPayment = entryLine.ProvisionalPayments.AddNew();
			var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			AssertNoRowMessageErrorContaining(entryLine, diamondLevyError);
			additionalInfo.CY_Code = "DLV";
			AssertHasRowMessageErrorContaining(entryLine, diamondLevyError);
			additionalInfo.CY_Code = "ABC";
			AssertNoRowMessageErrorContaining(entryLine, diamondLevyError);
			provisionalPayment.CY_Code = "DLA";
			additionalInfo.CY_Code = "DLV";
			AssertNoRowMessageErrorContaining(entryLine, diamondLevyError);
		}
	}
}
