using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NotificationType = CargoWise.EntityFramework.NotificationType;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	public class ETerminalReleaseMessage : NonPersistentBusinessObject
	{
		public ETerminalReleaseMessage(JobVoyage voyage)
			: base(voyage?.Factory)
		{
			this.voyage = Argument.NotNull(voyage, nameof(voyage));
		}

		readonly JobVoyage voyage;

		public void SendMessage(IProgressNotifications notifications)
		{
			var messageConsolsToBeSent = MessageConsols.Cast<ETerminalReleaseMessageConsol>().Where(c => c.Send).ToArray();
			if (!messageConsolsToBeSent.Any())
			{
				notifications.Notify(NotificationType.Information, Res.GetString("0a01e885-c809-4ac5-b6b2-6b3449682ce4", "No consol is selected for sending."));
				return;
			}

			notifications.SetProgressMax(messageConsolsToBeSent.Length);

			foreach (var messageConsol in messageConsolsToBeSent)
			{
				notifications.NotifyFormat(NotificationType.Information, "{0:G}:", Hyperlink(messageConsol.Consol)); // non-translatable

				if (messageConsol.Status == Events.MessageSent.Description
					|| messageConsol.Status == Events.MessageWithdrawCancelRequest.Description)
				{
					notifications.Notify(NotificationType.Error, Res.GetString("d120b5de-3754-4dfe-9cf4-b931f8c71b38", @"eTerminal Release Manifest message for this Consol has been sent but no reply has been received from WTG eHub.
Ningbo EDI Center can only process original messages, and therefore if you need to send this Consol again, please reset its status to Original from within this Consol's > Electronic Messaging > Port Messaging menu."));
				}
				else if (MessageSender.SendMessage(messageConsol.Consol, MenuItem, notifications))
				{
					messageConsol.RefreshStatus();
					notifications.Notify(NotificationType.Information, Res.GetString("63f6dc9d-5243-4a1e-8968-7717f422b9f6", "eTerminal Release Manifest message has be sent successfully!"));
				}

				notifications.Notify(NotificationType.Information, string.Empty); // for the splitter empty line
				notifications.BumpProgress();
			}

			if (notifications.Notifications.HasNotifications(NotificationType.MessageError) || notifications.Notifications.HasNotifications(NotificationType.Error))
			{
				notifications.Notify(NotificationType.Information, Res.GetString("37a383f2-1866-4bf1-aab1-13066b0c4fac", @"There are Consols with eTerminal Release errors on this voyage.
View errors by opening each Consol with errors from the list, go to Consol > Electronic Messaging > Port Messaging > eTerminal Release.
Note you can send eTerminal Release Manifest from each Consol individually."));
			}
		}

		public ETerminalReleaseMessageConsolCollection MessageConsols
		{
			get
			{
				if (consols == null)
				{
					consols = new ETerminalReleaseMessageConsolCollection(voyage);
					consols.Load();
				}

				return consols;
			}
		}
		ETerminalReleaseMessageConsolCollection consols;

		public static LogHyperlink Hyperlink(ForwardingConsol consol)
		{
			return new LogControllerLink(consol.JK_UniqueConsignRef, ControllerIDs.JobConsol, consol.PK);
		}

		public ETerminalReleaseMessageSender MessageSender => messageSender ?? (messageSender = new ETerminalReleaseMessageSender());
		ETerminalReleaseMessageSender messageSender;

		IStmMenuItem MenuItem => menuItem ?? (menuItem = Factory.Load<VisualizerMenuItem>(ConsolSystemFormMenuItems.DocumentMenuETerminalReleaseManifestCNPK));
		IStmMenuItem menuItem;
	}
}
