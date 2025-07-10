namespace Enterprise.Customs.GUI
{
	partial class EntriesWithMessagesOnDeclarationUserControl
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
			this.MessagesTabControl.SuspendLayout();
			this.DiscardedTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F782A3B4-3DE7-45D8-9BBE-AFC3FDB3A844", "Entries");
			// 
			// DiscardedTabPage
			// 
			this.DiscardedTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("E68DB628-7260-4825-BB3E-01AF390DC69A", "Messages");
			// 
			// EntriesWithMessagesOnDeclarationUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "EntriesWithMessagesOnDeclarationUserControl";
			this.MessagesTabControl.ResumeLayout(false);
			this.DiscardedTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
