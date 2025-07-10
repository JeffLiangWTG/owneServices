using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.GUI
{
	partial class GlbAccreditationForm
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
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 557, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 535, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 535, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 535, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 557, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.GlbAccreditation);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.GlbAccreditation)(null)).Requirements)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.GlbAccreditation)(null)).Lookups.AccreditationList)));
			// 
			// 
			// 
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.GlbAccreditation)(null)).HAC_Code)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.GlbAccreditation)(null)).HAC_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.GlbAccreditation)(null)).HAC_CertificateCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruiter.Business.GlbAccreditation)(null)).HAC_IsRefresher)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Recruiter.Business.GlbAccreditation)(null)).HAC_IsAutoNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.GlbAccreditation)(null)).HAC_MustCompleteInDays)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Recruiter.Business.GlbAccreditation)(null)).HAC_ValidityMonths)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Recruiter.Business.GlbAccreditation)(null)).HAC_RefresherCertificateExpiryType)));
			// 
			// GlbAccreditationForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 626, true);
			this.DataSourceType = typeof(Enterprise.Recruiter.Business.GlbAccreditation);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(497, 626, true);
			this.Name = "GlbAccreditationForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.preReqGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.accreditationGroupTreeControl = new Enterprise.Recruiter.GUI.GlbAccreditationGroupTreeControl();
			this.codeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.certCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.isRefresherCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isWebPublishedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.autoNumberCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.completeInDays = new Enterprise.ZArchitecture.ZCalcEdit();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.validityMonths = new Enterprise.ZArchitecture.ZCalcEdit();
			this.accreditationGroupsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.accreditationPreReqsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.achievementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.refresherExpDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.preReqGrid.InnerGrid)).BeginInit();
			this.preReqGrid.SuspendLayout();
			this.accreditationGroupTreeControl.SuspendLayout();
			this.certCodeDropEdit.SuspendLayout();
			this.accreditationGroupsGroupBox.SuspendLayout();
			this.accreditationPreReqsGroupBox.SuspendLayout();
			this.achievementGroupBox.SuspendLayout();
			this.refresherExpDropEdit.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.MainTabPage.Controls.Add(this.accreditationGroupsGroupBox);
			this.MainTabPage.Controls.Add(this.accreditationPreReqsGroupBox);
			this.MainTabPage.Controls.Add(this.achievementGroupBox);
			this.MainTabPage.Controls.Add(this.topPanel);
			// 
			// preReqGrid
			// 
			this.preReqGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.preReqGrid, "Requirements");
			this.preReqGrid.BindToFindBoxList = "Lookups.AccreditationList";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("GlbAccreditationForm|DED539E8-83EF-4B61-820F-45A92306575F", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "HAC_Code";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("GlbAccreditationForm|23CB51D8-BD3C-430E-9A42-4E93166DAF08", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "HAC_Description";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.preReqGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.preReqGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.preReqGrid.GridId = "AF60012C-AAC9-4B80-900B-F5267E5E2EA2";
			this.preReqGrid.InnerGrid.AllowNavigation = false;
			this.preReqGrid.InnerGrid.CaptionVisible = false;
			this.preReqGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.preReqGrid.InnerGrid.GridId = null;
			this.preReqGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.preReqGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.preReqGrid.InnerGrid.LayoutKey = "Grid";
			this.preReqGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.preReqGrid.InnerGrid.Name = "Grid";
			this.preReqGrid.InnerGrid.ReadOnly = true;
			this.preReqGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 109, true);
			this.preReqGrid.InnerGrid.TabIndex = 0;
			this.preReqGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.preReqGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbAccreditation;
			this.preReqGrid.Name = "preReqGrid";
			this.preReqGrid.Dock = DockStyle.Fill;
			this.preReqGrid.NameOfAGridElement = Enterprise.Recruiter.GUI.Res.GetData("2E4F8441-0F9A-4E8A-886F-13B422D4555A", "Requirement");
			this.preReqGrid.ReadOnly = true;
			this.preReqGrid.ShowEditButton = false;
			this.preReqGrid.ShowNewButton = false;
			this.preReqGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 128, true);
			this.preReqGrid.TabIndex = 1;
			this.preReqGrid.DoubleClick += PreReqGrid_DoubleClick;
			// 
			// accreditationGroupsGroupBox
			// 
			this.accreditationGroupsGroupBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("9668887b-7e28-4b0c-8920-f4b4c4aecc3d", "Skill Requirements");
			this.accreditationGroupsGroupBox.Controls.Add(this.accreditationGroupTreeControl);
			this.accreditationGroupsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accreditationGroupsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 79, true);
			this.accreditationGroupsGroupBox.Name = "accreditationGroupsGroupBox";
			this.accreditationGroupsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 306, true);
			this.accreditationGroupsGroupBox.TabIndex = 2;
			this.accreditationGroupsGroupBox.TabStop = false;
			// 
			// accreditationGroupTreeControl
			// 
			this.BindingSource.SetBindingMember(this.accreditationGroupTreeControl, "GlbAccreditationTreeModel");
			this.accreditationGroupTreeControl.AllowDrop = true;
			this.accreditationGroupTreeControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.accreditationGroupTreeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 15, true);
			this.accreditationGroupTreeControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.accreditationGroupTreeControl.Name = "accreditationGroupTreeControl";
			this.accreditationGroupTreeControl.ShowAttachButton = false;
			this.accreditationGroupTreeControl.ShowDetachButton = false;
			this.accreditationGroupTreeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 284, true);
			this.accreditationGroupTreeControl.TabIndex = 0;
			// 
			// accreditationPreReqsGroupBox
			// 
			this.accreditationPreReqsGroupBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("af1645ad-2dd8-465a-87dd-45ea17104f5e", "Pre Requisite Accreditations");
			this.accreditationPreReqsGroupBox.Controls.Add(this.preReqGrid);
			this.accreditationPreReqsGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.accreditationPreReqsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 354, true);
			this.accreditationPreReqsGroupBox.Name = "accreditationPreReqsGroupBox";
			this.accreditationPreReqsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 146, true);
			this.accreditationPreReqsGroupBox.TabIndex = 3;
			this.accreditationPreReqsGroupBox.TabStop = false;
			// 
			// achievementGroupBox
			// 
			this.achievementGroupBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("4f78e6c5-8b6d-4455-a4aa-f7976566324b", "Completion Certificate Achievement");
			this.achievementGroupBox.Controls.Add(this.certCodeDropEdit);
			this.achievementGroupBox.Controls.Add(this.autoNumberCheckBox);
			this.achievementGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.achievementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 497, true);
			this.achievementGroupBox.Name = "achievementGroupBox";
			this.achievementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 40, true);
			this.achievementGroupBox.TabIndex = 3;
			this.achievementGroupBox.TabStop = false;
			// 
			// codeTextBox
			// 
			this.BindingSource.SetBindingMember(this.codeTextBox, "HAC_Code");
			this.codeTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("GlbAccreditation|Code", "Code");
			this.codeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 7, true);
			this.codeTextBox.Name = "codeTextBox";
			this.codeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 17, true);
			this.codeTextBox.TabIndex = 1;
			// 
			// descriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "HAC_Description");
			this.descriptionTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("GlbAccreditation|Description", "Description");
			this.descriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 29, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.descriptionTextBox.TabIndex = 2;
			// 
			// certCodeDropEdit
			// 
			this.certCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.certCodeDropEdit, "HAC_CertificateCode");
			this.certCodeDropEdit.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("bade1eb4-e8cf-43ed-8798-4a5782664c58", "Certificate Code");
			this.certCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 16, true);
			this.certCodeDropEdit.Name = "certCodeDropEdit";
			this.certCodeDropEdit.ShowDescriptionBox = false;
			this.certCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 23, true);
			this.certCodeDropEdit.TabIndex = 3;
			// 
			// isRefresherCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isRefresherCheckBox, "HAC_IsRefresher");
			this.isRefresherCheckBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("b596f47a-ac3f-4d68-9e67-1377b8b2eac7", "Is Refresher");
			this.isRefresherCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isRefresherCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isRefresherCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 49, true);
			this.isRefresherCheckBox.Name = "isRefresherCheckBox";
			this.isRefresherCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 24, true);
			this.isRefresherCheckBox.TabIndex = 5;
			// 
			// isWebPublishedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isWebPublishedCheckBox, "HAC_IsWebPublished");
			this.isWebPublishedCheckBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("f68b70b1-eef7-4dba-a182-bdbc3fc9e978", "Web Published");
			this.isWebPublishedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.isWebPublishedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isWebPublishedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 49, true);
			this.isWebPublishedCheckBox.Name = "isWebPublishedCheckBox";
			this.isWebPublishedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 24, true);
			this.isWebPublishedCheckBox.TabIndex = 6;
			// 
			// refresherExpDropEdit
			// 
			this.refresherExpDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.refresherExpDropEdit, "HAC_RefresherCertificateExpiryType");
			this.refresherExpDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 53, true);
			this.refresherExpDropEdit.Name = "refresherExpDropEdit";
			this.refresherExpDropEdit.ShowDescriptionBox = false;
			this.refresherExpDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.refresherExpDropEdit.TabIndex = 7;
			// 
			// autoNumberCheckBox
			// 
			this.BindingSource.SetBindingMember(this.autoNumberCheckBox, "HAC_IsAutoNumber");
			this.autoNumberCheckBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("B6F6C427-F057-4DEF-B850-091DB641D662", "Set Certificate ID");
			this.autoNumberCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.autoNumberCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.autoNumberCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(303, 10, true);
			this.autoNumberCheckBox.Name = "autoNumberCheckBox";
			this.autoNumberCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 24, true);
			this.autoNumberCheckBox.TabIndex = 4;
			// 
			// completeInDays
			// 
			this.BindingSource.SetBindingMember(this.completeInDays, "HAC_MustCompleteInDays");
			this.completeInDays.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("GlbAccreditation|MustCompleteInDays", "Completion Tolerance (days)");
			this.completeInDays.DecimalPlaces = 0;
			this.completeInDays.Decimals = 0;
			this.completeInDays.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 7, true);
			this.completeInDays.Name = "completeInDays";
			this.completeInDays.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.completeInDays.TabIndex = 3;
			this.completeInDays.Text = "0";
			this.completeInDays.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.validityMonths);
			this.topPanel.Controls.Add(this.codeTextBox);
			this.topPanel.Controls.Add(this.descriptionTextBox);
			this.topPanel.Controls.Add(this.completeInDays);
			this.topPanel.Controls.Add(this.isRefresherCheckBox);
			this.topPanel.Controls.Add(this.isWebPublishedCheckBox);
			this.topPanel.Controls.Add(this.refresherExpDropEdit);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 75, true);
			this.topPanel.TabIndex = 0;
			// 
			// validityMonths
			// 
			this.BindingSource.SetBindingMember(this.validityMonths, "HAC_ValidityMonths");
			this.validityMonths.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("085f05f2-743e-4e57-a978-7ea503a97b25", "Validity Period (mths.)");
			this.validityMonths.DecimalPlaces = 0;
			this.validityMonths.Decimals = 0;
			this.accreditationGroupsGroupBox.ResumeLayout(false);
			this.accreditationGroupsGroupBox.PerformLayout();
			this.validityMonths.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 29, true);
			this.validityMonths.Name = "validityMonths";
			this.validityMonths.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.validityMonths.TabIndex = 4;
			this.validityMonths.Text = "0";
			this.validityMonths.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.MainTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.preReqGrid.InnerGrid)).EndInit();
			this.preReqGrid.ResumeLayout(true);
			this.preReqGrid.PerformLayout();
			this.accreditationGroupTreeControl.ResumeLayout(true);
			this.accreditationGroupTreeControl.PerformLayout();
			this.certCodeDropEdit.ResumeLayout(true);
			this.certCodeDropEdit.PerformLayout();
			this.accreditationPreReqsGroupBox.ResumeLayout(true);
			this.accreditationPreReqsGroupBox.PerformLayout();
			this.achievementGroupBox.ResumeLayout(true);
			this.achievementGroupBox.PerformLayout();
			this.refresherExpDropEdit.ResumeLayout(true);
			this.refresherExpDropEdit.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.MainTabPage.ResumeLayout(true);

		}

		void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		#endregion

		private ZPanel topPanel;
		private ZArchitecture.ZTextBox codeTextBox;
		private ZArchitecture.ZTextBox descriptionTextBox;
		private ZDropEdit certCodeDropEdit;
		protected ZCheckBox isRefresherCheckBox;
		protected ZCheckBox isWebPublishedCheckBox;
		private ZDropEdit refresherExpDropEdit;
		private ZCheckBox autoNumberCheckBox;
		private GlbAccreditationGroupTreeControl accreditationGroupTreeControl;
		private ZModuleButtonGrid preReqGrid;
		private ZCalcEdit completeInDays;
		private ZCalcEdit validityMonths;
		private ZGroupBox accreditationGroupsGroupBox;
		private ZGroupBox accreditationPreReqsGroupBox;
		private ZGroupBox achievementGroupBox;
	}
}
