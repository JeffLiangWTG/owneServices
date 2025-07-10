
using System.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefDocSourceForm
	{
		#region Windows Form Designer generated code

		private ZTemplateTabControl DocSourceTabControl;
		private ZTabPage MainTabPage;
		private ZCheckBox IsActiveCheckBox;
		private ZTranslatableTextControl RDS_DescriptionBoundTextBox;
		private ZTextBox RDS_CodeBoundTextBox;
		private ZStmNoteTabPage zStmNoteTabPage1;
		private ZLogsTabPage zLogsTabPage1;
		protected Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private IContainer components;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.DocSourceTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RDS_DescriptionBoundTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.RDS_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DocSourceTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.RDS_DescriptionBoundTextBox.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 245, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(568);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefDocSource);
			// 
			// DocSourceTabControl
			// 
			this.DocSourceTabControl.Controls.Add(this.MainTabPage);
			this.DocSourceTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.DocSourceTabControl.Controls.Add(this.zLogsTabPage1);
			this.DocSourceTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.DocSourceTabControl.Name = "DocSourceTabControl";
			this.DocSourceTabControl.SelectedIndex = 0;
			this.DocSourceTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 207, true);
			this.DocSourceTabControl.TabIndex = 1;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocSourceForm|b4ce410e-90a9-41d4-a753-5d1d216ca98d", "Doc Source");
			this.MainTabPage.Controls.Add(this.IsActiveCheckBox);
			this.MainTabPage.Controls.Add(this.RDS_DescriptionBoundTextBox);
			this.MainTabPage.Controls.Add(this.RDS_CodeBoundTextBox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 180, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "RDS_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefDocSource)(null)).RDS_IsActive)));
			this.IsActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 58, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsActiveCheckBox.TabIndex = 3;
			// 
			// RDS_DescriptionBoundTextBox
			// 
			this.RDS_DescriptionBoundTextBox.AcceptsReturn = false;
			this.RDS_DescriptionBoundTextBox.AllowDrop = true;
			this.RDS_DescriptionBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.RDS_DescriptionBoundTextBox, "RDS_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDocSource)(null)).RDS_Desc)));
			this.RDS_DescriptionBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RDS_DescriptionBoundTextBox.GridCurrent = null;
			this.RDS_DescriptionBoundTextBox.GridMember = null;
			this.RDS_DescriptionBoundTextBox.IsLanguageEditingEnabled = true;
			this.RDS_DescriptionBoundTextBox.IsMultiLine = false;
			this.RDS_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 34, true);
			this.RDS_DescriptionBoundTextBox.Name = "RDS_DescriptionBoundTextBox";
			this.RDS_DescriptionBoundTextBox.ReadOnly = false;
			this.RDS_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.RDS_DescriptionBoundTextBox.TabIndex = 1;
			// 
			// RDS_CodeBoundTextBox
			// 
			this.RDS_CodeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RDS_CodeBoundTextBox, "RDS_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefDocSource)(null)).RDS_Code)));
			this.RDS_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 10, true);
			this.RDS_CodeBoundTextBox.Name = "RDS_CodeBoundTextBox";
			this.RDS_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.RDS_CodeBoundTextBox.TabIndex = 0;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 180, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 180, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 214, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// RefDocSourceForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefDocSourceForm|097da133-caca-4fd2-8e4a-80637b3a4217", "Document Source");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 269, true);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.DocSourceTabControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefDocSource);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 308, true);
			this.Name = "RefDocSourceForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DocSourceTabControl, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DocSourceTabControl.ResumeLayout(false);
			this.DocSourceTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.RDS_DescriptionBoundTextBox.ResumeLayout(true);
			this.RDS_DescriptionBoundTextBox.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
