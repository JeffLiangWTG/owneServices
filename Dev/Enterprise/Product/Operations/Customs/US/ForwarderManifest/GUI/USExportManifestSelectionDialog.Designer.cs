namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	partial class USExportManifestSelectionDialog
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
		protected new void InitializeComponent()
		{
			this.Text = "Send Export Manifest";
			this.DescPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();

			this.SuspendLayout();
			// 
			// DescPanel
			// 
			this.DescPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 380, true);
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.GridId = "63F93453-35DA-42DC-9006-A7D6C3C4285E";
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 243, true);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ForwarderManifest.Business.UEMMessageChooserItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.MessageChooser)(null)).ChooserItems)).SyncRoot)).ActionType)));
			// 
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ForwarderManifest.Business.UEMMessageChooser);
			this.DataSourceType = typeof(Enterprise.Customs.US.ForwarderManifest.Business.UEMMessageChooser);
			this.DataSourceTypeName = nameof(Enterprise.Customs.US.ForwarderManifest.Business.UEMMessageChooser);
			this.DescPanel.ResumeLayout(false);
			this.DescPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
