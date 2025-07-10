using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.MasterFiles.Module
{
	public partial class AccGLAccountDescriptorFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 

			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccGLAccountDescriptorFilterControl|3ea1ff28-fac2-42b7-b11a-21d282076df1", "Parent Account");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ParentGLHeaderPK";

			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("f53ae527-6371-491e-bd03-c8149ea46fd3", "Language");

			zTextBoxColumnStyleInfo1.ColumnName = "AJ_Language";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("8f3821ca-96a0-4d6a-8ab5-f45884d424fa", "Country/Region");

			zTextBoxColumnStyleInfo2.ColumnName = "AJ_RN_NKCountryOfCompliance";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("538f3ae9-d6ec-49ee-8478-90de6a9ca0d0", "Category", "Report Category");

			zTextBoxColumnStyleInfo3.ColumnName = "AJ_ReportCategory";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccGLAccountDescriptorFilterControl|f68e2e3f-870b-4cb8-8382-2212864b6a49", "Local Account Num.", "Local Account Number");

			zTextBoxColumnStyleInfo4.ColumnName = "AJ_LocalAccountNumber";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("367a9762-abc3-4a63-9935-3b60a0867cfc", "Description", "Account Description");

			zTextBoxColumnStyleInfo5.ColumnName = "AJ_AccountDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccGLAccountDescriptorFilterControl|6912e082-196e-4a7c-9cd7-cebdc53c07c0", "DR/CR");

			zTextBoxColumnStyleInfo6.ColumnName = "AJ_DebitCredit";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("be4e66e4-aeb4-496a-9b03-2e3e25dbf196", "Print Sequence");

			zCalcEditColumnStyleInfo1.ColumnName = "AJ_PrintSequence";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("69739ab3-dc02-47e9-937c-959fb449a101", "Total Level");

			zCalcEditColumnStyleInfo2.ColumnName = "AJ_TotalLevel";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zGuidFindBoxColumnStyleInfo2.Caption = null;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccGLAccountDescriptorFilterControl|6b77c7ff-0420-4633-a080-a0a75ebd63d1", "Carried Fwd Account");

			zGuidFindBoxColumnStyleInfo2.ColumnName = "AJ_AJ_CarriedForwardAccount";
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo3.Caption = null;
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccGLAccountDescriptorFilterControl|b617a73c-9922-4e5c-8189-63f4724bba28", "Percent Of", "Percent of Account.");

			zGuidFindBoxColumnStyleInfo3.ColumnName = "AJ_AJ_PercentNum";
			zGuidFindBoxColumnStyleInfo4.Caption = null;
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccGLAccountDescriptorFilterControl|ffbb36a1-69f3-4728-852c-d8190d0549a6", "Alternative");

			zGuidFindBoxColumnStyleInfo4.ColumnName = "AJ_AJ_AlternativeNum";
			zGuidFindBoxColumnStyleInfo5.Caption = null;
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccGLAccountDescriptorFilterControl|53f7029c-387f-40c8-bde5-10a3f02a86c0", "Consolidation");

			zGuidFindBoxColumnStyleInfo5.ColumnName = "AJ_AJ_ConsolidationNum";
			zGuidFindBoxColumnStyleInfo6.Caption = null;
			zGuidFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("AccGLAccountDescriptorFilterControl|b20f2dfd-c9f5-4a1b-a6bc-f3547ba3229c", "Total Reference");

			zGuidFindBoxColumnStyleInfo6.ColumnName = "AJ_AJ_HeaderDependsOnTotal";
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 144, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 240, true);
			this.FilteredGrid.TabIndex = 3;
			// 
			// AddStripButton
			// 
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 28, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccGLAccountDescriptor);
			// 
			// AccGLAccountDescriptorFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "AccGLAccountDescriptorFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 384, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
