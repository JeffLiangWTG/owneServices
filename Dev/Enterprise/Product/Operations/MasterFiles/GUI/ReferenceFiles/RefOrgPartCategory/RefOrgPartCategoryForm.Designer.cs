namespace Enterprise.MasterFiles.GUI
{
	public partial class RefOrgPartCategoryForm
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
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsUserControl.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 206, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(590);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgPartCategory);
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 193, true);
			this.ButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.ButtonsUserControl.TabIndex = 1;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 156, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("abb503e5-cd66-4e4a-bf15-bf538207d67e", "Category");
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 135, true);
			this.MainTabPage.TabIndex = 0;
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartCategory)(null)).OPC_CategoryCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgPartCategory)(null)).OPC_OPC_Parent)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgPartCategory)(null)).OPC_CategoryDescription)));
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 135, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			this.zStmNoteTabPage1.RunWhenBindingOrFirstShown(new System.EventHandler(this.zStmNoteTabPage1_InitializeTab));
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(539, 135, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// RefOrgPartCategoryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefOrgPartCategoryForm|8cd4a326-17b0-40bf-abc6-38a8aee5be44", "Category");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 230, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.ButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgPartCategory);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 270, true);
			this.Name = "RefOrgPartCategoryForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsUserControl.ResumeLayout(true);
			this.ButtonsUserControl.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.OPC_CategoryCodeText = new Enterprise.ZArchitecture.ZTextBox();
			this.ParentCategoryGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CategoryDescriptionTranslatableControl = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.MainTabPage.SuspendLayout();
			this.ParentCategoryGuidFindBox.SuspendLayout();
			this.CategoryDescriptionTranslatableControl.SuspendLayout();
			this.MainTabPage.Controls.Add(this.CategoryDescriptionTranslatableControl);
			this.MainTabPage.Controls.Add(this.ParentCategoryGuidFindBox);
			this.MainTabPage.Controls.Add(this.OPC_CategoryCodeText);
			// 
			// OPC_CategoryCodeText
			// 
			this.OPC_CategoryCodeText.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OPC_CategoryCodeText, "OPC_CategoryCode");
			this.OPC_CategoryCodeText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 14, true);
			this.OPC_CategoryCodeText.Name = "OPC_CategoryCodeText";
			this.OPC_CategoryCodeText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 18, true);
			this.OPC_CategoryCodeText.TabIndex = 0;
			// 
			// ParentCategoryGuidFindBox
			// 
			this.ParentCategoryGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ParentCategoryGuidFindBox, "OPC_OPC_Parent");
			this.ParentCategoryGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 66, true);
			this.ParentCategoryGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefOrgPartCategory;
			this.ParentCategoryGuidFindBox.Name = "ParentCategoryGuidFindBox";
			this.ParentCategoryGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 18, true);
			this.ParentCategoryGuidFindBox.TabIndex = 2;
			// 
			// CategoryDescriptionTranslatableControl
			// 
			this.CategoryDescriptionTranslatableControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CategoryDescriptionTranslatableControl, "OPC_CategoryDescription");
			this.CategoryDescriptionTranslatableControl.GridCurrent = null;
			this.CategoryDescriptionTranslatableControl.GridMember = null;
			this.CategoryDescriptionTranslatableControl.IsMultiLine = false;
			this.CategoryDescriptionTranslatableControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 39, true);
			this.CategoryDescriptionTranslatableControl.Name = "CategoryDescriptionTranslatableControl";
			this.CategoryDescriptionTranslatableControl.ReadOnly = false;
			this.CategoryDescriptionTranslatableControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.CategoryDescriptionTranslatableControl.TabIndex = 1;
			this.MainTabPage.PerformLayout();
			this.ParentCategoryGuidFindBox.ResumeLayout(true);
			this.ParentCategoryGuidFindBox.PerformLayout();
			this.CategoryDescriptionTranslatableControl.ResumeLayout(true);
			this.CategoryDescriptionTranslatableControl.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		private void zStmNoteTabPage1_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.zStmNoteTabPage1.SuspendLayout();
			this.zStmNoteTabPage1.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(true);
		}

		private Enterprise.ZArchitecture.ZTextBox OPC_CategoryCodeText;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ParentCategoryGuidFindBox;
		private ZArchitecture.ZTranslatableTextControl CategoryDescriptionTranslatableControl;
	}
}
