using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(T3OMessageObject))]
	class T3OMessageObjectTest : SoapMessageObjectAbstractTest<T3OMessageObject>
	{
		public void TestT3OMessageObject()
		{
			var messageObject = new T3OMessageObject(TRMessageTestHelper.GetFileText("T3OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming."), Factory.New<TRManifestMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized T3O Message xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.Cast<InnerMessageObjectBase>().FirstOrDefault();
				AssertType<InnerXmlRegistrationNumberObject>("T3OInnerMessageObject", innerMessageObject);
				AssertEquals("RegistrationNumber", "22066666VB0000000032", ((InnerXmlRegistrationNumberObject)innerMessageObject).RegistrationNumber);
				AssertZDatesWithin5Minutes("RegistrationTime", new ZDateTime(2022, 11, 28, 10, 52, 07), ((InnerXmlRegistrationNumberObject)innerMessageObject).RegistrationTime);
			});
		}

		public void TestErrorMessageObjcet()
		{
			var messageText = TRMessageTestHelper.GetFileText("T3OError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var messageObject = new T3OMessageObject(messageText, Factory.New<TRManifestMessage>(), new LoggingInformation());

			CombineAssertions(() =>
			{
				var responseObjcet = messageObject.InnerMessageObjects.FirstOrDefault() as InnerXmlErrorMessagesObject;
				AssertType<InnerXmlErrorMessagesObject>(responseObjcet);
				AssertEquals(2, responseObjcet.ErrorMessages.Count);
				AssertEquals("Konteyner oldugunda acentanin vergi numarasi girilmeli (TS=3432534534)", responseObjcet.ErrorMessages.FirstOrDefault());
			});
		}

		public void TestInnerMessageObjcet()
		{
			var messageText = TRMessageTestHelper.GetFileText("Manifest.Incoming.T3OError2.xml");
			var messageObject = new T3OMessageObject(messageText, Factory.New<TRManifestMessage>(), new LoggingInformation());

			CombineAssertions(() =>
			{
				var responseObjcet = messageObject.InnerMessageObjects.FirstOrDefault();
				AssertType<InnerMessageObjectBase>(responseObjcet);
				AssertEquals("Mesajınız işleme alınamamıştır, lütfen yeniden gönderiniz.", responseObjcet.MessageText);
			});
		}
	}
}
