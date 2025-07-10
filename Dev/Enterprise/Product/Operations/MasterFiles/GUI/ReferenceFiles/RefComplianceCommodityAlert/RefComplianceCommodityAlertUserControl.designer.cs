using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefComplianceCommodityAlertUserControl
	{
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RefComplianceCommodityAlertUserControl));
			this.ConfigurationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CautionDetailsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CautionCaptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RiskStatusDropEdit = new Enterprise.MasterFiles.GUI.CommodityAlertRiskStatusDropEdit();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SourceURLCaptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SourceURLLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LastEditedTimeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CreatedTimeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AlertDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryRegionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TradeDirectionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AlertTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AlertNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConfigurationGroupBox.SuspendLayout();
			this.RiskStatusDropEdit.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.TradeDirectionDropEdit.SuspendLayout();
			this.AlertTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefComplianceCommodityAlert);
			// 
			// ConfigurationGroupBox
			// 
			this.ConfigurationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6dce61a2-92fb-402e-afd6-8427a0b4c725", "ComplianceWise Risk Configuration");
			this.ConfigurationGroupBox.Controls.Add(this.CautionDetailsLabel);
			this.ConfigurationGroupBox.Controls.Add(this.CautionCaptionLabel);
			this.ConfigurationGroupBox.Controls.Add(this.RiskStatusDropEdit);
			this.ConfigurationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ConfigurationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConfigurationGroupBox.Name = "ConfigurationGroupBox";
			this.ConfigurationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1293, 137, true);
			this.ConfigurationGroupBox.TabIndex = 0;
			this.ConfigurationGroupBox.TabStop = false;
			// 
			// CautionDetailsLabel
			// 
			this.CautionDetailsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8f937231-4e7e-453a-ab19-e0a1d8818cc1", "This setting will affect the calculation of the commodity risk status within the \"Compliance Risk\" tab of all jobs. Please note that by changing the status to Possible Risk, jobs associated with this alert will not be blocked.");
			this.CautionDetailsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CautionDetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 67, true);
			this.CautionDetailsLabel.Name = "CautionDetailsLabel";
			this.CautionDetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 50, true);
			this.CautionDetailsLabel.TabIndex = 2;
			this.CautionDetailsLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			this.CautionDetailsLabel.UseMnemonic = false;
			// 
			// CautionCaptionLabel
			// 
			this.CautionCaptionLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7d30e7be-cfad-4aba-a9d7-a179c994950a", "CAUTION:");
			this.CautionCaptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CautionCaptionLabel.IsFontBold = true;
			this.CautionCaptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 67, true);
			this.CautionCaptionLabel.Name = "CautionCaptionLabel";
			this.CautionCaptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 50, true);
			this.CautionCaptionLabel.TabIndex = 1;
			this.CautionCaptionLabel.Text = "CAUTION:";
			this.CautionCaptionLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
			this.CautionCaptionLabel.UseMnemonic = false;
			// 
			// RiskStatusDropEdit
			// 
			this.RiskStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RiskStatusDropEdit, "RCR_CommodityRiskStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefComplianceCommodityAlert)(null)).RCR_CommodityRiskStatus)));
			this.RiskStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 33, true);
			this.RiskStatusDropEdit.Name = "RiskStatusDropEdit";
			this.RiskStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.RiskStatusDropEdit.TabIndex = 0;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e816a938-aaa7-4f5b-a1fd-4a037d40547f", "Compliance List Details");
			this.DetailsGroupBox.Controls.Add(this.SourceURLCaptionLabel);
			this.DetailsGroupBox.Controls.Add(this.SourceURLLinkLabel);
			this.DetailsGroupBox.Controls.Add(this.IsActiveCheckBox);
			this.DetailsGroupBox.Controls.Add(this.LastEditedTimeTextBox);
			this.DetailsGroupBox.Controls.Add(this.CreatedTimeTextBox);
			this.DetailsGroupBox.Controls.Add(this.AlertDescriptionTextBox);
			this.DetailsGroupBox.Controls.Add(this.CountryRegionTextBox);
			this.DetailsGroupBox.Controls.Add(this.TradeDirectionDropEdit);
			this.DetailsGroupBox.Controls.Add(this.AlertTypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.AlertNameTextBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 137, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1293, 363, true);
			this.DetailsGroupBox.TabIndex = 1;
			this.DetailsGroupBox.TabStop = false;
			// 
			// SourceURLCaptionLabel
			// 
			this.SourceURLCaptionLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("7f2a9aee-d966-4f81-a474-3b6487b5c270", "Source URL");
			this.SourceURLCaptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SourceURLCaptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 335, true);
			this.SourceURLCaptionLabel.Name = "SourceURLCaptionLabel";
			this.SourceURLCaptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 18, true);
			this.SourceURLCaptionLabel.TabIndex = 11;
			this.SourceURLCaptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.SourceURLCaptionLabel.UseMnemonic = false;
			// 
			// SourceURLLinkLabel
			// 
			this.BindingSource.SetBindingMember(this.SourceURLLinkLabel, "SourceURL");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceCommodityAlert)(null)).SourceURL)));
			this.SourceURLLinkLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.SourceURLLinkLabel.IsFontBold = false;
			this.SourceURLLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 335, true);
			this.SourceURLLinkLabel.Name = "SourceURLLinkLabel";
			this.SourceURLLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 18, true);
			this.SourceURLLinkLabel.TabIndex = 7;
			this.SourceURLLinkLabel.TabStop = false;
			this.SourceURLLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SourceURLLinkLabel_LinkClicked);
			this.SourceURLLinkLabel.UseMnemonic = false;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "RCR_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefComplianceCommodityAlert)(null)).RCR_IsActive)));
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(990, 99, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 24, true);
			this.IsActiveCheckBox.TabIndex = 10;
			this.IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// LastEditedTimeTextBox
			// 
			this.BindingSource.SetBindingMember(this.LastEditedTimeTextBox, "LastEditedTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.RefComplianceCommodityAlert)(null)).LastEditedTime)));
			this.LastEditedTimeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LastEditedTimeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(990, 66, true);
			this.LastEditedTimeTextBox.Name = "LastEditedTimeTextBox";
			this.LastEditedTimeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 18, true);
			this.LastEditedTimeTextBox.TabIndex = 8;
			// 
			// CreatedTimeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CreatedTimeTextBox, "CreatedTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.RefComplianceCommodityAlert)(null)).CreatedTime)));
			this.CreatedTimeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CreatedTimeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(990, 33, true);
			this.CreatedTimeTextBox.Name = "CreatedTimeTextBox";
			this.CreatedTimeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 18, true);
			this.CreatedTimeTextBox.TabIndex = 7;
			// 
			// AlertDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.AlertDescriptionTextBox, "RCR_AlertDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceCommodityAlert)(null)).RCR_AlertDescription)));
			this.AlertDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AlertDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 198, true);
			this.AlertDescriptionTextBox.Multiline = true;
			this.AlertDescriptionTextBox.Name = "AlertDescriptionTextBox";
			this.AlertDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 122, true);
			this.AlertDescriptionTextBox.TabIndex = 6;
			// 
			// CountryRegionTextBox
			// 
			this.BindingSource.SetBindingMember(this.CountryRegionTextBox, "CountryRegionDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceCommodityAlert)(null)).CountryRegionDetails)));
			this.CountryRegionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CountryRegionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 72, true);
			this.CountryRegionTextBox.Name = "CountryRegionTextBox";
			this.CountryRegionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 18, true);
			this.CountryRegionTextBox.TabIndex = 3;
			// 
			// TradeDirectionDropEdit
			// 
			this.TradeDirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TradeDirectionDropEdit, "RCR_TradeDirection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefComplianceCommodityAlert)(null)).RCR_TradeDirection)));
			this.TradeDirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 116, true);
			this.TradeDirectionDropEdit.Name = "TradeDirectionDropEdit";
			this.TradeDirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 18, true);
			this.TradeDirectionDropEdit.TabIndex = 4;
			// 
			// AlertTypeDropEdit
			// 
			this.AlertTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AlertTypeDropEdit, "RCR_AlertType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefComplianceCommodityAlert)(null)).RCR_AlertType)));
			this.AlertTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 157, true);
			this.AlertTypeDropEdit.Name = "AlertTypeDropEdit";
			this.AlertTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 18, true);
			this.AlertTypeDropEdit.TabIndex = 5;
			// 
			// AlertNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.AlertNameTextBox, "RCR_AlertName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefComplianceCommodityAlert)(null)).RCR_AlertName)));
			this.AlertNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AlertNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 33, true);
			this.AlertNameTextBox.Name = "AlertNameTextBox";
			this.AlertNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 18, true);
			this.AlertNameTextBox.TabIndex = 1;
			// 
			// RefComplianceCommodityAlertUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsGroupBox);
			this.Controls.Add(this.ConfigurationGroupBox);
			this.Name = "RefComplianceCommodityAlertUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1293, 500, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConfigurationGroupBox.ResumeLayout(false);
			this.ConfigurationGroupBox.PerformLayout();
			this.RiskStatusDropEdit.ResumeLayout(true);
			this.RiskStatusDropEdit.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.TradeDirectionDropEdit.ResumeLayout(true);
			this.TradeDirectionDropEdit.PerformLayout();
			this.AlertTypeDropEdit.ResumeLayout(true);
			this.AlertTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZGroupBox ConfigurationGroupBox;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private CommodityAlertRiskStatusDropEdit RiskStatusDropEdit;
		private ZArchitecture.ZLabel CautionCaptionLabel;
		private ZArchitecture.ZLabel CautionDetailsLabel;
		private ZArchitecture.ZTextBox AlertNameTextBox;
		private ZArchitecture.GUI.ZDropEdit TradeDirectionDropEdit;
		private ZArchitecture.GUI.ZDropEdit AlertTypeDropEdit;
		private ZArchitecture.ZTextBox CountryRegionTextBox;
		private ZArchitecture.ZTextBox AlertDescriptionTextBox;
		private ZArchitecture.ZTextBox CreatedTimeTextBox;
		private ZArchitecture.ZTextBox LastEditedTimeTextBox;
		private ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		internal ZArchitecture.GUI.ZLinkLabel SourceURLLinkLabel;
		private ZArchitecture.ZLabel SourceURLCaptionLabel;
	}
}
