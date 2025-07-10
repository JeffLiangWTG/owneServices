using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessagingProcess.Declaration;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.Security;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	public class CustomsMessagingProviderFactoryImplForTest : ICustomsMessagingProviderFactory
	{
		public CustomsMessagingProviderFactoryImplForTest(CustomsMessagingProviderImplForTest provider)
		{
			this.provider = provider;
		}
		readonly CustomsMessagingProviderImplForTest provider;

		ICustomsMessagingProvider ICustomsMessagingProviderFactory.CreateProvider(BusinessObject businessObject) => provider;
	}

	public class CustomsMessagingProviderFactoryForTest : ICustomsMessagingProviderFactory
	{
		ICustomsMessagingProvider ICustomsMessagingProviderFactory.CreateProvider(BusinessObject businessObject)
		{
			var owner = (DummyBizObjWithMessages)businessObject;
			var messenger = new CustomsMessengerImplForTest(owner);
			var provider = new CustomsMessagingProviderImplForTest(new[] { messenger });

			return provider;
		}
	}

	public class CustomsMessagingProviderImplForTest : ICustomsMessagingProvider
	{
		public CustomsMessagingProviderImplForTest(ICustomsMessenger[] messengers)
		{
			this.messengers = Argument.NotNull(messengers, nameof(messengers));
		}
		readonly ICustomsMessenger[] messengers;

		public MessageSendingNotification[] PreSendValidationMessagesForTest { get; set; }

		public Action<ActionChain> ConfigProcessForTesting { get; set; }

		public bool IsInTestMode { get; set; } = true;
		public bool EnableTestModeValidation { get; set; }
		public IReadOnlyCollection<ICustomsMessenger> GetMessengers() => messengers;

		public SecurityCheckpoint SendMessagesSecurityCheckpointForTesting { get; set; }
		public SecurityCheckpoint SendMessagesWithErrorsSecurityCheckpointForTesting { get; set; }
		public SecurityCheckpoint SendMessagesWithErrorsOverrideSecurityCheckpointForTesting { get; set; }

		public bool MustRunCreditCheckForTesting { get; set; } = true;
		public string CreditRestrictionMessageCaptionForTesting { get; set; } = "Credit Restriction Caption";
		public string DefaultApprovalRequestReasonForTesting { get; set; } = "Default Approval Request Reason";
		public ICreditControlledDocumentDelivery DocumentDeliveryObjectForTesting { get; set; }
	}

	public class CustomsMessagingProviderAllImplForTest : CustomsMessagingProviderImplForTest, ISupportPreSendValidation, ISupportConfigureProcess, ISupportSecurityCheckpoints, ISupportCreditAndDPSCheck
	{
		public CustomsMessagingProviderAllImplForTest(ICustomsMessenger[] messengers) : base(messengers)
		{
		}

		IReadOnlyCollection<MessageSendingNotification> ISupportPreSendValidation.RunPreSendValidation(ActionResult previousResult) => PreSendValidationMessagesForTest ?? Array.Empty<MessageSendingNotification>();
		void ISupportConfigureProcess.ConfigureProcess(ActionChain sendChain) => ConfigProcessForTesting?.Invoke(sendChain);
		SecurityCheckpoint ISupportSecurityCheckpoints.SendMessagesSecurityCheckpoint => SendMessagesSecurityCheckpointForTesting;
		SecurityCheckpoint ISupportSecurityCheckpoints.SendMessagesWithErrorsSecurityCheckpoint => SendMessagesWithErrorsSecurityCheckpointForTesting;
		SecurityCheckpoint ISupportSecurityCheckpoints.SendMessagesWithErrorsOverrideSecurityCheckpoint => SendMessagesWithErrorsOverrideSecurityCheckpointForTesting;

		bool ISupportCreditAndDPSCheckOptions.MustRunCreditCheck => MustRunCreditCheckForTesting;
		string ISupportCreditAndDPSCheckOptions.CreditRestrictionMessageCaption => CreditRestrictionMessageCaptionForTesting;
		string ISupportCreditAndDPSCheckOptions.DefaultApprovalRequestReason => DefaultApprovalRequestReasonForTesting;
		ICreditControlledDocumentDelivery ISupportCreditAndDPSCheck.DocumentDeliveryObject => DocumentDeliveryObjectForTesting;
	}

	public sealed class CustomsMessagingProviderWithCommonJobDecProviderImplForTest : CustomsMessagingProviderAllImplForTest, ICommonJobDeclarationProviderFactory
	{
		public CustomsMessagingProviderWithCommonJobDecProviderImplForTest(ICustomsMessenger[] messengers, ICommonJobDeclarationProvider provider) : base(messengers)
		{
			this.provider = provider;
		}
		readonly ICommonJobDeclarationProvider provider;

		ICommonJobDeclarationProvider ICommonJobDeclarationProviderFactory.Provider => provider;
	}

	public sealed class CustomsMessagingProviderWithJobDecProviderOnlyImplForTest : CustomsMessagingProviderImplForTest, ICommonJobDeclarationProviderFactory
	{
		public CustomsMessagingProviderWithJobDecProviderOnlyImplForTest(ICustomsMessenger[] messengers, ICommonJobDeclarationProvider provider) : base(messengers)
		{
			this.provider = provider;
		}
		readonly ICommonJobDeclarationProvider provider;

		ICommonJobDeclarationProvider ICommonJobDeclarationProviderFactory.Provider => provider;
	}

	public sealed class CustomsMessagingProviderWithAdditionalValidationImplForTest : CustomsMessagingProviderImplForTest, IValidateAdditionalBusinessObjects
	{
		public CustomsMessagingProviderWithAdditionalValidationImplForTest(BusinessObjectFactory factory, ICustomsMessenger[] messengers) : base(messengers)
		{
			LocalBO = factory.New<DummyBusinessObject>();
			LocalBO.Z0_Description = "No errors";
		}
		public DummyBusinessObject LocalBO { get; private set; }

		public IReadOnlyCollection<BusinessObject> AdditionalBusniessObjectsToValidate => new[] { LocalBO };
	}
}
