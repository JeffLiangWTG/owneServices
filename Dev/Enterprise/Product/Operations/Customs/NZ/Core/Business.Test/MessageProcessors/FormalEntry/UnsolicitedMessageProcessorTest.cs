using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Core;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.FormalEntry.Testing
{
	public class UnsolicitedMessageProcessorTest : TestCaseWithFactory
	{
		public void TestEmailGroupUserSettings()
		{
			var unsolicitedDOGroup = Guid.NewGuid();
			ZString unsolicitedDOMode = Constants.EmailTo.NominatedGroup;

			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unsolicitedDOGroup);
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unsolicitedDOMode);

			var processor = new UnsolicitedMessageProcessor_ForTesting(new LoggingInformation());
			AssertEquals("UnsolicitedDeliveryOrderGroup", unsolicitedDOGroup, processor.Get_UnsolicitedDOGroup());
			AssertEquals("UnsolicitedDeliveryOrderMode", unsolicitedDOMode, processor.Get_UnsolicitedDOMode());
		}

		public void TestProcessUnsolicitedMessage()
		{
			var message = Factory.New<NZCMessage>();
			message.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			message.EM_MessageText = @"UNH+553211+CUSRES:D:96B:UN+22326971872015'BGM+932+61352032:01'FTX+DIN+++39 LOOSE PACKAGE(S) OR ITEM(S)'TDT+20++4+++++:::NZ99'LOC+9+NZAKL'GIS+819:120:143'NAD+AL+40342956C:ZZZ:143+FONTERRA LIMITED'NAD+CB+40342956C:ZZZ:143+FONTERRA LIMITED'DOC+964+1'PAC+39++CT'RFF+HWB:08651411091'UNT+12+553211'";
			var logger = new LoggingInformation();
			var messageProcessorFactory = new MessageProcessorFactory(logger);
			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("message.EM_Status should indicated the unsolicited Delivery Order has been processed.", EDIMessage.Status.Received, message.EM_Status);

			var expectedOutputEmail = @"Unsolicited Delivery Order

An unsolicited Delivery Order message has been received from New Zealand Customs.
Below are the details contained in the message:

Delivery Order
----------------------------------------------------------------------
Client Reference Number: 22326971872015
Entry Number   : 61352032
Message No     : 553211

Message Status : (819) Delivery Order Herewith.
                 Method of Payment as Specified.
Transport Carriage : NZ99
Port of Loading : NZAKL

Client         : FONTERRA LIMITED
Broker         : FONTERRA LIMITED
Bill Reference : 08651411091

Delivery Instructions
----------------------------------------------------------------------
39 LOOSE PACKAGE(S) OR ITEM(S)
";
			AssertEquals("Output generated from unsolicited Delivery Order", expectedOutputEmail, message.EM_MessageInterpretation);
		}

		class UnsolicitedMessageProcessor_ForTesting : UnsolicitedMessageProcessor
		{
			public UnsolicitedMessageProcessor_ForTesting(LoggingInformation logger) : base(logger)
			{
			}

			public void Call_SetupPropertiesForMessageProcessing(NZCMessage message)
			{
				base.SetupPropertiesForMessageProcessing(message);
			}

			public ZGuid Get_AcknowledgementEmailGroup() => AcknowledgementEmailGroup;
			public ZString Get_AcknowledgementEmailMode() => AcknowledgementEmailMode;
			public ZGuid Get_ImpedimentEmailGroup() => ImpedimentEmailGroup;
			public ZString Get_ImpedimentEmailMode() => ImpedimentEmailMode;
			public ZGuid Get_ErrorEmailGroup() => ErrorEmailGroup;
			public ZString Get_ErrorEmailMode() => ErrorEmailMode;

			public ZGuid Get_UnsolicitedDOGroup() => UnsolicitedDOGroup;

			public ZString Get_UnsolicitedDOMode() => UnsolicitedDOMode;
		}
	}
}
