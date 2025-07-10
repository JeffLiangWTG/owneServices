using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.CusTempStorage;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.CusTempStorage;

public class CusTempStorageFormMenu : ZMenuItem
{
	public CusTempStorageFormMenu()
	{
		Caption = ResString.GetMultilingualString("PLCusTempStorageFormMenu|CusTempStorageFormMenu", "&Messages");
	}

	public CusTempStorageJobHeader Header
	{
		get => header;
		set
		{
			header = value;
			RefreshMenuItems();
		}
	}
	CusTempStorageJobHeader header;

	void RefreshMenuItems()
	{
		if (Header != null)
		{
			MenuItems.Clear();

			AddMessageMenus(); //This is only temporary, will be changed in future WIs
		}
	}

	void AddMessageMenus()
	{
		var receiveIntoTransitShedMenu = new ZMenuItem(ResString.GetMultilingualString("D43BBC5D-3527-468A-B659-C38878F88F1E", "Receive into Transit Shed"));
		receiveIntoTransitShedMenu.Click += ReceiveIntoTransitShedMenu_Click;
		MenuItems.Add(receiveIntoTransitShedMenu);

		var transferToTransitShedMenu = new ZMenuItem(ResString.GetMultilingualString("80546277-89BB-434C-AFFB-B99DA9136825", "Transfer to Onward Transit Shed"));
		transferToTransitShedMenu.Click += TransferToTransitShedMenu_Click;
		MenuItems.Add(transferToTransitShedMenu);

		var reportDeconsolidationMenu = new ZMenuItem(ResString.GetMultilingualString("84B7C36E-937E-4F7E-8AFB-F507FA131E27", "Report De-consolidation"));
		reportDeconsolidationMenu.Click += ReportDeconsolidationMenu_Click;
		MenuItems.Add(reportDeconsolidationMenu);

		var sendDdtMenu = new ZMenuItem(ResString.GetMultilingualString("DCF11B02-DCA5-4071-B6E4-E1D631CA7FF7", "Send DDT"));
		sendDdtMenu.Click += SendDdtMenu_Click;
		MenuItems.Add(sendDdtMenu);
	}

	protected override void OnPopup(EventArgs e)
	{
		base.OnPopup(e);
		RefreshMenuItems();
	}

	void ReportDeconsolidationMenu_Click(object sender, EventArgs e)
	{
		SendMessage();
	}

	void TransferToTransitShedMenu_Click(object sender, EventArgs e)
	{
		SendMessage();
	}

	void ReceiveIntoTransitShedMenu_Click(object sender, EventArgs e)
	{
		SendMessage();
	}

	void SendDdtMenu_Click(object sender, EventArgs e)
	{
		SendMessage();
	}

	void SendMessage()
	{
		if (SaveAndContinue())
		{
			Globals.Message.Show(Res.GetString("9321f511-fbde-49cd-bb89-c1846be6df1b", "CW1 PL doesn't yet support building messages in Temporary Storage"));
		}
	}

	protected ZForm Form => (ZForm)GetMainMenu()?.GetForm();

	bool SaveAndContinue()
	{
		var canContinue = true;

		if (Header.HasChanges)
		{
			var messageBoxResult = Globals.Message.Show(
				Res.GetString("72F1912B-8AFC-48A2-B489-E41A1D64CBC3", "The Job has not yet been saved. Do you want to save and proceed?"),
				Res.GetString("45226CF0-BC4D-45A2-9EC6-05745C102FFE", "Save Job"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				DialogResult.Yes);

			canContinue = messageBoxResult == DialogResult.Yes && Form.FireSaveButton() == ContinueWithSave.Yes;
		}

		return canContinue && !Header.HasChanges;
	}
}
