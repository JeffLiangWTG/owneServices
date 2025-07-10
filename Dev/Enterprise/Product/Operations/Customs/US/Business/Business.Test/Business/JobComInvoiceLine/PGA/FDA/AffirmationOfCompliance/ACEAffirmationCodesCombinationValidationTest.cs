using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class ACEAffirmationCodesCombinationValidationTest : TestCaseWithFactory
	{
		public void TestValidateAocForDRU()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			var affirmationCode = fda.AffirmationCodes.AddNew();
			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_804;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._080012;
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "DA", fda.US_ProcessingCode, fda.US_IntendedUseCode));
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "DLS", fda.US_ProcessingCode, fda.US_IntendedUseCode));
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "FSR", fda.US_ProcessingCode, fda.US_IntendedUseCode));
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "PRN", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PRE;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._080012;
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "REG", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PRE;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._980000;
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "REG", fda.US_ProcessingCode, fda.US_IntendedUseCode));
		}

		[TestDate(2016, 5, 18)]
		public void TestValidateAffirmationOfCodesForProcessingCodeAndIntendedUseCode1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._180009;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_ALG;
			fda.AffirmationCodes.RemoveAndDeleteAll();
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "IND", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			var affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.IND;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "IND", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._080000;
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "BLN", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.BLN;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "BLN", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_BBA;
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequiredIncludes, "DA", fda.US_ProcessingCode, fda.US_IntendedUseCode, "NDA' and 'AND"));

			affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.NDA;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequiredIncludes, "DA", fda.US_ProcessingCode, fda.US_IntendedUseCode, "NDA' and 'AND"));

			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._082000;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_HCT;
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "HRN", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.HRN;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "HRN", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._180016;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.BIO_ALG;
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "BLN", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.BLN;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "BLN", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._970000;
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "IFE", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.IFE;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "IFE", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PRE;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._080012;
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "DA", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.DA;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "DA", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.VME_ADR;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._085003;
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.EitherVNAOrVANIsRequired, "VNA", fda.US_ProcessingCode));

			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._100000;
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.EitherVNAOrVANIsRequired, "VNA", fda.US_ProcessingCode));

			affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.VNA;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.EitherVNAOrVANIsRequired, "VNA", fda.US_ProcessingCode));
		}

		public void TestValidateAffirmationOfCodesForProcessingCodeAndIntendedUseCode2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._085003;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.VME_ADR;
			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, ACEAffirmationCodesCombinationValidation.EitherVNAOrVANIsRequired);

			var affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.VNA;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, ACEAffirmationCodesCombinationValidation.EitherVNAOrVANIsRequired);

			fda.AffirmationCodes.RemoveAndDeleteAll();

			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._150020;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, "Affirmation Of Compliance code 'NDC' is required when Processing Code is 'ADR'");

			affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.NDC;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, "Affirmation Of Compliance code 'NDC' is required when Processing Code is 'ADR'");

			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._085003;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, "Affirmation Of Compliance code 'NDC' is required when Processing Code is 'ADR'");

			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_PRE;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._080012;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "REG", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.REG;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "REG", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._130000;
			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "REG", fda.US_ProcessingCode, fda.US_IntendedUseCode));
			affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.REG;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "REG", fda.US_ProcessingCode, fda.US_IntendedUseCode));

			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._180009;
			fda.AffirmationCodes.RemoveAndDeleteAll();
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertHasMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "IND", fda.US_ProcessingCode, fda.US_IntendedUseCode));
			affirmationCode = fda.AffirmationCodes.AddNew();
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.IND;
			fda.AddInfoValidation.ValidateUS_ProcessingCode();
			AssertNoMessageErrorContaining(fda.US_ProcessingCodeInfo, string.Format(ACEAffirmationCodesCombinationValidation.AffirmationOfCodeForCombinationIsRequired, "IND", fda.US_ProcessingCode, fda.US_IntendedUseCode));
		}
	}
}
