namespace Enterprise.Customs.PL.NCTS.GUI
{
	partial class MessagesTabUserControl
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
			((System.ComponentModel.ISupportInitialize)(this.MessageGrid)).BeginInit();
			this.MessageGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// MessageGrid
			//
			MessageGrid.SelectedRowsChangedInMouseDown += MessageGrid_SelectedRowsChangedInMouseDown;
			// 
			// MessagesTabUserControl
			// 
			this.Name = "MessagesTabUserControl";
			((System.ComponentModel.ISupportInitialize)(this.MessageGrid)).EndInit();
			this.MessageGrid.ResumeLayout(false);
			this.MessageGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

	}
}
