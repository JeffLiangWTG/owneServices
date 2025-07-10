using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACEEntryMessage7501Print))]
	sealed class ACEEntryMessage7501PrintTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSeparateTariffDescriptionBetweenLines()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Declaration.US_IsInvoiceByRequest = true;
			var invoice = Declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			var invoiceLine4 = invoice.InvoiceLines.AddNew();

			invoiceLine1.JI_Description = "AVERYLONGDESCRIPTIONAVERYLONGDESCRIPTIONAVERYLONGDESCRIPTIONAVERYLONGDESCRIPTIONAVERYLONGDESCRIPTIONAVERYLONGDESCRIPTIONAVERYLONGDESCRIPTIONAVERYLONGDESCRIPTION";
			invoiceLine3.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine4.JI_Tariff = "8703330045";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Declaration.ActiveEntryHeaders.EntrySummaryEntry.CreateDocPrintingDetails(message.PK);
			Factory.Save();

			var eNSCusEntryHeader = Declaration.FormalEntry != null ? Declaration.FormalEntry.PK : ZGuid.Empty;
			var query = new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.US7501DocPrinting);
			query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, CusEntryHeaderSchema.Constants.Prefix);
			query.AddToFilter(CusAddInfoSchema.B7_ParentID, eNSCusEntryHeader);
			var docData = Factory.LoadTop1<US7501DocPrinting>(query);

			var rateStrings = new ZString[8];

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("There are 4 lines", 4, printBO.LineObjectCollection.Count);
			var line1 = new ACEEntryMessage7501Line(printBO.LineObjectCollection[0], false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("Line1's description", ZString.Empty, line1.Description);
			var line2 = new ACEEntryMessage7501Line(printBO.LineObjectCollection[1], false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("Line2's description", ZString.Empty, line2.Description);
			var line3 = new ACEEntryMessage7501Line(printBO.LineObjectCollection[2], false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("Line3's description", ZString.Empty, line3.Description);
			var line4 = new ACEEntryMessage7501Line(printBO.LineObjectCollection[3], false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("Line4's description should come from tariff", "OTHER, NEW:CYL>2500CC;DIES", line4.Description);
		}

		public void TestTotalDutyAmountForEntryType21()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			invoiceLine1.CusEntryLine.Fees.AddOrUpdate("DTY", 100m);

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Declaration.ActiveEntryHeaders.EntrySummaryEntry.CreateDocPrintingDetails(message.PK);
			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			var aens90 = (AENS90)message.MessageBlock.MessageBlocks.FirstOrDefault(x => x is AENS90);
			AssertEquals(100m, aens90.GrandTotalDutyAmount);
			AssertEquals("DutyAmount", 0m, printBO.TotalDutyAmt);
		}

		[TestDate(2021, 06, 06)]
		public void TestACEEntryMessage7501Print()
		{
			var expirationDateType = Factory.New<RefSysConfigType>();
			expirationDateType.ZRT_ConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.CBP7501ED;
			expirationDateType.ZRT_Description = "Expiration Statement to print on CBP Form 7501";
			expirationDateType.ZRT_LongDescription = "Expiration Statement to print on CBP Form 7501";
			var expirationDate = Factory.New<RefSysConfig>();
			expirationDate.ZRC_ZRT_NKConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.CBP7501ED;
			expirationDate.ZRC_DecimalValue = 0;
			expirationDate.ZRC_StringValue = "EXPIRATION DATE 01/31/2021";
			expirationDate.ZRC_StartDate = new ZDateTime(2021, 01, 31);
			expirationDate.ZRC_EndDate = new ZDateTime(2079, 06, 06);

			var revisionDateType = Factory.New<RefSysConfigType>();
			revisionDateType.ZRT_ConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.CBP7501RD;
			revisionDateType.ZRT_Description = "Revision Statement to print on CBP Form 7501";
			revisionDateType.ZRT_LongDescription = "Revision Statement to print on CBP Form 7501";
			var revisionDate = Factory.New<RefSysConfig>();
			revisionDate.ZRC_ZRT_NKConfigCode = UniversalReferenceConstants.RefSysConfig.Codes.CBP7501RD;
			revisionDate.ZRC_DecimalValue = 0;
			revisionDate.ZRC_StringValue = "(12/19)";
			revisionDate.ZRC_StartDate = new ZDateTime(2019, 12, 01);
			revisionDate.ZRC_EndDate = new ZDateTime(2079, 06, 06);
			Factory.Save();

			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var expectedEntryDate = new ZDate(2009, 06, 05);
			Declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			Factory.Save();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8703330045";
			invoiceLine1.US_SupTariff = "98178501";
			invoiceLine1.JI_LinePrice = 48000m;
			Declaration.JE_EntryAuthorisationDate = expectedEntryDate;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var outMsg = message;
			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              " +
"10ASV9  10002333 3902            0110 X           2020711                       " +
"1191-01319900091-01319900091-013199001                      IL                  " +
"20AAAA3902      N598APL EMERALD                                                 " +
"2200000001PK                                                                    " +
"23MAAAAOBLACE69                                                                 " +
"318B 891                                                                        " +
"40  001 CAAU012411        0000000050602670000009000    N                        " +
"44COMMERCIAL DESCRIPTION                                                        " +
"47MTHLIATHA191NAK                                                               " +
"47C91-013199000                                                                 " +
"47S91-013199000                                                                 " +
"5098178501   0000000000 0000500000                                              " +
"508703330045 0000000000 0000000000             NO                               " +
"6250100062500                                                                   " +
"8950100000062500                                                                " +
"9000000000000 00000062500 00000000000 00000000000 00000000000                   " +
"Y  3902SV9AE";

			var printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);

			AssertEquals("ExpirationDate", "EXPIRATION DATE 01/31/2021", printBO.ExpirationDate);
			AssertEquals("RevisionDate", "(12/19)", printBO.RevisionDate);
			AssertEquals("Formatted entry number", "SV9-1000233-3", printBO.FormattedEntryNumber);
			AssertEquals("EntryType", "01", printBO.EntryType);
			AssertEquals("EntryTypeCode", "ABI/A", printBO.EntryTypeCode);
			AssertEquals("SuretyCode", "891", printBO.SuretyCode);
			AssertEquals("ImportingCarrier", "APL EMERALD (AAAA)", printBO.ImportingCarrier);
			AssertEquals("USTransportMode", "10", printBO.USTransportMode);
			AssertEquals("BondType", "8", printBO.BondType);
			AssertEquals("EstimatedEntryDate needs to print the Declaration Entry Date", expectedEntryDate, printBO.EstimatedEntryDate);
			AssertEquals("EffectiveUltimateConsigneeCustomsRegNo", USConstants.Same, printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("ImporterOfRecordCustomsRegNo", "91-013199000", printBO.ImporterOfRecordCustomsRegNo);
			AssertEquals("LocationOfGoodsAndName", "N598/WANDO TERMINAL", printBO.LocationOfGoodsAndName);
			AssertEquals("SCACAndMBillNumber", "AAAAOBLACE69", printBO.SCACAndMBillNumber);
			AssertEquals("SchDArrival", "3902", printBO.SchDArrival);
			AssertEquals("SchDEntry", "3902", printBO.SchDEntry);
			AssertEquals("Block24ReferenceNumber", "91-013199001", printBO.Block24ReferenceNumber);
			AssertEquals("UniqueCountryOfOrigin", "CA", printBO.UniqueCountryOfOrigin);
			AssertEquals("UniqueCountryOfExport", "AU", printBO.UniqueCountryOfExport);
			AssertEquals("UniquePortOfLading", "60267", printBO.UniquePortOfLading);
			AssertEquals("ManufacturerID", "THLIATHA191NAK", printBO.ManufacturerID);
			AssertEquals("BrokerFileNo", "B00001000", printBO.BrokerFileNo);
			AssertEquals("TotalEnteredValue", 48000m, printBO.TotalEnteredValue);
			AssertEquals("Entry print bills count", 1, printBO.EntryPrintBills.Count);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);

			AssertEquals("Line 1 - Block29Element1", "", printBO.EntryPrintLines[0].Block29Element1);
			AssertEquals("Line 1 - Block29Element2", "", printBO.EntryPrintLines[0].Block29Element2);
			AssertEquals("Line 1 - Block29Element3", "", printBO.EntryPrintLines[0].Block29Element3);
			AssertEquals("Line 1 - fee", "0.21%", printBO.EntryPrintLines[0].MPFPercentAsString);
			AssertEquals("Line 1 - fee", 0m, printBO.EntryPrintLines[0].MPFAmount);
			AssertEquals("Line 1 - fee", "0.125%", printBO.EntryPrintLines[0].HMFPercentAsString);
			AssertEquals("Line 1 - fee", 625m, printBO.EntryPrintLines[0].HMFAmount);

			AssertEquals("Summary fee desc", "501 501 Desc from DB", printBO.SummaryFeeDesc1);
			AssertEquals("Summary fee 1", 625m, printBO.SummaryFee1);
			AssertEquals("Summary fee desc", "", printBO.SummaryFeeDesc2);
			AssertEquals("Summary fee 2", 0m, printBO.SummaryFee2);

			AssertEquals("Should be Census Warning", USConstants.EntrySummaryDisposition.CensusWarning, printBO.SummaryStatus);
		}

		public void TestEntryDate()
		{
			var entryDate = new ZDateTime(2013, 06, 26);
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "3923100000";
			invoiceLine1.JI_LinePrice = 205278m;
			Declaration.JE_EntryAuthorisationDate = entryDate;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			message.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_147490     " +
"10ASV9  70032543 1101B00160298   0110 X           2071713                       " +
"1113-14792700013-147927000                     070313       PA                  " +
"20KKLU1101070513H572GRANVILLE BRIDE                                             " +
"21001                                                                           " +
"2200000015PCS                                                                   " +
"23MKKLUJPTA304368                                                               " +
"318B 037                                                                        " +
"40  001 JPJP061413        0000006298588860000009576    N                        " +
"47MJPSHIELE7214TOK                                                              " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"503923100000 0000575253 0000191751             X                                " +
"6250100023969                                                                   " +
"6249900066423                                                                   " +
"895010000002396949900000048500                                                  " +
"9000000575253 00000072469 00000000000 00000000000 00000000000                   " +
"Y  1101SV9AE";

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("EstimatedEntryDate should be JE_EntryAuthorisationDate in this case, not the message aens11.EstimatedEntryDate", entryDate, printBO.EstimatedEntryDate);
		}

		public void TestUniquePortOfLading()
		{
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8703330045";
			invoiceLine1.US_SupTariff = "98178501";
			invoiceLine1.JI_LinePrice = 48000m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			message.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_146622     " +
"10ASV9  70031685 1101B00160009   0110 XY    2     2041213                       " +
"1113-14792700013-147927000               040213040213       TX                  " +
"20APLU1101040213A00123                                                          " +
"21GT456                                                                         " +
"2200000020PC                                                                    " +
"23MAPLUFDDFGDFGDFG                                                              " +
"318B 037                                                                        " +
"40  001XCHCH040213        0000000006520510000000001    N                        " +
"47MCHHARWIN8PLA                                                                 " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"503305100000 0000000000 0000000600             X                                " +
"6250100000075                                                                   " +
"6249900000208                                                                   " +
"40  002VCHCH040213        0000000003520300000000015    N                        " +
"47MCHHARWIN8PLA                                                                 " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"503305100000 0000000000 0000000300             X                                " +
"40  003VCHCH040213        0000000003520300000000018    N                        " +
"47MCHHARWIN8PLA                                                                 " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"503303001000 0000000000 0000000300 000000000500L                                " +
"40  004 CHCH040213        0000000003523250000000010    N                        " +
"47MCHHARWIN8PLA                                                                 " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"508211100000 0000001830 0000000300             PCS                              " +
"508211929045 0000000000 0000000000             NO                               " +
"6250100000038                                                                   " +
"6249900000104                                                                   " +
"40  005 KRKR040213      KR0000000006520000000000016    N                        " +
"47MCHHARWIN8PLA                                                                 " +
"47C13-147927000                                                                 " +
"47S13-147927000                                                                 " +
"509102111010 0000000000 0000000000             NO                               " +
"509102111020 0000000000 0000000600             NO                               " +
"509102111030 0000000000 0000000000             NO                               " +
"509102111040 0000000000 0000000000             NO                               " +
"6250100000075                                                                   " +
"895010000000018849900000002500                                                  " +
"9000000001830 00000002688 00000000000 00000000000 00000000000                   " +
"Y  1101SV9AE";

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("UniquePortOfLading", USConstants.MultipleValueIndicator, printBO.UniquePortOfLading);
		}

		public void TestACEEntryMessage7501PrintForHMFDeMinimus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 500m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "AU";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);

			AssertEquals("Summary fee desc", "501 HMF (De Minimus)         $0.00", printBO.SummaryFeeDesc1);
		}

		[TestDate(2009, 6, 1)]
		public void TestEntryTypeCode()
		{
			Declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			Factory.Save();

			OrgHeader ior = Factory.NewWithValidTestData<OrgHeader>();
			ior.CustomsCodes.AddNew("EIN", "91-013199000");
			var iorWrapper = OrgHeaderWrapper.New(ior);
			iorWrapper.ZO_PayMethod = ACHPaymentTypeList.Codes.ImporterCheck;

			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8703330045";
			invoiceLine1.US_SupTariff = "98178501";
			invoiceLine1.JI_LinePrice = 48000m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var outMsg = message;
			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              10ASV9  10002333 3902            0110 X           2020711                       1191-01319900091-013199000                                  IL                  Y  3902SV9AE";
			var printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);
			AssertEquals("EntryTypeCode", "ABI/A", printBO.EntryTypeCode);

			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              10ASV9  10002333 3902            0110 X           3020711                       1191-01319900091-013199000                                  IL                  Y  3902SV9AE";
			printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);
			AssertEquals("EntryTypeCode", "ABI/S", printBO.EntryTypeCode);

			iorWrapper.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHCredit;
			Factory.Save();
			printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);
			AssertEquals("EntryTypeCode", "ABI/A", printBO.EntryTypeCode);

			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              10ASV9  10002333 3902            0110 X           6020711                       1191-01319900091-013199000                                  IL                  Y  3902SV9AE";
			Factory.Save();
			printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);
			AssertEquals("EntryTypeCode", "ABI/P", printBO.EntryTypeCode);

			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              10ASV9  10002333 3902            0110 X           1020711                       1191-01319900091-013199000                                  IL                  Y  3902SV9AE";
			Factory.Save();
			printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);
			AssertEquals("EntryTypeCode", "ABI/N", printBO.EntryTypeCode);
		}

		public void TestImportTeamNoFromAENSE0()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seBLUMessageOutgoing = Factory.New<MQEDIMessage>();
			seBLUMessageOutgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			seBLUMessageOutgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			seBLUMessageOutgoing.EM_MessageNum = "HYEDUSCMT_148767";
			seBLUMessageOutgoing.EM_MessageText = "B011101SV9SE                                               HYEDUSCMT_148767     SE10USV9  71002966 11EI 23-45678901240800000001001101                           SE15R    00591325221                                       00000150AP           SE15I    V9542365479                                                            SE15I    V1234567892                                                            SE15M    00601234566                                                            SE15H    HB00600166                                                             SE15S    SHB006001                                         00000023FL           SE15I    12345678965                                                            SE15I    96541256956                                                            SE20CR B00160865                                                                Y  1101SV9SE00010";
			seBLUMessageOutgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			declaration.Messages.Add(seBLUMessageOutgoing);

			var seBLUMessageResponse = Factory.New<MQEDIMessage>();
			seBLUMessageResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			seBLUMessageResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			seBLUMessageResponse.EM_MessageNum = "HYEDUSCMT_148767";
			seBLUMessageResponse.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002677 01EI 23-45678901240800000000101101                           SE15R    ALP61325205                                       00000200BL           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00004";
			seBLUMessageResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration.Messages.Add(seBLUMessageResponse);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var outgoing7501 = builder.PopulateMessage();
			Factory.Save();

			outgoing7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoing7501.EM_MessageNum = "HYEDUSCMT_148768";
			outgoing7501.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_148687     10ASV9  71002677 1101B00160830   0140 X           2110413                       1158-12345678958-123456789                     090913       MD                  2011  1101090913A001                                                            212525                                                                          2200000200BL                                                                    23M    ALP61325205                                                              318B 891                                                                        40  001 GBHK090913                       0000000015    N                        47MGB0UAFOR218ROA                                                               47C58-123456789                                                                 47S58-123456789                                                                 503201100000 0000000000 0000000010             KG                               6249900000003                                                                   8949900000002500                                                                9000000000000 00000002500 00000000000 00000000000 00000000000                   Y  1101SV9AE";
			outgoing7501.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entry.Messages.Add(outgoing7501);

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_MessageNum = "HYEDUSCMT_148768";
			incoming7501.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_148687     E0 SUMMRY 000001 REF ID: SV9 71002677 B00160830    123                          E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7100267700100B00160830   Y  1101SV9AX00002";
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var print = new ACEEntryMessage7501Print(entry, outgoing7501, incoming7501, seBLUMessageOutgoing);
			AssertEquals("123", print.USTeamNo);
		}

		public void TestPropertiesFromNotificationMessage()
		{
			var builder = new ACEEntrySummaryMessageBuilderForTesting(Entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			Factory.Save();

			var notificationMessage = Factory.New<MQEDIMessage>();
			notificationMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			notificationMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			notificationMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			notificationMessage.EM_Status = EDIMessage.Status.Received;
			notificationMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-2);
			notificationMessage.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E131333010090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888XJ5UC00002";

			Entry.Messages.Add(notificationMessage);
			Factory.Save();
			var printBO = new ACEEntryMessage7501Print(Entry, message, notificationMessage, null);
			AssertEquals("Status should be 'Document Required'", USConstants.EntrySummaryDisposition.DocsRequired, printBO.SummaryStatus);
			AssertEquals("Team Number should be printed from Notification Message", "333", printBO.USTeamNo);

			notificationMessage = Factory.New<MQEDIMessage>();
			notificationMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			notificationMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			notificationMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			notificationMessage.EM_Status = EDIMessage.Status.Received;
			notificationMessage.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-1);
			notificationMessage.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E171333010090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888XJ5UC00002";
			Entry.Messages.Add(notificationMessage);
			Factory.Save();
			printBO = new ACEEntryMessage7501Print(Entry, message, notificationMessage, null);
			AssertEquals("Status should be 'Document Required'", USConstants.EntrySummaryDisposition.DocsRequired, printBO.SummaryStatus);

			Declaration.US_PaperlessEntry = YesNoDefaultList.Codes.Yes;
			AssertEquals("Status should be 'Paperless'", USConstants.EntrySummaryDisposition.Paperless, printBO.SummaryStatus);
		}

		public void TestMissingDocuments()
		{
			var builder = new ACEEntrySummaryMessageBuilderForTesting(Entry, false, true, UpdateActionCode.Add);
			var outMsg = builder.PopulateMessage();
			Factory.Save();

			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              " +
"10ASV9  10002333 3902            0110 X           2020711                       " +
"1191-01319900091-013199000                                  IL                  " +
"20AAAA3902      N598APL EMERALD                                                 " +
"2200000001PK                                                                    " +
"23MAAAAOBLACE69                                                                 " +
"318B 891                                                                        " +
"332218                                                                          " +
"40  001 CAAU012411        0000000050602670000009000    N                        " +
"44COMMERCIAL DESCRIPTION                                                        " +
"47MTHLIATHA191NAK                                                               " +
"47C91-013199000                                                                 " +
"47S91-013199000                                                                 " +
"5098178501   0000000000 0000500000                                              " +
"508703330045 0000000000 0000000000             NO                               " +
"6250100062500                                                                   " +
"8950100000062500                                                                " +
"9000000000000 00000062500 00000000000 00000000000 00000000000                   " +
"Y  3902SV9AE";

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			var printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);

			AssertEquals("Missing Document 1", "22", printBO.MissingDoc1);
			AssertEquals("Missing Document 2", "18", printBO.MissingDoc2);
		}

		public void TestWithBLUMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_TotalNoOfPacksPackType = "";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seBLUMessageOutgoing = Factory.New<MQEDIMessage>();
			seBLUMessageOutgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			seBLUMessageOutgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			seBLUMessageOutgoing.EM_MessageNum = "HYEDUSCMT_148767";
			seBLUMessageOutgoing.EM_MessageText = @"B011101SV9SE                                               HYEDUSCMT_148767     
