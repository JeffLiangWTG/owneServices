using System.Globalization;
using System.IO;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Customs.US.MessageContracts;
using CargoWise.Customs.US.MessageDefinitions.ExportManifest;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class UEMEDIMessageSender
	{
		readonly USExportAsycudaBill bill;
		readonly IXmlMessageObjectProvider<ManifestFiling> messageBuilder;
		readonly string billActionType;

		public UEMEDIMessageSender(USExportAsycudaBill bill, string action)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			billActionType = action;
			messageBuilder = new USMessageBuilder(new CBPManifestMessageProvider(bill, action));
		}

		public void SendUEMMessage()
		{
			var message = bill.Factory.New<UEMEDIMessage>();
			message.EM_MessageType = MessageTypeList.Codes.ExportManifestSubmission;
			message.EM_MessageSubType = MessageSubTypeList.GetMessageSubTypeFromActionType(billActionType);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = SerializeToMessageString();
			message.EM_LinkedObject = bill;
		}

		public ZString SerializeToMessageString()
		{
			var result = ZString.Empty;
			var manifestFiling = messageBuilder.GetMessage();
			var serializer = ZXmlSerializer.New(manifestFiling.GetType());
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				serializer.Serialize(writer, manifestFiling);
				result = writer.ToString();
			}

			return result;
		}
	}
}
