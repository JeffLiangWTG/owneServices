using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Edifact.D96BNZ.Elements;
using Enterprise.Edifact.D96BNZ.Segments;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.Testing
{
	sealed class MessageBuilderForTesting : MessageBuilder
	{
		public MessageBuilderForTesting(CusEntryHeader entryHeader, MessageTypes messageType)
			: base(entryHeader, messageType)
		{
		}

		public void Call_GenerateSegmentFTX(FTXSegmentMessageSection fTXSection, TextSubjectQualifierList textSubjectQualifier, ZString notes)
		{
			GenerateSegmentFTX(fTXSection, textSubjectQualifier, notes);
		}

		public FTXSegmentMessageSection Get_FTXSection()
		{
			return EDIFACTMessage.FTX;
		}
	}

	sealed class MessageBuilderTest : MessageBuilders.Testing.MessageBuilderFromEntryHeaderTest
	{
		[TestDate(2017, 10, 16)]
		public void TestSetParentMessagingStatusAfterMessagePosting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_EDITransmitDate = ZDateTime.Today.AddDays(1);
			MessageBuilder messageBuilder = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original);
			messageBuilder.GenerateMessage();
			AssertEquals(FormalEntryStatusList.Codes.QueuedForSending, declaration.JE_EntryStatus);
			Assert(declaration.JE_EntrySubmittedDate.IsEmpty);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_EDITransmitDate = ZDateTime.Today;
			messageBuilder = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original);
			messageBuilder.GenerateMessage();
			AssertEquals(FormalEntryStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			Assert(!declaration.JE_EntrySubmittedDate.IsEmpty);
		}

		public void TestImportAirOneLinerSendsPaymentMethodSegmentEvenWhenDutyAndGSTAreZero()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 0.00m, 0.00m);

			CreateMessageAndTestResultAgainstString(ImportAirOneLinerSendsPaymentMethodSegmentEvenWhenDutyAndGSTAreZeroMessage, MessageBuilder.MessageTypes.Original);
		}
		#region ImportAirOneLinerSendsPaymentMethodSegmentEvenWhenDutyAndGSTAreZeroMessageMessage
		const string ImportAirOneLinerSendsPaymentMethodSegmentEvenWhenDutyAndGSTAreZeroMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:1000
MOA+70:10
GIS+N:109:143
UNS+S
CNT+4:1
CNT+5:1
CNT+11:100
TAX+3+CUD++10000
MOA+161:0.00
TAX+4+TOT
MOA+161:0.00
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+40+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestCH_LastEntryStyle()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			AssertEquals("Pre-condition: declaration.CusEntryHeader.CH_LastEntryStyle", string.Empty, declaration.CusEntryHeader.CH_LastEntryStyle);

			MessageBuilder messageBuilder = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original);
			messageBuilder.GenerateMessage();

			AssertEquals("declaration.CusEntryHeader.CH_LastEntryStyle", JobMessageSubTypeList.Codes.Normal, declaration.CusEntryHeader.CH_LastEntryStyle);
		}

		public void TestTrimDocumentMessageNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = " INV1230984";
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			MessageBuilder messageBuilder = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original);
			messageBuilder.GenerateMessage();
			AssertEquals(false, messageBuilder.MessageBusinessObject.EM_MessageTextDetail.Contains(invoice.JZ_InvoiceNumber));
			AssertEquals(true, messageBuilder.MessageBusinessObject.EM_MessageTextDetail.Contains(invoice.JZ_InvoiceNumber.ToString().TrimStart()));
		}

		public void TestSetMessageType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			MessageBuilder messageBuilder = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original);
			messageBuilder.GenerateMessage();

			AssertEquals("EM_MessageType should be set", NZCMessage.MessageTypes.FormalEntry.MessageType, declaration.CusEntryHeader.Messages[0].EM_MessageType);
		}

		[TestDate(2008, 10, 15)]
		public void TestFuelWithACCAndPFML()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForImportFromAU();
			declaration.JE_DateOfArrival = new ZDateTime(2008, 10, 15);
			declaration.JE_EDITransmitDate = new ZDateTime(2008, 10, 15);
			declaration.JE_TotalWeight = 1000000m;
			decCreator.SetupInvoiceGroup(10000m, "NZD", 100m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 1000000m);
			decCreator.SetupImportInvoiceLine("2710.19.21.10C", "UNLEADED MOTOR FUEL", "AU", "AU", "N", 1000000m, 1000000, "LTR");
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 1, "VL");

			decCreator.MergeDeclaration();

			CreateMessageAndTestResultAgainstString(FuelWithACCAndPFMLMessage, MessageBuilder.MessageTypes.Original);
		}
		#region FuelWithACCAndPFMLMessage
		const string FuelWithACCAndPFMLMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20081015:102
MEA+WT+AAD+KGM:1000000
RFF+BM:OBL123456
PAC+0++PK
RFF+BM:HOUSEBILL1
PAC+1++VL
TDT+20+109+1+++++:::BUNGA BIDARA
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+2710192110C:169:143
FTX+AAA+++UNLEADED MOTOR FUEL
LOC+27+AU
LOC+35+AU
MEA+AAR++LTR:1000000.000
NAD+SU+00710841Y:ZZZ:143
MOA+14:1000000.00:NZD
CUX+2++1.00
MOA+40:1000000
MOA+64:10000
MOA+70:100
GIS+N:109:143
TAX+1+CST+ACC:167:143
MOA+161:93400.00
TAX+1+CST+PFML:167:143
MOA+161:450.00
TAX+1+CUD
MOA+161:425240.00
TAX+1+GST
MOA+161:191148.75
UNS+S
CNT+4:1
CNT+5:1
CNT+11:1
TAX+3+CST+ACC:167:143
MOA+161:93400.00
TAX+3+CST+PFML:167:143
MOA+161:450.00
TAX+3+CUD++1000000
MOA+161:425240.00
TAX+3+GST
MOA+161:191148.75
TAX+4+TOT
MOA+161:710238.75
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+56+<<MSGNO PLACEHOLDER>>
";
		#endregion

		[TestDate(2007, 12, 15)]
		public void TestExportSeaPeriodicDrawback()
		{
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_Code = "PERIODIC VARIOUS";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("TEOTH", "TEOTH", Core.Constants.CountryCodes.NewZealand);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "TEOTH", "PDO", "PDO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "TEOTH", "PER", "PER", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType("UNPKG", "UNPKG", Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, "UNPKG", "PK", "Package", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			declaration.DisableDefaultPackingInformation = false;

			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.LocalCustomsClientCode = "00782903F";
			declaration.JE_OH_Supplier = supplier.PK;

			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;
			declaration.JE_SoldOrConsigned = TermsOfSaleList.Codes.Sold;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			declaration.JE_VesselName = "PERIODIC VARIOUS";
			declaration.JE_VoyageFlightNo = "1";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2007, 11, 29);
			declaration.JE_RL_NKPortOfArrival = "VARIO";
			declaration.JE_HouseBill = "PERIODIC NOVEMBER 2007";
			declaration.JE_RL_NKOrigin = "NZAKL";
			declaration.JE_DateAtOrigin = new ZDateTime(2007, 11, 29);
			declaration.JE_RL_NKFinalDestination = "VARIO";

			declaration.JE_TotalWeight = 1200.00m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalNoOfPacks = 12;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			AssertEquals("Precondition: Declaration.IsPeriodicDrawback", true, declaration.IsPeriodicDrawback);

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_ShippingLine = carrier.PK;

			RefCurrency nzd = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew("OFT", 100.00m, "NZD");

			JobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "438934";
			invoiceHeader.JZ_InvoiceAmount = 1000.00m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = nzd.RX_Code;
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1101.00.00.00A";
			invoiceLine.JI_Description = "WHEAT OR MESLIN FLOUR";
			invoiceLine.JI_LinePrice = 1000.00m;
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_CustomsQuantity = 1200.00m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_DutyCreditAmount = 60.00m;
			invoiceLine.JI_GSTCreditAmount = 125.00m;

			ZString mergeErrors = declaration.MergeAndSaveIfNotMergedAlreadyReturningErrors();
			AssertEquals("Merge Errors", "", mergeErrors);

			ZString validationErrors = Customs.Business.MessageSendingValidation.New(declaration, null).MessageErrors.ToUniqueMessageListString();
			AssertEquals("Validation Errors", "", validationErrors);

			CreateMessageAndTestResultAgainstString(ExportSeaPeriodicDrawbackMessage, MessageBuilder.MessageTypes.Original);
		}
		#region ExportSeaPeriodicDrawbackMessage
		const string ExportSeaPeriodicDrawbackMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+830+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++41:105:143
