using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Windows.UI;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class GlbCompanyCampaignForm : ZTemplateForm
	{
		public GlbCompanyCampaignForm(GlbCompanyCampaign campaign)
			: this(campaign, true)
		{
		}

		public GlbCompanyCampaignForm(GlbCompanyCampaign campaign, bool shouldShowSendingTab)
			: base(campaign)
		{
			AddSendCampaignControl();
			AddTouchSetupControl();
			AddEmailContentControl();
			SetupChartControl();

			if (!shouldShowSendingTab)
			{
				SendingTabPage.TabVisible = false;
			}

			if (campaign.IsHRCampaign)
			{
				SalesRelationTabPage.TabVisible = false;
			}

			SetupContactCrossReferencesControl();

			WorkflowTabPage.Initialize(campaign);

			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.VoteCampaignPlugIn, GetTabIndexForSurveyAndVote);
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.SurveyCampaignPlugIn, GetTabIndexForSurveyAndVote);

			PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn); //call GetPlugIn() for eDocs to make it hook up its events before the DragDrop event below
			this.DragDrop += new DragEventHandler(GlbCompanyCampaignForm_DragDrop);
			CampaignCustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("C56AF50A-5C7E-4E78-A06F-7F405DCD932C", "To make use of this tab, please setup campaign custom fields in Workflow Manager.");

			TouchesTabPage.BindingOrFirstShown += TouchesTabPageOnBindingOrFirstShown;

			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("C573ADFD-2E12-4F7E-8E8D-8C2ADF12ADE1", "Translate Text Elements"), OnTranslateTextElements);
		}

		internal int GetTabIndexForSurveyAndVote() => TopLevelTabControl.TabPages.IndexOf(ContentDesignerTabPage) + 1;

		void OnTranslateTextElements(object sender, EventArgs e)
		{
			var source = new MultipleDataCaptionSource(new[]
			{
				DataSource.G0_CampaignNameInfo,
				DataSource.G0_CampaignCommentInfo
			}, DataSource.G0_CampaignNameMultilingual);

			ObjectFactory.Get<ICustomizableDataTranslationEditor>().EditTranslations(
				new CustomizableDataResourceStrings(source),
				null, DataSource);
		}

		protected override void SaveToRecentItems()
		{
			if (DataSource != null && !DataSource.G0_G0_Master.IsEmpty)
			{
				return;
			}

			base.SaveToRecentItems();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		protected void TouchesTabPageOnBindingOrFirstShown(object sender, EventArgs eventArgs)
		{
			if (TouchSummary == null)
			{
				SetupTouchSummary();

				TouchSummary.BeginLongRefresh += ControlOnBeginLongRefresh;
				TouchSummary.LongRefreshProgress += ControlOnLongRefreshProgress;
				TouchSummary.EndLongRefresh += ControlOnEndLongRefresh;
				TouchesTabPage.Controls.Add(TouchSummary);

				if (TouchSummary.ViewModel != null)
				{
					TouchSummary.ViewModel.NewCampaignSave += (sender2, e1) => { if (DataSource != null) { ((SuperActivityRelatedChildActivityPivotCollection)DataSource.RelatedChildActivityPivotCollection).RefreshFromDb(); } };
				}

				ControlOnBeginLongRefresh(this, new GlbCompanyCampaign.TransitionProgressEventArgs());
				try
				{
					Application.DoEvents();
					TouchSummary.SetDataContext(DataSource);
					Application.DoEvents();
				}
				finally
				{
					ControlOnEndLongRefresh(this, EventArgs.Empty);
				}
			}
			else
			{
				TouchSummary.Visible = true;
			}
		}

		#region Progress Form For Touch Summary

		void ControlOnBeginLongRefresh(object sender, GlbCompanyCampaign.TransitionProgressEventArgs eventArgs)
		{
			progressFormForTouchSummary?.HideForm();
			progressFormForTouchSummary?.Dispose();
			progressFormForTouchSummary = new SaveProgressMediator();
			progressFormForTouchSummary.ShowModalProgressForm(Bounds,
				string.IsNullOrEmpty(eventArgs.StatusMessage) ? ResString.GetMultilingualString("fd33c45c-1479-4610-b017-3907588cdcdc", "Processing...") : eventArgs.StatusMessage,
				eventArgs.PercentComplete);
		}

		void ControlOnLongRefreshProgress(object sender, GlbCompanyCampaign.TransitionProgressEventArgs eventArgs)
		{
			progressFormForTouchSummary?.SetStatusAndPercentCompleteShowingFormIfItIsInvisible(eventArgs.StatusMessage, eventArgs.PercentComplete);
		}

		void ControlOnEndLongRefresh(object sender, EventArgs eventArgs)
		{
			progressFormForTouchSummary?.HideForm();
		}

		SaveProgressMediator progressFormForTouchSummary;

		#endregion

		new GlbCompanyCampaign DataSource
		{
			get { return (GlbCompanyCampaign)base.DataSource; }
		}

		#region Form/Control Events

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			DataSource.G0_BroadcastVoteSurveyExamInfo.ValueChanged += G0_BroadcastVoteSurveyExamInfo_ValueChanged;
			SetupControlsByCampaignType();

			SetEmailSenderOptions();

			if (DataSource.IsInDatabase)
			{
				EmailContentControl.SetDataBinding(DataSource, "");
			}
		}

		protected override void Delete()
		{
			if (DisplayMode == ODisplayMode.Delete)
			{
				if (DataSource.IsMasterCampaign)
				{
					DataSource.AllTouches.DeleteAll();
				}
			}
			base.Delete();
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			DataSource.Saved -= DripCampaignSaved;

			if (DataSource.TemplateEditor != null && EmailContentControl != null && EmailContentControl.CurrentDataItem != null)
			{
				if (EmailContentControl.RecordChangesIfAny() == ContinueWithSave.No)
				{
					return ContinueWithSave.No;
				}
			}
			if (!DataSource.IsInDatabase && DataSource.G0_BroadcastVoteSurveyExam == CampaignTypeList.Codes.DripMarketing)
			{
				DataSource.Saved += DripCampaignSaved;
			}

			return base.ValidateAndSave();
		}

		void DripCampaignSaved(object sender, EventArgs eventArgs)
		{
			var answer = Globals.Message.Show(
				ResString.GetMultilingualString("7C44A5B1-877C-4566-87C6-EE0AE24A169A", "Do you want to create your first Touch now?"),
				ResString.GetMultilingualString("0487D19C-0BBC-43B3-B3D8-120F7AA5AEDF", "Master Campaign Saved"),
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (answer == DialogResult.Yes)
			{
				var viewModel = TouchSummary?.ViewModel
					?? new TouchSummaryViewModel();

				if (!viewModel.HasDataContext)
				{
					viewModel.SetDataSource(DataSource);
				}

				viewModel.AddHorizontal();
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			result = IFrameElementsExistWarning();
			if (result == ContinueWithSave.Yes)
			{
				result = CheckSendScheduleChanges();
			}

			return result;
		}

		ContinueWithSave IFrameElementsExistWarning()
		{
			if (DataSource.TemplateEditor.HasIFrameElements())
			{
				var dialogResult = Globals.Message.Show(
	ResString.GetMultilingualString("6E45303F-C9F2-4b14-BF62-0875189BF1CF", "There is at least one in-line frame element in the Email Content HTML. In-line frame elements are not currently supported by most email providers and in-line frame elements typically don’t work in email. Do you want to continue the Save?"),
	ResString.GetMultilingualString("D860F129-6046-4544-B24A-BB6048B9D65C", "Email Content has in-line frame elements"),
	MessageBoxButtons.OKCancel,
	DialogResult.Cancel);

				return dialogResult == DialogResult.OK ? ContinueWithSave.Yes : ContinueWithSave.No;
			}
			return ContinueWithSave.Yes;
		}

		ContinueWithSave CheckSendScheduleChanges()
		{
			var sendSettings = DataSource.SendSettings;

			if (DataSource.IsTouchCampaign && sendSettings != null)
			{
				if (SendSettingsHasChangesAndQueuedItems(sendSettings))
				{
					var dialogResult = Globals.Message.Show(
						ResString.GetMultilingualString("4FACED2E-C26A-40A9-912E-0571964132EE", "Send schedule settings were changed. Select OK to recalculate all queued contacts."),
						ResString.GetMultilingualString("6194927F-6C62-4436-A8B9-BC7E110CA560", "Send Schedule Changed"),
						MessageBoxButtons.OKCancel,
						DialogResult.Cancel);

					if (dialogResult == DialogResult.OK)
					{
						if (sendSettings.IsBatchSchedule)
						{
							sendSettings.SetBatchRecurrenceQueuedItems();
						}
						else
						{
							sendSettings.RecalculateAllScheduledTimes();
						}
					}
					else
					{
						return ContinueWithSave.No;
					}
				}

				if (sendSettings.ScheduleTask?.IsInDatabase ?? false)
				{
					sendSettings.ScheduleTask.ShouldPreventNextRunTimeBounceBack = true;
				}
			}
			else if (EmailSenderChangesAndCampaignHasQueuedItems)
			{
				var dialogResult = Globals.Message.Show(
					ResString.GetMultilingualString("69FED431-95C8-4382-A726-604DC6495D09", "Email sender was changed. Select OK to recalculate all queued contacts."),
					ResString.GetMultilingualString("839B4DF6-4056-4209-AA71-8D0FB78B5179", "Email Sender Changed"),
					MessageBoxButtons.OKCancel,
					DialogResult.Cancel);

				if (dialogResult == DialogResult.OK)
				{
					DataSource.RecalculateItemsStandAloneCampaign();
				}
				else
				{
					return ContinueWithSave.No;
				}
			}

			return ContinueWithSave.Yes;
		}

		bool SendSettingsHasChangesAndQueuedItems(GlbCompanyCampaignSendSettings sendSettings)
		{
			if (DataSource == null || sendSettings == null || !sendSettings.HasQueuedItems)
			{
				return false;
			}

			return sendSettings.HasChanges || EmailSenderHasChanges;
		}

		bool EmailSenderHasChanges => DataSource.G0_EmailSenderOptionInfo.HasChanges || SenderPoolHasChanges;

		bool SenderPoolHasChanges
		{
			get
			{
				var senderPool = DataSource.SenderPool;
				if (!DataSource.IsInDatabase || senderPool == null)
				{
					return false;
				}

				return senderPool.Count != SenderPoolCount || senderPool.Any(poolItem => poolItem.HasChanges && poolItem.IsInDatabase);
			}
		}

		bool EmailSenderChangesAndCampaignHasQueuedItems => EmailSenderHasChanges && DataSource.HasQueuedItems;

		public void FocusOnCampaignItem<T>(FocusOnTrackingTabTypes focusType, T value)
		{
			SelectTab(TrackingTabPage);
			GlbCompanyCampaignFormHelper.FocusOnCampaignItem(CampaignTrackingControl, focusType, value);
		}

		public void SelectTab(ZTabPage tabPage)
		{
			MainTabControl.SelectedTab = tabPage;
		}

		Action callbackWhenTrackedLinksAdded;
		public void SelectEmailDesignerTab(Action callback)
		{
			SelectTab(ContentDesignerTabPage);
			PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = true;
			MainTabControl.SelectedIndexChanged += OnMainTabControlSelectedIndexChanged;
			callbackWhenTrackedLinksAdded = callback;
		}

		void OnMainTabControlSelectedIndexChanged(object sender, EventArgs e)
		{
			PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = false;
			MainTabControl.SelectedIndexChanged -= OnMainTabControlSelectedIndexChanged;
			callbackWhenTrackedLinksAdded = null;
		}

		void SendEmailToSelectedButton_Click(object sender, EventArgs e)
		{
			DataSource.Factory.Save();
			callbackWhenTrackedLinksAdded?.Invoke();
			PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = false;
		}

		void G0_BroadcastVoteSurveyExamInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupControlsByCampaignType();
			SetEmailSenderOptions();
		}

		void SetupControlsByCampaignType()
		{
			if (DataSource == null)
			{
				return;
			}

			SetupTouchControls();
			SetupChartControl();
			SetupCaptions();

			if (DataSource.IsOpportunityCreationCampaign)
			{
				SetControlsOpportunityCreationCampaign();
			}
			else if (DataSource.IsPreApproachEmailCampaign)
			{
				SetControlsPreApproachEmailCampaign();
			}
			else if (DataSource.IsLinkTrackCampaign)
			{
				SetControlsLinkTrackCampaign();
			}
			else if (DataSource.IsTargetList)
			{
				SetControlsTargetListCampaign();
			}
			else if (DataSource.IsMasterCampaign)
			{
				SetControlsMasterCampaign();
			}
			else
			{
				SetControlsOthersCampaigns();
			}

			SetPluginsTabPosition();
			SendCampaignControl?.SetupControlsForTargetListCampaign();
			CampaignTrackingControl?.SetupControlsForTargetListCampaign();
		}

		void SetupCaptions()
		{
			var isMasterCampaign = DataSource.IsMasterCampaign;

			TrackingTabPage.CaptionResourceString = isMasterCampaign
					? Res.GetData("GlbCompanyCampaignForm|1DB475E2-2B1E-428D-B274-BE5E3594CD10", "Master List")
					: Res.GetData("GlbCompanyCampaignForm|76226f8e-0788-49c1-8638-71ae17b05c23", "Tracking");
			TrackingTabPage.UpdateCaption();

			SendingOptionsGroupBox.CaptionResourceString = isMasterCampaign
				? Res.GetData("EA289738-E62D-48DD-A780-EC846D0C1195", "Contact Filter Batch Options")
				: Res.GetData("c078e508-14f3-4fe5-9b81-6cb6ad648caf", "Batch Options");
			SendingOptionsGroupBox.UpdateCaption();
		}

		void SetupTouchControls()
		{
			TouchSetupControl.TouchSetupScheduleUserControl.groupBoxOuter.CaptionResourceString = DataSource.IsOpportunityCreationCampaign
				? Res.GetData("A93ECADD-E821-46C7-B5E5-380C709454EC", "Opportunity Creation Schedule")
				: Res.GetData("928a266e-6893-4357-9cf8-c499709a87ac", "Touch Send Schedule");
			TouchSetupControl.TouchSetupScheduleUserControl.groupBoxOuter.UpdateCaption();

			TouchesTabPage.TabVisible = DataSource.IsMasterCampaign;
			TouchSetupTabPage.TabVisible = DataSource.IsTouchCampaign;
		}

		void SetControlsOpportunityCreationCampaign()
		{
			CampaignDocumentGroupBox.Visible = false;
			TrackingSummaryGroupBox.Visible = false;
			TrackingStatusGroupBox.Visible = true;
			ContentDesignerTabPage.TabVisible = false;
			SendingTabPage.TabVisible = false;
			SendingOptionsGroupBox.Visible = false;

			if (OpportunityCreationTemplateControl == null)
			{
				AddOpportunityCreationTemplateControl();
			}

			OpportunityCreationTemplateControl.Visible = true;
		}

		void SetControlsPreApproachEmailCampaign()
		{
			SendingTabPage.TabVisible = false;
			ContentDesignerTabPage.TabVisible = true;
			SendingOptionsGroupBox.Visible = false;
		}

		void SetControlsLinkTrackCampaign()
		{
			CampaignDocumentGroupBox.Visible = false;
			TrackingSummaryGroupBox.Visible = false;
			TrackingStatusGroupBox.Visible = false;
			SendingOptionsGroupBox.Visible = false;

			if (linkTrackControl == null)
			{
				AddLinkTrackControl();
			}
			linkTrackControl.Visible = true;

			ContentDesignerTabPage.TabVisible = false;
			SendingTabPage.TabVisible = false;
			SendScheduleTabPage.TabVisible = false;
			TrackingTabPage.TabVisible = false;
		}

		void SetControlsTargetListCampaign()
		{
			CampaignDocumentGroupBoxControlsVisibilityOnTargetList(false);
			TrackingSummaryGroupBox.Visible = false;
			TrackingStatusGroupBox.Visible = false;
			CampaignDocumentGroupBox.Visible = false;
			SendingOptionsGroupBox.Visible = !DataSource.IsTouchCampaign;

			ContentDesignerTabPage.TabVisible = false;
			SendingTabPage.TabVisible = true;
			SendScheduleTabPage.TabVisible = false;
			TrackingTabPage.TabVisible = true;
		}

		void SetControlsMasterCampaign()
		{
			SetControlsTargetListCampaign();
			BudgetTabPage.TabVisible = false;
		}

		void SetControlsOthersCampaigns()
		{
			CampaignDocumentGroupBoxControlsVisibilityOnTargetList(true);
			TrackingSummaryGroupBox.Visible = true;
			TrackingStatusGroupBox.Visible = true;
			ContentDesignerTabPage.TabVisible = true;
			SendScheduleTabPage.TabVisible = true;
			TrackingTabPage.TabVisible = true;
			PermanentlySaveAgainstRecipientCheckBox.Visible = DataSource.IsBroadcastCampaign;

			var isTouchCampaign = DataSource.IsTouchCampaign;
			SendingTabPage.TabVisible = !isTouchCampaign;
			SendingOptionsGroupBox.Visible = !isTouchCampaign;
		}

		void SetPluginsTabPosition()
		{
			if (DataSource.IsVoteCampaign)
			{
				ReorderPluginTabPosition(PlugIns.GetPlugIn(ControllerIDs.VoteCampaignPlugIn).TabPage);
			}
			else if (DataSource.IsSurveyCampaign)
			{
				ReorderPluginTabPosition(PlugIns.GetPlugIn(ControllerIDs.SurveyCampaignPlugIn).TabPage);
			}

			void ReorderPluginTabPosition(ZTabPage tabPage)
			{
				if (TopLevelTabControl.TabPages.IndexOf(tabPage) != GetTabIndexForSurveyAndVote())
				{
					TopLevelTabControl.TabPages.Remove(tabPage);
					TopLevelTabControl.TabPages.Insert(tabPage, GetTabIndexForSurveyAndVote());
				}
			}
		}

		void CampaignDocumentGroupBoxControlsVisibilityOnTargetList(bool shouldShow)
		{
			var controlsToExclude = new[] { SendingOptionsGroupBox.Name, PermanentlySaveAgainstRecipientCheckBox.Name };

			foreach (Control control in CampaignDocumentGroupBox.Controls)
			{
				if (!controlsToExclude.Contains(control.Name))
				{
					control.Visible = shouldShow;
				}
			}

			if (!CampaignDocumentGroupBox.Visible)
			{
				CampaignDocumentGroupBox.Visible = true;
			}

			if (linkTrackControl != null)
			{
				linkTrackControl.Visible = false;
			}

			RepositionVisibleControlsOnTargetList(shouldShow);
		}

		void RepositionVisibleControlsOnTargetList(bool isHidingParent)
		{
			if (isHidingParent)
			{
				SendingOptionsGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(500, 220, true);
				ControlDpiScalingHelper.SetHeight(ref CampaignDocumentGroupBox, 175, true);
			}
			else
			{
				SendingOptionsGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(500, 40, true);
				ControlDpiScalingHelper.SetHeight(ref CampaignDocumentGroupBox, 80, true);
			}
		}

		void AddLinkTrackControl()
		{
			linkTrackControl = new LinkTrackCampaignClickStatUserControl();
			this.BindingSource.SetBindingMember(linkTrackControl, "StatModel");
			linkTrackControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
			linkTrackControl.Location = ControlDpiScalingHelper.NewScaledPoint(CampaignDocumentGroupBox.Location.X, CampaignDocumentGroupBox.Location.Y, false);
			linkTrackControl.Size = ControlDpiScalingHelper.NewScaledSize(CampaignDocumentGroupBox.Width, TrackingSummaryGroupBox.Location.Y + TrackingSummaryGroupBox.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(40), false);
			MainTabPage.Controls.Add(linkTrackControl);
		}
		LinkTrackCampaignClickStatUserControl linkTrackControl;

		internal void G0_EmailSenderOption_ValueChanged(object sender, EventArgs e)
		{
			SetEmailSenderOptions();
		}

		void SetEmailSenderOptions()
		{
			if (DataSource != null && !DataSource.IsTargetList && !DataSource.IsMasterCampaign)
			{
				if (DataSource.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.COR ||
					DataSource.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.EML)
				{
					var isSenderOptionEML = DataSource.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.EML;
					SenderEmailAddressTextBox.Visible = isSenderOptionEML;
					SenderEmailAddressDropEdit.Visible = !isSenderOptionEML;
					EmailSendersNameTextBox.Visible = true;
					StaffAssignmentDropEdit.Visible = false;
					SenderPoolButton.Visible = false;
					SenderUnlocoCodeFindBox.Visible = isSenderOptionEML;
					return;
				}

				if (DataSource.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.ORG)
				{
					EmailSendersNameTextBox.Visible = false;
					SenderEmailAddressTextBox.Visible = false;
					SenderEmailAddressDropEdit.Visible = false;
					StaffAssignmentDropEdit.Visible = true;
					SenderPoolButton.Visible = false;
					SenderUnlocoCodeFindBox.Visible = false;
					return;
				}

				if (DataSource.G0_EmailSenderOption == EmailSenderOptionCodeDescriptionList.Codes.SPS)
				{
					EmailSendersNameTextBox.Visible = false;
					SenderEmailAddressTextBox.Visible = false;
					SenderEmailAddressDropEdit.Visible = false;
					StaffAssignmentDropEdit.Visible = false;
					SenderPoolButton.Visible = true;
					SenderUnlocoCodeFindBox.Visible = false;
					SenderPoolCount = DataSource?.SenderPool?.Count ?? 0;
					return;
				}
			}

			if (DataSource != null && DataSource.IsMasterCampaign)
			{
				DataSource.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			}

			EmailSendersNameTextBox.Visible = false;
			SenderEmailAddressTextBox.Visible = false;
			StaffAssignmentDropEdit.Visible = false;
			SenderPoolButton.Visible = false;
			SenderUnlocoCodeFindBox.Visible = false;
		}

		void SenderPoolButton_Click(object sender, EventArgs e)
		{
			var glbCompanyCampaignSenderPool = DataSource?.SenderPool;
			if (glbCompanyCampaignSenderPool != null)
			{
				SenderPoolCount = glbCompanyCampaignSenderPool.Count;

				var senderPoolForm = new SelectStaffForSenderPoolForm(glbCompanyCampaignSenderPool);
				senderPoolForm.Closed += (o, args) => DataSource.Validation.ValidateSenderPool();
				ZFormModaliser.Show(senderPoolForm, this);
			}
		}

		int SenderPoolCount { get; set; }

		#endregion

		#region Bind

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (DataSource != null)
			{
				DataSource.CampaignTypeChanging -= BusinessEntity_CampaignTypeChanging;
				MediaTypeDropEdit.GetExtension<LabelCaptionRenderer>().DataBindings.RemoveBinding("Caption");
				MediaCategoryDropEdit.GetExtension<LabelCaptionRenderer>().DataBindings.RemoveBinding("Caption");
			}
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null)
			{
				DataSource.CampaignTypeChanging += BusinessEntity_CampaignTypeChanging;
				MediaTypeDropEdit.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new KBinding("Caption", DataSource, "MediaTypeLabel", true));
				MediaCategoryDropEdit.GetExtension<LabelCaptionRenderer>().DataBindings.Add(new KBinding("Caption", DataSource, "MediaCategoryLabel", true));
				DataSource.G0_EmailSenderOptionInfo.ValueChanged += G0_EmailSenderOption_ValueChanged;
			}
		}

		protected virtual void ButtonMasterCampaignOnClick(object sender, EventArgs eventArgs)
		{
			var master = DataSource.MasterCampaign;
			if (master != null)
			{
				ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaign).ShowEditForm(master);
			}
		}

		void ButtonDefibrillatorOnClick(object sender, EventArgs eventArgs)
		{
			DataSource?.DefibrillateTransitions();
		}

		void BusinessEntity_CampaignTypeChanging(object sender, CancelEventArgs e)
		{
			if (DataSource.GetActualQuestions().Length > 0)
			{
				string message = Res.GetString("4369f8fa-a87f-4a9d-8468-c3661073fa45", "You have entered some Questions for the campaign. This action will remove them. Are you sure?");
				e.Cancel = (Globals.Message.Show(message, Res.GetString("12ca2224-5695-4802-ae2d-16f65c43a8fe", "Changing Campaign Type"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No);
			}
		}

		#endregion

		#region Form Caption

		public override string FormCaption => DataSource.IsTouchCampaign ? Res.GetString("GlbCompanyCampaignForm|Touch", "Touch: {0}", DataSource.G0_CampaignID + " - " + DataSource.G0_CampaignNameMultilingual) : Res.GetString("GlbCompanyCampaignForm|Caption", "Campaign: {0}", DataSource.G0_CampaignID + " - " + DataSource.G0_CampaignNameMultilingual);

		#endregion

		#region Campaign Document Selection

		public void GlbCompanyCampaignForm_DragDrop(object sender, DragEventArgs e)
		{
			SynchroniseCampaignAttachments();
		}

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			SynchroniseCampaignAttachments();

			if (MainTabControl.SelectedTab == SendScheduleTabPage)
			{
				if (SendScheduleControl.FilterStripControl != null)
				{
					if (DataSource.CampaignItemSchedule.ItemsDeleted)
					{
						DataSource.CampaignItemSchedule.ItemsDeleted = false;
						SendScheduleControl.FilterStripControl.FirePerformSearch(false, true);
					}
				}
			}
		}

		void SynchroniseCampaignAttachments()
		{
			if (!this.IsDesignMode() && DataSource != null)
			{
				DataSource.CampaignAttachments.Synchronise();
			}
		}

		internal void DocumentFieldHelpButton_Click(object sender, EventArgs e)
		{
			if (mapTreePresenter == null)
			{
				mapTreePresenter = ObjectFactory.Get<IMapTreePresentationManager>();
				mapTreePresenter.ParentTypes = new Type[] { ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocCompanyCampaignItem>(), typeof(GlbCompanyCampaignItem) };
				mapTreePresenter.ModalParent = this;
				mapTreePresenter.ShowEditField = true;
				mapTreePresenter.DataFieldsOnly = true;
				mapTreePresenter.OpeningMacroTag = Core.Constants.DocumentEngine.EmailParsing.StartTag;
				mapTreePresenter.ClosingMacroTag = Core.Constants.DocumentEngine.EmailParsing.EndTag;
				mapTreePresenter.UseBrowseMode = true;
			}
			mapTreePresenter.ShowPresentationManagerForm();
		}

		IMapTreePresentationManager mapTreePresenter;

		#endregion

		#region ContactCrossReferences

		void SetupContactCrossReferencesControl()
		{
			CampaignTrackingControl.CampaignItemContactCrossReferencesControl.CrossReferenceSelected += campaignItemContactCrossReferencesControl_CrossReferenceSelected;
		}

		internal void campaignItemContactCrossReferencesControl_CrossReferenceSelected(object sender, CampaignItemContactCrossReferencesControl.CrossReferenceEventArgs e)
		{
			var gridCollection = CampaignTrackingControl.FilterStripControl.FilteredGrid.ListManager.List.Cast<GlbCompanyCampaignItem>();
			if (e.CrossReference.CampaignItem != null && !gridCollection.Any(item => item.PK == e.CrossReference.CampaignItem.PK))
			{
				if (PopupSalesRelationOnCampaignItemNotFoundInGrid(e))
				{
					var model = e.CrossReference.CampaignItem.SalesRelationModel;
					if (model != null)
					{
						var form = new SalesRelationPopupForm(model);
						ZFormModaliser.Show(form, this);
					}
				}
			}
			else if (e.CrossReference.CampaignItem != null && e.CrossReference.CampaignItem.RelatedChildActivityPivotCollection.Activities.Any())
			{
				CampaignTrackingControl.FilterStripControl.FilteredGrid.SelectSingleElement(e.CrossReference.CampaignItem);
			}
			else if (e.CrossReference.CampaignChildren != null && e.CrossReference.CampaignChildren.Count > 0)
			{
				MainTabControl.SelectedTab = SalesRelationTabPage;

				if (!salesRelationControl.ModelView.ShowCommunication && e.CrossReference.CampaignChildren.Any(child => child.ActivityType == RelatableActivityTypeList.Codes.Communication))
				{
					salesRelationControl.ModelView.ShowCommunication = true;
				}
				salesRelationControl.Tree.ClearSelection();
				foreach (var node in salesRelationControl.Tree.AllNodes.Where(node => e.CrossReference.CampaignChildren.Contains(((ZNode<IRelatableActivity>)node.Tag).BizObj)))
				{
					node.IsSelected = true;
				}
				salesRelationControl.Tree.Focus();
			}
			else if (e.CrossReference.CampaignItem != null)
			{
				CampaignTrackingControl.FilterStripControl.FilteredGrid.SelectSingleElement(e.CrossReference.CampaignItem);
			}
		}

		bool PopupSalesRelationOnCampaignItemNotFoundInGrid(CampaignItemContactCrossReferencesControl.CrossReferenceEventArgs e)
		{
			return DialogResult.Yes == Globals.Message.Show(Res.GetString("35a28e2f-e090-45da-a250-007432f11455", "Contact is not displayed in your filtered result.  Would you like to view {0}'s Sales Relation?", e.CrossReference.CampaignItem.ContactName),
					Res.GetString("9aca037b-b696-4a53-8468-0314bebd57af", "View Sales Relations"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		#endregion

		#region TouchSetupControl

		protected TouchSetupControl TouchSetupControl;

		void AddTouchSetupControl()
		{
			SetupTouchSetupControl();

			TouchSetupControl.SuspendLayout();
			TouchSetupTabPage.Controls.Add(TouchSetupControl);
			TouchSetupControl.AllowDrop = true;
			TouchSetupControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			BindingSource.SetBindingMember(TouchSetupControl, ".");
			TouchSetupControl.Location = ControlDpiScalingHelper.NewScaledPoint(5, 40);
			TouchSetupControl.Name = "touchSetupControl";
			TouchSetupControl.Size = ControlDpiScalingHelper.NewScaledSize(1335, 600);
			TouchSetupControl.TabIndex = 1;
			TouchSetupControl.ResumeLayout(true);
			TouchSetupControl.PerformLayout();

			if (DataSource != null && DataSource.IsTouchCampaign)
			{
				CampaignNameTextBox.CaptionResourceString = Res.GetData("7CD6FDFC-CDEB-46C5-8A5B-03F44CC856D9", "Touch Name");
				CampaignTypeDropEdit.CaptionResourceString = Res.GetData("BC8CDFC1-BB3A-4A64-B64C-03386ADD9B52", "Touch Type");
				CampaignDocumentGroupBox.CaptionResourceString = Res.GetData("91474C58-5E7B-4E49-9491-3BBBA5C8F3F7", "Touch Sending Options");
				TrackingTabPage.CaptionResourceString = Res.GetData("GlbCompanyCampaignForm|1DB475E2-2B1E-428D-B274-BE5E3594CD10", "Master List");

				if (DataSource.MasterCampaign != null)
				{
					if (DataSource.MasterCampaign.IsDripMarketingCampaign)
					{
						if (!DataSource.IsInDatabase)
						{
							DataSource.G0_BroadcastVoteSurveyExam = DripMarketingTouchTypeList.Codes.Broadcast;
						}

						CampaignTypeDropEdit.BindToList = "Lookups+DripMarketingTouchTypeList";
					}
					else
					{
						if (!DataSource.IsInDatabase)
						{
							DataSource.G0_BroadcastVoteSurveyExam = InsideSalesTouchTypeList.Codes.OpportunityCreation;
						}

						CampaignTypeDropEdit.BindToList = "Lookups+InsideSalesTouchTypeList";
					}

					CampaignTypeDropEdit.Refresh();
				}
			}
		}

		protected virtual void SetupTouchSetupControl()
		{
			TouchSetupControl = new TouchSetupControl();
		}

		#endregion

		#region IntegratedTouchSummary

		protected IntegratedTouchSummary TouchSummary;

		protected virtual void SetupTouchSummary()
		{
			TouchSummary = new IntegratedTouchSummary
			{
				Dock = DockStyle.Fill
			};
		}

		#endregion

		#region SendCampaignsControl

		protected internal SendCampaignsControl SendCampaignControl;

		void AddSendCampaignControl()
		{
			SetupSendCampaignControl();
			SendCampaignControl.SuspendLayout();
			SendingTabPage.Controls.Add(SendCampaignControl);
			// 
			// SendCampaignControl
			// 
			SendCampaignControl.AllowDrop = true;
			BindingSource.SetBindingMember(SendCampaignControl, ".");
			SendCampaignControl.Dock = System.Windows.Forms.DockStyle.Fill;
			SendCampaignControl.IsDripMarketingMode = false;
			SendCampaignControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			SendCampaignControl.Name = "SendCampaignControl";
			SendCampaignControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1346, 653);
			SendCampaignControl.TabIndex = 0;
			SendCampaignControl.ResumeLayout(true);
			SendCampaignControl.PerformLayout();
		}

		protected virtual void SetupSendCampaignControl()
		{
			SendCampaignControl = new SendCampaignsControl();
		}

		#endregion

		#region TrackingStatusControl

		void SetupChartControl()
		{
			AddTrackingStatusControl();
			AddOpportunityCreationControl();
			AddOpportunityCreationTemplateControl();

			if (DataSource != null && DataSource.IsOpportunityCreationCampaign)
			{
				TrackingStatusControl.Hide();
				TrackingStatusControl.Visible = false;
				OpportunityCreationControl.Show();
				OpportunityCreationControl.Visible = true;
				OpportunityCreationTemplateControl.Visible = true;
				CampaignDocumentGroupBox.Visible = false;
				TrackingSummaryGroupBox.Visible = false;
				TrackingStatusGroupBox.Visible = true;
				TrackingStatusGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(500, 475, true);
				DataSource.OpportunityCreationTemplate.HasChanges = false;
			}
			else
			{
				OpportunityCreationControl.Hide();
				OpportunityCreationControl.Visible = false;
				OpportunityCreationTemplateControl.Visible = false;
				TrackingStatusControl.Show();
				TrackingStatusControl.Visible = true;
				CampaignDocumentGroupBox.Visible = true;
				TrackingSummaryGroupBox.Visible = true;
				TrackingStatusGroupBox.Visible = true;
				TrackingStatusGroupBox.Location = ControlDpiScalingHelper.NewScaledPoint(500, 285, true);
			}
		}

		protected internal TrackingStatusChartUserControl TrackingStatusControl;

		void AddTrackingStatusControl()
		{
			if (TrackingStatusControl != null)
			{
				return;
			}

			SetupTrackingStatusControl();
			TrackingStatusGroupBox.Controls.Add(TrackingStatusControl);

			TrackingStatusControl.AllowDrop = true;
			TrackingStatusControl.BackColor = System.Drawing.Color.White;
			BindingSource.SetBindingMember(TrackingStatusControl, ".");
			TrackingStatusControl.Dock = DockStyle.Fill;
			TrackingStatusControl.Location = ControlDpiScalingHelper.NewScaledPoint(2, 15);
			TrackingStatusControl.Name = "TrackingStatusControl";
			TrackingStatusControl.Size = ControlDpiScalingHelper.NewScaledSize(837, 125);
			TrackingStatusControl.TabIndex = 0;
		}

		protected virtual void SetupTrackingStatusControl()
		{
			TrackingStatusControl = new TrackingStatusChartUserControl();
		}

		#endregion

		void AddOpportunityCreationTemplateControl()
		{
			if (OpportunityCreationTemplateControl != null)
			{
				return;
			}

			OpportunityCreationTemplateControl = new OpportunityCreationTemplateControl();
			BindingSource.SetBindingMember(OpportunityCreationTemplateControl, "OpportunityCreationTemplate");

			OpportunityCreationTemplateControl.Location = ControlDpiScalingHelper.NewScaledPoint(CampaignDocumentGroupBox.Location.X, CampaignDocumentGroupBox.Location.Y, false);
			MainTabPage.Controls.Add(OpportunityCreationTemplateControl);
		}

		OpportunityCreationTemplateControl OpportunityCreationTemplateControl;

		protected internal OpportunityCreationChartUserControl OpportunityCreationControl;

		void AddOpportunityCreationControl()
		{
			if (OpportunityCreationControl != null)
			{
				return;
			}

			SetupOpportunityCreationChartUserControl();
			TrackingStatusGroupBox.Controls.Add(OpportunityCreationControl);

			OpportunityCreationControl.AllowDrop = true;
			OpportunityCreationControl.BackColor = System.Drawing.Color.White;
			BindingSource.SetBindingMember(OpportunityCreationControl, ".");
			OpportunityCreationControl.Dock = DockStyle.Fill;
			OpportunityCreationControl.Location = ControlDpiScalingHelper.NewScaledPoint(2, 15);
			OpportunityCreationControl.Name = "OpportunityCreationControl";
			OpportunityCreationControl.Size = ControlDpiScalingHelper.NewScaledSize(370, 125);
			OpportunityCreationControl.TabIndex = 0;
		}

		protected virtual void SetupOpportunityCreationChartUserControl()
		{
			OpportunityCreationControl = new OpportunityCreationChartUserControl();
		}

		#region EmailContentControl

		protected ContentDesignerControl EmailContentControl;

		void AddEmailContentControl()
		{
			SetupEmailContentControl();
			EmailContentControl.SuspendLayout();
			ContentDesignerTabPage.Controls.Add(EmailContentControl);

			// 
			// EmailContentControl
			// 
			EmailContentControl.AllowDrop = true;
			BindingSource.SetBindingMember(EmailContentControl, ".");
			EmailContentControl.Dock = System.Windows.Forms.DockStyle.Fill;
			EmailContentControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			EmailContentControl.Name = "EmailContentControl";
			EmailContentControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1346, 653);
			EmailContentControl.TabIndex = 0;
			EmailContentControl.ResumeLayout(true);
			EmailContentControl.PerformLayout();
		}

		protected virtual void SetupEmailContentControl()
		{
			EmailContentControl = new ContentDesignerControl();
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (DataSource != null && !DataSource.IsDeleted)
				{
					DataSource.G0_BroadcastVoteSurveyExamInfo.ValueChanged -= G0_BroadcastVoteSurveyExamInfo_ValueChanged;
					if (DataSource.SendSettings?.ScheduleTask != null)
					{
						DataSource.SendSettings.ScheduleTask.ShouldPreventNextRunTimeBounceBack = false;
					}
				}
				if (components != null)
				{
					components.Dispose();
				}
				if (mapTreePresenter != null)
				{
					mapTreePresenter.Dispose();
				}
				if (linkTrackControl != null)
				{
					linkTrackControl.Dispose();
				}
				if (TrackingStatusControl != null)
				{
					TrackingStatusControl.Dispose();
				}
				if (progressFormForTouchSummary != null)
				{
					progressFormForTouchSummary.Dispose();
					progressFormForTouchSummary = null;
				}

				OpportunityCreationControl?.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
