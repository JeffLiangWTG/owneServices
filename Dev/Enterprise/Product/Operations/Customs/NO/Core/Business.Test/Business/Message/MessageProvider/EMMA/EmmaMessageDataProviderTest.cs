using System.Linq;
using CargoWise.Customs.NO.MessageContracts.EMMA;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EmmaMessageDataProvider))]
sealed class EmmaMessageDataProviderTest : TestCaseWithFactory
{
	public void TestProviderRoot()
	{
		var provider = EmmaMessageDataProvider.CreateProvider(entryHeaderWrapper, additionalDataProviderMock.Object);
		AssertNotNull("Entry Headers", provider?.Fortolling);
		AssertEquals("Record Count in Headers", 1, provider!.Fortolling.Count);
	}

	[TestDate(2024, 04, 01, 0, 0, 0)]
	public void TestEmmaFortollingDataProvider()
	{
		SetupExchangeRate();

		var cusAgentStaff = Factory.New<GlbStaff>();
		cusAgentStaff.GS_Code = "ZZZ";
		cusAgentStaff.GS_City = "Wiesbaden";
		cusAgentStaff.GS_FullName = "Mr Test User";
		cusAgentStaff.GS_EmailAddress = "test@invalid.com";
		declaration.JE_GS_NKCusAgent = cusAgentStaff.GS_Code;

		additionalDataProviderMock.Setup(a => a.GetContainerMode(entryHeader)).Returns("0");
		additionalDataProviderMock.Setup(a => a.GetTotalInvoiceOrLinesAmount(entryHeader)).Returns(1000);
		additionalDataProviderMock.Setup(a => a.GetGNONumberFirstPart(entryHeader)).Returns("888");
		additionalDataProviderMock.Setup(a => a.GetGNOorGSPNumberSecondPart(entryHeader)).Returns("777");

		var vedleggMock = Mock.Of<EmmaSystemsFortollingFortollingVedleggDataProviderAbstractClass>();
		additionalDataProviderMock.Setup(a => a.GetDocuments(entryHeader)).Returns([vedleggMock]);

		invoice.JZ_InvoiceAmount = 1000m;
		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedKingdom;
		invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 10m, Core.Constants.CurrencyCodes.UnitedKingdom);
		invoice.InvoiceLines.RemoveAndDeleteAll();
		var newInvoiceLine = invoice.InvoiceLines.AddNew();
		newInvoiceLine.JI_LinePrice = 1000m;
		newInvoiceLine.JI_Weight = 197m;
		newInvoiceLine.JI_NetWeight = 193m;
		Factory.Save();

		newInvoiceLine.JI_CL = entryLine.PK;

		var declarant = OrgHeaderTestDataHelper.CreateOrgHeaderWithMainAddress(Factory,
			"Mr Declarant",
			"Street 1",
			"Some Locality",
			"12345",
			"Wiesbaden",
			"DE");
		declarant.MainAddress.OA_Phone = "12345";
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		AddCusRegNo(declarant, UniversalReferenceConstants.OrgCodeType.DefermentApprovalNumber, "54635222");

		declaration.JE_MessageSubType = "IM";
		declaration.JE_DeclarationReference = "DECREF";
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		declaration.JE_GoodsOrigin = "SE";
		declaration.JE_VesselName = "Vessel";
		declaration.JE_RN_NKTransportNationality = "DE";
		declaration.JE_CustomsTransportMode = "40";
		declaration.JE_LocationOfGoods = "A";
		CreateEntryReleaseNumber("NUM123");
		entryHeader.CH_BGMReference = "REF123";
		entryInstruction.CEI_PackageCount = 10;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_PaymentMethod = "M";
		entryHeader.CH_SystemLastEditTimeUtc = new ZDateTime(2023, 11, 21);
		entryHeader.MergedLines.AddNew();
		entryHeader.MergedLines.AddNew();

		var staff = Factory.New<GlbStaff>();
		staff.GS_City = "Wiesbaden";
		staff.GS_FullName = "Mr Test User";
		staff.GS_EmailAddress = "test@invalid.com";
		staff.GS_Code = "STW";
		declaration.JE_GS_NKCusAgent = staff.GS_Code;

