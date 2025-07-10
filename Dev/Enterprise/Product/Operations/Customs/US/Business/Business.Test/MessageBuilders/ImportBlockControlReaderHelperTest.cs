using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ImportBlockControlReaderHelperTest : TestCase
	{
		public void TestIsYBlockData()
		{
			var helper = new ImportBlockControlReaderHelper();
			AssertEquals(true, helper.IsYBlockData(new char[] { 'Y', 'Y' }));
			AssertEquals(false, helper.IsYBlockData(new char[] { 'Z', 'Y' }));
			AssertEquals(true, helper.IsYBlockData(new char[] { 'Z', 'Z' }));
		}

		public void TestSetBAndYBlocks()
		{
			CombineAssertions(delegate
			{
				var helper = new ImportBlockControlReaderHelper();
				helper.SetBAndYBlocks("B", ApplicationIdentifierCodeList.Codes.EntrySummary, out var b, out var y);
				AssertEquals(typeof(APLB), b.GetType());
				AssertEquals(typeof(APLY), y.GetType());
				helper.SetBAndYBlocks("B", ACEApplicationIdentifierCodeList.Codes.EntrySummary, out b, out y);
				AssertEquals(typeof(AABIOutputB), b.GetType());
				AssertEquals(typeof(AABIOutputY), y.GetType());
				helper.SetBAndYBlocks("AA", ACEApplicationIdentifierCodeList.Codes.CensusWarningQueryResponse, out b, out y);
				AssertEquals(typeof(BRDAA), b.GetType());
				AssertEquals(typeof(BRDZZ), y.GetType());
				AssertExceptionThrown(typeof(InvalidMessageFormatException), "message does not start with a 'B' or 'AA'", () => helper.SetBAndYBlocks("Z", ApplicationIdentifierCodeList.AES.CommodityShipmentResponse, out b, out y));
			});
		}
	}
}
