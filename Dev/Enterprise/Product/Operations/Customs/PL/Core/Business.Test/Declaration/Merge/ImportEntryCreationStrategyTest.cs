using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportEntryCreationStrategyTest : EntryCreationStrategyTest
{
	public void TestCreateEntryCreationStrategy()
	{
		var (dec, _) = PrepareTestJobData(Factory, 0);
		var strategy = dec.CreateEntryCreationStrategy();
		Assert(strategy is ImportEntryCreationStrategy);
	}

	public void TestGetKeyForLine()
	{
		SetUpRefData();
		var (declaration, invLines) = PrepareTestJobData(Factory, 1);
		var invoiceLine = invLines[0];
		PopulateDeclarationDataForGetKeyForLineFromPL(invoiceLine);

		CombineAssertions("When JE_MergeBy = empty", () =>
		{
			declaration.JE_MergeBy = ZString.Empty;
			AssertKeyForLine(invoiceLine, invoiceLine.PK);
		});

		CombineAssertions("When JE_MergeBy = NON", () =>
		{
			declaration.JE_MergeBy = "NON";
			AssertKeyForLine(invoiceLine, invoiceLine.PK);
		});

		CombineAssertions("When JE_MergeBy = NOP", () =>
		{
			declaration.JE_MergeBy = "NOP";
			AssertKeyForLine(invoiceLine, invoiceLine.PK);
		});

		CombineAssertions("When JE_MergeBy = TRF", () =>
		{
			declaration.JE_MergeBy = "TRF";
			AssertKeyForLine(invoiceLine, invoiceLine.JI_Tariff);
		});

		CombineAssertions("When JE_MergeBy = TRM", () =>
		{
			declaration.JE_MergeBy = "TRM";
			AssertKeyForLine(invoiceLine, invoiceLine.JI_Tariff);
		});
	}

	public void TestBranNameAffectingMerge()
	{
		var (dec, invLines) = PrepareTestJobData(Factory, 4);
		var strategy = dec.CreateEntryCreationStrategy();

		CombineAssertions("BaseLine: all lines' keys are the same without Brand", () =>
		{
			var keyForLine1 = strategy.GetKeyForLine(invLines[0]);
			AssertEquals(keyForLine1, strategy.GetKeyForLine(invLines[1]));
			AssertEquals(keyForLine1, strategy.GetKeyForLine(invLines[2]));
			AssertEquals(keyForLine1, strategy.GetKeyForLine(invLines[3]));
		});

		invLines[0].JI_BrandName = ZString.Empty;
		invLines[1].JI_BrandName = ZString.Empty;
		invLines[2].JI_BrandName = "Brand";
		invLines[3].JI_BrandName = "Brand";

		CombineAssertions("Line3/4 should be on their own even though the BrandName is the same", () =>
		{
			var keyForLine1 = strategy.GetKeyForLine(invLines[0]);
			var keyForLine3 = strategy.GetKeyForLine(invLines[2]);
			var keyForLine4 = strategy.GetKeyForLine(invLines[3]);
			AssertEquals(keyForLine1, strategy.GetKeyForLine(invLines[1]));
			AssertNotEquals(keyForLine3, keyForLine1);
			AssertNotEquals(keyForLine3, keyForLine4);
			AssertNotEquals(keyForLine4, keyForLine1);
			AssertNotEquals(keyForLine4, keyForLine3);
		});
	}

	public void TestPreviousDocumentsAffectingMergingByCSI_Code()
	{
		TestPreviousDocumentsAffectingMergingBy(nameof(PreviousDocument.CSI_Code), new ZString("C001"), new ZString("C002"));
	}

	public void TestPreviousDocumentsAffectingMergingByCSI_SubType()
	{
		TestPreviousDocumentsAffectingMergingBy(nameof(PreviousDocument.CSI_SubType), new ZString("A"), new ZString("B"));
	}

	public void TestPreviousDocumentsAffectingMergingByCSI_ReferenceNumber()
	{
		TestPreviousDocumentsAffectingMergingBy(nameof(PreviousDocument.CSI_ReferenceNumber), new ZString("R001"), new ZString("R002"));
	}

	public void TestPreviousDocumentsAffectingMergingByCSI_LineNo()
	{
		TestPreviousDocumentsAffectingMergingBy(nameof(PreviousDocument.CSI_LineNo), new ZInt(1), new ZInt(2));
	}

	public void TestSupportingDocumentsAffectingMerging()
	{
		var (dec, invLines) = PrepareTestJobData(Factory, 4);
		var strategy = dec.CreateEntryCreationStrategy();

		CombineAssertions("BaseLine: all lines' keys are the same without supportingDocs", () =>
		{
			var keyForLine1 = strategy.GetKeyForLine(invLines[0]);
			AssertEquals(keyForLine1, strategy.GetKeyForLine(invLines[1]));
			AssertEquals(keyForLine1, strategy.GetKeyForLine(invLines[2]));
			AssertEquals(keyForLine1, strategy.GetKeyForLine(invLines[3]));
		});

		AddSupportingDocumentForInvoiceLines(invLines[0], "C001", "R001", "OK", "KG", "PL", "R201", "DESCR1");
		AddSupportingDocumentForInvoiceLines(invLines[1], "C001", "R001", "OK", "KG", "PL", "R201", "DESCR1");
		AddSupportingDocumentForInvoiceLines(invLines[1], "C002", "R002", "OK", "KG", "PL", "R202", "DESCR2");
		AddSupportingDocumentForInvoiceLines(invLines[2], "C002", "R002", "OK", "KG", "PL", "R202", "DESCR2");
		AddSupportingDocumentForInvoiceLines(invLines[2], "C001", "R001", "OK", "KG", "PL", "R201", "DESCR1");
		AddSupportingDocumentForInvoiceLines(invLines[3], "C001", "R001", "OK", "KG", "PL", "R201", "DESCR1");
		AddSupportingDocumentForInvoiceLines(invLines[3], "C002", "R002", "OK", "kg", "PL", "R202", "DESCR2");

		CombineAssertions("Line2/3 should be merged together, while 1/4 should be on their own", () =>
		{
			var keyForLine1 = strategy.GetKeyForLine(invLines[0]);
			var keyForLine2 = strategy.GetKeyForLine(invLines[1]);
			var keyForLine3 = strategy.GetKeyForLine(invLines[2]);
			var keyForLine4 = strategy.GetKeyForLine(invLines[3]);
			AssertNotEquals("Line1 != Line2", keyForLine1, keyForLine2);
			AssertNotEquals("Line1 != Line3", keyForLine1, keyForLine3);
			AssertNotEquals("Line1 != Line4", keyForLine1, keyForLine4);
			AssertEquals("Line2 == Line3", keyForLine2, keyForLine3);
			AssertNotEquals("Line2 != Line4", keyForLine2, keyForLine4);
		});

		void AddSupportingDocumentForInvoiceLines(JobComInvoiceLine line, string code, string reference,
			string status, string uq, string countryCode, string reference2, string description)
		{
			var supDoc = line.SupportingDocuments.AddNew();
			supDoc.CSI_Code = code;
			supDoc.CSI_ReferenceNumber = reference;
			supDoc.CSI_Status = status;
			supDoc.CSI_UnitOfQuantity = uq;
			supDoc.CSI_RN_NKCountryCode = countryCode;
			supDoc.CSI_ReferenceNumber2 = reference2;
			supDoc.CSI_Description = description;
		}
	}

	public void TestSupportingDocumentsAffectingMergingByCSI_Code()
	{
		TestSupportingDocumentsAffectingMergingBy(nameof(SupportingDocument.CSI_Code), new ZString("C001"), new ZString("C002"));
	}

	public void TestSupportingDocumentsAffectingMergingByCSI_ReferenceNumber()
	{
		TestSupportingDocumentsAffectingMergingBy(nameof(SupportingDocument.CSI_ReferenceNumber), new ZString("R001"), new ZString("R002"));
	}

	public void TestSupportingDocumentsAffectingMergingByCSI_Status()
	{
		TestSupportingDocumentsAffectingMergingBy(nameof(SupportingDocument.CSI_Status), new ZString("Ok"), new ZString("ok"));
	}

	public void TestSupportingDocumentsAffectingMergingByCSI_UnitOfQuantity()
	{
		TestSupportingDocumentsAffectingMergingBy(nameof(SupportingDocument.CSI_UnitOfQuantity), new ZString("KG"), new ZString("TN"));
	}

	public void TestSupportingDocumentsAffectingMergingByCSI_RN_NKCountryCode()
	{
		TestSupportingDocumentsAffectingMergingBy(nameof(SupportingDocument.CSI_RN_NKCountryCode), new ZString("PL"), new ZString("DE"));
	}

	public void TestSupportingDocumentsAffectingMergingByCSI_ReferenceNumber2()
	{
		TestSupportingDocumentsAffectingMergingBy(nameof(SupportingDocument.CSI_ReferenceNumber2), new ZString("R201"), new ZString("R202"));
	}

	public void TestSupportingDocumentsAffectingMergingByCSI_Description()
	{
		TestSupportingDocumentsAffectingMergingBy(nameof(SupportingDocument.CSI_Description), new ZString("DESC1"), new ZString("DESC2"));
	}

	protected override string[] GetExpectedPreviousDocumentKeys() => new[]
	{
		PreviousDocument.Schema.CSI_Code,
		PreviousDocument.Schema.CSI_SubType,
		PreviousDocument.Schema.CSI_ReferenceNumber,
		PreviousDocument.Schema.CSI_LineNo,
	};

	protected override string[] GetExpectedSupportingDocumentKeys() => new[]
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_Status,
		SupportingDocument.Schema.CSI_UnitOfQuantity,
		SupportingDocument.Schema.CSI_RN_NKCountryCode,
		SupportingDocument.Schema.CSI_ReferenceNumber2,
		SupportingDocument.Schema.CSI_Description,
	};

	#region Preperation

	protected override string GetTestedMessageType => JobMessageTypeList.Codes.Import;

	protected override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var entry = declaration.CustomsEntryInstructions.AddNew();
		entry.CEI_DateForDuty = ZDateTime.Now.AddDays(2);

		return declaration;
	}

	void PopulateDeclarationDataForGetKeyForLineFromPL(JobComInvoiceLine invLine)
	{
		invLine.ZG_CountryOfSupply = "CN";
		invLine.JI_DateForDutyOverride = new ZDateTime(ZDateTime.Now.AddDays(-2));
		invLine.JI_ValuationDateOverride = new ZDateTime(ZDateTime.Now.AddDays(-3));
		invLine.JI_CustomsUnitQty = "T";
		invLine.JI_BondedWhsUnitQty = "1.00";
	}

	void SetUpRefData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
		var cusCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "RPTID", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		cusCode.Attributes.AddNew("Direction", "IMPORT");
		cusCode.Attributes.AddNew("Direction", "EXPORT");
		cusCode.Attributes.AddNew("Level", "HEADER");
	}

	#endregion

	#region Assert

	const int KeysFromGetKeyForHeaderPrefixCount = 40;

	void AssertKeyForLine(JobComInvoiceLine invoiceLine, params IZType[] valuesToInsertAtTheBeginning)
	{
		var entryCreationStrategy = invoiceLine.Declaration.CreateEntryCreationStrategy();
		var wholeActualKeyForLine = entryCreationStrategy.GetKeyForLine(invoiceLine).Keys;
		var actualKeyForLineWithoutHeaderPrefix = wholeActualKeyForLine.Skip(KeysFromGetKeyForHeaderPrefixCount).ToArray();
		actualKeyForLineWithoutHeaderPrefix[0] = valuesToInsertAtTheBeginning[0];
		var expectedKeysFromGetKeyForLine = GetExpectedKeyForGetKeyForLineWithoutHeaderPrefix(invoiceLine, valuesToInsertAtTheBeginning);
		AssertArrayEqualsByElements(expectedKeysFromGetKeyForLine, actualKeyForLineWithoutHeaderPrefix);
	}

	IZType[] GetExpectedKeyForGetKeyForLineWithoutHeaderPrefix(JobComInvoiceLine invoiceLine, params IZType[] valuesToInsertAtTheBeginning)
	{
		var expectedKeysFromGetKeyForLine = new List<IZType>(valuesToInsertAtTheBeginning);
		expectedKeysFromGetKeyForLine.AddRange(new IZType[]
		{
			invoiceLine.JI_CustomsUnitQty,
			invoiceLine.JI_CustomsSecondUnitQty,
			invoiceLine.JI_CustomsThirdUnitQty,
			invoiceLine.JI_CustomsFourthUnitQty,
			invoiceLine.JI_CustomsFifthUnitQty,
			invoiceLine.JI_BondedWhsUnitQty,
			invoiceLine.ZG_CountryOfSupply,
			invoiceLine.JI_DateForDutyOverride,
			invoiceLine.JI_ValuationDateOverride,
		});
		return expectedKeysFromGetKeyForLine.ToArray();
	}
	#endregion
}
