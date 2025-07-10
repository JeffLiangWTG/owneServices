using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.Business.Utilities;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CustomsChargeTypeList = Enterprise.Customs.Business.CustomsChargeTypeList;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
	{
		public void TestClientIsInventoryManagementOn()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = GetJobDeclaration(JobMessageTypeList.Codes.Import);
			declaration.JE_OH_Importer = orgHeader.PK;
			AssertEquals(expected: false, declaration.ClientIsInventoryManagementOn);

			orgHeader.CompanyData.OB_IMUsedBondedWhs = true;
			AssertEquals(expected: true, declaration.ClientIsInventoryManagementOn);

			orgHeader.CompanyData.OB_IMUsedBondedWhs = false;
			orgHeader.CompanyData.OB_CusInventoryForInwardProcessing = true;
			AssertEquals(expected: false, declaration.ClientIsInventoryManagementOn);

			orgHeader.CompanyData.OB_CusInventoryForInwardProcessing = false;
			orgHeader.CompanyData.OB_CusInventoryForOutwardProcessing = true;
			AssertEquals(expected: false, declaration.ClientIsInventoryManagementOn);
		}

		public void TestTransportDocumentNumber_SEA_IMP()
		{
			var declaration = GetJobDeclaration(JobMessageTypeList.Codes.Import);
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "MSCU123450";
			declaration.JE_CarrierCode = "MSC";

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(true))
			{
				AssertEquals("MSCU123450", declaration.TransportDocumentNumber);
			}

			using (TDTSegmentSplitHelper.TemporarilyEnableTDTSegmentSplit(false))
			{
				AssertEquals("MSC MSCU123450", declaration.TransportDocumentNumber);
			}
		}

		public override void TestGetContainerModeForDeclaration()
		{
			var declaration = GetJobDeclaration(JobMessageTypeList.Codes.Import);
			AssertGetContainerModeForDeclaration(declaration);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertGetContainerModeForDeclaration(declaration);
		}

		void AssertGetContainerModeForDeclaration(JobDeclaration declaration)
		{
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.FCL));
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.LCL));
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.BuyersConsol));
			AssertEquals(Core.Constants.ContainerModes.Bulk, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.Bulk));
			AssertEquals(Core.Constants.ContainerModes.Liquid, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.Liquid));
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.BreakBulk));
			AssertEquals(Core.Constants.ContainerModes.BreakBulk, declaration.GetContainerModeForDeclaration(ZString.Empty, Core.Constants.ContainerModes.RollOnRollOff));
		}

		public override void TestGetSupportingDocSendingObject()
		{
			var sendingObject = declaration.GetSupportingDocSendingObject();
			AssertType(typeof(SupportingDocSendingObject), sendingObject);
		}

		public void TestTypeofInvoicingSupporter()
		{
			AssertEquals(typeof(JobDeclarationInvoicingSupporter), declaration.InvoicingSupporter.GetType());
		}

		public new void TestILandedCostHeader_SupportsNoCostApportionmentItem()
		{
			AssertEquals(expected: true, ((ILandedCostHeader)declaration).SupportsNoCostApportionmentItem);
		}

		[TestDate(2018, 1, 25)]
		public void TestValidateAllVINs()
		{
			var universalTestHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			var tariffType1P1 = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();

			var testVINTariff = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "001122", new ZDateTime(2000, 1, 1), new ZDateTime(2079, 06, 06));
			universalTestHelper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributes.VIN, UniversalReferenceConstants.TariffAttributes.Values.Optional, testVINTariff);
			Factory.Save();

			CreateInvoiceLineWithVINNumber("IMP", "B000001", "INV001", 1, "001122", "VIN1234", ZDateTime.Now);
			CreateInvoiceLineWithVINNumber("IMP", "B000002", "INV002", 1, "001122", "VIN1234", ZDateTime.Now);
			CreateInvoiceLineWithVINNumber("EXP", "B000003", "INV003", 1, "001122", "VIN3456", ZDateTime.Now);
			CreateInvoiceLineWithVINNumber("EXP", "B000004", "INV004", 1, "001122", "VIN3456", ZDateTime.Now);
			CreateInvoiceLineWithVINNumber("IMP", "B000005", "INV005", 1, "001122", "VIN1234", ZDateTime.Now.AddMonths(-7));
			Factory.Save();

			var invoiceLine = CreateInvoiceLineWithVINNumber("IMP", "B000006", "INV006", 1, "001122", "VIN1234", ZDateTime.Now);
			var invoiceLine2 = invoiceLine.InvoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_Tariff = "334455";
			invoiceLine2.JI_VIN = "VIN3456";
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var declaration2 = factory2.Load<JobDeclaration>(invoiceLine.Declaration.PK);
			declaration2.RunPreSaveValidationWithFetchHints();
			AssertNull("Dictionary must be disposed", declaration2.DuplicateVINWarningDictionary);

			var testInvoiceLine = declaration2.Invoices[0].InvoiceLines.GetByLineNo(1) as JobComInvoiceLine;
			AssertHasWarningContaining(testInvoiceLine.JI_VINInfo, "The VIN Number has been used on another declaration");
			AssertHasWarning(testInvoiceLine.JI_VINInfo, "The VIN Number has been used on another declaration:\r\nDeclaration: 'B000001', Invoice Number: 'INV001', Invoice Line Number: '1'\r\nDeclaration: 'B000002', Invoice Number: 'INV002', Invoice Line Number: '1'");

			invoiceLine.Declaration.JE_MessageType = "EXP";
			invoiceLine.JI_VIN = "VIN3456";
			Factory.Save();

			factory2 = new BusinessObjectFactory();
			declaration2 = factory2.Load<JobDeclaration>(invoiceLine.Declaration.PK);
			declaration2.RunPreSaveValidationWithFetchHints();
			AssertHasWarningContaining(invoiceLine.JI_VINInfo, "The VIN Number has been used on another declaration");
			AssertHasWarning(invoiceLine.JI_VINInfo, "The VIN Number has been used on another declaration:\r\nDeclaration: 'B000003', Invoice Number: 'INV003', Invoice Line Number: '1'\r\nDeclaration: 'B000004', Invoice Number: 'INV004', Invoice Line Number: '1'");

			invoiceLine.JI_VIN = "VIN5678";
			Factory.Save();

			factory2 = new BusinessObjectFactory();
			declaration2 = factory2.Load<JobDeclaration>(invoiceLine.Declaration.PK);
			declaration2.RunPreSaveValidationWithFetchHints();
			AssertNoWarningContaining(invoiceLine.JI_VINInfo, "The VIN Number has been used on another declaration");
		}

		public void TestVINLookup()
		{
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_VIN = "VIN1";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_VIN = "VIN1";
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_VIN = "VIN2";

			AssertEquals(expected: true, declaration.VINLookup.ContainsKey("VIN1"));
			AssertEquals(2, declaration.VINLookup["VIN1"].Count);
			AssertEquals(expected: true, declaration.VINLookup.ContainsKey("VIN2"));
			AssertEquals(1, declaration.VINLookup["VIN2"].Count);

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_VIN = "VIN2";
			AssertEquals(2, declaration.VINLookup["VIN2"].Count);

			invoiceLine1.JI_VIN = "VIN2";
			AssertEquals(1, declaration.VINLookup["VIN1"].Count);
			AssertEquals(3, declaration.VINLookup["VIN2"].Count);
		}

		public override void TestResetInvoiceDateForGroupingInvoiceInTemplateCopy()
		{
			var declaration = GetJobDeclaration(JobMessageTypeList.Codes.Import);
			var groupinvoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			groupinvoice.JZ_InvoiceDate = new ZDateTime(2007, 08, 14);
			var normalinvoice = declaration.Invoices.AddNew();
			groupinvoice.JZ_InvoiceDate = new ZDateTime(2006, 08, 14);
			Factory.Save();

			var declarationCopy = (JobDeclaration)declaration.TemplateCopy();
			var groupinvoiceCopy = (JobComInvoiceGroupHeader)declarationCopy.AllGroupHeaders[0];
			AssertEquals(ZDateTime.Empty, groupinvoiceCopy.JZ_InvoiceDate);

			var normalinvoiceCopy = (JobComInvoiceHeader)declarationCopy.Invoices.First(x => !x.JZ_GroupInvoice);
			AssertEquals(ZDateTime.Empty, normalinvoiceCopy.JZ_InvoiceDate);
		}

		public void TestResetValuesOnJobComInvoiceLinesForTemplateCopy()
		{
			var declaration = GetJobDeclaration(JobMessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "11";
			declaration.Invoices.RemoveAll();

			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "INV001";
			invHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			declaration.ResumeApportionment();
			invHeader.JobComInvoiceLines.RemoveAndDeleteAll();

			var invLine = invHeader.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			invLine.JI_Procedure = $"{invLine.EntryInstruction.CEI_Style}00";
			invLine.JI_TargetEntryLineNumber = 1;
			Factory.Save();

			AssertEquals("JobComInvoiceLine should have TargetEntryLineNumber=1", (ZShort)1, invLine.JI_TargetEntryLineNumber);

			var jobDeclarationCloned = (JobDeclaration)declaration.TemplateCopy();
			Factory.Save();
			var invoiceLineCloned = jobDeclarationCloned.InvoiceLines[0];
			AssertEquals("JobComInvoiceLine should have TargetEntryLineNumber=0 After deep cloned", (ZShort)0, invoiceLineCloned.JI_TargetEntryLineNumber);
		}

		public void TestMessageStatusDescription()
		{
			Assert(DeclarationStatusHelper.ShouldCombineEntryStatusFromHeaders(declaration.CountryCode, declaration.JE_GC));

			declaration.JE_MessageStatus = ZAMessageStatusList.Codes.AwaitingResponse;
			AssertEquals("Awaiting Response", declaration.JE_MessageStatusDescription);
			declaration.JE_MessageStatus = ZAMessageStatusList.Codes.Acknowledged;
			AssertEquals("Acknowledged", declaration.JE_MessageStatusDescription);

			var entryHeader0 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader0.CH_Status = ZAMessageStatusList.Codes.AwaitingResponse;
			AssertEquals("Awaiting Response", declaration.JE_MessageStatusDescription);
			AssertEquals("AWA", declaration.JE_MessageStatus);

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_Status = ZAMessageStatusList.Codes.AwaitingResponse;
			AssertEquals("Awaiting Response", declaration.JE_MessageStatusDescription);
			AssertEquals("AWA", declaration.JE_MessageStatus);

			entryHeader1.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
			AssertEquals(JobDeclaration.MultipleMessageStatusWithSameCategory("Awaiting"), declaration.JE_MessageStatusDescription);
			AssertEquals(Common.Shared.CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_MessageStatus);

			entryHeader1.CH_Status = ZAMessageStatusList.Codes.Error;
			AssertEquals(BaseJobDeclaration.MultipleMessageStatusWithSameCategory("Awaiting"), declaration.JE_MessageStatusDescription);
			AssertEquals(Common.Shared.CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_MessageStatus);

			entryHeader0.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
			AssertEquals(BaseJobDeclaration.MultipleMessageStatusWithSameCategory("Error"), declaration.JE_MessageStatusDescription);
			AssertEquals(Common.Shared.CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_MessageStatus);

			entryHeader1.CH_Status = "SNT";
			AssertEquals(Common.Shared.CommonEntryStatusList.Descriptions.MultipleEntryStatus, declaration.JE_MessageStatusDescription);
			AssertEquals(Common.Shared.CommonEntryStatusList.Codes.MultipleEntryStatus, declaration.JE_MessageStatus);

			entryHeader0.CH_Status = ZAMessageStatusList.Codes.NotSent;
			entryHeader1.CH_Status = ZAMessageStatusList.Codes.NotSent;
			AssertEquals("Acknowledged", declaration.JE_MessageStatusDescription);
			AssertEquals("ACK", declaration.JE_MessageStatus);
		}

		public void TestUniversalCopyAttributes()
		{
			var componentType = declaration.GetType();
			AssertEquals("JobDeclaration should have UniversalCopyWithExtendedEntitiesAttribute.", expected: true, componentType.GetCustomAttributes(typeof(UniversalCopyWithExtendedEntitiesAttribute), inherit: true).Length > 0);

			var entryInstructionCollectionInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "CustomsEntryInstructions");
			AssertEquals("CustomsEntryInstructions collection should have UniversalCopyCollectionEntityAttribute.", expected: true, entryInstructionCollectionInfo.GetCustomAttributes(typeof(UniversalCopyCollectionEntityAttribute), inherit: true)[0] != null);

			var invoicesInfo = BusinessObjectToCopyTemplateReflectionHelper.GetProperties(componentType).First(info => info.Name == "Invoices");
			AssertEquals("Invoices collection should have UniversalCopyCollectionEntityAttribute.", expected: true, entryInstructionCollectionInfo.GetCustomAttributes(typeof(UniversalCopyCollectionEntityAttribute), inherit: true)[0] != null);
		}

		public override void TestResetValuesOnTemplateCopyAfterClone()
		{
			base.TestResetValuesOnTemplateCopyAfterClone();
			declaration.JE_BOESightNumber = "111";
			declaration.JE_BOESightDate = new ZDateTime(2016, 09, 13);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV-001";
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_InvoiceDate = ZDateTime.Now.AddDays(-20);

			var clonedDeclaration = declaration.TemplateCopy() as JobDeclaration;
			AssertNotEquals("No Sight details copied", declaration.JE_BOESightDate, clonedDeclaration.JE_BOESightDate);
			AssertNotEquals("No Sight details copied", declaration.JE_BOESightNumber, clonedDeclaration.JE_BOESightNumber);
			AssertEquals("JZ_InvoiceNumber", "INV-001", clonedDeclaration.Invoices[0].JZ_InvoiceNumber);
			AssertEquals("JZ_InvoiceDate should not be cloned for ZA", ZDateTime.Empty, clonedDeclaration.Invoices[0].JZ_InvoiceDate);
		}

		public void TestJE_OH_AgentOverride()
		{
			CombineAssertions("JE_AGTCode cleared", () =>
			{
				declaration.JE_AGTCode = "12345678";
				AssertEquals("12345678", declaration.JE_AGTCode);
				AssertEquals("12345678", declaration.AgentCode);

				var testAgent = OrgHeader.New(Factory);
				testAgent.SetAgentCode(declaration.Branch.Country, "ABCDEFGH");
				declaration.JE_OH_AgentOverride = testAgent.PK;
				AssertEquals(ZString.Empty, declaration.JE_AGTCode);
				AssertEquals("ABCDEFGH", declaration.AgentCode);
			});
		}

		public void TestInvoiceDateDoesNotDefault()
		{
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("JZ_InvoiceDate should default as empty for ZA", ZDateTime.Empty, invoice.JZ_InvoiceDate);
		}

		public void TestJE_OH_AgentOverride_ReadOnly()
		{
			declaration.AgentCode = "ABC";
			declaration.JE_CustomsOffice = "123";
			zauniversalRefTestDataHelper.CreateCustomsStatusCusCodeEntry("6");
			Factory.Save();

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			Assert("Must be readonly if MRN is empty and never sent", !declaration.JE_OH_AgentOverride_ReadOnly);

			entryHeader.CH_Status = Common.ZA.ZAMessageStatusList.Codes.AwaitingResponse;
			Assert("Must be readonly if MRN is empty and awaiting status", declaration.JE_OH_AgentOverride_ReadOnly);

			entryHeader.CH_EntryStatus = "6";
			Assert("Must be editable if MRN is empty and entry status is rejected", !declaration.JE_OH_AgentOverride_ReadOnly);

			entryHeader.MovementReferenceNumberSetter("DBN201608221234567", ZDateTime.Now);
			Assert("Must be readonly if MRN is set", declaration.JE_OH_AgentOverride_ReadOnly);

			var result = CusEntryNumber.LoadOrCreate(declaration, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.SouthAfrica);
			result.CE_EntryIsSystemGenerated = true;
			Assert("Must be readonly if User Generated MRN", declaration.JE_OH_AgentOverride_ReadOnly);
		}

		public void TestSupportingDocumentStatus()
		{
			var instr1 = declaration.CustomsEntryInstructions.AddNew();
			var instr2 = declaration.CustomsEntryInstructions.AddNew();
			var case11 = instr1.CaseNumbers.AddNew();
			var case21 = instr2.CaseNumbers.AddNew();

			case11.Document_Status = DocumentStatusCodes.Codes.PND;
			AssertEquals(DocumentStatusCodes.Codes.PND, declaration.SupportingDocumentStatus);

			case21.Document_Status = DocumentStatusCodes.Codes.SNT;
			AssertEquals(DocumentStatusCodes.Codes.PND, declaration.SupportingDocumentStatus);

			case21.Document_Status = DocumentStatusCodes.Codes.FAL;
			AssertEquals(DocumentStatusCodes.Codes.PND, declaration.SupportingDocumentStatus);

			case11.Document_Status = DocumentStatusCodes.Codes.SNT;
			case21.Document_Status = DocumentStatusCodes.Codes.FAL;
			AssertEquals(DocumentStatusCodes.Codes.FAL, declaration.SupportingDocumentStatus);

			case11.Document_Status = DocumentStatusCodes.Codes.SNT;
			case21.Document_Status = DocumentStatusCodes.Codes.PND;
			AssertEquals(DocumentStatusCodes.Codes.PND, declaration.SupportingDocumentStatus);

			case11.Document_Status = DocumentStatusCodes.Codes.SNT;
			case21.Document_Status = DocumentStatusCodes.Codes.SNT;
			AssertEquals(DocumentStatusCodes.Codes.SNT, declaration.SupportingDocumentStatus);

			case11.Document_Status = DocumentStatusCodes.Codes.FAL;
			case21.Document_Status = DocumentStatusCodes.Codes.FAL;
			AssertEquals(DocumentStatusCodes.Codes.FAL, declaration.SupportingDocumentStatus);

			case11.Document_Status = DocumentStatusCodes.Codes.PND;
			case21.Document_Status = DocumentStatusCodes.Codes.PND;
			AssertEquals(DocumentStatusCodes.Codes.PND, declaration.SupportingDocumentStatus);
		}

		public void TestCaseNumbers()
		{
			var instr1 = declaration.CustomsEntryInstructions.AddNew();
			var instr2 = declaration.CustomsEntryInstructions.AddNew();
			var case11 = instr1.CaseNumbers.AddNew();
			case11.CY_Data = "1234567";
			var case12 = instr1.CaseNumbers.AddNew();
			case12.CY_Data = ZString.Empty;
			var case21 = instr2.CaseNumbers.AddNew();
			case21.CY_Data = "7654321";
			var case22 = instr2.CaseNumbers.AddNew();
			case22.CY_Data = "1111111";

			AssertEquals("1234567,7654321,1111111", declaration.CaseNumbers);
		}

		public void TestCanPrintDA65()
		{
			Assert("Pre-condition", !declaration.CanPrintDA65);
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._75;
			Assert("CanPrintDA65", declaration.CanPrintDA65);
			entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._76;
			Assert("CanPrintDA65", declaration.CanPrintDA65);
			entryInstruction.CEI_Style = UniversalReferenceConstants.ProcedureCodes._40;
			Assert("Cannot PrintDA65", !declaration.CanPrintDA65);
		}

		public override void TestContainersRequiredOnSea()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var containerModes = new ZString[] { Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModes.BreakBulk, Core.Constants.ContainerModes.Liquid };
			foreach (var containerMode in containerModes)
			{
				declaration.JE_ContainerMode = containerMode;
				Assert(ZString.Format("Precondition - Should not show for {0}", containerMode), !declaration.ContainersRequired);
			}

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			Assert("Should show for " + declaration.ContainersRequired, declaration.ContainersRequired);
		}

		public void TestBondedWarehouseProperties()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			var declaration = GetJobDeclaration(JobMessageTypeList.Codes.Import);
			declaration.JE_OH_Importer = helper.Importer.PK;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entryInstruction.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
			AssertBondedWarehouseProperties(declaration, entry, hasInwardInstruction: false, hasOutwardInstruction: false, isInwardBondedWhsEnabled: false, isOutwardBondedWhsEnabled: false, shouldUpdateOutwardLinesWithInventoryDetails: false, isInventorySelectionEnabled: false, entryIsInwardBondedWhsEnabled: false, entryIsOutwardBondedWhsEnabled: false);

			invoiceLine.JI_CEI = entryInstruction.PK;
			AssertBondedWarehouseProperties(declaration, entry, hasInwardInstruction: false, hasOutwardInstruction: false, isInwardBondedWhsEnabled: false, isOutwardBondedWhsEnabled: false, shouldUpdateOutwardLinesWithInventoryDetails: false, isInventorySelectionEnabled: false, entryIsInwardBondedWhsEnabled: false, entryIsOutwardBondedWhsEnabled: false);

			invoiceLine.JI_Procedure = $"{helper.InwardCusProcedure.ZZ6_ProcedureCode}{helper.InwardCusProcedure.ZZ6_PreviousProcedureCode}";
			AssertBondedWarehouseProperties(declaration, entry, hasInwardInstruction: true, hasOutwardInstruction: false, isInwardBondedWhsEnabled: false, isOutwardBondedWhsEnabled: false, shouldUpdateOutwardLinesWithInventoryDetails: false, isInventorySelectionEnabled: false, entryIsInwardBondedWhsEnabled: true, entryIsOutwardBondedWhsEnabled: false);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = helper.Importer.PK;
			AssertBondedWarehouseProperties(declaration, entry, hasInwardInstruction: true, hasOutwardInstruction: false, isInwardBondedWhsEnabled: false, isOutwardBondedWhsEnabled: false, shouldUpdateOutwardLinesWithInventoryDetails: true, isInventorySelectionEnabled: true, entryIsInwardBondedWhsEnabled: true, entryIsOutwardBondedWhsEnabled: false);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			AssertBondedWarehouseProperties(declaration, entry, hasInwardInstruction: true, hasOutwardInstruction: false, isInwardBondedWhsEnabled: false, isOutwardBondedWhsEnabled: false, shouldUpdateOutwardLinesWithInventoryDetails: true, isInventorySelectionEnabled: true, entryIsInwardBondedWhsEnabled: true, entryIsOutwardBondedWhsEnabled: false);

			entryInstruction.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
			AssertBondedWarehouseProperties(declaration, entry, hasInwardInstruction: false, hasOutwardInstruction: false, isInwardBondedWhsEnabled: false, isOutwardBondedWhsEnabled: false, shouldUpdateOutwardLinesWithInventoryDetails: true, isInventorySelectionEnabled: true, entryIsInwardBondedWhsEnabled: false, entryIsOutwardBondedWhsEnabled: false);

			invoiceLine.JI_Procedure = $"{helper.InwardCusProcedure.ZZ6_ProcedureCode}{helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode}";
			AssertBondedWarehouseProperties(declaration, entry, hasInwardInstruction: false, hasOutwardInstruction: true, isInwardBondedWhsEnabled: false, isOutwardBondedWhsEnabled: false, shouldUpdateOutwardLinesWithInventoryDetails: true, isInventorySelectionEnabled: true, entryIsInwardBondedWhsEnabled: false, entryIsOutwardBondedWhsEnabled: true);
		}

		void AssertBondedWarehouseProperties(JobDeclaration declaration, CusEntryHeader entryHeader, bool hasInwardInstruction, bool hasOutwardInstruction, bool isInwardBondedWhsEnabled, bool isOutwardBondedWhsEnabled, bool shouldUpdateOutwardLinesWithInventoryDetails, bool isInventorySelectionEnabled, bool entryIsInwardBondedWhsEnabled, bool entryIsOutwardBondedWhsEnabled)
		{
			AssertEquals("HasInwardEntryInstruction", hasInwardInstruction, declaration.HasInwardEntryInstruction);
			AssertEquals("HasOutwardEntryInstruction", hasOutwardInstruction, declaration.HasOutwardEntryInstruction);
			AssertEquals("IsInwardBondedWarehousingEnabled", isInwardBondedWhsEnabled, declaration.IsInwardBondedWarehousingEnabled);
			AssertEquals("IsOutwardBondedWarehousingEnabled", isOutwardBondedWhsEnabled, declaration.IsOutwardBondedWarehousingEnabled);
			AssertEquals("ShouldUpdateOutwardLinesWithInventoryDetails", shouldUpdateOutwardLinesWithInventoryDetails, declaration.ShouldUpdateOutwardLinesWithInventoryDetails);
			AssertEquals("IsInventorySelectionEnabled", isInventorySelectionEnabled, declaration.IsInventorySelectionEnabled);
			AssertEquals("entry.IsInwardBondedWarehousingEnabled", entryIsInwardBondedWhsEnabled, entryHeader.IsInwardBondedWarehousingEnabled);
			AssertEquals("entry.IsOutwardBondedWarehousingEnabled", entryIsOutwardBondedWhsEnabled, entryHeader.IsOutwardBondedWarehousingEnabled);
		}

		public override void TestContainersRequiredOnNonTransportDeclarationType()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("Pre-condition", expected: true, declaration.ContainersRequired);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			Assert("Should not show", !declaration.ContainersRequired);
		}

		public void TestContainersRequired()
		{
			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Export);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Assert("Should show for Sea and Containerised", declaration.ContainersRequired);

			var containerModes = new ZString[] { Core.Constants.ContainerModes.Liquid, Core.Constants.ContainerModes.Bulk, Core.Constants.ContainerModes.BreakBulk };
			foreach (var containerMode in containerModes)
			{
				declaration.JE_ContainerMode = containerMode;
				Assert($"Should not show for Sea and {containerMode}", !declaration.ContainersRequired);
			}

			var transportModes = new ZString[] { Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Rail };
			foreach (var transportMode in transportModes)
			{
				declaration.JE_TransportMode = transportMode;
				Assert($"Should show for {transportMode}", declaration.ContainersRequired);
			}

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Assert("Should not show for Air", !declaration.ContainersRequired);
		}

		public void TestReciprocalRates()
		{
			Assert(!Factory.New<JobDeclaration>().IsReciprocalRates);
		}

		public void TestDeclarationRefsIsLoadAndDeletedCorrectly()
		{
			var ref1 = Factory.New<JobDecRefs>();
			ref1.J3_JE = declaration.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedDec = newFactory.Load<JobDeclaration>(declaration.PK);
			loadedDec.LoadChildEditableObjects();
			AssertEquals("Db hits", 1, newFactory.GetTableHitCount(JobDecRefs.Schema.TableName));
			AssertEquals(1, loadedDec.DeclarationRefs.Count);
			AssertEquals("db hits", 1, newFactory.GetTableHitCount(JobDecRefs.Schema.TableName));
			newFactory = new BusinessObjectFactory();
			loadedDec = newFactory.Load<JobDeclaration>(declaration.PK);
			loadedDec.Delete();
			newFactory.Save();
			AssertEquals("ref1.IsDeleted", expected: true, ref1.IsDeleted);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals(Enterprise.Core.Constants.CurrencyCodes.SouthAfrica, GetJobDeclarationForTesting().LocalCurrencyCodeCoreExposed);
		}

		public void TestDisableAmendmentMessageDetection()
		{
			Assert("Amendment detection is disabled. No messaging capability exists", !((IMessageManageableBizObj)declaration).IsInAStatusAmendmentSendable);
		}

		public void TestJE_ContainerMode()
		{
			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Export);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.Empty;
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;

			AssertNoNotifications(container1.CO_FCL_LCL_AIRInfo);
			AssertNoNotifications(container2.CO_FCL_LCL_AIRInfo);
			AssertHasError(container3.CO_FCL_LCL_AIRInfo, "Modes for all containers need to match, or be EMP");

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertNoNotifications(container1.CO_FCL_LCL_AIRInfo);
			AssertNoNotifications(container2.CO_FCL_LCL_AIRInfo);
			AssertNoNotifications(container3.CO_FCL_LCL_AIRInfo);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertHasError(container1.CO_FCL_LCL_AIRInfo, "Modes for all containers need to match, or be EMP");
			AssertNoNotifications(container2.CO_FCL_LCL_AIRInfo);
			AssertHasError(container3.CO_FCL_LCL_AIRInfo, "Modes for all containers need to match, or be EMP");
		}

		public void TestCreditorPK()
		{
			zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("CTN");

			var creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.OH_IsCreditor = true;
			var creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			creditor2.OH_IsCreditor = true;
			var defaultCreditor = Factory.NewWithValidTestData<OrgHeader>();
			defaultCreditor.OH_IsCreditor = true;
			var agent1 = Factory.NewWithValidTestData<OrgHeader>();
			agent1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ABCDEFG", Core.Constants.CountryCodes.SouthAfrica);
			var agent2 = Factory.NewWithValidTestData<OrgHeader>();
			agent2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "12345678", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var agentOfficeToCreditorMappings = new FinancialAccountNumberPortMapCollection();
			var mapping1 = GetFinancialAccountNumberPortMap(agent1, "BFN", "1234567890", importerPays: false, 1);
			mapping1.CreditorPK = creditor1.PK;
			agentOfficeToCreditorMappings.Add(mapping1);
			var mapping2 = GetFinancialAccountNumberPortMap(agent2, "CTN", "1234567890", importerPays: false, 1);
			mapping2.CreditorPK = creditor2.PK;
			agentOfficeToCreditorMappings.Add(mapping2);
			Factory.SuspendValidation();
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, agentOfficeToCreditorMappings);

			var agent3 = Factory.NewWithValidTestData<OrgHeader>();
			agent3.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "QWERTYUI", Core.Constants.CountryCodes.SouthAfrica);
			Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultCreditor.PK.ToGuid());

			var declaration = GetJobDeclaration(JobMessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_CustomsOffice = "BFN";
			declaration.JE_OH_AgentOverride = agent3.PK;

			var customsJobInfo = declaration as ICustomsJobInfo;
			AssertEquals("Default Creditor", defaultCreditor.PK, customsJobInfo.CreditorPK);

			declaration.JE_OH_AgentOverride = agent1.PK;
			AssertEquals("Agent 1's creditor", creditor1.PK, customsJobInfo.CreditorPK);

			declaration.JE_CustomsOffice = "CTN";
			AssertEquals("Default Creditor", defaultCreditor.PK, customsJobInfo.CreditorPK);

			declaration.JE_OH_AgentOverride = agent2.PK;
			AssertEquals("Agent 2's creditor", creditor2.PK, customsJobInfo.CreditorPK);

			declaration.JE_CustomsOffice = "BFN";
			declaration.JE_AGTCode = "QWERTYUI";
			AssertEquals("Default Creditor", defaultCreditor.PK, customsJobInfo.CreditorPK);

			declaration.JE_AGTCode = "ABCDEFG";
			AssertEquals("Agent 1's creditor", creditor1.PK, customsJobInfo.CreditorPK);
		}

		public void TestCreditorPK_Legacy()
		{
			zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("JHB");

			var testAgentForFAN = Factory.NewWithValidTestData<OrgHeader>();
			testAgentForFAN.CustomsCodes.AddNew("CDP", "51051342", "ZA");
			testAgentForFAN.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
			var testCreditorForFAN = Factory.NewWithValidTestData<OrgHeader>();
			testCreditorForFAN.OH_IsCreditor = true;
			Factory.Save();

			var agentOfficeToCreditorMappings = new FinancialAccountNumberPortMapCollection();
			var mapping = GetFinancialAccountNumberPortMap(testAgentForFAN, "JHB", "1234567890", importerPays: false, 1);
			mapping.CreditorPK = testCreditorForFAN.PK;
			agentOfficeToCreditorMappings.Add(mapping);
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, agentOfficeToCreditorMappings);

			var buyer = OrgHeader.New(Factory);
			buyer.FillWithValidTestData();
			var collection = new CustomsDSBCreditorOverrideCollection();
			var creditor = collection.AddNew();
			var organisation = GetOrgHeader();
			creditor.DistrictOfficeCode = "BFN";
			creditor.CreditorPK = organisation.PK;
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			Enterprise.Customs.ZA.DataRegistry.Business.ZACustomsRegistry.Instance.DSBCreditors.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, organisation2.PK.ToGuid());

			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Import);
			declaration.JE_OH_Importer = buyer.PK;
			declaration.ActiveEntryHeaders.AddNew();
			declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_MergeBy = "NON";
			declaration.JE_CustomsOffice = "BFN";
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			ICustomsChargeEntry entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(ZGuid.Empty, entry.CreditorPK);

			declaration.JE_CustomsOffice = "BBR";
			AssertEquals("fallback to default one", ZGuid.Empty, entry.CreditorPK);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "JHB";
			declaration.JE_OH_AgentOverride = testAgentForFAN.PK;
			entry = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals(testCreditorForFAN.PK, entry.CreditorPK);
		}

		public override void TestMasterBillLabel()
		{
			var databoundBO = new DataBoundBusinessObject(declaration);
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, databoundBO);
			AssertEquals("Sea Master Bill Label", "Bill of Lading", resourceStringData.Caption);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, databoundBO);
			AssertEquals("Air Master Bill Label", "Air Waybill", resourceStringData.Caption);

			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, databoundBO);
			AssertEquals("Road Master Bill Label", "Road Manifest", resourceStringData.Caption);

			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, databoundBO);
			AssertEquals("Rail Master Bill Label", "Rail Consignment Note", resourceStringData.Caption);

			declaration.JE_TransportMode = declaration.TransportModeMailCodeForTesting;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, databoundBO);
			AssertEquals("Post Master Bill Label", "Parcel Advice No.", resourceStringData.Caption);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Storage;
			resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_MasterBillInfo, databoundBO);
			AssertEquals("Other Master Bill Label", "Document No.", resourceStringData.Caption);
		}

		public void TestJE_VoyageFlightNoCaption()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = Core.Constants.TransportModes.Road;
			var info = dec.JE_VoyageFlightNoInfo;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Road", "Vehicle Reg No", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Vehicle Reg No", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Sea", "Voyage", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Air", "Flight/Folio", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Flight/Folio", mediumCaption: "Flight No.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Rail", "Voyage / Flight No", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage / Flight No", shortCaption: "Voyage/Flight", fullDescription: "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
		}

		public void TestGetNewBondedWarehouseTransaction()
		{
			AssertEquals("Correct type", typeof(BondedWarehouseTransaction), ((JobDeclaration)GetNewBusinessObject()).GetNewBondedWarehouseTransactionForTesting().GetType());
		}

		public void TestBusinessObjectsWithRelatedEvents_ZA()
		{
			AssertEquals("Should be No BOs with related logs", 0, declaration.BusinessObjectsWithRelatedEvents.Length);
			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Should be 1", 1, declaration.BusinessObjectsWithRelatedEvents.Length);
		}

		public void TestJE_LocationOfGoods()
		{
			zauniversalRefTestDataHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			zauniversalRefTestDataHelper.CreateZACusCodeListEntry(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "49", "49 DESC");
			zauniversalRefTestDataHelper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			zauniversalRefTestDataHelper.CreateZACusCodeListEntry(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "6", "6 DESC");
			Factory.Save();

			declaration.JE_LocationOfGoods = "49";
			AssertEquals("49 DESC", ((IFindBoxListProvider)declaration.Lookups.LocationOfGoodsCollection).DescriptionFromCode(declaration.JE_LocationOfGoods));

			declaration.JE_LocationOfGoods = "6";
			AssertNull(((IFindBoxListProvider)declaration.Lookups.LocationOfGoodsCollection).DescriptionFromCode(declaration.JE_LocationOfGoods));
		}

		public void TestJE_OH_AgentOverrideDefaulting()
		{
			zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			var buyer = CreateOrgHeader("buyer", isConsignee: true, "IMP#@$43");
			buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var collection = new FinancialAccountNumberPortMapCollection();
			collection.Add(GetFinancialAccountNumberPortMap(buyer, "JHB", "3234002346", importerPays: true, 1));
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var declaration = GetJobDeclaration(JobMessageTypeList.Codes.Import);
			AssertEquals(GlbCompany.CurrentCompany.GC_OH_OrgProxy, declaration.JE_OH_AgentOverride);
			declaration.JE_OH_AgentOverride = ZGuid.Empty;
			declaration.JE_OH_Importer = buyer.PK;
			AssertEquals(GlbCompany.CurrentCompany.GC_OH_OrgProxy, declaration.JE_OH_AgentOverride);
			declaration.JE_OH_AgentOverride = ZGuid.Empty;
			declaration.JE_CustomsOffice = "JHB";
			AssertEquals(buyer.PK, declaration.JE_OH_AgentOverride);
			declaration.JE_CustomsOffice = "B%N";
			AssertEquals(GlbCompany.CurrentCompany.GC_OH_OrgProxy, declaration.JE_OH_AgentOverride);
		}

		public void TestJE_VATClaimBackIndicatorDefaulting()
		{
			AssertVATClaimBackIndicatorDefaultingDefaultForImporterOrSupplier();
			AssertVATClaimBackIndicatorDefaultingRefreshOnChangeOfMessageType();
		}

		void AssertVATClaimBackIndicatorDefaultingDefaultForImporterOrSupplier()
		{
			CombineAssertions("VATClaimBackIndicatorDefaulting - Default for Importer or Supplier", () =>
			{
				var buyer = CreateOrgHeader("buyer", isConsignee: true, "IMP#@$43");
				buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "4770181941", Core.Constants.CountryCodes.SouthAfrica);
				Factory.Save();

				var messageTypes = new string[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.ExWarehouse, JobMessageTypeList.Codes.MiscellaneousCustoms, JobMessageTypeList.Codes.Export };
				foreach (string messageType in messageTypes)
				{
					declaration.JE_OH_Importer = Guid.Empty;
					buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "4770181941", Core.Constants.CountryCodes.SouthAfrica);
					declaration.JE_MessageType = messageType;
					declaration.JE_VATClaimBackIndicator = ZString.Empty;
					declaration.JE_OH_Importer = messageType == JobMessageTypeList.Codes.Export ? ZGuid.Empty : buyer.PK;
					declaration.JE_OH_Supplier = messageType == JobMessageTypeList.Codes.Export ? buyer.PK : ZGuid.Empty;
					AssertEquals("Y", declaration.JE_VATClaimBackIndicator);
					declaration.JE_OH_Importer = ZGuid.Empty;
					declaration.JE_OH_Supplier = ZGuid.Empty;
					declaration.JE_VATClaimBackIndicator = ZString.Empty;
					AssertEquals("", declaration.JE_VATClaimBackIndicator);
					buyer.CustomsCodes.Remove(buyer.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.SouthAfrica));
					declaration.JE_OH_Importer = messageType == JobMessageTypeList.Codes.Export ? ZGuid.Empty : buyer.PK;
					declaration.JE_OH_Supplier = messageType == JobMessageTypeList.Codes.Export ? buyer.PK : ZGuid.Empty;
					AssertEquals("N", declaration.JE_VATClaimBackIndicator);
				}
			});
		}

		void AssertVATClaimBackIndicatorDefaultingRefreshOnChangeOfMessageType()
		{
			CombineAssertions("VATClaimBackIndicatorDefaulting - Refresh on change of messagetype", () =>
			{
				var agent1 = OrgHeader.New(Factory);
				agent1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "111");
				var agent2 = OrgHeader.New(Factory);
				var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Export);
				declaration.JE_OH_Importer = agent1.PK;
				declaration.JE_OH_Supplier = agent2.PK;
				AssertEquals(declaration.JE_VATClaimBackIndicator, "N");
				declaration.JE_OH_Supplier = agent1.PK;
				AssertEquals(declaration.JE_VATClaimBackIndicator, "Y");

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = agent2.PK;
				AssertEquals(declaration.JE_VATClaimBackIndicator, "N");
				declaration.JE_OH_Importer = agent1.PK;
				AssertEquals(declaration.JE_VATClaimBackIndicator, "Y");
				declaration.JE_OH_Importer = agent2.PK;
				AssertEquals(declaration.JE_VATClaimBackIndicator, "N");

				declaration.JE_MessageType = "EXB";
				AssertEquals(declaration.JE_VATClaimBackIndicator, "N");
				declaration.JE_OH_Importer = agent1.PK;
				AssertEquals(declaration.JE_VATClaimBackIndicator, "Y");
			});
		}

		public void TestAgentName()
		{
			zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			Factory.Save();

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_FullName = "buyer";
			buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var collection = new FinancialAccountNumberPortMapCollection();
			var creditor = GetFinancialAccountNumberPortMap(buyer, "BFN", "3234002346", importerPays: false, 1);
			collection.Add(creditor);
			var organisation = GetOrgHeader();
			organisation.OH_FullName = "test org";
			creditor.CreditorPK = organisation.PK;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals(declaration.Branch.Company.OrgProxy.OH_FullName, declaration.AgentName);
			var agentOrg = CreateOrgHeader("AgentName", "AGENTCODE");
			declaration.JE_OH_AgentOverride = agentOrg.PK;
			AssertEquals("AgentName", declaration.AgentName);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = buyer.PK;
			declaration.JE_CustomsOffice = "BFN";
			AssertEquals("buyer", declaration.AgentName);
			AssertEquals("ASBSD", declaration.AgentCode);
		}

		public void TestJZ_RX_NKInvoice_Currency()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			Assert(declaration.Invoices.All(x => x.JZ_RX_NKInvoice_Currency == Core.Constants.CurrencyCodes.SouthAfrica));
		}

		public void TestSupportsBondedWarehousing()
		{
			var importer = OrgHeader.New(Factory);
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("SupportsBondedWarehousing", expected: false, declaration.SupportsBondedWarehousing);
			AssertEquals("SupportMultipleWarehouseEntry", expected: true, declaration.SupportMultipleWarehouseEntry);
		}

		public void TestHouseBillsCollectionIsOfRightType()
		{
			AssertEquals(typeof(BillCollection<Bill, JobDeclaration>), declaration.Bills.GetType());
		}

		public override void TestILandedCostHeader()
		{
			ZAUniversalReferenceTestDataHelper.SetupBasicTariffTypesForZATesting(Factory);

			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader1Line = entryHeader1.MergedLines.AddNew();
			var entryHeader1LineDuty = entryHeader1Line.Fees.AddOrUpdate("1P1", 100m);
			var entryHeader1LineDutySch1P2B = entryHeader1Line.Fees.AddOrUpdate("12B", 120m);
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			var entryHeader2Line = entryHeader2.MergedLines.AddNew();
			var entryHeader2LineDuty = entryHeader2Line.Fees.AddOrUpdate("1P1", 200m);
			var entryHeader2LineDutySch1P2B = entryHeader2Line.Fees.AddOrUpdate("12B", 220m);

			var total = ((ILandedCostHeader)declaration).TotalDutyTaxEntryFeeItems;
			AssertILandedCostHeader(total);

			entryHeader1LineDuty.CF_IsLandedCostOnly = true;
			entryHeader1LineDutySch1P2B.CF_IsLandedCostOnly = true;
			entryHeader2LineDuty.CF_IsLandedCostOnly = true;
			entryHeader2LineDutySch1P2B.CF_IsLandedCostOnly = true;
			total = ((ILandedCostHeader)declaration).TotalDutyTaxEntryFeeItems;
			AssertILandedCostHeader(total);
		}

		void AssertILandedCostHeader(DutyTaxEntryFee entryFee)
		{
			AssertEquals("ILandedCostHeader.TotalEntryFee", 0m, entryFee["ENT"]);
			AssertEquals("ILandedCostHeader.TotalDuty", 640m, entryFee["TDT"]);
			AssertEquals("ILandedCostHeader.TotalExcise", 0m, entryFee["EXC"]);
			AssertEquals("ILandedCostHeader.TotalSpecialTax1", 340m, entryFee["ST1"]);
			AssertEquals("ILandedCostHeader.TotalSpecialTax2", 0m, entryFee["ST2"]);
			AssertEquals("ILandedCostHeader.TotalSpecialTax3", 0m, entryFee["ST3"]);
			AssertEquals("ILandedCostHeader.TotalOtherDutyOrFlatDuty", 0m, entryFee["OTH"]);
		}

		public void TestLookupObjectIsCached()
		{
			var bizO = (JobDeclaration)GetNewBusinessObject();
			var firstLookup = bizO.Lookups;
			var secondLookup = bizO.Lookups;
			AssertSame(secondLookup, firstLookup);
		}

		public void TestIsPackingInformationRelevant()
		{
			Assert(!declaration.IsPackingInformationRelevant);
		}

		[ExpectNoExceptions]
		public override void TestHouseBillProxiedThroughHouseBillsFirstElement()
		{
		}

		[ExpectNoExceptions]
		public override void TestFirstArrivalVoyageDestination()
		{
			//Port of First Arrival cannot be set in ZA
		}

		public void TestHouseBillProxiesToCusDecHouseBill()
		{
			declaration.JE_HouseBill = "123";
			AssertEquals("123", declaration.Bills[0].CU_HouseBill);
		}

		public void TestHouseBillIssueDateProxiesToCusDecHouseBill()
		{
			declaration.HouseBillIssuedDate = new ZDateTime(2004, 1, 1);
			AssertEquals(new ZDateTime(2004, 1, 1), declaration.Bills[0].CU_IssueDate);
		}

		public override void TestBondedWarehouseEditable()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals(expected: true, declaration.BondedWarehouseEditable);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals(expected: true, declaration.BondedWarehouseEditable);
		}

		public void TestExampleEntryFromRohligZA()
		{
			var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			new TestHelper(Factory).SetExchangeRate(uSDCurrency, 0.1653m, new ZDateTime(2004, 11, 25));

			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Import);
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2004, 11, 25);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 9069888m;
			invoice.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			invoice.JZ_IncoTerm = "FOB";
			invoice.GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 5164m, uSDCurrency.RX_Code);
			invoice.GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.AdditionCharge, 25m, uSDCurrency.RX_Code);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 9069888m;
			invoiceLine.JI_Tariff = "7108.12.00 6";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			declaration.ResumeApportionment();
			AssertEquals("FOB value - invoice", 9069913m, invoice.JZ_Calc_FOBAmount);
			AssertEquals("FOB value - invoice line", 9069913.00m, invoiceLine.JI_Calc_FOB);
		}

		public void TestDateOfValuation()
		{
			using var func = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: false);

			var today = ZDate.Today;
			var yesterday = today.AddDays(-1);
			var testDate2 = yesterday.AddDays(-1);
			var testDate = testDate2.AddDays(-1);
			CombineAssertions("Valuation Date", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				AssertEquals("Import without Date", ZDateTime.Empty, declaration.DateOfValuation);
				declaration.JE_MasterBillIssuedDate = testDate;
				AssertEquals("Import with Date", testDate, declaration.DateOfValuation);
				declaration.HouseBillIssuedDate = testDate2;
				AssertEquals("Import with HouseBill Date", testDate, declaration.DateOfValuation);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				AssertEquals("Export", yesterday, declaration.DateOfValuation);
				declaration.JE_ValuationDate = testDate;
				AssertEquals("Export with set ValuationDate", yesterday, declaration.DateOfValuation);
			});
		}

		public void TestDateOfValuation_ZAINVDET()
		{
			using var func = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ZAAddInvoiceDetailsToCUSDECMessage, GlbCompany.CurrentCompany.Country.Code, ZDateTime.Today, value: true);
			var today = ZDate.Today;
			var yesterday = today.AddDays(-1);
			var testDate2 = yesterday.AddDays(-1);
			var testDate = testDate2.AddDays(-1);
			CombineAssertions("Valuation Date", () =>
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				AssertEquals("Import without Date", ZDateTime.Empty, declaration.DateOfValuation);
				declaration.JE_MasterBillIssuedDate = testDate;
				AssertEquals("Import with Date", testDate, declaration.DateOfValuation);
				declaration.HouseBillIssuedDate = testDate2;
				AssertEquals("Import with HouseBill Date", testDate2, declaration.DateOfValuation);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
				AssertEquals("Export", yesterday, declaration.DateOfValuation);
				declaration.JE_ValuationDate = testDate;
				AssertEquals("Export with set ValuationDate", testDate, declaration.DateOfValuation);
			});
		}

		public void TestLookups()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals(typeof(ExportJobDeclarationLookups), declaration.Lookups.GetType());
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals(typeof(ImportJobDeclarationLookups), declaration.Lookups.GetType());
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals(typeof(ExportJobDeclarationLookups), declaration.Lookups.GetType());
		}

		public void TestIsImportForExWarehouseEntry()
		{
			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.ExBond);
			AssertEquals(expected: true, declaration.IsImport);
		}

		public void TestReferenceNumber()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			var subShipment = Factory.New<ForwardingShipment>();
			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			declaration.JE_JS = masterShipment.PK;
			Factory.Save();

			AssertEquals("Master Shipment's reference number is used for declaration", masterShipment.JS_UniqueConsignRef, declaration.JE_DeclarationReference);
		}

		public void TestDeriveExportDeclarationStatusNotSent()
		{
			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Export);
			declaration.DeriveDeclarationStatus();
			AssertEquals("EntryStatus", ZAMessageStatusList.Codes.NotSent, declaration.JE_EntryStatus);
		}

		public void TestDeriveExportDeclarationStatusMixed()
		{
			var declaration = CreateTwoEntryExportDeclaration();
			declaration.CustomsEntryHeaders[0].CH_Status = "2";
			declaration.CustomsEntryHeaders[1].CH_Status = "1";
			declaration.DeriveDeclarationStatus();
			AssertEquals("EntryStatus", "", declaration.JE_EntryStatus);
		}

		public void TestDeriveExportDeclarationStatusUnknown()
		{
			var declaration = CreateTwoEntryExportDeclaration();
			declaration.CustomsEntryHeaders[0].CH_Status = "53";
			declaration.CustomsEntryHeaders[1].CH_Status = "54";
			declaration.DeriveDeclarationStatus();
			AssertEquals("EntryStatus", "", declaration.JE_EntryStatus);
		}

		public void TestSeaTransportDocumentNumber()
		{
			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Export);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "MBL";
			declaration.JE_CarrierCode = "MEA";
			AssertEquals("Export TransportDocumentNumber", "MEA MBL", declaration.TransportDocumentNumber);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals("Import TransportDocumentNumber", "MEA MBL", declaration.TransportDocumentNumber);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			AssertEquals("ExBond TransportDocumentNumber", "MBL", declaration.TransportDocumentNumber);
		}

		public void TestAirTransportDocumentNumber()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = "08111111111";
			declaration.JE_CarrierCode = "MEA";
			AssertEquals("TransportDocumentNumber", "081-11111111", declaration.TransportDocumentNumber);
		}

		public void TestAgentCode()
		{
			var declaration = GetJobDeclaration(JobMessageTypeList.Codes.Import);
			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			var orgProxy = declaration.Branch.Company.OrgProxy;
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ORG45344", Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals("ORG45344", declaration.AgentCode);
			var agentOrg = CreateOrgHeader("AgentName", "AGENTCODE");
			agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AGT7/634", Core.Constants.CountryCodes.SouthAfrica);
			declaration.JE_OH_AgentOverride = agentOrg.PK;
			AssertEquals("AGT7", declaration.AgentCode);
			AssertEquals(expected: true, declaration.AgentCodeInfo.ReadOnly);
			declaration.AgentCode = "KSDJH23/343";
			AssertEquals("KSDJH23", declaration.AgentCode);
		}

		public void TestAgentDualProfileCode()
		{
			declaration.Branch.Company.OrgProxy.SetAgentCode(declaration.Branch.Company.Country, "1234");
			AssertEquals("Test 1", ZString.Empty, declaration.AgentDualProfileCode);
			declaration.Branch.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "TAU", "AU");
			AssertEquals("Test 2", ZString.Empty, declaration.AgentDualProfileCode);
			declaration.Branch.Company.OrgProxy.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "TZA", "ZA");
			AssertEquals("Test 3", "TZA", declaration.AgentDualProfileCode);

			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.SetAgentCode(declaration.Branch.Company.Country, "9587");
			newOrg.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "TNO", "ZA");
			AssertEquals("Getting AgentCode From AgentOverride", "1234", declaration.AgentCode);
			AssertEquals("Getting DualProfileCode From AgentOverride", "TZA", declaration.AgentDualProfileCode);
			declaration.JE_OH_AgentOverride = ZGuid.Empty;
			declaration.AgentCode = "9587";
			AssertEquals("Getting AgentCode From DeclarationRef", "9587", declaration.AgentCode);
			AssertEquals("Getting DualProfileCode From AgentCodeRelated OrgHeader", "TNO", declaration.AgentDualProfileCode);

			declaration.AgentCode = ZString.Empty;
			declaration.Branch.Company.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			var orgWithDuplicateAgentCode = Factory.NewWithValidTestData<OrgHeader>();
			orgWithDuplicateAgentCode.SetAgentCode(declaration.Branch.Company.Country, "1234");
			orgWithDuplicateAgentCode.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "ABC", "ZA");
			declaration.JE_OH_AgentOverride = orgWithDuplicateAgentCode.PK;
			AssertEquals("Getting DualProfileCode From Correct Agent", "ABC", declaration.AgentDualProfileCode);
		}

		public void TestRoadTransportDocumentNumber()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.JE_MasterBill = "08111111111";
			declaration.JE_CarrierCode = "MEA";
			AssertEquals("TransportDocumentNumber", "08111111111", declaration.TransportDocumentNumber);
		}

		public void TestTransportDocumentNumber()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MasterBill = "08351925016";
			declaration.JE_CarrierCode = "MEA";
			AssertEquals("Master Bill number", "MEA 08351925016", declaration.TransportDocumentNumber);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_MasterBill = "083";
			AssertEquals("Master Bill number", "083-", declaration.TransportDocumentNumber);

			declaration.JE_MasterBill = "0835";
			AssertEquals("Master Bill number", "083-5", declaration.TransportDocumentNumber);

			declaration.JE_MasterBill = "08351925016";
			AssertEquals("Master Bill number", "083-51925016", declaration.TransportDocumentNumber);
		}

		public void TestDefaultRadioCallSignFromSystemDataFirstWithOutDuplicateVesselNames()
		{
			var result = SetupDefaultRadioCallSignFromSystemData(ZAJobMessageTypeList.Codes.Export);
			result.declaration.JE_VesselName = result.overrideVessel.RV_Code;
			AssertEquals("AAAA", result.declaration.JE_RadioCallSign);
			result.declaration.JE_VesselName = result.customVessel.RV_Code;
			AssertEquals("CCCC", result.declaration.JE_RadioCallSign);
		}

		public void TestDefaultRadioCallSignFromSystemDataFirstWithDuplicateVesselNames()
		{
			var result = SetupDefaultRadioCallSignFromSystemData(ZAJobMessageTypeList.Codes.Export);
			var systemVessel2nd = Factory.NewWithValidTestData<RefVesselZZ>();
			systemVessel2nd.ZZO_Code = result.systemVessel.ZZO_Code;
			systemVessel2nd.ZZO_RadioCallSign = "DDDD";
			result.declaration.JE_VesselName = result.overrideVessel.RV_Code;
			AssertEquals(ZString.Empty, result.declaration.JE_RadioCallSign);
			result.declaration.JE_VesselName = result.customVessel.RV_Code;
			AssertEquals("CCCC", result.declaration.JE_RadioCallSign);
		}

		public void TestRadioCallSignWithEmptyVesselName()
		{
			var result = SetupDefaultRadioCallSignFromSystemData(ZAJobMessageTypeList.Codes.Export);
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Black Pearl";
			vessel.RV_RadioCallSign = "parley!";
			result.declaration.JE_VesselName = vessel.RV_Name;
			AssertEquals(vessel.RV_RadioCallSign, result.declaration.JE_RadioCallSign);
			result.declaration.JE_VesselName = ZString.Empty;
			AssertEquals(ZString.Empty, result.declaration.JE_RadioCallSign);
		}

		(JobDeclaration declaration, RefVesselZZ systemVessel, RefVessel overrideVessel, RefVessel customVessel) SetupDefaultRadioCallSignFromSystemData(string messageType)
		{
			zauniversalRefTestDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var declaration = GetJobDeclaration(messageType);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var systemVessel = Factory.NewWithValidTestData<RefVesselZZ>();
			systemVessel.ZZO_RadioCallSign = "AAAA";

			var overrideVessel = Factory.NewWithValidTestData<RefVessel>();
			overrideVessel.RV_Code = systemVessel.ZZO_Code;
			overrideVessel.RV_RadioCallSign = "BBBB";

			var customVessel = Factory.NewWithValidTestData<RefVessel>();
			customVessel.RV_RadioCallSign = "CCCC";
			Factory.Save();

			return (declaration, systemVessel, overrideVessel, customVessel);
		}

		public void TestDefaultCarrierCodeFromSystemDataFirst()
		{
			zauniversalRefTestDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Export);
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var systemVessel = Factory.NewWithValidTestData<RefVesselZZ>();
			var carrier = Factory.NewWithValidTestData<RefCarrierCode>();
			carrier.ZZ4_Code = "AAAA";
			carrier.ZZ4_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;

			var pivot = Factory.New<RefCarrierVesselPivot>();
			pivot.ZZQ_ZZO = systemVessel.PK;
			pivot.ZZQ_ZZ4 = carrier.PK;
			Factory.Save();

			var overrideVessel = Factory.NewWithValidTestData<RefVessel>();
			overrideVessel.RV_Code = systemVessel.ZZO_Code;
			overrideVessel.RV_CarrierCode = "BBBB";

			var customVessel = Factory.NewWithValidTestData<RefVessel>();
			customVessel.RV_CarrierCode = "CCCC";
			Factory.Save();

			declaration.JE_VesselName = overrideVessel.RV_Code;
			AssertEquals("AAAA", declaration.JE_Carrier);

			declaration.JE_VesselName = customVessel.RV_Code;
			AssertEquals("CCCC", declaration.JE_Carrier);
		}

		public void TestSettingPackagesClearsPackagesOnEntries()
		{
			declaration.CustomsEntryHeaders.AddNew().CH_Packages = 123;
			AssertEquals("Precondition", 123, declaration.CustomsEntryHeaders[0].CH_Packages);
			declaration.CustomsEntryHeaders.AddNew().CH_Packages = 124;
			AssertEquals("Precondition", 124, declaration.CustomsEntryHeaders[1].CH_Packages);
			declaration.JE_TotalNoOfPacks = 5;
			AssertEquals(0, declaration.CustomsEntryHeaders[0].CH_Packages);
			AssertEquals(0, declaration.CustomsEntryHeaders[1].CH_Packages);
		}

		public void TestWhenSavingFactoryVoyageFlightNoWillBeEmptyForRAIL()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "123";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			Factory.Save();
			AssertEquals(ZString.Empty, declaration.JE_VoyageFlightNo);
		}

		public void TestEntryStatusUpdateOnFactorySaving()
		{
			var header = declaration.CustomsEntryHeaders.AddNew();
			header.CH_EntryStatus = "1";
			Factory.Save();
			AssertEquals(header.CH_EntryStatus, declaration.JE_EntryStatus);
			declaration.JE_EntryStatus = "1";
			Factory.Save();
			AssertEquals("1", declaration.JE_EntryStatus);
			AssertNotEquals(header.CH_Status, declaration.JE_EntryStatus);
		}

		public void TestSuppliersWithThresholdOrGreater()
		{
			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Import);
			AssertEquals("Precondition : no entries", 0, declaration.SuppliersRequiringROOCertificate.Length);
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_OH_Supplier = OrgHeader.New(Factory).PK;
			var invoiceHeader2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader2.JZ_OH_Supplier = OrgHeader.New(Factory).PK;
			invoiceHeader.JZ_InvoiceAmount = SupplierROOCalculator.MinimumRandForDeclaration;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			AssertEquals("Matching supplier count", 1, declaration.SuppliersRequiringROOCertificate.Length);
			AssertEquals("Supplier", invoiceHeader.JZ_OH_Supplier, declaration.SuppliersRequiringROOCertificate[0].PK);
		}

		public void TestJE_EntryStatusDescription()
		{
			zauniversalRefTestDataHelper.CreateCustomsStatusCusCodeEntry("1");
			zauniversalRefTestDataHelper.CreateCustomsStatusCusCodeEntry("2");
			Factory.Save();

			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Import);
			AssertEquals(ZString.Empty, declaration.JE_EntryStatusDescription);//TODO future WI to modify JE_EntryStatus

			var header0 = declaration.ActiveEntryHeaders.AddNew();
			header0.CH_EntryStatus = "1";
			AssertEquals("Release", declaration.JE_EntryStatusDescription);

			header0.CH_EntryStatus = "XXX";
			AssertEquals("Unknown", declaration.JE_EntryStatusDescription);

			var header1 = declaration.ActiveEntryHeaders.AddNew();
			header1.CH_EntryStatus = "2";
			AssertEquals("Multiple - See Entries", declaration.JE_EntryStatusDescription);
		}

		public void TestMergedDeclarationSetsJI_ActualPrice()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, Core.Constants.CurrencyCodes.SouthAfrica));
			currency.ExchangeRates.DeleteAll();
			var rate = CreateRefExhangeRate("CUS", 0.70m);
			currency.ExchangeRates.Add(rate);

			var declaration = GetJobDeclaration(JobMessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_CustomsOffice = "JHB";
			declaration.AgentCode = "ASBSD";

			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 12, 1);
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currency.RX_Code;
			invoiceHeader.JZ_InvoiceCurrExRate = 0.7m;
			Factory.Save();

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			AssertEquals(0m, invoiceLine1.JI_ActualPrice);

			declaration.DoMerge();
			AssertEquals(1428.57m, invoiceLine1.JI_ActualPrice);
		}

		public override void TestMergeMethodThatTakesISendsMessageToCustoms()
		{
			Assert(true);
		}

		public override void TestEntryStatusChangedLogged()
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = "FLO";
			Factory.Save();
			AssertEquals("Precondition", "", declaration.JE_EntryStatus);
			entryHeader.CH_Status = "1";
			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			filter.AddToFilter(StmALogSchema.SL_Parent, declaration.PK);
			AssertEquals("Event Created", 0, Factory.GetDatabaseCount(typeof(StmALog), filter));
		}

		public void TestEntryDetailsInARInvoice()
		{
			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Import);
			declaration.JE_AgentsReference = "TEST";

			const string expectedTemplate = "<ExpandToFit><B>MRN:</B> {0}<BR><B>LRN:</B> {1}<BR><B>{2}:</B> {3}";
			AssertEquals(string.Format(expectedTemplate, "", "", "ICN", ""), declaration.EntryDetailsInARInvoice);

			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_BGMReference = "BGM1";
			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			entry2.CH_BGMReference = "BGM2";
			AddCusEntryNumber(entry1, CusEntryNumberTypes.Standard.MovementReferenceNumber, "MRN1");
			AddCusEntryNumber(entry2, CusEntryNumberTypes.Standard.MovementReferenceNumber, "MRN2");
			AddCusEntryNumber(entry1, CusEntryNumberTypes.Standard.ImportControlNumber, "ICN1");
			AddCusEntryNumber(entry2, CusEntryNumberTypes.Standard.ImportControlNumber, "ICN2");
			AddCusEntryNumber(entry1, CusEntryNumberTypes.Standard.UniqueConsignementReference, "UCR1");
			AddCusEntryNumber(entry2, CusEntryNumberTypes.Standard.UniqueConsignementReference, "UCR2");
			AssertEquals(string.Format(expectedTemplate, "MRN1, MRN2", "BGM1, BGM2", "ICN", "ICN1, ICN2"), declaration.EntryDetailsInARInvoice);

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			AssertEquals(string.Format(expectedTemplate, "MRN1, MRN2", "BGM1, BGM2", "UCR", "UCR1, UCR2"), declaration.EntryDetailsInARInvoice);
		}

		public void TestSetDefaultValues()
		{
			var bbrCode = zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			var jhbCode = zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			var longCode = zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("ACD");
			Factory.Save();

			using (ZACustomsRegistry.Instance.CustomsOfficeCode.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, bbrCode.PK.ToGuid()))
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("BBR", declaration.JE_CustomsOffice);
			}

			using (ZACustomsRegistry.Instance.CustomsOfficeCode.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), jhbCode.PK.ToGuid()))
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("JHB", declaration.JE_CustomsOffice);
			}

			using (ZACustomsRegistry.Instance.CustomsOfficeCode.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), longCode.PK.ToGuid()))
			{
				longCode.ZZD_Code = "ACDRE";
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("ACD", declaration.JE_CustomsOffice);

				var customsInterface = new LocalCountryCustomsInterface();
				customsInterface.RecipientID = "RecipientID";
				customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("BLT", Factory.New<JobDeclaration>().JE_ApplicationCode);
				}
				customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
				{
					AssertEquals("JE_ApplicationCode", DeclarationApplicationCodeList.Codes.Interfaced, Factory.New<BaseJobDeclaration>().JE_ApplicationCode);
				}
			}
		}

		public void TestAlexanderBayIsRemovedFromDistrictPortCode()
		{
			zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			Factory.Save();

			declaration.JE_CustomsOffice = "ALX";
			AssertHasMessageErrorContaining(declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_CustomsOffice = "JHB";
			AssertNoMessageErrorContaining(declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestAutoRating_BuiltIn()
		{
			var entry = SetupAutoRating(DeclarationApplicationCodeList.Codes.Builtin);
			var ratedLines = ServiceLocator.GetService<ICustomsCharges>(entry).GetCustomsCharges(null);

			CombineAssertions(() =>
			{
				var expected = new string[] {
					$"{ChargeCodeDisbursementDuty.PK} {"Duty"} {21m}" ,
					$"{ChargeCodeDisbursementLevy.PK} {"Levy"} {546m}" ,
					$"{ChargeCodeDisbursementPenalty.PK} {"Penalties"} {96.33m}" ,
					$"{ChargeCodeDisbursementProvisionalPayment.PK} {"Provisional Payments"} {169.00m}" ,
					$"{ChargeCodeDisbursementDefault.PK} {"VAT Normal"} {70m}"
				};
				var actual = ratedLines.Select(x => $"{x.GetChargeCode(Factory)?.PK ?? ZGuid.Empty} {x.Description} {x.Amount}");
				AssertContainsExactElementsInAnyOrder(expected, actual);
			});
		}

		public void TestAutoRating_InterfacedJob()
		{
			var entry = SetupAutoRating(DeclarationApplicationCodeList.Codes.Interfaced);
			var ratedLines = ServiceLocator.GetService<ICustomsCharges>(entry).GetCustomsCharges(null);

			CombineAssertions(() =>
			{
				var expected = new string[] {
					$"{ChargeCodeDisbursementDuty.PK} {"Duty"} {21m}" ,
					$"{ChargeCodeDisbursementLevy.PK} {"Levy"} {546m}" ,
					$"{ChargeCodeDisbursementPenalty.PK} {"Penalties"} {96.33m}" ,
					$"{ChargeCodeDisbursementProvisionalPayment.PK} {"Provisional Payments"} {169.00m}" ,
					$"{ChargeCodeDisbursementDefault.PK} {"VAT Normal"} {141m}"
				};
				var actual = ratedLines.Select(x => $"{x.GetChargeCode(Factory)?.PK ?? ZGuid.Empty} {x.Description} {x.Amount}");
				AssertContainsExactElementsInAnyOrder(expected, actual);
			});
		}

		CusEntryHeader SetupAutoRating(string applicationCode)
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			var today = ZDateTime.Today;
			helper.CreateTaxOrFee("VZR", 0, Core.Constants.CountryCodes.SouthAfrica, today.AddDays(-1), today.AddDays(1), "VAT Zero Rated");
			helper.CreateTaxOrFee("VAT", 0.14, Core.Constants.CountryCodes.SouthAfrica, today.AddDays(-1), today.AddDays(1), "VAT Normal");
			helper.CreateTaxOrFee("VEX", 0, Core.Constants.CountryCodes.SouthAfrica, today.AddDays(-1), today.AddDays(1), "VAT Exempt");

			var prpRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.ProvisionalPayment);
			var penRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Penalty);
			var vatRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "VAT");
			var levRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Levy);
			var dtyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty);
			var lvyRateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Levy);
			prpRateType.ZZR_IsPayable = true;
			prpRateType.ZZR_Description = "Provisional Payments";
			penRateType.ZZR_IsPayable = true;
			penRateType.ZZR_Description = "Penalties";
			vatRateType.ZZR_IsPayable = true;
			vatRateType.ZZR_Description = "VAT Normal";
			levRateType.ZZR_IsPayable = true;
			levRateType.ZZR_Description = "Levy";
			helper.LoadOrCreateNewCusRateCode(Factory, "1P1", dtyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "13A", lvyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "13B", lvyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "13C", lvyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "13D", lvyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "15A", lvyRateType.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, "15B", lvyRateType.PK);

			helper.CreateCustomsOfficeCusCodeEntry("JHB");
			Factory.Save();

			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeCodeDisbursementDefault.PK.ToGuid());
			RatingDataRegistry.Instance.CustomDeferredChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeCodeDeferred.PK.ToGuid());
			var entryChargeTypesAndCodes = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			entryChargeTypesAndCodes.Add(CreateEntryChargeType(ChargeCodeDisbursementDuty, Constants.RateTypes.Duty));
			entryChargeTypesAndCodes.Add(CreateEntryChargeType(ChargeCodeDisbursementLevy, Constants.RateTypes.Levy));
			entryChargeTypesAndCodes.Add(CreateEntryChargeType(ChargeCodeDisbursementPenalty, Constants.RateTypes.Penalty));
			entryChargeTypesAndCodes.Add(CreateEntryChargeType(ChargeCodeDisbursementProvisionalPayment, Constants.RateTypes.ProvisionalPayment));
			entryChargeTypesAndCodes.Add(CreateEntryChargeType(ChargeCodeDisbursementDefault, "VAT"));
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryChargeTypesAndCodes);

			var buyer = CreateOrgHeader("buyer", isConsignee: true, "IMP#@$43");
			buyer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "ASBSD", Core.Constants.CountryCodes.SouthAfrica);
			var creditor = CreateOrgHeader("creditor", isConsignee: true, "CRE#@$43");
			creditor.CompanyData.OB_IsCreditor = true;
			Factory.Save();

			var collection = new FinancialAccountNumberPortMapCollection();
			var map = GetFinancialAccountNumberPortMap(buyer, "JHB", "3234002346", importerPays: false, 1);
			map.CreditorPK = creditor.PK;
			collection.Add(map);
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var declaration = GetJobDeclaration(JobMessageTypeList.Codes.Import);
			declaration.JE_ApplicationCode = applicationCode;
			declaration.JE_CustomsOffice = "JHB";
			declaration.AgentCode = "ASBSD";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
			instruction.CEI_ProvisionalPaymentAmount = 100m;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_ZZF_NKTaxType = "VAT";
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_ZZF_NKTaxType = "VXX";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.EntryNumber = "902100210";
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.Fees.AddOrUpdate("1P1", 10m);
			entryLine1.Fees.AddOrUpdate("13A", 20m);
			entryLine1.Fees.AddOrUpdate("13C", 30m);
			entryLine1.Fees.AddOrUpdate("15B", 40m);
			entryLine1.Fees.AddOrUpdate("13D", 50m);
			entryLine1.Fees.AddOrUpdate("13B", 60m);
			entryLine1.Fees.AddOrUpdate("15A", 70m);
			entryLine1.Fees.AddOrUpdate("VAT", 70m);
			entryLine1.InvoiceLines.Add(invoiceLine1);

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.Fees.AddOrUpdate("1P1", 11m);
			entryLine2.Fees.AddOrUpdate("13A", 21m);
			entryLine2.Fees.AddOrUpdate("13C", 31m);
			entryLine2.Fees.AddOrUpdate("15B", 41m);
			entryLine2.Fees.AddOrUpdate("13D", 51m);
			entryLine2.Fees.AddOrUpdate("13B", 61m);
			entryLine2.Fees.AddOrUpdate("15A", 71m);
			entryLine2.Fees.AddOrUpdate("VAT", 71m);
			entryLine2.ProvisionalPayments.AddNew("PPA", 13.00m);
			entryLine2.ProvisionalPayments.AddNew("PPA", 23.00m);
			entryLine2.ProvisionalPayments.AddNew("PPC", 33.00m);
			entryLine2.ProvisionalPayments.AddNew("PEN", 43.11m);
			entryLine2.ProvisionalPayments.AddNew("FOR", 53.22m);
			entryLine2.ProvisionalPayments.AddNew("XXX", 53.00m);
			entryLine2.InvoiceLines.Add(invoiceLine2);

			return entry;
		}

		public void TestJE_PaymentMethod()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals("JE_PaymentMethod default a value", PaidByCodeList.Codes.BRK, declaration.JE_PaymentMethod);

			var consignee = Factory.New<OrgHeader>();
			consignee.MiscServ.OM_IMPaymentMethod = PaidByCodeList.Codes.CLI;
			declaration.JE_OH_Importer = consignee.PK;
			AssertEquals("JE_PaymentMethod value should now be set for CLI", PaidByCodeList.Codes.CLI, declaration.JE_PaymentMethod);
		}

		public void TestJE_ApplicationCode()
		{
			CombineAssertions("No registry", () =>
			{
				((IRegistryItemInternals)CustomsDataRegistry.Instance.LocalCountryCustomsInterface).DeleteValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				declaration = Factory.New<JobDeclaration>();
				AssertEquals(DeclarationApplicationCodeList.Codes.Builtin, declaration.JE_ApplicationCode);
				AssertEquals("BLT", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
			});

			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration = Factory.New<JobDeclaration>();
				AssertEquals(DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
				AssertEquals("ITF", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
			}

			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration = Factory.New<JobDeclaration>();
				AssertEquals(DeclarationApplicationCodeList.Codes.Builtin, declaration.JE_ApplicationCode);
				AssertEquals("BLT", true, declaration.JE_ApplicationCodeInfo.ReadOnly);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration = Factory.New<JobDeclaration>();
				AssertEquals(DeclarationApplicationCodeList.Codes.Interfaced, declaration.JE_ApplicationCode);
				AssertEquals("BIT", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
			}

			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				declaration = Factory.New<JobDeclaration>();
				AssertEquals(DeclarationApplicationCodeList.Codes.Builtin, declaration.JE_ApplicationCode);
				AssertEquals("BTH", false, declaration.JE_ApplicationCodeInfo.ReadOnly);
			}
			var testEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(expected: true, declaration.JE_ApplicationCodeInfo.ReadOnly);
		}

		public override void TestIsDeclarationWithEntryInstruction()
		{
			Assert("ZA Declaration should support EntryInstructions", !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
		}

		public override void TestCloneHasChanges()
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "cnumber";
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders.AddNew();
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "inumber";
			invoiceHeader.JobComInvoiceLines.AddNew();
			var clonedDeclaration = (JobDeclaration)declaration.TemplateCopy();

			AssertEquals("Container Count", 0, clonedDeclaration.CusContainers.Count);
			AssertEquals("Invoice Group Header Count", declaration.JobComInvoiceGroupHeaders.Count, clonedDeclaration.JobComInvoiceGroupHeaders.Count);
			AssertEquals("Invoice Count", declaration.Invoices.Count, clonedDeclaration.Invoices.Count);
			AssertEquals("Invoice Line Count", declaration.Invoices[0].JobComInvoiceLines.Count, clonedDeclaration.Invoices[0].JobComInvoiceLines.Count);
			AssertEquals("Declaration hasn't clones DocsAndCartage properly.", clonedDeclaration.PK, clonedDeclaration.DocsAndCartage.JP_ParentID);
			AssertEquals("Declaration Has changes because we set default values for Invoice Line", expected: true, clonedDeclaration.HasChanges);
		}

		public void TestRefreshIncotermAndChargeFactory_WithMessageTypeChange()
		{
			CombineAssertions(() =>
			{
				var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Export);
				var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				var header = declaration.Invoices.AddNew();
				var testHitter1 = groupHeader.IncoTermAndChargeFactory;
				var testHitter3 = header.IncoTermAndChargeFactory;
				AssertEquals("1-1", expected: false, groupHeader.NeedToGetNewIncoTermAndChargeFactory);
				AssertEquals("1-3", expected: false, header.NeedToGetNewIncoTermAndChargeFactory);

				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				AssertEquals("2-1", expected: true, groupHeader.NeedToGetNewIncoTermAndChargeFactory);
				AssertEquals("2-3", expected: true, header.NeedToGetNewIncoTermAndChargeFactory);
			});
		}

		public void TestIDA63ValueRecalculationParent()
		{
			zauniversalRefTestDataHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			Factory.Save();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "XX";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = $"{invoiceLine.EntryInstruction.CEI_Style}XX";
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_Procedure = $"{invoiceLine2.EntryInstruction.CEI_Style}YY";
			AssertEquals(expected: true, (declaration as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
			AssertEquals(expected: true, (invoice as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
			AssertEquals(expected: false, (invoiceLine as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
			AssertEquals(expected: true, (invoiceLine2 as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
			declaration.RecalculateDA63Values();
			AssertEquals(expected: false, (declaration as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
			AssertEquals(expected: false, (invoice as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
			AssertEquals(expected: false, (invoiceLine as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
			AssertEquals(expected: false, (invoiceLine2 as IDA63ValueRecalculationParent).DA63NeedsRecalculation);
		}

		public void TestJobDeclarationSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(typeof(JobDeclarationSynchroniser), declaration.ShipmentSynchroniser.GetType());
		}

		public void TestICurrencyConverterDataProvider()
		{
			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Fall back days", 0, ((ICurrencyConverterDataProvider)declaration).MaximumDaysToFallback);
			AssertEquals("Fall back days", 0, ((ICurrencyConverterDataProvider)invoice).MaximumDaysToFallback);
			AssertEquals("Fall back days", 0, ((ICurrencyConverterDataProvider)invoice.GroupHeader).MaximumDaysToFallback);
		}

		public void TestSetExchangeRateWhenJE_MasterBillIssuedDateChanged()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, Core.Constants.CurrencyCodes.SouthAfrica));
			currency.ExchangeRates.DeleteAll();
			var rate = CreateRefExhangeRate("CUS", 0.70m);
			currency.ExchangeRates.Add(rate);

			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Import);
			declaration.ApportionmentDirty = false;
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 12, 2);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;
			AssertEquals("JZ_InvoiceCurrExRate", 0m, invoice.JZ_InvoiceCurrExRate);
			AssertHasMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "There is no valid exchange rate for this currency");
			Assert("ApportionmentDirty", declaration.ApportionmentDirty);

			declaration.ApportionmentDirty = false;
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 12, 1);
			AssertEquals("JZ_InvoiceCurrExRate", 0.7m, invoice.JZ_InvoiceCurrExRate);
			AssertNoMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "There is no valid exchange rate for this currency");
			Assert("ApportionmentDirty", declaration.ApportionmentDirty);

			declaration.ApportionmentDirty = false;
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 11, 30);
			AssertEquals("JZ_InvoiceCurrExRate", 0m, invoice.JZ_InvoiceCurrExRate);
			AssertHasMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "There is no valid exchange rate for this currency");
			Assert("ApportionmentDirty", declaration.ApportionmentDirty);
		}

		public void TestRefreshExRateBeforeMerge()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, Core.Constants.CurrencyCodes.SouthAfrica));
			currency.ExchangeRates.DeleteAll();
			var rate = CreateRefExhangeRate("CUS", 0.70m);
			currency.ExchangeRates.Add(rate);

			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Import);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MasterBillIssuedDate = new ZDateTime(2016, 12, 2);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = currency.RX_Code;
			invoice.InvoiceLines.AddNew();
			AssertEquals("JZ_InvoiceCurrExRate", 0m, invoice.JZ_InvoiceCurrExRate);
			AssertHasMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "There is no valid exchange rate for this currency");

			rate.RE_StartDate = new ZDateTime(2016, 12, 2);
			rate.RE_ExpiryDate = new ZDateTime(2016, 12, 2);
			Factory.Save();
			declaration.DoMerge();
			AssertEquals("JZ_InvoiceCurrExRate", 0.7m, invoice.JZ_InvoiceCurrExRate);
			AssertNoMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "There is no valid exchange rate for this currency");

			rate.RE_ExRateType = "SEL";
			Factory.Save();
			declaration.DoMerge();
			AssertEquals("JZ_InvoiceCurrExRate", 0m, invoice.JZ_InvoiceCurrExRate);
			AssertHasMessageErrorContaining(invoice.JZ_RX_NKInvoice_CurrencyInfo, "There is no valid exchange rate for this currency");
		}

		[TestDate(2013, 03, 13, 13, 13, 33)]
		public void TestRefreshTariffAssessmentDateBeforeMerge()
		{
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals("Default date is 'Now'", new ZDateTime(2013, 03, 13, 13, 13, 33), invoiceLine.EffectiveAssessmentDate);

			CusEntryInstruction.ResetDefaultAssessmentDate(Factory);
			Factory.GetCachedValue(CusEntryInstruction.DefaultAssessmentDateCacheKey, () => new ZDateTime(2012, 02, 12, 12, 12, 00));
			AssertEquals("Precondition; an earlier date", new ZDateTime(2012, 02, 12, 12, 12, 00), invoiceLine.EffectiveAssessmentDate);

			declaration.DoMerge();
			AssertEquals("Merge updates the date to 'now'", new ZDateTime(2013, 03, 13, 13, 13, 33), invoiceLine.EffectiveAssessmentDate);
		}

		public void TestSetNeedsNewBGMReference()
		{
			var agent = OrgHeader.New(Factory);
			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Export);
			var entry = declaration.ActiveEntryHeaders.AddNew();
			Assert("NeedsNewBGMReference defaults to false", !entry.NeedsNewBGMReference);
			declaration.JE_OH_AgentOverride = agent.PK;
			Assert("NeedsNewBGMReference should be set", entry.NeedsNewBGMReference);
			entry.NeedsNewBGMReference = false;
			declaration.JE_CustomsOffice = "JSA";
			Assert("NeedsNewBGMReference should be set", entry.NeedsNewBGMReference);
		}

		public void TestRegenerateNewBGMReference()
		{
			var (declaration, entry) = SetupRegenerateNewBGMReference();
			AssertEquals("CH_BGMReference", ZString.Empty, entry.CH_BGMReference);
			AssertRegenerateNewBGMReference(declaration, declaration.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected, !declaration.JE_OH_AgentOverrideInfo.ReadOnly, !declaration.JE_CustomsOfficeInfo.ReadOnly);

			declaration.JE_CustomsOffice = "JSA";
			var lrn = entry.CH_BGMReference;
			var agentOrg = CreateOrgHeader("AgentName", "AGENTCODE");
			agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AC2222", Core.Constants.CountryCodes.SouthAfrica);
			declaration.JE_OH_AgentOverride = agentOrg.PK;
			Factory.Save();
			AssertNotEquals("CH_BGMReference should be regenerated", lrn, entry.CH_BGMReference);
			AssertRegenerateNewBGMReference(declaration, declaration.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected, !declaration.JE_OH_AgentOverrideInfo.ReadOnly, !declaration.JE_CustomsOfficeInfo.ReadOnly);

			declaration.JE_CustomsOffice = "BBR";
			AssertNotEquals("CH_BGMReference should be regenerated", lrn, entry.CH_BGMReference);
			AssertRegenerateNewBGMReference(declaration, declaration.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected, !declaration.JE_OH_AgentOverrideInfo.ReadOnly, !declaration.JE_CustomsOfficeInfo.ReadOnly);

			declaration.JE_CustomsOffice = "JSA";
			AssertNotEquals("CH_BGMReference should be regenerated", lrn, entry.CH_BGMReference);

			lrn = entry.CH_BGMReference;
			entry.CH_Status = ZAMessageStatusList.Codes.AwaitingResponse;
			AssertRegenerateNewBGMReference(declaration, !declaration.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected, declaration.JE_OH_AgentOverrideInfo.ReadOnly, declaration.JE_CustomsOfficeInfo.ReadOnly);

			declaration.JE_CustomsOffice = "BBR";
			AssertEquals("CH_BGMReference should NOT be regenerated", lrn, entry.CH_BGMReference);
			Assert("JE_OH_AgentOverrideInfo.ReadOnly", declaration.JE_OH_AgentOverrideInfo.ReadOnly);
			Assert("JE_CustomsOfficeInfo.ReadOnly", declaration.JE_CustomsOfficeInfo.ReadOnly);

			declaration.JE_CustomsOffice = "JSA";
			entry.CH_EntryStatus = "6";
			Factory.Save();
			AssertNotEquals("CH_BGMReference should be regenerated", lrn, entry.CH_BGMReference);
			AssertRegenerateNewBGMReference(declaration, declaration.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected, !declaration.JE_OH_AgentOverrideInfo.ReadOnly, !declaration.JE_CustomsOfficeInfo.ReadOnly);

			lrn = entry.CH_BGMReference;
			entry.MovementReferenceNumberSetter("MRN", ZDateTime.Today);
			AssertRegenerateNewBGMReference(declaration, !declaration.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected, declaration.JE_OH_AgentOverrideInfo.ReadOnly, declaration.JE_CustomsOfficeInfo.ReadOnly);

			declaration.JE_CustomsOffice = "BBR";
			Factory.Save();
			AssertEquals("CH_BGMReference should NOT be regenerated", lrn, entry.CH_BGMReference);
			Assert("JE_OH_AgentOverrideInfo.ReadOnly", declaration.JE_OH_AgentOverrideInfo.ReadOnly);
			Assert("JE_CustomsOfficeInfo.ReadOnly", declaration.JE_CustomsOfficeInfo.ReadOnly);

			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();
			AssertNotEquals("CH_BGMReference generated", ZString.Empty, entry2.CH_BGMReference);

			var lrn2 = entry2.CH_BGMReference;
			declaration.JE_CustomsOffice = "JSA";
			Assert("JE_OH_AgentOverrideInfo.ReadOnly", declaration.JE_OH_AgentOverrideInfo.ReadOnly);
			Assert("JE_CustomsOfficeInfo.ReadOnly", declaration.JE_CustomsOfficeInfo.ReadOnly);
			AssertEquals("CH_BGMReference should NOT be regenerated", lrn, entry.CH_BGMReference);
			AssertEquals("CH_BGMReference should NOT be regenerated", lrn2, entry2.CH_BGMReference);
		}

		(JobDeclaration declaration, CusEntryHeader entryHeader) SetupRegenerateNewBGMReference()
		{
			zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			zauniversalRefTestDataHelper.CreateCustomsOfficeCusCodeEntry("JSA");
			zauniversalRefTestDataHelper.CreateCustomsStatusCusCodeEntry("6");
			Factory.Save();

			var declaration = GetJobDeclaration(JobMessageTypeList.Codes.Export);
			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			var orgProxy = declaration.Branch.Company.OrgProxy;
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AC1111", Core.Constants.CountryCodes.SouthAfrica);
			var entry = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();
			return (declaration, entry);
		}

		void AssertRegenerateNewBGMReference(JobDeclaration declaration, bool allowReassignmentOfLRN, bool agentOverrideInfoReadOnly, bool customsOfficeInfoReadOnly)
		{
			Assert("AllowReassignmentOfLRN", allowReassignmentOfLRN);
			Assert("JE_OH_AgentOverrideInfo.ReadOnly", agentOverrideInfoReadOnly);
			Assert("JE_CustomsOfficeInfo.ReadOnly", customsOfficeInfoReadOnly);
		}

		public void TestClearanceParts()
		{
			var declaration = GetJobDeclaration(ZAJobMessageTypeList.Codes.Import);
			var testInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_Style = "11";
			var testInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction2.CEI_Style = "20";
			var invoiceHeader1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceHeader2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = testInstruction1.PK;
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = testInstruction2.PK;
			new LineMerger(declaration).DoMerge();
			var entry1 = invoiceLine1.CusEntryLine.Header;
			var entry2 = invoiceLine2.CusEntryLine.Header;
			AssertContainsExactElementsInAnyOrder(new CusEntryHeader[] { entry1, entry2 }, declaration.ClearanceParts);

			entry1.MovementReferenceNumberSetter("MRN_REP", ZDateTime.Now);
			entry2.MovementReferenceNumberSetter("MRN_NOREP", ZDateTime.Now);
			var testInstruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction3.CEI_Style = "11";
			testInstruction3.CEI_MRNToBeReplaced = "MRN_REP";
			var testInstruction4 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction4.CEI_Style = "20";
			testInstruction4.CEI_MRNToBeReplaced = "MRN_REP_NOTINJOB";
			var invoiceHeader3 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceHeader4 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine3 = invoiceHeader3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = testInstruction3.PK;
			var invoiceLine4 = invoiceHeader4.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CEI = testInstruction4.PK;
			new LineMerger(declaration).DoMerge();
			var entry3 = invoiceLine3.CusEntryLine.Header;
			var entry4 = invoiceLine4.CusEntryLine.Header;
			AssertContainsExactElementsInAnyOrder(new CusEntryHeader[] { entry2, entry3, entry4 }, declaration.ClearanceParts);
		}

		public void TestPortDirectionForImport()
		{
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "MZAME";
			AssertEquals("Port of Exit", declaration.PortDirection);

			declaration.JE_RL_NKFinalDestination = "MZAME";
			AssertEquals("Port of Exit", declaration.PortDirection);

			declaration.JE_RL_NKFinalDestination = "";
			AssertEquals("Port of Destination", declaration.PortDirection);

			declaration.JE_RL_NKFinalDestination = "ZAAAM";
			AssertEquals("Port of Destination", declaration.PortDirection);
		}

		public void TestPortDirectionForExport()
		{
			AssertEquals("Port of Exit", declaration.PortDirection);
			declaration.JE_RL_NKFinalDestination = "MZAME";
			AssertEquals("Port of Exit", declaration.PortDirection);
		}

		public override void TestIsBillIssueDateVisible()
		{
			Assert("IsBillIssueDateVisible", declaration.IsBillIssueDateVisible);
		}

		public override void TestContainerNotLinkedSeverity()
		{
			AssertEquals("ContainerNotLinkedSeverity", CargoWise.ComponentModel.NotificationType.Warning, declaration.ContainerNotLinkedSeverity);
			declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("ContainerNotLinkedSeverity", CargoWise.EntityFramework.NotificationType.MessageError, declaration.ContainerNotLinkedSeverity);
		}

		public void TestHasMultiCustomsEntryInstructions()
		{
			Assert("HasMultiCustomsEntryInstructions", !declaration.HasMultiCustomsEntryInstructions);
			declaration.CustomsEntryInstructions.AddNew();
			declaration.CustomsEntryInstructions.AddNew();
			Assert("HasMultiCustomsEntryInstructions", declaration.HasMultiCustomsEntryInstructions);
		}

		public void TestInvoicesMentionNewOrInactiveProducts()
		{
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "PRODUCT";
			invoiceLine.JI_Description = "PRODUCT DESCRIPTION";
			invoiceLine.JI_CC = ZGuid.Empty;
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_InvoiceUQ = "UNT";
			Assert("InvoicesMentionNewOrInactiveProducts", !declaration.InvoicesMentionNewOrInactiveProducts);

			invoiceLine.JI_CC = ZGuid.NewZGuid();
			invoiceLine.JI_Tariff = ZString.Empty;
			Assert("InvoicesMentionNewOrInactiveProducts", declaration.InvoicesMentionNewOrInactiveProducts);

			invoiceLine.JI_CC = ZGuid.Empty;
			invoiceLine.JI_Tariff = "0121323122";
			Assert("InvoicesMentionNewOrInactiveProducts", declaration.InvoicesMentionNewOrInactiveProducts);
		}

		public void TestEntryNumberUCR()
		{
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			CreateCusEntryNumber(cusEntryHeader.PK, CusEntryNumberTypes.Standard.UniqueConsignementReference, "test1");
			CreateCusEntryNumber(cusEntryHeader.PK, CusEntryNumberTypes.Standard.UniqueConsignementReference, "test2");
			CreateCusEntryNumber(cusEntryHeader.PK, CusEntryNumberTypes.Standard.MovementReferenceNumber, "test3");
			CreateCusEntryNumber(declaration.PK, CusEntryNumberTypes.Standard.UniqueConsignementReference, "test4");
			CreateCusEntryNumber(declaration.PK, CusEntryNumberTypes.Standard.MovementReferenceNumber, "test5");

			cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			CreateCusEntryNumber(cusEntryHeader.PK, CusEntryNumberTypes.Standard.UniqueConsignementReference, "test6");
			AssertEquals("test4, test1, test2, test6", declaration.CombinedUCREntryNumbers);
		}

		public void TestHasEntryLineNumberExceedingMax()
		{
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 9999;
			AssertEquals(expected: false, declaration.HasEntryLineNumberExceedingMax);

			entryLine.CL_LineNumber = 10000;
			AssertEquals(expected: true, declaration.HasEntryLineNumberExceedingMax);
		}

		public void TestIsAnyInvoiceLinePreviousProcedureCodeNot00ByIsMergeByValidForPreviousProcedureCode()
		{
			declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			AssertEquals("IsMergeByValidForPreviousProcedureCode is True when there are no invoice lines",
				declaration.IsMergeByValidForPreviousProcedureCode, true);
			AssertEquals("Cache value same as real time calculation value",
				declaration.IsMergeByValidForPreviousProcedureCode,
				!((declaration.InvoiceLines?.Cast<JobComInvoiceLine>().Any(x =>
					x.JI_Calc_PreviousProcedure != string.Empty && x.JI_Calc_PreviousProcedure != UniversalReferenceConstants.ProcedureCodes._00) ?? false)
					&& declaration.JE_MergeBy != Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge
					&& declaration.JE_MergeBy != Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription));

			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine1.JI_Procedure = "1000";

			AssertEquals("IsMergeByValidForPreviousProcedureCode is True when only invoice line ends with 00",
				declaration.IsMergeByValidForPreviousProcedureCode, true);
			AssertEquals("Cache value same as real time calculation value",
				declaration.IsMergeByValidForPreviousProcedureCode,
				!((declaration.InvoiceLines?.Cast<JobComInvoiceLine>().Any(x =>
					  x.JI_Calc_PreviousProcedure != string.Empty && x.JI_Calc_PreviousProcedure != UniversalReferenceConstants.ProcedureCodes._00) ?? false)
				  && declaration.JE_MergeBy != Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge
				  && declaration.JE_MergeBy != Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription));
			
			var invoiceLine2 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine2.JI_Procedure = "10AA";

			AssertEquals("IsMergeByValidForPreviousProcedureCode is False when exists one invoice line ends with 00",
				declaration.IsMergeByValidForPreviousProcedureCode, false);
			AssertEquals("Cache value same as real time calculation value",
				declaration.IsMergeByValidForPreviousProcedureCode,
				!((declaration.InvoiceLines?.Cast<JobComInvoiceLine>().Any(x =>
					  x.JI_Calc_PreviousProcedure != string.Empty && x.JI_Calc_PreviousProcedure != UniversalReferenceConstants.ProcedureCodes._00) ?? false)
				  && declaration.JE_MergeBy != Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge
				  && declaration.JE_MergeBy != Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription));

			invoiceHeader1.InvoiceLines.Remove(invoiceLine2);

			AssertEquals("IsMergeByValidForPreviousProcedureCode is True when non-00 invoice line is removed",
				declaration.IsMergeByValidForPreviousProcedureCode, true);
			AssertEquals("Cache value same as real time calculation value",
				declaration.IsMergeByValidForPreviousProcedureCode,
				!((declaration.InvoiceLines?.Cast<JobComInvoiceLine>().Any(x =>
					  x.JI_Calc_PreviousProcedure != string.Empty && x.JI_Calc_PreviousProcedure != UniversalReferenceConstants.ProcedureCodes._00) ?? false)
				  && declaration.JE_MergeBy != Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge
				  && declaration.JE_MergeBy != Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription));
		}

		public void TestGetPreviousEntryLineNumbersMinMaxValueMessageError()
		{
			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceNumber = "INV001";
			var mockInvoiceLine1 = Factory.New<JobComInvoiceLineForTesting>();
			mockInvoiceLine1.JI_PreviousEntryLineNumberReturns = new ZShort("11111");
			var invoiceLine1 = mockInvoiceLine1;
			invoiceHeader1.InvoiceLines.Add(invoiceLine1);
			invoiceLine1.JI_JZ = invoiceHeader1.PK;
			var invoiceLine2 = invoiceHeader1.InvoiceLines.AddNew();
			invoiceLine2.JI_PreviousEntryLineNumber = 123;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INV002";
			var mockInvoiceLine2 = Factory.New<JobComInvoiceLineForTesting>();
			mockInvoiceLine2.JI_PreviousEntryLineNumberReturns = new ZShort("22222");
			var invoiceLine3 = mockInvoiceLine2;
			invoiceHeader2.InvoiceLines.Add(invoiceLine3);
			invoiceLine3.JI_JZ = invoiceHeader2.PK;
			var errorMsg = declaration.GetPreviousEntryLineNumbersMinMaxValueMessageError();
			Assert(errorMsg.Contains("Invoice: INV001 Line: 1"));
			Assert(errorMsg.Contains("Invoice: INV002 Line: 1"));
		}

		public void TestIsBLNSValidationRequired()
		{
			var blnsCountry = Factory.NewWithValidTestData<RefCountry>();
			blnsCountry.RN_Code = "XX";
			blnsCountry.RN_EconomicGrouping = EconomicGroupList.Codes.BLNS;
			var nonBLNSCountry = Factory.NewWithValidTestData<RefCountry>();
			nonBLNSCountry.RN_Code = "YY";
			nonBLNSCountry.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;
			AssertEquals(expected: true, declaration.IsBLNSValidationRequired);

			var checkList = new Dictionary<string, bool>()
			{
				{ blnsCountry.RN_Code, true },
				{ nonBLNSCountry.RN_Code, false }
			};

			foreach (var check in checkList)
			{
				CombineAssertions("Importing", () =>
				{
					foreach (var messageType in new string[] { ZAJobMessageTypeList.Codes.Import, ZAJobMessageTypeList.Codes.ImportByExternalBroker })
					{
						declaration.JE_MessageType = messageType;
						declaration.JE_RL_NKOrigin = "";
						declaration.JE_GoodsOrigin = "";
						declaration.JE_RL_NKPortOfLoading = "";
						AssertEquals($"Field: None ShipType: {messageType} Country: {check.Key}", expected: false, declaration.IsBLNSValidationRequired);

						declaration.JE_RL_NKPortOfLoading = check.Key;
						declaration.JE_RL_NKOrigin = "";
						declaration.JE_GoodsOrigin = "";
						AssertEquals($"Field: JE_RL_NKPortOfLoading ShipType: {messageType} Country: {check.Key}", check.Value, declaration.IsBLNSValidationRequired);

						declaration.JE_RL_NKOrigin = check.Key;
						declaration.JE_GoodsOrigin = "";
						declaration.JE_RL_NKPortOfLoading = "";
						AssertEquals($"Field: JE_RL_NKOrigin ShipType: {messageType} Country: {check.Key}", check.Value, declaration.IsBLNSValidationRequired);

						declaration.JE_GoodsOrigin = check.Key;
						declaration.JE_RL_NKPortOfLoading = "";
						declaration.JE_RL_NKOrigin = "";
						AssertEquals($"Field: JE_GoodsOrigin ShipType: {messageType} Country: {check.Key}", check.Value, declaration.IsBLNSValidationRequired);
					}
				});

				CombineAssertions("Exporting", () =>
				{
					declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
					declaration.JE_RL_NKPortOfArrival = "";
					declaration.JE_RL_NKFinalDestination = "";
					AssertEquals($"Field: None ShipType: {ZAJobMessageTypeList.Codes.ExBond} Country: {check.Key}", expected: false, declaration.IsBLNSValidationRequired);
					declaration.JE_RL_NKPortOfArrival = check.Key;
					declaration.JE_RL_NKFinalDestination = "";
					AssertEquals($"Field: JE_RL_NKPortOfArrival ShipType: {ZAJobMessageTypeList.Codes.ExBond} Country: {check.Key}", check.Value, declaration.IsBLNSValidationRequired);
					declaration.JE_RL_NKFinalDestination = check.Key;
					declaration.JE_RL_NKPortOfArrival = "";
					AssertEquals($"Field: JE_RL_NKFinalDestination ShipType: {ZAJobMessageTypeList.Codes.ExBond} Country: {check.Key}", check.Value, declaration.IsBLNSValidationRequired);
				});
			}
		}

		public void TestCombinedPrintReleaseIndicator()
		{
			AssertEquals("CombinedPrintReleaseIndicator Empty", ZString.Empty, declaration.CombinedReleasePrintIndicator);

			var header1 = declaration.CustomsEntryHeaders.AddNew();
			var yes = new ZString("Y");
			var no = new ZString("N");
			var multi = new ZString("MULTI");

			AssertEquals("CombinedPrintReleaseIndicator Empty", ZString.Empty, declaration.CombinedReleasePrintIndicator);
			header1.CH_RelPrintInd = "";
			AssertEquals("CombinedPrintReleaseIndicator Still Empty", ZString.Empty, declaration.CombinedReleasePrintIndicator);
			header1.CH_RelPrintInd = "Y";
			AssertEquals("CombinedPrintReleaseIndicator Y", yes, declaration.CombinedReleasePrintIndicator);
			header1.CH_RelPrintInd = "N";
			AssertEquals("CombinedPrintReleaseIndicator N", no, declaration.CombinedReleasePrintIndicator);

			var header2 = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("CombinedPrintReleaseIndicator Multi", multi, declaration.CombinedReleasePrintIndicator);
			header2.CH_RelPrintInd = "Y";
			AssertEquals("CombinedPrintReleaseIndicator Multi", multi, declaration.CombinedReleasePrintIndicator);
			header2.CH_RelPrintInd = "N";
			AssertEquals("CombinedPrintReleaseIndicator N", no, declaration.CombinedReleasePrintIndicator);
		}

		public void IsUnknownOrNotApplicable()
		{
			declaration.JE_TransportMode = "";
			AssertEquals("Should be true", expected: true, declaration.IsUnknownOrNotApplicable);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Should be false", expected: true, declaration.IsUnknownOrNotApplicable);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals("Should be true", expected: true, declaration.IsUnknownOrNotApplicable);
		}

		public void TestFilteredInvoiceLines()
		{
			AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		public void TestHasAnyInvoiceLinesLinkedToEntryInstructions()
		{
			AssertEquals(expected: false, declaration.HasAnyInvoiceLinesLinkedToEntryInstructions);
			var invoiceHeader1 = declaration.Invoices.AddNew();
			AssertEquals(expected: false, declaration.HasAnyInvoiceLinesLinkedToEntryInstructions);
			invoiceHeader1.InvoiceLines.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.InvoiceLines.AddNew();
			var line3 = invoiceHeader2.InvoiceLines.AddNew();
			AssertEquals(expected: false, declaration.HasAnyInvoiceLinesLinkedToEntryInstructions);
			line3.JI_CEI = new ZGuid("65A6EDBD-4CAC-4310-A09B-8BC41776104F");
			AssertEquals(expected: true, declaration.HasAnyInvoiceLinesLinkedToEntryInstructions);
		}

		public void TestAutoCreateEntryInstructions()
		{
			var preference = zauniversalRefTestDataHelper.CreatePreferenceForCountry(UniversalReferenceConstants.PrimaryPreference.Standard, "None", Core.Constants.CountryCodes.SouthAfrica);
			var dtyRateType = zauniversalRefTestDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty);
			var rateCode = zauniversalRefTestDataHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, dtyRateType.PK);
			var tariffType = zauniversalRefTestDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var t1 = zauniversalRefTestDataHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "010121", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			zauniversalRefTestDataHelper.CreateRate(t1, rateCode.PK, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2), "0", preference.PK);
			var t2 = zauniversalRefTestDataHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "39199050", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			zauniversalRefTestDataHelper.CreateRate(t2, rateCode.PK, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2), "0.1 * VFD", preference.PK);

			AssertEquals("Pre-requisite: test data should ensure 39199050 has duty", expected: true, declaration.TariffCodeHasDuty(t2.PK, ZDateTime.Today, Core.Constants.CountryCodes.SouthAfrica));
			AssertEquals("Pre-requisite: test data should ensure 010121 has no duty", expected: false, declaration.TariffCodeHasDuty(t1.PK, ZDateTime.Today, Core.Constants.CountryCodes.SouthAfrica));

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV001";
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_Tariff = "010121";
			var line2 = invoiceHeader.InvoiceLines.AddNew();
			line2.JI_Tariff = "39199050";
			Factory.Save();
			declaration.AutoCreateEntryInstructions(false);

			AssertEquals(2, invoiceHeader.CusEntryInstructions.Count());
			AssertContainsExactElementsInAnyOrder(new List<string> { UniversalReferenceConstants.ProcedureCodes._11, UniversalReferenceConstants.ProcedureCodes._40 }, invoiceHeader.CusEntryInstructions.Select(ei => ei.CEI_Style));
			var key11 = invoiceHeader.CusEntryInstructions.First(ei => ei.CEI_Style == UniversalReferenceConstants.ProcedureCodes._11).PK;
			var key40 = invoiceHeader.CusEntryInstructions.First(ei => ei.CEI_Style == UniversalReferenceConstants.ProcedureCodes._40).PK;
			AssertEquals(line1.JI_CEI, key11);
			AssertEquals(line2.JI_CEI, key40);

			line1.JI_CEI = ZGuid.Empty;
			line2.JI_CEI = ZGuid.Empty;
			Factory.Save();
			declaration.AutoCreateEntryInstructions(false);

			AssertEquals(2, invoiceHeader.CusEntryInstructions.Count());
			AssertEquals(line1.JI_CEI, key11);
			AssertEquals(line2.JI_CEI, key40);

			var ei68 = declaration.CustomsEntryInstructions.AddNew();
			ei68.CEI_Style = UniversalReferenceConstants.ProcedureCodes._68;
			line1.JI_CEI = ei68.PK;
			line2.JI_CEI = ei68.PK;
			Factory.Save();
			declaration.AutoCreateEntryInstructions(false);

			CombineAssertions("Existing links should not be overwritten", () =>
			{
				AssertEquals(line1.JI_CEI, ei68.PK);
				AssertEquals(line2.JI_CEI, ei68.PK);
			});

			declaration.AutoCreateEntryInstructions(true);

			CombineAssertions("Existing links should be overwritten", () =>
			{
				AssertEquals(line1.JI_CEI, key11);
				AssertEquals(line2.JI_CEI, key40);
			});
		}

		public void TestAutoCreateEntryInstructions_NoTariff()
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV001";
			invoiceHeader.InvoiceLines.AddNew();
			declaration.AutoCreateEntryInstructions(false);
			AssertEquals(0, invoiceHeader.CusEntryInstructions.Count());
		}

		public void TestSupportValidateCustomsMessaging()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			var supportStatus = declaration.SupportValidateCustomsMessagingCoreExposed;
			Assert("ZA JobDeclaration must support validation for Customs messaging.", supportStatus);
		}

		public void TestGetEntryDeclarationMessageProcessor()
		{
			var messageProcessor = declaration.GetEntryDeclarationMessageProcessor(declaration);
			AssertNotNull("A message processor instance must be provided.", messageProcessor);

			var autoSendMessageProcessor = messageProcessor as AutoSendCustomsMessageProcessor;
			AssertNotNull("The message processor must be of type <AutoSendCustomsMessageProcessor>.", autoSendMessageProcessor);
		}

		public override void TestICusEntryNumFilterProviderImplementation()
		{
			// ZA implementation of ValidCusEntryNumFilter is different from implementation in shared.
			// CusEntryNumber cannot be attached to EU declaration directly.

			var cusEntryNumFilterProvider = (ICusEntryNumFilterProvider)declaration;
			var entryNumbers = new CusEntryNumCollection(Factory);
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertEquals("pre-condition", 0, entryNumbers.Count);

			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.EntryNumber = "12121212";
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertICusEntryNumFilterProviderImplementation(entryNumbers, new string[] { "12121212" });

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.EntryNumber = "21212121";
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertICusEntryNumFilterProviderImplementation(entryNumbers, new string[] { "12121212", "21212121" });

			entryHeader1.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
			entryNumbers.Load(cusEntryNumFilterProvider.ValidCusEntryNumFilter);
			AssertICusEntryNumFilterProviderImplementation(entryNumbers, new string[] { "21212121" });

			var otherDeclaration = Factory.New<BaseJobDeclaration>();
			var otherEntryHeader = otherDeclaration.CustomsEntryHeaders.AddNew();
			otherEntryHeader.EntryNumber = "66666667";
			entryNumbers.Load(((ICusEntryNumFilterProvider)otherDeclaration).ValidCusEntryNumFilter);
			AssertICusEntryNumFilterProviderImplementation(entryNumbers, new string[] { "66666667" });

			var ucr = CusEntryNumber.New(otherEntryHeader, CusEntryNumberTypes.Standard.UniqueConsignementReference, otherDeclaration.CountryCode);
			ucr.CE_EntryNum = "UCR";
			entryNumbers.Load(((ICusEntryNumFilterProvider)otherDeclaration).ValidCusEntryNumFilter);
			AssertICusEntryNumFilterProviderImplementation(entryNumbers, new string[] { "66666667" });
		}

		void AssertICusEntryNumFilterProviderImplementation(CusEntryNumCollection entryNumbers, string[] expectedEntryNumbers)
		{
			AssertArrayEqualsByElements(expectedEntryNumbers, entryNumbers.Cast<CusEntryNumber>().OrderBy(e => e.CE_EntryNum).Select(e => (string)e.CE_EntryNum).ToArray());
		}

		public void TestInvoiceLineMaxCountUpdatedByMessageType()
		{
			using (ZACustomsRegistry.Instance.ExbondMaxNumberJobInvoiceLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2))
			{
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
				AssertEquals(-1, declaration.InvoiceLines.MaxCount);
				declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
				AssertEquals(2, declaration.InvoiceLines.MaxCount);
			}

			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			AssertEquals(-1, declaration.InvoiceLines.MaxCount);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			AssertEquals(-1, declaration.InvoiceLines.MaxCount);
		}

		public void TestDisableFlushImporterDocumentaryAddress()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OH_Importer = helper.Importer.PK;
			AssertEquals(helper.Importer.MainAddress, declaration.ImporterDocumentaryAddress.Address);

			var originalAddressPK = declaration.ImporterDocumentaryAddress.PK;
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(originalAddressPK, declaration.ImporterDocumentaryAddress.PK);

			var newImporter = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = newImporter.PK;
			AssertEquals(originalAddressPK, declaration.ImporterDocumentaryAddress.PK);
			AssertEquals(newImporter.MainAddress, declaration.ImporterDocumentaryAddress.Address);
		}

		public void TestTransportSupporterType()
		{
			AssertType<JobDeclarationTransportSupporter>(((Freight.Business.ITransportParent)declaration).TransportSupporter);
		}

		public override void TestShouldCopyProcedureFromPreviousInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (CustomsDataRegistry.Instance.CopyPreviousLineCustomsProcedureCode.SetTemporaryValue(Guid.Empty, declaration.Branch.EntityPK.ToGuid(), Guid.Empty, true))
			{
				AssertEquals("ShouldCopyProcedureFromPreviousInvoiceLine always be true if registry is true", true, declaration.ShouldCopyProcedureFromPreviousInvoiceLine);
			}
			var declaration2 = Factory.New<JobDeclaration>();
			using (CustomsDataRegistry.Instance.CopyPreviousLineCustomsProcedureCode.SetTemporaryValue(Guid.Empty, declaration2.Branch.EntityPK.ToGuid(), Guid.Empty, false))
			{
				AssertEquals("ShouldCopyProcedureFromPreviousInvoiceLine always be true even registry is false", true, declaration2.ShouldCopyProcedureFromPreviousInvoiceLine);
			}
		}

		public override void TestJE_DateOfArrivalCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var databoundBO = new DataBoundBusinessObject(declaration);
			CombineAssertions(() =>
			{
				var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_DateOfArrivalInfo, databoundBO);
				AssertEquals("Caption", "Arrival", resourceStringData.Caption);
				AssertEquals("ShortCaption", "Arr.", resourceStringData.ShortCaption);
			});

			var messageDeferralSettings = new AutomaticDeferredSelection { AllowAutomaticDeferredSelection = true };
			using (ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings))
			{
				declaration.JE_MessageType = Common.ZA.ZAJobMessageTypeList.Codes.ExBond;
				CombineAssertions(() =>
				{
					var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_DateOfArrivalInfo, databoundBO);
					AssertEquals("Caption", "Estimated Arrival Date", resourceStringData.Caption);
				});
			}
		}

		protected override void SetUp()
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			zauniversalRefTestDataHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
		}

		JobDeclaration declaration;
		ZAUniversalReferenceTestDataHelper zauniversalRefTestDataHelper;

		JobDeclaration GetJobDeclaration(string messageType)
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = messageType;
			return declaration;
		}

		FinancialAccountNumberPortMap GetFinancialAccountNumberPortMap(OrgHeader organization, string customsOfficeCode, string accountNumber, bool importerPays, int startDay)
		{
			var mapping = new FinancialAccountNumberPortMap();
			mapping.OrganizationPK = organization.PK;
			mapping.CustomsOfficeCode = customsOfficeCode;
			mapping.FinancialAccountNumber = accountNumber;
			mapping.ImporterPays = importerPays;
			mapping.AccountStartDay = startDay;
			return mapping;
		}

		OrgHeader CreateOrgHeader(string fullName, bool isConsignee, string code)
		{
			var org = CreateOrgHeader(fullName, code);
			org.OH_IsConsignee = isConsignee;
			return org;
		}

		OrgHeader CreateOrgHeader(string fullName, string code)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = fullName;
			org.OH_Code = code;
			return org;
		}

		OrgHeader GetOrgHeader()
		{
			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
			orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);
			return Factory.LoadTop1<OrgHeader>(orgHeaderQuery);
		}

		RefExchangeRate CreateRefExhangeRate(string rateType, decimal sellRate)
		{
			var rate = Factory.New<RefExchangeRate>();
			rate.RE_ExRateType = rateType;
			rate.RE_StartDate = new ZDateTime(2016, 12, 1);
			rate.RE_ExpiryDate = new ZDateTime(2016, 12, 1);
			rate.RE_SellRate = sellRate;
			return rate;
		}

		CusEntryNumber AddCusEntryNumber(CusEntryHeader entry, string entryType, string entryNum, string countryCode = Core.Constants.CountryCodes.SouthAfrica)
		{
			var entryNumber = CusEntryNumber.New(entry, entryType, countryCode);
			entryNumber.CE_EntryNum = entryNum;
			return entryNumber;
		}

		CusEntryNumber CreateCusEntryNumber(ZGuid parentID, string entryType, string entryNum)
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = parentID;
			entryNumber.CE_EntryType = entryType;
			entryNumber.CE_EntryNum = entryNum;
			return entryNumber;
		}

		EntryChargeTypeSetting CreateEntryChargeType(AccChargeCode chargeCode, string chargeType)
		{
			var entryChargeType = new EntryChargeTypeSetting();
			entryChargeType.ChargeType = chargeType;
			entryChargeType.AC_ChargeCode = chargeCode.PK;
			return entryChargeType;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBillIssuedDate = ZDateTime.Empty;
			return declaration;
		}

		protected override void AfterInitialise(BaseJobDeclaration declaration)
		{
			base.AfterInitialise(declaration);
			var zaDec = (JobDeclaration)declaration;
			zaDec.JE_MasterBillIssuedDate = ZDateTime.Empty;
		}

		protected override ZString[] TransportModesNotToTestForGenericTranslation => new ZString[] { ZString.Empty };

		protected override string GetAutoCreatedExBondInvoiceNumber() => string.Empty;

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var dec = base.GetJobDeclaration();
			dec.JE_ValuationDate = ZDate.Today;
			return dec;
		}

		protected override void SetupInvoice(BaseJobDeclaration declaration)
		{
			var jobDeclaration = (JobDeclaration)declaration;
			jobDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "11234";
		}

		protected override bool ExpectedSupportInvoiceLineRefs => true;

		AccChargeCode CreateChargeCode(string code, string description, string chargeType)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = $"BG~{code}~";
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = 0m;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			return chargeCode;
		}

		AccChargeCode chargeCodeDisbursementDefault;
		AccChargeCode ChargeCodeDisbursementDefault
			=> chargeCodeDisbursementDefault ?? (chargeCodeDisbursementDefault = CreateChargeCode("DSB", "Customs Disbursements Default", Core.Constants.ChargeType.Disbursement));

		AccChargeCode chargeCodeDisbursementDuty;
		AccChargeCode ChargeCodeDisbursementDuty
			=> chargeCodeDisbursementDuty ?? (chargeCodeDisbursementDuty = CreateChargeCode("DUT", "Customs Disbursements Duty", Core.Constants.ChargeType.Disbursement));

		AccChargeCode chargeCodeDisbursementLevy;
		AccChargeCode ChargeCodeDisbursementLevy
			=> chargeCodeDisbursementLevy ?? (chargeCodeDisbursementLevy = CreateChargeCode("LVY", "Customs Disbursements Levy", Core.Constants.ChargeType.Disbursement));

		AccChargeCode chargeCodeDisbursementPenalty;
		AccChargeCode ChargeCodeDisbursementPenalty
			=> chargeCodeDisbursementPenalty ?? (chargeCodeDisbursementPenalty = CreateChargeCode("PEN", "Customs Disbursements Penalty", Core.Constants.ChargeType.Disbursement));

		AccChargeCode chargeCodeDisbursementProvisionalPayment;
		AccChargeCode ChargeCodeDisbursementProvisionalPayment
			=> chargeCodeDisbursementProvisionalPayment ?? (chargeCodeDisbursementProvisionalPayment = CreateChargeCode("PRP", "Customs Disbursements ProvisionalPayment", Core.Constants.ChargeType.Disbursement));

		AccChargeCode chargeCodeDeferred;
		AccChargeCode ChargeCodeDeferred
			=> chargeCodeDeferred ?? (chargeCodeDeferred = CreateChargeCode("DEF", "Customs Deferred Charge (For information only)", Core.Constants.ChargeType.Comment));

		JobComInvoiceLine CreateInvoiceLineWithVINNumber(string messageType, string jobNumber, string invoiceNumber, short invoiceLineNo, string tariffNumber, string vinNumber, ZDateTime createDate)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = messageType;
			declaration.JE_DeclarationReference = jobNumber;
			declaration.JE_SystemCreateTimeUtc = createDate;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = invoiceNumber;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = invoiceLineNo;
			invoiceLine.JI_Tariff = tariffNumber;
			invoiceLine.JI_VIN = vinNumber;
			return invoiceLine;
		}

		JobDeclarationForTesting GetJobDeclarationForTesting()
		{
			var dec = Factory.New<JobDeclarationForTesting>();
			dec.DisableDefaultPackingInformation = true;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return dec;
		}

		JobDeclaration CreateTwoEntryExportDeclaration()
		{
			var result = Factory.New<JobDeclaration>();
			result.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			result.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			result.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			result.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew().JI_CL = result.CustomsEntryHeaders[0].MergedLines[0].PK;
			result.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew().JI_CL = result.CustomsEntryHeaders[1].MergedLines[0].PK;
			return result;
		}
	}

	sealed class MergedDeclarationCreator : Customs.Business.Testing.MergedDeclarationCreator<JobDeclaration>
	{
		public MergedDeclarationCreator(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new CusEntryHeader Entry1
		{
			get { return (CusEntryHeader)base.Entry1; }
		}

		public new CusEntryLine EntryLine1
		{
			get { return (CusEntryLine)base.EntryLine1; }
		}

		public new JobComInvoiceLine InvoiceLine1
		{
			get { return (JobComInvoiceLine)base.InvoiceLine1; }
		}
	}

	sealed class TestHelper
	{
		public TestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public RefCurrency USDCurrency
		{
			get { return factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates); }
		}

		public RefCurrency EURCurrency
		{
			get { return factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.EuropeanUnion); }
		}

		public RefCurrency ZARCurrency
		{
			get { return factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.SouthAfrica); }
		}

		readonly BusinessObjectFactory factory;

		public void SetExchangeRate(RefCurrency currency, ZDecimal rate, ZDateTime effectiveDate)
		{
			var filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, currency.RX_Code);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, "CUS");

			var exchangeRate = factory.LoadTop1<RefExchangeRate>(filter);
			if (exchangeRate == null)
			{
				exchangeRate = factory.New<RefExchangeRate>();
				exchangeRate.RE_RX_NKExCurrency = currency.RX_Code;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = effectiveDate;
				exchangeRate.RE_ExpiryDate = effectiveDate.AddDays(1);
				exchangeRate.RE_ExRateType = "CUS";
			}
			exchangeRate.RE_SellRate = rate;
		}
	}

	sealed class JobComInvoiceLineForTesting : JobComInvoiceLine
	{
		public JobComInvoiceLineForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZShort JI_PreviousEntryLineNumberReturns { get; set; }

		public override ZShort JI_PreviousEntryLineNumber => JI_PreviousEntryLineNumberReturns;
	}

	sealed class JobDeclarationForTesting : JobDeclaration
	{
		public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString LocalCurrencyCodeCoreExposed
		{
			get { return LocalCurrencyCodeCore; }
		}

		public ZBool SupportValidateCustomsMessagingCoreExposed => SupportValidateCustomsMessagingCore;
	}

	sealed class RelatedDeclaration
	{
		public RelatedDeclaration(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public JobDeclaration GetRelatedImportDeclaration()
		{
			var importDec = factory.New<JobDeclaration>();
			importDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			importDec.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			importDec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			importDec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			importDec.Invoices.AddNew().JobComInvoiceLines.AddNew().JI_CL = importDec.CustomsEntryHeaders[0].MergedLines[0].PK;
			importDec.Invoices.AddNew().JobComInvoiceLines.AddNew().JI_CL = importDec.CustomsEntryHeaders[1].MergedLines[0].PK;
			importDec.CustomsEntryHeaders[0].EntryNumber = "9999991";
			importDec.CustomsEntryHeaders[1].EntryNumber = "9999992";
			factory.Save();

			return importDec;
		}

		readonly BusinessObjectFactory factory;
	}
}
