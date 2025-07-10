using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	partial class TouchScheduleUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.groupBoxOuter = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.tableLayoutPanelOuter = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.panelBatchControlRules = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.groupBoxBatchSettings = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsContactLimitPerOrganizationEachBatchUsedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GSC_ContactLimitPerOrganizationEachBatchCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IsContactLimitEachBatchUsedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GSC_ContactLimitEachBatchCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.recurrenceControl = new Enterprise.MarketingManager.GUI.BatchRecurrenceControl();
			this.groupBoxContactLimitPerOrganization = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GSC_ContactLimitPerOrganizationInHorizontalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.IsContactLimitPerOrganizationInTouchUsedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GSC_ContactLimitPerOrganizationInTouchCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysLabel = new Enterprise.ZArchitecture.ZLabel();
			this.groupBoxOptions = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.tableLayoutPanelOptions = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.panelDaysOffset = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zNumericUpDownDaysOffset = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.checkBoxUseCurrentTime = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.panelScheduleDateTime = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.nextScheduledPrintRunTimeLocalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zNumericUpDownHoursOffset = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.ScheduleSendTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.panelZones = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.GSC_IsRecipientLocalTimeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.GSC_IsSenderLocalTimeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.groupBoxScheduleType = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.radioBatchRecurrencePattern = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.radioSendOnFixed = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.radioButtonUseOffset = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.radioSendImmediately = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBoxOuter.SuspendLayout();
			this.tableLayoutPanelOuter.SuspendLayout();
			this.panelBatchControlRules.SuspendLayout();
			this.groupBoxBatchSettings.SuspendLayout();
			this.recurrenceControl.SuspendLayout();
			this.groupBoxContactLimitPerOrganization.SuspendLayout();
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.SuspendLayout();
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.SuspendLayout();
			this.groupBoxOptions.SuspendLayout();
			this.tableLayoutPanelOptions.SuspendLayout();
			this.panelDaysOffset.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zNumericUpDownDaysOffset)).BeginInit();
			this.zNumericUpDownDaysOffset.SuspendLayout();
			this.panelScheduleDateTime.SuspendLayout();
			this.nextScheduledPrintRunTimeLocalDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zNumericUpDownHoursOffset)).BeginInit();
			this.zNumericUpDownHoursOffset.SuspendLayout();
			this.ScheduleSendTimeDateEdit.SuspendLayout();
			this.panelZones.SuspendLayout();
			this.groupBoxScheduleType.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings);
			// 
			// groupBoxOuter
			// 
			this.groupBoxOuter.Controls.Add(this.tableLayoutPanelOuter);
			this.groupBoxOuter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBoxOuter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBoxOuter.Name = "groupBoxOuter";
			this.groupBoxOuter.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 6, 6, 6, true);
			this.groupBoxOuter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 754, true);
			this.groupBoxOuter.TabIndex = 0;
			this.groupBoxOuter.TabStop = false;
			// 
			// tableLayoutPanelOuter
			// 
			this.tableLayoutPanelOuter.ColumnCount = 1;
			this.tableLayoutPanelOuter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelOuter.Controls.Add(this.panelBatchControlRules, 0, 1);
			this.tableLayoutPanelOuter.Controls.Add(this.recurrenceControl, 0, 3);
			this.tableLayoutPanelOuter.Controls.Add(this.groupBoxContactLimitPerOrganization, 0, 4);
			this.tableLayoutPanelOuter.Controls.Add(this.groupBoxOptions, 0, 2);
			this.tableLayoutPanelOuter.Controls.Add(this.groupBoxScheduleType, 0, 0);
			this.tableLayoutPanelOuter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanelOuter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.tableLayoutPanelOuter.Name = "tableLayoutPanelOuter";
			this.tableLayoutPanelOuter.RowCount = 5;
			this.tableLayoutPanelOuter.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelOuter.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelOuter.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelOuter.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelOuter.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelOuter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(13)));
			this.tableLayoutPanelOuter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 729, true);
			this.tableLayoutPanelOuter.TabIndex = 0;
			// 
			// panelBatchControlRules
			// 
			this.panelBatchControlRules.AutoSize = true;
			this.panelBatchControlRules.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.panelBatchControlRules.Controls.Add(this.groupBoxBatchSettings);
			this.panelBatchControlRules.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelBatchControlRules.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 109, true);
			this.panelBatchControlRules.Name = "panelBatchControlRules";
			this.panelBatchControlRules.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 63, true);
			this.panelBatchControlRules.TabIndex = 9;
			// 
			// groupBoxBatchSettings
			// 
			this.groupBoxBatchSettings.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("7a79c0da-7be7-40bf-a057-b6202bb4a5db", "Batch Settings");
			this.groupBoxBatchSettings.Controls.Add(this.IsContactLimitPerOrganizationEachBatchUsedCheckBox);
			this.groupBoxBatchSettings.Controls.Add(this.GSC_ContactLimitPerOrganizationEachBatchCalcEdit);
			this.groupBoxBatchSettings.Controls.Add(this.IsContactLimitEachBatchUsedCheckBox);
			this.groupBoxBatchSettings.Controls.Add(this.GSC_ContactLimitEachBatchCalcEdit);
			this.groupBoxBatchSettings.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBoxBatchSettings.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.groupBoxBatchSettings.Name = "groupBoxBatchSettings";
			this.groupBoxBatchSettings.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 63, true);
			this.groupBoxBatchSettings.TabIndex = 18;
			this.groupBoxBatchSettings.TabStop = false;
			// 
			// IsContactLimitPerOrganizationEachBatchUsedCheckBox
			// 
			this.IsContactLimitPerOrganizationEachBatchUsedCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.IsContactLimitPerOrganizationEachBatchUsedCheckBox, "IsContactLimitPerOrganizationEachBatchUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).IsContactLimitPerOrganizationEachBatchUsed)));
			this.IsContactLimitPerOrganizationEachBatchUsedCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("492f2efa-b6c1-47ea-8f0d-35f0475e6799", "Contact Limit per Organization");
			this.IsContactLimitPerOrganizationEachBatchUsedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsContactLimitPerOrganizationEachBatchUsedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsContactLimitPerOrganizationEachBatchUsedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 38, true);
			this.IsContactLimitPerOrganizationEachBatchUsedCheckBox.Name = "IsContactLimitPerOrganizationEachBatchUsedCheckBox";
			this.IsContactLimitPerOrganizationEachBatchUsedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 16, true);
			this.IsContactLimitPerOrganizationEachBatchUsedCheckBox.TabIndex = 9;
			this.IsContactLimitPerOrganizationEachBatchUsedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsContactLimitPerOrganizationEachBatchUsedCheckBox.UseVisualStyleBackColor = true;
			// 
			// GSC_ContactLimitPerOrganizationEachBatchCalcEdit
			// 
			this.GSC_ContactLimitPerOrganizationEachBatchCalcEdit.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.GSC_ContactLimitPerOrganizationEachBatchCalcEdit, "GSC_ContactLimitPerOrganizationEachBatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).GSC_ContactLimitPerOrganizationEachBatch)));
			this.GSC_ContactLimitPerOrganizationEachBatchCalcEdit.DecimalPlaces = 2;
			this.GSC_ContactLimitPerOrganizationEachBatchCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 38, true);
			this.GSC_ContactLimitPerOrganizationEachBatchCalcEdit.Name = "GSC_ContactLimitPerOrganizationEachBatchCalcEdit";
			this.GSC_ContactLimitPerOrganizationEachBatchCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 17, true);
			this.GSC_ContactLimitPerOrganizationEachBatchCalcEdit.TabIndex = 0;
			this.GSC_ContactLimitPerOrganizationEachBatchCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IsContactLimitEachBatchUsedCheckBox
			// 
			this.IsContactLimitEachBatchUsedCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.IsContactLimitEachBatchUsedCheckBox, "IsContactLimitEachBatchUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).IsContactLimitEachBatchUsed)));
			this.IsContactLimitEachBatchUsedCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("053db361-906d-4b0f-8950-a67a96ca17ec", "Contact Limit");
			this.IsContactLimitEachBatchUsedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsContactLimitEachBatchUsedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsContactLimitEachBatchUsedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 17, true);
			this.IsContactLimitEachBatchUsedCheckBox.Name = "IsContactLimitEachBatchUsedCheckBox";
			this.IsContactLimitEachBatchUsedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(213, 16, true);
			this.IsContactLimitEachBatchUsedCheckBox.TabIndex = 1;
			this.IsContactLimitEachBatchUsedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsContactLimitEachBatchUsedCheckBox.UseVisualStyleBackColor = true;
			// 
			// GSC_ContactLimitEachBatchCalcEdit
			// 
			this.GSC_ContactLimitEachBatchCalcEdit.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.GSC_ContactLimitEachBatchCalcEdit, "GSC_ContactLimitEachBatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).GSC_ContactLimitEachBatch)));
			this.GSC_ContactLimitEachBatchCalcEdit.DecimalPlaces = 2;
			this.GSC_ContactLimitEachBatchCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 16, true);
			this.GSC_ContactLimitEachBatchCalcEdit.Name = "GSC_ContactLimitEachBatchCalcEdit";
			this.GSC_ContactLimitEachBatchCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 17, true);
			this.GSC_ContactLimitEachBatchCalcEdit.TabIndex = 3;
			this.GSC_ContactLimitEachBatchCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// recurrenceControl
			// 
			this.recurrenceControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.recurrenceControl, "ScheduleTask+Recurrence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Scheduler.Business.StmScheduleTaskRecurrence)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).ScheduleTask.Recurrence)));
			this.recurrenceControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.recurrenceControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 304, true);
			this.recurrenceControl.Name = "recurrenceControl";
			this.recurrenceControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 150, true);
			this.recurrenceControl.TabIndex = 5;
			// 
			// groupBoxContactLimitPerOrganization
			// 
			this.groupBoxContactLimitPerOrganization.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("4c44f587-2155-43d2-8537-7374ff97824e", "Contact Limit per Organization");
			this.groupBoxContactLimitPerOrganization.Controls.Add(this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel);
			this.groupBoxContactLimitPerOrganization.Controls.Add(this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel);
			this.groupBoxContactLimitPerOrganization.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBoxContactLimitPerOrganization.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 458, true);
			this.groupBoxContactLimitPerOrganization.Name = "groupBoxContactLimitPerOrganization";
			this.groupBoxContactLimitPerOrganization.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 65, true);
			this.groupBoxContactLimitPerOrganization.TabIndex = 17;
			this.groupBoxContactLimitPerOrganization.TabStop = false;
			// 
			// GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel
			// 
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.Controls.Add(this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.Controls.Add(this.GSC_ContactLimitPerOrganizationInHorizontalCalcEdit);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.Controls.Add(this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.Controls.Add(this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.Controls.Add(this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysLabel);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.Name = "GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel";
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 23, true);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.TabIndex = 4;
			// 
			// IsContactLimitPerOrganizationInHorizontalUsedCheckBox
			// 
			this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox, "IsContactLimitPerOrganizationInHorizontalUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).IsContactLimitPerOrganizationInHorizontalUsed)));
			this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("c87de5c0-e500-47a0-a512-941c27c4226d", "Total from all touches in Horizontal");
			this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 3, true);
			this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox.Name = "IsContactLimitPerOrganizationInHorizontalUsedCheckBox";
			this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 16, true);
			this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox.TabIndex = 0;
			this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsContactLimitPerOrganizationInHorizontalUsedCheckBox.UseVisualStyleBackColor = true;
			// 
			// GSC_ContactLimitPerOrganizationInHorizontalCalcEdit
			// 
			this.GSC_ContactLimitPerOrganizationInHorizontalCalcEdit.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.GSC_ContactLimitPerOrganizationInHorizontalCalcEdit, "GSC_ContactLimitPerOrganizationInHorizontal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).GSC_ContactLimitPerOrganizationInHorizontal)));
			this.GSC_ContactLimitPerOrganizationInHorizontalCalcEdit.DecimalPlaces = 2;
			this.GSC_ContactLimitPerOrganizationInHorizontalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 2, true);
			this.GSC_ContactLimitPerOrganizationInHorizontalCalcEdit.Name = "GSC_ContactLimitPerOrganizationInHorizontalCalcEdit";
			this.GSC_ContactLimitPerOrganizationInHorizontalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 17, true);
			this.GSC_ContactLimitPerOrganizationInHorizontalCalcEdit.TabIndex = 1;
			this.GSC_ContactLimitPerOrganizationInHorizontalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox
			// 
			this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox, "IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsed)));
			this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("8b5454a7-efb2-407d-8c05-f68da13dd40a", "Over a period");
			this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 3, true);
			this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox.Name = "IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox";
			this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox.TabIndex = 2;
			this.IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox.UseVisualStyleBackColor = true;
			// 
			// GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit
			// 
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit, "GSC_ContactLimitPerOrgInHorizontalPeriodInDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).GSC_ContactLimitPerOrgInHorizontalPeriodInDays)));
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit.DecimalPlaces = 2;
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 2, true);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit.Name = "GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit";
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 17, true);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit.TabIndex = 0;
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GSC_ContactLimitPerOrgInHorizontalPeriodInDaysLabel
			// 
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("e7e0d920-c5f5-4a90-9e2b-b3227695746b", "Day(s)");
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 3, true);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysLabel.Name = "GSC_ContactLimitPerOrgInHorizontalPeriodInDaysLabel";
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysLabel.TabIndex = 1;
			// 
			// GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel
			// 
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.Controls.Add(this.IsContactLimitPerOrganizationInTouchUsedCheckBox);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.Controls.Add(this.GSC_ContactLimitPerOrganizationInTouchCalcEdit);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.Controls.Add(this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.Controls.Add(this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.Controls.Add(this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysLabel);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 39, true);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.Name = "GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel";
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 23, true);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.TabIndex = 12;
			// 
			// IsContactLimitPerOrganizationInTouchUsedCheckBox
			// 
			this.IsContactLimitPerOrganizationInTouchUsedCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.IsContactLimitPerOrganizationInTouchUsedCheckBox, "IsContactLimitPerOrganizationInTouchUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).IsContactLimitPerOrganizationInTouchUsed)));
			this.IsContactLimitPerOrganizationInTouchUsedCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("a53734b5-3ff4-48ee-be42-aba5c59fe92d", "Total from this Touch");
			this.IsContactLimitPerOrganizationInTouchUsedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsContactLimitPerOrganizationInTouchUsedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsContactLimitPerOrganizationInTouchUsedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 3, true);
			this.IsContactLimitPerOrganizationInTouchUsedCheckBox.Name = "IsContactLimitPerOrganizationInTouchUsedCheckBox";
			this.IsContactLimitPerOrganizationInTouchUsedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 16, true);
			this.IsContactLimitPerOrganizationInTouchUsedCheckBox.TabIndex = 5;
			this.IsContactLimitPerOrganizationInTouchUsedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsContactLimitPerOrganizationInTouchUsedCheckBox.UseVisualStyleBackColor = true;
			// 
			// GSC_ContactLimitPerOrganizationInTouchCalcEdit
			// 
			this.GSC_ContactLimitPerOrganizationInTouchCalcEdit.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.GSC_ContactLimitPerOrganizationInTouchCalcEdit, "GSC_ContactLimitPerOrganizationInTouch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).GSC_ContactLimitPerOrganizationInTouch)));
			this.GSC_ContactLimitPerOrganizationInTouchCalcEdit.DecimalPlaces = 2;
			this.GSC_ContactLimitPerOrganizationInTouchCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 2, true);
			this.GSC_ContactLimitPerOrganizationInTouchCalcEdit.Name = "GSC_ContactLimitPerOrganizationInTouchCalcEdit";
			this.GSC_ContactLimitPerOrganizationInTouchCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 17, true);
			this.GSC_ContactLimitPerOrganizationInTouchCalcEdit.TabIndex = 6;
			this.GSC_ContactLimitPerOrganizationInTouchCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox
			// 
			this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox, "IsContactLimitPerOrganizationInTouchPeriodInDaysUsed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).IsContactLimitPerOrganizationInTouchPeriodInDaysUsed)));
			this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6b2f13e5-283b-46aa-b91d-4a192f4b6aa3", "Over a period");
			this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 3, true);
			this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox.Name = "IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox";
			this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox.TabIndex = 8;
			this.IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox.UseVisualStyleBackColor = true;
			// 
			// GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit
			// 
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.BindingSource.SetBindingMember(this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit, "GSC_ContactLimitPerOrganizationInTouchPeriodInDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).GSC_ContactLimitPerOrganizationInTouchPeriodInDays)));
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit.DecimalPlaces = 2;
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 2, true);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit.Name = "GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit";
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 17, true);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit.TabIndex = 1;
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GSC_ContactLimitPerOrganizationInTouchPeriodInDaysLabel
			// 
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("84562ae2-a1da-4bc4-8ace-5c367fd9fcba", "Day(s)");
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 3, true);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysLabel.Name = "GSC_ContactLimitPerOrganizationInTouchPeriodInDaysLabel";
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysLabel.TabIndex = 0;
			// 
			// groupBoxOptions
			// 
			this.groupBoxOptions.AutoSize = true;
			this.groupBoxOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.groupBoxOptions.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("1ff60603-cd95-46cb-963a-b6eb76191ec3", "Schedule Options");
			this.groupBoxOptions.Controls.Add(this.tableLayoutPanelOptions);
			this.groupBoxOptions.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBoxOptions.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 175, true);
			this.groupBoxOptions.Name = "groupBoxOptions";
			this.groupBoxOptions.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 125, true);
			this.groupBoxOptions.TabIndex = 16;
			this.groupBoxOptions.TabStop = false;
			// 
			// tableLayoutPanelOptions
			// 
			this.tableLayoutPanelOptions.AutoSize = true;
			this.tableLayoutPanelOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanelOptions.ColumnCount = 1;
			this.tableLayoutPanelOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelOptions.Controls.Add(this.panelDaysOffset, 0, 0);
			this.tableLayoutPanelOptions.Controls.Add(this.panelScheduleDateTime, 0, 1);
			this.tableLayoutPanelOptions.Controls.Add(this.panelZones, 0, 2);
			this.tableLayoutPanelOptions.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanelOptions.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.tableLayoutPanelOptions.Name = "tableLayoutPanelOptions";
			this.tableLayoutPanelOptions.RowCount = 3;
			this.tableLayoutPanelOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelOptions.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelOptions.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(877, 108, true);
			this.tableLayoutPanelOptions.TabIndex = 13;
			// 
			// panelDaysOffset
			// 
			this.panelDaysOffset.Controls.Add(this.zNumericUpDownDaysOffset);
			this.panelDaysOffset.Controls.Add(this.checkBoxUseCurrentTime);
			this.panelDaysOffset.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelDaysOffset.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.panelDaysOffset.Name = "panelDaysOffset";
			this.panelDaysOffset.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 25, true);
			this.panelDaysOffset.TabIndex = 8;
			// 
			// zNumericUpDownDaysOffset
			// 
			this.BindingSource.SetBindingMember(this.zNumericUpDownDaysOffset, "DaysOffset");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).DaysOffset)));
			this.zNumericUpDownDaysOffset.BindTo = "DaysOffset";
			this.zNumericUpDownDaysOffset.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("0fba6591-6ecf-4489-b5f7-75b686394100", "Offset value in days");
			this.zNumericUpDownDaysOffset.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 4, true);
			this.zNumericUpDownDaysOffset.Name = "zNumericUpDownDaysOffset";
			this.zNumericUpDownDaysOffset.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 17, true);
			this.zNumericUpDownDaysOffset.TabIndex = 0;
			// 
			// checkBoxUseCurrentTime
			// 
			this.checkBoxUseCurrentTime.AutoSize = true;
			this.BindingSource.SetBindingMember(this.checkBoxUseCurrentTime, "IsUseCurrentTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).IsUseCurrentTime)));
			this.checkBoxUseCurrentTime.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("46913c49-3a0f-4161-954f-765ae52d0dae", "Use Current Time");
			this.checkBoxUseCurrentTime.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxUseCurrentTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(198, 6, true);
			this.checkBoxUseCurrentTime.Name = "checkBoxUseCurrentTime";
			this.checkBoxUseCurrentTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 16, true);
			this.checkBoxUseCurrentTime.TabIndex = 14;
			this.checkBoxUseCurrentTime.UseVisualStyleBackColor = true;
			// 
			// panelScheduleDateTime
			// 
			this.panelScheduleDateTime.Controls.Add(this.nextScheduledPrintRunTimeLocalDateEdit);
			this.panelScheduleDateTime.Controls.Add(this.zNumericUpDownHoursOffset);
			this.panelScheduleDateTime.Controls.Add(this.ScheduleSendTimeDateEdit);
			this.panelScheduleDateTime.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelScheduleDateTime.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 31, true);
			this.panelScheduleDateTime.Name = "panelScheduleDateTime";
			this.panelScheduleDateTime.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 25, true);
			this.panelScheduleDateTime.TabIndex = 10;
			// 
			// nextScheduledPrintRunTimeLocalDateEdit
			// 
			this.nextScheduledPrintRunTimeLocalDateEdit.AllowDrop = true;
			this.nextScheduledPrintRunTimeLocalDateEdit.AutoCompleteMonthThreshold = 1;
			this.nextScheduledPrintRunTimeLocalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.nextScheduledPrintRunTimeLocalDateEdit, "ScheduleTask+Recurrence+NextScheduledPrintRunTimeLocal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).ScheduleTask.Recurrence.NextScheduledPrintRunTimeLocal)));
			this.nextScheduledPrintRunTimeLocalDateEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("89cc36a1-c265-492f-b186-523171d00811", "Next Schedule Date");
			this.nextScheduledPrintRunTimeLocalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.nextScheduledPrintRunTimeLocalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 4, true);
			this.nextScheduledPrintRunTimeLocalDateEdit.Name = "nextScheduledPrintRunTimeLocalDateEdit";
			this.nextScheduledPrintRunTimeLocalDateEdit.TabIndex = 15;
			// 
			// zNumericUpDownHoursOffset
			// 
			this.BindingSource.SetBindingMember(this.zNumericUpDownHoursOffset, "HoursOffset");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).HoursOffset)));
			this.zNumericUpDownHoursOffset.BindTo = "HoursOffset";
			this.zNumericUpDownHoursOffset.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("79c6a705-ef3f-4106-a837-4d1228496e0e", "Offset value in hours");
			this.zNumericUpDownHoursOffset.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 4, true);
			this.zNumericUpDownHoursOffset.Maximum = new decimal(new int[] {
            23,
            0,
            0,
            0});
			this.zNumericUpDownHoursOffset.Name = "zNumericUpDownHoursOffset";
			this.zNumericUpDownHoursOffset.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 17, true);
			this.zNumericUpDownHoursOffset.TabIndex = 1;
			// 
			// ScheduleSendTimeDateEdit
			// 
			this.ScheduleSendTimeDateEdit.AllowDrop = true;
			this.ScheduleSendTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.ScheduleSendTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ScheduleSendTimeDateEdit, "GSC_ScheduleTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).GSC_ScheduleTime)));
			this.ScheduleSendTimeDateEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("edcee485-f5f8-4e85-ace2-c6551d348d38", "Scheduled Send Time");
			this.ScheduleSendTimeDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Time;
			this.ScheduleSendTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 4, true);
			this.ScheduleSendTimeDateEdit.Name = "ScheduleSendTimeDateEdit";
			this.ScheduleSendTimeDateEdit.TabIndex = 2;
			// 
			// panelZones
			// 
			this.panelZones.Controls.Add(this.GSC_IsRecipientLocalTimeRadioButton);
			this.panelZones.Controls.Add(this.GSC_IsSenderLocalTimeRadioButton);
			this.panelZones.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelZones.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 59, true);
			this.panelZones.Name = "panelZones";
			this.panelZones.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(873, 47, true);
			this.panelZones.TabIndex = 11;
			// 
			// GSC_IsRecipientLocalTimeRadioButton
			// 
			this.GSC_IsRecipientLocalTimeRadioButton.AutoCheck = false;
			this.GSC_IsRecipientLocalTimeRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GSC_IsRecipientLocalTimeRadioButton, "GSC_IsRecipientLocalTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).GSC_IsRecipientLocalTime)));
			this.GSC_IsRecipientLocalTimeRadioButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("38b42b8a-b73c-4c5e-a255-a38b56c21f6b", "Recipient Local Time");
			this.GSC_IsRecipientLocalTimeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GSC_IsRecipientLocalTimeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 4, true);
			this.GSC_IsRecipientLocalTimeRadioButton.Name = "GSC_IsRecipientLocalTimeRadioButton";
			this.GSC_IsRecipientLocalTimeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 16, true);
			this.GSC_IsRecipientLocalTimeRadioButton.TabIndex = 2;
			this.GSC_IsRecipientLocalTimeRadioButton.UseVisualStyleBackColor = true;
			// 
			// GSC_IsSenderLocalTimeRadioButton
			// 
			this.GSC_IsSenderLocalTimeRadioButton.AutoCheck = false;
			this.GSC_IsSenderLocalTimeRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GSC_IsSenderLocalTimeRadioButton, "GSC_IsSenderLocalTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).GSC_IsSenderLocalTime)));
			this.GSC_IsSenderLocalTimeRadioButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("91a6ffe7-c5ad-42f5-b90f-f88d60004770", "Sender Local Time");
			this.GSC_IsSenderLocalTimeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GSC_IsSenderLocalTimeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 24, true);
			this.GSC_IsSenderLocalTimeRadioButton.Name = "GSC_IsSenderLocalTimeRadioButton";
			this.GSC_IsSenderLocalTimeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 16, true);
			this.GSC_IsSenderLocalTimeRadioButton.TabIndex = 3;
			this.GSC_IsSenderLocalTimeRadioButton.TabStop = true;
			this.GSC_IsSenderLocalTimeRadioButton.UseVisualStyleBackColor = true;
			// 
			// groupBoxScheduleType
			// 
			this.groupBoxScheduleType.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("4ee11f70-c0d7-432d-854f-e37ac51f2a20", "Schedule Type");
			this.groupBoxScheduleType.Controls.Add(this.radioBatchRecurrencePattern);
			this.groupBoxScheduleType.Controls.Add(this.radioSendOnFixed);
			this.groupBoxScheduleType.Controls.Add(this.radioButtonUseOffset);
			this.groupBoxScheduleType.Controls.Add(this.radioSendImmediately);
			this.groupBoxScheduleType.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBoxScheduleType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.groupBoxScheduleType.Name = "groupBoxScheduleType";
			this.groupBoxScheduleType.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.groupBoxScheduleType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(881, 103, true);
			this.groupBoxScheduleType.TabIndex = 15;
			this.groupBoxScheduleType.TabStop = false;
			// 
			// radioBatchRecurrencePattern
			// 
			this.radioBatchRecurrencePattern.AutoCheck = false;
			this.radioBatchRecurrencePattern.AutoSize = true;
			this.BindingSource.SetBindingMember(this.radioBatchRecurrencePattern, "IsBatchSchedule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).IsBatchSchedule)));
			this.radioBatchRecurrencePattern.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioBatchRecurrencePattern.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 83, true);
			this.radioBatchRecurrencePattern.Name = "radioBatchRecurrencePattern";
			this.radioBatchRecurrencePattern.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 16, true);
			this.radioBatchRecurrencePattern.TabIndex = 20;
			this.radioBatchRecurrencePattern.UseVisualStyleBackColor = true;
			// 
			// radioSendOnFixed
			// 
			this.radioSendOnFixed.AutoCheck = false;
			this.radioSendOnFixed.AutoSize = true;
			this.BindingSource.SetBindingMember(this.radioSendOnFixed, "IsFixedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).IsFixedDate)));
			this.radioSendOnFixed.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioSendOnFixed.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 61, true);
			this.radioSendOnFixed.Name = "radioSendOnFixed";
			this.radioSendOnFixed.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 16, true);
			this.radioSendOnFixed.TabIndex = 17;
			this.radioSendOnFixed.UseVisualStyleBackColor = true;
			// 
			// radioButtonUseOffset
			// 
			this.radioButtonUseOffset.AutoCheck = false;
			this.radioButtonUseOffset.AutoSize = true;
			this.BindingSource.SetBindingMember(this.radioButtonUseOffset, "IsDelayed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).IsDelayed)));
			this.radioButtonUseOffset.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioButtonUseOffset.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 40, true);
			this.radioButtonUseOffset.Name = "radioButtonUseOffset";
			this.radioButtonUseOffset.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 16, true);
			this.radioButtonUseOffset.TabIndex = 16;
			this.radioButtonUseOffset.UseVisualStyleBackColor = true;
			// 
			// radioSendImmediately
			// 
			this.radioSendImmediately.AutoCheck = false;
			this.radioSendImmediately.AutoSize = true;
			this.BindingSource.SetBindingMember(this.radioSendImmediately, "IsImmediate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.Business.GlbCompanyCampaignSendSettings)(null)).IsImmediate)));
			this.radioSendImmediately.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.radioSendImmediately.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.radioSendImmediately.Name = "radioSendImmediately";
			this.radioSendImmediately.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 16, true);
			this.radioSendImmediately.TabIndex = 15;
			this.radioSendImmediately.UseVisualStyleBackColor = true;
			// 
			// TouchScheduleUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.groupBoxOuter);
			this.Name = "TouchScheduleUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 754, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBoxOuter.ResumeLayout(false);
			this.groupBoxOuter.PerformLayout();
			this.tableLayoutPanelOuter.ResumeLayout(false);
			this.tableLayoutPanelOuter.PerformLayout();
			this.panelBatchControlRules.ResumeLayout(false);
			this.panelBatchControlRules.PerformLayout();
			this.groupBoxBatchSettings.ResumeLayout(false);
			this.groupBoxBatchSettings.PerformLayout();
			this.recurrenceControl.ResumeLayout(true);
			this.recurrenceControl.PerformLayout();
			this.groupBoxContactLimitPerOrganization.ResumeLayout(false);
			this.groupBoxContactLimitPerOrganization.PerformLayout();
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.ResumeLayout(false);
			this.GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel.PerformLayout();
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.ResumeLayout(false);
			this.GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel.PerformLayout();
			this.groupBoxOptions.ResumeLayout(false);
			this.groupBoxOptions.PerformLayout();
			this.tableLayoutPanelOptions.ResumeLayout(false);
			this.tableLayoutPanelOptions.PerformLayout();
			this.panelDaysOffset.ResumeLayout(false);
			this.panelDaysOffset.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zNumericUpDownDaysOffset)).EndInit();
			this.zNumericUpDownDaysOffset.ResumeLayout(false);
			this.zNumericUpDownDaysOffset.PerformLayout();
			this.panelScheduleDateTime.ResumeLayout(false);
			this.panelScheduleDateTime.PerformLayout();
			this.nextScheduledPrintRunTimeLocalDateEdit.ResumeLayout(true);
			this.nextScheduledPrintRunTimeLocalDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zNumericUpDownHoursOffset)).EndInit();
			this.zNumericUpDownHoursOffset.ResumeLayout(false);
			this.zNumericUpDownHoursOffset.PerformLayout();
			this.ScheduleSendTimeDateEdit.ResumeLayout(true);
			this.ScheduleSendTimeDateEdit.PerformLayout();
			this.panelZones.ResumeLayout(false);
			this.panelZones.PerformLayout();
			this.groupBoxScheduleType.ResumeLayout(false);
			this.groupBoxScheduleType.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZGroupBox groupBoxOuter;
		private ZArchitecture.GUI.ZRadioButton GSC_IsRecipientLocalTimeRadioButton;
		private ZArchitecture.GUI.ZRadioButton GSC_IsSenderLocalTimeRadioButton;
		private ZGroupBox groupBoxScheduleType;
		private ZRadioButton radioSendOnFixed;
		private ZRadioButton radioButtonUseOffset;
		private ZRadioButton radioSendImmediately;
		protected ZGroupBox groupBoxOptions;
		protected ZPanel panelDaysOffset;
		protected ZPanel panelScheduleDateTime;
		private ZNumericUpDown zNumericUpDownDaysOffset;
		private ZNumericUpDown zNumericUpDownHoursOffset;
		protected ZCheckBox checkBoxUseCurrentTime;
		private ZDateEdit ScheduleSendTimeDateEdit;
		protected ZPanel panelZones;
		protected ZGroupBox groupBoxContactLimitPerOrganization;
		private ZCheckBox IsContactLimitEachBatchUsedCheckBox;
		private ZCheckBox IsContactLimitPerOrganizationEachBatchUsedCheckBox;
		private ZArchitecture.ZCalcEdit GSC_ContactLimitEachBatchCalcEdit;
		private ZArchitecture.ZCalcEdit GSC_ContactLimitPerOrganizationInHorizontalCalcEdit;
		private ZCheckBox IsContactLimitPerOrganizationInHorizontalUsedCheckBox;
		private ZArchitecture.ZCalcEdit GSC_ContactLimitPerOrganizationEachBatchCalcEdit;
		private ZCheckBox IsContactLimitPerOrganizationInHorizontalPeriodInDaysUsedCheckBox;
		private CargoWise.Windows.UI.KFlowLayoutPanel GSC_ContactLimitPerOrgInHorizontalPeriodInDaysPanel;
		private ZArchitecture.ZCalcEdit GSC_ContactLimitPerOrgInHorizontalPeriodInDaysCalcEdit;
		private ZArchitecture.ZLabel GSC_ContactLimitPerOrgInHorizontalPeriodInDaysLabel;
		protected ZRadioButton radioBatchRecurrencePattern;
		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanelOuter;
		private CargoWise.Windows.UI.KTableLayoutPanel tableLayoutPanelOptions;
		protected ZPanel panelBatchControlRules;
		private ZGroupBox groupBoxBatchSettings;
		private ZCheckBox IsContactLimitPerOrganizationInTouchPeriodInDaysUsedCheckBox;
		private CargoWise.Windows.UI.KFlowLayoutPanel GSC_ContactLimitPerOrganizationInTouchPeriodInDaysPanel;
		private ZArchitecture.ZCalcEdit GSC_ContactLimitPerOrganizationInTouchPeriodInDaysCalcEdit;
		private ZArchitecture.ZLabel GSC_ContactLimitPerOrganizationInTouchPeriodInDaysLabel;
		private ZCheckBox IsContactLimitPerOrganizationInTouchUsedCheckBox;
		private ZArchitecture.ZCalcEdit GSC_ContactLimitPerOrganizationInTouchCalcEdit;
		private ZDateEdit nextScheduledPrintRunTimeLocalDateEdit;
		protected BatchRecurrenceControl recurrenceControl;
	}
}
