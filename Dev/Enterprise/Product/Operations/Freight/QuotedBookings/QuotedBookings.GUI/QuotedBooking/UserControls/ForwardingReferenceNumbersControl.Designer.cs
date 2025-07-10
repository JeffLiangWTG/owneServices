namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class ForwardingReferenceNumbersControl
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
            this.referenceNumbersControl = new Enterprise.MasterFiles.GUI.NumbersControl();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.referenceNumbersControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
            // 
            // referenceNumbersControl
            // 
            this.referenceNumbersControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.referenceNumbersControl, "Booking.Numbers");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).Booking.Numbers)));
            this.referenceNumbersControl.DisplayDetailPanel = false;
            this.referenceNumbersControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.referenceNumbersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.referenceNumbersControl.Name = "referenceNumbersControl";
			this.referenceNumbersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 100, true);
			this.referenceNumbersControl.TabIndex = 22;
            // 
            // ForwardingReferenceNumbersControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.referenceNumbersControl);
            this.Name = "ForwardingReferenceNumbersControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 168, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.referenceNumbersControl.ResumeLayout(true);
            this.referenceNumbersControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private Enterprise.MasterFiles.GUI.NumbersControl referenceNumbersControl;
		#endregion
	}
}
