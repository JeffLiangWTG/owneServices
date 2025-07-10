using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMArrivalManifestTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var additionalMessageInformation = new AdditionalMessageInformation(manifestHeader);
			additionalMessageInformation.AM_BoardedQty = 200;
			additionalMessageInformation.AM_BoardedWeight = 35.5m;
			additionalMessageInformation.AM_BoardedWeightUQ = "L";
			additionalMessageInformation.AM_FlightNo = "UA118";
			additionalMessageInformation.AM_FlightArrivalDate = new ZDate(2021, 10, 01);
			additionalMessageInformation.AM_IsSplitShipment = true;
			var aimArrivalManifest = new AIMArrivalForManifestMsg(additionalMessageInformation);
			AssertEquals("UA118", aimArrivalManifest.FlightNumber);
			AssertEquals(new ZDate(2021, 10, 01), aimArrivalManifest.ScheduledArrivalDate);
			AssertEquals(ZString.Empty, aimArrivalManifest.PartArrivalReference);
			AssertEquals("L", aimArrivalManifest.WeightCode);
			AssertEquals(true, aimArrivalManifest.IsBoardedQuantity);
			AssertEquals(200m, aimArrivalManifest.BoardedPieceCount);
			AssertEquals(35.5m, aimArrivalManifest.Weight);
		}
	}
}
