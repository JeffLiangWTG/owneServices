using System.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output;

namespace Enterprise.Customs.US.Business
{
	class AESInboundInterchangeProcessor : InboundInterchangeProcessor
	{
		public AESInboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string[] ApplicationCodes
		{
			get { return new string[] { CBPEDIInterchange.ApplicationCodes.USCustomsExport }; }
		}

		protected override Enterprise.Messaging.Business.IInboundMessageCreator GetMessageCreator(Enterprise.Messaging.Business.EDIInterchange interchange)
		{
			var messageType = interchange.EI_HeaderText.SubstringSafe(21, 2);

			switch (messageType)
			{
				case ApplicationIdentifierCodeList.AES.CommodityShipmentWarningReminder:
					return WarningMessageCreator;
				default:
					return ShipmentMessageCreator;
			}
		}

		Enterprise.Messaging.Business.IInboundMessageCreator ShipmentMessageCreator
		{
			get { return shipmentMessageCreator ?? (shipmentMessageCreator = new AESShipmentInboundMessageCreator()); }
		}
		Enterprise.Messaging.Business.IInboundMessageCreator shipmentMessageCreator;

		Enterprise.Messaging.Business.IInboundMessageCreator WarningMessageCreator
		{
			get { return warningMessageCreator ?? (warningMessageCreator = new AESWarningInboundMessageCreator()); }
		}
		Enterprise.Messaging.Business.IInboundMessageCreator warningMessageCreator;

		#region AESInboundMessageCreator Classes

		abstract class AESBaseInboundMessageCreator<ControlMessageBlockAa, ControlMessageBlockBb, ControlMessageBlockYy, ControlMessageBlockZz>
			: InboundMessageCreator<ControlMessageBlockAa, ControlMessageBlockBb, ControlMessageBlockYy, ControlMessageBlockZz, AESTIREDIMessage>
			where ControlMessageBlockAa : MessageBlock, IAESControlMessageBlockA, new()
			where ControlMessageBlockBb : MessageBlock, IAESControlMessageBlockB, new()
			where ControlMessageBlockYy : MessageBlock, IAESControlMessageBlockY, new()
			where ControlMessageBlockZz : MessageBlock, IAESControlMessageBlockZ, new()
		{
			protected override ZString GetMessageNum(ControlMessageBlockAa msgBlockA, ControlMessageBlockBb msgBlockB, Stream messageTextStream, ControlMessageBlockYy msgBlockY, ControlMessageBlockZz msgBlockZ)
			{
				if (!msgBlockA.BatchControlNumber.Trim('0').IsEmpty)
				{
					return AESTIRMessageNumberEncoder.Decode(msgBlockA.BatchControlNumber).ToString();
				}

				return ZString.Empty;
			}

			protected override ZString GetApplicationIdentifier(ControlMessageBlockAa msgBlockA, ControlMessageBlockBb msgBlockB)
			{
				return msgBlockA.ApplicationIdentifier;
			}
		}

		class AESShipmentInboundMessageCreator : AESBaseInboundMessageCreator<AESCommShipAXT, AESCommShipBXT, AESCommShipYXT, AESCommShipZXT>
		{
		}

		class AESWarningInboundMessageCreator : AESBaseInboundMessageCreator<AESCommWarnAXN, AESCommWarnBXN, AESCommWarnYXN, AESCommWarnZXN>
		{
		}

		#endregion
	}
}
