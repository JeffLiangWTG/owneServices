using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.GUI.Testing
{
	internal class PickupConfirmControlTestForm : ZForm
	{
		public PickupConfirmControlTestForm(CommonShipment shipment)
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

			var control = new ShipmentPickupConfirmControl();
			control.Dock = DockStyle.Fill;

			this.Controls.Add(control);
			this.ClientSize = control.Size;

			ConfirmsGrid = control.LoosePickupConfirmationGrid;

			this.CaptionRenderingEnabled = true;
		}

		public ZGrid ConfirmsGrid;
	}
}
