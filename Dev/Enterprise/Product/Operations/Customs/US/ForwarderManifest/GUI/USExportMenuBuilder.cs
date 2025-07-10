using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public class USExportMenuBuilder : MenuBuilder
	{
		public USExportMenuBuilder(AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("926498EA-F12B-4ADB-962C-2E4DEA6A79F8", "US Export Manifest");

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			if (Header.IsBillLevelManifestType)
			{
				var uemCaption = ResString.GetMultilingualString("A7D05FA7-9953-4FCD-8187-EF0AE10469DE", "Send Export Manifest");
				ASYCUDA.GUI.MenuBuilderHelper.AddMenuItem(mainForm, menuItems, uemCaption, Header, () => SendExportManifestSelectedItems(Header));
			}
			else
			{
				throw new NotSupportedException("US export does not have any manifest message level manifests.");
			}

			return menuItems.ToArray();
		}

		void SendExportManifestSelectedItems(AsycudaManifestHeader header)
		{
			var items = CreateUEMSelectionItems(header);
			var itemLabel = Res.GetString("F37145A4-CAF5-432C-8AD9-0A39E2EA6F1E", "Bill");
			var messageChooser = (UEMMessageChooser)header.GetNewMessageChooser(items, MessageTypeList.Codes.ExportManifestSubmission, true);
			var dialogResult = ShowItemSelectionDialog(header, messageChooser, itemLabel, MessageTypeList.Codes.ExportManifestSubmission);
			if (dialogResult.IsSuccess)
			{
				var selectedItems = dialogResult.SelectedItems.Cast<SplitBillSelectionItem>().ToArray();
				var logBuilder = new ZStringBuilder();
				foreach (var item in selectedItems)
				{
					var sender = new UEMEDIMessageSender((USExportAsycudaBill)item.Bill, item.ActionType);
					sender.SendUEMMessage();
					item.Bill.ABL_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
					logBuilder.AppendLine(Res.GetString("ADA5B924-BD5C-4755-9452-D514622190E1", "Export Manifest [{0}] queued for sending.\r\n", item.SelectionDescription(false)));
				}
				Save(header, logBuilder.ToString());
			}
		}

		IEnumerable<SplitBillSelectionItem> CreateUEMSelectionItems(AsycudaManifestHeader manifestHeader)
		{
			var list = new Dictionary<string, SplitBillSelectionItem>();

			foreach (AsycudaBill bill in manifestHeader.Bills)
			{
				list[bill.ABL_BillNumber] = new SplitBillSelectionItem(bill);
			}
			return list.Values;
		}

		(bool IsSuccess, ISelectionItem[] SelectedItems) ShowItemSelectionDialog(AsycudaManifestHeader header, MessageChooser messageChooser, ZString itemLabel, string messageSubType)
		{
			bool succeeded = false;
			ISelectionItem[] selectedItems = null;

			using (var dlg = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetNewAsycudaItemSelectionDialog(messageChooser, itemLabel, messageSubType))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(dlg) == DialogResult.OK)
				{
					selectedItems = dlg.GetSelectedItems();
					succeeded = (selectedItems.Length > 0);
					if (!succeeded)
					{
						Globals.Message.ShowError(Res.GetString("375074D8-5DD4-4D3A-B598-839F5C3E3C7B", "At least one {0} must be selected when sending a {0}-level manifest message.", itemLabel));
					}
				}
			}

			return (succeeded, selectedItems);
		}

		void Save(AsycudaManifestHeader header, string message, Action rollbackAction = null)
		{
			bool success = false;
			var caption = Res.GetString("CA5E551F-1D1B-414D-BC9B-DCB31AEE2193", "Send Export Manifest Message");

			try
			{
				header.Factory.Save();
				success = true;
			}
			catch (ZSaveException ex)
			{
				message = Res.GetString("B88264B5-25C1-42B4-B97D-578C0BC39ACC", "An Error occurred while saving the changes.{0}{1}", System.Environment.NewLine, ex.Message);
				ErrorReporter.ReportOnce("31CB5831-9F8F-4F08-8916-C1A8E7AE2800", caption, ex);
			}
			finally
			{
				if (!success)
				{
					rollbackAction?.Invoke();
				}
			}

			header.Messages.Reload(false);

			if (success)
			{
				Globals.Message.ShowInformation(message, caption);
			}
			else
			{
				Globals.Message.ShowWarning(message, caption);
			}
		}
	}
}