LOC+9+NZAKL
LOC+11+VARIO
LOC+41+NZAKL
LOC+28+VA
DTM+129:20071129:102
GIS+S:112:143
GIS+PER:110:143
MEA+WT+AAD+KGM:1200
RFF+BM:PERIODIC NOVEMBER 2007
PAC+12++PK
TDT+20+1+1+++++:::PERIODIC VARIOUS
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+1101000000A:169:143
FTX+AAA+++WHEAT OR MESLIN FLOUR
LOC+27+AU
MEA+AAR++KGM:1200.000
MOA+14:1000.00:NZD
CUX+2++1.00+N
TAX+1+CUD
MOA+161:60.00
TAX+1+GST
MOA+161:125.00
UNS+S
CNT+5:1
CNT+11:12
TAX+3+CUD++1000
MOA+161:60.00
TAX+3+GST
MOA+161:125.00
TAX+4+TOT
MOA+161:185.00
GIS+C:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+39+<<MSGNO PLACEHOLDER>>
";
		#endregion

		[TestDate(2007, 12, 15)]
		public void TestExportAirPeriodicDrawback()
		{
			var refDataHelper = new UniversalReferenceTestDataHelper(Factory);
			refDataHelper.CreateNewOrGetExistingCusCodeList("UNE", "UNPKG", "PK", "Package", ZDateTime.Today, ZDateTime.Today.AddDays(1));
			Factory.Save();
			declaration.DisableDefaultPackingInformation = false;

			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.LocalCustomsClientCode = "00782903F";
			declaration.JE_OH_Supplier = supplier.PK;

			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByBroker;
			declaration.JE_SoldOrConsigned = TermsOfSaleList.Codes.Sold;

			declaration.JE_VoyageFlightNo = "PD0011";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_ExportDate = new ZDateTime(2007, 11, 29);
			declaration.JE_RL_NKPortOfArrival = "VARIO";
			declaration.JE_HouseBill = "PERIODIC NOVEMBER 2007";
			declaration.JE_RL_NKOrigin = "NZAKL";
			declaration.JE_DateAtOrigin = new ZDateTime(2007, 11, 29);
			declaration.JE_RL_NKFinalDestination = "VARIO";

			declaration.JE_TotalWeight = 1200.00m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_TotalNoOfPacks = 12;
			declaration.JE_TotalNoOfPacksPackType = "PK";
			AssertEquals("Precondition: Declaration.IsPeriodicDrawback", true, declaration.IsPeriodicDrawback);

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_ShippingLine = carrier.PK;

			RefCurrency nzd = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");
			JobComInvoiceGroupHeader groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			groupHeader.Charges.AddNew("OFT", 100.00m, "NZD");

			JobComInvoiceHeader invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "438934";
			invoiceHeader.JZ_InvoiceAmount = 1000.00m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = nzd.RX_Code;
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1101.00.00.00A";
			invoiceLine.JI_Description = "WHEAT OR MESLIN FLOUR";
			invoiceLine.JI_LinePrice = 1000.00m;
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_CustomsQuantity = 1200.00m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_DutyCreditAmount = 60.00m;
			invoiceLine.JI_GSTCreditAmount = 125.00m;

			ZString mergeErrors = declaration.MergeAndSaveIfNotMergedAlreadyReturningErrors();
			AssertEquals("Merge Errors", "", mergeErrors);

			ZString validationErrors = Customs.Business.MessageSendingValidation.New(declaration, null).MessageErrors.ToUniqueMessageListString();
			AssertEquals("Validation Errors", "", validationErrors);

			CreateMessageAndTestResultAgainstString(ExportAirPeriodicDrawbackMessage, MessageBuilder.MessageTypes.Original);
		}
		#region ExportAirPeriodicDrawbackMessage
		const string ExportAirPeriodicDrawbackMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+830+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++41:105:143
