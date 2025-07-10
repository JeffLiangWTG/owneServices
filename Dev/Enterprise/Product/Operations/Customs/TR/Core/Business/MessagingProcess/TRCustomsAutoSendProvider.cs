using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business.MessagingProcess
{
	internal sealed class TRCustomsAutoSendProviderFactory : ICustomsMessagingProviderFactory
	{
		public static void SendMessage<TBOInterfaceProvider>(object businessObjectWrapper, Func<IMessageSender, ITRCustomsMessageGenerator> generatorCreator, Action<ActionResult> afterSendMessage = null) where TBOInterfaceProvider : IMessageSender
		{
			if (businessObjectWrapper is BusinessObject businessObject)
			{
				var providerFactory = new TRCustomsAutoSendProviderFactory(afterSendMessage, generatorCreator, (bo) => (IMessageSender)Activator.CreateInstance(typeof(TBOInterfaceProvider), bo));
				SendMessage(businessObject, providerFactory);
			}
		}

		static void SendMessage(BusinessObject bo, TRCustomsAutoSendProviderFactory providerFactory)
		{
			var customsMessagingSupporter = new CustomsMessagingSupporter(bo, providerFactory);
			SendMessagesProcess.SendMessages(new SendMessagesBusinessActionProvider(customsMessagingSupporter));
		}

		public TRCustomsAutoSendProviderFactory(Action<ActionResult> afterSendMessage, Func<IMessageSender, ITRCustomsMessageGenerator> generatorCreator, Func<BusinessObject, IMessageSender> senderCreator)
		{
			this.afterSendMessage = afterSendMessage;
			this.senderCreator = senderCreator;
			this.generatorCreator = generatorCreator;
		}
		readonly Action<ActionResult> afterSendMessage;
		readonly Func<BusinessObject, IMessageSender> senderCreator;
		readonly Func<IMessageSender, ITRCustomsMessageGenerator> generatorCreator;

		ICustomsMessagingProvider ICustomsMessagingProviderFactory.CreateProvider(BusinessObject businessObject)
		{
			var sender = senderCreator(businessObject);
			return new TRCustomsAutoSendProvider(sender, afterSendMessage, generatorCreator(sender));
		}
	}

	internal sealed class TRCustomsAutoSendProvider : ITRCustomsMessagingProvider, ICustomsMessagingProvider, ISupportPreSendValidation
	{
		public TRCustomsAutoSendProvider(IMessageSender sender, Action<ActionResult> afterSendMessage, ITRCustomsMessageGenerator generator)
		{
			var messengers = CreateMessengers(sender, afterSendMessage, generator);
			commonProvider = new TRCustomsMessagingCommonProvider(messengers, null);
		}
		readonly TRCustomsMessagingCommonProvider commonProvider;

		static IReadOnlyCollection<ITRCustomsMessenger> CreateMessengers(IMessageSender sender, Action<ActionResult> afterSendMessage, ITRCustomsMessageGenerator generator) => [new TRCustomsAutoSendMessenger(sender, generator, afterSendMessage)];

		GlbExternalPassword_TR ITRCustomsMessagingProvider.TRBPassword => commonProvider.TRBPassword;
		IReadOnlyCollection<ITRCustomsMessenger> ITRCustomsMessagingProvider.TRMessengers => commonProvider.TRMessengers;

		bool ICustomsMessagingProvider.IsInTestMode => commonProvider.IsInTestMode;
		bool ICustomsMessagingProvider.EnableTestModeValidation => commonProvider.EnableTestModeValidation;
		IReadOnlyCollection<ICustomsMessenger> ICustomsMessagingProvider.GetMessengers() => commonProvider.GetMessengers();

		IReadOnlyCollection<Customs.Business.MessageSendingNotification> ISupportPreSendValidation.RunPreSendValidation(ActionResult previousResult) => commonProvider.RunPreSendValidation();
	}
}
