using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	class ZAEDIMessageValidationTest : TestCaseWithFactory
	{
		public void TestCheckEM_MessageNum()
		{
			var cusresText = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00505655JSA20191021011281:0'
DTM+178:20191021:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+62::ZZZ'
GIS+14:120:ZZZ:N'
NAD+AG+00626166'
RFF+AAS:083-32154323'
DTM+137:20191020:102'
RFF+ABT:JSA201910215000501'
DTM+137:20191021:102'
RFF+ACD:7615'
RFF+AAV:105394631'
ERP+1:0'
ERC+0000::ZZZ'
FTX+AAO+++PLEASE SUBMIT YOUR SUPPORTING DOCUMENTS ELECTRONICALLY'
TAX+3+CUS:107:ZZZ'
MOA+161:166'
CNT+7:1.13'
CNT+11:16'
UNT+22+1'";
			var msg = GetMesssageForTest(cusresText.Replace("\r\n", ""));
			msg.EM_MessageType = SARSEDIMessage.MessageTypes.CUSRES;
			msg.EM_Status = ZAMessage.Status.Discarded;
			msg.Validation.ValidateEM_MessageNum();

			Assert(msg.GetWarnings().Any(x => x.Message.Contains("Warning - EDI Message: Unable to link this CUSRES back to entry. CUSRES has been linked via LRN. This message has been discarded.")));
		}

		CUSRESEDIMessageForTest GetMesssageForTest(ZString messageText)
		{
			var testMessage = Factory.NewWithValidTestData<CUSRESEDIMessageForTest>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = messageText;
			testMessage.EM_SystemCreateTimeUtc = new ZDateTime(2016, 9, 29, 1, 5, 0);
			testMessage.EM_MessageNum = "IN1";
			return testMessage;
		}
	}
}
