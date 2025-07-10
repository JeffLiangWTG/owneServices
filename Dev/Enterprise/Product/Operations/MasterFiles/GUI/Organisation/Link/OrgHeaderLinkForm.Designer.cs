using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class OrgHeaderLinkForm
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
		new public void InitializeComponent()
		{
			this.OrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.SaveAndCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelLinkingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Organisation1Button = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.Organisation2Button = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.OtherButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.LinkTitle = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrganisationFindBox.SuspendLayout();
			this.LinkTitle.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 148, true);
			this.MainStatusBar.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 25, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeaderLink);
			// 
			// OrganisationFindBox
			// 
			this.OrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganisationFindBox, "OtherOrganisationPk");
			this.OrganisationFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("034114c5-f759-4fea-b776-2c00e7646e36", " ");
			this.OrganisationFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 80, true);
			this.OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrganisationFindBox.Name = "OrganisationFindBox";
			this.OrganisationFindBox.ShouldResize = true;
			this.OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 20, true);
			this.OrganisationFindBox.TabIndex = 4;
			// 
			// SaveAndCloseButton
			// 
			this.SaveAndCloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e2492426-98c3-46cb-9150-fcdbedbe6799", "Save && Close");
			this.SaveAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(218, 108, true);
			this.SaveAndCloseButton.Name = "SaveAndCloseButton";
			this.SaveAndCloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.SaveAndCloseButton.TabIndex = 5;
			this.SaveAndCloseButton.ToolTipCaption = null;
			this.SaveAndCloseButton.UseVisualStyleBackColor = true;
			this.SaveAndCloseButton.Click += new System.EventHandler(this.SaveAndCloseButton_Click);
			// 
			// CancelLinkingButton
			// 
			this.CancelLinkingButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("af3da7de-53d3-4cf7-8d8e-aa7f253fac91", "Cancel");
			this.CancelLinkingButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelLinkingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(337, 108, true);
			this.CancelLinkingButton.Name = "CancelLinkingButton";
			this.CancelLinkingButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelLinkingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 23, true);
			this.CancelLinkingButton.TabIndex = 6;
			this.CancelLinkingButton.ToolTipCaption = null;
			this.CancelLinkingButton.UseVisualStyleBackColor = true;
			this.CancelLinkingButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// Organisation1Button
			// 
			this.Organisation1Button.AutoCheck = false;
			this.Organisation1Button.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("27bfd3ec-d191-478c-8ca3-17ff27f31530", "Current Organization");
			this.Organisation1Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Organisation1Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 23, true);
			this.Organisation1Button.Name = "Organisation1Button";
			this.Organisation1Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 24, true);
			this.Organisation1Button.TabIndex = 1;
			this.Organisation1Button.TabStop = true;
			this.Organisation1Button.Text = "Current Organization";
			this.Organisation1Button.UseVisualStyleBackColor = true;
			// 
			// Organisation2Button
			// 
			this.Organisation2Button.AutoCheck = false;
			this.Organisation2Button.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d342db34-f112-4269-addc-935adc7112e5", "Potential Duplicate");
			this.Organisation2Button.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.Organisation2Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 49, true);
			this.Organisation2Button.Name = "Organisation2Button";
			this.Organisation2Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 24, true);
			this.Organisation2Button.TabIndex = 2;
			this.Organisation2Button.TabStop = true;
			this.Organisation2Button.UseVisualStyleBackColor = true;
			// 
			// OtherButton
			// 
			this.OtherButton.AutoCheck = false;
			this.OtherButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("642cabc0-27fd-4271-afaf-94149a7b2f4a", "Other");
			this.OtherButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OtherButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 75, true);
			this.OtherButton.Name = "OtherButton";
			this.OtherButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 24, true);
			this.OtherButton.TabIndex = 3;
			this.OtherButton.TabStop = true;
			this.OtherButton.UseVisualStyleBackColor = true;
			// 
			// LinkTitle
			// 
			this.LinkTitle.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f9ada93b-d277-4a0a-b333-1703f396d4a8", "Which organization represents the head office?");
			this.LinkTitle.Controls.Add(this.OtherButton);
			this.LinkTitle.Controls.Add(this.CancelLinkingButton);
			this.LinkTitle.Controls.Add(this.Organisation1Button);
			this.LinkTitle.Controls.Add(this.Organisation2Button);
			this.LinkTitle.Controls.Add(this.SaveAndCloseButton);
			this.LinkTitle.Controls.Add(this.OrganisationFindBox);
			this.LinkTitle.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.LinkTitle.Name = "LinkTitle";
			this.LinkTitle.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 139, true);
			this.LinkTitle.TabIndex = 7;
			this.LinkTitle.TabStop = false;
			// 
			// OrgHeaderLinkForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelLinkingButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ed3db375-b42b-451e-a339-d61b463bc095", "Link Organizations");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 176, true);
			this.Controls.Add(this.LinkTitle);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeaderLink);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "OrgHeaderLinkForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Controls.SetChildIndex(this.LinkTitle, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrganisationFindBox.ResumeLayout(true);
			this.OrganisationFindBox.PerformLayout();
			this.LinkTitle.ResumeLayout(false);
			this.LinkTitle.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZOrganisationFindBox OrganisationFindBox;
		internal ZArchitecture.GUI.ZButton SaveAndCloseButton;
		internal ZArchitecture.GUI.ZButton CancelLinkingButton;
		private ZArchitecture.GUI.ZRadioButton Organisation1Button;
		private ZArchitecture.GUI.ZRadioButton Organisation2Button;
		private ZArchitecture.GUI.ZRadioButton OtherButton;
		private ZArchitecture.GUI.ZGroupBox LinkTitle;
	}
}