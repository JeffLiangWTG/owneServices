using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USAMSAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_OA_CertifyingBody()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.OR1;
			amsLine.US_OA_CertifyingBody = ZGuid.Empty;
			AssertHasMessageError(amsLine.US_OA_CertifyingBodyInfo, "You have not entered a value.");

			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			amsLine.US_OA_CertifyingBody = address.PK;
			AssertNoMessageError(amsLine.US_OA_CertifyingBodyInfo, "You have not entered a value.");
			AssertHasMessageError(amsLine.US_OA_CertifyingBodyInfo, "Organization must have Registration Code of type AMS on file.");

			var cusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "1111", Core.Constants.CountryCodes.UnitedStates);
			amsLine.AddInfoValidation.ValidateUS_OA_CertifyingBody();
			AssertNoMessageError(amsLine.US_OA_CertifyingBodyInfo, "Organization must have Registration Code of type AMS on file.");
			AssertHasMessageError(amsLine.US_OA_CertifyingBodyInfo, "AMS code must be 3 digits.");

			cusCode.OK_CustomsRegNo = "123";
			amsLine.AddInfoValidation.ValidateUS_OA_CertifyingBody();
			AssertNoMessageError(amsLine.US_OA_CertifyingBodyInfo, "AMS code must be 3 digits.");
			AssertHasMessageError(amsLine.US_OA_CertifyingBodyInfo, "Must have valid PGA contact info on file.");

			var contact = org.Contacts.AddNew();
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = "USP";
			contact.OC_ContactName = "Joey";
			amsLine.AddInfoValidation.ValidateUS_OA_CertifyingBody();
			AssertHasMessageError(amsLine.US_OA_CertifyingBodyInfo, "PGA contact is missing phone number or email address.");

			contact.OC_Phone = "13222222222";
			amsLine.AddInfoValidation.ValidateUS_OA_CertifyingBody();
			AssertHasMessageError(amsLine.US_OA_CertifyingBodyInfo, "PGA contact is missing phone number or email address.");

			contact.OC_Phone = "";
			contact.OC_Email = "test@wisetechglobal.com";
			amsLine.AddInfoValidation.ValidateUS_OA_CertifyingBody();
			AssertHasMessageError(amsLine.US_OA_CertifyingBodyInfo, "PGA contact is missing phone number or email address.");

			contact.OC_Phone = "13222222222";
			amsLine.AddInfoValidation.ValidateUS_OA_CertifyingBody();
			AssertNoMessageError(amsLine.US_OA_CertifyingBodyInfo, "PGA contact is missing phone number or email address.");
			AssertNoMessageError(amsLine.US_OA_CertifyingBodyInfo, "Must have valid PGA contact info on file.");
		}

		public void TestCheckUS_OA_Recipient()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.OR1;
			amsLine.US_OA_Recipient = ZGuid.Empty;
			AssertHasMessageError(amsLine.US_OA_RecipientInfo, "You have not entered a value.");

			var org = Factory.New<OrgHeader>();
			var address = org.MainAddress;
			amsLine.US_OA_Recipient = address.PK;
			AssertNoMessageError(amsLine.US_OA_RecipientInfo, "You have not entered a value.");
			AssertHasMessageError(amsLine.US_OA_RecipientInfo, "Organization must have Registration Code of type AMS on file.");

			var cusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.AMSRegistrationNumber, "111", Core.Constants.CountryCodes.UnitedStates);
			amsLine.AddInfoValidation.ValidateUS_OA_Recipient();
			AssertNoMessageError(amsLine.US_OA_RecipientInfo, "Organization must have Registration Code of type AMS on file.");
			AssertHasMessageError(amsLine.US_OA_RecipientInfo, "AMS code must be 10 digits.");

			cusCode.OK_CustomsRegNo = "1234567891";
			amsLine.AddInfoValidation.ValidateUS_OA_Recipient();
			AssertNoMessageError(amsLine.US_OA_RecipientInfo, "AMS code must be 10 digits.");
			AssertHasMessageError(amsLine.US_OA_RecipientInfo, "Must have valid PGA contact info on file.");

			var contact = org.Contacts.AddNew();
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = "USP";
			contact.OC_ContactName = "Joey";
			contact.OC_Phone = "13222222222";
			contact.OC_Email = "test@wisetechglobal.com";
			amsLine.AddInfoValidation.ValidateUS_OA_Recipient();
			AssertNoMessageError(amsLine.US_OA_RecipientInfo, "Must have valid PGA contact info on file.");
		}

		public void TestUS_Program()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			invoiceLine.US_AMSInd = "D";

			amsLine.US_Program = AMSProgramList.Codes.MO1;
			AssertNoMessageErrorContaining(amsLine.US_ProgramInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_Program = "~";
			AssertHasMessageErrorContaining(amsLine.US_ProgramInfo, ListValidation.InvalidCodeMessageError);

			amsLine.US_Program = AMSProgramList.Codes.MO2;
			AssertNoMessageErrorContaining(amsLine.US_ProgramInfo, ListValidation.InvalidCodeMessageError);

			var amsLine2 = invoiceLine.AMSLines.AddNew();
			amsLine2.US_Program = AMSProgramList.Codes.MO2;
			AssertHasMessageError(amsLine2.US_ProgramInfo, USAMSAddInfoValidation.ProgramsRepeat);

			amsLine2.US_Program = AMSProgramList.Codes.MO4;
			AssertNoMessageError(amsLine2.US_ProgramInfo, USAMSAddInfoValidation.ProgramsRepeat);

			amsLine.US_Program = "";
			AssertHasMessageErrorContaining(amsLine.US_ProgramInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_Program = AMSProgramList.Codes.OR2;
			AssertHasMessageError(amsLine.US_ProgramInfo, USAMSAddInfoValidation.AtLeastOneCertificateLineRequiredForOR2);
			amsLine.AMSLines.AddNew();
			amsLine.AddInfoValidation.ValidateUS_Program();
			AssertNoMessageError(amsLine.US_ProgramInfo, USAMSAddInfoValidation.AtLeastOneCertificateLineRequiredForOR2);
		}

		public void TestUS_NetWeightAndUQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			invoiceLine.US_AMSInd = "D";

			amsLine.US_Program = AMSProgramList.Codes.MO8;
			amsLine.AddInfoValidation.ValidateUS_NetWeight();
			amsLine.AddInfoValidation.ValidateUS_NetWeightUQ();
			AssertHasMessageError(amsLine.US_NetWeightInfo, "You have not entered a value.");
			AssertHasMessageError(amsLine.US_NetWeightUQInfo, "You have not entered a value.");

			amsLine.US_NetWeight = 12m;
			amsLine.US_NetWeightUQ = "KG";
			amsLine.AddInfoValidation.ValidateUS_NetWeight();
			amsLine.AddInfoValidation.ValidateUS_NetWeightUQ();
			AssertNoMessageError(amsLine.US_NetWeightInfo, "You have not entered a value.");
			AssertNoMessageError(amsLine.US_NetWeightUQInfo, "You have not entered a value.");
		}

		public void TestUS_NetWeightUQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();
			invoiceLine.US_AMSInd = "D";

			amsLine.US_Program = AMSProgramList.Codes.OR1;
			amsLine.US_NetWeightUQ = "~";
			amsLine.AddInfoValidation.ValidateUS_NetWeightUQ();
			AssertHasMessageErrorContaining(amsLine.US_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);

			amsLine.US_NetWeightUQ = "KG";
			amsLine.AddInfoValidation.ValidateUS_NetWeightUQ();
			AssertNoMessageErrorContaining(amsLine.US_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_IntendedUseDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;

			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO3;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._010000;
			AssertNoMessageErrorContaining(amsLine.US_IntendedUseDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._980000;
			AssertHasMessageErrorContaining(amsLine.US_IntendedUseDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_IntendedUseCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._010000;
			AssertNoMessageErrorContaining(amsLine.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_IntendedUseCode = "~";
			AssertHasMessageErrorContaining(amsLine.US_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);

			amsLine.US_IntendedUseCode = AMSIntendedUseCodesList.Codes._230000;
			AssertNoMessageErrorContaining(amsLine.US_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);

			amsLine.US_IntendedUseCode = "";
			AssertHasMessageErrorContaining(amsLine.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_Program = AMSProgramList.Codes.MO7;
			amsLine.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(amsLine.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_Program = AMSProgramList.Codes.MO8;
			amsLine.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(amsLine.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_Program = AMSProgramList.Codes.OR1;
			amsLine.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(amsLine.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_Program = AMSProgramList.Codes.OR2;
			amsLine.AddInfoValidation.ValidateUS_IntendedUseCode();
			AssertNoMessageErrorContaining(amsLine.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_CommercialDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO1;
			amsLine.AddInfoValidation.ValidateUS_CommercialDescription();
			AssertHasMessageErrorContaining(amsLine.US_CommercialDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_CommercialDescription = "Test";
			AssertNoMessageErrorContaining(amsLine.US_CommercialDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_Program = AMSProgramList.Codes.MO7;
			amsLine.US_CommercialDescription = ZString.Empty;
			amsLine.AddInfoValidation.ValidateUS_CommercialDescription();
			AssertNoMessageErrorContaining(amsLine.US_CommercialDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_Program = AMSProgramList.Codes.MO8;
			amsLine.US_CommercialDescription = ZString.Empty;
			amsLine.AddInfoValidation.ValidateUS_CommercialDescription();
			AssertNoMessageErrorContaining(amsLine.US_CommercialDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			amsLine.US_Program = AMSProgramList.Codes.OR2;
			amsLine.AddInfoValidation.ValidateUS_CommercialDescription();
			AssertNoMessageErrorContaining(amsLine.US_CommercialDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestTariffRuleCodeForMO5()
		{
			TestTariffRuleCodeCore(AMSProgramList.Codes.MO5, TariffRuleList.Codes.HTSExemptFromAMSMO5ProgramRequirement);
		}

		public void TestTariffRuleCodeForEG1()
		{
			TestTariffRuleCodeCore(AMSProgramList.Codes.EG1, TariffRuleList.Codes.HTSExemptFromAMSEG1ProgramRequirement);
		}

		public void TestTariffRuleCodeForEG2()
		{
			TestTariffRuleCodeCore(AMSProgramList.Codes.EG2, TariffRuleList.Codes.HTSExemptFromAMSEG2ProgramRequirement);
		}

		public void TestTariffRuleCodeForPN1()
		{
			TestTariffRuleCodeCore(AMSProgramList.Codes.PN1, TariffRuleList.Codes.HTSExemptFromAMSPN1ProgramRequirement);
		}

		public void TestTariffRuleCodeCore(ZString amsProgramCode, ZString tariffRuleCode)
		{
			SetupTariffRules(tariffRuleCode);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2016, 7, 25);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0802511";
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = amsProgramCode;
			AssertHasMessageErrorContaining(amsLine.US_ProgramInfo, USAMSAddInfoValidation.TariffCodeNotForProgram);

			invoiceLine.JI_Tariff = "0802512";
			invoiceLine.AMSLines.RemoveAndDeleteAll();
			amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = amsProgramCode;
			AssertNoMessageErrorContaining(amsLine.US_ProgramInfo, USAMSAddInfoValidation.TariffCodeNotForProgram);
		}

		void SetupTariffRules(ZString tariffRuleCode)
		{
			var tariff1 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0802511")).FirstOrDefault();
			if (tariff1 == null)
			{
				tariff1 = Factory.New<USCTariff>();
				tariff1.UE_Tariff = "0802511";
			}
			tariff1.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff1.UE_DateTo = new ZDateTime(2099, 1, 1);
			tariff1.TariffRules.RemoveAllFromRelationship();

			var tariffRule1 = Factory.Load<USCTariffRule>(new ZQuery(USCTariffRuleSchema.U1_Tariff, "0802511")).FirstOrDefault();
			if (tariffRule1 == null)
			{
				tariffRule1 = tariff1.TariffRules.AddNew();
				tariffRule1.U1_Tariff = "0802511";
			}
			tariffRule1.U1_RuleCode = tariffRuleCode;
			tariffRule1.U1_DateFrom = new ZDateTime(2000, 1, 1);
			tariffRule1.U1_DateTo = new ZDateTime(2016, 1, 1);

			var tariff2 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0802512")).FirstOrDefault();
			if (tariff2 == null)
			{
				tariff2 = Factory.New<USCTariff>();
				tariff2.UE_Tariff = "0802512";
			}
			tariff2.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff2.UE_DateTo = new ZDateTime(2099, 1, 1);
			tariff2.TariffRules.RemoveAllFromRelationship();

			var tariffRule2 = Factory.Load<USCTariffRule>(new ZQuery(USCTariffRuleSchema.U1_Tariff, "0802512")).FirstOrDefault();
			if (tariffRule2 == null)
			{
				tariffRule2 = tariff2.TariffRules.AddNew();
				tariffRule2.U1_Tariff = "0802512";
			}
			tariffRule2.U1_RuleCode = tariffRuleCode;
			tariffRule2.U1_DateFrom = new ZDateTime(2000, 1, 1);
			tariffRule2.U1_DateTo = new ZDateTime(2099, 12, 31);
		}

		public void TestEnsureThatAtLeastOneContainerSelectedForAMS()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var amsLine = invoiceLine.AMSLines.AddNew();

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX1234562";

			var npContainer = invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainer(container);
			npContainer.IsForInvoiceLine = false;

			amsLine.US_Program = AMSProgramList.Codes.MO7;

			amsLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageError(amsLine, USAMSAddInfoValidation.AtLeastOneContainerRequiredForAMS);

			foreach (string programCode in new string[] { AMSProgramList.Codes.MO1, AMSProgramList.Codes.MO5, AMSProgramList.Codes.EG1, AMSProgramList.Codes.PN1 })
			{
				amsLine.US_Program = programCode;

				amsLine.AddInfoValidation.ValidateAll();
				AssertHasRowMessageError(amsLine, USAMSAddInfoValidation.AtLeastOneContainerRequiredForAMS);
			}

			npContainer.IsForInvoiceLine = true;

			foreach (string programCode in new string[] { AMSProgramList.Codes.MO1, AMSProgramList.Codes.MO5, AMSProgramList.Codes.EG1, AMSProgramList.Codes.PN1 })
			{
				amsLine.US_Program = programCode;

				amsLine.AddInfoValidation.ValidateAll();
				AssertNoRowMessageError(amsLine, USAMSAddInfoValidation.AtLeastOneContainerRequiredForAMS);
			}
		}

		public void TestTestTariffRegulatedPeriodForMO1MO6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EstimatedEntryDate = new ZDateTime(2018, 06, 28);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0805100040";
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = AMSProgramList.Codes.MO1;
			amsHeader.AMSLines.AddNew();
			amsHeader.AddInfoValidation.ValidateUS_Program();
			AssertNoMessageErrors(amsHeader.US_ProgramInfo);

			declaration.US_EstimatedEntryDate = new ZDateTime(2018, 7, 1);
			amsHeader.AddInfoValidation.ValidateUS_Program();
			AssertHasMessageErrorContaining(amsHeader.US_ProgramInfo, "Program MO1 may not be used for tariff 0805100040 with entry date 01-Jul-18. This program may only be used during the regulated period of 1/Sep through 30/Jun.");

			declaration.US_EstimatedEntryDate = new ZDateTime(2018, 06, 28);
			invoiceLine.JI_Tariff = "0806104000";
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.AMSLines.RemoveAndDeleteAll();
			amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = AMSProgramList.Codes.MO1;
			amsHeader.AMSLines.AddNew();
			amsHeader.AddInfoValidation.ValidateUS_Program();
			AssertNoMessageErrors(amsHeader.US_ProgramInfo);

			declaration.US_EstimatedEntryDate = new ZDateTime(2018, 7, 11);
			amsHeader.AddInfoValidation.ValidateUS_Program();
			AssertHasMessageErrorContaining(amsHeader.US_ProgramInfo, "Program MO1 may not be used for tariff 0806104000 with entry date 11-Jul-18. This program may only be used during the regulated period of 10/Apr through 10/Jul.");
		}

		public void TestTestTariffRegulatedPeriodForMO7()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EstimatedEntryDate = new ZDateTime(2018, 06, 28);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0805100040";
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = AMSProgramList.Codes.MO7;
			amsHeader.AddInfoValidation.ValidateUS_Program();
			AssertHasMessageErrorContaining(amsHeader.US_ProgramInfo, "Program MO7 may not be used for tariff 0805100040 with entry date 28-Jun-18. This program may only be used outside the regulated period of 1/Sep through 30/Jun.");

			declaration.US_EstimatedEntryDate = new ZDateTime(2018, 7, 1);
			amsHeader.AddInfoValidation.ValidateUS_Program();
			AssertNoMessageErrors(amsHeader.US_ProgramInfo);

			declaration.US_EstimatedEntryDate = new ZDateTime(2018, 06, 28);
			invoiceLine.JI_Tariff = "0806104000";
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.AMSLines.RemoveAndDeleteAll();
			amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = AMSProgramList.Codes.MO7;
			amsHeader.AddInfoValidation.ValidateUS_Program();
			AssertHasMessageErrorContaining(amsHeader.US_ProgramInfo, "Program MO7 may not be used for tariff 0806104000 with entry date 28-Jun-18. This program may only be used outside the regulated period of 10/Apr through 10/Jul.");

			declaration.US_EstimatedEntryDate = new ZDateTime(2018, 7, 11);
			amsHeader.AddInfoValidation.ValidateUS_Program();
			AssertNoMessageErrors(amsHeader.US_ProgramInfo);
		}

		public void TestCheckUS_IsElecImageSubmitted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EstimatedEntryDate = new ZDateTime(2018, 06, 28);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0805100040";
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsHeader = invoiceLine.AMSLines.AddNew();
			amsHeader.US_Program = AMSProgramList.Codes.MO1;
			amsHeader.AMSLines.AddNew();
			amsHeader.AddInfoValidation.ValidateUS_IsElecImageSubmitted();
			AssertNoMessageErrors(amsHeader.US_IsElecImageSubmittedInfo);

			amsHeader.US_Program = AMSProgramList.Codes.OR1;
			amsHeader.AddInfoValidation.ValidateUS_IsElecImageSubmitted();
			AssertHasMessageError(amsHeader.US_IsElecImageSubmittedInfo, USAMSAddInfoValidation.ElecImageSubmittedShouldBeTicked);

			amsHeader.US_IsElecImageSubmitted = true;
			amsHeader.AddInfoValidation.ValidateUS_IsElecImageSubmitted();
			AssertNoMessageError(amsHeader.US_IsElecImageSubmittedInfo, USAMSAddInfoValidation.ElecImageSubmittedShouldBeTicked);

			amsHeader.US_Program = AMSProgramList.Codes.OR2;
			amsHeader.AddInfoValidation.ValidateUS_IsElecImageSubmitted();
			AssertNoMessageError(amsHeader.US_IsElecImageSubmittedInfo, USAMSAddInfoValidation.ElecImageSubmittedShouldBeTicked);
		}

		[TestDate(2021, 12, 01)]
		public void TestTariffEligbleForAMS_MO3()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0058580058";
			tariff.UE_DateFrom = new ZDateTime(2021, 01, 01);
			tariff.UE_DateTo = new ZDateTime(2022, 01, 01);
			Factory.Save();

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariffView = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, tariff.UE_Tariff, new ZDate(2021, 01, 01), new ZDate(2079, 01, 01));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, "MO3,MO4", tariffView);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO3;
			AssertNoMessageError(amsLine.US_ProgramInfo, USAMSAddInfoValidation.TariffCodeNotForProgram);

			amsLine.US_Program = AMSProgramList.Codes.MO5;
			AssertHasMessageError(amsLine.US_ProgramInfo, USAMSAddInfoValidation.TariffCodeNotForProgram);
		}

		[TestDate(2021, 12, 01)]
		public void TestTariffEligbleForAMS_OR1OR2()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "0058580058";
			tariff.UE_DateFrom = new ZDateTime(2021, 01, 01);
			tariff.UE_DateTo = new ZDateTime(2022, 01, 01);
			Factory.Save();

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();

			var tariffView = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, tariff.UE_Tariff, new ZDate(2021, 01, 01), new ZDate(2079, 01, 01));
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, "MO3,MO4", tariffView);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
			var amsLine = invoiceLine.AMSLines.AddNew();
			amsLine.US_Program = AMSProgramList.Codes.MO5;
			AssertHasMessageError(amsLine.US_ProgramInfo, USAMSAddInfoValidation.TariffCodeNotForProgram);

			amsLine.US_Program = AMSProgramList.Codes.OR1;
			AssertNoMessageError(amsLine.US_ProgramInfo, USAMSAddInfoValidation.TariffCodeNotForProgram);

			amsLine.US_Program = AMSProgramList.Codes.OR2;
			AssertNoMessageError(amsLine.US_ProgramInfo, USAMSAddInfoValidation.TariffCodeNotForProgram);
		}
	}
}
