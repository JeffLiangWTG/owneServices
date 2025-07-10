namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class QuotedBookingOrderManagementControl
	{
		private void InitializeComponent()
		{
			this.OrderLinks = new Enterprise.Freight.Forwarding.GUI.GenericOrderManagementControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
			// 
			// OrderLinks
			// 
			this.OrderLinks.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrderLinks, ".");
			this.OrderLinks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderLinks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrderLinks.Name = "OrderLinks";
			this.OrderLinks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 273, true);
			this.OrderLinks.TabIndex = 2;
			// 
			// QuotedBookingOrderManagementControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrderLinks);
			this.Name = "QuotedBookingOrderManagementControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 273, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		private Enterprise.Freight.Forwarding.GUI.GenericOrderManagementControl OrderLinks;
	}
}
