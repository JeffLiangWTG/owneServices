namespace Enterprise.Customs.PL.GUI
{
	partial class EntryInstructionAuthorisationsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationsGrid)).BeginInit();
			this.AuthorisationsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// AuthorisationsGrid
			// 
			this.AuthorisationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 282, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.CusAuthorizationUsage);
			// 
			// EntryInstructionAuthorisationsUserControl
			// 
			this.Name = "EntryInstructionAuthorisationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 282, true);
			((System.ComponentModel.ISupportInitialize)(this.AuthorisationsGrid)).EndInit();
			this.AuthorisationsGrid.ResumeLayout(false);
			this.AuthorisationsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
