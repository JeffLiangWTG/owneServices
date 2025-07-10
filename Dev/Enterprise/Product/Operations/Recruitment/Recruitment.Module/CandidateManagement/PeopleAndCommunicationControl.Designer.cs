using System.Drawing;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Recruitment.Module.CandidateManagement
{
	partial class PeopleAndCommunicationControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.peopleAndCommunicationTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.emailTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.permittedToContactReferencesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.openInMailClientButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.attachTipLabel = new Enterprise.ZArchitecture.ZLabel();
			this.contactsZGrid = new Enterprise.ZArchitecture.GUI.ZFilterGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.peopleAndCommunicationTabControl.SuspendLayout();
			this.emailTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.contactsZGrid)).BeginInit();
			this.contactsZGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruitment.Common.CommunicationContactRowBusinessObjectCollection);
			// 
			// peopleAndCommunicationTabControl
			// 
			this.peopleAndCommunicationTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.peopleAndCommunicationTabControl.Controls.Add(this.emailTabPage);
			this.peopleAndCommunicationTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.peopleAndCommunicationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.peopleAndCommunicationTabControl.Name = "peopleAndCommunicationTabControl";
			this.peopleAndCommunicationTabControl.SelectedIndex = 0;
			this.peopleAndCommunicationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 469, true);
			this.peopleAndCommunicationTabControl.TabIndex = 0;
			// 
			// emailTabPage
			// 
			this.emailTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.emailTabPage.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("2fe0e596-897a-40b2-b237-7cabfae1160f", "Email");
			this.emailTabPage.Controls.Add(this.permittedToContactReferencesCheckBox);
			this.emailTabPage.Controls.Add(this.openInMailClientButton);
			this.emailTabPage.Controls.Add(this.attachTipLabel);
			this.emailTabPage.Controls.Add(this.contactsZGrid);
			this.emailTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.emailTabPage.Name = "emailTabPage";
			this.emailTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.emailTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 442, true);
			this.emailTabPage.TabIndex = 0;
			// 
			// permittedToContactReferencesCheckBox
			// 
			this.permittedToContactReferencesCheckBox.AutoSize = true;
			this.permittedToContactReferencesCheckBox.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("2675540c-1639-4718-a02a-2ad6a185390f", "Permitted to contact references");
			this.permittedToContactReferencesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.permittedToContactReferencesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 13, true);
			this.permittedToContactReferencesCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 10, 10, 5, true);
			this.permittedToContactReferencesCheckBox.Name = "permittedToContactReferencesCheckBox";
			this.permittedToContactReferencesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 17, true);
			this.permittedToContactReferencesCheckBox.TabIndex = 5;
			this.permittedToContactReferencesCheckBox.UseVisualStyleBackColor = true;
			// 
			// openInMailClientButton
			// 
			this.openInMailClientButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.openInMailClientButton.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("28bf6652-7e07-4dbe-9fbc-88de8134425e", "Open in Mail Client");
			this.openInMailClientButton.IsCaptionOverridden = false;
			this.openInMailClientButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 390, true);
			this.openInMailClientButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, true);
			this.openInMailClientButton.Name = "openInMailClientButton";
			this.openInMailClientButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.openInMailClientButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 25, true);
			this.openInMailClientButton.TabIndex = 4;
			this.openInMailClientButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.openInMailClientButton.ToolTipCaption = null;
			this.openInMailClientButton.UseVisualStyleBackColor = true;
			this.openInMailClientButton.Click += new System.EventHandler(this.OpenInMailClientButton_Click);
			// 
			// attachTipLabel
			// 
			this.attachTipLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.attachTipLabel.Font = new System.Drawing.Font("Tahoma", 8f, System.Drawing.FontStyle.Italic);
			this.attachTipLabel.BackColor = System.Drawing.Color.Yellow;
			this.attachTipLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 415, true);
			this.attachTipLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.attachTipLabel.Name = "attachTipLabel";
			this.attachTipLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 23, true);
			this.attachTipLabel.TabIndex = 1;
			this.attachTipLabel.Text = Res.GetString("E7ED53D3-1C05-44B1-8172-D4FA7A65EA16", "Tip: you can copy && paste eDocs to the email from the eDocs tab.");
			// 
			// contactsZGrid
			// 
			this.contactsZGrid.AllowNavigation = false;
			this.contactsZGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.contactsZGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruitment.Common.CommunicationContactRow)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruitment.Common.CommunicationContactRow)(null)).Selected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.CommunicationContactRow)(null)).Contact.Position)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruitment.Common.CommunicationContactRow)(null)).Contact.Positions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.CommunicationContactRow)(null)).Contact.Staff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruitment.Common.CommunicationContactRow)(null)).Contact.Email)));
			this.contactsZGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("8bf40ab0-03dd-4ced-8a0c-78d3e1c2c53b", "Selected");
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "Contact.Positions";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("6b137c2a-c58a-4a0c-92a0-15c676879246", "Position");
			zDropEditColumnStyleInfo1.ColumnName = "Contact+Position";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCodeFindBoxColumnStyleInfo1.Caption = "";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("342b5e50-beaf-4b1d-bc89-d5a1e7054613", "Staff");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Contact+Staff";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruitment.Module.Res.GetData("a5d62be8-c5f6-4042-917a-32cfc9276faf", "Email");
			zTextBoxColumnStyleInfo1.ColumnName = "Contact+Email";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.contactsZGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.contactsZGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.contactsZGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.contactsZGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.contactsZGrid.GridId = "9558ed50-40a7-43be-8c65-49542612a61a";
			this.contactsZGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.contactsZGrid.LayoutKey = "contactsZGrid";
			this.contactsZGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 40, true);
			this.contactsZGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 5, 10, 10, true);
			this.contactsZGrid.Name = "contactsZGrid";
			this.contactsZGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.contactsZGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 344, true);
			this.contactsZGrid.TabIndex = 3;
			// 
			// PeopleAndCommunicationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.peopleAndCommunicationTabControl);
			this.Name = "PeopleAndCommunicationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 469, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.peopleAndCommunicationTabControl.ResumeLayout(false);
			this.peopleAndCommunicationTabControl.PerformLayout();
			this.emailTabPage.ResumeLayout(false);
			this.emailTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.contactsZGrid)).EndInit();
			this.contactsZGrid.ResumeLayout(false);
			this.contactsZGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl peopleAndCommunicationTabControl;
		private ZArchitecture.GUI.ZTabPage emailTabPage;
		private ZArchitecture.GUI.ZCheckBox permittedToContactReferencesCheckBox;
		private ZArchitecture.GUI.ZButton openInMailClientButton;
		private ZArchitecture.GUI.ZFilterGrid contactsZGrid;
		private ZArchitecture.ZLabel attachTipLabel;
	}
}
