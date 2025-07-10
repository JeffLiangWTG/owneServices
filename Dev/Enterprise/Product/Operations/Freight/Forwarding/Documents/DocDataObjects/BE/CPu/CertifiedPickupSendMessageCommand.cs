using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	public sealed class CertifiedPickupSendMessageCommand : MessagingCommand
	{
		public override string Id => CommandIds.SendMessage;

		public override bool Invoke(MacroMap map, IDocumentInfo info)
		{
			if (info?.Document.Data.Value is CertifiedPickup certifiedPickup)
			{
				return SendCertifiedPickup(map, info, certifiedPickup);
			}

			return false;
		}

		bool SendCertifiedPickup(MacroMap map, IDocumentInfo info, CertifiedPickup certifiedPickup)
		{
			if (certifiedPickup.IsRevokeMode)
			{
				var sendWithdrawalCommand = new SendMessageWithdrawalCommand();
				return sendWithdrawalCommand.Invoke(map, info);
			}

			var sendMessageCommand = new SendMessageCommand();
			return sendMessageCommand.Invoke(map, info);
		}
	}
}
