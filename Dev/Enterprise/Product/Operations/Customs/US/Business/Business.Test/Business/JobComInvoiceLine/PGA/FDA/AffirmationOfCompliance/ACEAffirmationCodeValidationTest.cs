using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class ACEAffirmationCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidationForVQI()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;

			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();
			affirmationOfCompliance.CY_Code = "VQI";
			affirmationOfCompliance.CY_Data = ZString.Empty;
			affirmationOfCompliance.Validation.ValidateCY_Data();
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 5 digit number")));

			affirmationOfCompliance.CY_Data = "123";
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 5 digit number")));

			affirmationOfCompliance.CY_Data = "AB123";
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 5 digit number")));

			affirmationOfCompliance.CY_Data = "12345";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 5 digit number")));
		}
		public void TestAoCForFDAProgramCodeDRU()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.DRU_804;
			fda.US_IntendedUseCode = FDAIntendedUseCodesHelper.Codes._080012;

			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();
			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.FSR;
			affirmationOfCompliance.CY_Data = "55555";
			affirmationOfCompliance.Validation.ValidateCY_Data();
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 9 digit number")));

			var affirmationOfCompliance2 = fda.AffirmationCodes.AddNew();
			affirmationOfCompliance2.CY_Code = ACE_AffirmationOfComplianceList.Codes.PRN;
			affirmationOfCompliance2.CY_Data = "55555";
			affirmationOfCompliance2.Validation.ValidateCY_Data();
			AssertHasMessageError(affirmationOfCompliance2.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance2.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 6 to 10 digit number")));
		}

		public void TestExpiredAffirmationCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();
			affirmationOfCompliance.CY_Code = "VFT";
			affirmationOfCompliance.Validation.ValidateCY_Code();
			AssertHasMessageError(affirmationOfCompliance.CY_CodeInfo, ACEAffirmationCodeValidation.AffirmationCodeExcluded);
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();
			affirmationOfCompliance.Validation.ValidateCY_Code();
			affirmationOfCompliance.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(affirmationOfCompliance.CY_CodeInfo, ACEAffirmationCodeValidation.CodeRequired);
			AssertEquals(0, affirmationOfCompliance.CY_DataInfo.Notifications.Count());

			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.NDA;
			affirmationOfCompliance.CY_Data = "test";
			AssertNoMessageErrorContaining(affirmationOfCompliance.CY_CodeInfo, ACEAffirmationCodeValidation.CodeRequired);
			AssertEquals(0, affirmationOfCompliance.CY_DataInfo.Notifications.Count());
		}

		[NUnit.Framework.TestDate(2016, 5, 28)]
		public void TestValidateDataFormatForBiologic()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();

			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.DA;
			affirmationOfCompliance.CY_Data = "1234";
			var text = string.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "BA followed by a 4 to 6 digit number OR BN followed by a 5 to 6 digit number OR a 6 digit number"));
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "BA1234";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "BN1234";
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "BN12345";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "123456";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "1234567";
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, text);
		}

		public void TestValidateDataFormatForMedicalDevices()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			fda.US_ProgramCode = FDAProgramCodeList.Codes.DEV;
			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();
			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.PM;
			affirmationOfCompliance.CY_Data = "123";
			var text = string.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "Any of the following: P+6N; N+4N, 5N,or 6N; D+6N; H+6N; K+6N; DEN+6N; BP+4-6N; BK+6N; BH+6N; BM+6N; BR+6N; DK+6N; BD+6N, where N is a number"));
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "D123456";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "H123456";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "N12345";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "P123456";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "BK123456";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);

			affirmationOfCompliance = fda.AffirmationCodes.AddNew();
			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.IDE;
			affirmationOfCompliance.CY_Data = "1234567890123";
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "NNNN o NNNNN or GNNNNNN should be entered except for 'G000000', '0000' and '00000', where N is a number")));

			affirmationOfCompliance.CY_Data = "G012345";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "NNNN o NNNNN or GNNNNNN should be entered except for 'G000000', '0000' and '00000', where N is a number")));
		}

		public void TestValidateDataFormat()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;

			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();
			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.REG;
			affirmationOfCompliance.CY_Data = "ABC";
			var text = string.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 9 digit number"));
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "123456789";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
		}

		[NUnit.Framework.TestDate(2016, 5, 28)]
		public void TestValidateDataForCosmetics()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.COS;

			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();
			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.COS;
			affirmationOfCompliance.CY_Data = "123";
			var text = string.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 7 or 10 digit number"));
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "1234567";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
		}

		public void TestValidateDataFormatForFood()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();

			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.SID;
			fda.US_ProductCode = "28FRF01";
			affirmationOfCompliance.CY_Data = "45941425845";
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, string.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "an 11 digit number format 'CCYYMMDDNNN'")));
		}

		public void TestValidateDataFormatForVeterinary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			fda.US_ProgramCode = FDAProgramCodeList.Codes.VME;
			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();

			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.VNA;
			affirmationOfCompliance.CY_Data = "456";
			var text = string.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 6 digit number"));
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "985445";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
		}

		public void TestValidateDataFormatForDrugs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();

			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.PM;
			affirmationOfCompliance.CY_Data = "123";
			var text = string.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "one of: D, H, K or P followed by 6 digits OR N followed by 4 to 6 digits OR DEN followed by 6 digits"));
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "D123456";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "H123456";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "N12345";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "P123456";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, text);
			affirmationOfCompliance.CY_Data = "BK123456";
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, text);
		}

		public void TestValidateDataForRadiationEmittingProducts()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			fda.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();
			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.RB1;
			AssertHasMessageError(affirmationOfCompliance.CY_CodeInfo, ZString.Format(ACEAffirmationCodeValidation.AdditionalCodeRequired, "ACC", " or 'ANC'", "RB1"));

			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			affirmationOfCompliance.Validation.ValidateCY_Data();
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "NNXNNNN or NNXNNNN-NNN, where N is a number and X is a character.")));
			AssertNoMessageError(affirmationOfCompliance.CY_CodeInfo, ZString.Format(ACEAffirmationCodeValidation.AdditionalCodeRequired, "ACC", " or 'ANC'", "RB1"));

			fda.AffirmationCodes.RemoveAndDeleteAll();
			affirmationOfCompliance = fda.AffirmationCodes.AddNew();
			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			AssertHasMessageErrorContaining(affirmationOfCompliance.CY_CodeInfo, ZString.Format(ACEAffirmationCodeValidation.AdditionalCodeRequired, "RB1", ZString.Empty, "ACC"));

			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.ANC;
			affirmationOfCompliance.Validation.ValidateCY_Code();
			AssertHasMessageErrorContaining(affirmationOfCompliance.CY_CodeInfo, ZString.Format(ACEAffirmationCodeValidation.AdditionalCodeRequired, "RB1", ZString.Empty, "ANC"));

			fda.AffirmationCodes.AddNew(ACE_AffirmationOfComplianceList.Codes.RB1, "");
			affirmationOfCompliance.Validation.ValidateCY_Code();
			AssertNoMessageErrorContaining(affirmationOfCompliance.CY_CodeInfo, ZString.Format(ACEAffirmationCodeValidation.AdditionalCodeRequired, "RB1", ZString.Empty, "ANC"));
		}

		public void TestValidateDataNotRequiredAndDataFormat()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			fda.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();

			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.RA3;
			affirmationOfCompliance.Validation.ValidateCY_Data();
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, "Only the Affirmation of Compliance Code should be entered"));
			affirmationOfCompliance.CY_Data = "1";
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, "Only the Affirmation of Compliance Code should be entered"));

			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.ACC;
			affirmationOfCompliance.Validation.ValidateCY_Data();
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "NNXNNNN or NNXNNNN-NNN, where N is a number and X is a character.")));
			affirmationOfCompliance.CY_Data = "41c5433";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "NNXNNNN or NNXNNNN-NNN, where N is a number and X is a character.")));

			var fda2 = invoiceLine.ACE_FDALines.AddNew();
			fda2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			affirmationOfCompliance = fda2.AffirmationCodes.AddNew();
			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.FCE;
			affirmationOfCompliance.CY_Data = "4154";
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 5 digit number")));
			affirmationOfCompliance.CY_Data = "41541";
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 5 digit number")));
		}

		public void TestCheckCY_DataForFSXAndRNE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			var affirmationOfCompliance = fda.AffirmationCodes.AddNew();

			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.FSX;
			affirmationOfCompliance.Validation.ValidateCY_Data();
			AssertNoMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, "Only the Affirmation of Compliance Code should be entered"));
			affirmationOfCompliance.CY_Data = "1111";
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, "Only the Affirmation of Compliance Code should be entered"));

			affirmationOfCompliance.CY_Code = ACE_AffirmationOfComplianceList.Codes.RNE;
			affirmationOfCompliance.CY_Data = "2222";
			AssertHasMessageError(affirmationOfCompliance.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationOfCompliance.CY_Code, "Only the Affirmation of Compliance Code should be entered"));
		}

		public void TestCheckCY_DataForRB1()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			var affirmationCode = fda.AffirmationCodes.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			affirmationCode.CY_Code = ACE_AffirmationOfComplianceList.Codes.RB1;
			affirmationCode.CY_Data = "1111";
			AssertHasMessageError(affirmationCode.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationCode.CY_Code, "Only the Affirmation of Compliance Code should be entered"));

			affirmationCode.CY_Code = "OTH";
			affirmationCode.CY_Data = "1234";
			AssertEquals(0, affirmationCode.CY_DataInfo.Notifications.Count());
		}

		public void TestCheckCY_DataMaskAndErrorText()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			var affirmationCodes = fda.AffirmationCodes.AddNew();
			affirmationCodes.CY_Code = ACE_AffirmationOfComplianceList.Codes.STN;
			affirmationCodes.CY_Data = "1111";
			AssertHasMessageError(affirmationCodes.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationCodes.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 6 digit number")));
			affirmationCodes.CY_Data = "111111";
			AssertNoMessageError(affirmationCodes.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationCodes.CY_Code, ZString.Format(ACEAffirmationCodeValidation.AoCDataFormatIsIncorrect, "a 6 digit number")));

			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			affirmationCodes.CY_Code = ACE_AffirmationOfComplianceList.Codes.UFC;
			affirmationCodes.CY_Data = "1111";
			AssertHasMessageError(affirmationCodes.CY_DataInfo, ZString.Format(ACEAffirmationCodeValidation.AoCDataMessageErrorPrefix, affirmationCodes.CY_Code, ACEAffirmationCodeValidation.AoCDataNoErrorTextInRefDb));
		}

		#region Set Up

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var codeTypeBIO = helper.CreateNewOrGetExistingCusCodeType("ACBIO", "FDA BIO Affirmation of Compliance Code", dataGrouping.ZZZ_DataGrouping);
			var codeTypeCOS = helper.CreateNewOrGetExistingCusCodeType("ACCOS", "FDA COS Affirmation of Compliance Code", dataGrouping.ZZZ_DataGrouping);
			var codeTypeDEV = helper.CreateNewOrGetExistingCusCodeType("ACDEV", "FDA DEV Affirmation of Compliance Code", dataGrouping.ZZZ_DataGrouping);
			var codeTypeFOO = helper.CreateNewOrGetExistingCusCodeType("ACFOO", "FDA FOO Affirmation of Compliance Code", dataGrouping.ZZZ_DataGrouping);
			var codeTypeDRU = helper.CreateNewOrGetExistingCusCodeType("ACDRU", "FDA DRU Affirmation of Compliance Code", dataGrouping.ZZZ_DataGrouping);
			var codeTypeRAD = helper.CreateNewOrGetExistingCusCodeType("ACRAD", "FDA RAD Affirmation of Compliance Code", dataGrouping.ZZZ_DataGrouping);
			var codeTypeVME = helper.CreateNewOrGetExistingCusCodeType("ACVME", "FDA VME Affirmation of Compliance Code", dataGrouping.ZZZ_DataGrouping);
			var attributeBIOUSAOCCodeMask = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeMask, "AOC Format Mask", codeTypeBIO.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeCOSUSAOCCodeMask = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeMask, "AOC Format Mask", codeTypeCOS.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeDEVUSAOCCodeMask = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeMask, "AOC Format Mask", codeTypeDEV.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeFOOUSAOCCodeMask = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeMask, "AOC Format Mask", codeTypeFOO.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeDRUUSAOCCodeMask = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeMask, "AOC Format Mask", codeTypeDRU.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeRADUSAOCCodeMask = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeMask, "AOC Format Mask", codeTypeRAD.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeVMEUSAOCCodeMask = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeMask, "AOC Format Mask", codeTypeVME.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeBIOUSAOCCodeErrorText = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeErrorText, "AOC Error Text", codeTypeBIO.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeCOSUSAOCCodeErrorText = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeErrorText, "AOC Error Text", codeTypeCOS.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeDEVUSAOCCodeErrorText = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeErrorText, "AOC Error Text", codeTypeDEV.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeFOOUSAOCCodeErrorText = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeErrorText, "AOC Error Text", codeTypeFOO.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeDRUUSAOCCodeErrorText = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeErrorText, "AOC Error Text", codeTypeDRU.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeRADUSAOCCodeErrorText = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeErrorText, "AOC Error Text", codeTypeRAD.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var attributeVMEUSAOCCodeErrorText = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.USAOCCodeErrorText, "AOC Error Text", codeTypeVME.ZZK_CodeType, dataGrouping.ZZZ_DataGrouping);
			var codeListSTN = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeBIO.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.STN, ACE_AffirmationOfComplianceList.Descriptions.STN, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListDA = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeBIO.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.DA, ACE_AffirmationOfComplianceList.Descriptions.DA, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListDLS = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeBIO.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.DLS, ACE_AffirmationOfComplianceList.Descriptions.DLS, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListCOS = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeCOS.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.COS, ACE_AffirmationOfComplianceList.Descriptions.COS, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListDEVPM = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeDEV.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.PM, ACE_AffirmationOfComplianceList.Descriptions.PM, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListIDE = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeDEV.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.IDE, ACE_AffirmationOfComplianceList.Descriptions.IDE, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListVQI = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeFOO.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.VQI, ACE_AffirmationOfComplianceList.Descriptions.VQI, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListSID = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeFOO.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.SID, ACE_AffirmationOfComplianceList.Descriptions.SID, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListFCE = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeFOO.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.FCE, ACE_AffirmationOfComplianceList.Descriptions.FCE, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListFSX = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeFOO.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.FSX, ACE_AffirmationOfComplianceList.Descriptions.FSX, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListRNE = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeFOO.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.RNE, ACE_AffirmationOfComplianceList.Descriptions.RNE, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListFSR = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeDRU.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.FSR, ACE_AffirmationOfComplianceList.Descriptions.FSR, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListPM = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeDRU.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.PM, ACE_AffirmationOfComplianceList.Descriptions.PM, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListPRN = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeDRU.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.PRN, ACE_AffirmationOfComplianceList.Descriptions.PRN, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListREG = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeDRU.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.REG, ACE_AffirmationOfComplianceList.Descriptions.REG, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListUFC = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeDRU.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.UFC, ACE_AffirmationOfComplianceList.Descriptions.UFC, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListRA3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeRAD.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.RA3, ACE_AffirmationOfComplianceList.Descriptions.RA3, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListRA4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeRAD.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.RA4, ACE_AffirmationOfComplianceList.Descriptions.RA4, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListRB1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeRAD.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.RB1, ACE_AffirmationOfComplianceList.Descriptions.RB1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListACC = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeRAD.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.ACC, ACE_AffirmationOfComplianceList.Descriptions.ACC, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var codeListVNA = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeTypeVME.ZZK_CodeType, ACE_AffirmationOfComplianceList.Codes.VNA, ACE_AffirmationOfComplianceList.Descriptions.VNA, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListSTN.PK, attributeBIOUSAOCCodeMask.ZXE_Name, @"[0-9]{6}");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListDA.PK, attributeBIOUSAOCCodeMask.ZXE_Name, @"(BA[0-9]{4,6}|BN[0-9]{5,6}|[0-9]{6})");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListDLS.PK, attributeBIOUSAOCCodeMask.ZXE_Name, @"[0-9]{7}");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListCOS.PK, attributeCOSUSAOCCodeMask.ZXE_Name, @"([0-9]{7}|[0-9]{10})");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListDEVPM.PK, attributeDEVUSAOCCodeMask.ZXE_Name, @"(([PDHK][0-9]{6})|(BP)[0-9]{4,6}|N[0-9]{4,6}|(DEN|BK|BH|BM|BR|DK|BD)\d{6})");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListIDE.PK, attributeDEVUSAOCCodeMask.ZXE_Name, @"((?!0{4})\d{4}|(?!0{5})\d{5}|G(?!0{6})\d{6}|NSR)");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListVQI.PK, attributeFOOUSAOCCodeMask.ZXE_Name, @"[0-9]{5}");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListSID.PK, attributeFOOUSAOCCodeMask.ZXE_Name, @"(((19|20|21|22)\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])\d{3}))");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListFCE.PK, attributeFOOUSAOCCodeMask.ZXE_Name, @"[0-9]{5}");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListFSX.PK, attributeFOOUSAOCCodeMask.ZXE_Name, @"(?![\s\S])");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListRNE.PK, attributeFOOUSAOCCodeMask.ZXE_Name, @"(?![\s\S])");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListFSR.PK, attributeDRUUSAOCCodeMask.ZXE_Name, @"[0-9]{9}");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPM.PK, attributeDRUUSAOCCodeMask.ZXE_Name, @"([P|D|H|K][0-9]{6}|DEN[0-9]{6}|N[0-9]{4,6})");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPRN.PK, attributeDRUUSAOCCodeMask.ZXE_Name, @"[0-9]{6,10}");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListREG.PK, attributeDRUUSAOCCodeMask.ZXE_Name, @"[0-9]{9}");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListUFC.PK, attributeDRUUSAOCCodeMask.ZXE_Name, @"[0-9]{6,10}");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListRA3.PK, attributeRADUSAOCCodeMask.ZXE_Name, @"(?![\s\S])");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListRA4.PK, attributeRADUSAOCCodeMask.ZXE_Name, @"(?![\s\S])");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListRB1.PK, attributeRADUSAOCCodeMask.ZXE_Name, @"(?![\s\S])");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListACC.PK, attributeRADUSAOCCodeMask.ZXE_Name, @"[0-9]{2}\w{1}[0-9]{4}(-[0-9]{3})?");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListVNA.PK, attributeVMEUSAOCCodeMask.ZXE_Name, @"[0-9]{6}");
			var errorMessageSTN = @"a 6 digit number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListSTN.PK, attributeBIOUSAOCCodeErrorText.ZXE_Name, errorMessageSTN);
			var errorMessageDA = @"BA followed by a 4 to 6 digit number OR BN followed by a 5 to 6 digit number OR a 6 digit number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListDA.PK, attributeBIOUSAOCCodeErrorText.ZXE_Name, errorMessageDA);
			var errorMessageDLS = @"a 7 digit number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListDLS.PK, attributeBIOUSAOCCodeErrorText.ZXE_Name, errorMessageDLS);
			var errorMessageCOS = @"a 7 or 10 digit number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListCOS.PK, attributeCOSUSAOCCodeErrorText.ZXE_Name, errorMessageCOS);
			var errorMessageDEVPM = @"Any of the following: P+6N; N+4N, 5N,or 6N; D+6N; H+6N; K+6N; DEN+6N; BP+4-6N; BK+6N; BH+6N; BM+6N; BR+6N; DK+6N; BD+6N, where N is a number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListDEVPM.PK, attributeDEVUSAOCCodeErrorText.ZXE_Name, errorMessageDEVPM);
			var errorMessageIDE = @"NNNN o NNNNN or GNNNNNN should be entered except for 'G000000', '0000' and '00000', where N is a number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListIDE.PK, attributeDEVUSAOCCodeErrorText.ZXE_Name, errorMessageIDE);
			var errorMessageVQI = @"a 5 digit number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListVQI.PK, attributeFOOUSAOCCodeErrorText.ZXE_Name, errorMessageVQI);
			var errorMessageSID = @"an 11 digit number format 'CCYYMMDDNNN'";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListSID.PK, attributeFOOUSAOCCodeErrorText.ZXE_Name, errorMessageSID);
			var errorMessageFCE = @"a 5 digit number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListFCE.PK, attributeFOOUSAOCCodeErrorText.ZXE_Name, errorMessageFCE);
			var errorMessageFSX = @"Only the Affirmation of Compliance Code should be entered";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListFSX.PK, attributeFOOUSAOCCodeErrorText.ZXE_Name, errorMessageFSX);
			var errorMessageRNE = @"Only the Affirmation of Compliance Code should be entered";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListRNE.PK, attributeFOOUSAOCCodeErrorText.ZXE_Name, errorMessageRNE);
			var errorMessageFSR = @"a 9 digit number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListFSR.PK, attributeDRUUSAOCCodeErrorText.ZXE_Name, errorMessageFSR);
			var errorMessagePM = @"one of: D, H, K or P followed by 6 digits OR N followed by 4 to 6 digits OR DEN followed by 6 digits";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPM.PK, attributeDRUUSAOCCodeErrorText.ZXE_Name, errorMessagePM);
			var errorMessagePRN = @"a 6 to 10 digit number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPRN.PK, attributeDRUUSAOCCodeErrorText.ZXE_Name, errorMessagePRN);
			var errorMessageREG = @"a 9 digit number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListREG.PK, attributeDRUUSAOCCodeErrorText.ZXE_Name, errorMessageREG);
			var errorMessageRA3 = @"Only the Affirmation of Compliance Code should be entered";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListRA3.PK, attributeRADUSAOCCodeErrorText.ZXE_Name, errorMessageRA3);
			var errorMessageRA4 = @"Only the Affirmation of Compliance Code should be entered";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListRA4.PK, attributeRADUSAOCCodeErrorText.ZXE_Name, errorMessageRA4);
			var errorMessageRB1 = @"Only the Affirmation of Compliance Code should be entered";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListRB1.PK, attributeRADUSAOCCodeErrorText.ZXE_Name, errorMessageRB1);
			var errorMessageACC = @"NNXNNNN or NNXNNNN-NNN, where N is a number and X is a character.";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListACC.PK, attributeRADUSAOCCodeErrorText.ZXE_Name, errorMessageACC);
			var errorMessageVNA = @"a 6 digit number";
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListVNA.PK, attributeVMEUSAOCCodeErrorText.ZXE_Name, errorMessageVNA);
			Factory.Save();
		}
		#endregion
	}
}
