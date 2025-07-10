using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Manifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		new AsycudaManifestHeader Header => base.Header as AsycudaManifestHeader;

		public override ResourceString MenuCaption => ResString.GetMultilingualString("106CC45A-C5AC-4B8C-8026-71B6ED2F5C56", "TW Manifest");

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			if (IsValidForMessage())
			{
				var sendToCustomsResString = ResString.GetMultilingualString("20BEF631-B3BB-4211-A757-F1EACA81089E", "Send to Customs");
				MenuBuilderHelper.AddMenuItem(mainForm, menuItems, sendToCustomsResString, Header, () => { CreateMessageForSendingToCustoms(false, sendToCustomsResString.EnglishText); }, false);

				var sendToCustomsByBagResString = ResString.GetMultilingualString("8FA9DEBD-2CD6-4211-A0A7-0AB71B0396D5", "Send to Customs (By Bag)");
				MenuBuilderHelper.AddMenuItem(mainForm, menuItems, sendToCustomsByBagResString, Header, () => { CreateMessageForSendingToCustoms(true, sendToCustomsByBagResString.EnglishText); }, false);
			}
			else
			{
				var menuItem = GetInvalidMessageMenuItem();
				menuItems.Add(menuItem);
			}

			return menuItems.ToArray();
		}

		void CreateMessageForSendingToCustoms(bool byBag, string menuCaption)
		{
			if (SaveDataFirst.Confirm(Header, mainForm))
			{
				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				if (orgProxy == null || !OrgHeaderHelper.CheckHasVatInTW(orgProxy))
				{
					Globals.Message.ShowError(Res.GetString("dd470eb4-9e11-45d2-9748-0680c18001d4", "A valid entry number is missing. The message cannot be sent. To generate entry number, a valid TW-VAT is required for Organization Proxy. To create a valid TW-VAT for Organization Proxy, visit Maintain > User Admin > Companies > Company Info. > Organization Proxy > Details > Config > Registration Numbers / Codes."));
					return;
				}

				var sendingObjectParent = new MessageSendingObjectParent(Header, menuCaption);
				var form = byBag ? new ByBagMessageSendingForm(sendingObjectParent) : new MessageSendingForm(sendingObjectParent);
				var result = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
				if (result)
				{
					var messageManager = byBag ? new N5101HByBagMultiMessageManager(sendingObjectParent) : new N5101HMultiMessageManager(sendingObjectParent);
					messageManager.SendMessages(new SendsMessagesToCustomsGUI());
				}
			}
		}
	}
}
