using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Manifest.Business.MessagingProcess
{
	public sealed class TRManifestCustomsMessagingProvider : ITRCustomsMessagingProvider, ICustomsMessagingProvider, ISupportPreSendValidation
	{
		public static ICustomsMessagingProvider New(BusinessObject header, ZString messageType, TRMessageSigner signer = null) => new TRManifestCustomsMessagingProvider((AsycudaManifestHeader)header, messageType, signer);

		TRManifestCustomsMessagingProvider(AsycudaManifestHeader header, ZString messageType, TRMessageSigner signer = null)
		{
			this.header = header;
			var messengers = CreateMessengers(header, messageType, signer);
			commonProvider = new TRCustomsMessagingCommonProvider(messengers, signer);
		}
		readonly AsycudaManifestHeader header;
		readonly TRCustomsMessagingCommonProvider commonProvider;

		static IReadOnlyCollection<ITRCustomsMessenger> CreateMessengers(AsycudaManifestHeader header, ZString messageType, TRMessageSigner signer) => [TRManifestCustomsMessenger.New(header, messageType, signer)];

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
				results.Add(new MessageSendingWarning(Res.GetString("7A94D740-9745-4879-A111-E375FBA630C6", "The status is wait for response message, if you send again, the message will be rejected.")));
			}

			return results;
		}
	}
}
