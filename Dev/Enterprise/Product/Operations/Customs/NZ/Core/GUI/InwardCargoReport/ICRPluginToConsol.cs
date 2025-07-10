using System;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.GUI.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.NZ.GUI
{
	[CodeAlive("ICR is WIP development - Currently disabled - ICR from Consols is still under development and to be determined - Gary / BKG.")]
	public class ICRPluginToConsol : CustomsCargoManifestPlugin
	{
		public ICRPluginToConsol(ForwardingConsol hostBusinessEntity)
			: base(hostBusinessEntity)
		{
			this.Consol = hostBusinessEntity;
			ChangeTheVisibility();
		}
		protected readonly ForwardingConsol Consol;

		#region IZPlugIn Members

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.ImportManifest; }
		}

		protected override string NameCore
		{
			get { return "Inward Cargo Report"; }
		}

		protected override CustomsManifestStatus GetCustomsManifestStatus(IManifestProvider manifestProvider)
		{
			return ManifestStatus;
		}

		protected override Control GetNewUserControl()
		{
			if (fMessageUserControl == null)
			{
				fMessageUserControl = new ConsolICRManifestUserControl(ManifestStatus);
			}
			return fMessageUserControl;
		}
		ZManifestMessageHistoryUserControl fMessageUserControl;

		#endregion

		#region Implementation

		protected ICRManifestStatus ManifestStatus
		{
			get
			{
				if (fManifestStatus == null)
				{
					fManifestStatus = new ICRManifestStatus(Consol);
				}
				return fManifestStatus;
			}
		}
		ICRManifestStatus fManifestStatus;

		protected override void ChangeTheVisibilityCore()
		{
			Enabled = Consol != null
				&& Consol.Transports.ImportTransport != null
				&& (Consol.IsAir || Consol.IsSea);
		}

		#endregion

		#region Menu Setup and Handlers

		protected override MultilingualString MainMenuText
		{
			get { return (NoResString)"&Inward Cargo Report"; }
		}

		protected override MultilingualString DeclareManifestMenuText
		{
			get { return (NoResString)"Submit &ICR"; }
		}

		protected override MultilingualString WithdrawManifestMenuText
		{
			get { return (NoResString)"&Cancel ICR"; }
		}

		protected override void SetupTopLevelMenu()
		{
			AddMenuItem("Submit ICR", SendICRMenuItem_Click, 0);
			AddMenuItem("Cancel ICR", CancelICRMenuItem_Click, 1);
		}

		#region TradeSingleWindow Menu Handling

		void AddMenuItem(string name, EventHandler clickMethod, int index)
		{
			MenuItem menuItem = new ZMenuItem(name, new EventHandler(clickMethod));
			menuItem.Name = menuItem.Text;
			mainMenuItem.MenuItems.Add(index, menuItem);
		}

		void SendICRMenuItem_Click(object sender, EventArgs e)
		{
			SendICRMenuItem(sender, e, TSWTransactionTypes.Original);
		}

		void CancelICRMenuItem_Click(object sender, EventArgs e)
		{
			SendICRMenuItem(sender, e, TSWTransactionTypes.Cancel);
		}

		void SendICRMenuItem(object sender, EventArgs e, TSWTransactionTypes type)
		{
			var sender1 = new SendsMessagesToCustomsGUI();
			if (CanSendICR(Consol, sender1))
			{
				var lastICRMessage = GetLastSentICRMessage();
				var eDocsForSelection = new IStorageDocsBaseCollection[] { Consol.DocManagerInfo.AllEDocs };
				var additionalMessageInformation = new AdditionalMessageInformation(lastICRMessage, eDocsForSelection, type, Consol.Factory, MessageTypeList.Codes.ICR);
				additionalMessageInformation.GatherPotentialSupportingDocuments();
				var icrSender = new SendICRFromConsol(Consol, additionalMessageInformation, ManifestStatus, type);
				if (icrSender.ErrorCount == 0)
				{
					if (!icrSender.CheckWarningsBeforeGeneratingMessage() || sender1.ContinueWithSend(icrSender.MessageWarnings))
					{
						using (var form = new TSWSendFormWithAttachments(additionalMessageInformation))
						{
							if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
							{
								var messageErrorsOnConsol = icrSender.GetBOValidationMessageErrors();
								if (string.IsNullOrEmpty(messageErrorsOnConsol) || sender1.ContinueWithAction(messageErrorsOnConsol, "Continue to Send"))
								{
									if (type == TSWTransactionTypes.Original)
									{
										var msgSentSuccesfully = icrSender.SendMessage();
										if (msgSentSuccesfully)
										{
											sender1.NotifyUserOfASuccessfulSend("ICR message queued for sending");
										}
									}
									else if (type == TSWTransactionTypes.Cancel)
									{
										var msgSentSuccesfully = icrSender.SendMessage();
										if (msgSentSuccesfully)
										{
											sender1.NotifyUserOfASuccessfulSend("ICR withdrawal queued for sending");
										}
									}
								}
							}
						}
					}
				}
				else
				{
					sender1.NotifyUserOfAnInvalidOperation((Res.GetString("5A3BE17D-70C2-418A-8727-0428A79E801B", "Unable to send ICR due to the following errors:")) + "\r\n\r\n" + icrSender.Errors);
				}
			}
		}

		#endregion

		#endregion

		#region TradeSingleWindow ICR Processing

		bool CanSendICR(ForwardingConsol consol, SendsMessagesToCustomsGUI sender)
		{
			if (consol.HasChanges)
			{
				sender.NotifyUserOfAnInvalidOperation(Res.GetString("4AC8D554-5BB6-40AF-8592-9C0259240FB8", "You must save the current Consol details before generating a message"));
				return false;
			}
			else if (IsWaitingForResponse)
			{
				sender.NotifyUserOfAnInvalidOperation(Res.GetString("28923FD8-1FE3-40D7-A39D-86274F8A2535", "There is a message pending. Please wait for the Customs response."));
				return false;
			}

			return true;
		}

		protected bool IsWaitingForResponse
		{
			get { return ManifestStatus.E2_MessageStatus == LowValueManifestStatusList.Descriptions.SentToCustoms; }
		}

		protected bool IsStatusNotSent
		{
			get { return ManifestStatus.E2_MessageStatus == LowValueManifestStatusList.Descriptions.NotSentToCustoms; }
		}

		protected bool IsClearCore
		{
			get { return ManifestStatus.E2_MessageStatus == LowValueManifestStatusList.Descriptions.ManifestAccepted; }
		}

		protected bool IsCancelled
		{
			get { return ManifestStatus.E2_MessageStatus == LowValueManifestStatusList.Descriptions.ManifestCancelled; }
		}

		public TSWMessage GetLastSentICRMessage()
		{
			//TODO: confirm requirement for eDocs
			TSWMessage result = null;
			var lastICRMessage = ManifestStatus.MessagesIncludingInterchangeRejections.GetLastMessage(EDIMessage.ApplicationCodes.NewZealandCustoms, TSWMessage.MessageTypes.InwardCargoReport.MessageType, EDIMessage.Direction.Transmit);
			if (lastICRMessage != null)
			{
				result = (TSWMessage)lastICRMessage;
			}

			return result;
		}

		#endregion
	}
}
