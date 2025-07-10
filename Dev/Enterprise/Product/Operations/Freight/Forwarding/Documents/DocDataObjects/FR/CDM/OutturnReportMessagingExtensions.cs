using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class OutturnReportMessagingExtensions : BaseMessagingExtensions
	{
		public OutturnReportMessagingExtensions(IDocument document, IStmALogParent logParent, IMessageInstructions messageInstructions)
		{
			Argument.NotNull(document, nameof(document));
			outturnReport = document?.Data.Value as OutturnReport;
			Argument.NotNull(outturnReport, nameof(outturnReport));

			Argument.NotNull(messageInstructions, nameof(messageInstructions));
			messageContext = new FrenchMessageContext(logParent, messageInstructions.DocumentName, ConsolDocumentDataStoreNames.OutturnReportCDM);
		}

		readonly OutturnReport outturnReport;
		readonly FrenchMessageContext messageContext;

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications) => messageContext.ContinueWithSendingMessageAmendment(notifications);

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications) => messageContext.ContinueWithSendingMessageWithdrawal(notifications);

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications) => messageContext.ContinueWithResetToOriginal(notifications);
	}
}
