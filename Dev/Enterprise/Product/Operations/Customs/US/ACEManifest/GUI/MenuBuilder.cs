using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.Customs.US.ACEManifest.Business.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs.Manifest;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.US.AIM.Messaging.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("615F7BDA-2FFF-4D6A-8E4C-C6FE57CCF9C3", "ACE Manifest");

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			if (Header.IsBillLevelManifestType)
			{
				var billLabel = Res.GetString("77A89ECF-6131-46B6-B137-7C4164D57996", "Bills");
				ASYCUDA.GUI.MenuBuilderHelper.AddBillLevelMenuItem(mainForm, menuItems, Header, new AIMMessageStatusProvider(), CreateAirAMSFreightReportMessage, billLabel);

				var requestTransferCaption = ResString.GetMultilingualString("ACEManifest.GUI.MenuBuilder|RequestTransferMessages", "Request Transfer");
				ASYCUDA.GUI.MenuBuilderHelper.AddMenuItem(mainForm, menuItems, requestTransferCaption, Header, () => SendTransferMessage(Header, false));
				var cancelTransferCaption = ResString.GetMultilingualString("ACEManifest.GUI.MenuBuilder|CancelTransferMessages", "Cancel Transfer");
				ASYCUDA.GUI.MenuBuilderHelper.AddMenuItem(mainForm, menuItems, cancelTransferCaption, Header, () => SendTransferMessage(Header, true));

				var arrivalCaption = ResString.GetMultilingualString("AsycudaMenu|SubmitArrivalMessages", "Arrival Messages");
				ASYCUDA.GUI.MenuBuilderHelper.AddMenuItem(mainForm, menuItems, arrivalCaption, Header, () => SendFlightSelectedItems(Header));

				menuItems.Add(new ZMenuItem("-"));
				var fsqMAWBCaption = ResString.GetMultilingualString("AsycudaMenu|SubmitFSQMAWBMessages", "Freight Status Query (MAWB)");
				ASYCUDA.GUI.MenuBuilderHelper.AddMenuItem(mainForm, menuItems, fsqMAWBCaption, Header, () => SendFSQMAWBSelectedItems(Header));
				var fsqHAWBCaption = ResString.GetMultilingualString("AsycudaMenu|SubmitFSQHAWBMessages", "Freight Status Query (HAWB)");
				ASYCUDA.GUI.MenuBuilderHelper.AddMenuItem(mainForm, menuItems, fsqHAWBCaption, Header, () => SendFSQHAWBSelectedItems(Header));

				if (Header.AMA_ManifestType == ACEManifestTypes.Codes.IAM && ManifestCustomsDataRegistry.Instance.EnableFDMMessage.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					menuItems.Add(new ZMenuItem("-"));
					var departureCaption = ResString.GetMultilingualString("EEC853A9-F0E6-481D-B709-EC9B30CCF781", "Send Flight Departure Message (FDM)");
					ASYCUDA.GUI.MenuBuilderHelper.AddMenuItem(mainForm, menuItems, departureCaption, Header, () => SendDepartureMessage(Header));
				}

				if (ManifestCustomsDataRegistry.Instance.EnableMAWBMessaging.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					var aceHeader = (AsycudaManifestHeader)Header;
					menuItems.Add(new ZMenuItem("-"));

					var isExpressCourier = aceHeader.IsExpressCourier;
					var mawbMessageCaption = isExpressCourier ? ResString.GetMultilingualString("53879704-67C8-4E56-8030-2E10F7622EF5", "Send MAWB Message (FXI)")
						: ResString.GetMultilingualString("3BE88FFE-7A20-48A5-8357-167B214FB60C", "Send MAWB Message (FRI)");
					ASYCUDA.GUI.MenuBuilderHelper.AddMenuItem(mainForm, menuItems, mawbMessageCaption, Header, () => SendManifestMessage(aceHeader, isExpressCourier ? AIMMessageSubTypes.FXI : AIMMessageSubTypes.FRI));
					var mawbAmendmentCaption = isExpressCourier ? ResString.GetMultilingualString("43BCF195-BB41-4C38-94C8-C3C5C44F443A", "Send MAWB Amendment Message (FXC)")
						: ResString.GetMultilingualString("82938AD2-4B3B-46A8-BC30-06C2B48F0D12", "Send MAWB Amendment Message (FRC)");
					ASYCUDA.GUI.MenuBuilderHelper.AddMenuItem(mainForm, menuItems, mawbAmendmentCaption, Header, () => SendManifestMessage(aceHeader, isExpressCourier ? AIMMessageSubTypes.FXC : AIMMessageSubTypes.FRC));
					var mawbCancellationCaption = isExpressCourier ? ResString.GetMultilingualString("D9D5EA49-FEE3-4167-8D56-BDBBE34FCB8D", "Send MAWB Cancellation Message (FXX)")
						: ResString.GetMultilingualString("939489F4-FC53-4AC6-9407-DBAF329948FD", "Send MAWB Cancellation Message (FRX)");
					ASYCUDA.GUI.MenuBuilderHelper.AddMenuItem(mainForm, menuItems, mawbCancellationCaption, Header, () => SendManifestMessage(aceHeader, isExpressCourier ? AIMMessageSubTypes.FXX : AIMMessageSubTypes.FRX));
				}
			}
			else
			{
				throw new NotSupportedException("ACE does not have any pack level manifests.");
			}

			return menuItems.ToArray();
		}

		#region Transfer Messages

		void SendTransferMessage(ASYCUDA.Business.AsycudaManifestHeader header, bool isCancel)
		{
			var itemsToShow = GetTransferHeaderSelectionItemList((AsycudaManifestHeader)header).Where(x => x.TransferHeader.TransferBills.Any(t => isCancel ? t.IsCancellable : t.IsSendable));

			var itemLabel = Res.GetString("5C9952AB-432E-495F-A0D5-399551AB81C0", "Transfer");
			var messageChooser = new TransferHeaderMessageChooser(header, itemsToShow, true, true);
			using (var dlg = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetNewAsycudaItemSelectionDialog(messageChooser, itemLabel, string.Empty))
			{
				var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(dlg);
				if (dialogResult == DialogResult.OK)
				{
					var selectedItems = dlg.GetSelectedItems().Cast<TransferHeaderSelectionItem>().ToArray();
					if (selectedItems.Length == 0)
					{
						Globals.Message.ShowError(Res.GetString("FBF04301-7CA4-4B7D-96EB-6B84A7AC9C44", "At least one {0} must be selected when sending a {0}-level manifest message.", itemLabel));
					}
					else
					{
						SendTransferMessages(header, selectedItems, isCancel);
					}
				}
			}
		}

		void SendTransferMessages(ASYCUDA.Business.AsycudaManifestHeader header, IReadOnlyList<TransferHeaderSelectionItem> selectedItems, bool isCancel)
		{
			var logBuilder = new ZStringBuilder();
			var sender = new TransferMessageSender();
			Action rollbackAction = null;

			foreach (var transferHeaderSelectionItem in selectedItems)
			{
				var transferHeader = transferHeaderSelectionItem.TransferHeader;
				var arrivalHeader = transferHeader.ArrivalHeader;
				var transferBills = transferHeader.TransferBills.Where(t => isCancel ? t.IsCancellable : t.IsSendable).ToArray();

				if (transferBills.Length == 0)
				{
					logBuilder.AppendLine(Res.GetString("CEF839DD-917F-4A7A-A4AE-E1230F23C274", "Unable to Send Transfer for Flight {0} because there are no Bills to be sent.", arrivalHeader.ATH_VoyageFlightNo));
				}
				else
				{
					foreach (var transferBill in transferBills)
					{
						rollbackAction += sender.SendMessage(transferBill, isCancel);
						logBuilder.AppendLine(Res.GetString("819C291F-8BFC-495D-A1D2-1EFEB9CFE9FE", "Transfer message for [{0}/{1}] queued for sending.", arrivalHeader.ATH_VoyageFlightNo, transferBill.ATB_BillNumber));
					}
				}
			}

			Save(header, logBuilder.ToString(), rollbackAction);
		}

		#endregion

		#region Arrival Messages

		void SendFlightSelectedItems(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var itemLabel = Res.GetString("16FB916A-5CB1-4B79-8EBE-F7C2F31E8293", "Arrival");
			var itemsToShow = GetTransferHeaderSelectionItemList((AsycudaManifestHeader)header);

			var messageChooser = new TransferHeaderMessageChooser(header, itemsToShow, true, true);
			using (var dlg = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetNewAsycudaItemSelectionDialog(messageChooser, itemLabel, string.Empty))
			{
				var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(dlg);
				if (dialogResult == DialogResult.OK)
				{
					var selectedItems = dlg.GetSelectedItems().Cast<TransferHeaderSelectionItem>().ToArray();
					if (selectedItems.Length == 0)
					{
						Globals.Message.ShowError(Res.GetString("0A639C64-8E9F-4195-88C4-0AF783E799C9", "At least one {0} must be selected when sending a {0}-level manifest message.", itemLabel));
					}
					else
					{
						SendArrivalMessages(header, selectedItems);
					}
				}
			}
		}

		IEnumerable<TransferHeaderSelectionItem> GetTransferHeaderSelectionItemList(AsycudaManifestHeader header)
		{
			var result = new List<TransferHeaderSelectionItem>();
			foreach (AsycudaArrivalHeader arrivalHeader in header.ArrivalHeaders)
			{
				foreach (var transferHeader in arrivalHeader.TransferHeaders)
				{
					if (transferHeader.TransferBills.Count > 0)
					{
						result.Add(new TransferHeaderSelectionItem(transferHeader));
					}
				}
			}
			return result;
		}

		void SendArrivalMessages(ASYCUDA.Business.AsycudaManifestHeader header, IReadOnlyList<TransferHeaderSelectionItem> messageParents)
		{
			var logBuilder = new ZStringBuilder();
			Action rollbackAction = null;
			foreach (var transferHeaderSelectionItem in messageParents)
			{
				if (transferHeaderSelectionItem.TransferHeader.TransferBills.Count == 0)
				{
					logBuilder.AppendLine(Res.GetString("28570AAC-F370-46DB-B26B-E991AF042462", "Unable to send message because there are no bills to be sent."));
				}
				else
				{
					foreach (var transferBill in transferHeaderSelectionItem.TransferHeader.TransferBills)
					{
						var bill = transferBill.Bill;
						if (bill != null)
						{
							var arrivalHeader = transferBill.TransferHeader.ArrivalHeader;
							var sender = new FSNMessageSender(transferBill, transferHeaderSelectionItem.StatusCode);
							rollbackAction += sender.SendMessage();
							logBuilder.AppendLine(Res.GetString("DB42D26D-8A3D-49C3-B3AA-861EB5715DC0", "Arrival message with [{0}/{1}] queued for sending.", arrivalHeader.ATH_VoyageFlightNo, transferBill.ATB_BillNumber));
						}
					}
				}
			}

			Save(header, logBuilder.ToString(), rollbackAction);
		}

		#endregion

		#region DepartureMessage

		void SendDepartureMessage(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var additionalMessageInformation = new AdditionalMessageInformation((AsycudaManifestHeader)header);
			if (ZFormModaliser.ShowDialogAndDispose(new DepartureSelectionDialog(additionalMessageInformation)) == DialogResult.OK)
			{
				var sender = new FDMMessageSender(additionalMessageInformation);
				sender.SendMessage();

				Save(header, Res.GetString("AE8A409F-79C5-4097-A18D-0811D5138176", "Departure message for [{0}] queued for sending.", header.AMA_Voyage));
			}
		}

		#endregion

		void CreateAirAMSFreightReportMessage(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType, IList<ASYCUDA.Business.IMessageParent> messageParents, ASYCUDA.Business.MessageChooser messageChooser)
		{
			var result = new AIMMessagingHelper().SendBillMessage(header, messageSubType, messageParents, messageChooser);
			Save(header, result.Message, result.RollbackAction);
		}

		void SendFSQMAWBSelectedItems(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var items = CreateMAWBSelectionItems(header);
			var itemLabel = Res.GetString("188472e4-b451-47a8-905d-0a7a7c976d77", "Bill");
			var messageChooser = (AIMMessageChooser)header.GetNewMessageChooser(items, AIMMessageSubTypes.FSQ, true);
			messageChooser.IsManifestMessage = true;

			var dialogResult = ShowItemSelectionDialog(header, messageChooser, itemLabel, AIMMessageSubTypes.FSQ);
			if (dialogResult.IsSuccess)
			{
				var selectedItems = dialogResult.SelectedItems.Cast<SplitBillSelectionItem>().ToArray();
				var sender = new FSQMessageSender(header);
				sender.SendMessage(selectedItems, messageChooser.RequestCode);

				var logBuilder = new ZStringBuilder();
				foreach (var item in selectedItems)
				{
					logBuilder.AppendLine(Res.GetString("49ec2b7c-050e-4d83-a4ac-e356fa4f4ec5", "Freight Status Query (MAWB) [{0}] queued for sending.\r\n", item.SelectionDescription(false)));
				}

				Save(header, logBuilder.ToString());
			}
		}

		IEnumerable<SplitBillSelectionItem> CreateMAWBSelectionItems(ASYCUDA.Business.AsycudaManifestHeader manifestHeader)
		{
			var list = new Dictionary<string, SplitBillSelectionItem>();

			var masterbill = (AsycudaBill)manifestHeader.MasterBill;
			var masterBillNumber = masterbill.ABL_BillNumber;
			list[masterBillNumber] = new SplitBillSelectionItem(masterbill, null);

			foreach (AsycudaArrivalHeader arrivalHeader in manifestHeader.ArrivalHeaders)
			{
				if (!arrivalHeader.ATH_Reference.IsEmpty)
				{
					list[$"{masterBillNumber}-{arrivalHeader.ATH_Reference}"] = new SplitBillSelectionItem(masterbill, arrivalHeader);
				}
			}

			return list.Values;
		}

		void SendFSQHAWBSelectedItems(ASYCUDA.Business.AsycudaManifestHeader header)
		{
			var items = CreateHAWBSelectionItems(header);
			var itemLabel = Res.GetString("188472e4-b451-47a8-905d-0a7a7c976d77", "Bill");
			var messageChooser = (AIMMessageChooser)header.GetNewMessageChooser(items, AIMMessageSubTypes.FSQ, true);

			var dialogResult = ShowItemSelectionDialog(header, messageChooser, itemLabel, AIMMessageSubTypes.FSQ);
			if (dialogResult.IsSuccess)
			{
				var selectedItems = dialogResult.SelectedItems.Cast<SplitBillSelectionItem>().ToArray();
				var sender = new FSQMessageSender(header);
				sender.SendMessage(selectedItems, messageChooser.RequestCode);

				var logBuilder = new ZStringBuilder();
				foreach (var item in selectedItems)
				{
					logBuilder.AppendLine(Res.GetString("c18c8740-87d7-4fd5-ac39-95a76a23a810", "Freight Status Query (HAWB) [{0}] queued for sending.\r\n", item.SelectionDescription(false)));
				}

				Save(header, logBuilder.ToString());
			}
		}

		IEnumerable<SplitBillSelectionItem> CreateHAWBSelectionItems(ASYCUDA.Business.AsycudaManifestHeader manifestHeader)
		{
			var list = new Dictionary<string, SplitBillSelectionItem>();

			foreach (AsycudaBill bill in manifestHeader.Bills)
			{
				list[bill.ABL_BillNumber] = new SplitBillSelectionItem(bill, null);
			}

			foreach (AsycudaArrivalHeader arrivalHeader in manifestHeader.ArrivalHeaders)
			{
				if (!arrivalHeader.ATH_Reference.IsEmpty)
				{
					foreach (var arrivalLine in arrivalHeader.ArrivalDetails)
					{
						if (arrivalLine.ATL_Reference == arrivalHeader.ATH_Reference && arrivalLine.Bill is AsycudaBill bill && !bill.IsChildMasterBill)
						{
							list[$"{bill.ABL_BillNumber}-{arrivalHeader.ATH_Reference}"] = new SplitBillSelectionItem(bill, arrivalHeader);
						}
					}
				}
			}

			return list.Values;
		}

		void SendManifestMessage(AsycudaManifestHeader header, string messageSubType)
		{
			ASYCUDA.Business.MessageChooser messageChooser = null;
			var canSend = true;

			var additionalMessageInformation = new AdditionalMessageInformation(header);
			using (var form = new SplitShipments(additionalMessageInformation, messageSubType))
			{
				canSend = ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK;
			}

			if (canSend)
			{
				if (messageSubType == AIMMessageSubTypes.FRC || messageSubType == AIMMessageSubTypes.FXC
					|| messageSubType == AIMMessageSubTypes.FRX || messageSubType == AIMMessageSubTypes.FXX)
				{
					var itemLabel = Res.GetString("643B72F0-72C2-4160-9969-D7F9766F456D", "Manifest");
					var items = new ASYCUDA.Business.ISelectionItem[] { header };
					messageChooser = header.GetNewMessageChooser(items, messageSubType, true);
					canSend = ShowItemSelectionDialog(header, messageChooser, itemLabel, messageSubType).IsSuccess;
				}
			}

			if (canSend)
			{
				var result = new AIMMessagingHelper().SendManifestMessage(header, messageSubType, messageChooser, additionalMessageInformation);
				Save(header, result.Message);
			}
		}

		(bool IsSuccess, ASYCUDA.Business.ISelectionItem[] SelectedItems) ShowItemSelectionDialog(ASYCUDA.Business.AsycudaManifestHeader header, ASYCUDA.Business.MessageChooser messageChooser, ZString itemLabel, string messageSubType)
		{
			var succeeded = false;
			ASYCUDA.Business.ISelectionItem[] selectedItems = null;

			using (var dlg = ApplicationGUIProvider.GetApplicationGuiProvider(header).GetNewAsycudaItemSelectionDialog(messageChooser, itemLabel, messageSubType))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(dlg) == DialogResult.OK)
				{
					selectedItems = dlg.GetSelectedItems();
					succeeded = selectedItems.Length > 0;
					if (!succeeded)
					{
						Globals.Message.ShowError(Res.GetString("42af15b4-f58a-4bc2-8eba-4a80bf068a76", "At least one {0} must be selected when sending a {0}-level manifest message.", itemLabel));
					}
				}
			}

			return (succeeded, selectedItems);
		}

		void Save(ASYCUDA.Business.AsycudaManifestHeader header, string message, Action rollbackAction = null)
		{
			var success = false;
			var caption = Res.GetString("C005F4B7-405F-4F88-94E4-895636792C56", "Send Air Import Message");

			try
			{
				header.Factory.Save();
				success = true;
			}
			catch (ZSaveException ex)
			{
				message = Res.GetString("85346473-2B69-4194-8BC0-FE660AFCCF55", "An Error occurred while saving the changes.{0}{1}", System.Environment.NewLine, ex.Message);
				ErrorReporter.ReportOnce("C01610A8-6171-4472-B7C5-9C48B40386EB", caption, ex);
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
