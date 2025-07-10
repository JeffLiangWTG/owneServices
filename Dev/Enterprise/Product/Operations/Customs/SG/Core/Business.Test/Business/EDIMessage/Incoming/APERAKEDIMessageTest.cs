using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(APERAKEDIMessage))]
	public class APERAKEDIMessageTest : SGEDIMessageTest
	{
		#region TradeNet 4.0
		public void TestAPERAK_ERROR_1()
		{
			APERAKEDIMessage message = Factory.New<APERAKEDIMessage>();
			message.EM_MessageText = "UNH+1+APERAK:D:05B:UN:040+STATUS'BGM+963:::ARD+XXXXXXXXE01T        200609139014'RFF+MR:E01T.E01T001'RFF+MS:DCST.DCST201'ERC+CAREJ'FTX+AAO+++PERMIT APPLICATION NOT APPROVED PLS CHECK EXPIRY DATE       LCEN 1:END OF MESSAGES'UNT+7+1'";
			string expectedResult = "ERROR : CONTROLLING AGENCY REJECTION\r\n\r\n";
			expectedResult += "CODE : CAREJ\r\n\r\n";
			expectedResult += "PERMIT APPLICATION NOT APPROVED PLS CHECK EXPIRY DATE       LCEN 1\r\nEND OF MESSAGES";
			AssertEquals(expectedResult, message.EM_MessageInterpretation);
		}

		public void TestAPERAK_ERROR_2()
		{
			APERAKEDIMessage message = Factory.New<APERAKEDIMessage>();
			message.EM_MessageText = "UNH+1+APERAK:D:05B:UN:040+STATUS'BGM+963:::CRD+XXXXXXXXE01T        200609139013'RFF+MR:E01T.E01T001'RFF+MS:DCST.DCST201'ERC+CRDETAIL'FTX+AAO+++THE FOLLOWING FIELDS ARE ERRONEOUS *ALCOHOLIC STRENG1                  'RFF+LI:1'ERC+CRDETAIL'FTX+AAO+++THE FOLLOWING FIELDS ARE ERRONEOUS *ALCOHOLIC STRENG2                  'RFF+LI:2'ERC+CRDETAIL'FTX+AAO+++THE FOLLOWING FIELDS ARE ERRONEOUS *ALCOHOLIC STRENG3                  'RFF+LI:3'ERC+CRDETAIL'FTX+AAO+++THE FOLLOWING FIELDS ARE ERRONEOUS *ALCOHOLIC STRENG4                  'RFF+LI:4'ERC+CRDETAIL'FTX+AAO+++THE FOLLOWING FIELDS ARE ERRONEOUS *ALCOHOLIC STRENG5                  'RFF+LI:5'ERC+CRDETAIL'FTX+AAO+++THE FOLLOWING FIELDS ARE ERRONEOUS *ALCOHOLIC STRENG6                  'RFF+LI:6'UNT+23+1'";
			string expectedResult = "ERROR : SINGAPORE CUSTOMS REJECTION\r\n\r\n";
			expectedResult += "CODE : CRDETAIL\r\n\r\n";
			expectedResult += "THE FOLLOWING FIELDS ARE ERRONEOUS *ALCOHOLIC STRENG1";
			expectedResult += "\r\n\r\n";
			expectedResult += "CODE : CRDETAIL\r\n\r\n";
			expectedResult += "THE FOLLOWING FIELDS ARE ERRONEOUS *ALCOHOLIC STRENG2";
			expectedResult += "\r\n\r\n";
			expectedResult += "CODE : CRDETAIL\r\n\r\n";
			expectedResult += "THE FOLLOWING FIELDS ARE ERRONEOUS *ALCOHOLIC STRENG3";
			expectedResult += "\r\n\r\n";
			expectedResult += "CODE : CRDETAIL\r\n\r\n";
			expectedResult += "THE FOLLOWING FIELDS ARE ERRONEOUS *ALCOHOLIC STRENG4";
			expectedResult += "\r\n\r\n";
			expectedResult += "CODE : CRDETAIL\r\n\r\n";
			expectedResult += "THE FOLLOWING FIELDS ARE ERRONEOUS *ALCOHOLIC STRENG5";
			expectedResult += "\r\n\r\n";
			expectedResult += "CODE : CRDETAIL\r\n\r\n";
			expectedResult += "THE FOLLOWING FIELDS ARE ERRONEOUS *ALCOHOLIC STRENG6";
			AssertEquals(expectedResult, message.EM_MessageInterpretation);
		}

		public void TestAPERAK_ERROR_3()
		{
			APERAKEDIMessage message = Factory.New<APERAKEDIMessage>();
			message.EM_MessageText = "UNH+1+APERAK:D:05B:UN:040+ERRORM'BGM+963+12247970000Z        200707140868'ERC+E15253'FTX+AAO+++OUTWARD TRANSPORT DETAILS HAS TO BE SPECIFIED:4:TDT:3#0'UNT+5+1'";
			string expectedResult = "ERROR : SINGAPORE CUSTOMS REJECTION\r\n\r\nCODE : E15253\r\n\r\nOUTWARD TRANSPORT DETAILS HAS TO BE SPECIFIED\r\n4";
			AssertEquals(expectedResult, message.EM_MessageInterpretation);
		}

		#endregion
		#region TradeNet 4.1
		public void TestAperak09b_ERROR_1()
		{
			var message = Factory.New<APERAKEDIMessage>();
			message.EM_MessageText = "UNH+1+APERAK:D:09B:UN:041+ERRORM'BGM+963+XXXXXXXXE01T     201102141101'ERC+E17007'FTX+AAO+++PORT OF LOADING DOES NOT EXIST IN THE DATABASE:0#0:LOC#4:2#1'UNT+5+1'";
			var expectedResult = "ERROR : SINGAPORE CUSTOMS REJECTION\r\n\r\n";
			expectedResult += "CODE : E17007\r\n";
			expectedResult += "PORT OF LOADING DOES NOT EXIST IN THE DATABASE\r\n0#0";
			AssertEquals(expectedResult, message.EM_MessageInterpretation);
		}

		public void TestAperak09b_ERROR_2()
		{
			var message = Factory.New<APERAKEDIMessage>();
			message.EM_MessageText = "UNH+1+APERAK:D:09B:UN:041+STATUS'BGM+963:::ARD+XXXXXXXXE01T     201102149011'RFF+MR:E01T.E01T001'RFF+MS:DCST.DCST201'ERC+CAREJ'FTX+AAO+++R01 - PLEASE CHECK DEPARTURE DATE.:FOR ALL ITEMS.'UNT+7+1'";
			var expectedResult = "Controlling Agency Reject Declaration\r\n\r\n";
			expectedResult += "CODE : CAREJ\r\n";
			expectedResult += "R01 - PLEASE CHECK DEPARTURE DATE.\r\nFOR ALL ITEMS.";
			AssertEquals(expectedResult, message.EM_MessageInterpretation);
		}

		public void TestAperak09b_ERROR_3()
		{
			var message = Factory.New<APERAKEDIMessage>();
			message.EM_MessageText = "UNH+1+APERAK:D:09B:UN:041+ERRORM'BGM+963+XXXXXXXXE01T     201102141103'ERC+E18004'FTX+AAO+++AMENDMENT REQUEST EXCEEDED 48 HOURS:0#0:#0:0#0'UNT+5+1'";
			var expectedResult = "ERROR : SINGAPORE CUSTOMS REJECTION\r\n\r\n";
			expectedResult += "CODE : E18004\r\n";
			expectedResult += "AMENDMENT REQUEST EXCEEDED 48 HOURS\r\n0#0";
			AssertEquals(expectedResult, message.EM_MessageInterpretation);
		}

		#endregion
	}
}
