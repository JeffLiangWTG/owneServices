
using System.Windows.Forms;

namespace Enterprise.Customs.GUI
{
	partial class ProductCreationConfirmationForm
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
			this.ProductRelationOptionForImporter = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ConfirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.QuitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InformationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WarningLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BehaviourLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ProductRelationOptionForSupplier = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 220, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// ProductRelationOptionForImporter
			// 
			this.ProductRelationOptionForImporter.AutoCheck = false;
			this.ProductRelationOptionForImporter.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ProductRelationOptionForImporter|41C0FC05-FECD-4762-B1AD-9D3BF37258B6", "Option 2");
			this.ProductRelationOptionForImporter.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ProductRelationOptionForImporter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 55, true);
			this.ProductRelationOptionForImporter.Name = "ProductRelationOptionForImporter";
			this.ProductRelationOptionForImporter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 24, true);
			this.ProductRelationOptionForImporter.TabIndex = 3;
			this.ProductRelationOptionForImporter.UseVisualStyleBackColor = true;
			this.ProductRelationOptionForImporter.Click += new System.EventHandler(ProductRelationOption_Click);
			// 
			// ConfirmButton
			// 
			this.ConfirmButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ConfirmButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ConfirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(394, 194, true);
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ConfirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 23, true);
			this.ConfirmButton.TabIndex = 6;
			this.ConfirmButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5416577C-A1C4-47C3-B549-CF1E8C8D5004", "Confirm");
			this.ConfirmButton.ToolTipCaption = null;
			this.ConfirmButton.UseVisualStyleBackColor = true;
			this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// QuitButton
			// 
			this.QuitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.QuitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.QuitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 194, true);
			this.QuitButton.Name = "QuitButton";
			this.QuitButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.QuitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 23, true);
			this.QuitButton.TabIndex = 7;
			this.QuitButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("EE781FA9-8E30-4177-9534-5227220CA65A", "Cancel");
			this.QuitButton.ToolTipCaption = null;
			this.QuitButton.UseVisualStyleBackColor = true;
			this.QuitButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// InformationLabel
			// 
			this.InformationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InformationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.InformationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.InformationLabel.Name = "InformationLabel";
			this.InformationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 44, true);
			this.InformationLabel.TabIndex = 1;
			this.InformationLabel.Text = "If you are seeing this, something went wrong.";
			this.InformationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// WarningLabel
			// 
			this.WarningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.WarningLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WarningLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 105, true);
			this.WarningLabel.Name = "WarningLabel";
			this.WarningLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 21, true);
			this.WarningLabel.TabIndex = 4;
			this.WarningLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1D1A7C96-CD56-4523-AACE-00E5503B6E94", "New products were found without Sales Unit of Measure, these products will not be added.");
			this.WarningLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// BehaviourLabel
			// 
			this.BehaviourLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BehaviourLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.BehaviourLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 133, true);
			this.BehaviourLabel.Name = "BehaviourLabel";
			this.BehaviourLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 21, true);
			this.BehaviourLabel.TabIndex = 5;
			this.BehaviourLabel.Text = "If you are seeing this, something went wrong.";
			this.BehaviourLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ProductRelationOptionForSupplier
			// 
			this.ProductRelationOptionForSupplier.AutoCheck = false;
			this.ProductRelationOptionForSupplier.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ProductRelationOptionForSupplier|FD15A2C3-2C0B-42C5-858E-353C13C3BDEC", "Option 3");
			this.ProductRelationOptionForSupplier.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ProductRelationOptionForSupplier.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 75, true);
			this.ProductRelationOptionForSupplier.Name = "ProductRelationOptionForSupplier";
			this.ProductRelationOptionForSupplier.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 24, true);
			this.ProductRelationOptionForSupplier.TabIndex = 8;
			this.ProductRelationOptionForSupplier.UseVisualStyleBackColor = true;
			this.ProductRelationOptionForSupplier.Click += new System.EventHandler(ProductRelationOption_Click);
			// 
			// ProductCreationConfirmationForm
			// 
			this.AcceptButton = this.ConfirmButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.QuitButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 244, true);
			this.Controls.Add(this.ProductRelationOptionForSupplier);
			this.Controls.Add(this.BehaviourLabel);
			this.Controls.Add(this.InformationLabel);
			this.Controls.Add(this.QuitButton);
			this.Controls.Add(this.ConfirmButton);
			this.Controls.Add(this.ProductRelationOptionForImporter);
			this.Controls.Add(this.WarningLabel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 251, true);
			this.Name = "ProductCreationConfirmationForm";
			this.Text = "ProductCreationConfirmationForm";
			this.Controls.SetChildIndex(this.WarningLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ProductRelationOptionForImporter, 0);
			this.Controls.SetChildIndex(this.ConfirmButton, 0);
			this.Controls.SetChildIndex(this.QuitButton, 0);
			this.Controls.SetChildIndex(this.InformationLabel, 0);
			this.Controls.SetChildIndex(this.BehaviourLabel, 0);
			this.Controls.SetChildIndex(this.ProductRelationOptionForSupplier, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZRadioButton ProductRelationOptionForImporter;
		private ZArchitecture.GUI.ZButton ConfirmButton;
		private ZArchitecture.GUI.ZButton QuitButton;
		private ZArchitecture.ZLabel InformationLabel;
		internal ZArchitecture.ZLabel WarningLabel;
		private ZArchitecture.ZLabel BehaviourLabel;
		internal ZArchitecture.GUI.ZRadioButton ProductRelationOptionForSupplier;
	}
}
