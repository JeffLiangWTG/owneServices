using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ExportEntryCreationStrategyTest : EntryCreationStrategyTest
{
	#region Tests

	public void TestCreateEntryCreationStrategy()
	{
		var (dec, _) = PrepareTestJobData(Factory, 0);
		var strategy = dec.CreateEntryCreationStrategy();
		Assert(strategy is ExportEntryCreationStrategy);
	}

	public void TestGetKeyForLine_And_TestGetKeyForHeaderCore()
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

	// PL does not use default EU CH_BGMReference codes
	public void TestHeaderAdditionalInfoOnInvoiceHeadersCreatesTwoEntries()
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
		Factory.Save();
		var dec = GetJobDeclarationForTest();
		var inv1 = dec.Invoices.AddNew();
		var invLine1 = inv1.JobComInvoiceLines.AddNew();
		invLine1.JI_Tariff = "2203001010";
		var inv2 = dec.Invoices.AddNew();
		var invLine2 = inv2.JobComInvoiceLines.AddNew();
		invLine2.JI_Tariff = "2203001010";

		dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals(1, dec.CustomsEntryHeaders.Count);
		var inv1AddInfo = inv1.AdditionalInfos.AddNew();
		inv1AddInfo.CSI_Code = "RPTID";
		inv1AddInfo.CSI_Description = "123";

		var inv2AddInfo = inv2.AdditionalInfos.AddNew();
		inv2AddInfo.CSI_Code = "RPTID";
		inv2AddInfo.CSI_Description = "123";

		dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		AssertEquals(1, dec.CustomsEntryHeaders.Count);

		inv2AddInfo.CSI_Description = "124";

		dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		Factory.Save();
		AssertEquals("Two entries should be created because description is different for each RPTID", 2, dec.CustomsEntryHeaders.Count);
		Factory.Save(); // Triggers save and calculation of BGM Reference
		ZString year = ZDateTime.Now.ToString("yy");
		string[] expected = { $"{year}BNE000000001", $"{year}BNE000000002" };
		string[] actual = { dec.CustomsEntryHeaders[0].CH_BGMReference, dec.CustomsEntryHeaders[1].CH_BGMReference };
		AssertContainsExactElementsInAnyOrder(expected, actual);
	}

	#region Previous Documents

	protected override string[] GetExpectedPreviousDocumentKeys() => new[]
	{
		PreviousDocument.Schema.CSI_DateOfIssue,
		PreviousDocument.Schema.CSI_LineNo,
		PreviousDocument.Schema.CSI_SubType,
		PreviousDocument.Schema.CSI_ReferenceNumber,
		PreviousDocument.Schema.CSI_Code
	};

	public void TestPreviousDocumentsAffectingMergingByCSI_DateOfIssue()
	{
		TestPreviousDocumentsAffectingMergingBy(nameof(PreviousDocument.CSI_DateOfIssue), ZDateTime.Today, ZDateTime.Today.AddDays(2));
	}

	public void TestPreviousDocumentsAffectingMergingByCSI_LineNo()
	{
		TestPreviousDocumentsAffectingMergingBy(nameof(PreviousDocument.CSI_LineNo), new ZInt(1), new ZInt(2));
	}

	public void TestPreviousDocumentsAffectingMergingByCSI_SubType()
	{
		TestPreviousDocumentsAffectingMergingBy(nameof(PreviousDocument.CSI_SubType), new ZString("A"), new ZString("B"));
	}

	public void TestPreviousDocumentsAffectingMergingByCSI_ReferenceNumber()
	{
		TestPreviousDocumentsAffectingMergingBy(nameof(PreviousDocument.CSI_ReferenceNumber), new ZString("R001"), new ZString("R002"));
	}

	public void TestPreviousDocumentsAffectingMergingByCSI_Code()
	{
		TestPreviousDocumentsAffectingMergingBy(nameof(PreviousDocument.CSI_Code), new ZString("C001"), new ZString("C002"));
	}

	#endregion

	#region Supporting Documents

	public void TestSupportingDocumentsAffectingMergingByMultipleDocuments()
	{
		void AddSupportingDocument(
			JobComInvoiceLine line,
			string code,
			string reference,
			string status,
			string uq,
			string countryCode,
			string currency,
			short itemNum,
			string description)
		{
			var supDoc = line.SupportingDocuments.AddNew();
			supDoc.CSI_Code = code;
			supDoc.CSI_ReferenceNumber = reference;
			supDoc.CSI_Status = status;
			supDoc.CSI_UnitOfQuantity = uq;
			supDoc.CSI_RN_NKCountryCode = countryCode;
			supDoc.CSI_RX_NKCurrency = currency;
			supDoc.CSI_ItemNumber = itemNum;
			supDoc.CSI_AdditionalDescription = description;
		}

		var (dec, invLines) = PrepareTestJobData(Factory, 2);
		var strategy = dec.CreateEntryCreationStrategy();
		var line0 = invLines[0];
		var line1 = invLines[1];

		CombineAssertions(() =>
		{
			AssertEquals("BaseLine: all lines' keys are the same without supportingDocs", strategy.GetKeyForLine(line0), strategy.GetKeyForLine(line1));

			AddSupportingDocument(line0, "C001", "R001", "OK", "KG", "PL", "PLN", 1, "test1");
			AddSupportingDocument(line0, "C002", "R002", "OK", "KG", "PL", "PLN", 2, "test2");

			AddSupportingDocument(line1, "C001", "R001", "OK", "KG", "PL", "PLN", 1, "test1");
			AssertNotEquals("Lines have different number of documents.", strategy.GetKeyForLine(line0), strategy.GetKeyForLine(line1));

			line1.SupportingDocuments.RemoveAll();
			AddSupportingDocument(line1, "C001", "R001", "OK", "KG", "PL", "PLN", 1, "test1");
			AddSupportingDocument(line1, "C002", "R002", "OK", "KG", "DE", "EUR", 2, "test2");
			AssertNotEquals("Lines have not identical documents.", strategy.GetKeyForLine(line0), strategy.GetKeyForLine(line1));

			line1.SupportingDocuments.RemoveAll();
			AddSupportingDocument(line1, "C001", "R001", "OK", "KG", "PL", "PLN", 1, "test1");
			AddSupportingDocument(line1, "C002", "R002", "OK", "KG", "PL", "PLN", 2, "test2");
			AssertEquals("Lines have identical ordered documents.", strategy.GetKeyForLine(line0), strategy.GetKeyForLine(line1));

			line1.SupportingDocuments.RemoveAll();
			AddSupportingDocument(line1, "C002", "R002", "OK", "KG", "PL", "PLN", 2, "test2");
			AddSupportingDocument(line1, "C001", "R001", "OK", "KG", "PL", "PLN", 1, "test1");
			AssertEquals("Lines have identical unordered documents .", strategy.GetKeyForLine(line0), strategy.GetKeyForLine(line1));
		});
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

	public void TestSupportingDocumentsAffectingMergingByCSI_RX_NKCurrency()
	{
		TestSupportingDocumentsAffectingMergingBy(nameof(SupportingDocument.CSI_RX_NKCurrency), new ZString("PLN"), new ZString("EUR"));
	}

	public void TestSupportingDocumentsAffectingMergingByCSI_ItemNumber()
	{
		TestSupportingDocumentsAffectingMergingBy(nameof(SupportingDocument.CSI_ItemNumber), new ZInt(123), new ZInt(321));
	}

	public void TestSupportingDocumentsAffectingMergingByCSI_AdditionalDescription()
	{
		TestSupportingDocumentsAffectingMergingBy(nameof(SupportingDocument.CSI_AdditionalDescription), new ZString("DESCR1"), new ZString("DESCR2"));
	}

	protected override string[] GetExpectedSupportingDocumentKeys() => new[]
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_Status,
		SupportingDocument.Schema.CSI_UnitOfQuantity,
		SupportingDocument.Schema.CSI_RN_NKCountryCode,
		SupportingDocument.Schema.CSI_RX_NKCurrency,
		SupportingDocument.Schema.CSI_ItemNumber,
		SupportingDocument.Schema.CSI_AdditionalDescription,
	};

	#endregion

	#region Additional Info

	public void TestAdditionalInfoKeys()
	{
		var dec = GetJobDeclarationForTest();
		var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
		var inv1 = dec.Invoices.AddNew();
		var invLine = inv1.JobComInvoiceLines.AddNew();
		invLine.JI_CEI = cei.PK;
		var mergeStrategy = dec.CreateEntryCreationStrategy();
		var supportingDocumentKeys = mergeStrategy.GetAdditionalInfoKeys();
		AssertArrayEqualsByElements(supportingDocumentKeys, GetExpectedAdditionalInfoKeys());
	}

	public void TestAdditionalInfoAffectingMerging()
	{
		var (dec, invLines) = PrepareTestJobData(Factory, 4);
		var strategy = dec.CreateEntryCreationStrategy();

		CombineAssertions("BaseLine: all lines's keys are the same without supportingDocs", () =>
		{
			var keyForLine1 = strategy.GetKeyForLine(invLines[0]);
			AssertEquals("Line1 == Line2", keyForLine1, strategy.GetKeyForLine(invLines[1]));
			AssertEquals("Line1 == Line3", keyForLine1, strategy.GetKeyForLine(invLines[2]));
			AssertEquals("Line1 == Line4", keyForLine1, strategy.GetKeyForLine(invLines[3]));
		});

		AddAdditionalInfoForInvoiceLines(invLines[0], "C001", "R001", "DESC");
		AddAdditionalInfoForInvoiceLines(invLines[1], "C001", "R001", "DESC");
		AddAdditionalInfoForInvoiceLines(invLines[1], "C002", "R002", "DESC");
		AddAdditionalInfoForInvoiceLines(invLines[2], "C002", "R002", "DESC");
		AddAdditionalInfoForInvoiceLines(invLines[2], "C001", "R001", "DESC");
		AddAdditionalInfoForInvoiceLines(invLines[3], "C001", "R001", "DESC");
		AddAdditionalInfoForInvoiceLines(invLines[3], "C002", "R002", "OTHER DESC");

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
	}

	string[] GetExpectedAdditionalInfoKeys() => new[]
	{
		AdditionalInfo.Schema.CSI_Code,
		AdditionalInfo.Schema.CSI_Description,
		AdditionalInfo.Schema.CSI_ReferenceNumber,
	};

	#endregion

	#endregion

	#region Preperation

	protected override string GetTestedMessageType => JobMessageTypeList.Codes.Export;

	protected override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		var entry = declaration.CustomsEntryInstructions.AddNew();
		entry.CEI_DateForDuty = ZDateTime.Now.AddDays(2);

		return declaration;
	}

	void PopulateDeclarationDataForGetKeyForLineFromPL(JobComInvoiceLine invLine)
	{
		invLine.JI_CustomsUnitQty = "T";
		invLine.JI_BondedWhsUnitQty = "1.00";

		invLine.InvoiceHeader.ZG_TransportChargesMethodOfPayment = "1";

		var org = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
		orgAddress.OA_Address1 = "Test Street";
		orgAddress.CompanyName = "Test Declarant";
		orgAddress.OA_OH = org.PK;
		orgAddress.OA_RN_NKCountryCode = "PL";
		invLine.InvoiceHeader.ConsigneeOrgPK = org.PK;
		invLine.InvoiceHeader.JZ_OA_ConsigneeAddress = orgAddress.PK;
		invLine.JI_TargetEntryLineNumber = ZShort.Parse("1");
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

	void AddAdditionalInfoForInvoiceLines(JobComInvoiceLine line, string code, string reference, string description)
	{
		var addInfo = line.AdditionalInfos.AddNew();
		addInfo.CSI_Code = code;
		addInfo.CSI_ReferenceNumber = reference;
		addInfo.CSI_Description = description;
	}

	#endregion

	#region Assert

	const int KeysFromGetKeyForHeaderPrefixCount = 34;

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
			invoiceLine.InvoiceHeader.ZG_TransportChargesMethodOfPayment,
			invoiceLine.InvoiceHeader.JZ_OA_ConsigneeAddress,
			invoiceLine.JI_CustomsUnitQty,
			invoiceLine.JI_CustomsSecondUnitQty,
			invoiceLine.JI_CustomsThirdUnitQty,
			invoiceLine.JI_CustomsFourthUnitQty,
			invoiceLine.JI_CustomsFifthUnitQty,
			invoiceLine.JI_BondedWhsUnitQty,
			invoiceLine.JI_TargetEntryLineNumber,
			invoiceLine.PacksMeasure,
		});
		return expectedKeysFromGetKeyForLine.ToArray();
	}

	#endregion
}
