using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MessagingProcess.Declaration;
using Enterprise.Security;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public static class CustomsMessagingProviderExtensions
	{
		public static IReadOnlyCollection<MessageSendingNotification> RunPreSendValidation(this ICustomsMessagingProvider provider, ActionResult previousResult)
		{
			var notifications = new List<MessageSendingNotification>();

			if (provider.IsInTestMode && provider.EnableTestModeValidation)
			{
				notifications.Add(new MessageSendingWarning(MessageSendingValidation.WarningWhenInTestModeText));
			}

			if (provider is ICommonJobDeclarationProviderFactory commoonProvider)
			{
				notifications.AddRange(commoonProvider.Provider.RunPreSendValidation(previousResult));
			}

			if (provider is ISupportPreSendValidation validator)
			{
				notifications.AddRange(validator.RunPreSendValidation(previousResult));
			}

			return notifications;
		}

		public static IReadOnlyCollection<BusinessObject> GetAdditionalValidationBusinessObjects(this ICustomsMessagingProvider provider)
		{
			if (provider is IValidateAdditionalBusinessObjects additional)
			{
				return additional.AdditionalBusniessObjectsToValidate;
			}

			return Array.Empty<BusinessObject>();
		}

		public static void ConfigureProcess(this ICustomsMessagingProvider provider, ActionChain sendChain)
		{
			if (provider is ICommonJobDeclarationProviderFactory commonProvider)
			{
				commonProvider.Provider.ConfigureProcess(sendChain);
			}

			if (provider is ISupportConfigureProcess config)
			{
				config.ConfigureProcess(sendChain);
			}
		}

		public static SecurityCheckpoint GetSendMessagesSecurityCheckpoint(this ICustomsMessagingProvider provider)
		{
			if (provider is ISupportSecurityCheckpoints checkpoints)
			{
				return checkpoints.SendMessagesSecurityCheckpoint;
			}

			if (provider is ICommonJobDeclarationProviderFactory commonProvider)
			{
				return commonProvider.Provider.SendMessagesSecurityCheckpoint;
			}

			return null;
		}

		public static SecurityCheckpoint GetSendMessagesWithErrorsSecurityCheckpoint(this ICustomsMessagingProvider provider)
		{
			if (provider is ISupportSecurityCheckpoints checkpoints)
			{
				return checkpoints.SendMessagesWithErrorsSecurityCheckpoint;
			}

			if (provider is ICommonJobDeclarationProviderFactory commonProvider)
			{
				return commonProvider.Provider.SendMessagesWithErrorsSecurityCheckpoint;
			}

			return null;
		}

		public static SecurityCheckpoint GetSendMessagesWithErrorsOverrideSecurityCheckpoint(this ICustomsMessagingProvider provider)
		{
			if (provider is ISupportSecurityCheckpoints checkpoints)
			{
				return checkpoints.SendMessagesWithErrorsOverrideSecurityCheckpoint;
			}

			if (provider is ICommonJobDeclarationProviderFactory commonProvider)
			{
				return commonProvider.Provider.SendMessagesWithErrorsOverrideSecurityCheckpoint;
			}

			return null;
		}

		public static ISupportCreditAndDPSCheck GetCreditAndDPSCheckSupporter(this ICustomsMessagingProvider provider)
		{
			if (provider is ISupportCreditAndDPSCheck checkSupporter)
			{
				return checkSupporter;
			}

			if (provider is ICommonJobDeclarationProviderFactory commonProvider)
			{
				return commonProvider.Provider;
			}

			return null;
		}
	}
}
