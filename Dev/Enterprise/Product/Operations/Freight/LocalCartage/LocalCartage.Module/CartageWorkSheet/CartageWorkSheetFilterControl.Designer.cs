namespace Enterprise.Freight.LocalCartage.Module
{
	public partial class CartageWorkSheetFilterControl
	{
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).EY_RunSheetNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).EY_StartTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).EY_EndTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).EY_RQ_Truck)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).VehicleType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).EY_GS_NKTruckDriver)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).EY_DriversName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).EY_OH_TransportCo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).TransportCompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).EY_DriversLicence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).EY_TruckRegistration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).Truck.RQ_DescriptionMultilingual)));
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.LocalCartage.Module.Res.GetData("CartageWorkSheetFilterControl|b0e7fefc-2f9a-45c5-b369-ba7b025f71ca", "Run Sheet#");
			zTextBoxColumnStyleInfo1.ColumnName = "EY_RunSheetNumber";
			zTextBoxColumnStyleInfo1.GroupName = null;
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = null;
			zDateEditColumnStyleInfo1.ColumnName = "EY_StartTime";
			zDateEditColumnStyleInfo1.GroupName = null;
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = null;
			zDateEditColumnStyleInfo2.ColumnName = "EY_EndTime";
			zDateEditColumnStyleInfo2.GroupName = null;
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = null;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "EY_RQ_Truck";
			zGuidFindBoxColumnStyleInfo1.GroupName = Enterprise.Freight.LocalCartage.Module.Res.GetData("CartageWorkSheetFilterControl|ad56ce39-4ba3-42cc-b05c-12f480f0a103", "Vehicle");
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.LocalCartage.Module.Res.GetData("CartageWorkSheetFilterControl|ac4c60c5-1a9c-4d67-80b0-d38542da921d", "Vehicle Type");
			zTextBoxColumnStyleInfo2.ColumnName = "VehicleType";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Freight.LocalCartage.Module.Res.GetData("CartageWorkSheetFilterControl|ad56ce39-4ba3-42cc-b05c-12f480f0a103", "Vehicle");
			zCodeFindBoxColumnStyleInfo1.Caption = null;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = null;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "EY_GS_NKTruckDriver";
			zCodeFindBoxColumnStyleInfo1.GroupName = null;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = null;
			zTextBoxColumnStyleInfo3.ColumnName = "EY_DriversName";
			zTextBoxColumnStyleInfo3.GroupName = null;
			zOrganisationFindBoxColumnStyleInfo1.Caption = null;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.LocalCartage.Module.Res.GetData("CartageWorkSheetFilterControl|9eea0561-67fd-4c8b-829c-32f87331669f", "Transport Co");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "EY_OH_TransportCo";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = null;
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.LocalCartage.Module.Res.GetData("CartageWorkSheetFilterControl|0f8551c8-725a-42fc-8339-825a35ab24bc", "Transport Co Name");
			zTextBoxColumnStyleInfo4.ColumnName = "TransportCompanyName";
			zTextBoxColumnStyleInfo4.GroupName = null;
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = null;
			zTextBoxColumnStyleInfo5.ColumnName = "EY_DriversLicence";
			zTextBoxColumnStyleInfo5.GroupName = null;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = null;
			zTextBoxColumnStyleInfo6.ColumnName = "EY_TruckRegistration";
			zTextBoxColumnStyleInfo6.GroupName = null;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = null;
			zTextBoxColumnStyleInfo7.ColumnName = "Truck+RQ_DescriptionMultilingual";
			zTextBoxColumnStyleInfo7.GroupName = null;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonWorkSheet);
			// 
			// CartageWorkSheetFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "CartageWorkSheetFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
