using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageProcessor.Testing
{
	[TestedType(typeof(STATACMessageSG3Helper))]
	sealed class STATACMessageSG3HelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTransactionDate()
		{
			AssertEquals("TransactionDate", new ZDateTime(2016, 7, 10), dailySG3Helper.TransactionDate);
			AssertEquals("TransactionDateString", "20160710", dailySG3Helper.TransactionDateString);
			AssertEquals("TransactionDate", new ZDateTime(2016, 6, 13), detailSG3Helper.TransactionDate);
			AssertEquals("TransactionDateString", "20160613", detailSG3Helper.TransactionDateString);
		}

		public void TestDueDate()
		{
			AssertEquals("DueDate", new ZDateTime(2016, 7, 10), dailySG3Helper.DueDate);
			AssertEquals("DueDate", new ZDateTime(2016, 7, 14), detailSG3Helper.DueDate);
		}

		public void TestLevel()
		{
			AssertEquals("Level", "H", dailySG3Helper.Level);
			AssertEquals("Level", "H", detailSG3Helper.Level);
		}

		public void TestType()
		{
			AssertEquals("Type", "C", dailySG3Helper.Type);
			AssertEquals("Type", ZString.Empty, detailSG3Helper.Type);
			AssertEquals("Type", "V", detailSG3Helper2.Type);
		}

		public void TestTransactionReference()
		{
			AssertEquals("TransactionReference", "01862282JSA20160710360193", dailySG3Helper.TransactionReference);
			AssertEquals("TransactionReference", "01862282LBA20160613355310", detailSG3Helper.TransactionReference);
		}

		public void TestTransactionDescription()
		{
			AssertEquals("TransactionDescription", "CASH", dailySG3Helper.TransactionDescription);
			AssertEquals("TransactionDescription", "DEFERMENT DECLARATION", detailSG3Helper.TransactionDescription);
		}

		public void TestAmount()
		{
			AssertEquals("Amount", new ZDecimal("1500"), dailySG3Helper.Amount);
			AssertEquals("Amount", new ZDecimal("13694.44"), detailSG3Helper.Amount);
		}

		protected override BusinessObject GetNewBusinessObject() => new STATACMessageSG3Helper(null);

		protected override void SetUp()
		{
			base.SetUp();
			var testInterchange = Factory.NewWithValidTestData<ZACInterchange>();
			var dailyMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			dailyMessage.EM_MessageText = STATAC_DAILY_Message;
			var detailMessage = (ZAMessage)testInterchange.ContainedMessages.AddNew(typeof(ZAMessage));
			detailMessage.EM_MessageText = STATAC_DETAIL_Message;
			var dailyHelper = STATACMessageHelper.New(dailyMessage);
			var detailHelper = STATACMessageHelper.New(detailMessage);
			dailySG3Helper = new STATACMessageSG3Helper(dailyHelper.statacMessage.Group3[0]);
			detailSG3Helper = new STATACMessageSG3Helper(detailHelper.statacMessage.Group3[0]);
			detailSG3Helper2 = new STATACMessageSG3Helper(detailHelper.statacMessage.Group3[1]);
		}

		const string STATAC_DAILY_Message = "UNH+1+STATAC:D:96B:UN:ZZZ01'BGM+342:::DAILY'DTM+137:201607110120:203'DTM+90:20160710:102'DTM+91:20160710:102'RFF+ADE:8120050169'NAD+CM+CTN'NAD+AG+21044566'DOC+914:::HC+01862282JSA20160710360193::CASH'MOA+9:1500'DTM+353:20160710:102'DTM+140:20160710:102'UNS+S'MOA+86:267737.46'UNT+15+1'";
		const string STATAC_DETAIL_Message = "UNH+1+STATAC:D:96B:UN:ZZZ01'BGM+342:::DETAIL+333596'DTM+137:201607100725:203'DTM+90:20160613:102'DTM+91:20160707:102'RFF+ADE:8120223246'NAD+CM+LBA'NAD+AG+01862282'DOC+914:::H+01862282LBA20160613355310::DEFERMENT DECLARATION'MOA+9:13694.44'DTM+353:20160613:102'DTM+140:20160714:102'DOC+914:::IV+01862282LBA20160613355310::VAT'MOA+9:6251.84'DTM+353:20160613:102'DTM+140:20160620:102'DOC+914:::ID+01862282LBA20160613355310::DUTIES'MOA+9:7442.6'DTM+353:20160613:102'DTM+140:20160620:102'DOC+914:::I+8120223246CF0000697::PAYMENT'MOA+9:-13694.44'DTM+353:20160617:102'UNS+S'MOA+86:443443.42'UNT+25+1'";
		STATACMessageSG3Helper dailySG3Helper;
		STATACMessageSG3Helper detailSG3Helper;
		STATACMessageSG3Helper detailSG3Helper2;
	}
}
