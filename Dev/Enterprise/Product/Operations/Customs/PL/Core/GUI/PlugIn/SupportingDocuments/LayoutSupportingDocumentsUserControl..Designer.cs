namespace Enterprise.Customs.PL.GUI.PlugIn
{
	partial class LayoutSupportingDocumentsUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
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
            // SupportingDocumentsGrid
            // 
            zTextBoxColumnStyleInfo1.ColumnName = "CSI_Description";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            // 
            // LayoutSupportingDocumentsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Name = "LayoutSupportingDocumentsUserControl";
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
