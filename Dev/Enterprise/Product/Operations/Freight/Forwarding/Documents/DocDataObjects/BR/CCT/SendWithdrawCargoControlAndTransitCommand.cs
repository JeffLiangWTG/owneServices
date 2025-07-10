using System.Collections.Generic;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	sealed class SendWithdrawCargoControlAndTransitCommand : CargoControlAndTransitCommand
	{
		public override string Id => CommandIds.SendWithdrawal;
		public override string Caption => CommandResources.Captions.SendWithdrawal;
		string UnableToSendCaption => Res.GetString("1E211799-2E86-4D34-8DEA-8A76DDD58BAE", "Unable to send message");

		public override bool Invoke()
		{
			var errorMessage = string.Empty;
			var check = CheckAllowSendMessage()
				&& CheckHasNoChanges(out errorMessage)
				&& CheckHasNoErrors(out errorMessage)
				&& CheckHasNoMessageErrors(out errorMessage);

			if (!check)
			{
				ShowMessage(errorMessage, UnableToSendCaption);
				return false;
			}

			var withdrawalMessageSender = new WithdrawalMessageSender(
				documentInfo?.Descriptor?.MessageInstructions,
				documentInfo?.Document,
				documentInfo?.DocumentData,
				documentInfo?.Services);

			var map = new MacroMap(new Dictionary<string, object>
			{
				[WithdrawalMessageSender.Parameters.SentCurrentDocumentDataName] = false
			});

			var parameters = WithdrawalMessageSender.Parameters.New(map);
			var eDocsInstructions = documentInfo?.Descriptor?.EDocsInstructions;

			var res = withdrawalMessageSender.Send(parameters);

			if (res && eDocsInstructions.SaveCopyToEDocs)
			{
				documentInfo?.NotifyProgress(Res.GetString("F0905400-A6EA-40A8-B479-C5EF7CB4952C", "Attaching copy to eDocs"));
				var eDocDeliveryParameters = CreateEDocsDeliveryParameters(documentInfo?.Descriptor, eDocsInstructions);
				Document?.AddCopyToEDocs(eDocDeliveryParameters);
			}

			return res;
		}
	}
}
