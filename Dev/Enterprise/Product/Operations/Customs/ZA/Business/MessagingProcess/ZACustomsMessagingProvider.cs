using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Business.MessagingProcess.Declaration;
using Enterprise.Environment;

namespace Enterprise.Customs.ZA.Business.MessagingProcess
{
	public sealed class ZACustomsMessagingProviderFactory : ICustomsMessagingProviderFactory
	{
		ICustomsMessagingProvider ICustomsMessagingProviderFactory.CreateProvider(BusinessObject businessObject) => ZACustomsMessagingProvider.New((JobDeclaration)businessObject);
	}

	public sealed class ZACustomsMessagingProvider :
		ICustomsMessagingProvider,
		ISupportConfigureProcess,
		ICommonJobDeclarationProviderFactory,
		IValidateAdditionalBusinessObjects,
		ISupportCreditAndDPSCheckOptions
	{
		public static ZACustomsMessagingProvider New(JobDeclaration jobDeclaration)
		{
			var jobDeclarationMessageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(jobDeclaration) { IsMessagingPOC = true };

			return new ZACustomsMessagingProvider(jobDeclarationMessageSendingObjectParent, CreateMessengers(jobDeclarationMessageSendingObjectParent));
		}

		static List<ICustomsMessenger> CreateMessengers(JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
		{
			return jobDeclarationMessageSendingObjectParent.SendingObjectsCollection.Cast<MessageSendingObject>().Select(s => ZACustomsMessenger.New(s)).ToList();
		}

		ZACustomsMessagingProvider(JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent, IReadOnlyCollection<ICustomsMessenger> messengers)
		{
			isInTestMode = GetIsTestMode(jobDeclarationMessageSendingObjectParent.ParentDeclaration);

			DeclarationWrapper = jobDeclarationMessageSendingObjectParent;
			DeferrredIntegrator = new DeferredSubmissionIntegrator(DeclarationWrapper);
			jobDeclarationMessagingProvider = new JobDeclarationMessagingProvider<JobDeclarationMessageSendingObjectParent, MessageSendingObject>(DeclarationWrapper, this);
			this.messengers = messengers;
		}
		public readonly JobDeclarationMessageSendingObjectParent DeclarationWrapper;
		public readonly DeferredSubmissionIntegrator DeferrredIntegrator;
		readonly JobDeclarationMessagingProvider<JobDeclarationMessageSendingObjectParent, MessageSendingObject> jobDeclarationMessagingProvider;
		readonly IReadOnlyCollection<ICustomsMessenger> messengers;
		readonly bool isInTestMode;

		public IReadOnlyCollection<ICustomsMessenger> Messengers => messengers;

		static bool GetIsTestMode(JobDeclaration declaration) => Env.Registry.ZACustoms.GetIsTestMode(declaration.Branch);

		ActionResult UpdateDeferment(ActionResult previousResult)
		{
			var result = previousResult;

			if (result.Success && result.DataSource is DeferredSubmission)
			{
				DeferrredIntegrator.UpdateDeferment();
			}

			return result;
		}

		IReadOnlyCollection<BusinessObject> IValidateAdditionalBusinessObjects.AdditionalBusniessObjectsToValidate => new[] { DeclarationWrapper };

		void ISupportConfigureProcess.ConfigureProcess(ActionChain subChain)
		{
			subChain.FindAction(SendMessagesProcess.SendMessageActions.ShowSendDialog)
				.InsertActionAfter("ZADefermentUpdate", UpdateDeferment, ActionLink.Success);
		}

		bool ICustomsMessagingProvider.IsInTestMode => isInTestMode;
		bool ICustomsMessagingProvider.EnableTestModeValidation => true;
		IReadOnlyCollection<ICustomsMessenger> ICustomsMessagingProvider.GetMessengers() => messengers;

		ICommonJobDeclarationProvider ICommonJobDeclarationProviderFactory.Provider => jobDeclarationMessagingProvider;

		#region ISupportCreditAndDPSCheckOptions
		bool ISupportCreditAndDPSCheckOptions.MustRunCreditCheck => true;
		string ISupportCreditAndDPSCheckOptions.CreditRestrictionMessageCaption => DeclarationWrapper.ParentDeclaration.CreditRestrictionMessageCaption;
		string ISupportCreditAndDPSCheckOptions.DefaultApprovalRequestReason => JobDeclarationMessageSendingObjectParent.DocumentApprovalReasonDescription;
		#endregion
	}
}
