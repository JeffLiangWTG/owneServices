using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class AMSBlockControlReaderHelperTest : TestCase
	{
		public void TestIsYBlockData()
		{
			var helper = new AMSBlockControlReaderHelper();
			AssertEquals(false, helper.IsYBlockData(new char[] { 'Z', 'Y' }));
		}

		public void TestSetBAndYBlocks()
		{
			var helper = new AMSBlockControlReaderHelper();
			helper.SetBAndYBlocks("", AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse, out var b, out var y);
			AssertNull(b);
			AssertNull(y);
		}
	}
}
