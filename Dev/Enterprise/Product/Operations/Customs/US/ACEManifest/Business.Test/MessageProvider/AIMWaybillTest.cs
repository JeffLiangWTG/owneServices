using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMWaybillTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_E_ARV = new ZDateTime(2019, 2, 5, 11, 0, 0);
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfFirstArrival = "USLAX";
			header.AMA_RL_NKPortOfDischarge = "NZAKL";
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "AUMEL";
			bill.ABL_RL_NKPortOfDischarge = "USNYC";
			bill.ABL_ManifestQty = 55;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_GrossWeight = 13.26m;
			bill.ABL_GoodsDescription = "Desc: for testing";
			IAIMWaybill aimWaybill = new AIMWaybill(bill);
			AssertEquals("MEL", aimWaybill.AirportOfOrigin);
			AssertEquals(55m, aimWaybill.NumberOfPieces);
			AssertEquals("K", aimWaybill.WeightCode);
			AssertEquals(13.26m, aimWaybill.Weight);
			AssertEquals("Desc: for testing", aimWaybill.CargoDescription);
			AssertEquals(ZString.Empty, aimWaybill.PermitToProceedDestinationAirport);
			AssertEquals(ZDate.Empty, aimWaybill.DateOfArrivalAtThePermitToProceedDestinationAirport);
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;
			IAIMWaybill aimWaybill2 = new AIMWaybill(bill);
			AssertEquals(WeightUnits.Codes.Pounds, aimWaybill2.WeightCode);
			AssertEquals(13.26m, aimWaybill2.Weight);
		}
	}
}
