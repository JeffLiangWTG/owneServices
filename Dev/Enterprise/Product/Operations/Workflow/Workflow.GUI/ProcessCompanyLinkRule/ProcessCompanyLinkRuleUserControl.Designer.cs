using System;
using Enterprise.MasterFiles.GUI;
using Enterprise.Workflow.Business;

namespace Enterprise.Workflow.GUI
{
	partial class ProcessCompanyLinkRuleUserControl
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

		#region Cloning

		void CompaniesGrid_AfterBind(object sender, EventArgs eventArgs)
		{
			canDeleteRule = ((ProcessCompanyLinkRule)DataSource).PreventDeletionWhileCloningMasterRule();
		}

		void CompaniesGrid_Disposed(object sender, EventArgs eventArgs)
		{
			canDeleteRule?.Dispose();
		}

		IDisposable canDeleteRule;

		#endregion

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            Enterprise.MasterFiles.GUI.ZProcessCompanyGridRuleGuidFindBoxColumnStyleInfo zProcessCompanyGridRuleGuidFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZProcessCompanyGridRuleGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.macroTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.isActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.workflowTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.companyGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.companiesGrid = new Enterprise.ZArchitecture.ZGrid();
            this.zCompanyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.workflowTypeDropEdit.SuspendLayout();
            this.companyGuidFindBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.companiesGrid)).BeginInit();
            this.companiesGrid.SuspendLayout();
            this.zCompanyGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Workflow.Business.ProcessCompanyLinkRule);
            // 
            // macroTextBox
            // 
            this.macroTextBox.AcceptsReturn = true;
            this.macroTextBox.AcceptsTab = true;
            this.macroTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.macroTextBox, "PCR_Macro");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessCompanyLinkRule)(null)).PCR_Macro)));
			this.macroTextBox.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("8b9cd611-eecc-4de5-97b0-ed9fc85000b2", "Macro");
			this.macroTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.macroTextBox.IsDynamicMultiline = true;
            this.macroTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 59, true);
            this.macroTextBox.Multiline = true;
            this.macroTextBox.Name = "macroTextBox";
            this.macroTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(489, 214, true);
            this.macroTextBox.SupportsMacroTemplates = true;
            this.macroTextBox.TabIndex = 2;
            // 
            // isActiveCheckBox
            // 
            this.BindingSource.SetBindingMember(this.isActiveCheckBox, "PCR_IsActive");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Workflow.Business.ProcessCompanyLinkRule)(null)).PCR_IsActive)));
            this.isActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(388, 3, true);
            this.isActiveCheckBox.Name = "isActiveCheckBox";
            this.isActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
            this.isActiveCheckBox.TabIndex = 3;
            this.isActiveCheckBox.UseVisualStyleBackColor = false;
            // 
            // workflowTypeDropEdit
            // 
            this.workflowTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.workflowTypeDropEdit, "PCR_Type");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Workflow.Business.ProcessCompanyLinkRule)(null)).PCR_Type)));
            this.workflowTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 7, true);
            this.workflowTypeDropEdit.Name = "workflowTypeDropEdit";
            this.workflowTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.workflowTypeDropEdit.TabIndex = 4;
			// 
			// companyGuidFindBox
			// 
			this.companyGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.companyGuidFindBox, "PCR_GC_Company");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Workflow.Business.ProcessCompanyLinkRule)(null)).PCR_GC_Company)));
			this.companyGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.companyGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 33, true);
			this.companyGuidFindBox.Name = "companyGuidFindBox";
			this.companyGuidFindBox.ShouldResize = true;
			this.companyGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.companyGuidFindBox.TabIndex = 5;
			// 
			// companiesGrid
			// 
			this.companiesGrid.AllowNavigation = false;
            this.companiesGrid.AllowSorting = false;
            this.BindingSource.SetBindingMember(this.companiesGrid, "RulesForBinding");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Workflow.Business.ProcessCompanyLinkRule)(null)).RulesForBinding)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Workflow.Business.ProcessCompanyLinkRule)(((System.Collections.IList)(((Enterprise.Workflow.Business.ProcessCompanyLinkRule)(null)).RulesForBinding)).SyncRoot)).PCR_GC_Company)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.ProcessCompanyLinkRule)(((System.Collections.IList)(((Enterprise.Workflow.Business.ProcessCompanyLinkRule)(null)).RulesForBinding)).SyncRoot)).Company.GC_Name)));
            this.companiesGrid.CaptionVisible = false;
            this.companiesGrid.ColumnHeadersVisible = false;
            zProcessCompanyGridRuleGuidFindBoxColumnStyleInfo1.AllowModuleMultiSelect = true;
            zProcessCompanyGridRuleGuidFindBoxColumnStyleInfo1.ColumnName = "PCR_GC_Company";
            zProcessCompanyGridRuleGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zProcessCompanyGridRuleGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo1.ColumnName = "Company+GC_Name";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
            this.companiesGrid.ColumnStyles.Add(zProcessCompanyGridRuleGuidFindBoxColumnStyleInfo1);
            this.companiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.companiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.companiesGrid.GridId = "e7577370-738d-46d6-bde0-6a356fe51c19";
            this.companiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.companiesGrid.LayoutKey = "zGrid1";
            this.companiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.companiesGrid.Name = "companiesGrid";
            this.companiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 81, true);
            this.companiesGrid.TabIndex = 6;
            this.companiesGrid.Visible = false;
            this.companiesGrid.AfterBind += new System.EventHandler(this.CompaniesGrid_AfterBind);
            this.companiesGrid.Disposed += new System.EventHandler(this.CompaniesGrid_Disposed);
			// 
			// zCompanyGroupBox
			//
			this.zCompanyGroupBox.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("cc311557-7d56-4f32-bae3-7cba734419ea", "Company");
			this.zCompanyGroupBox.Controls.Add(this.companiesGrid);
			this.zCompanyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 32, true);
			this.zCompanyGroupBox.Name = "zCompanyGroupBox";
            this.zCompanyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 100, true);
            this.zCompanyGroupBox.TabIndex = 7;
            this.zCompanyGroupBox.TabStop = false;
            this.zCompanyGroupBox.Visible = false;
            // 
            // ProcessCompanyLinkRuleUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.companyGuidFindBox);
			this.Controls.Add(this.zCompanyGroupBox);
            this.Controls.Add(this.workflowTypeDropEdit);
            this.Controls.Add(this.isActiveCheckBox);
            this.Controls.Add(this.macroTextBox);
            this.Name = "ProcessCompanyLinkRuleUserControl";
            this.ShouldSerializeTabPageMethods = false;
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 276, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.workflowTypeDropEdit.ResumeLayout(true);
            this.workflowTypeDropEdit.PerformLayout();
            this.companyGuidFindBox.ResumeLayout(true);
            this.companyGuidFindBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.companiesGrid)).EndInit();
            this.companiesGrid.ResumeLayout(false);
            this.companiesGrid.PerformLayout();
            this.zCompanyGroupBox.ResumeLayout(false);
            this.zCompanyGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZTextBox macroTextBox;
		private ZArchitecture.GUI.ZCheckBox isActiveCheckBox;
		private ZArchitecture.GUI.ZDropEdit workflowTypeDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox companyGuidFindBox;
		private ZArchitecture.ZGrid companiesGrid;
		private ZArchitecture.GUI.ZGroupBox zCompanyGroupBox;
	}
}
