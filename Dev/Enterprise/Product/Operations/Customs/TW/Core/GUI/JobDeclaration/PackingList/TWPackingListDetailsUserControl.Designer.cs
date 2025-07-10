namespace Enterprise.Customs.TW.GUI
{
	partial class TWPackingListDetailsUserControl
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
			this.CUL_PackingListDateDateEdit.SuspendLayout();
			this.CUL_RemarksLongTextBox.SuspendLayout();
			this.HeaderPanel.SuspendLayout();
			this.CUL_DescriptionLongTextBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.CusPackingList);
			// 
			// TWPackingListDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "TWPackingListDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1276, 652, true);
			this.CUL_PackingListDateDateEdit.ResumeLayout(true);
			this.CUL_PackingListDateDateEdit.PerformLayout();
			this.CUL_RemarksLongTextBox.ResumeLayout(true);
			this.CUL_RemarksLongTextBox.PerformLayout();
			this.HeaderPanel.ResumeLayout(false);
			this.HeaderPanel.PerformLayout();
			this.CUL_DescriptionLongTextBox.ResumeLayout(true);
			this.CUL_DescriptionLongTextBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
