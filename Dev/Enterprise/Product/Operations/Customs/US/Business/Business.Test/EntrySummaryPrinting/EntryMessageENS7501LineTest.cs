using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntryMessageENS7501Line))]
	sealed class EntryMessageENS7501LineTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFactoryPassedInFromCusEntryLineIsUsedToLoadOtherObjects()
		{
			SetUpMergedInvoices("6103220010");

			var entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());

			var entryMessageLineWrapper = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false);
			AssertEquals("Wrapper should use factory from line to avoid complete Dec being reloaded in a new factory for every Entry Line"
				, entryMessageLine.Factory
				, entryMessageLineWrapper.Factory);
		}

		public void TestConstructor()
		{
			SetUpMergedInvoices("6103220010");

			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}

			EntryMessageENS7501Line eml = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false);
			AssertEquals(false, eml.PrintInvoiceHeading);
			AssertEquals(false, eml.PrintInvoiceDetails);
			AssertEquals("1", eml.LineNumber);

			Factory.Save();

			eml = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false, false, false, true, true, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals(true, eml.PrintInvoiceHeading);
			AssertEquals(true, eml.PrintInvoiceDetails);
			AssertEquals("001/INV001", eml.InvoiceDetails.InvoiceNo);
		}

		public void TestSoftwoodLumberDetails()
		{
			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false, false, false, true, true, ZString.Empty, rateStrings, docData, null, ZString.Empty);
			AssertEquals("LumberExportPrice", 375.00m, msgLineDetails.LumberExportPrice);
			AssertEquals("LumberExportPrice", "Y", msgLineDetails.LumberImporterDeclaration);
			AssertEquals("LumberExportPrice", 15.00m, msgLineDetails.LumberExportCharges);
		}

		public void TestMultiRelationships()
		{
			SetUpMergedInvoices("2204215030");
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false, true, false, true, true, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("TransRelatedInd", "Y", msgLineDetails.TransRelatedInd);

			msgLineDetails = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, ZString.Empty);
			AssertEquals("TransRelatedInd on line should be blank when relationship printed on Invoice", "", msgLineDetails.TransRelatedInd);
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
			var message = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false).PopulateMessage();

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;

			var printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);
			var entryPrintLine = printBO.EntryPrintLines.Cast<EntryMessageENS7501Line>().FirstOrDefault(x => x.LineNumber == "1");
			AssertEquals("", entryPrintLine.ExportDate);

			invoiceLine2.US_DateOfExport = ZDateTime.BrettsBirthday.AddDays(1);
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			message = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false).PopulateMessage();
			printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);
			entryPrintLine = printBO.EntryPrintLines.Cast<EntryMessageENS7501Line>().FirstOrDefault(x => x.LineNumber == "1");
			AssertEquals(ZDateTime.BrettsBirthday.ToString("MMddyy"), entryPrintLine.ExportDate);

			entryPrintLine = printBO.EntryPrintLines.Cast<EntryMessageENS7501Line>().FirstOrDefault(x => x.LineNumber == "2");
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1).ToString("MMddyy"), entryPrintLine.ExportDate);
		}

		public void TestProductNumber()
		{
			SetUpMergedInvoices("7318160060");
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "JJ Imports";
			OrgCusCode importerEINCode = importer.CustomsCodes.AddNew();
			importerEINCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			importerEINCode.OK_CustomsRegNo = "75-2221134";
			var countryData = importer.CountryData;
			var wrapper = OrgHeaderWrapper.New(importer);
			wrapper.ZO_ENSPrintProduct = true;

			Declaration.JE_OH_Importer = importer.PK;
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();

			message.EM_MessageText = @"B013910SV9EI                                               HYEDUSCMT_146941     10A390113-14792700013-147927000                 8         SV9 7003192522037  TX 20                         403901043013B00160089            001  043013B002     22            00144234923                         00000005PC                    30                                  0               2051013             AA      40001CH00000000000000000000                                                     50 7318160060          000000002000KG                               CH043013N   60                                        CHHARWIN8PLA                          62          49900000000                                                         8949900000002500                                                                90                      0                       00000002500                     Y  3910SV9EI00010";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;

			var printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);
			AssertEquals("Entry print lines should be merged", 1, printBO.EntryPrintLines.Count);
			var entryLine1 = printBO.EntryPrintLines[0];
			AssertEquals("Part Number should not be printed when merged by tariff", "", entryLine1.Block29Element1);

			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CreateDocPrintingDetails(message.PK);
			inMsg.EM_LinkUniqueID = entry.PK;
			message.EM_MessageText = @"B013910SV9EI                                               HYEDUSCMT_146942     10A390113-14792700013-147927000                 8         SV9 7003192501037  TX 20                         403901043013B00160089            001  043013B002     22            00144234923                         00000005PC                    30                                  0               2051013             AA      40001CH00000026350000000010                    0000000135                       50 7318160060          000000001000KG                               CH043013N   60                                        CHHARWIN8PLA                          62          49900000913                                                         40002CH00000026350000000010                    0000000135                       50 7318160060          000000001000KG                               CH043013N   60                                        CHHARWIN8PLA                          62          49900000913                                                         8949900000002500                                                                90                      0                       0000000250000000005270          Y  3910SV9EI00014";
			printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);
			AssertEquals("Entry print lines should not be merged now", 2, printBO.EntryPrintLines.Count);
			entryLine1 = printBO.EntryPrintLines[0];
			var entryLine2 = printBO.EntryPrintLines[1];
			AssertEquals("Product Number (Block29Element1) should print when merged by product", "0-423-55-500-2", entryLine1.Block29Element1);
			AssertEquals("Product Number (Block29Element1) should print when merged by product", "0-02-90-124-0", entryLine2.Block29Element1);
		}

		public void TestBlock29Elements()
		{
			Declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = Declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV001";
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 1000m;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "9506910030";

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, true);
			var message = builder.PopulateMessage();
			Factory.Save();

			message.EM_MessageText = "B011703286EI                                  2704286  1   YASYUSPRD_234461     10A170336-35111200036-351112000                 8      E  286 6769032601281  GA 20     HYUNDAI INTEGRAL    111703090313AX001460430          035E 090313L737     22            2372170230                          00000917CT         NYKS       30                                  11            107091313             NYKS    40001CN00000092000000007516                    000000692057035                  42CNMAGINT88LAK  10997OD-1        0001                                          43      D BFX 552 SELECTECH DB                                                  50 95069100300000042320            X                                CN080813N   60                                        CNMAGINT88LAK                         62          50100001150                                                         62          49900003187                                                         895010000000115049900000003187                                                  9000000042320           0                       0000000433700000009200          Y  1703286EI00013000000042320";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			inMsg.EM_MessageText = "B011703286ER                                  2704286  1   YASYUSPRD_234461     E01703286 67690326AX0014604PAPERLESS - FILER RETAIN RECORDS        0143357A     E01703286 67690326AX0014604CERT-RELEASE CERTIFIED VIA SUMMARY      014332A5     Y  1703286ER00002000000042320";

			entry.CreateDocPrintingDetails(message.PK);
			Factory.Save();
			Declaration.Invoices.RemoveFromRelationship(invoice);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var declarationReloaded = newFactory.Load<JobDeclaration>(Declaration.PK);

			var invoice2 = declarationReloaded.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "INV001";
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_InvoiceAmount = 1000m;
			var invoiceLineNew = invoice2.JobComInvoiceLines.AddNew();
			invoiceLineNew.JI_Tariff = "6304930000";

			var supplier = newFactory.New<OrgHeader>();
			supplier.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			var address = supplier.MainAddress;
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AUBEREQU6LON");
			invoiceLineNew.JI_OA_ManufacturerAddress = address.PK;
			invoiceLineNew.JI_CustomAttrib2 = "ATTRIBUTE2";

			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "6304930000";
			invoiceLine2.JI_OA_ManufacturerAddress = address.PK;

			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "6304930000";
			invoiceLine3.JI_OA_ManufacturerAddress = address.PK;
			var printBO = new EntryMessageENS7501Print(declarationReloaded.ActiveEntryHeaders.EntrySummaryEntry, message, inMsg, null);

			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);
			AssertEquals(ZString.Empty, printBO.EntryPrintLines[0].Block29Element1);
		}

		public void TestSPIAndOrSecondarySPIDetails()
		{
			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(spiEntryMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, ZString.Empty);
			AssertEquals("SPI from message", "BH", msgLineDetails.SPIAndOrSecondarySPI);
		}

		[TestDate(2008, 3, 25)]
		public void TestWineExciseTaxPrintsFromMessage()
		{
			SetUpMergedInvoices("2204215030");
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid(new ZGuid()));
			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}

			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(exciseMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("Wine Tax", 2246.31m, msgLineDetails.LineFeeAmount);
			AssertEquals("HMF", 107.26m, msgLineDetails.HMFAmount);
			AssertEquals("MPF", 180.19m, msgLineDetails.MPFAmount);
			AssertEquals(true, msgLineDetails.HasMPF);

			AssertEquals("SPI from message", "S", msgLineDetails.SPIAndOrSecondarySPI);
		}

		[TestDate(2008, 3, 25)]
		public void TestMPFAndHMFCodeAndDescription()
		{
			SetUpMergedInvoices("2204215030");
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid(new ZGuid()));
			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}

			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(exciseMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("499 499 DESC FROM DB (MPF)", msgLineDetails.MPFCodeAndDescription);
			AssertEquals("501 501 DESC FROM DB (HMF)", msgLineDetails.HMFCodeAndDescription);
		}

		[TestDate(2008, 3, 25)]
		public void TestHasMPF()
		{
			SetUpMergedInvoices("8466939585");
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid(new ZGuid()));
			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}

			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(MPFMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("MPF", 0m, msgLineDetails.MPFAmount);
			AssertEquals(true, msgLineDetails.HasMPF);

			msgLineDetails = new EntryMessageENS7501Line(NOMPFMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("MPF", 0m, msgLineDetails.MPFAmount);
			AssertEquals(false, msgLineDetails.HasMPF);
		}

		public void TestMPFPercentAsString()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			var invLine = invoice1.JobComInvoiceLines.AddNew();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			entry.CreateDocPrintingDetails(message.PK);
			Factory.Save();

			message.EM_MessageText = "B018888XJ5EI                                               44376                10A110191-01319900091-013199000                 8         XJ5 7002660001891  AK 20     ADMIRALENGRACHT     111101013110B00153049            123  013110C001 001 22            TESTAPLU                            00000010BX         APLU       30                                  0               1                   APLU    40001HN00000025450000000437                    000000005760204                  50 9802008068                                                       AU013110Y   51                  647                                                         60                                        AUABCEXP72ALE                         62          50100000626                                                         62          49900000518                                                         706203434030 0000068774000000008300DOZ000000043700KG                0000002465  40002US00000002190000000019                    000000000260204                  50 9801001010                      X                                AU013110Y   51                                                                              60                                        AUABCEXP72ALE                         62          50100000027                                                         894990000000250050100000000653                                                  9000000068774           0                       0000000315300000005229          Y  8888XJ5EI00018000000068774";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			var printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);

			var printLine = printBO.EntryPrintLines[0];
			AssertEquals("MPF Rate as string", "0.3464%", printLine.MPFPercentAsString);

			message.EM_SystemCreateTimeUtc = new ZDateTime(2011, 09, 27);
			printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);
			printLine = printBO.EntryPrintLines[0];
			AssertEquals("MPF Rate as string should be found based on Message Sent date", "0.21%", printLine.MPFPercentAsString);
		}

		[TestDate(2008, 3, 25)]
		public void TestBeerExciseTaxPrintsFromMessage()
		{
			SetUpMergedInvoices("2203000060");
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}

			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(exciseMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("Other Excise Tax (Beer)", 2246.31m, msgLineDetails.LineFeeAmount);
		}

		[TestDate(2008, 3, 25)]
		public void TestTobaccoExciseTaxPrintsFromMessage()
		{
			SetUpMergedInvoices("2402103030");
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}

			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(exciseMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("Tobacco Excise", 2246.31m, msgLineDetails.LineFeeAmount);
		}

		public void TestSecondaryTariffLines()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			var invLine = invoice1.JobComInvoiceLines.AddNew();

			var entry = Declaration.CustomsEntryHeaders.AddNew();
			var line = entry.MergedLines.AddNew();
			line.CL_LineNumber = (ZShort)1;
			invLine.JI_CL = line.PK;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			entry.CreateDocPrintingDetails(message.PK);

			var documentData = entry.US7501DocPrintingData[0];
			var msgLineDetails = new EntryMessageENS7501Line(watchMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, documentData, null, ZString.Empty);

			AssertEquals("SecondaryLine2FormattedTariff", "9802.00.4040", msgLineDetails.SecondaryLine2FormattedTariff);
			AssertEquals("SecondaryLine5CustomsQuantity", "WATCH BATTERY,BAT POW, AU,", msgLineDetails.SecondaryLine4Description);
			AssertEquals("SecondaryLine5CustomsQuantity", "NO", msgLineDetails.SecondaryLine5CustomsUnitQty);
			AssertEquals("SecondaryLine6FormattedTariff", "9102.11.1040", msgLineDetails.SecondaryLine6FormattedTariff);
		}

		public void TestSecondaryLine1SecondQtyAndUQ()
		{
			entryMessageLine.ens70 = new ENS70();
			entryMessageLine.ens70.Quantity2 = 10;
			entryMessageLine.ens70.Unit2 = Core.Constants.Weight.Kilograms;

			entryMessageLine.ens80 = new ENS80();
			entryMessageLine.ens80.Quantity2 = 11;
			entryMessageLine.ens80.Unit2 = Core.Constants.Weight.Kilograms;

			List<ENS81> ens81s = new List<ENS81>();

			AddEns81(ens81s, 12m);
			AddEns81(ens81s, 13m);
			AddEns81(ens81s, 14m);
			AddEns81(ens81s, 15m);
			AddEns81(ens81s, 16m);

			entryMessageLine.ens81 = ens81s;

			EntryMessageENS7501Line ens7501Line = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false);
			AssertEquals("10 KG", ens7501Line.SecondaryLine1SecondQtyAndUQ);
			AssertEquals("11 KG", ens7501Line.SecondaryLine2SecondQtyAndUQ);
			AssertEquals("12 KG", ens7501Line.SecondaryLine3SecondQtyAndUQ);
			AssertEquals("13 KG", ens7501Line.SecondaryLine4SecondQtyAndUQ);
			AssertEquals("14 KG", ens7501Line.SecondaryLine5SecondQtyAndUQ);
			AssertEquals("15 KG", ens7501Line.SecondaryLine6SecondQtyAndUQ);
			AssertEquals("16 KG", ens7501Line.SecondaryLine7SecondQtyAndUQ);
		}

		public void TestSecondaryLine1SecondQtyAndUQ_WhereThereAreNoSecondaryLines()
		{
			EntryMessageENS7501Line ens7501Line = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false);
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
			EntryMessageENS7501Line ens7501Line = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false);
			AssertEquals(9.43m, ens7501Line.LineFeeAmount);
		}

		public void TestHideSecondaryLine1SecondQtyLineTrue()
		{
			EntryMessageENS7501Line ens7501Line = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false);
			AssertEquals("HideSecondaryLine1SecondQtyLine - no qty, no ADD details", true, ens7501Line.HideSecondaryLine1SecondQtyLine);
		}

		public void TestHideSecondaryLine1SecondQtyLineFalse()
		{
			SetUpMergedInvoices("8211100000");
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}
			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(ADDCVDMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("HideSecondaryLine1SecondQtyLine - ADD details exist", false, msgLineDetails.HideSecondaryLine1SecondQtyLine);
		}

		public void TestSecondaryLine1ADDDetails()
		{
			SetUpMergedInvoices("8211100000");
			var entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}
			var msgLineDetails = new EntryMessageENS7501Line(ADDCVDMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("SecondaryLine1ADDNo", "A570-204-006", msgLineDetails.SecondaryLine1ADDNo);
			AssertEquals("SecondaryLine1ADDFormatted", "6627.95", msgLineDetails.SecondaryLine1ADDFormatted);
			AssertEquals("SecondaryLine1ADDRate", "189.37%", msgLineDetails.SecondaryLine1ADDRate);
			AssertEquals("SecondaryLine1ADDSpecificDepositValueFormatted", "(3500)", msgLineDetails.SecondaryLine1ADDSpecificDepositValueFormatted);

			msgLineDetails = new EntryMessageENS7501Line(ADDCVDMessageLine, false, false, false, false, false, false, false, false, "444", rateStrings, docData, null, entry.EntryType);
			AssertEquals("SecondaryLine1ADDSurety", "Surety Code #444", msgLineDetails.SecondaryLine1ADDSurety);
			AssertEquals("Line1ADDSurety should be blank as ADD is on secondary line", "", msgLineDetails.ADDSurety);
		}

		public void TestSecondaryLine1ADDNoWhenSupplementaryTariff()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A570204006";
			addCase.U5_ISOCountryCode = "IT";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "8205203000";
			var addRate = addCase.CaseRates.AddNew();
			addRate.U6_AdValoremRate = 0.19m;
			addRate.U6_EffectiveDate = ZDateTime.Today;

			USCACCase cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C427819000";
			cvdCase.U5_ISOCountryCode = "IT";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "8205203000";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 0.01m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

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

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			entry.CreateDocPrintingDetails(message.PK);
			Factory.Save();

			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}

			var msgLineDetails = new EntryMessageENS7501Line(ADDCVDMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, new ZString[8], docData, null, ZString.Empty);
			AssertEquals("SecondaryLine1ADDNo", "A570-204-006", msgLineDetails.SecondaryLine1ADDNo);
		}

		public void TestSecondaryLineADDDetailsPrintForTIBEntry()
		{
			SetUpMergedInvoices("7220209060");

			var tIBADDMessageLine = new EntryMessageLine(Factory);
			var ens40 = new ENS40();
			ens40.Deserialise("40001JP00000000000000000200                    0000000500                       ");
			tIBADDMessageLine.ens40 = ens40;

			var ens50 = new ENS50();
			ens50.Deserialise("50 9813000540                      X                                JP062712N   ");
			tIBADDMessageLine.ens50 = ens50;

			var ens60 = new ENS60();
			ens60.Deserialise("600000024300C427819000A5888450000000662795                          012151893700");
			tIBADDMessageLine.ens60 = ens60;

			var ens70 = new ENS70();
			ens70.Deserialise("707220209060           000000020000KG                               0000004500  ");
			tIBADDMessageLine.ens70 = ens70;

			var builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Declaration.ActiveEntryHeaders.EntrySummaryEntry.CreateDocPrintingDetails(message.PK);

			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}

			var msgLineDetails = new EntryMessageENS7501Line(tIBADDMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, EntryTypeList.Codes.TemporaryImportationBond);
			AssertEquals("SecondaryLine1ADDNo", "A588-845-000", msgLineDetails.SecondaryLine1ADDNo);
			AssertEquals("SecondaryLine1ADDRate", "189.37%", msgLineDetails.SecondaryLine1ADDRate);
			AssertEquals("SecondaryLine1ADDFormatted", "6627.95", msgLineDetails.SecondaryLine1ADDFormatted);
		}

		[ExpectNoExceptions]
		public void TestSecondaryLine1ADDNoWhenNoSecondaryLine()
		{
			SetUpMergedInvoices("6103220010");
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}
			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("SecondaryLine1ADDNo should be blank and not cause exception when no secondary line present", "", msgLineDetails.SecondaryLine1ADDNo);
		}

		public void TestSecondaryLine1CVDNo()
		{
			SetUpMergedInvoices("8211100010");
			var entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}
			var msgLineDetails = new EntryMessageENS7501Line(ADDCVDMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("SecondaryLine1CVDNo", "C427-819-000", msgLineDetails.SecondaryLine1CVDNo);
			AssertEquals("SecondaryLine1CVDRate", "12.15%", msgLineDetails.SecondaryLine1CVDRate);
			AssertEquals("SecondaryLine1CVDFormatted", "243.00", msgLineDetails.SecondaryLine1CVDFormatted);
			AssertEquals("SecondaryLine1CVDSpecificDepositValueFormatted", "(2000)", msgLineDetails.SecondaryLine1CVDSpecificDepositValueFormatted);

			msgLineDetails = new EntryMessageENS7501Line(ADDCVDMessageLine, false, false, false, false, false, false, false, false, "444", rateStrings, docData, null, entry.EntryType);
			AssertEquals("SecondaryLine1CVDSurety", "Surety Code #444", msgLineDetails.SecondaryLine1CVDSurety);
		}

		public void TestSecondaryLine1NullReference()
		{
			SetUpMergedInvoices("8211100001");
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}
			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(ADDCVDMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("Accessing SecondaryLine1CVDNo should not cause exception", "", msgLineDetails.SecondaryLine1CVDNo);
		}

		public void TestAllOtherSecondaryLinesNullReference()
		{
			SetUpMergedInvoices("8211100001");
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			docData = null;
			if (DocPrintingData.Length > 0)
			{
				docData = DocPrintingData[0];
			}
			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(ADDCVDMessageLine, false, false, false, false, false, false, false, false, ZString.Empty, rateStrings, docData, null, entry.EntryType);
			AssertEquals("Accessing SecondaryLine3DutyAmount should not cause exception", 0m, msgLineDetails.SecondaryLine3DutyAmount);
			AssertEquals("Accessing SecondaryLine5DutyAmount should not cause exception", 0m, msgLineDetails.SecondaryLine5DutyAmount);
			AssertEquals("Accessing SecondaryLine7DutyAmount should not cause exception", 0m, msgLineDetails.SecondaryLine7DutyAmount);
		}

		public void TestLicenses()
		{
			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false);
			AssertEquals("Licence should blank", "", msgLineDetails.LicenseNumber);
			AssertEquals("Licence Text should be blank", "", msgLineDetails.LicenseText);
		}

		public void TestVisaCertificateNumber()
		{
			EntryMessageENS7501Line msgLineDetails = new EntryMessageENS7501Line(entryMessageLine, false, false, false, false);
			AssertEquals("Visa should blank", "", msgLineDetails.VisaCertificateNumber);
			AssertEquals("SPI from message", "K.S", msgLineDetails.SPIAndOrSecondarySPI);
		}

		[TestDate(2009, 12, 02)]
		public void TestSecondaryLineDutyForNonWatchOSAssembledGoods()
		{
			CreateOSAssembledGoodsDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               44376                10A110191-01319900091-013199000                 8         XJ5 7002660001891  AK 20     ADMIRALENGRACHT     111101013110B00153049            123  013110C001 001 22            TESTAPLU                            00000010BX         APLU       30                                  0               1                   APLU    40001HN00000025450000000437                    000000005760204                  50 9802008068                                                       AU013110Y   51                  647                                                         60                                        AUABCEXP72ALE                         62          50100000626                                                         62          49900000518                                                         706203434030 0000068774000000008300DOZ000000043700KG                0000002465  40002US00000002190000000019                    000000000260204                  50 9801001010                      X                                AU013110Y   51                                                                              60                                        AUABCEXP72ALE                         62          50100000027                                                         894990000000250050100000000653                                                  9000000068774           0                       0000000315300000005229          Y  8888XJ5EI00018000000068774";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);

			AssertEquals("Entry print lines count", 2, printBO.EntryPrintLines.Count);

			AssertEquals("Parent 1 Tariff number", "9802.00.8068", printBO.EntryPrintLines[0].FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "6203.43.4030", printBO.EntryPrintLines[0].SecondaryLine1FormattedTariff);
			AssertEquals("Formatted Tariff Line 2", "9801.00.1010", printBO.EntryPrintLines[1].FormattedTariff);

			EntrySummary7501Line entryLine = printBO.EntryPrintLines[0];

			AssertEquals("Box 37 Total Duty should remain as actual Duty", 687.74m, printBO.TotalDutyAmt);

			AssertEquals("Line 1 Assembled Component Duty to print on 7501 should be same", 0m, printBO.EntryPrintLines[0].DutyAmount);
			AssertEquals("Line 1 Assembled Component Duty Rate to print on 7501", "Free", printBO.EntryPrintLines[0].DutyPercentAsString);

			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501 should be same", 687.74m, printBO.EntryPrintLines[0].SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "27.90%", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);
		}

		[TestDate(2013, 08, 15)]
		public void TestSecondaryLineCompoundDutyRateForWI00046587()
		{
			CreateDeclarationForWI00046587();
			var entry = Declaration.CustomsEntryHeaders[0];

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B013910SV9EI                                               HYEDUSCMT_147714     10A390120-15074560020-150745600                 8         SV9 7003277401035  IL 20     HY EXPLORER         103901082013B00160470            227E 082013J170 001 22            OB94328                             00000001PC         AWAD       30                                  01              1                   AWAD    40001HK00000050000000001000                    000000100058201                  50 9802008068                                                       HK081213N   60                                        HKABCEXP123HON                        62          50100002500                                                         62          49900005196                                                         706201110010 0000275250000000004200DOZ000000100000KG                0000015000  894990000000519650100000002500                                                  9000000275250           0                       0000000769600000020000          Y  3910SV9EI00012000000275250";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);

			AssertEquals("Entry print lines count", 1, printBO.EntryPrintLines.Count);
			AssertEquals("Parent 1 Tariff number", "9802.00.8068", printBO.EntryPrintLines[0].FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "6201.11.0010", printBO.EntryPrintLines[0].SecondaryLine1FormattedTariff);

			EntrySummary7501Line entryLine = printBO.EntryPrintLines[0];

			AssertEquals("Box 37 Total Duty should remain as actual Duty", 2752.50m, printBO.TotalDutyAmt);

			AssertEquals("Line 1 Duty to print on 7501", 0m, entryLine.DutyAmount);
			AssertEquals("Line 1 Duty Rate to print on 7501", "Free", entryLine.DutyPercentAsString);

			AssertEquals("Secondary Line 1 Compound Duty to print on 7501 should be total duty", 2752.50m, entryLine.SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 1 Compound Duty Rate to print on 7501", "18.35%", entryLine.SecondaryLine1DutyPercentAsString);
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
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Declaration.ActiveEntryHeaders.EntrySummaryEntry.CreateDocPrintingDetails(message.PK);
			entry.US7501DocPrintingData[0].US_DutyDate = ZDateTime.Today.AddDays(-1);
			Factory.Save();

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;

			var headerPrint = new EntryMessageENS7501Print(entry, message, inMsg, null);
			var linePrint = (EntryMessageENS7501Line)headerPrint.EntryPrintLines[0];
			AssertEquals("Entry Line should be null", null, linePrint.line);
			AssertEquals("DateForDutyCalculation should be from Doc Data", entry.US7501DocPrintingData[0].US_DutyDate, linePrint.DateForDutyCalculation);

			entry.US7501DocPrintingData[0].US_DutyDate = ZDateTime.Empty;
			headerPrint = new EntryMessageENS7501Print(entry, message, inMsg, null);
			linePrint = (EntryMessageENS7501Line)headerPrint.EntryPrintLines[0];
			AssertEquals("Entry Line should be null", null, linePrint.line);
			AssertEquals("DateForDutyCalculation should be from Doc Data", ZDate.Today, linePrint.DateForDutyCalculation);
		}

		public void TestBindingRulingWithLabel()
		{
			CreateOSAssembledGoodsDeclaration();
			var builder = new EntrySummaryMessageBuilder(Declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			message.EM_MessageText = "B018888XJ5EI                                               55881                10A888891-01319900091-013199000                 8      E  XJ5 7003672401891  IL 20                         408888012511B00155018            430  012511I317     22            00112552551                         00000001PC                    30                                  11              2021011             AA      40001GB00000100000000000091                    0000000455                       42GBBOOMED295LON 12555            0001                                          43      D PLASTIC SEAL                                                          50 39269045900000035000            X                                GB012511N   51                                                                              60                                        GBBOOMED295LON                        62          49900002100                                                         40002GB00000010000000000009                    0000000045                       42GBBOOMED295LON 12555            0002                                          4387604 R                                                                       43      D SWITCHES                                                              50 85365090650000002700000000010000NO                               GB012511N   51                                                                              60                                        GBBOOMED295LON                        62          49900000210                                                         8949900000002500                                                                9000000037700           0                       0000000250000000011000          Y  8888XJ5EI00021000000037700";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_MessageText = "B018888XJ5ER                                               55881                E08888XJ5 70036724B00155018PAPERLESS - FILER RETAIN RECORDS        0180857A     E08888XJ5 70036724B00155018CERT-RELEASE CERTIFIED VIA SUMMARY      018082A5     Y  8888XJ5ER00002000000037700";
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);

			AssertEquals("Entry print lines count", 2, printBO.EntryPrintLines.Count);
			AssertEquals("Formatted Tariff Line 1", "3926.90.4590", printBO.EntryPrintLines[0].FormattedTariff);
			AssertEquals("BindingRulingWithLabel not expected on this line", "", printBO.EntryPrintLines[0].BindingRulingWithLabel);
			AssertEquals("Formatted Tariff Line 2", "8536.50.9065", printBO.EntryPrintLines[1].FormattedTariff);
			AssertEquals("BindingRulingWithLabel expected with this line", "RLNG 87604", printBO.EntryPrintLines[1].BindingRulingWithLabel);
		}

		[TestDate(2010, 09, 28)]
		public void TestCottonFeeOnChildLine()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.JE_TransportMode = "SEA";
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
			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();

			message.EM_MessageText = "B018888XJ5EI                                               53947                10A390191-013199000                             8         XJ5 7003365501891  AK 20     23                  103901092710B00154216            56   092710I310     22            78324UIO                            00000001PK         APLU       30                                  0               1                   APLU    40001DE00000050010000006571                    000000002558886                  50 61042200100000079516000000084000DOZ000000328600KG                DE092710N   51                  335                                                         60                                        DEFRIGMB2731MAR                       62          50100000625                                                         62          49900001050                                                         62610220002005600003609                                                         706102200020           000000084000DOZ000000328600KG                            40002DE00000049000000004393                    000000002558886                  50 61042200400000073010000000084000DOZ000000219700KG                DE092710N   51                  348                                                         60                                        DEFRIGMB2731MAR                       62610422004005600002152                                                         62          50100000613                                                         62          49900001029                                                         706104622028           000000084000DOZ000000219700KG                            89501000000012384990000000250005600000005761                                    9000000152526           0                       0000000949900000009901          Y  8888XJ5EI00022000000152526";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;

			var printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);
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
			AssertEquals("Line 1 Fee", 36.09m, entryLine1.LineFeeAmount);
			AssertEquals("Line 1 Fee Rate", "0.8345c/KG", entryLine1.LineFeePercentAsString);

			AssertEquals("Line 2 Fee Desc", "056 056 DESC FROM DB", entryLine2.LineFeeDescription);
			AssertEquals("Line 2 Fee", 21.52m, entryLine2.LineFeeAmount);
			AssertEquals("Line 2 Fee Rate", "0.8889c/KG", entryLine2.LineFeePercentAsString);
		}

		[TestDate(2008, 12, 31)]
		public void TestDutyPercentAsString()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5203003000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Now;
			tariff.UE_SPICode = "AU";

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_AdValoremSpecialRate = 9999.99990000m;
			dutyRate.UD_ISOCountryCode = "AU";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "AU";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine5 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine6 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine7 = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
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

			CusEntryLine entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];
			CusEntryLine entryLine2 = declaration.ActiveEntryHeaders[0].MergedLines[1];
			CusEntryLine entryLine3 = declaration.ActiveEntryHeaders[0].MergedLines[2];
			CusEntryLine entryLine4 = declaration.ActiveEntryHeaders[0].MergedLines[3];
			CusEntryLine entryLine5 = declaration.ActiveEntryHeaders[0].MergedLines[4];
			CusEntryLine entryLine6 = declaration.ActiveEntryHeaders[0].MergedLines[5];
			CusEntryLine entryLine7 = declaration.ActiveEntryHeaders[0].MergedLines[6];

			AssertEquals("DutyPercentAsString", "9.6%", entryLine1.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "1.7c/KG", entryLine2.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "Free", entryLine3.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "$1.035/KG + 17%", entryLine4.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "28.8c/GR + 4.6%", entryLine5.CL_DutyPercentAsString);
			AssertEquals("DutyPercentAsString", "24.3c/KG", entryLine6.CL_DutyPercentAsString);

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               49928                " +
"10A888891-01319900091-013199000                 8         XJ5 7002809301891  WA " +
"20     HAKUBA MARU         112704041310B00153323            345  041310W138     " +
"22            OB38478     WICC09288374            00000015PK         YMLUUSNW   " +
"30                                  0               2042310             YMLU    " +
"40001KR00000010000000015000                    000000000555976                  " +
"50 20019038000000009600000001500000KG                               SG123108N   " +
"51                                                                              " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000125                                                         " +
"62          49900000210                                                         " +
"40002KR00000235000000000300                    000000011155976                  " +
"50 11051000000000000510000000030000KG                               SG123108N   " +
"51                                                                              " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100002938                                                         " +
"62          49900004935                                                         " +
"62110510000009000000142                                                         " +
"40003KR00000005000000000100                    000000000255976                  " +
"50 0901210060          000000010000KG                               SG123108N   " +
"51                                                                              " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000063                                                         " +
"62          49900000105                                                         " +
"40004KR00000001000000000100                              55976                  " +
"50 04031050000000012050000000010000KG                               SG123108N   " +
"51                                                                              " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000013                                                         " +
"62          49900000021                                                         " +
"40005KR00000050000000000000                    000000002455976                  " +
"50 96151130000000025880000000010000GR                               SG123108N   " +
"51                                                                              " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000625                                                         " +
"62          49900001050                                                         " +
"40006AU00000000000000000000                              55976                  " +
"50 99135220  0000041600000000200000KG                               AU123108NAU " +
"51                                                                              " +
"60                                        KRJUHCOR3325KIM                       " +
"40007AU00000050000000002000                    000000002455976                  " +
"50 5203003000          000000200000KG                               AU123108NAU " +
"51                                                                              " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000625                                                         " +
"89501000000043894990000000632109000000000142                                    " +
"9000000089640           0                       0000001085200000035100          " +
"Y  8888XJ5EI00046000000089640";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(declaration.ActiveEntryHeaders.EntrySummaryEntry, outMsg, inMsg, null);

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
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "GB";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			invoiceLine1.JI_Tariff = "6402917060";
			invoiceLine1.JI_LinePrice = 31500m;
			invoiceLine1.JI_CustomsQuantity = 9000m;
			invoiceLine1.JI_Weight = 5000m;
			invoiceLine1.US_UC_NKCountryOfOrigin = "GB";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			Factory.Save();

			CusEntryLine entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];

			AssertEquals("DutyPercentAsString", "90c/PRS + 37.5%", entryLine1.CL_DutyPercentAsString);

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               49928                " +
"10A888891-01319900091-013199000                 8         XJ5 7002809301891  WA " +
"20     HAKUBA MARU         112704041310B00153323            345  041310W138     " +
"22            OB38478     WICC09288374            00000015PK         YMLUUSNW   " +
"30                                  0               2042310             YMLU    " +
"40001KR00000010000000015000                    000000000555976                  " +
"50 64029170600000009600000001500000PRS                              GB123108N   " +
"51                                                                              " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000125                                                         " +
"62          49900000210                                                         " +
"89501000000043894990000000632109000000000142                                    " +
"9000000089640           0                       0000001085200000035100          " +
"Y  8888XJ5EI00046000000089640";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);

			AssertEquals("DutyPercentAsString calculated from message on 7501", "90c/PRS + 37.5%", printBO.EntryPrintLines[0].DutyPercentAsString);
		}

		[TestDate(2010, 04, 16)]
		public void TestDutyPercentAsStringWhenMsgDoesNotMatchEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_RL_NKPortOfLoading = "SGSIN";
			declaration.JE_ExportDate = new ZDateTime(2010, 04, 07);
			declaration.US_EntryDate = new ZDateTime(2010, 04, 15);
			declaration.US_PaymentType = "3";
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2010, 04, 28);

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2554.40m;
			invoice.JZ_IncoTerm = "FOB";
			invoice.JZ_RX_NKInvoice_Currency = "USD";
			invoice.JZ_InvoiceCurrExRate = 1m;
			invoice.US_UC_NKCountryOfOrigin = "SG";
			invoice.US_UC_NKCountryOfExport = "KR";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine4 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine5 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine6 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine7 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine8 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine9 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine10 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine11 = declaration.InvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
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

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CreateDocPrintingDetails(message.PK);

			CusEntryLine entryLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];
			CusEntryLine entryLine2 = declaration.ActiveEntryHeaders[0].MergedLines[1];
			CusEntryLine entryLine3 = declaration.ActiveEntryHeaders[0].MergedLines[2];
			CusEntryLine entryLine4 = declaration.ActiveEntryHeaders[0].MergedLines[3];
			CusEntryLine entryLine5 = declaration.ActiveEntryHeaders[0].MergedLines[4];
			CusEntryLine entryLine6 = declaration.ActiveEntryHeaders[0].MergedLines[5];

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
			CusEntryLine entryLine7 = declaration.ActiveEntryHeaders[0].MergedLines[6];
			CusEntryLine entryLine8 = declaration.ActiveEntryHeaders[0].MergedLines[7];
			CusEntryLine entryLine9 = declaration.ActiveEntryHeaders[0].MergedLines[8];
			CusEntryLine entryLine10 = declaration.ActiveEntryHeaders[0].MergedLines[9];
			CusEntryLine entryLine11 = declaration.ActiveEntryHeaders[0].MergedLines[10];

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

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			EDIMessage outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               49991                " +
"10A888891-01319900091-013199000                 8         XJ5 7002832501891  WA " +
"20     JADE TRADER         112704041510B00153357            345  041510W138     " +
"22            OB33745     WICC09288374            00000015PK         YMLUUSNW   " +
"30                                  0               2042710             YMLU    " +
"40001KR00000008120000000101                    000000005355976                  " +
"50 61099080300000012992000000001600DOZ000000010100KG                SG040710N   " +
"51                  838                                                         " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000102                                                         " +
"62          49900000171                                                         " +
"40002KR00000000780000000005                    000000000555976                  " +
"50 61046980380000000437000000000100DOZ000000000500KG                SG040710N   " +
"51                  847                                                         " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000010                                                         " +
"62          49900000016                                                         " +
"40003KR00000006370000000090                    000000004155976                  " +
"50 61046980400000003567000000001500DOZ000000009000KG                SG040710N   " +
"51                  847                                                         " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000080                                                         " +
"62          49900000134                                                         " +
"40004KR00000008980000000129                    000000005855976                  " +
"50 61061000100000017691000000002100DOZ000000012900KG                SG040710N   " +
"51                  339                                                         " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000112                                                         " +
"62          49900000189                                                         " +
"40005KR00000000680000000003                    000000000455976                  " +
"50 61159690200000000993000000000100DPR000000000300KG                SG040710N   " +
"51                  632                                                         " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000009                                                         " +
"62          49900000014                                                         " +
"40006KR00000000610000000014                    000000000455976                  " +
"50 61171060200000000580000000000200DOZ000000001400KG                SG040710N   " +
"51                  859                                                         " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000008                                                         " +
"62          49900000013                                                         " +
"895010000000032149900000002500                                                  " +
"9000000036260           0                       0000000282100000002554          " +
"Y  8888XJ5EI00042000000036260";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);
			Factory.Save();

			AssertEquals("Entry lines to print when printing from message should be 6", 6, printBO.EntryPrintLines.Count);
			AssertEquals("DutyPercentAsString calculated from message on 7501 should be values from entry when Msg was built", "16%", printBO.EntryPrintLines[0].DutyPercentAsString);
			AssertEquals("DutyPercentAsString calculated from message on 7501", "5.6%", printBO.EntryPrintLines[1].DutyPercentAsString);
			AssertEquals("DutyPercentAsString calculated from message on 7501", "5.6%", printBO.EntryPrintLines[2].DutyPercentAsString);
			AssertEquals("DutyPercentAsString calculated from message on 7501", "Free", printBO.EntryPrintLines[3].DutyPercentAsString);
			AssertEquals("DutyPercentAsString calculated from message on 7501", "Free", printBO.EntryPrintLines[4].DutyPercentAsString);
			AssertEquals("DutyPercentAsString calculated from message on 7501", "Free", printBO.EntryPrintLines[5].DutyPercentAsString);
		}

		[TestDate(2011, 03, 28)]
		public void TestDutyRatePrintsForTIBEntry()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;

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

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();

			message.EM_MessageText = "B018888XJ5EI                                               56574                " +