SE10USV9  71002966 11EI 23-45678901240800000001001101                           
SE15I    V9542365479                                                            
SE15R    00591325221                                       00000150AP           
SE15I    V1234567892                                                            
SE15R    00591325221                                       00000150AP           
SE15I    12345678965                                                            
SE15M    00601234566                                                            
SE15H    HB00600166                                                             
SE15S    SHB006001                                         00000023FL           
SE15I    96541256956                                                            
SE15M    00601234566                                                            
SE15H    HB00600166                                                             
SE15S    SHB006001                                         00000023FL           
SE20CR B00160865                                                                
Y  1101SV9SE00010".Replace("\r\n", "");

			seBLUMessageOutgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			declaration.Messages.Add(seBLUMessageOutgoing);

			var seBLUMessageResponse = Factory.New<MQEDIMessage>();
			seBLUMessageResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			seBLUMessageResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			seBLUMessageResponse.EM_MessageNum = "HYEDUSCMT_148767";
			seBLUMessageResponse.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002677 01EI 23-45678901240800000000101101                           SE15R    ALP61325205                                       00000200BL           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00004";
			seBLUMessageResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration.Messages.Add(seBLUMessageResponse);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var outgoing7501 = Factory.New<MQEDIMessage>();
			outgoing7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoing7501.EM_MessageNum = "HYEDUSCMT_148768";
			outgoing7501.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_148687     10ASV9  71002677 1101B00160830   0140 X           2110413                       1158-12345678958-123456789                     090913       MD                  2011  1101090913A001                                                            212525                                                                          2200000200BL                                                                    23M    ALP61325205                                                              318B 891                                                                        40  001 GBHK090913                       0000000015    N                        47MGB0UAFOR218ROA                                                               47C58-123456789                                                                 47S58-123456789                                                                 503201100000 0000000000 0000000010             KG                               6249900000003                                                                   8949900000002500                                                                9000000000000 00000002500 00000000000 00000000000 00000000000                   Y  1101SV9AE";
			outgoing7501.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entry.Messages.Add(outgoing7501);

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_MessageNum = "HYEDUSCMT_148768";
			incoming7501.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_148687     E0 SUMMRY 000001 REF ID: SV9 71002677 B00160830                                 E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7100267700100B00160830   Y  1101SV9AX00002";
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var print = new ACEEntryMessage7501Print(entry, outgoing7501, incoming7501, seBLUMessageOutgoing);
			AssertEquals(4, print.EntryPrintBills.Count);
			AssertEquals(ZDateTime.Empty, print.FirstBillITDate);
			AssertEquals("V9542365479", print.FirstBillITNO);
			AssertEquals("00591325221", print.SCACAndMBillNumber);

			var bill1 = print.EntryPrintBills[0];
			AssertEquals(ZString.Empty, bill1.EffectiveHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill1.EffectiveMasterBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill1.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("V9542365479", bill1.ITNO);
			AssertEquals("00591325221", bill1.MasterBill);
			AssertEquals(ZString.Empty, bill1.HouseBill);
			AssertEquals(ZString.Empty, bill1.SubHouseBill);

			var bill2 = print.EntryPrintBills[1];
			AssertEquals(ZString.Empty, bill2.EffectiveHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill2.EffectiveMasterBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill2.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("V1234567892", bill2.ITNO);
			AssertEquals("00591325221", bill2.MasterBill);
			AssertEquals(ZString.Empty, bill2.HouseBill);
			AssertEquals(ZString.Empty, bill2.SubHouseBill);

			var bill3 = print.EntryPrintBills[2];
			AssertEquals(ZString.Empty, bill3.EffectiveHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill3.EffectiveMasterBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill3.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("12345678965", bill3.ITNO);
			AssertEquals("00601234566", bill3.MasterBill);
			AssertEquals("HB00600166", bill3.HouseBill);
			AssertEquals("SHB006001", bill3.SubHouseBill);

			var bill4 = print.EntryPrintBills[3];
			AssertEquals(ZString.Empty, bill4.EffectiveHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill4.EffectiveMasterBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill4.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("96541256956", bill4.ITNO);
			AssertEquals("00601234566", bill4.MasterBill);
			AssertEquals("HB00600166", bill4.HouseBill);
			AssertEquals("SHB006001", bill4.SubHouseBill);

			//another BLU message without IT details, but with multiple bills
			var seBLUMessageOutgoing2 = Factory.New<MQEDIMessage>();
			seBLUMessageOutgoing2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			seBLUMessageOutgoing2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			seBLUMessageOutgoing2.EM_SystemCreateTimeUtc = ZDateTime.Today;
			seBLUMessageOutgoing2.EM_MessageNum = "HYEDUSCMT_148769";
			seBLUMessageOutgoing2.EM_MessageText = @"B011101SV9SE                                               HYEDUSCMT_148784     
SE10USV9  71002958 01EI 23-45678901240800000001201101                           
SE15R    00591325210                                                            
SE16COA 6020 09091300000450                                                     
SE15M    AAA00178975                                                            
SE15H    AAAHB343234                                       00000002CU           
SE15M    AAA00178975                                                            
SE15H    AAAHB000006                                       00000009VY           
SE15M    BBB002124564                                                           
SE15H    BBB00245454HB                                                          
SE15S    BBB00215454SHB                                    00000009AM           
SE20CR B00160864                                                                
Y  1101SV9SE00010".Replace("\r\n", "");

			seBLUMessageOutgoing2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			declaration.Messages.Add(seBLUMessageOutgoing2);

			var seBLUMessageResponse2 = Factory.New<MQEDIMessage>();
			seBLUMessageResponse2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			seBLUMessageResponse2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			seBLUMessageResponse2.EM_SystemCreateTimeUtc = ZDateTime.Today;
			seBLUMessageResponse2.EM_MessageNum = "HYEDUSCMT_148769";
			seBLUMessageResponse2.EM_MessageText = "B011101SV9SX                                                                    SE10USV9  71002677 01EI 23-45678901240800000000101101                           SE15R    ALP61325205                                       00000200BL           SE9002   SE DATA ACCEPTED                                                       Y  1101SV9SX00004";
			seBLUMessageResponse2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration.Messages.Add(seBLUMessageResponse2);

			print = new ACEEntryMessage7501Print(entry, outgoing7501, incoming7501, seBLUMessageOutgoing2);
			AssertEquals(4, print.EntryPrintBills.Count);
			AssertEquals(ZDateTime.Empty, print.FirstBillITDate);
			AssertEquals("", print.FirstBillITNO);
			AssertEquals("00591325210", print.SCACAndMBillNumber);

			bill1 = print.EntryPrintBills[0];
			AssertEquals(ZString.Empty, bill1.EffectiveHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill1.EffectiveMasterBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill1.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill1.ITNO);
			AssertEquals("00591325210", bill1.MasterBill);
			AssertEquals(ZString.Empty, bill1.HouseBill);
			AssertEquals(ZString.Empty, bill1.SubHouseBill);
			AssertEquals("from SE16", 450, bill1.PkgQty);
			AssertEquals(ZString.Empty, bill1.PkgType);

			bill2 = print.EntryPrintBills[1];
			AssertEquals(ZString.Empty, bill2.EffectiveHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill2.EffectiveMasterBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill2.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill2.ITNO);
			AssertEquals("AAA00178975", bill2.MasterBill);
			AssertEquals("AAAHB343234", bill2.HouseBill);
			AssertEquals(ZString.Empty, bill2.SubHouseBill);
			AssertEquals(2, bill2.PkgQty);
			AssertEquals(ZString.Empty, bill2.PkgType);

			bill3 = print.EntryPrintBills[2];
			AssertEquals(ZString.Empty, bill3.EffectiveHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill3.EffectiveMasterBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill3.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill3.ITNO);
			AssertEquals("AAA00178975", bill3.MasterBill);
			AssertEquals("AAAHB000006", bill3.HouseBill);
			AssertEquals(ZString.Empty, bill3.SubHouseBill);
			AssertEquals(9, bill3.PkgQty);
			AssertEquals(ZString.Empty, bill3.PkgType);

			bill4 = print.EntryPrintBills[3];
			AssertEquals(ZString.Empty, bill4.EffectiveHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill4.EffectiveMasterBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill4.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill4.ITNO);
			AssertEquals("BBB002124564", bill4.MasterBill);
			AssertEquals("BBB00245454HB", bill4.HouseBill);
			AssertEquals("BBB00215454SHB", bill4.SubHouseBill);
			AssertEquals(9, bill4.PkgQty);
			AssertEquals(ZString.Empty, bill4.PkgType);
		}

		public void TestITDateAndITNumberForBLU()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.US_ITDate = ZDateTime.Today.AddDays(-2);

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MasterBill";
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";

			var houseBill1 = masterBill.ChildBills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_BillNum = "HouseBill1";
			houseBill1.US_UI_NKBillIssuerSCAC = "APLU";

			var subHouseBill = houseBill1.ChildBills.AddNew();
			subHouseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.CU_BillNum = "SubHouseBill";
			subHouseBill.US_UI_NKBillIssuerSCAC = "APLU";
			subHouseBill.ITNumber = "IT12345";

			var houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.ITNumber = "IT1234";
			houseBill2.CU_BillNum = "HouseBill2";
			houseBill2.US_UI_NKBillIssuerSCAC = "APLU";

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "MasterBill 2";
			masterBill2.US_UI_NKBillIssuerSCAC = "OOOP";

			var houseBill1_MB2 = masterBill2.ChildBills.AddNew();
			houseBill1_MB2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1_MB2.CU_BillNum = "HouseBill1";
			houseBill1_MB2.US_UI_NKBillIssuerSCAC = "APLU";
			houseBill1_MB2.ITNumber = "V1245678910";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			entry.CreateDocPrintingDetails(message.PK);
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var print = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("IT Number", "IT12345", print.FirstBillITNO);
			AssertEquals("IT Date", ZDateTime.Today.AddDays(-2), print.FirstBillITDate);

			AssertEquals("EntryPrintBills", 3, print.EntryPrintBills.Count);
			var billDetail1 = print.EntryPrintBills[0];
			var billDetail2 = print.EntryPrintBills[1];
			var billDetail3 = print.EntryPrintBills[2];

			AssertEquals("IT Number", "IT12345", billDetail1.ITNO);
			AssertEquals("IT Date", ZDateTime.Today.AddDays(-2), billDetail1.ITDate);

			AssertEquals("IT Number", "IT1234", billDetail2.ITNO);
			AssertEquals("IT Date", ZDateTime.Today.AddDays(-2), billDetail2.ITDate);

			AssertEquals("IT Number", "V1245678910", billDetail3.ITNO);
			AssertEquals("IT Date", ZDateTime.Today.AddDays(-2), billDetail3.ITDate);

			houseBill1_MB2.ITNumber = "V777755555";
			Factory.Save();

			var bluBuilder = new TestBillOfLadingUpdateBuilder(declaration);
			var bluMessage = bluBuilder.PopulateMessage();

			Factory.Save();
			print = new ACEEntryMessage7501Print(entry, message, incoming7501, bluMessage);
			AssertEquals("EntryPrintBills", 3, print.EntryPrintBills.Count);

			billDetail1 = print.EntryPrintBills[0];
			billDetail2 = print.EntryPrintBills[1];
			billDetail3 = print.EntryPrintBills[2];

			AssertEquals("Updated 'V' type IT Number should print from BLU details", "V777755555", billDetail1.ITNO);
			AssertEquals("IT Date should remain as per previous ens22", ZDateTime.Today.AddDays(-2), billDetail1.ITDate);

			AssertEquals(@"Printing should be based on registry settings. If only V numbers have been sent in BLU, 
			we should get other IT Numbers from previous ens22 record", "IT12345", billDetail2.ITNO);
			AssertEquals("IT Date should remain as per previous ens22", ZDateTime.Today.AddDays(-2), billDetail2.ITDate);

			AssertEquals("IT Number after BLU - should get IT Number from previous ens22 record", "IT1234", billDetail3.ITNO);
			AssertEquals("IT Date should remain as per previous ens22", ZDateTime.Today.AddDays(-2), billDetail3.ITDate);

			//change registry item to 'send all IT numbers' and send BLU message with all IT nos - should be printed from message
			USCustomsDataRegistry.Instance.SendAllITNumbersInBOLMessage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), System.Guid.Empty, true);
			houseBill1_MB2.ITNumber = "V777755566";
			houseBill2.ITNumber = "987451301";
			bluBuilder = new TestBillOfLadingUpdateBuilder(declaration);
			bluMessage = bluBuilder.PopulateMessage();

			Factory.Save();
			print = new ACEEntryMessage7501Print(entry, message, incoming7501, bluMessage);
			AssertEquals("EntryPrintBills", 3, print.EntryPrintBills.Count);

			billDetail1 = print.EntryPrintBills[0];
			billDetail2 = print.EntryPrintBills[1];
			billDetail3 = print.EntryPrintBills[2];

			AssertEquals(@"Printing should be based on registry settings (set to true). All IT Numbers should print from latest BLU message", "IT12345", billDetail1.ITNO);
			AssertEquals("IT Date should print from latest BLU message", ZDateTime.Today.AddDays(-2), billDetail1.ITDate);

			AssertEquals("IT Numbers should print from latest BLU message", "987451301", billDetail2.ITNO);
			AssertEquals("IT Date should print from latest BLU message", ZDateTime.Today.AddDays(-2), billDetail2.ITDate);

			AssertEquals("IT Numbers should print from latest BLU message", "V777755566", billDetail3.ITNO);
			AssertEquals("IT Date should print from latest BLU message", ZDateTime.Today.AddDays(-2), billDetail3.ITDate);
		}

		public void TestWithBLUMessageWhenITNumbersNotForAllBills()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "1";
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";

			var houseBill1 = masterBill.ChildBills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_BillNum = "H1";
			houseBill1.US_UI_NKBillIssuerSCAC = "OPLI";
			houseBill1.CU_NoOfPacks = 11m;
			houseBill1.ITNumber = "569324561";
			houseBill1.ITAndSplitDetails[0].US_NoOfPacks = 7;

			var houseBill2 = masterBill.ChildBills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "H2";
			houseBill2.CU_NoOfPacks = 43m;
			houseBill2.CU_PackType = ShippingOrPackingingUnitList.Codes.Package;
			houseBill2.US_UI_NKBillIssuerSCAC = "QWER";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			var outgoing7501 = Factory.New<MQEDIMessage>();
			outgoing7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			outgoing7501.EM_MessageNum = "HYEDUSCMT_148768";
			outgoing7501.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_148687     10ASV9  71002677 1101B00160830   0140 X           2110413                       1158-12345678958-123456789                     090913       MD                  2011  1101090913A001                                                            212525                                                                          2200000200BL                                                                    23M    ALP61325205                                                              318B 891                                                                        40  001 GBHK090913                       0000000015    N                        47MGB0UAFOR218ROA                                                               47C58-123456789                                                                 47S58-123456789                                                                 503201100000 0000000000 0000000010             KG                               6249900000003                                                                   8949900000002500                                                                9000000000000 00000002500 00000000000 00000000000 00000000000                   Y  1101SV9AE";
			outgoing7501.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entry.Messages.Add(outgoing7501);

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_MessageNum = "HYEDUSCMT_148768";
			incoming7501.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_148687     E0 SUMMRY 000001 REF ID: SV9 71002677 B00160830                                 E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7100267700100B00160830   Y  1101SV9AX00002";
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var action = (EntryHeaderMessageSendingAction)actions[0];

			var message = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Update, ACEEntrySummaryMessageSendingOption.New(action)).PopulateMessage();

			var print = new ACEEntryMessage7501Print(entry, outgoing7501, incoming7501, message);
			AssertEquals(2, print.EntryPrintBills.Count);
			AssertEquals(ZDateTime.Empty, print.FirstBillITDate);
			AssertEquals("569324561", print.FirstBillITNO);
			AssertEquals("1", print.SCACAndMBillNumber);

			var bill1 = print.EntryPrintBills[0];
			AssertEquals(ZString.Empty, bill1.EffectiveHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill1.EffectiveMasterBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill1.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("569324561", bill1.ITNO);
			AssertEquals("1", bill1.MasterBill);
			AssertEquals("H1", bill1.HouseBill);
			AssertEquals(ZString.Empty, bill1.SubHouseBill);

			var bill2 = print.EntryPrintBills[1];
			AssertEquals(ZString.Empty, bill2.EffectiveHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill2.EffectiveMasterBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill2.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill2.ITNO);
			AssertEquals("1", bill2.MasterBill);
			AssertEquals("H2", bill2.HouseBill);
			AssertEquals(ZString.Empty, bill2.SubHouseBill);
		}

		public void TestLicenseNumbersForMultipleLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 500m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			invoiceLine.JI_Tariff = "7306191050";
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._01, "S0PRCS072");

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500m;
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			invoiceLine2.JI_Tariff = "7306191010";
			invoiceLine2.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._01, "aaaaa1222");

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 500m;
			invoiceLine3.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			invoiceLine3.JI_Tariff = "7306191050";
			invoiceLine3.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._01, "B8PRH2072");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			var entryPrintLine1 = (ACEEntryMessage7501Line)printBO.EntryPrintLines[0];
			AssertEquals("License", "01-S0PRCS072", entryPrintLine1.LicenseNumber);

			var entryPrintLine2 = (ACEEntryMessage7501Line)printBO.EntryPrintLines[1];
			AssertEquals("License", "01-AAAAA1222", entryPrintLine2.LicenseNumber);

			var entryPrintLine3 = (ACEEntryMessage7501Line)printBO.EntryPrintLines[2];
			AssertEquals("License", "01-B8PRH2072", entryPrintLine3.LicenseNumber);
		}

		public void TestADD_CVDNumbersForMultipleLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 500m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			invoiceLine.JI_Tariff = "1902192030";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 500m;
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			invoiceLine2.JI_Tariff = "1902192030";
			invoiceLine2.US_ADDCaseNo = "A475818001";
			invoiceLine2.US_ADDDepositValue = 300m;
			invoiceLine2.US_CVDCaseNo = "C475819017";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 500m;
			invoiceLine3.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			invoiceLine3.JI_Tariff = "7211140045";
			invoiceLine3.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._01, "AAAA789923");
			invoiceLine3.US_CVDCaseNo = "C357109000";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			var entryPrintLine1 = (ACEEntryMessage7501Line)printBO.EntryPrintLines[0];
			AssertEquals("ADD/CVD", "", entryPrintLine1.ADDNo);
			AssertEquals("ADD/CVD", "", entryPrintLine1.CVDNo);

			var entryPrintLine2 = (ACEEntryMessage7501Line)printBO.EntryPrintLines[1];
			AssertEquals("ADD/CVD", "A475-818-001", entryPrintLine2.ADDNo);
			AssertEquals("ADD/CVD", "C475-819-017", entryPrintLine2.CVDNo);

			var entryPrintLine3 = (ACEEntryMessage7501Line)printBO.EntryPrintLines[2];
			AssertEquals("ADD/CVD", "", entryPrintLine3.ADDNo);
			AssertEquals("ADD/CVD", "C357-109-000", entryPrintLine3.CVDNo);
		}

		public void TestPrintCoffeFee()
		{
			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(Entry, false, true, UpdateActionCode.Add);
			var outMsg = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              " +
"10ASV9  10002333 3902            0110 X     1     2020711                       " +
"1191-01319900091-013199000                                  IL                  " +
"20AAAA3902      N598APL EMERALD                                                 " +
"2200000001PK                                                                    " +
"23MAAAAOBLACE69                                                                 " +
"318B 891                                                                        " +
"332218                                                                          " +
"40  001 CAAU012411        0000000050602670000009000    N                        " +
"44COMMERCIAL DESCRIPTION                                                        " +
"47MTHLIATHA191NAK                                                               " +
"47C91-013199000                                                                 " +
"47S91-013199000                                                                 " +
"5098178501   0000000000 0000500000                                              " +
"508703330045 0000000000 0000000000             NO                               " +
"6250100062500                                                                   " +
"8950100000062500                                                                " +
"9000000000000 00000000000 00000000000 00000000000 00000000000 00000062501       " +
"Y  3902SV9AE";

			var printBO = new ACEEntryMessage7501Print(Entry, outMsg, incoming7501, null);
			AssertEquals(625.01m, printBO.TotalOther);
		}

		public void TestTaxDeferred()
		{
			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(Entry, false, true, UpdateActionCode.Add);
			var outMsg = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              " +
"10ASV9  10002333 3902            0110 X     1     2020711                       " +
"1191-01319900091-013199000                                  IL                  " +
"20AAAA3902      N598APL EMERALD                                                 " +
"2200000001PK                                                                    " +
"23MAAAAOBLACE69                                                                 " +
"318B 891                                                                        " +
"332218                                                                          " +
"40  001 CAAU012411        0000000050602670000009000    N                        " +
"44COMMERCIAL DESCRIPTION                                                        " +
"47MTHLIATHA191NAK                                                               " +
"47C91-013199000                                                                 " +
"47S91-013199000                                                                 " +
"5098178501   0000000000 0000500000                                              " +
"508703330045 0000000000 0000000000             NO                               " +
"6250100062500                                                                   " +
"8950100000062500                                                                " +
"9000000000000 00000062501 00000000000 00000000000 00000000000                   " +
"Y  3902SV9AE";

			var printBO = new ACEEntryMessage7501Print(Entry, outMsg, incoming7501, null);
			Assert("TaxToBeDeferred", printBO.TaxToBeDeferred);

			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.BulkLiquorDeferred;
			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              " +
"10ASV9  10002333 3902            0110 X           2020711                       " +
"1191-01319900091-013199000                                  IL                  " +
"20AAAA3902      N598APL EMERALD                                                 " +
"2200000001PK                                                                    " +
"23MAAAAOBLACE69                                                                 " +
"318B 891                                                                        " +
"332218                                                                          " +
"40  001 CAAU012411        0000000050602670000009000    N                        " +
"44COMMERCIAL DESCRIPTION                                                        " +
"47MTHLIATHA191NAK                                                               " +
"47C91-013199000                                                                 " +
"47S91-013199000                                                                 " +
"5098178501   0000000000 0000500000                                              " +
"508703330045 0000000000 0000000000             NO                               " +
"6250100062500                                                                   " +
"8950100000062500                                                                " +
"9000000000000 00000062501 00000000000 00000000000 00000000000                   " +
"Y  3902SV9AE";

			printBO = new ACEEntryMessage7501Print(Entry, outMsg, incoming7501, null);
			Assert("BulkLiquorTaxDeferred", printBO.BulkLiquorTaxDeferred);
		}

		public void TestPrintInvoiceDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EnableENS = true;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV-1";

			var inv_1_invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			inv_1_invoiceLine1.JI_Tariff = "6205202066";
			inv_1_invoiceLine1.US_SupTariff = "9808003000";
			inv_1_invoiceLine1.JI_LinePrice = 1000m;
			inv_1_invoiceLine1.JI_CustomsQuantity = 100m;
			inv_1_invoiceLine1.JI_CustomsUnitQty = "KG";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV-2";

			var inv_2_invoiceLine1 = invoice2.JobComInvoiceLines.AddNew();
			inv_2_invoiceLine1.JI_Tariff = "3924905650";
			inv_2_invoiceLine1.US_SupTariff = "9808003000";
			inv_2_invoiceLine1.JI_LinePrice = 495m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var builder = new ACEEntrySummaryMessageBuilderForTesting(declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			Factory.Save();

			var outMsg = message;
			outMsg.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_151209     10ASV9  70035561 1101B00161791   0110 XY          2041814                       1158-12345678958-123456789               040814040814       PA                  20APLU1101040814A001TITANIC                                                     21001IO                                                                         2200000045BN                                                                    23MAPLUFRT5434534                                                               318B 891                                                                        40  001 SVSV040814        0000000020602370000000995340 N                        47MSVTEXGUY1SAN                                                                 47C58-123456789                                                                 47S58-123456789                                                                 509808003000 0000000000 0000005495                                              506205202066 0000000000 0000000000 000000190000DOZ000000020000KG                6250100000687                                                                   6205600000266                                                                   40  002 SVAU040814                  602370000000120    N                        47MSVTEXGUY1SAN                                                                 47C58-123456789                                                                 47S58-123456789                                                                 509808003000 0000000000 0000000118                                              503924905650 0000000000 0000000000             X                                OA  FD0                                                                         6250100000015                                                                   895010000000070205600000000266                                                  9000000000000 00000000968 00000000000 00000000000 00000000000                   Y  1101SV9AE";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;
			inMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			inMsg.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			inMsg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inMsg.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_151209     E0 SUMMRY 000001 REF ID: SV9 71002677 B00160830    123                          E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7100267700100B00160830   Y  1101SV9AX00002";

			var printBO = new ACEEntryMessage7501Print(declaration.ActiveEntryHeaders.EntrySummaryEntry, outMsg, inMsg, null);
			AssertEquals(2, printBO.EntryPrintLines.Count);

			var line = printBO.EntryPrintLines[0];
			AssertEquals("Invoice contains sequence 1", "001/INV-1", line.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, line.PrintInvoiceHeading);
			AssertEquals("Print Details", true, line.PrintInvoiceDetails);

			line = printBO.EntryPrintLines[1];
			AssertEquals("Invoice contains sequence 1", "002/INV-2", line.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, line.PrintInvoiceHeading);
			AssertEquals("Print Details", true, line.PrintInvoiceDetails);
		}

		[TestDate(2014, 06, 16)]
		public void TestDateOfFirstArrival()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "ARV";
			declaration.JE_ExportDate = new ZDateTime(2014, 05, 17);
			declaration.JE_DateOfArrival = new ZDateTime(2014, 05, 29);
			declaration.US_EntryDate = new ZDateTime(2014, 06, 08);
			declaration.US_EstimatedEntryDate = new ZDateTime(2014, 06, 08);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 500m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 500m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("DateOfFirstArrival", new ZDateTime(2014, 05, 29), printBO.DateOfFirstArrival);
		}

		public void TestInvoiceTotalDetailsPrintedOnce()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV-1";

			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "6205202066";
			invoiceLine1.US_SupTariff = "9808003000";
			invoiceLine1.JI_LinePrice = 1000m;
			invoiceLine1.JI_CustomsQuantity = 100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0201200200";
			invoiceLine2.JI_LinePrice = 820m;
			invoiceLine2.JI_CustomsQuantity = 326m;
			invoiceLine2.JI_CustomsUnitQty = "KG";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var builder = new ACEEntrySummaryMessageBuilderForTesting(Declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			Factory.Save();

			var outMsg = message;
			outMsg.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_159705     10ASV9  70038680 1101B00162701   0111 XA          2121614                       1158-12345678958-123456789                     120414       PA                  20APLU1101120414B815TITANIC                                                     21005TR                                                                         2200000045BH                                                                    23MAPLUPST001IO456                                                              23HAPLU40056031                                                                 318B 891                                                                        SE30MF ACE TEST SUPPLIER                                                        SE3515P O BOX 288                                                               SE36OSAKA                                       530-01         JP               SE30CN                                    EI 58-123456789                       SE30BY ACE TEST IMPORTER 1                                                      SE35155000 TOWERS CRESCENT DRIVE         15NWD ENTERPRISES 12A                  SE36PHILLY                                      19444          US               40  001 JPJP120414        0000000107588660000002000    N                        47MJPACETAC288OSA                                                               47C58-123456789                                                                 47S58-123456789                                                                 500811202025 0000081000 0000018000 000000198700KG                               6250100002250                                                                   6249900006235                                                                   6205700004371                                                                   40  002 JPJP120414        0000000047588660000000465    N                        47MJPACETAC288OSA                                                               47C58-123456789                                                                 47S58-123456789                                                                 500201200200 0000032000 0000008000 000000046500KG                               OI        DES                                                                   PG01001FSIFSI         FDSFSD             210.000                                PG02P                                                                           PG0639 GB                                                                       PG13                                   ISOGB                                    PG14 FS779215                                                                   PG50                                                                            PG10FS1   1    RPI 1C                                                           PG19EXE   123                                                                   PG19PE    FDS                                                                   PG25           45                       1204201412042014                        PG266000000001000CS   SDF SDFS                                                  PG29LB 000000150000                                                             PG51                                                                            PG19IM                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US19444        PG21IM CRAIG SCOTT SEELIG     2158885555     TEST@IMPORTER.COM                  PG19CN                   ACE TEST IMPORTER 1             5000 TOWERS CRESCENT DRPG20NWD ENTERPRISES 12A                  PHILLY               PA US19444        PG21CN CRAIG SCOTT SEELIG     2158885555     TEST@IMPORTER.COM                  PG19CB                   VISHNEVETSKAYA                  TEST LINE 1            PG20                                     NORTH SYDNEY         AL   2060         PG21CB VISHNEVETSKAYA         041562012      ITLANA.VISHNEVETSKAYA@CARGOWISE.COMPG22 956         CI FS3 Y12042014                                               PG30I12042014    8  10203-P10203                                                6205300000247                                                                   6250100001000                                                                   6249900002771                                                                   8950100000003250499000000090060570000000437105300000000247                      9000000113000 00000016874 00000000000 00000000000 00000000000                   Y  1101SV9AE";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;
			inMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			inMsg.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			inMsg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inMsg.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_159705     E0 SUMMRY 000001 REF ID: SV9 70038680 B00162701    171                          E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7003868000100B00162701   Y  1101SV9AX00002";

			var printBO = new ACEEntryMessage7501Print(Declaration.ActiveEntryHeaders.EntrySummaryEntry, outMsg, inMsg, null);
			AssertEquals(2, printBO.EntryPrintLines.Count);

			var line = printBO.EntryPrintLines[0];
			AssertEquals("Print Heading", true, line.PrintInvoiceHeading);
			AssertEquals("Print Details", false, line.PrintInvoiceDetails);

			line = printBO.EntryPrintLines[1];
			AssertEquals("Print Heading", false, line.PrintInvoiceHeading);
			AssertEquals("Print Details", true, line.PrintInvoiceDetails);

			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			for (int i = 1; i <= 1050; i++)
			{
				invoice1.JobComInvoiceLines.AddNew();
			}
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("Line Number", "A1G", invoice1.JobComInvoiceLines[1051].CusEntryLine.CL_LineNumberFormatted);

			builder = new ACEEntrySummaryMessageBuilderForTesting(Declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			message.EM_MessageNum = "HYEDUSCMT_159707";
			Factory.Save();

			var inMsg2 = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg2.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg2.EM_LinkUniqueID = declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;
			inMsg2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			inMsg2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			inMsg2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inMsg2.EM_MessageNum = "HYEDUSCMT_159707";
			inMsg2.EM_MessageText = "B001101SV9AX                                               HYEDUSCMT_159707     E0 SUMMRY 000001 REF ID: SV9 70038680 B00162701    171                          E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7003868000100B00162701   Y  1101SV9AX00002";

			printBO = new ACEEntryMessage7501Print(Declaration.ActiveEntryHeaders.EntrySummaryEntry, message, inMsg2, null);
			AssertEquals(1052, printBO.EntryPrintLines.Count);

			line = (EntrySummary7501Line)printBO.EntryPrintLines.LastOrDefault();
			AssertEquals("Should print details, because this is a last line", true, line.PrintInvoiceDetails);

			line = printBO.EntryPrintLines[1050];
			AssertEquals("Should not print details, because this is not a last line", false, line.PrintInvoiceDetails);
		}

		[TestDate(2015, 12, 21)]
		public void TestForConsumptionFTZ()
		{
			var expectedEntryDate = new ZDate(2015, 12, 21);
			Declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			Declaration.Invoices.RemoveAll();
			Factory.Save();

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Declaration.US_EnableENS = true;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8703330045";
			invoiceLine1.US_SupTariff = "98178501";
			invoiceLine1.JI_LinePrice = 48000m;
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine1.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;

			Declaration.US_PresentationDate = expectedEntryDate;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);

			AssertEquals("UniqueCountryOfExport", Core.Constants.CountryCodes.Canada, printBO.UniqueCountryOfExport);

			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Chile;
			invoiceLine1.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Mexico;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("UniqueCountryOfExport should be calculated for FTZ", Core.Constants.CountryCodes.Mexico, printBO.UniqueCountryOfExport);

			invoiceLine1.JI_LinePrice = 2900m;
			var invoice2 = Declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8703330045";
			invoiceLine2.JI_LinePrice = 3600m;
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine2.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Mauritania;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("When there are multiple countries of export, that the highest entered value (customs value in USD) one is shown.", Core.Constants.CountryCodes.Mauritania, printBO.UniqueCountryOfExport);
		}

		public void TestPrintAENS50FormattedTariff()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			Declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			Factory.Save();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8703330045";
			invoiceLine1.US_SupTariff = "98178501";
			invoiceLine1.JI_LinePrice = 48000m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var outMsg = message;
			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              " +
