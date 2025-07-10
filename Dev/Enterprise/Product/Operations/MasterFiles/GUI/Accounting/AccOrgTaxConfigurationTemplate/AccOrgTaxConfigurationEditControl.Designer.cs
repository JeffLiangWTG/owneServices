namespace Enterprise.MasterFiles.GUI
{
	partial class AccOrgTaxConfigurationEditControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.taxConfigGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.taxConfigGrid)).BeginInit();
			this.taxConfigGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate);
			// 
			// taxConfigGrid
			// 
			this.taxConfigGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.taxConfigGrid, "AccOrgTaxConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).AccOrgTaxConfigurations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplateItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).AccOrgTaxConfigurations)).SyncRoot)).OTC_ETC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplateItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).AccOrgTaxConfigurations)).SyncRoot)).OTC_ETC_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplateItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).AccOrgTaxConfigurations)).SyncRoot)).OTC_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplateItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).AccOrgTaxConfigurations)).SyncRoot)).OTC_IsThresholdUsed)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplateItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).AccOrgTaxConfigurations)).SyncRoot)).OTC_RecoverTax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplateItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).AccOrgTaxConfigurations)).SyncRoot)).OTC_SuperType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplateItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).AccOrgTaxConfigurations)).SyncRoot)).OTC_TaxSystem)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplateItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).AccOrgTaxConfigurations)).SyncRoot)).OTC_TaxAuthority)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplateItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).AccOrgTaxConfigurations)).SyncRoot)).OTC_Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplateItem)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).AccOrgTaxConfigurations)).SyncRoot)).OTC_BranchCode)));
			this.taxConfigGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c5a33196-214c-4507-aeb3-fda5c9bb46c5", "Tax Code");
			zGuidDropEditColumnStyleInfo1.ColumnName = "OTC_ETC";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b6c8d852-89f4-446f-bc87-f368f0429164", "Tax Description");
			zTextBoxColumnStyleInfo1.ColumnName = "OTC_ETC_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b6cf855d-447f-4d2b-b9d9-0db0e04339fb", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "OTC_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("016a0325-b628-4cec-bf57-f89713dfd3ff", "Threshold is Used");
			zCheckBoxColumnStyleInfo2.ColumnName = "OTC_IsThresholdUsed";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ee586738-a5b1-47b0-91d1-1511fd8dea40", "Recover Tax");
			zCheckBoxColumnStyleInfo3.ColumnName = "OTC_RecoverTax";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3f03c44b-600e-4dce-bd31-6fa8b428d340", "Super Type");
			zTextBoxColumnStyleInfo2.ColumnName = "OTC_SuperType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3353b4f5-16bc-4dae-98a4-07937cfd73e2", "Tax System");
			zTextBoxColumnStyleInfo3.ColumnName = "OTC_TaxSystem";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("51a3287b-4fbf-48d2-bea9-b6472e499030", "Tax Authority");
			zTextBoxColumnStyleInfo4.ColumnName = "OTC_TaxAuthority";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dca8fdda-efea-4c76-a247-bdcc32c41423", "Ledger");
			zTextBoxColumnStyleInfo5.ColumnName = "OTC_Ledger";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a311c812-3ab5-489f-b510-c17b406ca00f", "Tax System Branch");
			zTextBoxColumnStyleInfo6.ColumnName = "OTC_BranchCode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.taxConfigGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.taxConfigGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.taxConfigGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.taxConfigGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.taxConfigGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.taxConfigGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.taxConfigGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.taxConfigGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.taxConfigGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.taxConfigGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.taxConfigGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.taxConfigGrid.GridId = "5dafdbe1-4000-4d62-bf57-3e8a1a56cb7c";
			this.taxConfigGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.taxConfigGrid.LayoutKey = "zGrid1";
			this.taxConfigGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.taxConfigGrid.Name = "taxConfigGrid";
			this.taxConfigGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(993, 355, true);
			this.taxConfigGrid.TabIndex = 5;
			// 
			// AccOrgTaxConfigurationEditControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.taxConfigGrid);
			this.Name = "AccOrgTaxConfigurationEditControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(993, 355, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.taxConfigGrid)).EndInit();
			this.taxConfigGrid.ResumeLayout(false);
			this.taxConfigGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.ZGrid taxConfigGrid;
	}
}
