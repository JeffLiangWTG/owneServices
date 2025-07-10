using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class DossierMessagingExtensions : BaseMessagingExtensions
	{
		public DossierMessagingExtensions(IDocument document, IStmALogParent logParent, IMessageInstructions messageInstructions)
		{
			Argument.NotNull(document, nameof(document));
			dossier = document?.Data.Value as Dossier;
			Argument.NotNull(dossier, nameof(dossier));

			this.logParent = Argument.NotNull(logParent, nameof(logParent));
			Argument.NotNull(messageInstructions, nameof(messageInstructions));
			messageContext = new FrenchMessageContext(logParent, messageInstructions.DocumentName, GetDataStoreName(messageInstructions.DocumentName));
		}

		readonly Dossier dossier;
		readonly FrenchMessageContext messageContext;
		readonly IStmALogParent logParent;

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications) => messageContext.ContinueWithSendingMessageAmendment(notifications);

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications) => messageContext.ContinueWithSendingMessageWithdrawal(notifications);

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications) => messageContext.ContinueWithResetToOriginal(notifications);

		string GetDataStoreName(string documentName)
		{
			if (logParent is ForwardingShipment)
			{
				return documentName == FrenchPortsConstants.DocumentNames.DOSImport
					? ShipmentDocumentDataStoreNames.DossierImport
					: ShipmentDocumentDataStoreNames.DossierExport;
			}
			if (logParent is ForwardingConsol)
			{
				return documentName == FrenchPortsConstants.DocumentNames.DOSImport
					? ConsolDocumentDataStoreNames.DossierImport
					: ConsolDocumentDataStoreNames.DossierExport;
			}

			return documentName;
		}
	}
}
