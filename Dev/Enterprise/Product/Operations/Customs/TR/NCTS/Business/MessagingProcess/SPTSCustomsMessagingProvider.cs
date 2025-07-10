using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.TR.Business.MessagingProcess;

namespace Enterprise.Customs.TR.NCTS.Business.MessagingProcess
{
	public sealed class SPTSCustomsMessagingProvider : ITRCustomsMessagingProvider, ICustomsMessagingProvider, ISupportPreSendValidation
	{
		public static ICustomsMessagingProvider New(BusinessObject header, ZString messageType, TRMessageSigner signer) => new SPTSCustomsMessagingProvider((SPTSHeader)header, signer);

		SPTSCustomsMessagingProvider(SPTSHeader header, TRMessageSigner signer = null)
		{
			this.header = header;
			var messengers = CreateMessengers(header, signer);
			commonProvider = new TRCustomsMessagingCommonProvider(messengers, signer);
		}
		readonly SPTSHeader header;
		readonly TRCustomsMessagingCommonProvider commonProvider;

		static IReadOnlyCollection<ITRCustomsMessenger> CreateMessengers(SPTSHeader header, TRMessageSigner signer)
		{
			var generator = new SPTSMessageGenerator(new SPTSMessageProvider(header));
			return [new TRCustomsMessenger(header, generator, signer)];
		}

		GlbExternalPassword_TR ITRCustomsMessagingProvider.TRBPassword => commonProvider.TRBPassword;
		IReadOnlyCollection<ITRCustomsMessenger> ITRCustomsMessagingProvider.TRMessengers => commonProvider.TRMessengers;

		bool ICustomsMessagingProvider.IsInTestMode => commonProvider.IsInTestMode;
		bool ICustomsMessagingProvider.EnableTestModeValidation => commonProvider.EnableTestModeValidation;
		IReadOnlyCollection<ICustomsMessenger> ICustomsMessagingProvider.GetMessengers() => commonProvider.GetMessengers();

		IReadOnlyCollection<MessageSendingNotification> ISupportPreSendValidation.RunPreSendValidation(ActionResult previousResult)
		{
			var results = commonProvider.RunPreSendValidation();

			if (!header.RegistrationNumber.IsEmpty)
			{
				results.Add(new MessageSendingError(Res.GetString("3563D3F9-3DC4-47D0-A81A-D9C97D656474", "You are sending a SPTS Declaration which has a Registration No\r\nPlease first use Amend SPTS Declaration menu item to prepare the declaration for resending")));
			}

			return results;
		}
	}
}
