using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.Customs.SG.Access.Business.UniversalDataTransfer;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.Access.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm)
			: base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("A631AEC8-7403-470F-B09B-D62171C084A8", "SG Access");

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			if (IsValidForMessage())
			{
				if (Header.IsPackedItemLevelManifestType)
				{
					var messageLabel = Res.GetString("DED42227-A87F-4D17-A054-DBA154D7BB06", "Manifest");
					ASYCUDA.GUI.MenuBuilderHelper.AddPackLevelMenuItem(mainForm, menuItems, Header, CreatePackLevelMessage, messageLabel);
				}
				else
				{
					var caption = ResString.GetMultilingualString("AsycudaMenu|SGNoSentManifest", "Send &Manifest");

					var noSentMenuItem = new ZMenuItem(caption);
					noSentMenuItem.Click += (s, e) =>
					{
						Globals.Message.ShowError(Res.GetString("E2605DE2-09F6-4832-812E-44504C996448",
							"No messages can be sent at this time. SG Access only supports Pack level manifests."));
					};

					menuItems.Add(noSentMenuItem);
				}
			}
			else
			{
				var menuItem = GetInvalidMessageMenuItem();
				menuItems.Add(menuItem);
			}

			return menuItems.ToArray();
		}

		void CreatePackLevelMessage(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType, IList<ASYCUDA.Business.IMessageParent> messageParents, ASYCUDA.Business.MessageChooser messageChooser)
		{
			var context = header.GetCurrentManifestContext();
			context.ManifestSendPacks = messageParents.OfType<AsycudaPackedItem>().Select(x => x.Pack);
			context.ManifestSendBills = context.ManifestSendPacks.Select(pack => pack.Bill).Distinct();

			Globals.Message.Show(new SGAsycudaManifestUniversalMessagingHelper(new NullLogger(), (MessageChooser)messageChooser).SendViaEHub(header, header.AMA_ManifestType, messageSubType, messageParents.Distinct().ToList()));
		}

		internal class NullLogger : INotifications
		{
			public void Add(INotification notification)
			{
				// Do nothing, because probably the person who write this didn't care about these logs.
			}
		}
	}
}
