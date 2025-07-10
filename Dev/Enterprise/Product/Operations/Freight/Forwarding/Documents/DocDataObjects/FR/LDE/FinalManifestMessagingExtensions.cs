using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class FinalManifestMessagingExtensions : BaseMessagingExtensions
	{
		public FinalManifestMessagingExtensions(IDocument document, IStmALogParent logParent, IMessageInstructions messageInstructions)
		{
			Argument.NotNull(document, nameof(document));
			finalManifest = document?.Data.Value as FinalManifest;
			Argument.NotNull(finalManifest, nameof(finalManifest));
			Argument.NotNull(messageInstructions, nameof(messageInstructions));
			messageContext = new FrenchMessageContext(logParent, messageInstructions.DocumentName, ConsolDocumentDataStoreNames.FinalContainerManifestLDE);
		}

		readonly FinalManifest finalManifest;
		readonly FrenchMessageContext messageContext;

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications) => messageContext.ContinueWithSendingMessageAmendment(notifications);

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications) => messageContext.ContinueWithSendingMessageWithdrawal(notifications);

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications) => messageContext.ContinueWithResetToOriginal(notifications);
	}
}
