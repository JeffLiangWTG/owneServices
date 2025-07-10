using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.TR.Messaging.Testing
{
	class ETradeQueryForRegNoMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGetMessageText()
		{
			var messageHeaderBuilder = new ETradeQueryForRegNoMessageBuilder();
			var mock = new Mock<IETradeQueryForRegNo>();
			mock.Setup(m => m.UserName).Returns("20201224104");
			mock.Setup(m => m.UserPassword).Returns("25d55ad283aa400af464c76d713c07ad");
			mock.Setup(m => m.TemporaryRegistrationNo).Returns("22066666GI0000000036");
			var result = messageHeaderBuilder.GetMessageText(mock.Object);
			var xmlMessage = TRMessageTestHelper.GetFileText("ETradeQueryForRegNoMessageTest.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.QueryRegistrationNo.").ToString();
			AssertEquals(xmlMessage, result);
		}
	}
}
