using System;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;

namespace Enterprise.Customs.TR.ETrade.Business.MessagingProcess
{
	public static class ETradeCustomsMessenger
	{
		public static TRCustomsMessenger New(AsycudaManifestHeader header, ZString messageType, TRMessageSigner signer = null)
		{
			return new TRCustomsMessenger(header, CreateMessageSender(header, messageType), signer);
		}

		static ETradeBaseMessageGenerator CreateMessageSender(AsycudaManifestHeader header, ZString messageType)
		{
			switch (messageType)
			{
				case TRMessageTypes.Codes.TRE:
					return new ETradeTemporaryRegistrationMessageGenerator(new ETradeTemporaryRegistrationMessageProvider(header));
				case TRMessageTypes.Codes.TRQ:
					return new ETradeQueryForRegNoMessageGenerator(new ETradeQueryForRegNoMessageProvider(header));
				case TRMessageTypes.Codes.TRS:
					return new ETradeSendForRegistrationNoMessageGenerator(new ETradeSendForRegistrationNoMessageProvider(header));
				case TRMessageTypes.Codes.TRI:
					return new ETradeQueryForInspectionClerkMessageGenerator(new ETradeQueryForInspectionClerkMessageProvider(header));
				case TRMessageTypes.Codes.TRL:
					return new ETradeQueryForInspectionLineMessageGenerator(new ETradeQueryForInspectionLineMessageProvider(header));
				case TRMessageTypes.Codes.TRB:
					return new ETradeQueryRemainingBillsforImpDecMessageGenerator(new ETradeQueryRemainingBillsforImpDecMessageProvider(header));
				case TRMessageTypes.Codes.TRD:
					return new ETradeDischargeListMessageGenerator(new ETradeDischargeListMessageProvider(header));
				case TRMessageTypes.Codes.TCD:
					return new ETradeComplementaryDecMessageGenerator(new ETradeComplementaryDecMessageProvider(header));

				default:
					throw new NotSupportedException($"Message type: {messageType} not supported");
			}
		}
	}
}