"10ASV9  10002333 3902            0110 X           2020711                       " +
"1191-01319900091-01319900091-013199001                      IL                  " +
"20AAAA3902      N598APL EMERALD                                                 " +
"2200000001PK                                                                    " +
"23MAAAAOBLACE69                                                                 " +
"318B 891                                                                        " +
"40  001 CAAU012411        0000000050602670000009000    N                        " +
"44COMMERCIAL DESCRIPTION                                                        " +
"47MTHLIATHA191NAK                                                               " +
"47C91-013199000                                                                 " +
"47S91-013199000                                                                 " +
"5098178501   0000000000 0000500000                                              " +
"508703330045 0000000000 0000000000             NO                               " +
"5402STL000111                                                                   " +
"6250100062500                                                                   " +
"8950100000062500                                                                " +
"9000000000000 00000062500 00000000000 00000000000 00000000000                   " +
"Y  3902SV9AE";

			var printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);
			var aens50Lines = printBO.LineObjectCollection[0].aens50;
			AssertEquals("Two AENS50 Lines", 2, aens50Lines.Count);
			AssertNotNull(aens50Lines.First(x => x.HTSNumber == "98178501"));

			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              " +
"10ASV9  10002333 3902            0110 X           2020711                       " +
"1191-01319900091-01319900091-013199001                      IL                  " +
"20AAAA3902      N598APL EMERALD                                                 " +
"2200000001PK                                                                    " +
"23MAAAAOBLACE69                                                                 " +
"318B 891                                                                        " +
"40  001 CAAU012411        0000000050602670000009000    N                        " +
"44COMMERCIAL DESCRIPTION                                                        " +
"47MTHLIATHA191NAK                                                               " +
"47C91-013199000                                                                 " +
"47S91-013199000                                                                 " +
"50N/A        0000000000 0000500000                                              " +
"508703330045 0000000000 0000000000             NO                               " +
"5402STL000111                                                                   " +
"6250100062500                                                                   " +
"8950100000062500                                                                " +
"9000000000000 00000062500 00000000000 00000000000 00000000000                   " +
"Y  3902SV9AE";
			printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);
			aens50Lines = printBO.LineObjectCollection[0].aens50;
			AssertEquals("One AENS50 Line", 1, aens50Lines.Count);
			AssertEquals(false, aens50Lines.Exists(x => x.HTSNumber == TariffViewAsCodeDescription.NotApplicableCode));
		}

		public void TestPrintAENS54()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var expectedEntryDate = new ZDate(2009, 06, 05);
			Declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			Factory.Save();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8703330045";
			invoiceLine1.US_SupTariff = "98178501";
			invoiceLine1.JI_LinePrice = 48000m;
			invoiceLine1.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			invoiceLine1.US_ExclusionNumber = "STL000111";
			Declaration.JE_EntryAuthorisationDate = expectedEntryDate;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var outMsg = message;
			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              " +
