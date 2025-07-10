//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondCargoDescValidation
//
//    This class should be used for overriding validation in AutoCusInBondCargoDescValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusInBondCargoDescValidation : AutoCusInBondCargoDescValidation
	{
		public CusInBondCargoDescValidation(AutoCusInBondCargoDesc parent)
			: base(parent)
		{
		}

		protected override void CheckBY_LineNo()
		{
			base.CheckBY_LineNo();
			MandatoryValidation.CheckNotNegative(Parent.BY_LineNoInfo);
		}

		protected override void CheckBY_NetWeight()
		{
			base.CheckBY_NetWeight();
			MandatoryValidation.CheckNotNegative(Parent.BY_NetWeightInfo);
		}

		protected override void CheckBY_NetWeightUnit()
		{
			base.CheckBY_NetWeightUnit();
			if (!Parent.BY_NetWeight.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.BY_NetWeightUnitInfo);
			}
		}

		protected override void CheckBY_TransportChargesMethodOfPayment()
		{
			base.CheckBY_TransportChargesMethodOfPayment();
			ListValidation.ErrorIfInvalidCode(Parent.BY_TransportChargesMethodOfPaymentInfo, Parent.Lookups.TransportChargesModeOfPaymentList);
		}

		protected override void CheckBY_CustomsSecondQuantity()
		{
			base.CheckBY_CustomsSecondQuantity();
			MandatoryValidation.CheckNotNegative(Parent.BY_CustomsSecondQuantityInfo);
		}
	}
}
