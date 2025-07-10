using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.AIM.Messaging;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMWaybillManifestTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_E_ARV = new ZDateTime(2019, 2, 5, 11, 0, 0);
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfFirstArrival = "USLAX";
			header.AMA_RL_NKPortOfDischarge = "NZAKL";
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_IsConsolidation = false;
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "AUMEL";
			bill.ABL_RL_NKPortOfDischarge = "USNYC";
			bill.ABL_ManifestQty = 55;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_GrossWeight = 13.26m;
			bill.ABL_GoodsDescription = "Desc: for testing";
			IAIMWaybill aimWaybill = new AIMWaybillForManifestMsg(header, additionalMessageInformation);
			AssertEquals("SYD", aimWaybill.AirportOfOrigin);
			AssertEquals(55m, aimWaybill.NumberOfPieces);
			AssertEquals("K", aimWaybill.WeightCode);
			AssertEquals(13.3m, aimWaybill.Weight);
			AssertEquals("Desc: for testing", aimWaybill.CargoDescription);
			AssertEquals("AKL", aimWaybill.PermitToProceedDestinationAirport);
			AssertEquals(new ZDate(2019, 2, 5), aimWaybill.DateOfArrivalAtThePermitToProceedDestinationAirport);
			bill.ABL_GrossWeightUQ = Constants.Weight.Pounds;
			IAIMWaybill aimWaybill2 = new AIMWaybillForManifestMsg(header, additionalMessageInformation);
			AssertEquals(WeightUnits.Codes.Kilograms, aimWaybill2.WeightCode);
			AssertEquals(6.0m, aimWaybill2.Weight);
			header.AMA_RL_NKPortOfFirstArrival = "NZAKL";
			IAIMWaybill aimWaybill3 = new AIMWaybillForManifestMsg(header, additionalMessageInformation);
			AssertNullOrEmpty("Empty when the First Arrival Port is the same as the Discharge Port", aimWaybill3.PermitToProceedDestinationAirport);
			AssertEquals(ZDateTime.Empty, aimWaybill3.DateOfArrivalAtThePermitToProceedDestinationAirport);
			header.AMA_RL_NKPortOfFirstArrival = ZString.Empty;
			IAIMWaybill aimWaybill4 = new AIMWaybillForManifestMsg(header, additionalMessageInformation);
			AssertNullOrEmpty("Empty when the First Arrival Port is empty", aimWaybill4.PermitToProceedDestinationAirport);
			AssertEquals(ZDateTime.Empty, aimWaybill4.DateOfArrivalAtThePermitToProceedDestinationAirport);
		}

		public void TestProperties_Consolidation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_E_ARV = new ZDateTime(2019, 2, 5, 11, 0, 0);
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfFirstArrival = "USLAX";
			header.AMA_RL_NKPortOfDischarge = "NZAKL";
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_IsConsolidation = true;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_RL_NKOrigin = "AUMEL";
			bill1.ABL_RL_NKPortOfDischarge = "USNYC";
			bill1.ABL_ManifestQty = 15;
			bill1.ABL_GrossWeightUQ = "KG";
			bill1.ABL_GrossWeight = 13.26m;
			bill1.ABL_GoodsDescription = "This is Bill 1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_RL_NKOrigin = "AUPER";
			bill2.ABL_RL_NKPortOfDischarge = "USNYC";
			bill2.ABL_ManifestQty = 13;
			bill2.ABL_GrossWeightUQ = "LB";
			bill2.ABL_GrossWeight = 20m;
			bill2.ABL_GoodsDescription = "This is Bill 2";
			IAIMWaybill aimWaybill = new AIMWaybillForManifestMsg(header, additionalMessageInformation);
			AssertEquals("SYD", aimWaybill.AirportOfOrigin);
			AssertEquals(28m, aimWaybill.NumberOfPieces);
			AssertEquals("K", aimWaybill.WeightCode);
			AssertEquals(22.3m, aimWaybill.Weight);
			AssertEquals("CONSOLIDATION", aimWaybill.CargoDescription);
			AssertEquals("AKL", aimWaybill.PermitToProceedDestinationAirport);
			AssertEquals(new ZDate(2019, 2, 5), aimWaybill.DateOfArrivalAtThePermitToProceedDestinationAirport);
		}

		public void TestGoodsDescForConsolidation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_E_ARV = new ZDateTime(2019, 2, 5, 11, 0, 0);
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfFirstArrival = "USLAX";
			header.AMA_RL_NKPortOfDischarge = "NZAKL";
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_FlightNo = "UA210";
			additionalMessageInformation.AM_FlightArrivalDate = new ZDate(2021, 09, 13);
			additionalMessageInformation.AM_IsConsolidation = true;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_RL_NKOrigin = "AUMEL";
			bill1.ABL_RL_NKPortOfDischarge = "USNYC";
			bill1.ABL_ManifestQty = 15;
			bill1.ABL_GrossWeightUQ = "KG";
			bill1.ABL_GrossWeight = 13.26m;
			bill1.ABL_GoodsDescription = "This is Bill 1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_RL_NKOrigin = "AUPER";
			bill2.ABL_RL_NKPortOfDischarge = "USNYC";
			bill2.ABL_ManifestQty = 13;
			bill2.ABL_GrossWeightUQ = "LB";
			bill2.ABL_GrossWeight = 20m;
			bill2.ABL_GoodsDescription = "This is Bill 2";
			IAIMWaybill aimWaybill = new AIMWaybillForManifestMsg(header, additionalMessageInformation);
			AssertEquals("CONSOLIDATION", aimWaybill.CargoDescription);
		}

		public void TestGoodsDescForNonConsolidation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_E_ARV = new ZDateTime(2019, 2, 5, 11, 0, 0);
			header.AMA_RL_NKPortOfLoading = "AUSYD";
			header.AMA_RL_NKPortOfFirstArrival = "USLAX";
			header.AMA_RL_NKPortOfDischarge = "NZAKL";
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			additionalMessageInformation.AM_FlightNo = "UA210";
			additionalMessageInformation.AM_FlightArrivalDate = new ZDate(2021, 09, 13);
			additionalMessageInformation.AM_IsConsolidation = false;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_RL_NKOrigin = "AUMEL";
			bill1.ABL_RL_NKPortOfDischarge = "USNYC";
			bill1.ABL_ManifestQty = 15;
			bill1.ABL_GrossWeightUQ = "KG";
			bill1.ABL_GrossWeight = 13.26m;
			bill1.ABL_GoodsDescription = "Magazines";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_RL_NKOrigin = "AUPER";
			bill2.ABL_RL_NKPortOfDischarge = "USNYC";
			bill2.ABL_ManifestQty = 13;
			bill2.ABL_GrossWeightUQ = "LB";
			bill2.ABL_GrossWeight = 20m;
			bill2.ABL_GoodsDescription = "Newspapers";
			IAIMWaybill aimWaybill = new AIMWaybillForManifestMsg(header, additionalMessageInformation);
			AssertEquals("Magazines", aimWaybill.CargoDescription);
		}
	}
}
