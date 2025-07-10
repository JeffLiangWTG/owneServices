using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DEBADVEDIMessage))]
	public class DEBADVEDIMessageTest : SGEDIMessageTest
	{
		#region TradeNet 4.0
		public void TestDEBADV_1()
		{
			DEBADVEDIMessage message = Factory.New<DEBADVEDIMessage>();
			message.EM_MessageText = "UNH+1+DEBADV:D:05B:UN:040+FEEMSG'BGM+80+XXXXXXXXE01T        200609138891+9'BUS+1:LIF'DTM+137:200609130949:203'RFF+MS:AEBT.AEBT101'RFF+MR:E01T.E01T001'RFF+DM:AE/001693/2005/T'MOA+9:20.00:SGD'DTM+205:20060913:102'FTX+PMD+++ | | | | | | | | |50|LOYANG WAY| | | |508743| | | | | | |'FII+AO'UNT+12+1'";
			string expectedResult = "DEBIT ADVICE MESSAGE\r\n";
			expectedResult += "ISSUED ON 20060913 AT 0949\r\n";
			expectedResult += "LICENCE NO. : AE/001693/2005/T\r\n";
			expectedResult += "FEE         : 20.00 SGD\r\n";
			expectedResult += "DATE        : 20060913\r\n";
			expectedResult += "REMARKS     : | | | | | | | | |50|LOYANG WAY| | | |508743| | | | | | |";
			AssertMultilineASCIIEquals("", expectedResult, message.EM_MessageInterpretation);
		}

		public void TestDEBADV_2()
		{
			DEBADVEDIMessage message = Factory.New<DEBADVEDIMessage>();
			message.EM_MessageText = "UNH+1+DEBADV:D:05B:UN:040+FEEMSG'BGM+80+XXXXXXXXE01T        200609138888+9'BUS+1:LIF'DTM+137:200609130902:203'RFF+MS:RPIT.RPIT101'RFF+MR:E01T.E01T001'RFF+ABT:IG6B000002W'MOA+9:40.00:SGD'DTM+205:20060913:102'FII+AO'UNT+11+1'";
			string expectedResult = "DEBIT ADVICE MESSAGE\r\n";
			expectedResult += "ISSUED ON 20060913 AT 0902\r\n";
			expectedResult += "PERMIT NO.  : IG6B000002W\r\n";
			expectedResult += "FEE         : 40.00 SGD\r\n";
			expectedResult += "DATE        : 20060913\r\n";
			expectedResult += "REMARKS     : ";
			AssertMultilineASCIIEquals("", expectedResult, message.EM_MessageInterpretation);
		}

		#endregion
		#region TradeNet 4.1
		public void TestDebadv09B_1()
		{
			var message = Factory.New<DEBADVEDIMessage>();
			message.EM_MessageText = "UNH+1+DEBADV:D:09B:UN:041+FEEMSG'BGM+80+XXXXXXXXE01T     201102148888+9'BUS+1:LIF'DTM+137:201102140902:203'RFF+ABT:IG6B000002W'MOA+9:40.00:SGD'DTM+205:20110214:102'FII+AO'NAD+MS+RPIT.RPIT101'NAD+MR+E01T.E01T001'UNT+11+1'";
			var expectedResult = "DEBIT ADVICE: FEE MESSAGE\r\n";
			expectedResult += "ISSUED ON 14-FEB-2011 AT 09:02\r\n";
			expectedResult += "PERMIT NO.      : IG6B000002W\r\n";
			expectedResult += "FEE             : 40.00 SGD\r\n";
			expectedResult += "SETTLEMENT DATE : 14-FEB-2011\r\n";
			expectedResult += "REMARKS         : ";
			AssertMultilineASCIIEquals("", expectedResult, message.EM_MessageInterpretation);
		}

		public void TestDebadv09B_2()
		{
			var message = Factory.New<DEBADVEDIMessage>();
			message.EM_MessageText = "UNH+1+DEBADV:D:09B:UN:041+FEEMSG'BGM+80+XXXXXXXXE01T     201102148889+9'BUS+1:LIF'DTM+137:201102140924:203'RFF+DM:AE/001688/2005/T'MOA+9:20.00:SGD'DTM+205:20110214:102'FTX+PMD+++ | | | | | | | | |301|JLN AHMAD IBRAHIM| | | |639526| | | | | | |'FII+AO'NAD+MS+AEBT.AEBT101'NAD+MR+E01T.E01T001'UNT+12+1'";
			var expectedResult = "DEBIT ADVICE: FEE MESSAGE\r\n";
			expectedResult += "ISSUED ON 14-FEB-2011 AT 09:24\r\n";
			expectedResult += "LICENCE NO.     : AE/001688/2005/T\r\n";
			expectedResult += "FEE             : 20.00 SGD\r\n";
			expectedResult += "SETTLEMENT DATE : 14-FEB-2011\r\n";
			expectedResult += "REMARKS         : | | | | | | | | |301|JLN AHMAD IBRAHIM| | | |639526| | | | | | |";
			AssertMultilineASCIIEquals("", expectedResult, message.EM_MessageInterpretation);
		}

		#endregion
	}
}
