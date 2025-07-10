using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Module
{
	public partial class CusClassificationFilterControl : Customs.Module.CusClassificationFilterControl
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			// 
			// FilteredGrid
			// 
			this.FilteredGrid.Name = "FilteredGrid";
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 148, true);
			// 
			// CusClassificationFilterControl
			// 
			this.Name = "CusClassificationFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
		}
	}
}
