using System;
using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	partial class SendCampaignsControl
	{
		#region Component Designer generated code

		private ZArchitecture.ZLabel SearchRecordsLabel;
		private ZToolStrip ToolStrip;
		private ZToolStrip buttonsToolStrip;
		internal ZToolStripDropDownButton dropDownButtonsToolStrip;
		internal ZToolStripMenuItem SendButton;
		internal ZToolStripMenuItem ScheduleSendButton;
		System.ComponentModel.IContainer components = null;
		private bool isDripMarketingMode;

		private void InitializeComponent()
		{
			this.ToolStrip = new ZToolStrip();
			this.SearchRecordsLabel = new ZArchitecture.ZLabel();
			this.buttonsToolStrip = new ZToolStrip();
			this.dropDownButtonsToolStrip = new ZToolStripDropDownButton();
			this.ScheduleSendButton = new ZToolStripMenuItem();
			this.SendButton = new ZToolStripMenuItem();
			this.FindRecipientContactsGroupBox = new ZGroupBox();
			this.SourceCampaignPKGuidFindBox = new ZGuidFindBox();
			this.ContactDataSourceDropEdit = new ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ToolStrip.SuspendLayout();
			this.FindRecipientContactsGroupBox.SuspendLayout();
			this.SourceCampaignPKGuidFindBox.SuspendLayout();
			this.ContactDataSourceDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GlbCompanyCampaign);
			// 
			// ToolStrip
			// 
			this.ToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ToolStrip.Name = "ToolStrip";
			this.ToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 20, true);
			this.ToolStrip.TabIndex = 0;
			// 
			// SearchRecordsLabel
			// 
			this.SearchRecordsLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SearchRecordsLabel, false);
			this.SearchRecordsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 0, true);
			this.SearchRecordsLabel.Name = "SearchRecordsLabel";
			this.SearchRecordsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 39, true);
			this.SearchRecordsLabel.TabIndex = 0;
			// 
			// buttonsToolStrip
			// 

			this.buttonsToolStrip.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaign)(null)).SourceCampaignPK);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaign)(null)).ContactDataSource);
			this.buttonsToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.buttonsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.buttonsToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.buttonsToolStrip.Items.AddRange(new ToolStripItem[] {
						this.dropDownButtonsToolStrip });
			this.buttonsToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(762, 8, true);
			this.buttonsToolStrip.Name = "buttonsToolStrip";
			this.buttonsToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 22, true);
			this.buttonsToolStrip.TabIndex = 8;
			this.buttonsToolStrip.Text = "zToolStrip1";
			// 
			// dropDownButtonsToolStrip
			// 
			this.dropDownButtonsToolStrip.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("d243ef2c-e4d9-49ff-ae47-d27fc5aa04f5", "Send to Selected");
			this.dropDownButtonsToolStrip.DropDownItems.AddRange(new ToolStripItem[] {
						this.ScheduleSendButton,
						this.SendButton });
			this.dropDownButtonsToolStrip.Image = global::Enterprise.MarketingManager.GUI.Properties.Resources.CrowdImage;
			this.dropDownButtonsToolStrip.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.dropDownButtonsToolStrip.Name = "dropDownButtonsToolStrip";
			this.dropDownButtonsToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 25, true);
			this.dropDownButtonsToolStrip.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ScheduleSendButton
			// 
			this.ScheduleSendButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("f38662b6-223c-4837-bfc9-430bc425bb27", "Schedule Send");
			this.ScheduleSendButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ScheduleSendButton.Name = "ScheduleSendButton";
			this.ScheduleSendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.ScheduleSendButton.Click += new EventHandler(ScheduleSendButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("54dbc46e-eabb-4c3e-b7fd-cac79af18146", "Send Now");
			this.SendButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.SendButton.Click += new EventHandler(SendToSelectedButton_Click);
			// 
			// FindRecipientContactsGroupBox
			// 
			this.FindRecipientContactsGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("da7fe690-baec-4bfd-ad05-e4776b8e9483", "Find Recipient Contacts");
			this.FindRecipientContactsGroupBox.Controls.Add(this.SourceCampaignPKGuidFindBox);
			this.FindRecipientContactsGroupBox.Controls.Add(this.ContactDataSourceDropEdit);
			this.FindRecipientContactsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FindRecipientContactsGroupBox.Name = "FindRecipientContactsGroupBox";
			this.FindRecipientContactsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 43, true);
			this.FindRecipientContactsGroupBox.TabIndex = 0;
			this.FindRecipientContactsGroupBox.TabStop = false;
			// 
			// SourceCampaignPKGuidFindBox
			// 
			this.SourceCampaignPKGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SourceCampaignPKGuidFindBox, "SourceCampaignPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaign)(null)).SourceCampaignPK);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SourceCampaignPKGuidFindBox, false);
			this.SourceCampaignPKGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 16, true);
			this.SourceCampaignPKGuidFindBox.Name = "SourceCampaignPKGuidFindBox";
			this.SourceCampaignPKGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 18, true);
			this.SourceCampaignPKGuidFindBox.TabIndex = 1;
			// 
			// ContactDataSourceDropEdit
			// 
			this.ContactDataSourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactDataSourceDropEdit, "ContactDataSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaign)(null)).ContactDataSource);
			this.ContactDataSourceDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("6ccc7710-5380-48df-a655-27cb0bcc3106", "Data Source");
			this.ContactDataSourceDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactDataSourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 16, true);
			this.ContactDataSourceDropEdit.Name = "ContactDataSourceDropEdit";
			this.ContactDataSourceDropEdit.ShowDescriptionBox = false;
			this.ContactDataSourceDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ContactDataSourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 18, true);
			this.ContactDataSourceDropEdit.TabIndex = 0;
			// 
			// SendCampaignsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "SendCampaignsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 560, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ToolStrip.ResumeLayout(false);
			this.ToolStrip.PerformLayout();
			this.FindRecipientContactsGroupBox.ResumeLayout(false);
			this.FindRecipientContactsGroupBox.PerformLayout();
			this.SourceCampaignPKGuidFindBox.ResumeLayout(true);
			this.SourceCampaignPKGuidFindBox.PerformLayout();
			this.ContactDataSourceDropEdit.ResumeLayout(true);
			this.ContactDataSourceDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
