namespace Enterprise.Customs.TW.GUI
{
	partial class OrgSupplierPartForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.OP_StockKeepingUnitBoundDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PartUnitsGrid)).BeginInit();
			this.PartUnitsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PartBarcodesGrid)).BeginInit();
			this.PartBarcodesGrid.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OrgSupplierPartForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 642, true);
			this.DataSourceType = typeof(Enterprise.Customs.TW.Business.OrgSupplierPart);
			this.DataSourceTypeName = "Enterprise.Customs.TW.Business.OrgSupplierPart";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 725, true);
			this.Name = "OrgSupplierPartForm";
			this.OP_StockKeepingUnitBoundDropEdit.ResumeLayout(true);
			this.OP_StockKeepingUnitBoundDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PartUnitsGrid)).EndInit();
			this.PartUnitsGrid.ResumeLayout(false);
			this.PartUnitsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PartBarcodesGrid)).EndInit();
			this.PartBarcodesGrid.ResumeLayout(false);
			this.PartBarcodesGrid.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
