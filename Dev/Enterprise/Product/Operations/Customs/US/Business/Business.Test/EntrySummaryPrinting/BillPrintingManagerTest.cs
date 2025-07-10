using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BillPrintingManagerTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_MasterBill = "61325205";
			var masterBill = declaration.Bills[0];
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";
			masterBill.CU_PackType = "PLI";
			masterBill.CU_NoOfPacks = 26m;

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "OPLI";
			houseBill.CU_BillNum = "house2001";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			masterBill.US_UI_NKBillIssuerSCAC = "QWER";
			masterBill.CU_PackType = ZString.Empty;
			masterBill.CU_NoOfPacks = ZDecimal.Zero;
			houseBill.CU_PackType = "PK";
			houseBill.CU_NoOfPacks = 26m;
			declaration.US_ITDate = ZDateTime.Today.AddDays(1);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var action = (EntryHeaderMessageSendingAction)actions[0];

			var messageBuilder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Update, ACEEntrySummaryMessageSendingOption.New(action));
			var seMessageOutgoing = messageBuilder.PopulateMessage();

			var manager = new BillPrintingManager(entry, seMessageOutgoing, declaration.Factory, new EntrySummary7501BillCollection(Factory));
			manager.PopulateElements(message.MessageBlock.MessageBlocks.FindAll(x => x is AENS20 || x is AENS22 || x is AENS23));
			AssertEquals(ZDateTime.Empty, manager.FirstBillITDate);
			AssertEquals("", manager.FirstBillITNO);
			AssertEquals("SCAC and master bill number should come from ACE BLU message", "61325205", manager.SCACAndMBillNumber);

			var bluBuilder = new TestBillOfLadingUpdateBuilder(declaration);
			var bluMessage = bluBuilder.PopulateMessage();

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, bluMessage);
			AssertEquals("EntryPrintBills", 1, printBO.EntryPrintBills.Count);

			var bill1 = printBO.EntryPrintBills[0];
			AssertEquals(ZString.Empty, bill1.EffectiveHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill1.EffectiveMasterBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill1.EffectiveSubHouseBillIssuerSCAC);
			AssertEquals(ZString.Empty, bill1.ITNO);
			AssertEquals("61325205", bill1.MasterBill);
			AssertEquals("HOUSE2001", bill1.HouseBill);
			AssertEquals(ZString.Empty, bill1.SubHouseBill);
			AssertEquals(26, bill1.PkgQty);
			AssertEquals("PK", bill1.PkgType);
			AssertEquals(ZDateTime.Empty, bill1.ITDate);
		}

		public void TestSplitShipmentBillQuantityDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_MasterBill = "00161325205";

			var masterBill = declaration.Bills[0];
			masterBill.US_UI_NKBillIssuerSCAC = "APLU";
			masterBill.CU_PackType = "PLI";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.US_UI_NKBillIssuerSCAC = "OPLI";
			houseBill.CU_BillNum = "HB2001";
			houseBill.CU_NoOfPacks = 26m;
			houseBill.CU_PackType = "PK";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 100m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3601000000";
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			Factory.Save();

			houseBill.US_SESplitShip = true;
			var splitShipment1 = houseBill.ITAndSplitDetails.AddNew();
			splitShipment1.US_NoOfPacks = 29;

			var splitShipment2 = houseBill.ITAndSplitDetails.AddNew();
			splitShipment2.US_NoOfPacks = 35;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.CH_Status = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var action = (EntryHeaderMessageSendingAction)actions[0];

			var messageBuilder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Update, ACEEntrySummaryMessageSendingOption.New(action));
			var seMessageOutgoing = messageBuilder.PopulateMessage();
			Factory.Save();

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			var printBO = new ACEEntryMessage7501Print(entry, message, incoming7501, seMessageOutgoing);
			AssertEquals("EntryPrintBills", 1, printBO.EntryPrintBills.Count);

			var bill1 = printBO.EntryPrintBills[0];
			AssertEquals("00161325205", bill1.MasterBill);
			AssertEquals("HB2001", bill1.HouseBill);
			AssertEquals(64, bill1.PkgQty);
			AssertEquals("PK", bill1.PkgType);
		}

		public void TestMultipleMasterBillsWithVNumber()
		{
			var masterBill0 = CreateBillForTestITNumber(null, Enterprise.Customs.Business.BillTypeList.Codes.MasterBill, "00100100133");
			var masterBill1 = CreateBillForTestITNumber(null, Enterprise.Customs.Business.BillTypeList.Codes.MasterBill, "00145945340");

			var itNumber0 = CreateITAndSplitDetails(masterBill0, "V3423423424", 15);
			var itNumber1 = CreateITAndSplitDetails(masterBill1, "V4578978972", 20);

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader entry = null;
			var message = PopulateMessage(out entry);

			itNumber0.US_NoOfPacks = 38;
			itNumber1.US_NoOfPacks = 20;

			var print = GetACEEntryMessage7501Print(entry, message);
			AssertEqualsForTestITNumber(print.EntryPrintBills, new[] { itNumber0, itNumber1 });
		}

		public void TestSingleMasterBillWithVAndNormalNumbers()
		{
			var masterBill = CreateBillForTestITNumber(null, Enterprise.Customs.Business.BillTypeList.Codes.MasterBill, "249008808");

			var itNumber0 = CreateITAndSplitDetails(masterBill, "V1213213211", 20);
			var itNumber1 = CreateITAndSplitDetails(masterBill, "123952765", 10);

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader entry = null;
			var message = PopulateMessage(out entry);

			itNumber0.US_NoOfPacks = 49;

			var print = GetACEEntryMessage7501Print(entry, message);
			AssertEqualsForTestITNumber(print.EntryPrintBills, new[] { itNumber0, itNumber1 });
		}

		public void TestSingleMasterBillWithMultipleConventionalNumbers()
		{
			var masterBill = CreateBillForTestITNumber(null, Enterprise.Customs.Business.BillTypeList.Codes.MasterBill, "0124546546");

			var itNumber0 = CreateITAndSplitDetails(masterBill, "789650142", 10);
			var itNumber1 = CreateITAndSplitDetails(masterBill, "565698895", 10);

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader entry = null;
			var message = PopulateMessage(out entry);

			itNumber0.US_NoOfPacks = 30;
			itNumber1.US_NoOfPacks = 20;

			var print = GetACEEntryMessage7501Print(entry, message);
			AssertEqualsForTestITNumber(print.EntryPrintBills, new[] { CreateITAndSplitDetails(masterBill, "789650142", 10), CreateITAndSplitDetails(masterBill, "565698895", 10) });

			masterBill.CU_BillNum = "0124546547";
			print = GetACEEntryMessage7501Print(entry, message);
			AssertEqualsForTestITNumber(print.EntryPrintBills, new[] { CreateITAndSplitDetails(masterBill, ZString.Empty, 0) });
		}

		public void TestMasterAndHouseBillWithConventionalNumbers()
		{
			var masterBill = CreateBillForTestITNumber(null, Enterprise.Customs.Business.BillTypeList.Codes.MasterBill, "LNB34001");
			var houseBill = CreateBillForTestITNumber(masterBill, Enterprise.Customs.Business.BillTypeList.Codes.HouseBill, "LNHB001001");

			var itNumber0 = CreateITAndSplitDetails(houseBill, "008008000", 9);
			var itNumber1 = CreateITAndSplitDetails(houseBill, "005004016", 10);

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader entry = null;
			var message = PopulateMessage(out entry);

			itNumber0.US_NoOfPacks = 18;
			itNumber1.US_NoOfPacks = 12;

			var print = GetACEEntryMessage7501Print(entry, message);
			AssertEqualsForTestITNumber(print.EntryPrintBills, new[] { CreateITAndSplitDetails(houseBill, "008008000", 9), CreateITAndSplitDetails(houseBill, "005004016", 10) });

			houseBill.CU_BillNum = "LNHB001002";
			print = GetACEEntryMessage7501Print(entry, message);
			AssertEqualsForTestITNumber(print.EntryPrintBills, new[] { CreateITAndSplitDetails(houseBill, ZString.Empty, 0) });
		}

		public void TestMasterAndHouseBillWithVAndConventionalNumbers()
		{
			var masterBill = CreateBillForTestITNumber(null, Enterprise.Customs.Business.BillTypeList.Codes.MasterBill, "OY9893452");
			var houseBill = CreateBillForTestITNumber(masterBill, Enterprise.Customs.Business.BillTypeList.Codes.HouseBill, "F34243424");

			var itNumber0 = CreateITAndSplitDetails(houseBill, "V3842938440", 60);
			var itNumber1 = CreateITAndSplitDetails(houseBill, "000888882", 200);

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader entry = null;
			var message = PopulateMessage(out entry);

			itNumber0.US_NoOfPacks = 100;
			itNumber1.US_NoOfPacks = 100;

			var print = GetACEEntryMessage7501Print(entry, message);
			AssertEqualsForTestITNumber(print.EntryPrintBills, new[] { itNumber0, CreateITAndSplitDetails(houseBill, "000888882", 200) });
		}

		public void TestMultipleMasterHouseAndSubHouseBillsWithMultipleConventionalNumbers()
		{
			var masterBill0 = CreateBillForTestITNumber(null, Enterprise.Customs.Business.BillTypeList.Codes.MasterBill, "002");
			var masterBill1 = CreateBillForTestITNumber(null, Enterprise.Customs.Business.BillTypeList.Codes.MasterBill, "RT453453");
			var houseBill = CreateBillForTestITNumber(masterBill0, Enterprise.Customs.Business.BillTypeList.Codes.HouseBill, "HB002");
			var subHouseBill = CreateBillForTestITNumber(houseBill, Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill, "SHB002");

			var itNumber0 = CreateITAndSplitDetails(masterBill1, "234323445", 20);
			var itNumber1 = CreateITAndSplitDetails(subHouseBill, "780012774", 40);
			var itNumber2 = CreateITAndSplitDetails(subHouseBill, "478787783", 16);

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader entry = null;
			var message = PopulateMessage(out entry);

			itNumber2.US_NoOfPacks = 26;

			var print = GetACEEntryMessage7501Print(entry, message);
			AssertEqualsForTestITNumber(print.EntryPrintBills, new[] { itNumber0, itNumber1, CreateITAndSplitDetails(subHouseBill, "478787783", 16) });

			subHouseBill.CU_BillNum = "SHB022";
			print = GetACEEntryMessage7501Print(entry, message);
			AssertEqualsForTestITNumber(print.EntryPrintBills, new[] { itNumber0, CreateITAndSplitDetails(subHouseBill, ZString.Empty, 0) });
		}

		public void TestMultipleMasterHouseAndSubHouseBillsWithCombinationOfNormalAndVNumbers()
		{
			var masterBill0 = CreateBillForTestITNumber(null, Enterprise.Customs.Business.BillTypeList.Codes.MasterBill, "00100100F");
			var masterBill1 = CreateBillForTestITNumber(null, Enterprise.Customs.Business.BillTypeList.Codes.MasterBill, "HNGJH4353");
			var houseBill = CreateBillForTestITNumber(masterBill0, Enterprise.Customs.Business.BillTypeList.Codes.HouseBill, "HB001001");
			var subHouseBill = CreateBillForTestITNumber(houseBill, Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill, "CHB0011");

			var itNumber0 = CreateITAndSplitDetails(masterBill1, "V4564454566", 160);
			var itNumber1 = CreateITAndSplitDetails(subHouseBill, "342342346", 400);
			var itNumber2 = CreateITAndSplitDetails(subHouseBill, "V5465465460", 60);

			Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			CusEntryHeader entry = null;
			var message = PopulateMessage(out entry);

			itNumber0.US_NoOfPacks = 289;
			itNumber1.US_NoOfPacks = 402;
			itNumber2.US_NoOfPacks = 82;
			itNumber2.US_ITNumber = "V1122334455";

			var print = GetACEEntryMessage7501Print(entry, message);
			AssertEqualsForTestITNumber(print.EntryPrintBills, new[] { itNumber0, CreateITAndSplitDetails(subHouseBill, "342342346", 400), itNumber2 });
		}

		Bill CreateBillForTestITNumber(Bill parentBill, ZString billType, ZString billNum)
		{
			Bill bill = null;
			if (parentBill == null && billType == Customs.Business.BillTypeList.Codes.MasterBill)
			{
				bill = Declaration.Bills.AddNew();
				bill.CU_BillType = billType;
			}
			else if (parentBill != null && billType != Customs.Business.BillTypeList.Codes.MasterBill)
			{
				bill = parentBill.ChildBills.AddNew();
				bill.CU_BillType = billType;
			}

			bill.CU_BillNum = billNum;

			return bill;
		}

		ITAndSplitDetails CreateITAndSplitDetails(Bill parentBill, ZString itNumber, ZInt numberOfPacks)
		{
			var it = parentBill.ITAndSplitDetails.AddNew();
			it.US_ITNumber = itNumber;
			it.US_NoOfPacks = numberOfPacks;

			return it;
		}

		MQEDIMessage PopulateMessage(out CusEntryHeader entry)
		{
			entry = Declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			entry.CreateDocPrintingDetails(message.PK);

			return message;
		}

		ACEEntryMessage7501Print GetACEEntryMessage7501Print(CusEntryHeader entry, MQEDIMessage message, bool isCRLEnabled = false)
		{
			MQEDIMessage bluMessage = null;
			if (isCRLEnabled)
			{
				bluMessage = new SimplifiedEntryMessageBuilder(entry, UpdateActionCode.Update, null).PopulateMessage();
			}
			else
			{
				var bluBuilder = new TestBillOfLadingUpdateBuilder(Declaration);
				bluMessage = bluBuilder.PopulateMessage();
			}

			var incoming7501 = Factory.New<MQEDIMessage>();
			incoming7501.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incoming7501.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incoming7501.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			entry.Messages.Add(incoming7501);

			return new ACEEntryMessage7501Print(entry, message, incoming7501, bluMessage);
		}

		void AssertEqualsForTestITNumber(EntrySummary7501BillCollection billCollection, ITAndSplitDetails[] details)
		{
			AssertEquals("Count of printed bill details.", billCollection.Count, details.Length);

			billCollection.ForEach(bill =>
			{
				var entrySummary = bill as EntrySummary7501Bill;
				AssertNotNull(entrySummary);

				var matchedBill = details.FirstOrDefault(it => it.US_ITNumber == entrySummary.ITNO);
				AssertNotNull(matchedBill);
				AssertEquals(entrySummary.PkgQty, matchedBill.US_NoOfPacks);

				if (entrySummary.SubHouseBill.IsEmpty)
				{
					if (entrySummary.HouseBill.IsEmpty)
					{
						AssertEquals(entrySummary.MasterBill, matchedBill.Bill.CU_BillNum);
					}
					else
					{
						AssertEquals(entrySummary.MasterBill, matchedBill.Bill.CU_MasterBill);
						AssertEquals(entrySummary.HouseBill, matchedBill.Bill.CU_BillNum);
					}
				}
				else
				{
					AssertEquals(entrySummary.MasterBill, matchedBill.Bill.CU_MasterBill);
					AssertEquals(entrySummary.HouseBill, matchedBill.Bill.CU_HouseBill);
					AssertEquals(entrySummary.SubHouseBill, matchedBill.Bill.CU_BillNum);
				}
			});
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
					DeclarationTestHelper.SetEntryFilerCode("SV9");
					declaration.US_EntryFilerCode = "SV9";
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = "ACS";

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				}
				return declaration;
			}
		}
		JobDeclaration declaration;
	}
}
