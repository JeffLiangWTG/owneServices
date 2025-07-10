using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(T2OMessageObject))]
	class T2OMessageObjectTest : SoapMessageObjectAbstractTest<T2OMessageObject>
	{
		public void TestT2OMessageObject()
		{
			var messageObject = new T2OMessageObject(TRMessageTestHelper.GetFileText("Common.IslemSorgula3Response.xml"), Factory.New<TRManifestMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized T2O Message xml correctly.", () =>
			{
				var innerMessageObjects = messageObject.InnerMessageObjects.Cast<DiffGramGuidObject>();
				AssertEquals("Should have 2 items in InnerMessageObjects", 2, innerMessageObjects.Count());

				var item1 = innerMessageObjects.FirstOrDefault(@object => @object.Guid == new ZGuid("a26e6ecc-5e93-44a9-bc45-be64a6f544e6"));
				AssertNotNull("Result 1", item1);
				AssertEquals("T2OInnerMessageObject1.RefId", "ULU-MAN0000231|20201224104", item1.RefId);
				AssertEquals("T2OInnerMessageObject1.Tip", 0, item1.Tip);
				AssertEquals("T2OInnerMessageObject1.Situation", "1", item1.Situation);
				AssertZDatesWithin5Minutes("T2OInnerMessageObject1.OptionTime", new ZDateTime(2022, 6, 16, 15, 29, 12), item1.OptionTime);

				var item2 = innerMessageObjects.FirstOrDefault(@object => @object.Guid == new ZGuid("344d5a52-938e-4fc4-b38b-c58d2f03319d"));
				AssertNotNull("Result 2", item2);
				AssertEquals("T2OInnerMessageObject2.RefId", "ULU-MAN0000231|20201224104", item2.RefId);
				AssertEquals("T2OInnerMessageObject2.Tip", 2, item2.Tip);
				AssertEquals("T2OInnerMessageObject2.Situation", "1", item2.Situation);
				AssertZDatesWithin5Minutes("T2OInnerMessageObject2.OptionTime", new ZDateTime(2022, 6, 16, 15, 53, 21), item2.OptionTime);
			});
		}
	}
}