LOC+9+NZAKL
LOC+11+VARIO
LOC+41+NZAKL
LOC+28+VA
DTM+129:20071129:102
GIS+S:112:143
GIS+PER:110:143
MEA+WT+AAD+KGM:1200
RFF+HWB:PERIODIC NOVEMBER 2007
PAC+12++PK
TDT+20++4+++++:::PD0011
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+1101000000A:169:143
FTX+AAA+++WHEAT OR MESLIN FLOUR
LOC+27+AU
MEA+AAR++KGM:1200.000
MOA+14:1000.00:NZD
CUX+2++1.00+N
TAX+1+CUD
MOA+161:60.00
TAX+1+GST
MOA+161:125.00
UNS+S
CNT+5:1
CNT+11:12
TAX+3+CUD++1000
MOA+161:60.00
TAX+3+GST
MOA+161:125.00
TAX+4+TOT
MOA+161:185.00
GIS+C:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+39+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestSimplifiedImportAirOneLinerUsingMiscConsignorAndConsignee()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			ZGuid miscOrgPK = declaration.CachedMiscOrgPK;
			declaration.JE_OH_Importer = miscOrgPK;
			declaration.MiscImporterName = "Miscellaneous Importers Guzzle Beer with Alacrity.";
			declaration.JE_OH_Supplier = miscOrgPK;
			declaration.MiscSupplierName = "Miscellaneous Suppliers Aren't Really Much Better.";

			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");

			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(SimplifiedImportAirOneLinerUsingMiscConsignorAndConsigneeMessage, MessageBuilder.MessageTypes.Original);
		}
		#region SimplifiedImportAirOneLinerUsingMiscConsignorAndConsigneeMessage
		const string SimplifiedImportAirOneLinerUsingMiscConsignorAndConsigneeMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++11:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL++MISCELLANEOUS IMPORTERS GUZZLE BEER: WITH ALACRITY.
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU++MISCELLANEOUS SUPPLIERS AREN T REAL:LY MUCH BETTER.
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:1000
MOA+70:10
GIS+N:109:143
TAX+1+CUD
MOA+161:650.00
TAX+1+GST
MOA+161:1457.50
UNS+S
CNT+4:1
CNT+5:1
CNT+11:100
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+46+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestPeriodicBulkOilEntry()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(2745982m, "NZD", 13418m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 51848327.50m);
			decCreator.SetupImportInvoiceLine("2710190100E", "CRACKED GASOLINE", "AU", "AU", "N", 2791017.49m, 4201.500m, "TNE");
			decCreator.SetupImportInvoiceLine("2710190100E", "CRACKED GASOLINE", "AU", "AU", "N", 3805114.85m, 6573.800m, "TNE");
			decCreator.SetupImportInvoiceLine("2709000000L", "CRUDE OIL", "AU", "AU", "N", 45252195.16m, 83587.373m, "TNE");

			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 1, "VL");
			decCreator.SetupTestContainer();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2007, 2, 28);
			declaration.JE_RL_NKPortOfLoading = "VARIO";
			declaration.JE_RL_NKPortOfArrival = "NZMAP";
			declaration.JE_RL_NKProcessingPort = "NZWLG";
			declaration.JE_TotalWeight = 94719960m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_VesselName = "PERIODIC VARIOUS";
			declaration.JE_VoyageFlightNo = "1";
			declaration.JE_PaymentMethod = PaymentMethodList.Codes.CashPaidByClient;

			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 0.00m, 545169.63m);
			decCreator.MergeLineSetDutyAndTax(1, 0.00m, 750996.63m);
			decCreator.MergeLineSetDutyAndTax(2, 0.00m, 8579701.25m);

			CreateMessageAndTestResultAgainstString(MessagePeriodicBulkOilEntry, MessageBuilder.MessageTypes.Original);
		}
		#region MessagePeriodicBulkOilEntry
		const string MessagePeriodicBulkOilEntry = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++53:105:143
LOC+9+VARIO
LOC+11+NZMAP
LOC+18+1234Z
LOC+41+NZWLG
DTM+324:200702:610
MEA+WT+AAD+KGM:94719960
TDT+20+1+1+++++:::PERIODIC VARIOUS
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+2710190100E:169:143
FTX+AAA+++CRACKED GASOLINE
LOC+27+AU
LOC+35+AU
MEA+AAR++TNE:4201.500
NAD+SU+00710841Y:ZZZ:143
MOA+14:2791017.00:NZD
CUX+2++1.00
MOA+40:2791017
MOA+64:147817
MOA+70:722
GIS+N:109:143
TAX+1+GST
MOA+161:545169.63
CST+2+2710190100E:169:143
FTX+AAA+++CRACKED GASOLINE
LOC+27+AU
LOC+35+AU
MEA+AAR++TNE:6573.800
NAD+SU+00710841Y:ZZZ:143
MOA+14:3805115.00:NZD
CUX+2++1.00
MOA+40:3805115
MOA+64:201526
MOA+70:985
GIS+N:109:143
TAX+1+GST
MOA+161:750996.63
CST+3+2709000000L:169:143
FTX+AAA+++CRUDE OIL
LOC+27+AU
LOC+35+AU
MEA+AAR++TNE:83587.373
NAD+SU+00710841Y:ZZZ:143
MOA+14:45252195.00:NZD
CUX+2++1.00
MOA+40:45252195
MOA+64:2396639
MOA+70:11711
GIS+N:109:143
TAX+1+GST
MOA+161:8579701.25
UNS+S
CNT+5:3
TAX+3+CUD++51848327
MOA+161:0.00
TAX+3+GST
MOA+161:9875867.51
TAX+4+TOT
MOA+161:9875867.51
GIS+C:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+66+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestImportAirWithDeliveryAuthority()
		{
			OrgHeader deliveryAuthority = Factory.New<OrgHeader>();
			deliveryAuthority.FillWithValidTestData();
			deliveryAuthority.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "12345678A");

			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			declaration.JE_OH_NotifyParty = deliveryAuthority.PK;
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(MessageImportAirWithDeliveryAuthority, MessageBuilder.MessageTypes.Original);
		}
		#region MessageImportAirWithDeliveryAuthority
		const string MessageImportAirWithDeliveryAuthority =
@"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
NAD+DP+12345678A:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:1000
MOA+70:10
GIS+N:109:143
TAX+1+CUD
MOA+161:650.00
TAX+1+GST
MOA+161:1457.50
UNS+S
CNT+4:1
CNT+5:1
CNT+11:100
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+47+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestSimplifiedImportWithoutSupplierAndImporterCodes()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			declaration.Importer.CustomsCodes.RemoveAndDeleteAll();
			declaration.Supplier.CustomsCodes.RemoveAndDeleteAll();
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(MessageSimplifiedImportWithoutSupplierAndImporterCodes, MessageBuilder.MessageTypes.Original);
		}
		#region MessageSimplifiedImportWithoutSupplierAndImporterCodes
		const string MessageSimplifiedImportWithoutSupplierAndImporterCodes =
@"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++11:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL++ADULT BOOKS LTD
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU++TEST SUPPLIER AU
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:1000
MOA+70:10
GIS+N:109:143
TAX+1+CUD
MOA+161:650.00
TAX+1+GST
MOA+161:1457.50
UNS+S
CNT+4:1
CNT+5:1
CNT+11:100
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+46+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestSimplifiedImportWithSupplierAndImporterCodes()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(MessageSimplifiedImportWithSupplierAndImporterCodes, MessageBuilder.MessageTypes.Original);
		}
		#region MessageSimplifiedImportWithSupplierAndImporterCodes
		const string MessageSimplifiedImportWithSupplierAndImporterCodes =
@"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++11:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:1000
MOA+70:10
GIS+N:109:143
TAX+1+CUD
MOA+161:650.00
TAX+1+GST
MOA+161:1457.50
UNS+S
CNT+4:1
CNT+5:1
CNT+11:100
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+46+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestExportAirWithPartsOtherInfo()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8302100009H", "CONCEALED HINGES", "IT", 10000m, 0.00m, StatisticalUQList.Codes.Kilograms);
			decCreator.CurrentInvoiceLine.OtherInfos.AddNew(LineOtherInfoList.Codes.Parts, "");
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();

			CreateMessageAndTestResultAgainstString(MessageExportAirWithPartsOtherInfo, MessageBuilder.MessageTypes.Original);
		}
		#region MessageExportAirWithPartsOtherInfo
		const string MessageExportAirWithPartsOtherInfo = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+830+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++40:105:143
