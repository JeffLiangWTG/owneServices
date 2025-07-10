using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.MarketingManager.Business;

namespace Enterprise.MarketingManager.GUI
{
	partial class GlbCompanyCampaignItemForm
	{
		#region Designer generated code
		IContainer components = null;
		ZArchitecture.GUI.ZDateEdit DateSentDateEdit;
		ZArchitecture.ZTextBox ContactNameTextBox;
		ZArchitecture.GUI.ZCodeFindBox zCodeFindBox1;
		ZArchitecture.GUI.ZCodeFindBox FollowedUpByCodeFindBox;
		ZArchitecture.GUI.ZDateEdit FollowedUpDateEdit;
		MasterFiles.GUI.ZOrganisationControl ContactOrganisationControl;
		ZArchitecture.ZLabel CampaignNameLabel;
		ZArchitecture.ZTextBox EmailTextBox;
		ZArchitecture.ZTextBox PhoneTextBox;
		ZArchitecture.ZLabel CampaignNameValueLabel;
		ZArchitecture.ZLabel zLabel2;
		ZArchitecture.GUI.ZDropEdit zDropEdit1;

		protected override void InitializeComponent()
		{
			this.DateSentDateEdit = new ZArchitecture.GUI.ZDateEdit();
			this.ContactNameTextBox = new ZArchitecture.ZTextBox();
			this.zCodeFindBox1 = new ZArchitecture.GUI.ZCodeFindBox();
			this.FollowedUpByCodeFindBox = new ZArchitecture.GUI.ZCodeFindBox();
			this.FollowedUpDateEdit = new ZArchitecture.GUI.ZDateEdit();
			this.ContactOrganisationControl = new MasterFiles.GUI.ZOrganisationControl();
			this.CampaignNameLabel = new ZArchitecture.ZLabel();
			this.EmailTextBox = new ZArchitecture.ZTextBox();
			this.PhoneTextBox = new ZArchitecture.ZTextBox();
			this.CampaignNameValueLabel = new ZArchitecture.ZLabel();
			this.zLabel2 = new ZArchitecture.ZLabel();
			this.zDropEdit1 = new ZArchitecture.GUI.ZDropEdit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 28, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 297, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zDropEdit1);
			this.MainTabPage.Controls.Add(this.zLabel2);
			this.MainTabPage.Controls.Add(this.CampaignNameValueLabel);
			this.MainTabPage.Controls.Add(this.PhoneTextBox);
			this.MainTabPage.Controls.Add(this.EmailTextBox);
			this.MainTabPage.Controls.Add(this.ContactNameTextBox);
			this.MainTabPage.Controls.Add(this.CampaignNameLabel);
			this.MainTabPage.Controls.Add(this.ContactOrganisationControl);
			this.MainTabPage.Controls.Add(this.FollowedUpDateEdit);
			this.MainTabPage.Controls.Add(this.FollowedUpByCodeFindBox);
			this.MainTabPage.Controls.Add(this.zCodeFindBox1);
			this.MainTabPage.Controls.Add(this.DateSentDateEdit);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 270, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(337);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(337);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GlbCompanyCampaignItem);
			// 
			// DateSentDateEdit
			// 
			this.DateSentDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateSentDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateSentDateEdit, "G8_SystemCreateTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).G8_SystemCreateTimeUtc);
			this.DateSentDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DateSentDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 134, true);
			this.DateSentDateEdit.Name = "DateSentDateEdit";
			this.DateSentDateEdit.TabIndex = 1;
			// 
			// ContactNameTextBox
			// 
			this.ContactNameTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ContactNameTextBox, "ContactName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).ContactName);
			this.ContactNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ContactNameTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignItemForm|072d2698-da21-4e41-8aed-a5ba312d6843", "Contact Name");
			this.ContactNameTextBox.IsDynamicMultiline = false;
			this.ContactNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 52, true);
			this.ContactNameTextBox.Name = "ContactNameTextBox";
			this.ContactNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.ContactNameTextBox.TabIndex = 0;
			// 
			// zCodeFindBox1
			// 
			this.BindingSource.SetBindingMember(this.zCodeFindBox1, "G8_SystemCreateUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).G8_SystemCreateUser);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).Lookups.FollowedUpBys);
			this.zCodeFindBox1.BindToList = "Lookups+FollowedUpBys";
			this.zCodeFindBox1.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignItemForm|006ac9cf-3ae0-4c0d-9289-64d90f42c729", "Sent by");
			this.zCodeFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 156, true);
			this.zCodeFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.zCodeFindBox1.Name = "zCodeFindBox1";
			this.zCodeFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.zCodeFindBox1.TabIndex = 2;
			// 
			// FollowedUpByCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.FollowedUpByCodeFindBox, "G8_GS_NKFollowedUpBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).G8_GS_NKFollowedUpBy);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).Lookups.FollowedUpBys);
			this.FollowedUpByCodeFindBox.BindToList = "Lookups+FollowedUpBys";
			this.FollowedUpByCodeFindBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignItemForm|9fc6caee-7867-4617-b4cc-3f9aa1659865", "Followed up by");
			this.FollowedUpByCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 215, true);
			this.FollowedUpByCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.FollowedUpByCodeFindBox.Name = "FollowedUpByCodeFindBox";
			this.FollowedUpByCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.FollowedUpByCodeFindBox.TabIndex = 4;
			// 
			// FollowedUpDateEdit
			// 
			this.FollowedUpDateEdit.AutoCompleteMonthThreshold = 1;
			this.FollowedUpDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FollowedUpDateEdit, "G8_FollowedUp");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).G8_FollowedUp);
			this.FollowedUpDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.FollowedUpDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 193, true);
			this.FollowedUpDateEdit.Name = "FollowedUpDateEdit";
			this.FollowedUpDateEdit.TabIndex = 3;
			// 
			// ContactOrganisationControl
			// 
			this.BindingSource.SetBindingMember(this.ContactOrganisationControl, "OrgPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).OrgPK);
			this.ContactOrganisationControl.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignItemForm|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Organization  Information");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ContactOrganisationControl, false);
			this.ContactOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 52, true);
			this.ContactOrganisationControl.Name = "ContactOrganisationControl";
			this.ContactOrganisationControl.PopupCaption = "";
			this.ContactOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.ContactOrganisationControl.TabIndex = 12;
			// 
			// CampaignNameLabel
			// 
			this.CampaignNameLabel.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignItemForm|f47e15c1-3d18-4f66-84cd-5f6a495f78fe", "Campaign");
			this.CampaignNameLabel.IsFontBold = true;
			this.CampaignNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 15, true);
			this.CampaignNameLabel.Name = "CampaignNameLabel";
			this.CampaignNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 18, true);
			this.CampaignNameLabel.TabIndex = 13;
			// 
			// EmailTextBox
			// 
			this.EmailTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.EmailTextBox, "EmailAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).EmailAddress);
			this.EmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.EmailTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignItemForm|826c3a49-9ef9-43d3-9bd3-473e91bcce0d", "Email");
			this.EmailTextBox.IsDynamicMultiline = false;
			this.EmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 74, true);
			this.EmailTextBox.Name = "EmailTextBox";
			this.EmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.EmailTextBox.TabIndex = 14;
			// 
			// PhoneTextBox
			// 
			this.PhoneTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PhoneTextBox, "WorkPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).WorkPhone);
			this.PhoneTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PhoneTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignItemForm|8fcef59f-d954-4b87-b09b-6e83cab728be", "Work Phone");
			this.PhoneTextBox.IsDynamicMultiline = false;
			this.PhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 97, true);
			this.PhoneTextBox.Name = "PhoneTextBox";
			this.PhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.PhoneTextBox.TabIndex = 16;
			// 
			// CampaignNameValueLabel
			// 
			this.BindingSource.SetBindingMember(this.CampaignNameValueLabel, "CampaignName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).CampaignName);
			this.CampaignNameValueLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CampaignNameValueLabel, false);
			this.CampaignNameValueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 15, true);
			this.CampaignNameValueLabel.Name = "CampaignNameValueLabel";
			this.CampaignNameValueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 18, true);
			this.CampaignNameValueLabel.TabIndex = 18;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("GlbCompanyCampaignItemForm|c66e1387-2e2a-4749-a35c-cbbff61662df", "Delivery Method");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 215, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 19, true);
			this.zLabel2.TabIndex = 19;
			// 
			// zDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit1, "G8_DeliveryMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).G8_DeliveryMethod);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GlbCompanyCampaignItem)(null)).Lookups.DeliveryMethods);
			this.zDropEdit1.BindToList = "Lookups+DeliveryMethods";
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(456, 215, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.zDropEdit1.TabIndex = 20;
			// 
			// GlbCompanyCampaignItemForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 353, true);
			this.DataSourceAssemblyName = "Enterprise.MarketingManager.Business";
			this.DataSourceType = typeof(GlbCompanyCampaignItem);
			this.DataSourceTypeName = "Enterprise.MarketingManager.Business.GlbCompanyCampaignItem";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 379, true);
			this.Name = "GlbCompanyCampaignItemForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
