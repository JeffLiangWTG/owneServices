using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Business.MessageProcessors;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	sealed class WriteOffResponseSeaCargoTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCREWriteOffMessageResponse()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				outboundMessage.EM_ApplicationReference = "X00001002";
				inboundMessage.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\CREWriteOffResponse.txt"));
				oceanBill.CB_MessageReference = "X00001002";
				oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
				oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
				var houseBill1 = oceanBill.HouseBills.AddNew();
				houseBill1.CA_HouseBill = "869020238";
				houseBill1.CA_ConsignmentNum = 1;
				var houseBill2 = oceanBill.HouseBills.AddNew();
				houseBill2.CA_HouseBill = "873409184";
				houseBill2.CA_ConsignmentNum = 2;
				Factory.Save();

				var processor = new MessageProcessorFactory(new LoggingInformation());
				processor.ProcessMessage(inboundMessage);
				AssertMultilineASCIIEquals("Message", File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\CRESeaCargoFormattedWriteOffResponse.txt")), inboundMessage.EM_MessageInterpretation);
				AssertEquals("86747270", oceanBill.EntryNumber);
				AssertEquals(LowValueManifestStatusList.Codes.Acknowledgement, oceanBill.CB_MessageStatus);
				AssertEquals(LowValueManifestStatusList.Codes.ManifestAccepted, oceanBill.CB_CustomsStatus);
				AssertEquals(2, oceanBill.Messages.Count);

				var response = new BaseTSWResponse(inboundMessage);
				var responseSeaCargo = new WriteOffResponseSeaCargo(response) as IWriteOffStatus;
				AssertEquals(null, responseSeaCargo.EntryHeader);
				AssertEquals("C06", responseSeaCargo.CustomsStatus);
				AssertEquals("", responseSeaCargo.GoodsClearanceStatus);
				AssertEquals(LowValueManifestStatusList.Codes.ManifestAccepted, responseSeaCargo.CombinedStatus);
				AssertEquals(null, responseSeaCargo.Declaration);
				AssertEquals("", responseSeaCargo.ResponseStatus);
				AssertEquals("C06", responseSeaCargo.EnterpriseStatus);
				AssertEquals("NZCS", responseSeaCargo.Agency);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestICRWriteOffMessageResponse()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				var houseBill1 = oceanBill.HouseBills.AddNew();
				var houseBill2 = oceanBill.HouseBills.AddNew();
				SetupICRWriteOffResponse(houseBill1, houseBill2);

				var processor = new MessageProcessorFactory(new LoggingInformation());
				processor.ProcessMessage(inboundMessage);
				AssertMultilineASCIIEquals("Message", File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRSeaCargoFormattedWriteOffResponse.txt")), inboundMessage.EM_MessageInterpretation);
				AssertEquals("7435668", oceanBill.EntryNumber);
				AssertEquals(LowValueManifestStatusList.Codes.Acknowledgement, oceanBill.CB_MessageStatus);
				AssertEquals(LowValueManifestStatusList.Codes.ManifestAccepted, oceanBill.CB_CustomsStatus);
				AssertEquals(2, oceanBill.Messages.Count);

				var response = new BaseTSWResponse(inboundMessage);
				var responseSeaCargo = new WriteOffResponseSeaCargo(response) as IWriteOffStatus;
				AssertEquals(null, responseSeaCargo.EntryHeader);
				AssertEquals("C06", responseSeaCargo.CustomsStatus);
				AssertEquals("WOF", responseSeaCargo.GoodsClearanceStatus);
				AssertEquals(LowValueManifestStatusList.Codes.ManifestAccepted, responseSeaCargo.CombinedStatus);
				AssertEquals(null, responseSeaCargo.Declaration);
				AssertEquals("", responseSeaCargo.ResponseStatus);
				AssertEquals("C06", responseSeaCargo.EnterpriseStatus);
				AssertEquals("NZCS", responseSeaCargo.Agency);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShipmentStatusForCustomsResponseThenBioResponseForSingleHouseThenBioResponseForBothHouseBills()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				var houseBill1 = oceanBill.HouseBills.AddNew();
				var houseBill2 = oceanBill.HouseBills.AddNew();
				SetupICRWriteOffResponse(houseBill1, houseBill2);

				var processor = new MessageProcessorFactory(new LoggingInformation());
				processor.ProcessMessage(inboundMessage);
				AssertEquals(LowValueConsignmentStatusList.Codes.Acknowledgement, houseBill1.CA_MessageStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.CP, houseBill1.CA_ShipmentStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.Acknowledgement, houseBill2.CA_MessageStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.CP, houseBill2.CA_ShipmentStatus);

				var inboundMessage2 = Factory.New<TSWMessage>();
				inboundMessage2.EM_MessageType = NZCMessage.MessageTypes.ResponseMsg;
				inboundMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				inboundMessage2.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRBIOResponseForSingleHouseBill.txt"));
				Factory.Save();
				processor = new MessageProcessorFactory(new LoggingInformation());
				processor.ProcessMessage(inboundMessage2);
				AssertEquals(LowValueConsignmentStatusList.Codes.Acknowledgement, houseBill1.CA_MessageStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.CP, houseBill1.CA_ShipmentStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.Acknowledgement, houseBill2.CA_MessageStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.CD, houseBill2.CA_ShipmentStatus);

				var inboundMessage3 = Factory.New<TSWMessage>();
				inboundMessage3.EM_MessageType = NZCMessage.MessageTypes.ResponseMsg;
				inboundMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				inboundMessage3.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRBIOResponseForBothHouseBills.txt"));
				Factory.Save();
				processor = new MessageProcessorFactory(new LoggingInformation());
				processor.ProcessMessage(inboundMessage3);
				AssertEquals(LowValueConsignmentStatusList.Codes.Acknowledgement, houseBill1.CA_MessageStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.CD, houseBill1.CA_ShipmentStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.Acknowledgement, houseBill2.CA_MessageStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.CD, houseBill2.CA_ShipmentStatus);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCustomsProcessingErrorResponse()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				outboundMessage.EM_ApplicationReference = "X00001002";
				inboundMessage.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRProcessingErrorResponse.txt"));
				oceanBill.CB_MessageReference = "X00001002";
				oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
				oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
				var houseBill1 = oceanBill.HouseBills.AddNew();
				houseBill1.CA_HouseBill = "869020238";
				houseBill1.CA_ConsignmentNum = 1;
				houseBill1.CA_MessageStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
				houseBill1.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.CC;
				var houseBill2 = oceanBill.HouseBills.AddNew();
				houseBill2.CA_HouseBill = "873409184";
				houseBill2.CA_ConsignmentNum = 2;
				houseBill2.CA_MessageStatus = LowValueManifestStatusList.Codes.SentToCustoms;
				houseBill2.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.CH;
				var houseBill3 = oceanBill.HouseBills.AddNew();
				houseBill3.CA_HouseBill = "873409357";
				houseBill3.CA_ConsignmentNum = 3;
				houseBill3.CA_MessageStatus = LowValueManifestStatusList.Codes.NotSentToCustoms;
				houseBill3.CA_ShipmentStatus = ZString.Empty;
				var houseBill4 = oceanBill.HouseBills.AddNew();
				houseBill4.CA_HouseBill = "873408745";
				houseBill4.CA_ConsignmentNum = 4;
				houseBill4.CA_MessageStatus = ZString.Empty;
				houseBill4.CA_ShipmentStatus = ZString.Empty;
				Factory.Save();

				var processor = new MessageProcessorFactory(new LoggingInformation());
				processor.ProcessMessage(inboundMessage);
				AssertEquals(LowValueManifestStatusList.Codes.Acknowledgement, oceanBill.CB_MessageStatus);
				AssertEquals(LowValueManifestStatusList.Codes.ManifestRejected, oceanBill.CB_CustomsStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.Acknowledgement, houseBill1.CA_MessageStatus);
				AssertEquals("Message has been rejected", LowValueConsignmentStatusList.Codes.EE, houseBill1.CA_ShipmentStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.Acknowledgement, houseBill2.CA_MessageStatus);
				AssertEquals("Message has been rejected", LowValueConsignmentStatusList.Codes.EE, houseBill2.CA_ShipmentStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.NotSentToCustoms, houseBill3.CA_MessageStatus);
				Assert(houseBill3.CA_ShipmentStatus.IsEmpty);
				Assert(houseBill4.CA_MessageStatus.IsEmpty);
				Assert(houseBill4.CA_ShipmentStatus.IsEmpty);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_OceanBill = "MOBL123456";
			oceanBill.HouseBills.AddNew();

			outboundMessage = Factory.New<TSWMessage>();
			outboundMessage.EM_MessageType = MessageTypeList.Codes.CRE;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_LinkedObject = oceanBill;

			inboundMessage = Factory.New<TSWMessage>();
			inboundMessage.EM_MessageType = NZCMessage.MessageTypes.ResponseMsg;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}
		CusSCAOceanBill oceanBill;
		TSWMessage outboundMessage;
		TSWMessage inboundMessage;

		void SetupICRWriteOffResponse(CusSCAHouse houseBill1, CusSCAHouse houseBill2)
		{
			outboundMessage.EM_ApplicationReference = "X00001215";
			inboundMessage.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRSeaCargoWriteOffResponse.txt"));
			oceanBill.CB_MessageReference = "X00001215";
			oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			houseBill1.CA_HouseBill = "Y202498";
			houseBill1.CA_ConsignmentNum = 1;
			houseBill2.CA_HouseBill = "Y202497";
			houseBill2.CA_ConsignmentNum = 2;
			Factory.Save();
		}
	}
}
