using CargoWise.Windows.UI;

namespace Enterprise.MasterFiles.GUI
{
	partial class UpdateMultipleJobDocAddressesForm
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
			this.UpdateThisRecordButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UpdateAllRecordsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.panel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.YesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelSelectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.tableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.panel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.panel1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 162, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 24, true);
			// 
			// UpdateThisRecordBtn
			// 
			this.UpdateThisRecordButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b7612135-e150-43cb-977a-9dad57684151", "This Record");
			this.UpdateThisRecordButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 15, true);
			this.UpdateThisRecordButton.Name = "UpdateThisRecordButton";
			this.UpdateThisRecordButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdateThisRecordButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.UpdateThisRecordButton.TabIndex = 2;
			this.UpdateThisRecordButton.ToolTipCaption = null;
			this.UpdateThisRecordButton.Click += new System.EventHandler(this.UpdateThisRecordButton_Click);
			// 
			// UpdateAllRecordsBtn
			// 
			this.UpdateAllRecordsButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b7611145-e150-43cb-877a-9dad57684152", "All Records");
			this.UpdateAllRecordsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 15, true);
			this.UpdateAllRecordsButton.Name = "UpdateAllRecordsButton";
			this.UpdateAllRecordsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdateAllRecordsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 23, true);
			this.UpdateAllRecordsButton.TabIndex = 3;
			this.UpdateAllRecordsButton.ToolTipCaption = null;
			this.UpdateAllRecordsButton.Click += new System.EventHandler(this.UpdateAllRecordsButton_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.TitleLabel);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.panel1.Name = "panel1";
			this.panel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, true);
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 96, true);
			this.panel1.TabIndex = 5;
			// 
			// TitleLabel
			// 
			this.TitleLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b7611145-e150-43cb-877a-9dad57684148", "This is Title.");
			this.TitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 76, true);
			this.TitleLabel.TabIndex = 0;
			this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// YesBtn
			// 
			this.YesButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b7611135-e250-43cb-977a-9dad57684253", "Yes");
			this.YesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 15, true);
			this.YesButton.Name = "YesButton";
			this.YesButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.YesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.YesButton.TabIndex = 1;
			this.YesButton.ToolTipCaption = null;
			this.YesButton.Click += new System.EventHandler(this.YesButton_Click);
			// 
			// CancelBtn
			// 
			this.CancelSelectButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b7611135-e150-43cb-977a-9dad57684253", "Cancel");
			this.CancelSelectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 15, true);
			this.CancelSelectButton.Name = "CancelSelectButton";
			this.CancelSelectButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelSelectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.CancelSelectButton.TabIndex = 4;
			this.CancelSelectButton.ToolTipCaption = null;
			this.CancelSelectButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(60)));
			this.tableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 162, true);
			this.tableLayoutPanel1.TabIndex = 6;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.UpdateThisRecordButton);
			this.panel2.Controls.Add(this.CancelSelectButton);
			this.panel2.Controls.Add(this.YesButton);
			this.panel2.Controls.Add(this.UpdateAllRecordsButton);
			this.panel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 105, true);
			this.panel2.Name = "panel2";
			this.panel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 54, true);
			this.panel2.TabIndex = 7;
			// 
			// UpdateMultipleJobDocAddressesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b7611135-e150-43cb-977a-7dad57684253", "Update Multiple Addresses");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 186, true);
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "UpdateMultipleJobDocAddressesForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZButton UpdateThisRecordButton;
		protected ZArchitecture.GUI.ZButton UpdateAllRecordsButton;
		private ZArchitecture.GUI.ZPanel panel1;
		private Enterprise.ZArchitecture.ZLabel TitleLabel;
		protected ZArchitecture.GUI.ZButton CancelSelectButton;
		protected ZArchitecture.GUI.ZButton YesButton;
		private KTableLayoutPanel tableLayoutPanel1;
		private ZArchitecture.GUI.ZPanel panel2;
	}
}