"10ASV9  10002333 3902            0110 X           2020711                       " +
"1191-01319900091-01319900091-013199001                      IL                  " +
"20AAAA3902      N598APL EMERALD                                                 " +
"2200000001PK                                                                    " +
"23MAAAAOBLACE69                                                                 " +
"318B 891                                                                        " +
"40  001 CAAU012411        0000000050602670000009000    N                        " +
"44COMMERCIAL DESCRIPTION                                                        " +
"47MTHLIATHA191NAK                                                               " +
"47C91-013199000                                                                 " +
"47S91-013199000                                                                 " +
"5098178501   0000000000 0000500000                                              " +
"508703330045 0000000000 0000000000             NO                               " +
"5402STL000111                                                                   " +
"6250100062500                                                                   " +
"8950100000062500                                                                " +
"9000000000000 00000062500 00000000000 00000000000 00000000000                   " +
"Y  3902SV9AE";

			var printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);

			AssertEquals("Formatted entry number", "SV9-1000233-3", printBO.FormattedEntryNumber);
			AssertEquals("EntryType", "01", printBO.EntryType);
			AssertEquals("EntryTypeCode", "ABI/A", printBO.EntryTypeCode);
			AssertEquals("SuretyCode", "891", printBO.SuretyCode);
			AssertEquals("ImportingCarrier", "APL EMERALD (AAAA)", printBO.ImportingCarrier);
			AssertEquals("USTransportMode", "10", printBO.USTransportMode);
			AssertEquals("BondType", "8", printBO.BondType);
			AssertEquals("EstimatedEntryDate needs to print the Declaration Entry Date", expectedEntryDate, printBO.EstimatedEntryDate);
			AssertEquals("EffectiveUltimateConsigneeCustomsRegNo", USConstants.Same, printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("ImporterOfRecordCustomsRegNo", "91-013199000", printBO.ImporterOfRecordCustomsRegNo);
			AssertEquals("LocationOfGoodsAndName", "N598/WANDO TERMINAL", printBO.LocationOfGoodsAndName);
			AssertEquals("SCACAndMBillNumber", "AAAAOBLACE69", printBO.SCACAndMBillNumber);
			AssertEquals("SchDArrival", "3902", printBO.SchDArrival);
			AssertEquals("SchDEntry", "3902", printBO.SchDEntry);
			AssertEquals("Block24ReferenceNumber", "91-013199001", printBO.Block24ReferenceNumber);
			AssertEquals("UniqueCountryOfOrigin", "CA", printBO.UniqueCountryOfOrigin);
			AssertEquals("UniqueCountryOfExport", "AU", printBO.UniqueCountryOfExport);
			AssertEquals("UniquePortOfLading", "60267", printBO.UniquePortOfLading);
			AssertEquals("ManufacturerID", "THLIATHA191NAK", printBO.ManufacturerID);
			AssertEquals("BrokerFileNo", "B00001000", printBO.BrokerFileNo);
			AssertEquals("TotalEnteredValue", 48000m, printBO.TotalEnteredValue);
			AssertEquals("Entry print bills count", 1, printBO.EntryPrintBills.Count);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);

			AssertEquals("Line 1 - Block29Element1", "", printBO.EntryPrintLines[0].Block29Element1);
			AssertEquals("Line 1 - Block29Element2", "", printBO.EntryPrintLines[0].Block29Element2);
			AssertEquals("Line 1 - Block29Element3", "", printBO.EntryPrintLines[0].Block29Element3);
			AssertEquals("Line 1 - fee", "0.21%", printBO.EntryPrintLines[0].MPFPercentAsString);
			AssertEquals("Line 1 - fee", 0m, printBO.EntryPrintLines[0].MPFAmount);
			AssertEquals("Line 1 - fee", "0.125%", printBO.EntryPrintLines[0].HMFPercentAsString);
			AssertEquals("Line 1 - fee", 625m, printBO.EntryPrintLines[0].HMFAmount);

			AssertEquals("Summary fee desc", "501 501 Desc from DB", printBO.SummaryFeeDesc1);
			AssertEquals("Summary fee 1", 625m, printBO.SummaryFee1);
			AssertEquals("Summary fee desc", "", printBO.SummaryFeeDesc2);
			AssertEquals("Summary fee 2", 0m, printBO.SummaryFee2);

			AssertEquals("Exclusion Number in AENS54", "Product Exclusion No: " + invoiceLine1.US_ExclusionNumber, printBO.EntryPrintLines[0].ExclusionNumber);

			AssertEquals("Should be Census Warning", USConstants.EntrySummaryDisposition.CensusWarning, printBO.SummaryStatus);
		}

		public void TestPrintSeperateAENS54()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var expectedEntryDate = new ZDate(2009, 06, 05);
			Declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			Factory.Save();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8703330045";
			invoiceLine1.US_SupTariff = "98178501";
			invoiceLine1.JI_LinePrice = 48000m;
			invoiceLine1.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			invoiceLine1.US_ExclusionNumber = "STL000111";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8703330046";
			invoiceLine2.JI_LinePrice = 54000m;
			invoiceLine2.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._03;
			invoiceLine2.US_ExclusionNumber = "ALU000333";
			Declaration.JE_EntryAuthorisationDate = expectedEntryDate;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var outMsg = message;
			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              " +
