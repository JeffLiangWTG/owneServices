namespace Enterprise.MasterFiles.GUI
{
	partial class ProductOrgMergerForm
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
		protected override void InitializeComponent()
		{
			this.ProceedButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelFormButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OverallPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OverallCaptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SingleRelationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SingleRelationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeactivateSingleRelationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MultiRelationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MultiRelationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeactivateMultiRelationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemoveMultiRelationButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OverallPanel.SuspendLayout();
			this.SingleRelationPanel.SuspendLayout();
			this.MultiRelationPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 371, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.DefaultOSMG);
			// 
			// ProceedButton
			// 
			this.ProceedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ProceedButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3e14d650-eb4a-4e64-b880-11de7d36fa30", "Proceed");
			this.ProceedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 345, true);
			this.ProceedButton.Name = "ProceedButton";
			this.ProceedButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ProceedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ProceedButton.TabIndex = 51;
			this.ProceedButton.ToolTipCaption = null;
			this.ProceedButton.UseVisualStyleBackColor = true;
			this.ProceedButton.Click += new System.EventHandler(this.ProceedButton_Click);
			// 
			// CancelFormButton
			// 
			this.CancelFormButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelFormButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("edec763f-5489-4c59-ad34-4b9aac62c99b", "Cancel");
			this.CancelFormButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelFormButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 345, true);
			this.CancelFormButton.Name = "CancelFormButton";
			this.CancelFormButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelFormButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelFormButton.TabIndex = 52;
			this.CancelFormButton.ToolTipCaption = null;
			this.CancelFormButton.UseVisualStyleBackColor = true;
			this.CancelFormButton.Click += new System.EventHandler(this.CancelFormButton_Click);
			// 
			// OverallPanel
			// 
			this.OverallPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OverallPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.OverallPanel.Controls.Add(this.OverallCaptionLabel);
			this.OverallPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.OverallPanel.Name = "OverallPanel";
			this.OverallPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 57, true);
			this.OverallPanel.TabIndex = 10;
			// 
			// OverallCaptionLabel
			// 
			this.OverallCaptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OverallCaptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OverallCaptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.OverallCaptionLabel.Name = "OverallCaptionLabel";
			this.OverallCaptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 49, true);
			this.OverallCaptionLabel.TabIndex = 21;
			this.OverallCaptionLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// SingleRelationPanel
			// 
			this.SingleRelationPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SingleRelationPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.SingleRelationPanel.Controls.Add(this.SingleRelationLabel);
			this.SingleRelationPanel.Controls.Add(this.DeactivateSingleRelationButton);
			this.SingleRelationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 69, true);
			this.SingleRelationPanel.Name = "SingleRelationPanel";
			this.SingleRelationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 133, true);
			this.SingleRelationPanel.TabIndex = 11;
			// 
			// SingleRelationLabel
			// 
			this.SingleRelationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SingleRelationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SingleRelationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.SingleRelationLabel.Name = "SingleRelationLabel";
			this.SingleRelationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 99, true);
			this.SingleRelationLabel.TabIndex = 31;
			this.SingleRelationLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// DeactivateSingleRelationButton
			// 
			this.DeactivateSingleRelationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DeactivateSingleRelationButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("cc8c0c10-adde-4ce7-872d-aa6bd3884cd6", "Deactivate Products");
			this.DeactivateSingleRelationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 105, true);
			this.DeactivateSingleRelationButton.Name = "DeactivateSingleRelationButton";
			this.DeactivateSingleRelationButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeactivateSingleRelationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 23, true);
			this.DeactivateSingleRelationButton.TabIndex = 32;
			this.DeactivateSingleRelationButton.ToolTipCaption = null;
			this.DeactivateSingleRelationButton.UseVisualStyleBackColor = true;
			this.DeactivateSingleRelationButton.Click += new System.EventHandler(this.DeactivateSingleRelationButton_Click);
			// 
			// MultiRelationPanel
			// 
			this.MultiRelationPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MultiRelationPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.MultiRelationPanel.Controls.Add(this.MultiRelationLabel);
			this.MultiRelationPanel.Controls.Add(this.DeactivateMultiRelationButton);
			this.MultiRelationPanel.Controls.Add(this.RemoveMultiRelationButton);
			this.MultiRelationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 207, true);
			this.MultiRelationPanel.Name = "MultiRelationPanel";
			this.MultiRelationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 133, true);
			this.MultiRelationPanel.TabIndex = 12;
			// 
			// MultiRelationLabel
			// 
			this.MultiRelationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MultiRelationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MultiRelationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.MultiRelationLabel.Name = "MultiRelationLabel";
			this.MultiRelationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 99, true);
			this.MultiRelationLabel.TabIndex = 41;
			this.MultiRelationLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// DeactivateMultiRelationButton
			// 
			this.DeactivateMultiRelationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DeactivateMultiRelationButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a38a1ce5-5dc5-4222-a7a0-7f3912c95f4c", "Deactivate Products");
			this.DeactivateMultiRelationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 105, true);
			this.DeactivateMultiRelationButton.Name = "DeactivateMultiRelationButton";
			this.DeactivateMultiRelationButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeactivateMultiRelationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 23, true);
			this.DeactivateMultiRelationButton.TabIndex = 43;
			this.DeactivateMultiRelationButton.ToolTipCaption = null;
			this.DeactivateMultiRelationButton.UseVisualStyleBackColor = true;
			this.DeactivateMultiRelationButton.Click += new System.EventHandler(this.DeactivateMultiRelationButton_Click);
			// 
			// RemoveMultiRelationButton
			// 
			this.RemoveMultiRelationButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RemoveMultiRelationButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d2f08f98-3785-4835-abcd-5510f5542605", "Remove Relations");
			this.RemoveMultiRelationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 105, true);
			this.RemoveMultiRelationButton.Name = "RemoveMultiRelationButton";
			this.RemoveMultiRelationButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RemoveMultiRelationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 23, true);
			this.RemoveMultiRelationButton.TabIndex = 42;
			this.RemoveMultiRelationButton.ToolTipCaption = null;
			this.RemoveMultiRelationButton.UseVisualStyleBackColor = true;
			this.RemoveMultiRelationButton.Click += new System.EventHandler(this.RemoveMultiRelationButton_Click);
			// 
			// ProductOrgMergerForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelFormButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("546d8be3-b996-4f0e-8bcd-cd046dea7344", "Duplicate Products");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 395, true);
			this.Controls.Add(this.MultiRelationPanel);
			this.Controls.Add(this.SingleRelationPanel);
			this.Controls.Add(this.OverallPanel);
			this.Controls.Add(this.CancelFormButton);
			this.Controls.Add(this.ProceedButton);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.DefaultOSMG);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 433, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 433, true);
			this.Name = "ProductOrgMergerForm";
			this.Controls.SetChildIndex(this.ProceedButton, 0);
			this.Controls.SetChildIndex(this.CancelFormButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OverallPanel, 0);
			this.Controls.SetChildIndex(this.SingleRelationPanel, 0);
			this.Controls.SetChildIndex(this.MultiRelationPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OverallPanel.ResumeLayout(false);
			this.OverallPanel.PerformLayout();
			this.SingleRelationPanel.ResumeLayout(false);
			this.SingleRelationPanel.PerformLayout();
			this.MultiRelationPanel.ResumeLayout(false);
			this.MultiRelationPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZButton ProceedButton;
		private ZArchitecture.GUI.ZButton CancelFormButton;
		private ZArchitecture.GUI.ZPanel OverallPanel;
		private ZArchitecture.GUI.ZPanel SingleRelationPanel;
		internal ZArchitecture.GUI.ZButton DeactivateSingleRelationButton;
		private ZArchitecture.GUI.ZPanel MultiRelationPanel;
		internal ZArchitecture.GUI.ZButton DeactivateMultiRelationButton;
		internal ZArchitecture.GUI.ZButton RemoveMultiRelationButton;
		private ZArchitecture.ZLabel OverallCaptionLabel;
		private ZArchitecture.ZLabel SingleRelationLabel;
		private ZArchitecture.ZLabel MultiRelationLabel;
	}
}