namespace Enterprise.MasterFiles.GUI
{
	public partial class OpportunityManagementValueAnalysisControl
	{

		#region Component Designer generated code

		protected Enterprise.ZArchitecture.ZGrid ValueDetailsGrid;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ValueDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ValueDetailsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgOpportunity);
			// 
			// ValueDetailsGrid
			// 
			this.ValueDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ValueDetailsGrid, "ValueItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).ValueItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunityValue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).ValueItems)).SyncRoot)).PV_RevenueType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunityValue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).ValueItems)).SyncRoot)).RevenueTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgOpportunityValue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).ValueItems)).SyncRoot)).PV_Value)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgOpportunityValue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).ValueItems)).SyncRoot)).PV_DiscountBasis)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgOpportunityValue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).ValueItems)).SyncRoot)).PV_DiscountPercent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgOpportunityValue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).ValueItems)).SyncRoot)).PV_Discount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgOpportunityValue)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgOpportunity)(null)).ValueItems)).SyncRoot)).ValueAfterDiscount)));
			this.ValueDetailsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "PV_RevenueType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementValueAnalysisControl|ae5f305b-e2d9-4a34-b24b-ae96deb60e17", "Value Type");
			zTextBoxColumnStyleInfo1.ColumnName = "RevenueTypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "PV_Value";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.ColumnName = "PV_DiscountBasis";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "PV_DiscountPercent";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "PV_Discount";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementValueAnalysisControl|8e83d714-64ea-48a3-b00c-29665856855c", "Value");
			zCalcEditColumnStyleInfo4.ColumnName = "ValueAfterDiscount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.ValueDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ValueDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ValueDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ValueDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ValueDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ValueDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ValueDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ValueDetailsGrid.GridId = "609254f3-bc1d-4d4a-a870-a4754b5ff9bf";
			this.ValueDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ValueDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ValueDetailsGrid.LayoutKey = "ValueDetailsGrid";
			this.ValueDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ValueDetailsGrid.Name = "ValueDetailsGrid";
			this.ValueDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 117, true);
			this.ValueDetailsGrid.TabIndex = 1;
			// 
			// OpportunityManagementValueAnalysisControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ValueDetailsGrid);
			this.Name = "OpportunityManagementValueAnalysisControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 117, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ValueDetailsGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
