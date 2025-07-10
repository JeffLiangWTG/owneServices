namespace Enterprise.Customs.PL.GUI
{
	public partial class EntryLineTaxAndFeeUserControl
	{
		void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.AllCusEntryLineCollection<Enterprise.Customs.PL.Business.Declaration.CusEntryLine>);
			// 
			// EntryLineAdditionalDataUserControl
			// 
			this.Name = "EntryLineTaxAndFeeUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
