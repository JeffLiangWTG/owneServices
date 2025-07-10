using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Manifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.Manifest.GUI;

sealed class SendToCustomsMenuItem : ZMenuItem
{
	public SendToCustomsMenuItem(AsycudaManifestHeader header)
	{
		this.header = Argument.NotNull(header, nameof(header));
		CaptionResourceString = Res.GetData("E718130F-6AE0-450F-B68B-D38E9A7EBC80", "Send to Customs");
	}

	protected override void OnClick(EventArgs eventArgs)
	{
		CreateManifestMessage();
	}

	void CreateManifestMessage()
	{
		if (SaveDataFirst.Confirm(header, MainForm))
		{
			var sendingObjectParent = new DMOMessageSendingObjectParent(header);
			using var form = new MessageSendingForm(sendingObjectParent);
			if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
			{
				SendManifestMessage(sendingObjectParent);
			}
		}
	}

	void SendManifestMessage(DMOMessageSendingObjectParent sendingObjectParent)
	{
		if (sendingObjectParent.CreateAndSaveMessage())
		{
			Globals.Message.Show(Res.GetString("B6313EB8-5624-4FE6-8624-9ACFDD4131FA", "Message(s) sent successfully."));
		}
		else
		{
			Globals.Message.ShowWarning(Res.GetString("ECA8BF5B-7B47-4B73-AA25-F9F3FAE1FE8B", "Message(s) not generated."));
		}
	}

	ZForm MainForm => (ZForm)GetMainMenu()?.GetForm();
	readonly AsycudaManifestHeader header;
}