LOC+9+NZAKL
LOC+11+AUSYD
LOC+18+1234Z
LOC+41+NZAKL
LOC+28+AU
DTM+129:20040101:102
GIS+S:112:143
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+8302100009H:169:143
FTX+AAA+++CONCEALED HINGES
LOC+27+IT
MEA+AAR++KGM:0.000
MOA+14:10000.00:NZD
CUX+2++1.00+N
GIS+PTS:110:143
UNS+S
CNT+5:1
CNT+11:100
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+30+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestCompletionFromSightEntry()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(111m, "NZD", 0m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 1000m, "AU", "AU", "Q");
			decCreator.SetupImportInvoiceLine("6505900023J", "PLASTIC BITS", "", "", "", 1000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 0.00m, 100.36m);
			declaration.DeclarationNumber = "01020304";
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 0.00m, 139.37m);

			CreateMessageAndTestResultAgainstString(CompletionFromSightEntryMessage, MessageBuilder.MessageTypes.CompletionEntry);
		}
		#region CompletionFromSightEntryMessage
		const string CompletionFromSightEntryMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN+01020304
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+22
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+6505900023J:169:143
FTX+AAA+++PLASTIC BITS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:1000.00:NZD
CUX+2++1.00
MOA+40:1000
MOA+64:111
MOA+70:0
GIS+N:109:143
TAX+1+GST
MOA+161:139.37
GIS+Q:116:143
UNS+S
CNT+4:1
CNT+5:1
CNT+11:100
TAX+3+CUD++1000
MOA+161:0.00
TAX+3+GST
MOA+161:139.37
TAX+4+TOT
MOA+161:139.37
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+45+<<MSGNO PLACEHOLDER>>";
		#endregion

		public void TestCompletionFromTemporaryImport()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			declaration.DeclarationNumber = "01020304";
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			decCreator.MergeDeclaration();

			CreateMessageAndTestResultAgainstString(CompletionFromTemporaryImportMessage, MessageBuilder.MessageTypes.CompletionEntry);
		}
		#region CompletionFromTemporaryImportMessage
		const string CompletionFromTemporaryImportMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN+01020304
BGM+830+<<SENDERS REFERENCE PLACE HOLDER>>+22
CST++40:105:143
LOC+9+NZAKL
LOC+11+AUSYD
LOC+18+1234Z
LOC+41+NZAKL
LOC+28+AU
DTM+129:20040101:102
GIS+S:112:143
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
MOA+14:10000.00:NZD
CUX+2++1.00+N
UNS+S
CNT+5:1
CNT+11:100
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+28+<<MSGNO PLACEHOLDER>>";
		#endregion

		public void TestPeriodicImportSeafreight()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2003, 12, 1);
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(PeriodicImportSeafreightMessage, MessageBuilder.MessageTypes.Original);
		}
		#region PeriodicImportSeafreightMessage
		const string PeriodicImportSeafreightMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++53:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+324:200312:610
MEA+WT+AAD+KGM:1000
TDT+20+109+1+++++:::BUNGA BIDARA
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:1000
MOA+70:10
GIS+N:109:143
TAX+1+CUD
MOA+161:650.00
TAX+1+GST
MOA+161:1457.50
UNS+S
CNT+5:1
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+39+<<MSGNO PLACEHOLDER>>";
		#endregion

		public void TestMessageSendsDefaultCountryOfExportSlashOriginAndQualForPrefDutyFromDefaultsOnInvoiceHeader()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(111m, "NZD", 0m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 1000m, "AU", "AU", "Q");
			decCreator.SetupImportInvoiceLine("6505900023J", "PLASTIC BITS", "", "", "", 1000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 0.00m, 139.37m);

			CreateMessageAndTestResultAgainstString(ImportAirOneLinerQualifyingMessage, MessageBuilder.MessageTypes.Original);
		}

		public void TestBlankPermitOtherInfoAndProhibitedCodesDontGetSent()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			JobComInvoiceLine invoiceLine = declaration.FilteredInvoiceLines[0];

			PermitCode declarationPermitCode = declaration.PermitCodes.AddNew();
			OtherInfo declarationOtherInfo = declaration.OtherInfos.AddNew();

			PermitCode linePermitCode = invoiceLine.PermitCodes.AddNew();
			ProhibitedCode lineProhibitedCode = invoiceLine.ProhibitedCodes.AddNew();
			OtherInfo lineOtherInfo = invoiceLine.OtherInfos.AddNew();

			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(ImportAirOneLinerMessage, MessageBuilder.MessageTypes.Original);
		}

		public void TestSimplifiedImportAirOneLinerWithLongCompanyName()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			OrgHeader simplifiedImporter = OrgHeader.New(Factory);
			simplifiedImporter.FillWithValidTestData();
			simplifiedImporter.OH_FullName = "123456789 123456789 123456789 123456789 ";
			declaration.JE_OH_Importer = simplifiedImporter.PK;
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(SimplifiedImportAirOneLinerWithLongCompanyNameMessage, MessageBuilder.MessageTypes.Original);
		}
		#region SimplifiedImportAirOneLinerWithLongCompanyNameMessage
		const string SimplifiedImportAirOneLinerWithLongCompanyNameMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++11:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL++123456789 123456789 123456789 12345:6789
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:1000
MOA+70:10
GIS+N:109:143
TAX+1+CUD
MOA+161:650.00
TAX+1+GST
MOA+161:1457.50
UNS+S
CNT+4:1
CNT+5:1
CNT+11:100
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+46+<<MSGNO PLACEHOLDER>>
";
		#endregion

		[ExpectNoExceptions()]
		public override void TestEmptyCusEntryHeaderThrowsNoExceptions()
		{
			messageBuilder = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original);
			string generatedMessage = messageBuilder.GetMessageText();
		}

		public void TestCheckBrokerPassword()
		{
			messageBuilder = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original);
			Assert("Password Should Validate", messageBuilder.IsPasswordOk(CurrentUsersPin.TestSystemPinCode));
			Assert("Password Should Not Validate", !messageBuilder.IsPasswordOk("WRONGPIN"));
		}

		[TestDate(2024, 3, 1)]
		public void TestDeclarationWithFutureEDITransmitDateGetsQueuedInsteadOfBeingSentStraightAway()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.NewZealand))
			{
				SetupForLiveMessagePostingTest();
				var queuedDate = new ZDateTime(2024, 3, 21);
				declaration.JE_EDITransmitDate = queuedDate;
				messageBuilder = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original);
				messageBuilder.GenerateMessage();
				var message = messageBuilder.MessageBusinessObject;
				AssertEquals("Message is queued in UTC date", queuedDate.ToUniversalBranchTime(Factory).AddMinutes(15), message.EM_HeldUntilDate);
				AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.QueuedForSending, declaration.JE_EntryStatus);
				AssertEquals("Declaration.CusEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.QueuedForSending, declaration.CusEntryHeader.CH_EntryStatus);
			}
		}

		public void TestDeclarationWithPastEDITransmitDateGetsSentStraightAway()
		{
			SetupForLiveMessagePostingTest();
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate.AddDays(-1);
			messageBuilder = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original);
			messageBuilder.GenerateMessage();
			EDIMessage message = messageBuilder.MessageBusinessObject;
			AssertNotNull("MessageBuilder.Message", message);
			AssertEquals("Message.EM_HeldUntilDate", ZDateTime.Empty, message.EM_HeldUntilDate);
		}

		public void TestDeclarationWithTodaysEDITransmitDateGetsSentStraightAway()
		{
			SetupForLiveMessagePostingTest();
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate;
			messageBuilder = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original);
			messageBuilder.GenerateMessage();
			EDIMessage message = messageBuilder.MessageBusinessObject;
			AssertNotNull("MessageBuilder.Message", message);
			AssertEquals("Message.EM_HeldUntilDate", ZDateTime.Empty, message.EM_HeldUntilDate);
		}

		public void TestImportAirOneLiner()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(ImportAirOneLinerMessage, MessageBuilder.MessageTypes.Original);
		}
		#region ImportAirOneLinerMessage
		const string ImportAirOneLinerMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:1000
