using System;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EmmaMessageGeneratorProcessor))]
sealed class EmmaMessageGeneratorProcessorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When entryHeader is null", () => new EmmaMessageGeneratorProcessor(entryHeader: null));
	}

	[TestDate(2024, 04, 01, 0, 0, 0)]
	public void TestProcess()
	{
		SetupExchangeRate();
		var notificationBuffer = new NotificationBuffer();
		var entryHeader = PrepareCusEntryHeaderDataForProcessorTesting();
		IProcessor processor = new EmmaMessageGeneratorProcessor(entryHeader);
		processor.Process(notificationBuffer);

		var messages = entryHeader.Messages;
		AssertEquals("Messages Count", 1, messages.Count);

		var message = messages[0];

		CombineAssertions(() =>
		{
			AssertEquals("EM_ApplicationCode", Messaging.Integration.ApplicationCodeList.Codes.NOCustomsEmma, message.EM_ApplicationCode);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_MessageType", "EMM", message.EM_MessageType);
			AssertEquals("EM_ApplicationReference", ZString.Empty, message.EM_ApplicationReference);
			AssertEquals("EM_MessageSubType", ZString.Empty, message.EM_MessageSubType);
			AssertEquals("EM_LinkedObject", entryHeader.PK, message.EM_LinkedObject.PK);
			AssertEquals("EM_MessageText", EmmaMessageForTesting, message.EM_MessageText);
			AssertEquals("EM_Status", "QUE", message.EM_Status);
			AssertEquals("EM_GB", entryHeader.Declaration.JE_GB, message.EM_GB);

			var informationNotifications = notificationBuffer.GetEventsByType(NotificationType.Information).ToArray();
			AssertContainsExactElementsInAnyOrder("Information Messages",
				[
					$"'EMMA' Message generation started for EntryHeader with PK [{entryHeader.PK}].",
					$"'EMMA' Message with PK [{message.PK}] generated for EntryHeader with PK [{entryHeader.PK}]."
				],
				informationNotifications.Select(i => i.Message));
		});
	}

	public void TestProcess_WhenExceptionIsThrown()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var notificationBuffer = new NotificationBuffer();
		IProcessor processor = new EmmaMessageGeneratorProcessorForTest(entryHeader);
		CombineAssertions(() =>
		{
			AssertExceptionThrown<Exception>("Exception", "Exception thrown on purpose.", () => processor.Process(notificationBuffer));
			AssertEquals("HasErrors", expected: true, notificationBuffer.HasErrors);
			AssertContainsExactElementsInAnyOrder(
				[$"'EMMA' Message generation failed for EntryHeader with PK [{entryHeader.PK}]."],
				notificationBuffer.GetEventsByType(NotificationType.Error).Select(e => e.Message));
		});
	}

	CusEntryHeader PrepareCusEntryHeaderDataForProcessorTesting()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		declaration.JE_MessageSubType = "IM";
		declaration.JE_DeclarationReference = "DECREF";

		declaration.JE_GoodsOrigin = "SE";
		declaration.JE_VesselName = "Vessel";
		declaration.JE_RN_NKTransportNationality = "DE";
		declaration.JE_CustomsTransportMode = "40";
		declaration.JE_LocationOfGoods = "A";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = "4";
		entryInstruction.CEI_PackageCount = 10;

		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_InvoiceNumber = "INV0001";
		invoice.JZ_InvoiceAmount = 1000m;
		invoice.JZ_InvoiceDate = new ZDate(2024, 03, 12);
		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedKingdom;
		invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 10m, Core.Constants.CurrencyCodes.UnitedKingdom);

		var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		invoiceLine.JI_LinePrice = 100m;
		invoiceLine.JI_LinePrice = 1000m;
		invoiceLine.JI_Weight = 197m;
		invoiceLine.JI_NetWeight = 193m;
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_ReducedCustomsFlag = "N";
		invoiceLine.JI_CountryOfOrigin = "SE";
		invoiceLine.JI_StateOrRegionOfOrigin = "D";
		invoiceLine.JI_PrimaryPreference = "N";
		invoiceLine.JI_Description = "DESC LINE 1";

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = UniversalReferenceConstants.CusEntryStatus.TKR;
		entryHeader.SetEntryReleaseNumber("4430012400346001");
		entryHeader.CH_BGMReference = "REF123";
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_PaymentMethod = "M";
		entryHeader.CH_SystemLastEditTimeUtc = new ZDateTime(2023, 11, 21);

		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		entryLine.CL_CustomsValue = 22;
		entryLine.CL_AdValoremTariff = "11111";
		entryLine.CL_StatisticalValue = 76;
		entryLine.CL_LineNumber = 1;

		var feeOne = entryLine.Fees.AddNew("TL", 100);
		feeOne.CF_Rate = 1;
		var feeTwo = entryLine.Fees.AddNew("MA200", 50);
		feeTwo.CF_Rate = 1.2m;

		Factory.Save();

		var declarant = OrgHeaderTestDataHelper.CreateOrgHeaderWithMainAddress(Factory,
			"Mr Declarant",
			"Street 1",
			"Some Locality",
			"12345",
			"Wiesbaden",
			"DE",
			"DEC");
		declarant.MainAddress.OA_Phone = "12345";
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
		declarant.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.DefermentApprovalNumber, "54635222");

		var supplier = OrgHeaderTestDataHelper.CreateOrgHeaderWithMainAddress(Factory,
			"Mr Supplier",
			"Street One",
			"Street Two",
			"99999",
			"City",
			"DE",
			"SUP");
		supplier.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.MVARegistrationNumber, "12234423333");
		declaration.JE_OA_SupplierAddress = supplier.MainAddress.PK;

		var importer = OrgHeaderTestDataHelper.CreateOrgHeaderWithMainAddress(Factory,
			"Mr Importer",
			"Street One",
			"Street Two",
			"99999",
			"City",
			"DE",
			"IMP");
		importer.OH_Code = "IMP";
		importer.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
		importer.WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.MVARegistrationNumber, "12234423339");
		declaration.JE_OA_ImporterAddress = importer.MainAddress.PK;

		var entryHeaderDocManager = entryHeader.DocManagerInfo();
		entryHeaderDocManager.AddFileOrDocument([1, 2, 3], "SAD Print Document.pdf", "EPR", true);

		var declarationDocManager = declaration.DocManagerInfo();
		declarationDocManager.AddFileOrDocument([1, 2, 3], "InvoiceDocument.pdf", "INV", true);

		var staff = Factory.New<GlbStaff>();
		staff.GS_City = "Wiesbaden";
		staff.GS_FullName = "Mr Test User";
		staff.GS_EmailAddress = "test@invalid.com";
		staff.GS_Code = "STW";
		declaration.JE_GS_NKCusAgent = staff.GS_Code;

		return entryHeader;
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

	const string EmmaMessageForTesting =
		"""
		<?xml version="1.0" encoding="iso-8859-1" standalone="yes"?>
		<EmmaSystemsFortolling>
			<Fortolling>
				<Tollnummer>4430012400346001</Tollnummer>
				<MRN>4430012400346001</MRN>
				<Unhsekvens>REF123</Unhsekvens>
				<Unhversjon>REF123</Unhversjon>
				<Deklarasjon1A>IM</Deklarasjon1A>
				<Deklarasjon1B>4</Deklarasjon1B>
				<Avsender>
					<Orgnr>12234423333</Orgnr>
					<Navn>Mr Supplier</Navn>
					<Gateadresse>Street One</Gateadresse>
					<Postadresse>Street Two</Postadresse>
					<Postnr>99999</Postnr>
					<Poststed>City</Poststed>
					<Land>Germany</Land>
					<Landkode>DE</Landkode>
				</Avsender>
				<Vareposter>1</Vareposter>
				<Antallkolli>10</Antallkolli>
				<TotBruttoVekt>197</TotBruttoVekt>
				<TotNettoVekt>193</TotNettoVekt>
				<Referanse>DECREF</Referanse>
				<Mottaker>
					<Orgnr>12234423339</Orgnr>
					<Navn>Mr Importer</Navn>
					<Gateadresse>Street One</Gateadresse>
					<Postadresse>Street Two</Postadresse>
					<Postnr>99999</Postnr>
					<Poststed>City</Poststed>
					<Land>Germany</Land>
					<Landkode>DE</Landkode>
					<Nr>IMP</Nr>
					<Privatkunde>true</Privatkunde>
				</Mottaker>
				<Verdiopplysninger>9.09</Verdiopplysninger>
				<Deklarantnavn>Mr Declarant</Deklarantnavn>
				<Deklaranttlf>12345</Deklaranttlf>
				<Landkode>SE</Landkode>
				<Landtekst>Sweden</Landtekst>
				<Container>Nei</Container>
				<AktivtransportID>Vessel</AktivtransportID>
				<AktivtransportTYPE>DE</AktivtransportTYPE>
				<Transaksjonstype>01</Transaksjonstype>
				<Transportvedgrensen>40</Transportvedgrensen>
				<Betaler>M</Betaler>
				<Faktura>
					<Fakturanr>INV0001</Fakturanr>
					<Valuta>GBP</Valuta>
					<Dato>12-03-2024</Dato>
					<Kurs>1,100</Kurs>
					<Belop>1000,00</Belop>
				</Faktura>
				<Varenslagringsstedkode>A</Varenslagringsstedkode>
				<Varenslagringsstedtekst>Customs Warehouse A</Varenslagringsstedtekst>
				<Varelinje>
					<Grunnlag>22</Grunnlag>
					<MerkeogNummer>ADR</MerkeogNummer>
					<Art>10</Art>
					<Vareslag>DESC LINE 1</Vareslag>
					<Godsbeskrivelse1>DESC LINE 1</Godsbeskrivelse1>
					<Varepostnr>1</Varepostnr>
					<Varenummer>11111</Varenummer>
					<Tollnedsettelse>S</Tollnedsettelse>
					<Opprinnelse>SE</Opprinnelse>
					<Opprfylke>D</Opprfylke>
					<Bruttovekt>197</Bruttovekt>
					<Preferanse>N</Preferanse>
					<Nettovekt>193</Nettovekt>
					<Verdifastsettelse>1</Verdifastsettelse>
					<Justernig>-978</Justernig>
					<Statistiskverdi>76</Statistiskverdi>
					<Avgift>
						<Avgkode>TL</Avgkode>
						<Grunnlag>0</Grunnlag>
						<Belop>100</Belop>
						<Sats>1,00</Sats>
					</Avgift>
					<Avgift>
						<Avgkode>MA</Avgkode>
						<Grunnlag>0</Grunnlag>
						<Belop>50</Belop>
						<Sats>1,20</Sats>
					</Avgift>
				</Varelinje>
				<Vedlegg>
					<Filnavn>EPR-4430012400346001-SAD Print Document.pdf</Filnavn>
					<Beskrivelse>Entry Print/ Customs Declaration Documents</Beskrivelse>
					<DokumentDato>01-Apr-24</DokumentDato>
				</Vedlegg>
				<Vedlegg>
					<Filnavn>INV-4430012400346001-InvoiceDocument.pdf</Filnavn>
					<Beskrivelse>Invoice</Beskrivelse>
					<DokumentDato>01-Apr-24</DokumentDato>
				</Vedlegg>
				<Tollkontonr>546352</Tollkontonr>
				<Kontrollsiffer>22</Kontrollsiffer>
				<Sted>Wiesbaden</Sted>
				<Dato>01-04-2024</Dato>
				<Representantensnavn>Eagle Datamation International</Representantensnavn>
				<Representantensunderskrift>Mr Test User</Representantensunderskrift>
				<Representantensemail>test@invalid.com</Representantensemail>
				<TilleggsOpplysninger>
					<InternReferanse>DECREF</InternReferanse>
					<ImportEksport>I</ImportEksport>
					<SumAvgifter>150</SumAvgifter>
					<SumMoms>0</SumMoms>
					<SumEkslMoms>100</SumEkslMoms>
				</TilleggsOpplysninger>
				<Omberegning />
				<Fakturabelop>1000,00</Fakturabelop>
				<Fakturuakurs>1,100</Fakturuakurs>
			</Fortolling>
		</EmmaSystemsFortolling>
		""";

	class EmmaMessageGeneratorProcessorForTest(CusEntryHeader entryHeader) : EmmaMessageGeneratorProcessor(entryHeader)
	{
		protected override IXmlMessage GetEmmaXmlMessage(CusEntryHeader cusEntryHeader)
		{
			throw new Exception("Exception thrown on purpose.");
		}
	}
}
