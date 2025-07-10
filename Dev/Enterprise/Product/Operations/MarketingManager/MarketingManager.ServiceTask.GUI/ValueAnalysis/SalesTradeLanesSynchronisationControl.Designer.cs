namespace Enterprise.MarketingManager.ServiceTask.GUI
{
	partial class SalesTradeLanesSynchronisationControl
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
		private void InitializeComponent()
		{
			this.syncYearly = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.monthsToSyncMain = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.yearlySyncPeriod = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.groupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.runOnDayOfMonth = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.lastYearlySyncDate = new Enterprise.ZArchitecture.ZLabel();
			this.lastSyncLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.monthsToSyncMain)).BeginInit();
			this.monthsToSyncMain.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.yearlySyncPeriod)).BeginInit();
			this.yearlySyncPeriod.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.runOnDayOfMonth)).BeginInit();
			this.runOnDayOfMonth.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.ServiceTask.GUI.SalesTradeLanesSyncTaskConfig);
			// 
			// syncYearly
			// 
			this.syncYearly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.syncYearly.AutoSize = true;
			this.BindingSource.SetBindingMember(this.syncYearly, "SyncYearly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MarketingManager.ServiceTask.GUI.SalesTradeLanesSyncTaskConfig)(null)).SyncYearly)));
			this.syncYearly.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.syncYearly.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 16, true);
			this.syncYearly.Name = "syncYearly";
			this.syncYearly.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 17, true);
			this.syncYearly.TabIndex = 1;
			this.syncYearly.UseVisualStyleBackColor = true;
			// 
			// monthsToSyncMain
			// 
			this.BindingSource.SetBindingMember(this.monthsToSyncMain, "SyncMonthsOnMainSchedule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.MarketingManager.ServiceTask.GUI.SalesTradeLanesSyncTaskConfig)(null)).SyncMonthsOnMainSchedule)));
			this.monthsToSyncMain.BindTo = "SyncMonthsOnMainSchedule";
			this.monthsToSyncMain.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 17, true);
			this.monthsToSyncMain.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
			this.monthsToSyncMain.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.monthsToSyncMain.Name = "monthsToSyncMain";
			this.monthsToSyncMain.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 18, true);
			this.monthsToSyncMain.TabIndex = 2;
			this.monthsToSyncMain.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
			// 
			// yearlySyncPeriod
			// 
			this.BindingSource.SetBindingMember(this.yearlySyncPeriod, "SyncYearlyEveryMonths");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.MarketingManager.ServiceTask.GUI.SalesTradeLanesSyncTaskConfig)(null)).SyncYearlyEveryMonths)));
			this.yearlySyncPeriod.BindTo = "SyncYearlyEveryMonths";
			this.yearlySyncPeriod.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 39, true);
			this.yearlySyncPeriod.Maximum = new decimal(new int[] {
            12,
            0,
            0,
            0});
			this.yearlySyncPeriod.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.yearlySyncPeriod.Name = "yearlySyncPeriod";
			this.yearlySyncPeriod.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 18, true);
			this.yearlySyncPeriod.TabIndex = 3;
			this.yearlySyncPeriod.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.CaptionResourceString = Res.GetData("95e33bdb-592c-4e07-b0fc-b4c17a98d80a", "Months to synchronize on set recurrence schedule");
			this.groupBox1.Controls.Add(this.monthsToSyncMain);
			this.groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 20, true);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 45, true);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.CaptionResourceString = Res.GetData("c3f8e59e-d5c9-46ed-9510-9d16f70a2737", "Synchronize 12 months every X months");
			this.groupBox2.Controls.Add(this.zLabel1);
			this.groupBox2.Controls.Add(this.runOnDayOfMonth);
			this.groupBox2.Controls.Add(this.lastYearlySyncDate);
			this.groupBox2.Controls.Add(this.lastSyncLabel);
			this.groupBox2.Controls.Add(this.syncYearly);
			this.groupBox2.Controls.Add(this.yearlySyncPeriod);
			this.groupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 83, true);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 89, true);
			this.groupBox2.TabIndex = 5;
			this.groupBox2.TabStop = false;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Res.GetData("2c566065-47a4-4340-90a9-9c22b879bfed", "Last day of month");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 66, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 14, true);
			this.zLabel1.TabIndex = 7;
			this.zLabel1.Visible = false;
			// 
			// runOnDayOfMonth
			// 
			this.BindingSource.SetBindingMember(this.runOnDayOfMonth, "RunYearlyOnDayOfMonth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((decimal)(((Enterprise.MarketingManager.ServiceTask.GUI.SalesTradeLanesSyncTaskConfig)(null)).RunYearlyOnDayOfMonth)));
			this.runOnDayOfMonth.BindTo = "RunYearlyOnDayOfMonth";
			this.runOnDayOfMonth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 64, true);
			this.runOnDayOfMonth.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
			this.runOnDayOfMonth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.runOnDayOfMonth.Name = "runOnDayOfMonth";
			this.runOnDayOfMonth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 18, true);
			this.runOnDayOfMonth.TabIndex = 6;
			this.runOnDayOfMonth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.runOnDayOfMonth.ValueChanged += new System.EventHandler(this.RunOnDayOfMonth_ValueChanged);
			// 
			// lastYearlySyncDate
			// 
			this.lastYearlySyncDate.AutoSize = true;
			this.lastYearlySyncDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 43, true);
			this.lastYearlySyncDate.Name = "lastYearlySyncDate";
			this.lastYearlySyncDate.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.lastYearlySyncDate.TabIndex = 5;
			// 
			// lastSyncLabel
			// 
			this.lastSyncLabel.AutoSize = true;
			this.lastSyncLabel.CaptionResourceString = Res.GetData("bc197acf-0603-4f65-a00a-ce6517b7edd5", "Last yearly synchronization");
			this.lastSyncLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 17, true);
			this.lastSyncLabel.Name = "lastSyncLabel";
			this.lastSyncLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 14, true);
			this.lastSyncLabel.TabIndex = 4;
			// 
			// SalesTradeLanesSynchronisationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Name = "SalesTradeLanesSynchronisationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 230, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.monthsToSyncMain)).EndInit();
			this.monthsToSyncMain.ResumeLayout(false);
			this.monthsToSyncMain.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.yearlySyncPeriod)).EndInit();
			this.yearlySyncPeriod.ResumeLayout(false);
			this.yearlySyncPeriod.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.runOnDayOfMonth)).EndInit();
			this.runOnDayOfMonth.ResumeLayout(false);
			this.runOnDayOfMonth.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox syncYearly;
		private ZArchitecture.GUI.ZNumericUpDown monthsToSyncMain;
		private ZArchitecture.GUI.ZNumericUpDown yearlySyncPeriod;
		private ZArchitecture.GUI.ZGroupBox groupBox1;
		private ZArchitecture.GUI.ZGroupBox groupBox2;
		private ZArchitecture.ZLabel lastSyncLabel;
		private ZArchitecture.ZLabel lastYearlySyncDate;
		private ZArchitecture.GUI.ZNumericUpDown runOnDayOfMonth;
		private ZArchitecture.ZLabel zLabel1;
	}
}
