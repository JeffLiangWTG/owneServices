using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Messaging.Testing
{
	class ETradeComplementaryDecMessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2020, 12, 31)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestExpectedXml()
		{
			TestDateAttribute.UseUNLOCO = true;

			var iBillComplementary = new Mock<IBillComplementary>();
			iBillComplementary.Setup(m => m.BillNo).Returns("1");
			iBillComplementary.Setup(m => m.ConsigneeTaxIDNo).Returns("20201224104");
			iBillComplementary.Setup(m => m.DeliveryDate).Returns(ZDateTime.Today);

			var iETradeComplementaryDec = new Mock<IETradeComplementaryDec>();
			iETradeComplementaryDec.Setup(m => m.DeclarationOwnerRepresentativeNameAndTitle).Returns("WiseTech Global");
			iETradeComplementaryDec.Setup(m => m.DeclarationOwnerRepresentativeTaxNo).Returns("12453687521");
			iETradeComplementaryDec.Setup(m => m.RegistrationNo).Returns("testRegNoforTest");

			iETradeComplementaryDec.Setup(m => m.Bills).Returns(new[] { iBillComplementary.Object });

			var eTradeComplementaryDecMessageBuilder = new ETradeComplementaryDecMessageBuilder(iETradeComplementaryDec.Object);

			var messageText = TRMessageTestHelper.GetFileText("ETrade.ETradeComplementaryDeclerationTest.xml");
			var actualMessage = eTradeComplementaryDecMessageBuilder.GetMessageText("12345678901", "12345678", "ETR0000001");

			AssertContains("messageText", messageText, actualMessage);
			iBillComplementary.VerifyAll();
		}
	}
}
