using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class NatureAndQtyOfGoodsLithiumBatteryValidation : NatureAndQtyOfGoodsValidation
	{
		public NatureAndQtyOfGoodsLithiumBatteryValidation(NatureAndQtyOfGoodsLithiumBattery parent)
			: base(parent)
		{
		}

		protected new NatureAndQtyOfGoodsLithiumBattery Parent
		{
			get { return (NatureAndQtyOfGoodsLithiumBattery)base.Parent; }
		}

		protected override bool IsValidationApplicable
		{
			get { return Parent.ParentRateLine.ER_NatureAndQtyOfGoodsType == Core.Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateLithiumBatteryType();
		}

		protected override void CheckText()
		{
		}

		public void ValidateLithiumBatteryType()
		{
			ValidateCalculatedProperty(Parent.LithiumBatteryTypeInfo);
		}

		protected virtual void CheckLithiumBatteryType()
		{
			if (IsValidationApplicable)
			{
				MandatoryValidation.CheckEntered(Parent.LithiumBatteryTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.LithiumBatteryTypeInfo, Parent.LithiumBatteryTypeList);
				ValidateHasEnoughSpaceToDisplayRemainingDescription();
				ValidateFlightIsCargoOnly();
			}
		}

		void ValidateHasEnoughSpaceToDisplayRemainingDescription()
		{
			var rateLine = Parent.ParentRateLine;
			for (var i = 1; i < Parent.WrappedDescriptions.Count; i++)
			{
				rateLine = rateLine.GetNextRateLine();

				if (rateLine == null || !rateLine.IsEmpty)
				{
					Parent.LithiumBatteryTypeInfo.AddError(Res.GetString("5f04ab5a-652d-45ea-8a3e-eaed420a20ef", "This is a multi-line entry ({0} lines), please leave enough empty lines.", Parent.WrappedDescriptions.Count));
					return;
				}
			}
		}

		protected virtual void ValidateFlightIsCargoOnly()
		{
		}
	}
}
