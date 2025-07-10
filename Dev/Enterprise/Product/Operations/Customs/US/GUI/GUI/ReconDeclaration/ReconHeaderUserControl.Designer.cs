using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.US.GUI
{
	partial class ReconHeaderUserControl
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
			base.Dispose(disposing);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (!CustomFieldsDisplayControl.IsDisposed)
			{
				CustomFieldsDisplayControl.ForceBindingIncludingParents();
			}
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ReconHeaderDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JobDocAddressTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DocRecipientTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DocRecipientPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DocRecipientPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DocProvidedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DocRecipientDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ClaimantTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ClaimantPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClaimantPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ClaimIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClaimantDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ClaimantDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.AggregateFeesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QualifyingGoodsFTADecLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AggregateFeesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BrokerFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.NotifyPartyGuidFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.ApplicationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JE_RS_NKServiceLevelFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.WaiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NoChangeAggregateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.US_OH_IORGuidFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.SuretyCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EstimatedReconEntryDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.teamNoDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IssueCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AggregateIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CommentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.US_SchDEntryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ImportEntrySourceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PaymentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BrokerToPayDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StatementPrintDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ClientBranchDesignationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JE_GBGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.summaryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.reconInterestCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OriginalTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.differenceTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.reconTaxCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OriginalFeesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.differenceFeesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.reconFeesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.originalDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.differenceDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.reconDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.differenceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.summaryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.originalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.reconLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ReconEntryDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AllocateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryNumberDividerLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FilerCodeZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterZDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.CustomFieldsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomFieldsDisplayControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReconHeaderDetailsGroupBox.SuspendLayout();
			this.JobDocAddressTabControl.SuspendLayout();
			this.DocRecipientTabPage.SuspendLayout();
			this.DocRecipientPanel1.SuspendLayout();
			this.DocRecipientPanel2.SuspendLayout();
			this.DocProvidedDateEdit.SuspendLayout();
			this.DocRecipientDocAddressControl.SuspendLayout();
			this.ClaimantTabPage.SuspendLayout();
			this.ClaimantPanel1.SuspendLayout();
			this.ClaimantPanel2.SuspendLayout();
			this.ClaimantDateEdit.SuspendLayout();
			this.ClaimantDocAddressControl.SuspendLayout();
			this.AggregateFeesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AggregateFeesGrid)).BeginInit();
			this.AggregateFeesGrid.SuspendLayout();
			this.MainDetailsGroupBox.SuspendLayout();
			this.BrokerFindBox.SuspendLayout();
			this.NotifyPartyGuidFindBox.SuspendLayout();
			this.ApplicationCodeDropEdit.SuspendLayout();
			this.JE_RS_NKServiceLevelFindBox.SuspendLayout();
			this.US_OH_IORGuidFindBox.SuspendLayout();
			this.SuretyCodeDropEdit.SuspendLayout();
			this.EstimatedReconEntryDateDateEdit.SuspendLayout();
			this.teamNoDropEdit.SuspendLayout();
			this.IssueCodeDropEdit.SuspendLayout();
			this.US_SchDEntryCodeFindBox.SuspendLayout();
			this.ImportEntrySourceDropEdit.SuspendLayout();
			this.PaymentGroupBox.SuspendLayout();
			this.PaymentTypeDropEdit.SuspendLayout();
			this.BrokerToPayDropEdit.SuspendLayout();
			this.StatementPrintDateDateEdit.SuspendLayout();
			this.JE_GBGuidFindBox.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			this.summaryGroupBox.SuspendLayout();
			this.ReconEntryDetailsGroupBox.SuspendLayout();
			this.ImporterZDocAddressControl.SuspendLayout();
			this.CustomFieldsGroupBox.SuspendLayout();
			this.CustomFieldsDisplayControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.ReconDeclaration);
			// 
			// ReconHeaderDetailsGroupBox
			// 
			this.ReconHeaderDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|74cca5e1-be5a-48ad-b545-19ae349c1a92", "Recon. Header Details");
			this.ReconHeaderDetailsGroupBox.Controls.Add(this.QualifyingGoodsFTADecLabel);
			this.ReconHeaderDetailsGroupBox.Controls.Add(this.JobDocAddressTabControl);
			this.ReconHeaderDetailsGroupBox.Controls.Add(this.AggregateFeesGroupBox);
			this.ReconHeaderDetailsGroupBox.Controls.Add(this.MainDetailsGroupBox);
			this.ReconHeaderDetailsGroupBox.Controls.Add(this.PaymentGroupBox);
			this.ReconHeaderDetailsGroupBox.Controls.Add(this.CustomFieldsGroupBox);
			this.ReconHeaderDetailsGroupBox.Controls.Add(this.summaryGroupBox);
			this.ReconHeaderDetailsGroupBox.Controls.Add(this.ReconEntryDetailsGroupBox);
			this.ReconHeaderDetailsGroupBox.Controls.Add(this.ImporterZDocAddressControl);
			this.ReconHeaderDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReconHeaderDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReconHeaderDetailsGroupBox.Name = "ReconHeaderDetailsGroupBox";
			this.ReconHeaderDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 600, true);
			this.ReconHeaderDetailsGroupBox.TabIndex = 0;
			this.ReconHeaderDetailsGroupBox.TabStop = false;
			// 
			// JobDocAddressTabControl
			// 
			this.JobDocAddressTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.JobDocAddressTabControl.Controls.Add(this.DocRecipientTabPage);
			this.JobDocAddressTabControl.Controls.Add(this.ClaimantTabPage);
			this.JobDocAddressTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 12, true);
			this.JobDocAddressTabControl.Name = "JobDocAddressTabControl";
			this.JobDocAddressTabControl.SelectedIndex = 0;
			this.JobDocAddressTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 247, true);
			this.JobDocAddressTabControl.TabIndex = 1;
			// 
			// DocRecipientTabPage
			// 
			this.DocRecipientTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3d09cabb-4bbc-4c59-a9bf-69436180961c", "Doc. Recipient");
			this.DocRecipientTabPage.Controls.Add(this.DocRecipientPanel1);
			this.DocRecipientTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DocRecipientTabPage.Name = "DocRecipientTabPage";
			this.DocRecipientTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DocRecipientTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 225, true);
			this.DocRecipientTabPage.TabIndex = 0;
			this.DocRecipientTabPage.Text = "Doc. Recipient";
			this.DocRecipientTabPage.UseVisualStyleBackColor = true;
			// 
			// DocRecipientPanel1
			// 
			this.DocRecipientPanel1.Controls.Add(this.DocRecipientPanel2);
			this.DocRecipientPanel1.Controls.Add(this.DocRecipientDocAddressControl);
			this.DocRecipientPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.DocRecipientPanel1.Name = "DocRecipientPanel1";
			this.DocRecipientPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 223, true);
			this.DocRecipientPanel1.TabIndex = 0;
			// 
			// DocRecipientPanel2
			// 
			this.DocRecipientPanel2.Controls.Add(this.DocProvidedDateEdit);
			this.DocRecipientPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocRecipientPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 182, true);
			this.DocRecipientPanel2.Name = "DocRecipientPanel2";
			this.DocRecipientPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 41, true);
			this.DocRecipientPanel2.TabIndex = 2;
			// 
			// DocProvidedDateEdit
			// 
			this.DocProvidedDateEdit.AllowDrop = true;
			this.DocProvidedDateEdit.AutoCompleteMonthThreshold = 1;
			this.DocProvidedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DocProvidedDateEdit, "US_DocProvidedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_DocProvidedDate)));
			this.DocProvidedDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("832afd94-3838-4ba8-8b84-4d59e1175e29", "Doc. Provided Date");
			this.DocProvidedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 9, true);
			this.DocProvidedDateEdit.Name = "DocProvidedDateEdit";
			this.DocProvidedDateEdit.TabIndex = 7;
			// 
			// DocRecipientDocAddressControl
			// 
			this.DocRecipientDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocRecipientDocAddressControl, "SummaryDocRecipientAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).SummaryDocRecipientAddress)));
			this.DocRecipientDocAddressControl.BindToOrganisations = "Lookups+Organisations";
			this.DocRecipientDocAddressControl.CaptionResourceString = null;
			this.DocRecipientDocAddressControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.DocRecipientDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocRecipientDocAddressControl.Name = "DocRecipientDocAddressControl";
			this.DocRecipientDocAddressControl.ReadOnly = false;
			this.DocRecipientDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DocRecipientDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.DocRecipientDocAddressControl.TabIndex = 1;
			this.DocRecipientDocAddressControl.ValidationJustForced = false;
			// 
			// ClaimantTabPage
			// 
			this.ClaimantTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d6eec6ac-daed-4f39-b082-1a0d9eebecb0", "Claimant");
			this.ClaimantTabPage.Controls.Add(this.ClaimantPanel1);
			this.ClaimantTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ClaimantTabPage.Name = "ClaimantTabPage";
			this.ClaimantTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ClaimantTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 225, true);
			this.ClaimantTabPage.TabIndex = 1;
			this.ClaimantTabPage.Text = "Claimant";
			this.ClaimantTabPage.UseVisualStyleBackColor = true;
			// 
			// ClaimantPanel1
			// 
			this.ClaimantPanel1.Controls.Add(this.ClaimantPanel2);
			this.ClaimantPanel1.Controls.Add(this.ClaimantDocAddressControl);
			this.ClaimantPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 3, true);
			this.ClaimantPanel1.Name = "ClaimantPanel1";
			this.ClaimantPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 223, true);
			this.ClaimantPanel1.TabIndex = 1;
			// 
			// ClaimantPanel2
			// 
			this.ClaimantPanel2.Controls.Add(this.ClaimIDTextBox);
			this.ClaimantPanel2.Controls.Add(this.ClaimantDateEdit);
			this.ClaimantPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ClaimantPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 182, true);
			this.ClaimantPanel2.Name = "ClaimantPanel2";
			this.ClaimantPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 41, true);
			this.ClaimantPanel2.TabIndex = 2;
			// 
			// ClaimIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClaimIDTextBox, "US_ClaimID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_ClaimID)));
			this.ClaimIDTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9d8390bb-03d2-44db-a888-26d0b38d0ba5", "Claim ID.");
			this.ClaimIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 21, true);
			this.ClaimIDTextBox.Name = "ClaimIDTextBox";
			this.ClaimIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 17, true);
			this.ClaimIDTextBox.TabIndex = 8;
			// 
			// ClaimantDateEdit
			// 
			this.ClaimantDateEdit.AllowDrop = true;
			this.ClaimantDateEdit.AutoCompleteMonthThreshold = 1;
			this.ClaimantDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ClaimantDateEdit, "US_ClaimDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_ClaimDate)));
			this.ClaimantDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c62d4f6a-3705-4d90-a287-f86d9ad5f0e5", "Claim Date");
			this.ClaimantDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 2, true);
			this.ClaimantDateEdit.Name = "ClaimantDateEdit";
			this.ClaimantDateEdit.TabIndex = 7;
			// 
			// ClaimantDocAddressControl
			// 
			this.ClaimantDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClaimantDocAddressControl, "ClaimantAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).ClaimantAddress)));
			this.ClaimantDocAddressControl.BindToOrganisations = "Lookups+Organisations";
			this.ClaimantDocAddressControl.CaptionResourceString = null;
			this.ClaimantDocAddressControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.ClaimantDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ClaimantDocAddressControl.Name = "ClaimantDocAddressControl";
			this.ClaimantDocAddressControl.ReadOnly = false;
			this.ClaimantDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ClaimantDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ClaimantDocAddressControl.TabIndex = 1;
			this.ClaimantDocAddressControl.ValidationJustForced = false;
			// 
			// AggregateFeesGroupBox
			// 
			this.AggregateFeesGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8162d008-0a5a-4101-a0a6-54d8b6afc350", "Refunded Fees (force zero-value fee in 89 records)");
			this.AggregateFeesGroupBox.Controls.Add(this.AggregateFeesGrid);
			this.AggregateFeesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 395, true);
			this.AggregateFeesGroupBox.Name = "AggregateFeesGroupBox";
			this.AggregateFeesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 161, true);
			this.AggregateFeesGroupBox.TabIndex = 6;
			this.AggregateFeesGroupBox.TabStop = false;
			// 
			// QualifyingGoodsFTADecLabel
			// 
			this.QualifyingGoodsFTADecLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.QualifyingGoodsFTADecLabel.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2997c704-c7f6-4e46-8f37-8393c66e3940", "When filing a Free Trade Agreement Reconciliation you are declaring that the goods qualified as originating goods at the time of importation.");
			this.QualifyingGoodsFTADecLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.QualifyingGoodsFTADecLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 395, true);
			this.QualifyingGoodsFTADecLabel.Name = "QualifyingGoodsFTADecLabel";
			this.QualifyingGoodsFTADecLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 57, true);
			this.QualifyingGoodsFTADecLabel.TabIndex = 6;
			this.QualifyingGoodsFTADecLabel.Text = "When filing a Free Trade Agreement Reconciliation you are declaring that the goods qualified as originating goods at the time of importation.";
			// 
			// AggregateFeesGrid
			// 
			this.AggregateFeesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AggregateFeesGrid, "AggregateRefundedFees");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).AggregateRefundedFees)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconRefundedCharge)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).AggregateRefundedFees)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconRefundedCharge)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).AggregateRefundedFees)).SyncRoot)).Description)));
			this.AggregateFeesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e162c956-87f5-4c25-a79e-c07f3f6ce3d8", "Charge Type");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("631329ae-839e-4fb6-8386-7eecfa9c83a3", "Charge Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.AggregateFeesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AggregateFeesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AggregateFeesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AggregateFeesGrid.GridId = "cf61b007-03ac-48b1-864f-53d8de49b5c6";
			this.AggregateFeesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AggregateFeesGrid.LayoutKey = "OriginalChargesGrid";
			this.AggregateFeesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.AggregateFeesGrid.Name = "AggregateFeesGrid";
			this.AggregateFeesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(417, 144, true);
			this.AggregateFeesGrid.TabIndex = 0;
			// 
			// MainDetailsGroupBox
			// 
			this.MainDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3dae063a-65a3-4ea6-9559-11157a7291f3", "Details");
			this.MainDetailsGroupBox.Controls.Add(this.BrokerFindBox);
			this.MainDetailsGroupBox.Controls.Add(this.NotifyPartyGuidFindBox);
			this.MainDetailsGroupBox.Controls.Add(this.ApplicationCodeDropEdit);
			this.MainDetailsGroupBox.Controls.Add(this.JE_RS_NKServiceLevelFindBox);
			this.MainDetailsGroupBox.Controls.Add(this.WaiveCheckBox);
			this.MainDetailsGroupBox.Controls.Add(this.NoChangeAggregateCheckBox);
			this.MainDetailsGroupBox.Controls.Add(this.US_OH_IORGuidFindBox);
			this.MainDetailsGroupBox.Controls.Add(this.SuretyCodeDropEdit);
			this.MainDetailsGroupBox.Controls.Add(this.EstimatedReconEntryDateDateEdit);
			this.MainDetailsGroupBox.Controls.Add(this.teamNoDropEdit);
			this.MainDetailsGroupBox.Controls.Add(this.IssueCodeDropEdit);
			this.MainDetailsGroupBox.Controls.Add(this.AggregateIndicatorCheckBox);
			this.MainDetailsGroupBox.Controls.Add(this.CommentTextBox);
			this.MainDetailsGroupBox.Controls.Add(this.US_SchDEntryCodeFindBox);
			this.MainDetailsGroupBox.Controls.Add(this.ImportEntrySourceDropEdit);
			this.MainDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 263, true);
			this.MainDetailsGroupBox.Name = "MainDetailsGroupBox";
			this.MainDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 333, true);
			this.MainDetailsGroupBox.TabIndex = 2;
			this.MainDetailsGroupBox.TabStop = false;
			// 
			// BrokerFindBox
			// 
			this.BrokerFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerFindBox, "JE_GS_NKCusAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).JE_GS_NKCusAgent)));
			this.BrokerFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2e2bfa82-08fd-4663-b44e-0083ad399705", "Broker");
			this.BrokerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 96, true);
			this.BrokerFindBox.Name = "BrokerFindBox";
			this.BrokerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 17, true);
			this.BrokerFindBox.TabIndex = 3;
			// 
			// NotifyPartyGuidFindBox
			// 
			this.NotifyPartyGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyGuidFindBox, "JE_OH_NotifyParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).JE_OH_NotifyParty)));
			this.NotifyPartyGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ad30f14b-0ded-4925-9eab-6402f54a2bcd", "4811 Party");
			this.NotifyPartyGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 69, true);
			this.NotifyPartyGuidFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.NotifyPartyGuidFindBox.Name = "NotifyPartyGuidFindBox";
			this.NotifyPartyGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 17, true);
			this.NotifyPartyGuidFindBox.TabIndex = 2;
			// 
			// ApplicationCodeDropEdit
			// 
			this.ApplicationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApplicationCodeDropEdit, "JE_ApplicationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).JE_ApplicationCode)));
			this.ApplicationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 14, true);
			this.ApplicationCodeDropEdit.Name = "ApplicationCodeDropEdit";
			this.ApplicationCodeDropEdit.PreBoundMaxLength = 3;
			this.ApplicationCodeDropEdit.ShowDescriptionBox = false;
			this.ApplicationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.ApplicationCodeDropEdit.TabIndex = 0;
			// 
			// JE_RS_NKServiceLevelFindBox
			// 
			this.JE_RS_NKServiceLevelFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_RS_NKServiceLevelFindBox, "JE_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).JE_RS_NKServiceLevel)));
			this.JE_RS_NKServiceLevelFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("8D53A171-2BF6-4B57-98B4-904F5E84F2DA", "Service Level");
			this.JE_RS_NKServiceLevelFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 232, true);
			this.JE_RS_NKServiceLevelFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ServiceLevel;
			this.JE_RS_NKServiceLevelFindBox.Name = "JE_RS_NKServiceLevelFindBox";
			this.JE_RS_NKServiceLevelFindBox.PreBoundMaxLength = 3;
			this.JE_RS_NKServiceLevelFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.JE_RS_NKServiceLevelFindBox.TabIndex = 13;
			// 
			// WaiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.WaiveCheckBox, "US_R_Waive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_R_Waive)));
			this.WaiveCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("0d7d509f-7fd4-46d6-a83a-14edddada53d", "Waive Refunds");
			this.WaiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.WaiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WaiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 205, true);
			this.WaiveCheckBox.Name = "WaiveCheckBox";
			this.WaiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 17, true);
			this.WaiveCheckBox.TabIndex = 11;
			this.WaiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// NoChangeAggregateCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NoChangeAggregateCheckBox, "US_R_IsNoChangeAgg");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_R_IsNoChangeAgg)));
			this.NoChangeAggregateCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("9b2bf247-7aac-47f9-b4b7-844e52d2d807", "No-Change Aggregate");
			this.NoChangeAggregateCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.NoChangeAggregateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NoChangeAggregateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 205, true);
			this.NoChangeAggregateCheckBox.Name = "NoChangeAggregateCheckBox";
			this.NoChangeAggregateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.NoChangeAggregateCheckBox.TabIndex = 10;
			this.NoChangeAggregateCheckBox.UseVisualStyleBackColor = true;
			// 
			// US_OH_IORGuidFindBox
			// 
			this.US_OH_IORGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_OH_IORGuidFindBox, "IOROrgPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).IOROrgPK)));
			this.US_OH_IORGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|83719b9c-dd0e-4c32-97cc-d620010d7b78", "IOR");
			this.US_OH_IORGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 41, true);
			this.US_OH_IORGuidFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.US_OH_IORGuidFindBox.Name = "US_OH_IORGuidFindBox";
			this.US_OH_IORGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 17, true);
			this.US_OH_IORGuidFindBox.TabIndex = 1;
			// 
			// SuretyCodeDropEdit
			// 
			this.SuretyCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SuretyCodeDropEdit, "US_SuretyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_SuretyCode)));
			this.SuretyCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|a98fc1bf-c232-4e03-9982-d3c7e563c6b4", "Surety Code");
			this.SuretyCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 178, true);
			this.SuretyCodeDropEdit.Name = "SuretyCodeDropEdit";
			this.SuretyCodeDropEdit.PreBoundMaxLength = 3;
			this.SuretyCodeDropEdit.ShowDescriptionBox = false;
			this.SuretyCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.SuretyCodeDropEdit.TabIndex = 8;
			// 
			// EstimatedReconEntryDateDateEdit
			// 
			this.EstimatedReconEntryDateDateEdit.AllowDrop = true;
			this.EstimatedReconEntryDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.EstimatedReconEntryDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EstimatedReconEntryDateDateEdit, "US_EstimatedEntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_EstimatedEntryDate)));
			this.EstimatedReconEntryDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|b1d1419a-76d0-44e2-8980-3f301e666e16", "Est. Recon. Date");
			this.EstimatedReconEntryDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 151, true);
			this.EstimatedReconEntryDateDateEdit.Name = "EstimatedReconEntryDateDateEdit";
			this.EstimatedReconEntryDateDateEdit.TabIndex = 6;
			// 
			// teamNoDropEdit
			// 
			this.teamNoDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.teamNoDropEdit, "US_TeamNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_TeamNo)));
			this.teamNoDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|6dfd79f6-968a-4603-8c6a-b21dfe152e48", "Team #");
			this.teamNoDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 123, true);
			this.teamNoDropEdit.Name = "teamNoDropEdit";
			this.teamNoDropEdit.PreBoundMaxLength = 2;
			this.teamNoDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 17, true);
			this.teamNoDropEdit.TabIndex = 5;
			// 
			// IssueCodeDropEdit
			// 
			this.IssueCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IssueCodeDropEdit, "US_IssueCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_IssueCode)));
			this.IssueCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|eae4a48c-ccc8-43e8-9825-e42a35364daf", "Issue Code");
			this.IssueCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 178, true);
			this.IssueCodeDropEdit.Name = "IssueCodeDropEdit";
			this.IssueCodeDropEdit.PreBoundMaxLength = 2;
			this.IssueCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 17, true);
			this.IssueCodeDropEdit.TabIndex = 7;
			// 
			// AggregateIndicatorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AggregateIndicatorCheckBox, "US_IsAggregate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_IsAggregate)));
			this.AggregateIndicatorCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|28ce3b02-1b78-4978-a31e-c768009b1302", "Is Aggregate?");
			this.AggregateIndicatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AggregateIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AggregateIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 205, true);
			this.AggregateIndicatorCheckBox.Name = "AggregateIndicatorCheckBox";
			this.AggregateIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.AggregateIndicatorCheckBox.TabIndex = 9;
			this.AggregateIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// CommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommentTextBox, "US_Comment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_Comment)));
			this.CommentTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("3c218e94-2c11-4c77-8dc4-3e02bca44eb4", "Comment");
			this.CommentTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 259, true);
			this.CommentTextBox.Multiline = true;
			this.CommentTextBox.Name = "CommentTextBox";
			this.CommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.CommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 65, true);
			this.CommentTextBox.TabIndex = 14;
			// 
			// US_SchDEntryDropEdit
			// 
			this.US_SchDEntryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.US_SchDEntryCodeFindBox, "US_SchDEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_SchDEntry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).Lookups.SchDPortList)));
			this.US_SchDEntryCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|88c93335-d07b-4bf8-96b1-571bef8b91dc", "Filing Port");
			this.US_SchDEntryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 123, true);
			this.US_SchDEntryCodeFindBox.Name = "US_SchDEntryDropEdit";
			this.US_SchDEntryCodeFindBox.PreBoundMaxLength = 4;
			this.US_SchDEntryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 17, true);
			this.US_SchDEntryCodeFindBox.TabIndex = 4;
			// 
			// ImportEntrySourceDropEdit
			// 
			this.ImportEntrySourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportEntrySourceDropEdit, "US_ImportEntrySource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_ImportEntrySource)));
			this.ImportEntrySourceDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|d38c22e5-1269-4452-afc9-4c0daae4caf2", "Import Source");
			this.ImportEntrySourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 232, true);
			this.ImportEntrySourceDropEdit.Name = "ImportEntrySourceDropEdit";
			this.ImportEntrySourceDropEdit.PreBoundMaxLength = 2;
			this.ImportEntrySourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 17, true);
			this.ImportEntrySourceDropEdit.TabIndex = 12;
			// 
			// PaymentGroupBox
			// 
			this.PaymentGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a977be34-49e0-455d-a18f-05945c048d61", "Payment");
			this.PaymentGroupBox.Controls.Add(this.PaymentTypeDropEdit);
			this.PaymentGroupBox.Controls.Add(this.BrokerToPayDropEdit);
			this.PaymentGroupBox.Controls.Add(this.StatementPrintDateDateEdit);
			this.PaymentGroupBox.Controls.Add(this.ClientBranchDesignationTextBox);
			this.PaymentGroupBox.Controls.Add(this.JE_GBGuidFindBox);
			this.PaymentGroupBox.Controls.Add(this.BrokerCodeFindBox);
			this.PaymentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 15, true);
			this.PaymentGroupBox.Name = "PaymentGroupBox";
			this.PaymentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 137, true);
			this.PaymentGroupBox.TabIndex = 3;
			this.PaymentGroupBox.TabStop = false;
			// 
			// PaymentTypeDropEdit
			// 
			this.PaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentTypeDropEdit, "US_PaymentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_PaymentType)));
			this.PaymentTypeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|58378ec6-5881-4818-975e-0f3f4655efa7", "Payment Type");
			this.PaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 14, true);
			this.PaymentTypeDropEdit.Name = "PaymentTypeDropEdit";
			this.PaymentTypeDropEdit.PreBoundMaxLength = 1;
			this.PaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 17, true);
			this.PaymentTypeDropEdit.TabIndex = 0;
			// 
			// BrokerToPayDropEdit
			// 
			this.BrokerToPayDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerToPayDropEdit, "BrokerToPayIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).BrokerToPayIndicator)));
			this.BrokerToPayDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|69e2ba45-d481-4878-a4fe-5e05f02b4b15", "Broker To Pay");
			this.BrokerToPayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 39, true);
			this.BrokerToPayDropEdit.Name = "BrokerToPayDropEdit";
			this.BrokerToPayDropEdit.PreBoundMaxLength = 2;
			this.BrokerToPayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.BrokerToPayDropEdit.TabIndex = 1;
			// 
			// StatementPrintDateDateEdit
			// 
			this.StatementPrintDateDateEdit.AllowDrop = true;
			this.StatementPrintDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.StatementPrintDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StatementPrintDateDateEdit, "US_PreliminaryStatementPrintDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_PreliminaryStatementPrintDate)));
			this.StatementPrintDateDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|12b9e99c-7069-4510-af11-3eb2d7ad7724", "Statement Print Date");
			this.StatementPrintDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 64, true);
			this.StatementPrintDateDateEdit.Name = "StatementPrintDateDateEdit";
			this.StatementPrintDateDateEdit.TabIndex = 2;
			// 
			// ClientBranchDesignationTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientBranchDesignationTextBox, "US_ClientBranchDesignation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_ClientBranchDesignation)));
			this.ClientBranchDesignationTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|0dbcb728-1a11-431a-84d1-5a4817188eb5", "Client Branch Designation");
			this.ClientBranchDesignationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 88, true);
			this.ClientBranchDesignationTextBox.Name = "ClientBranchDesignationTextBox";
			this.ClientBranchDesignationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 17, true);
			this.ClientBranchDesignationTextBox.TabIndex = 4;
			// 
			// JE_GBGuidFindBox
			// 
			this.JE_GBGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JE_GBGuidFindBox, "JE_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).JE_GB)));
			this.JE_GBGuidFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|ff43c047-08cc-4e98-9115-e8b0e2f838b9", "Branch");
			this.JE_GBGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 88, true);
			this.JE_GBGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			this.JE_GBGuidFindBox.Name = "JE_GBGuidFindBox";
			this.JE_GBGuidFindBox.PreBoundMaxLength = 3;
			this.JE_GBGuidFindBox.ShowDescriptionBox = false;
			this.JE_GBGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 17, true);
			this.JE_GBGuidFindBox.TabIndex = 3;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "JE_GS_NKCusAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).JE_GS_NKCusAgent)));
			this.BrokerCodeFindBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|a0e405b5-080c-4bee-a2ee-e612fb5ddf2f", "Preparer");
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 111, true);
			this.BrokerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.PreBoundMaxLength = 3;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 17, true);
			this.BrokerCodeFindBox.TabIndex = 5;
			// 
			// summaryGroupBox
			// 
			this.summaryGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|027ecdbd-53bf-48c7-8666-dfee5c9b6ac6", "Summary");
			this.summaryGroupBox.Controls.Add(this.reconInterestCalcEdit);
			this.summaryGroupBox.Controls.Add(this.OriginalTaxCalcEdit);
			this.summaryGroupBox.Controls.Add(this.differenceTaxCalcEdit);
			this.summaryGroupBox.Controls.Add(this.reconTaxCalcEdit);
			this.summaryGroupBox.Controls.Add(this.OriginalFeesCalcEdit);
			this.summaryGroupBox.Controls.Add(this.differenceFeesCalcEdit);
			this.summaryGroupBox.Controls.Add(this.reconFeesCalcEdit);
			this.summaryGroupBox.Controls.Add(this.originalDutyCalcEdit);
			this.summaryGroupBox.Controls.Add(this.differenceDutyCalcEdit);
			this.summaryGroupBox.Controls.Add(this.reconDutyCalcEdit);
			this.summaryGroupBox.Controls.Add(this.differenceLabel);
			this.summaryGroupBox.Controls.Add(this.summaryLabel);
			this.summaryGroupBox.Controls.Add(this.originalLabel);
			this.summaryGroupBox.Controls.Add(this.reconLabel);
			this.summaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 209, true);
			this.summaryGroupBox.Name = "summaryGroupBox";
			this.summaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 180, true);
			this.summaryGroupBox.TabIndex = 5;
			this.summaryGroupBox.TabStop = false;
			// 
			// reconInterestCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.reconInterestCalcEdit, "InterestPaymentAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).InterestPaymentAmount)));
			this.reconInterestCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|ae014442-eadd-4724-897b-4676d95fc64d", "Interest Payable");
			this.reconInterestCalcEdit.DecimalPlaces = 2;
			this.reconInterestCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 107, true);
			this.reconInterestCalcEdit.Name = "reconInterestCalcEdit";
			this.reconInterestCalcEdit.ReadOnly = true;
			this.reconInterestCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.reconInterestCalcEdit.TabIndex = 12;
			this.reconInterestCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OriginalTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OriginalTaxCalcEdit, "TotalOriginalTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).TotalOriginalTax)));
			this.OriginalTaxCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|de772755-ebd0-46a2-9a46-1fef9325078a", "Tax");
			this.OriginalTaxCalcEdit.DecimalPlaces = 2;
			this.OriginalTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 83, true);
			this.OriginalTaxCalcEdit.Name = "OriginalTaxCalcEdit";
			this.OriginalTaxCalcEdit.ReadOnly = true;
			this.OriginalTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.OriginalTaxCalcEdit.TabIndex = 9;
			this.OriginalTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// differenceTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.differenceTaxCalcEdit, "TotalTaxDifference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).TotalTaxDifference)));
			this.differenceTaxCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.differenceTaxCalcEdit, false);
			this.differenceTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 83, true);
			this.differenceTaxCalcEdit.Name = "differenceTaxCalcEdit";
			this.differenceTaxCalcEdit.ReadOnly = true;
			this.differenceTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.differenceTaxCalcEdit.TabIndex = 11;
			this.differenceTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// reconTaxCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.reconTaxCalcEdit, "TotalReconTax");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).TotalReconTax)));
			this.reconTaxCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.reconTaxCalcEdit, false);
			this.reconTaxCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 83, true);
			this.reconTaxCalcEdit.Name = "reconTaxCalcEdit";
			this.reconTaxCalcEdit.ReadOnly = true;
			this.reconTaxCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.reconTaxCalcEdit.TabIndex = 10;
			this.reconTaxCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OriginalFeesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OriginalFeesCalcEdit, "TotalOriginalFee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).TotalOriginalFee)));
			this.OriginalFeesCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|bab75a63-7bbd-4c70-a1ff-79f11fc10a8c", "Fees");
			this.OriginalFeesCalcEdit.DecimalPlaces = 2;
			this.OriginalFeesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 59, true);
			this.OriginalFeesCalcEdit.Name = "OriginalFeesCalcEdit";
			this.OriginalFeesCalcEdit.ReadOnly = true;
			this.OriginalFeesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.OriginalFeesCalcEdit.TabIndex = 6;
			this.OriginalFeesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// differenceFeesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.differenceFeesCalcEdit, "TotalFeeDifference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).TotalFeeDifference)));
			this.differenceFeesCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.differenceFeesCalcEdit, false);
			this.differenceFeesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 59, true);
			this.differenceFeesCalcEdit.Name = "differenceFeesCalcEdit";
			this.differenceFeesCalcEdit.ReadOnly = true;
			this.differenceFeesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.differenceFeesCalcEdit.TabIndex = 8;
			this.differenceFeesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// reconFeesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.reconFeesCalcEdit, "TotalReconFee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).TotalReconFee)));
			this.reconFeesCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.reconFeesCalcEdit, false);
			this.reconFeesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 59, true);
			this.reconFeesCalcEdit.Name = "reconFeesCalcEdit";
			this.reconFeesCalcEdit.ReadOnly = true;
			this.reconFeesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.reconFeesCalcEdit.TabIndex = 7;
			this.reconFeesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// originalDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.originalDutyCalcEdit, "TotalOriginalDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).TotalOriginalDuty)));
			this.originalDutyCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|83b25ec7-e18f-4fb7-857a-d7d5af929819", "Duty");
			this.originalDutyCalcEdit.DecimalPlaces = 2;
			this.originalDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 35, true);
			this.originalDutyCalcEdit.Name = "originalDutyCalcEdit";
			this.originalDutyCalcEdit.ReadOnly = true;
			this.originalDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.originalDutyCalcEdit.TabIndex = 3;
			this.originalDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// differenceDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.differenceDutyCalcEdit, "TotalDutyDifference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).TotalDutyDifference)));
			this.differenceDutyCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.differenceDutyCalcEdit, false);
			this.differenceDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 35, true);
			this.differenceDutyCalcEdit.Name = "differenceDutyCalcEdit";
			this.differenceDutyCalcEdit.ReadOnly = true;
			this.differenceDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.differenceDutyCalcEdit.TabIndex = 5;
			this.differenceDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// reconDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.reconDutyCalcEdit, "TotalReconDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).TotalReconDuty)));
			this.reconDutyCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.reconDutyCalcEdit, false);
			this.reconDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 35, true);
			this.reconDutyCalcEdit.Name = "reconDutyCalcEdit";
			this.reconDutyCalcEdit.ReadOnly = true;
			this.reconDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.reconDutyCalcEdit.TabIndex = 4;
			this.reconDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// differenceLabel
			// 
			this.differenceLabel.AutoSize = true;
			this.differenceLabel.IsFontBold = true;
			this.differenceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 16, true);
			this.differenceLabel.Name = "differenceLabel";
			this.differenceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 13, true);
			this.differenceLabel.TabIndex = 2;
			this.differenceLabel.Text = "Difference";
			// 
			// summaryLabel
			// 
			this.summaryLabel.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|d503cba5-104b-4ddc-a735-baa736ea620f", "Negative difference indicates that the Recon. amounts are less than the original amounts");
			this.summaryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 133, true);
			this.summaryLabel.Name = "summaryLabel";
			this.summaryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 36, true);
			this.summaryLabel.TabIndex = 13;
			// 
			// originalLabel
			// 
			this.originalLabel.AutoSize = true;
			this.originalLabel.IsFontBold = true;
			this.originalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 16, true);
			this.originalLabel.Name = "originalLabel";
			this.originalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 13, true);
			this.originalLabel.TabIndex = 0;
			this.originalLabel.Text = "Original";
			// 
			// reconLabel
			// 
			this.reconLabel.AutoSize = true;
			this.reconLabel.IsFontBold = true;
			this.reconLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 15, true);
			this.reconLabel.Name = "reconLabel";
			this.reconLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 13, true);
			this.reconLabel.TabIndex = 1;
			this.reconLabel.Text = "Recon";
			// 
			// ReconEntryDetailsGroupBox
			// 
			this.ReconEntryDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|29a10d75-cd2a-42de-9d01-77f2cd382c03", "Status");
			this.ReconEntryDetailsGroupBox.Controls.Add(this.AllocateButton);
			this.ReconEntryDetailsGroupBox.Controls.Add(this.EntryNumberTextBox);
			this.ReconEntryDetailsGroupBox.Controls.Add(this.EntryNumberDividerLabel);
			this.ReconEntryDetailsGroupBox.Controls.Add(this.FilerCodeZTextBox);
			this.ReconEntryDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(534, 155, true);
			this.ReconEntryDetailsGroupBox.Name = "ReconEntryDetailsGroupBox";
			this.ReconEntryDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 51, true);
			this.ReconEntryDetailsGroupBox.TabIndex = 4;
			this.ReconEntryDetailsGroupBox.TabStop = false;
			// 
			// AllocateButton
			// 
			this.AllocateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 17, true);
			this.AllocateButton.Name = "AllocateButton";
			this.AllocateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AllocateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AllocateButton.TabIndex = 3;
			this.AllocateButton.Text = "Allocate";
			this.AllocateButton.UseVisualStyleBackColor = true;
			this.AllocateButton.Click += new System.EventHandler(this.AllocateButton_Click);
			// 
			// EntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "ReconEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).ReconEntryNumber)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EntryNumberTextBox, false);
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 19, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 17, true);
			this.EntryNumberTextBox.TabIndex = 2;
			this.EntryNumberTextBox.Text = "888";
			// 
			// EntryNumberDividerLabel
			// 
			this.EntryNumberDividerLabel.AutoSize = true;
			this.EntryNumberDividerLabel.IsFontBold = true;
			this.EntryNumberDividerLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 22, true);
			this.EntryNumberDividerLabel.Name = "EntryNumberDividerLabel";
			this.EntryNumberDividerLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(11, 13, true);
			this.EntryNumberDividerLabel.TabIndex = 1;
			this.EntryNumberDividerLabel.Text = "-";
			// 
			// FilerCodeZTextBox
			// 
			this.BindingSource.SetBindingMember(this.FilerCodeZTextBox, "US_EntryFilerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).US_EntryFilerCode)));
			this.FilerCodeZTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|9205505a-4e31-4805-a2ce-69bf32d92452", "Entry #");
			this.FilerCodeZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 19, true);
			this.FilerCodeZTextBox.Name = "FilerCodeZTextBox";
			this.FilerCodeZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 17, true);
			this.FilerCodeZTextBox.TabIndex = 0;
			this.FilerCodeZTextBox.Text = "888";
			// 
			// ImporterZDocAddressControl
			// 
			this.ImporterZDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterZDocAddressControl, "ImporterAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.Business.ReconDeclaration)(null)).ImporterAddress)));
			this.ImporterZDocAddressControl.BindToOrganisations = "Lookups.ImporterList";
			this.ImporterZDocAddressControl.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Importer");
			this.ImporterZDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 14, true);
			this.ImporterZDocAddressControl.Name = "ImporterZDocAddressControl";
			this.ImporterZDocAddressControl.ReadOnly = false;
			this.ImporterZDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ImporterZDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ImporterZDocAddressControl.TabIndex = 0;
			this.ImporterZDocAddressControl.ValidationJustForced = false;
			// 
			// CustomFieldsGroupBox
			// 
			this.CustomFieldsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ReconHeaderUserControl|1476c9be-a612-4d1b-ab6d-3e29000c80d9", "Custom");
			this.CustomFieldsGroupBox.Controls.Add(this.CustomFieldsDisplayControl);
			this.CustomFieldsGroupBox.Name = "CustomFieldsGroupBox";
			this.CustomFieldsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(964, 15, true);
			this.CustomFieldsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(372, 530, true);
			this.CustomFieldsGroupBox.TabIndex = 14;
			this.CustomFieldsGroupBox.TabStop = false;
			// 
			// CustomFieldsDisplayControl
			// 
			this.CustomFieldsDisplayControl.AllowDrop = true;
			this.CustomFieldsDisplayControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsDisplayControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.CustomFieldsDisplayControl.Name = "shipmentCustomFieldsControl1";
			this.CustomFieldsDisplayControl.NothingSetupMessageLabelText = "To make use of this area, please setup Recon customized fields against Workflow Templates.";
			this.CustomFieldsDisplayControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 520, true);
			this.CustomFieldsDisplayControl.TabIndex = 0;
			// 
			// ReconHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ReconHeaderDetailsGroupBox);
			this.Name = "ReconHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReconHeaderDetailsGroupBox.ResumeLayout(false);
			this.ReconHeaderDetailsGroupBox.PerformLayout();
			this.JobDocAddressTabControl.ResumeLayout(false);
			this.JobDocAddressTabControl.PerformLayout();
			this.DocRecipientTabPage.ResumeLayout(false);
			this.DocRecipientTabPage.PerformLayout();
			this.DocRecipientPanel1.ResumeLayout(false);
			this.DocRecipientPanel1.PerformLayout();
			this.DocRecipientPanel2.ResumeLayout(false);
			this.DocRecipientPanel2.PerformLayout();
			this.DocProvidedDateEdit.ResumeLayout(true);
			this.DocProvidedDateEdit.PerformLayout();
			this.DocRecipientDocAddressControl.ResumeLayout(true);
			this.DocRecipientDocAddressControl.PerformLayout();
			this.ClaimantTabPage.ResumeLayout(false);
			this.ClaimantTabPage.PerformLayout();
			this.ClaimantPanel1.ResumeLayout(false);
			this.ClaimantPanel1.PerformLayout();
			this.ClaimantPanel2.ResumeLayout(false);
			this.ClaimantPanel2.PerformLayout();
			this.ClaimantDateEdit.ResumeLayout(true);
			this.ClaimantDateEdit.PerformLayout();
			this.ClaimantDocAddressControl.ResumeLayout(true);
			this.ClaimantDocAddressControl.PerformLayout();
			this.AggregateFeesGroupBox.ResumeLayout(false);
			this.AggregateFeesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AggregateFeesGrid)).EndInit();
			this.AggregateFeesGrid.ResumeLayout(false);
			this.AggregateFeesGrid.PerformLayout();
			this.MainDetailsGroupBox.ResumeLayout(false);
			this.MainDetailsGroupBox.PerformLayout();
			this.BrokerFindBox.ResumeLayout(true);
			this.BrokerFindBox.PerformLayout();
			this.NotifyPartyGuidFindBox.ResumeLayout(true);
			this.NotifyPartyGuidFindBox.PerformLayout();
			this.ApplicationCodeDropEdit.ResumeLayout(true);
			this.ApplicationCodeDropEdit.PerformLayout();
			this.JE_RS_NKServiceLevelFindBox.ResumeLayout(true);
			this.JE_RS_NKServiceLevelFindBox.PerformLayout();
			this.US_OH_IORGuidFindBox.ResumeLayout(true);
			this.US_OH_IORGuidFindBox.PerformLayout();
			this.SuretyCodeDropEdit.ResumeLayout(true);
			this.SuretyCodeDropEdit.PerformLayout();
			this.EstimatedReconEntryDateDateEdit.ResumeLayout(true);
			this.EstimatedReconEntryDateDateEdit.PerformLayout();
			this.teamNoDropEdit.ResumeLayout(true);
			this.teamNoDropEdit.PerformLayout();
			this.IssueCodeDropEdit.ResumeLayout(true);
			this.IssueCodeDropEdit.PerformLayout();
			this.US_SchDEntryCodeFindBox.ResumeLayout(true);
			this.US_SchDEntryCodeFindBox.PerformLayout();
			this.ImportEntrySourceDropEdit.ResumeLayout(true);
			this.ImportEntrySourceDropEdit.PerformLayout();
			this.PaymentGroupBox.ResumeLayout(false);
			this.PaymentGroupBox.PerformLayout();
			this.PaymentTypeDropEdit.ResumeLayout(true);
			this.PaymentTypeDropEdit.PerformLayout();
			this.BrokerToPayDropEdit.ResumeLayout(true);
			this.BrokerToPayDropEdit.PerformLayout();
			this.StatementPrintDateDateEdit.ResumeLayout(true);
			this.StatementPrintDateDateEdit.PerformLayout();
			this.JE_GBGuidFindBox.ResumeLayout(true);
			this.JE_GBGuidFindBox.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.summaryGroupBox.ResumeLayout(false);
			this.summaryGroupBox.PerformLayout();
			this.ReconEntryDetailsGroupBox.ResumeLayout(false);
			this.ReconEntryDetailsGroupBox.PerformLayout();
			this.ImporterZDocAddressControl.ResumeLayout(true);
			this.ImporterZDocAddressControl.PerformLayout();
			this.CustomFieldsGroupBox.ResumeLayout(false);
			this.CustomFieldsGroupBox.PerformLayout();
			this.CustomFieldsDisplayControl.ResumeLayout(true);
			this.CustomFieldsDisplayControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZGroupBox ReconHeaderDetailsGroupBox;
		ZCheckBox AggregateIndicatorCheckBox;
		ZDropEdit IssueCodeDropEdit;
		ZDateEdit EstimatedReconEntryDateDateEdit;
		ZDropEdit SuretyCodeDropEdit;
		ZTextBox CommentTextBox;
		ZDropEdit PaymentTypeDropEdit;
		ZDateEdit StatementPrintDateDateEdit;
		ZTextBox ClientBranchDesignationTextBox;
		ZGuidFindBox JE_GBGuidFindBox;
		internal ZDocAddressControl ImporterZDocAddressControl;
		ZDropEdit ImportEntrySourceDropEdit;
		protected internal ZCodeFindBox BrokerCodeFindBox;
		ZOrganisationFindBox US_OH_IORGuidFindBox;
		ZCodeFindBox US_SchDEntryCodeFindBox;
		ZGroupBox ReconEntryDetailsGroupBox;
		ZTextBox FilerCodeZTextBox;
		ZDropEdit teamNoDropEdit;
		ZGroupBox summaryGroupBox;
		ZLabel originalLabel;
		ZLabel reconLabel;
		ZLabel differenceLabel;
		ZLabel summaryLabel;
		ZCalcEdit reconInterestCalcEdit;
		ZCalcEdit OriginalTaxCalcEdit;
		ZCalcEdit differenceTaxCalcEdit;
		ZCalcEdit reconTaxCalcEdit;
		ZCalcEdit OriginalFeesCalcEdit;
		ZCalcEdit differenceFeesCalcEdit;
		ZCalcEdit reconFeesCalcEdit;
		ZCalcEdit originalDutyCalcEdit;
		ZCalcEdit differenceDutyCalcEdit;
		ZCalcEdit reconDutyCalcEdit;
		ZDropEdit BrokerToPayDropEdit;
		ZLabel EntryNumberDividerLabel;
		ZTextBox EntryNumberTextBox;
		ZButton AllocateButton;
		ZGroupBox PaymentGroupBox;
		ZGroupBox MainDetailsGroupBox;
		internal ZCheckBox NoChangeAggregateCheckBox;
		internal ZGroupBox AggregateFeesGroupBox;
		ZGrid AggregateFeesGrid;
		internal ZCheckBox WaiveCheckBox;
		protected ZCodeFindBox JE_RS_NKServiceLevelFindBox;
		ZDropEdit ApplicationCodeDropEdit;
		ZOrganisationFindBox NotifyPartyGuidFindBox;
		ZCodeFindBox BrokerFindBox;
		ZTabControl JobDocAddressTabControl;
		ZTabPage DocRecipientTabPage;
		ZTabPage ClaimantTabPage;
		ZPanel DocRecipientPanel1;
		internal ZDocAddressControl DocRecipientDocAddressControl;
		ZPanel DocRecipientPanel2;
		ZPanel ClaimantPanel1;
		ZPanel ClaimantPanel2;
		internal ZDocAddressControl ClaimantDocAddressControl;
		ZDateEdit DocProvidedDateEdit;
		ZTextBox ClaimIDTextBox;
		ZDateEdit ClaimantDateEdit;
		internal ZLabel QualifyingGoodsFTADecLabel;
		private ZArchitecture.GUI.ZGroupBox CustomFieldsGroupBox;
		private ZArchitecture.GUI.ProcessTemplateCustomFieldsControl CustomFieldsDisplayControl;
	}
}
