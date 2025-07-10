namespace Enterprise.MasterFiles.GUI
{
	partial class CompanyCredentialsUserControl
	{/// <summary> 
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
			this.components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.ICS2CredentialUserControl = new GlbCompany_ICS2CredentialUserControl();
			this.ICS2CredentialUserControl.SuspendLayout();
			// 
			// ICS2CredentialUserControl
			// 
			this.ICS2CredentialUserControl.AllowDrop = true;
			this.ICS2CredentialUserControl.Name = "ICS2CredentialUserControl";

			this.Controls.Add(this.ICS2CredentialUserControl);

			this.ICS2CredentialUserControl.ResumeLayout(true);
			this.ICS2CredentialUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.MasterFiles.GUI.GlbCompany_ICS2CredentialUserControl ICS2CredentialUserControl;
		ZArchitecture.GUI.DynamicLayoutPanel DynamicLayoutPanel;

	}
}
