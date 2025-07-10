using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryMessageENS7501Print))]
	sealed class EntryMessageENS7501PrintTest : NonPersistentBusinessObjectTestCase
	{
		public void TestEntryMessage7501PrintForHMFDeMinimus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
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
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, true);
			var message = builder.PopulateMessage();

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var printBO = new EntryMessageENS7501Print(entry, message, responseMessage, null);

			AssertEquals("Summary fee desc", "501 HMF (De Minimus)         $0.00", printBO.SummaryFeeDesc1);
		}

		public void TestUSTeamNo()
		{
			CreateRateEntryForMessageMatching();
			CusEntryHeader entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               55711                10A888891-01319900091-013199000                 8         XJ5 7003650001891  IL 20                         408888011011B00154946            430  011011I317     22            00112155216                         00000001PC                    30                                  01              2012111             AA      40001GB00000005000000000010                    0000000010                       50 85361000400000001350000000100000NO                               DE011011N   51                                                                              60                                        GBBOOMED125LON                        62          49900000105                                                         40002IT00000010000000000020                    0000000020                       50 63041120000000006500000000001000NO 000000010000KG                DE011011N   51                  666                                                         60                                        ITADOFORITA                           62          49900000210                                                         40003DE00000035000000000070                    0000000070                       50 39269045100000012250            X                                DE011011N   51                                                                              60                                        DEAKGTHE31HOF                         62          49900000735                                                         8949900000002500                                                                9000000020100           0                       0000000250000000005000          Y  8888XJ5EI00021000000020100";

			var inMsg = Factory.New<MQEDIMessage>();
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);
			inMsg.EM_MessageText = "B018888XJ5ER                                               55711                10A888891-01319900091-013199000                 8         XJ5 7003650001891  IL 40002IT00000010000000000020000000000000000000000000000020                       50 63041120000000006500000000001000NO 000000010000KG                DE011011N   E508888XJ5 7003650000227M01   *CENSUS* QTY2/QTY1 (TARIFF 1)            B001549469000000020100000000000000 00000000000000000000000000000250000000005000          E908888XJ5 70036500   58401808ENT-SUM ACCEPTED WITH WARNINGS           B00154946E08888XJ5 70036500B00154946CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00007000000020100";

			EntryMessageENS7501Print entryMessageENS7501Print = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);
			AssertEquals("USTeamNo", "808", entryMessageENS7501Print.USTeamNo);

			EDIMessage secondOutMsg = message;
			secondOutMsg.EM_MessageText = "B018888XJ5EI                                               55866                10A888891-01319900091-013199000                 8         XJ5 7003650001891  IL 20                         408888011711B00154946            430  011711I317     22            00112155216                         00000001PC                    30                                  0               2020911             AA      40001GB00000005000000000010                    0000000010                       50 85361000400000001350000000100000NO                               DE011711N   51                                                                              60                                        GBBOOMED125LON                        62          49900000105                                                         40002IT00000010000000000020                    0000000020                       50 63041120000000006500000000001000NO 000000010000KG                DE011711N   51                  666                                                         60                                        ITADOFORITA                           62          49900000210                                                         40003DE00000035000000000070                    0000000070                       50 39269045100000012250            X                                DE011711N   51                                                                              60                                        DEAKGTHE31HOF                         62          49900000735                                                         8949900000002500                                                                9000000020100           0                       0000000250000000005000          Y  8888XJ5EI00021000000020100";

			var secondInMsg = Factory.New<MQEDIMessage>();
			secondInMsg.EM_ApplicationCode = "USI";
			secondInMsg.EM_MessageType = "ER";
			secondInMsg.EM_Status = "";
			secondInMsg.EM_ReceiveTransmit = "RCV";
			secondInMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);
			secondInMsg.EM_MessageText = "B018888XJ5ER                                               55866                10A888891-01319900091-013199000                 8         XJ5 7003650001891  IL 40002IT00000010000000000020000000000000000000000000000020                       50 63041120000000006500000000001000NO 000000010000KG                DE011711N   E508888XJ5 7003650000227M01   *CENSUS* QTY2/QTY1 (TARIFF 1)            B001549469000000020100000000000000 00000000000000000000000000000250000000005000          E908888XJ5 70036500   58401808ENT-SUM ACCEPTED WITH WARNINGS           B00154946Y  8888XJ5ER00006000000020100";

			EntryMessageENS7501Print secondEntryMessageENS7501Print = new EntryMessageENS7501Print(entry, secondOutMsg, secondInMsg, null);
			AssertEquals("USTeamNo should still print from Census warning", "808", secondEntryMessageENS7501Print.USTeamNo);
		}

		public void TestEstimatedEntryDate()
		{
			CreateRateEntryForMessageMatching();
			CusEntryHeader entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;

			var inMsg = Factory.New<MQEDIMessage>();
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);

			EntryMessageENS7501Print entryMessageENS7501Print = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);
			AssertEquals(ZDateTime.Empty, entryMessageENS7501Print.EntrySummaryFiledDate);

			Declaration.US_PresentationDate = new ZDateTime(2007, 12, 11);
			AssertEquals(new ZDateTime(2007, 12, 11), entryMessageENS7501Print.EstimatedEntryDate);

			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			Declaration.JE_EntryAuthorisationDate = new ZDateTime(2008, 9, 11);
			AssertEquals(new ZDateTime(2008, 9, 11), entryMessageENS7501Print.EstimatedEntryDate);
		}

		public void TestEntrySummaryFiledDate()
		{
			CreateRateEntryForMessageMatching();
			entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               28                   10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22            OB9394043938SMTAOS005870            00000200CT         MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.New<MQEDIMessage>();
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);

			EntryMessageENS7501Print entryMessageENS7501Print = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals(ZDateTime.Empty, entryMessageENS7501Print.EntrySummaryFiledDate);

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			entry.Declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			ZDateTime statementDate = new ZDateTime(2008, 9, 11);
			entry.Declaration.US_PreliminaryStatementPrintDate = statementDate;
			AssertEquals(statementDate, entryMessageENS7501Print.EntrySummaryFiledDate);
		}

		public void TestWarehouseEntryNo()
		{
			CreateRateEntryForMessageMatching();
			CusEntryHeader entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               26318                10A460198-04177230098-041772300                 8082608   XJ5 7000462332891  NJ 20                           4601082508B00150778                 082508E005     30                  XJ570004599460110               2091708             APLU    40001PH00000742500000000000                    000000294756522                  50 16041430910000928125000001716500KG                               PH082508N   51                                                                              60                                        GBDEBEE45LON                          62          50100009281                                                         62          49900015592                                                         895010000001856249900000031184                                                  9000001856250           0                       0000004974600000148500          Y  8888XJ5EI00017000001856250";

			var inMsg = Factory.New<MQEDIMessage>();
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);

			EntryMessageENS7501Print entryMessageENS7501Print = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);
			AssertEquals("WarehouseEntryNo", "XJ5-7000459-9", entryMessageENS7501Print.WarehouseEntryNo);
		}

		public void TestInvoiceNoSequenceFromMsgAndSnapshot()
		{
			Declaration.Invoices.DeleteAll();

			//-----------------------------------invoice 1-----------------------------------
			JobComInvoiceHeader invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "TEST1";

			JobComInvoiceLine invoice1_invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1_invoiceLine1.JI_Tariff = "8003000000";
			invoice1_invoiceLine1.JI_LinePrice = 1000m;
			invoice1_invoiceLine1.JI_CustomsQuantity = 100m;
			invoice1_invoiceLine1.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoice1_invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1_invoiceLine2.JI_Tariff = "0101100010";
			invoice1_invoiceLine2.JI_LinePrice = 2000m;
			invoice1_invoiceLine2.JI_CustomsQuantity = 200m;
			invoice1_invoiceLine2.JI_CustomsUnitQty = "NO";

			JobComInvoiceLine invoice1_invoiceLine3 = invoice1.JobComInvoiceLines.AddNew();
			invoice1_invoiceLine3.JI_Tariff = "0201100510";
			invoice1_invoiceLine3.JI_LinePrice = 3000m;
			invoice1_invoiceLine3.JI_CustomsQuantity = 300m;
			invoice1_invoiceLine3.JI_CustomsUnitQty = "KG";

			//-----------------------------------invoice 2-----------------------------------
			JobComInvoiceHeader invoice2 = Declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "TEST2";

			JobComInvoiceLine invoice2_invoiceLine1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2_invoiceLine1.JI_Tariff = "0301100000";
			invoice2_invoiceLine1.JI_LinePrice = 4000m;
			invoice2_invoiceLine1.JI_CustomsQuantity = 0m;
			invoice2_invoiceLine1.JI_CustomsUnitQty = "X";

			JobComInvoiceLine invoice2_invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoice2_invoiceLine2.JI_Tariff = "0401100000";
			invoice2_invoiceLine2.JI_LinePrice = 5000m;
			invoice2_invoiceLine2.JI_CustomsQuantity = 500m;
			invoice2_invoiceLine2.JI_CustomsUnitQty = "L";

			//-----------------------------------invoice 3-----------------------------------
			JobComInvoiceHeader invoice3 = Declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "TEST3";

			JobComInvoiceLine invoice3_invoiceLine1 = invoice3.JobComInvoiceLines.AddNew();
			invoice3_invoiceLine1.JI_Tariff = "0501000000";
			invoice3_invoiceLine1.JI_LinePrice = 6000m;
			invoice3_invoiceLine1.JI_CustomsQuantity = 600m;
			invoice3_invoiceLine1.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoice3_invoiceLine2 = invoice3.JobComInvoiceLines.AddNew();
			invoice3_invoiceLine2.JI_Tariff = "0601101500";
			invoice3_invoiceLine2.JI_LinePrice = 7000m;
			invoice3_invoiceLine2.JI_CustomsQuantity = 700m;
			invoice3_invoiceLine2.JI_CustomsUnitQty = "NO";

			JobComInvoiceLine invoice3_invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			invoice3_invoiceLine3.JI_Tariff = "0701100020";
			invoice3_invoiceLine2.JI_LinePrice = 8000m;
			invoice3_invoiceLine2.JI_CustomsQuantity = 800m;
			invoice3_invoiceLine2.JI_CustomsUnitQty = "KG";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			//-----------------------------------outgoing message-----------------------------------

			EDIMessage outMsg = message;
			#region Outgoing Message Text

			outMsg.EM_MessageText =
"B011111SSSEI                                89             231                  " +
"10A888891-013199000                             8      E  SSS 0100000101891     " +
"20                         102809      B00001008                                " +
"22            BRENDON     BRENDN                  00000000                      " +
"22            BRENDON                             00000000                      " +
"30                                  11              1                   OTT1    " +
"40001PE00000010000000000000                    000000000860267                  " +
"42UA123123       1                0001                                          " +
"43INVREQR                                                                       " +
"43      D TIN BARS,RODS,PROFILES & W                                            " +
"50 80030000000000003000000000000100KG                                       N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900000210                                                         " +
"40002  00000020000000000000                    000000001760267                  " +
"42UA123123       1                0002                                          " +
"43INVREQR                                                                       " +
"43      D MALE HORSES, PUREBRED BREE                                            " +
"50 0101100010          000000000200NO                                       N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900000420                                                         " +
"40003  00000030000000000000                    000000002560267                  " +
"42UA123123       1                0003                                          " +
"43INVREQR                                                                       " +
"43      D VEAL FULL/HALF CARCASSES,N                                            " +
"50 02011005100000000013000000000300KG                                       N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62020110051005300000004                                                         " +
"62          49900000630                                                         " +
"40004  00000040000000000000                              60267                  " +
"42UA123123       2                0001                                          " +
"43INVREQR                                                                       " +
"43      D FISH, LIVE, ORNAMENTAL                                                " +
"50 0301100000                      X                                AU      N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900000840                                                         " +
"40005  00000050000000000000                              60267                  " +
"42UA123123       2                0002                                          " +
"43INVREQR                                                                       " +
"43      D MILK/CREAM NOV 1% FAT                                                 " +
"50 04011000000000000002000000000500L              KG                AU      N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900001050                                                         " +
"40006  00000060000000000000                              60267                  " +
"42UA123123       3                0001                                          " +
"43INVREQR                                                                       " +
"43      D HUMAN HAIR, UNWORKED, WAST                                            " +
"50 05010000000000008400000000000600KG                               AU      N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900001260                                                         " +
"40007  00000070000000000000                              60267                  " +
"42UA123123       3                0002                                          " +
"43INVREQR                                                                       " +
"43      D TULIP BULBS                                                           " +
"50 06011015000000000001000000000700NO                               AU      N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900001470                                                         " +
"40008  00000080000000000000                              60267                  " +
"42UA123123       3                0003                                          " +
"43INVREQR                                                                       " +
"43      D POTATOES, FR/CH SEED N/O 4                                            " +
"50 07011000200000000004000000000800KG                               AU      N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900001680                                                         " +
"894990000000756005300000000004                                                  " +
"9000000011420                                   0000000756400000036000          " +
"Y  1111SSSEI00072000000011420";

			#endregion

			//-----------------------------------incoming message-----------------------------------

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;

			//-----------------------------------asserts-----------------------------------

			EntryMessageENS7501Print emp = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals(8, emp.EntryPrintLines.Count);

			EntrySummary7501Line esl = emp.EntryPrintLines[0];
			AssertEquals("Invoice contains sequence 1", "001/TEST1", esl.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", false, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[1];
			AssertEquals("Print Heading", false, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", false, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[2];
			AssertEquals("Print Heading", false, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", true, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[3];
			AssertEquals("Invoice contains sequence 2", "002/TEST2", esl.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", false, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[4];
			AssertEquals("Print Heading", false, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", true, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[5];
			AssertEquals("Invoice contains sequence 3", "003/TEST3", esl.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", false, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[6];
			AssertEquals("Print Heading", false, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", false, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[7];
			AssertEquals("Print Heading", false, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", true, esl.PrintInvoiceDetails);
		}

		public void TestInvoiceNoSequenceWhenInvoiceDelimitersAreSentFromMsgAndSnapshot()
		{
			Declaration.Invoices.DeleteAll();

			//-----------------------------------invoice 1-----------------------------------
			JobComInvoiceHeader invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "TEST1";

			JobComInvoiceLine invoice1_invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1_invoiceLine1.JI_Tariff = "8003000000";
			invoice1_invoiceLine1.JI_LinePrice = 1000m;
			invoice1_invoiceLine1.JI_CustomsQuantity = 100m;
			invoice1_invoiceLine1.JI_CustomsUnitQty = "KG";

			//-----------------------------------invoice 2-----------------------------------
			JobComInvoiceHeader invoice2 = Declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "TEST2";

			JobComInvoiceLine invoice2_invoiceLine1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2_invoiceLine1.JI_Tariff = "0101100010";
			invoice2_invoiceLine1.JI_LinePrice = 2000m;
			invoice2_invoiceLine1.JI_CustomsQuantity = 100m;
			invoice2_invoiceLine1.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoice2_invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoice2_invoiceLine2.JI_Tariff = "0201100510";
			invoice2_invoiceLine2.JI_LinePrice = 3000m;
			invoice2_invoiceLine2.JI_CustomsQuantity = 300m;
			invoice2_invoiceLine2.JI_CustomsUnitQty = "KG";

			//-----------------------------------invoice 3-----------------------------------
			JobComInvoiceHeader invoice3 = Declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "TEST3";

			JobComInvoiceLine invoice3_invoiceLine1 = invoice3.JobComInvoiceLines.AddNew();
			invoice3_invoiceLine1.JI_Tariff = "0301100000";
			invoice3_invoiceLine1.JI_LinePrice = 4000m;
			invoice3_invoiceLine1.JI_CustomsQuantity = 0m;
			invoice3_invoiceLine1.JI_CustomsUnitQty = "X";

			JobComInvoiceLine invoice3_invoiceLine2 = invoice3.JobComInvoiceLines.AddNew();
			invoice3_invoiceLine2.JI_Tariff = "0401100000";
			invoice3_invoiceLine2.JI_LinePrice = 5000m;
			invoice3_invoiceLine2.JI_CustomsQuantity = 500m;
			invoice3_invoiceLine2.JI_CustomsUnitQty = "L";

			JobComInvoiceLine invoice3_invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			invoice3_invoiceLine2.JI_Tariff = "0501000000";
			invoice3_invoiceLine2.JI_LinePrice = 6000m;
			invoice3_invoiceLine2.JI_CustomsQuantity = 600m;
			invoice3_invoiceLine2.JI_CustomsUnitQty = "KG";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			//-----------------------------------outgoing message-----------------------------------

			EDIMessage outMsg = message;
			#region Outgoing Message Text

			outMsg.EM_MessageText =
"B011111SSSEI                                89             231                  " +
"10A888891-013199000                             8      E  SSS 0100000101891     " +
"20                         102809      B00001008                                " +
"22            BRENDON     BRENDN                  00000000                      " +
"22            BRENDON                             00000000                      " +
"30                                  11              1                   OTT1    " +
"40001PE00000010000000000000                    000000000860267          INV001  " + //line1
"42UA123123       1                0001                                          " +
"43INVREQR                                                                       " +
"43      D TIN BARS,RODS,PROFILES & W                                            " +
"50 80030000000000003000000000000100KG                                       N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900000210                                                         " +
"40002  00000020000000000000                    000000001760267                  " + //line2
"42UA123123       1                0002                                          " +
"43INVREQR                                                                       " +
"43      D MALE HORSES, PUREBRED BREE                                            " +
"50 0101100010          000000000200NO                                       N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900000420                                                         " +
"40003  00000030000000000000                    000000002560267          INV002  " + //line3
"42UA123123       1                0003                                          " +
"43INVREQR                                                                       " +
"43      D VEAL FULL/HALF CARCASSES,N                                            " +
"50 02011005100000000013000000000300KG                                       N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62020110051005300000004                                                         " +
"62          49900000630                                                         " +
"40004  00000040000000000000                              60267                  " + //line4
"42UA123123       2                0001                                          " +
"43INVREQR                                                                       " +
"43      D FISH, LIVE, ORNAMENTAL                                                " +
"50 0301100000                      X                                AU      N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900000840                                                         " +
"40005  00000050000000000000                              60267                  " + //line5
"42UA123123       2                0002                                          " +
"43INVREQR                                                                       " +
"43      D MILK/CREAM NOV 1% FAT                                                 " +
"50 04011000000000000002000000000500L              KG                AU      N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900001050                                                         " +
"40006  00000060000000000000                              60267          INV003  " + //line6
"42UA123123       3                0001                                          " +
"43INVREQR                                                                       " +
"43      D HUMAN HAIR, UNWORKED, WAST                                            " +
"50 05010000000000008400000000000600KG                               AU      N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900001260                                                         " +
"894990000000756005300000000004                                                  " +
"9000000011420                                   0000000756400000036000          " +
"Y  1111SSSEI00072000000011420";

			#endregion

			//-----------------------------------incoming message-----------------------------------

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;
			//-----------------------------------asserts-----------------------------------

			EntryMessageENS7501Print emp = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals(6, emp.EntryPrintLines.Count);

			EntrySummary7501Line esl = emp.EntryPrintLines[0];
			AssertEquals("Invoice contains sequence 1", "001/TEST1", esl.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", true, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[1];
			AssertEquals("Invoice contains sequence 2", "002/TEST2", esl.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", false, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[2];
			AssertEquals("Print Heading", false, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", true, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[3];
			AssertEquals("Invoice contains sequence 3", "003/TEST3", esl.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", false, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[4];
			AssertEquals("Print Heading", false, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", false, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[5];
			AssertEquals("Print Heading", false, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", true, esl.PrintInvoiceDetails);
		}

		public void TestInvoiceNoSequenceForSingleInvoiceFromMsgAndSnapshot()
		{
			//-----------------------------------Single Invoice-----------------------------------
			JobComInvoiceHeader invoice = Declaration.Invoices[0];
			Declaration.InvoiceLines.RemoveAndDeleteAll();
			invoice.JZ_InvoiceNumber = "TEST1";

			JobComInvoiceLine invoice_invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoice_invoiceLine1.JI_Tariff = "0301100000";
			invoice_invoiceLine1.JI_LinePrice = 4000m;
			invoice_invoiceLine1.JI_CustomsQuantity = 0m;
			invoice_invoiceLine1.JI_CustomsUnitQty = "X";

			JobComInvoiceLine invoice_invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoice_invoiceLine2.JI_Tariff = "0401100000";
			invoice_invoiceLine2.JI_LinePrice = 6000m;
			invoice_invoiceLine2.JI_CustomsQuantity = 500m;
			invoice_invoiceLine2.JI_CustomsUnitQty = "L";

			JobComInvoiceLine invoice_invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoice_invoiceLine3.JI_Tariff = "0501000000";
			invoice_invoiceLine3.JI_LinePrice = 6000m;
			invoice_invoiceLine3.JI_CustomsQuantity = 600m;
			invoice_invoiceLine3.JI_CustomsUnitQty = "KG";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			//-----------------------------------outgoing message-----------------------------------

			EDIMessage outMsg = message;
			#region Outgoing Message Text

			outMsg.EM_MessageText =
"B011111SSSEI                                89             231                  " +
"10A888891-013199000                             8      E  SSS 0100000101891     " +
"20                         102809      B00001008                                " +
"22            BRENDON     BRENDN                  00000000                      " +
"22            BRENDON                             00000000                      " +
"30                                  11              1                   OTT1    " +
"40001  00000040000000000000                              60267                  " + //line1
"42UA123123       2                0001                                          " +
"43INVREQR                                                                       " +
"43      D FISH, LIVE, ORNAMENTAL                                                " +
"50 0301100000                      X                                AU      N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900000840                                                         " +
"40002  00000050000000000000                              60267                  " + //line2
"42UA123123       2                0002                                          " +
"43INVREQR                                                                       " +
"43      D MILK/CREAM NOV 1% FAT                                                 " +
"50 04011000000000000002000000000500L              KG                AU      N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900001050                                                         " +
"40003  00000060000000000000                              60267                  " + //line3
"42UA123123       3                0001                                          " +
"43INVREQR                                                                       " +
"43      D HUMAN HAIR, UNWORKED, WAST                                            " +
"50 05010000000000008400000000000600KG                               AU      N   " +
"51                                                                              " +
"60                                        UA123123                              " +
"62          49900001260                                                         " +
"894990000000756005300000000004                                                  " +
"9000000011420                                   0000000756400000036000          " +
"Y  1111SSSEI00072000000011420";

			#endregion

			//-----------------------------------incoming message-----------------------------------

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;
			//-----------------------------------asserts-----------------------------------

			EntryMessageENS7501Print emp = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals(3, emp.EntryPrintLines.Count);

			EntrySummary7501Line esl = emp.EntryPrintLines[0];
			AssertEquals("Invoice contains sequence 1", "001/TEST1", esl.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", false, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[1];
			AssertEquals("Print Heading", false, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", false, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[2];
			AssertEquals("Print Heading", false, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", true, esl.PrintInvoiceDetails);
		}

		public void TestInvoiceNoSequenceFromMsgAndSnapshotForSupTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
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
			var builder = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();

			var outMsg = message;
			outMsg.EM_MessageText = "B013901SV9EI                                               HYEDUSCMT_151204     10A390113-14792700013-147927000                 8040814   SV9 7003555301037  IL 20                         403901040814B00161783            002Y 040814A001     22            00157575755                         00000004PC                    30                                  01              2041814             AA      40001SV00000005910000000025                                                     50P9808003000                                                       SV040814Y   51                  340                                                         60                                        SVTEXGUY1SAN                          706205202066           000000009400DOZ000000001200KG                          P 40002SV00000001500000000015                                                     50 9808003000                                                       SV040814N   60                                        SVTEXGUY1SAN                          703924905650                       X                                            OA  FD0                                                                         90                      0                                  00000000741          Y  3901SV9EI00015";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;

			var emp = new EntryMessageENS7501Print(declaration.ActiveEntryHeaders.EntrySummaryEntry, outMsg, inMsg, null);

			AssertEquals(2, emp.EntryPrintLines.Count);

			var esl = emp.EntryPrintLines[0];
			AssertEquals("001/INV-1", esl.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", true, esl.PrintInvoiceDetails);

			esl = emp.EntryPrintLines[1];
			AssertEquals("002/INV-2", esl.InvoiceDetails.InvoiceNo);
			AssertEquals("Print Heading", true, esl.PrintInvoiceHeading);
			AssertEquals("Print Details", true, esl.PrintInvoiceDetails);
		}

		public void TestIParentDocManagerSupportMembers()
		{
			EntryMessageENS7501Print print = (EntryMessageENS7501Print)GetNewBusinessObject();
			IParentDocManagerSupport supporter = print;
			AssertEquals(Core.Constants.DocManagerCodes.CustomsEntry, supporter.DocManagerInfo.DocManagerCode);
			AssertEquals("ParentGuid", Entry.PK, supporter.ParentGuid);
			AssertEquals("ParentTableName", Entry.TableName, supporter.ParentTableName);
		}

		public void TestIDocumentDeliveredLogSupporterMembers()
		{
			EntryMessageENS7501Print print = (EntryMessageENS7501Print)GetNewBusinessObject();

			IDocumentDeliveredLogSupporter supporter = print;
			AssertEquals("BusinessObjectTypeToLogAgainst", typeof(CusEntryHeader), supporter.BusinessObjectTypeToLogAgainst);
			AssertEquals("Identifier", Entry.PK, supporter.Identifier);
		}

		[TestDate(2010, 04, 30)]
		public void TestExciseTaxPercentAsStringWhenTaxIsDeferredFromMsgAndSnapshot()
		{
			CreateDeferredTaxEntry();
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();

			var outMsg = message;
			#region Outgoing Message Text

			outMsg.EM_MessageText =
"B018888XJ5EI                                               50376                " +
"10A888891-01319900091-013199000                 8         XJ5 7002914101891  IL " +
"20                         408888050610B00153473            395  050610I317     " +
"22            12500398484                         00000001PK                    " +
"30                                  0               2051810             BA      " +
"40001GB00000002240000000050                    0000000075                       " +
"50 2208303030          000000001878PFL                              GB050610N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON  0000006698            " +
"62          49900000047                                                         " +
"8949900000002500                                                                " +
"90           000000066981                       0000000250000000000224          " +
"Y  8888XJ5EI00011            000000006698";

			#endregion

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;

			var emp = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			var entryLine = emp.EntryPrintLines[0];
			AssertEquals("Distilled Spirits excise", 66.98m, entryLine.LineFeeAmount);
			AssertEquals("DISPercentAsString", "DEF $3.566322/PFL", entryLine.LineFeePercentAsString);

			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.BulkLiquorDeferred;
			outMsg.EM_MessageText =
"B018888XJ5EI                                               50376                " +
"10A888891-01319900091-013199000                 8         XJ5 7002914101891  IL " +
"20                         408888050610B00153473            395  050610I317     " +
"22            12500398484                         00000001PK                    " +
"30                                  0               2051810             BA      " +
"40001GB00000002240000000050                    0000000075                       " +
"50 2208303030          000000001878PFL                              GB050610N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON  0000006698            " +
"62          49900000047                                                         " +
"8949900000002500                                                                " +
"90           00000006698                        0000000250000000000224          " +
"Y  8888XJ5EI00011            000000006698";
			emp = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			Assert("BulkLiquorTaxDeferred", emp.BulkLiquorTaxDeferred);
		}

		[TestDate(2010, 04, 30)]
		public void TestBlock40TotalWithDistilledSpirits()
		{
			CreateDeferredTaxEntry();
			Declaration.US_TaxDeferIndicator = "";
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			#region Outgoing Message Text

			outMsg.EM_MessageText =
"B018888XJ5EI                                               50376                " +
"10A888891-01319900091-013199000                 8         XJ5 7002914101891  IL " +
"20                         408888050610B00153473            395  050610I317     " +
"22            12500398484                         00000001PK                    " +
"30                                  0               2051810             BA      " +
"40001GB00000002240000000050                    0000000075                       " +
"50 2208303030          000000001878PFL                              GB050610N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON  0000006698            " +
"62          49900000047                                                         " +
"8949900000002500                                                                " +
"90           000000066981                       0000000250000000000224          " +
"Y  8888XJ5EI00011            000000006698";

			#endregion

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;

			EntryMessageENS7501Print emp = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			EntrySummary7501Line entryLine = emp.EntryPrintLines[0];

			AssertEquals("TotalDutyAmt", 0m, emp.TotalDutyAmt);
			AssertEquals("TotalEstTax", 66.98m, emp.TotalEstTax);
			AssertEquals("TotalOther", 25m, emp.TotalOther);
			AssertEquals("Block40Total should not include spirits tax as tax has been deferred for this entry", 25m, emp.Block40Total);
		}

		[TestDate(2010, 04, 30)]
		public void TestBlock40TotalWithDistilledSpiritsForWarehouseEntry()
		{
			CreateDeferredTaxEntry();
			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			#region Outgoing Message Text

			outMsg.EM_MessageText =
"B018888XJ5EI                                               50376                " +
"10A888891-01319900091-013199000                 8         XJ5 7002914121891  IL " +
"20                         408888050610B00153473            395  050610I317     " +
"22            12500398484                         00000001PK                    " +
"30                                  0               2051810             BA      " +
"40001GB00000002240000000050                    0000000075                       " +
"50 2208303030          000000001878PFL                              GB050610N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON  0000006698            " +
"62          49900000047                                                         " +
"8949900000002500                                                                " +
"90           000000066981                       0000000250000000000224          " +
"Y  8888XJ5EI00011            000000006698";

			#endregion

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Declaration.ActiveEntryHeaders.EntrySummaryEntry.PK;

			EntryMessageENS7501Print emp = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			EntrySummary7501Line entryLine = emp.EntryPrintLines[0];

			AssertEquals("TotalDutyAmt", 0m, emp.TotalDutyAmt);
			AssertEquals("TotalEstTax", 66.98m, emp.TotalEstTax);
			AssertEquals("TotalOther", 0m, emp.TotalOther);
			AssertEquals("Block40Total should not include spirits tax for warehouse entry as tax is deferred", 0m, emp.Block40Total);
		}

		public void TestLocationOfGoodsAndNameForBondedWarehouseEntry()
		{
			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "LOCA", "Location", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var warehouse = Factory.New<OrgHeader>();
			warehouse.FillWithValidTestData();
			warehouse.OH_FullName = "JPDuminy Bond Stores";
			warehouse.OH_IsWarehouseClient = true;

			var address = warehouse.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "WH01");
			address.OA_Address1 = "Bond Street BONDVILLE BO";
			Declaration.WarehouseDocAddress.E2_OA_Address = address.PK;

			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			Declaration.US_US_NKLocationOfGoods = "LOCA";

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var entryLine1 = entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();

			var outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               56608                10A888826-27831970026-278319700                 8         XJ5 7003785421856  TX 20                         408888032911B00155542            430  032911I299     22            02011122554                         00000001PC                    30                                  01              2040811             LH      40001GB00000100000000000100                    0000001000                       50 84669395850000047000            X                                DE032911N   51                                                                              60                                        GBBEA1LON                             62          49900002100                                                         8949900000002500                                                                9000000047000           0                       0000000250000000010000          Y  8888XJ5EI00011000000047000";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               56608                E08888XJ5 70037854B00155542ACCEPTED - RECORDS REQUIRED             21808        E08888XJ5 70037854B00155542CERT-RELEASE CERTIFIED VIA SUMMARY      218082A5     EC8888XJ5 70037854B00155542CERT-CONSIGNEE IS FOREIGN BASED         2180848A     Y  8888XJ5ER00003000000047000";

			var printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);
			AssertEquals("For warehouse entry, location of goods should print warehouse name and FIRMS from base.", "WH01/JPDuminy Bond Stores", printBO.LocationOfGoodsAndName);
		}

		public void TestEntryMessageENS7501PrintSCACAndMBillNumberWhenAir()
		{
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			var entryLine1 = Entry.MergedLines.AddNew();
			var entryLine2 = Entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(outMsg);
			outMsg.EM_MessageText = "B01    GAZEI                                               229                  10A2818            91-013199000                 0         GAZ 0000087901        20                         402818052809B00001104            592  052809         22            61803294852                         00000100KG                    30                                  0                                   SQ      40001SG00000008240000000000                                                     50 9802008068                                                       KR052809N   51                                                                              60                                                                              90                      0                                  00000000824          Y      GAZEI00009";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);

			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("SCACAndMBillNumber Should be only Master Bill Number", "61803294852", printBO.SCACAndMBillNumber);
		}

		[TestDate(2009, 6, 1)]
		public void TestEntryMessageENS7501Print()
		{
			CusEntryLine entryLine1 = Entry.MergedLines.AddNew();
			CusEntryLine entryLine2 = Entry.MergedLines.AddNew();
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5ER                                               35183                10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                                        CNJINME22JIN                         062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60                                        CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("IT Number", "123456789012", printBO.FirstBillITNO);
			AssertEquals("IT Date", new ZDateTime(2008, 09, 11), printBO.FirstBillITDate);

			AssertEquals("Formatted entry number", "GAZ-0000019-2", printBO.FormattedEntryNumber);
			AssertEquals("EntryType", "03", printBO.EntryType);
			AssertEquals("EntryTypeCode", "ABI/N", printBO.EntryTypeCode);
			AssertEquals("SuretyCode", "732", printBO.SuretyCode);
			AssertEquals("ImportingCarrier", "HAKUBA MARU (MAEU)", printBO.ImportingCarrier);
			AssertEquals("USTransportMode", "11", printBO.USTransportMode);
			AssertEquals("BondType", "8", printBO.BondType);
			AssertEquals("ImporterOfRecordCustomsRegNo", "48-0920709WM", printBO.ImporterOfRecordCustomsRegNo);
			AssertEquals("LocationOfGoodsAndName", "N598/WANDO TERMINAL", printBO.LocationOfGoodsAndName);
			AssertEquals("SCACAndMBillNumber", "MAEUOB9394043938", printBO.SCACAndMBillNumber);
			AssertEquals("SchDArrival", "1601", printBO.SchDArrival);
			AssertEquals("SchDEntry", "8888", printBO.SchDEntry);
			AssertEquals("Block24ReferenceNumber", "", printBO.Block24ReferenceNumber);
			AssertEquals("UniqueCountryOfOrigin", "CN", printBO.UniqueCountryOfOrigin);
			AssertEquals("UniqueCountryOfExport", "CN", printBO.UniqueCountryOfExport);
			AssertEquals("UniquePortOfLading", "57047", printBO.UniquePortOfLading);
			AssertEquals("ManufacturerID", USConstants.MultipleValueIndicator, printBO.ManufacturerID);
			AssertEquals("EffectiveUltimateConsigneeCustomsRegNo", USConstants.Same, printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("BrokerFileNo", "B00001000", printBO.BrokerFileNo);
			AssertEquals("TotalEnteredValue", 48628m, printBO.TotalEnteredValue);
			AssertEquals("Entry print bills count", 1, printBO.EntryPrintBills.Count);
			AssertEquals("Entry print lines count", 2, printBO.EntryPrintLines.Count);

			AssertEquals("Line 1 - Block29Element1", "CAT 276", printBO.EntryPrintLines[0].Block29Element1);
			AssertEquals("Line 1 - Block29Element2", "MID CNJINME22JIN", printBO.EntryPrintLines[0].Block29Element2);
			AssertEquals("Line 1 - Block29Element3", "RLNG 123456", printBO.EntryPrintLines[0].Block29Element3);
			AssertEquals("Line 1 - fee", "0.21%", printBO.EntryPrintLines[0].MPFPercentAsString);
			AssertEquals("Line 1 - fee", 40.25m, printBO.EntryPrintLines[0].MPFAmount);
			AssertEquals("Line 1 - fee", "0.125%", printBO.EntryPrintLines[0].HMFPercentAsString);
			AssertEquals("Line 1 - fee", 23.96m, printBO.EntryPrintLines[0].HMFAmount);

			AssertEquals("Line 2 - Block29Element1", "MID CNJINMEI3JIN", printBO.EntryPrintLines[1].Block29Element1);
			AssertEquals("Line 3 - Block29Element2", "RLNG 123456", printBO.EntryPrintLines[1].Block29Element2);
			AssertEquals("Line 2 - Block29Element3", "", printBO.EntryPrintLines[1].Block29Element3);
			AssertEquals("Line 2 - fee", 61.87m, printBO.EntryPrintLines[1].MPFAmount);
			AssertEquals("Line 2 - fee", 36.83m, printBO.EntryPrintLines[1].HMFAmount);

			AssertEquals("Summary fee desc", "499 499 Desc from DB", printBO.SummaryFeeDesc1);
			AssertEquals("Summary fee 1", 102.12m, printBO.SummaryFee1);
			AssertEquals("Summary fee desc", "501 501 Desc from DB", printBO.SummaryFeeDesc2);
			AssertEquals("Summary fee 2", 60.79m, printBO.SummaryFee2);

			AssertEquals("Should show as Paperless entry", USConstants.EntrySummaryDisposition.Paperless, printBO.SummaryStatus);
		}

		[TestDate(2009, 6, 1)]
		public void TestEntryMessageENS7501PrintReWarehouse()
		{
			CusEntryLine entryLine1 = Entry.MergedLines.AddNew();
			CusEntryLine entryLine2 = Entry.MergedLines.AddNew();
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5ER                                               35183                10A888848-0920709WM48-0920709WM                 8         GAZ 0000019222732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                  GAZ00000192     0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                                        CNJINME22JIN                         062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60                                        CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("IT Number", "123456789012", printBO.FirstBillITNO);
			AssertEquals("IT Date", new ZDateTime(2008, 09, 11), printBO.FirstBillITDate);

			AssertEquals("Warehouse Entry Number", "GAZ-0000019-2", printBO.WarehouseEntryNo);

			Declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			Declaration.US_WHSEntryFilerCode = "BOB";
			Declaration.US_WHSEntryNumber = "00000128";
			AssertEquals("Warehouse Entry Number", "BOB-0000012-8", printBO.WarehouseEntryNo);

			AssertEquals("EntryType", "22", printBO.EntryType);
			AssertEquals("EntryTypeCode", "ABI/N", printBO.EntryTypeCode);
			AssertEquals("SuretyCode", "732", printBO.SuretyCode);
			AssertEquals("ImportingCarrier", "HAKUBA MARU (MAEU)", printBO.ImportingCarrier);
			AssertEquals("USTransportMode", "11", printBO.USTransportMode);
			AssertEquals("BondType", "8", printBO.BondType);
			AssertEquals("ImporterOfRecordCustomsRegNo", "48-0920709WM", printBO.ImporterOfRecordCustomsRegNo);
			AssertEquals("LocationOfGoodsAndName", "N598/WANDO TERMINAL", printBO.LocationOfGoodsAndName);
			AssertEquals("SCACAndMBillNumber", "MAEUOB9394043938", printBO.SCACAndMBillNumber);
			AssertEquals("SchDArrival", "1601", printBO.SchDArrival);
			AssertEquals("SchDEntry", "8888", printBO.SchDEntry);
			AssertEquals("Block24ReferenceNumber", "", printBO.Block24ReferenceNumber);
			AssertEquals("UniqueCountryOfOrigin", "CN", printBO.UniqueCountryOfOrigin);
			AssertEquals("UniqueCountryOfExport", "CN", printBO.UniqueCountryOfExport);
			AssertEquals("UniquePortOfLading", "57047", printBO.UniquePortOfLading);
			AssertEquals("ManufacturerID", USConstants.MultipleValueIndicator, printBO.ManufacturerID);
			AssertEquals("EffectiveUltimateConsigneeCustomsRegNo", USConstants.Same, printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("BrokerFileNo", "B00001000", printBO.BrokerFileNo);
			AssertEquals("TotalEnteredValue", 48628m, printBO.TotalEnteredValue);
			AssertEquals("Entry print bills count", 1, printBO.EntryPrintBills.Count);
			AssertEquals("Entry print lines count", 2, printBO.EntryPrintLines.Count);

			AssertEquals("Line 1 - Block29Element1", "CAT 276", printBO.EntryPrintLines[0].Block29Element1);
			AssertEquals("Line 1 - Block29Element2", "MID CNJINME22JIN", printBO.EntryPrintLines[0].Block29Element2);
			AssertEquals("Line 1 - Block29Element3", "RLNG 123456", printBO.EntryPrintLines[0].Block29Element3);
			AssertEquals("Line 1 - fee", "0.21%", printBO.EntryPrintLines[0].MPFPercentAsString);
			AssertEquals("Line 1 - fee", 40.25m, printBO.EntryPrintLines[0].MPFAmount);
			AssertEquals("Line 1 - fee", "0.125%", printBO.EntryPrintLines[0].HMFPercentAsString);
			AssertEquals("Line 1 - fee", 23.96m, printBO.EntryPrintLines[0].HMFAmount);

			AssertEquals("Line 2 - Block29Element1", "MID CNJINMEI3JIN", printBO.EntryPrintLines[1].Block29Element1);
			AssertEquals("Line 3 - Block29Element2", "RLNG 123456", printBO.EntryPrintLines[1].Block29Element2);
			AssertEquals("Line 2 - Block29Element3", "", printBO.EntryPrintLines[1].Block29Element3);
			AssertEquals("Line 2 - fee", 61.87m, printBO.EntryPrintLines[1].MPFAmount);
			AssertEquals("Line 2 - fee", 36.83m, printBO.EntryPrintLines[1].HMFAmount);

			AssertEquals("Summary fee desc", "499 499 Desc from DB", printBO.SummaryFeeDesc1);
			AssertEquals("Summary fee 1", 102.12m, printBO.SummaryFee1);
			AssertEquals("Summary fee desc", "501 501 Desc from DB", printBO.SummaryFeeDesc2);
			AssertEquals("Summary fee 2", 60.79m, printBO.SummaryFee2);

			AssertEquals("Should show as Paperless entry", USConstants.EntrySummaryDisposition.Paperless, printBO.SummaryStatus);
		}

		[TestDate(2009, 6, 1)]
		public void TestEntryTypeCode()
		{
			OrgHeader ior = Factory.NewWithValidTestData<OrgHeader>();
			ior.CustomsCodes.AddNew("EIN", "48-0920709WM");
			var iorWrapper = OrgHeaderWrapper.New(ior);
			iorWrapper.ZO_PayMethod = ACHPaymentTypeList.Codes.ImporterCheck;

			CusEntryLine entryLine1 = Entry.MergedLines.AddNew();
			CusEntryLine entryLine2 = Entry.MergedLines.AddNew();
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5ER                                               35183                10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                                        CNJINME22JIN                         062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60                                        CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("EntryTypeCode", "ABI/N", printBO.EntryTypeCode);

			outMsg.EM_MessageText = "B018888XJ5ER                                               35183                10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0               3                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                                        CNJINME22JIN                         062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60                                        CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";
			printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("EntryTypeCode", "ABI/S", printBO.EntryTypeCode);

			iorWrapper.ZO_PayMethod = ACHPaymentTypeList.Codes.ACHCredit;
			outMsg.EM_MessageText = "B018888XJ5ER                                               35183                10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0               2                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                                        CNJINME22JIN                         062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60                                        CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";
			Factory.Save();
			printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("EntryTypeCode", "ABI/A", printBO.EntryTypeCode);

			outMsg.EM_MessageText = "B018888XJ5ER                                               35183                10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0               6                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                                        CNJINME22JIN                         062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60                                        CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";
			Factory.Save();
			printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("EntryTypeCode", "ABI/P", printBO.EntryTypeCode);

			outMsg.EM_MessageText = "B018888XJ5ER                                               35183                10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0               1                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                                        CNJINME22JIN                         062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60                                        CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";
			Factory.Save();
			printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("EntryTypeCode", "ABI/N", printBO.EntryTypeCode);
		}

		public void TestFirstBillITDateAndITNumber()
		{
			CusEntryLine entryLine1 = Entry.MergedLines.AddNew();
			CusEntryLine entryLine2 = Entry.MergedLines.AddNew();
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			Declaration.US_ITDate = new ZDateTime(2008, 09, 11);

			Bill masterBill = Declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MasterBill";
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";

			Bill houseBill1 = Declaration.Bills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_CU_ParentBill = masterBill.PK;
			houseBill1.CU_BillNum = "HouseBill1";
			houseBill1.US_UI_NKBillIssuerSCAC = "APLU";

			Bill subHouseBill = Declaration.Bills.AddNew();
			subHouseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.CU_CU_ParentBill = houseBill1.PK;
			subHouseBill.CU_BillNum = "SubHouseBill";
			subHouseBill.US_UI_NKBillIssuerSCAC = "APLU";
			subHouseBill.ITNumber = "IT12345";

			Bill houseBill2 = Declaration.Bills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_CU_ParentBill = masterBill.PK;
			houseBill2.ITNumber = "IT1234";
			houseBill2.CU_BillNum = "HouseBill2";
			houseBill2.US_UI_NKBillIssuerSCAC = "APLU";

			Bill masterBill2 = Declaration.Bills.AddNew();
			masterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "MasterBill 2";
			masterBill2.US_UI_NKBillIssuerSCAC = "APLU";

			Bill houseBill1_MB2 = Declaration.Bills.AddNew();
			houseBill1_MB2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1_MB2.CU_CU_ParentBill = masterBill2.PK;
			houseBill1_MB2.CU_BillNum = "HouseBill1";
			houseBill1_MB2.US_UI_NKBillIssuerSCAC = "APLU";
			houseBill1_MB2.ITNumber = "V1245678910";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage outgoingMessage = builder.PopulateMessage();
			Factory.Save();

			var incomingMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			incomingMsg.EM_LinkedObject = Entry;
			incomingMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outgoingMessage, incomingMsg, null);

			AssertEquals("IT Number", "IT12345", printBO.FirstBillITNO);
			AssertEquals("IT Date", new ZDateTime(2008, 09, 11), printBO.FirstBillITDate);
		}

		public void TestEntrySummary7501BillsAfterBillOfLadingUpdate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MasterBill";
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";

			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_CU_ParentBill = masterBill.PK;
			houseBill1.CU_BillNum = "HouseBill1";
			houseBill1.US_UI_NKBillIssuerSCAC = "HLMU";

			var subHouseBill = declaration.Bills.AddNew();
			subHouseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.CU_CU_ParentBill = houseBill1.PK;
			subHouseBill.CU_BillNum = "SubHouseBill";
			subHouseBill.CU_NoOfPacks = 550m;
			subHouseBill.CU_PackType = ShippingOrPackingingUnitList.Codes.Parcel;

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_CU_ParentBill = masterBill.PK;
			houseBill2.CU_BillNum = "HouseBill2";
			houseBill2.US_UI_NKBillIssuerSCAC = "APLU";

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "MasterBill 2";
			masterBill2.US_UI_NKBillIssuerSCAC = "APLU";
			masterBill.CU_NoOfPacks = 520m;
			masterBill.CU_PackType = ShippingOrPackingingUnitList.Codes.Carton;

			var houseBill1_MB2 = declaration.Bills.AddNew();
			houseBill1_MB2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1_MB2.CU_CU_ParentBill = masterBill2.PK;
			houseBill1_MB2.CU_BillNum = "HouseBill1";
			houseBill1_MB2.US_UI_NKBillIssuerSCAC = "APLU";
			houseBill1_MB2.CU_NoOfPacks = 520m;
			houseBill1_MB2.CU_PackType = ShippingOrPackingingUnitList.Codes.Carton;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var outgoingMessage = builder.PopulateMessage();
			Factory.Save();

			var incomingMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			incomingMsg.EM_LinkedObject = entry;
			incomingMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			var printBO = new EntryMessageENS7501Print(entry, outgoingMessage, incomingMsg, null);
			AssertEquals("EntryPrintBills", 3, printBO.EntryPrintBills.Count);

			var billDetail1 = printBO.EntryPrintBills[0];
			var billDetail2 = printBO.EntryPrintBills[1];
			var billDetail3 = printBO.EntryPrintBills[2];

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail1.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "MASTERBILL", billDetail1.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "HLMU", billDetail1.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "HOUSEBILL1", billDetail1.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", billDetail1.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "SUBHOUSEBILL", billDetail1.SubHouseBill);
			AssertEquals("PkgQty", 550, billDetail1.PkgQty);
			AssertEquals("PkgType", ShippingOrPackingingUnitList.Codes.Parcel, billDetail1.PkgType);

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail2.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "MASTERBILL", billDetail2.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "APLU", billDetail2.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "HOUSEBILL2", billDetail2.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", billDetail2.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "", billDetail2.SubHouseBill);
			AssertEquals("PkgQty", 0, billDetail2.PkgQty);
			AssertEquals("PkgType", "", billDetail2.PkgType);

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail3.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "MASTERBILL2", billDetail3.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "APLU", billDetail3.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "HOUSEBILL1", billDetail3.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", billDetail3.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "", billDetail3.SubHouseBill);
			AssertEquals("PkgQty", 520, billDetail3.PkgQty);
			AssertEquals("PkgType", ShippingOrPackingingUnitList.Codes.Carton, billDetail3.PkgType);

			subHouseBill.CU_NoOfPacks = 580m;
			subHouseBill.CU_PackType = ShippingOrPackingingUnitList.Codes.Case;

			houseBill1_MB2.CU_NoOfPacks = 600m;
			houseBill1_MB2.CU_PackType = ShippingOrPackingingUnitList.Codes.Chest;
			Factory.Save();

			var bluBuilder = new MessageBuilders.Testing.TestBillOfLadingUpdateBuilder(declaration);
			var bluMessage = bluBuilder.PopulateMessage();

			var incomingBLUResponseMsg = Factory.New<MQEDIMessage>();
			incomingBLUResponseMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingBLUResponseMsg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingBLUResponseMsg.EM_MessageType = ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse;
			incomingBLUResponseMsg.EM_MessageSubType = EM_MessageSubTypeList.Codes.BillOfLadingUpdate;
			incomingBLUResponseMsg.EM_MessageNum = "3";
			incomingBLUResponseMsg.EM_LinkUniqueID = declaration.PK;
			incomingBLUResponseMsg.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			incomingBLUResponseMsg.EM_MessageText =
"B018888XJ5LS                                               3                    " +
"L78888XJ5 700214458VZBILL DATA UPDATED AS REQUESTED                             " +
"L78888XJ5 700214458VXBILL RESULTS REQUEST ACCEPTED                              " +
"Y  8888XJ5LS00002";

			Factory.Save();
			entry.Declaration.Messages.Load();
			printBO = new EntryMessageENS7501Print(entry, outgoingMessage, incomingMsg, bluMessage);
			AssertEquals("EntryPrintBills", 3, printBO.EntryPrintBills.Count);

			billDetail1 = printBO.EntryPrintBills[0];
			billDetail2 = printBO.EntryPrintBills[1];
			billDetail3 = printBO.EntryPrintBills[2];

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail1.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "MASTERBILL", billDetail1.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "HLMU", billDetail1.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "HOUSEBILL1", billDetail1.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", billDetail1.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "SUBHOUSEBILL", billDetail1.SubHouseBill);
			AssertEquals("PkgQty should print BLU message updated", 580, billDetail1.PkgQty);
			AssertEquals("PkgType should print BLU message updated", ShippingOrPackingingUnitList.Codes.Case, billDetail1.PkgType);

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail2.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "MASTERBILL", billDetail2.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "APLU", billDetail2.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "HOUSEBILL2", billDetail2.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", billDetail2.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "", billDetail2.SubHouseBill);
			AssertEquals("PkgQty", 0, billDetail2.PkgQty);
			AssertEquals("PkgType", "", billDetail2.PkgType);

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail3.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "MASTERBILL2", billDetail3.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "APLU", billDetail3.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "HOUSEBILL1", billDetail3.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", billDetail3.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "", billDetail3.SubHouseBill);
			AssertEquals("PkgQty should print BLU message updated", 600, billDetail3.PkgQty);
			AssertEquals("PkgType should print BLU message updated", ShippingOrPackingingUnitList.Codes.Chest, billDetail3.PkgType);
		}

		public void TestBLUMsgUpdatesPakagesOnDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "OB03089487";
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";
			masterBill.CU_NoOfPacks = 550m;
			masterBill.CU_PackType = ShippingOrPackingingUnitList.Codes.Parcel;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var outgoingMessage = builder.PopulateMessage();
			Factory.Save();

			var incomingMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			incomingMsg.EM_LinkedObject = entry;
			incomingMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			var printBO = new EntryMessageENS7501Print(entry, outgoingMessage, incomingMsg, null);
			AssertEquals("EntryPrintBills", 1, printBO.EntryPrintBills.Count);

			var billDetail1 = printBO.EntryPrintBills[0];

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail1.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "OB03089487", billDetail1.MasterBill);
			AssertEquals("PkgQty", 550, billDetail1.PkgQty);
			AssertEquals("PkgType", ShippingOrPackingingUnitList.Codes.Parcel, billDetail1.PkgType);

			masterBill.CU_NoOfPacks = 600m;
			masterBill.CU_PackType = ShippingOrPackingingUnitList.Codes.Chest;
			Factory.Save();

			var bluBuilder = new MessageBuilders.Testing.TestBillOfLadingUpdateBuilder(declaration);
			var bluMessage = bluBuilder.PopulateMessage();

			var incomingBLUResponseMsg = Factory.New<MQEDIMessage>();
			incomingBLUResponseMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingBLUResponseMsg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingBLUResponseMsg.EM_MessageType = ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse;
			incomingBLUResponseMsg.EM_MessageSubType = EM_MessageSubTypeList.Codes.BillOfLadingUpdate;
			incomingBLUResponseMsg.EM_MessageNum = "2";
			incomingBLUResponseMsg.EM_LinkUniqueID = declaration.PK;
			incomingBLUResponseMsg.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			incomingBLUResponseMsg.EM_MessageText =
"B018888XJ5LS                                               2                    " +
"L78888XJ5 700214458VZBILL DATA UPDATED AS REQUESTED                             " +
"L78888XJ5 700214458VXBILL RESULTS REQUEST ACCEPTED                              " +
"Y  8888XJ5LS00002";

			Factory.Save();
			entry.Declaration.Messages.Load();
			printBO = new EntryMessageENS7501Print(entry, outgoingMessage, incomingMsg, bluMessage);
			AssertEquals("EntryPrintBills", 1, printBO.EntryPrintBills.Count);

			billDetail1 = printBO.EntryPrintBills[0];

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail1.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "OB03089487", billDetail1.MasterBill);
			AssertEquals("PkgQty should print BLU message updated value - 600", 600, billDetail1.PkgQty);
			AssertEquals("PkgType should print BLU message updated value - Chest", ShippingOrPackingingUnitList.Codes.Chest, billDetail1.PkgType);
		}

		public void TestBLUMsgUpdatesChangeToBillNoAndSCACOnDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "OB03089487";
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";
			masterBill.CU_NoOfPacks = 550m;
			masterBill.CU_PackType = ShippingOrPackingingUnitList.Codes.Parcel;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var outgoingMessage = builder.PopulateMessage();
			Factory.Save();

			var incomingMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			incomingMsg.EM_LinkedObject = entry;
			incomingMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			var printBO = new EntryMessageENS7501Print(entry, outgoingMessage, incomingMsg, null);
			AssertEquals("EntryPrintBills", 1, printBO.EntryPrintBills.Count);

			var billDetail1 = printBO.EntryPrintBills[0];

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail1.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "OB03089487", billDetail1.MasterBill);
			AssertEquals("PkgQty", 550, billDetail1.PkgQty);
			AssertEquals("PkgType", ShippingOrPackingingUnitList.Codes.Parcel, billDetail1.PkgType);

			masterBill.CU_BillNum = "OB030894817";
			masterBill.US_UI_NKBillIssuerSCAC = "ABLU";
			Factory.Save();

			var bluBuilder = new MessageBuilders.Testing.TestBillOfLadingUpdateBuilder(declaration);
			var bluMessage = bluBuilder.PopulateMessage();

			var incomingBLUResponseMsg = Factory.New<MQEDIMessage>();
			incomingBLUResponseMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingBLUResponseMsg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingBLUResponseMsg.EM_MessageType = ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse;
			incomingBLUResponseMsg.EM_MessageSubType = EM_MessageSubTypeList.Codes.BillOfLadingUpdate;
			incomingBLUResponseMsg.EM_MessageNum = "2";
			incomingBLUResponseMsg.EM_LinkUniqueID = declaration.PK;
			incomingBLUResponseMsg.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			incomingBLUResponseMsg.EM_MessageText =
"B018888XJ5LS                                               2                    " +
"L78888XJ5 700214458VZBILL DATA UPDATED AS REQUESTED                             " +
"L78888XJ5 700214458VXBILL RESULTS REQUEST ACCEPTED                              " +
"Y  8888XJ5LS00002";

			Factory.Save();
			entry.Declaration.Messages.Load();
			printBO = new EntryMessageENS7501Print(entry, outgoingMessage, incomingMsg, bluMessage);
			AssertEquals("EntryPrintBills", 1, printBO.EntryPrintBills.Count);

			billDetail1 = printBO.EntryPrintBills[0];

			AssertEquals("EffectiveMasterBillIssuerSCAC should print BLU message updated value - ABLU", "ABLU", billDetail1.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill should print BLU message updated value - OB030894817", "OB030894817", billDetail1.MasterBill);
			AssertEquals("PkgQty", 550, billDetail1.PkgQty);
			AssertEquals("PkgType", ShippingOrPackingingUnitList.Codes.Parcel, billDetail1.PkgType);
		}

		public void TestITDateAndITNumberAfterBLUMsgChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			declaration.US_ITDate = new ZDateTime(2008, 09, 11);

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MasterBill";
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";

			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_CU_ParentBill = masterBill.PK;
			houseBill1.CU_BillNum = "HouseBill1";
			houseBill1.US_UI_NKBillIssuerSCAC = "APLU";

			var subHouseBill = declaration.Bills.AddNew();
			subHouseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.CU_CU_ParentBill = houseBill1.PK;
			subHouseBill.CU_BillNum = "SubHouseBill";
			subHouseBill.US_UI_NKBillIssuerSCAC = "APLU";
			subHouseBill.ITNumber = "IT12345";

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_CU_ParentBill = masterBill.PK;
			houseBill2.ITNumber = "IT1234";
			houseBill2.CU_BillNum = "HouseBill2";
			houseBill2.US_UI_NKBillIssuerSCAC = "APLU";

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "MasterBill 2";
			masterBill2.US_UI_NKBillIssuerSCAC = "APLU";

			var houseBill1_MB2 = declaration.Bills.AddNew();
			houseBill1_MB2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1_MB2.CU_CU_ParentBill = masterBill2.PK;
			houseBill1_MB2.CU_BillNum = "HouseBill1";
			houseBill1_MB2.US_UI_NKBillIssuerSCAC = "APLU";
			houseBill1_MB2.ITNumber = "V1245678910";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var outgoingMessage = builder.PopulateMessage();
			Factory.Save();

			var incomingMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			incomingMsg.EM_LinkedObject = entry;
			incomingMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			var printBO = new EntryMessageENS7501Print(entry, outgoingMessage, incomingMsg, null);

			AssertEquals("IT Number", "IT12345", printBO.FirstBillITNO);
			AssertEquals("IT Date", new ZDateTime(2008, 09, 11), printBO.FirstBillITDate);

			AssertEquals("EntryPrintBills", 3, printBO.EntryPrintBills.Count);
			var billDetail1 = printBO.EntryPrintBills[0];
			var billDetail2 = printBO.EntryPrintBills[1];
			var billDetail3 = printBO.EntryPrintBills[2];

			AssertEquals("IT Number", "IT12345", billDetail1.ITNO);
			AssertEquals("IT Date", new ZDateTime(2008, 09, 11), billDetail1.ITDate);

			AssertEquals("IT Number", "IT1234", billDetail2.ITNO);
			AssertEquals("IT Date", new ZDateTime(2008, 09, 11), billDetail2.ITDate);

			AssertEquals("IT Number", "V1245678910", billDetail3.ITNO);
			AssertEquals("IT Date", new ZDateTime(2008, 09, 11), billDetail3.ITDate);

			houseBill1_MB2.ITNumber = "V777755555";
			Factory.Save();

			var bluBuilder = new MessageBuilders.Testing.TestBillOfLadingUpdateBuilder(declaration);
			var bluMessage = bluBuilder.PopulateMessage();

			var incomingBLUResponseMsg = Factory.New<MQEDIMessage>();
			incomingBLUResponseMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingBLUResponseMsg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingBLUResponseMsg.EM_MessageType = ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse;
			incomingBLUResponseMsg.EM_MessageSubType = EM_MessageSubTypeList.Codes.BillOfLadingUpdate;
			incomingBLUResponseMsg.EM_MessageNum = "3";
			incomingBLUResponseMsg.EM_LinkUniqueID = declaration.PK;
			incomingBLUResponseMsg.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			incomingBLUResponseMsg.EM_MessageText =
"B018888XJ5LS                                               3                    " +
"L78888XJ5 700214458VZBILL DATA UPDATED AS REQUESTED                             " +
"L78888XJ5 700214458VXBILL RESULTS REQUEST ACCEPTED                              " +
"Y  8888XJ5LS00002";

			Factory.Save();
			declaration.Messages.Load();
			printBO = new EntryMessageENS7501Print(entry, outgoingMessage, incomingMsg, bluMessage);
			AssertEquals("EntryPrintBills", 3, printBO.EntryPrintBills.Count);

			billDetail1 = printBO.EntryPrintBills[0];
			billDetail2 = printBO.EntryPrintBills[1];
			billDetail3 = printBO.EntryPrintBills[2];

			AssertEquals("Updated 'V' type IT Number should print from BLU details", "V777755555", billDetail1.ITNO);
			AssertEquals("IT Date should remain as per previous ens22", new ZDateTime(2008, 09, 11), billDetail1.ITDate);

			AssertEquals("IT Number after BLU - should get IT Number from previous ens22 record", "IT12345", billDetail2.ITNO);
			AssertEquals("IT Date should remain as per previous ens22", new ZDateTime(2008, 09, 11), billDetail2.ITDate);

			AssertEquals("IT Number after BLU - should get IT Number from previous ens22 record", "IT1234", billDetail3.ITNO);
			AssertEquals("IT Date should remain as per previous ens22", new ZDateTime(2008, 09, 11), billDetail3.ITDate);
		}

		[TestDate(2009, 6, 1)]
		public void TestEntryMessageENS7501PrintWhenNoDocData()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader entry = Declaration.FormalEntry;
			EDIMessage outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outMsg.EM_LinkUniqueID = entry.PK;
			outMsg.EM_MessageText = "B018888XJ5ER                                               35183                10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                                        CNJINME22JIN                         062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60                                        CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);

			AssertEquals("IT Number", "123456789012", printBO.FirstBillITNO);
			AssertEquals("IT Date", new ZDateTime(2008, 09, 11), printBO.FirstBillITDate);

			AssertEquals("Formatted entry number", "GAZ-0000019-2", printBO.FormattedEntryNumber);
			AssertEquals("EntryType", "03", printBO.EntryType);
			AssertEquals("EntryTypeCode", "ABI/N", printBO.EntryTypeCode);
			AssertEquals("SuretyCode", "732", printBO.SuretyCode);
			AssertEquals("ImportingCarrier", "HAKUBA MARU (MAEU)", printBO.ImportingCarrier);
			AssertEquals("USTransportMode", "11", printBO.USTransportMode);
			AssertEquals("BondType", "8", printBO.BondType);
			AssertEquals("ImporterOfRecordCustomsRegNo", "48-0920709WM", printBO.ImporterOfRecordCustomsRegNo);
			AssertEquals("LocationOfGoodsAndName", "N598/WANDO TERMINAL", printBO.LocationOfGoodsAndName);
			AssertEquals("SCACAndMBillNumber", "MAEUOB9394043938", printBO.SCACAndMBillNumber);
			AssertEquals("SchDArrival", "1601", printBO.SchDArrival);
			AssertEquals("SchDEntry", "8888", printBO.SchDEntry);
			AssertEquals("Block24ReferenceNumber", "", printBO.Block24ReferenceNumber);
			AssertEquals("UniqueCountryOfOrigin", "CN", printBO.UniqueCountryOfOrigin);
			AssertEquals("UniqueCountryOfExport", "CN", printBO.UniqueCountryOfExport);
			AssertEquals("UniquePortOfLading", "57047", printBO.UniquePortOfLading);
			AssertEquals("ManufacturerID", USConstants.MultipleValueIndicator, printBO.ManufacturerID);
			AssertEquals("EffectiveUltimateConsigneeCustomsRegNo", USConstants.Same, printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("BrokerFileNo", "B00001000", printBO.BrokerFileNo);
			AssertEquals("TotalEnteredValue", 48628m, printBO.TotalEnteredValue);
			AssertEquals("Entry print bills count", 1, printBO.EntryPrintBills.Count);
			AssertEquals("Entry print lines count", 2, printBO.EntryPrintLines.Count);

			AssertEquals("Line 1 - Block29Element1", "CAT 276", printBO.EntryPrintLines[0].Block29Element1);
			AssertEquals("Line 1 - Block29Element2", "MID CNJINME22JIN", printBO.EntryPrintLines[0].Block29Element2);
			AssertEquals("Line 1 - Block29Element3", "RLNG 123456", printBO.EntryPrintLines[0].Block29Element3);
			AssertEquals("Line 1 - fee", "0.21%", printBO.EntryPrintLines[0].MPFPercentAsString);
			AssertEquals("Line 1 - fee", 40.25m, printBO.EntryPrintLines[0].MPFAmount);
			AssertEquals("Line 1 - fee", "0.125%", printBO.EntryPrintLines[0].HMFPercentAsString);
			AssertEquals("Line 1 - fee", 23.96m, printBO.EntryPrintLines[0].HMFAmount);

			AssertEquals("Line 2 - Block29Element1", "MID CNJINMEI3JIN", printBO.EntryPrintLines[1].Block29Element1);
			AssertEquals("Line 3 - Block29Element2", "RLNG 123456", printBO.EntryPrintLines[1].Block29Element2);
			AssertEquals("Line 2 - Block29Element3", "", printBO.EntryPrintLines[1].Block29Element3);
			AssertEquals("Line 2 - fee", 61.87m, printBO.EntryPrintLines[1].MPFAmount);
			AssertEquals("Line 2 - fee", 36.83m, printBO.EntryPrintLines[1].HMFAmount);

			AssertEquals("Summary fee desc", "499 499 Desc from DB", printBO.SummaryFeeDesc1);
			AssertEquals("Summary fee 1", 102.12m, printBO.SummaryFee1);
			AssertEquals("Summary fee desc", "501 501 Desc from DB", printBO.SummaryFeeDesc2);
			AssertEquals("Summary fee 2", 60.79m, printBO.SummaryFee2);

			AssertEquals("Should show as Paperless entry", USConstants.EntrySummaryDisposition.Paperless, printBO.SummaryStatus);
		}

		public void TestSummaryBlock39()
		{
			declaration = null;
			entry = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			var entryLine1 = Entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(outMsg);
			outMsg.EM_MessageText = "B018888123EI                                               50215                " +
"10A530191-01319900091-013199000                 8         123 7002207603891  CA " +
"20     MSC ORNELLA         105301042110B00153415            22N  042110V136     " +
"22            MSCUG1026256                        00000300BX         MSCU       " +
"30                                  0               2050310             MSCU    " +
"350000000000000000000000000000000000000000027301                                " +
"40001GB00000003280000000041                    000000002947507                  " +
"50 15099020000000000205000000004100KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000041                                                         " +
"62          49900000069                                                         " +
"40002GB00000003640000000180                    000000003247507                  " +
"50 1902192030          000000018000KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000046                                                         " +
"62          49900000076                                                         " +
"40003GB00000013240000000247                    000000011547507                  " +
"50 15091020000000001235000000024700KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000166                                                         " +
"62          49900000278                                                         " +
"40004GB00000003490000000164                    000000003047507                  " +
"50 21032040200000004048000000016400KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000044                                                         " +
"62          49900000073                                                         " +
"40005GB00000001960000000024                    000000001747507                  " +
"50 21039090910000001254000000002400KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000025                                                         " +
"62          49900000041                                                         " +
"40006GB00000224740000012351                    000000195347507                  " +
"50 1902192030          000001235100KG                               IT041510N   " +
"51                                                                              " +
"600000019777C475819005                    GBBOOMED295LON            00088     0 " +
"62          50100002809                                                         " +
"62          49900004720                                                         " +
"40007GB00000085500000004077                    000000074347507                  " +
"50 1902192030          000000407700KG                               IT041510N   " +
"51                                                                              " +
"600000007524C475819005                    GBBOOMED295LON            00088     0 " +
"62          50100001069                                                         " +
"62          49900001796                                                         " +
"40008GB00000009420000000240                    000000008247507                  " +
"50 1902112030          000000024000KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000118                                                         " +
"62          49900000198                                                         " +
"895010000000431849900000007251                                                  " +
"9000000006742           0 00000027301           0000001156900000034527          " +
"Y  8888123EI00055000000006742";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);
			inMsg.EM_MessageText = @"B018888123EI                                               50215                EBA AND B REC DP/FLR/OFFICE CONFLICT                                            EBTRANSACTION DATA REJECTED                                                     Y  8888123EI00055000000006742";

			Entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;
			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Summary Fee Desc 1", "013 CVD", printBO.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total CVD)", 273.01m, printBO.SummaryFee1);
			AssertEquals("Summary Fee 2", "499 499 Desc from DB", printBO.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total MPF)", 72.51m, printBO.SummaryFee2);
			AssertEquals("Summary Fee 3", "501 501 Desc from DB", printBO.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total HMF)", 43.18m, printBO.SummaryFee3);
			AssertEquals("Total Other Fees should include CVD", 388.70m, printBO.TotalOtherFees);
			AssertEquals("Total Other", 388.70m, printBO.TotalOther);
		}

		public void TestSummaryBlock39WithAD()
		{
			CreateRateEntryForMessageMatching();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5ER                                               35183                10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";
			outMsg.EM_Status = "";
			outMsg.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("Summary Fee Desc 1", "012 AD", printBO.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total AD)", 1781.33m, printBO.SummaryFee1);
			AssertEquals("Summary fee desc", "499 499 Desc from DB", printBO.SummaryFeeDesc2);
			AssertEquals("Summary fee 2", 102.12m, printBO.SummaryFee2);
			AssertEquals("Summary fee desc", "501 501 Desc from DB", printBO.SummaryFeeDesc3);
			AssertEquals("Summary fee 3", 60.79m, printBO.SummaryFee3);

			AssertEquals("Total Other Fees should include CVD", 1944.24m, printBO.TotalOtherFees);
			AssertEquals("Total Other", 1944.24m, printBO.TotalOther);
		}

		public void Test7501BillsAfterBillOfLadingUpdateDeletesABill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MAWB1";
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";

			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_CU_ParentBill = masterBill.PK;
			houseBill1.CU_BillNum = "HAWB1";
			houseBill1.US_UI_NKBillIssuerSCAC = "HLMU";

			var subHouseBill = declaration.Bills.AddNew();
			subHouseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.CU_CU_ParentBill = houseBill1.PK;
			subHouseBill.CU_BillNum = "SubHouseBill";
			subHouseBill.CU_NoOfPacks = 550m;
			subHouseBill.CU_PackType = ShippingOrPackingingUnitList.Codes.Parcel;

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_CU_ParentBill = masterBill.PK;
			houseBill2.CU_BillNum = "HAWB2";
			houseBill2.US_UI_NKBillIssuerSCAC = "CJMU";

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "Mawb2";
			masterBill2.US_UI_NKBillIssuerSCAC = "CFMU";
			masterBill.CU_NoOfPacks = 520m;
			masterBill.CU_PackType = ShippingOrPackingingUnitList.Codes.Carton;

			var houseBill1_MB2 = declaration.Bills.AddNew();
			houseBill1_MB2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1_MB2.CU_CU_ParentBill = masterBill2.PK;
			houseBill1_MB2.CU_BillNum = "HAWB1";
			houseBill1_MB2.US_UI_NKBillIssuerSCAC = "CFMU";
			houseBill1_MB2.CU_NoOfPacks = 520m;
			houseBill1_MB2.CU_PackType = ShippingOrPackingingUnitList.Codes.Carton;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var outgoingMessage = builder.PopulateMessage();
			Factory.Save();

			var incomingMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			incomingMsg.EM_LinkUniqueID = entry.PK;
			incomingMsg.EM_LinkTable = entry.TableName;
			incomingMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			var printBO = new EntryMessageENS7501Print(entry, outgoingMessage, incomingMsg, null);
			AssertEquals("EntryPrintBills", 3, printBO.EntryPrintBills.Count);

			var billDetail1 = printBO.EntryPrintBills[0];
			var billDetail2 = printBO.EntryPrintBills[1];
			var billDetail3 = printBO.EntryPrintBills[2];

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail1.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "MAWB1", billDetail1.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "HLMU", billDetail1.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "HAWB1", billDetail1.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", billDetail1.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "SUBHOUSEBILL", billDetail1.SubHouseBill);
			AssertEquals("PkgQty", 550, billDetail1.PkgQty);
			AssertEquals("PkgType", ShippingOrPackingingUnitList.Codes.Parcel, billDetail1.PkgType);

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail2.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "MAWB1", billDetail2.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "CJMU", billDetail2.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "HAWB2", billDetail2.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", billDetail2.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "", billDetail2.SubHouseBill);
			AssertEquals("PkgQty", 0, billDetail2.PkgQty);
			AssertEquals("PkgType", "", billDetail2.PkgType);

			AssertEquals("EffectiveMasterBillIssuerSCAC", "CFMU", billDetail3.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "MAWB2", billDetail3.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "CFMU", billDetail3.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "HAWB1", billDetail3.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", billDetail3.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "", billDetail3.SubHouseBill);
			AssertEquals("PkgQty", 520, billDetail3.PkgQty);
			AssertEquals("PkgType", ShippingOrPackingingUnitList.Codes.Carton, billDetail3.PkgType);

			houseBill1_MB2.Delete();
			masterBill2.Delete();

			subHouseBill.CU_NoOfPacks = 580m;
			subHouseBill.CU_PackType = ShippingOrPackingingUnitList.Codes.Case;

			houseBill2.CU_NoOfPacks = 600m;
			houseBill2.CU_PackType = ShippingOrPackingingUnitList.Codes.Chest;
			Factory.Save();
			AssertEquals("Pre-condition: Declaration messages", 1, entry.Declaration.Messages.Count);

			var bluBuilder = new MessageBuilders.Testing.TestBillOfLadingUpdateBuilder(declaration);
			var bluMessage = bluBuilder.PopulateMessage();
			Factory.Save();
			AssertEquals("Pre-condition: Declaration messages", 2, entry.Declaration.Messages.Count);

			var incomingBLUResponseMsg = Factory.New<MQEDIMessage>();
			incomingBLUResponseMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingBLUResponseMsg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingBLUResponseMsg.EM_MessageType = ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse;
			incomingBLUResponseMsg.EM_MessageSubType = EM_MessageSubTypeList.Codes.BillOfLadingUpdate;
			incomingBLUResponseMsg.EM_MessageNum = "3";
			incomingBLUResponseMsg.EM_LinkUniqueID = declaration.PK;
			incomingBLUResponseMsg.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			incomingBLUResponseMsg.EM_MessageText =
"B018888XJ5LS                                               3                    " +
"L78888XJ5 700214458VZBILL DATA UPDATED AS REQUESTED                             " +
"L78888XJ5 700214458VXBILL RESULTS REQUEST ACCEPTED                              " +
"Y  8888XJ5LS00002";
			Factory.Save();
			entry.Declaration.Messages.Load();
			AssertEquals("Pre-condition: Declaration messages", 3, entry.Declaration.Messages.Count);

			printBO = new EntryMessageENS7501Print(entry, outgoingMessage, incomingMsg, bluMessage);
			AssertEquals("EntryPrintBills collection should no longer include deleted bill", 2, printBO.EntryPrintBills.Count);

			billDetail1 = printBO.EntryPrintBills[0];
			billDetail2 = printBO.EntryPrintBills[1];

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail1.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "MAWB1", billDetail1.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "HLMU", billDetail1.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "HAWB1", billDetail1.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", billDetail1.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "SUBHOUSEBILL", billDetail1.SubHouseBill);
			AssertEquals("PkgQty should print BLU message updated", 580, billDetail1.PkgQty);
			AssertEquals("PkgType should print BLU message updated", ShippingOrPackingingUnitList.Codes.Case, billDetail1.PkgType);

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail2.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "MAWB1", billDetail2.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "CJMU", billDetail2.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "HAWB2", billDetail2.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", billDetail2.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "", billDetail2.SubHouseBill);
			AssertEquals("PkgQty should print BLU message updated", 600, billDetail2.PkgQty);
			AssertEquals("PkgType should print BLU message updated", ShippingOrPackingingUnitList.Codes.Chest, billDetail2.PkgType);
		}

		public void TestExcessFees()
		{
			declaration = null;
			entry = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			CusEntryLine entryLine1 = Entry.MergedLines.AddNew();
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			EDIMessage outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(outMsg);
			outMsg.EM_MessageText = "B018888123EI                                               50215                " +
"10A530191-01319900091-013199000                 8         123 7002207603891  CA " +
"20     MSC ORNELLA         105301042110B00153415            22N  042110V136     " +
"22            MSCUG1026256                        00000300BX         MSCU       " +
"30                                  0               2050310             MSCU    " +
"350000000000000000000000000000000000000000027301                                " +
"40001GB00000003280000000041                    000000002947507                  " +
"50 15099020000000000205000000004100KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000041                                                         " +
"62          49900000069                                                         " +
"40002GB00000003640000000180                    000000003247507                  " +
"50 1902192030          000000018000KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000046                                                         " +
"62          49900000076                                                         " +
"40003GB00000013240000000247                    000000011547507                  " +
"50 15091020000000001235000000024700KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000166                                                         " +
"62          49900000278                                                         " +
"40004GB00000003490000000164                    000000003047507                  " +
"50 21032040200000004048000000016400KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000044                                                         " +
"62          49900000073                                                         " +
"40005GB00000001960000000024                    000000001747507                  " +
"50 21039090910000001254000000002400KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000025                                                         " +
"62          49900000041                                                         " +
"40006GB00000224740000012351                    000000195347507                  " +
"50 1902192030          000001235100KG                               IT041510N   " +
"51                                                                              " +
"600000019777C475819005                    GBBOOMED295LON            00088     0 " +
"62          50100002809                                                         " +
"62          49900004720                                                         " +
"40007GB00000085500000004077                    000000074347507                  " +
"50 1902192030          000000407700KG                               IT041510N   " +
"51                                                                              " +
"600000007524C475819005                    GBBOOMED295LON            00088     0 " +
"62          50100001069                                                         " +
"62          49900001796                                                         " +
"40008GB00000009420000000240                    000000008247507                  " +
"50 1902112030          000000024000KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000118                                                         " +
"62          49900000198                                                         " +
"62          10600030000                                                         " +
"62          05600040000                                                         " +
"62          10200050000                                                         " +
"895010000000431849900000007251106000000300000560000004000010200000050000        " +
"9000000006742           0 00000027301           0000001156900000034527          " +
"Y  8888123EI00055000000006742";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);
			inMsg.EM_MessageText = @"B018888123EI                                               50215                EBA AND B REC DP/FLR/OFFICE CONFLICT                                            EBTRANSACTION DATA REJECTED                                                     Y  8888123EI00055000000006742";

			Entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Summary Fee Desc 1", "013 CVD", printBO.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total CVD)", 273.01m, printBO.SummaryFee1);
			AssertEquals("Summary Fee 2", "056 056 Desc from DB", printBO.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total Cotton Fee)", 400m, printBO.SummaryFee2);
			AssertEquals("Summary Fee 3", "102 102 Desc from DB", printBO.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total Fresh Limes Fee)", 500m, printBO.SummaryFee3);
			AssertEquals("Summary Fee 4", "106 106 Desc from DB", printBO.SummaryFeeDesc4);
			AssertEquals("SummaryFee4 (Total Blueberry Fee)", 300m, printBO.SummaryFee4);
			AssertEquals("Total Other Fees should include CVD", 388.70m, printBO.TotalOtherFees);
			AssertEquals("Total Other", 388.70m, printBO.TotalOther);

			AssertEquals("Excess Fee 1", "499 499 Desc from DB", printBO.EntryPrintExcessFees[0].SummaryFeeDesc);
			AssertEquals("ExcessFee1", 72.51m, printBO.EntryPrintExcessFees[0].SummaryFee);
			AssertEquals("Excess Fee 2", "501 501 Desc from DB", printBO.EntryPrintExcessFees[1].SummaryFeeDesc);
			AssertEquals("ExcessFee2", 43.18m, printBO.EntryPrintExcessFees[1].SummaryFee);
		}

		public void TestExcessFeesMultipleens89Lines()
		{
			declaration = null;
			entry = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			var entryLine1 = Entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(outMsg);
			outMsg.EM_MessageText = "B018888123EI                                               50215                " +
"10A530191-01319900091-013199000                 8         123 7002207603891  CA " +
"20     MSC ORNELLA         105301042110B00153415            22N  042110V136     " +
"22            MSCUG1026256                        00000300BX         MSCU       " +
"30                                  0               2050310             MSCU    " +
"350000000000000000000000000000000000000000027301                                " +
"40001GB00000003280000000041                    000000002947507                  " +
"50 15099020000000000205000000004100KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000041                                                         " +
"62          49900000069                                                         " +
"40002GB00000003640000000180                    000000003247507                  " +
"50 1902192030          000000018000KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000046                                                         " +
"62          49900000076                                                         " +
"40003GB00000013240000000247                    000000011547507                  " +
"50 15091020000000001235000000024700KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000166                                                         " +
"62          49900000278                                                         " +
"40004GB00000003490000000164                    000000003047507                  " +
"50 21032040200000004048000000016400KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000044                                                         " +
"62          49900000073                                                         " +
"40005GB00000001960000000024                    000000001747507                  " +
"50 21039090910000001254000000002400KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000025                                                         " +
"62          49900000041                                                         " +
"40006GB00000224740000012351                    000000195347507                  " +
"50 1902192030          000001235100KG                               IT041510N   " +
"51                                                                              " +
"600000019777C475819005                    GBBOOMED295LON            00088     0 " +
"62          50100002809                                                         " +
"62          49900004720                                                         " +
"62          10700060000                                                         " +
"40007GB00000085500000004077                    000000074347507                  " +
"50 1902192030          000000407700KG                               IT041510N   " +
"51                                                                              " +
"600000007524C475819005                    GBBOOMED295LON            00088     0 " +
"62          50100001069                                                         " +
"62          49900001796                                                         " +
"62          05300070000                                                         " +
"40008GB00000009420000000240                    000000008247507                  " +
"50 1902112030          000000024000KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000118                                                         " +
"62          49900000198                                                         " +
"62          10600030000                                                         " +
"62          05600040000                                                         " +
"62          10200050000                                                         " +
"895010000000431849900000007251106000000300000560000004000010200000050000        " +
"891070000006000005300000070000                                                  " +
"9000000006742           0 00000027301           0000001156900000034527          " +
"Y  8888123EI00055000000006742";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);
			inMsg.EM_MessageText = @"B018888123EI                                               50215                EBA AND B REC DP/FLR/OFFICE CONFLICT                                            EBTRANSACTION DATA REJECTED                                                     Y  8888123EI00055000000006742";

			Entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;
			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Summary Fee Desc 1", "013 CVD", printBO.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total CVD)", 273.01m, printBO.SummaryFee1);
			AssertEquals("Summary Fee 2", "053 053 Desc from DB", printBO.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total Beef Fee)", 700m, printBO.SummaryFee2);
			AssertEquals("Summary Fee 3", "056 056 Desc from DB", printBO.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total Cotton Fee)", 400m, printBO.SummaryFee3);
			AssertEquals("Summary Fee 4", "102 102 Desc from DB", printBO.SummaryFeeDesc4);
			AssertEquals("SummaryFee4 (Total Fresh Limes Fee)", 500m, printBO.SummaryFee4);
			AssertEquals("Total Other Fees should include CVD", 388.70m, printBO.TotalOtherFees);
			AssertEquals("Total Other", 388.70m, printBO.TotalOther);

			AssertEquals("Excess Fee 1", "106 106 Desc from DB", printBO.EntryPrintExcessFees[0].SummaryFeeDesc);
			AssertEquals("ExcessFee1", 300m, printBO.EntryPrintExcessFees[0].SummaryFee);
			AssertEquals("Excess Fee 2", "107 107 Desc from DB", printBO.EntryPrintExcessFees[1].SummaryFeeDesc);
			AssertEquals("ExcessFee2", 600m, printBO.EntryPrintExcessFees[1].SummaryFee);
			AssertEquals("Excess Fee 3", "499 499 Desc from DB", printBO.EntryPrintExcessFees[2].SummaryFeeDesc);
			AssertEquals("ExcessFee3 (Total MPF)", 72.51m, printBO.EntryPrintExcessFees[2].SummaryFee);
			AssertEquals("Excess Fee Desc 4", "501 501 Desc from DB", printBO.EntryPrintExcessFees[3].SummaryFeeDesc);
			AssertEquals("ExcessFee4 (Total HMF)", 43.18m, printBO.EntryPrintExcessFees[3].SummaryFee);
		}

		public void TestExcessFeesMegaMultipleens89Lines()
		{
			declaration = null;
			entry = null;

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			var entryLine1 = Entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(outMsg);
			outMsg.EM_MessageText = "B018888XJ5EI                                               54284                " +
"10A888891-01319900091-013199000                 9         XJ5 7003431501891  IL " +
"20     FALSTAFF            108888101910B00154351            18N  101910K111     " +
"2100001350008999955                                                             " +
"22            OB8937432                           00000002PAL        APLU       " +
"30                                  0               2102910             APLU    " +
"40001GB00000450000000000500                    000000010050200                  " +
"50 62034240510000747000000000020000DOZ            KG                GB101410N   " +
"51                  347                                                         " +
"60                                        GBBOOMED295LON  0000002000            " +
"62          50100005625                                                         " +
"62          49900009450                                                         " +
"62          10700000111                                                         " +
"62          05300000222                                                         " +
"62          10600000333                                                         " +
"62          10200000444                                                         " +
"62          05500000555                                                         " +
"62          10800000666                                                         " +
"62          10300000777                                                         " +
"62          05700000888                                                         " +
"62          05400000999                                                         " +
"62          09000010076                                                         " +
"62          10500012514                                                         " +
"62          10900020000                                                         " +
"62          07900031950                                                         " +
"62          10400045000                                                         " +
"895010000000562549900000009450107000000001110530000000022210600000000333        " +
"891020000000044405500000000555108000000006661030000000077705700000000888        " +
"890540000000099909000000010076105000000125141090000002000007900000031950        " +
"8910400000045000                                                                " +
"9000000747000000000020000                       0000013961000000045000          " +
"Y  8888XJ5EI00030000000747000000000002000";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);
			inMsg.EM_MessageText = @"B018888123EI                                               50215                EBA AND B REC DP/FLR/OFFICE CONFLICT                                            EBTRANSACTION DATA REJECTED                                                     Y  8888123EI00055000000006742";

			Entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;
			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Summary Fee Desc 1", "053 053 Desc from DB", printBO.SummaryFeeDesc1);
			AssertEquals("SummaryFee1", 2.22m, printBO.SummaryFee1);
			AssertEquals("Summary Fee 2", "054 054 Desc from DB", printBO.SummaryFeeDesc2);
			AssertEquals("SummaryFee2", 9.99m, printBO.SummaryFee2);
			AssertEquals("Summary Fee 3", "055 055 Desc from DB", printBO.SummaryFeeDesc3);
			AssertEquals("SummaryFee3", 5.55m, printBO.SummaryFee3);
			AssertEquals("Summary Fee 4", "057 057 Desc from DB", printBO.SummaryFeeDesc4);
			AssertEquals("SummaryFee4", 8.88m, printBO.SummaryFee4);
			AssertEquals("Total Other Fees", 1396.1m, printBO.TotalOtherFees);
			AssertEquals("Total Other", 1396.1m, printBO.TotalOther);

			AssertEquals("Excess Fee count", 12, printBO.EntryPrintExcessFees.Count);

			AssertEquals("Excess Fee 1", "079 079 Desc from DB", printBO.EntryPrintExcessFees[0].SummaryFeeDesc);
			AssertEquals("ExcessFee1", 319.5m, printBO.EntryPrintExcessFees[0].SummaryFee);
			AssertEquals("Excess Fee 2", "090 090 Desc from DB", printBO.EntryPrintExcessFees[1].SummaryFeeDesc);
			AssertEquals("ExcessFee2", 100.76m, printBO.EntryPrintExcessFees[1].SummaryFee);
			AssertEquals("Excess Fee 3", "102 102 Desc from DB", printBO.EntryPrintExcessFees[2].SummaryFeeDesc);
			AssertEquals("ExcessFee3", 4.44m, printBO.EntryPrintExcessFees[2].SummaryFee);
			AssertEquals("103 103 Desc from DB", printBO.EntryPrintExcessFees[3].SummaryFeeDesc);
			AssertEquals(7.77m, printBO.EntryPrintExcessFees[3].SummaryFee);
			AssertEquals("104 104 Desc from DB", printBO.EntryPrintExcessFees[4].SummaryFeeDesc);
			AssertEquals(450m, printBO.EntryPrintExcessFees[4].SummaryFee);
			AssertEquals("105 105 Desc from DB", printBO.EntryPrintExcessFees[5].SummaryFeeDesc);
			AssertEquals(125.14m, printBO.EntryPrintExcessFees[5].SummaryFee);
			AssertEquals("106 106 Desc from DB", printBO.EntryPrintExcessFees[6].SummaryFeeDesc);
			AssertEquals(3.33m, printBO.EntryPrintExcessFees[6].SummaryFee);
			AssertEquals("107 107 Desc from DB", printBO.EntryPrintExcessFees[7].SummaryFeeDesc);
			AssertEquals(1.11m, printBO.EntryPrintExcessFees[7].SummaryFee);
			AssertEquals("108 108 Desc from DB", printBO.EntryPrintExcessFees[8].SummaryFeeDesc);
			AssertEquals(6.66m, printBO.EntryPrintExcessFees[8].SummaryFee);
			AssertEquals("109 109 Desc from DB", printBO.EntryPrintExcessFees[9].SummaryFeeDesc);
			AssertEquals(200m, printBO.EntryPrintExcessFees[9].SummaryFee);
			AssertEquals("499 499 Desc from DB", printBO.EntryPrintExcessFees[10].SummaryFeeDesc);
			AssertEquals(94.5m, printBO.EntryPrintExcessFees[10].SummaryFee);
			AssertEquals("501 501 Desc from DB", printBO.EntryPrintExcessFees[11].SummaryFeeDesc);
			AssertEquals(56.25m, printBO.EntryPrintExcessFees[11].SummaryFee);
		}

		[TestDate(2014, 6, 5)]
		public void TestTotalOtherFeesForReWarehouseEntryType22()
		{
			declaration = null;
			entry = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			Declaration.US_EnableENS = true;
			var entryLine1 = Entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(outMsg);
			outMsg.EM_MessageText = "B018888123EI                                               50215                " +
"10A888891-01319900091-013199000                 8         XJ5 7002961222891  IL " +
"20     MSC ORNELLA         105301042110B00153415            22N  042110V136     " +
"22            MSCUG1026256                        00000300BX         MSCU       " +
"30                                  0               2050310             MSCU    " +
"350000000000000000000000000000000000000000027301                                " +
"40001GB00000003280000000041                    000000002947507                  " +
"50 15099020000000000205000000004100KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000041                                                         " +
"62          49900000069                                                         " +
"40002GB00000003640000000180                    000000003247507                  " +
"50 1902192030          000000018000KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000046                                                         " +
"62          49900000076                                                         " +
"40003GB00000013240000000247                    000000011547507                  " +
"50 15091020000000001235000000024700KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000166                                                         " +
"62          49900000278                                                         " +
"40004GB00000003490000000164                    000000003047507                  " +
"50 21032040200000004048000000016400KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000044                                                         " +
"62          49900000073                                                         " +
"40005GB00000001960000000024                    000000001747507                  " +
"50 21039090910000001254000000002400KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000025                                                         " +
"62          49900000041                                                         " +
"40006GB00000224740000012351                    000000195347507                  " +
"50 1902192030          000001235100KG                               IT041510N   " +
"51                                                                              " +
"600000019777C475819005                    GBBOOMED295LON            00088     0 " +
"62          50100002809                                                         " +
"62          49900004720                                                         " +
"40007GB00000085500000004077                    000000074347507                  " +
"50 1902192030          000000407700KG                               IT041510N   " +
"51                                                                              " +
"600000007524C475819005                    GBBOOMED295LON            00088     0 " +
"62          50100001069                                                         " +
"62          49900001796                                                         " +
"40008GB00000009420000000240                    000000008247507                  " +
"50 1902112030          000000024000KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000118                                                         " +
"62          49900000198                                                         " +
"895010000000431849900000007251                                                  " +
"9000000006742           0 00000027301           0000001156900000034527          " +
"Y  8888123EI00055000000006742";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);
			inMsg.EM_MessageText = @"B018888XJ5ER                                               50530                E08888XJ5 70029612B00153552ACCEPTED - RECORDS REQUIRED             21808        Y  8888XJ5ER00001000000027000";

			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Summary Fee Desc 1", "013 CVD", printBO.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total CVD)", 273.01m, printBO.SummaryFee1);
			AssertEquals("Summary Fee 2", "499 499 Desc from DB", printBO.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total MPF)", 72.51m, printBO.SummaryFee2);
			AssertEquals("Summary Fee 3", "501 501 Desc from DB", printBO.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total HMF)", 43.18m, printBO.SummaryFee3);
			AssertEquals("Nothing to pay for rewarehouse entry", 0m, printBO.TotalOtherFees);
			AssertEquals("Nothing to pay for rewarehouse entry", 0m, printBO.TotalOther);
		}

		public void TestTotalOtherFeesForWarehouseEntryType21()
		{
			declaration = null;
			entry = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			Declaration.US_EnableENS = true;
			var entryLine1 = Entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(outMsg);
			outMsg.EM_MessageText = "B018888123EI                                               50215                " +
"10A888891-01319900091-013199000                 8         XJ5 7002961221891  IL " +
"20     MSC ORNELLA         105301042110B00153415            22N  042110V136     " +
"22            MSCUG1026256                        00000300BX         MSCU       " +
"30                                  0               2050310             MSCU    " +
"350000000000000000000000000000000000000000027301                                " +
"40001GB00000003280000000041                    000000002947507                  " +
"50 15099020000000000205000000004100KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000041                                                         " +
"62          49900000069                                                         " +
"40002GB00000003640000000180                    000000003247507                  " +
"50 1902192030          000000018000KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000046                                                         " +
"62          49900000076                                                         " +
"40003GB00000013240000000247                    000000011547507                  " +
"50 15091020000000001235000000024700KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000166                                                         " +
"62          49900000278                                                         " +
"40004GB00000003490000000164                    000000003047507                  " +
"50 21032040200000004048000000016400KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000044                                                         " +
"62          49900000073                                                         " +
"40005GB00000001960000000024                    000000001747507                  " +
"50 21039090910000001254000000002400KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000025                                                         " +
"62          49900000041                                                         " +
"40006GB00000224740000012351                    000000195347507                  " +
"50 1902192030          000001235100KG                               IT041510N   " +
"51                                                                              " +
"600000019777C475819005                    GBBOOMED295LON            00088     0 " +
"62          50100002809                                                         " +
"62          49900004720                                                         " +
"40007GB00000085500000004077                    000000074347507                  " +
"50 1902192030          000000407700KG                               IT041510N   " +
"51                                                                              " +
"600000007524C475819005                    GBBOOMED295LON            00088     0 " +
"62          50100001069                                                         " +
"62          49900001796                                                         " +
"40008GB00000009420000000240                    000000008247507                  " +
"50 1902112030          000000024000KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000118                                                         " +
"62          49900000198                                                         " +
"895010000000431849900000007251                                                  " +
"9000000006742           0 00000027301           0000001156900000034527          " +
"Y  8888123EI00055000000006742";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);
			inMsg.EM_MessageText = @"B018888XJ5ER                                               50530                E08888XJ5 70029612B00153552ACCEPTED - RECORDS REQUIRED             21808        Y  8888XJ5ER00001000000027000";

			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Summary Fee Desc 1", "013 CVD", printBO.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total CVD)", 273.01m, printBO.SummaryFee1);
			AssertEquals("Summary Fee 2", "499 499 Desc from DB", printBO.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total MPF)", 72.51m, printBO.SummaryFee2);
			AssertEquals("Summary Fee 3", "501 501 Desc from DB", printBO.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total HMF)", 43.18m, printBO.SummaryFee3);
			AssertEquals("Total Other Fees should only be HMF for warehouse entry", 43.18m, printBO.TotalOtherFees);
			AssertEquals("Total Other", 43.18m, printBO.TotalOther);
		}

		public void TestBLUBillDetailsAreNotPrintedWhenBLURejected()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MAWB1";
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";

			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_CU_ParentBill = masterBill.PK;
			houseBill1.CU_BillNum = "HAWB1";
			houseBill1.US_UI_NKBillIssuerSCAC = "HLMU";
			houseBill1.CU_NoOfPacks = 250m;
			houseBill1.CU_PackType = ShippingOrPackingingUnitList.Codes.Chest;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var outgoingMessage = builder.PopulateMessage();
			Factory.Save();

			var incomingMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			incomingMsg.EM_LinkedObject = entry;
			incomingMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			var printBO = new EntryMessageENS7501Print(entry, outgoingMessage, incomingMsg, null);
			AssertEquals("EntryPrintBills", 1, printBO.EntryPrintBills.Count);

			var billDetail1 = printBO.EntryPrintBills[0];

			AssertEquals("EffectiveMasterBillIssuerSCAC", "APLU", billDetail1.EffectiveMasterBillIssuerSCAC);
			AssertEquals("MasterBill", "MAWB1", billDetail1.MasterBill);
			AssertEquals("EffectiveHouseBillIssuerSCAC", "HLMU", billDetail1.EffectiveHouseBillIssuerSCAC);
			AssertEquals("HouseBill", "HAWB1", billDetail1.HouseBill);
			AssertEquals("EffectiveSubHouseBillIssuerSCAC", "", billDetail1.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("SubHouseBill", "", billDetail1.SubHouseBill);
			AssertEquals("PkgQty", 250, billDetail1.PkgQty);
			AssertEquals("PkgType", ShippingOrPackingingUnitList.Codes.Chest, billDetail1.PkgType);

			houseBill1.US_UI_NKBillIssuerSCAC = "TNTU";
			houseBill1.CU_NoOfPacks = 280m;
			houseBill1.CU_PackType = ShippingOrPackingingUnitList.Codes.Case;

			Factory.Save();

			var bluBuilder = new MessageBuilders.Testing.TestBillOfLadingUpdateBuilder(declaration);
			var bluMessage = bluBuilder.PopulateMessage();
			Factory.Save();

			var incomingBLUResponseMsg = Factory.New<MQEDIMessage>();
			incomingBLUResponseMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingBLUResponseMsg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingBLUResponseMsg.EM_MessageType = ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse;
			incomingBLUResponseMsg.EM_MessageSubType = EM_MessageSubTypeList.Codes.BillOfLadingUpdate;
			incomingBLUResponseMsg.EM_MessageNum = "4";
			incomingBLUResponseMsg.EM_LinkUniqueID = declaration.PK;
			incomingBLUResponseMsg.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			incomingBLUResponseMsg.EM_MessageText =
"B018888XJ5LS                                               4                    " +
"L11902XJ5 70034851N            APLU18S        I104                              " +
"L71902XJ5 70034851999DB ERR = ABE  /REDKX/14                                    " +
"L71902XJ5 700348518VDINVALID ABI BROKER                                         " +
"L71902XJ5 700348518WBENTRY BELONGS TO ANOTHER DD/PP                             " +
"L71902XJ5 70034851524TRANSACTION DATA REJECTED                                  " +
"Y  8888XJ5LS00005";
			Factory.Save();
			entry.Declaration.Messages.Load();

			printBO = new EntryMessageENS7501Print(entry, outgoingMessage, incomingMsg, null);

			billDetail1 = printBO.EntryPrintBills[0];
			AssertEquals("Bill Details should not change - BLU MSG was rejected: EffectiveMasterBillIssuerSCAC", "APLU", billDetail1.EffectiveMasterBillIssuerSCAC);
			AssertEquals("Bill Details should not change - BLU MSG was rejected: MasterBill", "MAWB1", billDetail1.MasterBill);
			AssertEquals("Bill Details should not change - BLU MSG was rejected: EffectiveHouseBillIssuerSCAC", "HLMU", billDetail1.EffectiveHouseBillIssuerSCAC);
			AssertEquals("Bill Details should not change - BLU MSG was rejected: HouseBill", "HAWB1", billDetail1.HouseBill);
			AssertEquals("Bill Details should not change - BLU MSG was rejected: EffectiveSubHouseBillIssuerSCAC", "", billDetail1.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals("Bill Details should not change - BLU MSG was rejected: SubHouseBill", "", billDetail1.SubHouseBill);
			AssertEquals("Bill Details should not change - BLU MSG was rejected: PkgQty", 250, billDetail1.PkgQty);
			AssertEquals("Bill Details should not change - BLU MSG was rejected: PkgType", ShippingOrPackingingUnitList.Codes.Chest, billDetail1.PkgType);
		}

		public void TestENSPrintShowsCensusStatus()
		{
			declaration = null;
			entry = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			var entryLine1 = Entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(outMsg);
			outMsg.EM_MessageText = "B018888XJ5EI                                               48651                10A888891-01319900091-013199000                 8         XJ5 7002715201891  IL 20                         403901030210B00153178            297  030210I313     22            33444444444                         00000001AE                    30                                  01              2031210             BA      40001GB00000500000000454500                    0000007500                       50 84563010200000175000000000034400NO                               GB030210N   51                                                                              60                                        GBBOOMED295LON                        62          49900010500                                                         8949900000010500                                                                9000000175000           0                       0000001050000000050000          Y  8888XJ5EI00011000000175000";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);
			inMsg.EM_MessageText = "B018888XJ5ER                                               48651                10A888891-01319900091-013199000                 8         XJ5 7002715201891  IL 40001GB00000500000000454500000000000000000000000000007500                       E408888XJ5 7002715200127H01   *CENSUS* GROSS WEIGHT - AIR              B0015317850 84563010200000175000000000034400NO                               GB030210N   E508888XJ5 7002715200127C01   *CENSUS* OR-LO VAL/QTY(1) TARIFF1        B001531789000000175000000000000000 00000000000000000000000000001050000000050000          E908888XJ5 70027152   58401808ENT-SUM ACCEPTED WITH WARNINGS           B00153178E08888XJ5 70027152B00153178CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00008000000175000";

			Entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;
			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Should show as Census warning entry", USConstants.EntrySummaryDisposition.CensusWarning, printBO.SummaryStatus);
		}

		public void TestENSPrintShowsDocumentsRequiredStatus()
		{
			entry = null;
			declaration = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               47541                10A888823-13063440023-130634400                 8         XJ5 7002705301891  PA 20     TITANIC             118888030110B00153152            1234 030110W607 001 22            TESTBILL                            00000150PK         APLU       30                                  0               1                   APLU    40001AU00000100000000009900                    000001000041380                  50 39209950000000058000000000800000KG                               GB021510N   51                                                                              60                                        AUABCEXP72ALE                         62          50100001250                                                         62          49900002100                                                         895010000000125049900000002500                                                  9000000058000           0                       0000000375000000010000          Y  8888XJ5EI00012000000058000";
			outMsg.EM_Status = "";
			outMsg.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               47541                E08888XJ5 70027053B00153152ACCEPTED - RECORDS REQUIRED             01808        Y  8888XJ5ER00001000000058000";
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decReloaded = factory2.Load<JobDeclaration>(Declaration.PK);
			CusEntryHeader entryReloaded = factory2.Load<CusEntryHeader>(decReloaded.ActiveEntryHeaders.EntrySummaryEntry.PK);

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entryReloaded, outMsg, inMsg, null);
			AssertEquals("Should show as Documents Required entry", USConstants.EntrySummaryDisposition.DocsRequired, printBO.SummaryStatus);
		}

		public void TestENSPrintShowsDocsRequiredStatusFromPaperlessDBField()
		{
			declaration = null;
			entry = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_PaperlessEntry = YesNoDefaultList.Codes.No;
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outMsg.EM_LinkUniqueID = Entry.PK;
			outMsg.EM_MessageText = "B018888XJ5EI                                               47541                10A888823-13063440023-130634400                 8         XJ5 7002705301891  PA 20     TITANIC             118888030110B00153152            1234 030110W607 001 22            TESTBILL                            00000150PK         APLU       30                                  0               1                   APLU    40001AU00000100000000009900                    000001000041380                  50 39209950000000058000000000800000KG                               GB021510N   51                                                                              60                                        AUABCEXP72ALE                         62          50100001250                                                         62          49900002100                                                         895010000000125049900000002500                                                  9000000058000           0                       0000000375000000010000          Y  8888XJ5EI00012000000058000";
			outMsg.EM_ApplicationCode = "USI";
			outMsg.EM_MessageType = "EI";
			outMsg.EM_Status = "";
			outMsg.EM_ReceiveTransmit = "TRX";
			outMsg.EM_SystemCreateTimeUtc = ZDateTime.Now;

			MQEDIMessage inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               47541                E08888XJ5 70027053B00153152ACCEPTED - RECORDS REQUIRED             01808        Y  8888XJ5ER00001000000058000";
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("Should show as Docs Required entry", USConstants.EntrySummaryDisposition.DocsRequired, printBO.SummaryStatus);
		}

		public void TestENSPrintShowsDocumentsRequiredStatusWhenUpdatedAfterOriginalPaperless()
		{
			entry = null;
			declaration = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			MQEDIMessage outMsgOriginal = message;
			outMsgOriginal.EM_MessageText = "B01    GAZEI                                               35183                10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";
			outMsgOriginal.EM_Status = "";
			outMsgOriginal.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);

			MQEDIMessage inMsgOriginal = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsgOriginal.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsgOriginal.EM_LinkUniqueID = Entry.PK;
			inMsgOriginal.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";
			inMsgOriginal.EM_ApplicationCode = "USI";
			inMsgOriginal.EM_MessageType = "ER";
			inMsgOriginal.EM_Status = "";
			inMsgOriginal.EM_ReceiveTransmit = "RCV";
			inMsgOriginal.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

			builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message2 = builder.PopulateMessage();
			Factory.Save();

			MQEDIMessage outMsg = message2;
			outMsg.EM_MessageText = "B018888XJ5EI                                               47541                10A888823-13063440023-130634400                 8         XJ5 7002705301891  PA 20     TITANIC             118888030110B00153152            1234 030110W607 001 22            TESTBILL                            00000150PK         APLU       30                                  0               1                   APLU    40001AU00000100000000009900                    000001000041380                  50 39209950000000058000000000800000KG                               GB021510N   51                                                                              60                                        AUABCEXP72ALE                         62          50100001250                                                         62          49900002100                                                         895010000000125049900000002500                                                  9000000058000           0                       0000000375000000010000          Y  8888XJ5EI00012000000058000";
			outMsg.EM_Status = "";
			outMsg.EM_SystemCreateTimeUtc = ZDateTime.Now;

			MQEDIMessage inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               47541                E08888XJ5 70027053B00153152ACCEPTED - RECORDS REQUIRED             01808        Y  8888XJ5ER00001000000058000";
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decReloaded = factory2.Load<JobDeclaration>(Declaration.PK);
			CusEntryHeader entryReloaded = factory2.Load<CusEntryHeader>(decReloaded.ActiveEntryHeaders.EntrySummaryEntry.PK);

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entryReloaded, outMsg, inMsg, null);
			AssertEquals("Should show as Documents Required entry when printing from second message", USConstants.EntrySummaryDisposition.DocsRequired, printBO.SummaryStatus);
		}

		public void TestENSPrintShowsDocsRequiredStatusWhenUpdatedAfterOriginalyPaperlessFromPaperlessDBField()
		{
			entry = null;
			declaration = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_PaperlessEntry = YesNoDefaultList.Codes.Yes;
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			MQEDIMessage outMsgOriginal = message;
			outMsgOriginal.EM_MessageText = "B01    GAZEI                                               35183                10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";
			outMsgOriginal.EM_Status = "";
			outMsgOriginal.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);

			MQEDIMessage inMsgOriginal = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsgOriginal.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsgOriginal.EM_LinkUniqueID = Entry.PK;
			inMsgOriginal.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";
			inMsgOriginal.EM_ApplicationCode = "USI";
			inMsgOriginal.EM_MessageType = "ER";
			inMsgOriginal.EM_Status = "";
			inMsgOriginal.EM_ReceiveTransmit = "RCV";
			inMsgOriginal.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsgOriginal, inMsgOriginal, null);
			AssertEquals("Should show as PPLS entry when printing from original message", USConstants.EntrySummaryDisposition.Paperless, printBO.SummaryStatus);

			Declaration.US_PaperlessEntry = YesNoDefaultList.Codes.No;

			MQEDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               47541                10A888823-13063440023-130634400                 8         XJ5 7002705301891  PA 20     TITANIC             118888030110B00153152            1234 030110W607 001 22            TESTBILL                            00000150PK         APLU       30                                  0               1                   APLU    40001AU00000100000000009900                    000001000041380                  50 39209950000000058000000000800000KG                               GB021510N   51                                                                              60                                        AUABCEXP72ALE                         62          50100001250                                                         62          49900002100                                                         895010000000125049900000002500                                                  9000000058000           0                       0000000375000000010000          Y  8888XJ5EI00012000000058000";
			outMsg.EM_Status = "";
			outMsg.EM_SystemCreateTimeUtc = ZDateTime.Now;

			MQEDIMessage inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               47541                E08888XJ5 70027053B00153152ACCEPTED - RECORDS REQUIRED             01808        Y  8888XJ5ER00001000000058000";
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);

			CusEntryHeader updatedEntry = Declaration.ActiveEntryHeaders[0];
			printBO = new EntryMessageENS7501Print(updatedEntry, outMsg, inMsg, null);
			AssertEquals("Should show as Documents Required entry when printing from second message", USConstants.EntrySummaryDisposition.DocsRequired, printBO.SummaryStatus);
		}

		public void TestENSPrintShowsDocumentsRequiredStatusWhenUpdatedAfterOriginalCensus()
		{
			//based on CMR Job B00153574
			declaration = null;
			entry = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = Declaration.CustomsEntryHeaders[0];
			Entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			MQEDIMessage outMsgOriginal = message;
			outMsgOriginal.EM_MessageText = "B018888XJ5EI                                               50583                10A888891-01319900091-013199000                 8         XJ5 7002973701891  WA 20     23                  112704052610B00153574            56   052610W138     22            UIE90                               00000001PK         APLU       30                                  0               2060810             APLU    40001KR00000146040000001500                    000000005055976                  50 61042200100000232204            DOZ            KG                SG052610N   51                  335                                                         60                                        KRJUHCOR3325KIM                       62          50100001826                                                         62          49900003067                                                         706102200020           000000150000DOZ000000012500KG                            895010000000182649900000003067                                                  9000000232204           0                       0000000489300000014604          Y  8888XJ5EI00013000000232204";
			outMsgOriginal.EM_Status = "";
			outMsgOriginal.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);

			MQEDIMessage inMsgOriginal = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsgOriginal.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsgOriginal.EM_LinkUniqueID = Entry.PK;
			inMsgOriginal.EM_MessageText = "B018888XJ5ER                                               50583                10A888891-01319900091-013199000                 8         XJ5 7002973701891  WA 40001KR0000014604000000150000000000000000000000000000005055976                  50 61042200100000232204            DOZ            KG                SG052610N   E508888XJ5 7002973700127D01   *CENSUS* OR-HI VAL/QTY (1)TARIFF1        B001535749000000232204000000000000 00000000000000000000000000000489300000014604          E908888XJ5 70029737   58401808ENT-SUM ACCEPTED WITH WARNINGS           B00153574Y  8888XJ5ER00006000000232204";
			inMsgOriginal.EM_ApplicationCode = "USI";
			inMsgOriginal.EM_MessageType = "ER";
			inMsgOriginal.EM_Status = "";
			inMsgOriginal.EM_ReceiveTransmit = "RCV";
			inMsgOriginal.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsgOriginal, inMsgOriginal, null);
			AssertEquals("Should show as Census entry after original messages", USConstants.EntrySummaryDisposition.CensusWarning, printBO.SummaryStatus);

			builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message2 = builder.PopulateMessage();
			Factory.Save();

			MQEDIMessage outMsg = message2;
			outMsg.EM_MessageText = "B018888XJ5EI                                               50584                10A888891-01319900091-013199000                 8         XJ5 7002973701891  WA 20     23                  112704052610B00153574            56   052610W138     22            UIE90                               00000001PK         APLU       30                                  0               2060810             APLU    40001KR00000146040000001500                    000000005055976                  50 61042200100000232204000000150000DOZ000000012500KG                SG052610N   51                  335                                                         60                                        KRJUHCOR3325KIM                       62          50100001826                                                         62          49900003067                                                         706102200020           000000150000DOZ000000012500KG                            895010000000182649900000003067                                                  9000000232204           0                       0000000489300000014604          Y  8888XJ5EI00013000000232204";
			outMsg.EM_Status = "";
			outMsg.EM_SystemCreateTimeUtc = ZDateTime.Now;

			MQEDIMessage inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               50584                E08888XJ5 70029737B00153574ACCEPTED - RECORDS REQUIRED             01808        Y  8888XJ5ER00001000000232204";
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decReloaded = factory2.Load<JobDeclaration>(Declaration.PK);
			CusEntryHeader entryReloaded = factory2.Load<CusEntryHeader>(decReloaded.ActiveEntryHeaders.EntrySummaryEntry.PK);

			entryReloaded.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			printBO = new EntryMessageENS7501Print(entryReloaded, outMsg, inMsg, null);
			AssertEquals("Should now show as Documents Required entry when printing from second message", USConstants.EntrySummaryDisposition.DocsRequired, printBO.SummaryStatus);
		}

		public void TestENSPrintShowsPaperlessAfterSelectivityAlreadyPerformedResponse()
		{
			entry = null;
			declaration = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			MQEDIMessage outMsgOriginal = message;
			outMsgOriginal.EM_MessageText = "B018888XJ5EI                                               38875                10A888891-01319900091-013199000                 8         XJ5 7002359901891  IL 20                         408888120509B00152842            001  120509I317     22            00122332236                         00000001PC                    30                                  01              2121609             AA      40001GB00000010000000000100                    0000000250                       50 0901110010          000000009000KG                               GB120509N   OI        COFFEE                                                                FD0100131AAH01   GBSLNSMITH                    GBBOOMED295LON GBBOOMED295LON    FD020000010000KG                                                                FD030000001000            COFFEE                                03010301        FD04              JEREMY JON3724590000                                          FD05ADA12052009                                                                 FD05APA8888                                                                     FD05ATA1200                                                                     FD05CSHGB                                                                       FD05OFTI                                                                        FD05PFR12345678901                                                              FD05PFTM                                                                        FD05SA165 DEERFIELD RD                                                          FD05SACDEERFIELD                                                                FD05SASIL                                                                       FD05SCCUS                                                                       FD05SCNKRAFT FOODS INTERNATIONA                                                 FD05SCZ60046                                                                    FD05SEMNONE                                                                     FD05SFNJOE                                                                      FD05SFTI                                                                        FD05SFX3125552121                                                               FD05SPN3125552020                                                               FD05VFT001                                                                      FD05AWB00122332236                                                              51                                                                              60                                        GBBOOMED295LON                        62          49900000210                                                         8949900000002500                                                                90                      0                       0000000250000000001000          Y  8888XJ5EI00036";
			outMsgOriginal.EM_Status = "";
			outMsgOriginal.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-4);

			MQEDIMessage inMsgOriginal = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsgOriginal.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsgOriginal.EM_LinkUniqueID = Entry.PK;
			inMsgOriginal.EM_MessageText = "B018888XJ5ER                                               38875                E08888XJ5 70023599B00152842PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70023599B00152842CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002";
			inMsgOriginal.EM_ApplicationCode = "USI";
			inMsgOriginal.EM_MessageType = "ER";
			inMsgOriginal.EM_Status = "";
			inMsgOriginal.EM_ReceiveTransmit = "RCV";
			inMsgOriginal.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-3);

			builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message2 = builder.PopulateMessage();
			Factory.Save();

			MQEDIMessage outMsg2 = message2;
			outMsg2.EM_MessageText = "B018888XJ5EI                                               39301                10A888891-01319900091-013199000                 8         XJ5 7002359901891  IL 20                         408888120509B00152842            001  120509I317     22            00122332236                         00000001PC                    30                                  0               2121609             AA      40001GB00000015630000000100                    0000000250                       50 0901110010          000000009000KG                               GB120509N   51                                                                              60                                        GBBOOMED295LON                        62          49900000328                                                         8949900000002500                                                                90                      0                       0000000250000000001563          Y  8888XJ5EI00011";
			outMsg2.EM_Status = "";
			outMsg2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);

			MQEDIMessage inMsg2 = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg2.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg2.EM_LinkUniqueID = Entry.PK;
			inMsg2.EM_MessageText = "B018888XJ5ER                                               39301                E08888XJ5 70023599B00152842PAPERLESS - FILER RETAIN RECORDS        0180857A     Y  8888XJ5ER00001";
			inMsg2.EM_ApplicationCode = "USI";
			inMsg2.EM_MessageType = "ER";
			inMsg2.EM_Status = "";
			inMsg2.EM_ReceiveTransmit = "RCV";
			inMsg2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

			builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message3 = builder.PopulateMessage();
			Factory.Save();

			MQEDIMessage outMsg3 = message3;
			outMsg3.EM_MessageText = "B018888XJ5EI                                               39302                10A888891-01319900091-013199000                 8         XJ5 7002359901891  IL 20                         408888120509B00152842            001  120509I317     22            00122332236                         00000001PC                    30                                  01              2121609             AA      40001GB00000015630000000100                    0000000250                       50 0901110010          000000009000KG                               GB120509N   OI        COFFEE                                                                FD0100131AAH01   GBPNC095529242653             GBBOOMED295LON GBBOOMED295LON    FD020000010000KG                                                                FD030000001563            COFFEE                                03010301        FD04              JEREMY JON3724590000                                          51                                                                              60                                        GBBOOMED295LON                        62          49900000328                                                         8949900000002500                                                                90                      0                       0000000250000000001563          Y  8888XJ5EI00016";
			outMsg3.EM_Status = "";
			outMsg3.EM_SystemCreateTimeUtc = ZDateTime.Now;

			MQEDIMessage inMsg3 = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg3.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg3.EM_LinkUniqueID = Entry.PK;
			inMsg3.EM_MessageText = "B018888XJ5ER                                               39302                10A888891-01319900091-013199000                 8         XJ5 7002359901891  IL 40001GB00000015630000000100000000000000000000000000000250                       50 0901110010          000000009000KG                               GB120509N   OI        COFFEE                                                                EFD8888XJ5 700235990013A501   SELECTIVITY ALREADY PERFORMED            B00152842EFD8888XJ5 70023599001VBR01   OGA DATA IGNORED                         B001528429000000000000000000000000 00000000000000000000000000000250000000001563          E908888XJ5 70023599   58401808ENT-SUM ACCEPTED WITH WARNINGS           B00152842EC8888XJ5 70023599B00152842CERT-SELECTIVITY ALREADY PERFORMED      018083A5     Y  8888XJ5ER00009";
			inMsg3.EM_ApplicationCode = "USI";
			inMsg3.EM_MessageType = "ER";
			inMsg3.EM_Status = "";
			inMsg3.EM_ReceiveTransmit = "RCV";
			inMsg3.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decReloaded = factory2.Load<JobDeclaration>(Declaration.PK);
			CusEntryHeader entryReloaded = factory2.Load<CusEntryHeader>(decReloaded.ActiveEntryHeaders.EntrySummaryEntry.PK);

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entryReloaded, outMsg3, inMsg3, null);
			AssertEquals("Should show as Paperless entry", USConstants.EntrySummaryDisposition.Paperless, printBO.SummaryStatus);
		}

		public void TestENSPrintShowsDocumentsRequiredAfterStatementRemovalResponse()
		{
			entry = null;
			declaration = null;

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			MQEDIMessage outMsgOriginal = message;
			outMsgOriginal.EM_MessageText = "B018888XJ5EI                                               49064                10A888891-01319900091-013199000                 8         XJ5 7002721001891  IL 20                         403901030510B00153190            297  030510I313     22            12599988851                         00000001PC                    30                                  01              2031710             BA      40001GB00000010000000000100                    0000000100                       50 84669395850000004700            X                                GB030510N   51                                                                              60                                        GBBOOMED295LON                        62          49900000210                                                         8949900000002500                                                                9000000004700           0                       0000000250000000001000          Y  8888XJ5EI00011000000004700";
			outMsgOriginal.EM_ApplicationCode = "USI";
			outMsgOriginal.EM_MessageType = "EI";
			outMsgOriginal.EM_Status = "";
			outMsgOriginal.EM_ReceiveTransmit = "TRX";
			outMsgOriginal.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-4);

			MQEDIMessage inMsgOriginal = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsgOriginal.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsgOriginal.EM_LinkUniqueID = Entry.PK;
			inMsgOriginal.EM_MessageText = "B018888XJ5ER                                               49064                E08888XJ5 70027210B00153190PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70027210B00153190CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000004700";
			inMsgOriginal.EM_ApplicationCode = "USI";
			inMsgOriginal.EM_MessageType = "ER";
			inMsgOriginal.EM_Status = "";
			inMsgOriginal.EM_ReceiveTransmit = "RCV";
			inMsgOriginal.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-3);

			builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message2 = builder.PopulateMessage();
			Factory.Save();

			MQEDIMessage outMsg2 = message2;
			outMsg2.EM_MessageText = "B018888XJ5EI                                               49098                10A888891-01319900091-013199000                 8         XJ5 7002721001891  IL 20                         403901030510B00153190            297  030510I313     22            12599988851                         00000001PC                    30                                  0               2031710             BA      40001GB00000005000000000050                    0000000050                       50 84669395850000002350            X                                GB030510N   51                                                                              60                                        GBBOOMED295LON                        62          49900000105                                                         40002GB00000001000000000010                    0000000010                       50 85371090700000000270000000001000NO                               GB030510N   51                                                                              60                                        GBBOOMED295LON                        62          49900000021                                                         40003GB00000001000000000010                    0000000010                       50 85371090700000000270000000001000NO                               GB030510N   51                                                                              60                                        GBBOOMED295LON                        62          49900000021                                                         40004GB00000003000000000030                    0000000030                       50 85371090700000000810000000003000NO                               GB030510N   51                                                                              60                                        GBBOOMED295LON                        62          49900000063                                                         8949900000002500                                                                9000000003700           0                       0000000250000000001000          Y  8888XJ5EI00026000000003700";
			outMsg2.EM_Status = "";
			outMsg2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);

			MQEDIMessage inMsg2 = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg2.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg2.EM_LinkUniqueID = Entry.PK;
			inMsg2.EM_MessageText = "B018888XJ5ER                                               49098                E08888XJ5 70027210B00153190PAPERLESS - FILER RETAIN RECORDS        0180857A     Y  8888XJ5ER00001000000003700";
			inMsg2.EM_ApplicationCode = "USI";
			inMsg2.EM_MessageType = "ER";
			inMsg2.EM_Status = "";
			inMsg2.EM_ReceiveTransmit = "RCV";
			inMsg2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

			builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message3 = builder.PopulateMessage();
			Factory.Save();

			MQEDIMessage outMsg3 = message3;
			outMsg3.EM_MessageText = "B018888XJ5HP                                               49495                H8888XJ5 700272101                                                              Y  8888XJ5HP00001";
			outMsg3.EM_MessageType = "HP";
			outMsg3.EM_Status = "";
			outMsg3.EM_SystemCreateTimeUtc = ZDateTime.Now;

			MQEDIMessage inMsg3 = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg3.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg3.EM_LinkUniqueID = Entry.PK;
			inMsg3.EM_MessageText = "B018888XJ5HT                                               49495                H18888XJ5 7002721057FSUMM REMVD FR STMT, DOCS NOW REQD        1      B00153190  Y  8888XJ5HT00001";
			inMsg3.EM_ApplicationCode = "USI";
			inMsg3.EM_MessageType = "HT";
			inMsg3.EM_Status = "";
			inMsg3.EM_ReceiveTransmit = "RCV";
			inMsg3.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			JobDeclaration decReloaded = factory2.Load<JobDeclaration>(Declaration.PK);
			CusEntryHeader entryReloaded = factory2.Load<CusEntryHeader>(decReloaded.ActiveEntryHeaders.EntrySummaryEntry.PK);

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entryReloaded, outMsg3, inMsg3, null);
			AssertEquals("Should show as Docs Required entry as the entry summary has been removed from the statement", USConstants.EntrySummaryDisposition.DocsRequired, printBO.SummaryStatus);
		}

		public void TestENS7501PrintsGeneralOrderNoWhenRequired()
		{
			Declaration.US_GeneralOrderNo = "483720958101";
			var entryLine1 = Entry.MergedLines.AddNew();
			var entryLine2 = Entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(outMsg);
			outMsg.EM_MessageText = "B01    GAZEI                                               28                   10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);

			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("LocationOfGoodsAndName should print General Order No in this case", "G.O. 483720958101", printBO.LocationOfGoodsAndName);
		}

		public void TestGeneralOrder13DigitNoWhenRequired()
		{
			Declaration.US_GeneralOrderNo = "2012272000392";
			var entryLine1 = Entry.MergedLines.AddNew();
			var entryLine2 = Entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(outMsg);
			outMsg.EM_MessageText = "B01    GAZEI                                               28                   10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);

			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("LocationOfGoodsAndName should print Formatted General Order No in this case", "GO-2012-2720-00392", printBO.LocationOfGoodsAndName);
		}

		public void TestENS7501PrintsFlightNoWhenRequired()
		{
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			var entryLine1 = Entry.MergedLines.AddNew();
			var entryLine2 = Entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(outMsg);
			outMsg.EM_MessageText = "B01    GAZEI                                               229                  10A2818            91-013199000                 0         GAZ 0000087901        20                         402818052809B00001104            592  052809         22            61803294852                         00000100KG                    30                                  0                                   SQ      40001SG00000008240000000000                                                     50 9802008068                                                       KR052809N   51                                                                              60                                                                              90                      0                                  00000000824          Y      GAZEI00009";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);

			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("LocationOfGoodsAndName should print the Flight No in this case", "592", printBO.LocationOfGoodsAndName);
		}

		public void TestENS7501PrintsUpdatedFlightNoAfterBLU()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "592";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MAWB1";
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";

			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_CU_ParentBill = masterBill.PK;
			houseBill1.CU_BillNum = "HAWB1";
			houseBill1.US_UI_NKBillIssuerSCAC = "HLMU";
			houseBill1.CU_NoOfPacks = 250m;
			houseBill1.CU_PackType = ShippingOrPackingingUnitList.Codes.Chest;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outMsg.EM_LinkUniqueID = entry.PK;
			outMsg.EM_MessageText = "B01    GAZEI                                               229                  10A2818            91-013199000                 0         GAZ 0000087901        20                         402818052809B00001104            592  052809         22            61803294852                         00000100KG                    30                                  0                                   SQ      40001SG00000008240000000000                                                     50 9802008068                                                       KR052809N   51                                                                              60                                                                              90                      0                                  00000000824          Y      GAZEI00009";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;

			var printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);
			AssertEquals("LocationOfGoodsAndName should print the Flight No in this case", "592", printBO.LocationOfGoodsAndName);

			declaration.JE_VoyageFlightNo = "QF219";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var bluBuilder = new MessageBuilders.Testing.TestBillOfLadingUpdateBuilder(declaration);
			var bluMessage = bluBuilder.PopulateMessage();
			Factory.Save();

			var incomingBLUResponseMsg = Factory.New<MQEDIMessage>();
			incomingBLUResponseMsg.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingBLUResponseMsg.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingBLUResponseMsg.EM_MessageType = ApplicationIdentifierCodeList.Codes.BillofLadingUpdateResponse;
			incomingBLUResponseMsg.EM_MessageSubType = EM_MessageSubTypeList.Codes.BillOfLadingUpdate;
			incomingBLUResponseMsg.EM_MessageNum = "2";
			incomingBLUResponseMsg.EM_LinkUniqueID = declaration.PK;
			incomingBLUResponseMsg.EM_LinkTable = JobDeclarationSchema.Constants.TableName;
			incomingBLUResponseMsg.EM_MessageText =
