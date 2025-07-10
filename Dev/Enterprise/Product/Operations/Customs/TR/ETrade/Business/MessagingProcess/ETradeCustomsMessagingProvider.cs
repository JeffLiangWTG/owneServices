using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.ETrade.Business.MessagingProcess
{
	public sealed class ETradeCustomsMessagingProvider : ITRCustomsMessagingProvider, ICustomsMessagingProvider, ISupportPreSendValidation
	{
		public static ICustomsMessagingProvider New(BusinessObject header, ZString messageType, TRMessageSigner signer) => new ETradeCustomsMessagingProvider((AsycudaManifestHeader)header, messageType, signer);

		ETradeCustomsMessagingProvider(AsycudaManifestHeader header, ZString messageType, TRMessageSigner signer = null)
		{
			this.header = header;
			var messengers = CreateMessengers(header, messageType, signer);
			commonProvider = new TRCustomsMessagingCommonProvider(messengers, signer);
		}
		readonly AsycudaManifestHeader header;
		readonly TRCustomsMessagingCommonProvider commonProvider;

		static IReadOnlyCollection<ITRCustomsMessenger> CreateMessengers(AsycudaManifestHeader header, ZString messageType, TRMessageSigner signer) => [ETradeCustomsMessenger.New(header, messageType, signer)];

		GlbExternalPassword_TR ITRCustomsMessagingProvider.TRBPassword => commonProvider.TRBPassword;
		IReadOnlyCollection<ITRCustomsMessenger> ITRCustomsMessagingProvider.TRMessengers => commonProvider.TRMessengers;

		bool ICustomsMessagingProvider.IsInTestMode => commonProvider.IsInTestMode;
		bool ICustomsMessagingProvider.EnableTestModeValidation => commonProvider.EnableTestModeValidation;
		IReadOnlyCollection<ICustomsMessenger> ICustomsMessagingProvider.GetMessengers() => commonProvider.GetMessengers();

		IReadOnlyCollection<MessageSendingNotification> ISupportPreSendValidation.RunPreSendValidation(ActionResult previousResult)
		{
			var results = commonProvider.RunPreSendValidation();

			if (header.MessageStatus == TRMessageStatusCodeList.Codes.Awaiting)
			{
				results.Add(new MessageSendingWarning(Res.GetString("C0D45EE0-DAE7-4F3B-9159-0B69FA5A877E", "The status is wait for response message, if you send again, the message will be rejected.")));
			}

			return results;
		}
	}
}
