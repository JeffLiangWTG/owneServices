using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(TROMessageObject))]
	class TROMessageObjectTest : SoapMessageObjectAbstractTest<TROMessageObject>
	{
		public void TestSuccess()
		{
			var messageObject = new TROMessageObject(TRMessageTestHelper.GetFileText("TROSucceed.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming."), Factory.New<TRManifestMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized TRO Message xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.Cast<SOAPLevelRefIDAndGuidObject>().FirstOrDefault();
				AssertType<SOAPLevelRefIDAndGuidObject>("TROSucceedObject", innerMessageObject);
				AssertEquals("RefID", "ULU-MANULU230000356-2|20201224104", innerMessageObject.RefID);
				AssertEquals("Guid", new ZGuid("3ea2edd6-18b2-435d-bdd1-1e29b0d59a7b"), innerMessageObject.Guid);
				AssertEquals("Situation", "İşleminiz başlamıştır.Teşekkür ederiz.", innerMessageObject.Situation);
			});
		}

		public void TestErrorMessage()
		{
			var messageObject = new TROMessageObject(TRMessageTestHelper.GetFileText("TROError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming."), Factory.New<TRManifestMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized TRO Message xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.Cast<SOAPLevelErrorObject>().FirstOrDefault();
				AssertType<SOAPLevelErrorObject>("TROErrorMessageObject", innerMessageObject);
				AssertEquals("ErrorMessage", "Gonderdiginiz mesaj bu servise uygun degildir. Mesaj tipini veya mesaj namespace ni kontrol ediniz.", innerMessageObject.ErrorMessage);
			});
		}

		public void TestExceptionMessage()
		{
			var messageObject = new TROMessageObject(TRMessageTestHelper.GetFileText("TROException.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming."), Factory.New<TRManifestMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized TRO Message xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.Cast<SOAPLevelExceptionObject>().FirstOrDefault();
				AssertType<SOAPLevelExceptionObject>("ManifestSOAPLevelExceptionObject", innerMessageObject);
				AssertEquals("FaultString", "Data at the root level is invalid. Line 1, position 1.", innerMessageObject.FaultString);
			});
		}
	}
}
