using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMArrivallTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "HAWB001";
			var arrHeader = manifestHeader.ArrivalHeaders.AddNew();
			arrHeader.ATH_VoyageFlightNo = "VBR0669K";
			arrHeader.ATH_ETAAtDischargePort = new ZDateTime(2019, 4, 6, 12, 0, 0);
			var aimArrivalForTransfer = new AIMArrival(arrHeader);
			AssertEquals("VBR0669K", aimArrivalForTransfer.FlightNumber);
			AssertEquals(new ZDate(2019, 4, 6), aimArrivalForTransfer.ScheduledArrivalDate);
			AssertEquals("", aimArrivalForTransfer.PartArrivalReference);
			AssertEquals(false, aimArrivalForTransfer.IsBoardedQuantity);
			AssertEquals(0m, aimArrivalForTransfer.BoardedPieceCount);
			AssertEquals("", aimArrivalForTransfer.WeightCode);
			AssertEquals(0m, aimArrivalForTransfer.Weight);
		}
	}
}
