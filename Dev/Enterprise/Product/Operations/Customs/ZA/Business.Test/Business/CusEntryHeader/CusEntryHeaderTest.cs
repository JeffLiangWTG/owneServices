using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		public void TestSupportsBondedWarehousing()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "AA", "BB", ZString.Empty, "OP DESC1", ZAJobMessageTypeList.Codes.ExBond, outOfWarehouse: true);
			var procedure1 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "CC", "DD", ZString.Empty, "OP DESC2", ZAJobMessageTypeList.Codes.ExBond);
			procedure1.ZZ6_OutOfInwardProcessing = "Y";
			var procedure2 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "EE", "FF", ZString.Empty, "OP DESC3", ZAJobMessageTypeList.Codes.ExBond);
			procedure2.ZZ6_OutofOutwardProcessing = "Y";
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "GG", "HH", ZString.Empty, "OP DESC4", ZAJobMessageTypeList.Codes.Import, intoWarehouse: true);
			var procedure3 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "II", "JJ", ZString.Empty, "OP DESC5", ZAJobMessageTypeList.Codes.Import);
			procedure3.ZZ6_IntoInwardProcessing = "Y";
			var procedure4 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "KK", "LL", ZString.Empty, "OP DESC6", ZAJobMessageTypeList.Codes.Import);
			procedure4.ZZ6_IntoOutwardProcessing = "Y";
			helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, ZString.Empty, "MM", "NN", ZString.Empty, "OP DESC7", ZAJobMessageTypeList.Codes.ExBond);
			Factory.Save();

			var whshelper = new WhsDataTestHelper(Factory);
			whshelper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			whshelper.Warehouse2.CompanyData.OB_IMUsedBondedWhs = true;

			var declaration = Factory.New<JobDeclarationForTest>();
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_OA_Warehouse = whshelper.Warehouse.MainAddress.PK;
			entryInstruction.CEI_OA_Warehouse2 = whshelper.Warehouse2.MainAddress.PK;
			AssertSupportsBondedWarehousing(declaration, entryInstruction, "AA", "AABB", shouldSupport: true);
			AssertSupportsBondedWarehousing(declaration, entryInstruction, "CC", "CCDD", shouldSupport: false);
			AssertSupportsBondedWarehousing(declaration, entryInstruction, "EE", "EEFF", shouldSupport: false);
			AssertSupportsBondedWarehousing(declaration, entryInstruction, "GG", "GGHH", shouldSupport: true);
			AssertSupportsBondedWarehousing(declaration, entryInstruction, "II", "IIJJ", shouldSupport: false);
			AssertSupportsBondedWarehousing(declaration, entryInstruction, "KK", "KKLL", shouldSupport: false);
			AssertSupportsBondedWarehousing(declaration, entryInstruction, "MM", "MMNN", shouldSupport: false);
		}

		void AssertSupportsBondedWarehousing(JobDeclaration declaration, CusEntryInstruction entryInstruction, ZString style, ZString procedure, ZBool shouldSupport)
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_Style = style;
			invoiceLine.JI_Procedure = procedure;
			AssertEquals(shouldSupport, entryHeader.SupportsBondedWarehousing);
		}

		public void TestCustomLabelsConfigOrg()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertNull("Without a config org should return null", declaration.ConfigOrg);
			AssertNull("Without a config org should return null", ((ICustomLabelsConfigOrgProvider)entry).ConfigOrg);

			var importer = Factory.New<OrgHeader>();
			var supplier = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("ConfigOrg is Supplier", supplier.PK, declaration.ConfigOrg.PK);
			AssertEquals("ConfigOrg is Supplier", supplier.PK, ((ICustomLabelsConfigOrgProvider)entry).ConfigOrg.PK);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("ConfigOrg is Importer", importer.PK, declaration.ConfigOrg.PK);
			AssertEquals("ConfigOrg is Importer", importer.PK, ((ICustomLabelsConfigOrgProvider)entry).ConfigOrg.PK);
		}

		public void TestEntryNumberType()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.EntryNumber = "MRNOnly";
			Factory.InvalidateCachedProperties();
			AssertEntryNumberType(entryHeader);

			entryHeader.CusEntryNumber.Delete();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CustomsEntryHeaders.Add(entryHeader);
			entryHeader.EntryNumber = "MRNOnly";
			Factory.InvalidateCachedProperties();
			AssertEntryNumberType(entryHeader);

			entryHeader.CusEntryNumber.Delete();
			entryHeader.CH_MessageType = "I33";
			entryHeader.EntryNumber = "MRNOnly";
			Factory.InvalidateCachedProperties();
			AssertEntryNumberType(entryHeader);
		}

		void AssertEntryNumberType(CusEntryHeader entryHeader)
		{
			AssertEquals("Entry Type", CusEntryNumberTypes.Standard.MovementReferenceNumber, entryHeader.CusEntryNumber.CE_EntryType);
			AssertEquals("Entry Number", "MRNOnly", entryHeader.CusEntryNumber.CE_EntryNum);
		}

		public void TestICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			ICusCodeDataTypeSupporter supporter = entry;
			supporter.AssertType(null, CusCodeDataTypeList.Codes.VOCValueAfter);
			supporter.AssertType(typeof(VoucherOfCorrectionValueBefore), CusCodeDataTypeList.Codes.VOCValueBefore);
			supporter.AssertType(null, CusCodeDataTypeList.Codes.CaseNumber);
			supporter.AssertType(null, "ZZ!");

			var vocBefore = entry.VoucherOfCorrectionValueBefores.AddNew();
			vocBefore.CY_Value = 100m;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(vocBefore.PK);
			AssertEquals(typeof(VoucherOfCorrectionValueBefore), codeData.GetType());
		}

		public void TestBondedWarehousePropertiesWithDeletedInvoiceLine()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			var entry = helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
			entry.CH_BGMReference = "BOB";
			var declaration = entry.Declaration;
			var entryInstruction = entry.EntryInstruction;

			helper.AddInvoiceLine(declaration.Invoices[0], helper.Part, 100m, entryInstruction.PK, helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, "ENT3243", 1);
			declaration.DoMerge();

			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			entryInstruction.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			entryInstruction.InvoiceLines.First().Delete();

			CombineAssertions(() =>
			{
				AssertEquals("SupportsBondedWarehousing", expected: true, entry.SupportsBondedWarehousing);
				AssertEquals("IsInwardBondedWarehousingEnabled", expected: true, entry.IsInwardBondedWarehousingEnabled);
				AssertEquals("IsOutwardBondedWarehousingEnabled", expected: false, entry.IsOutwardBondedWarehousingEnabled);
				AssertEquals("HasLineGoingIntoABondedWarehouse", expected: true, entry.HasLineGoingIntoABondedWarehouse);
				AssertEquals("HasLineComingOutOfABondedWarehouse", expected: false, entry.HasLineComingOutOfABondedWarehouse);
				AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousing", expected: true, entry.HasAnInvoiceLineMarkedForBondedWarehousing);
				AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails", expected: true, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails);
				AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit", expected: false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutACountableQuantityOrUnit);
				AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit", expected: false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutAnInvoiceQuantityOrUnit);
				AssertEquals("HasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetails", expected: false, entry.HasAnInvoiceLineMarkedForBondedWarehousingWithoutEntryDetailsAndNonAssembledProduct);
			});
		}

		public void TestBondedWarehouseProperties()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			var entry = helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
			entry.CH_BGMReference = "BOB";
			var entryInstruction = entry.EntryInstruction;
			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			entryInstruction.CEI_Style = "DS";
			entryInstruction.CEI_Description = "HELLO";
			AssertBondedWarehouseProperties(entry, supportsBonded: false, isInwardBondedEnabled: false, isOutwardBondedEnabled: false);

			entryInstruction.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
			AssertBondedWarehouseProperties(entry, supportsBonded: true, isInwardBondedEnabled: true, isOutwardBondedEnabled: false);

			var declaration = entry.Declaration;
			helper.Importer.CompanyData.OB_IMUsedBondedWhs = false;
			AssertBondedWarehouseProperties(entry, supportsBonded: false, isInwardBondedEnabled: false, isOutwardBondedEnabled: false);

			helper.Warehouse.CompanyData.OB_IMUsedBondedWhs = true;
			AssertBondedWarehouseProperties(entry, supportsBonded: true, isInwardBondedEnabled: true, isOutwardBondedEnabled: false);

			var invoiceLine = entryInstruction.InvoiceLines.FirstOrDefault();
			invoiceLine.JI_Procedure = "";
			AssertBondedWarehouseProperties(entry, supportsBonded: false, isInwardBondedEnabled: false, isOutwardBondedEnabled: false);

			entryInstruction.CEI_Style = helper.OutwardCusProcedure.ZZ6_ProcedureCode;
			invoiceLine.JI_Procedure = helper.OutwardCusProcedure.ZZ6_ProcedureCode + helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode;
			AssertBondedWarehouseProperties(entry, supportsBonded: false, isInwardBondedEnabled: false, isOutwardBondedEnabled: false);

			entryInstruction.CEI_OA_Warehouse = helper.Warehouse.MainAddress.PK;
			AssertBondedWarehouseProperties(entry, supportsBonded: true, isInwardBondedEnabled: false, isOutwardBondedEnabled: true);

			invoiceLine.JI_Procedure = "";
			AssertBondedWarehouseProperties(entry, supportsBonded: false, isInwardBondedEnabled: false, isOutwardBondedEnabled: false);

			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			entryInstruction.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;
			invoiceLine.JI_Procedure = helper.InwardCusProcedure.ZZ6_ProcedureCode + helper.InwardCusProcedure.ZZ6_PreviousProcedureCode;
			AssertBondedWarehouseProperties(entry, supportsBonded: true, isInwardBondedEnabled: true, isOutwardBondedEnabled: false);

			entry.EntryNumber = ZString.Empty;
			AssertBondedWarehouseProperties(entry, supportsBonded: true, isInwardBondedEnabled: true, isOutwardBondedEnabled: false);

			declaration.JE_OH_Importer = ZGuid.Empty;
			var changeOfOwnershipCusProcedure = helper.ChangeOfOwnershipCusProcedure;
			entryInstruction.CEI_Style = changeOfOwnershipCusProcedure.ZZ6_ProcedureCode;
			invoiceLine.JI_Procedure = $"{changeOfOwnershipCusProcedure.ZZ6_ProcedureCode}{changeOfOwnershipCusProcedure.ZZ6_PreviousProcedureCode}";
			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			entryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
			AssertEquals("SupportsBondedWarehousing", expected: false, entry.SupportsBondedWarehousing);
			entryInstruction.CEI_OH_Owner = helper.Importer.PK;
			AssertEquals("SupportsBondedWarehousing", expected: true, entry.SupportsBondedWarehousing);
			invoiceLine.JI_Procedure = "xx$$";
			AssertEquals("SupportsBondedWarehousing", expected: false, entry.SupportsBondedWarehousing);
		}

		void AssertBondedWarehouseProperties(CusEntryHeader entryHeader, bool supportsBonded, bool isInwardBondedEnabled, bool isOutwardBondedEnabled)
		{
			AssertEquals("SupportsBondedWarehousing", supportsBonded, entryHeader.SupportsBondedWarehousing);
			AssertEquals("IsInwardBondedWarehousingEnabled", isInwardBondedEnabled, entryHeader.IsInwardBondedWarehousingEnabled);
			AssertEquals("IsOutwardBondedWarehousingEnabled", isOutwardBondedEnabled, entryHeader.IsOutwardBondedWarehousingEnabled);
		}

		public void TestAmountDue()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invLine1 = invoiceHeader.InvoiceLines.AddNew();
			invLine1.JI_ZZF_NKTaxType = "VAT";
			var invLine2 = invoiceHeader.InvoiceLines.AddNew();
			invLine2.JI_ZZF_NKTaxType = "VAT";

			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate("1P1", 10m);
			entryLine1.Fees.AddOrUpdate("13A", 20m);
			entryLine1.Fees.AddOrUpdate("13C", 30m);
			entryLine1.Fees.AddOrUpdate("15B", 40m);
			entryLine1.Fees.AddOrUpdate("13D", 50m);
			entryLine1.Fees.AddOrUpdate("13B", 60m);
			entryLine1.Fees.AddOrUpdate("15A", 70m);
			entryLine1.Fees.AddOrUpdate("VAT", 80m);
			entryLine1.Fees.AddOrUpdate("PPA", 90m);
			entryLine1.InvoiceLines.Add(invLine1);

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate("1P1", 11m);
			entryLine2.Fees.AddOrUpdate("13A", 21m);
			entryLine2.Fees.AddOrUpdate("13C", 31m);
			entryLine2.Fees.AddOrUpdate("15B", 41m);
			entryLine2.Fees.AddOrUpdate("13D", 51m);
			entryLine2.Fees.AddOrUpdate("13B", 61m);
			entryLine2.Fees.AddOrUpdate("15A", 81m);
			entryLine2.Fees.AddOrUpdate("VAT", 91m);
			entryLine2.ProvisionalPayments.AddNew("PPA", 13.00m);
			entryLine2.ProvisionalPayments.AddNew("PPA", 23.00m);
			entryLine2.ProvisionalPayments.AddNew("PPC", 33.00m);
			entryLine2.ProvisionalPayments.AddNew("PEN", 43.11m);
			entryLine2.ProvisionalPayments.AddNew("FOR", 53.22m);
			entryLine2.ProvisionalPayments.AddNew("XXX", 63.00m);
			entryLine2.InvoiceLines.Add(invLine2);

			entry.S1P2BDutyBefore = 1m;
			entry.CustomsDutyExcluding12BBefore = 2m;
			entry.ValueAddedTaxBefore = 3m;
			entry.ProvisionalPaymentAmountBefore = 4m;
			entry.PenaltyAmountBefore = 5m;

			CombineAssertions(() =>
			{
				AssertEquals(15m, entry.AmountDueBefore);
				AssertEquals("DTY", 577m, entry.CustomsDutyExcluding12BAfter);
				AssertEquals("12B", 0m, entry.S1P2BDutyAfter);
				AssertEquals("VAT", 171m, entry.ValueAddedTax);
				AssertEquals("PRP", 69m, entry.ProvisionalPaymentAmountAfter);
				AssertEquals("PEN", 96.33m, entry.PenaltyAmountAfter);
				AssertEquals("AmountDue", 913.33m, entry.AmountDueAfter);
			});
		}

		public void TestVOCBeforeValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "36";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertVOCBeforeValues(entry, cifValueReadOnly: true, ZDecimal.Zero, customsValueReadOnly: true, ZDecimal.Zero, dutyExcluding12BReadonly: true, ZDecimal.Zero, s1P2BDutyReadOnly: true, ZDecimal.Zero, valueAddedTaxReadOnly: true, ZDecimal.Zero);

			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			AssertVOCBeforeValues(entry, cifValueReadOnly: false, ZDecimal.Zero, customsValueReadOnly: false, ZDecimal.Zero, dutyExcluding12BReadonly: false, ZDecimal.Zero, s1P2BDutyReadOnly: false, ZDecimal.Zero, valueAddedTaxReadOnly: false, ZDecimal.Zero);

			entry.CIFValueBefore = 1m;
			entry.CustomsValueBefore = 2m;
			entry.CustomsDutyExcluding12BBefore = 3m;
			entry.S1P2BDutyBefore = 4m;
			entry.ValueAddedTaxBefore = 5m;
			entry.MovementReferenceNumberSetter("N432", ZDateTime.Today);
			AssertVOCBeforeValues(entry, cifValueReadOnly: true, 1m, customsValueReadOnly: true, 2m, dutyExcluding12BReadonly: true, 3m, s1P2BDutyReadOnly: true, 4m, valueAddedTaxReadOnly: true, 5m);

			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertVOCBeforeValues(entry, cifValueReadOnly: true, 1m, customsValueReadOnly: true, 2m, dutyExcluding12BReadonly: true, 3m, s1P2BDutyReadOnly: true, 4m, valueAddedTaxReadOnly: true, 5m);

			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			entry.MovementReferenceNumberSetter(ZString.Empty, ZDateTime.Today);
			AssertVOCBeforeValues(entry, cifValueReadOnly: false, 1m, customsValueReadOnly: false, 2m, dutyExcluding12BReadonly: false, 3m, s1P2BDutyReadOnly: false, 4m, valueAddedTaxReadOnly: false, 5m);

			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			AssertVOCBeforeValues(entry, cifValueReadOnly: true, ZDecimal.Zero, customsValueReadOnly: true, ZDecimal.Zero, dutyExcluding12BReadonly: true, ZDecimal.Zero, s1P2BDutyReadOnly: true, ZDecimal.Zero, valueAddedTaxReadOnly: true, ZDecimal.Zero);
		}

		void AssertVOCBeforeValues(CusEntryHeader entryHeader, bool cifValueReadOnly, decimal cifValue, bool customsValueReadOnly, decimal customsValue, bool dutyExcluding12BReadonly, decimal dutyExcluding12B, bool s1P2BDutyReadOnly, decimal s1P2BDuty, bool valueAddedTaxReadOnly, decimal valueAddedTax)
		{
			AssertEquals("entry.CIFValueBeforeInfo.ReadOnly", cifValueReadOnly, entryHeader.CIFValueBeforeInfo.ReadOnly);
			AssertEquals("entry.CIFValueBefore", cifValue, entryHeader.CIFValueBefore);
			AssertEquals("entry.CustomsValueBeforeInfo.ReadOnly", customsValueReadOnly, entryHeader.CustomsValueBeforeInfo.ReadOnly);
			AssertEquals("entry.CustomsValueBefore", customsValue, entryHeader.CustomsValueBefore);
			AssertEquals("entry.CustomsDutyExcluding12BBeforeInfo.ReadOnly", dutyExcluding12BReadonly, entryHeader.CustomsDutyExcluding12BBeforeInfo.ReadOnly);
			AssertEquals("entry.CustomsDutyExcluding12BBefore", dutyExcluding12B, entryHeader.CustomsDutyExcluding12BBefore);
			AssertEquals("entry.S1P2BDutyBeforeInfo.ReadOnly", s1P2BDutyReadOnly, entryHeader.S1P2BDutyBeforeInfo.ReadOnly);
			AssertEquals("entry.S1P2BDutyBefore", s1P2BDuty, entryHeader.S1P2BDutyBefore);
			AssertEquals("entry.ValueAddedTaxBeforeInfo.ReadOnly", valueAddedTaxReadOnly, entryHeader.ValueAddedTaxBeforeInfo.ReadOnly);
			AssertEquals("entry.ValueAddedTaxBefore", valueAddedTax, entryHeader.ValueAddedTaxBefore);
		}

		public void TestProvisionalPaymentAndPenaltyBeforeAndAfterValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var customsShutterUpperer = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = customsShutterUpperer;

			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "36";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertProvisionalPaymentAndPenaltyBeforeAndAfterValuesAckToOverwriteePPValues(entry, entryInstruction, customsShutterUpperer);
			AssertProvisionalPaymentAndPenaltyBeforeAndAfterValuesDisableOverwritingePPValues(entry, entryInstruction, customsShutterUpperer);
			AssertProvisionalPaymentAndPenaltyBeforeAndAfterValuesDefaultePPAfterValues(entry, entryInstruction, entryLine, customsShutterUpperer);
		}

		void AssertProvisionalPaymentAndPenaltyBeforeAndAfterValuesAckToOverwriteePPValues(CusEntryHeader entry, CusEntryInstruction entryInstruction, SendsMessagesToCustomsShutterUpperer customsShutterUpperer)
		{
			CombineAssertions("User acknowledged to overwrite ePP values", () =>
			{
				customsShutterUpperer.AnswerToContinueWithAction = true;
				entry.IsOverwriteProvisionalPaymentAfterValues = true;

				entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
				AssertEquals(expected: false, entry.PenaltyAmountBeforeInfo.ReadOnly);
				AssertEquals(expected: false, entry.PenaltyAmountAfterInfo.ReadOnly);
				AssertEquals(expected: false, entry.ProvisionalPaymentAmountBeforeInfo.ReadOnly);
				AssertEquals(expected: false, entry.ProvisionalPaymentAmountAfterInfo.ReadOnly);

				AssertEquals(0m, entry.PenaltyAmountBefore);
				AssertEquals(0m, entry.PenaltyAmountAfter);
				AssertEquals(0m, entry.PenaltyAmountDifference);
				AssertEquals(0m, entry.ProvisionalPaymentAmountBefore);
				AssertEquals(0m, entry.ProvisionalPaymentAmountAfter);
				AssertEquals(0m, entry.ProvisionalPaymentAmountDifference);

				entryInstruction.CEI_DateForDuty = ZDateTime.Today;
				AssertEquals(expected: false, entry.PenaltyAmountBeforeInfo.ReadOnly);
				AssertEquals(expected: false, entry.PenaltyAmountAfterInfo.ReadOnly);
				AssertEquals(expected: false, entry.ProvisionalPaymentAmountBeforeInfo.ReadOnly);
				AssertEquals(expected: false, entry.ProvisionalPaymentAmountAfterInfo.ReadOnly);

				entry.PenaltyAmountBefore = 1m;
				entry.PenaltyAmountAfter = 2m;
				entry.ProvisionalPaymentAmountBefore = 3m;
				entry.ProvisionalPaymentAmountAfter = 4m;
				AssertEquals(1m, entry.PenaltyAmountBefore);
				AssertEquals(2m, entry.PenaltyAmountAfter);
				AssertEquals(1m, entry.PenaltyAmountDifference);
				AssertEquals(3m, entry.ProvisionalPaymentAmountBefore);
				AssertEquals(4m, entry.ProvisionalPaymentAmountAfter);
				AssertEquals(1m, entry.ProvisionalPaymentAmountDifference);
			});
		}

		void AssertProvisionalPaymentAndPenaltyBeforeAndAfterValuesDisableOverwritingePPValues(CusEntryHeader entry, CusEntryInstruction entryInstruction, SendsMessagesToCustomsShutterUpperer customsShutterUpperer)
		{
			CombineAssertions("Disable overwriting ePP values", () =>
			{
				customsShutterUpperer.AnswerToContinueWithAction = false;
				entry.IsOverwriteProvisionalPaymentAfterValues = false;

				entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
				AssertEquals(expected: true, entry.PenaltyAmountBeforeInfo.ReadOnly);
				AssertEquals(expected: true, entry.PenaltyAmountAfterInfo.ReadOnly);
				AssertEquals(expected: true, entry.ProvisionalPaymentAmountBeforeInfo.ReadOnly);
				AssertEquals(expected: true, entry.ProvisionalPaymentAmountAfterInfo.ReadOnly);

				AssertEquals(0m, entry.PenaltyAmountBefore);
				AssertEquals(0m, entry.PenaltyAmountAfter);
				AssertEquals(0m, entry.PenaltyAmountDifference);
				AssertEquals(0m, entry.ProvisionalPaymentAmountBefore);
				AssertEquals(0m, entry.ProvisionalPaymentAmountAfter);
				AssertEquals(0m, entry.ProvisionalPaymentAmountDifference);

				entryInstruction.CEI_DateForDuty = ZDateTime.Today;
				AssertEquals(expected: false, entry.PenaltyAmountBeforeInfo.ReadOnly);
				AssertEquals(expected: true, entry.PenaltyAmountAfterInfo.ReadOnly);
				AssertEquals(expected: false, entry.ProvisionalPaymentAmountBeforeInfo.ReadOnly);
				AssertEquals(expected: true, entry.ProvisionalPaymentAmountAfterInfo.ReadOnly);

				entry.PenaltyAmountBefore = 1m;
				entry.ProvisionalPaymentAmountBefore = 2m;
				AssertEquals(1m, entry.PenaltyAmountBefore);
				AssertEquals(0m, entry.PenaltyAmountAfter);
				AssertEquals(-1m, entry.PenaltyAmountDifference);
				AssertEquals(2m, entry.ProvisionalPaymentAmountBefore);
				AssertEquals(0m, entry.ProvisionalPaymentAmountAfter);
				AssertEquals(-2m, entry.ProvisionalPaymentAmountDifference);
			});
		}

		void AssertProvisionalPaymentAndPenaltyBeforeAndAfterValuesDefaultePPAfterValues(CusEntryHeader entry, CusEntryInstruction entryInstruction, CusEntryLine entryLine, SendsMessagesToCustomsShutterUpperer customsShutterUpperer)
		{
			CombineAssertions("Default ePP after values", () =>
			{
				customsShutterUpperer.AnswerToContinueWithAction = true;
				entry.IsOverwriteProvisionalPaymentAfterValues = true;

				entryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 12.3m);
				entryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 23.4m);
				entryInstruction.CEI_DateForDuty = ZDateTime.Today;
				AssertEquals(12.3m, entry.PenaltyAmountAfter);
				AssertEquals(23.4m, entry.ProvisionalPaymentAmountAfter);

				entry.PenaltyAmountAfter = 34.5m;
				entry.ProvisionalPaymentAmountAfter = 45.6m;
				AssertEquals(34.5m, entry.PenaltyAmountAfter);
				AssertEquals(45.6m, entry.ProvisionalPaymentAmountAfter);

				entry.PenaltyAmountAfter = 0m;
				entry.ProvisionalPaymentAmountAfter = 0m;
				AssertEquals(0m, entry.PenaltyAmountAfter);
				AssertEquals(0m, entry.ProvisionalPaymentAmountAfter);

				entry.IsOverwriteProvisionalPaymentAfterValues = false;
				AssertEquals(12.3m, entry.PenaltyAmountAfter);
				AssertEquals(23.4m, entry.ProvisionalPaymentAmountAfter);
			});
		}

		public void TestVOCValues_40()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "40";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var fee1 = entryLine.Fees.AddOrUpdate("1P1", 100m);
			var fee2 = entryLine.Fees.AddOrUpdate("12B", 150m);
			var fee3 = entryLine.Fees.AddOrUpdate("VAT", 200m);
			fee1.CF_IsLandedCostOnly = true;
			fee2.CF_IsLandedCostOnly = true;
			fee3.CF_IsLandedCostOnly = true;
			AssertVOCValues(entry, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero);
			fee1.CF_IsLandedCostOnly = false;
			fee2.CF_IsLandedCostOnly = false;
			fee3.CF_IsLandedCostOnly = false;
			AssertVOCValues(entry, 100m, 150m, 200m);
		}

		public void TestVOCValues_DoNotClaimVATRefund()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "11";
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entry.MergedLines.AddNew();
			entryLine.InvoiceLines.Add(invoiceLine);
			entryLine.Fees.AddOrUpdate("1P1", 100m);
			entryLine.Fees.AddOrUpdate("12B", 150m);
			entryLine.Fees.AddOrUpdate("VAT", 200m);
			AssertVOCValues(entry, 100m, 150m, 200m);

			entry.ValueAddedTaxBefore = 300m;
			AssertEquals("entry.ValueAddedTax no ticked", 200m, entry.ValueAddedTax);
			AssertEquals(expected: false, entry.DoNotClaimVATRefund_ReadOnly);
			entry.DoNotClaimVATRefund = true;
			AssertEquals("entry.ValueAddedTax ticked", 300m, entry.ValueAddedTax);
			AssertEquals(expected: false, entry.DoNotClaimVATRefund_ReadOnly);
			entry.ValueAddedTaxBefore = 400m;
			AssertEquals("entry.ValueAddedTax ticked", 400m, entry.ValueAddedTax);
			AssertEquals(expected: false, entry.DoNotClaimVATRefund_ReadOnly);
			entry.ValueAddedTaxBefore = 150m;
			AssertEquals("entry.ValueAddedTax NOT eligible for claim", 200m, entry.ValueAddedTax);
			AssertEquals("entry.DoNoClaimVATRefund", expected: false, entry.DoNotClaimVATRefund);
			AssertEquals(expected: true, entry.DoNotClaimVATRefund_ReadOnly);
			entry.ValueAddedTaxBefore = 200m;
			AssertEquals(expected: false, entry.DoNotClaimVATRefund_ReadOnly);
		}

		void AssertVOCValues(CusEntryHeader entry, decimal customsDutyExcluding12BAfter, decimal s1P2BDutyAfter, decimal valueAddedTax)
		{
			AssertEquals("entry.CustomsDutyExcluding12BAfter", customsDutyExcluding12BAfter, entry.CustomsDutyExcluding12BAfter);
			AssertEquals("entry.S1P2BDutyAfter", s1P2BDutyAfter, entry.S1P2BDutyAfter);
			AssertEquals("entry.ValueAddedTax", valueAddedTax, entry.ValueAddedTax);
		}

		public void TestEntryNumberLookupFallback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertEquals("EntryNumber", ZString.Empty, entryHeader.EntryNumber);
			entryHeader.EntryNumber = "ENTA";
			AssertEquals("EntryNumber", "ENTA", entryHeader.EntryNumber);
			var number = entryHeader.CusEntryNumber;
			AssertEquals("CE_EntryNum", "ENTA", number.CE_EntryNum);
			AssertEquals("CE_EntryType", CusEntryNumberTypes.Standard.MovementReferenceNumber, number.CE_EntryType);
			number.Delete();
			CreateCusEntryNumber(entryHeader, ZAJobMessageTypeList.Codes.Import, "ENT1", days: -1);
			CreateCusEntryNumber(entryHeader, ZAJobMessageTypeList.Codes.Import, "ENT2", days: -2);
			CreateCusEntryNumber(entryHeader, ZAJobMessageTypeList.Codes.Export, "ENT3", days: -1);
			CreateCusEntryNumber(entryHeader, ZAJobMessageTypeList.Codes.Export, "ENT4", days: -2);
			AssertEquals("EntryNumber", "ENT2", entryHeader.EntryNumber);
			declaration.CustomsEntryHeaders.Add(entryHeader);
			AssertEquals("EntryNumber", "ENT4", entryHeader.EntryNumber);
			CreateCusEntryNumber(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "ENT5", days: -1);
			CreateCusEntryNumber(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "ENT6", days: -2);
			AssertEquals("EntryNumber", "ENT6", entryHeader.EntryNumber);
			declaration.JE_MessageType = ZString.Empty;
			AssertEquals("EntryNumber", "ENT6", entryHeader.EntryNumber);
		}

		public void TestCustomsDuty()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine1.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 200.0m;
			entryLine2.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 410.0m;
			entryLine1.Fees.GetOrAddFeeByFeeType("1P1").CF_ChargeAmount = 20.0m;
			entryLine2.Fees.GetOrAddFeeByFeeType("1P1").CF_ChargeAmount = 41.0m;
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals(61.0m, entryHeader.CustomsDuty);
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			AssertEquals(610.0m, entryHeader.CustomsDuty);
		}

		public void TestCustomsChargesIncludesSumOfInvoiceLineFees()
		{
			var add = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.AntiDumping, "AntiDumping");
			testHelper.LoadOrCreateNewCusRateCode(Factory, Constants.RateTypes.AntiDumping, add.PK);
			var dty = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			testHelper.LoadOrCreateNewCusRateCode(Factory, Constants.RateTypes.Duty, dty.PK);
			var lvy = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Levy, "Levy");
			testHelper.LoadOrCreateNewCusRateCode(Factory, Constants.RateTypes.Levy, lvy.PK);
			var exc = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Excise, "Excise");
			testHelper.LoadOrCreateNewCusRateCode(Factory, Constants.RateTypes.Excise, exc.PK);
			Factory.Save();

			var header = (CusEntryHeader)GetNewBusinessObject();
			var line1 = header.MergedLines.AddNew();
			var line2 = header.MergedLines.AddNew();
			line1.Fees.AddOrUpdate(Constants.RateTypes.AntiDumping, 10);
			line1.Fees.AddOrUpdate(Constants.RateTypes.Duty, 20);
			line1.Fees.AddOrUpdate(Constants.RateTypes.Levy, -1);
			line1.Fees.AddOrUpdate(Constants.RateTypes.Excise, 0);
			line2.Fees.AddOrUpdate(Constants.RateTypes.AntiDumping, 100);
			line2.Fees.AddOrUpdate(Constants.RateTypes.Duty, 200);
			line2.Fees.AddOrUpdate(Constants.RateTypes.Levy, -2);
			line2.Fees.AddOrUpdate(Constants.RateTypes.Excise, 0);

			AssertEquals("AntiDumping", new ZDecimal(110), FindCustomsCharge(header, "Anti-Dumping").Amount);
			AssertEquals("CustomsDuty", new ZDecimal(220), FindCustomsCharge(header, "Duty").Amount);
			AssertEquals("EnvironmentalLevy", new ZDecimal(-3), FindCustomsCharge(header, "Levy").Amount);
			AssertNull("Should not include zero fees", FindCustomsCharge(header, "Excise"));
		}

		public void TestCompareTo()
		{
			var entry1 = Factory.New<CusEntryHeader>();
			entry1.CH_BGMReference = "1";
			var entry2 = Factory.New<CusEntryHeader>();
			entry2.CH_BGMReference = "2";

			AssertEquals(0, entry1.CompareTo(entry1));
			AssertEquals(-1, entry1.CompareTo(entry2));
			AssertEquals(1, entry2.CompareTo(entry1));
		}

		public void TestZALoadForBGMReference()
		{
			var testDec1 = Factory.New<JobDeclaration>();
			var testHeader1 = testDec1.CustomsEntryHeaders.AddNew();
			var testDec2 = Factory.New<JobDeclaration>();
			var testHeader2 = testDec2.CustomsEntryHeaders.AddNew();
			Factory.Save();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			testHeader1.Logs.AddNew(Events.AddedARecordToTheSystem, ZDateTimeOffset.Today.AddDays(-1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			testHeader2.Logs.AddNew(Events.AddedARecordToTheSystem, ZDateTimeOffset.Today);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			string serialNumber = testHeader1.CH_BGMReference;
			testHeader2.CH_BGMReference = serialNumber;
			var result = CusEntryHeader.LoadForBGMReference(Factory, serialNumber);
			AssertEquals("Most recent header will be TestHeader2", testHeader2, result);
		}

		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
			var testDec = Factory.New<JobDeclaration>();
			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryHeader.Delete();
		}

		public void TestIfVDNIsSpecified_EffectiveValuationCodeIsEmpty()
		{
			var importer = OrgHeader.New(Factory);
			importer.OH_RL_NKClosestPort = "ZAAAM";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;

			var header = declaration.Invoices.AddNew();
			var supplier = OrgHeader.New(Factory);
			var link = supplier.BuyerLinks.AddNew(importer);
			link.OL_ValuationBasisDeterminationNum = "VDN";
			header.JZ_OH_Supplier = supplier.PK;
			AssertEquals("VDN", header.JZ_VDN);
			AssertEquals(ZString.Empty, header.JZ_ValuationCode);
		}

		public void TestReleaseAgentCodeAndHouseBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "11";
			var forwarder = OrgHeader.New(Factory);
			forwarder.LocalReleaseAgentCode = "12345678";
			declaration.JE_HouseBill = "1234";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = testInstruction.PK;
			var lCLContainer = declaration.CusContainers.AddNew();
			lCLContainer.CO_FCL_LCL_AIR = CusContainer.ContainerModes.LessContainerLoad;
			DoMerge(declaration);

			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("1234", entryHeader.ReleaseAgentCodeAndHouseBill);
			declaration.JE_OH_Forwarder = forwarder.PK;
			AssertEquals("123456781234", entryHeader.ReleaseAgentCodeAndHouseBill);
			lCLContainer.CO_FCL_LCL_AIR = "FCG";
			AssertEquals("123456781234", entryHeader.ReleaseAgentCodeAndHouseBill);
			lCLContainer.CO_FCL_LCL_AIR = "FCL";
			AssertEquals("1234", entryHeader.ReleaseAgentCodeAndHouseBill);
		}

		public void TestUniqueConsignmentReference()
		{
			var entry = Factory.New<CusEntryHeader>();
			var entryNum = CusEntryNumber.New(entry, CusEntryNumberTypes.Standard.UniqueConsignementReference, Core.Constants.CountryCodes.SouthAfrica);
			entryNum.CE_EntryNum = "TESTUCR";
			AssertEquals("TESTUCR", entry.UniqueConsignmentReference);
		}

		public void TestImportControlNumber()
		{
			var entry = Factory.New<CusEntryHeader>();
			var entryNum = CusEntryNumber.New(entry, CusEntryNumberTypes.Standard.ImportControlNumber, Core.Constants.CountryCodes.SouthAfrica);
			entryNum.CE_EntryNum = "TESTICN";
			AssertEquals("TESTICN", entry.ImportControlNumber);
		}

		public void TestEndorsementsNote()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MergedLines.AddNew();
			entryHeader.Endorsements = "test notes";
			var notes = entryHeader.Notes.FindByDescription(PredefinedNoteTypes.Instance.ZAEndorsement.Description);
			AssertEquals("Note exists", 1, notes.Length);
			AssertEquals("Note text is not empty", "test notes", notes[0].ST_NoteText);
			notes[0].ST_NoteText = "update note text";
			AssertEquals("Updated", "update note text", entryHeader.Endorsements);
		}

		public void TestIsAwaitingResponse()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_Status = ZAMessageStatusList.Codes.AwaitingResponse;
			Assert(entryHeader.IsAwaitingResponse);
			entryHeader.CH_Status = ZAMessageStatusList.Codes.Acknowledged;
			AssertEquals(expected: false, entryHeader.IsAwaitingResponse);
		}

		public void TestBGMReferenceIsSetWhenSavingIfItHasASpecialValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.NeedsNewBGMReference = true;
			Factory.Save();
			AssertEquals(expected: true, entryHeader.CH_BGMReference.IsEmpty);

			entryHeader.CH_BGMReference = "123";
			Factory.Save();
			AssertEquals(expected: true, entryHeader.CH_BGMReference.Equals("123"));
		}

		public void TestOnSavingSetsLocalReferenceNumber()
		{
			var declaration = SetupOnSavingSetsLocalReferenceNumber();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var expected = ZString.Empty;
			AssertEquals("CH_BGMReference", expected, entryHeader.CH_BGMReference);

			declaration.JE_CustomsOffice = "OFI";
			AssertEquals("CH_BGMReference", expected, entryHeader.CH_BGMReference);

			var agentOrg = CreateOrgHeader("AgentName", "AGENTCODE", "33344455", isConsignee: false);
			declaration.JE_CustomsOffice = "CFE";
			declaration.JE_OH_AgentOverride = agentOrg.PK;
			Factory.Save();

			Assert("CH_BGMReference", entryHeader.CH_BGMReference.StartsWith(declaration.AgentCode + declaration.JE_CustomsOffice));
			expected = entryHeader.CH_BGMReference;

			entryHeader.NeedsNewBGMReference = true;
			entryHeader.CH_Status = ZAMessageStatusList.Codes.AwaitingResponse;
			Factory.Save();
			AssertEquals("CH_BGMReference NOT changed", expected, entryHeader.CH_BGMReference);
			entryHeader.NeedsNewBGMReference = true;
			entryHeader.CH_EntryStatus = "6";
			Factory.Save();
			AssertNotEquals("CH_BGMReference regenerated", expected, entryHeader.CH_BGMReference);
			expected = entryHeader.CH_BGMReference;
			entryHeader.NeedsNewBGMReference = true;
			entryHeader.MovementReferenceNumberSetter("MRN", ZDateTime.Today);
			Factory.Save();
			AssertEquals("CH_BGMReference NOT changed", expected, entryHeader.CH_BGMReference);
		}

		public void TestOnSavingSetsLocalReferenceNumber_WithOverrideCustomsOffice()
		{
			var declaration = SetupOnSavingSetsLocalReferenceNumber();
			CombineAssertions(() =>
			{
				AssertEquals(1, declaration.CustomsEntryHeaders.Count);
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("CH_BGMReference: AgentCode is empty", ZString.Empty, entryHeader.CH_BGMReference);
				var agentOrg = CreateOrgHeader("AgentName", "AGENTCODE", "33344455", isConsignee: false);
				declaration.JE_CustomsOffice = "CFE";
				declaration.JE_OH_AgentOverride = agentOrg.PK;
				var instruction = declaration.CustomsEntryInstructions[0];
				instruction.CEI_CustomsOfficeOverride = "CNT";
				Factory.Save();
				Assert("CH_BGMReference: has CEI_CustomsOfficeOverride", entryHeader.CH_BGMReference.StartsWith(declaration.AgentCode + instruction.CEI_CustomsOfficeOverride));
				var oldBGMReference = entryHeader.CH_BGMReference;
				entryHeader.NeedsNewBGMReference = true;
				entryHeader.CH_Status = ZAMessageStatusList.Codes.AwaitingResponse;
				Factory.Save();
				AssertEquals("CH_BGMReference NOT changed: message has been sent", oldBGMReference, entryHeader.CH_BGMReference);
				entryHeader.NeedsNewBGMReference = true;
				entryHeader.CH_EntryStatus = "6";
				Factory.Save();
				AssertNotEquals("CH_BGMReference regenerated: IsStatusRejected", oldBGMReference, entryHeader.CH_BGMReference);
				oldBGMReference = entryHeader.CH_BGMReference;
				instruction.CEI_CustomsOfficeOverride = "JSH";
				Factory.Save();
				AssertNotEquals("CH_BGMReference regenerated: CEI_CustomsOfficeOverride reset", oldBGMReference, entryHeader.CH_BGMReference);
				Assert("CH_BGMReference: new CEI_CustomsOfficeOverride value", entryHeader.CH_BGMReference.StartsWith(declaration.AgentCode + instruction.CEI_CustomsOfficeOverride));
				oldBGMReference = entryHeader.CH_BGMReference;
				entryHeader.NeedsNewBGMReference = true;
				entryHeader.MovementReferenceNumberSetter("MRN", ZDateTime.Today);
				Factory.Save();
				AssertEquals("CH_BGMReference NOT changed: MRN not empty", oldBGMReference, entryHeader.CH_BGMReference);
			});
		}

		JobDeclaration SetupOnSavingSetsLocalReferenceNumber()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("6");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var invoice1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice1.JZ_InvoiceAmount = 170m;
			invoice1.JZ_IncoTerm = "FOB";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 13.45M;
			var line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 156.78M;
			DoMerge(declaration);
			Factory.Save();
			return declaration;
		}

		public void TestCustomsOffice()
		{
			CombineAssertions(() =>
			{
				var entryHeader = Factory.New<CusEntryHeader>();
				AssertEquals("No linked Declaration", ZString.Empty, entryHeader.CustomsOffice);

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_CustomsOffice = "JSA";
				entryHeader.CH_JE = declaration.PK;
				AssertEquals("No linked Instruction then fallback JE_CustomsOffice", "JSA", entryHeader.CustomsOffice);

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				AssertEquals("Has linked Instruction but empty CEI_CustomsOfficeOverride then still fallback JE_CustomsOffice", "JSA", entryHeader.CustomsOffice);

				instruction.CEI_CustomsOfficeOverride = "CNT";
				AssertEquals("Has linked Instruction and has CEI_CustomsOfficeOverride", "CNT", entryHeader.CustomsOffice);
			});
		}

		public void TestPortOfExitForExport()
		{
			var entryHeader = SetupPortOfExit(ZAJobMessageTypeList.Codes.Export);
			AssertEquals("JSA", entryHeader.PortOfExit);
		}

		public void TestPortOfExitForImport()
		{
			var entryHeader = SetupPortOfExit(ZAJobMessageTypeList.Codes.Import);
			AssertEquals("JSA", entryHeader.PortOfExit);
		}

		CusEntryHeader SetupPortOfExit(string messageType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_PortOfExit = "JSA";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			new LineMerger(declaration).DoMerge();
			var entryHeader = declaration.ActiveEntryHeaders[0];
			return entryHeader;
		}

		public void TestAddClearedEventToLog_Import()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testEntry = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			AssertAddClearedEventToLog(declaration, testEntry, "Pre-Check");

			testEntry.CH_Status = "ACK";
			Factory.Save();
			AssertAddClearedEventToLog(declaration, testEntry, "Change CH_Status won't trigger");

			testEntry.CH_Status = "";
			testEntry.CH_EntryStatus = "ACK";
			Factory.Save();
			AssertAddClearedEventToLog(declaration, testEntry, "Invalid CH_EntryStatus won't trigger");

			CombineAssertions("correct CH_EntryStatus will trigger", () =>
			{
				testEntry.CH_EntryStatus = "1";
				Factory.Save();
				AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared));
				AssertNotNull(testEntry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				AssertNull(testEntry.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared));
			});
		}

		public void TestAddClearedEventToLog_Export()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var testEntry = declaration.ActiveEntryHeaders.AddNew();
			Factory.Save();

			AssertAddClearedEventToLog(declaration, testEntry, "Pre-Check");
			testEntry.CH_Status = "ACK";
			Factory.Save();
			AssertAddClearedEventToLog(declaration, testEntry, "Change CH_Status won't trigger");
			testEntry.CH_Status = "";
			testEntry.CH_EntryStatus = "ACK";
			Factory.Save();
			AssertAddClearedEventToLog(declaration, testEntry, "Invalid CH_EntryStatus won't trigger");

			CombineAssertions("correct CH_EntryStatus will trigger", () =>
			{
				testEntry.CH_EntryStatus = "1";
				Factory.Save();
				AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared));
				AssertNull(testEntry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				AssertNotNull(testEntry.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared));
			});
		}

		void AssertAddClearedEventToLog(JobDeclaration declaration, CusEntryHeader entryHeader, string message)
		{
			CombineAssertions(message, () =>
			{
				AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared));
				AssertNull(entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
				AssertNull(entryHeader.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared));
			});
		}

		public void TestEntryHeaderStatusDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_EntryStatus = ZString.Empty;
			AssertEquals("EntryHeaderStatusDescription", ZString.Empty, entry.EntryHeaderStatusDescription);
		}

		public void TestCustomsChargesProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertType<InterfaceImplementations.CusEntryHeaderCustomsCharges>(ServiceLocator.GetService<ICustomsCharges>(entryHeader));
		}

		public void TestDefaultCreditorPK()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			testHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");

			var testAgentForFAN = Factory.NewWithValidTestData<OrgHeader>();
			testAgentForFAN.CustomsCodes.AddNew("CDP", "51051342", "ZA");
			testAgentForFAN.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
			var testCreditor1 = Factory.NewWithValidTestData<OrgHeader>();
			testCreditor1.OH_IsCreditor = true;
			var testCreditor2 = Factory.NewWithValidTestData<OrgHeader>();
			testCreditor2.OH_IsCreditor = true;
			Factory.Save();

			var agentOfficeToCreditorMappings = new FinancialAccountNumberPortMapCollection();
			CreateMapping(agentOfficeToCreditorMappings, testAgentForFAN, testCreditor1, "1234567890", "JHB");
			var mapping = CreateMapping(agentOfficeToCreditorMappings, testAgentForFAN, testCreditor2, "1234567899");
			mapping.Cash = true;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, agentOfficeToCreditorMappings);

			var buyer = OrgHeader.New(Factory);
			buyer.FillWithValidTestData();
			var collection = new CustomsDSBCreditorOverrideCollection();
			var creditor = collection.AddNew();
			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
			orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);
			var organisation = Factory.LoadTop1<OrgHeader>(orgHeaderQuery);
			creditor.DistrictOfficeCode = "BFN";
			creditor.CreditorPK = organisation.PK;
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			DataRegistry.Business.ZACustomsRegistry.Instance.DSBCreditors.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, organisation2.PK.ToGuid());
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "JHB";
			declaration.JE_OH_AgentOverride = testAgentForFAN.PK;
			declaration.ActiveEntryHeaders.AddNew();
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(testCreditor1.PK, entry.CreditorPK);
			entry.CH_PaymentMethod = PaymentMethodCodeList.Codes.VATOnly;
			foreach (EntryChargeType chargeType in entry.EntryChargeTypeList)
			{
				if (chargeType.ChargeCodeForRating != CusEntryHeader.RateTypes.VATNormal)
				{
					AssertEquals(testCreditor2.PK, entry.GetDefaultCreditorPK(chargeType));
				}
				else
				{
					AssertEquals(testCreditor1.PK, entry.GetDefaultCreditorPK(chargeType));
				}
			}
		}

		public void TestGetDefaultCreditorPKWithNullOrganization()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");

			var testAgentForFAN = Factory.NewWithValidTestData<OrgHeader>();
			testAgentForFAN.CustomsCodes.AddNew("CDP", "51051342", "ZA");
			testAgentForFAN.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
			var testCreditor = Factory.NewWithValidTestData<OrgHeader>();
			testCreditor.OH_IsCreditor = true;
			Factory.Save();

			var agentOfficeToCreditorMappings = new FinancialAccountNumberPortMapCollection();
			CreateMapping(agentOfficeToCreditorMappings, testAgentForFAN, testCreditor, "1234567890", "JHB");
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, agentOfficeToCreditorMappings);

			Factory.Load<OrgHeader>(testAgentForFAN.PK).Delete();
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "JHB";
			declaration.JE_OH_AgentOverride = testAgentForFAN.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_PaymentMethod = PaymentMethodCodeList.Codes.VATOnly;

			AssertNoExceptionThrown(() => entry.GetDefaultCreditorPK(entry.EntryChargeTypeList[0]));
		}

		public void TestCreditorPK()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("BBR");
			testHelper.CreateCustomsOfficeCusCodeEntry("BFN");
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");

			var testAgentForFAN = Factory.NewWithValidTestData<OrgHeader>();
			testAgentForFAN.CustomsCodes.AddNew("CDP", "51051342", "ZA");
			testAgentForFAN.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
			var testCreditorForFAN = Factory.NewWithValidTestData<OrgHeader>();
			testCreditorForFAN.OH_IsCreditor = true;
			Factory.Save();

			var agentOfficeToCreditorMappings = new FinancialAccountNumberPortMapCollection();
			CreateMapping(agentOfficeToCreditorMappings, testAgentForFAN, testCreditorForFAN, "1234567890", "JHB");
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, agentOfficeToCreditorMappings);

			var buyer = OrgHeader.New(Factory);
			buyer.FillWithValidTestData();
			var collection = new CustomsDSBCreditorOverrideCollection();
			var creditor = collection.AddNew();

			var orgHeaderQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			var orgCompanyDataQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			orgCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_IsCreditor, true);
			orgHeaderQuery.AddSubQuery(orgCompanyDataQuery, JoinCondition.And);
			var organisation = Factory.LoadTop1<OrgHeader>(orgHeaderQuery);
			creditor.DistrictOfficeCode = "BFN";
			creditor.CreditorPK = organisation.PK;
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			DataRegistry.Business.ZACustomsRegistry.Instance.DSBCreditors.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
			Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, organisation2.PK.ToGuid());

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = buyer.PK;
			declaration.ActiveEntryHeaders.AddNew();
			declaration.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_MergeBy = "NON";
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "BFN";
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			ICustomsChargeEntry entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(ZGuid.Empty, entry.CreditorPK);

			declaration.JE_CustomsOffice = "BBR";
			AssertEquals("fallback to default one", ZGuid.Empty, entry.CreditorPK);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_CustomsOffice = "JHB";
			declaration.JE_OH_AgentOverride = testAgentForFAN.PK;
			declaration.ActiveEntryHeaders.AddNew();
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(testCreditorForFAN.PK, entry.CreditorPK);
		}

		public void TestGetTotalChargeValueFor()
		{
			var prpRateType = testHelper.CreateNewOrGetExistingRateType("ZA", Constants.RateTypes.ProvisionalPayment);
			var penRateType = testHelper.CreateNewOrGetExistingRateType("ZA", Constants.RateTypes.Penalty);
			var dtyRateType = testHelper.CreateNewOrGetExistingRateType("ZA", Constants.RateTypes.Duty);
			var lvyRateType = testHelper.CreateNewOrGetExistingRateType("ZA", Constants.RateTypes.Levy);
			prpRateType.ZZR_IsPayable = true;
			penRateType.ZZR_IsPayable = true;
			testHelper.LoadOrCreateNewCusRateCode(Factory, "1P1", dtyRateType.PK);
			testHelper.LoadOrCreateNewCusRateCode(Factory, "13A", lvyRateType.PK);
			testHelper.LoadOrCreateNewCusRateCode(Factory, "13B", lvyRateType.PK);
			testHelper.LoadOrCreateNewCusRateCode(Factory, "13C", lvyRateType.PK);
			testHelper.LoadOrCreateNewCusRateCode(Factory, "13D", lvyRateType.PK);
			testHelper.LoadOrCreateNewCusRateCode(Factory, "15A", lvyRateType.PK);
			testHelper.LoadOrCreateNewCusRateCode(Factory, "15B", lvyRateType.PK);
			Factory.Save();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_ProvisionalPaymentType = HeaderLevelProvisionalPayments.Codes.PPE;
			instruction.CEI_ProvisionalPaymentAmount = 100m;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.Fees.AddOrUpdate("1P1", 10m);
			entryLine1.Fees.AddOrUpdate("13A", 20m);
			entryLine1.Fees.AddOrUpdate("13C", 30m);
			entryLine1.Fees.AddOrUpdate("15B", 40m);
			entryLine1.Fees.AddOrUpdate("13D", 50m);
			entryLine1.Fees.AddOrUpdate("13B", 60m);
			entryLine1.Fees.AddOrUpdate("15A", 70m);
			entryLine1.Fees.AddOrUpdate("TAX", 80m);
			entryLine1.Fees.AddOrUpdate("PPA", 90m);

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.Fees.AddOrUpdate("1P1", 11m);
			entryLine2.Fees.AddOrUpdate("13A", 21m);
			entryLine2.Fees.AddOrUpdate("13C", 31m);
			entryLine2.Fees.AddOrUpdate("15B", 41m);
			entryLine2.Fees.AddOrUpdate("13D", 51m);
			entryLine2.Fees.AddOrUpdate("13B", 61m);
			entryLine2.Fees.AddOrUpdate("15A", 81m);
			entryLine2.Fees.AddOrUpdate("TAX", 91m);
			entryLine2.ProvisionalPayments.AddNew("PPA", 13.00m);
			entryLine2.ProvisionalPayments.AddNew("PPA", 23.00m);
			entryLine2.ProvisionalPayments.AddNew("PPE", 23.00m);
			entryLine2.ProvisionalPayments.AddNew("PPC", 33.00m);
			entryLine2.ProvisionalPayments.AddNew("PEN", 43.11m);
			entryLine2.ProvisionalPayments.AddNew("FOR", 53.22m);
			entryLine2.ProvisionalPayments.AddNew("XXX", 63.00m);

			entryLine1.CL_LineNumber = 1;
			entryLine2.CL_LineNumber = 2;
			AssertGetTotalChargeValueFor(entry, 192m);

			entryLine1.CL_LineNumber = 10;
			entryLine2.CL_LineNumber = 11;
			AssertGetTotalChargeValueFor(entry, 92m);
		}

		void AssertGetTotalChargeValueFor(CusEntryHeader entry, ZDecimal provisionalPayment)
		{
			var expectedResult = new Dictionary<ZString, ZDecimal>();
			expectedResult.Add(Universal.Constants.RateTypes.Penalty, 96.33m);
			expectedResult.Add(Universal.Constants.RateTypes.ProvisionalPayment, provisionalPayment);
			expectedResult.Add(Universal.Constants.RateTypes.Duty, 21m);
			expectedResult.Add(Universal.Constants.RateTypes.Excise, 0m);
			expectedResult.Add(Universal.Constants.RateTypes.AdValoremExcise, 0m);
			expectedResult.Add(Universal.Constants.RateTypes.AntiDumping, 0m);
			expectedResult.Add(Universal.Constants.RateTypes.Rebate, 0m);
			expectedResult.Add(Universal.Constants.RateTypes.Refund, 0m);
			expectedResult.Add(Universal.Constants.RateTypes.Levy, 556m);

			var result = new Dictionary<ZString, ZDecimal>();
			var rateTypes = Factory.Load<RefCusRateType>(new ZQuery(RefCusRateTypeSchema.ZZR_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.SouthAfrica));
			foreach (var item in rateTypes.Select(x => x.ZZR_RateType))
			{
				result.Add(item, (entry as ICustomsChargeEntry).GetTotalChargeValueFor(new EntryChargeType(null, item, $"{item}desc", false, ZString.Empty), ""));
			}

			AssertContainsExactElementsInAnyOrder(expectedResult, result.ToArray());
		}

		public void TestGetTotalDutiesAndTaxes()
		{
			testHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			testHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			var tariffType1P1 = testHelper.CreateNewOrGetExistingTariffType("ZA", "1P1", Constants.RateTypes.Duty);
			testHelper.CreateNewOrGetExistingTariffType("ZA", "12B", "EX1");
			var testTariff1 = testHelper.CreateTariff("ZA", tariffType1P1.PK, "99991", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			testHelper.CreateTariffUOM(testTariff1, "CU1", "KG");
			var testTariff2 = testHelper.CreateTariff("ZA", tariffType1P1.PK, "99992", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			testHelper.CreateTariffUOM(testTariff2, "CU1", "LI");
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "YY";
			var testOrgInvHeader = declaration.Invoices.AddNew();
			var testOrgInvLine = testOrgInvHeader.InvoiceLines.AddNew();
			testOrgInvLine.JI_CustomsQuantity = 150;
			testOrgInvLine.JI_CustomsUnitQty = "KG";
			testOrgInvLine.JI_ZZF_NKTaxType = "VAT";
			var testOrgInvLine2 = testOrgInvHeader.InvoiceLines.AddNew();
			testOrgInvLine2.JI_CustomsQuantity = 50;
			testOrgInvLine2.JI_CustomsUnitQty = "KG";
			testOrgInvLine2.JI_ZZF_NKTaxType = "VAT";
			var testOrgEntry = declaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			testOrgInvLine.JI_CL = testEntryLine.PK;
			testOrgInvLine2.JI_CL = testEntryLine.PK;
			testEntryLine.CL_AdValoremTariff = "99991";
			testEntryLine.CL_CustomsValue = 2000m;
			testEntryLine.CL_LineNumber = 2;
			Factory.Save();

			AssertEquals(0m, testOrgEntry.TotalDutiesAndTaxes);
			testEntryLine.Fees.AddOrUpdate("1P1", 21);
			AssertEquals(21m, testOrgEntry.TotalDutiesAndTaxes);
			testEntryLine.Fees.AddOrUpdate("12B", 22);
			AssertEquals(43m, testOrgEntry.TotalDutiesAndTaxes);
			testEntryLine.Fees.AddOrUpdate("VAT", 23);
			AssertEquals(66m, testOrgEntry.TotalDutiesAndTaxes);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 24);
			AssertEquals(90m, testOrgEntry.TotalDutiesAndTaxes);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 25);
			AssertEquals(115m, testOrgEntry.TotalDutiesAndTaxes);

			var testEntryLine2 = testOrgEntry.MergedLines.AddNew();
			testEntryLine2.CL_AdValoremTariff = "99991";
			testEntryLine2.CL_CustomsValue = 3000m;
			testEntryLine2.CL_LineNumber = 2;
			testEntryLine2.Fees.AddOrUpdate("1P1", 21);
			AssertEquals(136m, testOrgEntry.TotalDutiesAndTaxes);
			testEntryLine2.Fees.AddOrUpdate("12B", 22);
			AssertEquals(158m, testOrgEntry.TotalDutiesAndTaxes);
			testEntryLine2.Fees.AddOrUpdate("VAT", 23);
			AssertEquals(158m, testOrgEntry.TotalDutiesAndTaxes);
			testEntryLine2.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 24);
			AssertEquals(182m, testOrgEntry.TotalDutiesAndTaxes);
			testEntryLine2.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 25);
			AssertEquals(207m, testOrgEntry.TotalDutiesAndTaxes);
		}

		public void TestGetCalcValues()
		{
			testHelper.CreateRefCusProcedure("ZA", "X", "YY", "00", "", "XXYY5", "IMP", calculateDuty: true);
			testHelper.CreateRefCusProcedure("ZA", "X", "XX", "YY", "5", "XXYY5", "IMP,EXP");
			var tariffType1P1 = testHelper.CreateNewOrGetExistingTariffType("ZA", "1P1", Constants.RateTypes.Duty);
			testHelper.CreateNewOrGetExistingTariffType("ZA", "12B", "EX1");
			var testTariff1 = testHelper.CreateTariff("ZA", tariffType1P1.PK, "99991", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			testHelper.CreateTariffUOM(testTariff1, "CU1", "KG");
			var testTariff2 = testHelper.CreateTariff("ZA", tariffType1P1.PK, "99992", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			testHelper.CreateTariffUOM(testTariff2, "CU1", "LI");
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "YY";
			var testOrgInvHeader = declaration.Invoices.AddNew();
			var testOrgInvLine = testOrgInvHeader.InvoiceLines.AddNew();
			testOrgInvLine.JI_CustomsQuantity = 150;
			testOrgInvLine.JI_CustomsUnitQty = "KG";
			testOrgInvLine.JI_ZZF_NKTaxType = "VAT";
			var testOrgInvLine2 = testOrgInvHeader.InvoiceLines.AddNew();
			testOrgInvLine2.JI_CustomsQuantity = 50;
			testOrgInvLine2.JI_CustomsUnitQty = "KG";
			testOrgInvLine2.JI_ZZF_NKTaxType = "VAT";
			var testOrgEntry = declaration.ActiveEntryHeaders.AddNew();
			testOrgEntry.MovementReferenceNumberSetter("TestMRN", ZDateTime.Today);
			var testEntryLine = testOrgEntry.MergedLines.AddNew();
			testOrgInvLine.JI_CL = testEntryLine.PK;
			testOrgInvLine2.JI_CL = testEntryLine.PK;
			testEntryLine.CL_AdValoremTariff = "99991";
			testEntryLine.CL_CustomsValue = 2000m;
			testEntryLine.CL_LineNumber = 2;
			Factory.Save();

			testEntryLine.Fees.AddOrUpdate("1P1", 21);
			testEntryLine.Fees.AddOrUpdate("2P2", 17);
			testEntryLine.Fees.AddOrUpdate("12A", 10);
			testEntryLine.Fees.AddOrUpdate("12B", 22);
			testEntryLine.Fees.AddOrUpdate("VAT", 23);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 24);
			testEntryLine.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 25);

			var testEntryLine2 = testOrgEntry.MergedLines.AddNew();
			testEntryLine2.CL_AdValoremTariff = "99991";
			testEntryLine2.CL_CustomsValue = 3000m;
			testEntryLine2.CL_LineNumber = 2;
			testEntryLine2.Fees.AddOrUpdate("1P1", 25);
			testEntryLine2.Fees.AddOrUpdate("2P2", 33);
			testEntryLine2.Fees.AddOrUpdate("12A", 10);
			testEntryLine2.Fees.AddOrUpdate("12B", 22);
			testEntryLine2.Fees.AddOrUpdate("VAT", 23);
			testEntryLine2.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PPA, 24);
			testEntryLine2.ProvisionalPayments.AddNew(LineLevelProvisionalPayments.Codes.PEN, 25);

			var calc = testOrgEntry.GetCalcFeeValues();
			CombineAssertions(() =>
			{
				AssertEquals("CustomsDutyExcluding12B", 116m, calc.CustomsDutyExcluding12B);
				AssertEquals("S1P2BDuty", 44m, calc.S1P2BDuty);
				AssertEquals("ValueAddedTax", 23m, calc.ValueAddedTax);
				AssertEquals("ProvisionalPayment", 48m, calc.ProvisionalPayment);
				AssertEquals("Penalty", 50m, calc.Penalty);
				AssertEquals("CustomsDutiesSchedule1P1andSchedule2", 96m, calc.CustomsDutiesSchedule1P1andSchedule2);
			});
		}

		public void TestRelatedARInvoices()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");

			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew("CDP", "51051342", "ZA");
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "51051342", Core.Constants.CountryCodes.SouthAfrica);
			var testCreditor = Factory.NewWithValidTestData<OrgHeader>();
			testCreditor.OH_IsCreditor = true;
			var testCreditor2 = Factory.NewWithValidTestData<OrgHeader>();
			testCreditor2.OH_IsCreditor = true;
			Factory.Save();

			var agentOfficeToCreditorMappings = new FinancialAccountNumberPortMapCollection();
			var mapping1 = CreateMapping(agentOfficeToCreditorMappings, testAgent, testCreditor, "1234567890", "JHB");
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, agentOfficeToCreditorMappings);

			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			dec.JE_OH_AgentOverride = testAgent.PK;
			dec.JE_CustomsOffice = "JHB";

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var accHeader1 = CreateARInvoice("INV0001");
			var accHeader2 = CreateARInvoice("INV0002");
			var accHeader3 = CreateARInvoice("INV0003");
			var accHeader4 = CreateARInvoice("INV0004");
			var accLines1 = CreateARInvoiceLine(accHeader1, glHeader);
			var accLines2 = CreateARInvoiceLine(accHeader2, glHeader);
			var accLines3 = CreateARInvoiceLine(accHeader3, glHeader);
			var accLines4 = CreateARInvoiceLine(accHeader4, glHeader);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = dec.PK;
			var jobCharge1 = CreateJobCharge(jobHeader);
			var jobCharge2 = CreateJobCharge(jobHeader);
			var jobCharge3 = CreateJobCharge(jobHeader);
			var jobCharge4 = CreateJobCharge(jobHeader);
			var jobCharge5 = CreateJobCharge(jobHeader);
			jobCharge2.JR_AL_ARLine = accLines1.PK;
			jobCharge3.JR_AL_ARLine = accLines2.PK;
			jobCharge4.JR_AL_ARLine = accLines3.PK;
			jobCharge5.JR_AL_ARLine = accLines4.PK;
			jobCharge1.JR_OH_CostAccount = testCreditor.PK;
			jobCharge3.JR_OH_CostAccount = testCreditor.PK;
			jobCharge4.JR_OH_CostAccount = testCreditor.PK;
			jobCharge5.JR_OH_CostAccount = testCreditor2.PK;

			var entry = dec.ActiveEntryHeaders.AddNew();
			Factory.Save();

			CombineAssertions(() =>
			{
				List<ZString> relatedInvoices = entry.RelatedARInvoices.Select(i => i.AH_TransactionNum).ToList();
				AssertEquals(2, relatedInvoices.Count);
				// note: assigned AH_TransactionNum is overridden in TransactionHeader.OnSavingCore
				AssertContainsExactElementsInAnyOrder(new string[] { accHeader2.AH_TransactionNum, accHeader3.AH_TransactionNum }, relatedInvoices);
			});
		}

		public void TestHasResponse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			Assert("HasResponse", !entryHeader.HasResponses);
			entryHeader.Messages.AddNew();
			Assert("HasResponse", !entryHeader.HasResponses);
			entryHeader.Messages.AddNew().EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Assert("HasResponse", entryHeader.HasResponses);
		}

		public void TestCustomsProcedureCode()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "10";
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "20";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals("CustomsProcedureCode", ZString.Empty, entryHeader.CustomsProcedureCode);
			entryHeader.CH_CEI_Instruction = instruction1.PK;
			AssertEquals("CustomsProcedureCode", "10", entryHeader.CustomsProcedureCode);
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction2.PK;
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("CustomsProcedureCode", "20", entryHeader.CustomsProcedureCode);
		}

		public void TestSettingReleaseDate()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testEntry = declaration.ActiveEntryHeaders.AddNew();
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			testInterchange.EI_HeaderText = "UNB+UNOB:4+SARSREQT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160620:0633+39++CONTRL+++GWWTGTEST+1'";
			var testMessage = Factory.NewWithValidTestData<EDIMessage>();
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_MessageType = "RES";
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_Status = "PRS";
			testMessage.EM_EI = testInterchange.PK;
			testEntry.Messages.Add(testMessage);
			Factory.Save();

			AssertEquals("Pre-Check", ZDateTime.Empty, testEntry.CH_EntryReleaseDate);
			testEntry.CH_EntryStatus = "1";
			Factory.Save();
			AssertEquals("Post-Check", new ZDateTime(2016, 06, 20), testEntry.CH_EntryReleaseDate);
		}

		public void TestCustomsProcedureInstructionDescription()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var inst1 = declaration.CustomsEntryInstructions.AddNew();
			var inst2 = declaration.CustomsEntryInstructions.AddNew();
			var inst3 = declaration.CustomsEntryInstructions.AddNew();
			inst1.CEI_Style = "11";
			inst2.CEI_Style = "11";
			inst3.CEI_Style = "11";
			inst1.CEI_Description = "DESC1";
			inst3.CEI_Description = "DESC3";

			var entry1 = declaration.ActiveEntryHeaders.AddNew();
			var entry2 = declaration.ActiveEntryHeaders.AddNew();
			var entry3 = declaration.ActiveEntryHeaders.AddNew();
			var entry4 = declaration.ActiveEntryHeaders.AddNew();

			entry1.CH_CEI_Instruction = inst1.PK;
			entry2.CH_CEI_Instruction = inst2.PK;
			entry3.CH_CEI_Instruction = inst3.PK;
			entry4.CH_CEI_Instruction = ZGuid.Empty;

			CombineAssertions(() =>
			{
				AssertEquals("1", "DESC1", entry1.CustomsProcedureInstructionDescription);
				AssertEquals("2", "", entry2.CustomsProcedureInstructionDescription);
				AssertEquals("3", "DESC3", entry3.CustomsProcedureInstructionDescription);
				AssertEquals("4", "", entry4.CustomsProcedureInstructionDescription);
			});
		}

		public void TestProvisionalPaymentsPayInfos()
		{
			var header = Factory.New<CusEntryHeader>();
			var pp01 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "REF1", closed: true);
			var pp02 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "REF2", closed: false);
			var pp03 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Empty, "C", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "REF", closed: true);
			var pp04 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Empty, "C", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "REF", closed: false);
			var pp05 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "REF1", closed: true);
			var pp06 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "D", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "REF1", closed: false);
			var pp07 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "VAT", 0, "1", "REF1", closed: true);
			var pp08 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "VAT", 0, "1", "REF1", closed: false);
			var pp09 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "", 0, "1", "REF1", closed: true);
			var pp10 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", "", 0, "1", "REF1", closed: false);
			var pp11 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PEN, 0, "1", "REF3", closed: true);
			var pp12 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PEN, 0, "1", "REF1", closed: false);
			var pp13 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", HeaderLevelProvisionalPayments.Codes.PPE, 0, "1", "REF", closed: true);
			var pp14 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", HeaderLevelProvisionalPayments.Codes.PPE, 0, "1", "REF", closed: false);
			var pp15 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "", closed: true);
			var pp16 = header.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "", closed: false);

			var tester = header.ProvisionalPaymentPayInfos;
			AssertContainsExactElementsInAnyOrder((new CusEntryPayInfo[] { pp01, pp02, pp03, pp04, pp11, pp12, pp13, pp14 }).Select(x => x.PK), tester.Select(x => x.PK));
		}

		[TestDate(2017, 01, 01)]
		public void TestRunAutoRateDSBOnBGMReferenceChanged()
		{
			var dut = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			testHelper.LoadOrCreateNewCusRateCode(Factory, "1P1", dut.PK);
			Factory.Save();
			var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var customsDefaultToCurrentLoginDeptRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
			customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = Factory.New<JobDeclaration>();
			var orgProxy = declaration.Branch.Company.OrgProxy;
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "00000001", Core.Constants.CountryCodes.SouthAfrica);
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "TS1";
			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate("1P1", 50m);
			Factory.Save();
			AssertEquals("CH_BGMReference", "00000001TS120170101000001", entry.CH_BGMReference);
			AssertNull(declaration.Job);

			new InvoicePostingAccountingIntegrator().IntegrateIfNecessary(new JobDeclarationIAccIntegrationDataProvider(ChargePosterBehaviours.AutoRateDSB, declaration.ActiveEntryHeaders.Select(x => x.PK), declaration.PK, true, Factory));
			AssertNotNull(declaration.Job);
			var invoiceJob = Factory.Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);

			CombineAssertions("Initial Rating", () =>
			{
				AssertEquals(1, invoiceJob.Charges.Count);
				AssertEquals("00000001TS120170101000001", invoiceJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(50m, invoiceJob.Charges[0].APLine.AL_LineAmount);
			});

			var message = Factory.New<ZAMessageForTest>();
			entry.Messages.Add(message);
			Factory.Save();
			CombineAssertions("Initial Rating", () =>
			{
				AssertEquals(1, invoiceJob.Charges.Count);
				AssertEquals("00000001TS120170101000001", invoiceJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(50m, invoiceJob.Charges[0].APLine.AL_LineAmount);
			});
			entry.CH_BGMReference = "20160101000001";
			Factory.Save();
			CombineAssertions("Change of LRN Rating, update Invoice Numbers", () =>
			{
				AssertEquals(1, invoiceJob.Charges.Count);
				AssertEquals("20160101000001/CUSDSB", invoiceJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(50m, invoiceJob.Charges[0].APLine.AL_LineAmount);
			});
		}

		public void TestAutoRatingForDoNotClaimVATRefund()
		{
			var (duty, vat) = SetupAutoRatingForDoNotClaimVATRefund();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_ZZF_NKTaxType = "VAT";
			invoiceLine2.JI_ZZF_NKTaxType = "VAT";

			var entryHeader1 = CreateCusEntryHeader(declaration, entryInstruction, "0001", 151m);
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			entryLine1.InvoiceLines.Add(invoiceLine1);
			entryLine1.Fees.AddOrUpdate("1P1", 50);
			entryLine1.Fees.AddOrUpdate("VAT", 51);
			var entryHeader2 = CreateCusEntryHeader(declaration, entryInstruction, "0002", 161m, doNotClaimVATRefund: false);
			var entryLine2 = entryHeader2.MergedLines.AddNew();
			entryLine2.InvoiceLines.Add(invoiceLine2);
			entryLine2.Fees.AddOrUpdate("1P1", 60);
			entryLine2.Fees.AddOrUpdate("VAT", 61);
			Factory.Save();

			new InvoicePostingAccountingIntegrator().IntegrateIfNecessary(new JobDeclarationIAccIntegrationDataProvider(ChargePosterBehaviours.AutoRateDSB, new ZGuid[] { entryHeader1.PK, entryHeader2.PK }, declaration.PK, true, Factory));
			AssertNotNull(declaration.Job);
			CombineAssertions(() =>
			{
				var invoiceJob = new BusinessObjectFactory().Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);
				AssertEquals(4, invoiceJob.Charges.Count);
				AssertEquals(50m, invoiceJob.Charges.OfType<JobCharge>().FirstOrDefault(x => x.JR_APInvoiceNum == "0001" && x.JR_AC == duty.PK)?.APLine?.AL_LineAmount);
				AssertEquals(51m, invoiceJob.Charges.OfType<JobCharge>().FirstOrDefault(x => x.JR_APInvoiceNum == "0001" && x.JR_AC == vat.PK)?.APLine?.AL_LineAmount);
				AssertEquals(60m, invoiceJob.Charges.OfType<JobCharge>().FirstOrDefault(x => x.JR_APInvoiceNum == "0002" && x.JR_AC == duty.PK)?.APLine?.AL_LineAmount);
				AssertEquals(61m, invoiceJob.Charges.OfType<JobCharge>().FirstOrDefault(x => x.JR_APInvoiceNum == "0002" && x.JR_AC == vat.PK)?.APLine?.AL_LineAmount);
			});
		}

		(AccChargeCode duty, AccChargeCode vat) SetupAutoRatingForDoNotClaimVATRefund()
		{
			new ZAUniversalReferenceTestDataHelper(Factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var data = (ZArchitecture.Environment.RegistryItemSet)CargoWise.Application.ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var customsDefaultToCurrentLoginDeptRegistry = (ZArchitecture.Environment.BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
			customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TT";
			staff.GS_EmailAddress = "test@edi.com.au";

			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;
			Factory.Save();

			var testHeader = new ZAUniversalReferenceTestDataHelper(Factory);
			var dut = testHeader.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "DTY", "Duty");
			testHeader.LoadOrCreateNewCusRateCode(Factory, "1P1", dut.PK);
			var vat = testHeader.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "VAT", "VAT");
			testHeader.LoadOrCreateNewCusRateCode(Factory, "VAT", vat.PK);
			Factory.Save();

			var chargeCodeDisbursementDEF = CreateChargeCode("DSB", "Customs Disbursements Default", Core.Constants.ChargeType.Disbursement);
			var chargeCodeDisbursementDTY = CreateChargeCode("DUT", "Customs Disbursements Duty", Core.Constants.ChargeType.Disbursement);
			var chargeCodeDisbursementVAT = CreateChargeCode("VTT", "Customs Disbursements VTT", Core.Constants.ChargeType.Disbursement);

			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCodeDisbursementDEF.PK.ToGuid());
			var entryChargeTypesAndCodes = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var entryChargeType1 = entryChargeTypesAndCodes.AddNew();
			entryChargeType1.ChargeType = Enterprise.Customs.Universal.Constants.RateTypes.Duty;
			entryChargeType1.AC_ChargeCode = chargeCodeDisbursementDTY.PK;
			var entryChargeType2 = entryChargeTypesAndCodes.AddNew();
			entryChargeType2.ChargeType = "VAT";
			entryChargeType2.AC_ChargeCode = chargeCodeDisbursementVAT.PK;
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryChargeTypesAndCodes);
			Factory.Save();
			return (chargeCodeDisbursementDTY, chargeCodeDisbursementVAT);
		}

		public void TestSetInventoryCustomsDeadlineOnBondValidToDateChange()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("000000000000000003");

			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "Rec1");
			receive.WD_DocketSubType = "CUS";

			var inventoryLine = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var bond = Factory.New<WhsBondedWarehouseAttribute>();
			bond.SetParent(inventoryLine.InDocketLine);

			bond.WB_CustomsDeadline = ZDate.Today;
			bond.WB_EntryKey = "000000000000000003";

			entryHeader.CH_BondValidToDate = ZDate.BrettsBirthday;
			Factory.Save();

			var reloadedBond = new BusinessObjectFactory().Load<WhsBondedWarehouseAttribute>(bond.PK);
			AssertEquals("Inventory Custom Deadline should follow CH_BondValidToDate", ZDate.BrettsBirthday, reloadedBond.WB_CustomsDeadline);
		}

		public void TestDefaultCH_BondValidToDate()
		{
			var registryValue = ZACustomsRegistry.Instance.CPCAcquitByDate.Value;
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ClusterKey = 2583639;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			registryValue.Quantity = 6;
			registryValue.Unit = "MONTH(S)";
			AssertDefaultCH_BondValidToDateMonths(entryHeader, registryValue);

			registryValue.Quantity = 1;
			registryValue.Unit = "DAY(S)";
			AssertDefaultCH_BondValidToDateDays(entryHeader, registryValue, new ZDate(2022, 9, 23), new ZDate(2022, 9, 23), new ZDate(2022, 9, 26), new ZDate(2022, 9, 26));

			registryValue.Quantity = 0;
			AssertDefaultCH_BondValidToDateDays(entryHeader, registryValue, new ZDate(2022, 9, 22), new ZDate(2022, 9, 23), new ZDate(2022, 9, 24), new ZDate(2022, 9, 24));
		}

		void AssertDefaultCH_BondValidToDateMonths(CusEntryHeader entryHeader, CPCAcquitByDate registryValue)
		{
			using (ZACustomsRegistry.Instance.CPCAcquitByDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				CombineAssertions(() =>
				{
					AssertEquals("Precondition:", ZDate.Empty, entryHeader.CH_BondValidToDate);
					entryHeader.CH_EntryReleaseDate = new ZDate(2022, 3, 23);
					AssertEquals("The CH_BondValidToDate is a working day", new ZDate(2022, 9, 23),
						entryHeader.CH_BondValidToDate);
					entryHeader.CH_BondValidToDate = ZDate.Empty;
					entryHeader.CH_EntryReleaseDate = new ZDate(2022, 3, 24);
					AssertEquals("The CH_BondValidToDate is Saturday", new ZDate(2022, 9, 23),
						entryHeader.CH_BondValidToDate);
					entryHeader.CH_BondValidToDate = ZDate.Empty;
					entryHeader.CH_EntryReleaseDate = new ZDate(2022, 3, 25);
					AssertEquals("The CH_BondValidToDate is Sunday", new ZDate(2022, 9, 26),
						entryHeader.CH_BondValidToDate);
					entryHeader.CH_EntryReleaseDate = new ZDate(2022, 3, 21);
					AssertEquals("CH_BondValidToDate NOT changed", new ZDate(2022, 9, 26),
						entryHeader.CH_BondValidToDate);
				});
			}
		}

		void AssertDefaultCH_BondValidToDateDays(CusEntryHeader entryHeader, CPCAcquitByDate registryValue, ZDate workingDay, ZDate saturday, ZDate sunday, ZDate notChanged)
		{
			using (ZACustomsRegistry.Instance.CPCAcquitByDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryValue))
			{
				CombineAssertions(() =>
				{
					entryHeader.CH_BondValidToDate = ZDate.Empty;
					entryHeader.CH_EntryReleaseDate = new ZDate(2022, 9, 22);
					AssertEquals("The CH_BondValidToDate is a working day", workingDay, entryHeader.CH_BondValidToDate);
					entryHeader.CH_BondValidToDate = ZDate.Empty;
					entryHeader.CH_EntryReleaseDate = new ZDate(2022, 9, 23);
					AssertEquals("The CH_BondValidToDate is Saturday", saturday, entryHeader.CH_BondValidToDate);
					entryHeader.CH_BondValidToDate = ZDate.Empty;
					entryHeader.CH_EntryReleaseDate = new ZDate(2022, 9, 24);
					AssertEquals("The CH_BondValidToDate is Sunday", sunday, entryHeader.CH_BondValidToDate);
					entryHeader.CH_EntryReleaseDate = new ZDate(2022, 9, 21);
					AssertEquals("CH_BondValidToDate NOT changed", notChanged, entryHeader.CH_BondValidToDate);
				});
			}
		}

		public void TestAddExtraRequiredFieldsMessageErrorForBondedWarehouse()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "BOB1";
			importer.OH_FullName = "BOB THE BUILDER";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var warehouse1 = Factory.New<OrgHeader>();
			warehouse1.OH_Code = "WAR1";
			warehouse1.OH_FullName = "WAREHOUSE 1";
			warehouse1.OH_RL_NKClosestPort = "AUSYD";
			warehouse1.MainAddress.OA_Address1 = "ADD 1";
			var procedure1 = CreateRefCusProcedure("AB", "AB DESC");
			procedure1.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			var procedure2 = CreateRefCusProcedure("CD", "CD DESC");
			procedure2.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			Factory.Save();

			var declaration = Factory.New<JobDeclarationForTestNoSupportMultipleWarehouseEntry>();
			declaration.SetSupportsBondedWarehousingForTesting(true);
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.JE_OH_Importer = importer.PK;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "CD";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "0010";
			invoiceLine.JI_LineNo = 1;
			entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			invoiceLine.SetUseBondedWarehouseAutomationForTesting(true);

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_Procedure = "0010";
			invoiceLine2.SetUseBondedWarehouseAutomationForTesting(true);

			DoMerge(declaration);
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders.First();

			AssertContains("Invoice Line marked for Bonded Warehousing must have a WHS MRN line number; not all Invoice Lines marked for Bonded Warehousing have a WHS MRN line number specified.", entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(checkProduct: false, checkQuantity: false, checkEntryDetails: false));
			invoiceLine.JI_PreviousEntryLineNumber = 5;
			invoiceLine2.JI_PreviousEntryLineNumber = 4;
			AssertNotContains("Invoice Line marked for Bonded Warehousing must have a WHS MRN line number; not all Invoice Lines marked for Bonded Warehousing have a WHS MRN line number specified.", entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(checkProduct: false, checkQuantity: false, checkEntryDetails: false));
			invoiceLine2.JI_PreviousEntryLineNumber = 5;
			AssertContains("For an Import By External Broker job WHS MRN line numbers must be unique per Entry Instruction; WHS MRN Line '5' has been entered more than once.", entry.GetMessageErrorOfRequiredFieldsForBondedWarehousing(checkProduct: false, checkQuantity: false, checkEntryDetails: false));
		}

		public void TestEntryNumberUCR()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			CreateCusEntryNumber(cusEntryHeader, CusEntryNumberTypes.Standard.UniqueConsignementReference, "test1");
			CreateCusEntryNumber(cusEntryHeader, CusEntryNumberTypes.Standard.UniqueConsignementReference, "test2");
			CreateCusEntryNumber(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "test3");
			AssertEquals(2, cusEntryHeader.UCREntryNumbers.Count());
			AssertEquals("test1, test2", cusEntryHeader.CombinedUCREntryNumbers);
		}

		public override void TestGetPermitReferenceNumberLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_EntryNumber = 1;
			AssertEquals(1, cusEntryHeader.GetPermitReferenceNumberLine());
		}

		public void TestIsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.AgentCode = "ABC";
			declaration.JE_CustomsOffice = "123";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.MessageStatus = ZAMessageStatusList.Codes.NotSent;
			AssertEquals(expected: true, cusEntryHeader.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected);

			cusEntryHeader.MovementReferenceNumberSetter("123");
			AssertEquals(expected: false, cusEntryHeader.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected);

			cusEntryHeader.MovementReferenceNumberSetter("");
			AssertEquals(expected: true, cusEntryHeader.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected);

			cusEntryHeader.MessageStatus = ZAMessageStatusList.Codes.Acknowledged;
			AssertEquals(expected: false, cusEntryHeader.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected);

			cusEntryHeader.MessageStatus = ZAMessageStatusList.Codes.NotSent;
			AssertEquals(expected: true, cusEntryHeader.IsMRNEmptyAndMessageStatusNotSentOrIsStatusRejected);
		}

		public void TestHAWBOverride()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			var entry = helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
			var entryInstruction = entry.EntryInstruction;
			entry.Declaration.JE_HouseBill = "12345";
			AssertEquals("HAWB not overridden so using JE_HouseBill", "12345", entry.HAWBOverride);

			entryInstruction.CEI_HAWBOverride = "ABCDE";
			AssertEquals("HAWB overriden so using entry instruction", "ABCDE", entry.HAWBOverride);

			entryInstruction.CEI_HAWBOverride = "";
			AssertEquals("HAWB not overriden so using JE_HouseBill", "12345", entry.HAWBOverride);
		}

		public void TestHAWBDateOverride()
		{
			var declarationHouseBillIssueDate = new ZDateTime(2021, 1, 1);
			var overrideHAWBDate = new ZDateTime(2021, 12, 31);
			var helper = new ZAWhsDataTestHelper(Factory);
			var entry = helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
			var entryInstruction = entry.EntryInstruction;
			entry.Declaration.HouseBillIssuedDate = declarationHouseBillIssueDate;
			AssertEquals("HAWB date not overridden so using HouseBillIssueDate", declarationHouseBillIssueDate, entry.HawbDateOverride);

			entryInstruction.CEI_HAWBDateOverride = overrideHAWBDate;
			AssertEquals("HAWB date overridden so using entry instruction", overrideHAWBDate, entry.HawbDateOverride);

			entryInstruction.CEI_HAWBDateOverride = ZDateTime.Empty;
			AssertEquals("HAWB date not overridden so using JE_HouseBill", declarationHouseBillIssueDate, entry.HawbDateOverride);
		}

		public void TestCarrierCodeOverride()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			var entry = helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
			var entryInstruction = entry.EntryInstruction;
			entry.Declaration.JE_CargoCarrier = "12345";
			AssertEquals("Cargo Carrier not overridden so using JE_CargoCarrier", "12345", entry.CargoCarrierOverride);

			entryInstruction.CEI_CargoCarrierOverride = "ABCDE";
			AssertEquals("Cargo Carrier overridden so using entry instruction", "ABCDE", entry.CargoCarrierOverride);

			entryInstruction.CEI_CargoCarrierOverride = "";
			AssertEquals("Cargo Carrier not overridden so using JE_CargoCarrier", "12345", entry.CargoCarrierOverride);
		}

		public void TestRemoverLocalCustomsCarrierCodeWhenRemoverIsForeignHaulier()
		{
			var foreignHaulier = OrgHeader.New(Factory);
			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "USXX1"; //Outside RSA (ZA)
			foreignHaulier.OH_RL_NKClosestPort = uNLOCO.Code;
			var entryHeader = SetupRemoverLocalCustomsCarrierCode(foreignHaulier);
			AssertEquals(ZString.Empty, entryHeader.RemoverLocalCustomsCarrierCode);
		}

		public void TestRemoverLocalCustomsCarrierCodeReturnsCarrierCodeOfTheRemoverWhenUNLOCOIsInZA()
		{
			var remover = OrgHeader.New(Factory);
			var cusCode = remover.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "555";
			AssertEquals("555", remover.LocalCustomsCarrierCode);
			cusCode = remover.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode;
			cusCode.OK_CustomsRegNo = "444";

			var uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = "ZAXXX"; //Outside RSA (ZA)
			remover.OH_RL_NKClosestPort = uNLOCO.Code;

			var entryHeader = SetupRemoverLocalCustomsCarrierCode(remover);
			AssertEquals("444", entryHeader.RemoverLocalCustomsCarrierCode);
			remover.OH_RL_NKClosestPort = "AUSYD";
			AssertEquals("444", entryHeader.RemoverLocalCustomsCarrierCode);
			cusCode.Delete();
			AssertEquals(ZString.Empty, entryHeader.RemoverLocalCustomsCarrierCode);
		}

		public void TestRemoverOrSubContractorCarrierCode()
		{
			var remover = OrgHeader.New(Factory);
			var removerCusCode = remover.CustomsCodes.AddNew();
			removerCusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode;
			removerCusCode.OK_CustomsRegNo = "MAIN";

			var subContractor = OrgHeader.New(Factory);
			var subContractorCusCode = subContractor.CustomsCodes.AddNew();
			subContractorCusCode.OK_CodeType = OrgCusCode.SouthAfricaCodeTypes.RemoverUserCode;
			subContractorCusCode.OK_CustomsRegNo = "SUB";

			var entryHeader = SetupRemoverLocalCustomsCarrierCode(remover);
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_RemoverEDI = true;
			AssertEquals("MAIN", entryHeader.RemoverLocalCustomsCarrierCode);
			AssertEquals(string.Empty, entryHeader.SubContractorRemoverCarrierCode);
			AssertEquals("MAIN", entryHeader.RemoverCarrierCodeForEDI);

			entryInstruction.OH_SubContractor = subContractor.PK;
			entryInstruction.CEI_RemoverEDI = true;
			AssertEquals("MAIN", entryHeader.RemoverLocalCustomsCarrierCode);
			AssertEquals("SUB", entryHeader.SubContractorRemoverCarrierCode);
			AssertEquals("MAIN", entryHeader.RemoverCarrierCodeForEDI);

			entryInstruction.CEI_SubContractorEDI = true;
			AssertEquals("MAIN", entryHeader.RemoverLocalCustomsCarrierCode);
			AssertEquals("SUB", entryHeader.SubContractorRemoverCarrierCode);
			AssertEquals("SUB", entryHeader.RemoverCarrierCodeForEDI);
		}

		CusEntryHeader SetupRemoverLocalCustomsCarrierCode(OrgHeader carrier)
		{
			var declaration = Factory.New<JobDeclaration>();
			var inst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			inst.CEI_Style = "11";
			inst.CEI_OH_Carrier = carrier.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = inst.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = inst.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "00";
			invLine.JI_CL = entryLine.PK;
			declaration.Branch.Company.OrgProxy.SetAgentCode(declaration.Branch.Country, "123");
			return entryHeader;
		}

		public void TestIsAgentOrgReadOnly()
		{
			var header = Factory.New<CusEntryHeader>();
			var entryNumber1 = CusEntryNumber.New(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.CountryCode);
			var mRNEntryNumber = header.CusEntryNumber;
			AssertEquals("Should Get Right Entry Number when there are Multiple Entry Numbers", entryNumber1, mRNEntryNumber);
			Assert(mRNEntryNumber.ReadOnly);
		}

		public void TestLoadCusEntryNumber()
		{
			var header = Factory.New<CusEntryHeader>();
			CusEntryNumber.New(header, "ZAZ", header.CountryCode);
			var entryNumber2 = CusEntryNumber.New(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, header.CountryCode);
			CusEntryNumber.New(header, "ZXX", header.CountryCode);
			AssertEquals("Should Get Right Entry Number when there are Multiple Entry Numbers", entryNumber2, header.CusEntryNumber);
		}

		public void TestPackagesCount()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			entryHeader.CH_Packages = 123;
			AssertEquals(123, entryHeader.PackagesCount);
		}

		public void TestInvoiceDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var provider = (IAccInvoiceDataProvider)entryHeader;
			AssertEquals(ZDateTime.Today, provider.InvoiceDate);

			var testMessage = CreateZAMessage(EDIMessage.Direction.Receive, SARSEDIMessage.MessageTypes.CUSRES, CUSRESMessageProcessorTest.TestMessage_NoPostingDate, "202");
			entryHeader.Messages.Add(testMessage);
			AssertEquals(ZDateTime.Empty, provider.InvoiceDate);
			testMessage.EM_MessageText = CUSRESMessageProcessorTest.TestMessage.Replace("\r\n", "");
			AssertEquals(new ZDateTime(2016, 3, 31), provider.InvoiceDate);

			testMessage.EM_MessageText = CUSRESMessageProcessorTest.AmendmentGrantedNoPostingDateMessageBody("UnimportantReferenceNumber");
			AssertEquals("InvoiceDate should fall back to Today for response status 27 without posting date",
				ZDateTime.Today, provider.InvoiceDate);
		}

		public void TestLastSendCUSDECMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var testMessage = CreateZAMessage(EDIMessage.Direction.Transmit, SARSEDIMessage.MessageTypes.CUSDEC, CUSRESMessageProcessorTest.TestOutGoingMessage, "202");
			entryHeader.Messages.Add(testMessage);
			AssertEquals("202", entryHeader.LastSentCUSDECMessage.EM_MessageNum);

			var testMessage2 = CreateZAMessage(EDIMessage.Direction.Transmit, SARSEDIMessage.MessageTypes.CUSDEC, CUSRESMessageProcessorTest.TestOutGoingMessage, "203");
			entryHeader.Messages.Add(testMessage2);
			AssertEquals("203", entryHeader.LastSentCUSDECMessage.EM_MessageNum);

			testMessage2.EM_Status = "DCD";
			AssertEquals("202", entryHeader.LastSentCUSDECMessage.EM_MessageNum);
		}

		public void TestEffectiveAgent()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			var agent = CreateOrgHeader("buyer", "IMP#@$43", "ASBSD", isConsignee: true);
			var agent2 = CreateOrgHeader("buyer", "IMP#@$55", "ASBSE", isConsignee: true);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "JHB";
			declaration.JE_AGTCode = "ASBSD";

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter("DBN201609201234567", ZDateTime.Now);
			declaration.JE_OH_AgentOverride = agent2.PK;
			AssertEquals(agent.PK, entryHeader.EffectiveAgent.PK);
		}

		public void TestGetImporterPays()
		{
			var declaration = SetupZAUniversalReferenceTestDataWithNoBasicTariffData();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals(ZBool.True, entryHeader.GetImporterPays());
			declaration.JE_CustomsOffice = "JHF";
			AssertEquals(ZBool.False, entryHeader.GetImporterPays());
		}

		public void TestGetFinancialAccountMapping()
		{
			var declaration = SetupZAUniversalReferenceTestDataWithNoBasicTariffData();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals("3234002346", entryHeader.GetFinancialAccountMapping().FinancialAccountNumber);
			declaration.JE_CustomsOffice = "JHF";
			AssertEquals(null, entryHeader.GetFinancialAccountMapping());
		}

		public void TestGetFinancialAccountNumber()
		{
			var declaration = SetupZAUniversalReferenceTestDataWithNoBasicTariffData();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			AssertEquals("3234002346", entryHeader.GetFinancialAccountNumber());
			declaration.JE_CustomsOffice = "JHF";
			AssertEquals(ZString.Empty, entryHeader.GetFinancialAccountNumber());
		}

		public void TestIsFeePaidByBroker()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			var cusHeader = declaration.ActiveEntryHeaders.AddNew();

			declaration.JE_PaymentMethod = PaidByCodeList.Codes.BRK;
			AssertEquals("IsFeePaidByBroker", expected: true, cusHeader.IsFeePaidByBroker(Enterprise.Customs.Universal.Constants.RateTypes.Duty, "", null));
			AssertEquals("IsFeePaidByBroker", expected: true, cusHeader.IsFeePaidByBroker(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, "", null));
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals("IsFeePaidByBroker", expected: true, cusHeader.IsFeePaidByBroker(Enterprise.Customs.Universal.Constants.RateTypes.Duty, "", null));
			AssertEquals("IsFeePaidByBroker", expected: true, cusHeader.IsFeePaidByBroker(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, "", null));

			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.JE_PaymentMethod = PaidByCodeList.Codes.CLI;
			AssertEquals("IsFeePaidByBroker", expected: false, cusHeader.IsFeePaidByBroker(Enterprise.Customs.Universal.Constants.RateTypes.Duty, "", null));
			AssertEquals("IsFeePaidByBroker", expected: false, cusHeader.IsFeePaidByBroker(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, "", null));
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			AssertEquals("IsFeePaidByBroker", expected: true, cusHeader.IsFeePaidByBroker(Enterprise.Customs.Universal.Constants.RateTypes.Duty, "", null));
			AssertEquals("IsFeePaidByBroker", expected: true, cusHeader.IsFeePaidByBroker(UniversalReferenceConstants.TaxOrFeeTypeCode.VAT, "", null));
		}

		JobDeclaration SetupZAUniversalReferenceTestDataWithNoBasicTariffData()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JHB");
			var buyer = CreateOrgHeader("buyer", "IMP#@$43", "ASBSD", isConsignee: true);
			Factory.Save();

			var collection = new FinancialAccountNumberPortMapCollection();
			var creditor = collection.AddNew();
			creditor.OrganizationPK = buyer.PK;
			creditor.CustomsOfficeCode = "JHB";
			creditor.FinancialAccountNumber = "3234002346";
			creditor.ImporterPays = ZBool.True;
			creditor.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "JHB";
			declaration.AgentCode = "ASBSD";

			return declaration;
		}

		public void TestIsStatusChangingToClearedInternal()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHelper.CreateCustomsStatusCusCodeEntry("2");
			testHelper.CreateCustomsStatusCusCodeEntry("7");
			testHelper.CreateCustomsStatusCusCodeEntry("14");
			Factory.Save();

			var header = Factory.New<CusEntryHeaderForTest>();

			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, value: false);
			AssertEquals("IsStatusChangingToCleared", expected: false, header.IsStatusChangingToCleared("blah", "noodle"));
			AssertEquals("IsStatusChangingToCleared", expected: false, header.IsStatusChangingToCleared("1", "2"));
			AssertEquals("IsStatusChangingToCleared", expected: true, header.IsStatusChangingToCleared("", "1"));
			AssertEquals("IsStatusChangingToCleared", expected: true, header.IsStatusChangingToCleared("7", "1"));

			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, value: true);
			AssertEquals("IsStatusChangingToCleared", expected: false, header.IsStatusChangingToCleared("14", "7"));
			AssertEquals("IsStatusChangingToCleared", expected: false, header.IsStatusChangingToCleared("", "7"));

			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, value: false);
			AssertEquals("IsStatusChangingToCleared", expected: false, header.IsStatusChangingToCleared("", "7"));
		}

		public void TestIsChangingToClearStatusForAccIntegration()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHelper.CreateCustomsStatusCusCodeEntry("2");
			testHelper.CreateCustomsStatusCusCodeEntry("7");
			testHelper.CreateCustomsStatusCusCodeEntry("14");
			Factory.Save();

			var header = Factory.New<CusEntryHeader>();

			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, value: false);
			AssertEquals("IsChangingToClearStatusForAccIntegration", expected: false, header.IsChangingToClearStatusForAccIntegrationCore("blah", "noodle"));
			AssertEquals("IsChangingToClearStatusForAccIntegration", expected: false, header.IsChangingToClearStatusForAccIntegrationCore("1", "2"));
			AssertEquals("IsChangingToClearStatusForAccIntegration", expected: true, header.IsChangingToClearStatusForAccIntegrationCore("", "1"));
			AssertEquals("IsChangingToClearStatusForAccIntegration", expected: true, header.IsChangingToClearStatusForAccIntegrationCore("7", "1"));

			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, value: true);
			AssertEquals("IsChangingToClearStatusForAccIntegration", expected: false, header.IsChangingToClearStatusForAccIntegrationCore("14", "7"));
			AssertEquals("IsChangingToClearStatusForAccIntegration", expected: false, header.IsChangingToClearStatusForAccIntegrationCore("", "7"));

			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, value: false);
			AssertEquals("IsChangingToClearStatusForAccIntegration", expected: false, header.IsChangingToClearStatusForAccIntegrationCore("", "7"));
		}

		public void TestUniqueNumbersForAccountingIntegration()
		{
			var entry = Factory.New<CusEntryHeader>();
			CombineAssertions("nothing is set", () =>
			{
				var tester = entry as IAccInvoiceDataProvider;
				AssertEquals("UniqueNumber", "", tester.UniqueNumber);
				AssertEquals("AdditionalUniqueNumber", "", tester.PreviousUniqueNumber);
			});
			CombineAssertions("When MRN is not Set", () =>
			{
				entry.CH_BGMReference = "00505655XXX20170101000001";
				var tester = entry as IAccInvoiceDataProvider;
				AssertEquals("UniqueNumber", "00505655XXX20170101000001", tester.UniqueNumber);
				AssertEquals("AdditionalUniqueNumber", "", tester.PreviousUniqueNumber);
			});
			CombineAssertions("When MRN is not Set, and LRN is short enough", () =>
			{
				entry.CH_BGMReference = "XXX20170101000001";
				var tester = entry as IAccInvoiceDataProvider;
				AssertEquals("UniqueNumber", "XXX20170101000001", tester.UniqueNumber);
				AssertEquals("AdditionalUniqueNumber", "", tester.PreviousUniqueNumber);
			});
			CombineAssertions("When MRN is not Set - oversize and invalid LRN", () =>
			{
				entry.Messages.AddNew();
				entry.CH_BGMReference = "00505655XXX2017010100X001";
				var tester = entry as IAccInvoiceDataProvider;
				AssertEquals("UniqueNumber", "00505655XXX2017010100X001", tester.UniqueNumber);
				AssertEquals("AdditionalUniqueNumber", "XXX20170101000001", tester.PreviousUniqueNumber);
			});
			CombineAssertions("When MRN is set", () =>
			{
				entry.EntryNumber = "0000014";
				var tester = entry as IAccInvoiceDataProvider;
				AssertEquals("UniqueNumber", "0000014", tester.UniqueNumber);
				AssertEquals("AdditionalUniqueNumber", "00505655XXX2017010100X001", tester.PreviousUniqueNumber);
				entry.CH_BGMReference = "00505655XXX20170101000001";
				AssertEquals("UniqueNumber", "0000014", tester.UniqueNumber);
				AssertEquals("AdditionalUniqueNumber", "00505655XXX20170101000001", tester.PreviousUniqueNumber);
			});
		}

		public void TestCustomsCharges()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			var dut = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			testHelper.LoadOrCreateNewCusRateCode(Factory, Constants.RateTypes.Duty, dut.PK);
			var vat = testHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "VAT", "Duty");
			testHelper.LoadOrCreateNewCusRateCode(Factory, "VAT", vat.PK);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_ZZF_NKTaxType = "VAT";

			var entryHeader0 = declaration.ActiveEntryHeaders.AddNew();
			var entryLine0 = entryHeader0.AllEntryLines.AddNew();
			entryLine0.InvoiceLines.Add(invoiceLine);

			entryLine0.Fees.AddOrUpdate("VAT", 10m);
			entryLine0.Fees.AddOrUpdate(Constants.RateTypes.Duty, 20m);

			AssertEquals(30m, GetCustomsCharges(entryHeader0));
		}

		public void TestSumBondSuretyAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = header.MergedLines.AddNew();

			var line1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			AddEntryLineAditionalInformation(entryLine1);
			line1.JI_CL = entryLine1.PK;
			AssertEquals(1000M, header.SumBondSuretyAmount);

			AddEntryLineAditionalInformation(entryLine1);
			AssertEquals(2000M, header.SumBondSuretyAmount);

			var entryLine2 = header.MergedLines.AddNew();
			var line2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			AddEntryLineAditionalInformation(entryLine2);
			line2.JI_CL = entryLine2.PK;
			AssertEquals(3000M, header.SumBondSuretyAmount);
		}

		public void TestBackPopulateInvoiceLineTargetEntryLineNumberIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			var line1 = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			DoMerge(declaration);

			var header = declaration.ActiveEntryHeaders[0];
			var entryLine = header.AllEntryLines[0];

			CombineAssertions(() =>
			{
				AssertEquals("Initial value", (short)0, line1.JI_TargetEntryLineNumber);
				header.BackPopulateInvoiceLineTargetEntryLineNumberIfNeeded();

				AssertEquals("Set from back populate", (short)1, line1.JI_TargetEntryLineNumber);
				line1.JI_TargetEntryLineNumber = 27;
				header.BackPopulateInvoiceLineTargetEntryLineNumberIfNeeded();

				AssertEquals("Not reset from back populate", (short)27, line1.JI_TargetEntryLineNumber);
			});
		}

		public void TestUpdateLRNIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();

			header.CH_BGMReference = "ORIG111";

			CombineAssertions(() =>
			{
				header.UpdateLRNIfNeeded("NEW222", false);
				AssertEquals("No change, LRN not editable", "ORIG111", header.CH_BGMReference);

				header.UpdateLRNIfNeeded(ZString.Empty, true);
				AssertEquals("No change, new LRN empty", "ORIG111", header.CH_BGMReference);

				header.UpdateLRNIfNeeded("NEW222", true);
				AssertEquals("LRN Changed", "NEW222", header.CH_BGMReference);
				AssertEquals("Log Event created", true, header.Logs.Find(x => x.SL_Reference.Contains("Local Reference Number updated:ORIG111 => NEW222")).Any());
			});
		}

		void AddEntryLineAditionalInformation(CusEntryLine entryLine)
		{
			var addInfo1ForLine = entryLine.AdditionalInformationCodes.AddNew();
			addInfo1ForLine.CY_Code = UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount;
			addInfo1ForLine.CY_Data = "1000";
			entryLine.AdditionalInformationCodes.Add(addInfo1ForLine);
		}

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			SetupDataEligibleForMerging(declaration);
			new LineMerger((JobDeclaration)declaration).DoMerge();
		}

		protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new ChargesCurrencyTestSetup();

		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew().JI_CL = jobDeclaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew().PK;
			jobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			return jobDeclaration.CustomsEntryHeaders[0];
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var jobDeclaration = factory.New<JobDeclaration>();
			jobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew().JI_CL = jobDeclaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew().PK;
			return jobDeclaration.CustomsEntryHeaders[0];
		}

		protected override Type ExpectedMetadataType => typeof(Metadata.Business.ZACusEntryHeader);

		protected override void SetInvoicesToResultInTwoEntries(BaseJobComInvoiceLine line1, BaseJobComInvoiceLine line2)
		{
			((JobComInvoiceLine)line1).InvoiceHeader.JZ_ValuationCode = "1";
			((JobComInvoiceLine)line1).InvoiceHeader.JZ_RelatedIndicator = "Y";
			((JobComInvoiceLine)line2).InvoiceHeader.JZ_ValuationCode = "2";
			((JobComInvoiceLine)line2).InvoiceHeader.JZ_RelatedIndicator = "Y";
		}

		protected override void SetUp()
		{
			base.SetUp();
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.RecipientID = "RecipientID";
			customsInterface.SubmissionType = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface);
			testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
		}

		ZAUniversalReferenceTestDataHelper testHelper;

		ZDecimal GetCustomsCharges(Customs.Business.CusEntryHeader entryHeader)
		{
			ZDecimal result = 0m;
			foreach (CustomsCharge customsCharge in ServiceLocator.GetService<ICustomsCharges>(entryHeader).GetCustomsCharges(null))
			{
				result += customsCharge.Amount;
			}
			return result;
		}

		CustomsCharge FindCustomsCharge(CusEntryHeader chargeProvider, string description)
		{
			ICustomsCharges customsCharges = ServiceLocator.GetService<ICustomsCharges>(chargeProvider);
			foreach (CustomsCharge charge in customsCharges.GetCustomsCharges(null))
			{
				if (charge.Description == description)
				{
					return charge;
				}
			}
			return null;
		}

		void SetupDataEligibleForMerging(BaseJobDeclaration declaration)
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var testInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInstruction.CEI_Style = "11";
			foreach (JobComInvoiceLine line in declaration.InvoiceLines.Cast<JobComInvoiceLine>())
			{
				line.JI_CEI = testInstruction.PK;
			}
		}

		CusEntryNumber CreateCusEntryNumber(CusEntryHeader entryHeader, string entryType, string entryNum)
		{
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_EntryType = entryType;
			entryNumber.CE_EntryNum = entryNum;
			return entryNumber;
		}

		CusEntryNumber CreateCusEntryNumber(CusEntryHeader entryHeader, string entryType, string entryNum, int days)
		{
			var entryNumber = CreateCusEntryNumber(entryHeader, entryType, entryNum);
			entryNumber.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(days);
			entryNumber.CE_ParentTable = entryHeader.TableName;
			return entryNumber;
		}

		ZAMessage CreateZAMessage(string receiveTransmit, string messageType, string messageText, string messageNum)
		{
			var message = Factory.New<ZAMessage>();
			message.EM_ReceiveTransmit = receiveTransmit;
			message.EM_ApplicationCode = "ZAC";
			message.EM_MessageType = messageType;
			message.EM_Status = "QUE";
			message.EM_MessageText = messageText.Replace("\r\n", "");
			message.EM_MessageNum = messageNum;
			return message;
		}

		OrgHeader CreateOrgHeader(string fullName, string code, string registrationNum, bool isConsignee)
		{
			var agent = Factory.New<OrgHeader>();
			agent.OH_FullName = fullName;
			agent.OH_IsConsignee = isConsignee;
			agent.OH_Code = code;
			agent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, registrationNum, Core.Constants.CountryCodes.SouthAfrica);
			return agent;
		}

		FinancialAccountNumberPortMap CreateMapping(FinancialAccountNumberPortMapCollection agentOfficeToCreditorMappings, OrgHeader testAgentForFAN, OrgHeader testCreditor, string financialAccountNumber, string customsOfficeCode = "")
		{
			var mapping = agentOfficeToCreditorMappings.AddNew();
			mapping.OrganizationPK = testAgentForFAN.PK;
			mapping.CustomsOfficeCode = customsOfficeCode;
			mapping.FinancialAccountNumber = financialAccountNumber;
			mapping.CreditorPK = testCreditor.PK;
			mapping.ImporterPays = false;
			mapping.AccountStartDay = 1;
			return mapping;
		}

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

		RefCusProcedure CreateRefCusProcedure(string code, string description)
		{
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = code;
			procedure.ZZ6_Description = description;
			procedure.ZZ6_PreviousProcedureCode = "10";
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			return procedure;
		}

		ARInvoice CreateARInvoice(string transactionNum)
		{
			var accHeader = Factory.NewWithValidTestData<ARInvoice>();
			accHeader.AH_TransactionNum = transactionNum;
			return accHeader;
		}

		ARInvoiceLine CreateARInvoiceLine(ARInvoice invoice, AccGLHeader header)
		{
			var accLine = Factory.NewWithValidTestData<ARInvoiceLine>();
			accLine.AL_AH = invoice.PK;
			accLine.AL_AG = header.PK;
			return accLine;
		}

		JobCharge CreateJobCharge(JobHeader jobHeader)
		{
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = jobHeader.PK;
			return jobCharge;
		}

		CusEntryHeader CreateCusEntryHeader(JobDeclaration declaration, CusEntryInstruction entryInstruction, string bgmReference, decimal valueAddedTaxBefore, bool doNotClaimVATRefund = true)
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_BGMReference = bgmReference;
			entryHeader.ValueAddedTaxBefore = valueAddedTaxBefore;
			entryHeader.DoNotClaimVATRefund = doNotClaimVATRefund;
			return entryHeader;
		}

		sealed class ChargesCurrencyTestSetup : IChargesCurrencyTestSetup
		{
			void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
			{
				declaration.AutoCreateChargesBasedOnIncoTerm = false;
				((JobDeclaration)declaration).JE_MasterBillIssuedDate = ZDateTime.Today;

				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceAmount = 10000m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 10000m;

				var nonDutiableCharge = invoiceHeader.Charges.AddNew();
				nonDutiableCharge.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight;
				nonDutiableCharge.J7_Amount = 200m;
				nonDutiableCharge.J7_IsDutiable = false;
				nonDutiableCharge.J7_IsIncludedInITOT = true;

				var oft = invoiceHeader.Charges.AddNew();
				oft.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
				oft.J7_Amount = 500m;
				oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
			}

			ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 9800m;
			ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 10300m;
		}

		sealed class CusEntryHeaderForTest : CusEntryHeader
		{
			public CusEntryHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public new bool IsStatusChangingToCleared(ZString originalStatus, ZString newStatus) => base.IsStatusChangingToCleared(originalStatus, newStatus);
		}

		sealed class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override bool SupportMultipleWarehouseEntryCore => true;
		}

		sealed class JobDeclarationForTestNoSupportMultipleWarehouseEntry : JobDeclaration
		{
			public JobDeclarationForTestNoSupportMultipleWarehouseEntry(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override bool SupportMultipleWarehouseEntryCore => false;
		}
	}
}
