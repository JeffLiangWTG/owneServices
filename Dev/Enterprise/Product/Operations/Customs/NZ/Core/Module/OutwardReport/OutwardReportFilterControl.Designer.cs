using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.Module
{
	public partial class OutwardReportFilterControl
	{
		#region Component Designer generated code

		System.ComponentModel.Container components = null;

		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("d1cd0734-6050-4052-9951-0612f56149e0", "Consol ID");
			zTextBoxColumnStyleInfo1.ColumnName = "CE_ConsolID";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("f6a382b6-142c-4aa2-8c6d-a7a164605bce", "MAWB");
			zTextBoxColumnStyleInfo2.ColumnName = "CE_MasterBillNumber";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("58ffcc50-af2e-4275-8e8a-f07dca7a56aa", "Status");
			zTextBoxColumnStyleInfo3.ColumnName = "CE_EntryStatus";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("d5899df0-f07d-40a7-a2f9-550added502a", "Desc.");
			zTextBoxColumnStyleInfo4.ColumnName = "CE_StatusDescription";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("de7b0d5c-eb86-4c34-81dd-75db71d1c190", "Clearance No");
			zTextBoxColumnStyleInfo5.ColumnName = "CE_EntryNum";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 413, true);
			this.FilteredGrid.TabIndex = 5;
			// 
			// OutwardReportFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.Declaration.OutwardReport.CusEntryNumber";
			this.Name = "OutwardReportFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
