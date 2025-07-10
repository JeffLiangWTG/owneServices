using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DensityValuesControl : ZUserControl
	{
		public DensityValuesControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
#if DEBUG
				TypeDescriptor.AddAttributes(ExcessVolumeWeightUnitLabel, new SuppressFormsLocalizedTestAttribute());
#endif
			}
		}

		#region Bind

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (Shipment != null)
			{
				Shipment.JS_ChargeableUnitInfo.ValueChanged -= JS_ChargeableUnit_ValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Shipment != null)
			{
				Shipment.JS_ChargeableUnitInfo.ValueChanged += JS_ChargeableUnit_ValueChanged;
				SetCaptionResourceString();
			}
		}

		#endregion

		void JS_ChargeableUnit_ValueChanged(Object sender, EventArgs e)
		{
			SetCaptionResourceString();
		}

		void SetCaptionResourceString()
		{
			if (Shipment != null)
			{
				ExcessVolumeWeightCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption = Shipment.IsShipmentChargeableByWeight
					? Res.GetString("f8ea96ab-799e-423a-bc5d-50b78f985590", "Excess Volume Weight")
					: Res.GetString("33070f6c-5130-42d7-8a79-c4ff8d65b8c1", "Excess Weight Volume");
			}
		}

		ForwardingShipment Shipment => (ForwardingShipment)base.CurrentDataItem;
	}
}
