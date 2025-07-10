namespace Enterprise.MasterFiles.GUI
{
	partial class GenCustomAddOnRuleForm
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
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBox codeTextBox;
			Enterprise.ZArchitecture.ZTextBox descriptionTextBox;
			Enterprise.ZArchitecture.GUI.ZCheckBox isActiveCheckBox;
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.rulesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.rulesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.detailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			codeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			isActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.rulesGroupBox.SuspendLayout();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.rulesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 269, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.rulesGroupBox);
			this.MainTabPage.Controls.Add(codeTextBox);
			this.MainTabPage.Controls.Add(descriptionTextBox);
			this.MainTabPage.Controls.Add(isActiveCheckBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(793, 242, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRule);
			// 
			// codeTextBox
			// 
			this.BindingSource.SetBindingMember(codeTextBox, "XR_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRule)(null)).XR_Code)));
			codeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 3, true);
			codeTextBox.Name = "codeTextBox";
			codeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			codeTextBox.TabIndex = 0;
			// 
			// descriptionTextBox
			// 
			this.BindingSource.SetBindingMember(descriptionTextBox, "XR_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRule)(null)).XR_Description)));
			descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 3, true);
			descriptionTextBox.Name = "descriptionTextBox";
			descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			descriptionTextBox.TabIndex = 1;
			// 
			// isActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(isActiveCheckBox, "XR_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRule)(null)).XR_IsActive)));
			isActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 3, true);
			isActiveCheckBox.Name = "isActiveCheckBox";
			isActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			isActiveCheckBox.TabIndex = 2;
			// 
			// rulesGroupBox
			// 
			this.rulesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.rulesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GenCustomAddOnRuleForm|f624f52e-a10a-475f-9c9d-4e0cb6207dfd", "Behavior");
			this.rulesGroupBox.Controls.Add(this.splitContainer);
			this.rulesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.rulesGroupBox.Name = "rulesGroupBox";
			this.rulesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 210, true);
			this.rulesGroupBox.TabIndex = 3;
			this.rulesGroupBox.TabStop = false;
			// 
			// splitContainer
			// 
			this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.splitContainer.Name = "splitContainer";
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.rulesGrid);
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.detailsPanel);
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 185, true);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(194);
			this.splitContainer.TabIndex = 4;
			// 
			// rulesGrid
			// 
			this.rulesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.rulesGrid, "AllRules");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRule)(null)).AllRules)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomValues.AvailableRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRule)(null)).AllRules)).SyncRoot)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.CustomValues.AvailableRule)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRule)(null)).AllRules)).SyncRoot)).IsEnabled)));
			this.rulesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.ColumnName = "IsEnabled";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.rulesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.rulesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.rulesGrid.GridId = "93fed391-17a8-4ee8-a045-3605ba29c95e";
			this.rulesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rulesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.rulesGrid.LayoutKey = "rulesGrid";
			this.rulesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rulesGrid.Name = "rulesGrid";
			this.rulesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 185, true);
			this.rulesGrid.TabIndex = 0;
			// 
			// detailsPanel
			// 
			this.detailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsPanel.Name = "detailsPanel";
			this.detailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 185, true);
			this.detailsPanel.TabIndex = 0;
			// 
			// GenCustomAddOnRuleForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GenCustomAddOnRuleForm|948e8d12-7563-40a8-a0c3-6c7990a9824c", "Add On Rule");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(801, 325, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.CustomValues.GenCustomAddOnRule);
			this.Name = "GenCustomAddOnRuleForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "GenCustomAddOnRuleForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.rulesGroupBox.ResumeLayout(false);
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			this.splitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.rulesGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid rulesGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox rulesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel detailsPanel;
		private CargoWise.Windows.UI.KSplitContainer splitContainer;
	}
}