MOA+70:10
GIS+N:109:143
TAX+1+CUD
MOA+161:650.00
TAX+1+GST
MOA+161:1457.50
UNS+S
CNT+4:1
CNT+5:1
CNT+11:100
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+46+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestImportAirOneLinerWith7DigitSupplierCode()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.OSParty.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "710841Y");
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(ImportAirOneLinerMessage, MessageBuilder.MessageTypes.Original);
		}

		public void TestImportAirOneLinerWith7DigitClientCode()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.LocalParty.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode, "782903F");
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(ImportAirOneLinerMessage, MessageBuilder.MessageTypes.Original);
		}

		public void TestImportAirCancelEntry()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(ImportAirCancelEntryMessage, MessageBuilder.MessageTypes.CancelEntry);
		}
		#region ImportAirCancelEntryMessage
		const string ImportAirCancelEntryMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+1
UNS+D
UNS+S
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+6+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestImportAirCancelLine()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
			decCreator.Declaration.CusEntryHeader.CH_LastNumberOfLinesSentToCustoms = 2;

			CreateMessageAndTestResultAgainstString(ImportAirCancelLineMessage, MessageBuilder.MessageTypes.CancelLine);
		}
		#region ImportAirCancelLineMessage
		const string ImportAirCancelLineMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+3
UNS+D
CST+2+:169:143
UNS+S
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+14+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestImportAirAddLine()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 325.00m, 728.75m);
			decCreator.MergeLineSetDutyAndTax(1, 325.00m, 728.75m);
			decCreator.Declaration.CusEntryHeader.CH_LastNumberOfLinesSentToCustoms = 1;

			CreateMessageAndTestResultAgainstString(ImportAirAddLineMessage, MessageBuilder.MessageTypes.AddLine);
		}
		#region ImportAirAddLineMessage
		const string ImportAirAddLineMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+2
UNS+D
CST+2+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:5000.00:NZD
CUX+2++1.00
MOA+40:5000
MOA+64:500
MOA+70:5
GIS+N:109:143
TAX+1+CUD
MOA+161:325.00
TAX+1+GST
MOA+161:728.75
UNS+S
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+28+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestImportAirOneLinerReplaceLines()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
			decCreator.Declaration.CusEntryHeader.CH_LastNumberOfLinesSentToCustoms = 1;

			CreateMessageAndTestResultAgainstString(ImportAirOneLinerReplaceLinesMessage, MessageBuilder.MessageTypes.ReplaceLines);
		}
		#region ImportAirOneLinerReplaceLinesMessage
		const string ImportAirOneLinerReplaceLinesMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+21
UNS+D
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:1000
MOA+70:10
GIS+N:109:143
TAX+1+CUD
MOA+161:650.00
TAX+1+GST
MOA+161:1457.50
UNS+S
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+28+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestImportAirOneLinerReplaceHeader()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(ImportAirOneLinerReplaceHeaderMessage, MessageBuilder.MessageTypes.ReplaceHeader);
		}
		#region ImportAirOneLinerReplaceHeaderMessage
		const string ImportAirOneLinerReplaceHeaderMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+20
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
UNS+S
CNT+4:1
CNT+11:100
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+23+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestImportAirOneLinerReplaceHeaderAndLines()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
			decCreator.Declaration.CusEntryHeader.CH_LastNumberOfLinesSentToCustoms = 1;

			CreateMessageAndTestResultAgainstString(ImportAirOneLinerReplaceHeaderMessage, MessageBuilder.MessageTypes.ReplaceHeader);
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			decCreator.MergeDeclaration();
			Factory.Save();
			CreateMessageAndTestResultAgainstString(ImportAirOneLinerReplaceLinesMessage, MessageBuilder.MessageTypes.ReplaceLines);
		}

		public void TestImportAirOneLinerQualifying()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(111m, "NZD", 0m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 1000m);
			decCreator.SetupImportInvoiceLine("6505900023J", "PLASTIC BITS", "AU", "AU", "Q", 1000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 0.00m, 139.37m);

			CreateMessageAndTestResultAgainstString(ImportAirOneLinerQualifyingMessage, MessageBuilder.MessageTypes.Original);
		}
		#region ImportAirOneLinerQualifyingMessage
		const string ImportAirOneLinerQualifyingMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+6505900023J:169:143
FTX+AAA+++PLASTIC BITS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:1000.00:NZD
CUX+2++1.00
MOA+40:1000
MOA+64:111
MOA+70:0
GIS+N:109:143
TAX+1+GST
MOA+161:139.37
GIS+Q:116:143
UNS+S
CNT+4:1
CNT+5:1
CNT+11:100
TAX+3+CUD++1000
MOA+161:0.00
TAX+3+GST
MOA+161:139.37
TAX+4+TOT
MOA+161:139.37
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+45+<<MSGNO PLACEHOLDER>>
";
		#endregion

		[ExpectNoExceptions]
		public void TestImportSeaWithNoVessel()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.Declaration.JE_VesselName = "";
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
			CreateMessageAndTestResultAgainstString(ImportSeaWithNoVesselMessage, MessageBuilder.MessageTypes.Original);
		}
		#region ImportSeaWithNoVesselMessage
		const string ImportSeaWithNoVesselMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+BM:OBL123456
PAC+0++PK
RFF+BM:HOUSEBILL1
PAC+100++PK
TDT+20+109+1
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:1000
MOA+70:10
GIS+N:109:143
TAX+1+CUD
MOA+161:650.00
TAX+1+GST
MOA+161:1457.50
UNS+S
CNT+4:1
CNT+5:1
CNT+11:100
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+47+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestImportSeaOneLiner()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			CreateMessageAndTestResultAgainstString(ImportSeaOneLinerMessage, MessageBuilder.MessageTypes.Original);
		}
		#region ImportSeaOneLinerMessage
		const string ImportSeaOneLinerMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20040101:102
