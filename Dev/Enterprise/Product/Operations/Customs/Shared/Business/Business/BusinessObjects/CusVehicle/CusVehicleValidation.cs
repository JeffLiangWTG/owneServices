//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusVehicleValidation
//
//    This class should be used for overriding validation in AutoCusVehicleValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusVehicleValidation : AutoCusVehicleValidation
	{
		public CusVehicleValidation(AutoCusVehicle parent) : base(parent)
		{
		}

		protected override void CheckCVH_Mileage()
		{
			base.CheckCVH_Mileage();
			MandatoryValidation.CheckNotNegative(Parent.CVH_MileageInfo);
		}

		protected override void CheckCVH_MileageUQ()
		{
			base.CheckCVH_MileageUQ();
			ListValidation.ErrorIfInvalidCode(Parent.CVH_MileageUQInfo);

			MandatoryValidation.AddErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CVH_MileageUQInfo, Parent.CVH_MileageInfo);
		}
	}
}
