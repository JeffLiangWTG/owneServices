using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;
using System.Windows.Forms;

namespace Enterprise.MarketingManager.GUI
{
	partial class OpportunityCreationTemplateControl
	{
		#region Windows Form Designer generated code

		ZGroupBox LeadSourceGroupBox;
		ZGroupBox OpportunityCreationGroupBox;
		ZGroupBox DetailsGroupBox;
		ZTextBox OpportunityDescriptionTextBox;
		ZDropEdit OpportunityTypeDropEdit;
		ZDropEdit OpportunityStatusDropEdit;
		ZDropEdit OpportunityStageDropEdit;
		ZRichTextBox OpportunityNotesTextBox;
		ZGroupBox InternalDetailsGroupBox;
		ZGroupBox DetailedNotesGroupBox;
		ZDropEditWithFixedWidth OpportunitySourceDropEdit;
		internal ZDropEditWithFixedWidth SourceDetailsDropEdit;
		ZDropEdit PackageTypeDropEdit;
		ZGroupBox CloseDetailsGroupBox;
		internal ZTextBox SourceDetailsTextBox;
		public ZLabel OverallDispositionLabel;
		ZDropEdit OpportunityAssignmentDropEdit;
		internal ZCodeFindBox SalesPersonCodeFindBox;
		internal ZDropEdit OrgStaffAssignmentDropEdit;
		internal ZButton StaffPoolAssignmentsButton;
		ZCheckBox UseCampaignCheckBox;