MEA+WT+AAD+KGM:1000
RFF+BM:OBL123456
PAC+0++PK
RFF+BM:HOUSEBILL1
PAC+100++PK
TDT+20+109+1+++++:::BUNGA BIDARA
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
LOC+35+AU
NAD+SU+00710841Y:ZZZ:143
MOA+14:10000.00:NZD
CUX+2++1.00
MOA+40:10000
MOA+64:1000
MOA+70:10
GIS+N:109:143
TAX+1+CUD
MOA+161:650.00
TAX+1+GST
MOA+161:1457.50
UNS+S
CNT+4:1
CNT+5:1
CNT+11:100
TAX+3+CUD++10000
MOA+161:650.00
TAX+3+GST
MOA+161:1457.50
TAX+4+TOT
MOA+161:2107.50
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+47+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestExportAirOneLiner()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();

			CreateMessageAndTestResultAgainstString(ExportAirOneLinerMessage, MessageBuilder.MessageTypes.Original);
		}
		#region ExportAirOneLinerMessage
		const string ExportAirOneLinerMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+830+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++40:105:143
LOC+9+NZAKL
LOC+11+AUSYD
LOC+18+1234Z
LOC+41+NZAKL
LOC+28+AU
DTM+129:20040101:102
GIS+S:112:143
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
MOA+14:10000.00:NZD
CUX+2++1.00+N
UNS+S
CNT+5:1
CNT+11:100
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+28+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestDrawbackAirOneLiner()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForExportToAU();
			decCreator.Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("30938", "FOB", "NZD", 16647m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("6110200101K", "SWEATSHIRTS OF COTTON", "US", 16647.00m, 1550m, "NMB", 5187.34m, 0m);
			decCreator.SetupExportInvoiceLine("6110200101K", "SWEATSHIRTS OF COTTON", "HN", 5079.90m, 615m, "NMB");
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();

			CreateMessageAndTestResultAgainstString(DrawbackAirOneLinerMessage, MessageBuilder.MessageTypes.Original);
		}
		#region DrawbackAirOneLinerMessage
		const string DrawbackAirOneLinerMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+830+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++41:105:143
LOC+9+NZAKL
LOC+11+AUSYD
LOC+18+1234Z
LOC+41+NZAKL
LOC+28+AU
DTM+129:20040101:102
GIS+S:112:143
MEA+WT+AAD+KGM:1000
RFF+MB:08111111111
RFF+HWB:HOUSEBILL1
PAC+100++PK
TDT+20++4+++++:::QF117
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+6110200101K:169:143
FTX+AAA+++SWEATSHIRTS OF COTTON
LOC+27+US
MEA+AAR++NMB:1550.000
MOA+14:16647.00:NZD
CUX+2++1.00+N
TAX+1+CUD
MOA+161:5187.34
CST+2+6110200101K:169:143
FTX+AAA+++SWEATSHIRTS OF COTTON
LOC+27+HN
MEA+AAR++NMB:615.000
MOA+14:5079.90:NZD
CUX+2++1.00+N
UNS+S
CNT+5:2
CNT+11:100
TAX+3+CUD++21727
MOA+161:5187.34
TAX+4+TOT
MOA+161:5187.34
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+42+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestExportSeaOneLiner()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();

			CreateMessageAndTestResultAgainstString(ExportSeaOneLinerMessage, MessageBuilder.MessageTypes.Original);
		}
		#region ExportSeaOneLinerMessage
		const string ExportSeaOneLinerMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+830+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++40:105:143
LOC+9+NZAKL
LOC+11+AUSYD
LOC+18+1234Z
LOC+41+NZAKL
LOC+28+AU
DTM+129:20040101:102
GIS+S:112:143
MEA+WT+AAD+KGM:1000
RFF+BM:OBL123456
PAC+0++PK
RFF+BM:HOUSEBILL1
PAC+100++PK
TDT+20+109+1+++++:::BUNGA BIDARA
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
MOA+14:10000.00:NZD
CUX+2++1.00+N
UNS+S
CNT+5:1
CNT+11:100
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+29+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestExportSeaWithOnePackedContainer()
		{
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly("HOUSEBILL1", "OOCL0000006", "FCL", 100, "PK");
			decCreator.MergeDeclaration();

			CreateMessageAndTestResultAgainstString(ExportSeaWithOnePackedContainerMessage, MessageBuilder.MessageTypes.Original);
		}
		#region ExportSeaWithOnePackedContainerMessage
		const string ExportSeaWithOnePackedContainerMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+830+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++40:105:143
LOC+9+NZAKL
LOC+11+AUSYD
LOC+18+1234Z
LOC+41+NZAKL
LOC+28+AU
DTM+129:20040101:102
GIS+S:112:143
MEA+WT+AAD+KGM:1000
EQD+CN+OOCL0000006++++5
RFF+BM:OBL123456
PAC+0++PK
RFF+BM:HOUSEBILL1
RFF+AAQ:OOCL0000006
PAC+100++PK
TDT+20+109+1+++++:::BUNGA BIDARA
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
MOA+14:10000.00:NZD
CUX+2++1.00+N
UNS+S
CNT+5:1
CNT+11:100
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+31+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestStatisticalAndSupplementaryQuantitiesRoundedInsteadOfTruncated()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeList.Codes.Completion;
			declaration.JE_DateOfArrival = new ZDateTime(2008, 9, 30, 23, 59, 59);
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			invoiceLine.JI_Tariff = "2710.19.11.11F";
			invoiceLine.JI_LinePrice = 1000.00m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CustomsQuantity = 1.1595m;
			invoiceLine.JI_SupplementaryUQ = "KGM";
			invoiceLine.JI_SupplementaryQty = 1.0595m;

			declaration.MergeAndSaveIfNotMergedAlreadyReturningErrors();
			CreateMessageAndTestResultAgainstString(StatisticalAndSupplementaryQuantitiesRoundedInsteadOfTruncated, MessageBuilder.MessageTypes.Original);
		}
		#region StatisticalAndSupplementaryQuantitiesRoundedInsteadOfTruncated
		const string StatisticalAndSupplementaryQuantitiesRoundedInsteadOfTruncated = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++10:105:143
