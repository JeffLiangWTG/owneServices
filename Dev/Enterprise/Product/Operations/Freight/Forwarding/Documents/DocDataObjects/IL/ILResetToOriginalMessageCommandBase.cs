using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	abstract class ILResetToOriginalMessageCommandBase : ILCustomCommandBase
	{
		protected ILResetToOriginalMessageCommandBase(IILElectronicMessageProvider messageProvider) : base(messageProvider)
		{
		}

		public override string Id => CommandIds.ResetToOriginal;

		public override string Caption => CommandResources.Captions.ResetToOriginal;

		public override bool Invoke()
		{
			if (!CheckAllowSendMessage())
			{
				return false;
			}
			var errorMessage = string.Empty;
			if (!CheckBeforeInvoke(ref errorMessage))
			{
				ShowMessage(errorMessage);
				return false;
			}

			var factory = messageProvider.Factory;

			var confirmation = Res.GetString("FF3C380A-A8AC-4DBF-8FEC-D6E7C69F51C6", "I confirm resetting to original and ignore the previously sent messages.");

			if (ShowConfirmation(Warning, confirmation)
				&& documentInfo?.DocumentData is IStmALogParent logParent)
			{
				ResetToOriginal();
				logParent.CreateStatusUpdateLog(documentInfo.Document.Name);
				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);

				Notify(new ResetToOriginalEvent(documentInfo.Document));
				Notify(new DocumentHardRefreshEvent(documentInfo.Document));
				return true;
			}

			return false;
		}

		public override bool IsEnabled
			=> CommandEnablerHelper.IsResetToOriginalCustomCommandEnabled(messageProvider, DocumentName);

		protected abstract void ResetToOriginal();

		protected abstract string Warning { get; }

		bool CheckBeforeInvoke(ref string errorMessage)
		{
			var messageExtensions = messageProvider.BusinessObject.GetSupporter().GetMessagingExtensions(documentInfo.Document, documentInfo.Descriptor?.MessageInstructions);
			var notifications = documentInfo.Services.Resolve<IUserNotificationService>();
			var canContinue = messageExtensions?.ContinueWithResetToOriginal(notifications);
			if (canContinue == false)
			{
				errorMessage = Res.GetString("681CD789-7B3B-4290-9172-57B578DBEA23", "Due to changes in the state of the entity, please reopen the form before resetting to original.");
				return false;
			}

			return true;
		}
	}
}
