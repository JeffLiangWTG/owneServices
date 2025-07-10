using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Recruiter.GUI
{
	partial class GlbAccreditationGroupForm
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
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 418, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 391, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 391, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 391, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 418, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Recruiter.Business.GlbAccreditationGroup);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Recruiter.Business.GlbAccreditationGroup)(null)).HAG_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.GlbAccreditationGroup)(null)).Accreditations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Recruiter.Business.GlbAccreditationGroup)(null)).Lookups.MainAccreditationList)));
			// 
			// 
			// 
			// 
			// GlbAccreditationGroupForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(441, 474, true);
			this.DataSourceType = typeof(Enterprise.Recruiter.Business.GlbAccreditationGroup);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 513, true);
			this.Name = "GlbAccreditationGroupForm";
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
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreReqGrid = new Enterprise.Recruiter.GUI.GlbAccreditationGroupingModuleButtonGrid();
			this.topPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.accreditationPreReqsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreReqGrid.InnerGrid)).BeginInit();
			this.PreReqGrid.SuspendLayout();
			this.topPanel.SuspendLayout();
			this.accreditationPreReqsGroupBox.SuspendLayout();
			this.MainTabPage.Controls.Add(this.accreditationPreReqsGroupBox);
			this.MainTabPage.Controls.Add(this.topPanel);
			// 
			// descriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "HAG_Description");
			this.descriptionTextBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("GlbAccreditationGroup|Description", "Description");
			this.descriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 7, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.descriptionTextBox.TabIndex = 0;
			// 
			// PreReqGrid
			// 
			this.PreReqGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreReqGrid, "Accreditations");
			this.PreReqGrid.BindToFindBoxList = "Lookups.MainAccreditationList";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("GlbAccreditationGroupForm|DED539E8-83EF-4B61-820F-45A92306575F", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "HAC_Code";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("GlbAccreditationGroupForm|23CB51D8-BD3C-430E-9A42-4E93166DAF08", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "HAC_Description";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.PreReqGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PreReqGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PreReqGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreReqGrid.GridId = "cc72e3d4-223f-420f-b5fa-d0fe5c41c134";
			this.PreReqGrid.InnerGrid.AllowNavigation = false;
			this.PreReqGrid.InnerGrid.CaptionVisible = false;
			this.PreReqGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreReqGrid.InnerGrid.GridId = null;
			this.PreReqGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PreReqGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.PreReqGrid.InnerGrid.LayoutKey = "Grid";
			this.PreReqGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PreReqGrid.InnerGrid.Name = "Grid";
			this.PreReqGrid.InnerGrid.ReadOnly = true;
			this.PreReqGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 272, true);
			this.PreReqGrid.InnerGrid.TabIndex = 0;
			this.PreReqGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PreReqGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbAccreditation;
			this.PreReqGrid.Name = "PreReqGrid";
			this.PreReqGrid.NameOfAGridElement = Enterprise.Recruiter.GUI.Res.GetData("c950d5d4-d6b9-48ca-9478-661317c09e95", "Accreditation");
			this.PreReqGrid.ReadOnly = true;
			this.PreReqGrid.ShowEditButton = false;
			this.PreReqGrid.ShowNewButton = false;
			this.PreReqGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 310, true);
			this.PreReqGrid.TabIndex = 0;
			this.PreReqGrid.DoubleClick += PreReqGrid_DoubleClick;
			// 
			// topPanel
			// 
			this.topPanel.Controls.Add(this.descriptionTextBox);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 35, true);
			this.topPanel.TabIndex = 0;
			// 
			// accreditationPreReqsGroupBox
			// 
			this.accreditationPreReqsGroupBox.CaptionResourceString = Enterprise.Recruiter.GUI.Res.GetData("e894e245-4f81-46f0-b196-ab4783506d5f", "Accreditations");
			this.accreditationPreReqsGroupBox.Controls.Add(this.PreReqGrid);
			this.accreditationPreReqsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.accreditationPreReqsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 41, true);
			this.accreditationPreReqsGroupBox.Name = "accreditationPreReqsGroupBox";
			this.accreditationPreReqsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 329, true);
			this.accreditationPreReqsGroupBox.TabIndex = 1;
			this.accreditationPreReqsGroupBox.TabStop = false;
			this.MainTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreReqGrid.InnerGrid)).EndInit();
			this.PreReqGrid.ResumeLayout(true);
			this.PreReqGrid.PerformLayout();
			this.topPanel.ResumeLayout(false);
			this.topPanel.PerformLayout();
			this.accreditationPreReqsGroupBox.ResumeLayout(false);
			this.accreditationPreReqsGroupBox.PerformLayout();
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
		private ZArchitecture.ZTextBox descriptionTextBox;
		protected GlbAccreditationGroupingModuleButtonGrid PreReqGrid;
		private ZGroupBox accreditationPreReqsGroupBox;
	}
}