		var root = EmmaMessageDataProvider.CreateProvider(entryHeaderWrapper, additionalDataProviderMock.Object);
		var dataProvider = root.Fortolling?.FirstOrDefault();
		CombineAssertions("[Pre-Condition]", () =>
		{
			AssertNotNull("Root", root);
			AssertNotNull("Fortolling Data Provider", dataProvider);
		});

		CombineAssertions("When Import Declaration", () =>
		{
			AssertEquals("Tollnummer (EntryReleaseNumber)", "NUM123", dataProvider!.Tollnummer);
			AssertEquals("MRN (EntryReleaseNumber)", "NUM123", dataProvider!.MRN);
			AssertEquals("Unhsekvens (CH_BGMReference)", "REF123", dataProvider!.Unhsekvens);
			AssertEquals("Unhversjon (CH_BGMReference)", "REF123", dataProvider!.Unhversjon);
			AssertEquals("Deklarasjon1A (JE_MessageSubType)", "IM", dataProvider!.Deklarasjon1A);
			AssertEquals("Deklarasjon1B (CEI_Style) ", "4", dataProvider!.Deklarasjon1B);
			AssertEquals("Vareposter (Merged Lines Count) ", "3", dataProvider!.Vareposter);
			AssertEquals("Antallkolli (CEI_PackageCount) ", "10", dataProvider!.Antallkolli);
			AssertEquals("TotBruttoVekt (TotalGrossWeight) ", 197m, dataProvider!.TotBruttoVekt);
			AssertEquals("TotNettoVekt (TotalNetWeight) ", 193m, dataProvider!.TotNettoVekt);
			AssertEquals("Referanse (JE_DeclarationReference) ", "DECREF", dataProvider!.Referanse);
			AssertEquals("Verdiopplysninger (TotalFreightInLocalCurrency) ", 9.09m, dataProvider!.Verdiopplysninger);
			AssertEquals("Deklarantnavn (Declarant.FullName) ", "Mr Declarant", dataProvider!.Deklarantnavn);
			AssertEquals("Deklaranttlf (Declarant.Phone) ", "12345", dataProvider!.Deklaranttlf);
			AssertEquals("Landkode (JE_GoodsOrigin) ", "SE", dataProvider!.Landkode);
			AssertEquals("Levvilktekst (JZ_IncoTermPlace)", "Place", dataProvider!.Levvilktekst);
			AssertEquals("Container", "0", dataProvider!.Container);
			AssertEquals("Levvilkkode (JE_IncoTerm)", "FOB", dataProvider!.Levvilkkode);
			AssertEquals("Levvilktekst (JE_IncoTermPlace)", "Place", dataProvider!.Levvilktekst);
			AssertEquals("AktivtransportID (JE_VesselName)", "Vessel", dataProvider!.AktivtransportID);
			AssertEquals("AktivtransportTYPE (JE_RN_NKTransportNationality)", "DE", dataProvider!.AktivtransportTYPE);
			AssertEquals("Fakturabelop", 1000m, dataProvider!.Fakturabelop);
			AssertEquals("Fakturuakurs (CurrentExchangeRate)", 1.1m, dataProvider!.Fakturuakurs);
			AssertEquals("Transaksjonstype (JZ_ValuationCode)", "CD", dataProvider!.Transaksjonstype);
			AssertEquals("Transportvedgrensen (JE_CustomsTransportMode)", "40", dataProvider!.Transportvedgrensen);
			AssertEquals("Betaler (CH_PaymentMethod)", "M", dataProvider!.Betaler);
			AssertEquals("Betaler (JE_CustomsOffice)", "", dataProvider!.Utpasseringstollstedkode);
			AssertEquals("Betaler (CustomsOfficeName)", "", dataProvider!.Utpasseringstollstedtekst);
			AssertEquals("Betaler (JE_LocationOfGoods)", "A", dataProvider!.Varenslagringsstedkode);
			AssertEquals("Betaler (LocationOfGoodsDescription)", "Customs Warehouse A", dataProvider!.Varenslagringsstedtekst);
			AssertEquals("Tollkontonr (Declarant.DefermentApprovalNumber Number first 5/6)", "546352", dataProvider!.Tollkontonr);
			AssertEquals("Kontrollsiffer (Declarant.DefermentApprovalNumber Number last 2)", "22", dataProvider!.Kontrollsiffer);
			AssertEquals("Godsnr", "888", dataProvider!.Godsnr);
			AssertEquals("Posnr", "777", dataProvider!.Posnr);
			AssertType<string>("Std Mapped Value Type", dataProvider!.Sted);
			AssertEquals("Std (CusAgent.CS_City", "Wiesbaden", dataProvider!.Sted);
			AssertEquals("Dato (CH_SystemLastEditTimeUtc", "21-11-2023", dataProvider!.Dato);
			AssertEquals("Representantensnavn (CusAgent.CompanyName)", Env.CurrentCompany.Name, dataProvider!.Representantensnavn);
			AssertEquals("Representantensunderskrift (CusAgent.GS_FullName)", "Mr Test User", dataProvider!.Representantensunderskrift);
			AssertEquals("Representantensemail (CusAgent.GS_EmailAddress)", "test@invalid.com", dataProvider!.Representantensemail);

			AssertNotNull("Avsender", dataProvider!.Avsender);
			AssertNotNull("Mottaker", dataProvider!.Mottaker);
			AssertNotNull("Faktura", dataProvider!.Faktura);
			AssertNotNull("Varelinje", dataProvider!.Varelinje);

			AssertNotNull("Vedlegg", dataProvider!.Vedlegg);
			AssertEquals("Vedlegg Record Count", 1, dataProvider!.Vedlegg.Count);
			AssertEquals("Vedlegg Item", vedleggMock, dataProvider.Vedlegg.FirstOrDefault());

			AssertNotNull("TilleggsOpplysninger", dataProvider!.TilleggsOpplysninger);
			AssertNotNull("Omberegning", dataProvider!.Omberegning);
		});

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.JE_CustomsOffice = "3775";
		declaration.JE_TransportMode = TransportModes.Sea;
		CombineAssertions("When Export Declaration", () =>
		{
			AssertEquals("Betaler (JE_CustomsOffice)", "3775", dataProvider!.Utpasseringstollstedkode);
			AssertEquals("Betaler (CustomsOfficeName)", "Halden", dataProvider!.Utpasseringstollstedtekst);
		});
	}

	public void TestAvsenderDataProvider()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var supplier = OrgHeaderTestDataHelper.CreateOrgHeaderWithMainAddress(Factory,
			"Mr Supplier",
			"Street One",
			"Street Two",
			"99999",
			"City",
			"DE");
		AddCusRegNo(supplier, "MVA", "12234423333");
		declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;

		var root = EmmaMessageDataProvider.CreateProvider(entryHeaderWrapper, additionalDataProviderMock.Object);
		var dataProvider = root.Fortolling?.FirstOrDefault()?.Avsender;
		AssertNotNull("[PPE-CONDITION] Avsender", dataProvider);

		CombineAssertions(() =>
		{
			AssertEquals("Orgnr", "12234423333", dataProvider!.Orgnr);
			AssertEquals("Navn", "Mr Supplier", dataProvider!.Navn);
			AssertEquals("Gateadresse", "Street One", dataProvider!.Gateadresse);
			AssertEquals("Postadresse", "Street Two", dataProvider!.Postadresse);
			AssertEquals("Postnr", "99999", dataProvider!.Postnr);
			AssertEquals("Poststed", "City", dataProvider!.Poststed);
			AssertEquals("Land", "Germany", dataProvider!.Land);
			AssertEquals("Landkode", "DE", dataProvider!.Landkode);
		});
	}

	public void TestMottakerDataProvider()
	{
		var importer = OrgHeaderTestDataHelper.CreateOrgHeaderWithMainAddress(Factory,
			"Mr Importer",
			"Street One",
			"Street Two",
			"99999",
			"City",
			"DE");
		importer.OH_Code = "IMP";
		importer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		AddCusRegNo(importer, "MVA", "12234423333");
		declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;

		var root = EmmaMessageDataProvider.CreateProvider(entryHeaderWrapper, additionalDataProviderMock.Object);
		var dataProvider = root.Fortolling?.FirstOrDefault()?.Mottaker;
		AssertNotNull("[PPE-CONDITION] Avsender", dataProvider);

		CombineAssertions(() =>
		{
			AssertEquals("Orgnr", "12234423333", dataProvider!.Orgnr);
			AssertEquals("Navn", "Mr Importer", dataProvider!.Navn);
			AssertEquals("Gateadresse", "Street One", dataProvider!.Gateadresse);
			AssertEquals("Postadresse", "Street Two", dataProvider!.Postadresse);
			AssertEquals("Postnr", "99999", dataProvider!.Postnr);
			AssertEquals("Poststed", "City", dataProvider!.Poststed);
			AssertEquals("Land", "Germany", dataProvider!.Land);
			AssertEquals("Landkode", "DE", dataProvider!.Landkode);
			AssertEquals("Privatkunde", true, dataProvider!.Privatkunde);
			AssertEquals("Nr", "IMP", dataProvider!.Nr);
		});
	}

	[TestDate(2024, 04, 01, 0, 0, 0)]
	public void TestFakturaDataProvider()
	{
		SetupExchangeRate();
		var invoiceTwo = declaration.Invoices.AddNew();
		invoiceTwo.JZ_RX_NKInvoice_Currency = "GBP";
		UpdateInvoiceData(invoice, "INV001", 222, new ZDate(2024, 04, 09));
		UpdateInvoiceData(invoiceTwo, "INV002", 344, new ZDate(2024, 04, 10));

		var invoiceLineTwo = invoiceTwo.InvoiceLines.AddNew();
		invoiceLineTwo.JI_CL = entryLine.PK;

		var root = EmmaMessageDataProvider.CreateProvider(entryHeaderWrapper, additionalDataProviderMock.Object);
		var dataProviderCollection = root.Fortolling?.FirstOrDefault()?.Faktura;
		AssertNotNull("[PPE-CONDITION] Faktura", dataProviderCollection);
		AssertEquals("[PPE-CONDITION] Faktura Record Count", 2, dataProviderCollection!.Count);

		var itemOne = dataProviderCollection.First();
		AssertFakturaItem(itemOne, "INV001", 222, "NOK", "09-04-2024", 1);
		var itemTwo = dataProviderCollection.Last();
		AssertFakturaItem(itemTwo, "INV002", 344, "GBP", "10-04-2024", 1.1m);

		static void AssertFakturaItem(EmmaSystemsFortollingFortollingFakturaDataProviderAbstractClass dataProviderItem,
			string expectedInvoiceNumber,
			decimal expectedAmount,
			string expectedCurrency,
			string expectedDate,
			decimal expectedExchangeRate)
		{
			CombineAssertions($"Invoice with Expected Number: {expectedInvoiceNumber}, Currency = {expectedCurrency}", () =>
			{
				AssertEquals("Fakturanr", expectedInvoiceNumber, dataProviderItem!.Fakturanr);
				AssertEquals("Belop", expectedAmount, dataProviderItem!.Belop);
				AssertEquals("Valuta", expectedCurrency, dataProviderItem!.Valuta);
				AssertEquals("Kurs", expectedExchangeRate, dataProviderItem!.Kurs);
				AssertEquals("Dato", expectedDate, dataProviderItem!.Dato);
			});
		}

		static void UpdateInvoiceData(JobComInvoiceHeader jobComInvoiceHeader, string invoiceNumber, decimal invoiceAmount, ZDate invoiceDate)
		{
			jobComInvoiceHeader.JZ_InvoiceNumber = invoiceNumber;
			jobComInvoiceHeader.JZ_InvoiceAmount = invoiceAmount;
			jobComInvoiceHeader.JZ_InvoiceDate = invoiceDate;
		}
	}

	public void TestVarelinjeDataProvider()
	{
		additionalDataProviderMock.Setup(a => a.GetGoodsMarksAndNos(entryLine)).Returns("VALUE");
		additionalDataProviderMock.Setup(a => a.GetGoodsDescription(entryLine)).Returns("Vareslag");
		additionalDataProviderMock.Setup(a => a.GetGrossWeight(entryLine)).Returns(1234);
		additionalDataProviderMock.Setup(a => a.GetProcedureCode(entryLine)).Returns("P");
		additionalDataProviderMock.Setup(a => a.GetNetWeight(entryLine)).Returns(23);
		additionalDataProviderMock.Setup(a => a.GetCustomsSecondQuantity(entryLine)).Returns(12);
		additionalDataProviderMock.Setup(a => a.GetValuationCodeOrMethod(entryLine)).Returns("Verdifastsettelse");
		additionalDataProviderMock.Setup(a => a.GetVatCode(entryLine)).Returns("MVA12");
		additionalDataProviderMock.Setup(a => a.GetCustomsValueWithoutLinesPrice(entryLine)).Returns(177);
		additionalDataProviderMock.Setup(a => a.GetContainer1(entryLine)).Returns("CONTAINER1");
		additionalDataProviderMock.Setup(a => a.GetContainer2(entryLine)).Returns("CONTAINER2");
		additionalDataProviderMock.Setup(a => a.GetContainer3(entryLine)).Returns("CONTAINER3");
		additionalDataProviderMock.Setup(a => a.GetContainer4(entryLine)).Returns("CONTAINER4");
		additionalDataProviderMock.Setup(a => a.GetContainer5(entryLine)).Returns("CONTAINER5");
		additionalDataProviderMock.Setup(a => a.GetContainer6(entryLine)).Returns("CONTAINER6");
		additionalDataProviderMock.Setup(a => a.GetContainer7(entryLine)).Returns("CONTAINER7");
		additionalDataProviderMock.Setup(a => a.GetContainer8(entryLine)).Returns("CONTAINER8");
		additionalDataProviderMock.Setup(a => a.GetContainer9(entryLine)).Returns("CONTAINER9");

		entryInstruction.CEI_PackageCount = 10;
		entryLine.CL_CustomsValue = 22;
		entryLine.CL_AdValoremTariff = "11111";
		entryLine.CL_StatisticalValue = 76;
		entryLine.CL_LineNumber = 1;
		invoiceLine.JI_ReducedCustomsFlag = "N";
		invoiceLine.JI_CountryOfOrigin = "SE";
		invoiceLine.JI_StateOrRegionOfOrigin = "D";
		invoiceLine.JI_PrimaryPreference = "N";
		invoiceLine.JI_Description = "DESC LINE 1";

		CreateInvoiceLineAndLinkToEntryLine(entryLine, "DESC LINE 2");
		CreateInvoiceLineAndLinkToEntryLine(entryLine, "DESC LINE 3");
		CreateInvoiceLineAndLinkToEntryLine(entryLine, "DESC LINE 4");
		CreateInvoiceLineAndLinkToEntryLine(entryLine, "DESC LINE 5");

		var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		supportingDocument.CSI_Code = "CER";
		supportingDocument.CSI_ReferenceNumber = "REF12345";

		var fee = entryLine.Fees.AddNew();
		fee.CF_ChargeType = "MA100";
		fee.CF_BaseValue = 10;
		fee.CF_Rate = 1.2m;
		fee.CF_ChargeAmount = 12;

		var root = EmmaMessageDataProvider.CreateProvider(entryHeaderWrapper, additionalDataProviderMock.Object);
		var dataProviderCollection = root.Fortolling?.FirstOrDefault()?.Varelinje;
		AssertNotNull("[PPE-CONDITION] Varelinje", dataProviderCollection);
		AssertEquals("[PPE-CONDITION] Varelinje Record Count", 1, dataProviderCollection.Count);

		var item = dataProviderCollection.First();

		CombineAssertions(() =>
		{
			AssertEquals("Grunnlag", 22m, item!.Grunnlag);
			AssertEquals("MerkeogNummer", "VALUE", item!.MerkeogNummer);
			AssertEquals("Art", "10", item!.Art);
			AssertEquals("Vareslag", "Vareslag", item!.Vareslag);
			AssertEquals("Varepostnr", "1", item!.Varepostnr);
			AssertEquals("Varenummer", "11111", item!.Varenummer);
			AssertEquals("Tollnedsettelse", "N", item!.Tollnedsettelse);
			AssertEquals("Opprinnelse", "SE", item!.Opprinnelse);
			AssertEquals("Opprfylke", "D", item!.Opprfylke);
			AssertEquals("Bruttovekt", 1234m, item!.Bruttovekt);
			AssertEquals("Preferanse", "N", item!.Preferanse);
			AssertEquals("Prosedyre", "P", item!.Prosedyre);
			AssertEquals("Nettovekt", 23m, item!.Nettovekt);
			AssertEquals("Annenenhet", 12m, item!.Annenenhet);
			AssertEquals("Verdifastsettelse", "Verdifastsettelse", item!.Verdifastsettelse);
			AssertEquals("MVAFritakKode", "MVA12", item!.MVAFritakKode);
			AssertEquals("Justernig", 177m, item!.Justernig);
			AssertEquals("Statistiskverdi", 76m, item!.Statistiskverdi);
			AssertEquals("Godsbeskrivelse1", "DESC LINE 1", item!.Godsbeskrivelse1);
			AssertEquals("Godsbeskrivelse2", "DESC LINE 2", item!.Godsbeskrivelse2);
			AssertEquals("Godsbeskrivelse3", "DESC LINE 3", item!.Godsbeskrivelse3);
			AssertEquals("Godsbeskrivelse4", "DESC LINE 4", item!.Godsbeskrivelse4);
			AssertEquals("Godsbeskrivelse5", "DESC LINE 5", item!.Godsbeskrivelse5);
			AssertEquals("Container1", "CONTAINER1", item!.Container1);
			AssertEquals("Container2", "CONTAINER2", item!.Container2);
			AssertEquals("Container3", "CONTAINER3", item!.Container3);
			AssertEquals("Container4", "CONTAINER4", item!.Container4);
			AssertEquals("Container5", "CONTAINER5", item!.Container5);
			AssertEquals("Container6", "CONTAINER6", item!.Container6);
			AssertEquals("Container7", "CONTAINER7", item!.Container7);
			AssertEquals("Container8", "CONTAINER8", item!.Container8);
			AssertEquals("Container9", "CONTAINER9", item!.Container9);
		});

		var supportingDocumentsProvider = item!.Dokument;
		AssertNotNull("Dokument", supportingDocumentsProvider);
		AssertEquals("Dokument Count", 1, supportingDocumentsProvider.Count);
		var documentProvider = supportingDocumentsProvider.First();
		CombineAssertions("Supporting Document", () =>
		{
			AssertEquals("Kode", "CER", documentProvider!.Kode);
			AssertEquals("Tekst", "REF12345", documentProvider!.Tekst);
		});

		var feesProvider = item!.Avgift;
		AssertNotNull("Avgift", feesProvider);
		AssertEquals("Avgift Count", 1, feesProvider.Count);
		var feeProvider = feesProvider.First();
		CombineAssertions("Fee", () =>
		{
			AssertEquals("Avgkode", "MA100", feeProvider!.Avgkode);
			AssertEquals("Grunnlag", 10m, feeProvider!.Grunnlag);
			AssertEquals("Sats", 1.2m, feeProvider!.Sats);
			AssertEquals("Belop", "12", feeProvider!.Belop);
		});

		void CreateInvoiceLineAndLinkToEntryLine(CusEntryLine cusEntryLine, string lineDescription)
		{
			var jobComInvoiceLine = invoice.InvoiceLines.AddNew();
			jobComInvoiceLine.JI_Description = lineDescription;
			jobComInvoiceLine.JI_CL = cusEntryLine.PK;
		}
	}

	public void TestTilleggsOpplysningerDataProvider()
	{
		var declarant = OrgHeaderTestDataHelper.CreateOrgHeaderWithMainAddress(Factory,
			"Mr Declarant",
			"Street 1",
			"Some Locality",
			"12345",
			"Wiesbaden",
			"DE");
		AddCusRegNo(declarant, "EMD", "EM123");
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		declaration.JE_DeclarationReference = "REF1233";
		entryHeader.CH_EntryReleaseDate = new ZDate(2024, 04, 09);

		entryLine.Fees.AddNew(NOCustomDutyCodeList.Codes.TL1, 100);
		entryLine.Fees.AddNew("MB100", 23);
		entryLine.Fees.AddNew(NOCustomDutyCodeList.Codes.MV1, 35);

		var root = EmmaMessageDataProvider.CreateProvider(entryHeaderWrapper, additionalDataProviderMock.Object);
		var dataProvider = root.Fortolling?.FirstOrDefault()?.TilleggsOpplysninger;
		AssertNotNull("[PPE-CONDITION] TilleggsOpplysninger", dataProvider);

		CombineAssertions(() =>
		{
			AssertEquals("Deklkode", "EM123", dataProvider!.Deklkode);
			AssertEquals("InternReferanse", "REF1233", dataProvider!.InternReferanse);
			AssertEquals("ImportEksport", "IMP", dataProvider!.ImportEksport);
			AssertEquals("Godkjentdato", "09-04-2024", dataProvider!.Godkjentdato);
			AssertEquals("SumAvgifter", 158m, dataProvider!.SumAvgifter);
			AssertEquals("SumMoms", 35m, dataProvider!.SumMoms);
			AssertEquals("SumEkslMoms", 100m, dataProvider!.SumEkslMoms);
		});
	}

	public void TestOmberegningDataProvider()
	{
		const string contentForTesting = """We use cookies to understand how our website is used, so we can make the content more accessible."""
			+ """ No personal information is stored or shared, and the data we collect is only used to improve the service."""
			+ """ We use cookies to understand how our website is used, so we can make the content more accessible."""
			+ """ No personal information is stored or shared, a""";

		entryHeader.CH_ReCalcReason = contentForTesting;
		entryHeader.CH_ReCalcReplyMessage = contentForTesting;
		entryHeader.CH_ReCalcOrigDecl = "BE";
		entryHeader.CH_ReCalcCaseCode = "IM";

		var root = EmmaMessageDataProvider.CreateProvider(entryHeaderWrapper, additionalDataProviderMock.Object);
		var dataProvider = root.Fortolling?.FirstOrDefault()?.Omberegning;
		AssertNotNull("[PPE-CONDITION] Omberegning", dataProvider);

		CombineAssertions(() =>
		{
			AssertEquals("TadRefOpprDekl", "BE", dataProvider!.TadRefOpprDekl);
			AssertEquals("Kode", "IM", dataProvider!.Kode);

			AssertEquals("Reason1", "We use cookies to understand how our website is used, so we can make t", dataProvider!.Reason1);
			AssertEquals("Reason2", "he content more accessible. No personal information is stored or share", dataProvider!.Reason2);
			AssertEquals("Reason3", "d, and the data we collect is only used to improve the service. We use", dataProvider!.Reason3);
			AssertEquals("Reason4", " cookies to understand how our website is used, so we can make the con", dataProvider!.Reason4);
			AssertEquals("Reason5", "tent more accessible. No personal information is stored or shared, a", dataProvider!.Reason5);

			AssertEquals("MeldingFraToller1", "We use cookies to understand how our website is used, so we can make t", dataProvider!.MeldingFraToller1);
			AssertEquals("MeldingFraToller2", "he content more accessible. No personal information is stored or share", dataProvider!.MeldingFraToller2);
			AssertEquals("MeldingFraToller3", "d, and the data we collect is only used to improve the service. We use", dataProvider!.MeldingFraToller3);
			AssertEquals("MeldingFraToller4", " cookies to understand how our website is used, so we can make the con", dataProvider!.MeldingFraToller4);
			AssertEquals("MeldingFraToller5", "tent more accessible. No personal information is stored or shared, a", dataProvider!.MeldingFraToller5);
		});
	}

	protected override void SetUp()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = MergeInvoiceLinesConstants.Maximum;

		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "4";

		invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = "NOK";
		invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
		invoice.JZ_IncoTermPlace = "Place";
		invoice.JZ_ValuationCode = "CD";
		invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		entryHeaderWrapper = new(entryHeader);
		additionalDataProviderMock = new Mock<IEmmaSystemsFortollingAdditionalDataProvider>();
	}

	Mock<IEmmaSystemsFortollingAdditionalDataProvider> additionalDataProviderMock;
	EmmaMessageEntryHeaderWrapper entryHeaderWrapper;
	CusEntryHeader entryHeader;
	CusEntryLine entryLine;
	JobComInvoiceLine invoiceLine;
	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceHeader invoice;

	void CreateEntryReleaseNumber(string number)
	{
		var entryNum = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Norway.CustomsEntryReleaseNumber, GlbCompany.CurrentCompany.Country.Code, false);
		entryNum.CE_EntryNum = number;
	}

	void  AddCusRegNo(OrgHeader header, string type, string number)
	{
		header.WithOrgCusCode(type, number);
	}

	void SetupExchangeRate()
	{
		var nok = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.Norway);
		nok.ExchangeRates.DeleteAll();

		var gbp = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyCodes.UnitedKingdom);
		gbp.ExchangeRates.DeleteAll();

		var exchangeRate = gbp.ExchangeRates.AddNew();
		exchangeRate.RE_ExRateType = "CUS";
		exchangeRate.RE_StartDate = new ZDateTime(2024, 1, 1, 0, 0, 0);
		exchangeRate.RE_ExpiryDate = new ZDateTime(2025, 12, 31, 0, 0, 0);
		exchangeRate.RE_SellRate = 1.1;

		Factory.Save();
	}
}