"10ASV9  10002333 3902            0110 X           2020711                       " +
"1191-01319900091-01319900091-013199001                      IL                  " +
"20AAAA3902      N598APL EMERALD                                                 " +
"2200000001PK                                                                    " +
"23MAAAAOBLACE69                                                                 " +
"318B 891                                                                        " +
"40  001 CAAU012411        0000000050602670000009000    N                        " +
"44COMMERCIAL DESCRIPTION                                                        " +
"47MTHLIATHA191NAK                                                               " +
"47C91-013199000                                                                 " +
"47S91-013199000                                                                 " +
"5098178501   0000000000 0000500000                                              " +
"508703330045 0000000000 0000000000             NO                               " +
"5402STL000111                                                                   " +
"40  002 CAAU012411        0000000050602670000009000    N                        " +
"44COMMERCIAL DESCRIPTION                                                        " +
"47MTHLIATHA191NAK                                                               " +
"47C91-013199000                                                                 " +
"47S91-013199000                                                                 " +
"508703330046 0000000000 0000000000                                              " +
"5403ALU000333                                                                   " +
"6250100062500                                                                   " +
"8950100000062500                                                                " +
"9000000000000 00000062500 00000000000 00000000000 00000000000                   " +
"Y  3902SV9AE";

			var printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);

			AssertEquals("Formatted entry number", "SV9-1000233-3", printBO.FormattedEntryNumber);
			AssertEquals("EntryType", "01", printBO.EntryType);
			AssertEquals("EntryTypeCode", "ABI/A", printBO.EntryTypeCode);
			AssertEquals("SuretyCode", "891", printBO.SuretyCode);
			AssertEquals("ImportingCarrier", "APL EMERALD (AAAA)", printBO.ImportingCarrier);
			AssertEquals("USTransportMode", "10", printBO.USTransportMode);
			AssertEquals("BondType", "8", printBO.BondType);
			AssertEquals("EstimatedEntryDate needs to print the Declaration Entry Date", expectedEntryDate, printBO.EstimatedEntryDate);
			AssertEquals("EffectiveUltimateConsigneeCustomsRegNo", USConstants.Same, printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("ImporterOfRecordCustomsRegNo", "91-013199000", printBO.ImporterOfRecordCustomsRegNo);
			AssertEquals("LocationOfGoodsAndName", "N598/WANDO TERMINAL", printBO.LocationOfGoodsAndName);
			AssertEquals("SCACAndMBillNumber", "AAAAOBLACE69", printBO.SCACAndMBillNumber);
			AssertEquals("SchDArrival", "3902", printBO.SchDArrival);
			AssertEquals("SchDEntry", "3902", printBO.SchDEntry);
			AssertEquals("Block24ReferenceNumber", "91-013199001", printBO.Block24ReferenceNumber);
			AssertEquals("UniqueCountryOfOrigin", "CA", printBO.UniqueCountryOfOrigin);
			AssertEquals("UniqueCountryOfExport", "AU", printBO.UniqueCountryOfExport);
			AssertEquals("UniquePortOfLading", "60267", printBO.UniquePortOfLading);
			AssertEquals("ManufacturerID", "THLIATHA191NAK", printBO.ManufacturerID);
			AssertEquals("BrokerFileNo", "B00001000", printBO.BrokerFileNo);
			AssertEquals("TotalEnteredValue", 102000m, printBO.TotalEnteredValue);
			AssertEquals("Entry print bills count", 1, printBO.EntryPrintBills.Count);
			AssertEquals("Entry print lines count", 2, printBO.EntryPrintLines.Count);

			AssertEquals("Line 1 - Block29Element1", "", printBO.EntryPrintLines[0].Block29Element1);
			AssertEquals("Line 1 - Block29Element2", "", printBO.EntryPrintLines[0].Block29Element2);
			AssertEquals("Line 1 - Block29Element3", "", printBO.EntryPrintLines[0].Block29Element3);
			AssertEquals("Line 1 - fee", "0.21%", printBO.EntryPrintLines[0].MPFPercentAsString);
			AssertEquals("Line 1 - fee", 0m, printBO.EntryPrintLines[0].MPFAmount);
			AssertEquals("Line 1 - fee", "0.125%", printBO.EntryPrintLines[0].HMFPercentAsString);
			AssertEquals("Line 1 - fee", 0m, printBO.EntryPrintLines[0].HMFAmount);

			AssertEquals("Summary fee desc", "501 501 Desc from DB", printBO.SummaryFeeDesc1);
			AssertEquals("Summary fee 1", 625m, printBO.SummaryFee1);
			AssertEquals("Summary fee desc", "", printBO.SummaryFeeDesc2);
			AssertEquals("Summary fee 2", 0m, printBO.SummaryFee2);

			AssertEquals("Exclusion Number in 1st AENS54", "Product Exclusion No: " + invoiceLine1.US_ExclusionNumber, printBO.EntryPrintLines[0].ExclusionNumber);
			AssertEquals("Exclusion Number in 2nd AENS54", "Product Exclusion No: " + invoiceLine2.US_ExclusionNumber, printBO.EntryPrintLines[1].ExclusionNumber);

			AssertEquals("Should be Census Warning", USConstants.EntrySummaryDisposition.CensusWarning, printBO.SummaryStatus);
		}

		public void TestPrintAllAENS54InCombinedLines()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var expectedEntryDate = new ZDate(2009, 06, 05);
			Declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			Factory.Save();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8703330045";
			invoiceLine1.US_SupTariff = "98178501";
			invoiceLine1.JI_LinePrice = 48000m;
			invoiceLine1.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			invoiceLine1.US_ExclusionNumber = "STL000111";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "99038801";
			invoiceLine2.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._03;
			invoiceLine2.US_ExclusionNumber = "ALU000333";
			invoiceLine1.JI_ParentLine = invoiceLine2.JI_LineNo;
			invoiceLine1.JI_ParentID = invoiceLine2.PK;
			invoiceLine2.US_IsParent = true;
			Declaration.JE_EntryAuthorisationDate = expectedEntryDate;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var outMsg = message;
			outMsg.EM_MessageText = "B  3902SV9AE                                               6008903              " +
