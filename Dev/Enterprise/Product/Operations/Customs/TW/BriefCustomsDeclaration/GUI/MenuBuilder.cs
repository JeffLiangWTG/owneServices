using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		new AsycudaManifestHeader Header => base.Header as AsycudaManifestHeader;

		public override ResourceString MenuCaption => ResString.GetMultilingualString("56F0A456-719A-466A-9BB9-9015FB30D3CF", "Brief Customs Declaration");

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();
			if (IsValidForMessage())
			{
				var sendToCustomsMenuItem = new ZMenuItem(ResString.GetMultilingualString("A6ACB1BD-9AC1-4953-82DB-2306A42BCC89", "Send to Customs"));
				var subMenuItems = new List<ZMenuItem>();

				if (Header.IsImport)
				{
					var sendN5135MessageToCustomsResString = ResString.GetMultilingualString("22D5C0CE-4FDF-4A16-9E89-8EA8579E8631", "Send N5135 Message");
					MenuBuilderHelper.AddMenuItem(mainForm, subMenuItems, sendN5135MessageToCustomsResString, Header, SendN5135MessageToCustoms, false);
				}

				if (Header.IsExport)
				{
					var sendN5205MessageToCustomsResString = ResString.GetMultilingualString("526FD096-64F8-48A2-B2B1-E9538169D0EC", "Send N5205 Message");
					MenuBuilderHelper.AddMenuItem(mainForm, subMenuItems, sendN5205MessageToCustomsResString, Header, SendN5205MessageToCustoms, false);
				}
				sendToCustomsMenuItem.MenuItems.AddRange(subMenuItems.ToArray());
				menuItems.Add(sendToCustomsMenuItem);
			}
			else
			{
				var menuItem = GetInvalidMessageMenuItem();
				menuItems.Add(menuItem);
			}
			return menuItems.ToArray();
		}

		void SendN5135MessageToCustoms()
		{
			SendMessageToCustoms(MessageTypeList.Codes.IBC);
		}

		void SendN5205MessageToCustoms()
		{
			SendMessageToCustoms(MessageTypeList.Codes.EBC);
		}

		void SendMessageToCustoms(ZString messageType)
		{
			if (SaveDataFirst.Confirm(Header, mainForm))
			{
				var sendingObjectParent = new MessageSendingObjectParent(Header, messageType);
				var form = new SendToCustomsMessageSendingForm(sendingObjectParent);
				var result = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
				if (result)
				{
					var messageManager = new MultiMessageManager(sendingObjectParent);
					messageManager.SendMessages(new SendsMessagesToCustomsGUI());
				}
			}
		}
	}
}
