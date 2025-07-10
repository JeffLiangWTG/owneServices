using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.GUI;
using Enterprise.Customs.NZ.Manifest.Business;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("AAF0FFF4-44FD-40A1-BA14-3FCE6E80FA58", "NZ Manifest");

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			if (IsValidForMessage())
			{
				if (Header.AMA_ManifestType == NZManifestTypes.Codes.ICR)
				{
					var messageLabel = Res.GetString("0461622F-A91F-4A48-8BA7-AA147F139F56", "Manifest");
					MenuBuilderHelper.AddSendManifestMenuItem(mainForm, menuItems, Header, CreateManifestLevelMessage, messageLabel);
				}
				else if (Header.AMA_ManifestType == NZManifestTypes.Codes.OCR)
				{
					AddOutwardReportMenuItem(menuItems);
				}
			}
			else
			{
				menuItems.Add(GetInvalidMessageMenuItem());
			}

			return menuItems.ToArray();
		}

		#region Outward Report
		void AddOutwardReportMenuItem(List<ZMenuItem> menuItems)
		{
			var caption = ResString.GetMultilingualString("42cf1a10-cdce-4f4f-9cab-c50c9b34e8b4", "Outward Report");
			var outwardReportMenuItem = new ZMenuItem(caption);
			menuItems.Add(outwardReportMenuItem);

			var currentMenuItems = outwardReportMenuItem.MenuItems;

			var sendCaption = ResString.GetMultilingualString("4bb5246b-b8c0-4416-8d50-d6318e8a92e2", "Submit Outward Report");
			var cancelCaption = ResString.GetMultilingualString("f21f9be1-9092-474c-afa4-cb8b89737714", "Cancel Outward Report");
			var sendWithCommentCaption = ResString.GetMultilingualString("3fc15cce-147c-4f1d-b9a8-19248015fff7", "Submit Outward Report with comment");
			var sendWithAttachmentsCaption = ResString.GetMultilingualString("81e21642-b6c7-4f2b-81ee-0052566f6c77", "Submit Outward Report with attachments");

			int index = 0;
			currentMenuItems.Add(index++, new ZMenuItem(sendCaption, SendOCRMenuItem_Click));
			if (Header.HasManifestBeenSubmittedToCustoms)
			{
				currentMenuItems.Add(index++, new ZMenuItem(cancelCaption, CancelOCRMenuItem_Click));
			}
			currentMenuItems.Add(index++, new ZMenuItem("-"));
			currentMenuItems.Add(index++, new ZMenuItem(sendWithCommentCaption, SendOCRWithCommentMenuItem_Click));
			currentMenuItems.Add(index++, new ZMenuItem(sendWithAttachmentsCaption, SendOCRWithAttachmentsMenuItem_Click));
		}

		void SendOCRWithAttachmentsMenuItem_Click(object sender, EventArgs e)
		{
			var type = TSWTransactionTypesForManifest;
			var additionalMessageInformation = OutwardReportHelper.GetAdditionalMessageInformation(Header, type, true);
			var ocrSender = new SendOCRFromManifestHeader((AsycudaManifestHeader)Header, additionalMessageInformation, type);
			var canSendOCR = CanSendOCR(new SendsMessagesToCustomsGUI());
			OutwardReportHelper.TryToSendOCR(null, ocrSender, additionalMessageInformation, type, canSendOCR, true);
		}

		void SendOCRWithCommentMenuItem_Click(object sender, EventArgs e)
		{
			var type = TSWTransactionTypesForManifest;
			var additionalMessageInformation = OutwardReportHelper.GetAdditionalMessageInformation(Header, type, false);
			var ocrSender = new SendOCRFromManifestHeader((AsycudaManifestHeader)Header, additionalMessageInformation, type);
			var canSendOCR = CanSendOCR(new SendsMessagesToCustomsGUI());
			OutwardReportHelper.TryToSendOCR(null, ocrSender, additionalMessageInformation, type, canSendOCR, false, true);
		}

		void SendOCRMenuItem_Click(object sender, EventArgs e)
		{
			var type = TSWTransactionTypesForManifest;
			var additionalMessageInformation = OutwardReportHelper.GetAdditionalMessageInformation(Header, type, false);
			var ocrSender = new SendOCRFromManifestHeader((AsycudaManifestHeader)Header, additionalMessageInformation, type);
			var canSendOCR = CanSendOCR(new SendsMessagesToCustomsGUI());
			OutwardReportHelper.TryToSendOCR(null, ocrSender, additionalMessageInformation, type, canSendOCR, false);
		}

		void CancelOCRMenuItem_Click(object sender, EventArgs e)
		{
			var type = TSWTransactionTypes.Cancel;
			var additionalMessageInformation = OutwardReportHelper.GetAdditionalMessageInformation(Header, type, false);
			var ocrSender = new SendOCRFromManifestHeader((AsycudaManifestHeader)Header, additionalMessageInformation, type);
			var canSendOCR = CanSendOCR(new SendsMessagesToCustomsGUI());
			OutwardReportHelper.TryToSendOCR(null, ocrSender, additionalMessageInformation, type, canSendOCR, false);
		}

		TSWTransactionTypes TSWTransactionTypesForManifest => Header.RegistrationNumber.IsEmpty ? TSWTransactionTypes.Original : TSWTransactionTypes.Replace;

		#endregion

		void CreateManifestLevelMessage(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType)
		{
			var tswTransactionType = TSWTransactionTypeFromAsycudaMessageSubType(messageSubType);
			var eDocsForSelection = new[] { header.DocManagerInfo.AllEDocs };
			var additionalMessageInformation = new AdditionalMessageInformation(null, eDocsForSelection, tswTransactionType, header.Factory, MessageTypeList.Codes.ICR);
			additionalMessageInformation.GatherPotentialSupportingDocuments();

			using (var form = GetSendFormForTransactionType(tswTransactionType, additionalMessageInformation))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					string result;
					var sender = new SendICRFromManifestHeader((AsycudaManifestHeader)header, additionalMessageInformation, tswTransactionType);
					if (sender.SendMessage())
					{
						result = Res.GetString("110B6D64-9126-4E2B-917D-9F603ED1F526", "{0} message queued for sending", sender.TransactionTypeName);
					}
					else
					{
						result = Res.GetString("83076BE0-D830-4FE8-9290-CC4A00567C4B", "Unable to send message");
					}
					Globals.Message.Show(result);
				}
			}
		}

		ZChildForm GetSendFormForTransactionType(TSWTransactionTypes transactionType, AdditionalMessageInformation additionalMessageInformation)
		{
			switch (transactionType)
			{
				case TSWTransactionTypes.Cancel:
					return new NZ.GUI.TradeSingleWindow.TSWCancelForm(additionalMessageInformation);
				case TSWTransactionTypes.Replace:
					return new NZ.GUI.TradeSingleWindow.TSWReplaceForm(additionalMessageInformation);
				default:
					return new NZ.GUI.TradeSingleWindow.TSWSendFormWithAttachments(additionalMessageInformation);
			}
		}

		TSWTransactionTypes TSWTransactionTypeFromAsycudaMessageSubType(string messageSubType)
		{
			var result = TSWTransactionTypes.None;
			switch (messageSubType)
			{
				case MessageSubTypeCodes.Codes.Original:
					result = TSWTransactionTypes.Original;
					break;
				case MessageSubTypeCodes.Codes.Cancellation:
					result = TSWTransactionTypes.Cancel;
					break;
				case MessageSubTypeCodes.Codes.Change:
					result = TSWTransactionTypes.Replace;
					break;
			}
			return result;
		}

		bool CanSendOCR(SendsMessagesToCustomsGUI notifier)
		{
			var result = true;
			if (IsWaitingForResponse)
			{
				notifier.NotifyUserOfAnInvalidOperation(Res.GetString("8B136E51-B50F-490B-80B2-550F8FAF01B5", "There is a message pending. Please wait for the Customs response."));
				result = false;
			}
			return result;
		}

		bool IsWaitingForResponse
		{
			get
			{
				return Header.AMA_MessageStatus == NZMessageStatusList.Codes.Sent
					|| Header.AMA_MessageStatus == NZMessageStatusList.Codes.Acknowledged;
			}
		}
	}
}
