using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.GUI.Testing
{
	internal class DeliveryConfirmControlTestForm : ZForm
	{
		public DeliveryConfirmControlTestForm(CommonShipment shipment)
			: base(shipment)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			base.InitializeComponent();

			var control = new ShipmentDeliveryConfirmControl();
			control.Dock = DockStyle.Fill;

			this.Controls.Add(control);
			this.ClientSize = control.Size;

			ConfirmsGrid = control.LooseDeliveryConfirmationGrid;

			this.CaptionRenderingEnabled = true;
		}

		public ZGrid ConfirmsGrid;
	}
}
