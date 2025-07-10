using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class CarrierAssignedBatchNumberCreatorTest : TestCase
	{
		public void TestCreateNumber()
		{
			AssertEquals("HELLOBILL123_" + AMSEDIMessage.AMSMessageNumberPlaceHolder, CarrierAssignedBatchNumberCreator.CreateNumber("HELLOBILL123"));
		}

		public void TestGetMessageNumber()
		{
			var inpm02 = new INPM02()
			{
				CarrierAssignedBatchNumber = CarrierAssignedBatchNumberCreator.CreateNumber("HIBILL123")
			};
			AssertEquals(AMSEDIMessage.AMSMessageNumberPlaceHolder, CarrierAssignedBatchNumberCreator.GetMessageNumber(inpm02));
		}

		public void TestGetBillOfLading()
		{
			var inpm02 = new INPM02()
			{
				CarrierAssignedBatchNumber = CarrierAssignedBatchNumberCreator.CreateNumber("HIBILL123")
			};
			AssertEquals("HIBILL123", CarrierAssignedBatchNumberCreator.GetBillOfLading(inpm02));
		}
	}
}
