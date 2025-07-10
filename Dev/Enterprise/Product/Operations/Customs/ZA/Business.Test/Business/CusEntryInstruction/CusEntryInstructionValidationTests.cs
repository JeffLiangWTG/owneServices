using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;
using ECB = Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryInstructionValidationTests : BusinessObjectValidationTestCase
	{
		public void TestCheckCEI_OH_BondHolder()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "CODCA", "CA");
			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "CODZA", "ZA");

			var declaration = SetupBondGuaranteeValueData();
			var entry = declaration.CustomsEntryHeaders[0];
			var addInfo1ForLine1 = entry.MergedLines[0].AdditionalInformationCodes[0];
			var entryInstruction = entry.EntryInstruction;
			CombineAssertions(() =>
			{
				AssertNoMessageErrors(entryInstruction.CEI_OH_BondHolderInfo);
				entryInstruction.CEI_OH_BondHolder = testOrg1.PK;
				AssertHasMessageErrorContaining(entryInstruction.CEI_OH_BondHolderInfo, "The selected organization does not have a Bond Holder Customs Carrier Code");
				entryInstruction.CEI_OH_BondHolder = testOrg2.PK;
				AssertHasMessageErrorContaining(entryInstruction.CEI_OH_BondHolderInfo, "The selected organization does not have a Bond Holder Customs Carrier Code");
				entryInstruction.CEI_OH_BondHolder = testOrg3.PK;
				AssertNoMessageErrors(entryInstruction.CEI_OH_BondHolderInfo);
			});

			using (ZACustomsRegistry.Instance.AllowAutomaticSplitEntriesByBondAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
				addInfo1ForLine1.CY_Data = "1000";
				entryInstruction.CEI_OH_BondHolder = testOrg1.PK;
				AssertHasMessageErrorContaining(entryInstruction.CEI_OH_BondHolderInfo, ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);

				addInfo1ForLine1.CY_Data = "0";
				entryInstruction.Validation.ValidateCEI_OH_BondHolder();
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_BondHolderInfo, ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);

				testOrg1.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "10000", "ZA");
				entryInstruction.Validation.ValidateCEI_OH_BondHolder();
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_BondHolderInfo, ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);
			}

			using (ZACustomsRegistry.Instance.AllowAutomaticSplitEntriesByBondAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				testOrg1.CustomsCodes.RemoveAndDeleteAll();
				addInfo1ForLine1.CY_Data = "1000";
				entryInstruction.CEI_OH_BondHolder = testOrg1.PK;
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_BondHolderInfo, ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);

				addInfo1ForLine1.CY_Data = "0";
				entryInstruction.Validation.ValidateCEI_OH_BondHolder();
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_BondHolderInfo, ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);

				testOrg1.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "10000", "ZA");
				entryInstruction.Validation.ValidateCEI_OH_BondHolder();
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_BondHolderInfo, ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);
			}
		}

		public void TestValidateUCRNumber()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "ZAAM";
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "DEC123";
			declaration.JE_RL_NKOrigin = unloco.RL_Code;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKFinalDestination = "BWAAA";
			var supplier = OrgHeader.New(Factory);
			var cusCode = supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "12345678", Core.Constants.CountryCodes.SouthAfrica);
			declaration.JE_OH_Supplier = supplier.PK;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_UCROverride = "1";
			entryInstruction.CEI_IsUCROverridden = true;
			AssertHasMessageErrorContaining(entryInstruction.CEI_UCROverrideInfo, ValidationConstants.EntryInstruction.InvalidUCRNumberOverride);
			entryInstruction.CEI_UCROverride = "FAA12345678COTH12345678901234S";
			AssertHasWarningContaining(entryInstruction.CEI_UCROverrideInfo, ValidationConstants.EntryInstruction.InvalidUCRYear);
			entryInstruction.CEI_UCROverride = "88A12345678COTH12345678901234S";
			AssertNoWarningContaining(entryInstruction.CEI_UCROverrideInfo, ValidationConstants.EntryInstruction.InvalidUCRYear);
			AssertHasWarningContaining(entryInstruction.CEI_UCROverrideInfo, ValidationConstants.EntryInstruction.InvalidUCRCountryOverride);
			entryInstruction.CEI_IsUCROverridden = false;
			entryInstruction.CEI_UCROverride = "88A12345678COTH12345678901234M";
			AssertHasWarningContaining(entryInstruction.CEI_UCROverrideInfo, ValidationConstants.EntryInstruction.InvalidUCRCountryOverride);
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.SouthAfrica;
			entryInstruction.CEI_UCROverride = "8ZA12345678COTH12345678901234S";
			AssertNoWarnings(entryInstruction.CEI_UCROverrideInfo);
			declaration.JE_RL_NKFinalDestination = "BWAAA";
			var ucrHelper = new UCRHelper();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			var cusEntryLine = Factory.New<CusEntryLine>();
			invoiceLine.JI_CL = cusEntryLine.PK;
			var cusEntryHeader = cusEntryLine.Header;
			cusEntryHeader.IsNull = false;
			entryInstruction.CEI_IsUCROverridden = false;
			entryInstruction.CEI_UCROverride = ZString.Empty;
			cusEntryHeader.MessageStatus = ZAMessageStatusList.Codes.Acknowledged;
			entryInstruction.Validation.ValidateCEI_UCROverride();
			AssertHasWarning(entryInstruction.CEI_UCROverrideInfo, ValidationConstants.EntryInstruction.UnableToCalculateUCR);
			cusEntryHeader.MessageStatus = ZAMessageStatusList.Codes.NotSent;
			entryInstruction.Validation.ValidateCEI_UCROverride();
			AssertNoWarning(entryInstruction.CEI_UCROverrideInfo, ValidationConstants.EntryInstruction.UnableToCalculateUCR);
			cusEntryHeader.MessageStatus = ZAMessageStatusList.Codes.AwaitingResponse;
			entryInstruction.Validation.ValidateCEI_UCROverride();
			AssertHasWarning(entryInstruction.CEI_UCROverrideInfo, ValidationConstants.EntryInstruction.UnableToCalculateUCR);
		}

		public void TestValidateUCRNumberDuplicates()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "ZAAM";
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_DeclarationReference = "JOB ONE";
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.LoadOrCreateUCRNumber("8ZA12345678COTH12345678901234S");
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_DeclarationReference = "JOB TWO";
			var entry2 = declaration1.CustomsEntryHeaders.AddNew();
			entry2.LoadOrCreateUCRNumber("88A12345678COTH12345678901234M");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "DEC123";
			declaration.JE_RL_NKOrigin = unloco.RL_Code;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			declaration.JE_RL_NKFinalDestination = "BWAAA";
			var supplier = OrgHeader.New(Factory);
			var cusCode = supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "12345678", Core.Constants.CountryCodes.SouthAfrica);
			declaration.JE_OH_Supplier = supplier.PK;
			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.LoadOrCreateUCRNumber("8ZA12345678COTH12345678901234S");
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entry3.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_IsUCROverridden = true;
			declaration.JE_RL_NKOrigin = Core.Constants.CountryCodes.SouthAfrica;
			entryInstruction.CEI_UCROverride = "8ZA12345678COTH12345678901234S";
			AssertNoMessageErrorContaining(entryInstruction.CEI_UCROverrideInfo, "The UCR number:8ZA12345678COTH12345678901234S already used on job:JOB ONE");
			entryInstruction.CEI_UCROverride = "88A12345678COTH12345678901234M";
			AssertNoMessageErrorContaining(entryInstruction.CEI_UCROverrideInfo, "The UCR number:8ZA12345678COTH12345678901234S already used on job:JOB ONE");
			AssertNoMessageErrors(entryInstruction.CEI_UCROverrideInfo);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			entryInstruction.CEI_UCROverride = "8ZA12345678COTH12345678901234S";
			AssertHasMessageErrorContaining(entryInstruction.CEI_UCROverrideInfo, "The UCR number:8ZA12345678COTH12345678901234S already used on job:JOB ONE");
			entryInstruction.CEI_UCROverride = "88A12345678COTH12345678901234M";
			AssertNoMessageErrorContaining(entryInstruction.CEI_UCROverrideInfo, "The UCR number:8ZA12345678COTH12345678901234S already used on job:JOB ONE");
			AssertNoMessageErrors(entryInstruction.CEI_UCROverrideInfo);
		}

		public void TestCheckCEI_Style()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var universalHelper = new UniversalReferenceTestDataHelper(Factory);
				universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "11", "", "", "", "IMP,EXW", "");
				universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "13", "", "", "", "IMP,EXW", "");
				universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "36", "", "", "", "EXP", "");
				universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "41", "", "", "", "EXW", "");
				Factory.Save();
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = "IMP";
				var testInstructions = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions;
				var testInstruction1 = testInstructions.AddNew();
				var testInstruction2 = testInstructions.AddNew();
				var testInstruction3 = testInstructions.AddNew();
				var notInListError = "The code you have selected is not in the list.";
				var invoice = testDeclaration.Invoices.AddNew();
				var helper = new ZAWhsDataTestHelper(Factory);
				helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
				var changeOfOwnershipCusProcedure = helper.ChangeOfOwnershipCusProcedure;
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = testInstruction1.PK;
				invoiceLine1.JI_Procedure = "00" + changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = testInstruction2.PK;
				invoiceLine2.JI_Procedure = "00" + changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
				var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine3.JI_CEI = testInstruction3.PK;
				invoiceLine3.JI_Procedure = "00" + changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
				CombineAssertions("ZA", () =>
				{
					testInstruction1.RunPreSaveValidation();
					AssertHasError(testInstruction1.CEI_StyleInfo, "Please enter a Customs Procedure.");
					testInstruction1.CEI_Style = "11";
					AssertNoErrors(testInstruction1.CEI_StyleInfo);
					AssertNoMessageErrors(testInstruction1.CEI_StyleInfo);
					testInstruction2.CEI_Style = "11";
					AssertNoErrors(testInstruction2.CEI_StyleInfo);
					testInstruction2.CEI_Style = "11";
					testInstruction2.CEI_Description = "Desc2";
					testInstruction1.RunPreSaveValidation();
					AssertNoErrors(testInstruction1.CEI_StyleInfo);
					AssertNoMessageErrors(testInstruction1.CEI_StyleInfo);
					AssertNoErrors(testInstruction1.CEI_DescriptionInfo);
					AssertNoMessageErrors(testInstruction1.CEI_DescriptionInfo);
					testInstruction2.CEI_Style = "13";
					AssertNoErrors(testInstruction2.CEI_StyleInfo);
					AssertNoMessageErrors(testInstruction2.CEI_StyleInfo);
					testInstruction2.CEI_Style = "XX";
					AssertNoErrors(testInstruction2.CEI_StyleInfo);
					AssertHasMessageErrorContaining(testInstruction2.CEI_StyleInfo, notInListError);
					testInstruction1.CEI_Style = "36";
					testInstruction2.CEI_Style = "13";
					testInstruction3.CEI_Style = "41";
					AssertHasMessageErrorContaining(testInstruction1.CEI_StyleInfo, notInListError);
					AssertNoMessageErrors("Import Procedure in IMP", testInstruction2.CEI_StyleInfo);
					AssertHasMessageErrorContaining(testInstruction3.CEI_StyleInfo, notInListError);
					testDeclaration.JE_MessageType = "EXP";
					testInstruction1.RunPreSaveValidation();
					testInstruction2.RunPreSaveValidation();
					testInstruction3.RunPreSaveValidation();
					AssertNoMessageErrors("Export Procedure in EXP", testInstruction1.CEI_StyleInfo);
					AssertHasMessageErrorContaining(testInstruction2.CEI_StyleInfo, notInListError);
					AssertHasMessageErrorContaining(testInstruction3.CEI_StyleInfo, notInListError);
					testInstruction2.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
					testDeclaration.JE_MessageType = "EXW";
					testInstruction1.RunPreSaveValidation();
					testInstruction2.RunPreSaveValidation();
					testInstruction3.RunPreSaveValidation();
					AssertHasMessageErrorContaining(testInstruction1.CEI_StyleInfo, notInListError);
					AssertNoMessageErrors("Exbond Procedure in EXW", testInstruction2.CEI_StyleInfo);
					AssertNoMessageErrors("Exbond Procedure in EXW", testInstruction3.CEI_StyleInfo);
					AssertNoError(testInstruction2.CEI_StyleInfo, CusEntryInstructionValidation.OnlyOneInstructionForChangeOfOwnership);
					testInstruction2.CEI_Style = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
					AssertHasError(testInstruction2.CEI_StyleInfo, CusEntryInstructionValidation.OnlyOneInstructionForChangeOfOwnership);
					testInstruction1.RunPreSaveValidation();
					AssertNoError(testInstruction1.CEI_StyleInfo, CusEntryInstructionValidation.OnlyOneInstructionForChangeOfOwnership);
				});
			}
		}

		public void TestCheckCEI_Style_RemovalTransportCode()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_RL_NKFinalDestination = "LSMSU";
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var inst = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			inst.CEI_Style = UniversalReferenceConstants.ProcedureCodes._52;
			AssertHasMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			inst.CEI_Style = UniversalReferenceConstants.ProcedureCodes._10;
			AssertNoMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			dec.JE_RL_NKFinalDestination = "ZAJNB";
			inst.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			AssertNoMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			dec.JE_RL_NKFinalDestination = "LSMSU";
			inst.Validation.ValidateCEI_Style();
			AssertHasMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			dec.JE_RemovalTransportCode = "ROA";
			inst.CEI_Style = UniversalReferenceConstants.ProcedureCodes._52;
			AssertNoMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			dec.JE_RemovalTransportCode = ZString.Empty;
			inst.CEI_Style = UniversalReferenceConstants.ProcedureCodes._52;
			AssertNoMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			dec.JE_RemovalTransportCode = "";
			inst.CEI_Style = ProcedureCodes._52;
			AssertHasMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			dec.JE_RemovalTransportCode = "ROA";
			inst.Validation.ValidateCEI_Style();
			AssertNoMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			dec.JE_RemovalTransportCode = "OTH";
			inst.CEI_Style = ProcedureCodes._53;
			AssertHasMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			inst.CEI_Style = ProcedureCodes._51;
			AssertNoMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			inst.CEI_Style = ProcedureCodes._67;
			AssertHasMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			inst.CEI_Style = ProcedureCodes._68;
			AssertHasMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
			dec.JE_RemovalTransportCode = "ROA";
			inst.Validation.ValidateCEI_Style();
			AssertNoMessageError(inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.RemovalTransportCodeRequired);
		}

		public void TestCheckCEI_StyleFinalDestinationCannotBeZBLNSWithCPC21()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "21", "", "", "", "", "BLNS");
			Factory.Save();
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var inst = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			dec.JE_ApplicationCode = "BLT";
			dec.JE_RL_NKFinalDestination = "SZMAL";
			dec.JE_OH_Importer = OrgHeader.New(Factory).PK;
			dec.JE_TransportMode = ECB.TransportTypeList.Codes.Sea;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			inst.CEI_Style = ProcedureCodes._21;
			inst.Validation.ValidateCEI_Style();
			AssertHasMessageError(inst.CEI_StyleInfo, CusEntryInstructionValidation.FinalDestinationCannotBeZAOrBLNS(inst.CusProcedure.ZZ6_Description));
			dec.JE_RL_NKFinalDestination = "CAAAB";
			inst.Validation.ValidateCEI_Style();
			AssertNoMessageError(inst.CEI_StyleInfo, CusEntryInstructionValidation.FinalDestinationCannotBeZAOrBLNS(inst.CusProcedure.ZZ6_Description));
		}

		public void TestCheckCEI_StyleFinalDestinationCannotBeZAWithCPC22()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "22", "", "", "", "", "");
			Factory.Save();
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var inst = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			dec.JE_ApplicationCode = "BLT";
			dec.JE_RL_NKFinalDestination = "ZAJNB";
			dec.JE_OH_Importer = OrgHeader.New(Factory).PK;
			dec.JE_TransportMode = ECB.TransportTypeList.Codes.Sea;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			inst.CEI_Style = ProcedureCodes._22;
			inst.Validation.ValidateCEI_Style();
			AssertHasMessageError(inst.CEI_StyleInfo, CusEntryInstructionValidation.FinalDestinationCannotBeZA(inst.CusProcedure.ZZ6_Description));
			dec.JE_RL_NKFinalDestination = "NZABY";
			inst.Validation.ValidateCEI_Style();
			AssertNoMessageError(inst.CEI_StyleInfo, CusEntryInstructionValidation.FinalDestinationCannotBeZA(inst.CusProcedure.ZZ6_Description));
		}

		public void TestCheckCEI_StylePortOfOriginCannotBeZBLNSWithCPC21()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "21", "", "", "", "", "");
			Factory.Save();
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var inst = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			dec.JE_ApplicationCode = "BLT";
			dec.JE_RL_NKOrigin = "SZMAL";
			dec.JE_OH_Importer = OrgHeader.New(Factory).PK;
			dec.JE_TransportMode = ECB.TransportTypeList.Codes.Sea;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			inst.CEI_Style = ProcedureCodes._21;
			inst.Validation.ValidateCEI_Style();
			AssertHasMessageError(inst.CEI_StyleInfo, CusEntryInstructionValidation.PortOfOriginCannotBeZAOrBLNS(inst.CusProcedure.ZZ6_Description));
			dec.JE_RL_NKOrigin = "CAAAB";
			inst.Validation.ValidateCEI_Style();
			AssertNoMessageError(inst.CEI_StyleInfo, CusEntryInstructionValidation.PortOfOriginCannotBeZAOrBLNS(inst.CusProcedure.ZZ6_Description));
		}

		public void TestCheckCEI_StylePortOfOriginMustBeBLNSWithCPC22()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "22", "", "", "", "", "");
			Factory.Save();
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var inst = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			dec.JE_ApplicationCode = "BLT";
			dec.JE_RL_NKOrigin = "ZAJNB";
			dec.JE_OH_Importer = OrgHeader.New(Factory).PK;
			dec.JE_TransportMode = ECB.TransportTypeList.Codes.Sea;
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			inst.CEI_Style = ProcedureCodes._22;
			inst.Validation.ValidateCEI_Style();
			AssertHasMessageError(inst.CEI_StyleInfo, CusEntryInstructionValidation.PortOfOriginCannotBeZA(inst.CusProcedure.ZZ6_Description));
			dec.JE_RL_NKOrigin = "SZMAL";
			inst.Validation.ValidateCEI_Style();
			AssertNoMessageError(inst.CEI_StyleInfo, CusEntryInstructionValidation.PortOfOriginCannotBeZA(inst.CusProcedure.ZZ6_Description));
		}

		public void TestCheckCEI_StyleRebateUserCode()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CCDZA", "ZA");
			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.RebateUserCode, "REBZA", "ZA");
			var testOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CCDZA2", "ZA");
			testOrg4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.RebateUserCode, "REBZA2", "ZA");
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("ZA", "A", "XX1", "", "3", "XX1_3", "IMP");
			helper.CreateRefCusProcedure("ZA", "A", "XX2", "", "4", "XX2_4", "IMP");
			helper.CreateRefCusProcedure("ZA", "A", "XX3", "", "5", "XX3_5", "IMP");
			helper.CreateRefCusProcedure("ZA", "A", "XX4", "", "6", "XX4_6", "IMP");
			helper.CreateRefCusProcedure("ZA", "A", "XX5", "", "", "XX5_0", "IMP");
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
			CombineAssertions("No Importer", () =>
			{
				testInstruction.CEI_Style = "";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX1";
				AssertHasMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX2";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX3";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX4";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX5";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XXX";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
			});
			CombineAssertions("Importer with no code", () =>
			{
				testDeclaration.JE_OH_Importer = testOrg1.PK;
				testInstruction.CEI_Style = "";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX1";
				AssertHasMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX2";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX3";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX4";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX5";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XXX";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
			});
			CombineAssertions("Importer with CCD code only", () =>
			{
				testDeclaration.JE_OH_Importer = testOrg2.PK;
				testInstruction.CEI_Style = "";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX1";
				AssertHasMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX2";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX3";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX4";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX5";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XXX";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
			});
			CombineAssertions("Importer with REB code only", () =>
			{
				testDeclaration.JE_OH_Importer = testOrg3.PK;
				testInstruction.CEI_Style = "";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX1";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX2";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX3";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX4";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX5";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XXX";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
			});
			CombineAssertions("Importer with both CCD REB code", () =>
			{
				testDeclaration.JE_OH_Importer = testOrg4.PK;
				testInstruction.CEI_Style = "";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX1";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX2";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX3";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX4";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XX5";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
				testInstruction.CEI_Style = "XXX";
				AssertNoMessageErrorContaining(testInstruction.CEI_StyleInfo, ValidationConstants.EntryInstruction.RebateUserCodeRequiredOnImporter);
			});
		}

		public void TestCheckCEI_Style_Group()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "11", "", "", "", "IMP", "");
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "37", "", "", "", "IMP", "BLNS");
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "36", "", "", "", "IMP", "");
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "38", "", "", "", "EXP", "BLNS");
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var inst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inst2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			inst.CEI_Style = ProcedureCodes._11;
			inst2.CEI_Style = ProcedureCodes._11;
			AssertNoMessageError("No same group error", inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.OnlyInstructionsFromTheSameGroup + "No Group");
			AssertNoMessageError("No same group error", inst2.CEI_StyleInfo, ValidationConstants.EntryInstruction.OnlyInstructionsFromTheSameGroup + "No Group");
			AssertNoMessageError("No same group error", inst2.CEI_StyleInfo, ValidationConstants.EntryInstruction.OnlyInstructionsFromTheSameGroup + "BLNS");
			inst2.CEI_Style = ProcedureCodes._37;
			inst.Validation.ValidateCEI_Style();
			AssertHasMessageError("Has same group error", inst.CEI_StyleInfo, ValidationConstants.EntryInstruction.OnlyInstructionsFromTheSameGroup + "No Group");
			AssertHasMessageError("Has same group error", inst2.CEI_StyleInfo, ValidationConstants.EntryInstruction.OnlyInstructionsFromTheSameGroup + inst2.ProcedureGroup);
			AssertNoMessageError("No import blns error", inst2.CEI_StyleInfo, ValidationConstants.EntryInstruction.ImportBlnsToZaRequired);
			declaration.JE_RL_NKOrigin = "ADALV";
			declaration.JE_RL_NKFinalDestination = "ADALV";
			inst2.RunPreSaveValidation();
			AssertHasMessageError("Has import blns error", inst2.CEI_StyleInfo, ValidationConstants.EntryInstruction.ImportBlnsToZaRequired);
			declaration.JE_RL_NKOrigin = "BWBBK";
			declaration.JE_RL_NKFinalDestination = "ZACPT";
			inst2.RunPreSaveValidation();
			AssertNoMessageError("No import blns error", inst2.CEI_StyleInfo, ValidationConstants.EntryInstruction.ImportBlnsToZaRequired);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKOrigin = "ADALV";
			declaration.JE_RL_NKFinalDestination = "ADALV";
			inst.CEI_Style = ProcedureCodes._36;
			inst2.CEI_Style = ProcedureCodes._38;
			AssertHasMessageError("Has export blns error", inst2.CEI_StyleInfo, ValidationConstants.EntryInstruction.ExportZaToBlnsRequired);
			declaration.JE_RL_NKOrigin = "ZACPT";
			declaration.JE_RL_NKFinalDestination = "BWBBK";
			inst2.RunPreSaveValidation();
			AssertNoMessageError("No export blns error", inst2.CEI_StyleInfo, ValidationConstants.EntryInstruction.ExportZaToBlnsRequired);
		}

		public void TestCheckCEI_OH_Carrier()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "E", "40", "", "", "", "", "");
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "B", "20", "", "", "", "", "");
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "41", "40", "", "", "", "");
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "52", "41", "", "", "", "");
			helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "67", "52", "", "", "", "");
			Factory.Save();

			var declaration = SetupBondGuaranteeValueData();
			var entryInstruction = declaration.CustomsEntryInstructions[0];
			var addInfo1ForLine1 = declaration.CustomsEntryHeaders[0].MergedLines[0].AdditionalInformationCodes[0];
			addInfo1ForLine1.CY_Data = "1000";
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, "CODCA", "CA");
			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, "CODZA", "ZA");
			CombineAssertions(() =>
			{
				entryInstruction.CEI_OH_Carrier = testOrg1.PK;
				AssertHasMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, "The selected organization does not have a Remover User Code");
				entryInstruction.CEI_OH_Carrier = testOrg2.PK;
				AssertHasMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, "The selected organization does not have a Remover User Code");
				entryInstruction.CEI_OH_Carrier = testOrg3.PK;
				AssertNoMessageErrors(entryInstruction.CEI_OH_CarrierInfo);
			});
			CombineAssertions("Test against CPC", () =>
			{
				entryInstruction.CEI_OH_Carrier = ZGuid.Empty;
				declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
				var procedureCodes = new[] { ProcedureCodes._20, ProcedureCodes._40, ProcedureCodes._52, ProcedureCodes._67 };
				foreach (var procedureCode in procedureCodes)
				{
					entryInstruction.CEI_Style = procedureCode;
					addInfo1ForLine1.CY_Data = "1000";
					entryInstruction.Validation.ValidateCEI_OH_Carrier();
					AssertHasMessageError($"{procedureCode}/ROAD - Should have Message Error", entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.RemoverRequiredForCPC);

					addInfo1ForLine1.CY_Data = "0";
					entryInstruction.Validation.ValidateCEI_OH_Carrier();
					AssertNoMessageError($"{procedureCode}/ROAD - Should NOT have Message Error", entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.RemoverRequiredForCPC);
				}

				addInfo1ForLine1.CY_Data = "1000";
				entryInstruction.CEI_Style = ProcedureCodes._41;
				entryInstruction.Validation.ValidateCEI_OH_Carrier();
				AssertNoMessageError("41/ROAD - Should NOT have Message Error", entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.RemoverRequiredForCPC);

				declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Sea;
				entryInstruction.CEI_Style = ProcedureCodes._20;
				entryInstruction.Validation.ValidateCEI_OH_Carrier();
				AssertNoMessageError("20/SEA - Should NOT have Message Error", entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.RemoverRequiredForCPC);
			});

			using (ZACustomsRegistry.Instance.AllowAutomaticSplitEntriesByBondAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
				entryInstruction.CEI_OH_Carrier = testOrg1.PK;
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.RemoverRequiredIfAutomaticSplitSet);
				entryInstruction.CEI_OH_Carrier = ZGuid.Empty;
				AssertHasMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.RemoverRequiredIfAutomaticSplitSet);

				declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Air;
				entryInstruction.Validation.ValidateCEI_OH_Carrier();
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.RemoverRequiredIfAutomaticSplitSet);

				declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
				addInfo1ForLine1.CY_Data = "1000";
				entryInstruction.CEI_OH_Carrier = testOrg1.PK;
				AssertHasMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);

				addInfo1ForLine1.CY_Data = "0";
				entryInstruction.Validation.ValidateCEI_OH_Carrier();
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);

				testOrg1.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "10000", "ZA");
				entryInstruction.Validation.ValidateCEI_OH_Carrier();
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);
			}

			using (ZACustomsRegistry.Instance.AllowAutomaticSplitEntriesByBondAmount.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
				testOrg1.CustomsCodes.RemoveAndDeleteAll();
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.RemoverRequiredIfAutomaticSplitSet);
				entryInstruction.CEI_OH_Carrier = ZGuid.Empty;
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.RemoverRequiredIfAutomaticSplitSet);

				addInfo1ForLine1.CY_Data = "1000";
				entryInstruction.CEI_OH_Carrier = testOrg1.PK;
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);

				addInfo1ForLine1.CY_Data = "0";
				entryInstruction.Validation.ValidateCEI_OH_Carrier();
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);

				testOrg1.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "10000", "ZA");
				entryInstruction.Validation.ValidateCEI_OH_Carrier();
				AssertNoMessageErrorContaining(entryInstruction.CEI_OH_CarrierInfo, ValidationConstants.EntryInstruction.BondGuaranteeValueRequiredIfAutomaticSplitSet);
			}
		}

		JobDeclaration SetupBondGuaranteeValueData()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new ECB.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_ApplicationCode = ECB.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = ProcedureCodes._11;
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine = testInvHeader.InvoiceLines.AddNew();
			testInvLine.JI_CEI = testInstruction.PK;
			testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + ProcedureCodes._00;
			Factory.Save();
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			Factory.Save();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_Packages = 0;
			entry.CH_BGMReference = "Test Reference Number";
			var entryLine1 = entry.MergedLines[0];
			var addInfo1ForLine1 = entryLine1.AdditionalInformationCodes.AddNew();
			addInfo1ForLine1.CY_Code = UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount;
			entryLine1.AdditionalInformationCodes.Add(addInfo1ForLine1);
			return declaration;
		}

		public void TestBondGuaranteeValueOnRemoverAndSubContractor()
		{
			var declaration = SetupBondGuaranteeValueData();
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_Code = "BHR";
			testOrg2.OH_Code = "REM";
			testOrg1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "BHR", Core.Constants.CountryCodes.SouthAfrica);
			testOrg2.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, "REM", Core.Constants.CountryCodes.SouthAfrica);
			testOrg1.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "2000", Core.Constants.CountryCodes.SouthAfrica);
			var customsCode2 = testOrg2.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "1000", Core.Constants.CountryCodes.SouthAfrica);
			var entry = declaration.CustomsEntryHeaders[0];
			var addInfo1ForLine1 = entry.MergedLines[0].AdditionalInformationCodes[0];
			var testInstruction = entry.EntryInstruction;

			const string expectedErrorMessage = "Bond amount from lines exceed Bond Guarantee Value for REM, customs will likely reject this entry.";
			testInstruction.CEI_OH_Carrier = testOrg2.PK;
			testInstruction.OH_SubContractor = testOrg2.PK;
			testInstruction.CEI_OH_BondHolder = ZGuid.Empty;
			addInfo1ForLine1.CY_Data = "3000";
			testInstruction.CEI_RemoverEDI = true;
			testInstruction.Validation.ValidateCEI_OH_Carrier();
			AssertHasMessageError(testInstruction.CEI_OH_CarrierInfo, expectedErrorMessage);
			testInstruction.CEI_SubContractorEDI = true;
			testInstruction.Validation.ValidateOH_SubContractor();
			AssertHasMessageError(testInstruction.OH_SubContractorInfo, expectedErrorMessage);

			testInstruction.CEI_OH_BondHolder = testOrg1.PK;
			testInstruction.CEI_RemoverEDI = true;
			testInstruction.Validation.ValidateCEI_OH_Carrier();
			AssertNoNotifications(testInstruction.CEI_OH_CarrierInfo);
			testInstruction.CEI_SubContractorEDI = true;
			testInstruction.Validation.ValidateOH_SubContractor();
			AssertNoNotifications(testInstruction.OH_SubContractorInfo);

			testInstruction.CEI_OH_BondHolder = ZGuid.Empty;
			addInfo1ForLine1.CY_Data = "1000";
			testInstruction.CEI_RemoverEDI = true;
			testInstruction.Validation.ValidateCEI_OH_Carrier();
			AssertNoNotifications(testInstruction.CEI_OH_CarrierInfo);
			testInstruction.CEI_SubContractorEDI = true;
			testInstruction.Validation.ValidateOH_SubContractor();
			AssertNoNotifications(testInstruction.OH_SubContractorInfo);

			addInfo1ForLine1.CY_Data = "4000";
			testInstruction.CEI_RemoverEDI = true;
			testInstruction.Validation.ValidateCEI_OH_Carrier();
			AssertHasMessageError(testInstruction.CEI_OH_CarrierInfo, expectedErrorMessage);
			AssertHasNotifications(testInstruction.CEI_OH_CarrierInfo);
			testInstruction.CEI_SubContractorEDI = true;
			testInstruction.Validation.ValidateOH_SubContractor();
			AssertHasMessageError(testInstruction.OH_SubContractorInfo, expectedErrorMessage);
			AssertHasNotifications(testInstruction.OH_SubContractorInfo);

			customsCode2.OK_CustomsRegNo = "";
			testInstruction.CEI_RemoverEDI = true;
			testInstruction.Validation.ValidateCEI_OH_Carrier();
			AssertNoNotifications(testInstruction.CEI_OH_CarrierInfo);
			testInstruction.CEI_SubContractorEDI = true;
			testInstruction.Validation.ValidateOH_SubContractor();
			AssertNoNotifications(testInstruction.OH_SubContractorInfo);
		}

		public void TestBondGuaranteeValueOnBondHolder()
		{
			var declaration = SetupBondGuaranteeValueData();
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_Code = "BHR";
			testOrg2.OH_Code = "REM";
			testOrg1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "BHR", Core.Constants.CountryCodes.SouthAfrica);
			testOrg2.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, "REM", Core.Constants.CountryCodes.SouthAfrica);
			var customsCode1 = testOrg1.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "2000", Core.Constants.CountryCodes.SouthAfrica);
			testOrg2.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "3000", Core.Constants.CountryCodes.SouthAfrica);
			testOrg1.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode, "REM", Core.Constants.CountryCodes.SouthAfrica);
			var entry = declaration.CustomsEntryHeaders[0];
			var addInfo1ForLine1 = entry.MergedLines[0].AdditionalInformationCodes[0];
			var testInstruction = entry.EntryInstruction;

			testInstruction.CEI_OH_Carrier = testOrg2.PK;
			addInfo1ForLine1.CY_Data = "4000";
			testInstruction.Validation.ValidateCEI_OH_BondHolder();
			AssertHasMessageError(testInstruction.CEI_OH_BondHolderInfo, ValidationConstants.EntryInstruction.AdditionalBondIsRequiredWhenTotalBNDExceedsBGV);
			testInstruction.CEI_OH_BondHolder = testOrg1.PK;
			testInstruction.CEI_ProvisionalPaymentSuretyAmount = 5000m;
			AssertNoMessageError(testInstruction.CEI_OH_BondHolderInfo, ValidationConstants.EntryInstruction.AdditionalBondIsRequiredWhenTotalBNDExceedsBGV);

			const string expectedErrorMessage = "Bond amount from lines exceed Bond Guarantee Value for BHR, customs will likely reject this entry.";
			testInstruction.CEI_OH_Carrier = ZGuid.Empty;
			testInstruction.CEI_OH_BondHolder = testOrg1.PK;
			addInfo1ForLine1.CY_Data = "2500";
			testInstruction.Validation.ValidateCEI_OH_BondHolder();
			AssertHasMessageError(testInstruction.CEI_OH_BondHolderInfo, expectedErrorMessage);

			testInstruction.CEI_OH_Carrier = testOrg2.PK;
			testInstruction.Validation.ValidateCEI_OH_BondHolder();
			AssertHasMessageError(testInstruction.CEI_OH_BondHolderInfo, expectedErrorMessage);

			testInstruction.CEI_OH_Carrier = ZGuid.Empty;
			addInfo1ForLine1.CY_Data = "2000";
			testInstruction.Validation.ValidateCEI_OH_BondHolder();
			AssertNoMessageError(testInstruction.CEI_OH_BondHolderInfo, expectedErrorMessage);

			addInfo1ForLine1.CY_Data = "4000";
			testInstruction.Validation.ValidateCEI_OH_BondHolder();
			AssertHasMessageError(testInstruction.CEI_OH_BondHolderInfo, expectedErrorMessage);

			customsCode1.OK_CustomsRegNo = "";
			testInstruction.Validation.ValidateCEI_OH_BondHolder();
			AssertNoMessageError(testInstruction.CEI_OH_BondHolderInfo, expectedErrorMessage);
		}

		public void TestCheckCEI_OH_Owner()
		{
			var procedure1 = Factory.New<RefCusProcedure>();
			procedure1.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure1.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure1.ZZ6_ProcedureCode = ProcedureCodes._41;
			var procedure2 = Factory.New<RefCusProcedure>();
			procedure2.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure2.ZZ6_ProcedureCode = ProcedureCodes._36;
			var procedure3 = Factory.New<RefCusProcedure>();
			procedure3.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			procedure3.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure3.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure3.ZZ6_ProcedureCode = ProcedureCodes._47;
			var helper = new ZAWhsDataTestHelper(Factory);
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			var invoiceLine1 = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var invoiceLine2 = dec.Invoices.AddNew().InvoiceLines.AddNew();
			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			var inst1 = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			inst1.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			var inst2 = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			inst2.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "01796636");
			CombineAssertions(() =>
			{
				invoiceLine1.JI_CEI = inst1.PK;
				inst1.CEI_Style = ProcedureCodes._41;
				inst1.Validation.ValidateCEI_OH_Owner();
				AssertHasMessageError(inst1.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.NewOwnerRequiredForCPC);
				inst1.CEI_Style = ProcedureCodes._36;
				inst1.Validation.ValidateCEI_OH_Owner();
				AssertNoMessageError(inst1.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.NewOwnerRequiredForCPC);
				inst1.CEI_Style = ProcedureCodes._41;
				inst1.CEI_OH_Owner = testOrg1.PK;
				AssertNoMessageError(inst1.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.NewOwnerRequiredForCPC);
			});
			CombineAssertions(() =>
			{
				invoiceLine1.JI_CEI = inst1.PK;
				inst1.CEI_Style = ProcedureCodes._41;
				dec.JE_OH_Importer = testOrg1.PK;
				inst1.CEI_OH_Owner = testOrg1.PK;
				AssertHasMessageError(inst1.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.NewOwnerShouldBeDifferentToOld);
				inst1.CEI_Style = ProcedureCodes._36;
				inst1.Validation.ValidateCEI_OH_Owner();
				AssertNoMessageError(inst1.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.NewOwnerShouldBeDifferentToOld);
				inst1.CEI_Style = ProcedureCodes._41;
				inst1.CEI_OH_Owner = testOrg2.PK;
				AssertNoMessageError(inst1.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.NewOwnerShouldBeDifferentToOld);
			});
			CombineAssertions(() =>
			{
				invoiceLine1.JI_CEI = inst1.PK;
				invoiceLine2.JI_CEI = inst2.PK;
				inst1.CEI_Style = ProcedureCodes._41;
				inst1.CEI_OH_Owner = testOrg1.PK;
				inst2.CEI_Style = ProcedureCodes._47;
				inst2.CEI_OH_Owner = testOrg2.PK;
				inst1.Validation.ValidateCEI_OH_Owner();
				AssertHasMessageError(inst1.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.AllNewOwnerShouldBeSame);
				AssertHasMessageError(inst2.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.AllNewOwnerShouldBeSame);
				inst1.CEI_Style = ProcedureCodes._36;
				inst1.Validation.ValidateCEI_OH_Owner();
				inst2.Validation.ValidateCEI_OH_Owner();
				AssertNoMessageError(inst1.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.AllNewOwnerShouldBeSame);
				AssertNoMessageError(inst2.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.AllNewOwnerShouldBeSame);
				inst1.CEI_Style = ProcedureCodes._41;
				inst2.CEI_OH_Owner = testOrg1.PK;
				inst1.Validation.ValidateCEI_OH_Owner();
				AssertNoMessageError(inst1.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.AllNewOwnerShouldBeSame);
				AssertNoMessageError(inst2.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.AllNewOwnerShouldBeSame);
			});
			CombineAssertions(() =>
			{
				invoiceLine1.JI_CEI = inst1.PK;
				inst1.CEI_OH_Owner = testOrg1.PK;
				AssertHasMessageError(inst1.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.NewOwnerRequireCustomsImporterCode);
				inst1.CEI_OH_Owner = testOrg2.PK;
				AssertNoMessageError(inst1.CEI_OH_OwnerInfo, ValidationConstants.EntryInstruction.NewOwnerRequireCustomsImporterCode);
			});
		}

		public void TestCheckCEI_OA_Warehouse()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var inst = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CODCA", "CA");
			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CODZA", "ZA");
			var testOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg4.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CODZA", "ZA");
			CombineAssertions("Testing presence of WHS Code", () =>
			{
				inst.CEI_OA_Warehouse = testOrg1.MainAddress.PK;
				AssertHasMessageErrorContaining(inst.CEI_OA_WarehouseInfo, "The selected organization Address does not have a Customs Controlled Premises Code - Warehouse");
				inst.CEI_OA_Warehouse = testOrg2.MainAddress.PK;
				AssertHasMessageErrorContaining(inst.CEI_OA_WarehouseInfo, "The selected organization Address does not have a Customs Controlled Premises Code - Warehouse");
				inst.CEI_OA_Warehouse = testOrg3.MainAddress.PK;
				AssertHasMessageErrorContaining(inst.CEI_OA_WarehouseInfo, "The selected organization Address does not have a Customs Controlled Premises Code - Warehouse");
				inst.CEI_OA_Warehouse = testOrg4.MainAddress.PK;
				AssertNoMessageError(inst.CEI_OA_WarehouseInfo, "The selected organization Address does not have a Customs Controlled Premises Code - Warehouse");
			});
			var testOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg5.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CPTZA", "ZA");
			var testOrg6 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg6.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CNTZA", "ZA");
			CombineAssertions("Testing format of WHS Code", () =>
			{
				AssertNoMessageErrors("No customs office exists so any whs code is valid", inst.CEI_OA_WarehouseInfo);
				ZString[] procedureCodes = new ZString[] { ProcedureCodes._40, ProcedureCodes._42, ProcedureCodes._43, ProcedureCodes._45, ProcedureCodes._49 };
				dec.JE_CustomsOffice = "CPT";
				inst.CEI_OA_Warehouse = ZGuid.Empty;
				foreach (var procedureCode in procedureCodes)
				{
					inst.CEI_Style = procedureCode;
					inst.CEI_OA_Warehouse = testOrg4.MainAddress.PK;
					AssertHasMessageErrorContaining(inst.CEI_OA_WarehouseInfo, "code must start with 'CPT'");
					inst.CEI_OA_Warehouse = testOrg5.MainAddress.PK;
					AssertNoMessageErrorContaining(inst.CEI_OA_WarehouseInfo, "code must start with 'CPT'");
				}

				inst.CEI_CustomsOfficeOverride = "CNT";
				foreach (var procedureCode in procedureCodes)
				{
					inst.CEI_Style = procedureCode;
					inst.CEI_OA_Warehouse = testOrg5.MainAddress.PK;
					AssertHasMessageErrorContaining("Has CustomsOfficeOverride = CNT", inst.CEI_OA_WarehouseInfo, "code must start with 'CNT'");
					inst.CEI_OA_Warehouse = testOrg6.MainAddress.PK;
					AssertNoMessageErrorContaining("Has CustomsOfficeOverride then not use JE_CustomsOffice", inst.CEI_OA_WarehouseInfo, "code must start with 'CPT'");
					AssertNoMessageErrorContaining("Use CustomsOfficeOverride if not empty", inst.CEI_OA_WarehouseInfo, "code must start with 'CNT'");
				}
			});
			CombineAssertions("Specific formatting of WHS Code not Required", () =>
			{
				inst.CEI_OA_Warehouse = testOrg4.MainAddress.PK;
				ZString[] procedureCodes = new ZString[] { ProcedureCodes._41, ProcedureCodes._44, ProcedureCodes._46, ProcedureCodes._47, ProcedureCodes._48 };
				foreach (ZString procedureCode in procedureCodes)
				{
					inst.CEI_Style = procedureCode;
					inst.Validation.ValidateCEI_OA_Warehouse();
					AssertNoMessageErrorContaining(inst.CEI_OA_WarehouseInfo, "code must start with 'CPT'");
					AssertNoMessageErrorContaining(inst.CEI_OA_WarehouseInfo, "code must start with 'CNT'");
				}
			});
			inst.CEI_Style = UniversalReferenceConstants.ProcedureCodes._52;
			inst.CEI_OA_Warehouse = ZGuid.Empty;
			AssertHasMessageError(inst.CEI_OA_WarehouseInfo, ValidationConstants.EntryInstruction.FromWarehouseRequiredForCPC);
			inst.CEI_OA_Warehouse = testOrg4.MainAddress.PK;
			AssertNoMessageError(inst.CEI_OA_WarehouseInfo, ValidationConstants.EntryInstruction.FromWarehouseRequiredForCPC);
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			inst.CEI_OA_Warehouse = ZGuid.Empty;
			inst.Validation.ValidateCEI_OA_Warehouse();
			AssertNoMessageError(inst.CEI_OA_WarehouseInfo, MandatoryValidation.DoNotEnterMessage(inst.CEI_OA_WarehouseInfo.HumanReadableName.ToString()));
			inst.CEI_OA_Warehouse = ZGuid.NewZGuid();
			inst.Validation.ValidateCEI_OA_Warehouse();
			AssertHasMessageError(inst.CEI_OA_WarehouseInfo, MandatoryValidation.DoNotEnterMessage(inst.CEI_OA_WarehouseInfo.HumanReadableName.ToString()));
		}

		public void TestCheckCEI_OA_Warehouse2()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.ProcedureCategoryCodes._E, "40", "", "", "", "EXW", "");
			Factory.Save();
			string notEnteredError = "You have not entered a To Warehouse.";
			string cpwError = "The selected organization Address does not have a Customs Controlled Premises Code - Warehouse";
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CODCA", "CA");
			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CODZA", "ZA");
			var testOrg4 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg4.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CODZA", "ZA");
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var inst = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			CombineAssertions("CPW Code", () =>
			{
				inst.CEI_OA_Warehouse2 = testOrg1.MainAddress.PK;
				AssertHasMessageErrorContaining(inst.CEI_OA_Warehouse2Info, cpwError);
				inst.CEI_OA_Warehouse2 = testOrg2.MainAddress.PK;
				AssertHasMessageErrorContaining(inst.CEI_OA_Warehouse2Info, cpwError);
				inst.CEI_OA_Warehouse2 = testOrg3.MainAddress.PK;
				AssertHasMessageErrorContaining(inst.CEI_OA_Warehouse2Info, cpwError);
				inst.CEI_OA_Warehouse2 = testOrg4.MainAddress.PK;
				AssertNoMessageError(inst.CEI_OA_Warehouse2Info, cpwError);
			});
			var testOrg5 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg5.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CPTZA", "ZA");
			var testOrg6 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg6.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CNTZA", "ZA");
			CombineAssertions("Testing format of WHS Code", () =>
			{
				AssertNoMessageErrors("No customs office exists so any whs code is valid", inst.CEI_OA_Warehouse2Info);
				ZString[] procedureCodes = new ZString[] { ProcedureCodes._41, ProcedureCodes._44, ProcedureCodes._46, ProcedureCodes._47, ProcedureCodes._48 };
				dec.JE_CustomsOffice = "CPT";
				inst.CEI_OA_Warehouse2 = ZGuid.Empty;
				foreach (var procedureCode in procedureCodes)
				{
					inst.CEI_Style = procedureCode;
					inst.CEI_OA_Warehouse2 = testOrg4.MainAddress.PK;
					AssertHasMessageErrorContaining(inst.CEI_OA_Warehouse2Info, "code must start with 'CPT'");
					inst.CEI_OA_Warehouse2 = testOrg5.MainAddress.PK;
					AssertNoMessageErrorContaining(inst.CEI_OA_Warehouse2Info, "code must start with 'CPT'");
				}

				inst.CEI_CustomsOfficeOverride = "CNT";
				foreach (var procedureCode in procedureCodes)
				{
					inst.CEI_Style = procedureCode;
					inst.CEI_OA_Warehouse2 = testOrg5.MainAddress.PK;
					AssertHasMessageErrorContaining(inst.CEI_OA_Warehouse2Info, "code must start with 'CNT'");
					inst.CEI_OA_Warehouse2 = testOrg6.MainAddress.PK;
					AssertNoMessageErrorContaining(inst.CEI_OA_Warehouse2Info, "code must start with 'CNT'");
					AssertNoMessageErrorContaining(inst.CEI_OA_Warehouse2Info, "code must start with 'CPT'");
				}
			});
			CombineAssertions("Specific formatting of WHS Code not Required", () =>
			{
				inst.CEI_OA_Warehouse2 = testOrg4.MainAddress.PK;
				ZString[] procedureCodes = new ZString[] { ProcedureCodes._40, ProcedureCodes._42, ProcedureCodes._43, ProcedureCodes._45, ProcedureCodes._49 };
				foreach (ZString procedureCode in procedureCodes)
				{
					inst.CEI_Style = procedureCode;
					inst.Validation.ValidateCEI_OA_Warehouse2();
					AssertNoMessageErrorContaining(inst.CEI_OA_Warehouse2Info, "code must start with 'CPT'");
					AssertNoMessageErrorContaining(inst.CEI_OA_Warehouse2Info, "code must start with 'CNT'");
				}
			});
			dec.JE_RL_NKFinalDestination = "BWBBK";
			inst.CEI_Style = "";
			dec.JE_CustomsOffice = "";
			inst.CEI_OA_Warehouse2 = ZGuid.Empty;
			CombineAssertions("CPC 20", () =>
			{
				AssertNoMessageErrors("Initial Check", inst.CEI_OA_Warehouse2Info);
				inst.CEI_Style = ProcedureCodes._20;
				inst.Validation.ValidateCEI_OA_Warehouse2();
				AssertHasMessageErrorContaining(inst.CEI_OA_Warehouse2Info, notEnteredError);
				inst.CEI_OA_Warehouse2 = testOrg4.MainAddress.PK;
				AssertNoMessageErrors("Final Check", inst.CEI_OA_Warehouse2Info);
			});
			inst.CEI_Style = ProcedureCodes._10;
			inst.CEI_OA_Warehouse2 = ZGuid.Empty;
			CombineAssertions("Category E", () =>
			{
				AssertNoMessageErrors("Initial Check", inst.CEI_OA_Warehouse2Info);
				inst.CEI_Style = ProcedureCodes._40;
				AssertEquals("Category Check", UniversalReferenceConstants.ProcedureCategoryCodes._E, inst.ProcedureCategory);
				inst.Validation.ValidateCEI_OA_Warehouse2();
				AssertHasMessageErrorContaining(inst.CEI_OA_Warehouse2Info, notEnteredError);
				inst.CEI_OA_Warehouse2 = testOrg4.MainAddress.PK;
				AssertNoMessageErrors("Final Check", inst.CEI_OA_Warehouse2Info);
			});
		}

		public void TestSimilarWarehousesCheck()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			instruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode + helper.ChangeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
			var testWarehouseAddr1 = Factory.NewWithValidTestData<OrgAddress>();
			var testWarehouseAddr2 = Factory.NewWithValidTestData<OrgAddress>();
			instruction.CEI_Style = ProcedureCodes._40;
			AssertNoMessageError("Empty Check", instruction.CEI_OA_Warehouse2Info, ValidationConstants.EntryInstruction.SameToFromWarehouseRequired);
			instruction.CEI_Style = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
			AssertHasMessageError("Requires Warehouses", instruction.CEI_OA_Warehouse2Info, ValidationConstants.EntryInstruction.SameToFromWarehouseRequired);
			instruction.CEI_OA_Warehouse = testWarehouseAddr1.PK;
			instruction.CEI_OA_Warehouse2 = testWarehouseAddr2.PK;
			AssertHasMessageError("Requires Similar Warehouses", instruction.CEI_OA_Warehouse2Info, ValidationConstants.EntryInstruction.SameToFromWarehouseRequired);
			instruction.CEI_OA_Warehouse2 = testWarehouseAddr1.PK;
			AssertNoMessageError("Has Similar Warehouses", instruction.CEI_OA_Warehouse2Info, ValidationConstants.EntryInstruction.SameToFromWarehouseRequired);
		}

		public void TestCheckCEI_ProvisionalPaymentSuretyAmount()
		{
			entryInstruction.CEI_ProvisionalPaymentSuretyAmount = 0m;
			entryInstruction.Validation.ValidateCEI_ProvisionalPaymentSuretyAmount();
			AssertNoMessageError(entryInstruction.CEI_ProvisionalPaymentSuretyAmountInfo, "value cannot be negative.");
			entryInstruction.CEI_ProvisionalPaymentSuretyAmount = 123m;
			entryInstruction.Validation.ValidateCEI_ProvisionalPaymentSuretyAmount();
			AssertNoMessageError(entryInstruction.CEI_ProvisionalPaymentSuretyAmountInfo, "value cannot be negative.");
			entryInstruction.CEI_ProvisionalPaymentSuretyAmount = -123m;
			entryInstruction.Validation.ValidateCEI_ProvisionalPaymentSuretyAmount();
			AssertHasMessageError(entryInstruction.CEI_ProvisionalPaymentSuretyAmountInfo, "value cannot be negative.");
		}

		public void TestCheckOH_SubContractor()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			AssertNoMessageErrors("Allow empty sub-contractor", entryInstruction.OH_SubContractorInfo);

			var subContractor = Factory.NewWithValidTestData<OrgHeader>();
			entryInstruction.OH_SubContractor = subContractor.PK;
			AssertHasMessageError(entryInstruction.OH_SubContractorInfo, "The selected organization does not have a Remover User Code");

			var subContractorCusCode = subContractor.CustomsCodes.AddNew();
			subContractorCusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode;
			subContractorCusCode.OK_CustomsRegNo = "111";
			entryInstruction.OH_SubContractor = subContractor.PK;
			AssertNoMessageErrors("With valid remover user code", entryInstruction.OH_SubContractorInfo);

			entryInstruction.OH_SubContractor = ZGuid.Empty;
			AssertNoMessageErrors(entryInstruction.OH_SubContractorInfo);
		}

		public void TestCheckUZ_OverrideCustomsOffice()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			Factory.Save();

			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			instruction.CEI_CustomsOfficeOverride = ZString.Empty;
			AssertNoNotifications(instruction.CEI_CustomsOfficeOverrideInfo);

			instruction.CEI_CustomsOfficeOverride = "XXX";
			AssertHasMessageError(instruction.CEI_CustomsOfficeOverrideInfo, "The code you have selected is not in the list.");

			instruction.CEI_CustomsOfficeOverride = "JHB";
			AssertNoNotifications(instruction.CEI_CustomsOfficeOverrideInfo);
		}

		public void TestCheckUZ_ExchangeRateDate()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = "EXP";
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.Validation.ValidateAll();
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "Exchange rate date will be calculated by system.");
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "Exchange rate date should not be greater than assessment date.");
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "The difference between Exchange rate date and assessment date should not be more than 2 days.");
			testInst.CEI_ExchangeRateDate = new ZDateTime(2018, 10, 1);
			testInst.Validation.ValidateAll();
			AssertHasWarning(testInst.CEI_ExchangeRateDateInfo, "Exchange rate date will be calculated by system.");
			testInst.CEI_DateForDuty = new ZDateTime(2018, 10, 2);
			testInst.Validation.ValidateAll();
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "Exchange rate date will be calculated by system.");
			testInst.CEI_ExchangeRateDate = new ZDateTime(2018, 10, 2);
			testInst.Validation.ValidateAll();
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "Exchange rate date should not be greater than assessment date.");
			testInst.CEI_ExchangeRateDate = new ZDateTime(2018, 10, 3);
			testInst.Validation.ValidateAll();
			AssertHasWarning(testInst.CEI_ExchangeRateDateInfo, "Exchange rate date should not be greater than assessment date.");
			testInst.CEI_ExchangeRateDate = new ZDateTime(2018, 9, 30);
			testInst.Validation.ValidateAll();
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "The difference between Exchange rate date and assessment date should not be more than 2 days.");
			testInst.CEI_ExchangeRateDate = new ZDateTime(2018, 9, 29);
			testInst.Validation.ValidateAll();
			AssertHasWarning(testInst.CEI_ExchangeRateDateInfo, "The difference between Exchange rate date and assessment date should not be more than 2 days.");
			testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = "EXP";
			testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var testInvHeader = testDeclaration.Invoices.AddNew();
			var testLine = testInvHeader.InvoiceLines.AddNew();
			testLine.JI_CEI = testInst.PK;
			var testEntryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var testEntryLine = testEntryHeader.AllEntryLines.AddNew();
			testLine.JI_CL = testEntryLine.PK;
			testInst.EntryHeader.MovementReferenceNumberSetter("test", ZDateTime.Today);
			testInst.Validation.ValidateAll();
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "Exchange rate date will be calculated by system.");
			testInst.CEI_ExchangeRateDate = new ZDateTime(2018, 10, 1);
			testInst.Validation.ValidateAll();
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "Exchange rate date will be calculated by system.");
			testInst.CEI_DateForDuty = new ZDateTime(2018, 10, 2);
			testInst.Validation.ValidateAll();
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "Exchange rate date will be calculated by system.");
			testInst.CEI_ExchangeRateDate = new ZDateTime(2018, 10, 2);
			testInst.Validation.ValidateAll();
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "Exchange rate date should not be greater than assessment date.");
			testInst.CEI_ExchangeRateDate = new ZDateTime(2018, 10, 3);
			testInst.Validation.ValidateAll();
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "Exchange rate date should not be greater than assessment date.");
			testInst.CEI_ExchangeRateDate = new ZDateTime(2018, 9, 30);
			testInst.Validation.ValidateAll();
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "The difference between Exchange rate date and assessment date should not be more than 2 days.");
			testInst.CEI_ExchangeRateDate = new ZDateTime(2018, 9, 29);
			testInst.Validation.ValidateAll();
			AssertNoWarning(testInst.CEI_ExchangeRateDateInfo, "The difference between Exchange rate date and assessment date should not be more than 2 days.");
		}

		public void TestBankCodeValidation()
		{
			CombineAssertions(() =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "36";
				AssertNoNotifications("Initial", testInst.CEI_BankCodeInfo);
				testInst.CEI_BankCode = "XXX";
				AssertHasMessageErrorContaining(testInst.CEI_BankCodeInfo, ListValidation.InvalidCodeMessageError);
				testInst.CEI_BankCode = "042";
				AssertNoNotifications("Valid: NoMessageError", testInst.CEI_CreditTermsInfo);
			});
		}

		public void TestCheckUZ_TransactionValue()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_MessageType = "IMP";
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_TransactionValue = 0m;
			AssertHasWarningContaining("Show message error when import declaration has a zero transaction value", testInst.CEI_TransactionValueInfo, "Transaction Value & Currency is required if an Advance Payment Notification (APN) has been declared.");
			testInst.CEI_TransactionValue = 5m;
			AssertNoWarningContaining("Do not show message error when import declaration has a NON zero transaction value", testInst.CEI_TransactionValueInfo, "Transaction Value & Currency is required if an Advance Payment Notification (APN) has been declared.");
			testDeclaration.JE_MessageType = "EXP";
			testInst.CEI_TransactionValue = 0m;
			AssertNoWarnings("Do not show message error when export declaration has a zero transaction value", testInst.CEI_TransactionValueInfo);
			testDeclaration.JE_MessageType = "EXW";
			testInst.Validation.ValidateCEI_TransactionValue();
			AssertNoWarnings("Do not show message error when Ex-Bond declaration has a zero transaction value", testInst.CEI_TransactionValueInfo);
			testInst.CEI_TransactionValue = 123.00m;
			AssertNoMessageError("No decimal places warning", testInst.CEI_TransactionValueInfo, "Transaction Value - Decimal Places not allowed, Please recapture");
			testInst.CEI_TransactionValue = 123.45m;
			AssertHasMessageErrorContaining("Has decimal places warning", testInst.CEI_TransactionValueInfo, "Transaction Value - Decimal Places not allowed, Please recapture");
		}

		public void TestCheckCEI_CreditTerms()
		{
			using var func = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: false);

			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			CombineAssertions(() =>
			{
				testInst.CEI_Style = "36";
				AssertNoNotifications("Initial", testInst.CEI_CreditTermsInfo);

				testInst.CEI_CreditTerms = "NEP";
				testInst.CEI_TransactionValue = 10;
				testInst.CEI_BankCode = "042";
				AssertHasMessageErrorContaining(testInst.CEI_CreditTermsInfo, ValidationConstants.EntryInstruction.InvalidNEPCreditTermSelection);
				testInst.CEI_BankCode = "";
				AssertHasMessageErrorContaining(testInst.CEI_CreditTermsInfo, ValidationConstants.EntryInstruction.InvalidNEPCreditTermSelection);
				testInst.CEI_TransactionValue = 0;
				AssertNoNotifications("Valid: NoMessageError", testInst.CEI_CreditTermsInfo);
				testInst.CEI_CreditTerms = "-10";
				AssertHasMessageErrorContaining(testInst.CEI_CreditTermsInfo, ValidationConstants.EntryInstruction.NoValidCreditTerm.ToString());
				testInst.CEI_CreditTerms = "AAA";
				AssertHasMessageErrorContaining(testInst.CEI_CreditTermsInfo, ValidationConstants.EntryInstruction.NoValidCreditTerm.ToString());
				testInst.CEI_CreditTerms = "500";
				AssertHasMessageErrorContaining(testInst.CEI_CreditTermsInfo, ValidationConstants.EntryInstruction.InvalidNumberCreditTermSelection);
				testInst.CEI_TransactionValue = 100;
				AssertNoNotifications("Valid: NoMessageError", testInst.CEI_CreditTermsInfo);

				testInst.CEI_CreditTerms = ZString.Empty;
				testDeclaration.JE_MessageType = "EXP";
				testInst.Validation.ValidateCEI_CreditTerms();
				AssertHasMessageError(testInst.CEI_CreditTermsInfo, ValidationConstants.EntryInstruction.EmptyCreditTerms.ToString());
				testDeclaration.JE_MessageType = "IMP";
				testInst.Validation.ValidateCEI_CreditTerms();
				AssertNoMessageError(testInst.CEI_CreditTermsInfo, ValidationConstants.EntryInstruction.EmptyCreditTerms.ToString());
			});
		}

		public void TestCheckCEI_CreditTerms_AddInvoiceDetailsToCUSDECMessageEnabled()
		{
			using var func = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			const string onlyForExportJobs = "Credit Terms only required for Export jobs";
			const string creditTermsBlank = "Credit Terms Required for Export Jobs";
			const string creditTermInvalid = "Credit Term invalid. Please choose a value from the list";

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				instruction.Validation.ValidateCEI_CreditTerms();
				AssertNoMessageErrorContaining("Blank credit terms for Import", instruction.CEI_CreditTermsInfo, creditTermsBlank);

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				instruction.Validation.ValidateCEI_CreditTerms();
				AssertNoMessageErrorContaining("Blank credit terms for Ex-Bond", instruction.CEI_CreditTermsInfo, creditTermsBlank);

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				instruction.Validation.ValidateCEI_CreditTerms();
				AssertHasMessageErrorContaining("Blank credit terms for Export", instruction.CEI_CreditTermsInfo, creditTermsBlank);

				instruction.CEI_CreditTerms = CreditTermsCodeList.Codes.ADV;
				AssertNoMessageErrorContaining("ADV for EXP", instruction.CEI_CreditTermsInfo, onlyForExportJobs);
				instruction.CEI_CreditTerms = CreditTermsCodeList.Codes.NEP;
				AssertNoMessageErrorContaining("NEP for EXP", instruction.CEI_CreditTermsInfo, onlyForExportJobs);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				instruction.Validation.ValidateCEI_CreditTerms();
				AssertHasMessageErrorContaining("NEP for IMP", instruction.CEI_CreditTermsInfo, onlyForExportJobs);
				instruction.CEI_CreditTerms = CreditTermsCodeList.Codes.ADV;
				AssertHasMessageErrorContaining("ADV for IMP", instruction.CEI_CreditTermsInfo, onlyForExportJobs);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				instruction.Validation.ValidateCEI_CreditTerms();
				AssertHasMessageErrorContaining("ADV for EXW", instruction.CEI_CreditTermsInfo, onlyForExportJobs);

				instruction.JobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				instruction.CEI_CreditTerms = "NEP";
				instruction.CEI_TransactionValue = 10;
				instruction.CEI_BankCode = "042";
				AssertHasMessageErrorContaining("ZAINVDET = true, Export, CreditTerms = NEP", instruction.CEI_CreditTermsInfo, ValidationConstants.EntryInstruction.InvalidNEPCreditTermSelection);
				instruction.CEI_BankCode = "";
				instruction.CEI_TransactionValue = 0;
				instruction.CEI_CreditTerms = "NEP";
				AssertNoNotifications("ZAINVDET = true, Export, CreditTerms = NEP", instruction.CEI_CreditTermsInfo);
				instruction.CEI_CreditTerms = "ADV";
				AssertNoNotifications("ZAINVDET = true, Export, CreditTerms = ADV", instruction.CEI_CreditTermsInfo);
				instruction.CEI_TransactionValue = 100;
				instruction.CEI_CreditTerms = "10";
				AssertNoNotifications("ZAINVDET = true, Export, CreditTerms = 10", instruction.CEI_CreditTermsInfo);
				instruction.CEI_CreditTerms = "0";
				AssertHasMessageErrorContaining("ZAINVDET = true, Export, CreditTerms = 0", instruction.CEI_CreditTermsInfo, creditTermInvalid);
				instruction.CEI_CreditTerms = "-10";
				AssertHasMessageErrorContaining("ZAINVDET = true, Export, CreditTerms = -10", instruction.CEI_CreditTermsInfo, creditTermInvalid);
			});
		}

		[TestDate(2018, 9, 19)]
		public void TestCheckUZ_UCROrderNumber()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			entryInstruction.CEI_RefType = ZString.Empty;
			entryInstruction.CEI_UCROrderNumber = "123456789012";
			AssertNoMessageErrors(entryInstruction.CEI_UCROrderNumberInfo);
			declaration.JE_RL_NKOrigin = Core.Constants.CountryCodes.Namibia;
			entryInstruction.CEI_UCROrderNumber = "1234567890123";
			AssertHasMessageErrorContaining(entryInstruction.CEI_UCROrderNumberInfo, ValidationConstants.EntryInstruction.InvalidUCRNumber);
			entryInstruction.CEI_UCROrderNumber = "12345678901234";
			AssertNoMessageErrors(entryInstruction.CEI_UCROrderNumberInfo);
			entryInstruction.CEI_UCROrderNumber = "123456789012345.";
			AssertHasMessageErrorContaining(entryInstruction.CEI_UCROrderNumberInfo, ValidationConstants.EntryInstruction.InvalidUCRNumber);
			entryInstruction.CEI_UCROrderNumber = "1234567890123456789";
			AssertNoMessageErrors(entryInstruction.CEI_UCROrderNumberInfo);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			entryInstruction.CEI_UCROrderNumber = ZString.Empty;
			AssertHasMessageError(entryInstruction.CEI_UCROrderNumberInfo, ValidationConstants.EntryInstruction.UCRNotFilledInForImportsOrExports);
			entryInstruction.CEI_UCROrderNumber = "1";
			AssertNoMessageError(entryInstruction.CEI_UCROrderNumberInfo, ValidationConstants.EntryInstruction.UCRNotFilledInForImportsOrExports);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKOrigin = Core.Constants.CountryCodes.Namibia;
			entryInstruction.CEI_UCROrderNumber = ZString.Empty;
			AssertHasMessageError(entryInstruction.CEI_UCROrderNumberInfo, ValidationConstants.EntryInstruction.UCRNotFilledInForImportsOrExports);
			entryInstruction.CEI_UCROrderNumber = "1";
			AssertNoMessageError(entryInstruction.CEI_UCROrderNumberInfo, ValidationConstants.EntryInstruction.UCRNotFilledInForImportsOrExports);
			entryInstruction.CEI_RefType = RefTypeList.Codes.Invoice;
			entryInstruction.CEI_UCROrderNumber = "2";
			AssertHasWarningContaining(entryInstruction.CEI_UCROrderNumberInfo, ValidationConstants.EntryInstruction.UCRValueWillNotBeUsed);
			entryInstruction.CEI_RefType = RefTypeList.Codes.DeclarantGenerated;
			entryInstruction.CEI_UCROrderNumber = "3";
			AssertHasWarningContaining(entryInstruction.CEI_UCROrderNumberInfo, ValidationConstants.EntryInstruction.UCRValueWillNotBeUsed);
			entryInstruction.CEI_RefType = RefTypeList.Codes.Contract;
			entryInstruction.CEI_UCROrderNumber = "4";
			AssertNoWarningContaining(entryInstruction.CEI_UCROrderNumberInfo, ValidationConstants.EntryInstruction.UCRValueWillNotBeUsed);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Miscellaneous;
			entryInstruction.CEI_UCROrderNumber = ZString.Empty;
			AssertNoMessageError(entryInstruction.CEI_UCROrderNumberInfo, ValidationConstants.EntryInstruction.UCRNotFilledInForImportsOrExports);
		}

		public void TestCheckUZ_RefType()
		{
			AssertEquals(0, declaration.Invoices.Count);
			entryInstruction.CEI_IsUCROverridden = false;
			entryInstruction.CEI_RefType = RefTypeList.Codes.Invoice;
			AssertHasWarningContaining(entryInstruction.CEI_RefTypeInfo, ValidationConstants.EntryInstruction.UCRNoInvoicesFound);
			entryInstruction.CEI_RefType = RefTypeList.Codes.DeclarantGenerated;
			var inv = declaration.Invoices.AddNew();
			inv.JZ_InvoiceNumber = "!@#$%^&*()-+.";
			entryInstruction.CEI_RefType = RefTypeList.Codes.Invoice;
			AssertHasWarningContaining(entryInstruction.CEI_RefTypeInfo, ValidationConstants.EntryInstruction.UCRNoInvoicesFound);
			entryInstruction.CEI_RefType = RefTypeList.Codes.DeclarantGenerated;
			inv.JZ_InvoiceNumber = "!@#$%^&*()-+.1234567890123";
			entryInstruction.CEI_RefType = RefTypeList.Codes.Invoice;
			AssertNoWarningContaining(entryInstruction.CEI_RefTypeInfo, ValidationConstants.EntryInstruction.UCRNoInvoicesFound);
			AssertNoWarningContaining(entryInstruction.CEI_RefTypeInfo, ValidationConstants.EntryInstruction.UCRInvoiceRefNumTruncated);
			entryInstruction.CEI_RefType = RefTypeList.Codes.DeclarantGenerated;
			inv.JZ_InvoiceNumber = "12345678901234567890";
			entryInstruction.CEI_RefType = RefTypeList.Codes.Invoice;
			AssertHasWarningContaining(entryInstruction.CEI_RefTypeInfo, ValidationConstants.EntryInstruction.UCRInvoiceRefNumTruncated);
		}

		public void TestCheckUZ_PreviousMRNMandatoryForReExportToBLNS()
		{
			var declaration = Factory.New<JobDeclaration>();
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._66;
			entryInstruction.CEI_PreviousMRN = "";
			AssertHasMessageErrorContaining(entryInstruction.CEI_PreviousMRNInfo, MandatoryValidation.YouHaveNotEntered);
			entryInstruction.CEI_PreviousMRN = "ABC";
			AssertNoMessageErrorContaining(entryInstruction.CEI_PreviousMRNInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUZ_PreviousMRNForIMX()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_PreviousMRN = "";
			AssertHasMessageErrorContaining(entryInstruction.CEI_PreviousMRNInfo, MandatoryValidation.YouHaveNotEntered);
			entryInstruction.CEI_PreviousMRN = "ABC";
			AssertNoMessageErrorContaining(entryInstruction.CEI_PreviousMRNInfo, MandatoryValidation.YouHaveNotEntered);
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_PreviousMRN = "ABC";
			AssertHasMessageErrorContaining(entryInstruction2.CEI_PreviousMRNInfo, "Same WHS MRN is entered on a different instruction on this job");
			entryInstruction2.CEI_PreviousMRN = "BCD";
			AssertNoMessageErrorContaining(entryInstruction2.CEI_PreviousMRNInfo, "Same WHS MRN is entered on a different instruction on this job");
		}

		public void TestCheckUZ_PreviousMRN()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryInstruction instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			CombineAssertions("MRN", () =>
			{
				instruction.CEI_PreviousMRN = "";
				AssertNoErrors(instruction.CEI_PreviousMRNInfo);
				instruction.CEI_PreviousMRN = "BBR201605230001110";
				AssertNoErrors(instruction.CEI_PreviousMRNInfo);
				instruction.CEI_PreviousMRN = "B";
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, Common.ZA.ZAValidationConstants.InvalidMRNLength);
				instruction.CEI_PreviousMRN = "12345678901234567a";
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, ValidationConstants.Shared.InvalidMRNCustomsOffice);
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, ValidationConstants.Shared.InvalidMRNDate);
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, ValidationConstants.Shared.InvalidMRNNumbersOnly);
				instruction.CEI_PreviousMRN = "BBR45678901234567a";
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, ValidationConstants.Shared.InvalidMRNDate);
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, ValidationConstants.Shared.InvalidMRNNumbersOnly);
				instruction.CEI_PreviousMRN = "B0R20160523000111a";
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, ValidationConstants.Shared.InvalidMRNCustomsOffice);
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, ValidationConstants.Shared.InvalidMRNNumbersOnly);
				instruction.CEI_PreviousMRN = "B0R999988770001110";
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, ValidationConstants.Shared.InvalidMRNCustomsOffice);
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, ValidationConstants.Shared.InvalidMRNDate);
				ZString mrn;
				var mrnDate = ZDateTime.Today;
				mrnDate = mrnDate.AddDays(1);
				mrn = "BBR" + mrnDate.ToString("yyyyMMdd") + "0001110";
				instruction.CEI_PreviousMRN = mrn;
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, ValidationConstants.Shared.InvalidMRNDate);
				mrnDate = mrnDate.AddDays(-1);
				mrnDate = mrnDate.AddMonths(1);
				mrn = "BBR" + mrnDate.ToString("yyyyMMdd") + "0001110";
				instruction.CEI_PreviousMRN = mrn;
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, ValidationConstants.Shared.InvalidMRNDate);
				mrnDate = mrnDate.AddMonths(-1);
				mrnDate = mrnDate.AddYears(1);
				mrn = "BBR" + mrnDate.ToString("yyyyMMdd") + "0001110";
				instruction.CEI_PreviousMRN = mrn;
				AssertHasMessageError(instruction.CEI_PreviousMRNInfo, ValidationConstants.Shared.InvalidMRNDate);
			});
		}

		public void TestCheckCEI_DateForDuty()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_DateForDuty = ZDateTime.Today.AddDays(-1);
			var entryHeader = testDeclaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("MRN", ZDateTime.Today);
			entryHeader.CH_CEI_Instruction = testInst.PK;
			testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			AssertNoWarningContaining(testInst.CEI_DateForDutyInfo, ValidationConstants.EntryInstruction.AssessmentDateManuallyEntered);
			testInst.CEI_DateForDuty = ZDateTime.Today;
			AssertHasWarningContaining(testInst.CEI_DateForDutyInfo, ValidationConstants.EntryInstruction.AssessmentDateManuallyEntered);
			testInst.CEI_MRNToBeReplaced = "MRN";
			testInst.Validation.ValidateCEI_DateForDuty();
			AssertNoWarningContaining(testInst.CEI_DateForDutyInfo, ValidationConstants.EntryInstruction.AssessmentDateManuallyEntered);
			AssertHasWarningContaining(testInst.CEI_DateForDutyInfo, ValidationConstants.EntryInstruction.AssessmentDateNotSameToOriginalAssessmentDate);
			testInst.CEI_DateForDuty = ZDateTime.Today.AddDays(-1);
			AssertNoWarningContaining(testInst.CEI_DateForDutyInfo, ValidationConstants.EntryInstruction.AssessmentDateManuallyEntered);
			AssertNoWarningContaining(testInst.CEI_DateForDutyInfo, ValidationConstants.EntryInstruction.AssessmentDateNotSameToOriginalAssessmentDate);
			testInst.CEI_MRNToBeReplaced = ZString.Empty;
			var testInvHeader = testDeclaration.Invoices.AddNew();
			var testLine = testInvHeader.InvoiceLines.AddNew();
			testLine.JI_CEI = testInst.PK;
			var testEntryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var testEntryLine = testEntryHeader.AllEntryLines.AddNew();
			testLine.JI_CL = testEntryLine.PK;
			testInst.EntryHeader.MovementReferenceNumberSetter("test", ZDateTime.Today);
			testInst.Validation.ValidateCEI_DateForDuty();
			AssertNoWarningContaining(testInst.CEI_DateForDutyInfo, ValidationConstants.EntryInstruction.AssessmentDateManuallyEntered);
		}

		public void TestCheckUZ_PortOfExit()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			testHelper.CreateCustomsOfficeCusCodeEntry("JSA");
			testHelper.CreateCustomsOfficeCusCodeEntry("KFN");
			Factory.Save();
			CombineAssertions("Port Of Exit", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				declaration.JE_CustomsOffice = "BBR";
				var transportModes = new string[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Rail };
				foreach (var transportMode in transportModes)
				{
					declaration.JE_TransportMode = transportMode;
					entryInstruction.CEI_PortOfExit = ZString.Empty;
					AssertHasMessageError("Export - " + transportMode + " - Empty: HasMessageError", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
					entryInstruction.CEI_PortOfExit = "BBR";
					AssertNoMessageErrors("Export - " + transportMode + " - Empty: NoMessageErrors", entryInstruction.CEI_PortOfExitInfo);
				}

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_RL_NKFinalDestination = "LSMSU";
				entryInstruction.CEI_PortOfExit = ZString.Empty;
				AssertHasMessageError("ExBond - CountryOfDestination.IsBLNS & Empty: HasMessageError", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_PortOfExit = "KFN";
				AssertNoMessageErrors("ExBond - CountryOfDestination.IsBLNS & !Empty: NoMessageError", entryInstruction.CEI_PortOfExitInfo);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				declaration.JE_RL_NKFinalDestination = "LSMSU";
				entryInstruction.CEI_Style = ProcedureCodes._11;
				entryInstruction.CEI_PortOfExit = ZString.Empty;
				AssertNoMessageError("Import - CountryOfDestination.IsBLNS & Empty: HasMessageError", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_PortOfExit = "KFN";
				AssertNoMessageErrors("Import - CountryOfDestination.IsBLNS & !Empty: NoMessageError", entryInstruction.CEI_PortOfExitInfo);
				declaration.JE_RL_NKFinalDestination = "DEFRA";
				entryInstruction.CEI_PortOfExit = ZString.Empty;
				AssertNoMessageErrors("ExBond - !CountryOfDestination.IsBLNS & Empty: NoMessageError", entryInstruction.CEI_PortOfExitInfo);
			});
			CombineAssertions("Port Of Destination", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				entryInstruction.CEI_Style = ProcedureCodes._20;
				entryInstruction.RunPreSaveValidation();
				AssertHasMessageError(entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_Style = ProcedureCodes._22;
				entryInstruction.RunPreSaveValidation();
				AssertHasMessageError(entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_Style = ProcedureCodes._40;
				entryInstruction.RunPreSaveValidation();
				AssertHasMessageError(entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_Style = ProcedureCodes._42;
				entryInstruction.RunPreSaveValidation();
				AssertHasMessageError(entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_Style = ProcedureCodes._10;
				entryInstruction.RunPreSaveValidation();
				AssertNoMessageError(entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_PortOfExit = "XXX";
				AssertHasMessageErrors("Invalid: HasMessageError", entryInstruction.CEI_PortOfExitInfo);
				entryInstruction.CEI_Style = ProcedureCodes._20;
				entryInstruction.CEI_PortOfExit = "JSA";
				AssertNoNotifications("Import - Valid: NoMessageError", entryInstruction.CEI_PortOfExitInfo);
				entryInstruction.CEI_Style = ProcedureCodes._22;
				entryInstruction.CEI_PortOfExit = "JSA";
				AssertNoNotifications("Import - Valid: NoMessageError", entryInstruction.CEI_PortOfExitInfo);
			});
			CombineAssertions("Port Of Exit Same As Customs Office", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._60;
				declaration.JE_CustomsOffice = "KFN";
				entryInstruction.CEI_PortOfExit = "BBR";
				AssertHasMessageError("Export - ROA - PortOfExit != CustomsOffice: HasMessageError", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfExitMustBeSameAsCustomsOffice);
				entryInstruction.CEI_PortOfExit = "KFN";
				AssertNoMessageErrors("Export - ROA - PortOfExit == CustomsOffice: NoMessageErrors", entryInstruction.CEI_PortOfExitInfo);
				entryInstruction.CEI_PortOfExit = "BBR";
				entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._52;
				entryInstruction.Validation.ValidateCEI_PortOfExit();
				AssertNoMessageErrors("Export - ROA - PortOfExit == CustomsOffice: NoMessageErrors", entryInstruction.CEI_PortOfExitInfo);
				entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._53;
				entryInstruction.Validation.ValidateCEI_PortOfExit();
				AssertNoMessageErrors("Export - ROA - PortOfExit == CustomsOffice: NoMessageErrors", entryInstruction.CEI_PortOfExitInfo);
				entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._67;
				entryInstruction.Validation.ValidateCEI_PortOfExit();
				AssertNoMessageErrors("Export - ROA - PortOfExit == CustomsOffice: NoMessageErrors", entryInstruction.CEI_PortOfExitInfo);
				entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._68;
				entryInstruction.Validation.ValidateCEI_PortOfExit();
				AssertNoMessageErrors("Export - ROA - PortOfExit == CustomsOffice: NoMessageErrors", entryInstruction.CEI_PortOfExitInfo);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				entryInstruction.Validation.ValidateCEI_PortOfExit();
				AssertNoMessageError("Import - ROA - PortOfExit != CustomsOffice: HasMessageError", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfExitMustBeSameAsCustomsOffice);
			});
		}

		public void TestCheckUZ_PortOfExit_OverrideCustomsOffice()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			testHelper.CreateCustomsOfficeCusCodeEntry("JSA");
			testHelper.CreateCustomsOfficeCusCodeEntry("KFN");
			testHelper.CreateCustomsOfficeCusCodeEntry("CTN");
			Factory.Save();

			CombineAssertions("Port Of Exit", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				declaration.JE_CustomsOffice = "BBR";
				entryInstruction.CEI_CustomsOfficeOverride = "CTN";
				var transportModes = new string[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.Rail };
				foreach (var transportMode in transportModes)
				{
					declaration.JE_TransportMode = transportMode;
					entryInstruction.CEI_PortOfExit = ZString.Empty;
					AssertHasMessageError("Export - " + transportMode + " - Empty: HasMessageError", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
					entryInstruction.CEI_PortOfExit = "CTN";
					AssertNoMessageErrors("Export - " + transportMode + " - Empty: NoMessageErrors", entryInstruction.CEI_PortOfExitInfo);
				}

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_RL_NKFinalDestination = "LSMSU";
				entryInstruction.CEI_PortOfExit = ZString.Empty;
				AssertHasMessageError("ExBond - CountryOfDestination.IsBLNS & Empty: HasMessageError", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_PortOfExit = "KFN";
				AssertNoMessageErrors("ExBond - CountryOfDestination.IsBLNS & !Empty: NoMessageError", entryInstruction.CEI_PortOfExitInfo);
			});

			CombineAssertions("Port Of Destination", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				entryInstruction.CEI_PortOfExit = ZString.Empty;
				entryInstruction.CEI_Style = ProcedureCodes._20;
				entryInstruction.RunPreSaveValidation();
				AssertHasMessageError("Import - _20: PortOfDestinationOrExitRequired", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_Style = ProcedureCodes._22;
				entryInstruction.RunPreSaveValidation();
				AssertHasMessageError("Import - _22: PortOfDestinationOrExitRequired", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_Style = ProcedureCodes._40;
				entryInstruction.RunPreSaveValidation();
				AssertHasMessageError("Import - _40: PortOfDestinationOrExitRequired", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_Style = ProcedureCodes._42;
				entryInstruction.RunPreSaveValidation();
				AssertHasMessageError("Import - _42: PortOfDestinationOrExitRequired", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_Style = ProcedureCodes._10;
				entryInstruction.RunPreSaveValidation();
				AssertNoMessageError("Import - _10: PortOfDestinationOrExitRequired", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_PortOfExit = "XXX";
				AssertHasMessageErrors("Invalid: HasMessageError", entryInstruction.CEI_PortOfExitInfo);
				entryInstruction.CEI_Style = ProcedureCodes._20;
				entryInstruction.CEI_PortOfExit = "JSA";
				AssertNoNotifications("Import - Valid - _20: NoMessageError when CEI_PortOfExit not empty", entryInstruction.CEI_PortOfExitInfo);
				entryInstruction.CEI_Style = ProcedureCodes._22;
				entryInstruction.CEI_PortOfExit = "JSA";
				AssertNoNotifications("Import - Valid - _22: NoMessageError when CEI_PortOfExit not empty", entryInstruction.CEI_PortOfExitInfo);

				declaration.JE_RL_NKFinalDestination = "LSMSU";
				entryInstruction.CEI_Style = ProcedureCodes._11;
				entryInstruction.CEI_PortOfExit = ZString.Empty;
				AssertNoMessageError("Import - CountryOfDestination.IsBLNS & Empty: HasMessageError", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfDestinationOrExitRequired);
				entryInstruction.CEI_PortOfExit = "KFN";
				AssertNoMessageErrors("Import - CountryOfDestination.IsBLNS & !Empty: NoMessageError", entryInstruction.CEI_PortOfExitInfo);
				declaration.JE_RL_NKFinalDestination = "DEFRA";
				entryInstruction.CEI_PortOfExit = ZString.Empty;
				AssertNoMessageErrors("Import - !CountryOfDestination.IsBLNS & Empty: NoMessageError", entryInstruction.CEI_PortOfExitInfo);
			});

			CombineAssertions("Port Of Exit Same As Customs Office", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._60;
				declaration.JE_CustomsOffice = "KFN";
				entryInstruction.CEI_PortOfExit = "BBR";
				AssertHasMessageError("Export - ROA - PortOfExit != JE_CustomsOffice and CEI_CustomsOfficeOverride", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfExitMustBeSameAsCustomsOffice);
				entryInstruction.CEI_PortOfExit = "KFN";
				AssertHasMessageError("Export - ROA - PortOfExit == CustomsOffice but != CEI_CustomsOfficeOverride", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfExitMustBeSameAsCustomsOffice);
				entryInstruction.CEI_PortOfExit = "CTN";
				AssertNoMessageErrors("Export - ROA - PortOfExit == CEI_CustomsOfficeOverride", entryInstruction.CEI_PortOfExitInfo);

				entryInstruction.CEI_PortOfExit = "BBR";
				entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._52;
				entryInstruction.Validation.ValidateCEI_PortOfExit();
				AssertNoMessageErrors("Export - ROA - PortOfExit != CEI_CustomsOfficeOverride but CPC = _52", entryInstruction.CEI_PortOfExitInfo);
				entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._53;
				entryInstruction.Validation.ValidateCEI_PortOfExit();
				AssertNoMessageErrors("Export - ROA - PortOfExit != CEI_CustomsOfficeOverride but CPC = _53", entryInstruction.CEI_PortOfExitInfo);
				entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._67;
				entryInstruction.Validation.ValidateCEI_PortOfExit();
				AssertNoMessageErrors("Export - ROA - PortOfExit != CEI_CustomsOfficeOverride but CPC = _67", entryInstruction.CEI_PortOfExitInfo);
				entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._68;
				entryInstruction.Validation.ValidateCEI_PortOfExit();
				AssertNoMessageErrors("Export - ROA - PortOfExit != CEI_CustomsOfficeOverride but CPC = _68", entryInstruction.CEI_PortOfExitInfo);

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._60;
				entryInstruction.Validation.ValidateCEI_PortOfExit();
				AssertNoMessageError("Import - ROA - PortOfExit != CustomsOffice but import", entryInstruction.CEI_PortOfExitInfo, ValidationConstants.EntryInstruction.PortOfExitMustBeSameAsCustomsOffice);
			});
		}

		public void TestCheckUZ_ProvisionalPaymentType()
		{
			CombineAssertions("IMP", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				entryInstruction.CEI_ProvisionalPaymentType = "XXX";
				AssertHasMessageErrorContaining(entryInstruction.CEI_ProvisionalPaymentTypeInfo, ListValidation.InvalidCodeMessageError);
				entryInstruction.CEI_ProvisionalPaymentType = "";
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentTypeInfo);
				entryInstruction.CEI_ProvisionalPaymentType = "PPE";
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentTypeInfo);
			});
			CombineAssertions("EXW", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				entryInstruction.CEI_ProvisionalPaymentType = "XXX";
				AssertHasMessageErrorContaining(entryInstruction.CEI_ProvisionalPaymentTypeInfo, ListValidation.InvalidCodeMessageError);
				entryInstruction.CEI_ProvisionalPaymentType = "";
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentTypeInfo);
				entryInstruction.CEI_ProvisionalPaymentType = "PPE";
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentTypeInfo);
			});
			CombineAssertions("EXP", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				entryInstruction.CEI_ProvisionalPaymentType = "XXX";
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentTypeInfo);
				entryInstruction.CEI_ProvisionalPaymentType = "";
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentTypeInfo);
				entryInstruction.CEI_ProvisionalPaymentType = "PPE";
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentTypeInfo);
			});
			CombineAssertions("Against Open Case", () =>
			{
				SetUp();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPE", 1m, "1", "ref1", false);
				entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPE", 1m, "2", "ref2", true);
				entryInstruction.CEI_ProvisionalPaymentType = "PPE";
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentTypeInfo);
			});
			CombineAssertions("Against Closed Case", () =>
			{
				SetUp();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPE", 1m, "1", "ref1", true);
				entryInstruction.CEI_ProvisionalPaymentType = "PPE";
				AssertHasWarningContaining(entryInstruction.CEI_ProvisionalPaymentTypeInfo, ValidationConstants.EntryInstruction.ProvisionalPaymentCaseAlreadyClosed);
			});
		}

		public void TestCheckUZ_ProvisionalPaymentAmount()
		{
			CombineAssertions("IMP", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				entryInstruction.CEI_ProvisionalPaymentType = "XXX";
				entryInstruction.CEI_ProvisionalPaymentAmount = 0m;
				AssertHasMessageErrorContaining(entryInstruction.CEI_ProvisionalPaymentAmountInfo, MandatoryValidation.ValueCannotBeZero);
				entryInstruction.CEI_ProvisionalPaymentAmount = 12m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
				entryInstruction.CEI_ProvisionalPaymentType = "";
				entryInstruction.CEI_ProvisionalPaymentAmount = 0m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
				entryInstruction.CEI_ProvisionalPaymentAmount = 12m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
				entryInstruction.CEI_ProvisionalPaymentType = "PPT";
				entryInstruction.CEI_ProvisionalPaymentAmount = 0m;
				AssertHasMessageErrorContaining(entryInstruction.CEI_ProvisionalPaymentAmountInfo, MandatoryValidation.ValueCannotBeZero);
				entryInstruction.CEI_ProvisionalPaymentAmount = 12m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
			});
			CombineAssertions("EXW", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				entryInstruction.CEI_ProvisionalPaymentType = "XXX";
				entryInstruction.CEI_ProvisionalPaymentAmount = 0m;
				AssertHasMessageErrorContaining(entryInstruction.CEI_ProvisionalPaymentAmountInfo, MandatoryValidation.ValueCannotBeZero);
				entryInstruction.CEI_ProvisionalPaymentAmount = 12m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
				entryInstruction.CEI_ProvisionalPaymentType = "";
				entryInstruction.CEI_ProvisionalPaymentAmount = 0m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
				entryInstruction.CEI_ProvisionalPaymentAmount = 12m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
				entryInstruction.CEI_ProvisionalPaymentType = "PPT";
				entryInstruction.CEI_ProvisionalPaymentAmount = 0m;
				AssertHasMessageErrorContaining(entryInstruction.CEI_ProvisionalPaymentAmountInfo, MandatoryValidation.ValueCannotBeZero);
				entryInstruction.CEI_ProvisionalPaymentAmount = 12m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
			});
			CombineAssertions("EXP", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				entryInstruction.CEI_ProvisionalPaymentType = "XXX";
				entryInstruction.CEI_ProvisionalPaymentAmount = 0m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
				entryInstruction.CEI_ProvisionalPaymentAmount = 12m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
				entryInstruction.CEI_ProvisionalPaymentType = "";
				entryInstruction.CEI_ProvisionalPaymentAmount = 0m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
				entryInstruction.CEI_ProvisionalPaymentAmount = 12m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
				entryInstruction.CEI_ProvisionalPaymentType = "PPT";
				entryInstruction.CEI_ProvisionalPaymentAmount = 0m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
				entryInstruction.CEI_ProvisionalPaymentAmount = 12m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
			});
			CombineAssertions("No Entry, no line 1 check", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				entryInstruction.CEI_ProvisionalPaymentType = "XXX";
				entryInstruction.CEI_ProvisionalPaymentAmount = 12m;
				AssertNoNotifications(entryInstruction.CEI_ProvisionalPaymentAmountInfo);
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var testInstruction = testDeclaration.CustomsEntryInstructions.AddNew();
				var testInvoice = testDeclaration.Invoices.AddNew();
				var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
				testInvoiceLine.JI_CEI = testInstruction.PK;
				var testEntry = testDeclaration.ActiveEntryHeaders.AddNew();
				var testEntryLine = testEntry.MergedLines.AddNew();
				testEntryLine.CL_LineNumber = 2;
				testInvoiceLine.JI_CL = testEntryLine.PK;
				testInstruction.CEI_ProvisionalPaymentType = "XXX";
				testInstruction.CEI_ProvisionalPaymentAmount = 123m;
				AssertHasMessageErrorContaining(testInstruction.CEI_ProvisionalPaymentAmountInfo, ValidationConstants.EntryInstruction.LineOneIsRequiredForHeaderLevelProvisionalPayment);
				testEntryLine.CL_LineNumber = 1;
				testInstruction.CEI_ProvisionalPaymentType = "XXX";
				testInstruction.CEI_ProvisionalPaymentAmount = 321m;
				AssertNoNotifications("no message error when there is line 1", testInstruction.CEI_ProvisionalPaymentAmountInfo);
			});
		}

		public void TestCheckUZ_MRNToBeReplaced()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.MovementReferenceNumberSetter("BBR201611145000001", ZDateTime.Today);
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_DateForDuty = ZDateTime.Empty;
			instruction.CEI_MRNToBeReplaced = "";
			AssertNoNotifications(instruction.CEI_MRNToBeReplacedInfo);
			instruction.CEI_MRNToBeReplaced = "B";
			AssertHasMessageError(instruction.CEI_MRNToBeReplacedInfo, Common.ZA.ZAValidationConstants.InvalidMRNLength);
			AssertHasMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.EntryInstruction.MRNToBeReplacedEntered);
			instruction.CEI_MRNToBeReplaced = "12345678901234567a";
			AssertHasMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNCustomsOffice);
			AssertHasMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNDate);
			AssertHasMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNNumbersOnly);
			instruction.CEI_MRNToBeReplaced = "BBR45678901234567a";
			AssertHasMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNDate);
			AssertHasMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNNumbersOnly);
			instruction.CEI_MRNToBeReplaced = "B0R20160523000111a";
			AssertHasMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNCustomsOffice);
			AssertHasMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNNumbersOnly);
			instruction.CEI_MRNToBeReplaced = "B0R999988770001110";
			AssertHasMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNCustomsOffice);
			AssertHasMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNDate);
			instruction.CEI_MRNToBeReplaced = "BBR201611145000002";
			AssertNoMessageError(instruction.CEI_MRNToBeReplacedInfo, Common.ZA.ZAValidationConstants.InvalidMRNLength);
			AssertNoMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNCustomsOffice);
			AssertNoMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNDate);
			AssertNoMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNNumbersOnly);
			AssertHasMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.EntryInstruction.MRNToBeReplacedEntered);
			instruction.CEI_DateForDuty = ZDateTime.Now;
			instruction.Validation.ValidateAll();
			AssertNoMessageError(instruction.CEI_MRNToBeReplacedInfo, Common.ZA.ZAValidationConstants.InvalidMRNLength);
			AssertNoMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNCustomsOffice);
			AssertNoMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNDate);
			AssertNoMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.Shared.InvalidMRNNumbersOnly);
			AssertNoMessageError(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.EntryInstruction.MRNToBeReplacedEntered);
			AssertHasWarning(instruction.CEI_MRNToBeReplacedInfo, ValidationConstants.EntryInstruction.MRNToBeReplacedNotInThisJob);
			instruction.CEI_MRNToBeReplaced = "BBR201611145000001";
			AssertNoNotifications(instruction.CEI_MRNToBeReplacedInfo);
		}

		public void TestCheckUZ_EntityType()
		{
			declaration.JE_OH_Supplier = ZGuid.Empty;
			declaration.JE_MessageType = "IMP";
			entryInstruction.Validation.ValidateCEI_EntityType();
			AssertNoMessageError(entryInstruction.CEI_EntityTypeInfo, "The Declaration must have a valid Main Supplier to use this Entity Type.");
			declaration.JE_RL_NKOrigin = Core.Constants.CountryCodes.Namibia;
			entryInstruction.Validation.ValidateCEI_EntityType();
			AssertHasMessageError(entryInstruction.CEI_EntityTypeInfo, "The Declaration must have a valid Main Supplier to use this Entity Type.");
			declaration.JE_MessageType = "EXW";
			declaration.JE_RL_NKFinalDestination = Core.Constants.CountryCodes.Namibia;
			entryInstruction.Validation.ValidateCEI_EntityType();
			AssertNoMessageError(entryInstruction.CEI_EntityTypeInfo, "The Declaration must have a valid Main Supplier to use this Entity Type.");
		}

		public void TestCheckUZ_RebateUserOverride()
		{
			var rebateOverrideOrg = Factory.NewWithValidTestData<OrgHeader>();
			var rebateOverrideOrgWithoutRebateCode = Factory.NewWithValidTestData<OrgHeader>();
			rebateOverrideOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.RebateUserCode, "1000", Core.Constants.CountryCodes.SouthAfrica);
			AssertNoMessageError(entryInstruction.CEI_RebateUserOverrideInfo, ValidationConstants.EntryInstruction.RebateUserOverrideHasNoRebateCode);
			entryInstruction.CEI_RebateUserOverride = rebateOverrideOrgWithoutRebateCode.PK;
			AssertHasMessageError(entryInstruction.CEI_RebateUserOverrideInfo, ValidationConstants.EntryInstruction.RebateUserOverrideHasNoRebateCode);
			entryInstruction.CEI_RebateUserOverride = rebateOverrideOrg.PK;
			AssertNoMessageError(entryInstruction.CEI_RebateUserOverrideInfo, ValidationConstants.EntryInstruction.RebateUserOverrideHasNoRebateCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
	}
}
