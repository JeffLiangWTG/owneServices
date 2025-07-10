using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.Forwarding.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI
{
	public class OutwardReportPlugInToConsol : CustomsCargoManifestPlugin
	{
		public OutwardReportPlugInToConsol(ForwardingConsol hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			this.Consol = hostBusinessEntity;
			ChangeTheVisibility();
		}
		protected readonly ForwardingConsol Consol;

		#region IZPlugIn Members

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.ExportManifest; }
		}

		protected override string NameCore
		{
			get { return "Outward Report"; }
		}

		protected override CustomsManifestStatus GetCustomsManifestStatus(IManifestProvider manifestProvider)
		{
			return ManifestStatus;
		}

		protected bool IsOCRActive => true;

		protected override Control GetNewUserControl()
		{
			if (fMessageUserControl == null)
			{
				if (IsOCRActive)
				{
					fMessageUserControl = new OCRManifestUserControl(ManifestStatus);
				}
				else
				{
					fMessageUserControl = new ConsolManifestUserControl(ManifestStatus);
				}
			}

			return fMessageUserControl;
		}
		ZManifestMessageHistoryUserControl fMessageUserControl;

		#endregion

		#region Implementation

		protected OutwardReportManifestStatus ManifestStatus
		{
			get
			{
				if (fManifestStatus == null)
				{
					fManifestStatus = new OutwardReportManifestStatus(Consol);
				}

				return fManifestStatus;
			}
		}
		OutwardReportManifestStatus fManifestStatus;

		protected override void ChangeTheVisibilityCore()
		{
			Enabled = Consol != null && Consol.Transports.ExportTransport != null && (Consol.IsAir || Consol.IsSea);
		}

		ForwardingConsolCustomsInformation ForwardingConsolCustomsInformation => forwardingConsolCustomsInformation ?? (forwardingConsolCustomsInformation = new ForwardingConsolCustomsInformation(Consol));
		ForwardingConsolCustomsInformation forwardingConsolCustomsInformation;

		#endregion

		#region Menu Setup and Handlers

		protected override MultilingualString MainMenuText
		{
			get { return (NoResString)"&Outward Report"; }
		}

		protected override MultilingualString DeclareManifestMenuText
		{
			get { return (NoResString)"Submit &Outward Report"; }
		}

		protected override MultilingualString WithdrawManifestMenuText
		{
			get { return (NoResString)"&Cancel Outward Report"; }
		}

		protected override void SetupTopLevelMenu()
		{
			AddMenuItem(DeclareManifestMenuText, SendOCRMenuItem_Click, 0);
			AddMenuItem(WithdrawManifestMenuText, CancelOCRMenuItem_Click, 1);
			mainMenuItem.MenuItems.Add(2, new ZMenuItem("-"));
			AddMenuItem("Submit &Outward Report with comment", SendOCRWithCommentMenuItem_Click, 3);
			AddMenuItem("Submit &Outward Report with attachments", SendOCRWithAttachmentsMenuItem_Click, 4);
		}

		#region TradeSingleWindow Menu Handling

		void AddMenuItem(string name, EventHandler clickMethod, int index)
		{
			MenuItem menuItem = new ZMenuItem(name, new EventHandler(clickMethod));
			menuItem.Name = menuItem.Text;
			mainMenuItem.MenuItems.Add(index, menuItem);
		}

		void SendOCRMenuItem_Click(object sender, EventArgs e)
		{
			var type = ManifestStatus.CreateOrReplaceTransaction;
			var additionalMessageInformation = OutwardReportHelper.GetAdditionalMessageInformation(Consol, type, false);
			var ocrSender = new SendOCRFromConsol(Consol, additionalMessageInformation, ManifestStatus, type);
			var canSendOCR = CanSendOCR(Consol, new SendsMessagesToCustomsGUI(), type);
			OutwardReportHelper.TryToSendOCR(ManifestStatus, ocrSender, additionalMessageInformation, type, canSendOCR, false);
		}

		void SendOCRWithCommentMenuItem_Click(object sender, EventArgs e)
		{
			var type = ManifestStatus.CreateOrReplaceTransaction;
			var additionalMessageInformation = OutwardReportHelper.GetAdditionalMessageInformation(Consol, type, false);
			var ocrSender = new SendOCRFromConsol(Consol, additionalMessageInformation, ManifestStatus, type);
			var canSendOCR = CanSendOCR(Consol, new SendsMessagesToCustomsGUI(), type);
			OutwardReportHelper.TryToSendOCR(ManifestStatus, ocrSender, additionalMessageInformation, type, canSendOCR, false, true);
		}

		void SendOCRWithAttachmentsMenuItem_Click(object sender, EventArgs e)
		{
			var type = ManifestStatus.CreateOrReplaceTransaction;
			var additionalMessageInformation = OutwardReportHelper.GetAdditionalMessageInformation(Consol, type, true);
			var ocrSender = new SendOCRFromConsol(Consol, additionalMessageInformation, ManifestStatus, type);
			var canSendOCR = CanSendOCR(Consol, new SendsMessagesToCustomsGUI(), type);
			OutwardReportHelper.TryToSendOCR(ManifestStatus, ocrSender, additionalMessageInformation, type, canSendOCR, true);
		}

		void CancelOCRMenuItem_Click(object sender, EventArgs e)
		{
			var additionalMessageInformation = OutwardReportHelper.GetAdditionalMessageInformation(Consol, TSWTransactionTypes.Cancel, false);
			var ocrSender = new SendOCRFromConsol(Consol, additionalMessageInformation, ManifestStatus, TSWTransactionTypes.Cancel);
			var canSendOCR = CanSendOCR(Consol, new SendsMessagesToCustomsGUI(), TSWTransactionTypes.Cancel);
			OutwardReportHelper.TryToSendOCR(ManifestStatus, ocrSender, additionalMessageInformation, TSWTransactionTypes.Cancel, canSendOCR, false);
		}
		#endregion

		#endregion

		#region TradeSingleWindow OCR Processing

		internal bool CanSendOCR(ForwardingConsol consol, SendsMessagesToCustomsGUI notifier, TSWTransactionTypes type)
		{
			if (!ForwardingConsolCustomsInformation.OutwardReportEntryNumber.IsEmpty && HasBeenSentAsLegacyMessage)
			{
				notifier.NotifyUserOfAnInvalidOperation(Res.GetString("8D8B4122-D6A1-4189-AE7F-F4271B7C4AC2", "This OCR was originally sent as a Legacy message, subsequent changes are no longer possible in TSW."));
				return false;
			}
			else if (consol.HasChanges)
			{
				notifier.NotifyUserOfAnInvalidOperation(Res.GetString("4AC8D554-5BB6-40AF-8592-9C0259240FB8", "You must save the current Consol details before generating a message"));
				return false;
			}
			else if (IsWaitingForResponse)
			{
				notifier.NotifyUserOfAnInvalidOperation(Res.GetString("28923FD8-1FE3-40D7-A39D-86274F8A2535", "There is a message pending. Please wait for the Customs response."));
				return false;
			}
			else if (IsCancelled)
			{
				notifier.NotifyUserOfAnInvalidOperation(Res.GetString("E0B730BE-9B5E-4B02-9AB7-4D80F2B86261", "This Outward Report has been canceled. No further OCR messages can be sent to Customs on this Consol."));
				return false;
			}
			else if (type == TSWTransactionTypes.Cancel)
			{
				if (ForwardingConsolCustomsInformation.OutwardReportEntryNumber.IsEmpty)
				{
					notifier.NotifyUserOfAnInvalidOperation(Res.GetString("FB244430-545E-48BF-ABE7-73F2BE672F0F", "There has been no Outward Report created as yet. You cannot cancel."));
					return false;
				}
			}

			return true;
		}

		protected bool HasBeenSentAsLegacyMessage
		{
			get
			{
				var result = false;
				foreach (EDIMessage msg in Consol.Messages)
				{
					if (msg.IsAnOriginal & msg.EM_ApplicationCode == EDIMessage.ApplicationCodes.NewZealandCustoms && msg.EM_MessageType == Enterprise.Customs.NZ.Business.Declaration.NZCMessage.MessageTypes.OutwardReport.MessageType)
					{
						result = msg.EM_MessageText.StartsWith("UNH", StringComparison.Ordinal);
						break;
					}
				}

				return result;
			}
		}

		protected bool IsWaitingForResponse
		{
			get
			{
				return ManifestStatus.E2_MessageStatus == OutwardReportStatusList.Descriptions.AwaitingResponse.ToString()
					|| ManifestStatus.E2_MessageStatus == OutwardReportStatusList.Descriptions.Acknowledgement.ToString();
			}
		}

		protected bool IsStatusNotSent
		{
			get { return ManifestStatus.E2_MessageStatus == OutwardReportStatusList.Descriptions.NotSent.ToString(); }
		}

		protected bool IsClearCore
		{
			get { return ManifestStatus.E2_MessageStatus == OutwardReportStatusList.Descriptions.Cleared.ToString(); }
		}

		protected bool IsCancelled
		{
			get { return ManifestStatus.E2_MessageStatus == OutwardReportStatusList.Descriptions.Cancelled.ToString(); }
		}

		#endregion
	}
}
