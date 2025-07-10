using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class EntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestLineIsValidForMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.ZA.ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var strategy = new EntryCreationStrategy(declaration);
			Assert("The line should be valid for merge.", strategy.LineIsValidForMerge(invoiceLine));
			instruction.Delete();
			Assert("The line should not be valid for merge as the instruction is deleted.", !strategy.LineIsValidForMerge(invoiceLine));
		}

		public void TestCH_CEI_InstructionIsSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "11";
			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "12";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction1.PK;
			invoiceLine1.JI_Tariff = "1.1.1";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction1.PK;
			invoiceLine2.JI_Tariff = "1.1.2";
			var merger = new LineMerger(declaration);
			merger.DoMerge();
			AssertEquals("declaration.ActiveEntryHeaders.Count", 1, declaration.ActiveEntryHeaders.Count);
			var entry = declaration.ActiveEntryHeaders[0];
			AssertEquals("entry.CH_CEI_Instruction", instruction1.PK, entry.CH_CEI_Instruction);
			invoiceLine1.JI_CEI = instruction2.PK;
			invoiceLine2.JI_CEI = instruction2.PK;
			AssertEquals("entry.CH_CEI_Instruction", instruction1.PK, entry.CH_CEI_Instruction);
			AssertEquals("entry.EntryInstruction", instruction1, entry.EntryInstruction);
			merger.DoMerge();
			AssertEquals("entry.CH_CEI_Instruction", instruction2.PK, entry.CH_CEI_Instruction);
			AssertEquals("entry.EntryInstruction", instruction2, entry.EntryInstruction);
		}

		public void TestCustomsProcedureCode_PreviousProcedureCode_AffectsMerge_Exports()
		{
			#region Preperation
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = ProcedureCodes._11;
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = ProcedureCodes._20;
			var invHeader1 = declaration.Invoices.AddNew();
			var invLine11 = invHeader1.JobComInvoiceLines.AddNew();
			invLine11.JI_CEI = entryInstruction1.PK;
			invLine11.JI_Procedure = invLine11.EntryInstruction.CEI_Style + ProcedureCodes._00;
			invLine11.JI_NewUsed = GoodsTypeList.Codes.S;
			var invLine12 = invHeader1.JobComInvoiceLines.AddNew();
			invLine12.JI_CEI = entryInstruction2.PK;
			invLine12.JI_Procedure = invLine12.EntryInstruction.CEI_Style + ProcedureCodes._00;
			var invHeader2 = declaration.Invoices.AddNew();
			var invLine21 = invHeader2.JobComInvoiceLines.AddNew();
			invLine21.JI_CEI = entryInstruction1.PK;
			invLine21.JI_Procedure = invLine21.EntryInstruction.CEI_Style + ProcedureCodes._21;
			var invLine22 = invHeader2.JobComInvoiceLines.AddNew();
			invLine22.JI_CEI = entryInstruction1.PK;
			invLine22.JI_Procedure = invLine22.EntryInstruction.CEI_Style + ProcedureCodes._00;
			invLine22.JI_NewUsed = GoodsTypeList.Codes.S;
			Factory.Save();
			#endregion
			var testStrategy = new EntryCreationStrategy(declaration);
			CombineAssertions("Strategy Test for Exports", () =>
			{
				AssertEquals("same instruction", testStrategy.GetKeyForHeader(invLine11), testStrategy.GetKeyForHeader(invLine21));
				AssertEquals("same instruction", testStrategy.GetKeyForHeader(invLine11), testStrategy.GetKeyForHeader(invLine22));
				AssertNotEquals("different instruction, different header key", testStrategy.GetKeyForHeader(invLine11), testStrategy.GetKeyForHeader(invLine12));
				AssertNotEquals("same instruction, different line Key_1", testStrategy.GetKeyForLine(invLine11), testStrategy.GetKeyForLine(invLine21));
				AssertEquals("same instruction, same Line Key_2", testStrategy.GetKeyForLine(invLine11), testStrategy.GetKeyForLine(invLine22));
				AssertNotEquals("different instruction, different Line key", testStrategy.GetKeyForLine(invLine11), testStrategy.GetKeyForLine(invLine12));
			});
			LineMerger lineMerger = new LineMerger(declaration);
			lineMerger.DoMerge();
			CombineAssertions("LineMerger Test for Exports", () =>
			{
				AssertEquals("line11 and line 22 merge into the same lines", invLine11.JI_CL, invLine22.JI_CL);
				var headers = declaration.CustomsEntryHeaders.ToList();
				AssertEquals("Entry Header Count", 2, headers.Count);
				var entryHeader_1 = headers.OfType<CusEntryHeader>().FirstOrDefault(a => a.CustomsProcedureCode == ProcedureCodes._11 && a.AllEntryLines.Count == 2);
				var entryHeader_2 = headers.OfType<CusEntryHeader>().FirstOrDefault(a => a.CustomsProcedureCode == ProcedureCodes._20);
				AssertEquals("Header 1 line Count, line12 and line22 are merged into one entry line.", 2, entryHeader_1.AllEntryLines.Count);
				Assert("line11 should be with header 1", entryHeader_1.AllEntryLines.Any<CusEntryLine>(a => a.PK == invLine11.JI_CL));
				Assert("line11 and line12 should be with different headers", !entryHeader_1.AllEntryLines.Any<CusEntryLine>(a => a.PK == invLine12.JI_CL));
				Assert("line11 and line22 should be with different headers", entryHeader_1.AllEntryLines.Any<CusEntryLine>(a => a.PK == invLine21.JI_CL));
				Assert("line11 and line22 should be with different headers", entryHeader_1.AllEntryLines.Any<CusEntryLine>(a => a.PK == invLine22.JI_CL));
				AssertEquals("Header 2 line Count", 1, entryHeader_2.AllEntryLines.Count);
				Assert("line12 should be with header 2", entryHeader_2.AllEntryLines.Any<CusEntryLine>(a => a.PK == invLine12.JI_CL));
			});
		}

		public void TestCustomsProcedureCode_PreviousProcedureCode_AffectsMerge()
		{
			#region Preperation
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			// todo: if JE_MergeBy is disregarded in ZA, it should be removed from UI
			declaration.JE_MergeBy = Enterprise.MasterFiles.Business.OrgConstants.MergeInvoiceLines.TariffAndDescription;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = ProcedureCodes._11;
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = ProcedureCodes._20;
			var invHeader1 = declaration.Invoices.AddNew();
			var invLine11 = invHeader1.JobComInvoiceLines.AddNew();
			invLine11.JI_CEI = entryInstruction1.PK;
			invLine11.JI_Procedure = invLine11.EntryInstruction.CEI_Style + ProcedureCodes._00;
			invLine11.JI_NewUsed = GoodsTypeList.Codes.S;
			var invLine12 = invHeader1.JobComInvoiceLines.AddNew();
			invLine12.JI_CEI = entryInstruction2.PK;
			invLine12.JI_Procedure = invLine12.EntryInstruction.CEI_Style + ProcedureCodes._00;
			var invHeader2 = declaration.Invoices.AddNew();
			var invLine21 = invHeader2.JobComInvoiceLines.AddNew();
			invLine21.JI_CEI = entryInstruction1.PK;
			invLine21.JI_Procedure = invLine21.EntryInstruction.CEI_Style + ProcedureCodes._21;
			var invLine22 = invHeader2.JobComInvoiceLines.AddNew();
			invLine22.JI_CEI = entryInstruction1.PK;
			invLine22.JI_Procedure = invLine22.EntryInstruction.CEI_Style + ProcedureCodes._00;
			invLine22.JI_NewUsed = GoodsTypeList.Codes.S;
			Factory.Save();
			#endregion
			var testStrategy = new EntryCreationStrategy(declaration);
			CombineAssertions("Strategy Test for Imports", () =>
			{
				AssertEquals("same instruction, same header Key_1", testStrategy.GetKeyForHeader(invLine11), testStrategy.GetKeyForHeader(invLine21));
				AssertEquals("same instruction, same header Key_2", testStrategy.GetKeyForHeader(invLine11), testStrategy.GetKeyForHeader(invLine22));
				AssertNotEquals("different instruction, different header key", testStrategy.GetKeyForHeader(invLine11), testStrategy.GetKeyForHeader(invLine12));
				AssertNotEquals("same instruction, different line Key_1", testStrategy.GetKeyForLine(invLine11), testStrategy.GetKeyForLine(invLine21));
				AssertEquals("same instruction, same Line Key_2", testStrategy.GetKeyForLine(invLine11), testStrategy.GetKeyForLine(invLine22));
				AssertNotEquals("different instruction, different Line key", testStrategy.GetKeyForLine(invLine11), testStrategy.GetKeyForLine(invLine12));
			});
			LineMerger lineMerger = new LineMerger(declaration);
			lineMerger.DoMerge();
			CombineAssertions("LineMerger Test for Imports", () =>
			{
				AssertEquals("line11 and line 22 merge into same line", invLine11.JI_CL, invLine22.JI_CL);
				var headers = declaration.CustomsEntryHeaders.ToList();
				AssertEquals("Entry Header Count", 2, headers.Count);
				var entryHeader_1 = headers.OfType<CusEntryHeader>().FirstOrDefault(a => a.CustomsProcedureCode == ProcedureCodes._11);
				var entryHeader_2 = headers.OfType<CusEntryHeader>().FirstOrDefault(a => a.CustomsProcedureCode == ProcedureCodes._20);
				AssertEquals("Header 1 line Count", 2, entryHeader_1.AllEntryLines.Count);
				Assert("line11 and line22 should be with header1", entryHeader_1.AllEntryLines.Any<CusEntryLine>(a => a.PK == invLine11.JI_CL));
				Assert("line21 should be with header 1", entryHeader_1.AllEntryLines.Any<CusEntryLine>(a => a.PK == invLine21.JI_CL));
				AssertEquals("Header 2 line Count", 1, entryHeader_2.AllEntryLines.Count);
				Assert("line12 should be with header 2", entryHeader_2.AllEntryLines.Any<CusEntryLine>(a => a.PK == invLine12.JI_CL));
			});
			#region Test JI_NewUsed is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_NewUsed", () =>
			{
				invLine22.JI_NewUsed = GoodsTypeList.Codes.U;
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_NewUsed is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_NewUsed = GoodsTypeList.Codes.S;
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_NewUsed is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_ZZF_NKTaxType is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_ZZF_NKTaxType", () =>
			{
				invLine11.JI_ZZF_NKTaxType = "VAT";
				invLine22.JI_ZZF_NKTaxType = "VEX";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_ZZF_NKTaxType is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_ZZF_NKTaxType = "VAT";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_ZZF_NKTaxType is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_PrimaryPreference is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_PrimaryPreference", () =>
			{
				invLine11.JI_PrimaryPreference = "EUR";
				invLine22.JI_PrimaryPreference = "GSP";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_PrimaryPreference is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_PrimaryPreference = "EUR";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_PrimaryPreference is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_Description is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_Description", () =>
			{
				invLine11.JI_Description = "Desc1";
				invLine22.JI_Description = "Desc2";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_Description is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_Description = "Desc1";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_Description is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_ROOCert is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_ROOCert", () =>
			{
				invLine11.JI_ROOCert = "123";
				invLine22.JI_ROOCert = "124";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_ROOCert is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_ROOCert = "123";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_ROOCert is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_DiamondBeneficiaryLicense is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_DiamondBeneficiaryLicense", () =>
			{
				invLine11.JI_DiamondBeneficiaryLicense = "XX";
				invLine22.JI_DiamondBeneficiaryLicense = "YY";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_DiamondBeneficiaryLicense is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_DiamondBeneficiaryLicense = "XX";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_DiamondBeneficiaryLicense is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_DiamondDealerLicense is considered as MergeKey
			CombineAssertions("LineMerger Test of DiamondDealerLicense", () =>
			{
				invLine11.JI_DiamondDealerLicense = "XX";
				invLine22.JI_DiamondDealerLicense = "YY";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_DiamondDealerLicense is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_DiamondDealerLicense = "XX";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_DiamondDealerLicense is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_TemporaryExportExemption is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_TemporaryExportExemption", () =>
			{
				invLine11.JI_TemporaryExportExemption = "XX";
				invLine22.JI_TemporaryExportExemption = "YY";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_TemporaryExportExemption is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_TemporaryExportExemption = "XX";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_TemporaryExportExemption is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_DiamondLevyValue is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_DiamondLevyValue", () =>
			{
				invLine11.JI_DiamondLevyValue = 1;
				invLine22.JI_DiamondLevyValue = 2;
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_DiamondLevyValue is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_DiamondLevyValue = 1;
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should STILL NOT be merged into same line because JI_DiamondLevyValue is the same", invLine11.JI_CL, invLine22.JI_CL);
				invLine11.JI_DiamondLevyValue = 0;
				invLine22.JI_DiamondLevyValue = 0;
			});
			#endregion
			#region Test JI_DiamondProducerRegistration is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_DiamondProducerRegistration", () =>
			{
				invLine11.JI_DiamondProducerRegistration = "XX";
				invLine22.JI_DiamondProducerRegistration = "YY";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_DiamondProducerRegistration is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_DiamondProducerRegistration = "XX";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_DiamondProducerRegistration is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_DiamondProducerExemption is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_DiamondProducerExemption", () =>
			{
				invLine11.JI_DiamondProducerExemption = "XX";
				invLine22.JI_DiamondProducerExemption = "YY";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_DiamondProducerExemption is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_DiamondProducerExemption = "XX";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_DiamondProducerExemption is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_ElectionsExemptionsLevy is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_ElectionsExemptionsLevy", () =>
			{
				invLine11.JI_ElectionsExemptionsLevy = "1";
				invLine22.JI_ElectionsExemptionsLevy = "2";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_ElectionsExemptionsLevy is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_ElectionsExemptionsLevy = "1";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_ElectionsExemptionsLevy is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_KimberleyCertificate is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_KimberleyCertificate", () =>
			{
				invLine11.JI_KimberleyCertificate = "XX";
				invLine22.JI_KimberleyCertificate = "YY";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_KimberleyCertificate is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_KimberleyCertificate = "XX";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_KimberleyCertificate is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_TemporaryBuyersPermit is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_TemporaryBuyersPermit", () =>
			{
				invLine11.JI_TemporaryBuyersPermit = "XX";
				invLine22.JI_TemporaryBuyersPermit = "YY";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_TemporaryBuyersPermit is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_TemporaryBuyersPermit = "XX";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_TemporaryBuyersPermit is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_PermitNumber is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_PermitNumber", () =>
			{
				invLine11.JI_PermitNumber = "XX";
				invLine22.JI_PermitNumber = "YY";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_PermitNumber is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_PermitNumber = "XX";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_PermitNumber is the same", invLine11.JI_CL, invLine22.JI_CL);
			});
			#endregion
			#region Test JI_VIN is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_VIN", () =>
			{
				invLine11.JI_VIN = "VIN1";
				invLine22.JI_VIN = "VIN2";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_VIN is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_VIN = "VIN1";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should STILL NOT be merged into same line even though JI_VIN is the same", invLine11.JI_CL, invLine22.JI_CL);
				invLine11.JI_VIN = ZString.Empty;
				invLine22.JI_VIN = ZString.Empty;
			});
			#endregion
			#region Test JI_EngineNumber is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_EngineNumber", () =>
			{
				invLine11.JI_EngineNumber = "ENG01";
				invLine22.JI_EngineNumber = "ENG02";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_EngineNumber is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_EngineNumber = "ENG01";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should STILL NOT be merged into same line because JI_EngineNumber is the same", invLine11.JI_CL, invLine22.JI_CL);
				invLine11.JI_EngineNumber = ZString.Empty;
				invLine22.JI_EngineNumber = ZString.Empty;
			});
			#endregion
			#region Test UZ_Make is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_Make", () =>
			{
				invLine11.JI_Make = "Make01";
				invLine22.JI_Make = "Make02";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_Make is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_Make = "Make01";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should STILL NOT be merged into same line because JI_Make is the same", invLine11.JI_CL, invLine22.JI_CL);
				invLine11.JI_Make = ZString.Empty;
				invLine22.JI_Make = ZString.Empty;
			});
			#endregion
			#region Test JI_Model is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_Model", () =>
			{
				invLine11.JI_Model = "Model01";
				invLine22.JI_Model = "Model02";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_Model is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_Model = "Model01";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should STILL NOT be merged into same line because JI_Model is the same", invLine11.JI_CL, invLine22.JI_CL);
				invLine11.JI_Model = ZString.Empty;
				invLine22.JI_Model = ZString.Empty;
			});
			#endregion
			#region Test JI_VehicleFormat is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_VehicleFormat", () =>
			{
				invLine11.JI_VehicleFormat = "OTH";
				invLine22.JI_VehicleFormat = "FBU";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_VehicleFormat is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_VehicleFormat = "OTH";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should STILL NOT be merged into same line because JI_VehicleFormat is the same", invLine11.JI_CL, invLine22.JI_CL);
				invLine11.JI_VehicleFormat = ZString.Empty;
				invLine22.JI_VehicleFormat = ZString.Empty;
			});
			#endregion
			#region Test JI_VehicleType is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_VehicleType", () =>
			{
				invLine11.JI_VehicleType = "VehType01";
				invLine22.JI_VehicleType = "VehType02";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_VehicleType is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_VehicleType = "VehType01";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should STILL NOT be merged into same line because JI_VehicleType is the same", invLine11.JI_CL, invLine22.JI_CL);
				invLine11.JI_VehicleType = ZString.Empty;
				invLine22.JI_VehicleType = ZString.Empty;
			});
			#endregion
			#region Test JI_Colour is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_Colour", () =>
			{
				invLine11.JI_Colour = "blue";
				invLine22.JI_Colour = "red";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_Colour is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_Colour = "blue";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should STILL NOT be merged into same line because JI_Colour is the same", invLine11.JI_CL, invLine22.JI_CL);
				invLine11.JI_Colour = ZString.Empty;
				invLine22.JI_Colour = ZString.Empty;
			});
			#endregion
			#region Test JI_YearOfManufacture is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_YearOfManufacture", () =>
			{
				invLine11.JI_YearOfManufacture = "2015";
				invLine22.JI_YearOfManufacture = "2016";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_YearOfManufacture is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_YearOfManufacture = "2015";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should STILL NOT be merged into same line because JI_YearOfManufacture is the same", invLine11.JI_CL, invLine22.JI_CL);
				invLine11.JI_YearOfManufacture = ZString.Empty;
				invLine22.JI_YearOfManufacture = ZString.Empty;
			});
			#endregion
			#region Test JI_PreviousEntryNumber is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_PreviousEntryNumber", delegate
			{
				invLine11.JI_PreviousEntryNumber = "PREMRN1";
				invLine22.JI_PreviousEntryNumber = "PREMRN2";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_PreviousEntryNumber is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_PreviousEntryNumber = "PREMRN1";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_PreviousEntryNumber is the same", invLine11.JI_CL, invLine22.JI_CL, false);
				invLine11.JI_PreviousEntryNumber = ZString.Empty;
				invLine22.JI_PreviousEntryNumber = ZString.Empty;
			});
			#endregion
			#region Test JI_PreviousEntryLineNumber is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_PreviousEntryLineNumber", delegate
			{
				invLine11.JI_PreviousEntryLineNumber = 1;
				invLine22.JI_PreviousEntryLineNumber = 2;
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_PreviousEntryLineNumber is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_PreviousEntryLineNumber = 1;
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_PreviousEntryLineNumber is the same", invLine11.JI_CL, invLine22.JI_CL, false);
				invLine11.JI_PreviousEntryLineNumber = 0;
				invLine22.JI_PreviousEntryLineNumber = 0;
			});
			#endregion
			#region Test JI_AdvancePaymentNo is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_AdvancePaymentNo", () =>
			{
				invLine11.JI_AdvancePaymentNo = "001";
				invLine22.JI_AdvancePaymentNo = "002";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_AdvancePaymentNo is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_AdvancePaymentNo = "001";
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_AdvancePaymentNo is the same", invLine11.JI_CL, invLine22.JI_CL);
				invLine11.JI_AdvancePaymentNo = ZString.Empty;
				invLine22.JI_AdvancePaymentNo = ZString.Empty;
			});
			#endregion
			#region Test JI_ConversionFactor is considered as MergeKey
			CombineAssertions("LineMerger Test of JI_AdvancePaymentNo", () =>
			{
				invLine11.JI_ConversionFactor = 1.1m;
				invLine22.JI_ConversionFactor = 1.2m;
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertNotEquals("line11 and line 22 should NOT be merged into same line because JI_ConversionFactor is different", invLine11.JI_CL, invLine22.JI_CL);
				invLine22.JI_ConversionFactor = 1.1m;
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				AssertEquals("line11 and line 22 should be merged into same line because JI_ConversionFactor is the same", invLine11.JI_CL, invLine22.JI_CL);
				invLine11.JI_ConversionFactor = ZDecimal.Zero;
				invLine22.JI_ConversionFactor = ZDecimal.Zero;
			});
			#endregion
		}

		public void TestReusingCusEntry()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			var inst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			inst.CEI_Style = "11";
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = inst.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "00";
			CombineAssertions(() =>
			{
				var lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				Factory.Save();
				AssertEquals("First Merge", 1, declaration.ActiveEntryHeaders.Count);
				var entry = declaration.ActiveEntryHeaders[0];
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				Factory.Save();
				AssertEquals("Second Merge", 1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("Second Merge, unchanged entry", entry.PK, declaration.ActiveEntryHeaders[0].PK);
				var testMessage = Factory.NewWithValidTestData<CUSRESEDIMessageForTest>();
				testMessage.EM_ApplicationCode = "ZAC";
				testMessage.EM_ReceiveTransmit = "TRX";
				declaration.ActiveEntryHeaders[0].Messages.Add(testMessage);
				Factory.Save();
				lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();
				Factory.Save();
				AssertEquals("Merge after message", 1, declaration.ActiveEntryHeaders.Count);
				AssertEquals("Merge after message, unchanged entry", entry.PK, declaration.ActiveEntryHeaders[0].PK);
			});
		}

		public void TestTargetEntryLineNumberAffectsMerge()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "11";
			testInst.CEI_DateForDuty = new ZDateTime(2019, 1, 1);
			var testInvoice = declaration.Invoices.AddNew();
			testInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var testInvoiceLine1 = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine1.JI_CEI = testInst.PK;
			testInvoiceLine1.JI_Procedure = testInvoiceLine1.EntryInstruction.CEI_Style + "00";
			testInvoiceLine1.JI_Tariff = "101021";
			var testInvoiceLine2 = testInvoice.InvoiceLines.AddNew();
			testInvoiceLine2.JI_CEI = testInst.PK;
			testInvoiceLine2.JI_Procedure = testInvoiceLine2.EntryInstruction.CEI_Style + "00";
			testInvoiceLine2.JI_Tariff = "101021";
			CombineAssertions("Natual Merge By Tariff", () =>
			{
				declaration.DoMerge();
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals(1, entryHeader.MergedLines.Count);
				var mergedLine = entryHeader.MergedLines[0];
				AssertEquals(mergedLine.PK, testInvoiceLine1.JI_CL);
				AssertEquals(mergedLine.PK, testInvoiceLine2.JI_CL);
			});
			CombineAssertions("Merge By Tariff with different TargetEntryLineNumber One Specified One default", () =>
			{
				testInvoiceLine1.JI_TargetEntryLineNumber = 0;
				testInvoiceLine2.JI_TargetEntryLineNumber = 1;
				declaration.DoMerge();
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals(2, entryHeader.MergedLines.Count);
				var mergedLine1 = entryHeader.MergedLines[0];
				var mergedLine2 = entryHeader.MergedLines[1];
				AssertEquals(mergedLine2.PK, testInvoiceLine1.JI_CL);
				AssertEquals(mergedLine1.PK, testInvoiceLine2.JI_CL);
			});
			CombineAssertions("Merge By Tariff with same TargetEntryLineNumber", () =>
			{
				testInvoiceLine1.JI_TargetEntryLineNumber = 1;
				testInvoiceLine2.JI_TargetEntryLineNumber = 1;
				declaration.DoMerge();
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals(1, entryHeader.MergedLines.Count);
				var mergedLine = entryHeader.MergedLines[0];
				AssertEquals(mergedLine.PK, testInvoiceLine1.JI_CL);
				AssertEquals(mergedLine.PK, testInvoiceLine2.JI_CL);
			});
			CombineAssertions("Merge By Tariff with different TargetEntryLineNumber both Specified", () =>
			{
				testInvoiceLine1.JI_TargetEntryLineNumber = 1;
				testInvoiceLine2.JI_TargetEntryLineNumber = 2;
				declaration.DoMerge();
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals(2, entryHeader.MergedLines.Count);
				var mergedLine1 = entryHeader.MergedLines[0];
				var mergedLine2 = entryHeader.MergedLines[1];
				AssertEquals(mergedLine1.PK, testInvoiceLine1.JI_CL);
				AssertEquals(mergedLine2.PK, testInvoiceLine2.JI_CL);
			});
		}

		public void TestMarkupCustomsValueOverrideAffectsMerge()
		{
			#region Preperation
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = ProcedureCodes._11;
			var invHeader1 = declaration.Invoices.AddNew();
			var invLine11 = invHeader1.JobComInvoiceLines.AddNew();
			invLine11.JI_CEI = entryInstruction1.PK;
			invLine11.JI_Procedure = invLine11.EntryInstruction.CEI_Style + ProcedureCodes._00;
			var invLine12 = invHeader1.JobComInvoiceLines.AddNew();
			invLine12.JI_CEI = entryInstruction1.PK;
			invLine12.JI_Procedure = invLine12.EntryInstruction.CEI_Style + ProcedureCodes._00;
			invLine12.JI_ValuationMarkup = 20;
			var invLine13 = invHeader1.JobComInvoiceLines.AddNew();
			invLine13.JI_CEI = entryInstruction1.PK;
			invLine13.JI_Procedure = invLine13.EntryInstruction.CEI_Style + ProcedureCodes._00;
			invLine13.JI_CustomsValueOverride = 10;
			invLine13.JI_RX_NKCustomsValueCurrencyOverride = "USD";
			var invLine14 = invHeader1.JobComInvoiceLines.AddNew();
			invLine14.JI_CEI = entryInstruction1.PK;
			invLine14.JI_Procedure = invLine14.EntryInstruction.CEI_Style + ProcedureCodes._00;
			var invLine15 = invHeader1.JobComInvoiceLines.AddNew();
			invLine15.JI_CEI = entryInstruction1.PK;
			invLine15.JI_Procedure = invLine15.EntryInstruction.CEI_Style + ProcedureCodes._00;
			invLine15.JI_CustomsValueOverride = 10;
			invLine15.JI_RX_NKCustomsValueCurrencyOverride = "XXX";
			Factory.Save();
			#endregion
			var testStrategy = new EntryCreationStrategy(declaration);
			CombineAssertions("Strategy Test for Exports", () =>
			{
				AssertNotEquals("no charge vs markup", testStrategy.GetKeyForLine(invLine11), testStrategy.GetKeyForLine(invLine12));
				AssertNotEquals("no charge vs override", testStrategy.GetKeyForLine(invLine11), testStrategy.GetKeyForLine(invLine13));
				AssertNotEquals("markup vs override", testStrategy.GetKeyForLine(invLine12), testStrategy.GetKeyForLine(invLine13));
				AssertEquals("no charge vs no charge", testStrategy.GetKeyForLine(invLine11), testStrategy.GetKeyForLine(invLine14));
				AssertEquals("no charge vs no valid override", testStrategy.GetKeyForLine(invLine11), testStrategy.GetKeyForLine(invLine15));
				new LineMerger(declaration).DoMerge();
				AssertEquals("Header Count", 1, declaration.CustomsEntryHeaders.Count);
				AssertEquals("Line Count", 3, declaration.CustomsEntryHeaders[0].MergedLines.Count);
			});
		}

		public void TestCreateNewEntryHeaderWhenOldEntryheaderHasResponse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction1.PK;
			invoiceLine1.JI_Procedure = invoiceLine1.EntryInstruction.CEI_Style + "00";
			invoiceLine1.JI_Tariff = "101021";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction1.PK;
			invoiceLine2.JI_Procedure = invoiceLine2.EntryInstruction.CEI_Style + "00";
			invoiceLine2.JI_Tariff = "101021";
			declaration.DoMerge();
			AssertEquals("declaration.ActiveEntryHeaders.Count", 1, declaration.ActiveEntryHeaders.Count);
			var entryHeader = declaration.ActiveEntryHeaders[0];
			AssertEquals("entryHeader.CH_CEI_Instruction", instruction1.PK, entryHeader.CH_CEI_Instruction);
			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Style = "20";
			invoiceLine1.JI_CEI = instruction2.PK;
			invoiceLine2.JI_CEI = instruction2.PK;
			declaration.DoMerge();
			AssertEquals("declaration.ActiveEntryHeaders.Count", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("entryHeader reused", entryHeader, declaration.ActiveEntryHeaders[0]);
			AssertEquals("entryHeader.CH_CEI_Instruction", instruction2.PK, entryHeader.CH_CEI_Instruction);
			entryHeader.Messages.AddNew().EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration.DoMerge();
			AssertEquals("declaration.ActiveEntryHeaders.Count", 1, declaration.ActiveEntryHeaders.Count);
			AssertEquals("entryHeader reused", entryHeader, declaration.ActiveEntryHeaders[0]);
			AssertEquals("entryHeader.CH_CEI_Instruction", instruction2.PK, entryHeader.CH_CEI_Instruction);
			invoiceLine1.JI_CEI = instruction1.PK;
			invoiceLine2.JI_CEI = instruction1.PK;
			declaration.DoMerge();
			AssertEquals("declaration.ActiveEntryHeaders.Count", 2, declaration.CustomsEntryHeaders.Count);
			Assert("entryHeader has NOT been deleted", !entryHeader.IsDeleted);
			AssertEquals("entryHeader.CH_CEI_Instruction has NOT been changed", instruction2.PK, entryHeader.CH_CEI_Instruction);
			var newEntryHeader = declaration.CustomsEntryHeaders.First(x => x != entryHeader);
			AssertEquals("entryHeader.CH_CEI_Instruction has NOT been changed", instruction1.PK, newEntryHeader.CH_CEI_Instruction);
		}

		[TestDate(2016, 12, 02)]
		public void TestMergingForExportJob_OnlyOneEntryInstructionWithMixedCurrency()
		{
			var helper = new TestHelper(Factory);
			var universalDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			helper.SetExchangeRate(helper.USDCurrency, 0.5m, new ZDateTime(2016, 12, 01));
			helper.SetExchangeRate(helper.EURCurrency, 0.6m, new ZDateTime(2016, 12, 01));
			universalDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", ZString.Empty, ZString.Empty, "XX__", JobMessageTypeList.Codes.Export);
			universalDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", "YY", ZString.Empty, "XXYY", JobMessageTypeList.Codes.Export);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryInst = declaration.CustomsEntryInstructions.AddNew();
			entryInst.CEI_Style = "XX";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var usdInvoice = declaration.Invoices.AddNew();
			usdInvoice.JZ_IncoTerm = "FOB";
			usdInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			usdInvoice.JZ_InvoiceAmount = 500m;
			var eurInvoice = declaration.Invoices.AddNew();
			eurInvoice.JZ_IncoTerm = "FOB";
			eurInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			eurInvoice.JZ_InvoiceAmount = 600m;
			var zarInvoice = declaration.Invoices.AddNew();
			zarInvoice.JZ_IncoTerm = "FOB";
			zarInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			zarInvoice.JZ_InvoiceAmount = 700m;
			var usdLine1 = usdInvoice.InvoiceLines.AddNew();
			usdLine1.JI_CEI = entryInst.PK;
			usdLine1.JI_Procedure = usdLine1.EntryInstruction.CEI_Style + "YY";
			var usdLine2 = usdInvoice.InvoiceLines.AddNew();
			usdLine2.JI_CEI = entryInst.PK;
			usdLine2.JI_Procedure = usdLine2.EntryInstruction.CEI_Style + "YY";
			var eurLine1 = eurInvoice.InvoiceLines.AddNew();
			eurLine1.JI_CEI = entryInst.PK;
			eurLine1.JI_Procedure = eurLine1.EntryInstruction.CEI_Style + "YY";
			var eurLine2 = eurInvoice.InvoiceLines.AddNew();
			eurLine2.JI_CEI = entryInst.PK;
			eurLine2.JI_Procedure = eurLine2.EntryInstruction.CEI_Style + "YY";
			var zarLine1 = zarInvoice.InvoiceLines.AddNew();
			zarLine1.JI_CEI = entryInst.PK;
			zarLine1.JI_Procedure = zarLine1.EntryInstruction.CEI_Style + "YY";
			var zarLine2 = zarInvoice.InvoiceLines.AddNew();
			zarLine2.JI_CEI = entryInst.PK;
			zarLine2.JI_Procedure = zarLine2.EntryInstruction.CEI_Style + "YY";
			Factory.Save();
			AssertEquals("Pre: No Merge", 0, declaration.ActiveEntryHeaders.Count);
			declaration.DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals("Merged Into One", 1, declaration.ActiveEntryHeaders.Count);
				var entryHeader = declaration.ActiveEntryHeaders[0];
				AssertEquals("usdLine1.CusEntryLine.Header", entryHeader, usdLine1.CusEntryLine.Header);
				AssertEquals("usdLine2.CusEntryLine.Header", entryHeader, usdLine2.CusEntryLine.Header);
				AssertEquals("eurLine1.CusEntryLine.Header", entryHeader, eurLine1.CusEntryLine.Header);
				AssertEquals("eurLine2.CusEntryLine.Header", entryHeader, eurLine2.CusEntryLine.Header);
				AssertEquals("zarLine1.CusEntryLine.Header", entryHeader, zarLine1.CusEntryLine.Header);
				AssertEquals("zarLine2.CusEntryLine.Header", entryHeader, zarLine2.CusEntryLine.Header);
			});
		}

		[TestDate(2016, 12, 02)]
		public void TestMergingForExportJob_OnlyOneEntryInstructionWithMixedCurrencyButDifferentValuationCode()
		{
			var helper = new TestHelper(Factory);
			var universalDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			helper.SetExchangeRate(helper.USDCurrency, 0.5m, new ZDateTime(2016, 12, 01));
			helper.SetExchangeRate(helper.EURCurrency, 0.6m, new ZDateTime(2016, 12, 01));
			universalDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", ZString.Empty, ZString.Empty, "XX__", JobMessageTypeList.Codes.Export);
			universalDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", "YY", ZString.Empty, "XXYY", JobMessageTypeList.Codes.Export);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryInst = declaration.CustomsEntryInstructions.AddNew();
			entryInst.CEI_Style = "XX";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var usdInvoice = declaration.Invoices.AddNew();
			usdInvoice.JZ_IncoTerm = "FOB";
			usdInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			usdInvoice.JZ_InvoiceAmount = 500m;
			var eurInvoice = declaration.Invoices.AddNew();
			eurInvoice.JZ_IncoTerm = "FOB";
			eurInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			eurInvoice.JZ_InvoiceAmount = 600m;
			var zarInvoice = declaration.Invoices.AddNew();
			zarInvoice.JZ_IncoTerm = "FOB";
			zarInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			zarInvoice.JZ_InvoiceAmount = 700m;
			var usdLine1 = usdInvoice.InvoiceLines.AddNew();
			usdLine1.JI_CEI = entryInst.PK;
			usdLine1.JI_Procedure = usdLine1.EntryInstruction.CEI_Style + "YY";
			var usdLine2 = usdInvoice.InvoiceLines.AddNew();
			usdLine2.JI_CEI = entryInst.PK;
			usdLine2.JI_Procedure = usdLine2.EntryInstruction.CEI_Style + "YY";
			var eurLine1 = eurInvoice.InvoiceLines.AddNew();
			eurLine1.JI_CEI = entryInst.PK;
			eurLine1.JI_Procedure = eurLine1.EntryInstruction.CEI_Style + "YY";
			var eurLine2 = eurInvoice.InvoiceLines.AddNew();
			eurLine2.JI_CEI = entryInst.PK;
			eurLine2.JI_Procedure = eurLine2.EntryInstruction.CEI_Style + "YY";
			var zarLine1 = zarInvoice.InvoiceLines.AddNew();
			zarLine1.JI_CEI = entryInst.PK;
			zarLine1.JI_Procedure = zarLine1.EntryInstruction.CEI_Style + "YY";
			var zarLine2 = zarInvoice.InvoiceLines.AddNew();
			zarLine2.JI_CEI = entryInst.PK;
			zarLine2.JI_Procedure = zarLine2.EntryInstruction.CEI_Style + "YY";
			usdInvoice.JZ_VDN = "VDNUSD";
			eurInvoice.JZ_VDN = "VDNEUR";
			zarInvoice.JZ_VDN = "VDNZAR";
			usdInvoice.JZ_RelatedIndicator = "Y";
			eurInvoice.JZ_RelatedIndicator = "N";
			zarInvoice.JZ_RelatedIndicator = "E";
			usdInvoice.JZ_ValuationCode = "1";
			eurInvoice.JZ_ValuationCode = "2";
			zarInvoice.JZ_ValuationCode = "3";
			Factory.Save();
			AssertEquals("Pre: No Merge", 0, declaration.ActiveEntryHeaders.Count);
			declaration.DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals("Merged Into One", 1, declaration.ActiveEntryHeaders.Count);
				var entryHeader = declaration.ActiveEntryHeaders[0];
				AssertEquals("usdLine1.CusEntryLine.Header", entryHeader, usdLine1.CusEntryLine.Header);
				AssertEquals("usdLine2.CusEntryLine.Header", entryHeader, usdLine2.CusEntryLine.Header);
				AssertEquals("eurLine1.CusEntryLine.Header", entryHeader, eurLine1.CusEntryLine.Header);
				AssertEquals("eurLine2.CusEntryLine.Header", entryHeader, eurLine2.CusEntryLine.Header);
				AssertEquals("zarLine1.CusEntryLine.Header", entryHeader, zarLine1.CusEntryLine.Header);
				AssertEquals("zarLine2.CusEntryLine.Header", entryHeader, zarLine2.CusEntryLine.Header);
			});
		}

		[TestDate(2016, 12, 02)]
		public void TestMergingForExportJob_MultipleEntryInstructionSingleInvoiceTwoLinesAllInZAR()
		{
			var helper = new TestHelper(Factory);
			var universalDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			helper.SetExchangeRate(helper.USDCurrency, 0.5m, new ZDateTime(2016, 12, 01));
			helper.SetExchangeRate(helper.EURCurrency, 0.6m, new ZDateTime(2016, 12, 01));
			universalDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", ZString.Empty, ZString.Empty, "XX__", JobMessageTypeList.Codes.Export);
			universalDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", "YY", ZString.Empty, "XXYY", JobMessageTypeList.Codes.Export);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryInst1 = declaration.CustomsEntryInstructions.AddNew();
			entryInst1.CEI_Style = "XX";
			entryInst1.CEI_Description = "XX_1";
			var entryInst2 = declaration.CustomsEntryInstructions.AddNew();
			entryInst2.CEI_Style = "XX";
			entryInst2.CEI_Description = "XX_2";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var zarInvoice = declaration.Invoices.AddNew();
			zarInvoice.JZ_IncoTerm = "FOB";
			zarInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			zarInvoice.JZ_InvoiceAmount = 700m;
			var zarLine1 = zarInvoice.InvoiceLines.AddNew();
			zarLine1.JI_CEI = entryInst1.PK;
			var zarLine2 = zarInvoice.InvoiceLines.AddNew();
			zarLine2.JI_CEI = entryInst2.PK;
			Factory.Save();
			AssertEquals("Pre: No Merge", 0, declaration.ActiveEntryHeaders.Count);
			declaration.DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals("Merged Into two", 2, declaration.ActiveEntryHeaders.Count);
				AssertNotEquals("On Different Entries", zarLine1.CusEntryLine.Header.PK, zarLine2.CusEntryLine.Header.PK);
			});
		}

		[TestDate(2016, 12, 02)]
		public void TestMergingForExportJob_MultipleEntryInstructionSingleInvoiceTwoLinesMixedCurrency()
		{
			var universalDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", ZString.Empty, ZString.Empty, "XX__", JobMessageTypeList.Codes.Export);
			universalDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", "YY", ZString.Empty, "XXYY", JobMessageTypeList.Codes.Export);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryInst1 = declaration.CustomsEntryInstructions.AddNew();
			entryInst1.CEI_Style = "XX";
			entryInst1.CEI_Description = "XX_1";
			var entryInst2 = declaration.CustomsEntryInstructions.AddNew();
			entryInst2.CEI_Style = "XX";
			entryInst2.CEI_Description = "XX_2";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var zarInvoice = declaration.Invoices.AddNew();
			zarInvoice.JZ_IncoTerm = "FOB";
			zarInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			zarInvoice.JZ_InvoiceAmount = 700m;
			var zarLine1 = zarInvoice.InvoiceLines.AddNew();
			zarLine1.JI_CEI = entryInst1.PK;
			zarLine1.JI_Procedure = zarLine1.EntryInstruction.CEI_Style + "YY";
			var zarLine2 = zarInvoice.InvoiceLines.AddNew();
			zarLine2.JI_CEI = entryInst2.PK;
			zarLine2.JI_Procedure = zarLine2.EntryInstruction.CEI_Style + "YY";
			var eurInvoice = declaration.Invoices.AddNew();
			eurInvoice.JZ_IncoTerm = "FOB";
			eurInvoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			eurInvoice.JZ_InvoiceAmount = 800m;
			var eurLine1 = eurInvoice.InvoiceLines.AddNew();
			eurLine1.JI_CEI = entryInst1.PK;
			eurLine1.JI_Procedure = eurLine1.EntryInstruction.CEI_Style + "YY";
			var eurLine2 = eurInvoice.InvoiceLines.AddNew();
			eurLine2.JI_CEI = entryInst2.PK;
			eurLine2.JI_Procedure = eurLine2.EntryInstruction.CEI_Style + "YY";
			AssertEquals("Pre: No Merge", 0, declaration.ActiveEntryHeaders.Count);
			declaration.DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals("Merged Into two entryHeaders", 2, declaration.ActiveEntryHeaders.Count);
				AssertNotEquals("zar1zar2 On Different Entries", zarLine1.CusEntryLine.Header.PK, zarLine2.CusEntryLine.Header.PK);
				AssertNotEquals("eur1eur2 On Different Entries", eurLine1.CusEntryLine.Header.PK, eurLine2.CusEntryLine.Header.PK);
				AssertEquals("zar1eur1 On The Same Entries", zarLine1.CusEntryLine.Header.PK, eurLine1.CusEntryLine.Header.PK);
				AssertEquals("zar2eur2 On The Same Entries", zarLine2.CusEntryLine.Header.PK, eurLine2.CusEntryLine.Header.PK);
				AssertNotEquals("zar1eur2 On Different Entries", zarLine1.CusEntryLine.Header.PK, eurLine2.CusEntryLine.Header.PK);
				AssertNotEquals("zar2eur1 On Different Entries", eurLine1.CusEntryLine.Header.PK, zarLine2.CusEntryLine.Header.PK);
			});
		}

		[TestDate(2016, 12, 02)]
		public void TestMergingForExportJob_MultipleEntryInstructionTwoInvoiceWithTwoAndThreeLinesAllInZAR()
		{
			var universalDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			universalDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", ZString.Empty, ZString.Empty, "XX__", JobMessageTypeList.Codes.Export);
			universalDataHelper.CreateRefCusProcedure(Core.Constants.CountryCodes.SouthAfrica, "X", "XX", "YY", ZString.Empty, "XXYY", JobMessageTypeList.Codes.Export);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryInst1 = declaration.CustomsEntryInstructions.AddNew();
			entryInst1.CEI_Style = "XX";
			entryInst1.CEI_Description = "XX_1";
			var entryInst2 = declaration.CustomsEntryInstructions.AddNew();
			entryInst2.CEI_Style = "XX";
			entryInst2.CEI_Description = "XX_2";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var zarInvoice1 = declaration.Invoices.AddNew();
			zarInvoice1.JZ_IncoTerm = "FOB";
			zarInvoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			zarInvoice1.JZ_InvoiceAmount = 700m;
			var zarInvoice2 = declaration.Invoices.AddNew();
			zarInvoice2.JZ_IncoTerm = "FOB";
			zarInvoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			zarInvoice2.JZ_InvoiceAmount = 700m;
			var zarLine11 = zarInvoice1.InvoiceLines.AddNew();
			zarLine11.JI_CEI = entryInst1.PK;
			zarLine11.JI_Procedure = zarLine11.EntryInstruction.CEI_Style + "YY";
			var zarLine12 = zarInvoice1.InvoiceLines.AddNew();
			zarLine12.JI_CEI = entryInst2.PK;
			zarLine12.JI_Procedure = zarLine12.EntryInstruction.CEI_Style + "YY";
			var zarLine21 = zarInvoice2.InvoiceLines.AddNew();
			zarLine21.JI_CEI = entryInst1.PK;
			zarLine21.JI_Procedure = zarLine21.EntryInstruction.CEI_Style + "YY";
			var zarLine22 = zarInvoice2.InvoiceLines.AddNew();
			zarLine22.JI_CEI = entryInst2.PK;
			zarLine22.JI_Procedure = zarLine22.EntryInstruction.CEI_Style + "YY";
			var zarLine23 = zarInvoice2.InvoiceLines.AddNew();
			zarLine23.JI_CEI = entryInst2.PK;
			zarLine23.JI_Procedure = zarLine23.EntryInstruction.CEI_Style + "YY";
			Factory.Save();
			AssertEquals("Pre: No Merge", 0, declaration.ActiveEntryHeaders.Count);
			declaration.DoMerge();
			CombineAssertions(() =>
			{
				AssertEquals("Merged Into two", 2, declaration.ActiveEntryHeaders.Count);
				AssertNotEquals(zarLine11.CusEntryLine.Header.PK, zarLine12.CusEntryLine.Header.PK);
				AssertNotEquals(zarLine21.CusEntryLine.Header.PK, zarLine22.CusEntryLine.Header.PK);
				AssertEquals(zarLine11.CusEntryLine.Header.PK, zarLine11.CusEntryLine.Header.PK);
				AssertEquals(zarLine12.CusEntryLine.Header.PK, zarLine22.CusEntryLine.Header.PK);
				AssertEquals(zarLine12.CusEntryLine.Header.PK, zarLine23.CusEntryLine.Header.PK);
			});
		}
	}
}
