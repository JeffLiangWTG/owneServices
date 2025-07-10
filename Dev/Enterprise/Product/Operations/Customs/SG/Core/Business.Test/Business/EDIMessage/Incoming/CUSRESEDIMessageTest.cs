using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CUSRESEDIMessage))]
	public class CUSRESEDIMessageTest : SGEDIMessageTest
	{
		public void TestCUSRES()
		{
			var message = Factory.New<CUSRESEDIMessage>();
			message.EM_MessageText = "UNH+1+CUSRES:D:05B:UN:040+STATUS'BGM+961:::CAC+XXXXXXXXE01T        200609130111'FTX+ACD+++APPROVED ON 20060913'RFF+ACW:ME6I309961C'RFF+MR:E01T.E01T001'RFF+MS:DCST.DCST201'UNT+7+1'";
			AssertEquals("CANCELLATION APPROVAL", message.EM_MessageInterpretation);
		}

		#region TradeNet 4.1
		public void TestCusres09B()
		{
			var message = Factory.New<CUSRESEDIMessage>();
			message.EM_MessageText = "UNH+1+CUSRES:D:09B:UN:041+STATUS'BGM+961:::CAC+XXXXXXXXE01T     201102140111'FTX+ACD++A01+APPROVED ON 20110214'RFF+ACW:ME6I309961C'RFF+MR:E01T.E01T001'RFF+MS:DCST.DCST201'DTM+416:20110214002800SST:304'UNT+8+1'";
			var expectedResult = "CANCELLATION APPROVAL\r\n\r\n";
			expectedResult += "Singapore Customs Approval of Request of Cancellation of Permit\r\n";
			expectedResult += "APPROVED ON 20110214";
			AssertMultilineASCIIEquals("", expectedResult, message.EM_MessageInterpretation);
		}

		#endregion
	}
}
