namespace Enterprise.MasterFiles.GUI
{
	partial class CommissionAgreementControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.detailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsApprovedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.detailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.commissionAgreementItemsControl = new Enterprise.MasterFiles.GUI.CommissionAgreementItemsControl();
			this.CA0_CommissionStreamDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.StatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StatusDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EffectiveDateDashLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CA0_ExpiredDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CA0_OH_CustomerGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.EffectiveDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CA0_CommissionTriggerTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CA0_CommissionBasisDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.recipientsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.recipientsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.RecipientsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ratesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CAR_EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RatesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.detailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.detailsSplitContainer)).BeginInit();
			this.detailsSplitContainer.Panel1.SuspendLayout();
			this.detailsSplitContainer.Panel2.SuspendLayout();
			this.detailsSplitContainer.SuspendLayout();
			this.commissionAgreementItemsControl.SuspendLayout();
			this.CA0_CommissionStreamDropEdit.SuspendLayout();
			this.CA0_ExpiredDateDateEdit.SuspendLayout();
			this.CA0_OH_CustomerGuidDropEdit.SuspendLayout();
			this.EffectiveDateDateEdit.SuspendLayout();
			this.CA0_CommissionTriggerTypeDropEdit.SuspendLayout();
			this.CA0_CommissionBasisDropEdit.SuspendLayout();
			this.recipientsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.recipientsSplitContainer)).BeginInit();
			this.recipientsSplitContainer.Panel1.SuspendLayout();
			this.recipientsSplitContainer.Panel2.SuspendLayout();
			this.recipientsSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).BeginInit();
			this.RecipientsGrid.SuspendLayout();
			this.ratesGroupBox.SuspendLayout();
			this.CAR_EndDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RatesGrid)).BeginInit();
			this.RatesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgCommissionAgreement);
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.detailsGroupBox);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 250, true);
			this.mainSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(130);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.recipientsGroupBox);
			this.mainSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(64);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(130);
			this.mainSplitContainer.TabIndex = 0;
			// 
			// detailsGroupBox
			// 
			this.detailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3e6d87e9-f76e-43db-a0fe-b1dbc2e923fa", "Details");
			this.detailsGroupBox.Controls.Add(this.IsApprovedCheckBox);
			this.detailsGroupBox.Controls.Add(this.detailsSplitContainer);
			this.detailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsGroupBox.Name = "detailsGroupBox";
			this.detailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 130, true);
			this.detailsGroupBox.TabIndex = 0;
			this.detailsGroupBox.TabStop = false;
			// 
			// IsApprovedCheckBox
			// 
			this.IsApprovedCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.IsApprovedCheckBox, "IsApproved");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).IsApproved)));
			this.IsApprovedCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b877fd46-dc52-4e99-b6bc-2fb1f861d909", "Approved");
			this.IsApprovedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsApprovedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsApprovedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(660, 0, true);
			this.IsApprovedCheckBox.Name = "IsApprovedCheckBox";
			this.IsApprovedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 17, true);
			this.IsApprovedCheckBox.TabIndex = 0;
			// 
			// detailsSplitContainer
			// 
			this.detailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.detailsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.detailsSplitContainer.IsSplitterFixed = true;
			this.detailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.detailsSplitContainer.Name = "detailsSplitContainer";
			// 
			// detailsSplitContainer.Panel1
			// 
			this.detailsSplitContainer.Panel1.Controls.Add(this.commissionAgreementItemsControl);
			// 
			// detailsSplitContainer.Panel2
			// 
			this.detailsSplitContainer.Panel2.Controls.Add(this.CA0_CommissionStreamDropEdit);
			this.detailsSplitContainer.Panel2.Controls.Add(this.StatusLabel);
			this.detailsSplitContainer.Panel2.Controls.Add(this.StatusDescriptionLabel);
			this.detailsSplitContainer.Panel2.Controls.Add(this.EffectiveDateDashLabel);
			this.detailsSplitContainer.Panel2.Controls.Add(this.CA0_ExpiredDateDateEdit);
			this.detailsSplitContainer.Panel2.Controls.Add(this.CA0_OH_CustomerGuidDropEdit);
			this.detailsSplitContainer.Panel2.Controls.Add(this.EffectiveDateDateEdit);
			this.detailsSplitContainer.Panel2.Controls.Add(this.CA0_CommissionTriggerTypeDropEdit);
			this.detailsSplitContainer.Panel2.Controls.Add(this.CA0_CommissionBasisDropEdit);
			this.detailsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(694, 111, true);
			this.detailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(418);
			this.detailsSplitContainer.TabIndex = 1;
			// 
			// commissionAgreementItemsControl
			// 
			this.commissionAgreementItemsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.commissionAgreementItemsControl, ".");
			this.commissionAgreementItemsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commissionAgreementItemsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.commissionAgreementItemsControl.Name = "commissionAgreementItemsControl";
			this.commissionAgreementItemsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 111, true);
			this.commissionAgreementItemsControl.TabIndex = 0;
			// 
			// CA0_CommissionStreamDropEdit
			// 
			this.CA0_CommissionStreamDropEdit.AllowDrop = true;
			this.CA0_CommissionStreamDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CA0_CommissionStreamDropEdit, "CA0_CommissionStream");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).CA0_CommissionStream)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).CA0_CommissionStreamDescription)));
			this.CA0_CommissionStreamDropEdit.BindToForDescription = "CA0_CommissionStreamDescription";
			this.CA0_CommissionStreamDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 90, true);
			this.CA0_CommissionStreamDropEdit.Name = "CA0_CommissionStreamDropEdit";
			this.CA0_CommissionStreamDropEdit.PreBoundMaxLength = 3;
			this.CA0_CommissionStreamDropEdit.ShowDescriptionBox = false;
			this.CA0_CommissionStreamDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CA0_CommissionStreamDropEdit.TabIndex = 8;
			// 
			// StatusLabel
			// 
			this.StatusLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7f5874a9-9fc9-40ba-af0a-12a933d04a9e", "Status:");
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 66, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.StatusLabel.TabIndex = 7;
			this.StatusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// StatusDescriptionLabel
			// 
			this.StatusDescriptionLabel.BackColor = System.Drawing.Color.Yellow;
			this.BindingSource.SetBindingMember(this.StatusDescriptionLabel, "StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).StatusDescription)));
			this.StatusDescriptionLabel.IsFontBold = true;
			this.StatusDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 66, true);
			this.StatusDescriptionLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.StatusDescriptionLabel.Name = "StatusDescriptionLabel";
			this.StatusDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.StatusDescriptionLabel.TabIndex = 5;
			this.StatusDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// EffectiveDateDashLabel
			// 
			this.EffectiveDateDashLabel.AutoSize = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EffectiveDateDashLabel, false);
			this.EffectiveDateDashLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 47, true);
			this.EffectiveDateDashLabel.Name = "EffectiveDateDashLabel";
			this.EffectiveDateDashLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 13, true);
			this.EffectiveDateDashLabel.TabIndex = 3;
			this.EffectiveDateDashLabel.Text = "-";
			// 
			// CA0_ExpiredDateDateEdit
			// 
			this.CA0_ExpiredDateDateEdit.AllowDrop = true;
			this.CA0_ExpiredDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.CA0_ExpiredDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CA0_ExpiredDateDateEdit, "CA0_ExpiredDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).CA0_ExpiredDate)));
			this.CA0_ExpiredDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 45, true);
			this.CA0_ExpiredDateDateEdit.Name = "CA0_ExpiredDateDateEdit";
			this.CA0_ExpiredDateDateEdit.TabIndex = 4;
			// 
			// CA0_OH_CustomerGuidDropEdit
			// 
			this.CA0_OH_CustomerGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA0_OH_CustomerGuidDropEdit, "CA0_OH_Customer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).CA0_OH_Customer)));
			this.CA0_OH_CustomerGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 0, true);
			this.CA0_OH_CustomerGuidDropEdit.Name = "CA0_OH_CustomerGuidDropEdit";
			this.CA0_OH_CustomerGuidDropEdit.ShowDescriptionBox = false;
			this.CA0_OH_CustomerGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.CA0_OH_CustomerGuidDropEdit.TabIndex = 0;
			// 
			// EffectiveDateDateEdit
			// 
			this.EffectiveDateDateEdit.AllowDrop = true;
			this.EffectiveDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.EffectiveDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EffectiveDateDateEdit, "EffectiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).EffectiveDate)));
			this.EffectiveDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 44, true);
			this.EffectiveDateDateEdit.Name = "EffectiveDateDateEdit";
			this.EffectiveDateDateEdit.TabIndex = 3;
			// 
			// CA0_CommissionTriggerTypeDropEdit
			// 
			this.CA0_CommissionTriggerTypeDropEdit.AllowDrop = true;
			this.CA0_CommissionTriggerTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CA0_CommissionTriggerTypeDropEdit, "CA0_CommissionTriggerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).CA0_CommissionTriggerType)));
			this.CA0_CommissionTriggerTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 22, true);
			this.CA0_CommissionTriggerTypeDropEdit.Name = "CA0_CommissionTriggerTypeDropEdit";
			this.CA0_CommissionTriggerTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.CA0_CommissionTriggerTypeDropEdit.TabIndex = 2;
			// 
			// CA0_CommissionBasisDropEdit
			// 
			this.CA0_CommissionBasisDropEdit.AllowDrop = true;
			this.CA0_CommissionBasisDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CA0_CommissionBasisDropEdit, "CA0_CommissionBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).CA0_CommissionBasis)));
			this.CA0_CommissionBasisDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 90, true);
			this.CA0_CommissionBasisDropEdit.Name = "CA0_CommissionBasisDropEdit";
			this.CA0_CommissionBasisDropEdit.PreBoundMaxLength = 3;
			this.CA0_CommissionBasisDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CA0_CommissionBasisDropEdit.TabIndex = 6;
			// 
			// recipientsGroupBox
			// 
			this.recipientsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8958b77c-19d3-41c2-98d1-8bcd8b87ea55", "Wolf Pack");
			this.recipientsGroupBox.Controls.Add(this.recipientsSplitContainer);
			this.recipientsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.recipientsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.recipientsGroupBox.Name = "recipientsGroupBox";
			this.recipientsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 116, true);
			this.recipientsGroupBox.TabIndex = 0;
			this.recipientsGroupBox.TabStop = false;
			// 
			// recipientsSplitContainer
			// 
			this.recipientsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.recipientsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.recipientsSplitContainer.Name = "recipientsSplitContainer";
			// 
			// recipientsSplitContainer.Panel1
			// 
			this.recipientsSplitContainer.Panel1.Controls.Add(this.RecipientsGrid);
			this.recipientsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(694, 97, true);
			this.recipientsSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			// 
			// recipientsSplitContainer.Panel2
			// 
			this.recipientsSplitContainer.Panel2.Controls.Add(this.ratesGroupBox);
			this.recipientsSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.recipientsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(296);
			this.recipientsSplitContainer.TabIndex = 1;
			// 
			// RecipientsGrid
			// 
			this.RecipientsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RecipientsGrid, "Recipients");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).CAR_GS_NKStaff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).CAR_OH_Party)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).CAR_CommissionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).CAR_Share)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).SharePercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).CAR_Comment)));
			this.RecipientsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CAR_GS_NKStaff";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CAR_OH_Party";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo1.ColumnName = "CAR_CommissionType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CAR_Share";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "SharePercentage";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zMultiLineTextBoxColumnInfo1.ColumnName = "CAR_Comment";
			zMultiLineTextBoxColumnInfo1.IsVisible = false;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.RecipientsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RecipientsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RecipientsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.RecipientsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RecipientsGrid.GridId = "08c2e3aa-6d48-48c5-9d05-83add7b73ce0";
			this.RecipientsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RecipientsGrid.LayoutKey = "recipientsGrid";
			this.RecipientsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RecipientsGrid.Name = "RecipientsGrid";
			this.RecipientsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 97, true);
			this.RecipientsGrid.TabIndex = 0;
			// 
			// ratesGroupBox
			// 
			this.ratesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4380fbb8-4096-4caf-87da-d34b13a51482", "Rates");
			this.ratesGroupBox.Controls.Add(this.CAR_EndDateEdit);
			this.ratesGroupBox.Controls.Add(this.RatesGrid);
			this.ratesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ratesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ratesGroupBox.Name = "ratesGroupBox";
			this.ratesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 97, true);
			this.ratesGroupBox.TabIndex = 0;
			this.ratesGroupBox.TabStop = false;
			// 
			// CAR_EndDateEdit
			// 
			this.CAR_EndDateEdit.AllowDrop = true;
			this.CAR_EndDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CAR_EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.CAR_EndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CAR_EndDateEdit, "Recipients.CAR_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).CAR_EndDate)));
			this.CAR_EndDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("231eb6f5-3bc2-4af4-9580-61c478fc8e3f", "Entity Entitlement End Date");
			this.CAR_EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 71, true);
			this.CAR_EndDateEdit.Name = "CAR_EndDateEdit";
			this.CAR_EndDateEdit.TabIndex = 1;
			// 
			// RatesGrid
			// 
			this.RatesGrid.AllowNavigation = false;
			this.RatesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RatesGrid, "Recipients.Rates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).Rates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipientRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).Rates)).SyncRoot)).CAT_CommissionPercentage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipientRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).Rates)).SyncRoot)).CAT_CommissionAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipientRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).Rates)).SyncRoot)).CAT_RX_NKCommissionCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipientRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).Rates)).SyncRoot)).CAT_CommissionPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipientRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).Rates)).SyncRoot)).CAT_CommissionStartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipientRate)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreementRecipient)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgCommissionAgreement)(null)).Recipients)).SyncRoot)).Rates)).SyncRoot)).CAT_CommissionEndDate)));
			this.RatesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "CAT_CommissionPercentage";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "CAT_CommissionAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CAT_RX_NKCommissionCurrency";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo2.ColumnName = "CAT_CommissionPeriod";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDateEditColumnStyleInfo1.ColumnName = "CAT_CommissionStartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo2.ColumnName = "CAT_CommissionEndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.RatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.RatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.RatesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.RatesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.RatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RatesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RatesGrid.GridId = "1d7f5870-c5df-4eeb-9209-ea97b8f98e19";
			this.RatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RatesGrid.LayoutKey = "RatesGrid";
			this.RatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.RatesGrid.Name = "RatesGrid";
			this.RatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 53, true);
			this.RatesGrid.TabIndex = 0;
			// 
			// CommissionAgreementControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainSplitContainer);
			this.Name = "CommissionAgreementControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.detailsGroupBox.ResumeLayout(false);
			this.detailsGroupBox.PerformLayout();
			this.detailsSplitContainer.Panel1.ResumeLayout(false);
			this.detailsSplitContainer.Panel2.ResumeLayout(false);
			this.detailsSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.detailsSplitContainer)).EndInit();
			this.detailsSplitContainer.ResumeLayout(false);
			this.detailsSplitContainer.PerformLayout();
			this.commissionAgreementItemsControl.ResumeLayout(true);
			this.commissionAgreementItemsControl.PerformLayout();
			this.CA0_CommissionStreamDropEdit.ResumeLayout(true);
			this.CA0_CommissionStreamDropEdit.PerformLayout();
			this.CA0_ExpiredDateDateEdit.ResumeLayout(true);
			this.CA0_ExpiredDateDateEdit.PerformLayout();
			this.CA0_OH_CustomerGuidDropEdit.ResumeLayout(true);
			this.CA0_OH_CustomerGuidDropEdit.PerformLayout();
			this.EffectiveDateDateEdit.ResumeLayout(true);
			this.EffectiveDateDateEdit.PerformLayout();
			this.CA0_CommissionTriggerTypeDropEdit.ResumeLayout(true);
			this.CA0_CommissionTriggerTypeDropEdit.PerformLayout();
			this.CA0_CommissionBasisDropEdit.ResumeLayout(true);
			this.CA0_CommissionBasisDropEdit.PerformLayout();
			this.recipientsGroupBox.ResumeLayout(false);
			this.recipientsGroupBox.PerformLayout();
			this.recipientsSplitContainer.Panel1.ResumeLayout(false);
			this.recipientsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.recipientsSplitContainer)).EndInit();
			this.recipientsSplitContainer.ResumeLayout(false);
			this.recipientsSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).EndInit();
			this.RecipientsGrid.ResumeLayout(false);
			this.RecipientsGrid.PerformLayout();
			this.ratesGroupBox.ResumeLayout(false);
			this.ratesGroupBox.PerformLayout();
			this.CAR_EndDateEdit.ResumeLayout(true);
			this.CAR_EndDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RatesGrid)).EndInit();
			this.RatesGrid.ResumeLayout(false);
			this.RatesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private ZArchitecture.GUI.ZGroupBox recipientsGroupBox;
		protected ZArchitecture.ZGrid RecipientsGrid;
		private CargoWise.Windows.UI.KSplitContainer recipientsSplitContainer;
		private ZArchitecture.GUI.ZGroupBox ratesGroupBox;
		protected ZArchitecture.ZGrid RatesGrid;
		protected CargoWise.Windows.UI.KSplitContainer detailsSplitContainer;
		protected ZArchitecture.GUI.ZDateEdit EffectiveDateDateEdit;
		protected ZArchitecture.GUI.ZDropEdit CA0_CommissionTriggerTypeDropEdit;
		protected ZArchitecture.GUI.ZDropEditWithFixedWidth CA0_CommissionBasisDropEdit;
		protected ZArchitecture.GUI.ZGuidDropEdit CA0_OH_CustomerGuidDropEdit;
		private ZArchitecture.GUI.ZGroupBox detailsGroupBox;
		private ZArchitecture.GUI.ZDateEdit CA0_ExpiredDateDateEdit;
		private ZArchitecture.ZLabel EffectiveDateDashLabel;
		private CommissionAgreementItemsControl commissionAgreementItemsControl;
		protected ZArchitecture.ZLabel StatusDescriptionLabel;
		private ZArchitecture.GUI.ZCheckBox IsApprovedCheckBox;
		private ZArchitecture.ZLabel StatusLabel;
		protected ZArchitecture.GUI.ZDateEdit CAR_EndDateEdit;
		protected ZArchitecture.GUI.ZDropEditWithFixedWidth CA0_CommissionStreamDropEdit;
	}
}
