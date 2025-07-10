using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(TRIMessageObject))]
	sealed class TRIMessageObjectTest : SoapMessageObjectAbstractTest<TRIMessageObject>
	{
		public void TestErrorMessageObjcet()
		{
			var messageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
	<s:Body>
		<ETGBMuayeneMemuruSorgulaResponse xmlns=""http://tempuri.org/"">
			<ETGBMuayeneMemuruSorgulaResult>&lt;?xml version=""1.0""?&gt;&lt;Sonuc xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""&gt;&lt;Hatalar&gt;&lt;Hata&gt;&lt;HataAciklamasi&gt;Kullanıcı kodu ve şifrenizi kontrol ediniz!&lt;/HataAciklamasi&gt;&lt;/Hata&gt;&lt;/Hatalar&gt;&lt;KayitTarihi&gt;0001-01-01T00:00:00&lt;/KayitTarihi&gt;&lt;/Sonuc&gt;</ETGBMuayeneMemuruSorgulaResult>
		</ETGBMuayeneMemuruSorgulaResponse>
	</s:Body>
</s:Envelope>";

			var messageObject = new TRIMessageObject(messageText, Factory.New<ETradeEDIMessage>(), new LoggingInformation());
			CombineAssertions(() =>
			{
				var responseObjcet = messageObject.InnerMessageObjects.FirstOrDefault() as TRIMessageResponseObject;
				AssertType<TRIMessageResponseObject>(responseObjcet);
				AssertEquals("messages count", 1, responseObjcet.Errors.Count);
				AssertEquals("Kullanıcı kodu ve şifrenizi kontrol ediniz!", responseObjcet.Errors[0].Description);
			});
		}

		public void TestSuccessMessageObjcet()
		{
			var messageText = TRMessageTestHelper.GetFileText("ETrade.QueryForInspectionClerk.QueryForInspectionClerkSuccess.xml");

			var messageObject = new TRIMessageObject(messageText, Factory.New<ETradeEDIMessage>(), new LoggingInformation());
			CombineAssertions(() =>
			{
				var responseObjcet = messageObject.InnerMessageObjects.FirstOrDefault();
				AssertType<InnerMessageObjectBase>(responseObjcet);
				AssertEquals("TEST KULLANICISI", responseObjcet.MessageText);
			});
		}
	}
}
