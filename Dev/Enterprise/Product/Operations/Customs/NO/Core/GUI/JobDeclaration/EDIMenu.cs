using System;
using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI;

public class EDIMenu : Customs.GUI.EDIMenu
{
	protected override bool DisplayGenerateEntriesMenuOption => true;

	public new JobDeclaration Declaration
	{
		get { return (JobDeclaration)base.Declaration; }
		set { base.Declaration = value; }
	}

	public override void RefreshMenu()
	{
		base.RefreshMenu();

		var isBuiltin = !(Declaration?.IsInterface ?? false);
		sendToCustomsMenuItem.Visible = isBuiltin;
	}

	protected override void SetupTopLevelMenu()
	{
		base.SetupTopLevelMenu();

		sendToCustomsMenuItem = new ZMenuItem(SendToCustoms, new EventHandler(SendMessageMenuItem_Click));
		sendToCustomsMenuItem.Visible = false;

		MenuItems.Add(sendToCustomsMenuItem);
	}

	protected void SendMessageMenuItem_Click(object sender, EventArgs e)
	{
		if (Declaration != null && PerformMergeIfNeeded() && PreSaveDeclaration(Declaration))
		{
			var messageSendingObjectParent = CreateNewMessageSendingObjectParent(Declaration);
			using var form = GetNewMessageSendingForm(messageSendingObjectParent);

			if ((ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK) && Declaration.CheckCredit())
			{
				var messageManager = new MessageManager(messageSendingObjectParent, new MessageNotificationCollector());
				messageManager.SendMessages();
			}
		}
	}

	bool PerformMergeIfNeeded()
	{
		var needMerge = !Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge;
		return !needMerge || PerformMerge();
	}

	protected Form GetNewMessageSendingForm(MessageSendingObjectParent sendingObjectParent)
	{
		return new MessageSendingForm(sendingObjectParent);
	}

	MessageSendingObjectParent CreateNewMessageSendingObjectParent(JobDeclaration declaration) => new MessageSendingObjectParent(declaration);

	protected ZMenuItem sendToCustomsMenuItem;
	static MultilingualString SendToCustoms => ResString.GetMultilingualString("7DBF3A6C-1E47-4A4E-ACFC-3C211F73B47A", "Send to Customs");
}
