using System.Collections.Generic;
using System.Linq;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	abstract class ILSendWithdrawalCustomCommandBase : ILSendCustomCommandBase
	{
		public ILSendWithdrawalCustomCommandBase(IILElectronicMessageProvider messageProvider)
			: base(messageProvider, true)
		{
		}

		public override string Id => CommandIds.SendWithdrawal;

		public override string Caption => CommandResources.Captions.SendWithdrawal;

		public override bool IsEnabled => CommandEnablerHelper.IsSendWithdrawalCustomCommandEnabled(messageProvider, DocumentName);

		protected override Event MessageSentEvent => Events.MessageWithdrawCancelRequest;

		protected override bool CheckBeforeInvoke(ref string errorMessage)
		{
			var messageExtensions = messageProvider.BusinessObject.GetSupporter().GetMessagingExtensions(documentInfo.Document, documentInfo.Descriptor?.MessageInstructions);
			var notifications = documentInfo.Services.Resolve<IUserNotificationService>();
			var canContinue = messageExtensions?.ContinueWithSendingMessageWithdrawal(notifications);
			if (canContinue == false)
			{
				errorMessage = Res.GetString("8D50AB40-1E7B-441C-8273-89B872747FC0", "Due to changes in the state of the entity, please reopen the form before sending a withdrawal message.");
				return false;
			}

			return true;
		}

		protected override KeyValuePair<string, string>[] GetEventParameters()
		{
			var dictionary = base.GetEventParameters().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			dictionary.Add("RFN", MessageReference);
			return dictionary.ToArray();
		}
	}
}
