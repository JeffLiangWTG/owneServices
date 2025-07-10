using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.eManifest.Business
{
	public class ShipmentActionValidation : AutoShipmentActionValidation
	{
		public ShipmentActionValidation(AutoShipmentAction parent)
			: base(parent) { }

		#region CheckB0_ShipmentControlNumber

		protected override void CheckB0_ShipmentControlNumber()
		{
			base.CheckB0_ShipmentControlNumber();
			Parent.Shipment.Validation.ValidateB0_MasterBillNumber();
		}

		#endregion

		#region CheckB0_ActionCode

		protected override void CheckB0_ActionCode()
		{
			base.CheckB0_ActionCode();
			ListValidation.ErrorIfInvalidCode(Parent.B0_ActionCodeInfo);
		}

		#endregion

		#region CheckB0_AmendmentReason

		protected override void CheckB0_AmendmentReason()
		{
			base.CheckB0_AmendmentReason();
			if (!Parent.B0_AmendmentReasonInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(Parent.B0_AmendmentReasonInfo);
				ListValidation.ErrorIfInvalidCode(Parent.B0_AmendmentReasonInfo);
			}
		}

		#endregion

		#region Implementation

		public new ShipmentAction Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ShipmentAction)base.Parent; }
		}

		#endregion
	}
}
