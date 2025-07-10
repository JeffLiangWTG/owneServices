using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.GUI
{
	public partial class HRJobRoleForm
	{
		Enterprise.ZArchitecture.ZTextBox TitleTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox SkillsGroupBox;
		ZPanel RoleTitlePanel;
		CargoWise.Windows.UI.KSplitContainer DescriptionSplitContainer;
		Enterprise.ZArchitecture.ZTextBox RoleDescriptionTextBox;
		CargoWise.Windows.UI.KSplitter splitter1;
		ZGroupBox DescriptionGroupBox;
		Enterprise.ZArchitecture.ZTextBox zTextBox1;
		System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.TitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SkillsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RoleTitlePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.DescriptionSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.RoleDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SkillsGroupBox.SuspendLayout();
			this.RoleTitlePanel.SuspendLayout();
			this.DescriptionSplitContainer.Panel1.SuspendLayout();
			this.DescriptionSplitContainer.Panel2.SuspendLayout();
			this.DescriptionSplitContainer.SuspendLayout();
			this.DescriptionGroupBox.SuspendLayout();
			this.SuspendLayout();
			//
			// MainTabControl
			//
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 470, true);
			//
			// MainTabPage
			//
			this.MainTabPage.Controls.Add(this.DescriptionGroupBox);
			this.MainTabPage.Controls.Add(this.splitter1);
			this.MainTabPage.Controls.Add(this.RoleTitlePanel);
			this.MainTabPage.Controls.Add(this.SkillsGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 443, true);
			//
			// MainStatusBar
			//
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 24, true);
			this.MainStatusBar.SizingGrip = false;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(345);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(345);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.HRJobRole);
			//
			// TitleTextBox
			//
			this.TitleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TitleTextBox, "HJ_JobTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobRole)(null)).HJ_JobTitle)));
			this.TitleTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 9, true);
			this.TitleTextBox.Name = "TitleTextBox";
			this.TitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 20, true);
			this.TitleTextBox.TabIndex = 0;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobRole)(null)).JobRoleSkills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Recruiter.Business.HRJobRoleSkillPivot)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobRole)(null)).JobRoleSkills)).SyncRoot)).H1_HS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.HRJobRoleSkillPivot)(((System.Collections.IList)(((Enterprise.Recruiter.Business.HRJobRole)(null)).JobRoleSkills)).SyncRoot)).H1_SkillsWeighting)));
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups+JobSkills";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "H1_HS";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo1.ColumnName = "JobSkill+HS_SkillDescriptionMultilingual";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("HRJobSkillForm|CA179188-FDE6-43A9-ACD3-837FC299AEA5", "Skill Description");
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "H1_SkillsWeighting";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.ToolTip = "Skill Weightings must add to 100%";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			//
			// SkillsGroupBox
			//
			this.SkillsGroupBox.CaptionResourceString = Res.GetData("HRJobRoleForm|16aa9041-1db2-4b54-87b6-2d01e4059e83", "Skills Needed");
			this.SkillsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SkillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 272, true);
			this.SkillsGroupBox.Name = "SkillsGroupBox";
			this.SkillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 171, true);
			this.SkillsGroupBox.TabIndex = 7;
			this.SkillsGroupBox.TabStop = false;
			//
			// RoleTitlePanel
			//
			this.RoleTitlePanel.Controls.Add(this.TitleTextBox);
			this.RoleTitlePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.RoleTitlePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RoleTitlePanel.Name = "RoleTitlePanel";
			this.RoleTitlePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 37, true);
			this.RoleTitlePanel.TabIndex = 9;
			//
			// splitter1
			//
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 269, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 3, true);
			this.splitter1.TabIndex = 10;
			this.splitter1.TabStop = false;
			//
			// DescriptionSplitContainer
			//
			this.DescriptionSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DescriptionSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DescriptionSplitContainer.Name = "DescriptionSplitContainer";
			//
			// DescriptionSplitContainer.Panel1
			//
			this.DescriptionSplitContainer.Panel1.Controls.Add(this.RoleDescriptionTextBox);
			//
			// DescriptionSplitContainer.Panel2
			//
			this.DescriptionSplitContainer.Panel2.Controls.Add(this.zTextBox1);
			this.DescriptionSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 213, true);
			this.DescriptionSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(224);
			this.DescriptionSplitContainer.SplitterWidth = 8;
			this.DescriptionSplitContainer.TabIndex = 11;
			//
			// RoleDescriptionTextBox
			//
			this.RoleDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RoleDescriptionTextBox, "HJ_JobRoleDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobRole)(null)).HJ_JobRoleDescription)));
			this.RoleDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.RoleDescriptionTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.RoleDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.RoleDescriptionTextBox.Multiline = true;
			this.RoleDescriptionTextBox.Name = "RoleDescriptionTextBox";
			this.RoleDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 188, true);
			this.RoleDescriptionTextBox.TabIndex = 1;
			//
			// zTextBox1
			//
			this.zTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox1, "FullJobRoleDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.HRJobRole)(null)).FullJobRoleDescription)));
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.CaptionResourceString = Res.GetData("HRJobRoleForm|a1217957-d4f2-4f59-b73f-367f08b33c28", "Full Description", "Full Description", "Full Description", "Description of Role and Duties.");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zTextBox1, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 188, true);
			this.zTextBox1.TabIndex = 7;
			//
			// DescriptionGroupBox
			//
			this.DescriptionGroupBox.CaptionResourceString = Res.GetData("HRJobRoleForm|ad7e0188-c173-48db-885b-6f29eb350477", "Description");
			this.DescriptionGroupBox.Controls.Add(this.DescriptionSplitContainer);
			this.DescriptionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 37, true);
			this.DescriptionGroupBox.Name = "DescriptionGroupBox";
			this.DescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 232, true);
			this.DescriptionGroupBox.TabIndex = 12;
			this.DescriptionGroupBox.TabStop = false;
			//
			// HRJobRoleForm
			//
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Res.GetData("HRJobRoleForm|948e8d12-7563-40a8-a0c3-6c7990a9824c", "Job Role");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 526, true);
			this.DataSourceAssemblyName = "Enterprise.Recruiter.Business";
			this.DataSourceType = typeof(Enterprise.Recruiter.Business.HRJobRole);
			this.DataSourceTypeName = "Enterprise.Recruiter.Business.HRJobRole";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 552, true);
			this.Name = "HRJobRoleForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SkillsGroupBox.ResumeLayout(false);
			this.RoleTitlePanel.ResumeLayout(false);
			this.RoleTitlePanel.PerformLayout();
			this.DescriptionSplitContainer.Panel1.ResumeLayout(false);
			this.DescriptionSplitContainer.Panel1.PerformLayout();
			this.DescriptionSplitContainer.Panel2.ResumeLayout(false);
			this.DescriptionSplitContainer.Panel2.PerformLayout();
			this.DescriptionSplitContainer.ResumeLayout(false);
			this.DescriptionGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
