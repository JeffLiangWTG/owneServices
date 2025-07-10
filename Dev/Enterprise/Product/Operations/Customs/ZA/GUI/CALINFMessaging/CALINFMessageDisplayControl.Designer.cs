namespace Enterprise.Customs.ZA.GUI
{
	partial class CALINFMessageDisplayControl
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
			this.MessagesUserControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessagesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.JobVoyage);
			// 
			// MessagesUserControl
			// 
			this.MessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessagesUserControl, ".");
			this.MessagesUserControl.BindPrepend = "";
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesUserControl.Name = "MessagesUserControl";
			this.MessagesUserControl.ShowChangingBlueMessageHeading = false;
			this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 413, true);
			this.MessagesUserControl.TabIndex = 0;
			// 
			// CALINFMessageDisplayControl
			// 
			this.Controls.Add(this.MessagesUserControl);
			this.Name = "CALINFMessageDisplayControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 413, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagesUserControl.ResumeLayout(true);
			this.MessagesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		Enterprise.Messaging.GUI.EDIMessageUserControl MessagesUserControl;
		#endregion
	}
}
