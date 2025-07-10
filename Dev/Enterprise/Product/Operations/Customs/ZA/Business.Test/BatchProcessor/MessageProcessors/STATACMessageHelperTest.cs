using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageProcessor.Testing
{
	[TestedType(typeof(STATACMessageHelper))]
	sealed class STATACMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDailyOrDetail()
		{
			AssertEquals("Daily", "DAILY", dailyHelper.DailyOrDetail);
			AssertEquals("Detail", "DETAIL", detailHelper.DailyOrDetail);
		}

		public void TestFinancialAccountNumber()
		{
			AssertEquals("FinancialAccountNumber", "8120050169", dailyHelper.FinancialAccountNumber);
			AssertEquals("FinancialAccountNumber", "8120223246", detailHelper.FinancialAccountNumber);
		}

		public void TestIsDetailMessage()
		{
			Assert("Daily", !dailyHelper.IsDetailMessage);
			Assert("Detail", detailHelper.IsDetailMessage);
		}

		public void TestCustomsOffice()
		{
			AssertEquals("Daily Customs Office", "CTN", dailyHelper.CustomsOffice);
			AssertEquals("Detail Customs Office", "LBA", detailHelper.CustomsOffice);
		}

		public void TestAgentCode()
		{
			AssertEquals("Daily Agent Code", "21044566", dailyHelper.AgentCode);
			AssertEquals("Detail Agent Code", "01862282", detailHelper.AgentCode);
			AssertEquals("Daily Agent Code", "21044566", agentHelper.AgentCode);
		}

		public void TestHelperForInvalidMessage()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = CUSDEC_BGM9_Message;
			var testHelper = STATACMessageHelper.New(testMessage);
			AssertNull(testHelper);
		}

		public void TestHelperForValidMessage_NULL()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = STATAC_NULL_Message;
			var testHelper = STATACMessageHelper.New(testMessage);
			CombineAssertions("STATAC NULL", () =>
			{
				AssertNotNull(testHelper);
				AssertEquals("DailyOrDetail", "DAILY", testHelper.DailyOrDetail);
				AssertEquals("FinancialAccountNumber", ZString.Empty, testHelper.FinancialAccountNumber);
				Assert(testHelper.statacMessage.Group3.Count == 0);
				var sg3 = testHelper.statacMessage.Group3[0];
				var sg3Helper = new STATACMessageSG3Helper(sg3);
				AssertEquals("TransactionDate", ZDateTime.Empty, sg3Helper.TransactionDate);
				AssertEquals("TransactionDateString", ZString.Empty, sg3Helper.TransactionDateString);
				AssertEquals("DueDate", ZDateTime.Empty, sg3Helper.DueDate);
				AssertEquals("Level", ZString.Empty, sg3Helper.Level);
				AssertEquals("Type", ZString.Empty, sg3Helper.Type);
				AssertEquals("TransactionReference", ZString.Empty, sg3Helper.TransactionReference);
				AssertEquals("TransactionDescription", ZString.Empty, sg3Helper.TransactionDescription);
				AssertEquals("Amount", ZDecimal.Zero, sg3Helper.Amount);
			});
		}

		public void TestHelperForValidMessage_DAILY()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = STATAC_DAILY_Message;
			var testHelper = STATACMessageHelper.New(testMessage);
			CombineAssertions("STATAC DAILY", () =>
			{
				AssertNotNull(testHelper);
				AssertEquals("DailyOrDetail", "DAILY", testHelper.DailyOrDetail);
				AssertEquals("FinancialAccountNumber", "8120050169", testHelper.FinancialAccountNumber);
				Assert(testHelper.statacMessage.Group3.Count > 0);
				var sg3 = testHelper.statacMessage.Group3[0];
				var sg3Helper = new STATACMessageSG3Helper(sg3);
				AssertEquals("TransactionDate", new ZDateTime(2016, 7, 10), sg3Helper.TransactionDate);
				AssertEquals("TransactionDateString", "20160710", sg3Helper.TransactionDateString);
				AssertEquals("DueDate", new ZDateTime(2016, 7, 10), sg3Helper.DueDate);
				AssertEquals("Level", "H", sg3Helper.Level);
				AssertEquals("Type", "C", sg3Helper.Type);
				AssertEquals("TransactionReference", "01862282JSA20160710360193", sg3Helper.TransactionReference);
				AssertEquals("TransactionDescription", "CASH", sg3Helper.TransactionDescription);
				AssertEquals("Amount", new ZDecimal("1500"), sg3Helper.Amount);
			});
		}

		public void TestHelperForvalidMessage_DETAIL()
		{
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var testMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			testMessage.EM_MessageText = STATAC_DETAIL_Message;
			var testHelper = STATACMessageHelper.New(testMessage);
			CombineAssertions("STATAC DETAIL", () =>
			{
				AssertNotNull(testHelper);
				AssertEquals("DailyOrDetail", "DETAIL", testHelper.DailyOrDetail);
				AssertEquals("FinancialAccountNumber", "8120223246", testHelper.FinancialAccountNumber);
				Assert(testHelper.statacMessage.Group3.Count > 0);
				var sg3 = testHelper.statacMessage.Group3[0];
				var sg3Helper = new STATACMessageSG3Helper(sg3);
				AssertEquals("TransactionDate", new ZDateTime(2016, 6, 13), sg3Helper.TransactionDate);
				AssertEquals("TransactionDateString", "20160613", sg3Helper.TransactionDateString);
				AssertEquals("DueDate", ZDateTime.Empty, sg3Helper.DueDate);
				AssertEquals("Level", "H", sg3Helper.Level);
				AssertEquals("Type", ZString.Empty, sg3Helper.Type);
				AssertEquals("TransactionReference", "01862282LBA20160613355310", sg3Helper.TransactionReference);
				AssertEquals("TransactionDescription", "DEFERMENT DECLARATION", sg3Helper.TransactionDescription);
				AssertEquals("Amount", new ZDecimal("13694.44"), sg3Helper.Amount);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<ZAMessage>();
			testMessage.EM_MessageText = STATAC_DAILY_Message;
			return STATACMessageHelper.New(testMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var dailyMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			dailyMessage.EM_MessageText = STATAC_DAILY_Message;
			var detailMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			detailMessage.EM_MessageText = STATAC_DETAIL_Message;
			var agentMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			agentMessage.EM_MessageText = STATAC_AgentDualProfile_Message;
			dailyHelper = STATACMessageHelper.New(dailyMessage);
			detailHelper = STATACMessageHelper.New(detailMessage);
			agentHelper = STATACMessageHelper.New(agentMessage);
		}

		const string STATAC_NULL_Message = "UNH+1+STATAC:D:96B:UN:ZZZ01'BGM+342:::DAILY'DTM+137:201607110120:203'DTM+90:20160710:102'DTM+91:20160710:102'UNT+5+1'";
		const string STATAC_DAILY_Message = "UNH+1+STATAC:D:96B:UN:ZZZ01'BGM+342:::DAILY'DTM+137:201607110120:203'DTM+90:20160710:102'DTM+91:20160710:102'RFF+ADE:8120050169'NAD+CM+CTN'NAD+AG+21044566'DOC+914:::HC+01862282JSA20160710360193::CASH'MOA+9:1500'DTM+353:20160710:102'DTM+140:20160710:102'UNS+S'MOA+86:267737.46'UNT+15+1'";
		const string STATAC_DETAIL_Message = "UNH+1+STATAC:D:96B:UN:ZZZ01'BGM+342:::DETAIL+333596'DTM+137:201607100725:203'DTM+90:20160613:102'DTM+91:20160707:102'RFF+ADE:8120223246'NAD+CM+LBA'NAD+AG+01862282'DOC+914:::H+01862282LBA20160613355310::DEFERMENT DECLARATION'MOA+9:13694.44'DTM+353:20160613:102'DOC+914:::IV+01862282LBA20160613355310::VAT'MOA+9:6251.84'DTM+353:20160613:102'DTM+140:20160620:102'DOC+914:::ID+01862282LBA20160613355310::DUTIES'MOA+9:7442.6'DTM+353:20160613:102'DTM+140:20160620:102'DOC+914:::I+8120223246CF0000697::PAYMENT'MOA+9:-13694.44'DTM+353:20160617:102'UNS+S'MOA+86:443443.42'UNT+25+1'";
		const string CUSDEC_BGM9_Message = "UNH+89+CUSDEC:D:96B:UN:ZZZ01'BGM+929+00505655JSA20160506000088::00001+9'CST++A:117:ZZZ'LOC+14+50::ZZZ'LOC+35+AU::5'LOC+36+ZA::5'LOC+96+JSA::ZZZ'LOC+9+AUSYD::5'GIS+D:134:ZZZ'MEA+AAE+AAD+KGM:500.00'FTX+LIN+++1::N'RFF+BH:VICTHB001'DTM+137:20160506:102'RFF+AAS:081-21333222'DTM+137:20160506:102'RFF+ABI:8120067395'RFF+ACD:89'PAC+2'PCI++MARKS AND NUMBERS TESTING HERE'TDT+20+0811002+4'DOC+380+INVH1'DTM+3:20160118:102'RFF+VA:123321'NAD+AG+00505655'NAD+MS+TST'UNS+D'CST+0001+845012907:108:ZZZ+100'FTX+AAA+++OTHER MACHINES WITH A BUILT-IN CENTRIFUGALDRIER VICDESC3'FTX+ACB+++NUIN'FTX+CCI+++11:00'LOC+27+AU'MEA+AAR++NO:20.00'MOA+38:2250'MOA+40:2250'TAX+1+1P1:107:ZZZ'MOA+161:675.00'TAX+1+VAT:107:ZZZ'MOA+161:441.00'UNS+S'TAX+3+CIF:107:ZZZ'MOA+161:2250'TAX+3+TDD:107:ZZZ'MOA+161:675.00'TAX+3+TVD:107:ZZZ'MOA+161:441.00'TAX+3+CUS:107:ZZZ'MOA+161:2250'UNT+48+89'";
		const string STATAC_AgentDualProfile_Message = "UNH+1+STATAC:D:96B:UN:ZZZ01'BGM+342:::DAILY'DTM+137:201607110120:203'DTM+90:20160710:102'DTM+91:20160710:102'RFF+ADE:8120050169'NAD+CM+CTN'NAD+AG+21044566ABC'DOC+914:::HC+01862282JSA20160710360193::CASH'MOA+9:1500'DTM+353:20160710:102'DTM+140:20160710:102'UNS+S'MOA+86:267737.46'UNT+15+1'";
		STATACMessageHelper dailyHelper;
		STATACMessageHelper detailHelper;
		STATACMessageHelper agentHelper;
	}
}
