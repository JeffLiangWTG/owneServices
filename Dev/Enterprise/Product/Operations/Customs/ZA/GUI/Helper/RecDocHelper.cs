using System;
using System.Windows.Forms;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.MessageManagers;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public static class RecDocHelper
	{
		public static void RefreshReqdocList(ZGrid entriesGrid, MenuItem reqdocRoot)
		{
			reqdocRoot.MenuItems.Clear();
			reqdocRoot.MenuItems.Add(new ZMenuItem("Latest Response", (object sender, EventArgs e) => RequestReqdocClick(entriesGrid)));

			if (entriesGrid.SelectedRowCount == 1)
			{
				var header = (CusEntryHeader)entriesGrid.SelectedElements[0];
				var list = header.ResendableResponseList;
				if (list.Count > 0)
				{
					reqdocRoot.MenuItems.Add(new ZMenuItem("-"));
				}
				foreach (ResendableResponseInformation responseInfo in list)
				{
					reqdocRoot.MenuItems.Add(new REQDOCMenuItem(responseInfo, (object sender, EventArgs e) => RequestSpecificReqdocClick(sender, entriesGrid)));
				}
			}
		}

		static void RequestReqdocClick(ZGrid entriesGrid)
		{
			if (entriesGrid.SelectedRowCount != 1)
			{
				Globals.Message.Show("Please select a single Entry to Request a REQDOC");
				return;
			}

			var header = (CusEntryHeader)entriesGrid.SelectedElements[0];
			var declaration = header.Declaration;

			if (declaration.HasChanges)
			{
				Globals.Message.ShowError("Please save the job before sending messages to Customs.");
				return;
			}

			new REQDOCMessageManager(new MessageSendingObjectForREQDOC(header), new Customs.GUI.UserNotification()).Send();
		}

		static void RequestSpecificReqdocClick(object sender, ZGrid entriesGrid)
		{
			var header = (CusEntryHeader)entriesGrid.SelectedElements[0];
			var menuItem = (REQDOCMenuItem)sender;
			var sendingObject = new MessageSendingObjectForREQDOC(header);
			sendingObject.DocumentMessageSource = menuItem.responseInfo.Version;
			new REQDOCMessageManager(sendingObject, new Customs.GUI.UserNotification()).Send();
		}

		class REQDOCMenuItem : ZMenuItem
		{
			public REQDOCMenuItem(ResendableResponseInformation responseInfo, EventHandler onClick) : base(responseInfo.DisplayName, onClick)
			{
				this.responseInfo = responseInfo;
			}
			public ResendableResponseInformation responseInfo;
		}
	}
}
