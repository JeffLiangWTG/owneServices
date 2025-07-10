using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	[TestedType(typeof(UniversalCMMMessageProcessor.NonPersistentEDIMessage))]
	internal class NonPersistentEDIMessageTest : EDIMessageTest
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Data cannot be saved", true);
		}
	}
}
