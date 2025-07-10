namespace Enterprise.MarketingManager.GUI
{
	partial class SendCampaignForContactForm
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
		new void InitializeComponent()
		{
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CampaignFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SendCampaignButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContactNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrganisationNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContactEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 190, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.SendCampaignForContactBizO);
			// 
			// MessageLabel
			// 
			this.MessageLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("SendCampaignForContactForm|0cf36516-fe88-4a2f-83f5-2400bc0fb2c7", "You are about to send campaign email to this contact");
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 9, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 32, true);
			this.MessageLabel.TabIndex = 1;
			// 
			// CampaignFindBox
			// 
			this.CampaignFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CampaignFindBox, "CampaignPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MarketingManager.Business.SendCampaignForContactBizO)(null)).CampaignPK)));
			this.CampaignFindBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("SendCampaignForContactForm|b3f500bf-5e88-4f59-a48c-bef5f06945c9", "Campaign");
			this.CampaignFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 128, true);
			this.CampaignFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbCompanyCampaignWithoutFilter;
			this.CampaignFindBox.Name = "CampaignFindBox";
			this.CampaignFindBox.PreBoundMaxLength = 20;
			this.CampaignFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(519, 20, true);
			this.CampaignFindBox.TabIndex = 5;
			// 
			// SendCampaignButton
			// 
			this.SendCampaignButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SendCampaignButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("SendCampaignForContactForm|3509031c-ed8c-4d4f-8273-35a3cff68b34", "Send Campaign");
			this.SendCampaignButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 154, true);
			this.SendCampaignButton.Name = "SendCampaignButton";
			this.SendCampaignButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.SendCampaignButton.TabIndex = 6;
			this.SendCampaignButton.Click += new System.EventHandler(this.SendCampaignButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("SendCampaignForContactForm|dbc864ef-f487-4246-b6f8-f006b0f2da39", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 154, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ContactNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactNameTextBox, "Contact.OC_ContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.SendCampaignForContactBizO)(null)).Contact.OC_ContactName)));
			this.ContactNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 76, true);
			this.ContactNameTextBox.Name = "ContactNameTextBox";
			this.ContactNameTextBox.ReadOnly = true;
			this.ContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ContactNameTextBox.TabIndex = 3;
			// 
			// OrganisationNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.OrganisationNameTextBox, "Contact.ParentOrg.OH_FullName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.SendCampaignForContactBizO)(null)).Contact.ParentOrg.OH_FullName)));
			this.OrganisationNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OrganisationNameTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("SendCampaignForContactForm|3ffd66f1-07f6-45e8-b3b8-d69e428b95bf", "Organization");
			this.OrganisationNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 50, true);
			this.OrganisationNameTextBox.Name = "OrganisationNameTextBox";
			this.OrganisationNameTextBox.ReadOnly = true;
			this.OrganisationNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.OrganisationNameTextBox.TabIndex = 2;
			// 
			// ContactEmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.ContactEmailTextBox, "Contact.OC_Email");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MarketingManager.Business.SendCampaignForContactBizO)(null)).Contact.OC_Email)));
			this.ContactEmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 102, true);
			this.ContactEmailTextBox.Name = "ContactEmailTextBox";
			this.ContactEmailTextBox.ReadOnly = true;
			this.ContactEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ContactEmailTextBox.TabIndex = 4;
			// 
			// SendCampaignForContactForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 214, true);
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("SendCampaignForContactForm|40e23e7f-ddfc-47b5-96db-67dfef304d61", "Send Campaign", "Send Campaign to Contact", "");
			this.Controls.Add(this.OrganisationNameTextBox);
			this.Controls.Add(this.ContactEmailTextBox);
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.CampaignFindBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ContactNameTextBox);
			this.Controls.Add(this.SendCampaignButton);
			this.DataSourceType = typeof(Enterprise.MarketingManager.Business.SendCampaignForContactBizO);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 250, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 250, true);
			this.Name = "SendCampaignForContactForm";
			this.Controls.SetChildIndex(this.SendCampaignButton, 0);
			this.Controls.SetChildIndex(this.ContactNameTextBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.CampaignFindBox, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.ContactEmailTextBox, 0);
			this.Controls.SetChildIndex(this.OrganisationNameTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel MessageLabel;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CampaignFindBox;
		private Enterprise.ZArchitecture.GUI.ZButton SendCampaignButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.ZTextBox ContactNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox OrganisationNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox ContactEmailTextBox;
	}
}
