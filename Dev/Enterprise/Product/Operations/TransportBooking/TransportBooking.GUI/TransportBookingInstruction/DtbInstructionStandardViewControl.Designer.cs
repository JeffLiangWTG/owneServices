namespace Enterprise.TransportBookings.GUI
{
	partial class DtbInstructionStandardViewControl
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
				UnhookEvents();
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
			this.ComplexViewLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportBookings.Business.DtbBooking);
			// 
			// ComplexViewLabel
			// 
			this.ComplexViewLabel.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("ac8d9824-c302-4f47-bfac-fa5acdfb23bc", "This Scenario is not supported in this View.");
			this.ComplexViewLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ComplexViewLabel.IsFontBold = true;
			this.ComplexViewLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ComplexViewLabel.Name = "ComplexViewLabel";
			this.ComplexViewLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 267, true);
			this.ComplexViewLabel.TabIndex = 0;
			this.ComplexViewLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ComplexViewLabel.Visible = false;
			// 
			// DtbInstructionStandardViewControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ComplexViewLabel);
			this.Name = "DtbInstructionStandardViewControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 267, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZLabel ComplexViewLabel;
	}
}
