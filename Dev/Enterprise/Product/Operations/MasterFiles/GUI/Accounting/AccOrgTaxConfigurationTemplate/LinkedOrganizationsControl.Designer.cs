namespace Enterprise.MasterFiles.GUI
{
	partial class LinkedOrganizationsControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zModuleButtonGrid = new Enterprise.MasterFiles.GUI.ZModuleButtonGridForDesigner();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zModuleButtonGrid.InnerGrid)).BeginInit();
			this.zModuleButtonGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate);
			// 
			// zModuleButtonGrid
			// 
			this.zModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zModuleButtonGrid, "LinkedOrganisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).LinkedOrganisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccOrgTaxConfigurationTemplate)(null)).FindBoxCollection)));
			this.zModuleButtonGrid.BindToFindBoxList = "FindBoxCollection";
			zTextBoxColumnStyleInfo1.ColumnName = "OH_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "OH_FullName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0e341437-c4e3-4658-b13b-e7a72364a1c3", "Country/Region");
			zTextBoxColumnStyleInfo3.ColumnName = "MainAddressCountryCodes";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "MainAddress+OA_City";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "MainAddress+OA_State";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "OH_Category";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "OH_RL_NKClosestPort";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.zModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.zModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.zModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zModuleButtonGrid.GridId = null;
			// 
			// 
			// 
			this.zModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.zModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.zModuleButtonGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zModuleButtonGrid.InnerGrid.GridId = null;
			this.zModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.zModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zModuleButtonGrid.InnerGrid.Name = "Grid";
			this.zModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 112, true);
			this.zModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.zModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zModuleButtonGrid.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.zModuleButtonGrid.Name = "zModuleButtonGrid";
			this.zModuleButtonGrid.ReadOnly = true;
			this.zModuleButtonGrid.ShowEditButton = false;
			this.zModuleButtonGrid.ShowNewButton = false;
			this.zModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.zModuleButtonGrid.TabIndex = 0;
			// 
			// LinkedOrganizationsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zModuleButtonGrid);
			this.Name = "LinkedOrganizationsControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zModuleButtonGrid.InnerGrid)).EndInit();
			this.zModuleButtonGrid.ResumeLayout(true);
			this.zModuleButtonGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.MasterFiles.GUI.ZModuleButtonGridForDesigner zModuleButtonGrid;
	}
}
