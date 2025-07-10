namespace Enterprise.Customs.PL.GUI
{
	public partial class SupportingInformationControl
	{
		void InitializeComponent()
		{
			this.SupportingInformationTabControl.SuspendLayout();
			this.SupportingDocumentTabPage.SuspendLayout();
			this.AdditionalInfoTabPage.SuspendLayout();
			this.PreviousDocumentTabPage.SuspendLayout();
			this.GuaranteesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobDeclaration);
			// 
			// SupportingInformationControl
			// 
			this.Name = "SupportingInformationControl";
			this.SupportingInformationTabControl.ResumeLayout(false);
			this.SupportingInformationTabControl.PerformLayout();
			this.SupportingDocumentTabPage.ResumeLayout(false);
			this.SupportingDocumentTabPage.PerformLayout();
			this.AdditionalInfoTabPage.ResumeLayout(false);
			this.AdditionalInfoTabPage.PerformLayout();
			this.PreviousDocumentTabPage.ResumeLayout(false);
			this.PreviousDocumentTabPage.PerformLayout();
			this.GuaranteesTabPage.ResumeLayout(false);
			this.GuaranteesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
