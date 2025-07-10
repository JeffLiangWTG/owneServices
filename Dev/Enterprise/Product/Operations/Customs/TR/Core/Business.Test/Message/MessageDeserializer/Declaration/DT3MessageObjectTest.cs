using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(DT3MessageObject))]
	class DT3MessageObjectTest : SoapMessageObjectAbstractTest<DT3MessageObject>
	{
		public void TestNotRegisteredObject()
		{
			var messageObject = new DT3MessageObject(TRMessageTestHelper.GetFileText("DT3SuccessNotRegistered.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), Factory.New<TRImportExportMessage>(), new LoggingInformation());
			CombineAssertions("Not Registered", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.FirstOrDefault() as InnerXmlRegisterAnswerObject;
				AssertNull("Null innerMessageObject", innerMessageObject);
			});
		}

		public void TestRegisteredObject()
		{
			var messageObject = new DT3MessageObject(TRMessageTestHelper.GetFileText("DT3Success.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming."), Factory.New<TRImportExportMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized Succeed Message xml correctly.", () =>
			{
				var innerMessageObject = messageObject.InnerMessageObjects.FirstOrDefault() as InnerXmlRegisterAnswerObject;
				AssertType<InnerXmlRegisterAnswerObject>("DT3MessageSucceedObject", innerMessageObject);
				AssertEquals("RegistrationNumber", "23066666EX00000026", innerMessageObject.RegistrationNumber);
				AssertEquals("RegistrationTime", new ZDateTime(2023, 3, 24), innerMessageObject.RegistrationDate);
			});
		}
	}
}
