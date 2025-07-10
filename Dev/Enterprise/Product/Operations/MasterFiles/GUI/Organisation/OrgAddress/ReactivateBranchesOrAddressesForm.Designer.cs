namespace Enterprise.MasterFiles.GUI
{
	partial class ReactivateBranchesOrAddressesForm
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
			this.FormLayout = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.HeaderPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HeaderLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SelectUnselectAllCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FooterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ActivateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FormLayout.SuspendLayout();
			this.HeaderPanel.SuspendLayout();
			this.FooterPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 343, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(541, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ReactivateBranchOrAddressModel);
			// 
			// FormLayout
			// 
			this.FormLayout.ColumnCount = 1;
			this.FormLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.FormLayout.Controls.Add(this.HeaderPanel, 0, 0);
			this.FormLayout.Controls.Add(this.FooterPanel, 0, 2);
			this.FormLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FormLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FormLayout.Name = "FormLayout";
			this.FormLayout.RowCount = 3;
			this.FormLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(30)));
			this.FormLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.FormLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(30)));
			this.FormLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(541, 343, true);
			this.FormLayout.TabIndex = 1;
			// 
			// HeaderPanel
			// 
			this.HeaderPanel.Controls.Add(this.HeaderLabel);
			this.HeaderPanel.Controls.Add(this.SelectUnselectAllCheckBox);
			this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HeaderPanel.Name = "HeaderPanel";
			this.HeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 24, true);
			this.HeaderPanel.TabIndex = 3;
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HeaderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 1, true);
			this.HeaderLabel.Name = "HeaderLabel";
			this.HeaderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 20, true);
			this.HeaderLabel.TabIndex = 3;
			// 
			// SelectUnselectAllCheckBox
			// 
			this.SelectUnselectAllCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SelectUnselectAllCheckBox, "SelectedAll");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ReactivateBranchOrAddressModel)(null)).SelectedAll)));
			this.SelectUnselectAllCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SelectUnselectAllCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SelectUnselectAllCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 2, true);
			this.SelectUnselectAllCheckBox.Name = "SelectUnselectAllCheckBox";
			this.SelectUnselectAllCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 21, true);
			this.SelectUnselectAllCheckBox.TabIndex = 1;
			this.SelectUnselectAllCheckBox.UseVisualStyleBackColor = true;
			this.SelectUnselectAllCheckBox.CheckStateChanged += new System.EventHandler(this.SelectUnselectAllCheckBox_CheckStateChanged);
			// 
			// FooterPanel
			// 
			this.FooterPanel.Controls.Add(this.CancelButton);
			this.FooterPanel.Controls.Add(this.ActivateButton);
			this.FooterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FooterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 316, true);
			this.FooterPanel.Name = "FooterPanel";
			this.FooterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 24, true);
			this.FooterPanel.TabIndex = 4;
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8ee06c75-ebfc-471a-b332-a4d92b315a68", "Cancel");
			this.CancelButton.IsCaptionOverridden = false;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 0, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 1;
			this.CancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ActivateButton
			// 
			this.ActivateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ActivateButton.BackColor = System.Drawing.Color.Red;
			this.ActivateButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e0cefdb5-3ba2-40b0-b967-b8d7251a1d1e", "Activate");
			this.ActivateButton.IsCaptionOverridden = false;
			this.ActivateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 0, true);
			this.ActivateButton.Name = "ActivateButton";
			this.ActivateButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ActivateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ActivateButton.TabIndex = 0;
			this.ActivateButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ActivateButton.ToolTipCaption = null;
			this.ActivateButton.UseVisualStyleBackColor = false;
			this.ActivateButton.Click += new System.EventHandler(this.ActivateButton_Click);
			// 
			// ReactivateBranchesOrAddressesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(541, 367, true);
			this.Controls.Add(this.FormLayout);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.ReactivateBranchOrAddressModel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "ReactivateBranchesOrAddressesForm";
			this.Text = "ReactivateBranchForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FormLayout, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FormLayout.ResumeLayout(false);
			this.FormLayout.PerformLayout();
			this.HeaderPanel.ResumeLayout(false);
			this.HeaderPanel.PerformLayout();
			this.FooterPanel.ResumeLayout(false);
			this.FooterPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel FormLayout;
		private ZArchitecture.GUI.ZPanel HeaderPanel;
		protected ZArchitecture.GUI.ZCheckBox SelectUnselectAllCheckBox;
		private ZArchitecture.GUI.ZPanel FooterPanel;
		protected ZArchitecture.GUI.ZButton ActivateButton;
		protected new ZArchitecture.GUI.ZButton CancelButton;
		protected ZArchitecture.ZLabel HeaderLabel;
	}
}
