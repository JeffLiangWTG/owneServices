namespace Enterprise.Customs.Universal.Module
{
	public partial class RefCusTradeGroupFilterControl
	{
		System.ComponentModel.Container components;

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Component Designer generated code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// FilteredGrid
			//
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("EU|RefCusTradeGroupFilterControl|74A1B112-8BE4-4184-9B3C-8FBFFAA93AE6", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "ZZA_TradeGroup";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.Universal.Module.Res.GetData("EU|RefCusTradeGroupFilterControl|D4EB0119-48FE-4CA9-B36E-F894915EB98C", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "ZZA_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.TabIndex = 3;
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCountry);
			//
			// RefCountryFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "RefCusTradeGroupFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
