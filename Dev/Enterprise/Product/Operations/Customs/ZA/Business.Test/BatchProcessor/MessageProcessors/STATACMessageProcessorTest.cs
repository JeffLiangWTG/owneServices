using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.ZArchitecture.Schema;

#pragma warning disable IDE0058 // Expression value is never used
#pragma warning disable WTG1018 // The meaning of boolean literals may not be easy to understand at the call-site.

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class STATACMessageProcessorTest : TestCaseWithFactory
	{
		public void TestDaily()
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<STATACEDIMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = STATAC_DAILY_Message.Replace("\r\n", "");
			testMessage.EM_MessageNum = "IN1";
			Factory.Save();
			CombineAssertions("New", () =>
			{
				var existingHeader = new CusStatementHeader.Loader(Factory).Load("8120050169", "CTN", "21044566", new ZDateTime(2016, 7, 10), new ZDateTime(2016, 7, 10));
				AssertNull("CusStatementHeader should not exist before ProcessMessage()", existingHeader);
				new STATACMessageProcessor(logger).ProcessMessage(testMessage);
				Factory.Save();
				existingHeader = new CusStatementHeader.Loader(Factory).Load("8120050169", "CTN", "21044566", new ZDateTime(2016, 7, 10), new ZDateTime(2016, 7, 10));
				AssertNotNull("CusStatementHeader should exist after ProcessMessage()", existingHeader);
				AssertEquals("CusStatementHeader must have one CusStatementLine", 1, existingHeader.StatementLines.Count);
				var statementLine = existingHeader.StatementLines[0];
				AssertEquals("01862282JSA20160710360193", statementLine.B3_EntryNum);
				AssertEquals("CASH", statementLine.B3_BrokerReference);
				AssertEquals(new ZDecimal(1500), statementLine.B3_CustomsFeesTotal);
				AssertEquals("CusStatementLine must have one CusStatementLineCharge", 1, statementLine.Charges.Count);
				var statementLineCharge = statementLine.Charges[0];
				AssertEquals("O", statementLineCharge.B4_ChargeType);
				AssertEquals(new ZDecimal(1500), statementLineCharge.B4_ChargeAmount);
			});
			CombineAssertions("Existing", () =>
			{
				var existingHeader = new CusStatementHeader.Loader(Factory).Load("8120050169", "CTN", "21044566", new ZDateTime(2016, 7, 10), new ZDateTime(2016, 7, 10));
				AssertNotNull("CusStatementHeader should exist before ProcessMessage()", existingHeader);
				new STATACMessageProcessor(logger).ProcessMessage(testMessage);
				Factory.Save();
				existingHeader = new CusStatementHeader.Loader(Factory).Load("8120050169", "CTN", "21044566", new ZDateTime(2016, 7, 10), new ZDateTime(2016, 7, 10));
				AssertNotNull("CusStatementHeader should exist after ProcessMessage()", existingHeader);
				AssertEquals("CusStatementHeader must have only one CusStatementLine", 1, existingHeader.StatementLines.Count);
				var statementLine = existingHeader.StatementLines[0];
				AssertEquals("01862282JSA20160710360193", statementLine.B3_EntryNum);
				AssertEquals("CASH", statementLine.B3_BrokerReference);
				AssertEquals(new ZDecimal(1500), statementLine.B3_CustomsFeesTotal);
				AssertEquals("CusStatementLine must have only one CusStatementLineCharge", 1, statementLine.Charges.Count);
				var statementLineCharge = statementLine.Charges[0];
				AssertEquals("O", statementLineCharge.B4_ChargeType);
				AssertEquals(new ZDecimal(1500), statementLineCharge.B4_ChargeAmount);
			});
		}

		public void TestMissingFAN()
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<STATACEDIMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = STATAC_DAILY_Message_Missing_FAN.Replace("\r\n", "");
			testMessage.EM_MessageNum = "IN1";
			Factory.Save();
			var processor = new STATACMessageProcessor(logger);
			processor.PreProcessMessage(testMessage);
			processor.ProcessMessage(testMessage);
			AssertEquals("DCD", testMessage.EM_Status);
			AssertMultilineASCIIEquals("Log", ZString.Format(@"Information: 	{0}", STATACMessageProcessor.MissingFAN(testMessage.EM_MessageNum)), logger.LogMessages.ToString());
		}

		public void TestInvalidTransactionDate()
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<STATACEDIMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = STATAC_DAILY_Message_Invalid_TransactionDate.Replace("\r\n", "");
			testMessage.EM_MessageNum = "IN1";
			Factory.Save();
			new STATACMessageProcessor(logger).ProcessMessage(testMessage);
			AssertEquals("PRS", testMessage.EM_Status);
			AssertMultilineASCIIEquals("Log", ZString.Format(@"Information: 	{0}", STATACMessageProcessor.TransactionDateInvalid(testMessage.EM_MessageNum, "20163710")), logger.LogMessages.ToString());
		}

		public void TestMissingEntryNum()
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<STATACEDIMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = STATAC_DAILY_Message_Missing_EntryNum.Replace("\r\n", "");
			testMessage.EM_MessageNum = "IN1";
			Factory.Save();
			new STATACMessageProcessor(logger).ProcessMessage(testMessage);
			AssertEquals("PRS", testMessage.EM_Status);
			AssertMultilineASCIIEquals("Log", ZString.Format(@"Information: 	{0}", STATACMessageProcessor.MissingEntryNumber(testMessage.EM_MessageNum)), logger.LogMessages.ToString());
		}

		public void TestDetail()
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<STATACEDIMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = STATAC_DETAIL_Message.Replace("\r\n", "");
			testMessage.EM_MessageNum = "IN1";
			CombineAssertions("New", () =>
			{
				var existingHeader = new CusStatementHeader.Loader(Factory).Load("8120223246", "LBA", "01862282", new ZDateTime(2016, 6, 13), new ZDateTime(2016, 6, 20));
				AssertNull("CusStatementHeader should not exist before ProcessMessage()", existingHeader);
				var existingHeaderPayment = new CusStatementHeader.Loader(Factory).Load("8120223246", "LBA", "01862282", new ZDateTime(2016, 6, 17), ZDateTime.Empty);
				AssertNull("CusStatementHeader (Payment) should not exist before ProcessMessage()", existingHeaderPayment);
				new STATACMessageProcessor(logger).ProcessMessage(testMessage);
				existingHeader = new CusStatementHeader.Loader(Factory).Load("8120223246", "LBA", "01862282", new ZDateTime(2016, 6, 13), new ZDateTime(2016, 6, 20));
				AssertNotNull("CusStatementHeader should exist after ProcessMessage()", existingHeader);
				AssertEquals("CusStatementHeader must have one CusStatementLine (Header Line skipped)", 1, existingHeader.StatementLines.Count);
				var statementLine = existingHeader.StatementLines[0];
				AssertEquals("01862282LBA20160613355310", statementLine.B3_EntryNum);
				AssertEquals("VAT", statementLine.B3_BrokerReference);
				AssertEquals(new ZDecimal(13694.44), statementLine.B3_CustomsFeesTotal);
				AssertEquals("CusStatementLine must have one CusStatementLineCharge", 2, statementLine.Charges.Count);
				var charges = statementLine.Charges.ToList();
				var statementLineVatCharge = charges.Find(x => x.B4_ChargeType == "V");
				AssertEquals("VAT amount", new ZDecimal(6251.84), statementLineVatCharge.B4_ChargeAmount);
				var statementLineDutyCharge = charges.Find(x => x.B4_ChargeType == "D");
				AssertEquals("Duties amount", new ZDecimal(7442.60), statementLineDutyCharge.B4_ChargeAmount);
				existingHeaderPayment = new CusStatementHeader.Loader(Factory).Load("8120223246", "LBA", "01862282", new ZDateTime(2016, 6, 17), ZDateTime.Empty);
				AssertNotNull("CusStatementHeader (Payment) should exist after ProcessMessage()", existingHeaderPayment);
				AssertEquals("CusStatementHeader (Payment) must have one CusStatementLines (Header Line skipped)", 1, existingHeaderPayment.StatementLines.Count);
				var statementLinePayment = existingHeaderPayment.StatementLines[0];
				AssertEquals("8120223246CF0000697", statementLinePayment.B3_EntryNum);
				AssertEquals("PAYMENT", statementLinePayment.B3_BrokerReference);
				AssertEquals(new ZDecimal(-13694.44), statementLinePayment.B3_CustomsFeesTotal);
				AssertEquals("CusStatementLine (Payment) must have one CusStatementLineCharge", 1, statementLinePayment.Charges.Count);
				var statementLineChargePayment = statementLinePayment.Charges[0];
				AssertEquals("P", statementLineChargePayment.B4_ChargeType);
				AssertEquals(new ZDecimal(-13694.44), statementLineChargePayment.B4_ChargeAmount);
			});
			CombineAssertions("Existing", () =>
			{
				var existingHeader = new CusStatementHeader.Loader(Factory).Load("8120223246", "LBA", "01862282", new ZDateTime(2016, 6, 13), new ZDateTime(2016, 6, 20));
				AssertNotNull("CusStatementHeader should exist before ProcessMessage()", existingHeader);
				var existingHeaderPayment = new CusStatementHeader.Loader(Factory).Load("8120223246", "LBA", "01862282", new ZDateTime(2016, 6, 17), ZDateTime.Empty);
				AssertNotNull("CusStatementHeader (Payment) should exist before ProcessMessage()", existingHeaderPayment);
				new STATACMessageProcessor(logger).ProcessMessage(testMessage);
				existingHeader = new CusStatementHeader.Loader(Factory).Load("8120223246", "LBA", "01862282", new ZDateTime(2016, 6, 13), new ZDateTime(2016, 6, 20));
				AssertNotNull("CusStatementHeader should exist after ProcessMessage()", existingHeader);
				AssertEquals("CusStatementHeader must have one CusStatementLine (Header Line skipped)", 1, existingHeader.StatementLines.Count);
				var statementLine = existingHeader.StatementLines[0];
				AssertEquals("01862282LBA20160613355310", statementLine.B3_EntryNum);
				AssertEquals("VAT", statementLine.B3_BrokerReference);
				AssertEquals(new ZDecimal(13694.44), statementLine.B3_CustomsFeesTotal);
				AssertEquals("CusStatementLine must have two CusStatementLineCharges", 2, statementLine.Charges.Count);
				var charges = statementLine.Charges.ToList();
				var statementLineVatCharge = charges.Find(x => x.B4_ChargeType == "V");
				AssertEquals(new ZDecimal(6251.84), statementLineVatCharge.B4_ChargeAmount);
				var statementLineDutyCharge = charges.Find(x => x.B4_ChargeType == "D");
				AssertEquals("Duties amount", new ZDecimal(7442.60), statementLineDutyCharge.B4_ChargeAmount);
				existingHeaderPayment = new CusStatementHeader.Loader(Factory).Load("8120223246", "LBA", "01862282", new ZDateTime(2016, 6, 17), ZDateTime.Empty);
				AssertNotNull("CusStatementHeader (Payment) should exist after ProcessMessage()", existingHeaderPayment);
				AssertEquals("CusStatementHeader (Payment) must have one CusStatementLines (Header Line skipped)", 1, existingHeaderPayment.StatementLines.Count);
				var statementLinePayment = existingHeaderPayment.StatementLines[0];
				AssertEquals("8120223246CF0000697", statementLinePayment.B3_EntryNum);
				AssertEquals("PAYMENT", statementLinePayment.B3_BrokerReference);
				AssertEquals(new ZDecimal(-13694.44), statementLinePayment.B3_CustomsFeesTotal);
				AssertEquals("CusStatementLine (Payment) must have one CusStatementLineCharge", 1, statementLinePayment.Charges.Count);
				var statementLineChargePayment = statementLinePayment.Charges[0];
				AssertEquals("P", statementLineChargePayment.B4_ChargeType);
				AssertEquals(new ZDecimal(-13694.44), statementLineChargePayment.B4_ChargeAmount);
			});
		}

		public void TestIsLRNFormat()
		{
			var cusHeader = Factory.New<CusStatementHeader>();
			var line = cusHeader.StatementLines.AddNew();
			cusHeader.B2_EntryFilerCode = "01234567";
			line.B3_EntryNum = "";
			AssertEquals("Invalid length", false, STATACMessageProcessor.IsEntryNumLRNFormat(line));
			line.B3_EntryNum = "01234567000ABCDEFGHIJKLM";
			AssertEquals("Invalid Customs Office", false, STATACMessageProcessor.IsEntryNumLRNFormat(line));
			line.B3_EntryNum = "01234567ABCABCDEFGHIJKLMN";
			AssertEquals("Invalid numeric", false, STATACMessageProcessor.IsEntryNumLRNFormat(line));
			line.B3_EntryNum = "01234567ABC00000000000001";
			AssertEquals("Invalid date", false, STATACMessageProcessor.IsEntryNumLRNFormat(line));
			line.B3_EntryNum = "01234567ABC10092014000001";
			AssertEquals("Invalid date format", false, STATACMessageProcessor.IsEntryNumLRNFormat(line));
			line.B3_EntryNum = "12345678000ABCDEFGHIJKLM";
			AssertEquals("Invalid agent code in Entry", false, STATACMessageProcessor.IsEntryNumLRNFormat(line));
			line.B3_EntryNum = "01234567ABC20141109000001";
			AssertEquals("Should be valid", true, STATACMessageProcessor.IsEntryNumLRNFormat(line));
			cusHeader.B2_EntryFilerCode = "12345";
			AssertEquals("Invalid agent code provided", false, STATACMessageProcessor.IsEntryNumLRNFormat(line));
		}

		public void TestIsPRNFormat()
		{
			var cusHeader = Factory.New<CusStatementHeader>();
			var line = cusHeader.StatementLines.AddNew();
			cusHeader.B2_AccountNo = "1234567890";
			line.B3_EntryNum = "";
			AssertEquals("Invalid length", false, STATACMessageProcessor.IsEntryNumPRNFormat(line));
			line.B3_EntryNum = "1234567891000000000";
			AssertEquals("Invalid FAN", false, STATACMessageProcessor.IsEntryNumPRNFormat(line));
			line.B3_EntryNum = "1234567890999999999";
			AssertEquals("Valid", true, STATACMessageProcessor.IsEntryNumPRNFormat(line));
			cusHeader.B2_AccountNo = "";
			AssertEquals("Missing FAN", false, STATACMessageProcessor.IsEntryNumPRNFormat(line));
		}

		public void TestChargeType()
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage1 = Factory.NewWithValidTestData<STATACEDIMessage>();
			testMessage1.EM_ReceiveTransmit = "RCV";
			testMessage1.EM_ApplicationCode = "ZAC";
			testMessage1.EM_Status = "QUE";
			testMessage1.EM_MessageText = STATAC_DAILY_Message.Replace("\r\n", "");
			testMessage1.EM_MessageNum = "IN1";
			Factory.Save();
			new STATACMessageProcessor(logger).ProcessMessage(testMessage1);
			Factory.Save();
			var existingHeader = new CusStatementHeader.Loader(Factory).Load("8120050169", "CTN", "21044566", new ZDateTime(2016, 7, 10), new ZDateTime(2016, 7, 10));
			AssertNotNull("CusStatementHeader should exist after ProcessMessage()", existingHeader);
			AssertEquals("CusStatementHeader must have one CusStatementLine", 1, existingHeader.StatementLines.Count);
			var statementLine = existingHeader.StatementLines[0];
			var statementLineCharge = statementLine.Charges[0];
			AssertEquals("O", statementLineCharge.B4_ChargeType);
			var testMessage2 = Factory.NewWithValidTestData<STATACEDIMessage>();
			testMessage2.EM_ReceiveTransmit = "RCV";
			testMessage2.EM_ApplicationCode = "ZAC";
			testMessage2.EM_Status = "QUE";
			testMessage2.EM_MessageText = STATAC_DAILY_Message.Replace("\r\n", "").Replace("HC+01862282JSA20160710360193", "HC+21044566JSA20160710360193").Replace("RFF+ADE:8120050169", "RFF+ADE:8120050170"); // Valid LRN Format
			testMessage2.EM_MessageNum = "IN2";
			Factory.Save();
			new STATACMessageProcessor(logger).ProcessMessage(testMessage2);
			Factory.Save();
			existingHeader = new CusStatementHeader.Loader(Factory).Load("8120050170", "CTN", "21044566", new ZDateTime(2016, 7, 10), new ZDateTime(2016, 7, 10));
			AssertNotNull("CusStatementHeader should exist after ProcessMessage()", existingHeader);
			AssertEquals("CusStatementHeader must have one CusStatementLine", 1, existingHeader.StatementLines.Count);
			statementLine = existingHeader.StatementLines[0];
			statementLineCharge = statementLine.Charges[0];
			AssertEquals("C", statementLineCharge.B4_ChargeType);
			var testMessage3 = Factory.NewWithValidTestData<STATACEDIMessage>();
			testMessage3.EM_ReceiveTransmit = "RCV";
			testMessage3.EM_ApplicationCode = "ZAC";
			testMessage3.EM_Status = "QUE";
			testMessage3.EM_MessageText = STATAC_DAILY_Message.Replace("\r\n", "").Replace("HC+01862282JSA20160710360193", "HC+8120050171AB1234567").Replace("RFF+ADE:8120050169", "RFF+ADE:8120050171"); // Valid PRN Format
			testMessage3.EM_MessageNum = "IN2";
			Factory.Save();
			new STATACMessageProcessor(logger).ProcessMessage(testMessage3);
			Factory.Save();
			existingHeader = new CusStatementHeader.Loader(Factory).Load("8120050171", "CTN", "21044566", new ZDateTime(2016, 7, 10), new ZDateTime(2016, 7, 10));
			AssertNotNull("CusStatementHeader should exist after ProcessMessage()", existingHeader);
			AssertEquals("CusStatementHeader must have one CusStatementLine", 1, existingHeader.StatementLines.Count);
			statementLine = existingHeader.StatementLines[0];
			statementLineCharge = statementLine.Charges[0];
			AssertEquals("P", statementLineCharge.B4_ChargeType);
		}

		public void TestProcessingOfStatacMessagesWithoutCausingDuplicateDataRecords()
		{
			string messageText = @"UNH+1+STATAC:D:96B:UN:ZZZ01'
BGM+342:::DETAIL+8120063931'
DTM+137:202002200936:203'
DTM+90:20190409:102'
DTM+91:20200220:102'
RFF+ADE:8120063931'
NAD+CM+DBN'
NAD+AG+00468380WTG'
DOC+914:::H+00468380DBN20190405026022::DEFERMENT DECLARATION'
MOA+9:2739945.3'
DTM+353:20190409:102'
DOC+914:::IV+00468380DBN20190405026022::VAT'
MOA+9:2739111.9'
DTM+353:20190409:102'
DTM+140:20190515:102'
DOC+914:::ID+00468380DBN20190405026022::DUTIES'
MOA+9:833.4'
DTM+353:20190409:102'
DTM+140:20190515:102'
DOC+914:::I+8120063931CF0001915::PAYMENT'
MOA+9:-2739945.3'
DTM+353:20190515:102'
DOC+914:::H+00468380DBN20190614034500::DEFERMENT DECLARATION'
MOA+9:3482185.6'
DTM+353:20190614:102'
DOC+914:::IV+00468380DBN20190614034500::VAT'
MOA+9:3471014.4'
DTM+353:20190614:102'
DTM+140:20190715:102'
DOC+914:::ID+00468380DBN20190614034500::DUTIES'
MOA+9:11171.2'
DTM+353:20190614:102'
DTM+140:20190715:102'
DOC+914:::ID+00468380DBN20190614034500::AMENDED DECLARATION'
MOA+9:-11122.4'
DTM+353:20200217:102'
DTM+140:20190715:102'
DOC+914:::I+8120063931CF0001915::PAYMENT'
MOA+9:-3471063.2'
DTM+353:20200217:102'
DOC+914:::H+00468380DBN20190711037568::DEFERMENT DECLARATION'
MOA+9:53687.4'
DTM+353:20190711:102'
DOC+914:::IV+00468380DBN20190711037568::VAT'
MOA+9:53687.4'
DTM+353:20190711:102'
DTM+140:20190815:102'
DOC+914:::I+8120063931CF0001915::PAYMENT'
MOA+9:-53687.4'
DTM+353:20200219:102'
DOC+914:::H+00468380DBN20190718038435::DEFERMENT DECLARATION'
MOA+9:208060.65'
DTM+353:20190718:102'
DOC+914:::IV+00468380DBN20190718038435::VAT'
MOA+9:147900.3'
DTM+353:20190718:102'
DTM+140:20190815:102'
DOC+914:::ID+00468380DBN20190718038435::DUTIES'
MOA+9:60160.35'
DTM+353:20190718:102'
DTM+140:20190815:102'
UNS+S'
MOA+86:93661026.22'
UNT+64+1'";
			var testMessage = Factory.NewWithValidTestData<STATACEDIMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = messageText.Replace("\r\n", "");
			testMessage.EM_MessageNum = "IN1";
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			new STATACMessageProcessor(logger).ProcessMessage(testMessage);
			AssertEquals("PRS", testMessage.EM_Status);
			var query = new ZQuery();
			query.AddToFilter(CusStatementLineSchema.B3_EntryNum, "00468380DBN20190614034500");
			var statementLines = new List<CusStatementLine>(Factory.Load<CusStatementLine>(query));
			AssertEquals("Check expected statement line records.", 2, statementLines.Count);
			var statementLine_for_MainDeclaration = statementLines.Find(x => x.B3_CustomsFeesTotal == 3482185.60m);
			var statementLine_for_Amendment = statementLines.Find(x => x.B3_CustomsFeesTotal == -11122.40m);
			AssertNotNull("Ensure that the statement line for the main declaration exists.", statementLine_for_MainDeclaration);
			AssertNotNull("Ensure that the statement line for the amendment exists.", statementLine_for_Amendment);
			AssertEquals("Main declaration statement line Due date.", new ZDateTime(2019, 07, 15), statementLine_for_MainDeclaration.StatementHeader.B2_DueDate);
			AssertEquals("Amendment statement line Due date.", new ZDateTime(2019, 07, 15), statementLine_for_Amendment.StatementHeader.B2_DueDate);
			#region Check Charges for main declaration:
			query = new ZQuery();
			query.AddToFilter(CusStatementLineChargeSchema.B4_B3, statementLine_for_MainDeclaration.PK);
			var lineCharges = new List<CusStatementLineCharge>(Factory.Load<CusStatementLineCharge>(query));
			AssertEquals("Check charges for main statement line.", 2, lineCharges.Count);
			var vatCharge = lineCharges.Find(x => x.B4_ChargeType == "V");
			AssertNotNull("Main declaration VAT Charge must exist.", vatCharge);
			AssertEquals("Main declaration VAT Charge amount.", 3471014.40m, vatCharge.B4_ChargeAmount);
			AssertEquals("Main declaration VAT Charge Process date.", new ZDateTime(2019, 06, 14), vatCharge.ProcessDate);
			var dutyCharge = lineCharges.Find(x => x.B4_ChargeType == "D");
			AssertNotNull("Main declaration Duty Charge must exist.", dutyCharge);
			AssertEquals("Main declaration Duty Charge amount.", 11171.2m, dutyCharge.B4_ChargeAmount);
			AssertEquals("Main declaration Duty Charge Process date.", new ZDateTime(2019, 06, 14), dutyCharge.ProcessDate);
			#endregion Check Charges for main declaration.
			#region Check Charges for amendment:
			query = new ZQuery();
			query.AddToFilter(CusStatementLineChargeSchema.B4_B3, statementLine_for_Amendment.PK);
			lineCharges = new List<CusStatementLineCharge>(Factory.Load<CusStatementLineCharge>(query));
			AssertEquals("Check charges for amendment statement line.", 1, lineCharges.Count);
			var amendmentCharge = lineCharges.Find(x => x.B4_ChargeType == "D");
			AssertNotNull("Amendment Duty Charge must exist.", amendmentCharge);
			AssertEquals("Amendment Duty Charge amount.", -11122.40m, amendmentCharge.B4_ChargeAmount);
			AssertEquals("Amendment Duty Charge Process date.", new ZDateTime(2020, 02, 17), amendmentCharge.ProcessDate);
			#endregion Check Charges for amendment.
		}

		internal const string STATAC_DAILY_Message = @"UNH+1+STATAC:D:96B:UN:ZZZ01'
