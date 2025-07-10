namespace Enterprise.MasterFiles.GUI
{
	public partial class TraceMonitorForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.PNLButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.ButtonClear = new Enterprise.ZArchitecture.GUI.ZButton();
            this.ButtonToggleStartStop = new Enterprise.ZArchitecture.GUI.ZButton();
            this.TextBoxStackTrace = new Enterprise.ZArchitecture.ZTextBox();
            this.LogDateTimeCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.OptionLabel = new Enterprise.ZArchitecture.ZLabel();
            this.LogProcessIdCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.LogThreadIdCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.LogCallStackCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.TraceSourceSettingsGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
            this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.PNLButtons.SuspendLayout();
            this.zPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TraceSourceSettingsGrid)).BeginInit();
            this.TraceSourceSettingsGrid.SuspendLayout();
            this.zPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 563, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.TraceMonitor);
            // 
            // PNLButtons
            // 
            this.PNLButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PNLButtons.Controls.Add(this.ButtonClear);
            this.PNLButtons.Controls.Add(this.ButtonToggleStartStop);
            this.PNLButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(541, 119, true);
            this.PNLButtons.Name = "PNLButtons";
            this.PNLButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 39, true);
            this.PNLButtons.TabIndex = 6;
            // 
            // ButtonClear
            // 
            this.ButtonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonClear.BackColor = System.Drawing.Color.Salmon;
            this.ButtonClear.IsCaptionOverridden = true;
            this.ButtonClear.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 6, true);
            this.ButtonClear.Name = "ButtonClear";
            this.ButtonClear.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 30, true);
            this.ButtonClear.TabIndex = 6;
            this.ButtonClear.Text = "Clear";
            this.ButtonClear.ToolTipCaption = null;
            this.ButtonClear.UseVisualStyleBackColor = false;
            this.ButtonClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // ButtonToggleStartStop
            // 
            this.ButtonToggleStartStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonToggleStartStop.BackColor = System.Drawing.Color.DodgerBlue;
            this.ButtonToggleStartStop.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bb5dbb46-8cfd-49c3-800e-16a83902cff2", "Start Tracking");
            this.ButtonToggleStartStop.ForeColor = System.Drawing.Color.White;
            this.ButtonToggleStartStop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 6, true);
            this.ButtonToggleStartStop.Name = "ButtonToggleStartStop";
            this.ButtonToggleStartStop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 30, true);
            this.ButtonToggleStartStop.TabIndex = 5;
            this.ButtonToggleStartStop.ToolTipCaption = null;
            this.ButtonToggleStartStop.UseVisualStyleBackColor = false;
            this.ButtonToggleStartStop.Click += new System.EventHandler(this.BtnCollect_Click);
            // 
            // TextBoxStackTrace
            // 
            this.TextBoxStackTrace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBoxStackTrace.CaptionResourceString = null;
            this.TextBoxStackTrace.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.TextBoxStackTrace.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxStackTrace.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 161, true);
            this.TextBoxStackTrace.Multiline = true;
            this.TextBoxStackTrace.Name = "TextBoxStackTrace";
            this.TextBoxStackTrace.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.TextBoxStackTrace.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 397, true);
            this.TextBoxStackTrace.TabIndex = 7;
            // 
            // LogDateTimeCheckbox
            // 
            this.LogDateTimeCheckbox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.LogDateTimeCheckbox, "LogDateTime");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.TraceMonitor)(null)).LogDateTime)));
            this.LogDateTimeCheckbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6039cb42-3c39-450f-be02-cc7682f23962", "UTC DateTime");
            this.LogDateTimeCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 43, true);
            this.LogDateTimeCheckbox.Name = "LogDateTimeCheckbox";
            this.LogDateTimeCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 14, true);
            this.LogDateTimeCheckbox.TabIndex = 0;
            // 
            // zPanel2
            // 
            this.zPanel2.Controls.Add(this.OptionLabel);
            this.zPanel2.Controls.Add(this.LogProcessIdCheckbox);
            this.zPanel2.Controls.Add(this.LogThreadIdCheckbox);
            this.zPanel2.Controls.Add(this.LogDateTimeCheckbox);
            this.zPanel2.Controls.Add(this.LogCallStackCheckBox);
            this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(541, 7, true);
            this.zPanel2.Name = "zPanel2";
            this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 109, true);
            this.zPanel2.TabIndex = 9;
            // 
            // OptionLabel
            // 
            this.OptionLabel.AutoSize = true;
            this.OptionLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ce9984f4-9f71-43e2-90a6-c782e06b33ae", "Include in the log");
            this.OptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.OptionLabel.IsFontBold = true;
            this.OptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.OptionLabel.Name = "OptionLabel";
            this.OptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 13, true);
            this.OptionLabel.TabIndex = 0;
            // 
            // LogProcessIdCheckbox
            // 
            this.LogProcessIdCheckbox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.LogProcessIdCheckbox, "LogProcessId");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.TraceMonitor)(null)).LogProcessId)));
            this.LogProcessIdCheckbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("182a20b2-188f-4f2b-98ca-1977dc15337e", "Process Id");
            this.LogProcessIdCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 80, true);
            this.LogProcessIdCheckbox.Name = "LogProcessIdCheckbox";
            this.LogProcessIdCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 14, true);
            this.LogProcessIdCheckbox.TabIndex = 2;
            // 
            // LogThreadIdCheckbox
            // 
            this.LogThreadIdCheckbox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.LogThreadIdCheckbox, "LogThreadId");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.TraceMonitor)(null)).LogThreadId)));
            this.LogThreadIdCheckbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("26e15744-b7eb-4907-a6ac-bd67dd71fea7", "Thread Id");
            this.LogThreadIdCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 62, true);
            this.LogThreadIdCheckbox.Name = "LogThreadIdCheckbox";
            this.LogThreadIdCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 14, true);
            this.LogThreadIdCheckbox.TabIndex = 1;
            // 
            // LogCallStackCheckBox
            // 
            this.BindingSource.SetBindingMember(this.LogCallStackCheckBox, "LogCallStack");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.TraceMonitor)(null)).LogCallStack)));
            this.LogCallStackCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a0b1a393-a7b8-4883-b55f-a0a588ff740f", "Call Stack");
            this.LogCallStackCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 22, true);
            this.LogCallStackCheckBox.Name = "LogCallStackCheckBox";
            this.LogCallStackCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
            this.LogCallStackCheckBox.TabIndex = 0;
            // 
            // TraceSourceSettingsGrid
            // 
            this.TraceSourceSettingsGrid.AllowNavigation = false;
            this.TraceSourceSettingsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.TraceSourceSettingsGrid, "TraceSourceSettingsList");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TraceMonitor)(null)).TraceSourceSettingsList)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TraceSourceSettings)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TraceMonitor)(null)).TraceSourceSettingsList)).SyncRoot)).TraceSourceCategory)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TraceSourceSettings)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TraceMonitor)(null)).TraceSourceSettingsList)).SyncRoot)).TraceSourceName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TraceSourceSettings)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TraceMonitor)(null)).TraceSourceSettingsList)).SyncRoot)).TraceSourceDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TraceSourceSettings)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TraceMonitor)(null)).TraceSourceSettingsList)).SyncRoot)).TraceLevel)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TraceSourceSettings)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TraceMonitor)(null)).TraceSourceSettingsList)).SyncRoot)).TraceFilter)));
            this.TraceSourceSettingsGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("14baf9de-f7e2-4dc2-87b8-e70e1988ba16", "Category");
            zTextBoxColumnStyleInfo1.ColumnName = "TraceSourceCategory";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(57);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c253cd94-a41f-4f9b-b332-7f6ae6b8cf33", "Code");
            zTextBoxColumnStyleInfo2.ColumnName = "TraceSourceName";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(57);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6606f56d-6770-4979-bc56-2795a2ee5430", "Description");
            zTextBoxColumnStyleInfo3.ColumnName = "TraceSourceDescription";
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("11c923fc-6ad3-4fb5-9293-4f8527667158", "Level");
            zDropEditColumnStyleInfo1.ColumnName = "TraceLevel";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114);
            zTextBoxColumnStyleInfo4.ColumnName = "TraceFilter";
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a4be51e5-cc91-44c9-8c63-0aff030f1ae5", "Filter");
            this.TraceSourceSettingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.TraceSourceSettingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.TraceSourceSettingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.TraceSourceSettingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.TraceSourceSettingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.TraceSourceSettingsGrid.GridId = "5b2c923d-ac0a-4ef6-8198-8c91ee4e4af4";
            this.TraceSourceSettingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.TraceSourceSettingsGrid.LayoutKey = "TraceSourceSettingsGrid";
            this.TraceSourceSettingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 13, true);
            this.TraceSourceSettingsGrid.Name = "TraceSourceSettingsGrid";
            this.TraceSourceSettingsGrid.ShouldSetErrorsOnTabPage = false;
            this.TraceSourceSettingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 133, true);
            this.TraceSourceSettingsGrid.TabIndex = 1;
            // 
            // zPanel1
            // 
            this.zPanel1.Controls.Add(this.zLabel1);
            this.zPanel1.Controls.Add(this.TraceSourceSettingsGrid);
            this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 10, true);
            this.zPanel1.Name = "zPanel1";
            this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 148, true);
            this.zPanel1.TabIndex = 10;
            // 
            // zLabel1
            // 
            this.zLabel1.AutoSize = true;
            this.zLabel1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("76cdec1a-7a63-4a16-be1f-0ded75596900", "Track");
            this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.zLabel1.IsFontBold = true;
            this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, -3, true);
            this.zLabel1.Name = "zLabel1";
            this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 13, true);
            this.zLabel1.TabIndex = 1;
            // 
            // TraceMonitorForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 587, true);
            this.Controls.Add(this.zPanel1);
            this.Controls.Add(this.zPanel2);
            this.Controls.Add(this.TextBoxStackTrace);
            this.Controls.Add(this.PNLButtons);
            this.DataSourceType = typeof(Enterprise.MasterFiles.Business.TraceMonitor);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 622, true);
            this.Name = "TraceMonitorForm";
            this.Text = "Monitor";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TraceForm_FormClosing);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.PNLButtons, 0);
            this.Controls.SetChildIndex(this.TextBoxStackTrace, 0);
            this.Controls.SetChildIndex(this.zPanel2, 0);
            this.Controls.SetChildIndex(this.zPanel1, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PNLButtons.ResumeLayout(false);
            this.PNLButtons.PerformLayout();
            this.zPanel2.ResumeLayout(false);
            this.zPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TraceSourceSettingsGrid)).EndInit();
            this.TraceSourceSettingsGrid.ResumeLayout(false);
            this.TraceSourceSettingsGrid.PerformLayout();
            this.zPanel1.ResumeLayout(false);
            this.zPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel PNLButtons;
		public ZArchitecture.GUI.ZButton ButtonClear;
		public ZArchitecture.GUI.ZButton ButtonToggleStartStop;
		public ZArchitecture.ZTextBox TextBoxStackTrace;
		private ZArchitecture.GUI.ZPanel zPanel2;
		public ZArchitecture.GUI.ZCheckBox LogDateTimeCheckbox;
		public ZArchitecture.GUI.ZCheckBox LogCallStackCheckBox;
		public Enterprise.ZArchitecture.GUI.ZDisplayGrid TraceSourceSettingsGrid;
		public ZArchitecture.GUI.ZCheckBox LogProcessIdCheckbox;
		public ZArchitecture.GUI.ZCheckBox LogThreadIdCheckbox;
		public ZArchitecture.ZLabel OptionLabel;
		private ZArchitecture.GUI.ZPanel zPanel1;
		public ZArchitecture.ZLabel zLabel1;
	}
}
