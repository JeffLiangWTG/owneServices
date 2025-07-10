using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

class AESGoodsItemProviderTest : Customs.Business.Testing.DataProviderTestCase<AESGoodsItemProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("Null EntryLine", "Value cannot be null.\r\nParameter name: entryLine", () => new AESGoodsItemProvider(null, 1, null));
		AssertExceptionThrown<ArgumentNullException>("Null Related Declaration", "Value cannot be null.\r\nParameter name: entryLine.Declaration", () => new AESGoodsItemProvider(Factory.New<CusEntryLine>(), 1, null));
		AssertExceptionThrown<ArgumentNullException>("Null Related Instruction", "Value cannot be null.\r\nParameter name: entryHeader.EntryInstruction", () => new AESGoodsItemProvider(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew().AllEntryLines.AddNew(), 1, null));
		AssertExceptionThrown<ArgumentNullException>("Null Parent Provider", "Value cannot be null.\r\nParameter name: parentProvider", () => new AESGoodsItemProvider(EntryLine, 1, null));
	});

	public void TestDeclarationGoodsItemNumber() => AssertEquals(999, GetProvider().DeclarationGoodsItemNumber);

	public void TestStatisticalValueValue() => CombineAssertions(() =>
	{
		EntryLine.CL_StatisticalValue = 15m;

		var allEntryStyleCodes = Declaration.Lookups.EntryStyleList.GetAllCodes();
		var allEntrySubStyleCodes = EntryInstruction.Lookups.EntrySubStyleList.GetAllCodes();

		using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			var testCases = allEntryStyleCodes.SelectMany(entryStyleCode => allEntrySubStyleCodes, (entryStyleCode, entrySubStyleCode) => (entryStyleCode, entrySubStyleCode));
			foreach (var (entryStyleCode, entrySubStyleCode) in testCases)
			{
				Declaration.JE_EntryStyle = entryStyleCode;
				EntryInstruction.CEI_SubStyle = entrySubStyleCode;
				AssertEquals($"IsUCC6: false, EntryStyle: {entryStyleCode}, EntrySubStyle: {entrySubStyleCode}", 15.00m, GetProvider().StatisticalValueValue);
			}
		}

		using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			Declaration.JE_EntryStyle = "CO";
			foreach (var entrySubStyleCode in allEntrySubStyleCodes)
			{
				EntryInstruction.CEI_SubStyle = entrySubStyleCode;
				AssertEquals($"IsUCC6: true, EntryStyle: CO, EntrySubStyle: {entrySubStyleCode}", 15.00m, GetProvider().StatisticalValueValue);
			}

			Declaration.JE_EntryStyle = "EX";
			string[] exclusiveRuleC0028SubStyleCodes = [EntrySubStyleList.Codes.IncompleteDeclaration, EntrySubStyleList.Codes.SimplifiedDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC];
			foreach (var entrySubStyleCode in allEntrySubStyleCodes.Except(exclusiveRuleC0028SubStyleCodes))
			{
				EntryInstruction.CEI_SubStyle = entrySubStyleCode;
				AssertEquals($"IsUCC6: true, EntryStyle: EX, EntrySubStyle: {entrySubStyleCode}", 15.00m, GetProvider().StatisticalValueValue);
			}

			foreach (var entrySubStyleCode in exclusiveRuleC0028SubStyleCodes)
			{
				EntryInstruction.CEI_SubStyle = entrySubStyleCode;
				AssertNull($"IsUCC6: true, EntryStyle: EX, EntrySubStyle: {entrySubStyleCode}", GetProvider().StatisticalValueValue);
			}
		}
	});

	public virtual void TestNatureOfTransaction() => CombineAssertions(() =>
	{
		using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			AssertNull("Empty Declaration", GetProvider().NatureOfTransaction);

			Invoice.JZ_ValuationCode = "A";
			AssertNull("Single JZ_ValuationCode exists", GetProvider().NatureOfTransaction);

			var invoice2 = Declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = EntryInstruction.PK;

			invoice2.JZ_ValuationCode = "A";
			AssertNull("More than 1 same JZ_ValuationCode exists / Nature of Transaction set at GoodsShipment", GetProvider().NatureOfTransaction);

			invoice2.JZ_ValuationCode = "B";
			AssertNotNull("More than 1 different JZ_ValuationCode exists / Nature of Transaction not set at GoodsShipment", GetProvider().NatureOfTransaction);
		}

		using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			AssertNull("After TransitionPeriod Declaration", GetProvider().NatureOfTransaction);
		}
	});

	public void TestMultiNatureOfTransaction() => CombineAssertions(() =>
	{
		using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_ValuationCode = "A";
			var invoice2 = Declaration.Invoices.AddNew();
			invoice2.JZ_ValuationCode = "B";
			var entryHeader = Declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_CL = entryLine1.PK;

			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			var provide1 = new AESGoodsItemProvider(entryLine1, 999, new AESGoodsShipmentProvider(entryHeader));
			var provide2 = new AESGoodsItemProvider(entryLine2, 999, new AESGoodsShipmentProvider(entryHeader));

			AssertEquals("Nature of Transaction set at GoodsItem", "A", provide1.NatureOfTransaction);
			AssertEquals("Nature of Transaction set at GoodsItem", "B", provide2.NatureOfTransaction);
		}
	});

	public void TestCountryOfExport() => AssertNull(GetProvider().CountryOfExport);

	public void TestCountryOfDestination() => CombineAssertions(() =>
	{
		var invoice2 = Declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();

		Declaration.JE_GoodsDestination = ZString.Empty;
		AssertNull("JE_GoodsDestination is empty", GetProvider().CountryOfDestination);

		Declaration.JE_GoodsDestination = CountryCodes.Germany;
		AssertNull("JE_GoodsDestination is Germany", GetProvider().CountryOfDestination);

		InvoiceLine.ZG_CountryOfDestination = CountryCodes.Germany;
		AssertNull("JE_GoodsDestination and ZG_CountryOfDestination is Germany", GetProvider().CountryOfDestination);

		InvoiceLine.ZG_CountryOfDestination = CountryCodes.Poland;
		AssertEquals("JE_GoodsDestination and ZG_CountryOfDestination are different", CountryCodes.Poland, GetProvider().CountryOfDestination);

		Declaration.JE_GoodsDestination = ZString.Empty;
		AssertNull("JE_GoodsDestination is empty and exists single ZG_CountryOfDestination", GetProvider().CountryOfDestination);

		invoiceLine2.ZG_CountryOfDestination = CountryCodes.Germany;
		AssertEquals("Different ZG_CountryOfDestination", CountryCodes.Poland, GetProvider().CountryOfDestination);

		Declaration.JE_GoodsDestination = CountryCodes.Germany;
		invoiceLine2.ZG_CountryOfDestination = CountryCodes.Poland;
		AssertNull("All same ZG_CountryOfDestination with different JE_GoodsDestination", GetProvider().CountryOfDestination);
	});

	public void TestReferenceNumberUCR() => AssertNull(GetProvider().ReferenceNumberUCR);

	public void TestAuthorisations() => CombineAssertions(() =>
	{
		AssertEquals("Empty declaration", 0, GetProvider().Authorisations.Count);

		var authUsage = InvoiceLine.CusAuthorizationUsages.AddNew();
		authUsage.AGC_Code = "A";
		authUsage.AGC_Number = "123";
		var authUsage2 = InvoiceLine.CusAuthorizationUsages.AddNew();
		authUsage2.AGC_Code = "A";
		authUsage2.AGC_Number = "123";
		var authUsage3 = InvoiceLine.CusAuthorizationUsages.AddNew();
		authUsage3.AGC_Code = "B";
		authUsage3.AGC_Number = "123";
		var authUsage4 = InvoiceLine.CusAuthorizationUsages.AddNew();
		authUsage4.AGC_Code = "B";
		authUsage4.AGC_Number = "456";

		AssertEquals("4 CusAuthorizationUsages where 3 have unique AGC_Code and AGC_Number", 3, GetProvider().Authorisations.Count);
	});

	public void TestProcedure() => AssertNotNull(GetProvider().Procedure);

	public virtual void TestConsignor() => CombineAssertions(() =>
	{
		AssertNull("Empty Consignor", GetProvider().Consignor);

		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = orgHeader.Addresses.AddNew();
		InvoiceLine.JI_OA_ExporterAddress = orgAddress.PK;
		AssertNotNull("Not Empty Consignor", GetProvider().Consignor);
	});

	public void TestConsignee() => CombineAssertions(() =>
	{
		using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			AssertNull("Empty Consignor", GetProvider().Consignee);
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			InvoiceLine.JI_OA_ConsigneeAddress = orgAddress.PK;
			AssertNotNull("Not Empty Consignor", GetProvider().Consignee);
			var addInfo = InvoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_Code = Constants.AdditionalInfoCodes._30600;
			AssertNotNull("30600 Add info and false IsAESTransitionPeriod", GetProvider().Consignee);
		}

		using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			AssertNull("30600 Add info and true IsAESTransitionPeriod", GetProvider().Consignee);
		}
	});

	public virtual void TestConsigneeType()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = orgHeader.Addresses.AddNew();
		InvoiceLine.JI_OA_ConsigneeAddress = orgAddress.PK;

		AssertType<AESConsigneeConsignorProvider>(GetProvider().Consignee);
	}

	public void TestAdditionalSupplyChainActors()
	{
		InvoiceLine.CusSupplyChainActorReferences.AddNew();
		InvoiceLine.CusSupplyChainActorReferences.AddNew();
		InvoiceLine.CusSupplyChainActorReferences.AddNew();
		AssertEquals(3, Provider.AdditionalSupplyChainActors.Count);
	}

	public void TestOrigin() => AssertNotNull(GetProvider().Origin);

	public void TestCommodity() => AssertNotNull(GetProvider().Commodity);

	public void TestPackaging() => CombineAssertions(() =>
	{
		AssertEquals("Empty Packages list", 0, GetProvider().Packaging.Count);

		var invoiceLine1 = EntryLine.InvoiceLines.AddNew();
		var invoiceLine2 = EntryLine.InvoiceLines.AddNew();

		var package1 = Declaration.Packages.AddNew();
		package1.CW_PackType = "ABC";
		package1.CW_MarksAndNos = "EFG";
		package1.CW_PackQty = 4;
		var package2 = Declaration.Packages.AddNew();
		package2.CW_PackType = "CBA";
		package2.CW_MarksAndNos = "GFE";
		package2.CW_PackQty = 11;

		var line1Pivot1 = invoiceLine1.PackagesForInvoiceLinesForBindingOnly.AddNew();
		line1Pivot1.Package = package1;
		line1Pivot1.IsLinked = true;
		line1Pivot1.PackQty = 2;

		var line2Pivot1 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.AddNew();
		line2Pivot1.Package = package1;
		line2Pivot1.IsLinked = true;
		line2Pivot1.PackQty = 1;
		var line2Pivot2 = invoiceLine2.PackagesForInvoiceLinesForBindingOnly.AddNew();
		line2Pivot2.Package = package2;
		line2Pivot2.IsLinked = true;
		line2Pivot2.PackQty = 6;

		var packages = GetProvider().Packaging.ToArray();
		AssertEquals("2 Packages combined", 2, packages.Length);
		TestHelper.TestCollectionHasCorrectIndexOrder(GetProvider().Packaging, (item) => item.SequenceNumber);
	});

	public void TestPreviousDocumentsFromEntryInstruction() => CombineAssertions(
		"The previous documents from entry instruction level should be added to GoodsItem " +
		"ONLY in transition period and only if EntryInstruction.CEI_SubStyle is not one of [X, Y, Z]", () =>
	{
		AssertEquals("No Previous Documents", 0, GetProvider().PreviousDocuments.Count);
		EntryInstruction.PreviousDocuments.AddNew();

		using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			AssertEquals("Is UCC6 and sub-style is not in [X, Y, Z]", 0, GetProvider().PreviousDocuments.Count);
			EntryInstruction.CEI_SubStyle = "X";
			AssertEquals("Is UCC6 and sub-style is X", 0, GetProvider().PreviousDocuments.Count);
			EntryInstruction.CEI_SubStyle = "Y";
			AssertEquals("Is UCC6 and sub-style is Y", 0, GetProvider().PreviousDocuments.Count);
			EntryInstruction.CEI_SubStyle = "Z";
			AssertEquals("Is UCC6 and sub-style is Z", 0, GetProvider().PreviousDocuments.Count);
		}

		using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			EntryInstruction.CEI_SubStyle = "";
			AssertEquals("Is in transition period and sub-style is not in [X, Y, Z]", 1, GetProvider().PreviousDocuments.Count);
			EntryInstruction.CEI_SubStyle = "X";
			AssertEquals("Is in transition period and sub-style is X", 0, GetProvider().PreviousDocuments.Count);
			EntryInstruction.CEI_SubStyle = "Y";
			AssertEquals("Is in transition period and sub-style is Y", 0, GetProvider().PreviousDocuments.Count);
			EntryInstruction.CEI_SubStyle = "Z";
			AssertEquals("Is in transition period and sub-style is Z", 0, GetProvider().PreviousDocuments.Count);
		}
	});

	public void TestPreviousDocuments() => CombineAssertions(() =>
	{
		AssertEquals("No Previous Documents", 0, GetProvider().PreviousDocuments.Count);

		var instructionLevelDoc = EntryInstruction.PreviousDocuments.AddNew();
		instructionLevelDoc.CSI_ReferenceNumber = "instruction";
		var headerLevelDoc = Invoice.PreviousDocuments.AddNew();
		headerLevelDoc.CSI_ReferenceNumber = "header";
		var lineLevelDoc = InvoiceLine.PreviousDocuments.AddNew();
		lineLevelDoc.CSI_ReferenceNumber = "line";

		using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			AssertEquals("2 Previous Documents, Declaration is UCC6", 2, GetProvider().PreviousDocuments.Count);
		}

		using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			AssertEquals("3 Previous Documents", 3, GetProvider().PreviousDocuments.Count);

			instructionLevelDoc.CSI_Code = PreviousDocumentCodes.SDE;
			headerLevelDoc.CSI_Code = PreviousDocumentCodes.CLE;
			lineLevelDoc.CSI_Code = PreviousDocumentCodes.MRN;
			AssertEquals("No Previous Documents due to CSI_Code", 0, GetProvider().PreviousDocuments.Count);
		}
	});

	public void TestDeduplicationPreviousDocuments() => CombineAssertions(() =>
	{
		var document1 = InvoiceLine.PreviousDocuments.AddNew();
		document1.CSI_Code = PreviousDocumentCodes._337;
		document1.CSI_ReferenceNumber = "Invoice Line document";
		var document2 = InvoiceLine.PreviousDocuments.AddNew();
		document2.CSI_ReferenceNumber = "Invoice Line document";
		document2.CSI_Code = PreviousDocumentCodes._337;
		AssertEquals("Previous Documents deduplicated, and only one document kept", 1, GetProvider().PreviousDocuments.Count);

		document2.CSI_ReferenceNumber = "Invoice Line document2";
		AssertEquals("Previous Documents are different", 2, GetProvider().PreviousDocuments.Count);

		var document3 = InvoiceLine.PreviousDocuments.AddNew();
		document3.CSI_Code = PreviousDocumentCodes._337;
		document3.CSI_ReferenceNumber = "Invoice Line document3";
		document2.CSI_ReferenceNumber = "Invoice Line document";
		AssertEquals("Previous Documents deduplicated, and only document1 and document3 kept", 2, GetProvider().PreviousDocuments.Count);
	});

	public void TestPreviousDocumentsEuTypesBeforeNational() =>
		AesRuleTestHelper.TestItemsAreOrderedByRuleR0093E(
			AesRuleHelper.RuleR0093E.Patterns.n1an3,
			new[] { Invoice.PreviousDocuments.AddNew(), InvoiceLine.PreviousDocuments.AddNew() },
			(doc, value) => doc.CSI_Code = value,
			() => GetProvider().PreviousDocuments.Select(x => x.Type));

	public virtual void TestPreviousDocumentSpecialProcedures() => CombineAssertions(() =>
	{
		AssertEquals("No Previous Documents", 0, GetProvider().PreviousDocumentSpecialProcedures.Count);

		var doc1 = EntryInstruction.PreviousDocuments.AddNew();
		var doc2 = Invoice.PreviousDocuments.AddNew();
		var doc3 = InvoiceLine.PreviousDocuments.AddNew();

		doc1.CSI_Code = string.Empty;
		doc2.CSI_Code = string.Empty;
		doc3.CSI_Code = string.Empty;
		AssertEquals("No Previous Documents due to CSI_Code", 0, GetProvider().PreviousDocumentSpecialProcedures.Count);

		doc1.CSI_Code = PreviousDocumentCodes.SDE;
		doc2.CSI_Code = PreviousDocumentCodes.CLE;
		doc3.CSI_Code = PreviousDocumentCodes.MRN;
		AssertEquals("3 Previous Documents", 3, GetProvider().PreviousDocumentSpecialProcedures.Count);

		doc2.CSI_Code = string.Empty;
		AssertEquals("2 Previous Documents", 2, GetProvider().PreviousDocumentSpecialProcedures.Count);
	});

	public void TestSupportingDocuments() => CombineAssertions("Supporting documents should be added from Invoice Header and Invoice Line. " +
		"Documents from Entry Instruction should be added only in transition period.", () =>
	{
		AssertEquals("No Supporting Documents", 0, GetProvider().SupportingDocuments.Count);

		Invoice.SupportingDocuments.AddNew().CSI_ReferenceNumber = "Invoice Header document";
		InvoiceLine.SupportingDocuments.AddNew().CSI_ReferenceNumber = "Invoice Line document";

		using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			AssertEquals("UCC6: 2 Supporting Documents", 2, GetProvider().SupportingDocuments.Count);

			EntryInstruction.SupportingDocuments.AddNew().CSI_ReferenceNumber = "Entry Instruction document";
			AssertEquals("UCC6: 2 Supporting Documents (document from Entry Instruction has not been included)", 2, GetProvider().SupportingDocuments.Count);
		}

		using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			AssertEquals("Transition period: 3 Supporting Documents", 3, GetProvider().SupportingDocuments.Count);
		}
	});

	public void TestDeduplicationSupportingDocuments() => CombineAssertions(() =>
	{
		var document1 = InvoiceLine.SupportingDocuments.AddNew();
		document1.CSI_ReferenceNumber = "Invoice Line document";
		var document2 = InvoiceLine.SupportingDocuments.AddNew();
		document2.CSI_ReferenceNumber = "Invoice Line document";
		AssertEquals("Supporting Documents deduplicated, and only one document kept", 1, GetProvider().SupportingDocuments.Count);

		document2.CSI_ReferenceNumber = "Invoice Line document2";
		AssertEquals("Supporting Documents are different", 2, GetProvider().SupportingDocuments.Count);

		var document3 = InvoiceLine.SupportingDocuments.AddNew();
		document3.CSI_ReferenceNumber = "Invoice Line document3";
		document2.CSI_ReferenceNumber = "Invoice Line document";
		AssertEquals("Supporting Documents deduplicated, and only document1 and document3 kept", 2, GetProvider().SupportingDocuments.Count);
	});

	public void TestSupportingDocumentsAmountMerge()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();

		var invoiceLine = AddInvoiceLine(instruction, invoice);
		AddSupportingDocument(invoiceLine, "doc1", "doc1Reference", amount: 10.01m);
		AddSupportingDocument(invoiceLine, "doc2", "doc2Reference", amount: 5.55m);

		var invoiceLine2 = AddInvoiceLine(instruction, invoice);
		AddSupportingDocument(invoiceLine2, "doc2", "doc2Reference", amount: 2.22m);
		AddSupportingDocument(invoiceLine2, "doc1", "doc1Reference", amount: 50.05m);

		new LineMerger(declaration).DoMerge();
		var mergedEntryHeader = declaration.CustomsEntryHeaders.Single();
		var mergedEntryLine = mergedEntryHeader.AllEntryLines.Cast<CusEntryLine>().Single();
		var provider = new AESGoodsItemProvider(mergedEntryLine, 999, new AESGoodsShipmentProvider(mergedEntryHeader));
		var docs = provider.SupportingDocuments;

		CombineAssertions(() =>
		{
			var documentUnderTest = docs.Single(x => x.IssuingAuthorityName == "doc1");
			AssertEquals("Merged amount - doc1:", 60.06m, documentUnderTest.AmountValue);
			documentUnderTest = docs.Single(x => x.IssuingAuthorityName == "doc2");
			AssertEquals("Merged amount - doc2:", 7.77m, documentUnderTest.AmountValue);
		});
	}

	public void TestSupportingDocumentsQuantityMerge()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();

		var invoiceLine = AddInvoiceLine(instruction, invoice);
		AddSupportingDocument(invoiceLine, "doc1", "doc1Reference", quantity: 2.2m);
		AddSupportingDocument(invoiceLine, "doc2", "doc2Reference", quantity: 1m);

		var invoiceLine2 = AddInvoiceLine(instruction, invoice);
		AddSupportingDocument(invoiceLine2, "doc2", "doc2Reference", quantity: 8m);
		AddSupportingDocument(invoiceLine2, "doc1", "doc1Reference", quantity: 5.5m);

		new LineMerger(declaration).DoMerge();
		var mergedEntryHeader = declaration.CustomsEntryHeaders.Single();
		var mergedEntryLine = mergedEntryHeader.AllEntryLines.Cast<CusEntryLine>().Single();
		var provider = new AESGoodsItemProvider(mergedEntryLine, 999, new AESGoodsShipmentProvider(mergedEntryHeader));
		var docs = provider.SupportingDocuments;
		CombineAssertions(() =>
		{
			var documentUnderTest = docs.Single(x => x.IssuingAuthorityName == "doc1");
			AssertEquals("Merged quantity - doc1:", 7.7m, documentUnderTest.QuantityValue);
			documentUnderTest = docs.Single(x => x.IssuingAuthorityName == "doc2");
			AssertEquals("Merged quantity - doc2:", 9m, documentUnderTest.QuantityValue);
		});
	}

	public void TestSupportingDocumentsEuTypesBeforeNational() =>
		AesRuleTestHelper.TestItemsAreOrderedByRuleR0093E(
			AesRuleHelper.RuleR0093E.Patterns.n1an3,
			new[] { Invoice.SupportingDocuments.AddNew(), InvoiceLine.SupportingDocuments.AddNew() },
			(doc, value) => doc.CSI_Code = value,
			() => GetProvider().SupportingDocuments.Select(x => x.Type));

	public virtual void TestTransportDocuments() => CombineAssertions(() =>
	{
		var documentInvoiceLine = Invoice.AdditionalInfos.AddNew();
		documentInvoiceLine.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
		var documentInvoiceHeader = InvoiceLine.AdditionalInfos.AddNew();
		documentInvoiceHeader.CSI_SubType = AdditionalInfoKindList.Codes.TRA;
		var documentEntryInstruction = EntryInstruction.AdditionalInfos.AddNew();
		documentEntryInstruction.CSI_SubType = AdditionalInfoKindList.Codes.TRA;

		using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			AssertEquals("UCC6: 0 transport documents - no documents from any levels", 0, GetProvider().TransportDocuments.Count);
		}

		using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			AssertEquals("not UCC6: 1 transport documents - duplicates should be excluded", 1, GetProvider().TransportDocuments.Count);

			documentInvoiceLine.CSI_ReferenceNumber = "ref 0";
			documentInvoiceHeader.CSI_ReferenceNumber = "ref 1";
			documentEntryInstruction.CSI_ReferenceNumber = "ref 2";
			AssertEquals("not UCC6: 2 transport documents - invoice header, invoice line, entry instruction", 3, GetProvider().TransportDocuments.Count);

			AssertEquals("Value from CSI_ReferenceNumber is used as description.", documentInvoiceLine.CSI_ReferenceNumber, GetProvider().TransportDocuments.First().Description);

			Invoice.AdditionalInfos.AddNew().CSI_SubType = AdditionalInfoKindList.Codes.INF;
			InvoiceLine.AdditionalInfos.AddNew().CSI_SubType = AdditionalInfoKindList.Codes.REF;
			AssertEquals("not UCC6: 2 transport documents - documents with other subtypes shouldn't be added", 3, GetProvider().TransportDocuments.Count);
		}
	});

	public virtual void TestAdditionalReferences() => AssertAdditionalInfoForSubType(AdditionalInfoKindList.Codes.REF,
		() => GetProvider().AdditionalReferences,
		codesToTest: new ZString[] { Constants.AdditionalInfoCodes._4PL03 });

	public void TestAdditionalInformations() => AssertAdditionalInfoForSubType(AdditionalInfoKindList.Codes.INF,
		() => GetProvider().AdditionalInformations,
		codesToTest: new ZString[] { Constants.AdditionalInfoCodes._4PL04 },
		excludedCodes: new ZString[] { Constants.AdditionalInfoCodes._4PL03 });

	public void TestAdditionalInformationEuCodesBeforeNational()
	{
		var doc1 = Invoice.AdditionalInfos.AddNew();
		var doc2 = InvoiceLine.AdditionalInfos.AddNew();
		doc1.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		doc2.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		AesRuleTestHelper.TestItemsAreOrderedByRuleR0093E(
			AesRuleHelper.RuleR0093E.Patterns.a1an4,
			new[] { doc1, doc2 },
			(doc, value) => doc.CSI_Code = value,
			() => GetProvider().AdditionalInformations.Select(x => x.Type));
	}

	void AssertAdditionalInfoForSubType(string subType,
		Func<IReadOnlyCollection<IDocument>> documentProvider,
		ZString[] codesToTest,
		ZString[] excludedCodes = null) => CombineAssertions(() =>
	{
		AssertEquals("No AdditionalInfo Documents", 0, documentProvider.Invoke().Count);

		var csiCode = "DFLT";
		var doc1 = EntryInstruction.AdditionalInfos.AddNew();
		doc1.CSI_SubType = subType;
		doc1.CSI_Code = csiCode;
		doc1.CSI_Description = "Description";
		doc1.CSI_ReferenceNumber = "Reference";

		var doc2 = Invoice.AdditionalInfos.AddNew();
		var doc3 = InvoiceLine.AdditionalInfos.AddNew();

		doc2.CSI_SubType = subType;
		doc2.CSI_Code = csiCode;
		doc2.CSI_Description = "Description2";
		doc2.CSI_ReferenceNumber = "Reference2";

		doc3.CSI_SubType = subType;
		doc3.CSI_Code = csiCode;
		doc3.CSI_Description = "Description3";
		doc3.CSI_ReferenceNumber = "Reference3";

		var duplicateDoc = Invoice.AdditionalInfos.AddNew();
		duplicateDoc.CSI_SubType = subType;
		duplicateDoc.CSI_Code = csiCode;
		duplicateDoc.CSI_ReferenceNumber = "Reference3";
		duplicateDoc.CSI_Description = "Description3";

		if (codesToTest != null)
		{
			foreach (var code in codesToTest)
			{
				var additionalInfo = InvoiceLine.AdditionalInfos.AddNew();
				additionalInfo.CSI_Code = code;
				additionalInfo.CSI_SubType = subType;
			}
		}

		if (excludedCodes != null)
		{
			foreach (var excludedCode in excludedCodes)
			{
				var excludedDoc = InvoiceLine.AdditionalInfos.AddNew();
				excludedDoc.CSI_Code = excludedCode;
				excludedDoc.CSI_SubType = subType;
			}
		}

		using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			AssertEquals("UCC6: 2 Documents", 2 + codesToTest?.Length ?? 0, documentProvider.Invoke().Count);
		}

		using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			var description = subType == AdditionalInfoKindList.Codes.INF ? "Description" : "Reference";
			AssertEquals("Description mapping", description, documentProvider.Invoke().Last().Description);

			AssertEquals("In transition period: 3 Documents", 3 + codesToTest?.Length ?? 0, documentProvider.Invoke().Count);

			if (subType == AdditionalInfoKindList.Codes.INF)
			{
				EntryInstruction.CEI_SubStyle = "Z";
				AssertEquals("In transition period (entry instruction substyle is Z): 2 Documents", 2 + codesToTest?.Length ?? 0, documentProvider.Invoke().Count);
			}
		}
	});

	public void TestTransportChargesMethodOfPayment()
	{
		var invoice2 = Declaration.Invoices.AddNew();
		invoice2.JZ_InvoiceNumber = "234";
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = EntryInstruction.PK;
		invoiceLine2.JI_Description = "invoice line 2";

		CombineAssertions(() =>
		{
			Invoice.ZG_TransportChargesMethodOfPayment = ExportTransportMethodOfPaymentList.Codes.A;
			lineMerger.DoMerge();
			EntryHeader = Declaration.CustomsEntryHeaders.First();
			EntryLine = EntryHeader.AllEntryLines.FirstOrDefault();
			AssertEquals("1 Not Empty and 1 empty TransportChargesMethodOfPayment", ExportTransportMethodOfPaymentList.Codes.A, GetProvider().TransportChargesMethodOfPayment);

			invoice2.ZG_TransportChargesMethodOfPayment = ExportTransportMethodOfPaymentList.Codes.A;
			lineMerger.DoMerge();
			EntryHeader = Declaration.CustomsEntryHeaders.First();
			EntryLine = EntryHeader.AllEntryLines.FirstOrDefault();
			AssertNull("2 Same TransportChargesMethodOfPayment", GetProvider().TransportChargesMethodOfPayment);

			invoice2.ZG_TransportChargesMethodOfPayment = ExportTransportMethodOfPaymentList.Codes.B;
			Invoice.ZG_TransportChargesMethodOfPayment = ExportTransportMethodOfPaymentList.Codes.C;
			lineMerger.DoMerge();
			EntryHeader = Declaration.CustomsEntryHeaders.First();
			EntryLine = EntryHeader.AllEntryLines.FirstOrDefault();
			AssertEquals("2 different TransportChargesMethodOfPayment", ExportTransportMethodOfPaymentList.Codes.C, GetProvider().TransportChargesMethodOfPayment);
		});
	}

	protected override AESGoodsItemProvider GetProvider() => new AESGoodsItemProvider(EntryLine, 999, new AESGoodsShipmentProvider(EntryHeader));

	protected override void SetUp()
	{
		base.SetUp();
		Declaration = Factory.New<JobDeclaration>();
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

		EntryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		Invoice = Declaration.Invoices.AddNew();
		Invoice.JZ_InvoiceNumber = "123";
		InvoiceLine = Invoice.InvoiceLines.AddNew();
		InvoiceLine.JI_CEI = EntryInstruction.PK;
		InvoiceLine.JI_Description = "invoice line 1";

		lineMerger = new LineMerger(Declaration);
		lineMerger.DoMerge();
		EntryHeader = Declaration.CustomsEntryHeaders.First();
		EntryLine = EntryHeader.AllEntryLines.FirstOrDefault();
	}

	LineMerger lineMerger;
	protected JobDeclaration Declaration { get; private set; }
	protected CusEntryHeader EntryHeader { get; private set; }
	protected CusEntryLine EntryLine { get; private set; }
	protected JobComInvoiceLine InvoiceLine { get; private set; }
	protected JobComInvoiceHeader Invoice { get; private set; }
	protected CusEntryInstruction EntryInstruction { get; private set; }

	static JobComInvoiceLine AddInvoiceLine(CusEntryInstruction instruction, JobComInvoiceHeader invoice, string description = "invoice line")
	{
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		invoiceLine.JI_Description = description;
		return invoiceLine;
	}

	static void AddSupportingDocument(JobComInvoiceLine invoiceLine, string description, string referenceNumber, decimal quantity = 0m, decimal amount = 0m)
	{
		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument.CSI_AdditionalDescription = description;
		supportingDocument.CSI_Quantity = quantity;
		supportingDocument.CSI_Value = amount;
		supportingDocument.CSI_ReferenceNumber = referenceNumber;
	}
}
