namespace Enterprise.MasterFiles.GUI
{
	public partial class ComplianceReportsSetupCategoryControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			ReportTypeGrid = new Enterprise.ZArchitecture.ZGrid();
			ReportCategoryGrid = new Enterprise.ZArchitecture.ZGrid();
			splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(ReportTypeGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(ReportCategoryGrid)).BeginInit();
			splitContainer1.Panel1.SuspendLayout();
			splitContainer1.Panel2.SuspendLayout();
			splitContainer1.SuspendLayout();
			SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ComplianceReportTypeCollection);
			// 
			// ReportTypeGrid
			// 
			ReportTypeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReportTypeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ComplianceReportType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ComplianceReportType)(null)).ReportType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ComplianceReportType)(null)).ReportTypeDescription)));
			this.ReportTypeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ComplianceReportsSetupCategoryControl|42115048-c9ca-4941-93c2-94a5effcc123", "Report");
			zTextBoxColumnStyleInfo1.ColumnName = "ReportType";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ComplianceReportsSetupCategoryControl|28e7e5d6-0c44-4d29-9942-5cab7ab64123", "Report Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ReportTypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.ReportTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReportTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReportTypeGrid.GridId = "62e42195-6592-48fc-a423-fb8277516123";
			this.ReportTypeGrid.CopySelectedRowsAllowed = true;
			this.ReportTypeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportTypeGrid.LayoutKey = "ComplianceReportTypeGrid";
			this.ReportTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportTypeGrid.Name = "ReportTypeGrid";
			this.ReportTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 118, true);
			this.ReportTypeGrid.TabIndex = 1;
			// 
			// ReportCategoryGrid
			// 
			this.ReportCategoryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReportCategoryGrid, "ReportTypeCategories");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ComplianceReportType)(null)).ReportTypeCategories)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ComplianceReportsSetupCategory)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ComplianceReportType)(null)).ReportTypeCategories)).SyncRoot)).Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ComplianceReportsSetupCategory)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ComplianceReportType)(null)).ReportTypeCategories)).SyncRoot)).CategoryDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ComplianceReportsSetupCategory)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ComplianceReportType)(null)).ReportTypeCategories)).SyncRoot)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ComplianceReportsSetupCategory)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ComplianceReportType)(null)).ReportTypeCategories)).SyncRoot)).AllowDuplicate)));
			this.ReportCategoryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ComplianceReportsSetupCategoryControl|083ade90-49a3-40c9-9dea-beb8b22b1433", "Category");
			zTextBoxColumnStyleInfo3.ColumnName = "Category";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ComplianceReportsSetupCategoryControl|62eaffb2-c5d6-4cb9-900a-4757a02f7ea1", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "CategoryDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ComplianceReportsSetupCategoryControl|fad227c4-580e-4e2a-afb5-17a3a3c2c103", "Allow Duplicate Mapping");
			zCheckBoxColumnStyleInfo1.ColumnName = "AllowDuplicate";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.ReportCategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ReportCategoryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ReportCategoryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ReportCategoryGrid.GridId = "0AB0627B-A630-4AC2-A7E8-E51D16648EF3";
			this.ReportCategoryGrid.CopySelectedRowsAllowed = true;
			this.ReportCategoryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportCategoryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportCategoryGrid.LayoutKey = "RevenueRecognitionByChargeGroupGrid";
			this.ReportCategoryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportCategoryGrid.Name = "ReportCategoryGrid";
			this.ReportCategoryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 200, true);
			this.ReportCategoryGrid.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.ReportTypeGrid);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ReportCategoryGrid);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 322, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(118);
			this.splitContainer1.TabIndex = 2;
			// 
			// ComplianceReportsSetupCategoryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ComplianceReportsSetupCategoryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 322, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportTypeGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportCategoryGrid)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		internal Enterprise.ZArchitecture.ZGrid ReportCategoryGrid;
		CargoWise.Windows.UI.KSplitContainer splitContainer1;
		internal Enterprise.ZArchitecture.ZGrid ReportTypeGrid;
	}
}
