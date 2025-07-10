using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

class AESGoodsShipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<AESGoodsShipmentProvider>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		_ = AssertExceptionThrown<ArgumentNullException>("Null EntryHeader", "Value cannot be null.\r\nParameter name: entryHeader",
			() => new AESGoodsShipmentProvider(null));
		_ = AssertExceptionThrown<ArgumentNullException>("Null Declaration", "Value cannot be null.\r\nParameter name: entryHeader.Declaration",
			() => new AESGoodsShipmentProvider(Factory.New<CusEntryHeader>()));
		_ = AssertExceptionThrown<ArgumentNullException>("Null EntryInstruction", "Value cannot be null.\r\nParameter name: entryHeader.EntryInstruction",
			() => new AESGoodsShipmentProvider(Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew()));
	});

	public virtual void TestNatureOfTransaction() => CombineAssertions(() =>
	{
		using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
		{
			AssertionNatureOfTransaction("Inside TP");
		}

		using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
		{
			AssertionNatureOfTransaction("Outside TP");
		}

		void AssertionNatureOfTransaction(string prefix)
		{
			Declaration.JE_GoodsOrigin = ZString.Empty;
			AssertEquals(prefix + " Empty JE_GoodsOrigin", string.Empty, GetProvider().NatureOfTransaction);

			invoice.JZ_ValuationCode = "A";
			EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
			AssertNull(prefix + " Single not empty JZ_ValuationCode with invalid CEI_SubStyle", GetProvider().NatureOfTransaction);

			EntryInstruction.CEI_SubStyle = "";
			invoice2.JZ_ValuationCode = "B";
			AssertNull(prefix + " Different JZ_ValuationCode", GetProvider().NatureOfTransaction);

			invoice2.JZ_ValuationCode = "A";
			AssertEquals(prefix + " Multiple same JZ_ValuationCode", "A", GetProvider().NatureOfTransaction);

			EntryInstruction.CEI_SubStyle = "";
			invoice.JZ_ValuationCode = "";
			invoice2.JZ_ValuationCode = "";
		}
	});

	public void TestCountryOfExport() => CombineAssertions(() =>
	{
		Declaration.JE_GoodsOrigin = ZString.Empty;
		AssertEquals("Empty JE_GoodsOrigin", string.Empty, GetProvider().CountryOfExport);

		Declaration.JE_GoodsOrigin = CountryCodes.Germany;
		AssertEquals("Not empty JE_GoodsOrigin", CountryCodes.Germany, GetProvider().CountryOfExport);
	});

	public void TestCountryOfDestination() => CombineAssertions(() =>
	{
		Declaration.JE_GoodsDestination = ZString.Empty;
		AssertNull("JE_GoodsDestination is empty", GetProvider().CountryOfDestination);

		Declaration.JE_GoodsDestination = CountryCodes.Germany;
		AssertEquals("JE_GoodsDestination is Germany", CountryCodes.Germany, GetProvider().CountryOfDestination);

		invoiceLine.ZG_CountryOfDestination = CountryCodes.Germany;
		AssertEquals("JE_GoodsDestination and ZG_CountryOfDestination is Germany", CountryCodes.Germany, GetProvider().CountryOfDestination);

		invoiceLine.ZG_CountryOfDestination = CountryCodes.Poland;
		AssertNull("JE_GoodsDestination and ZG_CountryOfDestination are different", GetProvider().CountryOfDestination);

		Declaration.JE_GoodsDestination = ZString.Empty;
		AssertEquals("JE_GoodsDestination is empty and exists single ZG_CountryOfDestination", CountryCodes.Poland, GetProvider().CountryOfDestination);

		invoiceLine2.ZG_CountryOfDestination = CountryCodes.Germany;
		AssertNull("Different ZG_CountryOfDestination", GetProvider().CountryOfDestination);

		Declaration.JE_GoodsDestination = CountryCodes.Germany;
		invoiceLine2.ZG_CountryOfDestination = CountryCodes.Poland;
		AssertEquals("All same ZG_CountryOfDestination with different JE_GoodsDestination", CountryCodes.Germany, GetProvider().CountryOfDestination);

		Declaration.JE_GoodsDestination = ZString.Empty;
		AssertEquals("All same ZG_CountryOfDestination with empty JE_GoodsDestination", CountryCodes.Poland, GetProvider().CountryOfDestination);
	});

	public void TestAdditionalSupplyChainActors() => CombineAssertions(() =>
	{
		AssertEquals("Empty CusSupplyChainActorReferences", 0, GetProvider().AdditionalSupplyChainActors.Count);

		var cusSupplyChainActorReferences1 = EntryInstruction.CusSupplyChainActorReferences.AddNew();
		var cusSupplyChainActorReferences2 = EntryInstruction.CusSupplyChainActorReferences.AddNew();
		AssertEquals("2 Should exist", 2, GetProvider().AdditionalSupplyChainActors.Count);
		TestHelper.TestCollectionHasCorrectIndexOrder(GetProvider().AdditionalSupplyChainActors, (item) => item.SequenceNumber);
	});

	public virtual void TestDeliveryTerms() => CombineAssertions(() =>
	{
		EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
		AssertNull("Null because of CEI_SubStyle", GetProvider().DeliveryTerms);

		EntryInstruction.CEI_SubStyle = ZString.Empty;
		AssertNotNull("Not null", GetProvider().DeliveryTerms);
	});

	public void TestWarehouses()
	{
		invoiceLine.PreviousProcedureCode = Constants.ProcedureCodes._71;
		CombineAssertions(() =>
		{
			AssertNull("Empty warehouse address", GetProvider().Warehouses);

			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			EntryInstruction.CEI_OA_Warehouse = address.PK;
			AssertNull("Empty cuscodes for address", GetProvider().Warehouses);

			_ = address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "123", CountryCodes.Denmark);
			AssertNull("No PL cuscodes for address", GetProvider().Warehouses);

			_ = address.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "123", CountryCodes.Poland);
			AssertNotNull("PL cuscodes for address exists and procedure 71/76/77 exists", GetProvider().Warehouses);

			EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.IncompleteDeclaration;
			AssertNull("Null because of CEI_SubStyle", GetProvider().Warehouses);

			invoiceLine.PreviousProcedureCode = Constants.ProcedureCodes._42;
			EntryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			AssertNull("Null because of PreviousProcedureCode", GetProvider().Warehouses);
		});
	}

	public void TestPreviousDocuments()
	{
		invoice.PreviousDocuments.AddNew().CSI_Description = "Invoice Header document";
		invoiceLine.PreviousDocuments.AddNew().CSI_Description = "Invoice Line document";

		CombineAssertions("Only documents from entry instruction level can be added, " +
						"and only if declaration is UCC6 (after transition period)", () =>
		{
			using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
			{
				AssertEquals("UCC6: 0 previous documents", 0, GetProvider().PreviousDocuments.Count);

				EntryInstruction.PreviousDocuments.AddNew().CSI_Description = "Entry Instruction document";
				AssertEquals("UCC6: 1 previous document", 1, GetProvider().PreviousDocuments.Count);

				EntryInstruction.PreviousDocuments.AddNew().CSI_Description = "Entry Instruction document 2";
			}

			using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
			{
				AssertEquals("In transition period: 0 previous documents", 0, GetProvider().PreviousDocuments.Count);
			}
		});
	}

	public void TestPreviousDocumentsFromEntryInstruction() => CombineAssertions(
		"All previous documents from entry instruction should be added to GoodsShipment if declaration is UCC6 (after transition period). " +
		"In transition period the previous documents should be added only if EntryInstruction.CEI_SubStyle is one of [X, Y, Z].", () =>
		{
			AssertEquals("No Previous Documents", 0, GetProvider().PreviousDocuments.Count);
			EntryInstruction.PreviousDocuments.AddNew();

			using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
			{
				AssertEquals("Is UCC6 and sub-style is not in [X, Y, Z]", 1, GetProvider().PreviousDocuments.Count);

				EntryInstruction.CEI_SubStyle = "X";
				AssertEquals("Is UCC6 and sub-style is X", 1, GetProvider().PreviousDocuments.Count);

				EntryInstruction.CEI_SubStyle = "Y";
				AssertEquals("Is UCC6 and sub-style is Y", 1, GetProvider().PreviousDocuments.Count);

				EntryInstruction.CEI_SubStyle = "Z";
				AssertEquals("Is UCC6 and sub-style is Z", 1, GetProvider().PreviousDocuments.Count);
			}

			using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
			{
				EntryInstruction.CEI_SubStyle = "0";
				AssertEquals("Is in transition period and sub-style is not in [X, Y, Z]", 0, GetProvider().PreviousDocuments.Count);

				EntryInstruction.CEI_SubStyle = "X";
				AssertEquals("Is in transition period and sub-style is X", 1, GetProvider().PreviousDocuments.Count);

				EntryInstruction.CEI_SubStyle = "Y";
				AssertEquals("Is in transition period and sub-style is Y", 1, GetProvider().PreviousDocuments.Count);

				EntryInstruction.CEI_SubStyle = "Z";
				AssertEquals("Is in transition period and sub-style is Z", 1, GetProvider().PreviousDocuments.Count);
			}
		});

	public void TestPreviousDocumentsEuTypesBeforeNational() =>
		AesRuleTestHelper.TestItemsAreOrderedByRuleR0093E(
			AesRuleHelper.RuleR0093E.Patterns.n1an3,
			new[] { EntryInstruction.PreviousDocuments.AddNew(), EntryInstruction.PreviousDocuments.AddNew() },
			(doc, value) => doc.CSI_Code = value,
			() => GetProvider().PreviousDocuments.Select(x => x.Type));

	public void TestSupportingDocuments()
	{
		invoice.SupportingDocuments.AddNew().CSI_Description = "Invoice Header document";
		invoiceLine.SupportingDocuments.AddNew().CSI_Description = "Invoice Line document";

		CombineAssertions("Only documents from entry instruction level can be added, " +
						"and only if declaration is UCC6 (after transition period)", () =>
		{
			using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
			{
				AssertEquals("UCC6: 0 supporting documents", 0, GetProvider().SupportingDocuments.Count);

				EntryInstruction.SupportingDocuments.AddNew().CSI_Description = "Entry Instruction document";
				AssertEquals("UCC6: 1 supporting document", 1, GetProvider().SupportingDocuments.Count);

				EntryInstruction.SupportingDocuments.AddNew().CSI_Description = "Entry Instruction document 2";
			}

			using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
			{
				AssertEquals("In Transition period: 0 supporting documents", 0, GetProvider().SupportingDocuments.Count);
			}
		});
	}

	public void TestSupportingDocumentsEuTypesBeforeNational() =>
		AesRuleTestHelper.TestItemsAreOrderedByRuleR0093E(
			AesRuleHelper.RuleR0093E.Patterns.n1an3,
			new[] { EntryInstruction.SupportingDocuments.AddNew(), EntryInstruction.SupportingDocuments.AddNew() },
			(doc, value) => doc.CSI_Code = value,
			() => GetProvider().SupportingDocuments.Select(x => x.Type));

	public virtual void TestAdditionalReferences()
	{
		AddAdditionalReference(invoice.AdditionalInfos, "Invoice Header document");
		AddAdditionalReference(invoiceLine.AdditionalInfos, "Invoice Line document");

		CombineAssertions("Only documents from entry instruction level can be added, " +
						"and only if declaration is UCC6 (after transition period)", () =>
		{
			using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
			{
				AssertEquals("UCC6: 0 documents", 0, GetProvider().AdditionalReferences.Count);

				AddAdditionalReference(EntryInstruction.AdditionalInfos, "Entry Instruction document");
				AssertEquals("UCC6: 1 documents", 1, GetProvider().AdditionalReferences.Count);

				AddAdditionalReference(EntryInstruction.AdditionalInfos, "Entry Instruction document 2");
			}

			using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
			{
				AssertEquals("In transition period: 0 documents", 0, GetProvider().AdditionalReferences.Count);
			}
		});

		void AddAdditionalReference(AdditionalInfoCollection collection, ZString description)
		{
			var result = collection.AddNew();
			result.CSI_SubType = AdditionalInfoKindList.Codes.REF;
			result.CSI_Description = description;
		}
	}

	public void TestAdditionalInformation()
	{
		var invoiceDocument = invoice.AdditionalInfos.AddNew();
		invoiceDocument.CSI_Description = "invoice document";
		invoiceDocument.CSI_SubType = AdditionalInfoKindList.Codes.INF;

		var invoiceLineDocument = invoiceLine.AdditionalInfos.AddNew();
		invoiceLineDocument.CSI_Description = "invoice line document";
		invoiceLineDocument.CSI_SubType = AdditionalInfoKindList.Codes.INF;

		var entryInstructionDocument = EntryInstruction.AdditionalInfos.AddNew();
		entryInstructionDocument.CSI_Description = "Entry Instruction document";
		entryInstructionDocument.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		entryInstructionDocument.CSI_Code = Constants.AdditionalInfoCodes._4PL04;

		entryInstructionDocument = EntryInstruction.AdditionalInfos.AddNew();
		entryInstructionDocument.CSI_Description = "Entry Instruction document duplicate not 4PL03";
		entryInstructionDocument.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		entryInstructionDocument.CSI_Code = Constants.AdditionalInfoCodes._4PL04;

		CombineAssertions("Only AdditionalInfos from entryInstruction level can be added to goods shipment provider." +
						"And only one (the first) occurence of AdditionalInfos with CSI_Code = 4PL03 can be added." +
						"In transition period AdditionalInfos can be added only if CEI_Substyle is [Z].", () =>
		{
			using (Declaration.TemporarilySetIsAESTransitionPeriod(false))
			{
				AssertEquals("2 document - UCC6", 2, GetProvider().AdditionalInformation.Count);
				var document = EntryInstruction.AdditionalInfos.AddNew();
				document.CSI_Description = "Entry Instruction document 2";
				document.CSI_SubType = AdditionalInfoKindList.Codes.INF;
				AssertEquals("3 documents - UCC6", 3, GetProvider().AdditionalInformation.Count);

				entryInstructionDocument = EntryInstruction.AdditionalInfos.AddNew();
				entryInstructionDocument.CSI_Description = "Entry Instruction document";
				entryInstructionDocument.CSI_SubType = AdditionalInfoKindList.Codes.INF;
				entryInstructionDocument.CSI_Code = Constants.AdditionalInfoCodes._4PL03;

				entryInstructionDocument = EntryInstruction.AdditionalInfos.AddNew();
				entryInstructionDocument.CSI_Description = "Entry Instruction document 4PL03 Duplicate";
				entryInstructionDocument.CSI_SubType = AdditionalInfoKindList.Codes.INF;
				entryInstructionDocument.CSI_Code = Constants.AdditionalInfoCodes._4PL03;

				var additionalInformation = GetProvider().AdditionalInformation;
				AssertEquals("4 documents - R359", 4, additionalInformation.Count);
				AssertEquals("1 document - R359 - unique 4PL03", 1, additionalInformation.Count(x => x.Type == Constants.AdditionalInfoCodes._4PL03));
			}

			using (Declaration.TemporarilySetIsAESTransitionPeriod(true))
			{
				AssertEquals("0 documents - not UCC6", 0, GetProvider().AdditionalInformation.Count);
				EntryInstruction.CEI_SubStyle = "Z";
				AssertEquals("4 documents (substyle Z) - not UCC6", 4, GetProvider().AdditionalInformation.Count);
			}
		});
	}

	public void TestAdditionalInformationEuCodesBeforeNational()
	{
		var doc1 = EntryInstruction.AdditionalInfos.AddNew();
		var doc2 = EntryInstruction.AdditionalInfos.AddNew();
		doc1.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		doc2.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		AesRuleTestHelper.TestItemsAreOrderedByRuleR0093E(
			AesRuleHelper.RuleR0093E.Patterns.a1an4,
			new[] { doc1, doc2 },
			(doc, value) => doc.CSI_Code = value,
			() => GetProvider().AdditionalInformation.Select(x => x.Type));
	}

	public void TestAESConsignment() => AssertNotNull(GetProvider().AESConsignment);

	public void TestGoodsItems() => CombineAssertions(() =>
	{
		AssertEquals("2 entry line - invoice 1 and invoice 2", 2, GetProvider().GoodsItems.Count);
		TestHelper.TestCollectionHasCorrectIndexOrder(GetProvider().GoodsItems, (item) => item.DeclarationGoodsItemNumber);

		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = EntryInstruction.PK;
		invoiceLine2.JI_Description = "invoice line 2";
		var lineMerger = new LineMerger(Declaration);
		lineMerger.DoMerge();
		AssertEquals("3 entry lines - invoice 1 with 2 lines and invoice 2 with 1 line", 3, GetProvider().GoodsItems.Count);
	});

	protected override AESGoodsShipmentProvider GetProvider() => new AESGoodsShipmentProvider(EntryHeader);

	protected override void SetUp()
	{
		base.SetUp();
		Declaration = Factory.New<JobDeclaration>();
		Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		EntryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		invoice = Declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = EntryInstruction.PK;
		invoiceLine.JI_Description = "invoice line 1";
		invoice2 = Declaration.Invoices.AddNew();
		invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CEI = EntryInstruction.PK;

		var lineMerger = new LineMerger(Declaration);
		lineMerger.DoMerge();
		EntryHeader = Declaration.CustomsEntryHeaders.FirstOrDefault();
	}

	protected JobDeclaration Declaration { get; set; }
	protected CusEntryHeader EntryHeader { get; set; }
	JobComInvoiceLine invoiceLine;
	JobComInvoiceLine invoiceLine2;
	JobComInvoiceHeader invoice;
	JobComInvoiceHeader invoice2;
	protected CusEntryInstruction EntryInstruction { get; set; }
}