		void InitializeComponent()
		{
			this.OpportunityCreationGroupBox = new ZGroupBox();
			this.DetailsGroupBox = new ZGroupBox();
			this.PackageTypeDropEdit = new ZDropEdit();
			this.OpportunityTypeDropEdit = new ZDropEdit();
			this.OpportunityDescriptionTextBox = new ZTextBox();
			this.CloseDetailsGroupBox = new ZGroupBox();
			this.OpportunityStatusDropEdit = new ZDropEdit();
			this.OpportunityStageDropEdit = new ZDropEdit();
			this.OverallDispositionLabel = new ZLabel();
			this.LeadSourceGroupBox = new ZGroupBox();
			this.OpportunitySourceDropEdit = new ZDropEditWithFixedWidth();
			this.SourceDetailsTextBox = new ZTextBox();
			this.SourceDetailsDropEdit = new ZDropEditWithFixedWidth();
			this.UseCampaignCheckBox = new ZCheckBox();
			this.InternalDetailsGroupBox = new ZGroupBox();
			this.OpportunityAssignmentDropEdit = new ZDropEdit();
			this.OrgStaffAssignmentDropEdit = new ZDropEdit();
			this.StaffPoolAssignmentsButton = new ZButton();
			this.SalesPersonCodeFindBox = new ZCodeFindBox();
			this.DetailedNotesGroupBox = new ZGroupBox();
			this.OpportunityNotesTextBox = new ZRichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OpportunityCreationGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.PackageTypeDropEdit.SuspendLayout();
			this.OpportunityTypeDropEdit.SuspendLayout();
			this.CloseDetailsGroupBox.SuspendLayout();
			this.OpportunityStatusDropEdit.SuspendLayout();
			this.OpportunityStageDropEdit.SuspendLayout();
			this.LeadSourceGroupBox.SuspendLayout();
			this.OpportunitySourceDropEdit.SuspendLayout();
			this.SourceDetailsDropEdit.SuspendLayout();
			this.InternalDetailsGroupBox.SuspendLayout();
			this.OpportunityAssignmentDropEdit.SuspendLayout();
			this.SalesPersonCodeFindBox.SuspendLayout();
			this.OrgStaffAssignmentDropEdit.SuspendLayout();
			this.DetailedNotesGroupBox.SuspendLayout();
			this.OpportunityNotesTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(OpportunityCreationTemplate);
			// 
			// OpportunityCreationGroupBox
			// 
			this.OpportunityCreationGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|9A15DE21-D3EC-4356-B499-749BDAA7839A", "Opportunity Creation Template");
			this.OpportunityCreationGroupBox.Controls.Add(this.DetailsGroupBox);
			this.OpportunityCreationGroupBox.Controls.Add(this.CloseDetailsGroupBox);
			this.OpportunityCreationGroupBox.Controls.Add(this.LeadSourceGroupBox);
			this.OpportunityCreationGroupBox.Controls.Add(this.InternalDetailsGroupBox);
			this.OpportunityCreationGroupBox.Controls.Add(this.DetailedNotesGroupBox);
			this.OpportunityCreationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 2, true);
			this.OpportunityCreationGroupBox.Name = "OpportunityCreationGroupBox";
			this.OpportunityCreationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(910, 378, true);
			this.OpportunityCreationGroupBox.TabIndex = 0;
			this.OpportunityCreationGroupBox.TabStop = false;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|410D6AD2-9FB4-489B-9CD2-C53F7CEA4EA9", "Opportunity Details");
			this.DetailsGroupBox.Controls.Add(this.PackageTypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.OpportunityTypeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.OpportunityDescriptionTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 26, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 127, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// PackageTypeDropEdit
			// 
			this.PackageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageTypeDropEdit, "PackageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).PackageType);
			this.PackageTypeDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|E513C1BA-AEA0-4641-8B47-8E1BFA406F71", "Product");
			this.PackageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 43, true);
			this.PackageTypeDropEdit.Name = "PackageTypeDropEdit";
			this.PackageTypeDropEdit.ShouldResizeByMaxLength = true;
			this.PackageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 18, true);
			this.PackageTypeDropEdit.TabIndex = 2;
			// 
			// OpportunityTypeDropEdit
			// 
			this.OpportunityTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OpportunityTypeDropEdit, "OpportunityType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).OpportunityType);
			this.OpportunityTypeDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|57A49054-0C43-4580-A917-EC64ABD9EC2B", "Sales Type");
			this.OpportunityTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 69, true);
			this.OpportunityTypeDropEdit.Name = "OpportunityTypeDropEdit";
			this.OpportunityTypeDropEdit.ShouldResizeByMaxLength = true;
			this.OpportunityTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 18, true);
			this.OpportunityTypeDropEdit.TabIndex = 3;
			// 
			// OpportunityDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.OpportunityDescriptionTextBox, "OpportunityDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).OpportunityDescription);
			this.OpportunityDescriptionTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|CDA070D3-2A5B-4399-AC70-C01ECF21B9E5", "Description", "Opportunity Description.");
			this.OpportunityDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OpportunityDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 17, true);
			this.OpportunityDescriptionTextBox.Name = "OpportunityDescriptionTextBox";
			this.OpportunityDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 18, true);
			this.OpportunityDescriptionTextBox.TabIndex = 1;
			// 
			// CloseDetailsGroupBox
			// 
			this.CloseDetailsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.CloseDetailsGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|59445A4D-F04F-4240-8779-C589BF2951DB", "Close Details");
			this.CloseDetailsGroupBox.Controls.Add(this.OpportunityStatusDropEdit);
			this.CloseDetailsGroupBox.Controls.Add(this.OpportunityStageDropEdit);
			this.CloseDetailsGroupBox.Controls.Add(this.OverallDispositionLabel);
			this.CloseDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 26, true);
			this.CloseDetailsGroupBox.Name = "CloseDetailsGroupBox";
			this.CloseDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 127, true);
			this.CloseDetailsGroupBox.TabIndex = 2;
			this.CloseDetailsGroupBox.TabStop = false;
			// 
			// OpportunityStatusDropEdit
			// 
			this.OpportunityStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OpportunityStatusDropEdit, "OpportunityStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).OpportunityStatus);
			this.OpportunityStatusDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|975E21FF-1D13-43E3-A05E-95911C235088", "Opportunity Status");
			this.OpportunityStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 47, true);
			this.OpportunityStatusDropEdit.Name = "OpportunityStatusDropEdit";
			this.OpportunityStatusDropEdit.PreBoundMaxLength = 3;
			this.OpportunityStatusDropEdit.ShouldResizeByMaxLength = true;
			this.OpportunityStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 18, true);
			this.OpportunityStatusDropEdit.TabIndex = 11;
			// 
			// OpportunityStageDropEdit
			// 
			this.OpportunityStageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OpportunityStageDropEdit, "OpportunityStage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).OpportunityStage);
			this.OpportunityStageDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|553038B4-7DF1-48A3-8833-48F61F7876C1", "Opportunity Stage");
			this.OpportunityStageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 21, true);
			this.OpportunityStageDropEdit.Name = "OpportunityStageDropEdit";
			this.OpportunityStageDropEdit.PreBoundMaxLength = 3;
			this.OpportunityStageDropEdit.ShouldResizeByMaxLength = true;
			this.OpportunityStageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 18, true);
			this.OpportunityStageDropEdit.TabIndex = 10;
			// 
			// OverallDispositionLabel
			// 
			this.BindingSource.SetBindingMember(this.OverallDispositionLabel, "OverallDispositionDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).OverallDispositionDescription);
			this.OverallDispositionLabel.FontType = (Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold);
			this.OverallDispositionLabel.ForeColor = System.Drawing.Color.White;
			this.OverallDispositionLabel.IsFontBold = true;
			this.OverallDispositionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(357, 47, true);
			this.OverallDispositionLabel.Name = "OverallDispositionLabel";
			this.OverallDispositionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.OverallDispositionLabel.TabIndex = 14;
			this.OverallDispositionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// LeadSourceGroupBox
			// 
			this.LeadSourceGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|FDF5DF34-4C1F-4A90-A8D7-9CDAB4029651", "Lead Source");
			this.LeadSourceGroupBox.Controls.Add(this.OpportunitySourceDropEdit);
			this.LeadSourceGroupBox.Controls.Add(this.SourceDetailsTextBox);
			this.LeadSourceGroupBox.Controls.Add(this.SourceDetailsDropEdit);
			this.LeadSourceGroupBox.Controls.Add(this.UseCampaignCheckBox);
			this.LeadSourceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 167, true);
			this.LeadSourceGroupBox.Name = "LeadSourceGroupBox";
			this.LeadSourceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 70, true);
			this.LeadSourceGroupBox.TabIndex = 4;
			this.LeadSourceGroupBox.TabStop = false;
			// 
			// OpportunitySourceDropEdit
			// 
			this.OpportunitySourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OpportunitySourceDropEdit, "Source");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).Source);
			this.OpportunitySourceDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|AC6F9495-BE9B-4481-AB4D-43D47B0FADD5", "Source");
			this.OpportunitySourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 17, true);
			this.OpportunitySourceDropEdit.Name = "OpportunitySourceDropEdit";
			this.OpportunitySourceDropEdit.PreBoundMaxLength = 5;
			this.OpportunitySourceDropEdit.ShouldResizeByMaxLength = true;
			this.OpportunitySourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 18, true);
			this.OpportunitySourceDropEdit.TabIndex = 5;
			// 
			// SourceDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.SourceDetailsTextBox, "ActiveSourceDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).ActiveSourceDetails);
			this.SourceDetailsTextBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|832A6E04-9D2D-4F03-8A6D-FC4D3AC1A11E", "Source Details");
			this.SourceDetailsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SourceDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 42, true);
			this.SourceDetailsTextBox.Name = "SourceDetailsTextBox";
			this.SourceDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 18, true);
			this.SourceDetailsTextBox.TabIndex = 5;
			// 
			// SourceDetailsDropEdit
			// 
			this.SourceDetailsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SourceDetailsDropEdit, "SourceDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).SourceDetails);
			this.SourceDetailsDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|84DE037C-5F54-48FC-9FCF-7CF08D72AEA3", "Source Details");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SourceDetailsDropEdit, false);
			this.SourceDetailsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 42, true);
			this.SourceDetailsDropEdit.Name = "SourceDetailsDropEdit";
			this.SourceDetailsDropEdit.PreBoundMaxLength = 5;
			this.SourceDetailsDropEdit.ShouldResizeByMaxLength = true;
			this.SourceDetailsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 18, true);
			this.SourceDetailsDropEdit.TabIndex = 7;
			this.SourceDetailsDropEdit.Visible = false;
			// 
			// UseCampaignCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UseCampaignCheckBox, "UseCampaignName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).UseCampaignName);
			this.UseCampaignCheckBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|C0595FC9-DAEB-4B6D-A46A-662CEE1C0E7B", "Use Campaign Name");
			this.UseCampaignCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseCampaignCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 42, true);
			this.UseCampaignCheckBox.Name = "UseCampaignCheckBox";
			this.UseCampaignCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
			this.UseCampaignCheckBox.TabIndex = 8;
			// 
			// InternalDetailsGroupBox
			// 
			this.InternalDetailsGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.InternalDetailsGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|A7DE1B5E-C79D-4E8D-B3C2-BACC3E220CEB", "Sales Person Assignment");
			this.InternalDetailsGroupBox.Controls.Add(this.OpportunityAssignmentDropEdit);
			this.InternalDetailsGroupBox.Controls.Add(this.SalesPersonCodeFindBox);
			this.InternalDetailsGroupBox.Controls.Add(this.OrgStaffAssignmentDropEdit);
			this.InternalDetailsGroupBox.Controls.Add(this.StaffPoolAssignmentsButton);
			this.InternalDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 167, true);
			this.InternalDetailsGroupBox.Name = "InternalDetailsGroupBox";
			this.InternalDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 70, true);
			this.InternalDetailsGroupBox.TabIndex = 3;
			this.InternalDetailsGroupBox.TabStop = false;
			// 
			// OpportunityAssignmentDropEdit
			// 
			this.OpportunityAssignmentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OpportunityAssignmentDropEdit, "OpportunityAssignment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).OpportunityAssignment);
			this.OpportunityAssignmentDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|53E4BEF6-4D20-4E5F-8A30-906549646AAC", "Opp. Assignment", "Opportunity Assignment", "Opportunity Assignments");
			this.OpportunityAssignmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 21, true);
			this.OpportunityAssignmentDropEdit.Name = "OpportunityAssignmentDropEdit";
			this.OpportunityAssignmentDropEdit.PreBoundMaxLength = 3;
			this.OpportunityAssignmentDropEdit.ShouldResizeByMaxLength = true;
			this.OpportunityAssignmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 18, true);
			this.OpportunityAssignmentDropEdit.TabIndex = 12;
			// 
			// SalesPersonCodeFindBox
			// 
			this.SalesPersonCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SalesPersonCodeFindBox, "SalesPerson");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).SalesPerson);
			this.SalesPersonCodeFindBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|2F0770F7-80A6-4888-A44D-938337E98548", "Sales Person", "Opportunity Primary Sales Person.");
			this.SalesPersonCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 44, true);
			this.SalesPersonCodeFindBox.Name = "SalesPersonCodeFindBox";
			this.SalesPersonCodeFindBox.PreBoundMaxLength = 3;
			this.SalesPersonCodeFindBox.ShouldResize = true;
			this.SalesPersonCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 18, true);
			this.SalesPersonCodeFindBox.TabIndex = 13;
			this.SalesPersonCodeFindBox.Visible = false;
			// 
			// StaffAssignmentDropEdit
			// 
			this.OrgStaffAssignmentDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgStaffAssignmentDropEdit, "StaffAssignment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).StaffAssignment);
			this.OrgStaffAssignmentDropEdit.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("638B48C7-B0DD-43F6-90DF-DF38321FA48A", "Staff Assignment");
			this.OrgStaffAssignmentDropEdit.PreBoundMaxLength = 3;
			this.OrgStaffAssignmentDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 44, true);
			this.OrgStaffAssignmentDropEdit.Name = "OrgStaffAssignmentDropEdit";
			this.OrgStaffAssignmentDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 18, true);
			this.OrgStaffAssignmentDropEdit.TabIndex = 14;
			this.OrgStaffAssignmentDropEdit.Visible = false;
			// 
			// StaffPoolAssignmentsButton
			// 
			this.StaffPoolAssignmentsButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("3944C1E2-9B8D-4274-8C50-4206EE79AFE2", "...", "", "Edit Staff Pool Assignments");
			this.StaffPoolAssignmentsButton.Font = new Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.StaffPoolAssignmentsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(412, 21, true);
			this.StaffPoolAssignmentsButton.Name = "StaffPoolAssignmentsButton";
			this.StaffPoolAssignmentsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.StaffPoolAssignmentsButton.TabIndex = 16;
			this.StaffPoolAssignmentsButton.TabStop = false;
			this.StaffPoolAssignmentsButton.UseVisualStyleBackColor = true;
			this.StaffPoolAssignmentsButton.Click += new EventHandler(this.StaffPoolSenderAssignmentButton_Click);
			// 
			// DetailedNotesGroupBox
			// 
			this.DetailedNotesGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|C8C5F708-2CD3-4F3E-BBB1-F914DAFE5C8F", "Detailed Notes");
			this.DetailedNotesGroupBox.Controls.Add(this.OpportunityNotesTextBox);
			this.DetailedNotesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 242, true);
			this.DetailedNotesGroupBox.Name = "DetailedNotesGroupBox";
			this.DetailedNotesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(893, 132, true);
			this.DetailedNotesGroupBox.TabIndex = 3;
			this.DetailedNotesGroupBox.TabStop = false;
			// 
			// OpportunityNotesTextBox
			// 
			this.BindingSource.SetBindingMember(this.OpportunityNotesTextBox, "OpportunityNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OpportunityCreationTemplate)(null)).OpportunityNotes);
			this.OpportunityNotesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OpportunityNotesTextBox, false);
			this.OpportunityNotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.OpportunityNotesTextBox.MaxLength = 10000000;
			this.OpportunityNotesTextBox.Name = "OpportunityNotesTextBox";
			this.OpportunityNotesTextBox.ParentZForm = null;
			this.OpportunityNotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 115, true);
			this.OpportunityNotesTextBox.TabIndex = 13;
			this.OpportunityNotesTextBox.IsAttachButtonVisible = false;
			this.OpportunityNotesTextBox.IsInsertImageButtonVisible = false;
			// 
			// OpportunityCreationTemplateControl
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("OpportunityCreationTemplateControl|7D767094-DBAD-47EE-8967-72D945175EAB", "Opportunity");
			this.Controls.Add(this.OpportunityCreationGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 380, true);
			this.Name = "OpportunityCreationTemplateControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 380, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OpportunityCreationGroupBox.ResumeLayout(false);
			this.OpportunityCreationGroupBox.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.PackageTypeDropEdit.ResumeLayout(true);
			this.PackageTypeDropEdit.PerformLayout();
			this.OpportunityTypeDropEdit.ResumeLayout(true);
			this.OpportunityTypeDropEdit.PerformLayout();
			this.CloseDetailsGroupBox.ResumeLayout(false);
			this.CloseDetailsGroupBox.PerformLayout();
			this.OpportunityStatusDropEdit.ResumeLayout(true);
			this.OpportunityStatusDropEdit.PerformLayout();
			this.OpportunityStageDropEdit.ResumeLayout(true);
			this.OpportunityStageDropEdit.PerformLayout();
			this.LeadSourceGroupBox.ResumeLayout(false);
			this.LeadSourceGroupBox.PerformLayout();
			this.OpportunitySourceDropEdit.ResumeLayout(true);
			this.OpportunitySourceDropEdit.PerformLayout();
			this.SourceDetailsDropEdit.ResumeLayout(true);
			this.SourceDetailsDropEdit.PerformLayout();
			this.InternalDetailsGroupBox.ResumeLayout(false);
			this.InternalDetailsGroupBox.PerformLayout();
			this.OpportunityAssignmentDropEdit.ResumeLayout(true);
			this.OpportunityAssignmentDropEdit.PerformLayout();
			this.SalesPersonCodeFindBox.ResumeLayout(true);
			this.SalesPersonCodeFindBox.PerformLayout();
			this.OrgStaffAssignmentDropEdit.ResumeLayout(true);
			this.OrgStaffAssignmentDropEdit.PerformLayout();
			this.DetailedNotesGroupBox.ResumeLayout(false);
			this.DetailedNotesGroupBox.PerformLayout();
			this.OpportunityNotesTextBox.ResumeLayout(true);
			this.OpportunityNotesTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
