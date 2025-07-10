using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMAirWaybillForArrivalTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new AIMAirWaybillForArrival(null, bill));
		}

		public void TestAirWaybillPrefix()
		{
			AssertEquals("SHA", airWaybillForArrival.AirWaybillPrefix);
		}

		public void TestAWBSerialNumber()
		{
			AssertEquals("12345678", airWaybillForArrival.AWBSerialNumber);
		}

		public void TestIsMasterAirWaybill()
		{
			AssertEquals(false, airWaybillForArrival.IsMasterAirWaybill);
			arrivalHeader.ATH_VoyageFlightNo = "FL01";
			arrivalHeader.ATH_Reference = "A";
			airWaybillForArrival = new AIMAirWaybillForArrival(arrivalHeader, (AsycudaBill)manifestHeader.MasterBill);
			AssertEquals(true, airWaybillForArrival.IsMasterAirWaybill);
		}

		public void TestHAWBNumber()
		{
			AssertEquals("BIL01", airWaybillForArrival.HAWBNumber);
			arrivalHeader.ATH_VoyageFlightNo = "FL01";
			arrivalHeader.ATH_Reference = "A";
			airWaybillForArrival = new AIMAirWaybillForArrival(arrivalHeader, (AsycudaBill)manifestHeader.MasterBill);
			AssertEquals(ZString.Empty, airWaybillForArrival.HAWBNumber);
		}

		public void TestPackageTrackingIdentifier()
		{
			AssertEquals("", airWaybillForArrival.PackageTrackingIdentifier);
		}

		protected override void SetUp()
		{
			manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_MasterBill = "SHA-123456789";
			bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "BIL01";
			arrivalHeader = manifestHeader.ArrivalHeaders.AddNew();
			transferHeader = arrivalHeader.TransferHeaders.AddNew();
			transferHeader.ATF_TransferType = ManifestBase.TransferTypeList.Codes.Domestic;
			transferBill = transferHeader.TransferBills.AddNew();
			transferBill.ATB_BillNumber = bill.ABL_BillNumber;
			airWaybillForArrival = new AIMAirWaybillForArrival(arrivalHeader, bill);
		}

		AsycudaTransferBill transferBill;
		AsycudaTransferHeader transferHeader;
		AsycudaBill bill;
		AsycudaArrivalHeader arrivalHeader;
		AsycudaManifestHeader manifestHeader;
		AIMAirWaybillForArrival airWaybillForArrival;
	}
}
