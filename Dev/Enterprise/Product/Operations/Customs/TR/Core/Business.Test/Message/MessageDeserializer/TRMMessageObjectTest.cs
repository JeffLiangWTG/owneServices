using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(TRMMessageObject))]
	class TRMMessageObjectTest : SoapMessageObjectAbstractTest<TRMMessageObject>
	{
		public void TestTRMMessageObject()
		{
			var messageObject = new TRMMessageObject(TRMessageTestHelper.GetFileText("Manifest.OzbyMuayeneMemuruAdiSorgulaResponse.xml"), Factory.New<TRManifestMessage>(), new LoggingInformation());
			CombineAssertions("Should have deserialized TRM Message xml correctly.", () =>
			{
				var innerMessageObjects = messageObject.InnerMessageObjects.Cast<InnerXmlInspectionClerkObject>();
				AssertEquals("Should have 1 item in InnerMessageObjects", 1, innerMessageObjects.Count());

				var item1 = innerMessageObjects.FirstOrDefault(@object => @object.MUAYENEMEMURU == new ZString("BÜLENT ALİ GÖZÜKÜÇÜK"));
				AssertNotNull("Result 1", item1);
				AssertEquals("TRMMessageObject.InspectionClerk", "BÜLENT ALİ GÖZÜKÜÇÜK", item1.MUAYENEMEMURU);
			});
		}
	}
}