"B018888XJ5LS                                               2                    " +
"L78888XJ5 700214458VZBILL DATA UPDATED AS REQUESTED                             " +
"L78888XJ5 700214458VXBILL RESULTS REQUEST ACCEPTED                              " +
"Y  8888XJ5LS00002";
			Factory.Save();
			entry.Declaration.Messages.Load();

			printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, bluMessage);
			AssertEquals("LocationOfGoodsAndName should now print updated Flight No. from BLU message", "219", printBO.LocationOfGoodsAndName);
		}

		public void TestEntryMessageENS7501PrintForConsumptionInFTZ()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var consignee = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "92-100001238");
			consignee.OH_FullName = "Consignee For FTZ(06)";
			var address = consignee.MainAddress;
			address.OA_Address1 = "Consignee Test Address 1";
			address.OA_Address2 = "Consignee Test Address 2";
			address.OA_City = "HO KNG";
			address.OA_State = "HO";
			address.OA_PostCode = "97000";
			Declaration.IOROrgPK = consignee.PK;
			Declaration.JE_OA_ConsigneeAddress = consignee.MainAddress.PK;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();

			message.EM_MessageText = "B01    GAZEI                                               28                   10A888892-10000123892-10000123839-0920456WP     8         GAZ 0000019206732     20     FTZ001X             301601070108B00001029            593  070108N598     22            OB9394043938SMTAOS005870            00000200CT         MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			var printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);

			AssertEquals("Formatted entry number", "GAZ-0000019-2", printBO.FormattedEntryNumber);
			AssertEquals("EntryType", "06", printBO.EntryType);
			AssertEquals("EntryTypeCode", "ABI/N", printBO.EntryTypeCode);
			AssertEquals("SuretyCode", "732", printBO.SuretyCode);
			AssertEquals("ImportingCarrier", "FTZ 001X", printBO.ImportingCarrier);
			AssertEquals("USTransportMode", "", printBO.USTransportMode);
			AssertEquals("BondType", "8", printBO.BondType);
			AssertEquals("ImporterOfRecordCustomsRegNo", "92-100001238", printBO.ImporterOfRecordCustomsRegNo);
			AssertEquals("ImporterCompanyName", "Consignee For FTZ(06)", printBO.ImporterCompanyName);
			AssertEquals("ImporterAddressLine1", "Consignee Test Address 1", printBO.ImporterAddressLine1);
			AssertEquals("ImporterAddressLine2", "Consignee Test Address 2", printBO.ImporterAddressLine2);
			AssertEquals("ImporterCity", "HO KNG", printBO.ImporterCity);
			AssertEquals("ImporterState", "HO", printBO.ImporterState);
			AssertEquals("ImporterPostCode", "97000", printBO.ImporterPostCode);
			AssertEquals("LocationOfGoodsAndName", "N598/WANDO TERMINAL", printBO.LocationOfGoodsAndName);
			AssertEquals("SCACAndMBillNumber", "MAEUOB9394043938", printBO.SCACAndMBillNumber);
			AssertEquals("SchDArrival", "", printBO.SchDArrival);
			AssertEquals("SchDEntry", "8888", printBO.SchDEntry);
			AssertEquals("Block24ReferenceNumber", "39-0920456WP", printBO.Block24ReferenceNumber);
			AssertEquals("UniqueCountryOfOrigin", "CN", printBO.UniqueCountryOfOrigin);
			AssertEquals("UniqueCountryOfExport", "CN", printBO.UniqueCountryOfExport);
			AssertEquals("UniquePortOfLading", "", printBO.UniquePortOfLading);
			AssertEquals("ManufacturerID", USConstants.MultipleValueIndicator, printBO.ManufacturerID);
			AssertEquals("EffectiveUltimateConsigneeCustomsRegNo", "92-100001238", printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("EffectiveUltimateConsigneeCompanyName", "Consignee For FTZ(06)", printBO.EffectiveUltimateConsigneeCompanyName);
			AssertEquals("EffectiveUltimateConsigneeAddressLine1", "Consignee Test Address 1", printBO.EffectiveUltimateConsigneeAddressLine1);
			AssertEquals("EffectiveUltimateConsigneeAddressLine2", "Consignee Test Address 2", printBO.EffectiveUltimateConsigneeAddressLine2);
			AssertEquals("EffectiveUltimateConsigneeCity", "HO KNG", printBO.EffectiveUltimateConsigneeCity);
			AssertEquals("EffectiveUltimateConsigneeState", "HO", printBO.EffectiveUltimateConsigneeState);
			AssertEquals("EffectiveUltimateConsigneePostCode", "97000", printBO.EffectiveUltimateConsigneePostCode);
			AssertEquals("BrokerFileNo", "B00001000", printBO.BrokerFileNo);
			AssertEquals("TotalEnteredValue", 48628m, printBO.TotalEnteredValue);
			AssertEquals("Entry print bills count", 1, printBO.EntryPrintBills.Count);
			AssertEquals("Entry print lines count", 2, printBO.EntryPrintLines.Count);

			AssertEquals("Line 1 - Block29Element1", "CAT 276", printBO.EntryPrintLines[0].Block29Element1);
			AssertEquals("Line 1 - Block29Element2", "MID CNJINME22JIN", printBO.EntryPrintLines[0].Block29Element2);
			AssertEquals("Line 1 - Block29Element3", "RLNG 123456", printBO.EntryPrintLines[0].Block29Element3);

			AssertEquals("Line 2 - Block29Element1", "MID CNJINMEI3JIN", printBO.EntryPrintLines[1].Block29Element1);
			AssertEquals("Line 3 - Block29Element2", "RLNG 123456", printBO.EntryPrintLines[1].Block29Element2);
			AssertEquals("Line 2 - Block29Element3", "", printBO.EntryPrintLines[1].Block29Element3);

			// test for Country of Export

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Declaration.US_EnableENS = true;
			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine1.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;

			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			invoiceLine2.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Mexico;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			message = builder.PopulateMessage();
			Factory.Save();
			printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);
			AssertEquals("UniqueCountryOfExport", "", printBO.UniqueCountryOfExport);

			invoiceLine1.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Micronesia;
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mauritania;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry.MergedLines[0].CL_CustomsValue = 120m;
			entry.MergedLines[1].CL_CustomsValue = 154m;
			entry.MergedLines[2].CL_CustomsValue = 98m;

			builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			message = builder.PopulateMessage();
			Factory.Save();
			printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);
			AssertEquals("When there are multiple countries of export, that the highest entered value (customs value in USD) one is shown.", Core.Constants.CountryCodes.Canada, printBO.UniqueCountryOfExport);
		}

		public void TestEntryMessageENS7501PrintWithSecondaryTariffLines()
		{
			CusEntryLine entryLine1 = Entry.MergedLines.AddNew();
			CusEntryLine entryLine2 = Entry.MergedLines.AddNew();
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               27322                10A390191-01319900091-013199000                 8         XJ5 7000557001891  IL 20     CAPE SCOTT          103901111108B00151012            583  111108I310     22            003988      876766                  00000600CT         APLUAPLU   30                                  0               2112108             APLU    40001AU00000018520000000062                    000000110060267                  50 9802004040                                                       AU111108Y   51                                                                              60                                        US8495956                             709102111010 0000012871            NO                               0000003406  809802004040                                                        0000001010  819102111020 0000006080            NO                               0000001609  819802004040                                                                    819102111030 0000005083            NO                               0000001345  819802004040                                                        0000000204  819102111040                       NO                                           9000000024034           0                                  00000009426          Y  8888XJ5EI00016000000024034";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Formatted entry number", "XJ5-7000557-0", printBO.FormattedEntryNumber);
			AssertEquals("EntryType", "01", printBO.EntryType);
			AssertEquals("EntryTypeCode", "ABI/A", printBO.EntryTypeCode);
			AssertEquals("SuretyCode", "891", printBO.SuretyCode);
			AssertEquals("ImportingCarrier", "CAPE SCOTT (APLU)", printBO.ImportingCarrier);
			AssertEquals("USTransportMode", "10", printBO.USTransportMode);
			AssertEquals("BondType", "8", printBO.BondType);
			AssertEquals("ImporterOfRecordCustomsRegNo", "91-013199000", printBO.ImporterOfRecordCustomsRegNo);
			AssertEquals("LocationOfGoodsAndName", "I310/UNITED AIRLINES CARGO", printBO.LocationOfGoodsAndName);
			AssertEquals("SCACAndMBillNumber", "APLU003988", printBO.SCACAndMBillNumber);
			AssertEquals("SchDArrival", "3901", printBO.SchDArrival);
			AssertEquals("SchDEntry", "3901", printBO.SchDEntry);
			AssertEquals("Block24ReferenceNumber", "", printBO.Block24ReferenceNumber);
			AssertEquals("UniqueCountryOfOrigin", "AU", printBO.UniqueCountryOfOrigin);
			AssertEquals("UniqueCountryOfExport", "AU", printBO.UniqueCountryOfExport);
			AssertEquals("UniquePortOfLading", "60267", printBO.UniquePortOfLading);
			AssertEquals("ManufacturerID", "US8495956", printBO.ManufacturerID);
			AssertEquals("EffectiveUltimateConsigneeCustomsRegNo", USConstants.Same, printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("BrokerFileNo", "B00001000", printBO.BrokerFileNo);
			AssertEquals("TotalEnteredValue", 9426m, printBO.TotalEnteredValue);
			AssertEquals("Entry print bills count", 1, printBO.EntryPrintBills.Count);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);

			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", printBO.EntryPrintLines[0].SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.4040", printBO.EntryPrintLines[0].SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", printBO.EntryPrintLines[0].SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.4040", printBO.EntryPrintLines[0].SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", printBO.EntryPrintLines[0].SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.4040", printBO.EntryPrintLines[0].SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", printBO.EntryPrintLines[0].SecondaryLine7FormattedTariff);
		}

		public void TestEntryTypeWhenPaidByBatchedPeriodicStatement()
		{
			var entryLine1 = Entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();

			var outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               54306                10A272091-01319900091-013199000                 9         XJ5 7003435601891  CA 20                         402720101810B00154363            415  101810I313     2100000030008999955                                                             22            08100398440                         00000010PC                    30                                  0             116102810             QF      40001GB00000010000000000350                    0000000100                       50 21039080000000006400000000035000KG                               GB101810N   51                                                                              60                                        GBBOOMED295LON                        62          49900000210                                                         8949900000002500                                                                9000000006400           0                       0000000250000000001000          Y  8888XJ5EI00012000000006400";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("EntryType", "01", printBO.EntryType);
			AssertEquals("EntryTypeCode", "ABI/P", printBO.EntryTypeCode);
		}

		public void TestEntryMessageENS7501PrintWithSecondaryTariffLinesWhenNoDocData()
		{
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader entry = Declaration.FormalEntry;

			EDIMessage outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outMsg.EM_LinkUniqueID = entry.PK;
			outMsg.EM_MessageText = "B018888XJ5EI                                               27322                10A390191-01319900091-013199000                 8         XJ5 7000557001891  IL 20     CAPE SCOTT          103901111108B00151012            583  111108I310     22            003988      876766                  00000600CT         APLUAPLU   30                                  0               2112108             APLU    40001AU00000018520000000062                    000000110060267                  50 9802004040                                                       AU111108Y   51                                                                              60                                        US8495956                             709102111010 0000012871            NO                               0000003406  809802004040                                                        0000001010  819102111020 0000006080            NO                               0000001609  819802004040                                                                    819102111030 0000005083            NO                               0000001345  819802004040                                                        0000000204  819102111040                       NO                                           9000000024034           0                                  00000009426          Y  8888XJ5EI00016000000024034";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);

			AssertEquals("Formatted entry number", "XJ5-7000557-0", printBO.FormattedEntryNumber);
			AssertEquals("EntryType", "01", printBO.EntryType);
			AssertEquals("EntryTypeCode", "ABI/A", printBO.EntryTypeCode);
			AssertEquals("SuretyCode", "891", printBO.SuretyCode);
			AssertEquals("ImportingCarrier", "CAPE SCOTT (APLU)", printBO.ImportingCarrier);
			AssertEquals("USTransportMode", "10", printBO.USTransportMode);
			AssertEquals("BondType", "8", printBO.BondType);
			AssertEquals("ImporterOfRecordCustomsRegNo", "91-013199000", printBO.ImporterOfRecordCustomsRegNo);
			AssertEquals("LocationOfGoodsAndName", "I310/UNITED AIRLINES CARGO", printBO.LocationOfGoodsAndName);
			AssertEquals("SCACAndMBillNumber", "APLU003988", printBO.SCACAndMBillNumber);
			AssertEquals("SchDArrival", "3901", printBO.SchDArrival);
			AssertEquals("SchDEntry", "3901", printBO.SchDEntry);
			AssertEquals("Block24ReferenceNumber", "", printBO.Block24ReferenceNumber);
			AssertEquals("UniqueCountryOfOrigin", "AU", printBO.UniqueCountryOfOrigin);
			AssertEquals("UniqueCountryOfExport", "AU", printBO.UniqueCountryOfExport);
			AssertEquals("UniquePortOfLading", "60267", printBO.UniquePortOfLading);
			AssertEquals("ManufacturerID", "US8495956", printBO.ManufacturerID);
			AssertEquals("EffectiveUltimateConsigneeCustomsRegNo", USConstants.Same, printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("BrokerFileNo", "B00001000", printBO.BrokerFileNo);
			AssertEquals("TotalEnteredValue", 9426m, printBO.TotalEnteredValue);
			AssertEquals("Entry print bills count", 1, printBO.EntryPrintBills.Count);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);

			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", printBO.EntryPrintLines[0].SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.4040", printBO.EntryPrintLines[0].SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", printBO.EntryPrintLines[0].SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.4040", printBO.EntryPrintLines[0].SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", printBO.EntryPrintLines[0].SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.4040", printBO.EntryPrintLines[0].SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", printBO.EntryPrintLines[0].SecondaryLine7FormattedTariff);
		}

		public void TestMultipleLineWatchEntryWithSecondaryTariffLines()
		{
			CreateMulitLineWatchDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               216                  10A470113-33026600013-330266000                 0         GAZ 0000073901     NY 20                         404701      B00001087            823  031909E819     22            27200963874 S00010608               00000020CT                    30                                  0               3040209             K4  248 40001CN00000007430000000053                    0000000117                       50 91021125100000019200000000048000NO                               HK031909N   51                                                                              60                                        HKFIRINT499HON                        62          49900000206                                                         709102112520 0000000570000000048000NO                               0000000067  809102112530 0000001078000000048000NO                               0000000077  819102112540 0000000509000000048000NO                               0000000096  40002CN00000009410000000072                    0000000144                       50 91021125100000009600000000024000NO                               HK031909N   51                                                                              60                                        HKFIRINT499HON                        62          49900000248                                                         709102112520 0000000714000000024000NO                               0000000084  809102112530 0000001442000000024000NO                               0000000103  819102112540 0000000254000000024000NO                               0000000048  40003CN00000004680000000038                    0000000076                       50 91021145100000009600000000024000NO                               HK031909N   51                                                                              60                                        HKFIRINT499HON                        62          49900000131                                                         709102114520 0000000408000000024000NO                               0000000048  809102114530 0000000168000000024000NO                               0000000060  819102114540 0000000454000000024000NO                               0000000048  8949900000002500                                                                9000000043797           0                       0000000250000000002783          Y      GAZEI00030000000043797";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Entry print lines count", 3, printBO.EntryPrintLines.Count);

			AssertEquals("Parent 1 Tariff number", "9102.11.2510", printBO.EntryPrintLines[0].FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.2520", printBO.EntryPrintLines[0].SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9102.11.2530", printBO.EntryPrintLines[0].SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.2540", printBO.EntryPrintLines[0].SecondaryLine3FormattedTariff);
			AssertEquals("Remaining secondary lines should be empty", "", printBO.EntryPrintLines[0].SecondaryLine4FormattedTariff);
			AssertEquals("Remaining secondary lines should be empty", "", printBO.EntryPrintLines[0].SecondaryLine5FormattedTariff);
			AssertEquals("Remaining secondary lines should be empty", "", printBO.EntryPrintLines[0].SecondaryLine6FormattedTariff);
			AssertEquals("Remaining secondary lines should be empty", "", printBO.EntryPrintLines[0].SecondaryLine7FormattedTariff);

			AssertEquals("Parent 2 Tariff number", "9102.11.2510", printBO.EntryPrintLines[1].FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.2520", printBO.EntryPrintLines[1].SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9102.11.2530", printBO.EntryPrintLines[1].SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.2540", printBO.EntryPrintLines[1].SecondaryLine3FormattedTariff);
			AssertEquals("Remaining secondary lines should be empty", "", printBO.EntryPrintLines[1].SecondaryLine4FormattedTariff);
			AssertEquals("Remaining secondary lines should be empty", "", printBO.EntryPrintLines[1].SecondaryLine5FormattedTariff);
			AssertEquals("Remaining secondary lines should be empty", "", printBO.EntryPrintLines[1].SecondaryLine6FormattedTariff);
			AssertEquals("Remaining secondary lines should be empty", "", printBO.EntryPrintLines[1].SecondaryLine7FormattedTariff);

			AssertEquals("Parent 3 Tariff number", "9102.11.4510", printBO.EntryPrintLines[2].FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.4520", printBO.EntryPrintLines[2].SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9102.11.4530", printBO.EntryPrintLines[2].SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.4540", printBO.EntryPrintLines[2].SecondaryLine3FormattedTariff);
			AssertEquals("Remaining secondary lines should be empty", "", printBO.EntryPrintLines[2].SecondaryLine4FormattedTariff);
			AssertEquals("Remaining secondary lines should be empty", "", printBO.EntryPrintLines[2].SecondaryLine5FormattedTariff);
			AssertEquals("Remaining secondary lines should be empty", "", printBO.EntryPrintLines[2].SecondaryLine6FormattedTariff);
			AssertEquals("Remaining secondary lines should be empty", "", printBO.EntryPrintLines[2].SecondaryLine7FormattedTariff);
		}

		[TestDate(2009, 6, 1)]
		public void TestTIBBondChgForExceptionTariffsCalculatedFromMessage()
		{
			CreateTIBWithExemptTariffsDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               30501                " +
"10A888891-01319900091-013199000                 8         XJ5 7000795623891  IL1" +
"20                         403901040109B00151248            001  040109I317     " +
"22            00125896323                         00000001PC                    " +
"30                                  0               2041309             AA      " +
"40001GB00000080000000000095                    0000000189                       " +
"50 98130050                                                         GB040109Y   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"708528723600           000000001000NO                                           " +
"OI        N/HI,PRJCT W/CTHD/TV/VDEO                                             " +
"FC0104 001                 TEST                          2552                   " +
"FC02000000000010                                                                " +
"40002GB00000004640000000005                    0000000011                       " +
"50 98130050                                                         GB040109Y   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"707318154000           000000005000KG                                           " +
"90                      0                                  00000008464          " +
"Y  8888XJ5EI00018";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Declaration.ActiveEntryHeaders.EntrySummaryEntry, outMsg, inMsg, null);

			AssertEquals("TIB Duty", 312m, printBO.TIBTotalDuty);
			AssertEquals("TIB Charges", 25m, printBO.TIBTotalCharges);
			AssertEquals("TIB Bond CHG for exception tariffs should only be 110% of est duties and charges", 370.70m, printBO.TIBBondChg);
			AssertEquals("Relationship should be on invoice", "Y", printBO.EntryPrintLines[0].InvoiceDetails.TransactionsRelatedIndicator);
			AssertEquals("Relationship should not be on lines", "", printBO.EntryPrintLines[0].TransRelatedInd);
			AssertEquals("Relationship should not be on lines", "", printBO.EntryPrintLines[1].TransRelatedInd);
		}

		public void TestMultipleRelationshipsFromMessage()
		{
			CreateTIBWithExemptTariffsDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               30501                " +
"10A888891-01319900091-013199000                 8         XJ5 7000795623891  IL1" +
"20                         403901040109B00151248            001  040109I317     " +
"22            00125896323                         00000001PC                    " +
"30                                  0               2041309             AA      " +
"40001GB00000080000000000095                    0000000189                       " +
"50 98130050                                                         GB040109N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"708528723600           000000001000NO                                           " +
"OI        N/HI,PRJCT W/CTHD/TV/VDEO                                             " +
"FC0104 001                 TEST                          2552                   " +
"FC02000000000010                                                                " +
"40002GB00000004640000000005                    0000000011                       " +
"50 98130050                                                         GB040109Y   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"707318154000           000000005000KG                                           " +
"90                      0                                  00000008464          " +
"Y  8888XJ5EI00018";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Relationship should not print against the invoice", "", printBO.EntryPrintLines[0].InvoiceDetails.TransactionsRelatedIndicator);
			AssertEquals("Relationship should be on line1 not invoice", "N", printBO.EntryPrintLines[0].TransRelatedInd);
			AssertEquals("Relationship should be on line2 not invoice", "Y", printBO.EntryPrintLines[1].TransRelatedInd);
		}

		[TestDate(2009, 04, 08)]
		public void TestWatchEntryPrintedFromMessageAndEntry()
		{
			CreateWatchDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               217                  10A2818            91-013199000                 0         GAZ 0000078801        20                         402818033109B00001092            650  033109         22            08109384852                         00000000                      30                                  0                                   QF      40001TH00000018520000000000                                                     50 9802008068                                                       TH033109N   51                                                                              60                                                                              62          49900001335                                                         709102111010 0000029688000000100000NO                               0000003406  809802008068                                                        0000001010  819102111020 0000010603000000100000NO                               0000001609  819802008068                                                                    819102111030 0000012705000000100000NO                               0000001345  819802008068                                                        0000000204  819102111040 0000000729000000100000NO                                           8949900000002500                                                                9000000053725           0                       0000000250000000009426          Y      GAZEI00018000000053725";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);

			AssertEquals("Parent 1 Tariff number", "9802.00.8068", printBO.EntryPrintLines[0].FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", printBO.EntryPrintLines[0].SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.8068", printBO.EntryPrintLines[0].SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", printBO.EntryPrintLines[0].SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.8068", printBO.EntryPrintLines[0].SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", printBO.EntryPrintLines[0].SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.8068", printBO.EntryPrintLines[0].SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", printBO.EntryPrintLines[0].SecondaryLine7FormattedTariff);

			EntrySummary7501Line entryLine = printBO.EntryPrintLines[0];
			AssertEquals("Entry should have pro-rated summary", true, entryLine.ProRatedCalculation);
			AssertEquals("Pro-rated summary line 1", "9802.00.8068 (Free) 3066 / 9426 (Total Value) = 32.527%", entryLine.ProRatedLine1);
			AssertEquals("Pro-rated summary line 2", "32.527% x $796.25 (Total Duty Column 34) = $259.00", entryLine.ProRatedLine2);
			AssertEquals("Pro-rated summary line 3", "$796.25 - $259.00 = $537.25 (Total Duty Due)", entryLine.ProRatedLine3);

			AssertEquals("Box 37 Total Duty should remain as actual Duty", 537.25m, printBO.TotalDutyAmt);

			AssertEquals("Secondary Line 1 Actual Duty", 296.88m, entry.MergedLines[0].ChildLines[0].DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, printBO.EntryPrintLines[0].SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 3 Actual Duty", 106.03m, entry.MergedLines[0].ChildLines[2].DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, printBO.EntryPrintLines[0].SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 5 Actual Duty", 127.05m, entry.MergedLines[0].ChildLines[4].DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.30m, printBO.EntryPrintLines[0].SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 7 Actual Duty", 7.29m, entry.MergedLines[0].ChildLines[6].DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, printBO.EntryPrintLines[0].SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine7DutyPercentAsString);
		}

		[TestDate(2009, 04, 08)]
		public void TestTotalDutyForWarehouseEntryType21()
		{
			CreateWatchDeclaration();
			Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			entry.CreateDocPrintingDetails(message.PK);
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               217                  10A2818            91-013199000                 0         GAZ 0000078821        20                         402818033109B00001092            650  033109         22            08109384852                         00000000                      30                                  0                                   QF      40001TH00000018520000000000                                                     50 9802008068                                                       TH033109N   51                                                                              60                                                                              62          49900001335                                                         709102111010 0000029688000000100000NO                               0000003406  809802008068                                                        0000001010  819102111020 0000010603000000100000NO                               0000001609  819802008068                                                                    819102111030 0000012705000000100000NO                               0000001345  819802008068                                                        0000000204  819102111040 0000000729000000100000NO                                           8949900000002500                                                                9000000053725           0                       0000000250000000009426          Y      GAZEI00018000000053725";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);
			EntrySummary7501Line entryLine = printBO.EntryPrintLines[0];

			//Duty should still print on lines
			AssertEquals("Secondary Line 1 Actual Duty", 296.88m, entry.MergedLines[0].ChildLines[0].DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, printBO.EntryPrintLines[0].SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 3 Actual Duty", 106.03m, entry.MergedLines[0].ChildLines[2].DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, printBO.EntryPrintLines[0].SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 5 Actual Duty", 127.05m, entry.MergedLines[0].ChildLines[4].DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.30m, printBO.EntryPrintLines[0].SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 7 Actual Duty", 7.29m, entry.MergedLines[0].ChildLines[6].DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, printBO.EntryPrintLines[0].SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine7DutyPercentAsString);

			AssertEquals("Box 37 Total Duty should be 0 for Warehouse entry", 0m, printBO.TotalDutyAmt);
			AssertEquals("Box 40 Total should not include duty for Warehouse entry", 0m, printBO.Block40Total);
		}

		[TestDate(2009, 04, 08)]
		public void TestWatchEntryPrintedFromMessageAndSnapshot()
		{
			CreateWatchDeclaration();
			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               217                  10A2818            91-013199000                 0         GAZ 0000078801        20                         402818033109B00001092            650  033109         22            08109384852                         00000000                      30                                  0                                   QF      40001TH00000018520000000000                                                     50 9802008068                                                       TH033109N   51                                                                              60                                                                              62          49900001335                                                         709102111010 0000029688000000100000NO                               0000003406  809802008068                                                        0000001010  819102111020 0000010603000000100000NO                               0000001609  819802008068                                                                    819102111030 0000012705000000100000NO                               0000001345  819802008068                                                        0000000204  819102111040 0000000729000000100000NO                                           8949900000002500                                                                9000000053725           0                       0000000250000000009426          Y      GAZEI00018000000053725";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);

			AssertEquals("Parent 1 Tariff number", "9802.00.8068", printBO.EntryPrintLines[0].FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", printBO.EntryPrintLines[0].SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.8068", printBO.EntryPrintLines[0].SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", printBO.EntryPrintLines[0].SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.8068", printBO.EntryPrintLines[0].SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", printBO.EntryPrintLines[0].SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.8068", printBO.EntryPrintLines[0].SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", printBO.EntryPrintLines[0].SecondaryLine7FormattedTariff);

			EntrySummary7501Line entryLine = printBO.EntryPrintLines[0];
			AssertEquals("Entry should have pro-rated summary", true, entryLine.ProRatedCalculation);
			AssertEquals("Pro-rated summary line 1", "9802.00.8068 (Free) 3066 / 9426 (Total Value) = 32.527%", entryLine.ProRatedLine1);
			AssertEquals("Pro-rated summary line 2", "32.527% x $796.25 (Total Duty Column 34) = $259.00", entryLine.ProRatedLine2);
			AssertEquals("Pro-rated summary line 3", "$796.25 - $259.00 = $537.25 (Total Duty Due)", entryLine.ProRatedLine3);

			AssertEquals("Box 37 Total Duty should remain as actual Duty", 537.25m, printBO.TotalDutyAmt);

			CusEntryLine secondaryTariffLine1 = null;
			CusEntryLine secondaryTariffLine3 = null;
			CusEntryLine secondaryTariffLine5 = null;
			CusEntryLine secondaryTariffLine7 = null;
			foreach (CusEntryLine cusLine in entry.EntryLines)
			{
				secondaryTariffLine1 = cusLine.ChildLines[0];
				secondaryTariffLine3 = cusLine.ChildLines[2];
				secondaryTariffLine5 = cusLine.ChildLines[4];
				secondaryTariffLine7 = cusLine.ChildLines[6];
			}

			AssertEquals("Secondary Line 1 Actual Duty", 296.88m, secondaryTariffLine1.DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, printBO.EntryPrintLines[0].SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 3 Actual Duty", 106.03m, secondaryTariffLine3.DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, printBO.EntryPrintLines[0].SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 5 Actual Duty", 127.05m, secondaryTariffLine5.DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.30m, printBO.EntryPrintLines[0].SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 7 Actual Duty", 7.29m, secondaryTariffLine7.DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, printBO.EntryPrintLines[0].SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", printBO.EntryPrintLines[0].SecondaryLine7DutyPercentAsString);
		}

		[TestDate(2009, 12, 28)]
		public void TestWatchRepairsAndAssemblyPrintedFromMessage()
		{
			CreateWatchRepairsDeclaration();
			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               50285                10A8888                                         8         XJ5 7002884602891  WA 20     JULIA OLDENDORF     112704      B00153440            345  040710W138     30                                  0               2050710             YMLU    40001KR00000018520000000010                    000000000355976                  50 9802008068                                                       SG040710N   51                                                                              60                                        KRJUHCOR3325KIM                       62          50100001179                                                         62          49900001335                                                         709102111010 0000029688000000100000NO                               0000003406  809802008068                                                        0000001010  819102111020 0000010603000000100000NO                               0000001609  819802008068                                                                    819102111030 0000012705000000100000NO                               0000001345  819802008068                                                        0000000204  819102111040 0000000729000000100000NO                                           40002KR00000018520000000010                    000000000355976                  50 9802004040                                                       SG040710N   51                                                                              60                                        KRJUHCOR3325KIM                       62          50100001179                                                         709102111010 0000028770000000100000NO                               0000003406  809802004040                                                        0000001010  819102111020 0000013591000000100000NO                               0000001609  819802004040                                                                    819102111030 0000011361000000100000NO                               0000001345  819802004040                                                        0000000204  819102111040           000000100000NO                                           40003XO00000018520000000010                    000000000355976                  50 9802004040                                                       CA040710NCA 51                                                                              60                                        XOJOHMAT130BRA                        62          50100001179                                                         709102111010           000000100000NO                               0000003406CA809802004040                                                        0000001010CA819102111020           000000100000NO                               0000001609CA819802004040                                                                  CA819102111030           000000100000NO                               0000001345CA819802004040                                                        0000000204CA819102111040           000000100000NO                                         CA895010000000353749900000002500                                                  9000000107447           0                       0000000603700000028278          Y  8888XJ5EI00042000000107447";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("Entry print lines count", 3, printBO.EntryPrintLines.Count);

			EntrySummary7501Line entryLine1 = printBO.EntryPrintLines[0];
			AssertEquals("Line 1 (Parent) Tariff number", "9802.00.8068", entryLine1.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", entryLine1.SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.8068", entryLine1.SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", entryLine1.SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.8068", entryLine1.SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", entryLine1.SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.8068", entryLine1.SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", entryLine1.SecondaryLine7FormattedTariff);

			AssertEquals("Entry should have pro-rated summary", true, entryLine1.ProRatedCalculation);
			AssertEquals("Pro-rated summary line 1", "9802.00.8068 (Free) 3066 / 9426 (Total Value) = 32.527%", entryLine1.ProRatedLine1);
			AssertEquals("Pro-rated summary line 2", "32.527% x $796.25 (Total Duty Column 34) = $259.00", entryLine1.ProRatedLine2);
			AssertEquals("Pro-rated summary line 3", "$796.25 - $259.00 = $537.25 (Total Duty Due)", entryLine1.ProRatedLine3);

			AssertEquals("Line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("Line 1 Duty Percentage as String", "Free", entryLine1.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, entryLine1.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", entryLine1.SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 2 Duty to print on 7501", 0m, entryLine1.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "Free", entryLine1.SecondaryLine2DutyPercentAsString);

			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, entryLine1.SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine1.SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 4 Duty to print on 7501", 0m, entryLine1.SecondaryLine4DutyAmount);
			AssertEquals("Secondary Line 4 Duty Rate to print on 7501", "Free", entryLine1.SecondaryLine4DutyPercentAsString);

			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.30m, entryLine1.SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine1.SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 6 Duty to print on 7501", 0m, entryLine1.SecondaryLine6DutyAmount);
			AssertEquals("Secondary Line 6 Duty Rate to print on 7501", "Free", entryLine1.SecondaryLine6DutyPercentAsString);

			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, entryLine1.SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine1.SecondaryLine7DutyPercentAsString);

			//Entry Line 2
			EntrySummary7501Line entryLine2 = printBO.EntryPrintLines[1];
			AssertEquals("Line 2 (Parent) Tariff number", "9802.00.4040", entryLine2.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", entryLine2.SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.4040", entryLine2.SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", entryLine2.SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.4040", entryLine2.SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", entryLine2.SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.4040", entryLine2.SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", entryLine2.SecondaryLine7FormattedTariff);

			AssertEquals("Entry should have ad-valorem calculation", true, entryLine2.AdValoremConversionCalculation);
			AssertEquals("AV Watches", "1000 x $0.44 NO", entryLine2.AVWatches);
			AssertEquals("AVWatchesDuty", 440m, entryLine2.AVWatchesDuty);
			AssertEquals("AV Cases", "$2619 x 6%", entryLine2.AVCases);
			AssertEquals("AVCasesDuty", 157.14m, entryLine2.AVCasesDuty);
			AssertEquals("AV Bracelets", "$1345 x 14%", entryLine2.AVBracelets);
			AssertEquals("AVBraceletsDuty", 188.30m, entryLine2.AVBraceletsDuty);
			AssertEquals("AVBatteries", "$204 x 5.3%", entryLine2.AVBatteries);
			AssertEquals("AVBatteriesDuty", 10.81m, entryLine2.AVBatteriesDuty);
			AssertEquals("AVTotalDuty", 796.25m, entryLine2.AVTotalDuty);
			AssertEquals("AV Conversion line", "$796.25/$9426.00 (Total Entered Value) = 8.447%", entryLine2.AVLine2);

			AssertEquals("Line 2 Duty", 0m, entryLine2.DutyAmount);
			AssertEquals("Line 2 Duty Percentage as String", "Free", entryLine2.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 287.70m, entryLine2.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "8.447%", entryLine2.SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 2 Duty to print on 7501", 0m, entryLine2.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "Free", entryLine2.SecondaryLine2DutyPercentAsString);

			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 135.91m, entryLine2.SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "8.447%", entryLine2.SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 4 Duty to print on 7501", 0m, entryLine2.SecondaryLine4DutyAmount);
			AssertEquals("Secondary Line 4 Duty Rate to print on 7501", "Free", entryLine2.SecondaryLine4DutyPercentAsString);

			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 113.61m, entryLine2.SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "8.447%", entryLine2.SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 6 Duty to print on 7501", 0m, entryLine2.SecondaryLine6DutyAmount);
			AssertEquals("Secondary Line 6 Duty Rate to print on 7501", "Free", entryLine2.SecondaryLine6DutyPercentAsString);

			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 0m, entryLine2.SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "8.447%", entryLine2.SecondaryLine7DutyPercentAsString);

			//Entry Line 3
			EntrySummary7501Line entryLine3 = printBO.EntryPrintLines[2];
			AssertEquals("Line 3 (Parent) Tariff number", "9802.00.4040", entryLine3.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", entryLine3.SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.4040", entryLine3.SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", entryLine3.SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.4040", entryLine3.SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", entryLine3.SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.4040", entryLine3.SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", entryLine3.SecondaryLine7FormattedTariff);
			AssertEquals("Line 3 Duty", 0m, entryLine3.DutyAmount);
			AssertEquals("Line 3 Duty Percentage as String", "Free", entryLine3.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 0m, entryLine3.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 2 Duty to print on 7501", 0m, entryLine3.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "Free", entryLine3.SecondaryLine2DutyPercentAsString);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 0m, entryLine3.SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 4 Duty to print on 7501", 0m, entryLine3.SecondaryLine4DutyAmount);
			AssertEquals("Secondary Line 4 Duty Rate to print on 7501", "Free", entryLine3.SecondaryLine4DutyPercentAsString);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 0m, entryLine3.SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 6 Duty to print on 7501", 0m, entryLine3.SecondaryLine6DutyAmount);
			AssertEquals("Secondary Line 6 Duty Rate to print on 7501", "Free", entryLine3.SecondaryLine6DutyPercentAsString);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 0m, entryLine3.SecondaryLine7DutyAmount);

			AssertEquals("Box 37 Total Duty", 1074.47m, printBO.TotalDutyAmt);
		}

		public void TestUltimateState()
		{
			CreateRateEntryForMessageMatching();
			entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               28                   10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732  CA 20     HAKUBA MARU         111601070108B00001029            593  070108N598     22            OB9394043938SMTAOS005870            00000200CT         MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.New<MQEDIMessage>();
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);

			EntryMessageENS7501Print entryMessageENS7501Print = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("EffectiveUltimateConsigneeCustomsRegNo", USConstants.Same, entryMessageENS7501Print.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("Ultimate Consignee State should show destination state when Consignee & importer is the same", "CA", entryMessageENS7501Print.EffectiveUltimateConsigneeState);
			AssertEquals("State of ultimate destination should only print when different from Consignee", "", entryMessageENS7501Print.UltimateState);

			outMsg.EM_MessageText = "B01    GAZEI                                               28                   10A888848-0920709WM50-0920709WM                 8         GAZ 0000019203732  CA 20     HAKUBA MARU         111601070108B00001029            593  070108N598     22            OB9394043938SMTAOS005870            00000200CT         MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";
			entryMessageENS7501Print = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertNotEquals("EffectiveUltimateConsignee is now different from IOR", USConstants.Same, entryMessageENS7501Print.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("EffectiveUltimateConsignee", "50-0920709WM", entryMessageENS7501Print.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("Ultimate State", "CA", entryMessageENS7501Print.UltimateState);
		}

		public void TestReconStatement()
		{
			CreateRateEntryForMessageMatching();
			entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               28                   10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732  CA 20     HAKUBA MARU         111601070108B00001029            593  070108N598 001 22            OB9394043938SMTAOS005870            00000200CT         MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.New<MQEDIMessage>();
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);

			EntryMessageENS7501Print entryMessageENS7501Print = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("Recon Statment", "Flagged For Recon - 001 - Value Recon.", entryMessageENS7501Print.ReconStatement);
		}

		public void TestReconStatementForNAFTA()
		{
			CreateRateEntryForMessageMatching();
			entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               28                   10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732  CA 20     HAKUBA MARU         111601070108B00001029            593  070108N5981    22            OB9394043938SMTAOS005870            00000200CT         MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.New<MQEDIMessage>();
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);

			EntryMessageENS7501Print entryMessageENS7501Print = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("Recon Statment", "Flagged For Recon - FTA", entryMessageENS7501Print.ReconStatement.Trim());
		}

		public void TestReconStatementIfEverBothArePresent()
		{
			CreateRateEntryForMessageMatching();
			entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B01    GAZEI                                               28                   10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732  CA 20     HAKUBA MARU         111601070108B00001029            593  070108N5981003 22            OB9394043938SMTAOS005870            00000200CT         MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.New<MQEDIMessage>();
			inMsg.EM_ApplicationCode = "USI";
			inMsg.EM_MessageType = "ER";
			inMsg.EM_Status = "";
			inMsg.EM_ReceiveTransmit = "RCV";
			inMsg.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(1);

			EntryMessageENS7501Print entryMessageENS7501Print = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("Recon Statment", "Flagged For Recon - FTA   Flagged For Recon - 003 - 9802 Recon.", entryMessageENS7501Print.ReconStatement.Trim());
		}

		public void TestImporterWhenMultipleOrgsWithSameEIN()
		{
			CreateMergedDeclaration();
			CusEntryHeader ensEntry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			EDIMessage outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outMsg.EM_LinkUniqueID = ensEntry.PK;
			outMsg.EM_MessageText = "B01    GAZEI                                               28                   10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = ensEntry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(ensEntry, outMsg, inMsg, null);
			AssertEquals("Importer No", "48-0920709WM", printBO.ImporterOfRecordCustomsRegNo);
			AssertEquals("Importer Name should find the correct importer when multiple organisations use the same EIN", "Test Import Company 1", printBO.ImporterCompanyName);

			OrgHeader importer4 = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "76-031001700");
			importer4.OH_FullName = "SAIPEM AMERICA INC.";
			Declaration.IOROrgPK = importer4.PK;
			Factory.Save();

			printBO = new EntryMessageENS7501Print(ensEntry, outMsg, inMsg, null);
			AssertEquals("Importer No", "48-0920709WM", printBO.ImporterOfRecordCustomsRegNo);
			AssertEquals("Changing the Importer on the entry when msg EIN returns multiple organisations should output special message", "***        Organization N&A cannot be determined        ***", printBO.ImporterCompanyName);
			AssertEquals("output special message", "*** multiple organizations with the same Importer No. ***", printBO.ImporterAddressLine1);
		}

		public void TestConsigneeWhenMultipleOrgsWithSameEIN()
		{
			CreateMergedDeclaration();
			CusEntryHeader ensEntry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Factory.Save();

			EDIMessage outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outMsg.EM_LinkUniqueID = ensEntry.PK;
			outMsg.EM_MessageText = "B01    GAZEI                                               28                   10A888848-0920709WM12-3456789                   8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                    A5708810010000178133CNJINME22JIN                   01131 062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60          C750001002                    CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = ensEntry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70022120B00152541PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(ensEntry, outMsg, inMsg, null);
			AssertEquals("Consignee No", "12-3456789", printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("Consignee Name should find the correct organisation when multiple organisations use the same EIN", "Ultimate Consignee", printBO.EffectiveUltimateConsigneeCompanyName);

			OrgHeader importer4 = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "76-031001700");
			importer4.OH_FullName = "SAIPEM AMERICA INC.";
			Declaration.JE_OH_Importer = importer4.PK;
			Declaration.JE_OA_ConsigneeAddress = importer4.MainAddress.PK;
			Factory.Save();

			printBO = new EntryMessageENS7501Print(ensEntry, outMsg, inMsg, null);
			AssertEquals("Consignee No", "12-3456789", printBO.EffectiveUltimateConsigneeCustomsRegNo);
			AssertEquals("Changing the Consignee on the entry when msg EIN returns multiple organisations should output special message", "***          Organization N&A cannot be determined           ***", printBO.EffectiveUltimateConsigneeCompanyName);
			AssertEquals("output special message", "*** multiple organizations with the same Consignee No. ***", printBO.EffectiveUltimateConsigneeAddressLine1);
		}

		[TestDate(2009, 12, 28)]
		public void Test99TariffAndSecondaryWithDiffRatesPrintedFromMessage()
		{
			Create99Declaration();
			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               50276                10A888891-01319900091-013199000                 8         XJ5 7002881202891  WA 20     23                  112704      B00153437            345  050410W138     22            OB113948    WICC09288374            00000014BL         YMLUUSNW   30                                  0               1                   YMLU    40001NZ00000100000013000000                    000000005055976                  50 99040237  0000088000                                             NZ041410N   51                                                                              60                                        KRJUHCOR3325KIM                       62          50100001250                                                         62          49900002100                                                         700201308010 0000264000000000150000KG                                           895010000000125049900000002500                                                  9000000352000           0                       0000000375000000010000          Y  8888XJ5EI00013000000352000";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);

			EntrySummary7501Line entryLine = printBO.EntryPrintLines[0];
			AssertEquals("Parent 1 Tariff number", "9904.02.37", entryLine.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "0201.30.8010", entryLine.SecondaryLine1FormattedTariff);

			AssertEquals("Box 37 Total Duty", 3520m, printBO.TotalDutyAmt);
			AssertEquals("Line 1 Duty", 880m, entryLine.DutyAmount);
			AssertEquals("Line 1 Duty Percentage as String", "8.8%", entryLine.DutyPercentAsString);
			AssertEquals("Secondary Line 1 Duty", 2640m, entryLine.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Duty Rate to print on 7501", "26.4%", entryLine.SecondaryLine1DutyPercentAsString);
		}

		[TestDate(2009, 12, 28)]
		public void TestNonWatchRepairsPrintedFromMessage()
		{
			CreateNonWatchRepairsDeclaration();
			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               50278                10A888891-01319900091-013199000                 8         XJ5 7002883801891  WA 20     23                  112704      B00153439            345  051110W138     22            OB5534E4    WICC09288374            00000001PK         YMLUUSNW   30                                  0               2052110             YMLU    40001KR00000049990000000150                    000000005055976                  50 9802004040                                                       NI042110N   51                                                                              60                                        KRJUHCOR3325KIM                       62          50100001250                                                         709207100065 0000027000000000010000NO                               0000005000  8950100000001250                                                                9000000027000           0                       0000000125000000009999          Y  8888XJ5EI00012000000027000";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);

			EntrySummary7501Line entryLine = printBO.EntryPrintLines[0];
			AssertEquals("Parent 1 Tariff number", "9802.00.4040", entryLine.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9207.10.0065", entryLine.SecondaryLine1FormattedTariff);

			AssertEquals("Box 37 Total Duty", 270m, printBO.TotalDutyAmt);
			AssertEquals("Line 1 Duty", 0m, entryLine.DutyAmount);
			AssertEquals("Line 1 Duty Percentage as String", "Free", entryLine.DutyPercentAsString);
			AssertEquals("Secondary Line 1 Duty", 270m, entryLine.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Duty Rate to print on 7501", "5.4%", entryLine.SecondaryLine1DutyPercentAsString);
		}

		public void TestAdditionalLineSectionPrintingFlags_ADDNo()
		{
			declaration = null;
			entry = null;

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			var entryLine1 = Entry.MergedLines.AddNew();
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			Entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(outMsg);
			outMsg.EM_MessageText = "B018888123EI                                               50215                " +
"10A530191-01319900091-013199000                 8         123 7002207603891  CA " +
"20     MSC ORNELLA         105301042110B00153415            22N  042110V136     " +
"22            MSCUG1026256                        00000300BX         MSCU       " +
"30                                  0               2050310             MSCU    " +
"350000000000000000000000000000000000000000027301                                " +
"40001GB00000003280000000041                    000000002947507                  " +
"50 15099020000000000205000000004100KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000041                                                         " +
"62          49900000069                                                         " +
"40002GB00000003640000000180                    000000003247507                  " +
"50 1902192030          000000018000KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000046                                                         " +
"62          49900000076                                                         " +
"40003GB00000013240000000247                    000000011547507                  " +
"50 15091020000000001235000000024700KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000166                                                         " +
"62          49900000278                                                         " +
"40004GB00000003490000000164                    000000003047507                  " +
"50 21032040200000004048000000016400KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000044                                                         " +
"62          49900000073                                                         " +
"40005GB00000001960000000024                    000000001747507                  " +
"50 21039090910000001254000000002400KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000025                                                         " +
"62          49900000041                                                         " +
"40006GB00000224740000012351                    000000195347507                  " +
"50 1902192030          000001235100KG                               IT041510N   " +
"51                                                                              " +
"600000019777C475819005                    GBBOOMED295LON            00088     0 " +
"62          50100002809                                                         " +
"62          49900004720                                                         " +
"40007GB00000085500000004077                    000000074347507                  " +
"50 1902192030          000000407700KG                               IT041510N   " +
"51                                                                              " +
"600000007524C475819005                    GBBOOMED295LON            00088     0 " +
"62          50100001069                                                         " +
"62          49900001796                                                         " +
"40008GB00000009420000000240                    000000008247507                  " +
"50 1902112030          000000024000KG                               IT041510N   " +
"51                                                                              " +
"60                                        GBBOOMED295LON                        " +
"62          50100000118                                                         " +
"62          49900000198                                                         " +
"895010000000431849900000007251                                                  " +
"9000000006742           0 00000027301           0000001156900000034527          " +
"Y  8888123EI00055000000006742";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);
			inMsg.EM_MessageText = @"B018888123EI                                               50215                EBA AND B REC DP/FLR/OFFICE CONFLICT                                            EBTRANSACTION DATA REJECTED                                                     Y  8888123EI00055000000006742";

			Entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings;
			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);

			AssertEquals("Summary Fee Desc 1", "013 CVD", printBO.SummaryFeeDesc1);
			AssertEquals("SummaryFee1 (Total CVD)", 273.01m, printBO.SummaryFee1);
			AssertEquals("Summary Fee 2", "499 499 Desc from DB", printBO.SummaryFeeDesc2);
			AssertEquals("SummaryFee2 (Total MPF)", 72.51m, printBO.SummaryFee2);
			AssertEquals("Summary Fee 3", "501 501 Desc from DB", printBO.SummaryFeeDesc3);
			AssertEquals("SummaryFee3 (Total HMF)", 43.18m, printBO.SummaryFee3);
			AssertEquals("Total Other Fees should inlcude CVD", 388.70m, printBO.TotalOtherFees);
			AssertEquals("Total Other", 388.70m, printBO.TotalOther);

			AssertEquals("EntryHasADDCVDLines", true, printBO.EntryHasADDCVDLines);
			AssertEquals("EntryHasSecondaryTariffLines", false, printBO.EntryHasSecondaryTariffLines);
			AssertEquals("EntryHasAdditionalTariffLines", false, printBO.EntryHasAdditionalTariffLines);
			AssertEquals("EntryHasProRatedCalculation", false, printBO.EntryHasProRatedCalculation);
			AssertEquals("EntryHasAdValoremConversionCalculation ", false, printBO.EntryHasAdValoremConversionCalculation);
		}

		[TestDate(2010, 09, 09)]
		public void TestAdditionalLineSectionPrintingFlags_ProRated_AdValorem()
		{
			CreateWatchRepairsDeclaration();
			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               50285                10A8888                                         8         XJ5 7002884602891  WA 20     JULIA OLDENDORF     112704      B00153440            345  040710W138     30                                  0               2050710             YMLU    40001KR00000018520000000010                    000000000355976                  50 9802008068                                                       SG040710N   51                                                                              60                                        KRJUHCOR3325KIM                       62          50100001179                                                         62          49900001335                                                         709102111010 0000029688000000100000NO                               0000003406  809802008068                                                        0000001010  819102111020 0000010603000000100000NO                               0000001609  819802008068                                                                    819102111030 0000012705000000100000NO                               0000001345  819802008068                                                        0000000204  819102111040 0000000729000000100000NO                                           40002KR00000018520000000010                    000000000355976                  50 9802004040                                                       SG040710N   51                                                                              60                                        KRJUHCOR3325KIM                       62          50100001179                                                         709102111010 0000028770000000100000NO                               0000003406  809802004040                                                        0000001010  819102111020 0000013591000000100000NO                               0000001609  819802004040                                                                    819102111030 0000011361000000100000NO                               0000001345  819802004040                                                        0000000204  819102111040           000000100000NO                                           40003XO00000018520000000010                    000000000355976                  50 9802004040                                                       CA040710NCA 51                                                                              60                                        XOJOHMAT130BRA                        62          50100001179                                                         709102111010           000000100000NO                               0000003406CA809802004040                                                        0000001010CA819102111020           000000100000NO                               0000001609CA819802004040                                                                  CA819102111030           000000100000NO                               0000001345CA819802004040                                                        0000000204CA819102111040           000000100000NO                                         CA895010000000353749900000002500                                                  9000000107447           0                       0000000603700000028278          Y  8888XJ5EI00042000000107447";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("Entry print lines count", 3, printBO.EntryPrintLines.Count);
			AssertEquals("EntryHasADDCVDLines", false, printBO.EntryHasADDCVDLines);
			AssertEquals("EntryHasSecondaryTariffLines", true, printBO.EntryHasSecondaryTariffLines);
			AssertEquals("EntryHasAdditionalTariffLines", true, printBO.EntryHasAdditionalTariffLines);
			AssertEquals("EntryHasProRatedCalculation", true, printBO.EntryHasProRatedCalculation);
			AssertEquals("EntryHasAdValoremConversionCalculation ", true, printBO.EntryHasAdValoremConversionCalculation);

			EntrySummary7501Line entryLine1 = printBO.EntryPrintLines[0];
			AssertEquals("Line 1 (Parent) Tariff number", "9802.00.8068", entryLine1.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", entryLine1.SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.8068", entryLine1.SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", entryLine1.SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.8068", entryLine1.SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", entryLine1.SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.8068", entryLine1.SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", entryLine1.SecondaryLine7FormattedTariff);

			AssertEquals("Entry should have pro-rated summary", true, entryLine1.ProRatedCalculation);
			AssertEquals("Pro-rated summary line 1", "9802.00.8068 (Free) 3066 / 9426 (Total Value) = 32.527%", entryLine1.ProRatedLine1);
			AssertEquals("Pro-rated summary line 2", "32.527% x $796.25 (Total Duty Column 34) = $259.00", entryLine1.ProRatedLine2);
			AssertEquals("Pro-rated summary line 3", "$796.25 - $259.00 = $537.25 (Total Duty Due)", entryLine1.ProRatedLine3);

			AssertEquals("Line 1 Duty", 0m, entryLine1.DutyAmount);
			AssertEquals("Line 1 Duty Percentage as String", "Free", entryLine1.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, entryLine1.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", entryLine1.SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 2 Duty to print on 7501", 0m, entryLine1.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "Free", entryLine1.SecondaryLine2DutyPercentAsString);

			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, entryLine1.SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine1.SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 4 Duty to print on 7501", 0m, entryLine1.SecondaryLine4DutyAmount);
			AssertEquals("Secondary Line 4 Duty Rate to print on 7501", "Free", entryLine1.SecondaryLine4DutyPercentAsString);

			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.30m, entryLine1.SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine1.SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 6 Duty to print on 7501", 0m, entryLine1.SecondaryLine6DutyAmount);
			AssertEquals("Secondary Line 6 Duty Rate to print on 7501", "Free", entryLine1.SecondaryLine6DutyPercentAsString);

			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, entryLine1.SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", entryLine1.SecondaryLine7DutyPercentAsString);

			//Entry Line 2
			EntrySummary7501Line entryLine2 = printBO.EntryPrintLines[1];
			AssertEquals("Line 2 (Parent) Tariff number", "9802.00.4040", entryLine2.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", entryLine2.SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.4040", entryLine2.SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", entryLine2.SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.4040", entryLine2.SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", entryLine2.SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.4040", entryLine2.SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", entryLine2.SecondaryLine7FormattedTariff);

			AssertEquals("Entry should have ad-valorem calculation", true, entryLine2.AdValoremConversionCalculation);
			AssertEquals("AV Watches", "1000 x $0.44 NO", entryLine2.AVWatches);
			AssertEquals("AVWatchesDuty", 440m, entryLine2.AVWatchesDuty);
			AssertEquals("AV Cases", "$2619 x 6%", entryLine2.AVCases);
			AssertEquals("AVCasesDuty", 157.14m, entryLine2.AVCasesDuty);
			AssertEquals("AV Bracelets", "$1345 x 14%", entryLine2.AVBracelets);
			AssertEquals("AVBraceletsDuty", 188.30m, entryLine2.AVBraceletsDuty);
			AssertEquals("AVBatteries", "$204 x 5.3%", entryLine2.AVBatteries);
			AssertEquals("AVBatteriesDuty", 10.81m, entryLine2.AVBatteriesDuty);
			AssertEquals("AVTotalDuty", 796.25m, entryLine2.AVTotalDuty);
			AssertEquals("AV Conversion line", "$796.25/$9426.00 (Total Entered Value) = 8.447%", entryLine2.AVLine2);

			AssertEquals("Line 2 Duty", 0m, entryLine2.DutyAmount);
			AssertEquals("Line 2 Duty Percentage as String", "Free", entryLine2.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 287.70m, entryLine2.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "8.447%", entryLine2.SecondaryLine1DutyPercentAsString);

			AssertEquals("Secondary Line 2 Duty to print on 7501", 0m, entryLine2.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "Free", entryLine2.SecondaryLine2DutyPercentAsString);

			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 135.91m, entryLine2.SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "8.447%", entryLine2.SecondaryLine3DutyPercentAsString);

			AssertEquals("Secondary Line 4 Duty to print on 7501", 0m, entryLine2.SecondaryLine4DutyAmount);
			AssertEquals("Secondary Line 4 Duty Rate to print on 7501", "Free", entryLine2.SecondaryLine4DutyPercentAsString);

			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 113.61m, entryLine2.SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "8.447%", entryLine2.SecondaryLine5DutyPercentAsString);

			AssertEquals("Secondary Line 6 Duty to print on 7501", 0m, entryLine2.SecondaryLine6DutyAmount);
			AssertEquals("Secondary Line 6 Duty Rate to print on 7501", "Free", entryLine2.SecondaryLine6DutyPercentAsString);

			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 0m, entryLine2.SecondaryLine7DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "8.447%", entryLine2.SecondaryLine7DutyPercentAsString);

			//Entry Line 3
			EntrySummary7501Line entryLine3 = printBO.EntryPrintLines[2];
			AssertEquals("Line 3 (Parent) Tariff number", "9802.00.4040", entryLine3.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "9102.11.1010", entryLine3.SecondaryLine1FormattedTariff);
			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.4040", entryLine3.SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine3FormattedTariff", "9102.11.1020", entryLine3.SecondaryLine3FormattedTariff);
			AssertEquals("SecondaryLine4FormattedTariff", "9802.00.4040", entryLine3.SecondaryLine4FormattedTariff);
			AssertEquals("SecondaryLine5FormattedTariff", "9102.11.1030", entryLine3.SecondaryLine5FormattedTariff);
			AssertEquals("SecondaryLine6FormattedTariff", "9802.00.4040", entryLine3.SecondaryLine6FormattedTariff);
			AssertEquals("SecondaryLine7FormattedTariff", "9102.11.1040", entryLine3.SecondaryLine7FormattedTariff);
			AssertEquals("Line 3 Duty", 0m, entryLine3.DutyAmount);
			AssertEquals("Line 3 Duty Percentage as String", "Free", entryLine3.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 0m, entryLine3.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 2 Duty to print on 7501", 0m, entryLine3.SecondaryLine2DutyAmount);
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "Free", entryLine3.SecondaryLine2DutyPercentAsString);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 0m, entryLine3.SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 4 Duty to print on 7501", 0m, entryLine3.SecondaryLine4DutyAmount);
			AssertEquals("Secondary Line 4 Duty Rate to print on 7501", "Free", entryLine3.SecondaryLine4DutyPercentAsString);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 0m, entryLine3.SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 6 Duty to print on 7501", 0m, entryLine3.SecondaryLine6DutyAmount);
			AssertEquals("Secondary Line 6 Duty Rate to print on 7501", "Free", entryLine3.SecondaryLine6DutyPercentAsString);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 0m, entryLine3.SecondaryLine7DutyAmount);

			AssertEquals("Box 37 Total Duty", 1074.47m, printBO.TotalDutyAmt);
		}

		public void TestDeclarantNameAndTitle()
		{
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(Declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, true);
			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(Declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, true);

			var broker = Factory.NewWithValidTestData<GlbStaff>();
			broker.GS_FullName = "Tim Brooke Broker";
			broker.GS_Code = "TBB";
			broker.GS_WorkPhone = "+1 2 11112222";
			broker.GS_Title = "Test Title";

			var brokerTestBranch = Factory.NewWithValidTestData<GlbBranch>();
			brokerTestBranch.GB_Address1 = "TB Addr1";
			brokerTestBranch.GB_Address2 = "TB Addr1";
			brokerTestBranch.GB_City = "New York";
			brokerTestBranch.GB_PostCode = "99999";

			broker.GS_GB_HomeBranch = brokerTestBranch.PK;

			Declaration.JE_GS_NKCusAgent = broker.GS_Code;
			AssertEquals("Pre-condition - By default AttorneyInFact should be true", true, Entry.IsAttorneyInFact);

			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outMsg.EM_MessageText = "B018888XJ5EI                                               44376                10A110191-01319900091-013199000                 8         XJ5 7002660001891  AK 20     ADMIRALENGRACHT     111101013110B00153049            123  013110C001 001 22            TESTAPLU                            00000010BX         APLU       30                                  0               1                   APLU    40001HN00000025450000000437                    000000005760204                  50 9802008068                                                       AU013110Y   51                  647                                                         60                                        AUABCEXP72ALE                         62          50100000626                                                         62          49900000518                                                         706203434030 0000068774000000008300DOZ000000043700KG                0000002465  40002US00000002190000000019                    000000000260204                  50 9801001010                      X                                AU013110Y   51                                                                              60                                        AUABCEXP72ALE                         62          50100000027                                                         894990000000250050100000000653                                                  9000000068774           0                       0000000315300000005229          Y  8888XJ5EI00018000000068774";
			outMsg.EM_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			AssertNotEquals("Pre-condition - Outgoing Message Branch not equal Declaration Branch", Declaration.Branch.GB_Code, outMsg.Branch.GB_Code);

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);

			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			AssertEquals("Declarant Name should default to declaration broker", ContactNameHelper.GetFormattedName(broker.GS_FullName, true), printBO.DeclarantName);
			AssertEquals("Declarant Title should show Atty in fact when registy is on", "ATTY-IN-FACT", printBO.DeclarantTitle);

			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(Declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("Declarant Title should show Broker Title, when registy is off", broker.GS_Title, printBO.DeclarantTitle);

			Declaration.JE_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Declarant Name is current user name", ContactNameHelper.GetFormattedName(Env.CurrentUser.FullName, true), printBO.DeclarantName);
			AssertEquals("Declarant Title is current user title", Env.CurrentUser.Title, printBO.DeclarantTitle);

			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(Declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			Declaration.JE_GS_NKCusAgent = "";
			AssertEquals(ZString.Empty, printBO.DeclarantName);
			AssertEquals(ZString.Empty, printBO.DeclarantTitle);
		}

		public void TestUniquePortOfLading()
		{
			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			Declaration.US_EnableENS = true;
			Declaration.US_EntryFilerCode = "XJ5";
			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "3305100000";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3305100001";
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3303001000";
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "9102111010";
			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "8211100000";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();

			entry.CreateDocPrintingDetails(message.PK);

			message.EM_MessageText = "B013910SV9EI                                               HYEDUSCMT_146618     " +
"10A390113-14792700013-147927000                 8040213   SV9 7003167701037  TX " +
"20     23                  103901040213B00160008            343FS040213A002     " +
"22            4SDF345234                          00000300PC         APLU       " +
"30                                  01              2041213             APLU    " +
"40001CH00000004000000000015                              52051                  " +
"50 3305100000                      X                                CH040213N  X" +
"60                                        CHHARWIN8PLA                          " +
"62          50100000050                                                         " +
"62          49900000139                                                         " +
"40002CH00000002000000000010                              52030                  " +
"50 3305100000                      X                                CH040213N  V" +
"60                                        CHHARWIN8PLA                          " +
"40003CH00000002000000000010                              52030                  " +
"50 3303001000          000000000100L                                CH040213N  V" +
"60                                        CHHARWIN8PLA                          " +
"40004KR00000004000000000050                              52000                  " +
"50 9102111010          000000001200NO                               KR040213NKR " +
"60                                        CHHARWIN8PLA                          " +
"62          50100000050                                                         " +
"709102111020           000000002000NO                                         KR" +
"809102111030                       NO                                         KR" +
"819102111040                       NO                                         KR" +
"40005CH00000002000000000012                              52325                  " +
"50 82111000000000001225000000001500PCS                              CH040213N   " +
"60                                        CHHARWIN8PLA                          " +
"62          50100000025                                                         " +
"62          49900000069                                                         " +
"708211929045           000000001300NO                                           " +
"895010000000012549900000002500                                                  " +
"9000000001225           2                       0000000262500000001000          " +
"Y  3910SV9EI00030000000001225";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = Entry.PK;
			inMsg.EM_MessageText = "B013910SV9ER                                               HYEDUSCMT_146618     E03901SV9 70031677B00160008PAPERLESS - FILER RETAIN RECORDS        0130557A     EC3901SV9 70031677B00160008CERT-OGA FORM DATA REQUIRED             01305AIC     EC3901SV9 70031677B00160008CERT-ENTRY CANNOT BE CERTIFIED          013052A3     EC3901SV9 70031677B00160008CERT-REQUIREMENT UNKNOWN                01305FD2     EC3901SV9 70031677B00160008CERT--->ENTRY SUMMARY LINE 0002         01305VDO     EC3901SV9 70031677B00160008CERT-REQUIREMENT UNKNOWN                01305FD2     EC3901SV9 70031677B00160008CERT--->ENTRY SUMMARY LINE 0003         01305VDO     Y  3910SV9ER00007000000001225";

			var printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);
			AssertEquals("UniquePortOfLading", USConstants.MultipleValueIndicator, printBO.UniquePortOfLading);

			var printLine = printBO.EntryPrintLines.OfType<EntrySummary7501Line>().FirstOrDefault(x => x.LineNumber == "1");
			AssertEquals("52051", printLine.PortOfLadingForLine);

			printLine = printBO.EntryPrintLines.OfType<EntrySummary7501Line>().FirstOrDefault(x => x.LineNumber == "2");
			AssertEquals("52030", printLine.PortOfLadingForLine);

			printLine = printBO.EntryPrintLines.OfType<EntrySummary7501Line>().FirstOrDefault(x => x.LineNumber == "3");
			AssertEquals("52030", printLine.PortOfLadingForLine);

			printLine = printBO.EntryPrintLines.OfType<EntrySummary7501Line>().FirstOrDefault(x => x.LineNumber == "4");
			AssertEquals("52000", printLine.PortOfLadingForLine);

			printLine = printBO.EntryPrintLines.OfType<EntrySummary7501Line>().FirstOrDefault(x => x.LineNumber == "5");
			AssertEquals("52325", printLine.PortOfLadingForLine);

			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			message.EM_MessageText = "B013910SV9EI                                               HYEDUSCMT_146616     " +
"10A390113-14792700013-147927000                 8         SV9 7003166901037  TX " +
"20                         403901040213B00160007            001A 040213A001     " +
"22            00145353453                         00000400PC                    " +
"30                                  01              2041213             AA      " +
"40001CH00000007000000000012                                                     " +
"50 3305100000                      X                                CH040213N  X" +
"60                                        CHHARWIN8PLA                          " +
"62          49900000242                                                         " +
"40002CH00000003000000000010                                                     " +
"50 3305100000                      X                                CH040213N  V" +
"60                                        CHHARWIN8PLA                          " +
"40003CH00000004000000000012                                                     " +
"50 3303001000          000000000100L                                CH040213N  V" +
"60                                        CHHARWIN8PLA                          " +
"40004KR00000002000000000012                                                     " +
"50 9102111010                      NO                               KR040213NKR " +
"60                                        CHHARWIN8PLA                          " +
"709102111020                       NO                                         KR" +
"809102111030                       NO                                         KR" +
"819102111040                       NO                                         KR" +
"40005CH00000001000000000010                                                     " +
"50 82111000000000000610            PCS                              CH040213N   " +
"60                                        CHHARWIN8PLA                          " +
"62          49900000035                                                         " +
"708211929045                       NO                                           " +
"8949900000002500                                                                " +
"9000000000610           2                       0000000250000000001000          " +
"Y  3910SV9EI00027000000000610";

			printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);
			AssertEquals("UniquePortOfLading should be empty, because this is AIR job and message doesn't contains Port Of Lading",
				ZString.Empty, printBO.UniquePortOfLading);

			printLine = printBO.EntryPrintLines.OfType<EntrySummary7501Line>().FirstOrDefault(x => x.LineNumber == "1");
			AssertEquals("Not relevant for AIR jobs", ZString.Empty, printLine.PortOfLadingForLine);

			printLine = printBO.EntryPrintLines.OfType<EntrySummary7501Line>().FirstOrDefault(x => x.LineNumber == "4");
			AssertEquals("Not relevant for AIR jobs", ZString.Empty, printLine.PortOfLadingForLine);
		}

		public void TestPPLSSummaryStatus()
		{
			var invoiceHeader = Declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader entry = Declaration.FormalEntry;
			EDIMessage outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outMsg.EM_LinkUniqueID = entry.PK;
			outMsg.EM_MessageText = "B018888XJ5ER                                               35183                10A888848-0920709WM48-0920709WM                 8         GAZ 0000019203732     20     HAKUBA MARU         111601070108B00001029            593  070108N598     22123456789012OB9394043938SMTAOS005870            00000200CT   091108MAEUHDMU   30                                  0                                   MAEU    350000000000000000017813300000000000000000000000891                             40001CN000001916600000000000000015750          000000197157047                  43123456R                                                                       50 73071990600000118829            KG                               CN070108N   51        8CN390434 27600000000100CTN                                           60                                        CNJINME22JIN                         062          49900004025                                                         62          50100002396                                                         40002CN00000294620000000000          0000007500000000302957047                  43123456R                                                                       50 20019038000000282835            KG                               CN070108N   51                                                                              60                                        CNJINMEI3JIN                        0 62          49900006187                                                         62          50100003683                                                         894990000001021250100000006079                                                  9000000401664           0            000001781330000001629100000048628          Y      GAZEI00021000000401664";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			inMsg.EM_MessageText = "B018888XJ5ER                                               35183                E08888XJ5 70029737B00152541SUM ACCPTD W/WARNING;NO PAPER REQ       0180858M     E08888XJ5 70022120B00152541CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000012250";

			Factory.Save();

			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);
			AssertEquals("Shouldn't show as Paperless entry", ZString.Empty, printBO.SummaryStatus);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var outMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			outMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outMsg.EM_MessageText = "B01    GAZEI                                               229                  10A2818            91-013199000                 0         GAZ 0000087901        20                         402818052809B00001104            592  052809         22            61803294852                         00000100KG                    30                                  0                                   SQ      40001SG00000008240000000000                                                     50 9802008068                                                       KR052809N   51                                                                              60                                                                              90                      0                                  00000000824          Y      GAZEI00009";
			Entry.Messages.Add(outMsg);
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			Entry.Messages.Add(inMsg);
			var printBO = new EntryMessageENS7501Print(Entry, outMsg, inMsg, null);
			return printBO;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			DeclarationTestHelper.SetEntryFilerCode("XJ6");
			new FeeCalculationHelperTest().PrepareFeeAndTexData();

			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.Invoices.AddNew();
			Declaration.InvoiceLines.AddNew();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var referenceTesthelper = new UniversalReferenceTestDataHelper(Factory);
			referenceTesthelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			referenceTesthelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "N598", "WANDO TERMINAL", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			referenceTesthelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "I310", "UNITED AIRLINES CARGO", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
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
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				}
				return declaration;
			}
		}

		CusEntryHeader entry;
		CusEntryHeader Entry => entry ?? (entry = Declaration.CustomsEntryHeaders.AddNew());

		OrgHeader CreateOrganisation(ZString codeType, ZString number)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			result.CustomsCodes.AddNew(codeType, number, GlbCompany.CurrentCompany.Country);
			return result;
		}

		void CreateDeferredTaxEntry()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-TAXind";
			invoiceHeader.JZ_InvoiceAmount = 223.50m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2208.30.3030";
			invoiceLine1.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine1.JI_LinePrice = 223.50;
			invoiceLine1.JI_CustomsQuantity = 18.78m;
			invoiceLine1.JI_Weight = 50m;
			invoiceLine1.US_UC_NKCountryOfExport = "GB";
			invoiceLine1.US_UC_NKCountryOfOrigin = "GB";
			invoiceLine1.US_DestinationState = "IL";
		}

		void CreateTIBWithExemptTariffsDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 8464m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130050";
			invoiceLine1.JI_InvoiceQuantity = 95m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 8000m;
			invoiceLine1.JI_Tariff = "8528723600";
			invoiceLine1.JI_CustomsQuantity = 10m;
			invoiceLine1.JI_CustomsUnitQty = "NO";

			JobComInvoiceLine invoiceLine2 = Declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "98130050";
			invoiceLine2.JI_InvoiceQuantity = 5m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_LinePrice = 464m;
			invoiceLine2.JI_Tariff = "7318154000";
			invoiceLine2.JI_CustomsQuantity = 50m;
			invoiceLine2.JI_CustomsUnitQty = "KG";

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateWatchDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "WATCH";
			invoiceHeader.JZ_InvoiceAmount = 9426m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802008068";
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406.23m;

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802008068";
				childLine2.US_98GoodsValue = 1010m;
				childLine2.JI_Tariff = "9102111020";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1608.75m;

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.US_SupTariff = "9802008068";
				childLine4.JI_Tariff = "9102111030";
				childLine4.JI_InvoiceQuantity = 1000m;
				childLine4.JI_InvoiceUQ = "NO";
				childLine4.JI_CustomsQuantity = 1000m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 1345.4m;

				JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine6.US_SupTariff = "9802008068";
				childLine6.US_98GoodsValue = 204m;
				childLine6.JI_Tariff = "9102111040";
				childLine6.JI_InvoiceQuantity = 1000m;
				childLine6.JI_InvoiceUQ = "NO";
				childLine6.JI_CustomsQuantity = 1000m;
				childLine6.JI_CustomsUnitQty = "NO";
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateMulitLineWatchDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			Declaration.US_EnableENS = true;
			Declaration.US_SchDLoading = "55976";
			Declaration.JE_RL_NKPortOfLoading = "SGSIN";

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "WATCH_Multi";
			invoiceHeader.JZ_InvoiceAmount = 19080m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "KR";
			invoiceHeader.US_UC_NKCountryOfExport = "SG";
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				#region entry line 1
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "9102.11.2510";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;
				invoiceLine1.JI_CountryOfOrigin = "KR";

				JobComInvoiceLine childLine1 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine1.JI_Tariff = "9102.11.2520";
				childLine1.JI_CustomsQuantity = 1000m;
				childLine1.JI_CustomsUnitQty = "NO";
				childLine1.JI_LinePrice = 1609m;

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.JI_Tariff = "9102.11.2530";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1345m;

				JobComInvoiceLine childLine3 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine3.JI_Tariff = "9102.11.1040";
				childLine3.JI_CustomsQuantity = 1000m;
				childLine3.JI_CustomsUnitQty = "NO";
				#endregion

				#region entry line 2
				JobComInvoiceLine invoiceLine2 = Declaration.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "9102.11.2510";
				invoiceLine2.JI_CustomsQuantity = 1000m;
				invoiceLine2.JI_CustomsUnitQty = "NO";
				invoiceLine2.JI_LinePrice = 3406m;
				invoiceLine2.JI_CountryOfOrigin = "KR";

				JobComInvoiceLine il2childLine2 = invoiceLine2.AddSecondaryInvoiceLine();
				il2childLine2.JI_Tariff = "9102.11.2520";
				il2childLine2.JI_CustomsQuantity = 1000m;
				il2childLine2.JI_CustomsUnitQty = "NO";
				il2childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine il2childLine4 = invoiceLine2.AddSecondaryInvoiceLine();
				il2childLine2.JI_Tariff = "9102.11.2530";
				il2childLine4.JI_CustomsQuantity = 1000m;
				il2childLine4.JI_CustomsUnitQty = "NO";
				il2childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine il2childLine6 = invoiceLine2.AddSecondaryInvoiceLine();
				il2childLine6.JI_Tariff = "9102.11.2540";
				il2childLine6.JI_CustomsQuantity = 1000m;
				il2childLine6.JI_CustomsUnitQty = "NO";
				#endregion

				#region entry line 3
				JobComInvoiceLine invoiceLine3 = Declaration.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "9102.11.4510";
				invoiceLine3.JI_CustomsQuantity = 1000m;
				invoiceLine3.JI_CustomsUnitQty = "NO";
				invoiceLine3.JI_LinePrice = 3406m;
				invoiceLine3.JI_CountryOfOrigin = "XO";
				invoiceLine3.US_UC_NKCountryOfExport = "CA";
				invoiceLine3.US_SPI = "CA";

				JobComInvoiceLine il3childLine2 = invoiceLine3.AddSecondaryInvoiceLine();
				il3childLine2.JI_Tariff = "9102.11.4520";
				il3childLine2.JI_CustomsQuantity = 1000m;
				il3childLine2.JI_CustomsUnitQty = "NO";
				il3childLine2.JI_LinePrice = 1609m;
				il3childLine2.JI_CountryOfOrigin = "XO";
				il3childLine2.US_UC_NKCountryOfExport = "CA";
				il3childLine2.US_SPI = "CA";

				JobComInvoiceLine il3childLine4 = invoiceLine3.AddSecondaryInvoiceLine();
				il3childLine4.JI_Tariff = "9102.11.4530";
				il3childLine4.JI_CustomsQuantity = 1000m;
				il3childLine4.JI_CustomsUnitQty = "NO";
				il3childLine4.JI_LinePrice = 1345m;
				il3childLine4.JI_CountryOfOrigin = "XO";
				il3childLine4.US_UC_NKCountryOfExport = "CA";
				il3childLine4.US_SPI = "CA";

				JobComInvoiceLine il3childLine6 = invoiceLine3.AddSecondaryInvoiceLine();
				il3childLine6.JI_Tariff = "9102.11.4540";
				il3childLine6.JI_CustomsQuantity = 1000m;
				il3childLine6.JI_CustomsUnitQty = "NO";
				il3childLine6.JI_CountryOfOrigin = "XO";
				il3childLine6.US_UC_NKCountryOfExport = "CA";
				il3childLine6.US_SPI = "CA";
				#endregion
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateWatchRepairsDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			Declaration.US_EnableENS = true;
			Declaration.US_SchDLoading = "55976";
			Declaration.JE_RL_NKPortOfLoading = "SGSIN";

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "WATCH_Repairs";
			invoiceHeader.JZ_InvoiceAmount = 19080m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "KR";
			invoiceHeader.US_UC_NKCountryOfExport = "SG";
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				#region entry line 1
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802.00.8068";
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102.11.1010";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;
				invoiceLine1.JI_CountryOfOrigin = "KR";

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802.00.8068";
				childLine2.US_98GoodsValue = 1010m;
				childLine2.JI_Tariff = "9102.11.1020";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.US_SupTariff = "9802.00.8068";
				childLine4.JI_Tariff = "9102.11.1030";
				childLine4.JI_CustomsQuantity = 1000m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine6.US_SupTariff = "9802.00.8068";
				childLine6.US_98GoodsValue = 204m;
				childLine6.JI_Tariff = "9102.11.1040";
				childLine6.JI_CustomsQuantity = 1000m;
				childLine6.JI_CustomsUnitQty = "NO";
				#endregion

				#region entry line 2
				JobComInvoiceLine invoiceLine2 = Declaration.InvoiceLines.AddNew();
				invoiceLine2.US_SupTariff = "9802.00.4040";
				invoiceLine2.US_98GoodsValue = 1852m;
				invoiceLine2.JI_Tariff = "9102.11.1010";
				invoiceLine2.JI_CustomsQuantity = 1000m;
				invoiceLine2.JI_CustomsUnitQty = "NO";
				invoiceLine2.JI_LinePrice = 3406m;
				invoiceLine2.JI_CountryOfOrigin = "KR";

				JobComInvoiceLine il2childLine2 = invoiceLine2.AddSecondaryInvoiceLine();
				il2childLine2.US_SupTariff = "9802.00.4040";
				il2childLine2.US_98GoodsValue = 1010m;
				il2childLine2.JI_Tariff = "9102.11.1020";
				il2childLine2.JI_CustomsQuantity = 1000m;
				il2childLine2.JI_CustomsUnitQty = "NO";
				il2childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine il2childLine4 = invoiceLine2.AddSecondaryInvoiceLine();
				il2childLine4.US_SupTariff = "9802.00.4040";
				il2childLine4.JI_Tariff = "9102.11.1030";
				il2childLine4.JI_CustomsQuantity = 1000m;
				il2childLine4.JI_CustomsUnitQty = "NO";
				il2childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine il2childLine6 = invoiceLine2.AddSecondaryInvoiceLine();
				il2childLine6.US_SupTariff = "9802.00.4040";
				il2childLine6.US_98GoodsValue = 204m;
				il2childLine6.JI_Tariff = "9102.11.1040";
				il2childLine6.JI_CustomsQuantity = 1000m;
				il2childLine6.JI_CustomsUnitQty = "NO";
				#endregion

				#region entry line 3
				JobComInvoiceLine invoiceLine3 = Declaration.InvoiceLines.AddNew();
				invoiceLine3.US_SupTariff = "9802.00.4040";
				invoiceLine3.US_98GoodsValue = 1852m;
				invoiceLine3.JI_Tariff = "9102111010";
				invoiceLine3.JI_CustomsQuantity = 1000m;
				invoiceLine3.JI_CustomsUnitQty = "NO";
				invoiceLine3.JI_LinePrice = 3406m;
				invoiceLine3.JI_CountryOfOrigin = "XO";
				invoiceLine3.US_UC_NKCountryOfExport = "CA";
				invoiceLine3.US_SPI = "CA";

				JobComInvoiceLine il3childLine2 = invoiceLine3.AddSecondaryInvoiceLine();
				il3childLine2.US_SupTariff = "9802.00.4040";
				il3childLine2.US_98GoodsValue = 1010m;
				il3childLine2.JI_Tariff = "9102111020";
				il3childLine2.JI_CustomsQuantity = 1000m;
				il3childLine2.JI_CustomsUnitQty = "NO";
				il3childLine2.JI_LinePrice = 1609m;
				il3childLine2.JI_CountryOfOrigin = "XO";
				il3childLine2.US_UC_NKCountryOfExport = "CA";
				il3childLine2.US_SPI = "CA";

				JobComInvoiceLine il3childLine4 = invoiceLine3.AddSecondaryInvoiceLine();
				il3childLine4.US_SupTariff = "9802.00.4040";
				il3childLine4.JI_Tariff = "9102111030";
				il3childLine4.JI_CustomsQuantity = 1000m;
				il3childLine4.JI_CustomsUnitQty = "NO";
				il3childLine4.JI_LinePrice = 1345m;
				il3childLine4.JI_CountryOfOrigin = "XO";
				il3childLine4.US_UC_NKCountryOfExport = "CA";
				il3childLine4.US_SPI = "CA";

				JobComInvoiceLine il3childLine6 = invoiceLine3.AddSecondaryInvoiceLine();
				il3childLine6.US_SupTariff = "9802.00.4040";
				il3childLine6.US_98GoodsValue = 204m;
				il3childLine6.JI_Tariff = "9102111040";
				il3childLine6.JI_CustomsQuantity = 1000m;
				il3childLine6.JI_CustomsUnitQty = "NO";
				il3childLine6.JI_CountryOfOrigin = "XO";
				il3childLine6.US_UC_NKCountryOfExport = "CA";
				il3childLine6.US_SPI = "CA";
				#endregion
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateMergedDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;

			OrgHeader importer2 = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "48-0920709WM");
			importer2.OH_FullName = "Test Import Company 2";
			OrgHeader importer3 = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "48-0920709WM");
			importer3.OH_FullName = "Another Test Company";

			OrgHeader importer = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "48-0920709WM");
			importer.OH_FullName = "Test Import Company 1";
			Declaration.JE_OH_Importer = importer.PK;
			Declaration.IOROrgPK = importer.PK;

			OrgHeader ultimateConsignee = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789");
			ultimateConsignee.OH_FullName = "Ultimate Consignee";
			Declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			OrgHeader ultimateConsignee2 = CreateOrganisation(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789");
			ultimateConsignee2.OH_FullName = "Test Ultimate Consignee 2";

			CusEntryHeader entry = Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();

			CusEntryHeader entry2 = Declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			JobComInvoiceHeader invoice1 = Declaration.Invoices.AddNew();
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			invoice1.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			manufacturer.OH_FullName = "Manufacturer";
			manufacturer.FillWithValidTestData();
			OrgCusCode manufacturerCode = manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line1.US_UC_NKCountryOfOrigin = "HK";
			line1.US_UC_NKCountryOfExport = "HK";

			JobComInvoiceHeader invoice2 = Declaration.Invoices.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;
			line2.US_UC_NKCountryOfOrigin = "HK";
			line2.US_UC_NKCountryOfExport = "HK";

			JobComInvoiceHeader invoice3 = Declaration.Invoices.AddNew();
			JobComInvoiceLine line3 = invoice3.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine2.PK;
			line3.US_UC_NKCountryOfOrigin = "HK";
			line3.US_UC_NKCountryOfExport = "HK";

			JobComInvoiceHeader invoice4 = Declaration.Invoices.AddNew();
			JobComInvoiceLine line4 = invoice4.JobComInvoiceLines.AddNew();
			line4.JI_CL = entryLine2.PK;
			line4.US_UC_NKCountryOfOrigin = "HK";
			line4.US_UC_NKCountryOfExport = "HK";

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void Create99Declaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			Declaration.US_EnableENS = true;
			Declaration.US_SchDLoading = "55976";
			Declaration.JE_RL_NKPortOfLoading = "SGSIN";

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "99Entry";
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "KR";
			invoiceHeader.US_UC_NKCountryOfExport = "SG";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.US_DateOfExport = new ZDateTime(2010, 04, 14);

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "0201.30.8010";
				invoiceLine1.US_SupTariff = "9904.02.37";
				invoiceLine1.US_98GoodsValue = 0m;
				invoiceLine1.JI_InvoiceQuantity = 0m;
				invoiceLine1.JI_CustomsQuantity = 1500m;
				invoiceLine1.JI_CustomsUnitQty = "KG";
				invoiceLine1.JI_LinePrice = 10000m;
				invoiceLine1.JI_Weight = 13m;
				invoiceLine1.JI_WeightUQ = "KT";
				invoiceLine1.JI_CountryOfOrigin = "NZ";
				invoiceLine1.US_UC_NKCountryOfExport = "NZ";
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateNonWatchRepairsDeclaration()
		{
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			Declaration.US_EnableENS = true;
			Declaration.US_SchDLoading = "55976";
			Declaration.JE_RL_NKPortOfLoading = "SGSIN";

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "99Entry";
			invoiceHeader.JZ_InvoiceAmount = 5000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "NI";
			invoiceHeader.US_UC_NKCountryOfExport = "NI";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.US_DateOfExport = new ZDateTime(2010, 04, 14);

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
				invoiceLine1.JI_Tariff = "9207.10.0065";
				invoiceLine1.US_SupTariff = "9802.00.4040";
				invoiceLine1.US_98GoodsValue = 4999m;
				invoiceLine1.JI_InvoiceQuantity = 0m;
				invoiceLine1.JI_CustomsQuantity = 100m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 5000m;
				invoiceLine1.JI_Weight = 150m;
				invoiceLine1.JI_WeightUQ = "KG";
				invoiceLine1.JI_CountryOfOrigin = "KR";
				invoiceLine1.US_UC_NKCountryOfExport = "NI";
			}

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void CreateRateEntryForMessageMatching()
		{
			entry = null;
			declaration = null;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			Declaration.US_EnableENS = true;
			Declaration.US_TeamNo = "808";

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "SimpleInv";
			invoiceHeader.JZ_InvoiceAmount = 1000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2208.30.3030";
			invoiceLine1.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine1.JI_LinePrice = 1000;
			invoiceLine1.JI_CustomsQuantity = 100m;
			invoiceLine1.JI_Weight = 50m;
			invoiceLine1.US_UC_NKCountryOfExport = "GB";
			invoiceLine1.US_UC_NKCountryOfOrigin = "GB";
			invoiceLine1.US_DestinationState = "IL";

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}
	}
}
