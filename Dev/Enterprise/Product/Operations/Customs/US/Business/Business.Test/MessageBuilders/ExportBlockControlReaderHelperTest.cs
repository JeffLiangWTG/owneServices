using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ExportBlockControlReaderHelperTest : TestCase
	{
		public void TestIsYBlockData()
		{
			var helper = new ExportBlockControlReaderHelper();
			AssertEquals(true, helper.IsYBlockData(new char[] { 'Y', 'Y' }));
			AssertEquals(false, helper.IsYBlockData(new char[] { 'Z', 'Y' }));
		}

		public void TestSetBAndYBlocks()
		{
			CombineAssertions(delegate
			{
				var helper = new ExportBlockControlReaderHelper();
				helper.SetBAndYBlocks("B", ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, out var b, out var y);
				AssertEquals(typeof(AESCommShipBXT), b.GetType());
				AssertEquals(typeof(AESCommShipYXT), y.GetType());
				helper.SetBAndYBlocks("B", ApplicationIdentifierCodeList.AES.CommodityShipmentWarningReminder, out b, out y);
				AssertEquals(typeof(AESCommWarnBXN), b.GetType());
				AssertEquals(typeof(AESCommWarnYXN), y.GetType());
				AssertExceptionThrown(typeof(InvalidMessageFormatException), "message does not start with a 'B'", () => helper.SetBAndYBlocks("Z", ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, out b, out y));
				AssertExceptionThrown(typeof(InvalidMessageFormatException), "cannot deserialise B and Y blocks for application code 'USE' and message type 'Z@'", () => helper.SetBAndYBlocks("B", "Z@", out b, out y));
			});
		}
	}
}
