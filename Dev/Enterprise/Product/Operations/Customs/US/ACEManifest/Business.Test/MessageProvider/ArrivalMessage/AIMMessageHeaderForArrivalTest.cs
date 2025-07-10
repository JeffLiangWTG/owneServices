using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMMessageHeaderForArrivalTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ArrivalMessageHeader(null, ZString.Empty));
		}

		public void TestMessageType()
		{
			AssertEquals(Constants.AIMMessageSubTypes.FSN, messageHeaderForArrival.MessageType);
		}

		public void TestReference()
		{
			AssertEquals("BIL01", messageHeaderForArrival.Reference);
		}

		public void TestCargoControlLine()
		{
			manifestHeader.AMA_CarrierCode = "AC01";
			AssertType<AIMCargoControlLocation>(messageHeaderForArrival.CargoControlLine);
			AssertEquals("AC0", messageHeaderForArrival.CargoControlLine.CargoTerminalOperator);
		}

		public void TestAirWaybill()
		{
			AssertType<AIMAirWaybillForArrival>(messageHeaderForArrival.AirWaybill);
			AssertEquals(false, messageHeaderForArrival.AirWaybill.IsMasterAirWaybill);
		}

		public void TestArrival()
		{
			arrivalHeader.ATH_VoyageFlightNo = "FL01";
			AssertType<AIMArrival>(messageHeaderForArrival.Arrival);
			AssertEquals("FL01", messageHeaderForArrival.Arrival.FlightNumber);
		}

		public void TestAirlineStatusNotification()
		{
			AssertType<AirlineStatusNotification>(messageHeaderForArrival.AirlineStatusNotification);
			AssertEquals(AIMArrivalStatusCodes.Codes.InbondArrivedAtDestination, messageHeaderForArrival.AirlineStatusNotification.StatusCode);
		}

		protected override void SetUp()
		{
			manifestHeader = Factory.New<AsycudaManifestHeader>();
			bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "BIL01";
			arrivalHeader = manifestHeader.ArrivalHeaders.AddNew();
			transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillNumber = bill.ABL_BillNumber;
			messageHeaderForArrival = new ArrivalMessageHeader(transferBill, AIMArrivalStatusCodes.Codes.InbondArrivedAtDestination);
		}

		AsycudaTransferBill transferBill;
		AsycudaTransferHeader transferHeader;
		AsycudaBill bill;
		AsycudaArrivalHeader arrivalHeader;
		AsycudaManifestHeader manifestHeader;
		ArrivalMessageHeader messageHeaderForArrival;
	}
}
