using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.MarketingManager.GUI
{
	public partial class SendCampaignsControl : ZUserControl
	{
		protected internal GlbCompanyCampaignContactModule FilterItemModule;
		protected internal GlbCompanyCampaignContactFilterControl FilterStripControl;
		protected void SetIsDripMarketingModeOnFilterStripControl(bool isDripMarketingMode)
		{
			FilterStripControl.IsDripMarketingMode = isDripMarketingMode;
		}
		internal ZPanel FindRecipientContactsPanel;
		private ZGroupBox FindRecipientContactsGroupBox;
		internal ZGuidFindBox SourceCampaignPKGuidFindBox;
		private ZDropEdit ContactDataSourceDropEdit;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Description only shown to developers")]
		public SendCampaignsControl()
		{
			InitializeComponent();
			AddSendEmailMainPanelAndContactLabel();

			if (!DesignModeFinder.IsDesigning)
			{
				var description = Env.CurrentUser.IsDeveloperLogin ? "Show query analyser tool" : null;
				Hotkeys.RegisterHotKey(Keys.Control | Keys.Alt | Keys.Q, ProcessShowQueryKeyPress, description);
			}
		}

		public bool IsDripMarketingMode
		{
			get { return isDripMarketingMode; }
			set
			{
				isDripMarketingMode = value;

				if (isDripMarketingMode)
				{
					isDripMarketingMode = value;
					FindRecipientContactsPanel.Visible = false;
					SendButton.Visible = false;
					ScheduleSendButton.Visible = false;
					dropDownButtonsToolStrip.Visible = false;
				}
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is GlbCompanyCampaignDripMarketing)
			{
				return;
			}

			if (!FilterGridAdded)
			{
				var campaign = dataSource as GlbCompanyCampaign;
				if (campaign != null)
				{
					AddFilterGrid(campaign);

					if (IsDripMarketingMode)
					{
						FilterStripControl.HideDropDownItems();
						(FilterStripControl.FilterBusinessObject as GlbCompanyCampaignContactFilterBusinessObject).IsPreview = true;
					}

					if (!campaign.ImportOrgCode.IsEmpty || !campaign.ImportContactName.IsEmpty || !campaign.ImportInquiryPK.IsEmpty)
					{
						FilterStripControl.InitializeFilterStripsOnImport();
					}
				}
			}

			if (dataSource != null)
			{
				SourceCampaignPKGuidFindBox.DataBindings.RemoveBinding("IsVisibleForBinding");
			}
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null)
			{
				SourceCampaignPKGuidFindBox.DataBindings.Add(new KBinding("IsVisibleForBinding", dataSource, "IsUsingCampaignTrackingDataSource"));
			}
		}

		protected virtual void AddFilterGrid(GlbCompanyCampaign campaign)
		{
			FilterGridAdded = true;

			FilterItemModule = (GlbCompanyCampaignContactModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbCompanyCampaignContact);
			((IGlbCompanyCampaignContactModule)FilterItemModule).Campaign = campaign;
			FilterItemModule.FireOnPerformSearch += SendCampaignsControl_FireOnPerformSearch;

			FilterStripControl = (GlbCompanyCampaignContactFilterControl)FilterItemModule.EmbeddedControl;
			FilterStripControl.Location = ControlDpiScalingHelper.NewScaledPoint(0, 30);
			FilterStripControl.Size = ControlDpiScalingHelper.NewScaledSize(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(SendEmailMainPanel.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(SendEmailMainPanel.Height) - ControlDpiScalingHelper.UnscaleFromCurrentDpiY(ContactLabel.Height) - 30);
			FilterStripControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
			FilterStripControl.FireValidationRequest += FilterStripControl_FireValidationRequest;
			FilterStripControl.IsDripMarketingMode = IsDripMarketingMode;
			SendEmailMainPanel.Controls.Add(FilterStripControl);
		}

		public void SetFilteredGridFilter(StmModuleFilter filter)
		{
			if (FilterGridAdded)
			{
				FilterStripControl.SetFilterData(filter);
			}
		}

		protected void AddSendEmailMainPanelAndContactLabel()
		{
			SendEmailMainPanel = new ZPanel();
			ContactLabel = new ZPanel();
			FindRecipientContactsPanel = new ZPanel();

			SendEmailMainPanel.SuspendLayout();
			ContactLabel.SuspendLayout();
			FindRecipientContactsPanel.SuspendLayout();

			// 
			// SendEmailMainPanel
			// 
			SendEmailMainPanel.Controls.Add(ToolStrip);
			SendEmailMainPanel.Controls.Add(ContactLabel);
			SendEmailMainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			SendEmailMainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 43);
			SendEmailMainPanel.Name = "SendEmailMainPanel";
			SendEmailMainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 517);
			// 
			// ContactLabel
			// 
			ContactLabel.Controls.Add(SearchRecordsLabel);
			ContactLabel.Controls.Add(buttonsToolStrip);
			ContactLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
			ContactLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 478);
			ContactLabel.Name = "ContactLabel";
			ContactLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 39);
			ContactLabel.TabIndex = 1;
			SendEmailMainPanel.TabIndex = 8;
			// 
			// FindRecipientContactsPanel
			// 
			FindRecipientContactsPanel.Controls.Add(FindRecipientContactsGroupBox);
			FindRecipientContactsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			FindRecipientContactsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			FindRecipientContactsPanel.Name = "FindRecipientContactsPanel";
			FindRecipientContactsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 43);
			FindRecipientContactsPanel.TabIndex = 0;

			Controls.Add(SendEmailMainPanel);
			Controls.Add(FindRecipientContactsPanel);

			SendEmailMainPanel.ResumeLayout(false);
			SendEmailMainPanel.PerformLayout();
			ContactLabel.ResumeLayout(false);
			ContactLabel.PerformLayout();
			FindRecipientContactsPanel.ResumeLayout(false);
			FindRecipientContactsPanel.PerformLayout();
		}

		protected ZPanel SendEmailMainPanel;
		protected ZPanel ContactLabel;

		protected void SendCampaignsControl_FireOnPerformSearch(object sender, EventArgs e)
		{
			SearchRecordsLabel.Text = FilterItemModule.SearchRecordsFoundMessage;
		}

		protected void FilterStripControl_FireValidationRequest(object sender, EventArgs e)
		{
			foreach (FilterStrip strip in FilterStripControl.FilterBusinessObject.FilterStrips)
			{
				strip.Validation.ValidateAll();
			}
		}

		protected bool FilterGridAdded;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (CurrentDataItem != null)
			{
				this.dropDownButtonsToolStrip.Enabled = !CurrentDataItem.ReadOnly;
				CurrentDataItem.SaveAssociatedObject += CurrentDataItem_SaveAssociatedObject;
				SetupControlsForTargetListCampaign();
			}

			if (!DesignModeFinder.IsDesigning)
			{
				if (FilterItemModule != null)
				{
					foreach (var menuItem in FilterItemModule.FormActionMenu)
					{
						ToolStrip.Items.Add(MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(menuItem, FilterItemModule));
					}
				}
			}
		}

		internal void SetupControlsForTargetListCampaign()
		{
			dropDownButtonsToolStrip.Click -= SendToSelectedButton_Click;

			if (CurrentDataItem != null && (CurrentDataItem.IsTargetList || CurrentDataItem.IsMasterCampaign))
			{
				dropDownButtonsToolStrip.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("SendCampaignsControl|4a450655-7545-4f59-9c7d-c2f54153af86", "&Target Selected");
				dropDownButtonsToolStrip.Click += SendToSelectedButton_Click;
				dropDownButtonsToolStrip.DropDownItems[0].Visible = false;
				dropDownButtonsToolStrip.DropDownItems[1].Visible = false;
				dropDownButtonsToolStrip.ShowDropDownArrow = false;
			}
			else
			{
				dropDownButtonsToolStrip.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("SendCampaignsControl|aae82b2c-9e13-4d0e-a2a0-2f042e345f5a", "&Send to Selected");
				dropDownButtonsToolStrip.DropDownItems[0].Visible = true;
				dropDownButtonsToolStrip.DropDownItems[1].Visible = true;
				dropDownButtonsToolStrip.ShowDropDownArrow = true;
			}
		}

		void CurrentDataItem_SaveAssociatedObject(object sender, EventArgs e)
		{
			SaveFilterDefaultLayout();
		}

		void SaveFilterDefaultLayout()
		{
			if (FilterStripControl.FilterBusinessObject.ActiveModuleFilters.Any())
			{
				FilterStripControl.SaveDefaultLayout();
			}
		}

		#region BusinessEntity

		public new GlbCompanyCampaign CurrentDataItem
		{
			get { return (GlbCompanyCampaign)base.CurrentDataItem; }
		}

		#endregion

		#region Filtering

		void ProcessShowQueryKeyPress()
		{
			if (DeveloperLoginForm.TryAuthenticate())
			{
				ShowQueryForm();
			}
		}

		void ShowQueryForm()
		{
			QueryAnalyserTool tool = new QueryAnalyserTool();
			string sql = CurrentDataItem != null ? CurrentDataItem.FormattedFilterText : string.Empty;
			tool.Show(sql);
		}

		#endregion

		#region Send to Selected

		GlbCompanyCampaignSender campaignSender;

		internal void ScheduleSendButton_Click(object sender, EventArgs e)
		{
			SaveFilterDefaultLayout();

			if (!CurrentDataItem.NeedsTrackedLinks)
			{
				SendScheduled();
			}
			else
			{
				PromptNoTrackedLinksAndSend(FilterItemModule.DisplayGrid.SelectedElements.Length, SendScheduled);
			}
		}

		void SendScheduled()
		{
			campaignSender = InitialiseCampaignSender(true);

			campaignSender.MessageOnCampaignSending += GlbCompanyCampaignForm_MessageOnCampaignSending;
			campaignSender.SendScheduleEvent += CampaignSender_SendScheduleEvent;
			campaignSender.ContactsNotScheduledCampaign += CampaignSender_ContactsNotScheduledCampaign;

			campaignSender.ItemSent += BusinessEntity_ItemSent;
			campaignSender.CampaignSendBegin += BusinessEntity_CampaignSendBegin;
			campaignSender.CampaignSendEnd += BusinessEntity_CampaignSendEnd;

			try
			{
				bool isSent = campaignSender.CheckAndSendCampaigns();
				if (isSent)
				{
					SearchForMoreRecords(false);
				}
			}
			finally
			{
				campaignSender.MessageOnCampaignSending -= GlbCompanyCampaignForm_MessageOnCampaignSending;
				campaignSender.SendScheduleEvent -= CampaignSender_SendScheduleEvent;
				campaignSender.ContactsNotScheduledCampaign -= CampaignSender_ContactsNotScheduledCampaign;

				campaignSender.ItemSent -= BusinessEntity_ItemSent;
				campaignSender.CampaignSendBegin -= BusinessEntity_CampaignSendBegin;
				campaignSender.CampaignSendEnd -= BusinessEntity_CampaignSendEnd;
			}
		}

		private void CampaignSender_SendScheduleEvent(object sender, GlbCompanyCampaignSender.ContactsToSendToEventArgs e)
		{
			var newFactory = new BusinessObjectFactory();
			var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(CurrentDataItem);
			ZFormModaliser.Show(new CampaignItemScheduleForm(campaignInNewFactory, e.ContactsToSendTo), ParentForm);
		}

		private void CampaignSender_ContactsNotScheduledCampaign(object sender, GlbCompanyCampaignSender.ContactsNotScheduledCampaignEventArgs e)
		{
			var newFactory = new BusinessObjectFactory();
			var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(CurrentDataItem);
			if (campaignInNewFactory != null)
			{
				var form = new UpdateContactsForm(e.ContactsScheduled, e.ContactsNotSentTo, new ContactsWithNonDeliveryReportsUpdater(newFactory, campaignInNewFactory))
				{
					Owner = ParentForm
				};
				ZFormModaliser.ShowDialogAndDispose(form, ParentForm);
			}
		}

		#region Campaign Scheduling Messages

		internal void PromptNoTrackedLinksAndSend(int numCampaignsToSend, Action callback)
		{
			string message = string.Format(CultureInfo.InvariantCulture,
				GlbCompanyCampaignSender.NotificationConstants.SendingCampaignTrackedLinkValidationQuestion, numCampaignsToSend);

			string caption = Res.GetString("SendCampaignsControl|SendingValidationCaption", "Warning");

			DialogResult dialogOptions = Globals.Message.Show(message, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

			switch (dialogOptions)
			{
				case DialogResult.Yes:
					GlbCompanyCampaignForm parentForm = FindForm() as GlbCompanyCampaignForm;
					parentForm?.SelectEmailDesignerTab(callback);

					break;

				case DialogResult.No:
					callback.Invoke();
					break;
			}
		}

		#endregion

		internal void SendToSelectedButton_Click(object sender, EventArgs e)
		{
			SaveFilterDefaultLayout();

			if (!CurrentDataItem.NeedsTrackedLinks)
			{
				SendToSelected();
			}
			else
			{
				PromptNoTrackedLinksAndSend(FilterItemModule.DisplayGrid.SelectedElements.Length, SendToSelectedAutoConfirm);
			}
		}

		void SendToSelectedAutoConfirm()
		{
			SendToSelected(true);
		}

		void SendToSelected(bool autoConfirm = false)
		{
			campaignSender = InitialiseCampaignSender();

			campaignSender.MessageOnCampaignSending += GlbCompanyCampaignForm_MessageOnCampaignSending;
			campaignSender.ContactsNotSentCampaign += GlbCompanyCampaignForm_ContactsNotSentCampaign;

			if (!autoConfirm)
			{
				campaignSender.ShouldContinueWithSending += GlbCompanyCampaignForm_ShouldContinueWithSending;
			}
			else
			{
				campaignSender.ShouldContinueWithSending += OnCampaignSenderOnShouldContinueWithSending_AutoConfirm;
			}

			campaignSender.ItemSent += BusinessEntity_ItemSent;
			campaignSender.CampaignSendBegin += BusinessEntity_CampaignSendBegin;
			campaignSender.CampaignSendEnd += BusinessEntity_CampaignSendEnd;

			try
			{
				bool isSent = campaignSender.CheckAndSendCampaigns();
				if (isSent)
				{
					SearchForMoreRecords(false);
				}
			}
			finally
			{
				campaignSender.MessageOnCampaignSending -= GlbCompanyCampaignForm_MessageOnCampaignSending;
				campaignSender.ContactsNotSentCampaign -= GlbCompanyCampaignForm_ContactsNotSentCampaign;
				campaignSender.ShouldContinueWithSending -= GlbCompanyCampaignForm_ShouldContinueWithSending;

				campaignSender.CampaignSendBegin -= BusinessEntity_CampaignSendBegin;
				campaignSender.CampaignSendEnd -= BusinessEntity_CampaignSendEnd;
				campaignSender.ItemSent -= BusinessEntity_ItemSent;
			}
		}

		protected GlbCompanyCampaignSender InitialiseCampaignSender(bool isScheduled = false)
		{
			return new GlbCompanyCampaignSender(CurrentDataItem,
				FilterItemModule.DisplayGrid.SelectedElements.Cast<CampaignContact>().ToList(), isScheduled);
		}

		bool OnCampaignSenderOnShouldContinueWithSending_AutoConfirm(int numCampaignsToSend, GlbCompanyCampaignItem[] campaignToResend)
		{
			return true;
		}

		public void SearchForMoreRecords(bool forceSearch)
		{
			if ((FilterItemModule.MoreContactsAvailable) || forceSearch)
			{
				FilterStripControl.FirePerformSearch();
			}
			SendCampaignsControl_FireOnPerformSearch(null, EventArgs.Empty);
		}

		#region Campaign Sending Messages

		public void GlbCompanyCampaignForm_MessageOnCampaignSending(object sender, GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs e)
		{
			if (e.IsError)
			{
				Globals.Message.ShowError(e.Message, e.Summary);
			}
			else
			{
				Globals.Message.ShowInformation(e.Message, e.Summary);
			}
		}

		public void GlbCompanyCampaignForm_ContactsNotSentCampaign(int numContactsSent, ReadOnlyCollection<CampaignContact> contactsNotSentTo)
		{
			var newFactory = new BusinessObjectFactory();
			var campaignInNewFactory = newFactory.ImportFromAnotherFactorySafe(CurrentDataItem);
			if (campaignInNewFactory != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new UpdateContactsForm(numContactsSent, contactsNotSentTo, new ContactsWithNonDeliveryReportsUpdater(newFactory, campaignInNewFactory)));
			}
		}

		public bool GlbCompanyCampaignForm_ShouldContinueWithSending(int numCampaignsToSend, GlbCompanyCampaignItem[] campaignsToResend)
		{
			return DialogResult.Yes == Globals.Message.Show(ShouldContinueMessage(numCampaignsToSend), Res.GetString("6318a49f-a610-488a-9011-62224dcc50c1", "Send Campaigns"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		string ShouldContinueMessage(int numCampaignsToSend)
		{
			if (CurrentDataItem.IsTargetList)
			{
				return Res.GetString("cd5fe47c-6f06-4cbd-b8aa-818c0fb62f42", "{0} contact(s) will be added to the Target List. Would you like to continue?", numCampaignsToSend);
			}

			return CurrentDataItem.IsMasterCampaign
				? Res.GetString("098976A5-623A-47CC-B369-FEBDBCCFDBC1", "{0} contact(s) will be added to the Master List. Would you like to continue?", numCampaignsToSend)
				: Res.GetString("2e6055a2-cab3-4ce8-a40f-6dc72883e78f", "About to send campaigns to {0} contacts. Would you like to continue?", numCampaignsToSend);
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Baseline issue")]
		void BusinessEntity_CampaignSendBegin(object sender, EventArgs e)
		{
			SendProgressForm = new ProgressForm();
			SendProgressForm.Status = Res.GetString("be84000f-4a59-4b55-bf43-97b665a15388", "Sending campaigns to selected...");
			SendProgressForm.ShowCancelButton = true;
			SendProgressForm.ShowProgressBar = true;
			SendProgressForm.Cancelled += new EventHandler(SendProgressForm_Cancelled);
#if DEBUG
			LastSendProgressForm = SendProgressForm;
#endif
			SendProgressForm.ShowModalTo(FindForm());
			Application.DoEvents();
		}

#if DEBUG
		internal ProgressForm LastSendProgressForm;
#endif

		void SendProgressForm_Cancelled(object sender, EventArgs e)
		{
			campaignSender.CancelSendingContacts();
			SendProgressForm.Close();
			SendProgressForm.Dispose();
		}

		void BusinessEntity_CampaignSendEnd(object sender, EventArgs e)
		{
			SendProgressForm.Close();
			SendProgressForm.Dispose();

			((GlbCampaignContactCollection)FilterItemModule.GridCollection).Load(ZQuery.NoResultQuery);
		}

		void BusinessEntity_ItemSent(object sender, GlbCompanyCampaignSender.ItemSentEventArgs e)
		{
			if (SendProgressForm != null)
			{
				SendProgressForm.Status = Res.GetString("de5f2752-7f22-4d22-a25d-a3db061629a4", "Sending campaigns to contacts ({0} of {1}).", e.Sent, e.Total);
				SendProgressForm.PercentComplete = (int)((e.Sent / (decimal)e.Total) * 100m);
			}
		}

		ProgressForm SendProgressForm;

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				var overflow = buttonsToolStrip.OverflowButton.DropDown as ToolStripOverflow;
				if (overflow != null)
				{
					overflow.Dispose();
				}
			}

			if (FilterItemModule != null)
			{
				FilterItemModule.Dispose();
				FilterItemModule = null;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
