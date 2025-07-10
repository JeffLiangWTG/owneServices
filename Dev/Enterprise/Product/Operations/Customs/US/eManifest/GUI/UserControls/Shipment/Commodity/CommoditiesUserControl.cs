using System;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class CommoditiesUserControl : ZUserControl
	{
		public CommoditiesUserControl()
		{
			InitializeComponent();
		}

		#region OnCurrentDataItemChanged

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var currentShipment = CurrentDataItem as Shipment;
			if (shipment != currentShipment)
			{
				if (shipment != null)
				{
					shipment.B0_ShipmentTypeInfo.ValueChanged -= OnShipmentTypeValueChanged;
				}

				shipment = currentShipment;
				if (shipment != null)
				{
					shipment.B0_ShipmentTypeInfo.ValueChanged += OnShipmentTypeValueChanged;
				}

				OnShipmentTypeValueChanged(shipment, EventArgs.Empty);
			}
		}

		void OnShipmentTypeValueChanged(object sender, EventArgs e)
		{
			var type = shipment != null ? shipment.B0_ShipmentType : ZString.Empty;
			CommoditiesGrid.SetColumnVisible(type == ShipmentTypes.Codes.BRASS, Commodity.Schema.BY_C4Codes);
			CommoditiesGrid.SetColumnVisible(type == ShipmentTypes.Codes.LowValue, Commodity.Schema.BY_RN_NKCountryOfOrigin);
		}

		#endregion

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && shipment != null)
			{
				shipment.B0_ShipmentTypeInfo.ValueChanged -= OnShipmentTypeValueChanged;
			}
			base.Dispose(disposing);
		}

		#endregion

		Shipment shipment;
	}
}