"10A110123-13063440023-130634400                 8         XJ5 7003775523891  PA " +
"20                         414701032711B00155526            219  042711B815     " +
"22            12565981930                         00000100CS                    " +
"30                                  01              2050611             BA      " +
"40001GB00001858800000000068                    0000000105                       " +
"50 98130050                                                         GB032711N   " +
"51                                                                              " +
"60                                        AUABCEXP72ALE                         " +
"707113195080                       X                                            " +
"40002GB00000871310000000032                    0000000049                       " +
"50 98130020                                                         GB032711N   " +
"51                                                                              " +
"60                                        AUABCEXP72ALE                         " +
"707113192980                       X                                            " +
"90                      0                                  00000273011          " +
"Y  8888XJ5EI00015";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_MessageText = "B018888XJ5ER                                               56574                E08888XJ5 70037755B00155526ACCEPTED - RECORDS REQUIRED             23808        E08888XJ5 70037755B00155526CERT-RELEASE CERTIFIED VIA SUMMARY      238082A5     Y  8888XJ5ER00002";
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			var printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff line 1", "Free", printBO.EntryPrintLines[0].DutyPercentAsString);
			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - secondary tariff tariff line 1", "5.5%", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);
			AssertEquals("TIB entry should also print the Duty amount as if not for TIB - secondary tariff line 1", 10223.40m, printBO.EntryPrintLines[0].SecondaryLine1DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff line 2", "Free", printBO.EntryPrintLines[1].DutyPercentAsString);
			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - secondary tariff line 2", "5.5%", printBO.EntryPrintLines[1].SecondaryLine1DutyPercentAsString);
			AssertEquals("TIB entry should also print the Duty amount as if not for TIB - secondary tariff line 2", 4792.21m, printBO.EntryPrintLines[1].SecondaryLine1DutyAmount);

			AssertEquals("TIB Duty", 15015.61m, printBO.TIBTotalDuty);
			AssertEquals("TIB Bond Charge", 17050.67m, printBO.TIBBondChg);
		}

		[TestDate(2011, 03, 28)]
		public void TestDutyRateAndAmountPrintsForTIBSetEntry()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;

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
			invoiceLine1.US_SecondarySPI = "X";
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
			invoiceLine2.US_SecondarySPI = "V";
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
			invoiceLine3.US_SecondarySPI = "V";
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
			invoiceLine4.US_SecondarySPI = "V";
			invoiceLine4.US_UC_NKCountryOfExport = "CH";
			invoiceLine4.JI_CountryOfOrigin = "CH";
			invoiceLine4.US_DestinationState = "IL";

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();

			message.EM_MessageText = "B018888XJ5EI                                               56601                " +