DTM+151:20080930:102
MEA+WT+AAD+KGM:0
TDT+20++5
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS++935
TOD+++:106:143
CST+1+2710191111F:169:143
FTX+AAA+++MOTOR SPIRIT IN BULK ETC, WITH RON LESS THAN 92
LOC+27+KR
MEA+AAR++KGM:1.160
MEA+AAS++KGM:1.060
NAD+SU+000000000:ZZZ:143
MOA+14:1000.00:NZD
CUX+2++1.00
MOA+40:1000
MOA+64:0
MOA+70:0
GIS+:109:143
TAX+1+CUD
MOA+161:0.57
TAX+1+GST
MOA+161:125.07
UNS+S
CNT+4:1
CNT+5:1
CNT+11:0
TAX+3+CUD++1000
MOA+161:0.57
TAX+3+GST
MOA+161:125.07
TAX+4+TOT
MOA+161:125.64
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+39+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestExportSeaWithOnePackedContainerWithShortClientID()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009917B");

			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly("HOUSEBILL1", "OOCL0000006", "FCL", 100, "PK");
			decCreator.MergeDeclaration();

			CreateMessageAndTestResultAgainstString(ExportSeaWithOnePackedContainerMessage, MessageBuilder.MessageTypes.Original);
		}

		public override void TestGenerateTestMessage()
		{
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);

			messageBuilder = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("0", messageBuilder.PostedMessageNumber);
			messageBuilder.GenerateMessage();
			Assert(messageBuilder.PostedMessageNumber != "0");

			EDIMessage eDIMessageBusObj = messageBuilder.MessageBusinessObject;
			AssertNotNull(eDIMessageBusObj);
			AssertEquals(false, eDIMessageBusObj.IsInDatabase);
			eDIMessageBusObj.Factory.Save();
			AssertEquals("SendersReferencePlaceHolder was not removed.", -1, eDIMessageBusObj.EM_MessageText.IndexOf(NZCMessage.SendersReferencePlaceHolder));
			AssertEquals("MessageNumberPlaceHolder was not removed.", -1, eDIMessageBusObj.EM_MessageText.IndexOf(NZCMessage.MessageNumberPlaceHolder));
			AssertEquals("ContainedChecksumPlaceHolder was not removed.", -1, eDIMessageBusObj.EM_MessageText.IndexOf(NZCMessage.ContainedChecksumPlaceHolder));
			AssertEquals("EM_Status should be Queued", NZCMessage.Status.Queued, eDIMessageBusObj.EM_Status);
			AssertEquals("EM_TestMessage Incorrectly Set", true, eDIMessageBusObj.EM_IsTestMessage);
			AssertEquals(FormalEntryStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("EM_MessageSubType Incorrectly Set", NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.Original, eDIMessageBusObj.EM_MessageSubType);
			AssertEquals("Last Number of Lines sent to Customs", 1, decCreator.Declaration.CusEntryHeader.CH_LastNumberOfLinesSentToCustoms);
		}

		public void TestOrganisationRegistrationSubmittedCorrectly()
		{
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			var usersPin = new CurrentUsersPin(Factory, false);
			var wrapper = staff.GetNZWrapper();
			var savePin = wrapper.NZBPassword.GP_CurrentPassword;
			wrapper.NZBPassword.GP_UserID = "THISISNOTADRILL";
			usersPin.DecryptedPinCode = "NeverUseThisPIN";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			declaration.JE_DeclarationReference = "BSES002932";
			declaration.JE_TransportMode = "AIR";
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "BOB THE BUILDER";
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, "8323CSC", Core.Constants.CountryCodes.NewZealand);
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientCode, "34334CCD", Core.Constants.CountryCodes.NewZealand);
			declaration.JE_OH_Supplier = supplier.PK;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "BOB THE BUILDER";
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, "47232CSC", Core.Constants.CountryCodes.NewZealand);
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientCode, "47723CCD", Core.Constants.CountryCodes.NewZealand);
			declaration.JE_OH_Importer = importer.PK;
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "JACK THE PEACEMAKER";
			shippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, "9544CSC", Core.Constants.CountryCodes.NewZealand);
			shippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CustomsClientCode, "2342CCD", Core.Constants.CountryCodes.NewZealand);
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.NotifyPartyDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.NewZealand;
			invoice.JZ_InvoiceDate = new ZDateTime(2013, 2, 22);

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3307.30.00.00E";
			invoiceLine.JI_LinePrice = 10000m;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.CustomsEntryHeaders[0];
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_OH_NotifyParty = importer.PK;
			declaration.ResetToOriginal();
			Factory.Save();

			TestHelper.SetupMessagingEnvironment();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryHeader = declaration.CustomsEntryHeaders[0];
			new MessageManager(declaration, MessageBuilders.MessageManager.OperationType.SubmitMessage).Execute();
			entryHeader.Messages.Load();
			var messageText = entryHeader.Messages[0].EM_FormattedMessageText;
			AssertContains("TSW Message", @"  <Exporter>
    <ID>034334CCD</ID>
  </Exporter>", messageText);
			AssertContains("TSW Message", @"    <NotifyParty>
      <Name>BOB THE BUILDER</Name>
      <RoleCode>N2</RoleCode>
      <Communication />
    </NotifyParty>", messageText);

			entryHeader.Delete();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_OH_NotifyParty = importer.PK;
			declaration.ResetToOriginal();
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryHeader = declaration.CustomsEntryHeaders[0];
			new MessageManager(declaration, MessageBuilders.MessageManager.OperationType.SubmitMessage).Execute();
			entryHeader.Messages.Load();
			messageText = entryHeader.Messages[0].EM_FormattedMessageText;
			AssertContains("TSW Message", @"    <Supplier>
      <ID>008323CSC</ID>
    </Supplier>", messageText);
			AssertContains("TSW Message", @"    <NotifyParty>
      <Name>BOB THE BUILDER</Name>
      <RoleCode>N2</RoleCode>
      <Communication />
    </NotifyParty>", messageText);
			AssertContains("TSW Message", @"  <Importer>
    <ID>047723CCD</ID>
  </Importer>", messageText);
		}

		public override void TestGenerateLiveMessage()
		{
			SetupForLiveMessagePostingTest();
			messageBuilder = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original);
			string generatedMessage = messageBuilder.GetMessageText();
			AssertEquals("0", messageBuilder.PostedMessageNumber);
			messageBuilder.GenerateMessage();
			Assert(messageBuilder.PostedMessageNumber != "0");

			EDIMessage eDIMessageBusObj = messageBuilder.MessageBusinessObject;
			AssertNotNull(eDIMessageBusObj);
			AssertEquals(false, eDIMessageBusObj.IsInDatabase);
			eDIMessageBusObj.Factory.Save();
			AssertEquals("SendersReferencePlaceHolder was not removed.", -1, eDIMessageBusObj.EM_MessageText.IndexOf(NZCMessage.SendersReferencePlaceHolder));
			AssertEquals("MessageNumberPlaceHolder was not removed.", -1, eDIMessageBusObj.EM_MessageText.IndexOf(NZCMessage.MessageNumberPlaceHolder));
			AssertEquals("ContainedChecksumPlaceHolder was not removed.", -1, eDIMessageBusObj.EM_MessageText.IndexOf(NZCMessage.ContainedChecksumPlaceHolder));
			AssertEquals("EM_Status should be Queued", NZCMessage.Status.Queued, eDIMessageBusObj.EM_Status);
			AssertEquals("EM_TestMessage Incorrectly Set", false, eDIMessageBusObj.EM_IsTestMessage);
			AssertEquals(FormalEntryStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("EM_MessageSubType Incorrectly Set", NZCMessage.MessageTypes.FormalEntry.MessageSubTypes.Original, eDIMessageBusObj.EM_MessageSubType);
			AssertEquals("Last Number of Lines sent to Customs", 1, decCreator.Declaration.CusEntryHeader.CH_LastNumberOfLinesSentToCustoms);
		}

		[TestDate(2008, 10, 15)]
		public void TestGenerateMessageWithSGGLevy()
		{
			TaxOrFeeTestHelper.SetUp(Factory);

			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForSea();
			decCreator.SetupTestForImportFromAU();
			declaration.JE_DateOfArrival = new ZDateTime(2012, 10, 15);
			declaration.JE_EDITransmitDate = new ZDateTime(2012, 10, 15);
			declaration.JE_TotalWeight = 1000000m;
			decCreator.SetupInvoiceGroup(10000m, "NZD", 100m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 1000000m);

			var tariff = NZCClassificationTest.CreateSGGTariff(Factory);
			decCreator.SetupImportInvoiceLine(tariff.U0_Tariff, tariff.U0_Description, "AU", "AU", "N", 1000000m, 1000000, "NMB");
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 1, "VL");

			decCreator.MergeDeclaration();

			CreateMessageAndTestResultAgainstString(SGGMessage, MessageBuilder.MessageTypes.Original);
		}

		#region SGGMessage

		const string SGGMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+929+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++10:105:143
LOC+9+AUSYD
LOC+11+NZAKL
LOC+18+1234Z
LOC+41+NZAKL
DTM+151:20121015:102
MEA+WT+AAD+KGM:1000000
RFF+BM:OBL123456
PAC+0++PK
RFF+BM:HOUSEBILL1
PAC+1++VL
TDT+20+109+1+++++:::BUNGA BIDARA
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
DMS+1001001+935
TOD+++FOB:106:143
CST+1+0000000000E:169:143
FTX+AAA+++GREENHOUSE GAS, AKA BEN ATE BEANS
LOC+27+AU
LOC+35+AU
MEA+AAR++NMB:1000000.000
NAD+SU+00710841Y:ZZZ:143
MOA+14:1000000.00:NZD
CUX+2++1.00
MOA+40:1000000
MOA+64:10000
MOA+70:100
GIS+N:109:143
TAX+1+CST+SGG:167:143
MOA+161:8550000.00
TAX+1+GST
MOA+161:1434015.00
UNS+S
CNT+4:1
CNT+5:1
CNT+11:1
TAX+3+CST+SGG:167:143
MOA+161:8550000.00
TAX+3+CUD++1000000
MOA+161:0.00
TAX+3+GST
MOA+161:1434015.00
TAX+4+TOT
MOA+161:9984015.00
GIS+B:134:143
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+50+<<MSGNO PLACEHOLDER>>
";
		#endregion

		public void TestGenerateMessageWithSealNumber()
		{
			decCreator.SetupTestForSea();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupExportInvoiceHeader("1001001", "FOB", "NZD", 10000m, ExchangeRateIndicatorList.Codes.NZD);
			decCreator.SetupExportInvoiceLine("8301100000F", "PADLOCKS", "AU", 10000m);
			decCreator.AddHouseBillAndContainerWithPackingDetailsAgainstContainerOnly("HOUSEBILL1", "OOCL0000006", "FCL", 100, "PK");
			var container = declaration.CusContainers[0];
			container.CO_Seal = "SEAL";

			decCreator.MergeDeclaration();
			CreateMessageAndTestResultAgainstString(ExportSealNumberWithOutSEPMessage, MessageBuilder.MessageTypes.Original);

			declaration.OtherInfos.AddNew(HeaderOtherInfoList.Codes.SecureExportPartnership, "328979879342");
			CreateMessageAndTestResultAgainstString(ExportSealNumberWithSEPMessage, MessageBuilder.MessageTypes.Original);
		}

		#region SealNumberMessage

		const string ExportSealNumberWithOutSEPMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+830+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++40:105:143
LOC+9+NZAKL
LOC+11+AUSYD
LOC+41+NZAKL
LOC+28+AU
DTM+129:20040101:102
GIS+S:112:143
MEA+WT+AAD+KGM:0
EQD+CN+OOCL0000006++++5
RFF+BM:OBL123456
PAC+0++PK
RFF+BM:HOUSEBILL1
RFF+AAQ:OOCL0000006
PAC+100++PK
TDT+20+109+1+++++:::BUNGA BIDARA
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
MOA+14:10000.00:NZD
CUX+2++1.00+N
UNS+S
CNT+5:1
CNT+11:100
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+30+<<MSGNO PLACEHOLDER>>";

		const string ExportSealNumberWithSEPMessage = @"UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:96B:UN
BGM+830+<<SENDERS REFERENCE PLACE HOLDER>>+9
CST++40:105:143
LOC+9+NZAKL
LOC+11+AUSYD
LOC+41+NZAKL
LOC+28+AU
DTM+129:20040101:102
GIS+S:112:143
GIS+SEP:110:143:328979879342
MEA+WT+AAD+KGM:0
EQD+CN+OOCL0000006++++5
SEL+SEAL
RFF+BM:OBL123456
PAC+0++PK
RFF+BM:HOUSEBILL1
RFF+AAQ:OOCL0000006
PAC+100++PK
TDT+20+109+1+++++:::BUNGA BIDARA
NAD+AL+00782903F:ZZZ:143
NAD+CB+00009917B:ZZZ:143
UNS+D
CST+1+8301100000F:169:143
FTX+AAA+++PADLOCKS
LOC+27+AU
MOA+14:10000.00:NZD
CUX+2++1.00+N
UNS+S
CNT+5:1
CNT+11:100
AUT+<<CONTAINED CHECKSUM PLACE HOLDER>>+65432198B
UNT+32+<<MSGNO PLACEHOLDER>>

";

		#endregion

		public void TestGenerateSegmentFTX()
		{
			var messageBuilder = new MessageBuilderForTesting(declaration.CusEntryHeader, MessageBuilder.MessageTypes.None);
			var ftx = messageBuilder.Get_FTXSection();

			string ftxMessage = @"LINE 1
				Line 2
				Line 3
				Line 4
				Line 5";

			messageBuilder.Call_GenerateSegmentFTX(ftx, TextSubjectQualifierList.GeneralInformation, ftxMessage);

			var segment = ftx[0];
			AssertEquals("Should be uppercase on the first line", "LINE 1LINE 2LINE 3LINE 4LINE 5", segment.TextLiteral.FreeText1);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		new MessageBuilder messageBuilder
		{
			get { return (MessageBuilder)base.messageBuilder; }
			set { base.messageBuilder = value; }
		}

		new TestFormalEntryCreator decCreator
		{
			get { return (TestFormalEntryCreator)base.decCreator; }
			set { base.decCreator = value; }
		}

		protected override void SetupJobDeclarationAndTestCreator()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			decCreator = new TestFormalEntryCreator(declaration);
		}

		void CreateMessageAndTestResultAgainstString(string expectedMessageText, MessageBuilder.MessageTypes messageType)
		{
			messageBuilder = new MessageBuilder(declaration.CusEntryHeader, messageType);
			TestMessageBuilderResultAgainstString(expectedMessageText);
		}

		void SetupForLiveMessagePostingTest()
		{
			NZCustomsDataRegistry.Instance.ExportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("8301100000F", "PADLOCKS", "AU", "AU", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
		}
		#endregion
	}
}
