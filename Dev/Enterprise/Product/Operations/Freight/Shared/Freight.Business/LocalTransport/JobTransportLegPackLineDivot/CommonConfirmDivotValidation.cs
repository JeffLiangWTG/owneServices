namespace Enterprise.Freight.Business
{
	public class CommonConfirmDivotValidation : JobTransportLegPackLineDivotValidation
	{
		public CommonConfirmDivotValidation(CommonConfirmDivot parent)
			: base(parent)
		{
		}

		protected override void CheckJ8_PackagesDelivered()
		{
			base.CheckJ8_PackagesDelivered();

			if (Shipment != null && Confirm != null && Confirm.PackagesConfirmedRemaining(PackLine) < 0)
			{
				Divot.J8_PackagesDeliveredInfo.AddWarning(Res.GetString("37e8ee41-e19c-412f-82c3-565ead59a841", "You cannot deliver more packs than there are in the shipment. Please check before proceeding."));
			}
		}

		protected override void CheckJ8_DeliveryWeight()
		{
			base.CheckJ8_DeliveryWeight();

			if (Shipment != null && Divot.J8_DeliveryWeight > Divot.PackLineWeight)
			{
				Divot.J8_DeliveryWeightInfo.AddWarning(Res.GetString("c7315e34-b3eb-45ce-b2f4-f1d46892c64b", "You cannot deliver more weight than there is in the shipment."));
			}
		}

		protected override void CheckJ8_DeliveryVolume()
		{
			base.CheckJ8_DeliveryVolume();

			if (Shipment != null && Divot.J8_DeliveryVolume > Divot.PackLineVolume)
			{
				Divot.J8_DeliveryVolumeInfo.AddWarning(Res.GetString("6ac78db7-561b-4822-9e87-6c8ac2eda77d", "You cannot deliver more volume than there is in the shipment."));
			}
		}

		#region Implementation

		CommonConfirmDivot Divot
		{
			get { return divot ?? (divot = (CommonConfirmDivot)Parent); }
		}
		CommonConfirmDivot divot;

		PackLine PackLine
		{
			get { return packLine ?? (packLine = Divot.PackLine); }
		}
		PackLine packLine;

		CommonShipment Shipment
		{
			get
			{
				if (shipment == null && PackLine != null && PackLine.Shipment != null)
				{
					shipment = PackLine.Shipment;
				}

				return shipment;
			}
		}
		CommonShipment shipment;

		CommonPickupDeliveryConfirm Confirm
		{
			get { return confirm ?? (confirm = Divot.Confirm); }
		}
		CommonPickupDeliveryConfirm confirm;

		#endregion
	}
}
