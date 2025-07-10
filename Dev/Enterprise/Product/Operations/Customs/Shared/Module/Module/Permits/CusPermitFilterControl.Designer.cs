using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.Module
{
	public partial class CusPermitFilterControl
	{
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo permitClosedColumnStyleInfo = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.BaseCusPermitHeader)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BaseCusPermitHeader)(null)).PermitHolder.OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BaseCusPermitHeader)(null)).CPH_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BaseCusPermitHeader)(null)).CPH_Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.BaseCusPermitHeader)(null)).CPH_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.BaseCusPermitHeader)(null)).CPH_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BaseCusPermitHeader)(null)).CPH_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BaseCusPermitHeader)(null)).CPH_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BaseCusPermitHeader)(null)).CPH_QtyValIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Business.BaseCusPermitHeader)(null)).CPH_Calc_OpeningBalance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Business.BaseCusPermitHeader)(null)).ValueBalance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Business.BaseCusPermitHeader)(null)).QuantityBalance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BaseCusPermitHeader)(null)).AuthLatestValueBalanceWithMsg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BaseCusPermitHeader)(null)).AuthLatestQuantityBalanceWithMsg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.BaseCusPermitHeader)(null)).CPH_UnitOfMeasure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.BaseCusPermitHeader)(null)).CPH_IsClosed)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|07726573-CDCD-4128-8F54-ECB4D723B3AF", "Permit Holder");
			zTextBoxColumnStyleInfo1.ColumnName = "PermitHolder.OH_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|5E1DD89F-4EF1-49B7-BAAE-AA885AA682E6", "Country Code");
			zTextBoxColumnStyleInfo2.ColumnName = "CPH_RN_NKCountryCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|8939DF73-7763-4D6B-82C3-F8329A616189", "Permit Number");
			zTextBoxColumnStyleInfo3.ColumnName = "CPH_Number";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|ECBFF20E-3109-4C17-989D-C353459FA9D8", "Start Date");
			zDateEditColumnStyleInfo1.ColumnName = "CPH_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|4E150826-AE92-416C-9DFA-2C31951568B3", "End Date");
			zDateEditColumnStyleInfo2.ColumnName = "CPH_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|1625A6AD-00BA-408B-9C08-8A1D42CB7D34", "Type");
			zTextBoxColumnStyleInfo4.ColumnName = "CPH_Type";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|3BA8A34D-8A61-40F3-972B-35CD7861E4C0", "Sub Type");
			zTextBoxColumnStyleInfo5.ColumnName = "CPH_SubType";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|FE1AA0FB-43A6-4841-BDBD-51F04E85882A", "Qty/Val Indicator");
			zTextBoxColumnStyleInfo6.ColumnName = "CPH_QtyValIndicator";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.Caption = "";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|0CCEA905-F236-4512-965B-BBC0B63FAA32", "Qty/Val Ind. Desc.", "Qty/Val Ind. Description", "Qty/Val Indicator Description");
			zTextBoxColumnStyleInfo7.ColumnName = "QtyValIndicatorDescription";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|AF93F831-B2DE-41D7-8EC8-A52934455BF3", "Guarantee Amount");
			zCalcEditColumnStyleInfo8.ColumnName = "CPH_Calc_OpeningBalance";
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|A743CF4C-4033-4ACA-8705-E26617B653B2", "Remaining Value");
			zCalcEditColumnStyleInfo9.ColumnName = "ValueBalance";
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|41408785-D08A-4FB1-9B80-F021E8520A8B", "Remaining Quantity");
			zTextBoxColumnStyleInfo9.ColumnName = "QuantityBalance";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|8176B1DE-342E-4091-9FEA-9A7C9B4DE63F", "Auth. Latest Val");
			zTextBoxColumnStyleInfo10.ColumnName = "AuthLatestValueBalanceWithMsg";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|EACE3A52-9BD4-493C-9B44-0A397851440A", "Auth. Latest Qty");
			zTextBoxColumnStyleInfo11.ColumnName = "AuthLatestQuantityBalanceWithMsg";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|2CE2CF8E-3362-4D7F-A16B-973A2D21A8D0", "UOM", "Unit of Measure", "Unit of Measure (UOM)");
			zTextBoxColumnStyleInfo12.ColumnName = "CPH_UnitOfMeasure";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			permitClosedColumnStyleInfo.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("CusPermitModule|346E4F25-F856-402D-B06A-3C8ABD104298", "CLS", "Closed", "Permit Closed");
			permitClosedColumnStyleInfo.ColumnName = "CPH_IsClosed";
			permitClosedColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(permitClosedColumnStyleInfo);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 141, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.BaseCusPermitHeader);
			// 
			// CusPermitFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "CusPermitFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 293, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
