using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USImportTTBLineAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestTTBValidateState()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "KNZ";
			refCountryStates1.RW_RN_NKCountryCode = "MX";
			refCountryStates1.RW_Description = "KNZTEST";
			Factory.Save();

			var header = TTBLine;
			header.US_IsReleaseUnderBond = true;
			var propertyInfo = TTBLine.US_OA_ConsigneeAddressInfo;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			var address = orgHeader.Addresses.AddNew(OrgAddressType.Delivery, true);
			address.OA_State = "XXXX";
			address.OA_RL_NKRelatedPortCode = "USABC";
			propertyInfo.Value = address.PK;
			header.Validation.ValidateAll();
			AssertHasMessageErrorContaining(propertyInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			propertyInfo.Value = address.PK;
			header.Validation.ValidateAll();
			AssertNoMessageErrorContaining(propertyInfo, "The state is not a valid");

			address.OA_State = "KNZTEST";
			address.OA_RL_NKRelatedPortCode = "MXTST";
			propertyInfo.Value = address.PK;
			header.Validation.ValidateAll();
			AssertNoMessageErrorContaining(propertyInfo, "The state is not a valid");
		}

		public void TestCheckUS_ProgramCode()
		{
			TTBLine.US_ProgramCode = ZString.Empty;
			AssertHasMessageErrorContaining(TTBLine.US_ProgramCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(TTBLine.US_ProgramCodeInfo, ListValidation.InvalidCodeMessageError);

			TTBLine.US_ProgramCode = "@#$";
			AssertNoMessageErrorContaining(TTBLine.US_ProgramCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(TTBLine.US_ProgramCodeInfo, ListValidation.InvalidCodeMessageError);

			foreach (ICodeDescription pair in new TTBProgramCodeList())
			{
				TTBLine.US_ProgramCode = pair.Code;
				AssertNoMessageErrorContaining(TTBLine.US_ProgramCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(TTBLine.US_ProgramCodeInfo, ListValidation.InvalidCodeMessageError);
			}

			Declaration.ValidationModes = ValidationModes.None;
			TTBLine.US_ProgramCode = ZString.Empty;
			AssertNoMessageErrorContaining(TTBLine.US_ProgramCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(TTBLine.US_ProgramCodeInfo, ListValidation.InvalidCodeMessageError);

			TTBLine.US_ProgramCode = "@#$";
			AssertNoMessageErrorContaining(TTBLine.US_ProgramCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(TTBLine.US_ProgramCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_ProcessingCode()
		{
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			TTBLine.US_ProcessingCode = ZString.Empty;
			AssertHasMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);

			TTBLine.US_ProcessingCode = "@#$";
			AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);

			foreach (ICodeDescription pair in new TTBWINProcessingCodeList())
			{
				TTBLine.US_ProcessingCode = pair.Code;
				AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			}

			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Beverage;
			foreach (ICodeDescription pair in new TTBBERProcessingCodeList())
			{
				TTBLine.US_ProcessingCode = pair.Code;
				AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			}

			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			foreach (ICodeDescription pair in new TTBTOBProcessingCodeList())
			{
				TTBLine.US_ProcessingCode = pair.Code;
				AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			}

			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.DistilledSpirits;
			foreach (ICodeDescription pair in new TTBDSPProcessingCodeList())
			{
				TTBLine.US_ProcessingCode = pair.Code;
				AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
			}

			Declaration.ValidationModes = ValidationModes.None;
			TTBLine.US_ProcessingCode = ZString.Empty;
			AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);

			TTBLine.US_ProcessingCode = "@#$";
			AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(TTBLine.US_ProcessingCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_NumberForIRC()
		{
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			TTBLine.US_IsReleaseUnderBond = ZBool.False;
			AssertNoWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertNoMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
			TTBLine.US_IsReleaseUnderBond = ZBool.True;
			AssertNoWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertHasMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
			TTBLine.US_NumberForIRC = "234";
			AssertNoWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertNoMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
			TTBLine.US_NumberForIRC = "";
			AssertNoWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertHasMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
			Declaration.ValidationModes = ValidationModes.None;
			TTBLine.AddInfoValidation.ValidateUS_NumberForIRC();
			AssertNoWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertNoMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
			Declaration.RecalculateValidationModesOnDeclaration();
			TTBLine.AddInfoValidation.ValidateUS_NumberForIRC();
			AssertNoWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertHasMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);

			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			AssertHasWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertNoMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
			TTBLine.US_IsReleaseUnderBond = ZBool.False;
			AssertNoWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertNoMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
			TTBLine.US_IsReleaseUnderBond = ZBool.True;
			AssertHasWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertNoMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
			TTBLine.US_NumberForIRC = "234";
			AssertNoWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertNoMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
			TTBLine.US_NumberForIRC = "";
			AssertHasWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertNoMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
			Declaration.ValidationModes = ValidationModes.None;

			TTBLine.AddInfoValidation.ValidateUS_NumberForIRC();
			AssertNoWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertNoMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
			Declaration.RecalculateValidationModesOnDeclaration();

			TTBLine.AddInfoValidation.ValidateUS_NumberForIRC();
			AssertHasWarning(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.IRCRegistryNumberIsRequired);
			AssertNoMessageError(TTBLine.US_NumberForIRCInfo, ValidationConstants.TTB.TTBIssuedPermitNumberIsRequired);
		}

		public void TestCheckUS_TTINumberForIRC()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Test Org";
			org.MainAddress.OA_Address1 = "MAIN ADD 1";
			ZString numberIRC = "DSP-CA-90210";
			org.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.TTIRegistrationNumber, numberIRC, Core.Constants.CountryCodes.UnitedStates);

			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			TTBLine.US_IsReleaseUnderBond = ZBool.True;
			TTBLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T30;
			TTBLine.US_OA_ConsigneeAddress = org.MainAddress.PK;
			AssertNoMessageError(TTBLine.US_NumberForIRCInfo, TTIRegistrationNumberValidator.TTIRegistrationNumberCorrectFormat);

			TTBLine.US_NumberForIRC = "AAA-BB";
			TTBLine.AddInfoValidation.ValidateUS_NumberForIRC();
			AssertHasMessageError(TTBLine.US_NumberForIRCInfo, TTIRegistrationNumberValidator.TTIRegistrationNumberCorrectFormat);

			TTBLine.US_IsReleaseUnderBond = ZBool.False;
			TTBLine.AddInfoValidation.ValidateUS_NumberForIRC();
			AssertNoMessageError(TTBLine.US_NumberForIRCInfo, TTIRegistrationNumberValidator.TTIRegistrationNumberCorrectFormat);
		}

		public void TestCheckUS_OA_ConsigneeAddress()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC123KDF";
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			TTBLine.US_IsReleaseUnderBond = ZBool.False;
			TTBLine.US_OA_ConsigneeAddress = org.MainAddress.PK;
			var tobaccoMessage = ValidationConstants.TTB.EINIsRequired("receiving manufacturer or export warehouse proprietor", "ABC123KDF");
			var nonTobaccoMessage = ValidationConstants.TTB.EINIsRequired("receiving brewery, bonded wine cellar, or DSP", "ABC123KDF");
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			TTBLine.US_IsReleaseUnderBond = ZBool.True;
			AssertHasMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			Declaration.ValidationModes = ValidationModes.None;
			TTBLine.AddInfoValidation.ValidateUS_OA_ConsigneeAddress();
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			Declaration.RecalculateValidationModesOnDeclaration();
			TTBLine.AddInfoValidation.ValidateUS_OA_ConsigneeAddress();
			AssertHasMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			var cusCode = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "2423", Core.Constants.CountryCodes.UnitedStates);
			TTBLine.AddInfoValidation.ValidateUS_OA_ConsigneeAddress();
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			TTBLine.AddInfoValidation.ValidateUS_OA_ConsigneeAddress();
			AssertHasMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			TTBLine.US_OA_ConsigneeAddress = ZGuid.Empty;
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			TTBLine.US_OA_ConsigneeAddress = org.MainAddress.PK;
			AssertHasMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);

			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertHasMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			TTBLine.US_IsReleaseUnderBond = ZBool.False;
			TTBLine.US_OA_ConsigneeAddress = org.MainAddress.PK;
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			TTBLine.US_IsReleaseUnderBond = ZBool.True;
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertHasMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			Declaration.ValidationModes = ValidationModes.None;
			TTBLine.AddInfoValidation.ValidateUS_OA_ConsigneeAddress();
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			Declaration.RecalculateValidationModesOnDeclaration();
			TTBLine.AddInfoValidation.ValidateUS_OA_ConsigneeAddress();
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertHasMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			TTBLine.AddInfoValidation.ValidateUS_OA_ConsigneeAddress();
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			TTBLine.AddInfoValidation.ValidateUS_OA_ConsigneeAddress();
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertHasMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			TTBLine.US_OA_ConsigneeAddress = ZGuid.Empty;
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
			TTBLine.US_OA_ConsigneeAddress = org.MainAddress.PK;
			AssertNoMessageError(TTBLine.US_OA_ConsigneeAddressInfo, tobaccoMessage);
			AssertHasMessageError(TTBLine.US_OA_ConsigneeAddressInfo, nonTobaccoMessage);
		}

		public void TestCheckUS_PermitNumber()
		{
			var rule = Factory.LoadFromNaturalKey<USCRule>(USCRuleSchema.U0_Code, TariffRuleList.Codes.PermitNumberTTBProgramRequirement);
			if (rule == null)
			{
				rule = Factory.New<USCRule>();
				rule.U0_Code = TariffRuleList.Codes.PermitNumberTTBProgramRequirement;
			}

			var tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.PermitNumberTTBProgramRequirement;
			tariffRule.U1_Tariff = "1010202030";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_DateTo = ZDateTime.Today.AddMonths(3);
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010202030";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			TTBLine.US_PermitNumber = ZString.Empty;
			AssertHasMessageError(TTBLine.US_PermitNumberInfo, ValidationConstants.TTB.ImporterPermitNumberIsRequiredForThisTTBEntry);
			Declaration.ValidationModes = ValidationModes.None;
			TTBLine.AddInfoValidation.ValidateUS_PermitNumber();
			AssertNoMessageErrors(TTBLine.US_PermitNumberInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			TTBLine.AddInfoValidation.ValidateUS_PermitNumber();
			AssertHasMessageError(TTBLine.US_PermitNumberInfo, ValidationConstants.TTB.ImporterPermitNumberIsRequiredForThisTTBEntry);
			TTBLine.US_PermitNumber = ZString.Empty;
			AssertHasMessageError(TTBLine.US_PermitNumberInfo, ValidationConstants.TTB.ImporterPermitNumberIsRequiredForThisTTBEntry);
			tariffRule.U1_DateTo = ZDateTime.Today.AddMonths(-3);
			TTBLine.AddInfoValidation.ValidateUS_PermitNumber();
			AssertNoMessageErrors(TTBLine.US_PermitNumberInfo);
			tariffRule.U1_DateTo = ZDateTime.Today.AddMonths(3);
			TTBLine.AddInfoValidation.ValidateUS_PermitNumber();
			AssertHasMessageError(TTBLine.US_PermitNumberInfo, ValidationConstants.TTB.ImporterPermitNumberIsRequiredForThisTTBEntry);
			invoiceLine.JI_Tariff = "1234554321";
			TTBLine.AddInfoValidation.ValidateUS_PermitNumber();
			AssertNoMessageErrors(TTBLine.US_PermitNumberInfo);
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			TTBLine.US_PermitExemptionCode = ZString.Empty;
			TTBLine.US_PermitNumber = ZString.Empty;
			AssertHasMessageError(TTBLine.US_PermitNumberInfo, ValidationConstants.TTB.ImporterPermitNumberIsRequiredForThisTTBEntry);
			TTBLine.US_PermitExemptionCode = TTBExemptionCodeList.Codes.TTBEX3;
			TTBLine.US_PermitNumber = ZString.Empty;
			AssertNoMessageError(TTBLine.US_PermitNumberInfo, ValidationConstants.TTB.ImporterPermitNumberIsNotRequiredWhenExemptionEntered);
			AssertNoMessageError(TTBLine.US_PermitNumberInfo, ValidationConstants.TTB.ImporterPermitNumberIsRequiredForThisTTBEntry);
			TTBLine.US_PermitExemptionCode = TTBExemptionCodeList.Codes.TTBEX3;
			TTBLine.US_PermitNumber = "AA-AA-1";
			AssertHasMessageError(TTBLine.US_PermitNumberInfo, ValidationConstants.TTB.ImporterPermitNumberIsNotRequiredWhenExemptionEntered);
		}

		public void TestValidateCOLANumberIsEnteredWhenRequiredByTariffRule()
		{
			var rule = Factory.LoadFromNaturalKey<USCRule>(USCRuleSchema.U0_Code, TariffRuleList.Codes.COLANumberTTBProgramRequirement);
			if (rule == null)
			{
				rule = Factory.New<USCRule>();
				rule.U0_Code = TariffRuleList.Codes.COLANumberTTBProgramRequirement;
			}

			var tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.COLANumberTTBProgramRequirement;
			tariffRule.U1_Tariff = "1010202030";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_DateTo = ZDateTime.Today.AddMonths(3);
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010202030";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageErrors(TTBLine);
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			TTBLine.AddInfoValidation.ValidateAll();
			AssertHasRowMessageError(TTBLine.Data, ValidationConstants.TTB.ACOLANumberIsRequiredForThisTTBEntry);
			Declaration.ValidationModes = ValidationModes.None;
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageErrors(TTBLine.Data);
			Declaration.RecalculateValidationModesOnDeclaration();
			TTBLine.AddInfoValidation.ValidateAll();
			AssertHasRowMessageError(TTBLine.Data, ValidationConstants.TTB.ACOLANumberIsRequiredForThisTTBEntry);
			var cola1 = TTBLine.COLAAndCertificates.AddNew();
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageErrors("No row error as it should be on the field level", TTBLine.Data);
			var cola2 = TTBLine.COLAAndCertificates.AddNew();
			TTBLine.AddInfoValidation.ValidateAll();
			AssertHasRowMessageError("no cola entered in any ros", TTBLine.Data, ValidationConstants.TTB.ACOLANumberIsRequiredForThisTTBEntry);
			cola1.US_COLA = "C";
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageErrors("COLA is entered on one of the rows", TTBLine.Data);
			cola1.US_COLA = ZString.Empty;
			cola1.US_COLAExemptionCode = "C";
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageErrors("COLA exemption is entered on one of the rows", TTBLine.Data);
			cola1.US_COLAExemptionCode = ZString.Empty;
			cola1.US_ForeignCertificateCountry = "C";
			TTBLine.AddInfoValidation.ValidateAll();
			AssertHasRowMessageError("no cola entered in any rows", TTBLine.Data, ValidationConstants.TTB.ACOLANumberIsRequiredForThisTTBEntry);
			invoiceLine.JI_Tariff = "1234554321";
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageErrors("No tariff", TTBLine.Data);
		}

		public void TestValidateForeignCertificateIsEnteredWhenRequiredByTariffRule()
		{
			var rule = Factory.LoadFromNaturalKey<USCRule>(USCRuleSchema.U0_Code, TariffRuleList.Codes.ForeignCertificateTTBProgramRequirement);
			if (rule == null)
			{
				rule = Factory.New<USCRule>();
				rule.U0_Code = TariffRuleList.Codes.ForeignCertificateTTBProgramRequirement;
			}

			var tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.ForeignCertificateTTBProgramRequirement;
			tariffRule.U1_Tariff = "1010202030";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_DateTo = ZDateTime.Today.AddMonths(3);
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010202030";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowWarnings(TTBLine.Data);
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			TTBLine.AddInfoValidation.ValidateAll();
			AssertHasRowWarning(TTBLine.Data, ValidationConstants.TTB.AForeignCertificateMaybeRequiredForThisTTBEntry);
			Declaration.ValidationModes = ValidationModes.None;
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowWarnings(TTBLine.Data);
			Declaration.RecalculateValidationModesOnDeclaration();
			TTBLine.AddInfoValidation.ValidateAll();
			AssertHasRowWarning(TTBLine.Data, ValidationConstants.TTB.AForeignCertificateMaybeRequiredForThisTTBEntry);
			var cola1 = TTBLine.COLAAndCertificates.AddNew();
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowWarnings("No row error as it should be on the field level", TTBLine.Data);
			var cola2 = TTBLine.COLAAndCertificates.AddNew();
			TTBLine.AddInfoValidation.ValidateAll();
			AssertHasRowWarning("no certificate entered in any ros", TTBLine.Data, ValidationConstants.TTB.AForeignCertificateMaybeRequiredForThisTTBEntry);
			cola1.US_ForeignCertificateCountry = "C";
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowWarnings("certificate is entered on one of the rows", TTBLine.Data);
			cola1.US_ForeignCertificateCountry = ZString.Empty;
			cola1.US_COLA = "C";
			TTBLine.AddInfoValidation.ValidateAll();
			AssertHasRowWarning("no certificate entered in any rows", TTBLine.Data, ValidationConstants.TTB.AForeignCertificateMaybeRequiredForThisTTBEntry);
			invoiceLine.JI_Tariff = "1234554321";
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowWarnings("No tariff", TTBLine.Data);
		}

		public void TestCheckUS_QuantityInPCS()
		{
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			TTBLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T51;
			TTBLine.US_QuantityInPCS = ZDecimal.Zero;
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Quantity in PCS");
			AssertHasMessageError(TTBLine.US_QuantityInPCSInfo, messageError);
			Declaration.ValidationModes = ValidationModes.None;
			TTBLine.AddInfoValidation.ValidateUS_QuantityInPCS();
			AssertNoMessageErrors(TTBLine.US_QuantityInPCSInfo);
			Declaration.RecalculateValidationModesOnDeclaration();
			TTBLine.AddInfoValidation.ValidateUS_QuantityInPCS();
			AssertHasMessageError(TTBLine.US_QuantityInPCSInfo, messageError);
			TTBLine.US_QuantityInPCS = 10m;
			AssertNoMessageErrors(TTBLine.US_QuantityInPCSInfo);
			TTBLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T30;
			AssertNoMessageErrors(TTBLine.US_QuantityInPCSInfo);
			TTBLine.US_QuantityInPCS = ZDecimal.Zero;
			AssertNoMessageErrors(TTBLine.US_QuantityInPCSInfo);
			TTBLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T52;
			AssertHasMessageError(TTBLine.US_QuantityInPCSInfo, messageError);
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			AssertNoMessageErrors(TTBLine.US_QuantityInPCSInfo);
		}

		public void TestCheckUS_PermitExemptionCode()
		{
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			TTBLine.US_PermitNumber = "23";
			AssertNoMessageError(TTBLine.US_PermitExemptionCodeInfo, ValidationConstants.TTB.EitherPermitNumberOrExemptionCodeIsRequiredButNotBoth);
			AssertNoMessageErrorContaining(TTBLine.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			TTBLine.US_PermitExemptionCode = TTBExemptionCodeList.Codes.TTBEX3;
			AssertHasMessageError(TTBLine.US_PermitExemptionCodeInfo, ValidationConstants.TTB.EitherPermitNumberOrExemptionCodeIsRequiredButNotBoth);
			AssertNoMessageErrorContaining(TTBLine.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			TTBLine.US_PermitNumber = "";
			AssertNoMessageError(TTBLine.US_PermitExemptionCodeInfo, ValidationConstants.TTB.EitherPermitNumberOrExemptionCodeIsRequiredButNotBoth);
			AssertNoMessageErrorContaining(TTBLine.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Beverage;
			AssertNoMessageError(TTBLine.US_PermitExemptionCodeInfo, ValidationConstants.TTB.EitherPermitNumberOrExemptionCodeIsRequiredButNotBoth);
			AssertHasMessageErrorContaining(TTBLine.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			Declaration.ValidationModes = ValidationModes.None;
			TTBLine.AddInfoValidation.ValidateUS_PermitExemptionCode();
			AssertNoMessageError(TTBLine.US_PermitExemptionCodeInfo, ValidationConstants.TTB.EitherPermitNumberOrExemptionCodeIsRequiredButNotBoth);
			AssertNoMessageErrorContaining(TTBLine.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestValidateCigarIsEnteredWhenRequiredByTariffRule()
		{
			var rule = Factory.LoadFromNaturalKey<USCRule>(USCRuleSchema.U0_Code, TariffRuleList.Codes.TTBPriceRequirement);
			if (rule == null)
			{
				rule = Factory.New<USCRule>();
				rule.U0_Code = TariffRuleList.Codes.TTBPriceRequirement;
			}

			var tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.TTBPriceRequirement;
			tariffRule.U1_Tariff = "2402103070";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule.U1_DateTo = ZDateTime.Today.AddMonths(3);
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2402103070";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			InvoiceLine.JI_Tariff = tariff.UE_Tariff;
			TTBLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			TTBLine.AddInfoValidation.ValidateAll();
			AssertHasRowMessageError(TTBLine.Data, ValidationConstants.TTB.CigarIsRequiredForThisTTBEntry);
			Declaration.ValidationModes = ValidationModes.None;
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageError(TTBLine.Data, ValidationConstants.TTB.CigarIsRequiredForThisTTBEntry);
			Declaration.RecalculateValidationModesOnDeclaration();
			TTBLine.AddInfoValidation.ValidateAll();
			AssertHasRowMessageError(TTBLine.Data, ValidationConstants.TTB.CigarIsRequiredForThisTTBEntry);
			TTBLine.Cigars.AddNew();
			TTBLine.AddInfoValidation.ValidateAll();
			AssertNoRowMessageError(TTBLine.Data, ValidationConstants.TTB.CigarIsRequiredForThisTTBEntry);
		}

		#region Implementation

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
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
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

		TTBLine TTBLine
		{
			get { return ttbLine ?? (ttbLine = InvoiceLine.TTBLines.AddNew()); }
		}
		TTBLine ttbLine;

		#endregion
	}
}
