using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class InventoryFilterUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsInventoryView);
			// 
			// InventoryFilterUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "InventoryFilterUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 277, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
