using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(DK1MessageObject))]
	class DK1MessageObjectTest : SoapMessageObjectAbstractTest<DK1MessageObject>
	{
		public void TestErrorMessage()
		{
			var messageObject = new DK1MessageObject(TRMessageTestHelper.GetFileText("DK1Error.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), Factory.New<TRImportExportMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized Error Message xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.FirstOrDefault() as InnerXmlControlAnswerObject;
				AssertType<InnerXmlControlAnswerObject>("DK1MessageErrorMessageObject", innerMessageObject);
				AssertEquals("ErrorMessages count", 2, innerMessageObject.Errors.Count);
				AssertEquals("DK1MessageErrorMessageObject.ErrorMessage", "Açmalarda esya ambar içinde seçilmis. Tasima senedi henüz ambara alinmamis ya da ambar çikis islemi yapilmistir. (226362740)", innerMessageObject.Errors.First().Description);
			});

			var messageObject2 = new DK1MessageObject(TRMessageTestHelper.GetFileText("DK1Error2.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), Factory.New<TRImportExportMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized Error Message xml correctly.", () =>
			{
				var innerMessageObject2 = messageObject2.InnerMessageObjects.FirstOrDefault() as InnerXmlControlAnswerObject;
				AssertType<InnerXmlControlAnswerObject>("DK1MessageErrorMessageObject", innerMessageObject2);
				AssertEquals("ErrorMessages count", 1, innerMessageObject2.Errors.Count);
				AssertEquals("DK1MessageErrorMessageObject.ErrorMessage", "066666 --> Gümrük Veri Tabanı Bağlantı Hatası", innerMessageObject2.Errors.First().Description);
			});
		}

		public void TestSucceedObject()
		{
			var messageObject = new DK1MessageObject(TRMessageTestHelper.GetFileText("DK1Success.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), Factory.New<TRImportExportMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized Succeedd Message xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.FirstOrDefault() as InnerXmlControlAnswerObject;
				AssertType<InnerXmlControlAnswerObject>("DK1MessageSucceedObject", innerMessageObject);
				AssertEquals("Entry Question count", 5, innerMessageObject.Questions.Count);
				AssertEquals("Question Message", 7780, innerMessageObject.Questions.First().Code);
				AssertEquals("Document count", 9, innerMessageObject.Documents.Count);
				AssertEquals("Document Message", "0887", innerMessageObject.Documents.First().Code);
				AssertEquals("Tax count", 5, innerMessageObject.Taxes.Count);
				AssertEquals("Tax Message", "10", innerMessageObject.Taxes.First().Code);
			});
		}
	}
}
