using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;
using Enterprise.Customs.TR.NCTS.Business.Messaging;

namespace Enterprise.Customs.TR.NCTS.Business.MessagingProcess
{
	public sealed class Phase5NctsCustomsTR5MessagingProvider : ITRCustomsMessagingProvider, ICustomsMessagingProvider, ISupportPreSendValidation
	{
		public static ICustomsMessagingProvider New(BusinessObject header, ZString messageType, TRMessageSigner signer) => new Phase5NctsCustomsTR5MessagingProvider((NctsHeader)header, signer);

		public Phase5NctsCustomsTR5MessagingProvider(NctsHeader header, TRMessageSigner signer = null)
		{
			var messengers = CreateMessengers(header, signer);
			commonProvider = new TRCustomsMessagingCommonProvider(messengers, signer);
		}
		readonly TRCustomsMessagingCommonProvider commonProvider;

		static IReadOnlyCollection<ITRCustomsMessenger> CreateMessengers(NctsHeader header, TRMessageSigner signer)
		{
			var generator = new TR5MessageGenerator(new TRNctsMessageSender(header));
			return [new TRCustomsMessenger(header, generator, signer)];
		}

		GlbExternalPassword_TR ITRCustomsMessagingProvider.TRBPassword => commonProvider.TRBPassword;
		IReadOnlyCollection<ITRCustomsMessenger> ITRCustomsMessagingProvider.TRMessengers => commonProvider.TRMessengers;

		bool ICustomsMessagingProvider.IsInTestMode => commonProvider.IsInTestMode;
		bool ICustomsMessagingProvider.EnableTestModeValidation => commonProvider.EnableTestModeValidation;
		IReadOnlyCollection<ICustomsMessenger> ICustomsMessagingProvider.GetMessengers() => commonProvider.GetMessengers();

		IReadOnlyCollection<Customs.Business.MessageSendingNotification> ISupportPreSendValidation.RunPreSendValidation(ActionResult previousResult) => commonProvider.RunPreSendValidation();
	}
}
