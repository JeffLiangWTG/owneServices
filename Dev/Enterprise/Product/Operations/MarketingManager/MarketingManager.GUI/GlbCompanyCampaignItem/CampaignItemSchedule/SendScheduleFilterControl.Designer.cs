namespace Enterprise.MarketingManager.GUI
{
	partial class SendScheduleFilterControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.CaptionVisible = false;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b80e1669-acd7-4e51-9db1-abd23f9e4db9", "Scheduled Send (UTC)");
			zDateEditColumnStyleInfo1.ColumnName = "ScheduleSendTimeUTC";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("095a0751-dee7-4ea8-bc01-7afb34f982e4", "UTC Offset");
			zTextBoxColumnStyleInfo1.ColumnName = "UtcOffsetText";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("1896ad71-d3a2-4118-b906-359038ec7c7c", "Time Zone");
			zTextBoxColumnStyleInfo2.ColumnName = "TimeZone";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(360);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("8522e6e6-7793-46d2-ad22-21ac6267209a", "Standard");
			zTextBoxColumnStyleInfo3.ColumnName = "StandardTimeZoneCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("c1177a24-2c6d-4e2a-97ca-3ade58851ed5", "Number of Contacts");
			zCalcEditColumnStyleInfo1.ColumnName = "ContactsCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("f6e7a772-f525-4b13-b10c-aff9d48c3682", "Scheduled Send (Time Zone)");
			zDateEditColumnStyleInfo2.ColumnName = "ScheduleSendTimeLocal";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("9f72f8c1-9a3e-4ca5-8c83-f25fc7e14e10", "Status");
			zTextBoxColumnStyleInfo4.ColumnName = "StatusText";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.CopySelectedRowsAllowed = true;
			this.grid.GridId = "5dcb1749-f65f-4d3e-a812-c8f534495584";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.LayoutKey = "zGrid1";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grid.Name = "grid";
			this.grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.grid.ShouldSetErrorsOnTabPage = false;
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 240, true);
			this.grid.TabIndex = 0;
			this.grid.DoubleClick += new System.EventHandler(grid_DoubleClick);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.GlbCompanyCampaignItemSchedule);
			// 
			// SendScheduleFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "SendScheduleFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
