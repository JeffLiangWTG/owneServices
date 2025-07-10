using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business
{
	public static class MessageProcessorHelper
	{
		public static ZString GetMessageSignedBy(Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader header)
		{
			var messageSignedBy = header.Messages.Cast<TRManifestMessage>().OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault(x => x.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && x.EM_MessageType == TRMessageTypes.Codes.TRO && !x.EM_MessageOwner.IsEmpty);
			return messageSignedBy?.EM_MessageOwner ?? ZString.Empty;
		}
	}
}
