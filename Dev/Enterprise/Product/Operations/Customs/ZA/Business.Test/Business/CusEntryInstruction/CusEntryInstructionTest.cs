using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	sealed class CusEntryInstructionTest : CusEntryInstructionAbstractTest
	{
		public void TestUZ_ExchangeRateDate_ReadOnly()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var instruction = testDeclaration.CustomsEntryInstructions.AddNew();
			var testInvHeader = testDeclaration.Invoices.AddNew();
			var testLine = testInvHeader.InvoiceLines.AddNew();
			testLine.JI_CEI = instruction.PK;
			var testEntryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var testEntryLine = testEntryHeader.AllEntryLines.AddNew();
			testLine.JI_CL = testEntryLine.PK;
			Assert(!instruction.CEI_ExchangeRateDateInfo.ReadOnly);
			instruction.EntryHeader.MovementReferenceNumberSetter("test", ZDateTime.Today);
			Assert(instruction.CEI_ExchangeRateDateInfo.ReadOnly);
		}

		public void TestWarehouseInventoryManagementOn()
		{
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			AssertEquals(expected: false, entryInstruction.WarehouseIsInventoryManagementOn);
			AssertEquals(expected: false, entryInstruction.Warehouse2IsInventoryManagementOn);

			var entryInstruction2 = Factory.New<CusEntryInstruction>();
			entryInstruction2.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			entryInstruction2.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals(expected: true, entryInstruction2.WarehouseIsInventoryManagementOn);
			AssertEquals(expected: true, entryInstruction2.Warehouse2IsInventoryManagementOn);

			var entryInstruction3 = Factory.New<CusEntryInstruction>();
			entryInstruction3.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			entryInstruction3.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			warehouse.CompanyData.OB_IMUsedBondedWhs = false;
			warehouse.CompanyData.OB_CusInventoryForInwardProcessing = true;
			AssertEquals(expected: false, entryInstruction3.WarehouseIsInventoryManagementOn);
			AssertEquals(expected: false, entryInstruction3.Warehouse2IsInventoryManagementOn);

			var entryInstruction4 = Factory.New<CusEntryInstruction>();
			entryInstruction4.CEI_OA_Warehouse = warehouse.MainAddress.PK;
			entryInstruction4.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			warehouse.CompanyData.OB_CusInventoryForOutwardProcessing = true;
			AssertEquals(expected: false, entryInstruction4.WarehouseIsInventoryManagementOn);
			AssertEquals(expected: false, entryInstruction4.Warehouse2IsInventoryManagementOn);
		}

		public void TestCustomsOffice()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_CustomsOffice = "JSA";
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				AssertEquals("empty CEI_CustomsOfficeOverride then fallback JE_CustomsOffice", "JSA", instruction.CustomsOffice);

				instruction.CEI_CustomsOfficeOverride = "CNT";
				AssertEquals("has CEI_CustomsOfficeOverride", "CNT", instruction.CustomsOffice);
			});
		}

		public void TestUZ_CustomsOfficeOverride_ReadOnly()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("6");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "JSA";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			CombineAssertions(() =>
			{
				AssertEquals("default", expected: false, instruction.UZ_CustomsOfficeOverride_ReadOnly);

				entryHeader.CH_Status = ZAMessageStatusList.Codes.AwaitingResponse;
				AssertEquals("Has sent to Customs", expected: true, instruction.UZ_CustomsOfficeOverride_ReadOnly);

				entryHeader.CH_EntryStatus = "6";
				AssertEquals("Has sent to Customs but rejected", expected: false, instruction.UZ_CustomsOfficeOverride_ReadOnly);

				entryHeader.MovementReferenceNumberSetter("MRN", ZDateTime.Today);
				AssertEquals("MRN not empty", expected: true, instruction.UZ_CustomsOfficeOverride_ReadOnly);
			});
		}

		public override void TestIsChangeOfOwnershipWarehousing()
		{
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = "AB";
			procedure.ZZ6_PreviousProcedureCode = "12";
			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_ZZZ_NKDataGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			procedure.ZZ6_Description = "DESCRIPTION";

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			entryInstruction.CEI_Style = "AB";
			invoiceLine1.JI_Procedure = "AB12";

			var helper = new WhsDataTestHelper(Factory);
			entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			AssertEquals("Prerequisite: there should not be any invoice line with IsChangeOfOwnershipWarehousing.", expected: false, entryInstruction.InvoiceLines.Any(x => x.IsChangeOfOwnershipWarehousing));
			AssertEquals("IsChangeOfOwnershipWarehousing should be false because no invoice line has IsChangeOfOwnershipWarehousing set to true.", expected: false, entryInstruction.IsChangeOfOwnershipWarehousing);

			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals("Prerequisite: there should be at least one invoice line with IsChangeOfOwnershipWarehousing.", expected: true, entryInstruction.InvoiceLines.Any(x => x.IsChangeOfOwnershipWarehousing));
			AssertEquals("IsChangeOfOwnershipWarehousing should be true because at least one invoice line has IsChangeOfOwnershipWarehousing set to true.", expected: true, entryInstruction.IsChangeOfOwnershipWarehousing);

			entryInstruction.CEI_OH_Owner = ZGuid.Invalid;
			AssertEquals("ZA IsChangeOfOwnershipWarehousing calculation should not rely on entry instruction CEI_OH_Owner validity.", expected: true, entryInstruction.IsChangeOfOwnershipWarehousing);
		}

		public void TestPreviousProcedures_IsCached()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var previousProcedures = entryInstruction.PreviousProcedures;
			AssertSame(previousProcedures, entryInstruction.PreviousProcedures);
			Factory.InvalidateCachedProperties();
			Assert(previousProcedures != entryInstruction.PreviousProcedures);
		}

		public void TestPreviousProcedures()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes._00 + UniversalReferenceConstants.ProcedureCodes._40;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = UniversalReferenceConstants.ProcedureCodes._00 + UniversalReferenceConstants.ProcedureCodes._41;

			AssertContainsExactElementsInAnyOrder(new[] { UniversalReferenceConstants.ProcedureCodes._40, UniversalReferenceConstants.ProcedureCodes._41 }, entryInstruction.PreviousProcedures);

			invoiceLine2.JI_Procedure = UniversalReferenceConstants.ProcedureCodes._00 + UniversalReferenceConstants.ProcedureCodes._42;

			AssertContainsExactElementsInAnyOrder(new[] { UniversalReferenceConstants.ProcedureCodes._40, UniversalReferenceConstants.ProcedureCodes._42 }, entryInstruction.PreviousProcedures);

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_Procedure = UniversalReferenceConstants.ProcedureCodes._00 + UniversalReferenceConstants.ProcedureCodes._43;

			AssertContainsExactElementsInAnyOrder(new[] { UniversalReferenceConstants.ProcedureCodes._40, UniversalReferenceConstants.ProcedureCodes._42, UniversalReferenceConstants.ProcedureCodes._43 }, entryInstruction.PreviousProcedures);
		}

		public void TestDelete()
		{
			var dec = Factory.New<JobDeclaration>();
			var cei1 = dec.CustomsEntryInstructions.AddNew();
			var cei2 = dec.CustomsEntryInstructions.AddNew();

			var cusEntryNum1 = CusEntryNumber.New(cei1, "111", Core.Constants.CountryCodes.SouthAfrica);
			var cusEntryNum2 = CusEntryNumber.New(cei1, "222", Core.Constants.CountryCodes.SouthAfrica);
			var cusEntryNum3 = CusEntryNumber.New(cei2, "999", Core.Constants.CountryCodes.SouthAfrica);

			cei1.Delete();

			Assert(cei1.IsDeleted);
			Assert(!cei2.IsDeleted);

			Assert(cusEntryNum1.IsDeleted);
			Assert(cusEntryNum2.IsDeleted);
			Assert(!cusEntryNum3.IsDeleted);
		}

		public void TestSetDefaultValues()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions.AddNew();
			AssertEquals(RefTypeList.Codes.Invoice, cei.CEI_RefType);
			AssertEquals(ScopeList.Codes.SingleUse, cei.CEI_Scope);
		}

		public void TestIsUCROverridden()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			instruction.CEI_IsUCROverridden = true;
			AssertEquals(expected: false, instruction.CEI_UCROverrideInfo.ReadOnly);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals(expected: false, instruction.CEI_UCROverrideInfo.ReadOnly);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			AssertEquals(expected: false, instruction.CEI_UCROverrideInfo.ReadOnly);

			instruction.CEI_IsUCROverridden = false;
			AssertEquals(expected: true, instruction.CEI_UCROverrideInfo.ReadOnly);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals(expected: true, instruction.CEI_UCROverrideInfo.ReadOnly);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			AssertEquals(expected: true, instruction.CEI_UCROverrideInfo.ReadOnly);
		}

		[TestDate(2018, 9, 19)]
		public void TestUpdateUCR()
		{
			var originUnloco = Factory.New<RefUNLOCO>();
			originUnloco.RL_Code = "ZAAM";
			originUnloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;

			var dec = Factory.New<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions.AddNew();

			var impOrg = Factory.New<OrgHeader>();
			var supOrg = Factory.New<OrgHeader>();

			impOrg.OH_Code = "ABC";
			supOrg.OH_Code = "DEF";

			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			dec.JE_OH_Importer = impOrg.PK;
			dec.JE_OH_Supplier = supOrg.PK;

			dec.JE_RL_NKOrigin = originUnloco.RL_Code;
			_ = supOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.IDNumber, "12345678", Core.Constants.CountryCodes.SouthAfrica);
			cei.CEI_EntityType = EntityTypeList.Codes.IDPassportNumber;
			cei.CEI_RefType = RefTypeList.Codes.Other;
			cei.CEI_UCROrderNumber = "12345678901234";
			cei.CEI_Scope = ScopeList.Codes.MultipleUse;
			cei.CEI_IsUCROverridden = false;

			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			Factory.Save();
			AssertEquals("Empty UCR number", ZString.Empty, cei.CEI_UCROverride);

			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			Factory.Save();
			AssertEquals("Regular UCR number", "8ZA12345678POTH12345678901234M", cei.CEI_UCROverride);
		}

		[TestDate(2018, 05, 16, 4, 33, 20)]
		public void TestGetEffectiveAssessmentDate()
		{
			CusEntryInstruction instruction = null;
			AssertEquals("Is Now when instruction is null", new ZDateTime(2018, 05, 16, 4, 33, 20), CusEntryInstruction.GetEffectiveAssessmentDate(instruction, Factory));

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertEquals("Is Now when CEI_DateForDuty is empty", new ZDateTime(2018, 05, 16, 4, 33, 20), CusEntryInstruction.GetEffectiveAssessmentDate(instruction, Factory));

			instruction.CEI_DateForDuty = new ZDateTime(2018, 05, 16, 5, 55, 38);
			AssertEquals("Is CEI_DateForDuty when CEI_DateForDuty is set", new ZDateTime(2018, 05, 16, 5, 55, 38), CusEntryInstruction.GetEffectiveAssessmentDate(instruction, Factory));
		}

		public void TestCEI_DateForDuty_Readonly()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var instruction = testDeclaration.CustomsEntryInstructions.AddNew();
			Assert(!instruction.CEI_DateForDuty_ReadOnly);

			var testInvHeader = testDeclaration.Invoices.AddNew();
			var testLine = testInvHeader.InvoiceLines.AddNew();
			testLine.JI_CEI = instruction.PK;
			Assert(!instruction.CEI_DateForDuty_ReadOnly);

			var testEntryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var testEntryLine = testEntryHeader.AllEntryLines.AddNew();
			Assert(!instruction.CEI_DateForDuty_ReadOnly);

			testLine.JI_CL = testEntryLine.PK;
			Assert(!instruction.CEI_DateForDuty_ReadOnly);

			instruction.EntryHeader.MovementReferenceNumberSetter("test", ZDateTime.Today);
			Assert(instruction.CEI_DateForDuty_ReadOnly);
		}

		public void TestUZ_PreviousMRNReadOnly()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				var entryHeader = declaration.ActiveEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				entryHeader.CH_CEI_Instruction = instruction.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.ImportDeclarationByExternalBroker;
				Assert(!instruction.CEI_PreviousMRNInfo.ReadOnly);
				entryHeader.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreated;
				Assert(instruction.CEI_PreviousMRNInfo.ReadOnly);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				Assert(!instruction.CEI_PreviousMRNInfo.ReadOnly);
			}
		}

		public void TestUCRDetails_ReadOnly()
		{
			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				var entryInstruction = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

				AssertEquals(entryInstruction.CEI_JE, testDeclaration.PK);
				Assert(entryInstruction.CEI_UCROrderNumberInfo.ReadOnly);
				Assert(entryInstruction.CEI_RefTypeInfo.ReadOnly);
				Assert(entryInstruction.CEI_ScopeInfo.ReadOnly);
				Assert(entryInstruction.CEI_EntityTypeInfo.ReadOnly);

				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				Assert(entryInstruction.CEI_UCROrderNumberInfo.ReadOnly);
				Assert(entryInstruction.CEI_RefTypeInfo.ReadOnly);
				Assert(entryInstruction.CEI_ScopeInfo.ReadOnly);
				Assert(entryInstruction.CEI_EntityTypeInfo.ReadOnly);

				testDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Assert(!entryInstruction.CEI_UCROrderNumberInfo.ReadOnly);
				Assert(!entryInstruction.CEI_RefTypeInfo.ReadOnly);
				Assert(!entryInstruction.CEI_ScopeInfo.ReadOnly);
				Assert(!entryInstruction.CEI_EntityTypeInfo.ReadOnly);
			}
		}

		public void TestUZ_AssessmentDateValuesAfterDeepClone()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			instruction.CEI_DateForDuty = ZDate.Today;

			var clonedDeclaration = (JobDeclaration)((ITemplateCopyable)declaration).TemplateCopy();
			var clonedInstruction = clonedDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			AssertEquals("Assessment Date", ZDate.Empty, clonedInstruction.CEI_DateForDuty);
			AssertEquals("Assessment Date", ZDate.Today, instruction.CEI_DateForDuty);
		}

		public void TestRCCCertificatesSequenceNumber()
		{
			var instr = Factory.New<CusEntryInstruction>();
			var rcc1 = instr.RCCCertificates.AddNew();
			AssertEquals((short)1, rcc1.CY_Order);

			var rcc2 = instr.RCCCertificates.AddNew();
			AssertEquals((short)2, rcc2.CY_Order);

			rcc2.CY_Order = 1;
			AssertEquals((short)1, rcc2.CY_Order);
			AssertEquals((short)2, rcc1.CY_Order);
		}

		public void TestDRCCertificatesSequenceNumber()
		{
			var instr = Factory.New<CusEntryInstruction>();
			var drc1 = instr.DutyRebateCertificates.AddNew();
			AssertEquals((short)1, drc1.CY_Order);

			var drc2 = instr.DutyRebateCertificates.AddNew();
			AssertEquals((short)2, drc2.CY_Order);

			drc2.CY_Order = 1;
			AssertEquals((short)1, drc2.CY_Order);
			AssertEquals((short)2, drc1.CY_Order);
		}

		public void TestFromWarehouseZAddress()
		{
			var warehouse = Factory.New<OrgHeader>();
			var warehouseAddress1 = warehouse.Addresses.AddNew();
			var cusCode1 = warehouse.CustomsCodes.AddNew();
			cusCode1.OK_CustomsRegNo = "DBNSOS78901";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			cusCode1.OK_OA_PremisesAddress = warehouseAddress1.PK;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_OA_Warehouse_ZAddress.OrgPK = warehouse.PK;
			AssertEquals(warehouseAddress1.PK, instruction.CEI_OA_Warehouse);

			instruction.CEI_OA_Warehouse = ZGuid.Empty;
			instruction.CEI_OA_Warehouse_ZAddress.OrgPK = ZGuid.Empty;
			var warehouseAddress2 = warehouse.Addresses.AddNew();
			var cusCode2 = warehouse.CustomsCodes.AddNew();
			cusCode2.OK_CustomsRegNo = "DBNSOS78902";
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			cusCode2.OK_OA_PremisesAddress = warehouseAddress2.PK;
			instruction.CEI_OA_Warehouse_ZAddress.OrgPK = warehouse.PK;
			AssertEquals(ZGuid.Empty, instruction.CEI_OA_Warehouse);
		}

		public void TestFromWarehouseZAddressCTNValidation()
		{
			var warehouse = Factory.New<OrgHeader>();
			var warehouseAddress = warehouse.Addresses.AddNew();
			var cusCode = warehouse.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "STEOS 12345";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			cusCode.OK_OA_PremisesAddress = warehouseAddress.PK;

			var dec = Factory.New<JobDeclaration>();
			var impOrg = Factory.New<OrgHeader>();
			var supOrg = Factory.New<OrgHeader>();

			impOrg.OH_Code = "ABC";
			supOrg.OH_Code = "DEF";
			var supCusCode = supOrg.CustomsCodes.AddNew();
			supCusCode.OK_CustomsRegNo = "SUPPLIER78901";
			supCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			supCusCode.OK_CodeType = OrgCusCode.CodeTypes.SupplierCode;

			dec.JE_OH_Importer = impOrg.PK;
			dec.JE_OH_Supplier = supOrg.PK;
			dec.JE_CustomsOffice = "CTN";
			dec.JE_MessageType = ZAJobMessageTypeList.Codes.Export;

			var cei1 = dec.CustomsEntryInstructions.AddNew();
			cusCode.OK_CustomsRegNo = "STEOS 12345";
			cei1.CEI_OA_Warehouse_ZAddress.OrgPK = warehouse.PK;
			AssertNoNotifications(cei1);
			cei1.Delete();

			cei1 = dec.CustomsEntryInstructions.AddNew();
			cusCode.OK_CustomsRegNo = "WOROS 12345";
			cei1.CEI_OA_Warehouse_ZAddress.OrgPK = warehouse.PK;
			AssertNoNotifications(cei1);
			cei1.Delete();

			cei1 = dec.CustomsEntryInstructions.AddNew();
			cusCode.OK_CustomsRegNo = "PRLOS 12345";
			cei1.CEI_OA_Warehouse_ZAddress.OrgPK = warehouse.PK;
			AssertNoNotifications(cei1);
			cei1.Delete();

			cei1 = dec.CustomsEntryInstructions.AddNew();
			cusCode.OK_CustomsRegNo = "CTNOS 12345";
			cei1.CEI_OA_Warehouse_ZAddress.OrgPK = warehouse.PK;
			AssertNoNotifications(cei1);
			cei1.Delete();

			cei1 = dec.CustomsEntryInstructions.AddNew();
			cusCode.OK_CustomsRegNo = "OTHOS 12345";
			cei1.CEI_OA_Warehouse_ZAddress.OrgPK = warehouse.PK;
			AssertHasNotifications(cei1.CEI_OA_WarehouseInfo);
			AssertHasMessageErrorContaining(cei1.CEI_OA_WarehouseInfo, CusEntryInstructionValidation.CTNCodeStartingWith);
			cei1.Delete();
		}

		public void TestToWarehouseZAddress()
		{
			var warehouse = Factory.New<OrgHeader>();
			var warehouseAddress1 = warehouse.Addresses.AddNew();
			var cusCode1 = warehouse.CustomsCodes.AddNew();
			cusCode1.OK_CustomsRegNo = "DBNSOS78901";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			cusCode1.OK_OA_PremisesAddress = warehouseAddress1.PK;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_OA_Warehouse2_ZAddress.OrgPK = warehouse.PK;
			AssertEquals(warehouseAddress1.PK, instruction.CEI_OA_Warehouse2);

			instruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			instruction.CEI_OA_Warehouse2_ZAddress.OrgPK = ZGuid.Empty;
			var warehouseAddress2 = warehouse.Addresses.AddNew();
			var cusCode2 = warehouse.CustomsCodes.AddNew();
			cusCode2.OK_CustomsRegNo = "DBNSOS78902";
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.SouthAfrica;
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.WarehouseControlledPremisesID;
			cusCode2.OK_OA_PremisesAddress = warehouseAddress2.PK;
			instruction.CEI_OA_Warehouse2_ZAddress.OrgPK = warehouse.PK;
			AssertEquals(ZGuid.Empty, instruction.CEI_OA_Warehouse2);
		}

		public void TestWarehouseAddressDefaulting()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = helper.ChangeOfOwnershipCusProcedure.ZZ6_ProcedureCode + helper.ChangeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode;
			instruction.CEI_OH_Owner = helper.Owner.PK;
			instruction.CEI_OA_Warehouse = helper.Warehouse.PK;
			AssertEquals("instruction.CEI_OA_Warehouse2", helper.Warehouse.PK, instruction.CEI_OA_Warehouse2);
			instruction.CEI_OA_Warehouse = ZGuid.Empty;
			instruction.CEI_OA_Warehouse2 = helper.Warehouse2.PK;
			AssertEquals("instruction.CEI_OA_Warehouse", helper.Warehouse2.PK, instruction.CEI_OA_Warehouse);
			instruction.CEI_OH_Owner = ZGuid.Empty;
			instruction.CEI_OA_Warehouse = ZGuid.Empty;
			instruction.CEI_OA_Warehouse2 = helper.Warehouse.PK;
			AssertEquals("instruction.CEI_OA_Warehouse", ZGuid.Empty, instruction.CEI_OA_Warehouse);
			instruction.CEI_OH_Owner = helper.Owner.PK;
			AssertEquals("instruction.CEI_OA_Warehouse", helper.Warehouse.PK, instruction.CEI_OA_Warehouse);
			instruction.CEI_OH_Owner = ZGuid.Empty;
			instruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			instruction.CEI_OA_Warehouse = helper.Warehouse2.PK;
			AssertEquals("instruction.CEI_OA_Warehouse2", ZGuid.Empty, instruction.CEI_OA_Warehouse2);
			instruction.CEI_OH_Owner = helper.Owner.PK;
			AssertEquals("instruction.CEI_OA_Warehouse2", helper.Warehouse2.PK, instruction.CEI_OA_Warehouse2);
		}

		public void TestDescriptionDefaulting()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "12", "", "", "", "", "BLNS");
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			AssertEquals("Blank", "", instruction.CEI_Description);

			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._12;
			AssertEquals("BLNS", instruction.CEI_Description);

			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._13;
			AssertEquals("", instruction.CEI_Description);

			instruction.CEI_Description = "TEST";
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._13;
			AssertEquals("TEST", instruction.CEI_Description);

			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._12;
			AssertEquals("TEST", instruction.CEI_Description);

			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			using (instruction2.SetterSuspender.SuspendSetting(CusEntryInstruction.Schema.CEI_Style))
			{
				instruction2.CEI_Style = UniversalReferenceConstants.ProcedureCodes._12;
				AssertEquals("", instruction2.CEI_Style);
				AssertEquals("", instruction2.CEI_Description);
			}
		}

		public void TestIsInventorySelectionEnabled()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			AssertEquals("IsInventorySelectionEnabled", expected: false, instruction.IsInventorySelectionEnabled);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			AssertEquals("IsInventorySelectionEnabled", expected: true, instruction.IsInventorySelectionEnabled);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Miscellaneous;
			AssertEquals("IsInventorySelectionEnabled", expected: false, instruction.IsInventorySelectionEnabled);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals("IsInventorySelectionEnabled", expected: true, instruction.IsInventorySelectionEnabled);
		}

		public void TestUpdateOutwardLinesWithInventoryDetails()
		{
			var helper = new ZAWhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				helper.SetTariffAndSave(helper.Part, "KG");
				helper.SetTariffAndSave(helper.Part2, "KG");
				var inwardEntry = helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				inwardEntry.CH_BGMReference = "BOB";
				var inwardDeclaration = inwardEntry.Declaration;
				var inwardInvoiceLine2 = inwardDeclaration.InvoiceLines.AddNew();
				inwardInvoiceLine2.JI_CEI = inwardEntry.CH_CEI_Instruction;
				inwardInvoiceLine2.JI_PartNo = helper.Part2.OP_PartNum;
				inwardInvoiceLine2.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
				inwardInvoiceLine2.JI_InvoiceQuantity = 400m;
				inwardInvoiceLine2.JI_InvoiceUQ = "NO";
				inwardInvoiceLine2.JI_BondedWhsQuantity = 200m;
				inwardInvoiceLine2.JI_BondedWhsUnitQty = "BX";
				inwardInvoiceLine2.JI_CustomsQuantity = 20m;
				inwardInvoiceLine2.JI_CustomsUnitQty = "KG";
				inwardInvoiceLine2.JI_LinePrice = 5000m;
				inwardInvoiceLine2.JI_EngineNumber = "EGN12";
				inwardInvoiceLine2.JI_PrimaryPreference = "EU";
				inwardInvoiceLine2.JI_ROOCert = "ROO32342";
				inwardInvoiceLine2.JI_VIN = "VIN4353";
				inwardInvoiceLine2.InvoiceHeader.JZ_InvoiceAmount += 5000m;
				inwardDeclaration.DoMerge();
				Factory.Save();
				inwardEntry.PublishShipmentForWHSInward(false);
				inwardEntry.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true);
				var bondedEntryKey1 = "ENT3243-1";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey1, 100m);
				var bondedEntryKey2 = "ENT3243-2";
				ZAWhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(bondedEntryKey2, 200m);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				declaration.JE_OH_Importer = inwardDeclaration.JE_OH_Importer;
				var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
				entryInstruction.CEI_Description = "OUT DESC";
				entryInstruction.CEI_OA_Warehouse = inwardEntry.EntryInstruction.CEI_OA_Warehouse2;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_PartNo = helper.Part.OP_PartNum;
				invoiceLine1.JI_BondedWhsQuantity = 50m;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction.PK;
				invoiceLine2.JI_PartNo = helper.Part2.OP_PartNum;
				invoiceLine2.JI_BondedWhsQuantity = 100m;

				entryInstruction.UpdateOutwardLinesWithInventoryDetails();
				AssertInvoiceLine(invoiceLine1, 50m, "NO", 50m, "NO", 500m, "KG", 5000m, "", "", "");
				AssertInvoiceLine(invoiceLine2, 100m, "BX", 100m, "BX", 10m, "KG", 2500m, "EGN12", "VIN4353", "ROO32342");
			}
		}

		public void TestCanDelete()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			Assert("Cannot delete Entry Instruction when Entry Line exists", !instruction.CanDelete);
			entryLine.Delete();
			invoiceLine.JI_CEI = ZGuid.Empty;
			Assert("Entry Instruction can be deleted when no Entry Line exists", instruction.CanDelete);

			entryHeader.Messages.AddNew().EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			Assert("Entry Instruction can be deleted when the entry that points to it has customs messages", !instruction.CanDelete);
		}

		public void TestIsLandedCost()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "A", "4A", "", "6", "BOB", "EXP", landedCostOnly: true);
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = instruction.PK;
			var entryHeader1Charge1 = entryHeader1.Charges.AddNew();
			var entryHeader1Charge2 = entryHeader1.Charges.AddNew();
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			var entryLine1Fee1 = entryLine1.Fees.AddNew();
			var entryLine1Fee2 = entryLine1.Fees.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_CL = entryLine1.PK;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2Charge1 = entryHeader2.Charges.AddNew();
			var entryHeader2Charge2 = entryHeader2.Charges.AddNew();
			var entryLine2 = entryHeader2.MergedLines.AddNew();
			var entryLine2Fee1 = entryLine2.Fees.AddNew();
			var entryLine2Fee2 = entryLine2.Fees.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_CEI_Instruction = instruction.PK;
			var entryHeader3Charge1 = entryHeader3.Charges.AddNew();
			var entryHeader3Charge2 = entryHeader3.Charges.AddNew();
			var entryLine3 = entryHeader3.MergedLines.AddNew();
			var entryLine3Fee1 = entryLine3.Fees.AddNew();
			var entryLine3Fee2 = entryLine3.Fees.AddNew();
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = instruction.PK;
			invoiceLine3.JI_CL = entryLine3.PK;

			AssertEquals("entryHeader1Charge1.C1_IsLandedCostOnly", false, entryHeader1Charge1.C1_IsLandedCostOnly);
			AssertEquals("entryHeader1Charge2.C1_IsLandedCostOnly", false, entryHeader1Charge2.C1_IsLandedCostOnly);
			AssertEquals("entryLine1Fee1.CF_IsLandedCostOnly", false, entryLine1Fee1.CF_IsLandedCostOnly);
			AssertEquals("entryLine1Fee2.CF_IsLandedCostOnly", false, entryLine1Fee2.CF_IsLandedCostOnly);
			AssertEquals("entryHeader2Charge1.C1_IsLandedCostOnly", false, entryHeader2Charge1.C1_IsLandedCostOnly);
			AssertEquals("entryHeader2Charge2.C1_IsLandedCostOnly", false, entryHeader2Charge2.C1_IsLandedCostOnly);
			AssertEquals("entryLine2Fee1.CF_IsLandedCostOnly", false, entryLine2Fee1.CF_IsLandedCostOnly);
			AssertEquals("entryLine2Fee2.CF_IsLandedCostOnly", false, entryLine2Fee2.CF_IsLandedCostOnly);
			AssertEquals("entryHeader3Charge1.C1_IsLandedCostOnly", false, entryHeader3Charge1.C1_IsLandedCostOnly);
			AssertEquals("entryHeader3Charge2.C1_IsLandedCostOnly", false, entryHeader3Charge2.C1_IsLandedCostOnly);
			AssertEquals("entryLine3Fee1.CF_IsLandedCostOnly", false, entryLine3Fee1.CF_IsLandedCostOnly);
			AssertEquals("entryLine3Fee2.CF_IsLandedCostOnly", false, entryLine3Fee2.CF_IsLandedCostOnly);
		}

		public void TestReasonForNotAbleToDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._11;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals("Entry Instruction with CPC 11 is being used by an Entry Line and cannot be deleted.", instruction.ReasonForNotAbleToDelete);
			invoiceLine.JI_CL = ZGuid.Empty;
			AssertEquals(ZString.Empty, instruction.ReasonForNotAbleToDelete);

			entryHeader.Messages.AddNew().EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			AssertEquals("Entry Instruction with CPC 11 is being used by an Entry Header which has responses against it and cannot be deleted.", instruction.ReasonForNotAbleToDelete);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			ICusCodeDataTypeSupporter supporter = instruction;
			supporter.AssertType(null, CusCodeDataTypeList.Codes.VOCValueAfter);
			supporter.AssertType(null, CusCodeDataTypeList.Codes.VOCValueBefore);
			supporter.AssertType(null, CusCodeDataTypeList.Codes.PPAmount);
			supporter.AssertType(null, CusCodeDataTypeList.Codes.AdditionalInformation);
			supporter.AssertType(null, CusCodeDataTypeList.Codes.VPBAmount);
			supporter.AssertType(typeof(CaseNumber), CusCodeDataTypeList.Codes.CaseNumber);
			supporter.AssertType(null, "ZZ!");

			var caseNumber = instruction.CaseNumbers.AddNew();
			caseNumber.CY_Data = "DSD";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(caseNumber.PK);
			AssertEquals(typeof(CaseNumber), codeData.GetType());
		}

		public void TestCEI_DateForDuty_HasMovementReferenceNumber_and_readonly()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInstruction = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			Assert(!testInstruction.HasMovementReferenceNumber);

			var testInvHeader = testDeclaration.Invoices.AddNew();
			var testLine = testInvHeader.InvoiceLines.AddNew();
			testLine.JI_CEI = testInstruction.PK;
			Assert(!testInstruction.HasMovementReferenceNumber);

			var testEntryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			var testEntryLine = testEntryHeader.AllEntryLines.AddNew();
			Assert(!testInstruction.HasMovementReferenceNumber);

			testLine.JI_CL = testEntryLine.PK;
			Assert(!testInstruction.HasMovementReferenceNumber);

			testInstruction.EntryHeader.MovementReferenceNumberSetter("test", ZDateTime.Today);
			Assert(testInstruction.HasMovementReferenceNumber);
		}

		public void TestCEI_DateForDuty_ClearVOCBeforeValues()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_ZZF_NKTaxType = "VAT";
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_ZZF_NKTaxType = "VAT";
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.InvoiceLines.Add(invoiceLine1);
			entryLine1.Fees.AddOrUpdate("VAT", 50m);
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.InvoiceLines.Add(invoiceLine2);
			entryLine2.Fees.AddOrUpdate("VAT", 50m);
			Factory.Save();

			AssertEquals(50m, entry1.ValueAddedTax);
			AssertEquals(50m, entry2.ValueAddedTax);

			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			entry1.ValueAddedTaxBefore = 30m;
			entry2.ValueAddedTaxBefore = 60m;
			entry2.DoNotClaimVATRefund = true;
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals(30m, entry1.ValueAddedTaxBefore);
				AssertEquals(50m, entry1.ValueAddedTax);
				AssertEquals(false, entry1.DoNotClaimVATRefund);

				AssertEquals(60m, entry2.ValueAddedTaxBefore);
				AssertEquals(60m, entry2.ValueAddedTax);
				AssertEquals(true, entry2.DoNotClaimVATRefund);
			});

			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			CombineAssertions(() =>
			{
				AssertEquals(0m, entry1.ValueAddedTaxBefore);
				AssertEquals(50m, entry1.ValueAddedTax);
				AssertEquals(false, entry1.DoNotClaimVATRefund);

				AssertEquals(0m, entry2.ValueAddedTaxBefore);
				AssertEquals(50m, entry2.ValueAddedTax);
				AssertEquals(false, entry2.DoNotClaimVATRefund);
			});
		}

		public void TestTypeDecider()
		{
			Assert("Update Enterprise.Customs.Business.CusEntryInstruction to include a decider for this class", Factory.New(typeof(Customs.Business.CusEntryInstruction)).GetType() == typeof(CusEntryInstruction));
		}

		[TestDate(2019, 07, 31)]
		public void TestAddInfos()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "BankCode");
			testHelper.CreateZACusCodeListEntry(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "091", "STANDARD CHARTERED BANK");
			Factory.Save();

			CombineAssertions(() =>
			{
				var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_RefType = ZString.Empty;
				testInst.CEI_Scope = ZString.Empty;
				testInst.CEI_EntityType = ZString.Empty;
				testInst.CEI_Style = "36";
				AssertEquals(string.Empty, testInst.CEI_AddInfo);
				testInst.CEI_BankCode = "TBC";
				Factory.Save();
				AssertEquals("BankCode=TBC*UCROverride=9", testInst.CEI_AddInfo);
				AssertHasMessageErrorContaining(testInst.CEI_BankCodeInfo, ListValidation.InvalidCodeMessageError);
				testInst.CEI_BankCode = "091";
				Factory.Save();
				AssertNoMessageErrors(testInst.CEI_BankCodeInfo);

				testInst.CEI_CreditTerms = "CCT";
				Factory.Save();
				AssertEquals("BankCode=091*CreditTerms=CCT*UCROverride=9", testInst.CEI_AddInfo);

				testInst.CEI_TransactionValue = 123456789012m;
				Factory.Save();
				AssertEquals("BankCode=091*CreditTerms=CCT*TransactionValue=123456789012*UCROverride=9", testInst.CEI_AddInfo);

				testInst.CEI_RX_NKTransactionValueCurrency = "ZAR";
				Factory.Save();
				AssertEquals("BankCode=091*CreditTerms=CCT*RX_NKTransactionValueCurrency=ZAR*TransactionValue=123456789012*UCROverride=9", testInst.CEI_AddInfo);

				testInst.CEI_UCROrderNumber = "ABCDEFGHIJKLMNOPQRS";
				Factory.Save();
				AssertEquals("BankCode=091*CreditTerms=CCT*RX_NKTransactionValueCurrency=ZAR*TransactionValue=123456789012*UCROrderNumber=ABCDEFGHIJKLMNOPQRS*UCROverride=9ABCDEFGHIJKLMNOPQRS", testInst.CEI_AddInfo);
			});
		}

		public void TestBondHolderCode()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();

			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInstruction = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			AssertEquals("testCase 1", string.Empty, testInstruction.BondHolderCode);

			testInstruction.CEI_OH_BondHolder = testOrg.PK;
			AssertEquals("testCase 2", string.Empty, testInstruction.BondHolderCode);

			testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BondHolderCode, "1000", Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals("testCase 3", "1000", testInstruction.BondHolderCode);
		}

		public void TestUZ_CreditTerms()
		{
			CusEntryInstruction inst = Factory.New<CusEntryInstruction>();
			inst.CEI_CreditTerms = "@@";
			AssertEquals("@@", inst.CEI_CreditTerms);
			inst.CEI_CreditTerms = "001";
			AssertEquals("1", inst.CEI_CreditTerms);
		}

		public void TestRebateUserCode()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInstruction = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testDeclaration.JE_OH_Importer = testOrg.PK;
			AssertEquals(string.Empty, testInstruction.ImporterCode);
			AssertEquals(string.Empty, testInstruction.RebateUserCode);
			testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "1000", Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals("1000", testInstruction.ImporterCode);
			AssertEquals(string.Empty, testInstruction.RebateUserCode);
			testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.RebateUserCode, "2000", Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals("1000", testInstruction.ImporterCode);
			AssertEquals("2000", testInstruction.RebateUserCode);
			var rebateOverrideOrg = Factory.NewWithValidTestData<OrgHeader>();
			testInstruction.CEI_RebateUserOverride = rebateOverrideOrg.PK;
			AssertEquals("", testInstruction.RebateUserCode);
			rebateOverrideOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.RebateUserCode, "3000", Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals("3000", testInstruction.RebateUserCode);
		}

		public void TestCaseNumbers()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var caseNumbers = entryInstruction.CaseNumbers;
			CombineAssertions(() =>
			{
				AssertSame("Collection is cached", caseNumbers, entryInstruction.CaseNumbers);
				AssertEquals("Is Registered Child Editable", expected: true, entryInstruction.IsRegisteredEditableChildObject(caseNumbers));
			});
		}

		public void TestProcedureCategory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "Z", "99", "", "", "aaa", "aaa", "");
			Factory.Save();

			var inst = Factory.NewWithValidTestData<CusEntryInstruction>();
			inst.CEI_Style = "99";
			AssertEquals(inst.ProcedureCategory, "Z");

			procedure.ZZ6_Category = "X";
			Factory.Save();
			AssertEquals(inst.ProcedureCategory, "X");
		}

		public void TestSupportsCloneCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();

			AssertEquals("Clone support - CusEntryInstruction", expected: true, instruction.SupportsClone());
		}

		public void TestProvisionalPayments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var instruction3 = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader11 = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader12 = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader21 = declaration.ActiveEntryHeaders.AddNew();
			var entryHeader01 = declaration.ActiveEntryHeaders.AddNew();
			var anotherDeclaration = Factory.New<JobDeclaration>();
			var anotherEntryHeader = declaration.ActiveEntryHeaders.AddNew();

			entryHeader11.CH_CEI_Instruction = instruction1.PK;
			entryHeader12.CH_CEI_Instruction = instruction1.PK;
			entryHeader21.CH_CEI_Instruction = instruction2.PK;
			anotherEntryHeader.CH_CEI_Instruction = instruction1.PK;

			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "VAT", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "DTY", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "VAT", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "DTY", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "OTH", 1m, "1", "REF");
			var pick1 = entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PEN", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "REF");
			var pick2 = entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "FOR", 1m, "1", "REF");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "");
			entryHeader11.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "XXX", 1m, "1", "REF");

			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "VAT", 1m, "1", "REF");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "", 1m, "1", "REF");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "DTY", 1m, "1", "REF");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "OTH", 1m, "1", "REF");
			var pick3 = entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PEN", 1m, "1", "REF");
			var pick4 = entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPA", 1m, "1", "REF");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "PPX", 1m, "1", "REF");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "");
			entryHeader12.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "XXX", 1m, "1", "REF");

			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "VAT", 1m, "1", "REF");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "", 1m, "1", "REF");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "DTY", 1m, "1", "REF");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "OTH", 1m, "1", "REF");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "REF");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "");
			entryHeader01.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "XXX", 1m, "1", "REF");

			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "VAT", 1m, "1", "REF");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "", 1m, "1", "REF");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "DTY", 1m, "1", "REF");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "OTH", 1m, "1", "REF");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "REF");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "PEN", 1m, "1", "");
			anotherEntryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", "XXX", 1m, "1", "REF");
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(new CusEntryPayInfo[] { pick1, pick2, pick3, pick4 }, instruction1.ProvisionalPaymentPayInfos.ToArray());
				AssertContainsExactElementsInAnyOrder(Array.Empty<CusEntryPayInfo>(), instruction2.ProvisionalPaymentPayInfos.ToArray());
				AssertContainsExactElementsInAnyOrder(Array.Empty<CusEntryPayInfo>(), instruction3.ProvisionalPaymentPayInfos.ToArray());
			});
		}

		public void TestDefaultAssessmentDateWhenSettingMRNToBeReplaced()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("MRN", new ZDateTime(2016, 11, 21));
			entryHeader.CH_CEI_Instruction = instruction1.PK;
			instruction1.CEI_DateForDuty = new ZDateTime(2016, 11, 21);

			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_MRNToBeReplaced = "MRN";
			AssertEquals(new ZDateTime(2016, 11, 21), instruction2.CEI_DateForDuty);
		}

		[ExpectNoExceptions]
		public void TestUZ_UCROverrideMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_UCROverride = "9CN91330302765207767NTINVGWAB1903113CS";
			AssertEquals(35, instruction.CEI_UCROverrideInfo.MaxLength);
			AssertEquals("9CN91330302765207767NTINVGWAB190311", instruction.CEI_UCROverride);
		}

		public void TestBondGuaranteeCodeAndValue()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInstruction = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			testInstruction.CEI_OH_BondHolder = ZGuid.Empty;
			testInstruction.CEI_OH_Carrier = ZGuid.Empty;
			AssertEquals("", testInstruction.BondGuaranteeCode);
			AssertEquals(0m, testInstruction.BondGuaranteeValue);

			testInstruction.CEI_OH_BondHolder = testOrg1.PK;
			testInstruction.CEI_OH_Carrier = testOrg2.PK;
			AssertEquals("", testInstruction.BondGuaranteeCode);
			AssertEquals(0m, testInstruction.BondGuaranteeValue);

			var customsCode2 = testOrg2.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "1000", Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals("", testInstruction.BondGuaranteeCode);
			AssertEquals(0m, testInstruction.BondGuaranteeValue);

			var customsCode1 = testOrg1.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.BGV, "2000", Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals("2000", testInstruction.BondGuaranteeCode);
			AssertEquals(2000m, testInstruction.BondGuaranteeValue);

			customsCode1.OK_CustomsRegNo = ZString.Empty;
			AssertEquals(0m, testInstruction.BondGuaranteeValue);

			testInstruction.CEI_OH_BondHolder = ZGuid.Empty;
			AssertEquals("1000", testInstruction.BondGuaranteeCode);
			AssertEquals(1000m, testInstruction.BondGuaranteeValue);

			testInstruction.CEI_OH_Carrier = ZGuid.Empty;
			testInstruction.OH_SubContractor = testOrg2.PK;
			AssertEquals("1000", testInstruction.BondGuaranteeCode);
			AssertEquals(1000m, testInstruction.BondGuaranteeValue);

			customsCode2.OK_CustomsRegNo = ZString.Empty;
			AssertEquals("", testInstruction.BondGuaranteeCode);
			AssertEquals(0m, testInstruction.BondGuaranteeValue);
		}

		public void TestAddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedError()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();

			AssertEquals(false, entryInstruction.AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedError);

			entryHeader1.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader1.MovementReferenceNumberSetter("ABC1234", new DateTime(2020, 07, 31));
			AssertEquals(expected: true, entryInstruction.AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedError);
			entryHeader1.MovementReferenceNumberSetter("ABC1234", new DateTime(2020, 08, 01));
			AssertEquals(expected: false, entryInstruction.AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedError);

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction.PK;

			entryHeader2.MovementReferenceNumberSetter("QWE12345", new DateTime(2020, 07, 31));
			AssertEquals(expected: true, entryInstruction.AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedError);
			entryHeader2.MovementReferenceNumberSetter("QWE12345", new DateTime(2020, 08, 01));
			AssertEquals(expected: false, entryInstruction.AddBlueRowMessageErrorForMultipleLinkedEntriesInsteadOfRedError);
		}

		public void TestCusProcedure()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "", "12", "", "", "", "", "BLNS");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			entryInstruction.CEI_Style = "12";
			AssertNotNull(entryInstruction.CusProcedure);

			entryInstruction.CEI_Style = "13";
			AssertNull(entryInstruction.CusProcedure);
		}

		public void TestICustomsCustomLabelsConfigOrgProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstructionCustomLabelProvider = declaration.CustomsEntryInstructions.AddNew() as ICustomsCustomLabelsConfigOrgProvider;

			AssertEquals(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib1, entryInstructionCustomLabelProvider.PartAttribute1);
			AssertEquals(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib2, entryInstructionCustomLabelProvider.PartAttribute2);
			AssertEquals(JobComInvoiceLine.Schema.JI_NewOwnerPartAttrib3, entryInstructionCustomLabelProvider.PartAttribute3);
			AssertEquals(JobComInvoiceLine.Schema.JI_NewOwnerSerialNum, entryInstructionCustomLabelProvider.SerialNumber);
		}

		public void TestIsBondHolderRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			declaration.JE_TransportMode = ZString.Empty;
			AssertEquals("JE_RemovalTransportCode is blank", false, entryInstruction.IsBondHolderRequired);

			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Road;
			AssertEquals("JE_RemovalTransportCode is road", true, entryInstruction.IsBondHolderRequired);

			declaration.JE_RemovalTransportCode = Core.Constants.TransportModes.Air;
			AssertEquals("JE_RemovalTransportCode is air", false, entryInstruction.IsBondHolderRequired);
		}

		public void TestRemoverAndSubContractorEDISelection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			AssertNull(entryInstruction.SubContractor);
			AssertEquals("Select remover by default", true, entryInstruction.CEI_RemoverEDI);
			AssertEquals("Deselect sub-contractor by default", false, entryInstruction.CEI_SubContractorEDI);
			AssertEquals("Sub-contractor EDI is readonly", true, entryInstruction.IsSubContractorEDIReadOnly);

			var subContractor = Factory.NewWithValidTestData<OrgHeader>();
			entryInstruction.OH_SubContractor = subContractor.PK;
			AssertEquals(subContractor, entryInstruction.SubContractor);
			AssertEquals("Deselect remover", false, entryInstruction.CEI_RemoverEDI);
			AssertEquals("Select sub-contractor", true, entryInstruction.CEI_SubContractorEDI);
			AssertEquals("Sub-contractor EDI is not readonly", false, entryInstruction.IsSubContractorEDIReadOnly);

			entryInstruction.OH_SubContractor = ZGuid.Empty;
			AssertNull(entryInstruction.SubContractor);
			AssertEquals("Select remover by default", true, entryInstruction.CEI_RemoverEDI);
			AssertEquals("Deselect sub-contractor by default", false, entryInstruction.CEI_SubContractorEDI);
			AssertEquals("Sub-contractor EDI is readonly", true, entryInstruction.IsSubContractorEDIReadOnly);

			entryInstruction.OH_SubContractor = ZGuid.Invalid;
			AssertNull(entryInstruction.SubContractor);
			AssertEquals("Select remover by default", true, entryInstruction.CEI_RemoverEDI);
			AssertEquals("Deselect sub-contractor by default", false, entryInstruction.CEI_SubContractorEDI);
			AssertEquals("Sub-contractor EDI is readonly", true, entryInstruction.IsSubContractorEDIReadOnly);
		}

		public void TestTotalBondSuretyAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			AssertEquals(0m, entryInstruction.TotalBondSuretyAmount);

			var entryLine = entryHeader.MergedLines.AddNew();
			var addInfo1 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo1.CY_Code = UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount;
			addInfo1.CY_Data = "111";
			entryLine.AdditionalInformationCodes.Add(addInfo1);

			var addInfo2 = entryLine.AdditionalInformationCodes.AddNew();
			addInfo2.CY_Code = UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount;
			addInfo2.CY_Data = "222";
			entryLine.AdditionalInformationCodes.Add(addInfo2);

			AssertEquals(333m, entryInstruction.TotalBondSuretyAmount);
		}

		public void TestShouldUpdateUCRNumber()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("6"); // CustomsRejected
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals("Import: EntryHeader = null", false, entryInstruction.ShouldUpdateUCRNumber);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals("Export: EntryHeader = null", true, entryInstruction.ShouldUpdateUCRNumber);

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			entryHeader.MessageStatus = ZAMessageStatusList.Codes.NotSent;
			entryHeader.JobStatus = "";
			AssertEquals("Import: message not sent", false, entryInstruction.ShouldUpdateUCRNumber);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			entryHeader.MessageStatus = ZAMessageStatusList.Codes.NotSent;
			entryHeader.JobStatus = "";
			AssertEquals("Export: message not sent", true, entryInstruction.ShouldUpdateUCRNumber);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			entryHeader.MessageStatus = ZAMessageStatusList.Codes.Acknowledged;
			entryHeader.JobStatus = "1";
			AssertEquals("Export: message acknowledged & released", false, entryInstruction.ShouldUpdateUCRNumber);
			entryHeader.JobStatus = "6";
			AssertEquals("Export: message acknowledged & rejected", true, entryInstruction.ShouldUpdateUCRNumber);
		}

		public void TestTransactionValueDecimalPlaces()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_TransactionValue = 123.00m;
			AssertEquals("123", entryInstruction1.CEI_TransactionValue.ToString());
			AssertEquals(0, entryInstruction1.TransactionValueDecimalPlaces);

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_TransactionValue = 123.45m;
			AssertEquals("123.45", entryInstruction2.CEI_TransactionValue.ToString());
			AssertEquals(2, entryInstruction2.TransactionValueDecimalPlaces);

			entryInstruction2.CEI_TransactionValue = 123.450000000001m;
			AssertEquals("123.45", entryInstruction2.CEI_TransactionValue.ToString());
			AssertEquals(2, entryInstruction2.TransactionValueDecimalPlaces);

			entryInstruction2.CEI_TransactionValue = 123.00m;
			AssertEquals("123", entryInstruction2.CEI_TransactionValue.ToString());
			AssertEquals(0, entryInstruction2.TransactionValueDecimalPlaces);
		}

		void AssertInvoiceLine(JobComInvoiceLine invoiceLine, ZDecimal invoiceQty, ZString invoiceUQ, ZDecimal countableQty, ZString countableUQ, ZDecimal customsQty, ZString customsUQ, ZDecimal linePrice, ZString engineNumber, ZString vin, ZString rooCert)
		{
			AssertEquals("invoiceLine.JI_InvoiceQuantity", invoiceQty, invoiceLine.JI_InvoiceQuantity);
			AssertEquals("invoiceLine.JI_InvoiceUQ", invoiceUQ, invoiceLine.JI_InvoiceUQ);
			AssertEquals("invoiceLine.JI_BondedWhsQuantity", countableQty, invoiceLine.JI_BondedWhsQuantity);
			AssertEquals("invoiceLine.JI_BondedWhsUnitQty", countableUQ, invoiceLine.JI_BondedWhsUnitQty);
			AssertEquals("invoiceLine.JI_CustomsUnitQty", customsUQ, invoiceLine.JI_CustomsUnitQty);
			AssertEquals("invoiceLine.JI_CustomsQuantity", customsQty, invoiceLine.JI_CustomsQuantity);
			AssertEquals("invoiceLine.JI_LinePrice", linePrice, invoiceLine.JI_LinePrice);
			AssertEquals("invoiceLine.JI_EngineNumber", engineNumber, invoiceLine.JI_EngineNumber);
			AssertEquals("invoiceLine.JI_VIN", vin, invoiceLine.JI_VIN);
			AssertEquals("invoiceLine.JI_ROOCert", rooCert, invoiceLine.JI_ROOCert);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.CustomsEntryInstructions.AddNew();
		}
	}

	internal static class TestHelperExtensions
	{
		internal static CusEntryPayInfo AddNewEntryPayInfo(this CusEntryPayInfoCollection<CusEntryPayInfo> collection, ZDateTime dateTimeOfPayment, string paymentParty, ZString transactionType, ZDecimal dutyAmount, ZString messageNum, string paymentReference, bool closed = false)
		{
			var factory = collection.Factory;
			var result = factory.New<ProvisionalPaymentCusEntryPayInfo>();
			result.C9_PaymentDate = dateTimeOfPayment;
			result.C9_CusResReceived = true;
			result.C9_RemAdvReceived = closed;
			result.C9_PaymentParty = paymentParty;
			result.C9_TransactionType = transactionType;
			result.C9_PaymentAmount = dutyAmount;
			result.C9_PaymentReference = paymentReference;
			result.C9_IncomingPayResponseNo = messageNum;
			collection.Add(result);
			return result;
		}
	}
}
