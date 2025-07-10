using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class BookingDetailsControlTestForm : ZForm
	{
		public BookingDetailsControlTestForm(AgencyShipment shipment) : base(shipment)
		{
		}

		protected override void InitializeComponent()
		{
			Control = new BookingDetailsControl();
			Control.Dock = DockStyle.Fill;
			this.Controls.Add(Control);
			this.Size = new Size(600, 300);
			ContentTabControl = (BookingContentTabControl)typeof(BookingDetailsControl).GetField("ShipmentContentTabControl", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Control);
		}

		public BookingDetailsControl Control;
		public BookingContentTabControl ContentTabControl;
	}
}