"10ASV9  10002333 3902            0110 X           2020711                       " +
"1191-01319900091-01319900091-013199001                      IL                  " +
"20AAAA3902      N598APL EMERALD                                                 " +
"2200000001PK                                                                    " +
"23MAAAAOBLACE69                                                                 " +
"318B 891                                                                        " +
"40  001 CAAU012411        0000000050602670000009000    N                        " +
"44COMMERCIAL DESCRIPTION                                                        " +
"47MTHLIATHA191NAK                                                               " +
"47C91-013199000                                                                 " +
"47S91-013199000                                                                 " +
"5098178501   0000000000 0000500000                                              " +
"508703330045 0000000000 0000000000             NO                               " +
"5099038801   0000000000 0000000000                                              " +
"5402STL000111                                                                   " +
"5403ALU000333                                                                   " +
"6250100062500                                                                   " +
"8950100000062500                                                                " +
"9000000000000 00000062500 00000000000 00000000000 00000000000                   " +
"Y  3902SV9AE";

			var printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);

			AssertEquals("Formatted entry number", "SV9-1000233-3", printBO.FormattedEntryNumber);
			AssertEquals("EntryType", "01", printBO.EntryType);
			AssertEquals("EntryTypeCode", "ABI/A", printBO.EntryTypeCode);
			AssertEquals("SuretyCode", "891", printBO.SuretyCode);
			AssertEquals("ImportingCarrier", "APL EMERALD (AAAA)", printBO.ImportingCarrier);
			AssertEquals("USTransportMode", "10", printBO.USTransportMode);
			AssertEquals("BondType", "8", printBO.BondType);
			AssertEquals("EstimatedEntryDate needs to print the Declaration Entry Date", expectedEntryDate, printBO.EstimatedEntryDate);
			AssertEquals("EffectiveUltimateConsigneeCustomsRegNo", USConstants.Same, printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("ImporterOfRecordCustomsRegNo", "91-013199000", printBO.ImporterOfRecordCustomsRegNo);
			AssertEquals("LocationOfGoodsAndName", "N598/WANDO TERMINAL", printBO.LocationOfGoodsAndName);
			AssertEquals("SCACAndMBillNumber", "AAAAOBLACE69", printBO.SCACAndMBillNumber);
			AssertEquals("SchDArrival", "3902", printBO.SchDArrival);
			AssertEquals("SchDEntry", "3902", printBO.SchDEntry);
			AssertEquals("Block24ReferenceNumber", "91-013199001", printBO.Block24ReferenceNumber);
			AssertEquals("UniqueCountryOfOrigin", "CA", printBO.UniqueCountryOfOrigin);
			AssertEquals("UniqueCountryOfExport", "AU", printBO.UniqueCountryOfExport);
			AssertEquals("UniquePortOfLading", "60267", printBO.UniquePortOfLading);
			AssertEquals("ManufacturerID", "THLIATHA191NAK", printBO.ManufacturerID);
			AssertEquals("BrokerFileNo", "B00001000", printBO.BrokerFileNo);
			AssertEquals("TotalEnteredValue", 48000m, printBO.TotalEnteredValue);
			AssertEquals("Entry print bills count", 1, printBO.EntryPrintBills.Count);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);

			AssertEquals("Line 1 - Block29Element1", "", printBO.EntryPrintLines[0].Block29Element1);
			AssertEquals("Line 1 - Block29Element2", "", printBO.EntryPrintLines[0].Block29Element2);
			AssertEquals("Line 1 - Block29Element3", "", printBO.EntryPrintLines[0].Block29Element3);
			AssertEquals("Line 1 - fee", "0.21%", printBO.EntryPrintLines[0].MPFPercentAsString);
			AssertEquals("Line 1 - fee", 0m, printBO.EntryPrintLines[0].MPFAmount);
			AssertEquals("Line 1 - fee", "0.125%", printBO.EntryPrintLines[0].HMFPercentAsString);
			AssertEquals("Line 1 - fee", 625m, printBO.EntryPrintLines[0].HMFAmount);

			AssertEquals("Summary fee desc", "501 501 Desc from DB", printBO.SummaryFeeDesc1);
			AssertEquals("Summary fee 1", 625m, printBO.SummaryFee1);
			AssertEquals("Summary fee desc", "", printBO.SummaryFeeDesc2);
			AssertEquals("Summary fee 2", 0m, printBO.SummaryFee2);

			AssertEquals("Exclusion Number in AENS54", "Product Exclusion No: " + invoiceLine1.US_ExclusionNumber + "\r\n" + "Product Exclusion No: " + invoiceLine2.US_ExclusionNumber, printBO.EntryPrintLines[0].ExclusionNumber);

			AssertEquals("Should be Census Warning", USConstants.EntrySummaryDisposition.CensusWarning, printBO.SummaryStatus);
		}

		public void TestImportingCarrierFor06FTZMessage()
		{
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Declaration.US_EnableENS = true;
			Declaration.US_FTZNo = "1234567";
			var invoice = Declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("ImportingCarrier", "123 4567", printBO.ImportingCarrier);

			Declaration.US_FTZNo = "123456789";
			message = builder.PopulateMessage();
			entry.Messages.Add(incoming7501);
			Factory.Save();
			printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("ImportingCarrier", "123 456789", printBO.ImportingCarrier);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outMsg.EM_LinkUniqueID = Entry.PK;
			outMsg.EM_MessageText = "B  0708267AE                                  4601267  1   " + MQEDIMessage.MessageNumberPlaceHolder + "10A267  02008504 0708100014595   0130 XYY         60822110902                   1104-34759790004-347597900                     081011       PA                  20ATIC0708081011A888                                                            2200000840CS                                                                    23MOTOE26088202                                                                 318B 856                                                                        40  001 XQCA081011      CA0000001200     0000019051    Y                        42XQFREMED3600VAUSOI132205         0001 0001                                    44LIQUID BICARBONATE 4000                                                       47MXQHAEINC383VAU                                                               47C04-347597900                                                                 47S04-347597900                                                                 503004909170 0000000000 0000010198 000001809800KG                               OI        LIQUID BICARBONATE 4000                                               FD0100178K--POA  CADEV1225714                  XQHAEINC383VAU XQFREMED3600VAU   FD020000084000CS  0000000300BO  0000000640L                                     FD030000010198            LIQUID BICARBONATE 4000                               FD04              TONY PATTI9086030660                                          FD05LSTE621843                                                                  FD05PMNK071387                                                                  CW02     27C50                                                                  9000000000000 00000000000 00000000000 00000000000 00000000000                   Y  0708267AE";
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Entry.Messages.Add(incoming7501);
			var printBO = new ACEEntryMessage7501Print(Entry, outMsg, incoming7501, null);
			return printBO;
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.JE_TotalNoOfPacksPackType = "";
					DeclarationTestHelper.SetEntryFilerCode("SV9");
					declaration.US_EntryFilerCode = "SV9";
					declaration.US_EnableENS = true;
				}
				return declaration;
			}
		}

		CusEntryHeader entry;
		CusEntryHeader Entry
		{
			get
			{
				if (entry == null)
				{
					entry = Declaration.ActiveEntryHeaders.AddNew();
					entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
				}
				return entry;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			var referenceTesthelper = new UniversalReferenceTestDataHelper(Factory);
			referenceTesthelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			referenceTesthelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "N598", "WANDO TERMINAL", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}
	}
}
