using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(T1OMessageObject))]
	class T1OMessageObjectTest : SoapMessageObjectAbstractTest<T1OMessageObject>
	{
		public void TestEmpty()
		{
			AssertNoExceptionThrown(
				"No exception should be throw when creating T1OMessageObject on empty T1O response xml.",
				() => { _ = new T1OMessageObject(TRMessageTestHelper.GetFileText("T1OEmpty.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming."), Factory.New<TRManifestMessage>(), new LoggingInformation()); }
			);
		}

		public void TestErrorMessage()
		{
			var messageObject = new T1OMessageObject(TRMessageTestHelper.GetFileText("T1OError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming."), Factory.New<TRManifestMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized Error Message xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.FirstOrDefault() as InnerXmlErrorMessagesObject;
				AssertType<InnerXmlErrorMessagesObject>("T1OMessageErrorMessageObject", innerMessageObject);
				AssertEquals("ErrorMessages count", 2, innerMessageObject.ErrorMessages.Count);
				AssertEquals("T1OMessageErrorMessageObject.ErrorMessage", "Konteyner oldugunda acentanin vergi numarasi girilmeli (TS=3432534534)", innerMessageObject.ErrorMessages.First());
			});
		}

		public void TestGroupageAnswerObject()
		{
			var messageObject = new T1OMessageObject(TRMessageTestHelper.GetFileText("T1OGroupageAnswer.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming."), Factory.New<TRManifestMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized Groupage Answer xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.FirstOrDefault() as InnerXmlRegistrationNumberObject;
				var groupageAnswerObject = innerMessageObject.GroupageAnswerObject;
				AssertType<InnerXmlGroupageAnswerObject>("T1OMessageGroupageAnswerObject", groupageAnswerObject);
				AssertEquals("T1OMessageGroupageAnswerObject.CountryCode", "052", groupageAnswerObject.CountryCode);
				AssertEquals("T1OMessageGroupageAnswerObject.LicensePlateNo", "TK1234", groupageAnswerObject.LicensePlateNo);
				AssertZDatesWithin5Minutes("T1OMessageGroupageAnswerObject.Arrivaldate", new ZDateTime(2022, 9, 28, 14, 20, 0), groupageAnswerObject.ArrivalDate);
				AssertZDatesWithin5Minutes("T1OMessageGroupageAnswerObject.GdStartDate", new ZDateTime(2022, 9, 28, 17, 28, 0), groupageAnswerObject.GdStartDate);
				AssertZDatesWithin5Minutes("T1OMessageGroupageAnswerObject.GdTime", new ZDateTime(2022, 10, 18, 17, 28, 0), groupageAnswerObject.GdTime);
				AssertEquals("T1OMessageGroupageAnswerObject.NameOfVehicle", "TURKUS AIRLINES", groupageAnswerObject.NameOfVehicle);
				AssertEquals("T1OMessageGroupageAnswerObject.ReferenceNumber", "235", groupageAnswerObject.ReferenceNumber);

				AssertEquals("T1OMessageGroupageAnswerObject.IdentificationNumber", "8890024379", groupageAnswerObject.IdentificationNumber);
				AssertEquals("T1OMessageGroupageAnswerObject.IdentityTour", "VERGINO", groupageAnswerObject.IdentityTour);
				AssertEquals("T1OMessageGroupageAnswerObject.IdentityTour", "ULUKOM BILGISAYAR YAZILIM DON.DANISMAN.VE TIC.LTD.ST", groupageAnswerObject.NameTitle);

				AssertEquals("T1OMessageGroupageAnswerObject.PortLocationNameYuk", "FRA", groupageAnswerObject.PortLocationNameYuk);
				AssertEquals("T1OMessageGroupageAnswerObject.PortLocationNameBos", "ISL", groupageAnswerObject.PortLocationNameBos);
				AssertEquals("T1OMessageGroupageAnswerObject.CountryCodeYuk", "004", groupageAnswerObject.CountryCodeYuk);
				AssertEquals("T1OMessageGroupageAnswerObject.CountryCodeBos", "052", groupageAnswerObject.CountryCodeBos);
			});
		}

		public void TestSucceedObject()
		{
			var messageObject = new T1OMessageObject(TRMessageTestHelper.GetFileText("T1OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming."), Factory.New<TRManifestMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized Succeedd Message xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.FirstOrDefault() as InnerXmlRegistrationNumberObject;
				AssertType<InnerXmlRegistrationNumberObject>("T1OMessageSucceedObject", innerMessageObject);
				AssertEquals("T1OMessageSucceedObject.RegistrationNumber", "22067777IM000002", innerMessageObject.RegistrationNumber);
				AssertZDatesWithin5Minutes("T1OMessageSucceedObject.GdTime", new ZDateTime(2022, 1, 26, 10, 3, 41), innerMessageObject.RegistrationTime);
			});
		}
	}
}
