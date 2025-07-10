
namespace Enterprise.Freight.Forwarding.GUI
{
	partial class SupplierBookingUserControl
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
			this.detailsTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BookingDetailLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.BookingIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.detailsTopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.JobSupplierBooking);
			// 
			// detailsTopPanel
			// 
			this.detailsTopPanel.Controls.Add(this.BookingDetailLinkLabel);
			this.detailsTopPanel.Controls.Add(this.BookingIdTextBox);
			this.detailsTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.detailsTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsTopPanel.Name = "detailsTopPanel";
			this.detailsTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1104, 149, true);
			this.detailsTopPanel.TabIndex = 5;
			// 
			// BookingDetailLinkLabel
			// 
			this.BookingDetailLinkLabel.AutoSize = true;
			this.BookingDetailLinkLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("370fa386-9d0e-4ea5-98e4-dbd4ae8595ce", "Click here to show Supplier Booking record");
			this.BookingDetailLinkLabel.IsFontBold = false;
			this.BookingDetailLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 72, true);
			this.BookingDetailLinkLabel.Name = "BookingDetailLinkLabel";
			this.BookingDetailLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 13, true);
			this.BookingDetailLinkLabel.TabIndex = 2;
			// 
			// BookingIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.BookingIdTextBox, "JSB_BookingId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Orders.Business.JobSupplierBooking)(null)).JSB_BookingId)));
			this.BookingIdTextBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("78cccd33-2126-4bf5-89c7-776c88a24154", "Supplier Booking ID");
			this.BookingIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 31, true);
			this.BookingIdTextBox.Name = "BookingIdTextBox";
			this.BookingIdTextBox.ReadOnly = true;
			this.BookingIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			this.BookingIdTextBox.TabIndex = 1;
			// 
			// SupplierBookingUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.detailsTopPanel);
			this.Name = "SupplierBookingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1104, 744, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.detailsTopPanel.ResumeLayout(false);
			this.detailsTopPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel detailsTopPanel;
		private ZArchitecture.GUI.ZLinkLabel BookingDetailLinkLabel;
		private ZArchitecture.ZTextBox BookingIdTextBox;
	}
}
