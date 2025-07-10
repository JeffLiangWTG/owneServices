using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.Universal.Module
{
	public partial class ZZRefCusRulingFilterStripControl
	{
		void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("7A9F6720-3D02-4E8B-B39A-D718FC40356E", "Ruling Number");
			zTextBoxColumnStyleInfo1.ColumnName = "ZZX_RulingNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("ACAB970F-102C-408C-AA5A-F2DFA4F5D78D", "Ruling Type");
			zTextBoxColumnStyleInfo2.ColumnName = "ZZX_RulingType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("331C0D7B-7DDE-4596-A527-AEA36C5DB136", "Ruling Type Description");
			zTextBoxColumnStyleInfo3.ColumnName = "RulingTypeDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("35FDB27C-38EF-4A52-B0AA-4B8E0A1521A5", "Description");
			zTextBoxColumnStyleInfo4.ColumnName = "ZZX_Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("0D9E5556-50E0-4671-A9AA-F8348EB0FDD9", "Org. Code");
			zTextBoxColumnStyleInfo5.ColumnName = "AppliesToAddress+Header+OH_Code";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("7D8E5787-752D-4601-B96C-6C7F303432D3", "Org. Name");
			zTextBoxColumnStyleInfo6.ColumnName = "AppliesToAddress+Header+OH_FullName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("A810F386-5906-4D57-AD1F-CF592AD04CF7", "Address 1");
			zTextBoxColumnStyleInfo7.ColumnName = "AppliesToAddress+Address1";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("41F2E411-3DE2-4044-A3E1-C8532681DD63", "Address 2");
			zTextBoxColumnStyleInfo8.ColumnName = "AppliesToAddress+Address2";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("C3A0092B-4BA9-4F9E-AA2C-94BE43C76C00", "Address State");
			zTextBoxColumnStyleInfo9.ColumnName = "AppliesToAddress+State";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("5E7E0A9C-1EDC-47BE-9141-8440CD786305", "Address Country");
			zTextBoxColumnStyleInfo10.ColumnName = "AppliesToAddressCountry";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("01CB93D3-12B7-4816-B7A3-ED180A166D7B", "Start Date");
			zTextBoxColumnStyleInfo11.ColumnName = "ZZX_StartDate";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("D93D5FF7-F862-46CB-85EC-AB27A9CCECE7", "End Date");
			zTextBoxColumnStyleInfo12.ColumnName = "ZZX_EndDate";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 504, true);
			this.grid.TabIndex = 9;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ZZRefCusRulingCombined);
			// 
			// ZZRefCusProcedureFilterStripControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "ZZRefCusRulingFilterStripControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 504, true);
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