"10A888891-013199000                             8         XJ5 7003782123891  IL " +
"20     CAPE SCOTT          103901032811B00155537            18S  112308I217     " +
"22            OB934578                            00000500CT         APLU       " +
"30                                  0               2040611             APLU    " +
"40001CH00000050000000002500                    000000050060267                  " +
"50 98130020                                                         CH032811N  X" +
"51                                                                              " +
"60                                        AUABCEXP72ALE                         " +
"62          50100000625                                                         " +
"701902194000           000000000300KG                                           " +
"40002CH00000013000000000650                              60267                  " +
"50 98130020                                                         CH032811N  V" +
"51                                                                              " +
"60                                        AUABCEXP72ALE                         " +
"700712311000           000000000300KG                                           " +
"40003CH00000013000000000650                              60267                  " +
"50 98130020                                                         CH032811N  V" +
"51                                                                              " +
"60                                        AUABCEXP72ALE                         " +
"702002908020           000000000300KG                                           " +
"40004CH00000024000000001200                              60267                  " +
"50 98130020                                                         CH032811N  V" +
"51                                                                              " +
"60                                        AUABCEXP72ALE                         " +
"701902194000           000000000300KG                                           " +
"8950100000000625                                                                " +
"90                      0                       0000000062500000005000          " +
"Y  8888XJ5EI00027";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_MessageText = "B018888XJ5ER                                               56601                E08888XJ5 70037755B00155526ACCEPTED - RECORDS REQUIRED             23808        E08888XJ5 70037755B00155526CERT-RELEASE CERTIFIED VIA SUMMARY      238082A5     Y  8888XJ5ER00002";
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			var printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);

			AssertEquals("X line - TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "Free", printBO.EntryPrintLines[0].DutyPercentAsString);
			AssertEquals("X line - TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "6.4%", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);
			AssertEquals("X line - TIB entry should also print the Duty amount as if not for TIB", 320m, printBO.EntryPrintLines[0].SecondaryLine1DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff", "Free", printBO.EntryPrintLines[1].DutyPercentAsString);
			AssertEquals("For 'V' lines, there is no duty rate for this tariff", "", printBO.EntryPrintLines[1].SecondaryLine1DutyPercentAsString);
			AssertEquals("For 'V' lines, there is no duty amount for this tariff", 0m, printBO.EntryPrintLines[1].SecondaryLine1DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff", "Free", printBO.EntryPrintLines[2].DutyPercentAsString);
			AssertEquals("For 'V' lines, there is no duty rate for this tariff", "", printBO.EntryPrintLines[2].SecondaryLine1DutyPercentAsString);
			AssertEquals("For 'V' lines, there is no duty amount for this tariff", 0m, printBO.EntryPrintLines[2].SecondaryLine1DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff", "Free", printBO.EntryPrintLines[3].DutyPercentAsString);
			AssertEquals("For 'V' lines, there is no duty rate for this tariff", "", printBO.EntryPrintLines[3].SecondaryLine1DutyPercentAsString);
			AssertEquals("For 'V' lines, there is no duty amount for this tariff", 0m, printBO.EntryPrintLines[3].SecondaryLine1DutyAmount);

			AssertEquals("TIB Total Duty", 320m, printBO.TIBTotalDuty);
			AssertEquals("TIB Bond Charge", 379.50m, printBO.TIBBondChg);
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
			invoice.JZ_InvoiceAmount = 25000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "AU";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			invoiceLine1.US_SupTariff = "1010101010";
			invoiceLine1.JI_Tariff = "9802008068";
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_CustomsQuantity = 15000m;

			invoiceLine2.US_SupTariff = "2010101010";
			invoiceLine2.JI_Tariff = "9802008068";
			invoiceLine2.JI_LinePrice = 23500m;
			invoiceLine2.JI_CustomsQuantity = 300m;

			invoiceLine3.JI_Tariff = "0901210060";
			invoiceLine3.JI_LinePrice = 500m;
			invoiceLine3.JI_CustomsQuantity = 100m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();

			Factory.Save();

			var outMsg = message;
			outMsg.EM_MessageText = "B018888XJ5EI                                               49928                " +
"10A888891-01319900091-013199000                 8         XJ5 7002809301891  WA " +
"20     HAKUBA MARU         112704041310B00153323            345  041310W138     " +
"22            OB38478     WICC09288374            00000015PK         YMLUUSNW   " +
"30                                  0               2042310             YMLU    " +
"40001KR00000010000000015000                    000000000555976                  " +
"50 10101010100000009600000001500000KG                               SG123108N   " +
"51                                                                              " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000125                                                         " +
"62          49900000210                                                         " +
"709802008068           000000000100NO                                           " +
"40002KR00000235000000000300                    000000011155976                  " +
"50 20101010100000000510000000030000KG                               SG123108N   " +
"51                                                                              " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100002938                                                         " +
"62          49900004935                                                         " +
"62201010101009000000142                                                         " +
"709802008068           000000000100NO                                           " +
"40003KR00000005000000000100                    000000000255976                  " +
"50 9802008068          000000010000KG                               SG123108N   " +
"51                                                                              " +
"60                                        KRJUHCOR3325KIM                       " +
"62          50100000063                                                         " +
"62          49900000105                                                         " +
"89501000000043894990000000632109000000000142                                    " +
"9000000089640           0                       0000001085200000035100          " +
"Y  8888XJ5EI00046000000089640";
			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			var printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);

			Assert(printBO.EntryPrintLines[0].PrintSPIOnSecondLine);
			Assert(!printBO.EntryPrintLines[1].PrintSPIOnSecondLine);
			Assert(!printBO.EntryPrintLines[2].PrintSPIOnSecondLine);
		}

		[TestDate(2011, 03, 28)]
		public void TestDutyRateAndAmountPrintsForTIBWatchEntry()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;

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

			var builder = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();
			var entry = declaration.CustomsEntryHeaders[0];

			var outMsg = message;
			outMsg.EM_MessageText = @"B018888XJ5EI                                               56644                " +
"10A110123-13063440023-130634400                 8         XJ5 7003790423891  PA " +
"20                         401101032911B00155561            310  032911C001     " +
"22            08133428216                         00000001PK                    " +
"30                                  0               2040711             QF      " +
"40001AU00000001000000000001                    0000000031                       " +
"50 9813000540                      X                                AU032911N   " +
"51                                                                              " +
"60                                        AUABCEXP72ALE                         " +
"709101114010           000000000100NO                                           " +
"809101114020           000000000100NO                               0000000100  " +
"819101114030           000000000100NO                               0000000100  " +
"819101114040           000000000100NO                               0000000100  " +
"90                      0                                  00000000400          " +
"Y  8888XJ5EI00013";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_MessageText = "B018888XJ5ER                                               56644                E08888XJ5 70037904B00155561ACCEPTED - RECORDS REQUIRED             23808        Y  8888XJ5ER00001";
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			var printBO = new EntryMessageENS7501Print(entry, outMsg, inMsg, null);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "Free", printBO.EntryPrintLines[0].DutyPercentAsString);
			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - tariff ", "51c/NO", printBO.EntryPrintLines[0].SecondaryLine1DutyPercentAsString);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - SecondaryLine2", "6.25%", printBO.EntryPrintLines[0].SecondaryLine2DutyPercentAsString);
			AssertEquals("TIB entry should also print the Duty amount as if not for TIB", 6.25m, printBO.EntryPrintLines[0].SecondaryLine2DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - SecondaryLine3", "6.25%", printBO.EntryPrintLines[0].SecondaryLine3DutyPercentAsString);
			AssertEquals("TIB entry should also print the Duty amount as if not for TIB - SecondaryLine3", 6.25m, printBO.EntryPrintLines[0].SecondaryLine3DutyAmount);

			AssertEquals("TIB entry should print normal Duty Rate as if entry was not for TIB - SecondaryLine4", "5.3%", printBO.EntryPrintLines[0].SecondaryLine4DutyPercentAsString);
			AssertEquals("TIB entry should also print the Duty amount as if not for TIB - SecondaryLine4", 5.30m, printBO.EntryPrintLines[0].SecondaryLine4DutyAmount);
		}

		[TestDate(2011, 03, 28)]
		public void TestLargeCustomsValuePrints()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-LV";
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_InvoiceAmount = 9200000000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.US_UC_NKCountryOfExport = "GB";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "9801001075";
			invoiceLine1.JI_LinePrice = 9200000000m;

			Factory.Save();
			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false);
			var message = builder.PopulateMessage();
			Factory.Save();

			message.EM_MessageText = "B013901SV9EI                                               115610               " +
