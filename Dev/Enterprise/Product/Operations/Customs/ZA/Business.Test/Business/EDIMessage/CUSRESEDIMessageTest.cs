using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CUSRESEDIMessage))]
	sealed class CUSRESEDIMessageTest : SARSEDIMessageAbstractTest
	{
		public void TestDocumentNumber()
		{
			var cusres = Factory.New<CUSRESEDIMessage>();
			cusres.EM_MessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+963+AEF5840B836D4F71B342D6B93E0692A3'DTM+178:20170715:102'TDT+20+V345+1'LOC+22+DUR::ZZZ'GIS+6:120:ZZZ'NAD+AG+00000000'RFF+BH:BILL1'RFF+AAS:MASTERBILLLXXX'DTM+137:20170718:102'ERP+6:0'ERC+0001::ZZZ'FTX+AAO+++CON GROSS MASS / WEIGHT where CCC=00053592,MCC=SAFM,TDN=BILL1,MTD=MAST:ERBILLLXXX,value=?'null?'?: Field is required: : : 'ERP+6:0'ERC+0001::ZZZ'FTX+AAO+++BOL CONSIGNOR NAME where CCC=00053592,MCC=SAFM,TDN=BILL1,MTD=MASTERBIL:LLXXX,value=?'null?'?: Field is required: : : 'ERP+6:0'ERC+0001::ZZZ'FTX+AAO+++BOL CONSIGNOR ADDRESS LINE - 1 where value=?'null?'?: Field is required: : : : 'ERP+6:0'ERC+0001::ZZZ'FTX+AAO+++BLL GROSS MASS / WEIGHT where CCC=00053592,MCC=SAFM,TDN=BILL1,MTD=MAST:ERBILLLXXX,value=?'null?'?: Field is required: : : 'UNT+23+1'";
			AssertEquals("AEF5840B836D4F71B342D6B93E0692A3", cusres.LocalReferenceNumber);
		}

		public void TestParentMessageNumber()
		{
			var cusresMessage = Factory.New<CUSRESEDIMessage>();
			cusresMessage.EM_MessageText = ZAMessageTest.CUSRESTestMessage.Replace("\r\n", "");

			AssertEquals("202", cusresMessage.ParentMessageNumber);
		}

		public void TestCUSRESHelper()
		{
			var cusresMessage = Factory.New<CUSRESEDIMessage>();
			cusresMessage.EM_MessageText = ZAMessageTest.CUSRESTestMessage.Replace("\r\n", "");
			var helper = cusresMessage.CUSRESHelper;
			AssertNotNull(helper);
			AssertType<CUSRESMessageHelper>(helper);
		}

		public void TestGetHumanFriendlyMessageInterpretation_NoNullExceptionIfCUSRESHelperIsNull()
		{
			var cusresMessage = Factory.New<CUSRESEDIMessageForTesting_NullReference>();
			AssertNoExceptionThrown(() => _ = cusresMessage.EM_MessageInterpretation);
			AssertContains("Message could not be interpreted correctly", cusresMessage.EM_MessageInterpretation);
		}

		public void TestEntryStatusAndPreparationDate()
		{
			var interchange1 = Factory.New<ZACInterchange>();
			interchange1.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160802:0844+1299++CUSRES+++GWWTGTEST+1'";
			var message = (CUSRESEDIMessage)interchange1.ContainedMessages.AddNew(typeof(CUSRESEDIMessage));
			AssertEquals(ZString.Empty, message.EntryStatus);
			AssertEquals(new ZDateTime(2016, 8, 2, 8, 44, 0), message.PreparationDate);

			message = (CUSRESEDIMessage)interchange1.ContainedMessages.AddNew(typeof(CUSRESEDIMessage));
			message.EM_MessageText = CUSRESMessageProcessorTest.GetTestMessageResNo("9");
			AssertEquals("9", message.EntryStatus);
			AssertEquals(new ZDateTime(2016, 8, 2, 8, 44, 0), message.PreparationDate);

			message = (CUSRESEDIMessage)interchange1.ContainedMessages.AddNew(typeof(CUSRESEDIMessage));
			message.EM_MessageText = CUSRESMessageProcessorTest.GetTestMessageResNo("10");
			AssertEquals("10", message.EntryStatus);
			AssertEquals(new ZDateTime(2016, 8, 2, 8, 44, 0), message.PreparationDate);
			interchange1.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+201608AA:0844+1299++CUSRES+++GWWTGTEST+1'";
			AssertEquals("10", message.EntryStatus);
			AssertEquals(new ZDateTime(2016, 8, 2, 8, 44, 0), message.PreparationDate);
			var interchange2 = Factory.New<ZACInterchange>();
			interchange2.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20161003:0944+1299++CUSRES+++GWWTGTEST+1'";

			message.EM_EI = interchange2.PK;
			AssertEquals(new ZDateTime(2016, 10, 3, 9, 44, 0), message.PreparationDate);

			message.EM_EI = interchange1.PK;
			AssertEquals(ZDateTime.Invalid, message.PreparationDate);
		}

		public void TestEntryStatusFields()
		{
			//var testMessage = Factory.New<ZAMessage>();
			//testMessage.EM_ReceiveTransmit = "TRX";
			//testMessage.EM_ApplicationCode = "ZAC";
			//testMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CUSDEC;
			//testMessage.EM_Status = "QUE";
			//testMessage.EM_MessageText = cusdecTestMessage.Replace("\r\n", "");
			//testMessage.EM_MessageNum = "IN1";

			//AssertEquals(ZString.Empty, testMessage.EntryStatus);
			//AssertEquals(ZString.Empty, testMessage.EntryStatusDescription);

			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();

			var testCUSRES = Factory.New<CUSRESEDIMessage>();
			testCUSRES.EM_ReceiveTransmit = "RCV";
			testCUSRES.EM_ApplicationCode = "ZAC";
			testCUSRES.EM_Status = "QUE";
			testCUSRES.EM_MessageText = cusresTestMessage.Replace("\r\n", "");
			testCUSRES.EM_MessageNum = "IN1";

			AssertEquals("1", testCUSRES.EntryStatus);
			AssertEquals("Release", testCUSRES.EntryStatusDescription);
		}

		public void TestDefaultValues()
		{
			var testMessage = Factory.New<CUSRESEDIMessage>();
			AssertEquals(CUSRESEDIMessage.MessageTypes.CUSRES, testMessage.EM_MessageType);
			AssertEquals(CUSRESEDIMessage.Direction.Receive, testMessage.EM_ReceiveTransmit);
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001234";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var interchange = Factory.New<ZACInterchange>();
			interchange.EI_HeaderText = "UNB+UNOB:4+SARSDECT+11200894::AAAAAAAAAAAAAABB:CORAS2+20160802:0844+1299++CUSRES+++GWWTGTEST+1'";
			var message = Factory.New<CUSRESEDIMessage>();
			message.EM_ReceiveTransmit = "RCV";
			message.EM_ApplicationCode = "ZAC";
			message.EM_Status = "QUE";
			message.EM_MessageText = cusresTestMessage.Replace("\r\n", "");
			message.EM_MessageNum = "IN1";
			message.EM_EI = interchange.PK;
			message.EM_LinkedObject = entry;

			AssertEquals("LRNNumber", "00626166JSA20160331008480", message.LocalReferenceNumber);
			AssertEquals("MRNNumber", "JSA201603315000938", message.MRNNumber);
			AssertEquals("ReceivingProfile", "11200894", message.ReceivingProfile);
			AssertEquals("AgentCode", "00626166", message.AgentCode);
			AssertEquals("CustomsOfficeCode", "JSA", message.CustomsOfficeCode);
			AssertEquals("TransportDocumentNumber", "083-01203226", message.TransportDocumentNumber);
			AssertEquals("ContainerNumbers", "SUDU1769365,SUDU1769366", message.ContainerNumbers);
			AssertEquals("LinkedObjectReference", "B00001234", message.LinkedObjectReference);
		}

		protected override string ExpectedReceiveTransmit => CUSRESEDIMessage.Direction.Receive;

		readonly string cusresTestMessage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166JSA20160331008480:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+1:120:ZZZ:N'
EQD+CN+SUDU1769365'
EQD+CN+SUDU1769366'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:202'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";
	}

	public class CUSRESEDIMessageForTest : CUSRESEDIMessage
	{
		internal string MessageNumForTesting { get; set; }

		public CUSRESEDIMessageForTest(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber()
		{
			return MessageNumForTesting ?? "0002";
		}

		public new ZDateTime EM_MessageDateTime
		{
			get { return base.EM_MessageDateTime; }
			set { EM_SystemCreateTimeUtc = value; }
		}
	}

	public class CUSRESEDIMessageForTesting_NullReference : CUSRESEDIMessage
	{
		internal string MessageNumForTesting { get; set; }

		public CUSRESEDIMessageForTesting_NullReference(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		public override ZString EM_MessageInterpretation => GetHumanFriendlyMessageInterpretation(null);
	}
}
