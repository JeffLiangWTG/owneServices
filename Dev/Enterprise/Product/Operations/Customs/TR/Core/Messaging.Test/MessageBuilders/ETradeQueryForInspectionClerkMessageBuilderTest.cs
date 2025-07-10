using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.TR.Messaging.Test
{
	class ETradeQueryForInspectionClerkMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGetMessageText()
		{
			var expected = @"<soapenv:Envelope xmlns:tem=""http://tempuri.org/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <tem:ETGBMuayeneMemuruSorgula>
      <tem:kullaniciAdi>20201224104</tem:kullaniciAdi>
      <tem:kullaniciSifre>25d55ad283aa400af464c76d713c07ad</tem:kullaniciSifre>
      <tem:beyannameNo>22066666GI0000000036</tem:beyannameNo>
    </tem:ETGBMuayeneMemuruSorgula>
  </soapenv:Body>
</soapenv:Envelope>";

			var messageHeaderBuilder = new ETradeQueryForInspectionClerkMessageBuilder();
			var mock = new Mock<IETradeQueryForInspectionClerk>();
			mock.Setup(m => m.UserName).Returns("20201224104");
			mock.Setup(m => m.UserPassword).Returns("25d55ad283aa400af464c76d713c07ad");
			mock.Setup(m => m.RegistrationNo).Returns("22066666GI0000000036");
			var result = messageHeaderBuilder.GetMessageText(mock.Object);
			AssertEquals(expected, result);
		}
	}
}
