using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class DepartureMessageHeaderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new DepartureMessageHeader(null, addtionalMsgInfo));
			AssertExceptionThrown<ArgumentException>(() => new DepartureMessageHeader(manifestHeader, null));
		}

		public void TestMessageType()
		{
			AssertEquals(Constants.AIMMessageSubTypes.FDM, departureMessageHeader.MessageType);
		}

		public void TestReference()
		{
			AssertEquals("MAN001", departureMessageHeader.Reference);
		}

		public void TestDeparture()
		{
			AssertType<AIMDeparture>(departureMessageHeader.Departure);
		}

		protected override void SetUp()
		{
			manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_MasterBill = "MAN001";
			bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "BIL01";
			addtionalMsgInfo = new AdditionalMessageInformation(manifestHeader);
			departureMessageHeader = new DepartureMessageHeader(manifestHeader, addtionalMsgInfo);
		}

		AdditionalMessageInformation addtionalMsgInfo;
		AsycudaBill bill;
		AsycudaManifestHeader manifestHeader;
		DepartureMessageHeader departureMessageHeader;
	}
}
