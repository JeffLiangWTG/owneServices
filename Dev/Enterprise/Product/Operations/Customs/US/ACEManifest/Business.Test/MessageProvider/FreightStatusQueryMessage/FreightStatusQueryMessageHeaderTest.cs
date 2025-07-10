using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class FreightStatusQueryMessageHeaderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new FreightStatusQueryMessageHeader(null, arrivalHeader));
		}

		public void TestMessageType()
		{
			AssertEquals(Constants.AIMMessageSubTypes.FSQ, freightStatusQueryMessageHeader.MessageType);
		}

		public void TestReference()
		{
			AssertEquals("BIL01", freightStatusQueryMessageHeader.Reference);
		}

		public void TestCargoControlLine()
		{
			manifestHeader.AMA_CarrierCode = "AAA";
			AssertType<AIMCargoControlLocation>(freightStatusQueryMessageHeader.CargoControlLine);
			AssertEquals("CargoTerminalOperator", "AAA", freightStatusQueryMessageHeader.CargoControlLine.CargoTerminalOperator);
		}

		public void TestAirWaybill()
		{
			arrivalHeader.ATH_Reference = "R";
			AssertType<AIMAirWaybillForFSQ>(freightStatusQueryMessageHeader.AirWaybill);
			AssertEquals("PartArrivalReference", "R", freightStatusQueryMessageHeader.AirWaybill.PartArrivalReference);
		}

		public void TestFreightStatusQuery()
		{
			AssertType<FreightStatusQuery>(freightStatusQueryMessageHeader.FreightStatusQuery);
			AssertEquals("StatusRequestCode", "05", freightStatusQueryMessageHeader.FreightStatusQuery.StatusRequestCode);
		}

		public void TestSetFreightStatusQueryRequestCode()
		{
			freightStatusQueryMessageHeader.SetFreightStatusQueryRequestCode("01");
			AssertEquals("StatusRequestCode", "01", freightStatusQueryMessageHeader.FreightStatusQuery.StatusRequestCode);
		}

		protected override void SetUp()
		{
			manifestHeader = Factory.New<AsycudaManifestHeader>();
			bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "BIL01";
			arrivalHeader = manifestHeader.ArrivalHeaders.AddNew();
			freightStatusQueryMessageHeader = new FreightStatusQueryMessageHeader(bill, arrivalHeader);
		}

		AsycudaBill bill;
		AsycudaArrivalHeader arrivalHeader;
		AsycudaManifestHeader manifestHeader;
		FreightStatusQueryMessageHeader freightStatusQueryMessageHeader;
	}
}
