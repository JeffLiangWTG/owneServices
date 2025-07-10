using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Business.MessageProcessors;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class ConsignmentSeaCargoTest : TestCaseWithFactory
	{
		public void TestCustomsStatus()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				houseBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
				Factory.Save();
				AssertEquals(LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, consignment.CustomsStatus);
			}
			consignment = new ConsignmentSeaCargo(new WriteOffResponseSeaCargo(new BaseTSWResponse(inboundMessage)), "INVALID", LowValueConsignmentStatusList.Codes.ConsignmentHeld, "");
			Assert(consignment.CustomsStatus.IsEmpty);
		}

		public void TestGoodsClearanceStatus()
		{
			AssertEquals(LowValueConsignmentStatusList.Codes.ConsignmentHeld, consignment.GoodsClearanceStatus);
		}

		public void TestCombinedStatus()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				houseBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.EH;
				Factory.Save();
				AssertEquals(LowValueConsignmentStatusList.Codes.EH, consignment.CombinedStatus);
			}
			consignment = new ConsignmentSeaCargo(new WriteOffResponseSeaCargo(new BaseTSWResponse(inboundMessage)), "INVALID", LowValueConsignmentStatusList.Codes.ConsignmentHeld, "");
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
			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.NoStatusReported, "XXX");

			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.ConsignmentInError, ConsignmentGoodsStatusList.Codes.Error);
			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.ConsignmentHeld, ConsignmentGoodsStatusList.Codes.Held);
			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, ConsignmentGoodsStatusList.Codes.WrittenOffCleared);
			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.DomesticTranshipmentApproved, ConsignmentGoodsStatusList.Codes.DomesticTranshipmentApproved);
			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved, ConsignmentGoodsStatusList.Codes.InternationalTranshipmentApproved);
			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.ImportDeclarationRequired, ConsignmentGoodsStatusList.Codes.ImportDeclarationRequired);
			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.MpiImportDecRequired, ConsignmentGoodsStatusList.Codes.MpiImportDeclarationRequired);
			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.ConsolidationIcrRequired, ConsignmentGoodsStatusList.Codes.ConsolidationIcrRequired);
			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined, ConsignmentGoodsStatusList.Codes.InternationalTranshipmentDeclined);
			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.DomesticTranshipmentDeclined, ConsignmentGoodsStatusList.Codes.DomesticTranshipmentDeclined);
			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.ExportDeclarationRequired, ConsignmentGoodsStatusList.Codes.ExportDeclarationRequired);
			AssertEnterpriseStatus(LowValueConsignmentStatusList.Codes.RescindPreviousStatusNotification, ConsignmentGoodsStatusList.Codes.RescindPreviousStatusNotification);

			AssertEquals("All possible ConsignmentGoodsStatusList.Codes are accounted for.", 12, new ConsignmentGoodsStatusList().Count);
		}

		void AssertEnterpriseStatus(string expectedEnterpriseStatus, string consignmentStatus)
		{
			var consignment = new ConsignmentSeaCargo(inboundResponse, "Y202498", consignmentStatus, "");
			AssertEquals(expectedEnterpriseStatus, consignment.EnterpriseStatus);
			AssertEquals("As Consignment", expectedEnterpriseStatus, consignment.EnterpriseStatus);
		}

		public void TestAgency()
		{
			AssertEquals(ResponsibleGovernmentAgencyList.Codes.NZCS, consignment.Agency);
		}

		public void TestSystemStatusICR()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
				oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
				inboundMessage.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRBioHoldResponse.txt"));
				Factory.Save();

				var processor = new MessageProcessorFactory(new LoggingInformation());
				processor.ProcessMessage(inboundMessage);
				AssertEquals(LowValueConsignmentStatusList.Codes.PH, houseBill.CA_ShipmentStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.Acknowledgement, houseBill.CA_MessageStatus);
			}
		}

		public void TestCRESystemStatus()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				outboundMessage.EM_ApplicationReference = "X00001002";
				inboundMessage.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\CREWriteOffResponse.txt"));
				oceanBill.CB_MessageReference = "X00001002";
				oceanBill.CB_RL_NKPortOfDischarge = "AUSYD";
				oceanBill.CB_RL_NKPortOfLoading = "NZAKL";
				houseBill.CA_HouseBill = "869020238";
				Factory.Save();

				var processor = new MessageProcessorFactory(new LoggingInformation());
				processor.ProcessMessage(inboundMessage);
				AssertEquals(LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, houseBill.CA_ShipmentStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.Acknowledgement, houseBill.CA_MessageStatus);
			}
		}

		public void TestICRStatusRejectionMessage()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				outboundMessage.EM_ApplicationReference = "X00001002";
				inboundMessage.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRProcessingErrorResponse.txt"));
				oceanBill.CB_MessageReference = "X00001002";
				oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
				oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
				houseBill.CA_HouseBill = "869020238";
				oceanBill.CB_MessageStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
				Factory.Save();

				var processor = new MessageProcessorFactory(new LoggingInformation());
				processor.ProcessMessage(inboundMessage);
				AssertEquals("Message has been processed", "RCV", inboundMessage.EM_Status);
				AssertEquals("Message was a rejection error message", "REJ", oceanBill.CB_CustomsStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.Acknowledgement, oceanBill.CB_MessageStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.Acknowledgement, houseBill.CA_MessageStatus);
				AssertEquals(LowValueConsignmentStatusList.Codes.EE, houseBill.CA_ShipmentStatus);
				AssertEquals("Consignment Error", houseBill.ShipmentStatusDescription);
			}
		}

		public void TestConsignment()
		{
			oceanBill.IsEmptyContainerMode = false;

			var consignmentSeaCargo = new ConsignmentSeaCargo(new WriteOffResponseSeaCargo(new BaseTSWResponse(inboundMessage)), "Y202500", LowValueConsignmentStatusList.Codes.ConsignmentHeld, string.Empty);
			AssertNull("Should be null when there is no related house bill which CA_HouseBill is Y202500.", consignmentSeaCargo.Consignment);

			oceanBill.IsEmptyContainerMode = true;

			consignmentSeaCargo = new ConsignmentSeaCargo(new WriteOffResponseSeaCargo(new BaseTSWResponse(inboundMessage)), "Y202500", LowValueConsignmentStatusList.Codes.ConsignmentHeld, string.Empty);
			AssertSame("Should be the first house bill when there IsEmptyContainerMode is true.", houseBill, consignmentSeaCargo.Consignment);
		}

		protected override void SetUp()
		{
			base.SetUp();
			oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_MessageReference = "X00001215";
			houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "Y202498";
			houseBill.CA_MessageStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			outboundMessage = Factory.New<TSWMessage>();
			outboundMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_ApplicationReference = "X00001215";
			outboundMessage.EM_LinkedObject = oceanBill;

			inboundMessage = Factory.New<TSWMessage>();
			inboundMessage.EM_MessageType = Declaration.NZCMessage.MessageTypes.ResponseMsg;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRSeaCargoWriteOffResponse.txt"));

			inboundResponse = new WriteOffResponseSeaCargo(new BaseTSWResponse(inboundMessage));
			consignment = new ConsignmentSeaCargo(inboundResponse, "Y202498", LowValueConsignmentStatusList.Codes.ConsignmentHeld, "");
		}

		CusSCAOceanBill oceanBill;
		CusSCAHouse houseBill;
		TSWMessage outboundMessage;
		TSWMessage inboundMessage;
		WriteOffResponseSeaCargo inboundResponse;
		IWriteOffStatus consignment;
	}
}
