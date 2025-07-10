using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(DT1MessageObject))]
	class DT1MessageObjectTest : SoapMessageObjectAbstractTest<DT1MessageObject>
	{
		public void TestErrorMessage()
		{
			var messageObject = new DT1MessageObject(TRMessageTestHelper.GetFileText("DT1Error.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), Factory.New<TRImportExportMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized Error Message xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.FirstOrDefault() as InnerXmlRegisterAnswerObject;
				AssertType<InnerXmlRegisterAnswerObject>("DT1MessageErrorMessageObject", innerMessageObject);
				AssertEquals("ErrorMessages count", 2, innerMessageObject.ControlAnswerObject.Errors.Count);
				AssertEquals("DT1MessageErrorMessageObject.ErrorMessage", "Açmalarda esya ambar içinde seçilmis. Tasima senedi henüz ambara alinmamis ya da ambar çikis islemi yapilmistir. (226362740)", innerMessageObject.ControlAnswerObject.Errors.First().Description);
			});

			var messageObject2 = new DT1MessageObject(TRMessageTestHelper.GetFileText("DT1Error2.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), Factory.New<TRImportExportMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized Error Message xml correctly.", () =>
			{
				var innerMessageObject2 = messageObject2.InnerMessageObjects.FirstOrDefault() as InnerXmlRegisterAnswerObject;
				AssertType<InnerXmlRegisterAnswerObject>("DT1MessageErrorMessageObject", innerMessageObject2);
				AssertEquals("ErrorMessages count", 1, innerMessageObject2.ControlAnswerObject.Errors.Count);
				AssertEquals("DT1MessageErrorMessageObject.ErrorMessage", "066666 --> Gümrük Veri Tabanı Bağlantı Hatası", innerMessageObject2.ControlAnswerObject.Errors.First().Description);
			});
		}

		public void TestSucceedObject()
		{
			var messageObject = new DT1MessageObject(TRMessageTestHelper.GetFileText("DT1Success.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), Factory.New<TRImportExportMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized Succeedd Message xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.FirstOrDefault() as InnerXmlRegisterAnswerObject;
				AssertType<InnerXmlRegisterAnswerObject>("DT1MessageSucceedObject", innerMessageObject);
				AssertEquals("RegistrationNumber", ZString.Empty, innerMessageObject.RegistrationNumber);
				AssertEquals("RegistrationTime", ZDateTime.Empty, innerMessageObject.RegistrationDate);
				AssertEquals("Entry Question count", 5, innerMessageObject.ControlAnswerObject.Questions.Count);
				AssertEquals("Question Message", 7780, innerMessageObject.ControlAnswerObject.Questions.First().Code);
				AssertEquals("Document count", 9, innerMessageObject.ControlAnswerObject.Documents.Count);
				AssertEquals("Document Message", "0887", innerMessageObject.ControlAnswerObject.Documents.First().Code);
				AssertEquals("Tax count", 5, innerMessageObject.ControlAnswerObject.Taxes.Count);
				AssertEquals("Tax Message", "10", innerMessageObject.ControlAnswerObject.Taxes.First().Code);
			});
		}

		public void TestRegisteredObject()
		{
			var messageObject = new DT1MessageObject(TRMessageTestHelper.GetFileText("DT1Registered.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), Factory.New<TRImportExportMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized Succeed Message xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.FirstOrDefault() as InnerXmlRegisterAnswerObject;
				AssertType<InnerXmlRegisterAnswerObject>("DT1MessageSucceedObject", innerMessageObject);
				AssertEquals("RegistrationNumber", "23343100IM00046770", innerMessageObject.RegistrationNumber);
				AssertEquals("RegistrationTime", new ZDateTime(2023, 3, 1), innerMessageObject.RegistrationDate);
				AssertEquals("Entry Question count", 5, innerMessageObject.ControlAnswerObject.Questions.Count);
				AssertEquals("Question Message", 7780, innerMessageObject.ControlAnswerObject.Questions.First().Code);
				AssertEquals("Document count", 9, innerMessageObject.ControlAnswerObject.Documents.Count);
				AssertEquals("Document Message", "0887", innerMessageObject.ControlAnswerObject.Documents.First().Code);
				AssertEquals("Tax count", 5, innerMessageObject.ControlAnswerObject.Taxes.Count);
				AssertEquals("Tax Message", "10", innerMessageObject.ControlAnswerObject.Taxes.First().Code);
			});
		}
	}
}
