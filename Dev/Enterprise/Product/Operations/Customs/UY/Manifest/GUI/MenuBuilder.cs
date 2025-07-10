using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Customs.UY.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.UY.Manifest.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.UY.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("83EB2394-5E37-43E6-9B8F-1A1CB8E2FDB6", "UY Manifest");

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			if (IsValidForMessage())
			{
				var messageStatusProvider = Header?.MessageStatusProvider;
				if (messageStatusProvider != null)
				{
					if (messageStatusProvider.AllowOriginalMessage(Header))
					{
						var caption = ResString.GetMultilingualString("D613FBA2-9DE0-4B3C-8479-B5E85E05CE1A", "Send Manifest");
						MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header, MessageSubTypeCodes.Codes.Original), true);
					}
					if (messageStatusProvider.AllowCancellationMessage(Header))
					{
						var caption = ResString.GetMultilingualString("F5BADF29-CFC4-4DB1-9180-F8C026672602", "Cancel Manifest");
						MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header, MessageSubTypeCodes.Codes.Cancellation), true);
					}
					if (messageStatusProvider.AllowModificationMessage(Header))
					{
						var caption = ResString.GetMultilingualString("DFCA9DA6-9606-4BCB-8263-1D18A5F5B6E0", "Amend Manifest");
						MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, Header, () => CreateManifestLevelMessage(Header, MessageSubTypeCodes.Codes.Change), true);
					}
				}
			}
			else
			{
				var menuItem = GetInvalidMessageMenuItem();
				menuItems.Add(menuItem);
			}

			return menuItems.ToArray();
		}

		void CreateManifestLevelMessage(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType)
		{
			var company = Business.GlbCompanyWrapper.GetWrapper<Business.GlbCompanyWrapper>(GlbCompany.CurrentCompany)?.GetGlbExternalPassword<GlbCompanyCredential>(PasswordTypesList.Codes.UTB);
			if (company != null && !company.GP_UserID.IsEmpty && !company.CurrentDecryptedPassword.IsEmpty)
			{
				var uyHeader = (Business.AsycudaManifestHeader)header;
				var message = ZString.Empty;

				IEnumerable<ISelectionItem> items = null;
				var wrapperType = new ZString();
				Business.AsycudaBill[] billsToSend;

				if (messageSubType == MessageSubTypeCodes.Codes.Original)
				{
					items = Header.Bills.Where(x => x.ABL_BillStatus.IsEmpty || x.ABL_BillStatus == MessageStatusCodeList.Codes.Error || x.ABL_MessageStatus == MessageStatusCodeList.Codes.Error).Cast<ISelectionItem>();
					wrapperType = RecordTypes.Add;
				}
				else if (messageSubType == MessageSubTypeCodes.Codes.Cancellation)
				{
					items = Header.Bills.Where(x => x.ABL_BillStatus == MessageStatusCodeList.Codes.Accepted).Cast<ISelectionItem>();
					wrapperType = RecordTypes.Delete;
				}
				else if (messageSubType == MessageSubTypeCodes.Codes.Change)
				{
					items = Header.Bills.Where(x => x.ABL_BillStatus != MessageStatusCodeList.Codes.Cancel).Cast<ISelectionItem>();
					wrapperType = RecordTypes.Change;
				}

				if (messageSubType != MessageSubTypeCodes.Codes.Change)
				{
					var messageChooser = header.GetNewMessageChooser(items, MessageTypes.Codes.UYC, true);

					ISelectionItem[] selectedItems = null;
					using (var dlg = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetNewAsycudaItemSelectionDialog(messageChooser, ManifestCaption, MessageTypes.Codes.UYC))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(dlg) == DialogResult.OK)
						{
							selectedItems = dlg.GetSelectedItems();
						}
					}
					if (selectedItems != null)
					{
						billsToSend = selectedItems.Cast<Business.AsycudaBill>().ToArray();
						var messageSender = new DAEMessageSender(uyHeader, new DAEWrapper(uyHeader, billsToSend), wrapperType);
						message = messageSender.SendDAEMessage(messageSubType, billsToSend);
					}
				}
				else
				{
					billsToSend = items.Cast<Business.AsycudaBill>().ToArray();

					var messageSender = new DAEMessageSender(uyHeader, new DAEAmendWrapper(uyHeader, billsToSend), wrapperType);
					message = messageSender.SendDAEMessage(messageSubType, billsToSend);
				}

				if (message.Length > 0)
				{
					Globals.Message.Show(message);
				}
			}
			else
			{
				Globals.Message.ShowInformation(EmptyCredentials);
			}
		}

		static ZString ManifestCaption => Res.GetString("37F07F59-A631-4CD2-88E6-DCBE3143BB5A", "Manifest");
		static MultilingualString EmptyCredentials => ResString.GetMultilingualString("68673F41-FA85-459E-91AB-83B65E52BF2F",
			"Company credentials have not been entered. You can enter the credentials from Maintain -> User Admin -> Companies. Picking the company, in the Brokerage Tab.");
	}
}
