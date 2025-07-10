namespace Enterprise.MasterFiles.GUI
{
	partial class AccSurchargeApplicationUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.surchargeApplicationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.surchargeApplicationGrid)).BeginInit();
			this.surchargeApplicationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccSurchargeApplicationCollection);
			// 
			// surchargeApplicationGrid
			// 
			this.surchargeApplicationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.surchargeApplicationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccSurchargeApplication)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccSurchargeApplication)(null)).ASP_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccSurchargeApplication)(null)).ASP_SupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccSurchargeApplication)(null)).ASP_HomeCountryOrZone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccSurchargeApplication)(null)).ASP_OrganizationCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccSurchargeApplication)(null)).ASP_PlaceOfSupply)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccSurchargeApplication)(null)).ASP_ASC_NKSurchargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.AccSurchargeApplication)(null)).ASP_AT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccSurchargeApplication)(null)).SurchargeCodeDescription)));
			this.surchargeApplicationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "ASP_JobType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "ASP_SupplyType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.ColumnName = "ASP_HomeCountryOrZone";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo4.ColumnName = "ASP_OrganizationCategory";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.ColumnName = "ASP_PlaceOfSupply";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo6.ColumnName = "ASP_ASC_NKSurchargeCode";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ASP_AT";
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "SurchargeCodeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.surchargeApplicationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.surchargeApplicationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.surchargeApplicationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.surchargeApplicationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.surchargeApplicationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.surchargeApplicationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.surchargeApplicationGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.surchargeApplicationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.surchargeApplicationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.surchargeApplicationGrid.GridId = "E276CFF7-99A9-4CC5-B3DE-153FABA5E845";
			this.surchargeApplicationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.surchargeApplicationGrid.LayoutKey = "surchargeApplicationGrid";
			this.surchargeApplicationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.surchargeApplicationGrid.Name = "surchargeApplicationGrid";
			this.surchargeApplicationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 245, true);
			this.surchargeApplicationGrid.TabIndex = 1;
			// 
			// AccSurchargeApplicationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = this.Enabled;
			this.Controls.Add(this.surchargeApplicationGrid);
			this.Name = "AccSurchargeApplicationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 245, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.surchargeApplicationGrid)).EndInit();
			this.surchargeApplicationGrid.ResumeLayout(false);
			this.surchargeApplicationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZGrid surchargeApplicationGrid;
	}
}
