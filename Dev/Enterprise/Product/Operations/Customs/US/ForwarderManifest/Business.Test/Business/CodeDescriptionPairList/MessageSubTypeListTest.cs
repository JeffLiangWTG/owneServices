using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	sealed class MessageSubTypeListTest : TestCase
	{
		public void TestGetMessageSubTypeFromActionType()
		{
			AssertEquals(MessageSubTypeList.Codes.BillOfLadingAdd, MessageSubTypeList.GetMessageSubTypeFromActionType(USExportBillOfLadingActionCodeType.Codes.A));
			AssertEquals(MessageSubTypeList.Codes.BillOfLadingReplace, MessageSubTypeList.GetMessageSubTypeFromActionType(USExportBillOfLadingActionCodeType.Codes.R));
			AssertEquals(MessageSubTypeList.Codes.BillOfLadingDelete, MessageSubTypeList.GetMessageSubTypeFromActionType(USExportBillOfLadingActionCodeType.Codes.D));
		}
	}
}
