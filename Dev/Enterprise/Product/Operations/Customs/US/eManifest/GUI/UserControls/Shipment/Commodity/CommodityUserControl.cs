using System;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class CommodityUserControl : ZUserControl
	{
		public CommodityUserControl()
		{
			InitializeComponent();
			BindingSource.SetBindingMember(CommodityDescriptionControl, "BY_Description");
			BindingSource.SetBindingMember(ShippingMarksControl, "BY_MarksAndNumbers");
			ShipmentTypeValueChanged(null, EventArgs.Empty);
		}

		#region OnCurrentDataItemChanged

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var commodity = CurrentDataItem as Commodity;
			var currentShipment = commodity != null ? commodity.Shipment : null;
			if (currentShipment != shipment)
			{
				if (shipment != null)
				{
					shipment.B0_ShipmentTypeInfo.ValueChanged -= ShipmentTypeValueChanged;
				}

				shipment = currentShipment;
				if (shipment != null)
				{
					shipment.B0_ShipmentTypeInfo.ValueChanged += ShipmentTypeValueChanged;
				}

				ShipmentTypeValueChanged(null, EventArgs.Empty);
			}
		}

		void ShipmentTypeValueChanged(object sender, EventArgs e)
		{
			var type = shipment != null ? shipment.B0_ShipmentType : ZString.Empty;
			C4CodesGrid.Visible = type == ShipmentTypes.Codes.BRASS;
			C4CodesLabel.Visible = type != ShipmentTypes.Codes.BRASS;

			CountryOfOriginCodeFindBox.Visible = type == ShipmentTypes.Codes.LowValue;
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
				shipment.B0_ShipmentTypeInfo.ValueChanged -= ShipmentTypeValueChanged;
			}
			base.Dispose(disposing);
		}

		Shipment shipment;

		#endregion
	}
}