BGM+342:::DAILY'
DTM+137:201607110120:203'
DTM+90:20160710:102'
DTM+91:20160710:102'
RFF+ADE:8120050169'
NAD+CM+CTN'
NAD+AG+21044566'
DOC+914:::HC+01862282JSA20160710360193::CASH'
MOA+9:1500'
DTM+353:20160710:102'
DTM+140:20160710:102'
UNS+S'
MOA+86:1500'
UNT+15+1'";

		const string STATAC_DAILY_Message_Missing_FAN = @"UNH+1+STATAC:D:96B:UN:ZZZ01'
BGM+342:::DAILY'
DTM+137:201607110120:203'
DTM+90:20160710:102'
DTM+91:20160710:102'
NAD+CM+CTN'
NAD+AG+21044566'
DOC+914:::HC+01862282JSA20160710360193::CASH'
MOA+9:1500'
DTM+353:20160710:102'
DTM+140:20160710:102'
UNS+S'
MOA+86:1500'
UNT+15+1'";

		const string STATAC_DAILY_Message_Invalid_TransactionDate = @"UNH+1+STATAC:D:96B:UN:ZZZ01'
BGM+342:::DAILY'
DTM+137:201607110120:203'
DTM+90:20160710:102'
DTM+91:20160710:102'
RFF+ADE:8120050169'
NAD+CM+CTN'
NAD+AG+21044566'
DOC+914:::HC+01862282JSA20160710360193::CASH'
MOA+9:1500'
DTM+353:20163710:102'
DTM+140:20160710:102'
UNS+S'
MOA+86:1500'
UNT+15+1'";

		const string STATAC_DAILY_Message_Missing_EntryNum = @"UNH+1+STATAC:D:96B:UN:ZZZ01'
