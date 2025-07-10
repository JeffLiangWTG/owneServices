namespace Enterprise.Freight.CFS.Business
{
	public class GatePassShipmentValidation : CFSShipmentValidation
	{
		public GatePassShipmentValidation(GatePassShipment parent) : base(parent)
		{
		}

		protected override void CheckJS_OuterPacks()
		{
			// by this stage we don't care what they've entered previously
		}

		#region Calculated

		public void ValidateJS_ToBeFullyDelivered()
		{
			ValidateCalculatedProperty(Parent.JS_ToBeFullyDeliveredInfo);
		}

		protected virtual void CheckJS_ToBeFullyDelivered()
		{
			int numNewDeliveries = Parent.NumberOfNewDeliveries();
			if (numNewDeliveries > 1 && Parent.JS_ToBeFullyDelivered)
			{
				Parent.JS_ToBeFullyDeliveredInfo.AddError(Res.GetString("6be3a6e2-1edf-426a-86e8-9424a2954699", "You cannot Fully Deliver in multiple deliveries. Please uncheck this box or delete the unneeded new deliveries."));
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateJS_ToBeFullyDelivered();
		}

		#endregion

		#region Implementation

		public new GatePassShipment Parent
		{
			get { return (GatePassShipment)base.Parent; }
		}

		#endregion
	}
}
