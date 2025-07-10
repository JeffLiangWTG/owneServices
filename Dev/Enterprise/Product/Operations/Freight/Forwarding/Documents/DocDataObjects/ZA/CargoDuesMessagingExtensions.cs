using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA
{
	sealed class CargoDuesMessagingExtensions : BaseMessagingExtensions
	{
		public CargoDuesMessagingExtensions(IDocument document, ForwardingConsol consol)
		{
			context = new MessageContext(document, consol);
		}
		readonly MessageContext context;

		static string confirmation => Res.GetString("c837c11e-6454-4d76-81bf-4c40cdc8ffa8", "Confirmation");

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
				var message = Res.GetString("8a1fe44e-fef2-4c19-8f85-f7fd9d02153d", "This function is not possible because TNPA have already responded to your original message and provided their Order No.");
				notifications?.ShowMessage(message, confirmation);
				return false;
			}

			return null;
		}

		#region Implementation

		void ShowAwaitingMessage(IUserNotifications notifications)
		{
			var message = Res.GetString("dfcac07d-037b-428e-a3c8-e9e84f5617dc", "A message has previously been sent and is awaiting reply from TNPA. Once TNPA reply, the TNPA Order Number will be populated here and then you can send further messages - either resend (as an amendment) or Withdraw/Cancel.");

			notifications?.ShowMessage(message, confirmation);
		}

		#endregion

		#region MessageContext

		class MessageContext
		{
			public MessageContext(IDocument document, ForwardingConsol consol)
			{
				this.document = document;
				this.consol = consol;
				this.cargoDues = document?.Data.Value as CargoDues;

				Argument.NotNull(document?.Data.Value, nameof(cargoDues));
			}
			readonly IDocument document;
			readonly ForwardingConsol consol;
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

			public IVisualizerDocumentData DocumentData => documentData ?? (documentData = GetDocumentData(DocumentName, consol));
			IVisualizerDocumentData documentData;

			IVisualizerDocumentData GetDocumentData(string documentName, ForwardingConsol consol)
			{
				var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
				var documentDataArray = documentDataLoader.Load(consol);

				var documentStoreName = string.Empty;
				switch (documentName)
				{
					case DocumentNames.CargoDuesImport:
					case DocumentNames.CargoDuesImportQuotation:
						documentStoreName = ConsolDocumentDataStoreNames.CargoDuesImport;
						break;
					case DocumentNames.CargoDuesExport:
					case DocumentNames.CargoDuesExportQuotation:
						documentStoreName = ConsolDocumentDataStoreNames.CargoDuesExport;
						break;
					case DocumentNames.CargoDuesLoadCoastwise:
					case DocumentNames.CargoDuesLoadCoastwiseQuotation:
						documentStoreName = ConsolDocumentDataStoreNames.CargoDuesLoadCoastwise;
						break;
					case DocumentNames.CargoDuesDischargeCoastwise:
					case DocumentNames.CargoDuesDischargeCoastwiseQuotation:
						documentStoreName = ConsolDocumentDataStoreNames.CargoDuesDischargeCoastwise;
						break;
				}

				return documentDataArray.FirstOrDefault(x => x.Name == documentStoreName);
			}
		}

		#endregion
	}
}
