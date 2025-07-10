using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	sealed class ConsignmentAsycudaTest : TestCaseWithFactory
	{
		public void TestCustomsStatus()
		{
			bill.ABL_BillStatus = LowValueConsignmentStatusList.Codes.EH;
			AssertEquals(LowValueConsignmentStatusList.Codes.EH, consignment.CustomsStatus);
			consignment = new ConsignmentAsycuda(new WriteOffResponseAsycuda(new BaseTSWResponse(inboundMessage)), "INVALID", LowValueConsignmentStatusList.Codes.ConsignmentHeld);
			Assert(consignment.CustomsStatus.IsEmpty);
		}

		public void TestGoodsClearanceStatus()
		{
			AssertEquals(LowValueConsignmentStatusList.Codes.ConsignmentHeld, consignment.GoodsClearanceStatus);
		}

		public void TestCombinedStatus()
		{
			bill.ABL_BillStatus = LowValueConsignmentStatusList.Codes.EH;
			AssertEquals(LowValueConsignmentStatusList.Codes.EH, consignment.CombinedStatus);
			consignment = new ConsignmentAsycuda(new WriteOffResponseAsycuda(new BaseTSWResponse(inboundMessage)), "INVALID", LowValueConsignmentStatusList.Codes.ConsignmentHeld);
			Assert(consignment.CombinedStatus.IsEmpty);
		}

		public void TestDeclaration()
		{
			AssertNull(consignment.Declaration);
		}

		public void TestResponseStatus()
		{
			Assert(consignment.ResponseStatus.IsEmpty);
		}

		public void TestEnterpriseStatus()
		{
			AssertEquals(LowValueConsignmentStatusList.Codes.ConsignmentHeld, consignment.EnterpriseStatus);
		}

		public void TestAgency()
		{
			AssertEquals(ResponsibleGovernmentAgencyList.Codes.TSW, consignment.Agency);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_MasterBill = "MAN0000026";
			bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "1";
			bill.ABL_MessageStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			outboundMessage = Factory.New<TSWMessage>();
			outboundMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outboundMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_ApplicationReference = "MAN0000026";
			outboundMessage.EM_LinkedObject = header;
			inboundMessage = Factory.New<TSWMessage>();
			inboundMessage.EM_MessageType = NZCMessage.MessageTypes.ResponseMsg;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_MessageText = new EmbeddedResourceRetriever().GetString("Enterprise.Customs.NZ.Manifest.Business.Testing.Messaging.TestFiles.ICRAsycudaWriterOffResponse.txt");
			consignment = new ConsignmentAsycuda(new WriteOffResponseAsycuda(new BaseTSWResponse(inboundMessage)), "1", LowValueConsignmentStatusList.Codes.ConsignmentHeld);
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
		TSWMessage outboundMessage;
		TSWMessage inboundMessage;
		IWriteOffStatus consignment;
	}
}
