using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class SecureContainerReleaseSendMessageCommand : MessagingCommand
	{
		public override string Id => CommandIds.SendMessage;

		public override bool Invoke(MacroMap map, IDocumentInfo info)
		{
			if (info?.Document.Data.Value is SecureContainerRelease scr)
			{
				return SendSecureContainerRelease(map, info, scr);
			}

			return false;
		}

		bool SendSecureContainerRelease(MacroMap map, IDocumentInfo info, SecureContainerRelease scr)
		{
			if (scr.IsRevokeMode)
			{
				var sendWithdrawalCommand = new SendMessageWithdrawalCommand();
				return sendWithdrawalCommand.Invoke(map, info);
			}

			var sendMessageCommand = new SendMessageCommand();
			return sendMessageCommand.Invoke(map, info);
		}
	}
}
