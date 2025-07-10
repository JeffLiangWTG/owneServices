using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.NL.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NctsHeader = Enterprise.Customs.NL.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.NL.NCTS.GUI;

public class MessagingMenuProvider : EU.NCTS.GUI.Phase5MessagingMenuProvider
{
	public MessagingMenuProvider(NctsHeader header)
		: base(header)
	{
	}

	public new NctsHeader Header => (NctsHeader)base.Header;

	protected override IEnumerable<ZMenuItem> CreateMenuItemsCore()
	{
		foreach (var menuItem in base.CreateMenuItemsCore())
		{
			yield return menuItem;
			if (menuItem == SendToCustomsMenuItem)
			{
				yield return PrintEmergencyDocumentMenuItem;
			}
		}
	}

	public override void RefreshMenu()
	{
		base.RefreshMenu();
		SetMenuItemVisibility(PrintEmergencyDocumentMenuItem, () => CanPrintEmergencyDocument);
	}

	protected override void SendToCustomsCore(ZMenuItem menuItem)
	{
		var sendingObjectParent = (MessageSendingActionParent)Header.Configuration.MessageSendingConfiguration.GetNewNctsHeaderMessageSendingObjectParent(Header);
		using (var form = GetMessageSendingForm(sendingObjectParent))
		{
			if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
			{
				if (sendingObjectParent.SendAndSaveMessages())
				{
					Globals.Message.Show(Res.GetString("52F0FEFA-D8E9-4630-B0CF-CF57ED38FE72", "The message has been sent."));
				}
				else
				{
					Globals.Message.ShowWarning(Res.GetString("D615FFE6-5DB0-402A-94D3-4959E4A69A2F", "The message has not been sent. This may be caused by a failure or because sending has not been implemented"));
				}
			}
		}
	}

	protected override bool IsResending(CusInBondMoveHeader movementHeader)
	{
		return Header.EffectiveMessageStatus == LogicalStatusList.Codes.Sent
			|| Header.EffectiveMessageStatus == LogicalStatusList.Codes.Acknowledged;
	}

	protected override bool CanSendToCustoms => !DepartureMovementHeader?.IsFallbackProcedure ?? base.CanSendToCustoms;

	protected ZMenuItem PrintEmergencyDocumentMenuItem => printEmergencyDocumentMenuItem ??= CreateNewPrintEmergencyDocumentMenuItem();
	ZMenuItem printEmergencyDocumentMenuItem;

	ZMenuItem CreateNewPrintEmergencyDocumentMenuItem()
		=> new ZMenuItem(ResString.GetMultilingualString("34EDC1AB-C70C-42A6-8550-3EA51B811E30", "Print Emergency Document"), PrintEmergencyDocumentClick);

	bool CanPrintEmergencyDocument => DepartureMovementHeader?.IsFallbackProcedure ?? false;

	NctsDepartureMovementHeader DepartureMovementHeader => departureMovementHeader ?? (Header.MovementHeader is NctsDepartureMovementHeader ? Header.MovementHeader : null);
	readonly NctsDepartureMovementHeader departureMovementHeader;

	void PrintEmergencyDocumentClick(object sender, EventArgs e)
	{
		PrintEmergencyDocument((ZMenuItem)sender);
	}

	void PrintEmergencyDocument(ZMenuItem menuItem)
	{
		DepartureMovementHeader.BM_EntryDate = ZDateTime.Now;
		if (SaveAndContinue(menuItem))
		{
			new EU.NCTS.Business.NctsTadEdocSaver(Header).RenderTadAndStoreInEdocs(Header);
			DepartureMovementHeader.BM_CustomsStatus = NCTSDepartureCustomsStatusList.Codes.EmergencyProcedure;
			DepartureMovementHeader.BM_Phase = EU.NCTS.Business.NctsMovementHeaderTransactionStatusList.Codes.Declaration;
			DepartureMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Accepted;
			DepartureMovementHeader.Factory.Save();
		}
		else
		{
			DepartureMovementHeader.BM_EntryDate = ZDateTime.Empty;
		}
	}

	protected MessageSendingForm GetMessageSendingForm(MessageSendingActionParent messageSendingActionParent) => new(messageSendingActionParent);
}
