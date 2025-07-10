using System;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;

namespace Enterprise.Customs.TR.Manifest.Business.MessagingProcess
{
	public static class TRManifestCustomsMessenger
	{
		public static TRCustomsMessenger New(AsycudaManifestHeader header, ZString messageType, TRMessageSigner signer = null)
		{
			return new TRCustomsMessenger(header, CreateMessageGenerator(header, messageType), signer);
		}

		static ITRCustomsMessageGenerator CreateMessageGenerator(AsycudaManifestHeader header, ZString messageType)
		{
			switch (messageType)
			{
				case TRMessageTypes.Codes.TRO:
					return new TRManifestMessageGenerator(new ManifestMessageProvider(header));

				default:
					throw new NotSupportedException($"Message type: {messageType} not supported");
			}
		}
	}
}
