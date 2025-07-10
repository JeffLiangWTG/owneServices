using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ACEEntryMessage7501Line))]
	sealed class ACEEntryMessage7501LineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			SetUpMergedInvoices("6103220010");
			var entry = Declaration.CustomsEntryHeaders[0];
			var eml = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, true, true, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals(true, eml.PrintInvoiceHeading);
			AssertEquals(true, eml.PrintInvoiceDetails);
			AssertEquals("001/INV1232", eml.InvoiceDetails.InvoiceNo);
		}

		public void TestDateForDutyCalculation()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 123620m;
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2402103030";
			line1.JI_InvoiceQuantity = 7947;
			line1.JI_InvoiceUQ = "L";
			line1.JI_LinePrice = 123620m;
			line1.US_UC_NKCountryOfOrigin = "AU";
			line1.US_SPI = "";
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_CustomsSecondQuantity = 1360m;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			Declaration.ActiveEntryHeaders.EntrySummaryEntry.CreateDocPrintingDetails(message.PK);
			entry.US7501DocPrintingData[0].US_DutyDate = ZDateTime.Today.AddDays(-1);

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);
			var entryPrintLine = (ACEEntryMessage7501Line)printBO.EntryPrintLines[0];
			AssertEquals("Entry Line should be null", null, entryPrintLine.line);
			AssertEquals("DateForDutyCalculation should be from Doc Data", entry.US7501DocPrintingData[0].US_DutyDate, entryPrintLine.DateForDutyCalculation);

			entry.US7501DocPrintingData[0].US_DutyDate = ZDateTime.Empty;
			printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			entryPrintLine = (ACEEntryMessage7501Line)printBO.EntryPrintLines[0];
			AssertEquals("Entry Line should be null", null, entryPrintLine.line);
			AssertEquals("DateForDutyCalculation should be from Doc Data", ZDate.Today, entryPrintLine.DateForDutyCalculation);
		}

		public void TestMPFPercentAsString()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 123620m;
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2402103030";
			line1.JI_InvoiceQuantity = 7947;
			line1.JI_InvoiceUQ = "L";
			line1.JI_LinePrice = 123620m;
			line1.JI_CustomsQuantity = 1000m;
			line1.JI_CustomsSecondQuantity = 1360m;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			message.EM_SystemCreateTimeUtc = ZDateTime.Today;
			entry.CreateDocPrintingDetails(message.PK);

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			var entryPrintLine = (ACEEntryMessage7501Line)printBO.EntryPrintLines[0];
			AssertEquals("MPF Rate as string - should be based on message sent date", "0.3464%", entryPrintLine.MPFPercentAsString);

			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 09, 27);
			printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			entryPrintLine = (ACEEntryMessage7501Line)printBO.EntryPrintLines[0];
			AssertEquals("MPF Rate as string should be based on message sent date", "0.21%", entryPrintLine.MPFPercentAsString);
		}

		public void TestADDSpecificRate()
		{
			var caseRecord = Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, "A570904091");
			if (caseRecord == null)
			{
				caseRecord = Factory.New<USCACCase>();
				caseRecord.U5_CaseNumber = "A570904091";
				caseRecord.U5_CaseStatus = "AC";
				caseRecord.U5_CaseStatusDate = new ZDateTime(2007, 4, 27);
			}

			caseRecord.CaseTariffs.DeleteAll();
			var caseTariff = caseRecord.CaseTariffs.AddNew();
			caseTariff.U9_TariffNumber = "3802100000";

			caseRecord.CaseRates.DeleteAll();
			var caseRate = caseRecord.CaseRates.AddNew();
			caseRate.U6_Unit = "KG";
			caseRate.U6_SpecificRate = 0.44m;
			caseRate.U6_EffectiveDate = new ZDateTime(2012, 11, 09);

			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 123620m;

			var invoiceLine = Declaration.InvoiceLines.AddNew();
			invoiceLine.US_ADDCaseNo = "A570904091";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDQty = 89m;
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var builder = new ACEEntrySummaryMessageBuilderForTesting(invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, true, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, message, incoming7501, null);

			AssertEquals(1, printBO.EntryPrintLines.Count);
			AssertEquals("44c/KG", printBO.EntryPrintLines[0].ADDRate);
		}

		public void TestSecondaryLine1ADDSpecificRate()
		{
			var caseRecord = Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, "A570904091");
			if (caseRecord == null)
			{
				caseRecord = Factory.New<USCACCase>();
				caseRecord.U5_CaseNumber = "A570904091";
				caseRecord.U5_CaseStatus = "AC";
				caseRecord.U5_CaseStatusDate = new ZDateTime(2007, 4, 27);
			}

			caseRecord.CaseTariffs.DeleteAll();
			var caseTariff = caseRecord.CaseTariffs.AddNew();
			caseTariff.U9_TariffNumber = "3802100000";

			caseRecord.CaseRates.DeleteAll();
			var caseRate = caseRecord.CaseRates.AddNew();
			caseRate.U6_Unit = "KG";
			caseRate.U6_SpecificRate = 0.44m;
			caseRate.U6_EffectiveDate = new ZDateTime(2012, 11, 09);

			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 123620m;

			var invoiceLine = Declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98130020";
			invoiceLine.JI_Tariff = "7113195080";
			invoiceLine.US_ADDCaseNo = "A570904091";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDQty = 89m;
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			entry.CreateDocPrintingDetails(message.PK);
			entry.US7501DocPrintingData[0].US_ADDOnParentOrChild = ADDCVDOnParentOrChild.Codes.Child;

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, message, incoming7501, null);

			AssertEquals(1, printBO.EntryPrintLines.Count);
			AssertEquals("44c/KG", printBO.EntryPrintLines[0].SecondaryLine1ADDRate);
		}

		public void TestSecondaryLine1CVDSpecificRate()
		{
			var caseRecord = Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, "C580208000");
			if (caseRecord == null)
			{
				caseRecord = Factory.New<USCACCase>();
				caseRecord.U5_CaseNumber = "C580208000";
				caseRecord.U5_CaseStatus = "AC";
				caseRecord.U5_CaseStatusDate = new ZDateTime(2007, 4, 27);
			}

			caseRecord.CaseTariffs.DeleteAll();
			var caseTariff = caseRecord.CaseTariffs.AddNew();
			caseTariff.U9_TariffNumber = "3802100000";

			caseRecord.CaseRates.DeleteAll();
			var caseRate = caseRecord.CaseRates.AddNew();
			caseRate.U6_Unit = "KG";
			caseRate.U6_SpecificRate = 0.42m;
			caseRate.U6_EffectiveDate = new ZDateTime(2012, 11, 09);

			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 123620m;

			var invoiceLine = Declaration.InvoiceLines.AddNew();
			invoiceLine.US_SupTariff = "98130020";
			invoiceLine.JI_Tariff = "7113195080";
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_CVDCaseNo = "C580208000";
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_CVDQty = 345m;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			entry.CreateDocPrintingDetails(message.PK);
			entry.US7501DocPrintingData[0].US_CVDOnParentOrChild = ADDCVDOnParentOrChild.Codes.Child;

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, message, incoming7501, null);

			AssertEquals(1, printBO.EntryPrintLines.Count);
			AssertEquals("42c/KG", printBO.EntryPrintLines[0].SecondaryLine1CVDRate);
		}

		public void TestSoftwoodLumberDetails()
		{
			declaration = null;
			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.US_TransactionsRelated = "N";
			invoiceHeader.JZ_InvoiceNumber = "INV1232";
			invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "44091020";
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber;
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(10);

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "44091020";
			AssertEquals("Precondition: is Softwood Lumber", true, invoiceLine.IsSoftwoodLumberSection804FarmBillRequirement);

			invoiceLine.JI_Weight = 345m;
			invoiceLine.JI_LinePrice = 126m;
			invoiceLine.JI_InvoiceQuantity = 28m;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsQuantity = 70m;
			invoiceLine.US_UC_NKCountryOfExport = "XA";
			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_LumberExportPrice = 16m;
			invoiceLine.US_LumberExportCharges = 100m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var builder = new ACEEntrySummaryMessageBuilderForTesting(Declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			Declaration.ActiveEntryHeaders.EntrySummaryEntry.CreateDocPrintingDetails(message.PK);
			Factory.Save();

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(Declaration.ActiveEntryHeaders.EntrySummaryEntry, message, incoming7501, null);
			var entryPrintLine = printBO.EntryPrintLines.Cast<ACEEntryMessage7501Line>().FirstOrDefault(x => x.FormattedTariff == "4409.10.20");
			AssertEquals("LumberExportPrice", 16m, entryPrintLine.LumberExportPrice);
			AssertEquals("LumberExportPrice", "Y", entryPrintLine.LumberImporterDeclaration);
			AssertEquals("LumberExportCharges", 100m, entryPrintLine.LumberExportCharges);

			message.EM_MessageText = "B  1703KC1AE                                  1704KC1  1   ALUMETJFK_10216      " +
"10AKC1  17169013 1703BATL00416   0111 XA          2061616                       " +
"11910901-0441895-452529600                     060716       GA                  " +
"20UASU1703060716L737CMA CGM MOLIERE                                             " +
"21017W                                                                          " +
"2200000285CT                                                                    " +
"23MUASUITGOA295175                                                              " +
"23HABPGH0386896                                                                 " +
"SE17UACU5570981                                                                 " +
"2200000099CT                                                                    " +
"23MUASUITGOA295175                                                              " +
"23HABPGH0386897                                                                 " +
"SE17UACU5570981                                                                 " +
"2200000034CT                                                                    " +
"23MUASUITGOA295175                                                              " +
"23HABPGH0386898                                                                 " +
"SE17UACU5570981                                                                 " +
"318B 353                                                                        " +
"SE20CESL737                                                                     " +
"40  001 ITIT052216        0000001969475310000006426    N                        " +
"47MITSMACOR1SAN                                                                 " +
"47C95-452529600                                                                 " +
"47S95-452529600                                                                 " +
"SE50MF SMAC CORNICI - SAN. COR. S.R.L.                                          " +
"SE5515VIA DELLA CASETTA 1                                                       " +
"SE56SAN GIMIGNANO                               53037          IT               " +
"SE50SE BOLOGNI ARREDA S.R.L.                                                    " +
"SE5515VIA TOSCANA 48-50                                                         " +
"SE56CERTALDO                                    50052          IT               " +
"504421909780 0000141395 0000042847             X                                " +
"OI        MOULDING                                                              " +
"PG01001APHAPL                                                                   " +
"PG02P                                                                           " +
"PG10                   MOUOLDING                                                " +
"PG04 ABIES ALBA MILL                                    000000163210M           " +
"PG05ABIES                 ALBA MILL                                             " +
"PG06HRVIT                                                                       " +
"PG04 ABIES ALBA MILL                                    000000201600M           " +
"PG05ABIES                 ALBA MILL                                             " +
"PG06HRVIT                                                                       " +
"PG04 TRIPLOCHITON SCLEROXYLON                           000000530825M           " +
"PG05TRIPLOCHITON          SCLEROXYLON                                           " +
"PG06HRVCM                                                                       " +
"PG22             IM AP6 Y06032016                                               " +
"PG21IM TORI HAMILTON          9058501500     RLEMIRE@ROMAMOULDING.COM           " +
"PG25                                                    000000042847            " +
"PG27UACU5570981                                                                 " +
"5401N0000000000                                                                 " +
"6250100005356                                                                   " +
"6249900014842                                                                   " +
"40  002 ITIT052216        0000000776475310000002531    N                        " +
"47MITDMDEC1214TOR                                                               " +
"47C95-452529600                                                                 " +
"47S95-452529600                                                                 " +
"SE50MF D.M. DECORAZIONE MOBILI DI TONIALIN                                      " +
"SE5515VIA UGO LA MALFA 12-14                                                    " +
"SE56TORRITA DI SIENA                            53049          IT               " +
"SE50SE SMAC CORNICI - SAN. COR. S.R.L.                                          " +
"SE5515VIA DELLA CASETTA 1                                                       " +
"SE56SAN GIMIGNANO                               53037          IT               " +
"504421909780 0000055694 0000016877             X                                " +
"OI        MOULDING                                                              " +
"PG01001APHAPL                                                                   " +
"PG02P                                                                           " +
"PG10                   MOULDING                                                 " +
"PG04 PICEA ABIES                                        000000383940M           " +
"PG05PICEA                 ABIES                                                 " +
"PG06HRVIT                                                                       " +
"PG04 TRIPLOCHITON SCLEROXYLON                           000000233060M           " +
"PG05TRIPLOCHITON          SCLEROXYLON                                           " +
"PG06HRVCM                                                                       " +
"PG22             IM AP6 Y06032016                                               " +
"PG21IM TORI HAMILTON          9058501500     RLEMIRE@ROMAMOULDING.COM           " +
"PG25                                                    000000016877            " +
"PG27UACU5570981                                                                 " +
"5401N0000000000                                                                 " +
"6250100002110                                                                   " +
"6249900005846                                                                   " +
"40  003 ITIT052216        0000000505475310000001647    N                        " +
"47MITDMDEC1214TOR                                                               " +
"47C95-452529600                                                                 " +
"47S95-452529600                                                                 " +
"SE50MF D.M. DECORAZIONE MOBILI DI TONIALIN                                      " +
"SE5515VIA UGO LA MALFA 12-14                                                    " +
"SE56TORRITA DI SIENA                            53049          IT               " +
"SE50SE D.M. DECORAZIONE MOBILI DI TONIALIN                                      " +
"SE5515VIA UGO LA MALFA 12-14                                                    " +
"SE56TORRITA DI SIENA                            53049          IT               " +
"504421909780 0000036247 0000010984             X                                " +
"OI        MOULDING                                                              " +
"PG01001APHAPL                                                                   " +
"PG02P                                                                           " +
"PG10                   MOULDING                                                 " +
"PG04 PINUS                                              000000312900M           " +
"PG05PINUS                 PINUS                                                 " +
"PG06HRVSE                                                                       " +
"PG22             IM AP6 Y06032016                                               " +
"PG21IM TORI HAMILTON          9058501500     RLEMIRE@ROMAMOULDING.COM           " +
"PG25                                                    000000010984            " +
"PG27UACU5570981                                                                 " +
"5401N0000000000                                                                 " +
"6250100001373                                                                   " +
"6249900003805                                                                   " +
"895010000000883949900000024493                                                  " +
"9000000233336 00000033332 00000000000 00000000000 00000000000                   " +
"Y  1703KC1AE";

			printBO = new ACEEntryMessage7501Print(Declaration.ActiveEntryHeaders.EntrySummaryEntry, message, incoming7501, null);
			entryPrintLine = (ACEEntryMessage7501Line)printBO.EntryPrintLines[0];
			AssertEquals("LumberExportPrice", 0m, entryPrintLine.LumberExportPrice);
			AssertEquals("LumberExportPrice", "N", entryPrintLine.LumberImporterDeclaration);
			AssertEquals("LumberExportCharges", 0m, entryPrintLine.LumberExportCharges);
		}

		public void TestMultiRelationships()
		{
			var msgLineDetails = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, true, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("TransRelatedInd on line should print when multiRelationships", "Y", msgLineDetails.TransRelatedInd);

			msgLineDetails = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("TransRelatedInd on line should be blank when relationship printed on Invoice", "", msgLineDetails.TransRelatedInd);
		}

		public void TestSPIAndOrSecondarySPIDetails()
		{
			//case 1
			var msgLineDetails = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("SPI from message", "M", msgLineDetails.SPIAndOrSecondarySPI);

			//case 2
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = true;
			declaration.US_ADDCVDSuretyCode = "893";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.US_TransactionsRelated = "N";
			invoiceHeader.JZ_InvoiceNumber = "INV1";
			invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8708292500";
			invoiceLine1.US_SetInd = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine1.US_UC_NKCountryOfExport = "KR";
			invoiceLine1.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine1.US_SPI = "KR";
			invoiceLine1.US_SecondarySPI = "M";

			var firstVLine = invoiceHeader.InvoiceLines.AddNew();
			firstVLine.JI_ParentID = invoiceLine1.PK;
			firstVLine.JI_Tariff = "8708292500";
			firstVLine.JI_LinePrice = 41m;
			firstVLine.US_SetInd = SecondarySpecProgIndicatorList.Codes.V;
			firstVLine.US_UC_NKCountryOfExport = "KR";
			firstVLine.US_UC_NKCountryOfOrigin = "KR";
			firstVLine.US_SPI = "KR";
			firstVLine.US_SecondarySPI = "M";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Description = "COMMERCIAL DESCRIPTION";

			invoiceLine2.JI_Weight = 9000m;
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_InvoiceQuantity = 10000m;
			invoiceLine2.JI_InvoiceUQ = "NO";
			invoiceLine2.JI_CustomsQuantity = 70m;
			invoiceLine2.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer.OH_FullName = "Mr manufacturer";
			manufacturer.OH_IsConsignor = true;
			manufacturer.OH_RL_NKClosestPort = "AUSYD";
			var manufacturerCode = manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AUSOUPAC195PAD", GlbCompany.CurrentCompany.Country);

			invoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine2.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;

			invoiceLine2.US_UC_NKCountryOfExport = "KR";
			invoiceLine2.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine2.US_SPI = "KR";
			invoiceLine2.US_SecondarySPI = "M";
			invoiceLine2.JI_Tariff = "7210490091";
			invoiceLine2.US_ADDCaseNo = "A580816000";
			invoiceLine2.US_ADDDecID = "112HY";
			invoiceLine2.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine2.US_ADDDepositValue = 560m;

			invoiceLine2.US_CVDCaseNo = "C580208000";
			invoiceLine2.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine2.US_CVDDepositValue = 345m;

			if (invoiceLine2.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A580816000", ZDateTime.BrettsBirthday, "KR", "7210490091", 0.5m, 0.123m);
			}

			if (invoiceLine2.CountervailingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("C580208000", ZDateTime.BrettsBirthday, "KR", "7210490091", 0.5m, 0.456m);
			}

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			message.EM_SystemCreateTimeUtc = ZDateTime.Today;
			entry.CreateDocPrintingDetails(message.PK);

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			var entryPrintLine = printBO.EntryPrintLines.Cast<ACEEntryMessage7501Line>().FirstOrDefault(x => x.FormattedTariff == "7210.49.0091");
			AssertEquals("KR.M", entryPrintLine.SPIAndOrSecondarySPI);

			entryPrintLine = printBO.EntryPrintLines.Cast<ACEEntryMessage7501Line>().FirstOrDefault(x => x.FormattedTariff == "8708.29.2500" && x.LineNumber == "001");
			AssertEquals("KR.M,X", entryPrintLine.SPIAndOrSecondarySPI);

			entryPrintLine = printBO.EntryPrintLines.Cast<ACEEntryMessage7501Line>().FirstOrDefault(x => x.FormattedTariff == "8708.29.2500" && x.LineNumber == "002");
			AssertEquals("KR.M,V", entryPrintLine.SPIAndOrSecondarySPI);
		}

		[TestDate(2008, 3, 25)]
		public void TestWineExciseTaxPrintsFromMessage()
		{
			SetUpMergedInvoices("2204215030");

			var msgLineDetails = new ACEEntryMessage7501Line(ExciseMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("Wine Tax", 46.31m, msgLineDetails.LineFeeAmount);
			AssertEquals("HMF", 107.26m, msgLineDetails.HMFAmount);
			AssertEquals("MPF", 180.19m, msgLineDetails.MPFAmount);
			AssertEquals(true, msgLineDetails.HasMPF);
			AssertEquals("SPI from message", "", msgLineDetails.SPIAndOrSecondarySPI);
		}

		[TestDate(2008, 3, 25)]
		public void TestBeerExciseTaxPrintsFromMessage()
		{
			SetUpMergedInvoices("2203000060");

			var msgLineDetails = new ACEEntryMessage7501Line(ExciseMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("Other Excise Tax (Beer)", 46.31m, msgLineDetails.LineFeeAmount);
		}

		[TestDate(2008, 3, 25)]
		public void TestTobaccoExciseTaxPrintsFromMessage()
		{
			SetUpMergedInvoices("2402103030");

			var msgLineDetails = new ACEEntryMessage7501Line(ExciseMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, ZString.Empty);
			AssertEquals("Tobacco Excise", 46.31m, msgLineDetails.LineFeeAmount);
		}

		public void TestCustomsQuantity()
		{
			EntryMessageLine.aens50.Clear();
			EntryMessageLine.aens50.Add(new AENS50() { Quantity1 = "1500", UnitOfMeasureCode1 = Core.Constants.Weight.Kilograms });
			var ens7501Line = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, true, true, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals(15m, ens7501Line.CustomsQuantity);
		}

		public void TestSecondaryLine1SecondQtyAndUQ()
		{
			AddBlock50("1000");
			AddBlock50("1100");
			AddBlock50("1200");
			AddBlock50("1300");
			AddBlock50("1400");
			AddBlock50("1500");

			var ens7501Line = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, true, true, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("10 KG", ens7501Line.SecondaryLine2SecondQtyAndUQ);
			AssertEquals("11 KG", ens7501Line.SecondaryLine3SecondQtyAndUQ);
			AssertEquals("12 KG", ens7501Line.SecondaryLine4SecondQtyAndUQ);
			AssertEquals("13 KG", ens7501Line.SecondaryLine5SecondQtyAndUQ);
			AssertEquals("14 KG", ens7501Line.SecondaryLine6SecondQtyAndUQ);
			AssertEquals("15 KG", ens7501Line.SecondaryLine7SecondQtyAndUQ);
		}

		public void TestSecondaryLine1SecondQtyAndUQ_WhereThereAreNoSecondaryLines()
		{
			var ens7501Line = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, true, true, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("", ens7501Line.SecondaryLine1SecondQtyAndUQ);
			AssertEquals("", ens7501Line.SecondaryLine2SecondQtyAndUQ);
			AssertEquals("", ens7501Line.SecondaryLine3SecondQtyAndUQ);
			AssertEquals("", ens7501Line.SecondaryLine4SecondQtyAndUQ);
			AssertEquals("", ens7501Line.SecondaryLine5SecondQtyAndUQ);
			AssertEquals("", ens7501Line.SecondaryLine6SecondQtyAndUQ);
			AssertEquals("", ens7501Line.SecondaryLine7SecondQtyAndUQ);
		}

		public void TestSorghumAmount()
		{
			SetUpMergedInvoices("8211100001");
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.MergedLines[0].Fees.SetAmount(Core.Constants.USCustoms.FeeCodes.Sorghum, 8.52m);

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);
			var entryPrintLine = printBO.EntryPrintLines[0];
			AssertEquals(8.52m, entryPrintLine.LineFeeAmount);
		}

		public void TestHideSecondaryLine1SecondQtyLineTrue()
		{
			var ens7501Line = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("HideSecondaryLine1SecondQtyLine - no qty, no ADD details", true, ens7501Line.HideSecondaryLine1SecondQtyLine);
		}

		public void TestHideSecondaryLine1SecondQtyLineFalse()
		{
			EntryMessageLine.aens50.Add(new AENS50());
			EntryMessageLine.aens50[1].Quantity2 = "120";
			EntryMessageLine.aens50[1].UnitOfMeasureCode2 = "KG";
			var msgLineDetails = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("HideSecondaryLine1SecondQtyLine - ADD details exist", false, msgLineDetails.HideSecondaryLine1SecondQtyLine);
		}

		public void TestADD_CVDDetails()
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
			invoiceLine2.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

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
			AssertEquals("ADDSpecificDepositValueFormatted", "", entryPrintLine1.ADDSpecificDepositValueFormatted);

			var entryPrintLine2 = (ACEEntryMessage7501Line)printBO.EntryPrintLines[1];
			AssertEquals("ADD/CVD", "A475-818-001", entryPrintLine2.ADDNo);
			AssertEquals("ADDSpecificDepositValueFormatted", "(300)", entryPrintLine2.ADDSpecificDepositValueFormatted);
			AssertEquals("ADD/CVD", "C475-819-017", entryPrintLine2.CVDNo);
			AssertEquals("ADDSpecificDepositValueFormatted", "", entryPrintLine2.CVDSpecificDepositValueFormatted);

			var entryPrintLine3 = (ACEEntryMessage7501Line)printBO.EntryPrintLines[2];
			AssertEquals("ADD/CVD", "", entryPrintLine3.ADDNo);
			AssertEquals("ADDSpecificDepositValueFormatted", "", entryPrintLine3.ADDSpecificDepositValueFormatted);
			AssertEquals("ADD/CVD", "C357-109-000", entryPrintLine3.CVDNo);
			AssertEquals("ADDSpecificDepositValueFormatted", "", entryPrintLine3.CVDSpecificDepositValueFormatted);
		}

		public void TestSecondaryLine1ADDDetails()
		{
			AssertNotNull(EntryMessageLine);
			docData.US_ADDOnParentOrChild = "C";
			var msgLineDetails = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, false, false, "444", rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("SecondaryLine1ADDNo", "A475-818-001", msgLineDetails.SecondaryLine1ADDNo);
			AssertEquals("SecondaryLine1ADDSurety", "Surety Code #444", msgLineDetails.SecondaryLine1ADDSurety);
			AssertEquals("Line1ADDSurety should be blank as ADD is on secondary line", "", msgLineDetails.ADDSurety);
			AssertEquals("SecondaryLine1ADDSpecificDepositValueFormatted", "(560)", msgLineDetails.SecondaryLine1ADDSpecificDepositValueFormatted);
			AssertEquals("SecondaryLine1ADDRate", "50.00%", msgLineDetails.SecondaryLine1ADDRate);
			AssertEquals("SecondaryLine1ADDFormatted", "5280.00", msgLineDetails.SecondaryLine1ADDFormatted);
		}

		[ExpectNoExceptions]
		public void TestSecondaryLine1ADDNoWhenNoSecondaryLine()
		{
			var msgLineDetails = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("SecondaryLine1ADDNo should be blank and not cause exception when no secondary line present", "", msgLineDetails.SecondaryLine1ADDNo);
		}

		public void TestSecondaryLine1ADDNoWhenSupplementaryTariff()
		{
			SetUpMergedInvoicesWithSupplementaryTariff();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var message = (MQEDIMessage)entry.Messages[0];
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("SecondaryLine1ADDNo", "", printBO.EntryPrintLines[0].SecondaryLine1ADDNo);
		}

		public void TestSecondaryLine1CVDDetails()
		{
			AssertNotNull(EntryMessageLine);
			docData.US_CVDOnParentOrChild = "C";
			var msgLineDetails = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, false, false, "444", rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("SecondaryLine1CVDNo", "C475-819-017", msgLineDetails.SecondaryLine1CVDNo);
			AssertEquals("SecondaryLine1CVDSurety", "Surety Code #444", msgLineDetails.SecondaryLine1CVDSurety);
			AssertEquals("SecondaryLine1CVDSpecificDepositValueFormatted", "(345)", msgLineDetails.SecondaryLine1CVDSpecificDepositValueFormatted);
			AssertEquals("SecondaryLine1CVDRate", "50.00%", msgLineDetails.SecondaryLine1CVDRate);
			AssertEquals("SecondaryLine1CVDFormatted", "5172.50", msgLineDetails.SecondaryLine1CVDFormatted);
		}

		public void TestAllOtherSecondaryLinesNullReference()
		{
			SetUpMergedInvoices("8211100001");
			var msgLineDetails = new ACEEntryMessage7501Line(new EntryMessageLine(Factory), false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("Accessing SecondaryLine3DutyAmount should not cause exception", 0m, msgLineDetails.SecondaryLine3DutyAmount);
			AssertEquals("Accessing SecondaryLine5DutyAmount should not cause exception", 0m, msgLineDetails.SecondaryLine5DutyAmount);
			AssertEquals("Accessing SecondaryLine7DutyAmount should not cause exception", 0m, msgLineDetails.SecondaryLine7DutyAmount);
		}

		public void TestLicenses()
		{
			var msgLineDetails = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, true, true, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("Licence should blank", "", msgLineDetails.LicenseNumber);
			AssertEquals("Licence Text should be blank", "", msgLineDetails.LicenseText);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.US_TransactionsRelated = "N";
			invoiceHeader.JZ_InvoiceNumber = "INV1";
			invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "7210490091";
			invoiceLine1.US_UC_NKCountryOfExport = "KR";
			invoiceLine1.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine1.US_SPI = "KR";
			invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._01, "u9arfb072");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			message.EM_SystemCreateTimeUtc = ZDateTime.Today;
			entry.CreateDocPrintingDetails(message.PK);
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			var entryPrintLine = printBO.EntryPrintLines.Cast<ACEEntryMessage7501Line>().FirstOrDefault(x => x.FormattedTariff == "7210.49.0091");
			AssertEquals("01-U9ARFB072", entryPrintLine.LicenseNumber);

			invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._14, "001AAA");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			message.EM_SystemCreateTimeUtc = ZDateTime.Today;
			entry.CreateDocPrintingDetails(message.PK);

			printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			entryPrintLine = printBO.EntryPrintLines.Cast<ACEEntryMessage7501Line>().FirstOrDefault(x => x.FormattedTariff == "7210.49.0091");
			AssertEquals("01-U9ARFB072\r\n14-001AAA", entryPrintLine.LicenseNumber);

			invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._17, "");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			message.EM_SystemCreateTimeUtc = ZDateTime.Today;
			entry.CreateDocPrintingDetails(message.PK);

			printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			entryPrintLine = printBO.EntryPrintLines.Cast<ACEEntryMessage7501Line>().FirstOrDefault(x => x.FormattedTariff == "7210.49.0091");
			AssertEquals("01-U9ARFB072\r\n14-001AAA", entryPrintLine.LicenseNumber);
		}

		public void TestVisaCertificateNumber()
		{
			var msgLineDetails = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, true, true, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("Visa should blank", "", msgLineDetails.VisaCertificateNumber);
			AssertEquals("SPI from message", "M", msgLineDetails.SPIAndOrSecondarySPI);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.US_TransactionsRelated = "N";
			invoiceHeader.JZ_InvoiceNumber = "INV1";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "7210490091";
			invoiceLine1.US_UC_NKCountryOfExport = "KR";
			invoiceLine1.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine1.US_SPI = "KR";
			invoiceLine1.US_VisaNo = "1CH001045";
			invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._01, "u9arfb072");
			var cottonCert = invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._12, "CTN01789");
			var cottonCert2 = invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._23, "AMSCERT45");
			var sugCert = invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._16, "CA_SG9890");
			invoiceLine1.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._18, "CBT178010");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			message.EM_SystemCreateTimeUtc = ZDateTime.Today;
			entry.CreateDocPrintingDetails(message.PK);

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			var entryPrintLine = printBO.EntryPrintLines.Cast<ACEEntryMessage7501Line>().FirstOrDefault(x => x.FormattedTariff == "7210.49.0091");
			AssertEquals("VisaCertificateNumber", "V 1CH001045", entryPrintLine.VisaCertificateNumber);

			invoiceLine1.US_VisaNo = ZString.Empty;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			message = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add).PopulateMessage();
			message.EM_SystemCreateTimeUtc = ZDateTime.Today;
			entry.CreateDocPrintingDetails(message.PK);

			printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			entryPrintLine = printBO.EntryPrintLines.Cast<ACEEntryMessage7501Line>().FirstOrDefault(x => x.FormattedTariff == "7210.49.0091");
			AssertEquals("VisaCertificateNumber", "C CTN01789, AMSCERT45", entryPrintLine.VisaCertificateNumber);

			invoiceLine1.LicenceAndPermits.RemoveAndDelete(cottonCert);
			invoiceLine1.LicenceAndPermits.RemoveAndDelete(cottonCert2);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			message = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add).PopulateMessage();
			message.EM_SystemCreateTimeUtc = ZDateTime.Today;
			entry.CreateDocPrintingDetails(message.PK);

			printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			entryPrintLine = printBO.EntryPrintLines.Cast<ACEEntryMessage7501Line>().FirstOrDefault(x => x.FormattedTariff == "7210.49.0091");
			AssertEquals("VisaCertificateNumber", "C CA_SG9890", entryPrintLine.VisaCertificateNumber);

			invoiceLine1.LicenceAndPermits.RemoveAndDelete(sugCert);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			message = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add).PopulateMessage();
			message.EM_SystemCreateTimeUtc = ZDateTime.Today;
			entry.CreateDocPrintingDetails(message.PK);

			printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			entryPrintLine = printBO.EntryPrintLines.Cast<ACEEntryMessage7501Line>().FirstOrDefault(x => x.FormattedTariff == "7210.49.0091");
			AssertEquals("VisaCertificateNumber", "C CBT178010", entryPrintLine.VisaCertificateNumber);
		}

		[TestDate(2009, 12, 02)]
		public void TestSecondaryLineDutyForNonWatchOSAssembledGoods()
		{
			CreateOSAssembledGoodsDeclaration();
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();
			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);

			AssertEquals("Entry print lines count", 2, printBO.EntryPrintLines.Count);
			AssertEquals("Parent 1 Tariff number", "9802.00.8068", printBO.EntryPrintLines[0].FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "6203.43.4030", printBO.EntryPrintLines[0].SecondaryLine1FormattedTariff);
			AssertEquals("Formatted Tariff Line 2", "9801.00.1010", printBO.EntryPrintLines[1].FormattedTariff);

			var entryLine = printBO.EntryPrintLines[0];

			AssertEquals("Box 37 Total Duty should remain as actual Duty", 687.74m, printBO.TotalDutyAmt);

			CusEntryLine entryLine1 = null;
			foreach (var cusLine in entry.EntryLines)
			{
				entryLine1 = cusLine;
			}

			AssertEquals("Line 1 Actual Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("Line 1 Assembled Component Duty to print on 7501 should be same", 0m, printBO.EntryPrintLines[0].DutyAmount);
			AssertEquals("Line 1 Assembled Component Duty Rate to print on 7501", "Free", printBO.EntryPrintLines[0].DutyPercentAsString);

			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 687.74m, printBO.EntryPrintLines[0].SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "27.90%", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);
		}

		[TestDate(2019, 02, 25)]
		public void TestPrintDutyAmountFromEntryLineFor9808003000()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_TransportMode = "SEA";
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_CertifyCargoRelease = true;
			Declaration.US_SchDLoading = "58201";
			Declaration.US_SchDArrival = "3901";

			var invoice1 = Declaration.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.US_SupTariff = "9808003000";
			line1.JI_Tariff = "8457100075";
			line1.JI_LinePrice = 2000m;

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

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals(84m, printBO.TotalDutyAmt);
			AssertEquals(0m, printBO.Block40Total);
			AssertEquals("Entry print lines count", 2, printBO.DocPrintingData.Count);
			var printingData = printBO.DocPrintingData[1];
			AssertEquals(84m, printingData.US_DutyAmount);
			AssertEquals("4.2%", printingData.US_RateAsString);
			Assert(printingData.US_IsAdditionalTotalPrintingDuty);
		}

		[TestDate(2013, 08, 13)]
		public void TestSecondaryLineCompoundDutyRateForWI00046587()
		{
			CreateDeclarationForWI00046587();
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);
			var entryLine1 = printBO.EntryPrintLines[0];

			AssertEquals("Box 37 Total Duty should be actual Duty", 2752.50m, printBO.TotalDutyAmt);

			AssertEquals("Line 1 Duty to print on 7501", 0m, entryLine1.DutyAmount);
			AssertEquals("Line 1 Duty Rate to print on 7501", "Free", entryLine1.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Compound Duty to print on 7501 should be total duty", 2752.50m, entryLine1.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Compound Duty Rate to print on 7501", "18.35%", entryLine1.SecondaryLine1DutyPercentAsString);
		}

		public void TestNoExceptionThownOnSecondaryLine1FormattedTariff()
		{
			var ens7501Line = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, true, true, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("8205.20.3000", ens7501Line.SecondaryLine1FormattedTariff);

			EntryMessageLine.aens50.Add(new AENS50());
			EntryMessageLine.aens50[1].HTSNumber = "77755";
			EntryMessageLine.aens50[1].UnitOfMeasureCode2 = "KG";

			ens7501Line = new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, true, true, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
			AssertEquals("7775.5", ens7501Line.SecondaryLine1FormattedTariff);
		}

		public void TestBlock29Elements()
		{
			CreateOSAssembledGoodsDeclaration();
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription;
			var invoiceLine = Declaration.Invoices[0].InvoiceLines.AddNew();

			var declarationImporter = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			declarationImporter.OH_RL_NKClosestPort = "USLAX";
			Declaration.JE_OH_Importer = declarationImporter.PK;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.ImporterWrapper.ZO_ENSPrintProduct = true;

			invoiceLine.JI_CustomAttrib1 = "Attribute Text 1";
			Declaration.ImporterWrapper.ZO_ENSPrintCustomAttrib1 = true;
			invoiceLine.JI_CustomAttrib2 = "Attribute Text 2";
			Declaration.ImporterWrapper.ZO_ENSPrintCustomAttrib2 = false;
			invoiceLine.JI_CustomAttrib3 = ZString.Empty;
			Declaration.ImporterWrapper.ZO_ENSPrintCustomAttrib3 = true;

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "Test Product";
			product1.RelatedOrganisations.AddOwner(declarationImporter);

			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "10000000";
			invoiceLine.JI_PartNo = product1.OP_PartNum;
			invoiceLine.US_TextileCategoryNo = "687";
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Precondition: RulingNo", "45678", entry.MergedLines[0].RandomLine.US_PIRPRulingNo);
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			entry.MergedLines[0].RandomLine.US_PIRPRulingNo = "";
			Factory.Save();
			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);

			AssertEquals("Entry print lines count", 3, printBO.EntryPrintLines.Count);
			AssertEquals("Formatted Tariff Line 1", "9802.00.8068", printBO.EntryPrintLines[0].FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "6203.43.4030", printBO.EntryPrintLines[0].SecondaryLine1FormattedTariff);
			AssertEquals("BindingRulingWithLabel expected with this line", "RLNG 45678", printBO.EntryPrintLines[0].BindingRulingWithLabel);
			AssertEquals("Formatted Tariff Line 2", "9801.00.1010", printBO.EntryPrintLines[1].FormattedTariff);
			AssertEquals("BindingRulingWithLabel not expected on this line", "", printBO.EntryPrintLines[1].BindingRulingWithLabel);

			AssertEquals("TextileCategoryNumberWithLabel should be shown", "CAT 687", printBO.EntryPrintLines[2].Block29Element1);
			AssertEquals(product1.OP_PartNum, printBO.EntryPrintLines[2].Block29Element2);

			Assert("JI_CustomAttrib1 exists", !invoiceLine.JI_CustomAttrib1.IsEmpty);
			AssertEquals("JI_CustomAttrib1 in block29Elements", invoiceLine.JI_CustomAttrib1, "Attribute Text 1");
			AssertEquals("JI_CustomAttrib1 in block29Elements", invoiceLine.JI_CustomAttrib1, printBO.EntryPrintLines[2].Block29Element3);
			Assert("JI_CustomAttrib2 exists", !invoiceLine.JI_CustomAttrib2.IsEmpty);
			AssertEquals("JI_CustomAttrib2 not in block29Elements", ZString.Empty, printBO.EntryPrintLines[2].Block29Element4);
			Assert("JI_CustomAttrib3 doesn't exist", invoiceLine.JI_CustomAttrib3.IsEmpty);
			AssertEquals("JI_CustomAttrib3 not in block29Elements", ZString.Empty, printBO.EntryPrintLines[2].Block29Element5);
		}

		[ExpectNoExceptions]
		public void TestBlock29ElementsWithoutException()
		{
			CreateOSAssembledGoodsDeclaration();
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMergeUsingProductNumberInDescription;
			var invoiceLine = Declaration.Invoices[0].InvoiceLines.AddNew();

			var declarationImporter = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true));
			declarationImporter.OH_RL_NKClosestPort = "USLAX";
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			invoiceLine.JI_CustomAttrib1 = "Attribute Text 1";
			invoiceLine.JI_CustomAttrib2 = "Attribute Text 2";
			invoiceLine.JI_CustomAttrib3 = ZString.Empty;

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "Test Product";
			product1.RelatedOrganisations.AddOwner(declarationImporter);

			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "10000000";
			invoiceLine.JI_PartNo = product1.OP_PartNum;
			invoiceLine.US_TextileCategoryNo = "687";
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Precondition: RulingNo", "45678", entry.MergedLines[0].RandomLine.US_PIRPRulingNo);
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			entry.MergedLines[0].RandomLine.US_PIRPRulingNo = "";
			Factory.Save();
			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);

			AssertEquals("Entry print lines count", 3, printBO.EntryPrintLines.Count);
			AssertEquals("Formatted Tariff Line 1", "9802.00.8068", printBO.EntryPrintLines[0].FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "6203.43.4030", printBO.EntryPrintLines[0].SecondaryLine1FormattedTariff);
			AssertEquals("BindingRulingWithLabel expected with this line", "RLNG 45678", printBO.EntryPrintLines[0].BindingRulingWithLabel);
			AssertEquals("Formatted Tariff Line 2", "9801.00.1010", printBO.EntryPrintLines[1].FormattedTariff);
			AssertEquals("BindingRulingWithLabel not expected on this line", "", printBO.EntryPrintLines[1].BindingRulingWithLabel);

			AssertEquals("TextileCategoryNumberWithLabel should be shown", "CAT 687", printBO.EntryPrintLines[2].Block29Element1);
			AssertEquals("", printBO.EntryPrintLines[2].Block29Element2);

			Assert("JI_CustomAttrib1 exists", !invoiceLine.JI_CustomAttrib1.IsEmpty);
			AssertEquals("JI_CustomAttrib1 in block29Elements", invoiceLine.JI_CustomAttrib1, "Attribute Text 1");
			AssertEquals("JI_CustomAttrib1 in block29Elements", "", printBO.EntryPrintLines[2].Block29Element3);
			Assert("JI_CustomAttrib2 exists", !invoiceLine.JI_CustomAttrib2.IsEmpty);
			AssertEquals("JI_CustomAttrib2 not in block29Elements", ZString.Empty, printBO.EntryPrintLines[2].Block29Element4);
			Assert("JI_CustomAttrib3 doesn't exist", invoiceLine.JI_CustomAttrib3.IsEmpty);
			AssertEquals("JI_CustomAttrib3 not in block29Elements", ZString.Empty, printBO.EntryPrintLines[2].Block29Element5);
		}

		[TestDate(2017, 12, 15)]
		public void TestCoffeeFeeOnChildLine()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_ShortDescription = "Coffee Fee Test";
			tariff.UE_Unit1 = "KG";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Coffee;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;
			Factory.Save();

			CreateCoffeeFeeDeclaration(tariff.UE_Tariff);
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryLine = entry.MergedLines[0];
			var coffeeFee = entryLine.Fees.Cast<IFee>().FirstOrDefault(x => x.Code == Core.Constants.USCustoms.FeeCodes.Coffee);
			AssertNotNull(coffeeFee);
			AssertEquals(420m, coffeeFee.Amount);

			var entryPrintLine = new ACEEntryHeaderENS7501Line(entryLine, false, false);
			AssertEquals(entryPrintLine.LineFeeAmount, 420m);
			Assert(entryPrintLine.LineFeeDescription.Contains(Core.Constants.USCustoms.FeeCodes.Coffee + " DESC FROM DB"));

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);
			var entryLine1 = printBO.EntryPrintLines[0];
			AssertEquals(entryLine1.LineFeeAmount, 420m);
			Assert(entryLine1.LineFeeDescription.Contains(Core.Constants.USCustoms.FeeCodes.Coffee + " DESC FROM DB"));
		}

		[TestDate(2010, 09, 28)]
		public void TestCottonFeeOnChildLine()
		{
			CreateCottonFeeDeclaration();
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			entry.MergedLines[2].Fees.SetAmount(Core.Constants.USCustoms.FeeCodes.Cotton, 0);
			entry.MergedLines[1].Fees.SetAmount(Core.Constants.USCustoms.FeeCodes.Cotton, 0);
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("Entry print lines count", 2, printBO.EntryPrintLines.Count);
			var entryLine1 = printBO.EntryPrintLines[0];
			var entryLine2 = printBO.EntryPrintLines[1];

			AssertEquals("Parent 1 Tariff Number", "6104.22.0010", entryLine1.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "6102.20.0020", entryLine1.SecondaryLine1FormattedTariff);
			AssertEquals("Parent 2 Tariff Number", "6104.22.0040", entryLine2.FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "6104.62.2028", entryLine2.SecondaryLine1FormattedTariff);

			AssertEquals("Box 37 Total Duty", 1525.26m, printBO.TotalDutyAmt);

			AssertEquals("Line 1 Duty to print on 7501", 795.16m, entryLine1.DutyAmount);
			AssertEquals("Line 1 Duty Rate to print on 7501", "15.9%", entryLine1.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Duty to print on 7501", 0m, entryLine1.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Duty Rate to print on 7501", "Free", entryLine1.SecondaryLine1DutyPercentAsString);

			AssertEquals("Line 1 MPF", 10.50m, entryLine1.MPFAmount);
			AssertEquals(true, entryLine1.HasMPF);
			AssertEquals("Line 1 MPF Rate", "0.21%", entryLine1.MPFPercentAsString);
			AssertEquals("Line 1 HMF", 6.25m, entryLine1.HMFAmount);
			AssertEquals("Line 1 HMF Rate", "0.125%", entryLine1.HMFPercentAsString);
			AssertEquals("Line 1 Fee Desc", "056 056 DESC FROM DB", entryLine1.LineFeeDescription);
			AssertEquals("Line 1 Fee", 27.42m, entryLine1.LineFeeAmount);
			AssertEquals("Line 1 Fee Rate", "0.8345c/KG", entryLine1.LineFeePercentAsString);

			AssertEquals("Line 2 Fee Desc", "056 056 DESC FROM DB", entryLine2.LineFeeDescription);
			AssertEquals("Line 2 Fee", 45.2m, entryLine2.LineFeeAmount);
			AssertEquals("Line 2 Fee Rate", "0.8889c/KG", entryLine2.LineFeePercentAsString);
		}

		[TestDate(2008, 12, 31)]
		public void TestDutyPercentAsString()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5203003000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;
			tariff.UE_SPICode = "AU";

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_AdValoremSpecialRate = 9999.99990000m;
			dutyRate.UD_ISOCountryCode = "AU";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "AU";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			var invoiceLine5 = declaration.InvoiceLines.AddNew();
			var invoiceLine6 = declaration.InvoiceLines.AddNew();
			var invoiceLine7 = declaration.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			invoiceLine1.JI_Tariff = "2001903800";
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_CustomsQuantity = 15000m;

			invoiceLine2.JI_Tariff = "1105100000";
			invoiceLine2.JI_LinePrice = 23500m;
			invoiceLine2.JI_CustomsQuantity = 300m;

			invoiceLine3.JI_Tariff = "0901210060";
			invoiceLine3.JI_LinePrice = 500m;
			invoiceLine3.JI_CustomsQuantity = 100m;

			invoiceLine4.JI_Tariff = "0403105000";
			invoiceLine4.JI_LinePrice = 100m;
			invoiceLine4.JI_CustomsQuantity = 100m;

			invoiceLine5.JI_Tariff = "9615113000";
			invoiceLine5.JI_LinePrice = 100m;
			invoiceLine5.JI_CustomsQuantity = 100m;

			invoiceLine6.US_SupTariff = "99135220";
			invoiceLine6.JI_InvoiceQuantity = 0m;
			invoiceLine6.JI_LinePrice = 0m;
			invoiceLine6.JI_CustomsQuantity = 2000m;
			invoiceLine6.JI_Weight = 0m;
			invoiceLine6.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine6.US_SPI = "AU";

			invoiceLine7.JI_Tariff = "5203003000";
			invoiceLine7.JI_LinePrice = 5000m;
			invoiceLine7.JI_CustomsQuantity = 2000m;
			invoiceLine7.JI_Weight = 1250m;
			invoiceLine7.US_SPI = "AU";// made duty free

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];
			var entryLine2 = declaration.ActiveEntryHeaders[0].MergedLines[1];
			var entryLine3 = declaration.ActiveEntryHeaders[0].MergedLines[2];
			var entryLine4 = declaration.ActiveEntryHeaders[0].MergedLines[3];
			var entryLine5 = declaration.ActiveEntryHeaders[0].MergedLines[4];
			var entryLine6 = declaration.ActiveEntryHeaders[0].MergedLines[5];
			var entryLine7 = declaration.ActiveEntryHeaders[0].MergedLines[6];

			AssertEquals("DutyPercentAsString", "9.6%", entryLine1.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "1.7c/KG", entryLine2.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "Free", entryLine3.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "$1.035/KG + 17%", entryLine4.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "28.8c/GR + 4.6%", entryLine5.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "24.3c/KG", entryLine6.CL_DutyPercentAsString);

			var builder = new ACEEntrySummaryMessageBuilderForTesting(declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(incoming7501);
			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(declaration.ActiveEntryHeaders.EntrySummaryEntry, message, incoming7501, null);
			AssertEquals("DutyPercentAsString calculated from message on 7501", "9.6%", printBO.EntryPrintLines[0].DutyPercentAsString);
			AssertEquals("DutyPercentAsString calculated from message on 7501", "1.7c/KG", printBO.EntryPrintLines[1].DutyPercentAsString);
			AssertEquals("DutyPercentAsString calculated from message on 7501", "Free", printBO.EntryPrintLines[2].DutyPercentAsString);
			AssertEquals("DutyPercentAsString calculated from message on 7501", "$1.035/KG + 17%", printBO.EntryPrintLines[3].DutyPercentAsString);
			AssertEquals("DutyPercentAsString calculated from message on 7501", "28.8c/GR + 4.6%", printBO.EntryPrintLines[4].DutyPercentAsString);
			AssertEquals("DutyPercentAsString calculated from message on 7501", "24.3c/KG", printBO.EntryPrintLines[5].DutyPercentAsString);
			AssertEquals("DutyPercentAsString calculated from message on 7501 should be blank not free in this case", "", printBO.EntryPrintLines[6].DutyPercentAsString);
		}

		[TestDate(2008, 12, 31)]
		public void TestDutyPercentAsStringExtra()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "GB";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			invoiceLine1.JI_Tariff = "6402917060";
			invoiceLine1.JI_LinePrice = 31500m;
			invoiceLine1.JI_CustomsQuantity = 9000m;
			invoiceLine1.JI_Weight = 5000m;
			invoiceLine1.US_UC_NKCountryOfOrigin = "GB";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var builder = new ACEEntrySummaryMessageBuilderForTesting(declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();

			Factory.Save();

			var entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];

			AssertEquals("DutyPercentAsString", "90c/PRS + 37.5%", entryLine1.CL_DutyPercentAsString);

			var entry = declaration.CustomsEntryHeaders[0];

			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("DutyPercentAsString calculated from message on 7501", "90c/PRS + 37.5%", printBO.EntryPrintLines[0].DutyPercentAsString);
		}

		[TestDate(2010, 04, 16)]
		public void TestDutyPercentAsStringWhenMsgDoesNotMatchEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_RL_NKPortOfLoading = "SGSIN";
			declaration.JE_ExportDate = new ZDateTime(2010, 04, 07);
			declaration.US_EntryDate = new ZDateTime(2010, 04, 15);
			declaration.US_PaymentType = "3";
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2010, 04, 28);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2554.40m;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_InvoiceCurrExRate = 1m;
			invoice.US_UC_NKCountryOfOrigin = "SG";
			invoice.US_UC_NKCountryOfExport = "KR";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			var invoiceLine5 = declaration.InvoiceLines.AddNew();
			var invoiceLine6 = declaration.InvoiceLines.AddNew();
			var invoiceLine7 = declaration.InvoiceLines.AddNew();
			var invoiceLine8 = declaration.InvoiceLines.AddNew();
			var invoiceLine9 = declaration.InvoiceLines.AddNew();
			var invoiceLine10 = declaration.InvoiceLines.AddNew();
			var invoiceLine11 = declaration.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			invoiceLine1.JI_Tariff = "6109.90.8030";
			invoiceLine1.JI_LinePrice = 23.25m;
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.JI_CustomsUnitQty = "DOZ";
			invoiceLine1.JI_CustomsSecondQuantity = 5m;
			invoiceLine1.JI_CustomsSecondUnitQty = "KG";

			invoiceLine2.JI_Tariff = "6104.69.8038";
			invoiceLine2.JI_LinePrice = 77.60m;
			invoiceLine2.JI_CustomsQuantity = 1m;
			invoiceLine2.JI_CustomsUnitQty = "DOZ";
			invoiceLine2.JI_CustomsSecondQuantity = 5m;
			invoiceLine2.JI_CustomsSecondUnitQty = "KG";

			invoiceLine3.JI_Tariff = "6109.90.8030";
			invoiceLine3.JI_LinePrice = 99m;
			invoiceLine3.JI_CustomsQuantity = 2m;
			invoiceLine3.JI_CustomsUnitQty = "DOZ";
			invoiceLine3.JI_CustomsSecondQuantity = 12m;
			invoiceLine3.JI_CustomsSecondUnitQty = "KG";

			invoiceLine4.JI_Tariff = "6104.69.8040";
			invoiceLine4.JI_LinePrice = 38.70m;
			invoiceLine4.JI_CustomsQuantity = 1m;
			invoiceLine4.JI_CustomsUnitQty = "DOZ";
			invoiceLine4.JI_CustomsSecondQuantity = 6m;
			invoiceLine4.JI_CustomsSecondUnitQty = "KG";

			invoiceLine5.JI_Tariff = "6106.10.0010";
			invoiceLine5.JI_LinePrice = 204.35m;
			invoiceLine5.JI_CustomsQuantity = 5m;
			invoiceLine5.JI_CustomsUnitQty = "DOZ";
			invoiceLine5.JI_CustomsSecondQuantity = 37m;
			invoiceLine5.JI_CustomsSecondUnitQty = "KG";
			invoiceLine5.US_TextileCategoryNo = "339";

			invoiceLine6.JI_Tariff = "6115.96.9020";
			invoiceLine6.JI_LinePrice = 67.90m;
			invoiceLine6.JI_CustomsQuantity = 1m;
			invoiceLine6.JI_CustomsUnitQty = "DPR";
			invoiceLine6.JI_CustomsSecondQuantity = 3m;
			invoiceLine6.JI_CustomsSecondUnitQty = "KG";

			invoiceLine7.JI_Tariff = "6104.69.8040";
			invoiceLine7.JI_LinePrice = 125.55m;
			invoiceLine7.JI_CustomsQuantity = 2m;
			invoiceLine7.JI_CustomsUnitQty = "DOZ";
			invoiceLine7.JI_CustomsSecondQuantity = 9m;
			invoiceLine7.JI_CustomsSecondUnitQty = "KG";

			invoiceLine8.JI_Tariff = "6109.90.8030";
			invoiceLine8.JI_LinePrice = 690.15m;
			invoiceLine8.JI_CustomsQuantity = 13m;
			invoiceLine8.JI_CustomsUnitQty = "DOZ";
			invoiceLine8.JI_CustomsSecondQuantity = 83.5m;
			invoiceLine8.JI_CustomsSecondUnitQty = "KG";

			invoiceLine9.JI_Tariff = "6106.10.0010";
			invoiceLine9.JI_LinePrice = 694.05m;
			invoiceLine9.JI_CustomsQuantity = 16m;
			invoiceLine9.JI_CustomsUnitQty = "DOZ";
			invoiceLine9.JI_CustomsSecondQuantity = 92m;
			invoiceLine9.JI_CustomsSecondUnitQty = "KG";

			invoiceLine10.JI_Tariff = "6104.69.8040";
			invoiceLine10.JI_LinePrice = 472.60m;
			invoiceLine10.JI_CustomsQuantity = 12m;
			invoiceLine10.JI_CustomsUnitQty = "DOZ";
			invoiceLine10.JI_CustomsSecondQuantity = 75m;
			invoiceLine10.JI_CustomsSecondUnitQty = "KG";

			invoiceLine11.JI_Tariff = "6117.10.6020";
			invoiceLine11.JI_LinePrice = 61.25m;
			invoiceLine11.JI_CustomsQuantity = 2m;
			invoiceLine11.JI_CustomsUnitQty = "DOZ";
			invoiceLine11.JI_CustomsSecondQuantity = 14m;
			invoiceLine11.JI_CustomsSecondUnitQty = "KG";

			declaration.JE_MergeBy = "TRF";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var builder = new ACEEntrySummaryMessageBuilderForTesting(declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(incoming7501);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CreateDocPrintingDetails(message.PK);

			var entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];
			var entryLine2 = declaration.ActiveEntryHeaders[0].MergedLines[1];
			var entryLine3 = declaration.ActiveEntryHeaders[0].MergedLines[2];
			var entryLine4 = declaration.ActiveEntryHeaders[0].MergedLines[3];
			var entryLine5 = declaration.ActiveEntryHeaders[0].MergedLines[4];
			var entryLine6 = declaration.ActiveEntryHeaders[0].MergedLines[5];

			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "16%", entryLine1.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "5.6%", entryLine2.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "5.6%", entryLine3.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "Free", entryLine4.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "Free", entryLine5.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "Free", entryLine6.CL_DutyPercentAsString);

			declaration.JE_MergeBy = "NON"; // message originally merged to 6 lines.... CusEntry now all 11 entered lines.
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];
			entryLine2 = declaration.ActiveEntryHeaders[0].MergedLines[1];
			entryLine3 = declaration.ActiveEntryHeaders[0].MergedLines[2];
			entryLine4 = declaration.ActiveEntryHeaders[0].MergedLines[3];
			entryLine5 = declaration.ActiveEntryHeaders[0].MergedLines[4];
			entryLine6 = declaration.ActiveEntryHeaders[0].MergedLines[5];
			var entryLine7 = declaration.ActiveEntryHeaders[0].MergedLines[6];
			var entryLine8 = declaration.ActiveEntryHeaders[0].MergedLines[7];
			var entryLine9 = declaration.ActiveEntryHeaders[0].MergedLines[8];
			var entryLine10 = declaration.ActiveEntryHeaders[0].MergedLines[9];
			var entryLine11 = declaration.ActiveEntryHeaders[0].MergedLines[10];

			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "16%", entryLine1.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "5.6%", entryLine2.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "16%", entryLine3.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "5.6%", entryLine4.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "Free", entryLine5.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "Free", entryLine6.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "5.6%", entryLine7.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "16%", entryLine8.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "Free", entryLine9.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "5.6%", entryLine10.CL_DutyPercentAsString);
			AssertEquals("Current Entry EntryLine - DutyPercentAsString", "9.5%", entryLine11.CL_DutyPercentAsString);

			var entry = declaration.CustomsEntryHeaders[0];

			var outMsg = message;
			outMsg.EM_MessageText = "B      XJ6AE                                               2                    10AXJ6  00000013     B00001000   01  0X           3042810                       11                                             041510                           20        041510                                                                310B                                                                            40  001 SGKR040710                       0000000000838 N                        506109908030 0000012992 0000000812 000000002100DOZ000000010100KG                6249900000171                                                                   40  002 SGKR040710                       0000000000847 N                        506104698038 0000000437 0000000078 000000000080DOZ000000000500KG                6249900000016                                                                   40  003 SGKR040710                       0000000000847 N                        506104698040 0000003567 0000000637 000000001500DOZ000000009000KG                6249900000134                                                                   40  004 SGKR040710                       0000000000339 N                        506106100010 0000000000 0000000204 000000000500DOZ000000003700KG                6249900000043                                                                   40  005 SGKR040710                       0000000000    N                        506115969020 0000000000 0000000068 000000000100DPR000000000300KG                6249900000014                                                                   40  006 SGKR040710                       0000000000    N                        506106100010 0000000000 0000000694 000000001800DOZ000000009200KG                6249900000146                                                                   8949900000002500                                                                9000000017576 00000002500 00000000000 00000000000 00000000000                   Y      XJ6AE";

			var printBO = new ACEEntryMessage7501Print(entry, outMsg, incoming7501, null);
			Factory.Save();

			AssertEquals("Entry lines to print when printing from message should be 1", 6, printBO.EntryPrintLines.Count);
			AssertEquals("DutyPercentAsString calculated from message on 7501 should be values from entry when Msg was built", "16%", printBO.EntryPrintLines[0].DutyPercentAsString);
		}

		[TestDate(2011, 03, 28)]
		public void TestDutyRatePrintsForTIBEntry()
		{
			CreateTIBDeclaration();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();
			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff line 1", "Free", printBO.EntryPrintLines[0].DutyPercentAsString);
			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - secondary tariff tariff line 1", "5.5%", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);
			AssertEquals("Duty amount", 10223.4m, printBO.EntryPrintLines[0].SecondaryLine1DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff line 2", "Free", printBO.EntryPrintLines[1].DutyPercentAsString);
			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - secondary tariff line 2", "5.5%", printBO.EntryPrintLines[1].SecondaryLine1DutyPercentAsString);
			AssertEquals("Duty amount - secondary tariff line 2", 4792.21m, printBO.EntryPrintLines[1].SecondaryLine1DutyAmount);

			AssertEquals("TIB Duty", 15015.61m, printBO.TIBTotalDuty);
			AssertEquals("TIB Bond Charge", 17050.67m, printBO.TIBBondChg);
		}

		public void TestPrintDutyAmountFromMessageForTIBChapter98()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038001Tariff.UE_Tariff;
			testHelper.ChildLine.US_SupTariff = testHelper.Test9813Tariff.UE_Tariff;
			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.Charpter98Job.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			testHelper.Charpter98Job.US_BondType = "9";
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryHeader = testHelper.Charpter98Job.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(entryHeader);

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entryHeader, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entryHeader.Messages.Add(incoming7501);
			Factory.Save();
			var printBO = new ACEEntryMessage7501Print(entryHeader, message, incoming7501, null);
			AssertEquals("99038001(1000 * 40% = 400)", 400m, printBO.EntryPrintLines[0].DutyAmount);
		}

		public void TestPrintSPIOnSecondLine()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "1010101010";
			tariff1.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff1.UE_DateTo = ZDateTime.Today.AddDays(+10);
			tariff1.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			tariff1.UE_Unit1 = "L";

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "2010101010";
			tariff2.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff2.UE_DateTo = ZDateTime.Today.AddDays(+10);
			tariff2.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			tariff2.UE_Unit1 = "L";

			var tariffRule1 = Factory.New<USCTariffRule>();
			tariffRule1.U1_RuleCode = "A99";
			tariffRule1.U1_Tariff = "101010";
			tariffRule1.U1_DateFrom = ZDateTime.Today;

			var tariffRule2 = Factory.New<USCTariffRule>();
			tariffRule2.U1_RuleCode = "I99";
			tariffRule2.U1_Tariff = "201010";
			tariffRule2.U1_DateFrom = ZDateTime.Today;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "1010101010";
			invoiceLine1.JI_Tariff = "9802008068";
			invoiceLine1.JI_LinePrice = 5000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "2010101010";
			invoiceLine2.JI_Tariff = "9802008068";
			invoiceLine2.JI_LinePrice = 6000m;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "9802008068";
			invoiceLine3.JI_LinePrice = 7000m;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);

			Assert(printBO.EntryPrintLines[0].PrintSPIOnSecondLine);
			Assert(!printBO.EntryPrintLines[1].PrintSPIOnSecondLine);
			Assert(!printBO.EntryPrintLines[2].PrintSPIOnSecondLine);
		}

		public void TestSecondaryLine1DutyAmountForTIBEntry()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-TIB1";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 500m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.US_UC_NKCountryOfExport = "GB";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130020";
			invoiceLine1.JI_Tariff = "7113195080";
			invoiceLine1.JI_CustomsQuantity = 5m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_LinePrice = 1500m;

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();
			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("SecondaryLine1DutyAmount", 82.5m, printBO.EntryPrintLines[0].SecondaryLine1DutyAmount);
		}

		[TestDate(2011, 03, 28)]
		public void TestDutyRateAndAmountPrintsForTIBSetEntry()
		{
			CreateTIBSetDeclaration();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);

			AssertEquals("X line - TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "Free", printBO.EntryPrintLines[0].DutyPercentAsString);
			AssertEquals("X line - TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "6.4%", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);
			AssertEquals("X line - TIB entry should also print the Duty amount as if not for TIB", 320m, printBO.EntryPrintLines[0].SecondaryLine1DutyAmount);

			AssertEquals("V line - TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "Free", printBO.EntryPrintLines[1].DutyPercentAsString);
			AssertEquals("V line - TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "", printBO.EntryPrintLines[1].SecondaryLine1DutyPercentAsString);
			AssertEquals("V line - TIB entry should also print the Duty amount as if not for TIB", 0m, printBO.EntryPrintLines[1].SecondaryLine1DutyAmount);

			AssertEquals("V line - TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "Free", printBO.EntryPrintLines[2].DutyPercentAsString);
			AssertEquals("V line - TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "", printBO.EntryPrintLines[2].SecondaryLine1DutyPercentAsString);
			AssertEquals("V line - TIB entry should also print the Duty amount as if not for TIB", 0m, printBO.EntryPrintLines[2].SecondaryLine1DutyAmount);

			AssertEquals("V line - TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "Free", printBO.EntryPrintLines[3].DutyPercentAsString);
			AssertEquals("V line - TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "", printBO.EntryPrintLines[3].SecondaryLine1DutyPercentAsString);
			AssertEquals("V line - TIB entry should also print the Duty amount as if not for TIB", 0m, printBO.EntryPrintLines[3].SecondaryLine1DutyAmount);

			AssertEquals("TIB Total Duty", 320m, printBO.TIBTotalDuty);
			AssertEquals("TIB Bond Charge", 379.5m, printBO.TIBBondChg);
		}

		[TestDate(2011, 03, 28)]
		public void TestDutyRateAndAmountPrintsForTIBWatchEntry()
		{
			CreateTIBWatchDeclaration();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();
			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "Free", printBO.EntryPrintLines[0].DutyPercentAsString);
			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "51c/NO", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - SecondaryLine2", "6.25%", printBO.EntryPrintLines[0].SecondaryLine2DutyPercentAsString);
			AssertEquals("TIB entry should also print the Duty amount as if not for TIB", 6.25m, printBO.EntryPrintLines[0].SecondaryLine2DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - SecondaryLine3", "6.25%", printBO.EntryPrintLines[0].SecondaryLine3DutyPercentAsString);
			AssertEquals("TIB entry should also print the Duty amount as if not for TIB - SecondaryLine3", 6.25m, printBO.EntryPrintLines[0].SecondaryLine3DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - SecondaryLine4", "5.3%", printBO.EntryPrintLines[0].SecondaryLine4DutyPercentAsString);
			AssertEquals("TIB entry should also print the Duty amount as if not for TIB - SecondaryLine4", 5.3m, printBO.EntryPrintLines[0].SecondaryLine4DutyAmount);
		}

		public void TestExportDate()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.US_EntryFilerCode = "XXX";
			dec.US_EnableENS = true;
			dec.US_DateOfExport = ZDateTime.BrettsBirthday;
			dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			var invoiceLine2 = dec.InvoiceLines.AddNew();
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = dec.ActiveEntryHeaders.EntrySummaryEntry;
			var message = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add).PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			var entryPrintLine = printBO.EntryPrintLines.OfType<ACEEntryMessage7501Line>().FirstOrDefault(x => x.LineNumber == "001");
			AssertEquals("", entryPrintLine.ExportDate);

			invoiceLine2.US_DateOfExport = ZDateTime.BrettsBirthday.AddDays(1);
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			message = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add).PopulateMessage();
			printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			entryPrintLine = printBO.EntryPrintLines.OfType<ACEEntryMessage7501Line>().FirstOrDefault(x => x.LineNumber == "001");
			AssertEquals(ZDateTime.BrettsBirthday.ToString("MMddyy"), entryPrintLine.ExportDate);

			entryPrintLine = printBO.EntryPrintLines.OfType<ACEEntryMessage7501Line>().FirstOrDefault(x => x.LineNumber == "002");
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1).ToString("MMddyy"), entryPrintLine.ExportDate);
		}

		public void TestPortOfLadingForLine()
		{
			Declaration.US_EntryFilerCode = "XJ5";
			var invoiceHeader = Declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8708292500";
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3305100001";
			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3303001000";
			var invoiceLine4 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "9102111010";
			var invoiceLine5 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "8211100000";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
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

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			entry.CreateDocPrintingDetails(message.PK);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			var entryPrintLine = printBO.EntryPrintLines.OfType<ACEEntryMessage7501Line>().FirstOrDefault(x => x.LineNumber == "001");
			AssertEquals("52051", entryPrintLine.PortOfLadingForLine);

			entryPrintLine = printBO.EntryPrintLines.OfType<ACEEntryMessage7501Line>().FirstOrDefault(x => x.LineNumber == "002");
			AssertEquals("52030", entryPrintLine.PortOfLadingForLine);

			entryPrintLine = printBO.EntryPrintLines.OfType<ACEEntryMessage7501Line>().FirstOrDefault(x => x.LineNumber == "003");
			AssertEquals("52030", entryPrintLine.PortOfLadingForLine);

			entryPrintLine = printBO.EntryPrintLines.OfType<ACEEntryMessage7501Line>().FirstOrDefault(x => x.LineNumber == "004");
			AssertEquals("52325", entryPrintLine.PortOfLadingForLine);

			entryPrintLine = printBO.EntryPrintLines.OfType<ACEEntryMessage7501Line>().FirstOrDefault(x => x.LineNumber == "005");
			AssertEquals("52000", entryPrintLine.PortOfLadingForLine);

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
"895010000000018849900000002500                                                  " +
"9000000001830 00000002688 00000000000 00000000000 00000000000                   " +
"Y  1101SV9AE";

			printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			entryPrintLine = printBO.EntryPrintLines.OfType<ACEEntryMessage7501Line>().FirstOrDefault(x => x.LineNumber == "001");
			AssertEquals("Should be empty, because will be printed on header level", ZString.Empty, entryPrintLine.PortOfLadingForLine);
			AssertEquals("52051", printBO.UniquePortOfLading);
		}

		public void TestDescription()
		{
			Declaration.US_EntryFilerCode = "SV9";
			var invoiceHeader = Declaration.Invoices.AddNew();

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8466939585";
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			message.EM_MessageText = "B  1101SV9AE                                               HYEDUSCMT_148883     10ASV9  71003238 1101B00160936   0140 XYY         2112113                       1113-14792700013-147927000                     111113       TX                  20AA  1101111113C785                                                            21001                                                                           2200000001PC                                                                    23M    00188522556                                                              318B 037                                                                        40  001 CHCH111113        0000000120     0000000080    Y                        42CHHARWIN8PLA   PRODDES           0001 0001                                    44SPARES FOR EDM  TEST                                                          47MCHHARWIN8PLA                                                                 47C13-147927000                                                                 47S13-147927000                                                                 508466939585 0000018800 0000004000             X                                6249900001386                                                                   40  002 CHCH111113        0000000030     0000000020    Y                        42CHHARWIN8PLA   PRODDES           0002 0002                                    44SNAP-ACT SWITCHS,OTH THNLI                                                    47MCHHARWIN8PLA                                                                 47C13-147927000                                                                 47S13-147927000                                                                 508536509040 0000002700 0000001000 000000100000NO                               6249900000346                                                                   8949900000002500                                                                9000000021500 00000002500 00000000000 00000000000 00000000000                   Y  1101SV9AE";
			entry.CreateDocPrintingDetails(message.PK);

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			var entryPrintLine = printBO.EntryPrintLines.OfType<ACEEntryMessage7501Line>().FirstOrDefault(x => x.LineNumber == "001");
			AssertEquals("Description should come from tariff", "MCH PTS,N/IRON,METAL,OTHER", entryPrintLine.Description);
		}

		public void TestPrintDutyAmountAndPercentageOnCombinedLine()
		{
			#region Setup tariffs for test

			var tariff9802008068 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9802008068")).LastOrDefault();
			if (tariff9802008068 == null)
			{
				tariff9802008068 = Factory.New<USCTariff>();
				tariff9802008068.UE_Tariff = "9802008068";
			}
			tariff9802008068.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff9802008068.UE_DateTo = new ZDateTime(2099, 1, 1);

			var tariff9802005060 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "9802005060")).LastOrDefault();
			if (tariff9802005060 == null)
			{
				tariff9802005060 = Factory.New<USCTariff>();
				tariff9802005060.UE_Tariff = "9802005060";
			}
			tariff9802005060.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff9802005060.UE_DateTo = new ZDateTime(2099, 1, 1);

			var tariff99038801 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038801")).LastOrDefault();
			if (tariff99038801 == null)
			{
				tariff99038801 = Factory.New<USCTariff>();
				tariff99038801.UE_Tariff = "99038801";
				tariff99038801.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
				tariff99038801.UE_Column1RateAdValorem = 0.25m;
			}
			tariff99038801.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff99038801.UE_DateTo = new ZDateTime(2099, 1, 1);

			var tariff8544300000 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "8544300000")).LastOrDefault();
			if (tariff8544300000 == null)
			{
				tariff8544300000 = Factory.New<USCTariff>();
				tariff8544300000.UE_Tariff = "8544300000";
				tariff8544300000.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
				tariff8544300000.UE_Column1RateAdValorem = 0.05m;
			}
			tariff8544300000.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff8544300000.UE_DateTo = new ZDateTime(2099, 1, 1);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV20081901";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.China;
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine1.US_SupTariff = tariff9802008068.UE_Tariff;
			invoiceLine1.US_98GoodsValue = 5000m;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.JI_Tariff = tariff8544300000.UE_Tariff;
			invoiceLine2.US_SupTariff = tariff99038801.UE_Tariff;
			invoiceLine2.JI_LinePrice = 10000m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine3.US_SupTariff = tariff9802005060.UE_Tariff;
			invoiceLine3.US_98GoodsValue = 5000m;
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_ParentID = invoiceLine3.PK;
			invoiceLine4.JI_Tariff = tariff8544300000.UE_Tariff;
			invoiceLine4.US_SupTariff = tariff99038801.UE_Tariff;
			invoiceLine4.JI_LinePrice = 10000m;

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
			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("Entry print lines count", 2, printBO.EntryPrintLines.Count);
			var printLine = printBO.EntryPrintLines[0];
			AssertEquals("Line 1 Duty to print on 7501", 0m, printLine.DutyAmount);
			AssertEquals("Line 1 Duty Rate to print on 7501", "Free", printLine.DutyPercentAsString);
			AssertEquals("Secondary Line 1 Duty to print on 7501", 2500m, printLine.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Duty Rate to print on 7501", "25%", printLine.SecondaryLine1DutyPercentAsString);
			AssertEquals("Secondary Line 2 Duty to print on 7501", 500m, printLine.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "5%", printLine.SecondaryLine2DutyPercentAsString);

			printLine = printBO.EntryPrintLines[1];
			AssertEquals("Line 2 Duty to print on 7501", 0m, printLine.DutyAmount);
			AssertEquals("Line 2 Duty Rate to print on 7501", "Free", printLine.DutyPercentAsString);
			AssertEquals("Secondary Line 1 Duty to print on 7501", 2500m, printLine.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Duty Rate to print on 7501", "25%", printLine.SecondaryLine1DutyPercentAsString);
			AssertEquals("Secondary Line 2 Duty to print on 7501", 500m, printLine.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "5%", printLine.SecondaryLine2DutyPercentAsString);
		}

		public void TestPrintMultiAENS54FromCombinedLinesMessage()
		{
			var tariff99038801 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038801")).LastOrDefault();
			if (tariff99038801 == null)
			{
				tariff99038801 = Factory.New<USCTariff>();
				tariff99038801.UE_Tariff = "99038801";
			}
			tariff99038801.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff99038801.UE_DateTo = new ZDateTime(2099, 1, 1);

			var tariff99038802 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99038802")).LastOrDefault();
			if (tariff99038802 == null)
			{
				tariff99038802 = Factory.New<USCTariff>();
				tariff99038802.UE_Tariff = "99038802";
			}
			tariff99038802.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff99038802.UE_DateTo = new ZDateTime(2099, 1, 1);

			var tariff7001001000 = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "7001001000")).LastOrDefault();
			if (tariff7001001000 == null)
			{
				tariff7001001000 = Factory.New<USCTariff>();
				tariff7001001000.UE_Tariff = "7001001000";
				tariff7001001000.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;
				tariff7001001000.UE_Column1RateAdValorem = 0.25m;
			}
			tariff7001001000.UE_DateFrom = new ZDateTime(2000, 1, 1);
			tariff7001001000.UE_DateTo = new ZDateTime(2099, 1, 1);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 20000m;
			invoice.JZ_RN_NKDefaultOrigin = Core.Constants.CountryCodes.China;
			invoice.JZ_IncoTerm = "FOB";

			var invoiceLine01 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine01.US_SupTariff = tariff99038801.UE_Tariff;
			invoiceLine01.US_IsParent = true;
			invoiceLine01.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			invoiceLine01.US_ExclusionNumber = "STL000001";

			var invoiceLine02 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine02.US_SupTariff = tariff99038802.UE_Tariff;
			invoiceLine02.JI_Tariff = tariff7001001000.UE_Tariff;
			invoiceLine02.JI_LinePrice = 20000m;
			invoiceLine02.JI_ParentID = invoiceLine01.PK;
			invoiceLine02.JI_ParentLine = invoiceLine01.JI_LineNo;
			invoiceLine02.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._03;
			invoiceLine02.US_ExclusionNumber = "ALU000001";

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
			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);
			var printLine = printBO.EntryPrintLines[0];
			AssertEquals("Product Exclusion No: STL000001\r\nProduct Exclusion No: ALU000001", printLine.ExclusionNumber);
		}

		public void TestPrintSPIOnSecondLineWithSupAdditionalTariffLines()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8471704065", "7", 0m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030104", "0", 0m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030127", "0", 0m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030130", "0", 0m, "");

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Mexico;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
			invoiceLine.JI_FormattedTariff = "8471.70.4065";
			invoiceLine.SupTariffFormatted = "9903.01.04";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.01.27";
			invoiceLine.SupFormattedAdditionalTariff2 = "9903.01.30";
			invoiceLine.US_SPI = SpecialProgramList.Codes.S;
			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);
			Factory.Save();

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);
			var entryPrintLine = printBO.EntryPrintLines[0];
			AssertEquals("SPIAndOrSecondarySPI", "S", entryPrintLine.SPIAndOrSecondarySPI);
			AssertEquals("PrintSPIOnSecondLine", true, entryPrintLine.PrintSPIOnSecondLine);
			AssertEquals("PrintSPIOnSecondLine1", false, entryPrintLine.PrintSPIOnSecondLine1);
			AssertEquals("PrintSPIOnSecondLine2", false, entryPrintLine.PrintSPIOnSecondLine2);
			AssertEquals("PrintSPIOnSecondLine3", true, entryPrintLine.PrintSPIOnSecondLine3);
			AssertEquals("PrintSPIOnSecondLine4", false, entryPrintLine.PrintSPIOnSecondLine4);
			AssertEquals("PrintSPIOnSecondLine5", false, entryPrintLine.PrintSPIOnSecondLine5);
			AssertEquals("PrintSPIOnSecondLine6", false, entryPrintLine.PrintSPIOnSecondLine6);
			AssertEquals("PrintSPIOnSecondLine7", false, entryPrintLine.PrintSPIOnSecondLine7);
		}

		public void TestWatchRepairsWithAdditionalTariff()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9802004040", "X", 0, ZString.Empty);
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030125", "7", 0.1m, ZString.Empty);

			var tariff = USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102296010", "1", 0m, "NO");
			tariff.UE_Column1RateSpecific = 1.75m;

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102296020", "7", 0.048m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9102296030", "7", 0.022m, "NO");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var tariffView99030125 = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, hsnTariffType.PK, "99030125", new ZDateTime(2025, 04, 05), new ZDateTime(2079, 06, 06), "ARTICLES THE PRODUCT OF ANY COUNTRY", 1);
			helper.CreateTariffAttribute(UniversalReferenceConstants.TariffAttributeTypes.Codes.RULE, TariffRuleList.Codes.AdditionalTariffs, tariffView99030125);

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.UnitedKingdom;
			parentLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			parentLine.JI_FormattedTariff = "9102.29.6010";
			parentLine.SupTariffFormatted = "9903.01.25";
			parentLine.SupFormattedAdditionalTariff1 = "9802.00.4040";
			parentLine.JI_LinePrice = 5000m;
			parentLine.JI_CustomsQuantity = 100m;
			parentLine.US_98GoodsValue = 3000m;

			var childLine1 = parentLine.ChildLines.ElementAt(0);
			childLine1.JI_FormattedTariff = "9102.29.6020";
			childLine1.JI_LinePrice = 2000m;
			childLine1.SupTariffFormatted = ZString.Empty;
			var childLine2 = parentLine.ChildLines.ElementAt(1);
			childLine2.JI_FormattedTariff = "9102.29.6030";
			childLine2.JI_LinePrice = 1000m;
			childLine2.SupTariffFormatted = ZString.Empty;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.Add(incoming7501);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CreateDocPrintingDetails(message.PK);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, null);
			AssertEquals("Entry should have Ad Valorem Calculation", true, printBO.EntryHasAdValoremConversionCalculation);

			var entryLine = printBO.EntryPrintLines[0];
			AssertEquals("Entry line should have Ad Valorem Calculation", true, entryLine.AdValoremConversionCalculation);
			AssertEquals("1) A: Total Watches", "100 x $1.75 NO", entryLine.AVWatches);
			AssertEquals("1) A: Total Watches Duty", 175m, entryLine.AVWatchesDuty);
			AssertEquals("1) B: Cases", "$5000 x 4.8%", entryLine.AVCases);
			AssertEquals("1) B: Cases Duty", 240m, entryLine.AVCasesDuty);
			AssertEquals("1) C: Bracelets", "$4000 x 2.2%", entryLine.AVBracelets);
			AssertEquals("1) C: Bracelets Duty", 88m, entryLine.AVBraceletsDuty);
			AssertEquals("1) D: Batteries", ZString.Empty, entryLine.AVBatteries);
			AssertEquals("1) C: Batteries Duty", 0m, entryLine.AVBatteriesDuty);
			AssertEquals("Ad Valorem Calculation summary line 2", "$503.00/$11000.00 (Total Entered Value) = 4.572%", entryLine.AVLine2);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var printLineObject = new EntryMessageLine(Factory);
			return new ACEEntryMessage7501Line(EntryMessageLine, false, false, false, false, false, false, true, true, ZString.Empty, rateStrings, docData, null, Declaration.US_EntryType);
		}

		void AddBlock50(ZString qty)
		{
			EntryMessageLine.aens50.Add(new AENS50() { Quantity2 = qty, UnitOfMeasureCode2 = Core.Constants.Weight.Kilograms });
		}

		void CreateOSAssembledGoodsDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_TransportMode = "SEA";

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_CertifyCargoRelease = true;
			Declaration.US_SchDLoading = "60204";
			Declaration.US_SchDArrival = "1101";

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-0089";
			invoiceHeader.JZ_InvoiceAmount = 2683.80m;
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			invoiceHeader.US_UC_NKCountryOfExport = "AU";
			invoiceHeader.US_TransactionsRelated = "Y";

			var freightCharge = invoiceHeader.Charges.AddNew();
			freightCharge.J7_Amount = 59m;
			freightCharge.J7_RX_NKCurrency = "USD";
			freightCharge.J7_ChargeDescription = "OVERSEAS FREIGHT";
			freightCharge.J7_ChargeType = "OFT";
			freightCharge.J7_DistributeBy = "VAL";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802008068";
				invoiceLine1.JI_InvoiceQuantity = 994m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_Weight = 387.66m;
				invoiceLine1.JI_WeightUQ = "KG";
				invoiceLine1.US_98GoodsValue = 2544.64m;
				invoiceLine1.US_UC_NKCountryOfExport = "AU";
				invoiceLine1.US_UC_NKCountryOfOrigin = "HN";
				invoiceLine1.US_SPI = "N/A";
				invoiceLine1.US_DestinationState = "AK";
				invoiceLine1.US_PIRPRulingType = PIRPRulingTypeList.Codes.BindingRulings;
				invoiceLine1.US_PIRPRulingNo = "45678";

				AssertEquals("PreCondition", 2544.64m, invoiceLine1.JI_CustomsValue);

				invoiceLine1.JI_Tariff = "6203434030";
				invoiceLine1.JI_InvoiceQuantity = 994m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 83m;
				invoiceLine1.JI_CustomsUnitQty = "DOZ";
				invoiceLine1.JI_LinePrice = 2465.12;
				invoiceLine1.JI_Weight = 49.7m;
				invoiceLine1.JI_WeightUQ = "KG";
				invoiceLine1.JI_CustomsSecondQuantity = 437.36m;
				invoiceLine1.JI_CustomsSecondUnitQty = "KG";

				var invoiceLine3 = Declaration.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "9801001010";
				invoiceLine3.JI_InvoiceQuantity = 994m;
				invoiceLine3.JI_InvoiceUQ = "NO";
				invoiceLine3.JI_CustomsQuantity = 0m;
				invoiceLine3.JI_LinePrice = 218.68m;
				invoiceLine3.JI_Weight = 19m;
				invoiceLine3.US_UC_NKCountryOfExport = "AU";
				invoiceLine3.US_UC_NKCountryOfOrigin = "US";
				invoiceLine3.US_DestinationState = "AK";
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateCoffeeFeeDeclaration(ZString tariffCode)
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "TEST NAME", startDate, endDate);
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, USStateList.Codes.PuertoRico);
			newFactory.Save();

			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_TransportMode = "SEA";

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_CertifyCargoRelease = true;

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-Cot";
			invoiceHeader.JZ_InvoiceAmount = 9901.25m;

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariffCode;
			invoiceLine1.JI_CustomsQuantity = 840m;
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.JI_Weight = 3285.59m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_CustomsSecondQuantity = 3285.59m;
			Declaration.US_SchDEntry = "1234";
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateCottonFeeDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_TransportMode = "SEA";

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_CertifyCargoRelease = true;
			Declaration.US_SchDLoading = "60204";
			Declaration.US_SchDArrival = "1101";

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-Cot";
			invoiceHeader.JZ_InvoiceAmount = 9901.25m;
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "JP";
			invoiceHeader.US_UC_NKCountryOfExport = "JP";
			invoiceHeader.US_TransactionsRelated = "N";

			var freightCharge = invoiceHeader.Charges.AddNew();
			freightCharge.J7_Amount = 50m;
			freightCharge.J7_RX_NKCurrency = "USD";
			freightCharge.J7_ChargeDescription = "OVERSEAS FREIGHT";
			freightCharge.J7_ChargeType = "OFT";
			freightCharge.J7_DistributeBy = "VAL";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "6104.22.0010";
				invoiceLine1.JI_CustomsQuantity = 840m;
				invoiceLine1.JI_CustomsUnitQty = "DOZ";
				invoiceLine1.JI_LinePrice = 0m;
				invoiceLine1.JI_Weight = 3285.59m;
				invoiceLine1.JI_WeightUQ = "KG";
				invoiceLine1.JI_CustomsSecondQuantity = 3285.59m;
				invoiceLine1.JI_CustomsSecondUnitQty = "KG";
				invoiceLine1.US_UC_NKCountryOfExport = "DE";
				invoiceLine1.US_UC_NKCountryOfOrigin = "DE";
				invoiceLine1.US_DestinationState = "AK";

				var childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.JI_Tariff = "6102.20.0020";
				childLine2.JI_CustomsQuantity = 840m;
				childLine2.JI_CustomsUnitQty = "DOZ";
				childLine2.JI_LinePrice = 5000.98m;
				childLine2.JI_Weight = 3285.59m;
				childLine2.JI_WeightUQ = "KG";
				childLine2.JI_CustomsSecondQuantity = 3285.59m;
				childLine2.JI_CustomsSecondUnitQty = "KG";
				childLine2.US_UC_NKCountryOfExport = "DE";
				childLine2.US_UC_NKCountryOfOrigin = "DE";
				childLine2.US_DestinationState = "AK";

				var invoiceLine3 = Declaration.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "6104.22.0040";
				invoiceLine3.JI_CustomsQuantity = 840m;
				invoiceLine3.JI_CustomsUnitQty = "DOZ";
				invoiceLine3.JI_LinePrice = 0m;
				invoiceLine3.JI_Weight = 3285.59m;
				invoiceLine3.JI_WeightUQ = "KG";
				invoiceLine3.JI_CustomsSecondQuantity = 3285.59m;
				invoiceLine3.JI_CustomsSecondUnitQty = "KG";
				invoiceLine3.US_UC_NKCountryOfExport = "DE";
				invoiceLine3.US_UC_NKCountryOfOrigin = "DE";
				invoiceLine3.US_DestinationState = "AK";

				var childLine4 = invoiceLine3.AddSecondaryInvoiceLine();
				childLine4.JI_Tariff = "6104.62.2028";
				childLine4.JI_CustomsQuantity = 840m;
				childLine4.JI_CustomsUnitQty = "DOZ";
				childLine4.JI_LinePrice = 4900.27m;
				childLine4.JI_Weight = 2196.51m;
				childLine4.JI_WeightUQ = "KG";
				childLine4.JI_CustomsSecondQuantity = 2196.51m;
				childLine4.JI_CustomsSecondUnitQty = "KG";
				childLine4.US_UC_NKCountryOfExport = "DE";
				childLine4.US_UC_NKCountryOfOrigin = "DE";
				childLine4.US_DestinationState = "AK";
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		EntryMessageLine entryMessageLine;
		EntryMessageLine EntryMessageLine
		{
			get
			{
				if (entryMessageLine == null)
				{
					declaration = null;
					Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
					Declaration.US_ADDCVDSuretyCode = "893";

					var invoiceHeader = Declaration.Invoices.AddNew();
					invoiceHeader.US_TransactionsRelated = "N";
					invoiceHeader.JZ_InvoiceNumber = "INV1232";
					invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.FreeOnBoard;
					invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
					invoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");

					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";

					invoiceLine.JI_Weight = 9000m;
					invoiceLine.JI_LinePrice = 10000m;
					invoiceLine.JI_InvoiceQuantity = 10000m;
					invoiceLine.JI_InvoiceUQ = "NO";
					invoiceLine.JI_CustomsQuantity = 70m;
					invoiceLine.US_UC_NKCountryOfExport = "AU";
					invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
					invoiceLine.US_SecondarySPI = "M";

					var manufacturer = Factory.New<OrgHeader>();
					manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
					manufacturer.OH_FullName = "Mr manufacturer";
					manufacturer.OH_IsConsignor = true;
					manufacturer.OH_RL_NKClosestPort = "AUSYD";
					var manufacturerCode = manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AUSOUPAC195PAD", GlbCompany.CurrentCompany.Country);

					invoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

					invoiceLine.JI_Tariff = "1901909095";
					invoiceLine.US_ADDCaseNo = "A475818001";
					invoiceLine.US_ADDDecID = "112HY";
					invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
					invoiceLine.US_ADDDepositValue = 560m;

					invoiceLine.US_CVDCaseNo = "C475819017";
					invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
					invoiceLine.US_CVDDepositValue = 345m;

					invoiceLine.US_UC_NKCountryOfOrigin = "IT";
					invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;
					invoiceLine.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;

					if (invoiceLine.AntidumpingDutyCase == null)
					{
						SetUpADDCVDCaseIfTestingNotSendingToCustoms("A475818001", ZDateTime.BrettsBirthday, "IT", "1901909095", 0.5m, 0.123m);
					}

					if (invoiceLine.CountervailingDutyCase == null)
					{
						SetUpADDCVDCaseIfTestingNotSendingToCustoms("C475819017", ZDateTime.BrettsBirthday, "IT", "1901909095", 0.5m, 0.456m);
					}

					var childInvoiceLine = invoiceLine.InvoiceHeader.InvoiceLines.AddNew();
					childInvoiceLine.JI_ParentID = invoiceLine.PK;
					childInvoiceLine.JI_Tariff = "8205203000";
					childInvoiceLine.JI_Weight = 10m;
					childInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
					childInvoiceLine.JI_LinePrice = 10000m;
					childInvoiceLine.US_ADDCaseNo = "A475818001";
					childInvoiceLine.US_CVDCaseNo = "C475819017";

					Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
					Factory.Save();

					var builder = new ACEEntrySummaryMessageBuilderForTesting(Declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Add);
					var message = builder.PopulateMessage();
					Declaration.ActiveEntryHeaders.EntrySummaryEntry.CreateDocPrintingDetails(message.PK);
					Factory.Save();

					entryMessageLine = new EntryMessageLine(Factory);
					var aens50List = new List<AENS50>();
					var aens53List = new List<AENS53>();
					foreach (var messageBlock in message.MessageBlock.MessageBlocks)
					{
						if (messageBlock is AENS40)
						{
							entryMessageLine.aens40 = messageBlock as AENS40;
						}
						else if (messageBlock is AENS50)
						{
							aens50List.Add(messageBlock as AENS50);
						}
						else if (messageBlock is AENS53)
						{
							aens53List.Add(messageBlock as AENS53);
						}
					}
					entryMessageLine.aens50 = aens50List;
					entryMessageLine.aens53 = aens53List;

					var aens60 = new AENS60();
					aens60.Deserialise("60                                                        0000224631            ");
					entryMessageLine.aens60 = aens60;
					var aens62 = new AENS62();
					aens62.Deserialise("62          50100010726                                                         ");
					entryMessageLine.aens62.Add(aens62);
					aens62 = new AENS62();
					aens62.Deserialise("62          49900018019                                                         ");
					entryMessageLine.aens62.Add(aens62);

					docData = null;
					if (DocPrintingData.Length > 0)
					{
						docData = DocPrintingData[0];
					}
				}

				return entryMessageLine;
			}
		}

		EntryMessageLine exciseMessageLine;
		EntryMessageLine ExciseMessageLine
		{
			get
			{
				if (exciseMessageLine == null)
				{
					var ens40 = new AENS40();
					ens40.Deserialise("40  001 ITAU              0000000050     0000009000    Y                        ");
					var ens50 = new AENS50();
					ens50.Deserialise("501901909095 0000064000 0000010000             KG                               ");
					var ens60 = new AENS60();
					ens60.Deserialise("600224631                                                                       ");
					var ens62HMF = new AENS62();
					ens62HMF.Deserialise("6250100010726                                                                   ");
					var ens62MPF = new AENS62();
					ens62MPF.Deserialise("6249900018019                                                                   ");

					exciseMessageLine = new EntryMessageLine(Factory);
					exciseMessageLine.aens40 = ens40;

					var ens50List = new List<AENS50>();
					ens50List.Add(ens50);
					exciseMessageLine.aens50 = ens50List;
					exciseMessageLine.aens60 = ens60;

					var ens62ChargesList = new List<IChargeBlock>();
					ens62ChargesList.Add(ens62HMF);
					ens62ChargesList.Add(ens62MPF);
					exciseMessageLine.aens62 = ens62ChargesList;

					var invoice = Declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				}

				return exciseMessageLine;
			}
		}

		void SetUpADDCVDCaseIfTestingNotSendingToCustoms(ZString acCaseNumber, ZDateTime caseDate, ZString countryCode, ZString tariff, ZDecimal adValoremRate, ZDecimal specificRate)
		{
			SetUpADDCVDCaseIfTestingNotSendingToCustoms(acCaseNumber, caseDate, countryCode, tariff, adValoremRate, specificRate, "AC");
		}

		void SetUpADDCVDCaseIfTestingNotSendingToCustoms(ZString acCaseNumber, ZDateTime caseDate, ZString countryCode, ZString tariff, ZDecimal adValoremRate, ZDecimal specificRate, ZString caseStatus)
		{
			var acCase = Factory.New<USCACCase>();
			acCase.U5_CaseNumber = acCaseNumber;
			acCase.U5_CaseStatus = caseStatus;
			acCase.U5_CaseStatusDate = caseDate;
			acCase.U5_ISOCountryCode = countryCode;

			var acCaseTariff = acCase.CaseTariffs.AddNew();
			acCaseTariff.U9_TariffNumber = tariff;

			var acCaseRate = acCase.CaseRates.AddNew();
			acCaseRate.U6_AdValoremRate = adValoremRate;
			acCaseRate.U6_SpecificRate = specificRate;
			acCaseRate.U6_EffectiveDate = caseDate;
			Factory.Save();
		}

		void CreateTIBDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-TIB1";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 273011.25m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.US_UC_NKCountryOfExport = "GB";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130020";
			invoiceLine1.JI_Tariff = "7113195080";
			invoiceLine1.JI_CustomsQuantity = 5m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_LinePrice = 185880m;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "98130020";
			invoiceLine2.JI_Tariff = "7113192980";
			invoiceLine2.JI_CustomsQuantity = 1m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 87131m;

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateTIBSetDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 5000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.US_UC_NKCountryOfExport = "AU";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130020";
			invoiceLine1.JI_Tariff = "1902194000";
			invoiceLine1.JI_CustomsQuantity = 3m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.JI_Weight = 2500;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.US_SetInd = "X";
			invoiceLine1.US_UC_NKCountryOfExport = "CH";
			invoiceLine1.JI_CountryOfOrigin = "CH";
			invoiceLine1.US_DestinationState = "IL";

			var invoiceLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine2.US_SupTariff = "98130020";
			invoiceLine2.JI_Tariff = "0712311000";
			invoiceLine2.JI_CustomsQuantity = 3m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 1300m;
			invoiceLine2.JI_Weight = 650;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.US_SetInd = "V";
			invoiceLine2.US_UC_NKCountryOfExport = "CH";
			invoiceLine2.JI_CountryOfOrigin = "CH";
			invoiceLine2.US_DestinationState = "IL";

			var invoiceLine3 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine3.US_SupTariff = "98130020";
			invoiceLine3.JI_Tariff = "2002908020";
			invoiceLine3.JI_CustomsQuantity = 3m;
			invoiceLine3.JI_CustomsUnitQty = "KG";
			invoiceLine3.JI_LinePrice = 1300m;
			invoiceLine3.JI_Weight = 650;
			invoiceLine3.JI_WeightUQ = "KG";
			invoiceLine3.US_SetInd = "V";
			invoiceLine3.US_UC_NKCountryOfExport = "CH";
			invoiceLine3.JI_CountryOfOrigin = "CH";
			invoiceLine3.US_DestinationState = "IL";

			var invoiceLine4 = invoiceLine1.AddSecondaryInvoiceLine();
			invoiceLine4.US_SupTariff = "98130020";
			invoiceLine4.JI_Tariff = "1902194000";
			invoiceLine4.JI_CustomsQuantity = 3m;
			invoiceLine4.JI_CustomsUnitQty = "KG";
			invoiceLine4.JI_LinePrice = 2400m;
			invoiceLine4.JI_Weight = 1200;
			invoiceLine4.JI_WeightUQ = "KG";
			invoiceLine4.US_SetInd = "V";
			invoiceLine4.US_UC_NKCountryOfExport = "CH";
			invoiceLine4.JI_CountryOfOrigin = "CH";
			invoiceLine4.US_DestinationState = "IL";

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateDeclarationForWI00046587()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_TransportMode = "SEA";
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_CertifyCargoRelease = true;
			Declaration.US_SchDLoading = "58201";
			Declaration.US_SchDArrival = "3901";

			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV081013";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 20000m;
			invoice1.JZ_RN_NKDefaultOrigin = "IR";
			invoice1.US_UC_NKCountryOfExport = "HK";
			invoice1.US_TransactionsRelated = "N";
			invoice1.JZ_IncoTerm = "FOB";

			var freightCharge = invoice1.Charges.AddNew();
			freightCharge.J7_Amount = 1000m;
			freightCharge.J7_RX_NKCurrency = "USD";
			freightCharge.J7_ChargeDescription = "OVERSEAS FREIGHT";
			freightCharge.J7_ChargeType = "OFT";
			freightCharge.J7_DistributeBy = "VAL";

			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "6201110010";
			line1.US_SupTariff = "9802008068";
			line1.JI_InvoiceQuantity = 42m;
			line1.JI_InvoiceUQ = "DOZ";
			line1.JI_CustomsQuantity = 42m;
			line1.JI_CustomsUnitQty = "DOZ";
			line1.JI_CustomsSecondQuantity = 1000m;
			line1.JI_CustomsSecondUnitQty = "KG";
			line1.JI_LinePrice = 15000m;
			line1.US_98GoodsValue = 0m;
			line1.US_98ValueInvCurr = 5000m;
			line1.JI_Weight = 1000m;
			line1.JI_WeightUQ = "KG";
			line1.US_DestinationState = "IL";
			AssertEquals("PreCondition", 20000m, line1.JI_CustomsValue);

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateTIBWatchDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "WATCH";
			invoiceHeader.JZ_InvoiceAmount = 400m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9813000540";
				invoiceLine1.JI_InvoiceQuantity = 1m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_Tariff = "9101114010";
				invoiceLine1.JI_LinePrice = 100m;
				invoiceLine1.US_SPI = "N/A";

				var childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.JI_Tariff = "9101114020";
				childLine2.JI_InvoiceQuantity = 1m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 100m;
				childLine2.US_SupTariff = "";
				childLine2.US_SPI = "N/A";

				var childLine3 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine3.JI_Tariff = "9101114030";
				childLine3.JI_InvoiceQuantity = 1m;
				childLine3.JI_InvoiceUQ = "NO";
				childLine3.JI_CustomsQuantity = 1m;
				childLine3.JI_CustomsUnitQty = "NO";
				childLine3.JI_LinePrice = 100m;
				childLine3.US_SupTariff = "";
				childLine3.US_SPI = "N/A";

				var childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.JI_Tariff = "9102111040";
				childLine4.JI_InvoiceQuantity = 1m;
				childLine4.JI_InvoiceUQ = "NO";
				childLine4.JI_CustomsQuantity = 1m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 100m;
				childLine4.US_SupTariff = "";
				childLine4.US_SPI = "N/A";
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
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
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
				}
				return declaration;
			}
		}

		void SetUpMergedInvoices(string tariffNo)
		{
			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 123620m;
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = tariffNo;
			line1.JI_InvoiceQuantity = 7947;
			line1.JI_InvoiceUQ = "L";
			line1.JI_LinePrice = 123620m;

			if (tariffNo == "2402103030")
			{
				line1.US_UC_NKCountryOfOrigin = "AU";
				line1.US_SPI = "";
				line1.JI_CustomsQuantity = 1000m;
				line1.JI_CustomsSecondQuantity = 1360m;
			}

			if (tariffNo == "8211100000")
			{
				Declaration.US_ADDCVDSuretyCode = "444";

				var childInvoiceLine = line1.InvoiceHeader.InvoiceLines.AddNew();
				childInvoiceLine.JI_ParentID = line1.PK;
				childInvoiceLine.JI_Tariff = "8205203000";
				childInvoiceLine.JI_Weight = 10m;
				childInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				childInvoiceLine.JI_LinePrice = 10000m;
				childInvoiceLine.US_ADDCaseNo = "A570204006";
				childInvoiceLine.US_CVDCaseNo = "C427819000";
			}

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var builder = new ACEEntrySummaryMessageBuilderForTesting(Declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			Declaration.ActiveEntryHeaders.EntrySummaryEntry.CreateDocPrintingDetails(message.PK);
			Factory.Save();

			rateStrings = new ZString[8];

			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}
		}

		void SetUpMergedInvoicesWithSupplementaryTariff()
		{
			declaration = null;
			Declaration.US_ADDCVDSuretyCode = "444";

			var invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 123620m;
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "8205203000";
			line1.US_SupTariff = "9811100000";
			line1.JI_InvoiceQuantity = 7947;
			line1.JI_InvoiceUQ = "L";
			line1.JI_LinePrice = 123620m;
			line1.US_ADDCaseNo = "A570204006";
			line1.US_CVDCaseNo = "C427819000";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var builder = new ACEEntrySummaryMessageBuilderForTesting(Declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			Declaration.ActiveEntryHeaders.EntrySummaryEntry.CreateDocPrintingDetails(message.PK);
			Factory.Save();

			rateStrings = new ZString[8];

			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}
		}

		ZString[] rateStrings;
		US7501DocPrinting docData;

		US7501DocPrinting[] DocPrintingData
		{
			get
			{
				if (docPrintingData == null)
				{
					docPrintingData = new CachedProperty<US7501DocPrinting[]>(Factory,
						delegate
						{
							var eNSCusEntryHeader = Declaration.FormalEntry != null ? Declaration.FormalEntry.PK : ZGuid.Empty;
							var query = new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.US7501DocPrinting);
							query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, CusEntryHeaderSchema.Constants.Prefix);
							query.AddToFilter(CusAddInfoSchema.B7_ParentID, eNSCusEntryHeader);
							var docData = Factory.Load<US7501DocPrinting>(query);
							return docData;
						}
						);
				}

				return docPrintingData.Value;
			}
		}
		CachedProperty<US7501DocPrinting[]> docPrintingData;

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			DeclarationTestHelper.SetEntryFilerCode("XJ6");
		}
	}
}
