using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.ZA.Business.DeclarationDocumentConstants;

namespace Enterprise.Customs.ZA.Business.Documents
{
	sealed class CargoDuesMessagingExtensions : BaseMessagingExtensions
	{
		public CargoDuesMessagingExtensions(IDocument document, JobDeclaration declaration)
		{
			context = new MessageContext(document, declaration);
		}
		readonly MessageContext context;
		static string confirmation => Res.GetString("b4177ee0-f863-427e-90e6-bb28a51345c4", "Confirmation");

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications)
		{
			if (context.IsOrderDocument && context.IsAwaitingResponse)
			{
				ShowAwaitingMessage(notifications);
				return false;
			}

			return null;
		}

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			if (context.IsOrderDocument && context.IsAwaitingResponse)
			{
				ShowAwaitingMessage(notifications);
				return false;
			}

			return null;
		}

		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications)
		{
			if (context.IsOrderDocument && !context.CargoDues.TNPAOrderNumber.IsEmpty)
			{
				var message = Res.GetString("59250bea-c94e-4fd0-8e18-9f9b9e603a33", "This function is not possible because TNPA have already responded to your original message and provided their Order No.");
				notifications?.ShowMessage(message, confirmation);
				return false;
			}

			return null;
		}

		#region Implementation

		void ShowAwaitingMessage(IUserNotifications notifications)
		{
			var message = Res.GetString("bc60b4bc-6409-4014-ba8c-e90350186d57", "A message has previously been sent and is awaiting reply from TNPA. Once TNPA reply, the TNPA Order Number will be populated here and then you can send further messages - either resend (as an amendment) or Withdraw/Cancel.");

			notifications?.ShowMessage(message, confirmation);
		}

		#endregion

		#region MessageContext

		class MessageContext
		{
			public MessageContext(IDocument document, JobDeclaration declaration)
			{
				this.document = document;
				this.declaration = declaration;
				this.cargoDues = document?.Data.Value as CargoDues;

				Argument.NotNull(document?.Data.Value, nameof(cargoDues));
			}
			readonly IDocument document;
			readonly JobDeclaration declaration;
			readonly CargoDues cargoDues;

			public CargoDues CargoDues => cargoDues;

			public string DocumentName => document.Name;

			public bool IsOrderDocument => !CargoDues.IsQuotationDocument;

			public bool IsAwaitingResponse => TransmissionCode != null && MessageEventCodes.SentMessagesEventCodes.Contains(TransmissionCode)
				&& ResponseCode != null && string.IsNullOrEmpty(ResponseCode);

			public IEnumerable<IDialog> Dialogs => dialogs ?? (dialogs = (DocumentData as IStmALogParent)?.GetDialogs(DocumentName, false));
			IEnumerable<IDialog> dialogs;

			public IDialog LastDialog => lastDialog ?? (lastDialog = Dialogs?.LastOrDefault());
			IDialog lastDialog;

			public string TransmissionCode => LastDialog?.TransmissionCode;
			public string ResponseCode => LastDialog?.ResponseCode;

			public IVisualizerDocumentData DocumentData => documentData ?? (documentData = GetDocumentData(DocumentName, declaration));
			IVisualizerDocumentData documentData;

			IVisualizerDocumentData GetDocumentData(string documentName, JobDeclaration declaration)
			{
				var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
				var documentDataArray = documentDataLoader.Load(declaration);

				var documentStoreName = string.Empty;
				switch (documentName)
				{
					case DocumentNames.CargoDuesImport:
						documentStoreName = DocumentDataStoreNames.CargoDuesImport;
						break;
					case DocumentNames.CargoDuesExport:
						documentStoreName = DocumentDataStoreNames.CargoDuesExport;
						break;
				}

				return documentDataArray.FirstOrDefault(x => x.Name == documentStoreName);
			}
		}

		#endregion
	}
}
