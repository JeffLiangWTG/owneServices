using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	partial class ReconDeclarationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (reconDeclaration != null)
			{
				reconDeclaration.US_IsAggregateInfo.ValueChanged -= US_IsAggregateInfo_ValueChanged;
				reconDeclaration.JE_ApplicationCodeInfo.ValueChanged -= JE_ApplicationCodeInfo_ValueChanged;
				reconDeclaration.US_IssueCodeInfo.ValueChanged -= US_IssueCodeInfo_ValueChanged;
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			EntryNumberColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new EntryNumberColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ENSLabel = new Enterprise.ZArchitecture.ZLabel();
			this.InvoiceLinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryLinesReconInvoiceLineUserControl = new Enterprise.Customs.US.GUI.ReconInvoiceLineUserControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageTextTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusErrorsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messagesStatusErrorsUserControl = new Enterprise.Customs.US.GUI.MessagesStatusErrorsUserControl();
			this.StatusesErrorsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MessagesSplitter = new CargoWise.Windows.UI.KSplitter();
			this.HistoryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EntriesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntriesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OriginalEntriesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.EntriesSplitter = new CargoWise.Windows.UI.KSplitter();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReconChargesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReconChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OriginalChargesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OriginalChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OriginalEntryFeeSummaryTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.SummaryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ReconInterestCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OriginalFeesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OriginalTaxesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReconFeesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReconTaxesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReconDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OriginalDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TaxesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OriginalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.InterestLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FeesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DutyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReconLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RefundedFeesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EntryRefundedFeesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.StatementsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StatementsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ProtestFiledLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NAFTA303Label = new Enterprise.ZArchitecture.ZLabel();
			this.ProtestFiledCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NAFTA303CheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.HeaderDetailsReconHeaderUserControl = new Enterprise.Customs.US.GUI.ReconHeaderUserControl();
			this.StatusTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StatusTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.StatusDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.StatementStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AnticipatedLiquidatedDutyZCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AnticipatedLiquidationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.collectionDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RelatedStatementPKFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.StatementPaymentDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StatementPrintDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RelatedStatementStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StatusSummaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LiquidationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.liquidationDetailsUserControl = new Enterprise.Customs.US.GUI.LiquidationDetailsUserControl();
			this.liquidationsUserControl = new Enterprise.Customs.US.GUI.LiquidationsUserControl();
			this.StatusNotificationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.entrySummaryStatusNotificationsUserControl = new Enterprise.Customs.US.GUI.EntrySummaryStatusNotificationsUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceLinesTabPage.SuspendLayout();
			this.EntryLinesReconInvoiceLineUserControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MessagesTabControl.SuspendLayout();
			this.MessageDetailsTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.StatusErrorsTabPage.SuspendLayout();
			this.messagesStatusErrorsUserControl.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessagesGrid.SuspendLayout();
			this.EntriesTabPage.SuspendLayout();
			this.EntriesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OriginalEntriesGrid)).BeginInit();
			this.OriginalEntriesGrid.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.ReconChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReconChargesGrid)).BeginInit();
			this.ReconChargesGrid.SuspendLayout();
			this.OriginalChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OriginalChargesGrid)).BeginInit();
			this.OriginalChargesGrid.SuspendLayout();
			this.OriginalEntryFeeSummaryTabControl.SuspendLayout();
			this.SummaryTabPage.SuspendLayout();
			this.DetailsPanel.SuspendLayout();
			this.RefundedFeesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryRefundedFeesGrid)).BeginInit();
			this.EntryRefundedFeesGrid.SuspendLayout();
			this.StatementsTabPage.SuspendLayout();
			this.StatementsPanel.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			this.HeaderDetailsReconHeaderUserControl.SuspendLayout();
			this.StatusTabPage.SuspendLayout();
			this.StatusTabControl.SuspendLayout();
			this.StatusDetailsTabPage.SuspendLayout();
			this.StatementStatusGroupBox.SuspendLayout();
			this.AnticipatedLiquidationDateDateEdit.SuspendLayout();
			this.collectionDateDateEdit.SuspendLayout();
			this.RelatedStatementPKFindBox.SuspendLayout();
			this.StatementPaymentDateDateEdit.SuspendLayout();
			this.StatementPrintDateDateEdit.SuspendLayout();
			this.RelatedStatementStatusDropEdit.SuspendLayout();
			this.StatusSummaryGroupBox.SuspendLayout();
			this.LiquidationTabPage.SuspendLayout();
			this.liquidationDetailsUserControl.SuspendLayout();
			this.liquidationsUserControl.SuspendLayout();
			this.StatusNotificationsTabPage.SuspendLayout();
			this.entrySummaryStatusNotificationsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.EntriesTabPage);
			this.MainTabControl.Controls.Add(this.InvoiceLinesTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.StatusTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1021, 631, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.StatusTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InvoiceLinesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.EntriesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.HeaderDetailsReconHeaderUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 604, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 604, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 604, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1021, 631, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 30, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1021, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.ReconDeclaration);
			// 
			// ENSLabel
			// 
			this.ENSLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ENSLabel, "ENSStatusNotificationsReqFurtherActions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).ENSStatusNotificationsReqFurtherActions)));
			this.ENSLabel.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("886df771a-0ef7-47d1-b577-60115b1212da", "Ent. Sum Requires Actions");
			this.ENSLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ENSLabel.ForeColor = System.Drawing.Color.Red;
			this.ENSLabel.IsFontBold = true;
			this.ENSLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 71, true);
			this.ENSLabel.Name = "ENSLabel";
			this.ENSLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 13, true);
			this.ENSLabel.TabIndex = 6;
			this.ENSLabel.Text = "Ent Sum Requires Actions";
			// 
			// InvoiceLinesTabPage
			// 
			this.InvoiceLinesTabPage.Controls.Add(this.EntryLinesReconInvoiceLineUserControl);
			this.InvoiceLinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InvoiceLinesTabPage.Name = "InvoiceLinesTabPage";
			this.InvoiceLinesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InvoiceLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.InvoiceLinesTabPage.TabIndex = 6;
			this.InvoiceLinesTabPage.Text = "Entry Lines";
			// 
			// EntryLinesReconInvoiceLineUserControl
			// 
			this.EntryLinesReconInvoiceLineUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryLinesReconInvoiceLineUserControl, ".");
			this.EntryLinesReconInvoiceLineUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLinesReconInvoiceLineUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryLinesReconInvoiceLineUserControl.Name = "EntryLinesReconInvoiceLineUserControl";
			this.EntryLinesReconInvoiceLineUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 544, true);
			this.EntryLinesReconInvoiceLineUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.Controls.Add(this.MessagesTabControl);
			this.MessagesTabPage.Controls.Add(this.MessagesSplitter);
			this.MessagesTabPage.Controls.Add(this.HistoryGroupBox);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 604, true);
			this.MessagesTabPage.TabIndex = 7;
			this.MessagesTabPage.Text = "Messages";
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessagesTabControl.Controls.Add(this.MessageDetailsTabPage);
			this.MessagesTabControl.Controls.Add(this.MessageTextTabPage);
			this.MessagesTabControl.Controls.Add(this.StatusErrorsTabPage);
			this.MessagesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(565, 3, true);
			this.MessagesTabControl.Name = "MessagesTabControl";
			this.MessagesTabControl.SelectedIndex = 0;
			this.MessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 598, true);
			this.MessagesTabControl.TabIndex = 15;
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.Controls.Add(this.MessageDetailsTextBox);
			this.MessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageDetailsTabPage.Name = "MessageDetailsTabPage";
			this.MessageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 571, true);
			this.MessageDetailsTabPage.TabIndex = 1;
			this.MessageDetailsTabPage.Text = "Message Details";
			// 
			// MessageDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageDetailsTextBox, "Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).EM_MessageInterpretation)));
			this.MessageDetailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageDetailsTextBox.Multiline = true;
			this.MessageDetailsTextBox.Name = "MessageDetailsTextBox";
			this.MessageDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 565, true);
			this.MessageDetailsTextBox.TabIndex = 2;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Controls.Add(this.MessageTextTextBox);
			this.MessageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageTextTabPage.Name = "MessageTextTabPage";
			this.MessageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 517, true);
			this.MessageTextTabPage.TabIndex = 0;
			this.MessageTextTabPage.Text = "Message Text";
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "Messages.EM_MessageTextDetail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).EM_MessageTextDetail)));
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 511, true);
			this.MessageTextTextBox.TabIndex = 1;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageTextTextBox, false);
			// 
			// StatusErrorsTabPage
			// 
			this.StatusErrorsTabPage.Controls.Add(this.messagesStatusErrorsUserControl);
			this.StatusErrorsTabPage.Controls.Add(this.StatusesErrorsLabel);
			this.StatusErrorsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StatusErrorsTabPage.Name = "StatusErrorsTabPage";
			this.StatusErrorsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 517, true);
			this.StatusErrorsTabPage.TabIndex = 2;
			this.StatusErrorsTabPage.Text = "Status/Errors";
			// 
			// messagesStatusErrorsUserControl
			// 
			this.messagesStatusErrorsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messagesStatusErrorsUserControl, "Messages.StatusesAndErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.Business.StatusErrorsDataViewCollection)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).StatusesAndErrors)));
			this.messagesStatusErrorsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesStatusErrorsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messagesStatusErrorsUserControl.Name = "messagesStatusErrorsUserControl";
			this.messagesStatusErrorsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 517, true);
			this.messagesStatusErrorsUserControl.TabIndex = 19;
			// 
			// StatusesErrorsLabel
			// 
			this.BindingSource.SetBindingMember(this.StatusesErrorsLabel, "Messages.StatusesErrorsExist");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).StatusesErrorsExist)));
			this.StatusesErrorsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusesErrorsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.StatusesErrorsLabel.ForeColor = System.Drawing.Color.Black;
			this.StatusesErrorsLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatusesErrorsLabel, false);
			this.StatusesErrorsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusesErrorsLabel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 0, true);
			this.StatusesErrorsLabel.Name = "StatusesErrorsLabel";
			this.StatusesErrorsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 517, true);
			this.StatusesErrorsLabel.TabIndex = 18;
			this.StatusesErrorsLabel.VisibleChanged += new System.EventHandler(this.StatusesErrorsLabel_VisibleChanged);
			// 
			// MessagesSplitter
			// 
			this.MessagesSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(562, 3, true);
			this.MessagesSplitter.Name = "MessagesSplitter";
			this.MessagesSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 598, true);
			this.MessagesSplitter.TabIndex = 14;
			this.MessagesSplitter.TabStop = false;
			// 
			// HistoryGroupBox
			// 
			this.HistoryGroupBox.Controls.Add(this.MessagesGrid);
			this.HistoryGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.HistoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HistoryGroupBox.Name = "HistoryGroupBox";
			this.HistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 598, true);
			this.HistoryGroupBox.TabIndex = 13;
			this.HistoryGroupBox.TabStop = false;
			this.HistoryGroupBox.Text = "History";
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).CountOfReconOriginalEntries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Messaging.Business.CBPEDIMessage)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Messages)).SyncRoot)).EM_SendWithMessageErrorsFormatted)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo10.Caption = "Message No";
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo10.IsMandatory = true;
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.ToolTip = "Message Number";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.Caption = "Message Type";
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo11.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.ToolTip = "Message Type";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo6.Caption = "Message Time";
			zDateEditColumnStyleInfo6.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo7.Caption = "Create Time (UTC)";
			zDateEditColumnStyleInfo7.ColumnName = "EM_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo7.IsReadOnly = true;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.Caption = "Interchange No";
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.ToolTip = "Interchange No in which this message was contained.";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo8.Caption = "Interchange Sent";
			zDateEditColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo8.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo8.IsReadOnly = true;
			zDateEditColumnStyleInfo8.ToolTip = "It is when messages are actually sent to or received from Customs.";
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo13.Caption = "Sender";
			zTextBoxColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo13.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.ToolTip = "It is the person who created this message.";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo14.Caption = "Original Entries #";
			zTextBoxColumnStyleInfo14.ColumnName = "CountOfReconOriginalEntries";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo15.Caption = "Sent With Errors";
			zTextBoxColumnStyleInfo15.ColumnName = "EM_SendWithMessageErrorsFormatted";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.GridId = "71cef950-145c-4fd3-8b31-ace39ab09f8e";
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "zGrid1";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 579, true);
			this.MessagesGrid.TabIndex = 2;
			// 
			// EntriesTabPage
			// 
			this.EntriesTabPage.Controls.Add(this.EntriesGroupBox);
			this.EntriesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EntriesTabPage.Name = "EntriesTabPage";
			this.EntriesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EntriesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 604, true);
			this.EntriesTabPage.TabIndex = 8;
			this.EntriesTabPage.Text = "Entries";
			// 
			// EntriesGroupBox
			// 
			this.EntriesGroupBox.Controls.Add(this.OriginalEntriesGrid);
			this.EntriesGroupBox.Controls.Add(this.EntriesSplitter);
			this.EntriesGroupBox.Controls.Add(this.DetailsGroupBox);
			this.EntriesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntriesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntriesGroupBox.Name = "EntriesGroupBox";
			this.EntriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1007, 598, true);
			this.EntriesGroupBox.TabIndex = 0;
			this.EntriesGroupBox.TabStop = false;
			this.EntriesGroupBox.Text = "Original Entries";
			// 
			// OriginalEntriesGrid
			// 
			this.OriginalEntriesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OriginalEntriesGrid, "OriginalEntries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_MsgMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).CH_OrigEntryReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_NoLineDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).CH_OriginalDeclarationReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_ImportDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_ReleaseDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_PaymentDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_SchDEntry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_DateForMPFCalc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_DutyRateDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_IsHMFApplicable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).Lookups.YesNoList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_OwnerRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).OriginalDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).ReconDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).DeclarationBondNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).OriginalFee)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).ReconFee)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).OriginalTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).ReconTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).ReconInterest)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_CalcOrigDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_PriorDisclosure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_NAFTAClaimStat)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_ProtestStat)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_ProtestID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_PendingActionIDType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_PendingActionID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_CottonFeeMandatory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_MonthlyFiling)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_ChangedLinesOnly)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).MPC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_R_OrigCV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_NAFTAReconIndicator)));
			this.OriginalEntriesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "Msg Mode";
			zDropEditColumnStyleInfo1.ColumnName = "US_R_MsgMode";
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.Caption = "Filer Code + Entry No.";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CH_OrigEntryReference";
			zCodeFindBoxColumnStyleInfo1.PopupCaption = "Original Entries";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.Caption = "No Change/No Lines";
			zCheckBoxColumnStyleInfo1.ColumnName = "US_R_NoLineDetails";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
			zTextBoxColumnStyleInfo1.Caption = "Job #";
			zTextBoxColumnStyleInfo1.ColumnName = "CH_OriginalDeclarationReference";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.Caption = "Import Date";
			zDateEditColumnStyleInfo1.ColumnName = "US_ImportDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "Entry Date";
			zDateEditColumnStyleInfo2.ColumnName = "US_R_ReleaseDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.Caption = "Payment Date";
			zDateEditColumnStyleInfo3.ColumnName = "US_PaymentDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			zCodeFindBoxColumnStyleInfo2.Caption = "Entry Port";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "US_SchDEntry";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDateEditColumnStyleInfo4.Caption = "MPF Date";
			zDateEditColumnStyleInfo4.ColumnName = "US_R_DateForMPFCalc";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo5.Caption = "Duty Rate Date";
			zDateEditColumnStyleInfo5.ColumnName = "US_R_DutyRateDate";
			zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.YesNoList";
			zDropEditColumnStyleInfo2.Caption = "Calculate HMF";
			zDropEditColumnStyleInfo2.ColumnName = "US_R_IsHMFApplicable";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zTextBoxColumnStyleInfo2.Caption = "Owner Ref.";
			zTextBoxColumnStyleInfo2.ColumnName = "US_R_OwnerRef";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.Caption = "Goods Description";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "US_R_GoodsDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Original Duty";
			zCalcEditColumnStyleInfo1.ColumnName = "OriginalDuty";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Recon Duty";
			zCalcEditColumnStyleInfo2.ColumnName = "ReconDuty";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.Caption = "Bond Producer Acc #";
			zTextBoxColumnStyleInfo4.ColumnName = "DeclarationBondNo";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Original Fee";
			zCalcEditColumnStyleInfo3.ColumnName = "OriginalFee";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Recon Fee";
			zCalcEditColumnStyleInfo4.ColumnName = "ReconFee";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = "Original Tax";
			zCalcEditColumnStyleInfo5.ColumnName = "OriginalTax";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.Caption = "Recon Tax";
			zCalcEditColumnStyleInfo6.ColumnName = "ReconTax";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.Caption = "Recon Interest";
			zCalcEditColumnStyleInfo7.ColumnName = "ReconInterest";
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zCheckBoxColumnStyleInfo2.Caption = "Calc Orig. Duty/Fees";
			zCheckBoxColumnStyleInfo2.ColumnName = "US_R_CalcOrigDuty";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("eb15d0e2-aaa7-42b2-985b-55c98c0179ce", "Prior Disclosure?");
			zCheckBoxColumnStyleInfo3.ColumnName = "US_PriorDisclosure";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b0f66687-ce00-40da-beb3-97082826f7b2", "NAFTA 303?");
			zCheckBoxColumnStyleInfo4.ColumnName = "US_NAFTAClaimStat";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5a378ced-6c38-45ea-8f8f-530992a21f97", "Protest Filed?");
			zCheckBoxColumnStyleInfo5.ColumnName = "US_ProtestStat";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a0f94fd4-cec7-496f-b16d-a30d071a4c41", "Protest ID.");
			zTextBoxColumnStyleInfo5.ColumnName = "US_ProtestID";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3285539f-5f68-4add-b152-3061f45558e2", "Pending Act. ID. Type");
			zDropEditColumnStyleInfo3.ColumnName = "US_PendingActionIDType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("89353f5a-f383-46a2-bf7a-c063ca20c5e5", "Pending Act. ID.");
			zTextBoxColumnStyleInfo6.ColumnName = "US_PendingActionID";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo6.ColumnName = "US_R_CottonFeeMandatory";
			zCheckBoxColumnStyleInfo6.IsVisible = false;
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f5f00545-4f84-4261-ab65-295de18a76fc", "Monthly Filing");
			zCheckBoxColumnStyleInfo7.ColumnName = "US_R_MonthlyFiling";
			zCheckBoxColumnStyleInfo7.GroupName = Enterprise.Customs.US.GUI.Res.GetData("daeac06f-4077-49f2-be7f-afea68458ac7", "Monthly Filing");
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo8.Caption = "Changed Lines Only";
			zCheckBoxColumnStyleInfo8.ColumnName = "US_R_ChangedLinesOnly";
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.Caption = "MPC";
			zCalcEditColumnStyleInfo8.ColumnName = "MPC";
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.Caption = "Total Orig. Customs Value";
			zCalcEditColumnStyleInfo9.ColumnName = "US_R_OrigCV";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCheckBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("4863a6e1-de0d-480d-b59e-254e2da29c03", "FTA Recon Filed?");
			zCheckBoxColumnStyleInfo9.ColumnName = "US_NAFTAReconIndicator";
			zCheckBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.OriginalEntriesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OriginalEntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OriginalEntriesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OriginalEntriesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.OriginalEntriesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.OriginalEntriesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.OriginalEntriesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.OriginalEntriesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OriginalEntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OriginalEntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.OriginalEntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.OriginalEntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.OriginalEntriesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OriginalEntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.OriginalEntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo9);
			this.OriginalEntriesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OriginalEntriesGrid.GridId = "c49651a3-50ac-4754-8ccf-641571f1ca41";
			this.OriginalEntriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OriginalEntriesGrid.LayoutKey = "OriginalEntriesGrid";
			this.OriginalEntriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OriginalEntriesGrid.Name = "OriginalEntriesGrid";
			this.OriginalEntriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 392, true);
			this.OriginalEntriesGrid.TabIndex = 2;
			// 
			// EntriesSplitter
			// 
			this.EntriesSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.EntriesSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 408, true);
			this.EntriesSplitter.Name = "EntriesSplitter";
			this.EntriesSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 3, true);
			this.EntriesSplitter.TabIndex = 1;
			this.EntriesSplitter.TabStop = false;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.ReconChargesGroupBox);
			this.DetailsGroupBox.Controls.Add(this.OriginalChargesGroupBox);
			this.DetailsGroupBox.Controls.Add(this.OriginalEntryFeeSummaryTabControl);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 411, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 184, true);
			this.DetailsGroupBox.TabIndex = 6;
			this.DetailsGroupBox.TabStop = false;
			this.DetailsGroupBox.Text = "Details";
			// 
			// ReconChargesGroupBox
			// 
			this.ReconChargesGroupBox.Controls.Add(this.ReconChargesGrid);
			this.ReconChargesGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ReconChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(665, 16, true);
			this.ReconChargesGroupBox.Name = "ReconChargesGroupBox";
			this.ReconChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 165, true);
			this.ReconChargesGroupBox.TabIndex = 5;
			this.ReconChargesGroupBox.TabStop = false;
			this.ReconChargesGroupBox.Text = "Recon Customs Fees";
			// 
			// ReconChargesGrid
			// 
			this.ReconChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReconChargesGrid, "OriginalEntries.ReconCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).ReconCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusEntryHeaderCharges)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).ReconCharges)).SyncRoot)).C1_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CusEntryHeaderCharges)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).ReconCharges)).SyncRoot)).ChargeTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.CusEntryHeaderCharges)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).ReconCharges)).SyncRoot)).C1_ChargeAmount)));
			this.ReconChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.Caption = "Charge Type";
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "C1_ChargeType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.Caption = "Charge Desc";
			zTextBoxColumnStyleInfo7.ColumnName = "ChargeTypeDescription";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.Caption = "Amount";
			zCalcEditColumnStyleInfo10.ColumnName = "C1_ChargeAmount";
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ReconChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ReconChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ReconChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.ReconChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReconChargesGrid.GridId = "3f4833a5-b8dc-4991-bf60-50deb3c28736";
			this.ReconChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReconChargesGrid.LayoutKey = "ReconChargesGrid";
			this.ReconChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ReconChargesGrid.Name = "ReconChargesGrid";
			this.ReconChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 146, true);
			this.ReconChargesGrid.TabIndex = 0;
			// 
			// OriginalChargesGroupBox
			// 
			this.OriginalChargesGroupBox.Controls.Add(this.OriginalChargesGrid);
			this.OriginalChargesGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.OriginalChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(348, 16, true);
			this.OriginalChargesGroupBox.Name = "OriginalChargesGroupBox";
			this.OriginalChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(317, 165, true);
			this.OriginalChargesGroupBox.TabIndex = 4;
			this.OriginalChargesGroupBox.TabStop = false;
			this.OriginalChargesGroupBox.Text = "Original Customs Fees";
			// 
			// OriginalChargesGrid
			// 
			this.OriginalChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OriginalChargesGrid, "OriginalEntries.OriginalCharges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).OriginalCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconEntryOriginalCharge)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).OriginalCharges)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconEntryOriginalCharge)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).OriginalCharges)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconEntryOriginalCharge)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).OriginalCharges)).SyncRoot)).CY_Amount)));
			this.OriginalChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo5.Caption = "Charge Type";
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.Caption = "Charge Desc";
			zTextBoxColumnStyleInfo8.ColumnName = "Description";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.Caption = "Amount";
			zCalcEditColumnStyleInfo11.ColumnName = "CY_Amount";
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OriginalChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.OriginalChargesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.OriginalChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.OriginalChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OriginalChargesGrid.GridId = "cf61b007-03ac-48b1-864f-53d8de49b5c6";
			this.OriginalChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OriginalChargesGrid.LayoutKey = "OriginalChargesGrid";
			this.OriginalChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OriginalChargesGrid.Name = "OriginalChargesGrid";
			this.OriginalChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 146, true);
			this.OriginalChargesGrid.TabIndex = 0;
			// 
			// OriginalEntryFeeSummaryTabControl
			// 
			this.OriginalEntryFeeSummaryTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.OriginalEntryFeeSummaryTabControl.Controls.Add(this.SummaryTabPage);
			this.OriginalEntryFeeSummaryTabControl.Controls.Add(this.RefundedFeesTabPage);
			this.OriginalEntryFeeSummaryTabControl.Controls.Add(this.StatementsTabPage);
			this.OriginalEntryFeeSummaryTabControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.OriginalEntryFeeSummaryTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OriginalEntryFeeSummaryTabControl.Name = "OriginalEntryFeeSummaryTabControl";
			this.OriginalEntryFeeSummaryTabControl.SelectedIndex = 0;
			this.OriginalEntryFeeSummaryTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 165, true);
			this.OriginalEntryFeeSummaryTabControl.TabIndex = 3;
			// 
			// SummaryTabPage
			// 
			this.SummaryTabPage.Controls.Add(this.DetailsPanel);
			this.SummaryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SummaryTabPage.Name = "SummaryTabPage";
			this.SummaryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SummaryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 138, true);
			this.SummaryTabPage.TabIndex = 0;
			this.SummaryTabPage.Text = "Summary";
			this.SummaryTabPage.UseVisualStyleBackColor = true;
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.BackColor = System.Drawing.SystemColors.Control;
			this.DetailsPanel.Controls.Add(this.ReconInterestCalcEdit);
			this.DetailsPanel.Controls.Add(this.OriginalFeesCalcEdit);
			this.DetailsPanel.Controls.Add(this.OriginalTaxesCalcEdit);
			this.DetailsPanel.Controls.Add(this.ReconFeesCalcEdit);
			this.DetailsPanel.Controls.Add(this.ReconTaxesCalcEdit);
			this.DetailsPanel.Controls.Add(this.ReconDutyCalcEdit);
			this.DetailsPanel.Controls.Add(this.OriginalDutyCalcEdit);
			this.DetailsPanel.Controls.Add(this.TaxesLabel);
			this.DetailsPanel.Controls.Add(this.OriginalLabel);
			this.DetailsPanel.Controls.Add(this.InterestLabel);
			this.DetailsPanel.Controls.Add(this.FeesLabel);
			this.DetailsPanel.Controls.Add(this.DutyLabel);
			this.DetailsPanel.Controls.Add(this.ReconLabel);
			this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 132, true);
			this.DetailsPanel.TabIndex = 1;
			// 
			// ReconInterestCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ReconInterestCalcEdit, "OriginalEntries.ReconInterest");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).ReconInterest)));
			this.ReconInterestCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReconInterestCalcEdit, false);
			this.ReconInterestCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 103, true);
			this.ReconInterestCalcEdit.Name = "ReconInterestCalcEdit";
			this.ReconInterestCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ReconInterestCalcEdit.TabIndex = 12;
			this.ReconInterestCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OriginalFeesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OriginalFeesCalcEdit, "OriginalEntries.OriginalFee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).OriginalFee)));
			this.OriginalFeesCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OriginalFeesCalcEdit, false);
			this.OriginalFeesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 51, true);
			this.OriginalFeesCalcEdit.Name = "OriginalFeesCalcEdit";
			this.OriginalFeesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OriginalFeesCalcEdit.TabIndex = 6;
			this.OriginalFeesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OriginalTaxesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OriginalTaxesCalcEdit, "OriginalEntries.OriginalTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).OriginalTax)));
			this.OriginalTaxesCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OriginalTaxesCalcEdit, false);
			this.OriginalTaxesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 77, true);
			this.OriginalTaxesCalcEdit.Name = "OriginalTaxesCalcEdit";
			this.OriginalTaxesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OriginalTaxesCalcEdit.TabIndex = 9;
			this.OriginalTaxesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReconFeesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ReconFeesCalcEdit, "OriginalEntries.ReconFee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).ReconFee)));
			this.ReconFeesCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReconFeesCalcEdit, false);
			this.ReconFeesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 51, true);
			this.ReconFeesCalcEdit.Name = "ReconFeesCalcEdit";
			this.ReconFeesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ReconFeesCalcEdit.TabIndex = 7;
			this.ReconFeesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReconTaxesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ReconTaxesCalcEdit, "OriginalEntries.ReconTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).ReconTax)));
			this.ReconTaxesCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReconTaxesCalcEdit, false);
			this.ReconTaxesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 77, true);
			this.ReconTaxesCalcEdit.Name = "ReconTaxesCalcEdit";
			this.ReconTaxesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ReconTaxesCalcEdit.TabIndex = 10;
			this.ReconTaxesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReconDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ReconDutyCalcEdit, "OriginalEntries.ReconDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).ReconDuty)));
			this.ReconDutyCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReconDutyCalcEdit, false);
			this.ReconDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 25, true);
			this.ReconDutyCalcEdit.Name = "ReconDutyCalcEdit";
			this.ReconDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ReconDutyCalcEdit.TabIndex = 4;
			this.ReconDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OriginalDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OriginalDutyCalcEdit, "OriginalEntries.OriginalDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).OriginalDuty)));
			this.OriginalDutyCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OriginalDutyCalcEdit, false);
			this.OriginalDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 25, true);
			this.OriginalDutyCalcEdit.Name = "OriginalDutyCalcEdit";
			this.OriginalDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OriginalDutyCalcEdit.TabIndex = 3;
			this.OriginalDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TaxesLabel
			// 
			this.TaxesLabel.AutoSize = true;
			this.TaxesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TaxesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 81, true);
			this.TaxesLabel.Name = "TaxesLabel";
			this.TaxesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 13, true);
			this.TaxesLabel.TabIndex = 8;
			this.TaxesLabel.Text = "Taxes:";
			// 
			// OriginalLabel
			// 
			this.OriginalLabel.AutoSize = true;
			this.OriginalLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.OriginalLabel.IsFontBold = true;
			this.OriginalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 6, true);
			this.OriginalLabel.Name = "OriginalLabel";
			this.OriginalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.OriginalLabel.TabIndex = 0;
			this.OriginalLabel.Text = "Original";
			// 
			// InterestLabel
			// 
			this.InterestLabel.AutoSize = true;
			this.InterestLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InterestLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 107, true);
			this.InterestLabel.Name = "InterestLabel";
			this.InterestLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 13, true);
			this.InterestLabel.TabIndex = 11;
			this.InterestLabel.Text = "Interest:";
			// 
			// FeesLabel
			// 
			this.FeesLabel.AutoSize = true;
			this.FeesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FeesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 55, true);
			this.FeesLabel.Name = "FeesLabel";
			this.FeesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 13, true);
			this.FeesLabel.TabIndex = 5;
			this.FeesLabel.Text = "Fees:";
			// 
			// DutyLabel
			// 
			this.DutyLabel.AutoSize = true;
			this.DutyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DutyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 29, true);
			this.DutyLabel.Name = "DutyLabel";
			this.DutyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 13, true);
			this.DutyLabel.TabIndex = 2;
			this.DutyLabel.Text = "Duty:";
			// 
			// ReconLabel
			// 
			this.ReconLabel.AutoSize = true;
			this.ReconLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ReconLabel.IsFontBold = true;
			this.ReconLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 6, true);
			this.ReconLabel.Name = "ReconLabel";
			this.ReconLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 13, true);
			this.ReconLabel.TabIndex = 1;
			this.ReconLabel.Text = "Recon";
			// 
			// RefundedFeesTabPage
			// 
			this.RefundedFeesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.RefundedFeesTabPage.Controls.Add(this.EntryRefundedFeesGrid);
			this.RefundedFeesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RefundedFeesTabPage.Name = "RefundedFeesTabPage";
			this.RefundedFeesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RefundedFeesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 138, true);
			this.RefundedFeesTabPage.TabIndex = 1;
			this.RefundedFeesTabPage.Text = "Refunded Fees (force zero valued fee in 21 records) ";
			// 
			// EntryRefundedFeesGrid
			// 
			this.EntryRefundedFeesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EntryRefundedFeesGrid, "OriginalEntries.RefundedFees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).RefundedFees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconRefundedCharge)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).RefundedFees)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconRefundedCharge)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).RefundedFees)).SyncRoot)).Description)));
			this.EntryRefundedFeesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo6.Caption = "Fee Code";
			zDropEditColumnStyleInfo6.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.Caption = "Description";
			zTextBoxColumnStyleInfo9.ColumnName = "Description";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.EntryRefundedFeesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.EntryRefundedFeesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.EntryRefundedFeesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryRefundedFeesGrid.GridId = "f95f02ed-40d7-4a06-909a-6dea12af01ff";
			this.EntryRefundedFeesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EntryRefundedFeesGrid.LayoutKey = "EntryRefundedFeesGrid";
			this.EntryRefundedFeesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.EntryRefundedFeesGrid.Name = "EntryRefundedFeesGrid";
			this.EntryRefundedFeesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 132, true);
			this.EntryRefundedFeesGrid.TabIndex = 0;
			// 
			// StatementsTabPage
			// 
			this.StatementsTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("53e67fda-64cd-492f-ad53-695d08a7c5b3", "Statements");
			this.StatementsTabPage.Controls.Add(this.StatementsPanel);
			this.StatementsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StatementsTabPage.Name = "StatementsTabPage";
			this.StatementsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 138, true);
			this.StatementsTabPage.TabIndex = 2;
			this.StatementsTabPage.Text = "Statements";
			// 
			// StatementsPanel
			// 
			this.StatementsPanel.Controls.Add(this.ProtestFiledLabel);
			this.StatementsPanel.Controls.Add(this.NAFTA303Label);
			this.StatementsPanel.Controls.Add(this.ProtestFiledCheckBox);
			this.StatementsPanel.Controls.Add(this.NAFTA303CheckBox);
			this.StatementsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatementsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatementsPanel.Name = "StatementsPanel";
			this.StatementsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 138, true);
			this.StatementsPanel.TabIndex = 0;
			// 
			// ProtestFiledLabel
			// 
			this.ProtestFiledLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ProtestFiledLabel.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e51ba5fc-618e-4f60-874c-3fc9c3441bc9", "Has any person filed a protest or a petition or request for re-liquidation relating to the good under any provision of law?");
			this.ProtestFiledLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ProtestFiledLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 77, true);
			this.ProtestFiledLabel.Name = "ProtestFiledLabel";
			this.ProtestFiledLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 61, true);
			this.ProtestFiledLabel.TabIndex = 3;
			this.ProtestFiledLabel.Text = "Has any person filed a protest or a petition or request for re-liquidation relati" +
    "ng to the good under any provision of law?";
			// 
			// NAFTA303Label
			// 
			this.NAFTA303Label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.NAFTA303Label.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("38ebc20e-f220-4067-b581-3347cb82cb55", "Is the importer of the goods aware of any claim for refund, waiver, or reduction of duties relating to the good within the meaning of NAFTA Article 303?");
			this.NAFTA303Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NAFTA303Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 7, true);
			this.NAFTA303Label.Name = "NAFTA303Label";
			this.NAFTA303Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 62, true);
			this.NAFTA303Label.TabIndex = 2;
			this.NAFTA303Label.Text = "Is the importer of the goods aware of any claim for refund, waiver, or reductio" +
	"n of duties relating to the good within the meaning of NAFTA Article 303?";
			// 
			// ProtestFiledCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ProtestFiledCheckBox, "OriginalEntries.US_ProtestStat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_ProtestStat)));
			this.ProtestFiledCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8ea49f1c-612a-4893-b6a2-e2b7cd52f37e", "Protest Filed?");
			this.ProtestFiledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ProtestFiledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 84, true);
			this.ProtestFiledCheckBox.Name = "ProtestFiledCheckBox";
			this.ProtestFiledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 45, true);
			this.ProtestFiledCheckBox.TabIndex = 1;
			this.ProtestFiledCheckBox.Text = "Protest Filed?";
			this.ProtestFiledCheckBox.UseVisualStyleBackColor = true;
			// 
			// NAFTA303CheckBox
			// 
			this.BindingSource.SetBindingMember(this.NAFTA303CheckBox, "OriginalEntries.US_NAFTAClaimStat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconOriginalEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).OriginalEntries)).SyncRoot)).US_NAFTAClaimStat)));
			this.NAFTA303CheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8173a8a4-3383-40dd-804a-1fd3d2e4bab6", "NAFTA 303?");
			this.NAFTA303CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NAFTA303CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 17, true);
			this.NAFTA303CheckBox.Name = "NAFTA303CheckBox";
			this.NAFTA303CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 45, true);
			this.NAFTA303CheckBox.TabIndex = 0;
			this.NAFTA303CheckBox.Text = "NAFTA 303?";
			this.NAFTA303CheckBox.UseVisualStyleBackColor = true;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.WorkflowTabPage.TabIndex = 9;
			// 
			// HeaderDetailsReconHeaderUserControl
			// 
			this.HeaderDetailsReconHeaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HeaderDetailsReconHeaderUserControl, ".");
			this.HeaderDetailsReconHeaderUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderDetailsReconHeaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderDetailsReconHeaderUserControl.Name = "HeaderDetailsReconHeaderUserControl";
			this.HeaderDetailsReconHeaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 604, true);
			this.HeaderDetailsReconHeaderUserControl.TabIndex = 0;
			// 
			// StatusTabPage
			// 
			this.StatusTabPage.Controls.Add(this.StatusTabControl);
			this.StatusTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StatusTabPage.Name = "StatusTabPage";
			this.StatusTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 604, true);
			this.StatusTabPage.TabIndex = 11;
			this.StatusTabPage.Text = "Status";
			// 
			// StatusTabControl
			// 
			this.StatusTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.StatusTabControl.Controls.Add(this.StatusDetailsTabPage);
			this.StatusTabControl.Controls.Add(this.LiquidationTabPage);
			this.StatusTabControl.Controls.Add(this.StatusNotificationsTabPage);
			this.StatusTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusTabControl.Name = "StatusTabControl";
			this.StatusTabControl.SelectedIndex = 0;
			this.StatusTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 604, true);
			this.StatusTabControl.TabIndex = 0;
			// 
			// StatusDetailsTabPage
			// 
			this.StatusDetailsTabPage.Controls.Add(this.StatementStatusGroupBox);
			this.StatusDetailsTabPage.Controls.Add(this.StatusSummaryGroupBox);
			this.StatusDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StatusDetailsTabPage.Name = "StatusDetailsTabPage";
			this.StatusDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 577, true);
			this.StatusDetailsTabPage.TabIndex = 0;
			this.StatusDetailsTabPage.Text = "Status";
			// 
			// StatementStatusGroupBox
			// 
			this.StatementStatusGroupBox.Controls.Add(this.AnticipatedLiquidatedDutyZCalcEdit);
			this.StatementStatusGroupBox.Controls.Add(this.AnticipatedLiquidationDateDateEdit);
			this.StatementStatusGroupBox.Controls.Add(this.collectionDateDateEdit);
			this.StatementStatusGroupBox.Controls.Add(this.RelatedStatementPKFindBox);
			this.StatementStatusGroupBox.Controls.Add(this.StatementPaymentDateDateEdit);
			this.StatementStatusGroupBox.Controls.Add(this.StatementPrintDateDateEdit);
			this.StatementStatusGroupBox.Controls.Add(this.RelatedStatementStatusDropEdit);
			this.StatementStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 109, true);
			this.StatementStatusGroupBox.Name = "StatementStatusGroupBox";
			this.StatementStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 152, true);
			this.StatementStatusGroupBox.TabIndex = 1;
			this.StatementStatusGroupBox.TabStop = false;
			this.StatementStatusGroupBox.Text = "Statement or Payment";
			// 
			// AnticipatedLiquidationDateDateEdit
			// 
			this.AnticipatedLiquidationDateDateEdit.AllowDrop = true;
			this.AnticipatedLiquidationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.AnticipatedLiquidationDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AnticipatedLiquidationDateDateEdit, "US_AnticipatedLiquidationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_AnticipatedLiquidationDate)));
			this.AnticipatedLiquidationDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5c4da0db-0fac-4463-9d14-e15e803d9da2", "Anticipated Liquidation Date");
			this.AnticipatedLiquidationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 119, true);
			this.AnticipatedLiquidationDateDateEdit.Name = "AnticipatedLiquidationDateDateEdit";
			this.AnticipatedLiquidationDateDateEdit.TabIndex = 6;
			// 
			// collectionDateDateEdit
			// 
			this.collectionDateDateEdit.AllowDrop = true;
			this.collectionDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.collectionDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.collectionDateDateEdit, "US_CollectionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_CollectionDate)));
			this.collectionDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("7956e654-57f9-4a3a-87a6-37fab80b91ab", "Collection Date");
			this.collectionDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 93, true);
			this.collectionDateDateEdit.Name = "collectionDateDateEdit";
			this.collectionDateDateEdit.TabIndex = 4;
			// 
			// RelatedStatementPKFindBox
			// 
			this.RelatedStatementPKFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedStatementPKFindBox, "RelatedStatementPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).RelatedStatementPK)));
			this.RelatedStatementPKFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8caf4279-c6b3-4a71-90d6-26466cb5493b", "Statement Number");
			this.RelatedStatementPKFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.RelatedStatementPKFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 15, true);
			this.RelatedStatementPKFindBox.Name = "RelatedStatementPKFindBox";
			this.RelatedStatementPKFindBox.PopupCaption = null;
			this.RelatedStatementPKFindBox.PreBoundMaxLength = 15;
			this.RelatedStatementPKFindBox.ShouldResize = true;
			this.RelatedStatementPKFindBox.ShowDescriptionBox = false;
			this.RelatedStatementPKFindBox.ShowNewFormWhenEmpty = false;
			this.RelatedStatementPKFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.RelatedStatementPKFindBox.TabIndex = 0;
			// 
			// StatementPaymentDateDateEdit
			// 
			this.StatementPaymentDateDateEdit.AllowDrop = true;
			this.StatementPaymentDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.StatementPaymentDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StatementPaymentDateDateEdit, "US_PaymentDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_PaymentDate)));
			this.StatementPaymentDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9fadb7b5-1f46-4cd2-91c4-a5c64d351974", "Payment Date");
			this.StatementPaymentDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 67, true);
			this.StatementPaymentDateDateEdit.Name = "StatementPaymentDateDateEdit";
			this.StatementPaymentDateDateEdit.TabIndex = 2;
			// 
			// StatementPrintDateDateEdit
			// 
			this.StatementPrintDateDateEdit.AllowDrop = true;
			this.StatementPrintDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.StatementPrintDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StatementPrintDateDateEdit, "RelatedStatement.B2_PrintDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).RelatedStatement.B2_PrintDate)));
			this.StatementPrintDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 67, true);
			this.StatementPrintDateDateEdit.Name = "StatementPrintDateDateEdit";
			this.StatementPrintDateDateEdit.TabIndex = 3;
			// 
			// RelatedStatementStatusDropEdit
			// 
			this.RelatedStatementStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedStatementStatusDropEdit, "RelatedStatement.B2_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).RelatedStatement.B2_Status)));
			this.RelatedStatementStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 41, true);
			this.RelatedStatementStatusDropEdit.Name = "RelatedStatementStatusDropEdit";
			this.RelatedStatementStatusDropEdit.PreBoundMaxLength = 3;
			this.RelatedStatementStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.RelatedStatementStatusDropEdit.TabIndex = 1;
			// 
			// StatusSummaryGroupBox
			// 
			this.StatusSummaryGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("f227dbf9-8525-45c0-a08d-9c8888102ee3", "Summary");
			this.StatusSummaryGroupBox.Controls.Add(this.CustomsStatusTextBox);
			this.StatusSummaryGroupBox.Controls.Add(this.MessageStatusTextBox);
			this.StatusSummaryGroupBox.Controls.Add(this.ENSLabel);
			this.StatusSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 3, true);
			this.StatusSummaryGroupBox.Name = "StatusSummaryGroupBox";
			this.StatusSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 100, true);
			this.StatusSummaryGroupBox.TabIndex = 0;
			this.StatusSummaryGroupBox.TabStop = false;
			// 
			// CustomsStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsStatusTextBox, "CustomsStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).CustomsStatusDescription)));
			this.CustomsStatusTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e41e8e8c-2f56-4898-ab40-651aec3b3660", "Status");
			this.CustomsStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 15, true);
			this.CustomsStatusTextBox.Name = "CustomsStatusTextBox";
			this.CustomsStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.CustomsStatusTextBox.TabIndex = 0;
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).MessageStatusDescription)));
			this.MessageStatusTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e3b94b1f-75b5-4fae-8be9-fd540e2ad4c5", "Message Status");
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 43, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.MessageStatusTextBox.TabIndex = 1;
			// 
			// LiquidationTabPage
			// 
			this.LiquidationTabPage.Controls.Add(this.liquidationDetailsUserControl);
			this.LiquidationTabPage.Controls.Add(this.liquidationsUserControl);
			this.LiquidationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LiquidationTabPage.Name = "LiquidationTabPage";
			this.LiquidationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 577, true);
			this.LiquidationTabPage.TabIndex = 1;
			this.LiquidationTabPage.Text = "Liquidations";
			// 
			// liquidationDetailsUserControl
			// 
			this.liquidationDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.liquidationDetailsUserControl, "Liquidations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.Business.CusLiquidation)(((Enterprise.Customs.US.Business.CusLiquidation)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Liquidations)).SyncRoot)))));
			this.liquidationDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.liquidationDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 135, true);
			this.liquidationDetailsUserControl.Name = "liquidationDetailsUserControl";
			this.liquidationDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 442, true);
			this.liquidationDetailsUserControl.TabIndex = 4;
			// 
			// liquidationsUserControl
			// 
			this.liquidationsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.liquidationsUserControl, "Liquidations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.Business.CusLiquidationCollection)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Liquidations)));
			this.liquidationsUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.liquidationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.liquidationsUserControl.Name = "liquidationsUserControl";
			this.liquidationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 135, true);
			this.liquidationsUserControl.TabIndex = 3;
			// 
			// StatusNotificationsTabPage
			// 
			this.StatusNotificationsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.StatusNotificationsTabPage.Controls.Add(this.entrySummaryStatusNotificationsUserControl);
			this.StatusNotificationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.StatusNotificationsTabPage.Name = "StatusNotificationsTabPage";
			this.StatusNotificationsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.StatusNotificationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 577, true);
			this.StatusNotificationsTabPage.TabIndex = 12;
			this.StatusNotificationsTabPage.Text = "Entry Summary Status Notifications";
			// 
			// entrySummaryStatusNotificationsUserControl
			// 
			this.entrySummaryStatusNotificationsUserControl.AllowDrop = true;
			this.entrySummaryStatusNotificationsUserControl.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.entrySummaryStatusNotificationsUserControl, "ENSStatusNotifications");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.US.Business.ErrorsRecordCollection)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).ENSStatusNotifications)));
			this.entrySummaryStatusNotificationsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.entrySummaryStatusNotificationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.entrySummaryStatusNotificationsUserControl.Name = "entrySummaryStatusNotificationsUserControl";
			this.entrySummaryStatusNotificationsUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.entrySummaryStatusNotificationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(999, 571, true);
			this.entrySummaryStatusNotificationsUserControl.TabIndex = 0;
			// 
			// AnticipatedLiquidatedDutyText
			// 
			this.BindingSource.SetBindingMember(this.AnticipatedLiquidatedDutyZCalcEdit, "US_AnticipatedLiquidatedDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_AnticipatedLiquidatedDuty)));
			this.AnticipatedLiquidatedDutyZCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1ba8a0c0-9d33-4653-a8fb-d6483909d336", "Liquidated Duty");
			this.AnticipatedLiquidatedDutyZCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 93, true);
			this.AnticipatedLiquidatedDutyZCalcEdit.Name = "AnticipatedLiquidatedDutyText";
			this.AnticipatedLiquidatedDutyZCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.AnticipatedLiquidatedDutyZCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.AnticipatedLiquidatedDutyZCalcEdit.TabIndex = 5;
			// 
			// ReconDeclarationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1363, 692, true);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.ReconDeclaration);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true);
			this.Name = "ReconDeclarationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "ReconDeclarationForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceLinesTabPage.ResumeLayout(false);
			this.InvoiceLinesTabPage.PerformLayout();
			this.EntryLinesReconInvoiceLineUserControl.ResumeLayout(true);
			this.EntryLinesReconInvoiceLineUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessagesTabControl.ResumeLayout(false);
			this.MessagesTabControl.PerformLayout();
			this.MessageDetailsTabPage.ResumeLayout(false);
			this.MessageDetailsTabPage.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.StatusErrorsTabPage.ResumeLayout(false);
			this.StatusErrorsTabPage.PerformLayout();
			this.messagesStatusErrorsUserControl.ResumeLayout(true);
			this.messagesStatusErrorsUserControl.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			this.HistoryGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessagesGrid.ResumeLayout(false);
			this.MessagesGrid.PerformLayout();
			this.EntriesTabPage.ResumeLayout(false);
			this.EntriesTabPage.PerformLayout();
			this.EntriesGroupBox.ResumeLayout(false);
			this.EntriesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OriginalEntriesGrid)).EndInit();
			this.OriginalEntriesGrid.ResumeLayout(false);
			this.OriginalEntriesGrid.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ReconChargesGroupBox.ResumeLayout(false);
			this.ReconChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReconChargesGrid)).EndInit();
			this.ReconChargesGrid.ResumeLayout(false);
			this.ReconChargesGrid.PerformLayout();
			this.OriginalChargesGroupBox.ResumeLayout(false);
			this.OriginalChargesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OriginalChargesGrid)).EndInit();
			this.OriginalChargesGrid.ResumeLayout(false);
			this.OriginalChargesGrid.PerformLayout();
			this.OriginalEntryFeeSummaryTabControl.ResumeLayout(false);
			this.OriginalEntryFeeSummaryTabControl.PerformLayout();
			this.SummaryTabPage.ResumeLayout(false);
			this.SummaryTabPage.PerformLayout();
			this.DetailsPanel.ResumeLayout(false);
			this.DetailsPanel.PerformLayout();
			this.RefundedFeesTabPage.ResumeLayout(false);
			this.RefundedFeesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EntryRefundedFeesGrid)).EndInit();
			this.EntryRefundedFeesGrid.ResumeLayout(false);
			this.EntryRefundedFeesGrid.PerformLayout();
			this.StatementsTabPage.ResumeLayout(false);
			this.StatementsTabPage.PerformLayout();
			this.StatementsPanel.ResumeLayout(false);
			this.StatementsPanel.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.HeaderDetailsReconHeaderUserControl.ResumeLayout(true);
			this.HeaderDetailsReconHeaderUserControl.PerformLayout();
			this.StatusTabPage.ResumeLayout(false);
			this.StatusTabPage.PerformLayout();
			this.StatusTabControl.ResumeLayout(false);
			this.StatusTabControl.PerformLayout();
			this.StatusDetailsTabPage.ResumeLayout(false);
			this.StatusDetailsTabPage.PerformLayout();
			this.StatementStatusGroupBox.ResumeLayout(false);
			this.StatementStatusGroupBox.PerformLayout();
			this.AnticipatedLiquidationDateDateEdit.ResumeLayout(true);
			this.AnticipatedLiquidationDateDateEdit.PerformLayout();
			this.collectionDateDateEdit.ResumeLayout(true);
			this.collectionDateDateEdit.PerformLayout();
			this.RelatedStatementPKFindBox.ResumeLayout(true);
			this.RelatedStatementPKFindBox.PerformLayout();
			this.StatementPaymentDateDateEdit.ResumeLayout(true);
			this.StatementPaymentDateDateEdit.PerformLayout();
			this.StatementPrintDateDateEdit.ResumeLayout(true);
			this.StatementPrintDateDateEdit.PerformLayout();
			this.RelatedStatementStatusDropEdit.ResumeLayout(true);
			this.RelatedStatementStatusDropEdit.PerformLayout();
			this.StatusSummaryGroupBox.ResumeLayout(false);
			this.StatusSummaryGroupBox.PerformLayout();
			this.LiquidationTabPage.ResumeLayout(false);
			this.LiquidationTabPage.PerformLayout();
			this.liquidationDetailsUserControl.ResumeLayout(true);
			this.liquidationDetailsUserControl.PerformLayout();
			this.liquidationsUserControl.ResumeLayout(true);
			this.liquidationsUserControl.PerformLayout();
			this.StatusNotificationsTabPage.ResumeLayout(false);
			this.StatusNotificationsTabPage.PerformLayout();
			this.entrySummaryStatusNotificationsUserControl.ResumeLayout(true);
			this.entrySummaryStatusNotificationsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZTabPage InvoiceLinesTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private ZTabPage EntriesTabPage;
		private ZGroupBox EntriesGroupBox;
		protected ZGroupBox HistoryGroupBox;
		protected Enterprise.ZArchitecture.ZGrid MessagesGrid;
		protected ZTabControl MessagesTabControl;
		private ZTabPage MessageDetailsTabPage;
		internal Enterprise.ZArchitecture.ZTextBox MessageDetailsTextBox;
		protected ZTabPage MessageTextTabPage;
		protected Enterprise.ZArchitecture.ZTextBox MessageTextTextBox;
		private CargoWise.Windows.UI.KSplitter MessagesSplitter;
		private Enterprise.ZArchitecture.ZGrid OriginalEntriesGrid;
		private CargoWise.Windows.UI.KSplitter EntriesSplitter;
		private ZGroupBox DetailsGroupBox;
		private ZTabPage StatusErrorsTabPage;
		public Enterprise.ZArchitecture.ZLabel StatusesErrorsLabel;
		private ZWorkflowTabPage WorkflowTabPage;
		public ReconInvoiceLineUserControl EntryLinesReconInvoiceLineUserControl;
		public ReconHeaderUserControl HeaderDetailsReconHeaderUserControl;
		private MessagesStatusErrorsUserControl messagesStatusErrorsUserControl;
		public ZTabPage StatusTabPage;
		private ZTabControl StatusTabControl;
		private ZTabPage StatusDetailsTabPage;
		private ZTabPage LiquidationTabPage;
		private ZTabPage StatusNotificationsTabPage;
		private LiquidationDetailsUserControl liquidationDetailsUserControl;
		private LiquidationsUserControl liquidationsUserControl;
		private EntrySummaryStatusNotificationsUserControl entrySummaryStatusNotificationsUserControl;
		private ZArchitecture.ZLabel ENSLabel;
		private ZGroupBox StatusSummaryGroupBox;
		private ZArchitecture.ZTextBox CustomsStatusTextBox;
		private ZArchitecture.ZTextBox MessageStatusTextBox;
		private ZGroupBox StatementStatusGroupBox;
		private ZGuidFindBox RelatedStatementPKFindBox;
		private ZDateEdit StatementPaymentDateDateEdit;
		private ZDateEdit StatementPrintDateDateEdit;
		private ZDropEdit RelatedStatementStatusDropEdit;
		internal ZGroupBox ReconChargesGroupBox;
		private ZArchitecture.ZGrid ReconChargesGrid;
		internal ZGroupBox OriginalChargesGroupBox;
		private ZArchitecture.ZGrid OriginalChargesGrid;
		private ZTabControl OriginalEntryFeeSummaryTabControl;
		private ZTabPage SummaryTabPage;
		private ZPanel DetailsPanel;
		private ZArchitecture.ZCalcEdit ReconInterestCalcEdit;
		private ZArchitecture.ZCalcEdit OriginalFeesCalcEdit;
		private ZArchitecture.ZCalcEdit OriginalTaxesCalcEdit;
		private ZArchitecture.ZCalcEdit ReconFeesCalcEdit;
		private ZArchitecture.ZCalcEdit ReconTaxesCalcEdit;
		private ZArchitecture.ZCalcEdit ReconDutyCalcEdit;
		private ZArchitecture.ZCalcEdit OriginalDutyCalcEdit;
		private ZArchitecture.ZLabel TaxesLabel;
		private ZArchitecture.ZLabel OriginalLabel;
		private ZArchitecture.ZLabel InterestLabel;
		private ZArchitecture.ZLabel FeesLabel;
		private ZArchitecture.ZLabel DutyLabel;
		private ZArchitecture.ZLabel ReconLabel;
		private ZTabPage RefundedFeesTabPage;
		private ZArchitecture.ZGrid EntryRefundedFeesGrid;
		private ZTabPage StatementsTabPage;
		private ZPanel StatementsPanel;
		private ZArchitecture.ZLabel ProtestFiledLabel;
		private ZArchitecture.ZLabel NAFTA303Label;
		private ZCheckBox ProtestFiledCheckBox;
		private ZCheckBox NAFTA303CheckBox;
		private ZDateEdit collectionDateDateEdit;
		private ZDateEdit AnticipatedLiquidationDateDateEdit;
		private ZArchitecture.ZCalcEdit AnticipatedLiquidatedDutyZCalcEdit;
	}
}
