namespace Enterprise.TransportBookings.GUI
{
	partial class CustomFieldsInstructionHostUserControl
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
			this.CustomFieldsHostUserControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsCollectionDetailsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBookingInstruction);
			// 
			// CustomFieldsHostUserControl
			// 
			this.CustomFieldsHostUserControl.AllowDrop = true;
			this.CustomFieldsHostUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsHostUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomFieldsHostUserControl.Name = "CustomFieldsHostUserControl";
			this.CustomFieldsHostUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 466, true);
			this.CustomFieldsHostUserControl.TabIndex = 1;
			// 
			// CustomFieldsInstructionHostUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomFieldsHostUserControl);
			this.Name = "CustomFieldsInstructionHostUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 466, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ProcessTemplateCustomFieldsCollectionDetailsControl CustomFieldsHostUserControl;
	}
}
