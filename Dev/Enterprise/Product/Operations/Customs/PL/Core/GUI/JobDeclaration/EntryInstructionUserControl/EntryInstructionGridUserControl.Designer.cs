namespace Enterprise.Customs.PL.GUI
{
	public partial class EntryInstructionGridUserControl
	{
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).BeginInit();
			this.EntryInstructionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobDeclaration);
			// 
			// EntryInstructionGridUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "EntryInstructionGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(667, 331, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EntryInstructionsGrid)).EndInit();
			this.EntryInstructionsGrid.ResumeLayout(false);
			this.EntryInstructionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
