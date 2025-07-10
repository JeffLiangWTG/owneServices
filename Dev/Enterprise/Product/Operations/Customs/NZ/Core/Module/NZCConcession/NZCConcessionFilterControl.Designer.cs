using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.Module
{
	public partial class NZCConcessionFilterControl
	{
		#region Component Designer generated code

		System.ComponentModel.Container components = null;

		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("603531f4-751e-45c5-8ade-0437ab802f31", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "U2_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("e379c0c8-d93b-4d74-a306-6306b7619f2a", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "U2_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("131dc95e-e1f8-4795-85b4-3f772abee818", "Tariff Heading");
			zTextBoxColumnStyleInfo3.ColumnName = "U2_Calc_Tariffs";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("e68c1ff4-2acd-4c7b-bfd0-e18769b619e3", "From");
			zDateEditColumnStyleInfo1.ColumnName = "U2_DateActiveFrom";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.NZ.Module.Res.GetData("ac100e6e-5b8f-4605-bea3-3b0087c2bde4", "To");
			zDateEditColumnStyleInfo2.ColumnName = "U2_DateActiveTo";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 19, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 155, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.EDITariff_ReferenceFiles_NZ.NZCConcession);
			// 
			// NZCConcessionFilterControl
			// 
			this.Name = "NZCConcessionFilterControl";
			this.CaptionRenderingEnabled = true;
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
