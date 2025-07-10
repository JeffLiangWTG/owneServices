using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class ImportMessageSendingActionForm : ZChildForm
	{
		public ImportMessageSendingActionForm()
		{
		}

		public ImportMessageSendingActionForm(ImportMessageSendingActionCollection actions)
			: base(actions)
		{
			this.actions = actions;
			this.actions.IsCancelled = true;
			AddOrRemoveColumns();

			EntriesGrid.ContextMenu.MenuItems.Add("-");
			EntriesGrid.ContextMenu.MenuItems.Add("Tick 'Send' for All", new EventHandler(SelectAll_Clicked));
			EntriesGrid.ContextMenu.MenuItems.Add("Untick 'Send' for All", new EventHandler(UnSelectAll_Clicked));

			FormSplitContainer.Panel2MinSize = 240; // need to be set after the HeaderLineSplitContainer.Size is set or else the system throw an exception

			MessageDetailsTabControl.SelectedIndexChanged += new EventHandler(MessageDetailsTabControl_SelectedIndexChanged);

			foreach (ImportMessageSendingAction action in actions)
			{
				if (action.IsACECargoRelease)
				{
					action.US_SE_ReasonCodeInfo.ValueChanged += US_SE_ReasonCode_ValueChanged;
				}
				else
				{
					action.US_CertifyCargoReleaseInfo.ValueChanged += US_CertifyCargoRelease_ValueChanged;
				}
				action.US_SE_DISIndicatorInfo.ValueChanged += US_SE_DISIndicatorInfo_ValueChanged;
			}

			ACEEntrySummaryActionTabPage.CheckForNotifications = false;
			ACSEntrySummaryActionTabPage.CheckForNotifications = false;
			ACECargoReleaseTabPage.CheckForNotifications = false;
			ACSCargoReleaseTabPage.CheckForNotifications = false;
			MiscTabPage.CheckForNotifications = false;
		}

		void SelectAll_Clicked(object sender, EventArgs e)
		{
			actions.SelectAll();
		}

		void UnSelectAll_Clicked(object sender, EventArgs e)
		{
			actions.UnSelectAll();
		}

		void AddOrRemoveColumns()
		{
			if (!actions.IsStandAlonePriorNotice || actions.IsStandAlonePriorNotice && !actions.declaration.CanHavePGAFDA)
			{
				EntriesGrid.RemoveFromAvailableColumns(ImportMessageSendingAction.Schema.US_PNActionCode);
			}

			EntriesGrid.Refresh();
			ChangePanelVisibility();
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			censusWarningOverrideUserControl1.SetAdditionalGridLayoutKey("MessageSendingForm");
		}

		internal readonly ImportMessageSendingActionCollection actions;

		public override string FormCaption
		{
			get
			{
				var messageTypeString = actions.messageSendingMessageType.ToString();
				if (actions.messageSendingMessageType == ImportMessageSendingMessageType.Replacement)
				{
					messageTypeString = "Replace / Update";
				}
				return "Send '" + messageTypeString + "' Messages";
			}
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		internal void OKButton_Click(object sender, EventArgs e)
		{
			actions.IsCancelled = true;

			if (!actions.HasAtLeastOneToSendMessageFor)
			{
				Globals.Message.ShowInformation(YouHaveNotSelectedAnythingToSendMessagesFor);
			}
			else
			{
				actions.RunPreSaveValidation();
				if (actions.HasErrors())
				{
					using (ZErrorMessageBox msgBox = new ZErrorMessageBox(actions, "message", "send", "sent"))
					{
						ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
					}
				}
				else if (((INotificationProvider)actions).HasNotifications(CargoWise.EntityFramework.NotificationType.MessageError) &&
					!Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed)
				{
					Globals.Message.ShowInformation(Enterprise.Customs.Business.MessageSendingValidation.MessageErrorsExistWithNoSecurityRight);
				}
				else if (!actions.HasNotifications() || Globals.Message.Show(ThereIsANotification, "Send messages", MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
				{
					actions.IsCancelled = false;
					Close();
				}
			}
		}

		internal void CancelButton_Click(object sender, EventArgs e)
		{
			actions.IsCancelled = true;
			Close();
		}

		public const string YouHaveNotSelectedAnythingToSendMessagesFor = "There is nothing to send a message for";
		public const string ThereIsANotification = "There is a notification. Are you sure you wish to continue?";

		void EntriesGrid_AfterBind(object sender, EventArgs e)
		{
			EntriesGrid.ListManager.PositionChanged += new EventHandler(ListManager_PositionChanged);
			ListManager_PositionChanged(null, null);
		}

		void ListManager_PositionChanged(object sender, EventArgs e)
		{
			var listManager = EntriesGrid.ListManager;
			if (listManager != null && listManager.Count > 0)
			{
				var current = (ImportMessageSendingAction)listManager.GetCurrent();
				if (current != null)
				{
					bool isDiff = (currentAction != current);
					if (isDiff)
					{
						currentAction = current;
						ChangePanelVisibility();
					}
				}
				if (currentAction != null)
				{
					UpdateReferenceNumberCaption();
				}
			}
		}

		void US_SE_ReasonCode_ValueChanged(object sender, EventArgs e)
		{
			UpdateReferenceNumberCaption();
		}

		void US_SE_DISIndicatorInfo_ValueChanged(object sender, EventArgs e)
		{
			DISIDRefNoForUpdateDropEdit.Visible = currentAction.US_SE_DISIndicator;
			DISIDRefNoForDeleteDropEdit.Visible = currentAction.US_SE_DISIndicator;
			DISReferenceNoDropEdit.Visible = currentAction.US_SE_DISIndicator;
		}

		void UpdateReferenceNumberCaption()
		{
			if (currentAction != null)
			{
				SEReferenceIdentifierTextBox.CaptionResourceString = currentAction.GetReferenceNumberCaption();
				SEReferenceIdentifierTextBox.UpdateCaption();
			}
		}

		void ChangePanelVisibility()
		{
			bool hasCurrentAction = actions != null && currentAction != null;

			MiscTabPage.TabVisible = hasCurrentAction && ((actions.IsEntrySummaryQuery && !actions.declaration.IsACE) || actions.IsStandAlonePriorNotice || currentAction.IsBorderCargoRelease);
			ACEEntrySummaryActionTabPage.TabVisible = hasCurrentAction && currentAction.IsEntrySummary && actions.declaration.IsACE && !MiscTabPage.TabVisible;
			ACSEntrySummaryActionTabPage.TabVisible = hasCurrentAction && currentAction.IsEntrySummary && !actions.declaration.IsACE && !MiscTabPage.TabVisible;
			ACECargoReleaseTabPage.TabVisible = hasCurrentAction && currentAction.IsACECargoRelease && actions.declaration.IsACE && !MiscTabPage.TabVisible;
			ACSCargoReleaseTabPage.TabVisible = hasCurrentAction && currentAction.IsCargoRelease && !MiscTabPage.TabVisible;
			CWTabPage.TabVisible = hasCurrentAction && currentAction.IsEntrySummary && actions.declaration.IsACE && (actions.IsOriginal || actions.IsAmendment);
			PSCReasonCodesTabPage.TabVisible = hasCurrentAction && currentAction.IsPSCReasonAndExplanationEnabled;
			PSCExplanationTabPage.TabVisible = hasCurrentAction && currentAction.IsPSCReasonAndExplanationEnabled;

			if (hasCurrentAction)
			{
				if (ACEEntrySummaryActionTabPage.TabVisible)
				{
					ChangeACEEntrySummaryTabControlsVisibility();
				}
				else if (ACSEntrySummaryActionTabPage.TabVisible)
				{
					ChangeACSEntrySummaryTabControlsVisibility();
				}
				else if (ACECargoReleaseTabPage.TabVisible)
				{
					ChangeACECargoReleaseTabControlsVisibility();
				}
				else if (ACSCargoReleaseTabPage.TabVisible)
				{
					ChangeACSCargoReleaseTabControlsVisibility();
				}
				else if (MiscTabPage.TabVisible)
				{
					ChangeMiscTabControlsVisibility();
				}
				UpdateDefaultCWOsButtonVisibility();
			}
		}

		internal void ChangeACEEntrySummaryTabControlsVisibility()
		{
			var isPreApprovalEnabled = Customs.DataRegistry.Business.CustomsDataRegistry.Instance.EnableAccountingIntegration.Value.PreApprovalBillingJob;
			EntrySummaryPanel.Visible = (currentAction.IsEntrySummary || currentAction.IsCargoRelease) && (actions.IsOriginal || actions.IsAmendment || actions.IsWithdrawal);
			BillingReadyPanel.Visible = isPreApprovalEnabled && currentAction.IsEntrySummary && (actions.IsOriginal || actions.IsAmendment);
			ACEPaymentMadePanel.Visible = currentAction.IsPaidRelevant;
			ACESignOriginalAmendmentPanel.Visible = actions.declaration.IsACE && currentAction.IsEntrySummary && (actions.IsOriginal || actions.IsAmendment);
			CertifyCargoReleasePanel.Visible = (currentAction.IsEntrySummary || currentAction.IsCargoRelease) && (actions.IsOriginal || actions.IsAmendment);
			ACEEntrySummarySE13DataPanel.Visible = currentAction.IsSE13DataRelevant;
			CertifyTIBPanel.Visible = currentAction.Declaration.IsTemporaryImportationBond && currentAction.IsEntrySummary;
			var result = currentAction.Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.US_SecondarySPI == SecondarySpecProgIndicatorList.Codes.C);
			CertifyCBMAPanel.Visible = result;
			eBondStatementForENS.Visible = currentAction.IsEntrySummary && eBondLogger.ShouldAddAutoSendLog(actions.declaration) && (actions.IsOriginal || actions.IsAmendment);
			DISStatementPanel.Visible = currentAction.IsEntrySummary && currentAction.Declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.IsFishNotFromRussia);
		}
		void ChangeACECargoReleaseTabControlsVisibility()
		{
			var isACE_SE = actions.declaration.IsACE && currentAction.IsACECargoRelease;

			SEOriginalActionPanel.Visible = currentAction.IsSE13DataRelevant && actions.IsOriginal;
			SEUpdateActionPanel.Visible = currentAction.IsSE13DataRelevant && actions.IsAmendment;
			SEDeleteActionPanel.Visible = currentAction.IsSE13DataRelevant && actions.IsWithdrawal;
			eBondStatementForACECRL.Visible = isACE_SE && eBondLogger.ShouldAddAutoSendLog(actions.declaration) && (actions.IsOriginal || actions.IsAmendment);
		}
		void ChangeACSEntrySummaryTabControlsVisibility()
		{
			var isPreApprovalEnabled = Customs.DataRegistry.Business.CustomsDataRegistry.Instance.EnableAccountingIntegration.Value.PreApprovalBillingJob;

			ACSCertifyCargoReleasePanel.Visible = (currentAction.IsEntrySummary || currentAction.IsCargoRelease) && (actions.IsOriginal || actions.IsAmendment);
			ACSBillingReadyPanel.Visible = isPreApprovalEnabled && currentAction.IsEntrySummary && (actions.IsOriginal || actions.IsAmendment);
		}
		void ChangeACSCargoReleaseTabControlsVisibility()
		{
			ACSCargoCertifyCargoReleasePanel.Visible = (currentAction.IsEntrySummary || currentAction.IsCargoRelease) && (actions.IsOriginal || actions.IsAmendment);
		}
		void ChangeMiscTabControlsVisibility()
		{
			EntrySummaryQueryPanel.Visible = actions.IsEntrySummaryQuery && !actions.declaration.IsACE;
		}

		ImportMessageSendingAction currentAction;

		void MessageDetailsTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDefaultCWOsButtonVisibility();
		}

		void UpdateDefaultCWOsButtonVisibility()
		{
			DefaultCWOsButton.Visible = CWTabPage.TabVisible && MessageDetailsTabControl.SelectedTab == CWTabPage;
		}

		void DefaultCWOsButton_Click(object sender, EventArgs e)
		{
			if (currentAction != null && currentAction.IsEntrySummary)
			{
				var coll = currentAction.CensusWarningCodes;

				var existingCount = coll.Count;
				coll.DefaultCustomsCWOs();

				if (coll.Count == existingCount)
				{
					Globals.Message.ShowInformation("Finished. No new Census Warning exists.");
				}
			}
		}

		void US_CertifyCargoRelease_ValueChanged(object sender, EventArgs e)
		{
			if (actions != null && currentAction != null)
			{
				if (currentAction.IsEntrySummary && actions.declaration.IsACE)
				{
					ACEEntrySummarySE13DataPanel.Visible = currentAction.IsSE13DataRelevant;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (ImportMessageSendingAction action in actions)
				{
					if (action.IsACECargoRelease)
					{
						action.US_SE_ReasonCodeInfo.ValueChanged -= US_SE_ReasonCode_ValueChanged;
					}
					else
					{
						action.US_CertifyCargoReleaseInfo.ValueChanged += US_CertifyCargoRelease_ValueChanged;
					}
					action.US_SE_DISIndicatorInfo.ValueChanged -= US_SE_DISIndicatorInfo_ValueChanged;
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
