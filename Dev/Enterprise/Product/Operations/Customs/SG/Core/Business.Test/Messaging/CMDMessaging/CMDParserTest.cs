using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	internal class CMDParserTest : TestCase
	{
		public void TestValidateMessage()
		{
			CMDParser parser = new CMDParser("testing123");
			Assert("Invalid input", !parser.IsValid);
			parser = new CMDParser("CMD/2\r\nads");
			Assert("Invalid input, header length is invalid", !parser.IsValid);
			parser = new CMDParser("CMD/2\r\nA/Y/N");
			Assert(parser.IsValid);
			parser = new CMDParser("CMD/2\r\nA/N/NOtherthingshere");
			Assert(parser.IsValid);
			parser = new CMDParser("XXXweirueCMD/2\r\nA/Y/N");
			Assert("Invalid input, CMD Message has to start with CMD", !parser.IsValid);
		}

		public void TestActionCode()
		{
			CMDParser parser = new CMDParser("CMD/2\r\nA/Y/N");
			AssertEquals(CMD.ActionCodes.Add, parser.ActionCode);
			parser = new CMDParser("CMD/2\r\nM/Y/N");
			AssertEquals(CMD.ActionCodes.Modify, parser.ActionCode);
			parser = new CMDParser("CMD/2\r\nD/Y/N");
			AssertEquals(CMD.ActionCodes.Delete, parser.ActionCode);
			parser = new CMDParser("CMD/2\r\n/LMNNAOWIWU");
			AssertEquals("/ is an invalid Action code, should be set to the default", (CMD.ActionCodes)0, parser.ActionCode);
			parser = new CMDParser("balksjfa\r\nlksjfdalksdflaksj\r\nflasjdflaks0iweroi\r\nasdasd\r\nCMD/2\r\nM/Y/N");
			AssertEquals("CMD message is invalid, action code should be set to the default", (CMD.ActionCodes)0, parser.ActionCode);
		}

		public void TestSetActionCode()
		{
			CMDParser parser = new CMDParser("CMD/2\r\nA/Y/N\r\nSomeOtherLine\r\nAndOthers");
			AssertEquals(CMD.ActionCodes.Add, parser.ActionCode);
			parser.ActionCode = CMD.ActionCodes.Delete;
			AssertEquals(CMD.ActionCodes.Delete, parser.ActionCode);
			AssertEquals("CMD/2\r\nD/Y/N\r\nSomeOtherLine\r\nAndOthers", parser.ModifiedMessageText);
			parser.ActionCode = CMD.ActionCodes.Modify;
			AssertEquals("CMD/2\r\nM/Y/N\r\nSomeOtherLine\r\nAndOthers", parser.ModifiedMessageText);
			parser.ActionCode = CMD.ActionCodes.Add;
			AssertEquals("CMD/2\r\nA/Y/N\r\nSomeOtherLine\r\nAndOthers", parser.ModifiedMessageText);
		}

		public void TestLateIndicator()
		{
			CMDParser parser = new CMDParser("CMD/2\r\nA/N/Y");
			AssertEquals(true, parser.IsLate);
			parser = new CMDParser("CMD/2\r\nA/N/N");
			AssertEquals(false, parser.IsLate);
		}

		public void TestMasterBillNum()
		{
			CMDParser parser = new CMDParser("CMD/2\r\nA/Y/N");
			AssertEquals("", parser.MasterBillNumber);
			parser = new CMDParser("CMD/2\r\nA/Y/N\r\nMWB/Testing12345");
			AssertEquals("Testing12345", parser.MasterBillNumber);
		}

		public void TestHouseBillNum()
		{
			CMDParser parser = new CMDParser("CMD/2\r\nA/Y/N\r\nMWB/Testing12345/23/lkjasd");
			AssertEquals("", parser.HouseBillNumber);
			parser = new CMDParser("CMD/2\r\nA/Y/N\r\nHWB/Testing88888/234/lkasdoiuw");
			AssertEquals("Testing88888", parser.HouseBillNumber);
			parser = new CMDParser("CMD/2\r\nA/Y/N\r\nHWB/123456789012345678/234/lkasdoiuw");
			AssertEquals("12345678901234567", parser.HouseBillNumber);
		}
	}
}
