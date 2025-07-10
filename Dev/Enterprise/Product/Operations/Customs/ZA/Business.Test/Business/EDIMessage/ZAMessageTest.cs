using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Integration.Testing;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ZAMessage))]
	sealed class ZAMessageTest : EDIMessageTest
	{
		public void TestICusCodeDataTypeSupporter()
		{
			var message = Factory.New<ZAMessageForTest>();
			ICusCodeDataTypeSupporter supporter = message;
			supporter.AssertType(typeof(VoucherOfCorrectionValueAfter), CusCodeDataTypeList.Codes.VOCValueAfter);
			supporter.AssertType(typeof(VoucherOfCorrectionValueBefore), CusCodeDataTypeList.Codes.VOCValueBefore);
			supporter.AssertType(null, CusCodeDataTypeList.Codes.CaseNumber);
			supporter.AssertType(null, "ZZ!");

			var vocAfter = message.VoucherOfCorrectionValueAfters.AddNew();
			vocAfter.CY_Value = 100m;
			var vocBefore = message.VoucherOfCorrectionValueBefores.AddNew();
			vocBefore.CY_Value = 100m;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<Customs.Business.CusCodeData>(vocAfter.PK);
			AssertEquals(typeof(VoucherOfCorrectionValueAfter), codeData.GetType());
			codeData = newFactory.Load<Customs.Business.CusCodeData>(vocBefore.PK);
			AssertEquals(typeof(VoucherOfCorrectionValueBefore), codeData.GetType());
		}

		public void TestPreparationDate()
		{
			var interchange1 = Factory.New<ZACInterchange>();
			interchange1.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160802:0844+1299++CUSRES+++GWWTGTEST+1'";
			var message = (ZAMessage)interchange1.ContainedMessages.AddNew(typeof(ZAMessage));
			AssertEquals(new ZDateTime(2016, 8, 2, 8, 44, 0), message.PreparationDate);
			message.EM_MessageText = CUSRESMessageProcessorTest.GetTestMessageResNo("9");
			AssertEquals(new ZDateTime(2016, 8, 2, 8, 44, 0), message.PreparationDate);
			message.EM_MessageText = CUSRESMessageProcessorTest.GetTestMessageResNo("10");
			AssertEquals(new ZDateTime(2016, 8, 2, 8, 44, 0), message.PreparationDate);
			interchange1.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+201608AA:0844+1299++CUSRES+++GWWTGTEST+1'";
			AssertEquals(new ZDateTime(2016, 8, 2, 8, 44, 0), message.PreparationDate);
			var interchange2 = Factory.New<ZACInterchange>();
			interchange2.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20161003:0944+1299++CUSRES+++GWWTGTEST+1'";
			message.EM_EI = interchange2.PK;
			AssertEquals(new ZDateTime(2016, 10, 3, 9, 44, 0), message.PreparationDate);
			message.EM_EI = interchange1.PK;
			AssertEquals(ZDateTime.Invalid, message.PreparationDate);
			interchange2.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+161003:0944+1299++CUSRES+++GWWTGTEST+1'";
			message.EM_EI = interchange2.PK;
			AssertEquals(new ZDateTime(2016, 10, 3, 9, 44, 0), message.PreparationDate);
		}

		public void TestApplicationCode()
		{
			var message = (ZAMessage)GetNewBusinessObject();
			AssertEquals(ZAMessage.ApplicationCodes.SouthAfricanCustoms, message.EM_ApplicationCode);
		}

		public void TestVOCReasonNote()
		{
			ZAMessage message1 = Factory.NewWithValidTestData<ZAMessage>();
			message1.SetVOCReason("TestReason");
			Factory.Save();

			ZQuery filter = new ZQuery(EDIMessageSchema.PK, message1.PK);
			var message2 = Factory.LoadTop1<ZAMessage>(filter);

			AssertEquals(message1.VOCReason, message2.VOCReason);
		}

		public void TestLocalReferenceNumber()
		{
			var testCUSDEC = Factory.New<ZAMessage>();
			testCUSDEC.EM_MessageNum = "66";
			testCUSDEC.EM_MessageText = CUSDECTestMessage.Replace("\r\n", "");

			AssertEquals(ZString.Empty, testCUSDEC.LocalReferenceNumber);
		}

		public void TestParentMessageNumber()
		{
			var testCUSDEC = Factory.New<ZAMessage>();
			testCUSDEC.EM_MessageNum = "66";
			testCUSDEC.EM_MessageText = CUSDECTestMessage.Replace("\r\n", "");

			AssertEquals(ZString.Empty, testCUSDEC.ParentMessageNumber);
		}

		public void TestEntryStatusFields()
		{
			var testMessage = Factory.New<CUSDECEDIMessage>();
			testMessage.EM_ReceiveTransmit = "TRX";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = CUSDECTestMessage.Replace("\r\n", "");
			testMessage.EM_MessageNum = "IN1";

			AssertEquals(ZString.Empty, testMessage.EntryStatus);
			AssertEquals(ZString.Empty, testMessage.EntryStatusDescription);
		}

		public void TestEM_MessageInterpretation()
		{
			var zaTestHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			zaTestHelper.CreateCustomsOfficeCusCodeEntry("JSA");
			zaTestHelper.CreateCustomsStatusCusCodeEntry("6");
			Factory.Save();

			var testMessage = Factory.New<CUSRESEDIMessage>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = CUSRES_FTX_Message.Replace("\r\n", "");
			testMessage.EM_MessageNum = "IN1";

			AssertEquals("<p><b><u>Status Code 6 - Reject To Clearer</u></b></p><p><b><u>Entry Header Message/s:</u></b></p><p> FIELD(Depot/Terminal Code) DESCR(Field must be empty)</p><p> FIELD(Depot/Terminal Code) DESCR(Invalid Transport Code/Removal Transport Code for Depot)</p><p><b><u>Entry Line Message/s:</u></b></p><p><b>Line 1:</b>  FIELD(Duty/Tax/Fee) DESCR(Amount Due Differences VAT - Calculated = 644.00 and TaxAssessed = 616.00)</p><p><b>Line 2:</b>  FIELD(Duty/Tax/Fee) DESCR(Amount Due Differences 1P1 - Calculated = 200.00 and TaxAssessed = null)</p><p><b><u>Entry Summary Message/s:</u></b></p><p>Samples to be declared according to the export price list of identic al goods/open market value.  VOC required to declare such value plus a PP req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read</p>", testMessage.EM_MessageInterpretation);

			AssertExceptionThrown(typeof(NotSupportedException), "Setting EM_MessageInterpretation is not supported for CUSRES message", () =>
			{
				testMessage.EM_MessageInterpretation = string.Empty;
			});

			var testMessage1 = Factory.New<CUSDECEDIMessage>();
			testMessage1.EM_ReceiveTransmit = "RCV";
			testMessage1.EM_ApplicationCode = "ZAC";
			testMessage1.EM_Status = "QUE";
			testMessage1.EM_MessageText = CUSRES_FTX_Message.Replace("\r\n", "");
			testMessage1.EM_MessageNum = "IN1";

			AssertEquals("UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JSA20160822001311:0'DTM+178:20160822:102'TDT+20+SA345+0+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A8::ZZZ'GIS+6:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:083-12345675'DTM+137:20160822:102'RFF+ACD:1312'ERP+1:0'ERC+8463::ZZZ'FTX+AAO+++ FIELD(Depot/Terminal Code) DESCR(Field must be empty)'ERP+1:0'ERC+1173::ZZZ'FTX+AAO+++ FIELD(Depot/Terminal Code) DESCR(Invalid Transport Code/Removal Trans:port Code for Depot)'ERP+2:1'ERC+1129::ZZZ'FTX+AAO+++ FIELD(Duty/Tax/Fee) DESCR(Amount Due Differences VAT - Calculated = 6:44.00 and TaxAssessed = 616.00)'ERP+2:2'ERC+1129::ZZZ'FTX+AAO+++ FIELD(Duty/Tax/Fee) DESCR(Amount Due Differences 1P1 - Calculated = 2:00.00 and TaxAssessed = null)'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'TAX+3+CUS:107:ZZZ'MOA+161:0'CNT+7:10.00'CNT+11:1'UNT+28+1'", testMessage1.EM_MessageInterpretation);

			AssertNoExceptionThrown(() =>
			{
				testMessage1.EM_MessageInterpretation = string.Empty;
			});
		}

		internal const string CONTRLTestMessage = @"UNH+1+CONTRL:D:3:UN:CONTRL'
UCI+00000000027997+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSREQT+7'
UCM+00000000000062+REQDOC:D:99B:UN:ZZZ01+7'
UNT+4+1'
";

		internal const string CUSRESTestMessage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166JSA20160331008480:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+1:120:ZZZ:N'
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

		internal const string CUSDECTestMessage = @"UNH+66+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+01020304JSA20160708000064::00002+9'
CST++A:117:ZZZ'
LOC+14+G5::ZZZ'
LOC+18+JHB0001::ZZZ'
LOC+35+DE::5'
LOC+36+ZA::5'
LOC+96+JSA::ZZZ'
LOC+9+DEBER::5'
GIS+V:134:ZZZ'
FTX+LIN+++1::N'
RFF+AAS:083-00000000'
DTM+137:20160607:102'
RFF+ABI:0123456789'
RFF+ACD:66'
TDT+20'
NAD+IM+00010005++SCOOBY DOO+THE MYSTERY MACHINE'
RFF+VA:4123546789'
NAD+AG+01020304'
NAD+SU+++SCOOBY DOO+THE MYSTERY MACHINE'
NAD+MS+51051342'
NAD+BY+00010005'
UNS+D'
CST+0001+853610007:108:ZZZ+100'
FTX+AAA+++FUSES'
FTX+ACB+++NUIN'
FTX+CCI+++11:40'
LOC+27+DE'
MEA+AAR++KG:100.00'
MEA+AAF++KG:100'
MOA+38:100'
MOA+40:100'
RFF+WE:DBN201501035000012'
TAX+1+VAT:107:ZZZ'
MOA+161:15.40'
UNS+S'
TAX+3+TVD:107:ZZZ'
MOA+161:15.40'
TAX+3+CUS:107:ZZZ'
MOA+161:100'
UNT+41+66'";

		const string CUSRES_FTX_Message = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00505655JSA20160822001311:0'
DTM+178:20160822:102'
TDT+20+SA345+0+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A8::ZZZ'
GIS+6:120:ZZZ:N'NAD+AG+00505655'
RFF+AAS:083-12345675'
DTM+137:20160822:102'
RFF+ACD:1312'
ERP+1:0'
ERC+8463::ZZZ'
FTX+AAO+++ FIELD(Depot/Terminal Code) DESCR(Field must be empty)'
ERP+1:0'
ERC+1173::ZZZ'
FTX+AAO+++ FIELD(Depot/Terminal Code) DESCR(Invalid Transport Code/Removal Trans:port Code for Depot)'
ERP+2:1'
ERC+1129::ZZZ'
FTX+AAO+++ FIELD(Duty/Tax/Fee) DESCR(Amount Due Differences VAT - Calculated = 6:44.00 and TaxAssessed = 616.00)'
ERP+2:2'
ERC+1129::ZZZ'FTX+AAO+++ FIELD(Duty/Tax/Fee) DESCR(Amount Due Differences 1P1 - Calculated = 2:00.00 and TaxAssessed = null)'
ERP+6:0'
ERC+100::ZZZ'
FTX+AAO+++Samples to be declared according to the export price list of identic al: goods/open market value.  VOC required to declare such value plus a P:P req i.t.o. Sect 91 for the contravention of Sect 40(1)(c) read'
TAX+3+CUS:107:ZZZ'
MOA+161:0'CNT+7:10.00'
CNT+11:1'
UNT+28+1'";
	}

	public class ZAMessageForTest : ZAMessage
	{
		internal string MessageNumForTesting { get; set; }

		public ZAMessageForTest(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber()
		{
			return MessageNumForTesting;
		}
	}
}
