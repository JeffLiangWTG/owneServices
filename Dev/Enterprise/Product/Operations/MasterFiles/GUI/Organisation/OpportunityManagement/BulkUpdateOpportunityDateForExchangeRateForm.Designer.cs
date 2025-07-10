namespace Enterprise.MasterFiles.GUI
{
	partial class BulkUpdateOpportunityDateForExchangeRateForm
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
		protected new void InitializeComponent()
		{
			this.bottomTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.updateDateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.instructionsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.bottomTableLayoutPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.DateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 131, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UpdateP8_DateForExchangeRateAction);
			// 
			// bottomTableLayoutPanel
			// 
			this.bottomTableLayoutPanel.ColumnCount = 3;
			this.bottomTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.bottomTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.bottomTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.bottomTableLayoutPanel.Controls.Add(this.zPanel1, 1, 0);
			this.bottomTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 67, true);
			this.bottomTableLayoutPanel.Name = "bottomTableLayoutPanel";
			this.bottomTableLayoutPanel.RowCount = 1;
			this.bottomTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.bottomTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 64, true);
			this.bottomTableLayoutPanel.TabIndex = 1;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.DateEdit);
			this.zPanel1.Controls.Add(this.updateDateButton);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 2, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 60, true);
			this.zPanel1.TabIndex = 0;
			// 
			// DateEdit
			// 
			this.DateEdit.AllowDrop = true;
			this.DateEdit.AutoCompleteMonthThreshold = 1;
			this.DateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateEdit, "Date");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.UpdateP8_DateForExchangeRateAction)(null)).Date)));
			this.DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 2, true);
			this.DateEdit.Name = "DateEdit";
			this.DateEdit.TabIndex = 0;
			// 
			// updateDateButton
			// 
			this.updateDateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.updateDateButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0d2cba2f-6e68-4ffc-aa2d-8b9c2a152e7d", "Update Date");
			this.updateDateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 32, true);
			this.updateDateButton.Name = "updateDateButton";
			this.updateDateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.updateDateButton.TabIndex = 1;
			this.updateDateButton.UseVisualStyleBackColor = true;
			this.updateDateButton.Click += new System.EventHandler(this.UpdateDateButton_Click);
			// 
			// instructionsLabel
			// 
			this.instructionsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.instructionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.instructionsLabel.Name = "instructionsLabel";
			this.instructionsLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, true);
			this.instructionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 67, true);
			this.instructionsLabel.TabIndex = 0;
			this.instructionsLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// BulkUpdateOpportunityDateForExchangeRateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("134b65f9-2dd7-42f8-acff-17a4e23b8b6b", "Update Estimated Value Exchange Rate Date");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 155, true);
			this.Controls.Add(this.instructionsLabel);
			this.Controls.Add(this.bottomTableLayoutPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UpdateP8_DateForExchangeRateAction);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "BulkUpdateOpportunityDateForExchangeRateForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.bottomTableLayoutPanel, 0);
			this.Controls.SetChildIndex(this.instructionsLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.bottomTableLayoutPanel.ResumeLayout(false);
			this.bottomTableLayoutPanel.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.DateEdit.ResumeLayout(true);
			this.DateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel bottomTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZButton updateDateButton;
		private ZArchitecture.GUI.ZDateEdit DateEdit;
		private ZArchitecture.ZLabel instructionsLabel;
	}
}