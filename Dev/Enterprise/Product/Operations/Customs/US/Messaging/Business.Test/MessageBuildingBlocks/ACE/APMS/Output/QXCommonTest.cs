namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output.Testing
{
	sealed class QXCommonTest : NUnit.Framework.TestCase
	{
		public void TestQXCommonIStatementRerouteResponse()
		{
			var common = new QXCommon();
			common.Data = " IMS9   THIS IS ACE PERIODIC MONTHLY STATEMENT     1                          ";
			var iStatementRerouteResponse = common as IStatementRerouteResponse;
			AssertEquals("MS9", iStatementRerouteResponse.ErrorCode);
			AssertEquals("THIS IS ACE PERIODIC MONTHLY STATEMENT", iStatementRerouteResponse.MessageText);
			AssertEquals(1, iStatementRerouteResponse.TotalNumberOfReroutes);
			common = new QXCommon();
			common.Data = "MS3THIS IS LEGACY PERIODIC MONTHLY ST  18                                     ";
			iStatementRerouteResponse = common;
			AssertEquals("MS3", iStatementRerouteResponse.ErrorCode);
			AssertEquals("THIS IS LEGACY PERIODIC MONTHLY ST", iStatementRerouteResponse.MessageText);
			AssertEquals(18, iStatementRerouteResponse.TotalNumberOfReroutes);
		}
	}
}
