using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class CustomsEntryNumbersForm : ZChildForm
	{
		public CustomsEntryNumbersForm(ForwardingShipment shipment)
			: base(new ForwardingShipmentCusEntryNumberProxyCollection(shipment))
		{
			InitializeComponent();

			closeButton.Click += (s, e) => Close();
			closeButton.Click += (s, e) =>
			{
				shipment.ShipmentCustomsEntryNumber.Validation.ValidateEntryNumber();
				shipment.CustomsEntryNumberForBindingInfo.RefreshBinding();
			};
			Closed += (s, e) =>
			{
				shipment.ShipmentCustomsEntryNumber.Validation.ValidateEntryNumber();
				shipment.CustomsEntryNumberForBindingInfo.RefreshBinding();
			};
		}

		public override string FormHeading
		{
			get { return Res.GetString("eaf48399-1045-4611-9494-6dc3848a4c65", "Customs Numbers"); }
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			ValidateAll(ValidationType.Light);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			ValidateAll(ValidationType.Light);

			ForwardingShipmentCusEntryNumberProxyCollection collection = (ForwardingShipmentCusEntryNumberProxyCollection)base.DataSource;

			if (collection.Cast<ForwardingShipmentCusEntryNumberProxy>().Any((proxy) => proxy.HasErrors))
			{
				e.Cancel = true;
				Globals.Message.ShowError(Res.GetString("1e000a37-0dbc-4a81-9c53-b977184ac29b", "Please correct all invalid entries before closing this form"));
			}
		}
	}
}
