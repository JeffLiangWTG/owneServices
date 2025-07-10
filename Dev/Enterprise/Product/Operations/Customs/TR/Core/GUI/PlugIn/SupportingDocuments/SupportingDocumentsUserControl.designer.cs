namespace Enterprise.Customs.TR.GUI
{
	partial class SupportingDocumentsUserControl
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SupportingDocumentsFieldsControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// SupportingDocumentsSplitter
			// 
			this.SupportingDocumentsSplitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.SupportingDocumentsSplitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			// 
			// gridSplitter
			// 
			this.gridSplitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.gridSplitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			// 
			// BottomPanel
			// 
			this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 135, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 135, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
			// 
			// SupportingDocumentsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "SupportingDocumentsUserControl";
			this.SupportingDocumentsFieldsControl.ResumeLayout(true);
			this.SupportingDocumentsFieldsControl.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
