using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.MasterFiles.Module
{
	public partial class OrgMatchApprovalFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.ColumnName = "P2_Reference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgMatchApprovalFilterControl|50635f5d-a098-49a9-9ab8-14a2e525814f", "Master Bill");
			zTextBoxColumnStyleInfo2.ColumnName = "MasterBill";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgMatchApprovalFilterControl|25e7af35-481c-4729-b4e2-7677f4f133bc", "Company Name");
			zTextBoxColumnStyleInfo3.ColumnName = "CompanyName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgMatchApprovalFilterControl|a5b27ada-ecf4-4e2b-8a1c-86b00807db17", "Street");
			zTextBoxColumnStyleInfo4.ColumnName = "Street";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgMatchApprovalFilterControl|d63f1a49-73d3-4343-a69c-ec29ac2d3d40", "City");
			zTextBoxColumnStyleInfo5.ColumnName = "City";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgMatchApprovalFilterControl|c318f3df-57a4-4b66-b677-100305ccbcf8", "UNLOCO");
			zTextBoxColumnStyleInfo6.ColumnName = "UNLoco";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgMatchApprovalFilterControl|e970c791-31f5-48af-9cc1-22f69c7784be", "State");
			zTextBoxColumnStyleInfo7.ColumnName = "State";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgMatchApprovalFilterControl|8d011c3e-8715-4779-8caf-6162e0c09707", "Postcode");
			zTextBoxColumnStyleInfo8.ColumnName = "PostCode";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgMatchApprovalFilterControl|90feb232-fb42-42e1-8145-0b77264b2404", "Phone");
			zTextBoxColumnStyleInfo9.ColumnName = "Phone";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgMatchApprovalFilterControl|bbbe2051-d429-4fd4-9b4f-208c571bb565", "Owner Code");
			zTextBoxColumnStyleInfo10.ColumnName = "OwnerCode";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 360, true);
			this.FilteredGrid.TabIndex = 5;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgMatchApproval);
			// 
			// OrgMatchApprovalFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "OrgMatchApprovalFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 512, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