BGM+342:::DAILY'
DTM+137:201607110120:203'
DTM+90:20160710:102'
DTM+91:20160710:102'
RFF+ADE:8120050169'
NAD+CM+CTN'
NAD+AG+21044566'
DOC+914:::HC+::CASH'
MOA+9:1500'
DTM+353:20160710:102'
DTM+140:20160710:102'
UNS+S'
MOA+86:1500'
UNT+15+1'";

		const string STATAC_DETAIL_Message = @"UNH+1+STATAC:D:96B:UN:ZZZ01'
BGM+342:::DETAIL+333596'
DTM+137:201607100725:203'
DTM+90:20160613:102'
DTM+91:20160707:102'
RFF+ADE:8120223246'
NAD+CM+LBA'
NAD+AG+01862282'
DOC+914:::H+01862282LBA20160613355310::DEFERMENT DECLARATION'
MOA+9:13694.44'
DTM+353:20160613:102'
DOC+914:::IV+01862282LBA20160613355310::VAT'
MOA+9:6251.84'
DTM+353:20160613:102'
DTM+140:20160620:102'
DOC+914:::ID+01862282LBA20160613355310::DUTIES'
MOA+9:7442.6'
DTM+353:20160613:102'
DTM+140:20160620:102'
DOC+914:::I+8120223246CF0000697::PAYMENT'
MOA+9:-13694.44'
DTM+353:20160617:102'
UNS+S'
MOA+86:443443.42'
UNT+25+1'";
	}
}