"10A390113-14792700013-147927000                 8         SV9 7002473001037  IL " +
"20                         403901122811B00156933            001  122811I317     " +
"22            00101222550                         00000001PC                    " +
"30                                  01              3011012             AA      " +
"40001US92000000000000246700                    0000015000                       " +
"50 9801001075                      X                                CH122811N   " +
"60                                        CHHARWIN8PLA                          " +
"90                      0                                  09200000000          " +
"Y  3901SV9EI00008";

			var inMsg = Factory.NewWithValidTestData<MQEDIMessage>();
			inMsg.EM_MessageText = "B013901SV9ER                                               115610               10A390113-14792700013-147927000                 8         SV9 7002473001037  IL 40001US92000000000000246700000000000000000000000000015000                       E403901SV9 7002473000127H01   *CENSUS* GROSS WEIGHT - AIR              B0015693350 9801001075                      X                                CH122811N   E503901SV9 7002473000127P01   *CENSUS* MAXIMUM VALUE EXCEEDED          B001569339000000000000000000000000 00000000000000000000000000000000009200000000          E903901SV9 70024730   58401322ENT-SUM ACCEPTED WITH WARNINGS           B00156933E03901SV9 70024730B00156933CERT-RELEASE CERTIFIED VIA SUMMARY      013222A5     Y  3901SV9ER00008";
			inMsg.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			inMsg.EM_LinkUniqueID = entry.PK;
			EntryMessageENS7501Print printBO = new EntryMessageENS7501Print(entry, message, inMsg, null);

			AssertEquals("TotalLinePriceInLocalCurrencyRounded", 9200000000m, printBO.EntryPrintLines[0].TotalLinePriceInLocalCurrencyRounded);
			AssertEquals("TotalLinePriceInLocalCurrencyRounded should print without decimals", "9200000000", printBO.EntryPrintLines[0].TotalLinePriceInLocalCurrencyRounded.ToString());
			AssertEquals("Total Entered Value", 9200000000m, printBO.TotalEnteredValue);
		}

		public void TestPortOfLadingForLine()
		{
			var messageLine = new EntryMessageLine(Factory);
			ENS40 ens40 = new ENS40();
			ens40.Deserialise("40001CH00000004000000000015                              52051                  ");
			messageLine.ens40 = ens40;

			var ens7501Line = new EntryMessageENS7501Line(messageLine, false, false, true, false);
			AssertEquals("52051", ens7501Line.PortOfLadingForLine);

			ens7501Line = new EntryMessageENS7501Line(messageLine, false, false, false, false);
			AssertEquals(ZString.Empty, ens7501Line.PortOfLadingForLine);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var printLineObject = new EntryMessageLine(Factory);
			return new EntryMessageENS7501Line(printLineObject, false, false, false, false);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			GlbBranch.CurrentBranch.SetCountry("US");
			DeclarationTestHelper.SetEntryFilerCode("XJ6");
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

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-0089";
			invoiceHeader.JZ_InvoiceAmount = 2683.80m;
			invoiceHeader.JZ_IncoTerm = "FOB";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "AU";
			invoiceHeader.US_UC_NKCountryOfExport = "AU";
			invoiceHeader.US_TransactionsRelated = "Y";

			InvoiceCharge freightCharge = invoiceHeader.Charges.AddNew();
			freightCharge.J7_Amount = 59m;
			freightCharge.J7_RX_NKCurrency = "USD";
			freightCharge.J7_ChargeDescription = "OVERSEAS FREIGHT";
			freightCharge.J7_ChargeType = "OFT";
			freightCharge.J7_DistributeBy = "VAL";

			using (Declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
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

				JobComInvoiceLine invoiceLine3 = Declaration.InvoiceLines.AddNew();
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

		EntryMessageLine entryMessageLine
		{
			get
			{
				if (fentryMessageLine == null)
				{
					fentryMessageLine = new EntryMessageLine(Factory);
					ENS40 ens40 = new ENS40();
					ENS43 ens43_1 = new ENS43();
					ENS43 ens43_2 = new ENS43();
					List<ENS43> ens43RulingList = new List<ENS43>();
					ENS50 ens50 = new ENS50();
					ENS51 ens51 = new ENS51();
					ENS52 ens52 = new ENS52();
					ENS60 ens60 = new ENS60();
					ENS62 ens62 = new ENS62();
					List<ENS62> ens62ChargesList = new List<ENS62>();

					ens40.Deserialise("40001AU00000000000000009000                    000000005060267                  ");
					ens43_1.Deserialise("43123456R                                                                       ");
					ens43_2.Deserialise("43      D SWITCHES                                                              ");
					ens50.Deserialise("50K8211100000          000000400000PCS                              AU071407Y  S");
					ens51.Deserialise("51                                                                              ");
					ens52.Deserialise("52                                    0100000037501Y00000000015F                ");
					ens60.Deserialise("60                                        XYBEREQU6LON                          ");
					ens62.Deserialise("62          49900005040                                                         ");
					ens62.Deserialise("62          10900000943                                                         ");

					fentryMessageLine.ens40 = ens40;
					ens43RulingList.Add(ens43_1);
					ens43RulingList.Add(ens43_2);
					fentryMessageLine.ens43 = ens43RulingList;
					fentryMessageLine.ens50 = ens50;
					fentryMessageLine.ens51 = ens51;
					fentryMessageLine.ens52 = ens52;
					fentryMessageLine.ens60 = ens60;
					ens62ChargesList.Add(ens62);
					fentryMessageLine.ens62 = ens62ChargesList;

					JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
					JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
				}

				return fentryMessageLine;
			}
		}
		EntryMessageLine fentryMessageLine;

		EntryMessageLine exciseMessageLine
		{
			get
			{
				if (fexciseMessageLine == null)
				{
					fexciseMessageLine = new EntryMessageLine(Factory);
					ENS40 ens40 = new ENS40();
					ENS50 ens50 = new ENS50();
					ENS51 ens51 = new ENS51();
					ENS52 ens52 = new ENS52();
					ENS60 ens60 = new ENS60();
					ENS62 ens62HMF = new ENS62();
					ENS62 ens62MPF = new ENS62();
					List<ENS62> ens62ChargesList = new List<ENS62>();

					ens40.Deserialise("40001AU00000858050000004867                                                     ");
					ens50.Deserialise("50S22042150300000050066000000794700L                                AU052109N   ");
					ens51.Deserialise("51                                                                              ");
					ens60.Deserialise("60                                                        0000224631            ");
					ens62HMF.Deserialise("62          50100010726                                                         ");
					ens62MPF.Deserialise("62          49900018019                                                         ");

					fexciseMessageLine.ens40 = ens40;
					fexciseMessageLine.ens50 = ens50;
					fexciseMessageLine.ens51 = ens51;
					fexciseMessageLine.ens60 = ens60;
					ens62ChargesList.Add(ens62HMF);
					ens62ChargesList.Add(ens62MPF);
					fexciseMessageLine.ens62 = ens62ChargesList;

					JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
					JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
				}

				return fexciseMessageLine;
			}
		}
		EntryMessageLine fexciseMessageLine;

		EntryMessageLine MPFMessageLine
		{
			get
			{
				if (fMPFMessageLine == null)
				{
					fMPFMessageLine = new EntryMessageLine(Factory);
					ENS40 ens40 = new ENS40();
					ENS62 ens62HMF = new ENS62();
					ENS62 ens62MPF = new ENS62();
					List<ENS62> ens62ChargesList = new List<ENS62>();

					ens40.Deserialise("40001AU00000858050000004867                                                     ");
					ens62HMF.Deserialise("62          50100010726                                                         ");
					ens62MPF.Deserialise("62          49900000000                                                         ");

					fMPFMessageLine.ens40 = ens40;
					ens62ChargesList.Add(ens62HMF);
					ens62ChargesList.Add(ens62MPF);
					fMPFMessageLine.ens62 = ens62ChargesList;
				}

				return fMPFMessageLine;
			}
		}
		EntryMessageLine fMPFMessageLine;

		EntryMessageLine NOMPFMessageLine
		{
			get
			{
				if (fNOMPFMessageLine == null)
				{
					fNOMPFMessageLine = new EntryMessageLine(Factory);
					ENS40 ens40 = new ENS40();
					ENS62 ens62HMF = new ENS62();
					List<ENS62> ens62ChargesList = new List<ENS62>();

					ens40.Deserialise("40001AU00000858050000004867                                                     ");
					ens62HMF.Deserialise("62          50100010726                                                         ");

					fNOMPFMessageLine.ens40 = ens40;
					ens62ChargesList.Add(ens62HMF);
					fNOMPFMessageLine.ens62 = ens62ChargesList;
				}

				return fNOMPFMessageLine;
			}
		}
		EntryMessageLine fNOMPFMessageLine;

		EntryMessageLine watchMessageLine
		{
			get
			{
				if (fwatchMessageLine == null)
				{
					fwatchMessageLine = new EntryMessageLine(Factory);
					ENS40 ens40 = new ENS40();
					ENS50 ens50 = new ENS50();
					ENS60 ens60 = new ENS60();
					ENS70 ens70 = new ENS70();
					ENS80 ens80 = new ENS80();
					ENS81 ens81 = new ENS81();
					List<ENS81> ens81SecondaryTariffList = new List<ENS81>();

					ens40.Deserialise("40001AU00000018520000000062                    000000110060267                  ");
					fwatchMessageLine.ens40 = ens40;
					ens50.Deserialise("50 9802004040                                                       AU111108Y   ");
					fwatchMessageLine.ens50 = ens50;

					//fwatchMessageLine.ens51 = ens51;
					ens60.Deserialise("60                                        US8495956                             ");
					fwatchMessageLine.ens60 = ens60;
					ens70.Deserialise("709102111010 0000012871            NO                               0000003406  ");
					fwatchMessageLine.ens70 = ens70;
					ens80.Deserialise("809802004040                                                        0000001010  ");
					fwatchMessageLine.ens80 = ens80;
					ens81.Deserialise("819102111020 0000006080            NO                               0000001609  ");
					ens81SecondaryTariffList.Add(ens81);
					ens81.Deserialise("819802004040                                                                    ");
					ens81SecondaryTariffList.Add(ens81);
					ens81.Deserialise("819102111030 0000005083            NO                               0000001345  ");
					ens81SecondaryTariffList.Add(ens81);
					ens81.Deserialise("819802004040                                                        0000000204  ");
					ens81SecondaryTariffList.Add(ens81);
					ens81.Deserialise("819102111040                       NO                                           ");
					ens81SecondaryTariffList.Add(ens81);

					fwatchMessageLine.ens81 = ens81SecondaryTariffList;
				}

				return fwatchMessageLine;
			}
		}
		EntryMessageLine fwatchMessageLine;

		EntryMessageLine ADDCVDMessageLine
		{
			get
			{
				if (fADDCVDMessageLine == null)
				{
					fADDCVDMessageLine = new EntryMessageLine(Factory);
					ENS40 ens40 = new ENS40();
					ENS50 ens50 = new ENS50();
					ENS51 ens51 = new ENS51();
					ENS52 ens52 = new ENS52();
					ENS60 ens60 = new ENS60();
					ENS62 ens62HMF = new ENS62();
					ENS62 ens62MPF = new ENS62();
					List<ENS62> ens62ChargesList = new List<ENS62>();
					ENS70 ens70 = new ENS70();

					ens40.Deserialise("40001CN0000016000000002000000000035000000002000                                 ");
					ens50.Deserialise("50 82111000000000062000000000400000PCS                              CN061109N   ");
					ens51.Deserialise("51                                                                              ");
					ens60.Deserialise("600000024300C427819000A5702040060000662795                          012151893700");
					ens62HMF.Deserialise("62          50100002000                                                         ");
					ens62MPF.Deserialise("62          49900003360                                                         ");
					ens70.Deserialise("708205203000           000001600000DOZ                                          ");

					fADDCVDMessageLine.ens40 = ens40;
					fADDCVDMessageLine.ens50 = ens50;
					fADDCVDMessageLine.ens51 = ens51;
					fADDCVDMessageLine.ens60 = ens60;
					ens62ChargesList.Add(ens62HMF);
					ens62ChargesList.Add(ens62MPF);
					fADDCVDMessageLine.ens62 = ens62ChargesList;
					fADDCVDMessageLine.ens70 = ens70;
				}

				return fADDCVDMessageLine;
			}
		}
		EntryMessageLine fADDCVDMessageLine;

		EntryMessageLine spiEntryMessageLine
		{
			get
			{
				if (fspiEntryMessageLine == null)
				{
					fspiEntryMessageLine = new EntryMessageLine(Factory);
					ENS40 ens40 = new ENS40();

					ENS50 ens50 = new ENS50();
					ENS51 ens51 = new ENS51();
					ENS52 ens52 = new ENS52();
					ENS60 ens60 = new ENS60();
					ENS62 ens62 = new ENS62();
					List<ENS62> ens62ChargesList = new List<ENS62>();
					ENS70 ens70 = new ENS70();

					ens40.Deserialise("40001BH00000020000000000100                    0000000100                       ");
					ens50.Deserialise("50 99149920                        KG                               BH041510NBH ");
					ens51.Deserialise("51                  648                                                         ");
					ens60.Deserialise("60                                        BHTEXGUY1ALH                          ");
					ens62.Deserialise("62          49900000420                                                         ");
					ens70.Deserialise("706204633510           000000001000DOZ000000009900KG                          BH");

					fspiEntryMessageLine.ens40 = ens40;
					fspiEntryMessageLine.ens50 = ens50;
					fspiEntryMessageLine.ens51 = ens51;
					fspiEntryMessageLine.ens52 = ens52;
					fspiEntryMessageLine.ens60 = ens60;
					ens62ChargesList.Add(ens62);
					fspiEntryMessageLine.ens62 = ens62ChargesList;
					fspiEntryMessageLine.ens70 = ens70;
				}

				return fspiEntryMessageLine;
			}
		}
		EntryMessageLine fspiEntryMessageLine;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableENS = true;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		void SetUpMergedInvoices(string tariffNo)
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A588845000";
			addCase.U5_ISOCountryCode = "JP";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "7220209060";
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "7219130051";
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "7220206080";
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "7219140065";
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "7219130081";
			var addRate = addCase.CaseRates.AddNew();
			addRate.U6_AdValoremRate = 0.40m;
			addRate.U6_EffectiveDate = ZDateTime.Today;

			USCACCase cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C427819000";
			cvdCase.U5_ISOCountryCode = "FR";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "2844200030";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 0.12m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			Factory.Save();

			JobComInvoiceHeader invoice1 = Declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV001";
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceAmount = 123620m;
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = tariffNo;
			line1.JI_InvoiceQuantity = 7947;
			line1.JI_InvoiceUQ = "L";
			line1.JI_LinePrice = 123620m;

			if (tariffNo == "7318160060")
			{
				line1.JI_PartNo = "0-423-55-500-2";
				var line2 = invoice1.JobComInvoiceLines.AddNew();
				line2.JI_Tariff = tariffNo;
				line2.JI_InvoiceQuantity = 100;
				line2.JI_InvoiceUQ = "L";
				line2.JI_LinePrice = 5000m;
				line2.JI_PartNo = "0-02-90-124-0";
			}

			if (tariffNo == "2402103030")
			{
				line1.US_UC_NKCountryOfOrigin = "AU";
				line1.US_SPI = "";
				line1.JI_CustomsQuantity = 1000m;
				line1.JI_CustomsSecondQuantity = 1360m;
				line1.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			}

			if (tariffNo == "8211100000")
			{
				Declaration.US_ADDCVDSuretyCode = "444";

				JobComInvoiceLine childInvoiceLine = line1.InvoiceHeader.InvoiceLines.AddNew();
				childInvoiceLine.JI_ParentID = line1.PK;
				childInvoiceLine.JI_Tariff = "8205203000";
				childInvoiceLine.JI_Weight = 10m;
				childInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				childInvoiceLine.JI_LinePrice = 10000m;
				childInvoiceLine.US_ADDCaseNo = "A570204006";
				childInvoiceLine.US_CVDCaseNo = "C427819000";
			}

			if (tariffNo == "8211100010")
			{
				AddSecondaryLineWithCVD(line1);
			}

			if (tariffNo == "7220209060")
			{
				Declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
				line1.US_SupTariff = "9813000540";
				line1.JI_CustomsQuantity = 200m;
				line1.JI_CustomsUnitQty = "KG";
				line1.JI_LinePrice = 4500m;
				line1.JI_CountryOfOrigin = "JP";
				line1.US_UC_NKCountryOfExport = "JP";
				line1.US_ADDCaseNo = "A588845000";
				line1.US_ADDDepositRateIndicator = "1";
			}

			rateStrings = new ZString[8];

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
		}

		void AddSecondaryLineWithCVD(JobComInvoiceLine parentLine)
		{
			JobComInvoiceLine childInvoiceLine = parentLine.InvoiceHeader.InvoiceLines.AddNew();
			childInvoiceLine.JI_ParentID = parentLine.PK;
			childInvoiceLine.JI_Tariff = "2844200050";
			childInvoiceLine.JI_Weight = 10m;
			childInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			childInvoiceLine.JI_LinePrice = 10000m;
			childInvoiceLine.JI_CustomsSecondQuantity = 100m;
			childInvoiceLine.US_CVDCaseNo = "C427819000";
			childInvoiceLine.Declaration.US_ADDCVDSuretyCode = "530";
		}

		void AddEns81(List<ENS81> ens81s, ZDecimal secondaryQuantity)
		{
			ENS81 ens81 = new ENS81();
			ens81.Quantity2 = secondaryQuantity;
			ens81.Unit2 = Core.Constants.Weight.Kilograms;
			ens81s.Add(ens81);
		}

		US7501DocPrinting[] DocPrintingData
		{
			get
			{
				if (docPrintingData == null)
				{
					docPrintingData = new CachedProperty<US7501DocPrinting[]>(Factory,
						delegate
						{
							ZGuid eNSCusEntryHeader = Declaration.FormalEntry != null ? Declaration.FormalEntry.PK : ZGuid.Empty;
							ZQuery query = new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.US7501DocPrinting);
							query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, CusEntryHeaderSchema.Constants.Prefix);
							query.AddToFilter(CusAddInfoSchema.B7_ParentID, eNSCusEntryHeader);
							US7501DocPrinting[] docData = Factory.Load<US7501DocPrinting>(query);
							return docData;
						}
						);
				}

				return docPrintingData.Value;
			}
		}
		CachedProperty<US7501DocPrinting[]> docPrintingData;
		ZString[] rateStrings;
		US7501DocPrinting docData;
	}
}
